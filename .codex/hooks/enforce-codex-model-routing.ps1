<#
.SYNOPSIS
    Denies covered subagent mutations when the actual Codex model/profile does not match routing.
#>
[CmdletBinding()]
param()

. (Join-Path $PSScriptRoot 'codex-authority-store.ps1')
. (Join-Path $PSScriptRoot 'codex-agent-profile-attestation.ps1')

$script:CodexModelGateRepositoryRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent

function ConvertFrom-CodexModelGateJson {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string] $Raw, [Parameter(Mandatory)][string] $Name)

    if ([string]::IsNullOrWhiteSpace($Raw)) {
        throw "MODEL_ROUTING_ATTESTATION_BLOCKED: $Name is empty."
    }
    try {
        return $Raw | ConvertFrom-Json -ErrorAction Stop
    } catch {
        throw "MODEL_ROUTING_ATTESTATION_BLOCKED: $Name is malformed JSON: $_"
    }
}

function Get-CodexModelGateAttestationKey {
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][string] $TranscriptPath)

    $bytes = [System.Text.Encoding]::UTF8.GetBytes($TranscriptPath)
    return [Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
}

function Test-CodexModelGateProfileAttestation {
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)] $Payload,
        [Parameter(Mandatory)] $Attestation,
        [Parameter(Mandatory)][string] $RepositoryRoot
    )

    if ($Attestation.schema_version -isnot [ValueType] -or
        [int]$Attestation.schema_version -ne 2 -or
        $Attestation.routing_valid -isnot [bool] -or
        -not [bool]$Attestation.routing_valid -or
        -not [string]::IsNullOrWhiteSpace([string]$Attestation.profile_validation_error)) {
        return $false
    }
    foreach ($requiredField in @(
            'agent_type', 'actual_model', 'expected_model',
            'actual_reasoning_effort', 'expected_reasoning_effort',
            'profile_name', 'profile_model', 'profile_path', 'profile_sha256'
        )) {
        if ([string]::IsNullOrWhiteSpace([string]$Attestation.$requiredField)) {
            return $false
        }
    }
    if ([string]$Attestation.profile_sha256 -notmatch '^[0-9a-f]{64}$') {
        return $false
    }
    try {
        $agentProfile = Get-CodexAgentProfileAttestation `
            -RepositoryRoot $RepositoryRoot `
            -AgentType ([string]$Attestation.agent_type)
    } catch {
        return $false
    }
    if (-not (Test-CodexAgentProfileBinding `
                -AgentProfile $agentProfile `
                -AgentType ([string]$Attestation.agent_type) `
                -ActualModel ([string]$Payload.model) `
                -ExpectedModel ([string]$Attestation.expected_model) `
                -ExpectedReasoningEffort ([string]$Attestation.expected_reasoning_effort) `
                -ExpectedProfilePath ([string]$Attestation.profile_path) `
                -ExpectedProfileSha256 ([string]$Attestation.profile_sha256))) {
        return $false
    }
    return [string]$Payload.model -ceq [string]$Attestation.actual_model -and
    [string]$Attestation.profile_name -ceq [string]$agentProfile.profile_name -and
    [string]$Attestation.profile_model -ceq [string]$agentProfile.profile_model -and
    [string]$Attestation.actual_reasoning_effort -ceq [string]$agentProfile.profile_reasoning_effort
}

function Invoke-CodexModelRoutingDecision {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)][string] $PayloadRaw,
        [AllowNull()][AllowEmptyString()][string] $AttestationRaw
    )

    $payload = ConvertFrom-CodexModelGateJson -Raw $PayloadRaw -Name 'PreToolUse input'
    if ([string]::IsNullOrWhiteSpace($AttestationRaw)) {
        if (-not [string]::IsNullOrWhiteSpace([string]$payload.agent_type) -and
            (Test-CodexModelGateAgentType -AgentType ([string]$payload.agent_type))) {
            return Get-CodexModelGateDenyDecision -Reason (
                "MODEL_ROUTING_ATTESTATION_BLOCKED: routed agent '$($payload.agent_type)' has " +
                'no matching SubagentStart attestation. Relaunch the exact generated profile.'
            )
        }
        return $null
    }

    $attestation = ConvertFrom-CodexModelGateJson -Raw $AttestationRaw -Name 'routing attestation'
    if (Test-CodexModelGateProfileAttestation `
            -Payload $payload `
            -Attestation $attestation `
            -RepositoryRoot $script:CodexModelGateRepositoryRoot) {
        return $null
    }

    return Get-CodexModelGateDenyDecision -Reason (
        "MODEL_ROUTING_ATTESTATION_BLOCKED: agent '$($attestation.agent_type)' has model, " +
        'reasoning, or profile drift from its persisted deployment receipt. Correct the ' +
        'receipt and relaunch the exact generated profile.'
    )
}

