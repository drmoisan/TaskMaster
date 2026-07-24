<#
.SYNOPSIS
    Records actual Codex subagent identity, model, routing, and epic-root provenance.

.DESCRIPTION
    SubagentStart hook. The hook cannot prevent startup, so it writes an attestation consumed by
    mutation and stop hooks. Epic personas require a matching single-use root receipt. Routed
    worker profiles require a matching model-routing receipt.
#>
[CmdletBinding()]
param()

. (Join-Path $PSScriptRoot 'codex-authority-store.ps1')
. (Join-Path $PSScriptRoot 'codex-agent-profile-attestation.ps1')
. (Join-Path $PSScriptRoot 'codex-epic-child-launch-attestation.ps1')
$script:EpicPersonas = @('epic-planner', 'epic-orchestrator')

function ConvertFrom-SubagentAttestationJson {
    [CmdletBinding()]
    param(
        [AllowNull()][AllowEmptyString()][string] $Raw,
        [Parameter(Mandatory)][string] $Name,
        [switch] $Optional
    )

    if ([string]::IsNullOrWhiteSpace($Raw)) {
        if ($Optional) {
            return $null
        }
        throw "MODEL_ROUTING_ATTESTATION_BLOCKED: $Name is empty."
    }
    try {
        return $Raw | ConvertFrom-Json -ErrorAction Stop
    } catch {
        if ($Optional) {
            return $null
        }
        throw "MODEL_ROUTING_ATTESTATION_BLOCKED: $Name is malformed JSON: $_"
    }
}

function Get-SubagentAttestationKey {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [AllowNull()][string] $TranscriptPath,
        [Parameter(Mandatory)][string] $AgentId
    )

    $identity = if ([string]::IsNullOrWhiteSpace($TranscriptPath)) { $AgentId } else { $TranscriptPath }
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($identity)
    return [Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
}

function Test-RootEpicReceipt {
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowNull()] $Receipt,
        [Parameter(Mandatory)] $Payload,
        [Parameter(Mandatory)][string] $RepositoryRoot,
        [Parameter(Mandatory)][string] $CurrentHeadSha,
        [Parameter(Mandatory)][datetimeoffset] $Now
    )

    if ($null -eq $Receipt) {
        return $false
    }
    $properties = @($Receipt.PSObject.Properties.Name)
    $expectedProperties = @(
        'schema_version', 'repository_root', 'repository_sha256', 'repository_head_sha',
        'session_id', 'turn_id', 'prompt_sha256', 'requested_persona', 'entry_kind',
        'epic_reference', 'kickoff_path', 'created_at', 'expires_at', 'consumed',
        'consumed_by', 'consumed_at'
    )
    if (@($properties | Where-Object { $expectedProperties -notcontains $_ }).Count -gt 0 -or
        @($expectedProperties | Where-Object { $properties -notcontains $_ }).Count -gt 0) {
        return $false
    }
    $canonicalRoot = Get-CodexCanonicalAuthorityPath -Path $RepositoryRoot
    $repositoryKey = Get-CodexAuthorityRepositoryKey -RepositoryRoot $RepositoryRoot
    if ($Receipt.schema_version -isnot [ValueType] -or [int]$Receipt.schema_version -ne 1 -or
        [string]$Receipt.repository_root -cne $canonicalRoot -or
        [string]$Receipt.repository_sha256 -ne $repositoryKey -or
        [string]$Receipt.repository_head_sha -ne $CurrentHeadSha -or
        [string]$Receipt.repository_head_sha -notmatch '^[0-9a-fA-F]{40,64}$' -or
        [string]$Receipt.session_id -ne [string]$Payload.session_id -or
        [string]$Receipt.turn_id -ne [string]$Payload.turn_id -or
        [string]$Receipt.requested_persona -ne [string]$Payload.agent_type -or
        [string]$Receipt.prompt_sha256 -notmatch '^[0-9a-f]{64}$' -or
        $Receipt.consumed -isnot [bool] -or [bool]$Receipt.consumed) {
        return $false
    }
    if ($null -ne $Receipt.consumed_by -or $null -ne $Receipt.consumed_at) {
        return $false
    }
    $entryKind = [string]$Receipt.entry_kind
    $entryMatchesPersona = switch ([string]$Receipt.requested_persona) {
        'epic-planner' { $entryKind -in @('epic-plan', 'direct') }
        'epic-orchestrator' { $entryKind -in @('epic-run', 'epic-orchestrate', 'direct') }
        default { $false }
    }
    if (-not $entryMatchesPersona) {
        return $false
    }
    $reference = [string]$Receipt.epic_reference
    $kickoffPath = [string]$Receipt.kickoff_path
    if ($entryKind -in @('epic-plan', 'epic-run', 'epic-orchestrate') -and
        [string]::IsNullOrWhiteSpace($reference)) {
        return $false
    }
    if ($entryKind -eq 'epic-run') {
        $referenceParts = @(
            ($reference -replace '\\', '/').Trim('/').Split('/') |
                Where-Object { $_ }
        )
        $slug = if ($referenceParts.Count -ge 2 -and
            $referenceParts[-1] -in @('epic.md', 'epic-kickoff.md')) {
            [string]$referenceParts[-2]
        } elseif ($referenceParts.Count -gt 0) {
            [string]$referenceParts[-1]
        } else {
            ''
        }
        if (-not $slug -or $kickoffPath -ne "docs/features/epics/$slug/epic-kickoff.md") {
            return $false
        }
    } elseif (-not [string]::IsNullOrWhiteSpace($kickoffPath)) {
        return $false
    }
    $created = [datetimeoffset]::MinValue
    $expiry = [datetimeoffset]::MinValue
    if (-not [datetimeoffset]::TryParse([string]$Receipt.created_at, [ref]$created) -or
        -not [datetimeoffset]::TryParse([string]$Receipt.expires_at, [ref]$expiry) -or
        $created -gt $Now.AddMinutes(1) -or
        $expiry -le $created -or
        $expiry -gt $created.AddMinutes(60)) {
        return $false
    }
    return $expiry -gt $Now
}

