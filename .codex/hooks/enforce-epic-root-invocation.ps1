<#
.SYNOPSIS
    Denies covered mutations by an epic persona without valid root provenance.

.DESCRIPTION
    PreToolUse hook. Reads Codex stdin JSON and the SubagentStart attestation keyed by transcript
    path. A matching epic attestation must carry literal provenance_valid true.
#>
[CmdletBinding()]
param()

. (Join-Path $PSScriptRoot 'codex-authority-store.ps1')

function ConvertFrom-EpicRootGatePayload {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string] $Raw, [Parameter(Mandatory)][string] $Name)

    if ([string]::IsNullOrWhiteSpace($Raw)) {
        throw "EPIC_INVOCATION_ORIGIN_BLOCKED: $Name is empty."
    }
    try {
        return $Raw | ConvertFrom-Json -ErrorAction Stop
    } catch {
        throw "EPIC_INVOCATION_ORIGIN_BLOCKED: $Name is malformed JSON: $_"
    }
}

function Get-EpicRootGateAttestationKey {
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][string] $TranscriptPath)

    $bytes = [System.Text.Encoding]::UTF8.GetBytes($TranscriptPath)
    return [Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
}

function Get-EpicRootGateDenyDecision {
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

function Invoke-EpicRootInvocationDecision {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $PayloadRaw,
        [AllowNull()][AllowEmptyString()][string] $AttestationRaw
    )

    $payload = ConvertFrom-EpicRootGatePayload -Raw $PayloadRaw -Name 'PreToolUse input'
    if ([string]::IsNullOrWhiteSpace($AttestationRaw)) {
        if (@('epic-planner', 'epic-orchestrator') -contains [string]$payload.agent_type) {
            return Get-EpicRootGateDenyDecision -Reason (
                "EPIC_INVOCATION_ORIGIN_BLOCKED: $($payload.agent_type) has no matching " +
                'SubagentStart attestation. Relaunch it from a fresh root-session epic invocation.'
            )
        }
        return $null
    }

    $attestation = ConvertFrom-EpicRootGatePayload -Raw $AttestationRaw -Name 'routing attestation'
    if (@('epic-planner', 'epic-orchestrator') -notcontains [string]$attestation.agent_type) {
        return $null
    }
    $properties = @($attestation.PSObject.Properties.Name)
    if ($properties -contains 'provenance_valid' -and
        $attestation.provenance_valid -is [bool] -and
        [bool]$attestation.provenance_valid) {
        return $null
    }

    return Get-EpicRootGateDenyDecision -Reason (
        "EPIC_INVOCATION_ORIGIN_BLOCKED: $($attestation.agent_type) was not authorized by a " +
        'fresh, single-use root-session epic invocation receipt. Invoke epic-plan, epic-run, ' +
        'or epic-orchestrate from the root session.'
    )
}

if ($MyInvocation.InvocationName -eq '.') {
    return
}

try {
    $payloadRaw = [Console]::In.ReadToEnd()
    $payload = ConvertFrom-EpicRootGatePayload -Raw $payloadRaw -Name 'PreToolUse input'
    $attestationRaw = ''
    $repositoryRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
    $sessionId = [string]$payload.session_id
    $transcriptPath = [string]$payload.transcript_path
    if (-not [string]::IsNullOrWhiteSpace($transcriptPath) -and
        -not [string]::IsNullOrWhiteSpace($sessionId)) {
        $key = Get-EpicRootGateAttestationKey -TranscriptPath $transcriptPath
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
    $decision = Invoke-EpicRootInvocationDecision -PayloadRaw $payloadRaw -AttestationRaw $attestationRaw
    if ($null -ne $decision) {
        $decision | ConvertTo-Json -Compress -Depth 5 | Write-Output
    }
    exit 0
} catch {
    [Console]::Error.WriteLine([string]$_)
    exit 2
}
