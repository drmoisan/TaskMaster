<#
.SYNOPSIS
    Records a bounded root-session authorization receipt for Codex epic personas.

.DESCRIPTION
    UserPromptSubmit hook. Reads the documented Codex JSON payload from stdin. Exact epic skill
    commands and direct requests to invoke an epic persona produce one turn-bound receipt in the
    non-workspace Codex authority store. Non-epic prompts are ignored. The receipt is consumed by
    the SubagentStart attestation hook.
#>
[CmdletBinding()]
param()

. (Join-Path $PSScriptRoot 'codex-authority-store.ps1')

$script:ReceiptLifetimeMinutes = 60

function ConvertFrom-RootEpicHookPayload {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string] $PayloadRaw)

    if ([string]::IsNullOrWhiteSpace($PayloadRaw)) {
        throw 'EPIC_INVOCATION_ORIGIN_BLOCKED: UserPromptSubmit hook input is empty.'
    }
    try {
        return $PayloadRaw | ConvertFrom-Json -ErrorAction Stop
    } catch {
        throw "EPIC_INVOCATION_ORIGIN_BLOCKED: UserPromptSubmit hook input is malformed JSON: $_"
    }
}

function Get-RootEpicRequestedPersona {
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $Prompt)

    if ($Prompt -match '(?im)^\s*(?:/|\$)?epic-plan(?:\s+|$)' -or
        $Prompt -match '(?im)^\s*(?:please\s+)?(?:invoke|spawn|use|run|delegate(?:\s+this)?\s+to)\s+(?:the\s+)?(?:Codex\s+)?epic-planner\b') {
        return 'epic-planner'
    }
    if ($Prompt -match '(?im)^\s*(?:/|\$)?epic-(?:run|orchestrate)(?:\s+|$)' -or
        $Prompt -match '(?im)^\s*(?:please\s+)?(?:invoke|spawn|use|run|delegate(?:\s+this)?\s+to)\s+(?:the\s+)?(?:Codex\s+)?epic-orchestrator\b') {
        return 'epic-orchestrator'
    }
    return ''
}

function Get-RootEpicReference {
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $Prompt)

    if ($Prompt -match '(?im)^\s*(?:/|\$)?epic-(?:plan|run|orchestrate)\s+(?<reference>[^\r\n]+)') {
        return $Matches.reference.Trim()
    }
    if ($Prompt -match '(?i)(?<reference>docs[\\/]features[\\/]epics[\\/][^\s"'']+)') {
        return ($Matches.reference -replace '\\', '/').TrimEnd('.', ',', ';')
    }
    return ''
}

function Get-RootEpicEntryKind {
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $Prompt)

    if ($Prompt -match '(?im)^\s*(?:/|\$)?epic-plan(?:\s+|$)') {
        return 'epic-plan'
    }
    if ($Prompt -match '(?im)^\s*(?:/|\$)?epic-run(?:\s+|$)') {
        return 'epic-run'
    }
    if ($Prompt -match '(?im)^\s*(?:/|\$)?epic-orchestrate(?:\s+|$)') {
        return 'epic-orchestrate'
    }
    return 'direct'
}

function Get-RootEpicSlug {
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $Reference)

    $parts = @(($Reference -replace '\\', '/').Trim('/').Split('/') | Where-Object { $_ })
    if ($parts.Count -eq 0) {
        return ''
    }
    if ($parts[-1] -in @('epic.md', 'epic-kickoff.md') -and $parts.Count -ge 2) {
        return [string]$parts[-2]
    }
    return [string]$parts[-1]
}

function Get-LowerSha256 {
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $Text)

    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
    return [Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
}

function Get-RootEpicReceiptFileName {
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][string] $TurnId)

    $safeTurnId = $TurnId -replace '[^A-Za-z0-9._-]', '_'
    return "epic-root-invocation.$safeTurnId.json"
}