function Get-CodexSubagentAttestation {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)] $Payload,
        [AllowNull()] $RootReceipt,
        [Parameter(Mandatory)][AllowEmptyCollection()][object[]] $Checkpoints,
        [Parameter(Mandatory)][string] $RepositoryRoot,
        [Parameter(Mandatory)][string] $CurrentHeadSha,
        [Parameter(Mandatory)][datetimeoffset] $Now
    )

    $agentId = [string]$Payload.agent_id
    $agentType = [string]$Payload.agent_type
    $model = [string]$Payload.model
    if (-not $agentId -or -not $agentType -or -not $model) {
        throw 'MODEL_ROUTING_ATTESTATION_BLOCKED: SubagentStart requires agent_id, agent_type, and model.'
    }
    $isEpic = $script:EpicPersonas -contains $agentType
    $rootAuthorized = if ($isEpic) {
        Test-RootEpicReceipt `
            -Receipt $RootReceipt `
            -Payload $Payload `
            -RepositoryRoot $RepositoryRoot `
            -CurrentHeadSha $CurrentHeadSha `
            -Now $Now
    } else {
        $null
    }

    $routingReceipt = $null
    $expectedModel = ''
    $expectedReasoningEffort = ''
    $agentProfile = $null
    $profileValidationError = $null
    $routingValid = $true
    if ($isEpic) {
        $expectedModel = 'gpt-5.6-sol'
        $expectedReasoningEffort = 'ultra'
    } elseif (Test-CodexRoutedAgentType -AgentType $agentType) {
        $routingReceipt = Find-CodexModelRoutingReceipt -AgentType $agentType -Checkpoints $Checkpoints
        if ($null -eq $routingReceipt) {
            $routingValid = $false
        } else {
            $receiptProperties = @($routingReceipt.PSObject.Properties.Name)
            if ($receiptProperties -contains 'model' -and
                $receiptProperties -contains 'model_reasoning_effort') {
                $expectedModel = [string]$routingReceipt.model
                $expectedReasoningEffort = [string]$routingReceipt.model_reasoning_effort
            }
        }
    }
    if ($isEpic -or (Test-CodexRoutedAgentType -AgentType $agentType)) {
        try {
            $agentProfile = Get-CodexAgentProfileAttestation `
                -RepositoryRoot $RepositoryRoot `
                -AgentType $agentType
            $routingValid = $routingValid -and (Test-CodexAgentProfileBinding `
                    -AgentProfile $agentProfile `
                    -AgentType $agentType `
                    -ActualModel $model `
                    -ExpectedModel $expectedModel `
                    -ExpectedReasoningEffort $expectedReasoningEffort)
        } catch {
            $routingValid = $false
            $profileValidationError = [string]$_
        }
    }
    $launchAuthorityValid = Test-CodexEpicChildRoutingLaunchAuthority -RoutingReceipt $routingReceipt -Payload $Payload -RepositoryRoot $RepositoryRoot
    $routingValid = $routingValid -and $launchAuthorityValid

    $provenanceValid = -not $isEpic -or [bool]$rootAuthorized
    return [ordered]@{
        schema_version              = 2
        session_id                  = [string]$Payload.session_id
        turn_id                     = [string]$Payload.turn_id
        agent_id                    = $agentId
        agent_type                  = $agentType
        transcript_path             = [string]$Payload.transcript_path
        attestation_key             = Get-SubagentAttestationKey -TranscriptPath ([string]$Payload.transcript_path) -AgentId $agentId
        actual_model                = $model
        expected_model              = $expectedModel
        actual_reasoning_effort     = if ($null -ne $agentProfile) { [string]$agentProfile.profile_reasoning_effort } else { $null }
        expected_reasoning_effort   = $expectedReasoningEffort
        profile_name                = if ($null -ne $agentProfile) { [string]$agentProfile.profile_name } else { $null }
        profile_model               = if ($null -ne $agentProfile) { [string]$agentProfile.profile_model } else { $null }
        profile_path                = if ($null -ne $agentProfile) { [string]$agentProfile.profile_path } else { $null }
        profile_sha256              = if ($null -ne $agentProfile) { [string]$agentProfile.profile_sha256 } else { $null }
        profile_validation_error    = $profileValidationError
        routing_valid               = $routingValid
        launch_authority_valid      = $launchAuthorityValid
        root_authorized             = $rootAuthorized
        root_entry_kind             = if ($rootAuthorized) { [string]$RootReceipt.entry_kind } else { $null }
        root_epic_reference         = if ($rootAuthorized) { [string]$RootReceipt.epic_reference } else { $null }
        root_kickoff_path           = if ($rootAuthorized) { [string]$RootReceipt.kickoff_path } else { $null }
        authority_repository_sha256 = Get-CodexAuthorityRepositoryKey -RepositoryRoot $RepositoryRoot
        provenance_valid            = $provenanceValid
        enforcement_marker          = if ($provenanceValid) { $null } else { 'EPIC_INVOCATION_ORIGIN_BLOCKED' }
        recorded_at                 = $Now.ToUniversalTime().ToString('o')
    }
}