function Test-CodexModelGateAgentType {
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][string] $AgentType)

    return $AgentType -match '-c(?:1|2|3|4|3-elevated)$' -or @(
        'orchestrator', 'atomic-planner', 'atomic-executor', 'feature-review',
        'feature-reviewer', 'task-researcher', 'prd-feature', 'pr-author',
        'python-typed-engineer', 'powershell-typed-engineer',
        'csharp-typed-engineer', 'typescript-engineer',
        'epic-planner', 'epic-orchestrator'
    ) -contains $AgentType
}

function Get-CodexModelGateDenyDecision {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param([Parameter(Mandatory)][string] $Reason)

    return [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName            = 'PreToolUse'
            permissionDecision       = 'deny'
            permissionDecisionReason = $Reason
        }
    }
}

if ($MyInvocation.InvocationName -eq '.') {
    return
}

try {
    $payloadRaw = [Console]::In.ReadToEnd()
    $payload = ConvertFrom-CodexModelGateJson -Raw $payloadRaw -Name 'PreToolUse input'
    $attestationRaw = ''
    $repositoryRoot = $script:CodexModelGateRepositoryRoot
    $sessionId = [string]$payload.session_id
    $transcriptPath = [string]$payload.transcript_path
    if (-not [string]::IsNullOrWhiteSpace($transcriptPath) -and
        -not [string]::IsNullOrWhiteSpace($sessionId)) {
        $key = Get-CodexModelGateAttestationKey -TranscriptPath $transcriptPath
        $path = Get-CodexAuthorityAttestationPath `
            -RepositoryRoot $repositoryRoot `
            -SessionId $sessionId `
            -AttestationKey $key
        if (Test-Path -LiteralPath $path -PathType Leaf) {
            $attestationRaw = Get-Content -Raw -LiteralPath $path
        }
    }
    if ([string]::IsNullOrWhiteSpace($attestationRaw) -and
        -not [string]::IsNullOrWhiteSpace([string]$payload.agent_id) -and
        -not [string]::IsNullOrWhiteSpace($sessionId)) {
        $stateRoot = Get-CodexAuthorityStateRoot `
            -RepositoryRoot $repositoryRoot `
            -SessionId $sessionId
        foreach ($path in Get-ChildItem -LiteralPath $stateRoot -Filter 'codex-routing-attestation.*.json' -File -ErrorAction SilentlyContinue) {
            $candidate = Get-Content -Raw -LiteralPath $path.FullName
            try {
                if ([string]($candidate | ConvertFrom-Json -ErrorAction Stop).agent_id -eq [string]$payload.agent_id) {
                    $attestationRaw = $candidate
                    break
                }
            } catch {
                continue
            }
        }
    }
    $decision = Invoke-CodexModelRoutingDecision -PayloadRaw $payloadRaw -AttestationRaw $attestationRaw
    if ($null -ne $decision) {
        $decision | ConvertTo-Json -Compress -Depth 5 | Write-Output
    }
    exit 0
} catch {
    [Console]::Error.WriteLine([string]$_)
    exit 2
}