function Get-RootEpicInvocationReceipt {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)] $Payload,
        [Parameter(Mandatory)][datetimeoffset] $Now,
        [Parameter(Mandatory)][string] $RepositoryRoot,
        [Parameter(Mandatory)][string] $HeadSha
    )

    $prompt = [string]$Payload.prompt
    $persona = Get-RootEpicRequestedPersona -Prompt $prompt
    if (-not $persona) {
        return $null
    }

    $turnId = [string]$Payload.turn_id
    $sessionId = [string]$Payload.session_id
    if ([string]::IsNullOrWhiteSpace($turnId) -or [string]::IsNullOrWhiteSpace($sessionId)) {
        throw 'EPIC_INVOCATION_ORIGIN_BLOCKED: an epic root invocation requires session_id and turn_id.'
    }

    $reference = Get-RootEpicReference -Prompt $prompt
    $entryKind = Get-RootEpicEntryKind -Prompt $prompt
    $kickoffPath = ''
    if ($persona -eq 'epic-orchestrator' -and $entryKind -eq 'epic-run') {
        $slug = Get-RootEpicSlug -Reference $reference
        if ($slug) {
            $kickoffPath = "docs/features/epics/$slug/epic-kickoff.md"
        }
    }

    return [ordered]@{
        schema_version      = 1
        repository_root     = Get-CodexCanonicalAuthorityPath -Path $RepositoryRoot
        repository_sha256   = Get-CodexAuthorityRepositoryKey -RepositoryRoot $RepositoryRoot
        repository_head_sha = $HeadSha
        session_id          = $sessionId
        turn_id             = $turnId
        prompt_sha256       = Get-LowerSha256 -Text $prompt
        requested_persona   = $persona
        entry_kind          = $entryKind
        epic_reference      = $reference
        kickoff_path        = $kickoffPath
        created_at          = $Now.ToUniversalTime().ToString('o')
        expires_at          = $Now.AddMinutes($script:ReceiptLifetimeMinutes).ToUniversalTime().ToString('o')
        consumed            = $false
        consumed_by         = $null
        consumed_at         = $null
    }
}

function Write-RootEpicInvocationReceipt {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $RepositoryRoot,
        [Parameter(Mandatory)][System.Collections.IDictionary] $Receipt
    )

    $directory = Get-CodexAuthorityStateRoot `
        -RepositoryRoot $RepositoryRoot `
        -SessionId ([string]$Receipt.session_id)
    [System.IO.Directory]::CreateDirectory($directory) | Out-Null
    $path = Get-CodexAuthorityReceiptPath `
        -RepositoryRoot $RepositoryRoot `
        -SessionId ([string]$Receipt.session_id) `
        -TurnId ([string]$Receipt.turn_id)
    $json = $Receipt | ConvertTo-Json -Depth 8
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
            $writer.Write($json)
            $writer.Flush()
        } finally {
            $writer.Dispose()
        }
    } finally {
        $stream.Dispose()
    }
    return $path
}

if ($MyInvocation.InvocationName -eq '.') {
    return
}

try {
    $payload = ConvertFrom-RootEpicHookPayload -PayloadRaw ([Console]::In.ReadToEnd())
    if (-not (Get-RootEpicRequestedPersona -Prompt ([string]$payload.prompt))) {
        exit 0
    }
    $repositoryRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
    $headSha = [string](& git -C $repositoryRoot rev-parse HEAD 2>$null)
    if ($LASTEXITCODE -ne 0 -or $headSha -notmatch '^[0-9a-fA-F]{40,64}$') {
        throw 'EPIC_INVOCATION_ORIGIN_BLOCKED: repository HEAD could not be resolved.'
    }
    $receipt = Get-RootEpicInvocationReceipt `
        -Payload $payload `
        -Now ([datetimeoffset]::UtcNow) `
        -RepositoryRoot $repositoryRoot `
        -HeadSha $headSha
    if ($null -eq $receipt) {
        exit 0
    }
    $receiptPath = Write-RootEpicInvocationReceipt -RepositoryRoot $repositoryRoot -Receipt $receipt
    [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName     = 'UserPromptSubmit'
            additionalContext = "Root epic invocation authorized for $($receipt.requested_persona); receipt: $receiptPath"
        }
    } | ConvertTo-Json -Compress -Depth 5 | Write-Output
    exit 0
} catch {
    [Console]::Error.WriteLine([string]$_)
    exit 2
}