function Write-CodexSubagentAttestation {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $RepositoryRoot,
        [Parameter(Mandatory)][System.Collections.IDictionary] $Attestation
    )

    $directory = Get-CodexAuthorityStateRoot `
        -RepositoryRoot $RepositoryRoot `
        -SessionId ([string]$Attestation.session_id)
    [System.IO.Directory]::CreateDirectory($directory) | Out-Null
    $path = Get-CodexAuthorityAttestationPath `
        -RepositoryRoot $RepositoryRoot `
        -SessionId ([string]$Attestation.session_id) `
        -AttestationKey ([string]$Attestation.attestation_key)
    $stream = [System.IO.File]::Open(
        $path,
        [System.IO.FileMode]::CreateNew,
        [System.IO.FileAccess]::Write,
        [System.IO.FileShare]::None
    )
    try {
        $writer = [System.IO.StreamWriter]::new(
            $stream,
            [System.Text.UTF8Encoding]::new($false)
        )
        try {
            $writer.Write(($Attestation | ConvertTo-Json -Depth 8))
            $writer.Flush()
        } finally {
            $writer.Dispose()
        }
    } finally {
        $stream.Dispose()
    }
    return $path
}

function Get-CodexSubagentAttestationFromAuthority {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)] $Payload,
        [Parameter(Mandatory)][string] $ReceiptPath,
        [Parameter(Mandatory)][AllowEmptyCollection()][object[]] $Checkpoints,
        [Parameter(Mandatory)][string] $RepositoryRoot,
        [Parameter(Mandatory)][string] $CurrentHeadSha,
        [Parameter(Mandatory)][datetimeoffset] $Now
    )

    $isEpic = $script:EpicPersonas -contains [string]$Payload.agent_type
    if (-not $isEpic -or -not (Test-Path -LiteralPath $ReceiptPath -PathType Leaf)) {
        return Get-CodexSubagentAttestation `
            -Payload $Payload `
            -RootReceipt $null `
            -Checkpoints $Checkpoints `
            -RepositoryRoot $RepositoryRoot `
            -CurrentHeadSha $CurrentHeadSha `
            -Now $Now
    }

    $stream = [System.IO.File]::Open(
        $ReceiptPath,
        [System.IO.FileMode]::Open,
        [System.IO.FileAccess]::ReadWrite,
        [System.IO.FileShare]::None
    )
    try {
        $reader = [System.IO.StreamReader]::new(
            $stream,
            [System.Text.Encoding]::UTF8,
            $true,
            1024,
            $true
        )
        try {
            $raw = $reader.ReadToEnd()
        } finally {
            $reader.Dispose()
        }
        $receipt = ConvertFrom-SubagentAttestationJson -Raw $raw -Name 'root receipt'
        $attestation = Get-CodexSubagentAttestation `
            -Payload $Payload `
            -RootReceipt $receipt `
            -Checkpoints $Checkpoints `
            -RepositoryRoot $RepositoryRoot `
            -CurrentHeadSha $CurrentHeadSha `
            -Now $Now
        if ($attestation.root_authorized -eq $true) {
            $receipt.consumed = $true
            $receipt.consumed_by = [string]$Payload.agent_id
            $receipt.consumed_at = $Now.ToUniversalTime().ToString('o')
            $stream.Position = 0
            $stream.SetLength(0)
            $writer = [System.IO.StreamWriter]::new(
                $stream,
                [System.Text.UTF8Encoding]::new($false),
                1024,
                $true
            )
            try {
                $writer.Write(($receipt | ConvertTo-Json -Depth 8))
                $writer.Flush()
                $stream.Flush($true)
            } finally {
                $writer.Dispose()
            }
        }
        return $attestation
    } finally {
        $stream.Dispose()
    }
}

if ($MyInvocation.InvocationName -eq '.') {
    return
}

try {
    $payload = ConvertFrom-SubagentAttestationJson -Raw ([Console]::In.ReadToEnd()) -Name 'SubagentStart input'
    $repositoryRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
    $stateRoot = Join-Path $repositoryRoot 'artifacts/orchestration'
    $receiptPath = Get-CodexAuthorityReceiptPath `
        -RepositoryRoot $repositoryRoot `
        -SessionId ([string]$payload.session_id) `
        -TurnId ([string]$payload.turn_id)
    $checkpoints = @()
    foreach ($name in @('epic-planner-state.json', 'epic-orchestrator-state.json', 'orchestrator-state.json')) {
        $path = Join-Path $stateRoot $name
        if (Test-Path -LiteralPath $path -PathType Leaf) {
            $parsed = ConvertFrom-SubagentAttestationJson -Raw (Get-Content -Raw -LiteralPath $path) -Name $name -Optional
            if ($null -ne $parsed) {
                $checkpoints += $parsed
            }
        }
    }
    $now = [datetimeoffset]::UtcNow
    $headSha = [string](& git -C $repositoryRoot rev-parse HEAD 2>$null)
    if ($LASTEXITCODE -ne 0 -or $headSha -notmatch '^[0-9a-fA-F]{40,64}$') {
        throw 'EPIC_INVOCATION_ORIGIN_BLOCKED: repository HEAD could not be resolved.'
    }
    $attestation = Get-CodexSubagentAttestationFromAuthority `
        -Payload $payload `
        -ReceiptPath $receiptPath `
        -Checkpoints $checkpoints `
        -RepositoryRoot $repositoryRoot `
        -CurrentHeadSha $headSha `
        -Now $now
    $attestationPath = Write-CodexSubagentAttestation -RepositoryRoot $repositoryRoot -Attestation $attestation
    if (-not $attestation.provenance_valid) {
        [ordered]@{
            systemMessage      = "EPIC_INVOCATION_ORIGIN_BLOCKED: $($payload.agent_type) requires a fresh root-session epic invocation receipt."
            hookSpecificOutput = [ordered]@{
                hookEventName     = 'SubagentStart'
                additionalContext = 'Do not mutate state. Report EPIC_INVOCATION_ORIGIN_BLOCKED and stop.'
            }
        } | ConvertTo-Json -Compress -Depth 5 | Write-Output
    } elseif (-not $attestation.routing_valid) {
        [ordered]@{
            systemMessage      = "MODEL_ROUTING_ATTESTATION_BLOCKED: actual model/profile does not match the persisted routing receipt. Attestation: $attestationPath"
            hookSpecificOutput = [ordered]@{
                hookEventName     = 'SubagentStart'
                additionalContext = 'Do not mutate state until the parent corrects the deployment profile and routing receipt.'
            }
        } | ConvertTo-Json -Compress -Depth 5 | Write-Output
    }
    exit 0
} catch {
    [Console]::Error.WriteLine([string]$_)
    exit 2
}
