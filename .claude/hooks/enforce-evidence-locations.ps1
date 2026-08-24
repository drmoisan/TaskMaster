<#
.SYNOPSIS
    Pre-tool-use hook that blocks writes to non-canonical evidence storage locations.

.DESCRIPTION
    This script is invoked by the Claude Code PreToolUse hook before any Write or Edit
    operation. It acquires the hook payload through the shared reader and reads
    file_path from the envelope's nested tool_input, then rejects the operation when
    the target path is a non-canonical evidence location.

    Forbidden path prefixes (case-sensitive, normalized to forward-slash):
      - artifacts/baselines/
      - artifacts/baseline/
      - artifacts/qa/
      - artifacts/qa-gates/
      - artifacts/coverage/
      - artifacts/evidence/
      - artifacts/regression-testing/
      - artifacts/post-change/
      - artifacts/research/

    All other paths pass through, including canonical evidence paths of the form
    <FEATURE>/evidence/<kind>/ and permitted artifacts/ sub-paths such as
    artifacts/orchestration/, artifacts/pr_context, artifacts/reviews/,
    artifacts/status/, artifacts/python/, artifacts/pester/, and
    artifacts/csharp/. Research output is no longer an artifacts/ sub-path; it
    is written to the tracked roots docs/features/<feature>/research/
    (feature-associated) or docs/research/ (one-off).

    If the file_path resolves to a forbidden prefix, the script writes a PreToolUse JSON
    response to stdout with hookSpecificOutput.permissionDecision = 'deny' and exits with
    code 0 so Claude Code surfaces the reason. For allowed paths, a PreToolUse response
    with permissionDecision = 'allow' is written to stdout and the script exits 0. On hard
    failure the script still exits 0 with a deny decision: exit 1 is non-blocking for
    PreToolUse, so an envelope anomaly (empty payload, unparseable JSON, missing or
    malformed tool_input) is emitted as a deny rather than raised.

.NOTES
    Compatible with PowerShell 7+.
    This script must not modify any state; it is a read-only validation gate.
#>
[CmdletBinding()]
param()

Import-Module (Join-Path $PSScriptRoot '../lib/hook-payload/HookPayload.psm1') -Force

function Test-EvidenceLocationForbidden {
    <#
    .SYNOPSIS
        Returns $true when the supplied file path targets a forbidden evidence sub-path.
    .PARAMETER FilePath
        The raw file_path value from the Claude Code tool-input JSON.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)]
        [string] $FilePath
    )

    # Normalize separators so both absolute Windows paths and relative POSIX paths match.
    $normalized = $FilePath -replace '\\', '/'

    $forbiddenPrefixes = @(
        'artifacts/baselines/',
        'artifacts/baseline/',
        'artifacts/qa/',
        'artifacts/qa-gates/',
        'artifacts/coverage/',
        'artifacts/evidence/',
        'artifacts/regression-testing/',
        'artifacts/post-change/',
        'artifacts/research/'
    )

    # Match the prefix either at the start of the string or after any directory separator,
    # to handle both relative and absolute path forms.
    foreach ($prefix in $forbiddenPrefixes) {
        $escapedPrefix = [regex]::Escape($prefix)
        if ($normalized -match "(^|/)$escapedPrefix") {
            return $true
        }
    }

    return $false
}

function Get-EvidenceLocationBlockDecision {
    <#
    .SYNOPSIS
        Constructs a deny-decision ordered dictionary for the supplied forbidden path.
    .PARAMETER FilePath
        The file path that triggered the deny decision.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)]
        [string] $FilePath
    )

    [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName            = 'PreToolUse'
            permissionDecision       = 'deny'
            permissionDecisionReason = "EVIDENCE_LOCATION_BLOCKED: '$FilePath' is not a canonical evidence location. Use <FEATURE>/evidence/<kind>/ instead. See .claude/skills/evidence-and-timestamp-conventions/SKILL.md for the canonical scheme."
        }
    }
}

function Get-EvidenceLocationAnomalyDecision {
    <#
    .SYNOPSIS
        Build the fail-closed deny decision for an unreadable PreToolUse envelope.
    .PARAMETER Anomaly
        The anomaly code reported by the shared payload reader.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [AllowNull()]
        [AllowEmptyString()]
        [string] $Anomaly
    )

    return [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName            = 'PreToolUse'
            permissionDecision       = 'deny'
            permissionDecisionReason = 'EVIDENCE_LOCATION_BLOCKED: payload anomaly - ' +
            (Get-ClaudeHookPayloadAnomalyReason -Anomaly $Anomaly) +
            '. The gate fails closed on an envelope it cannot read.'
        }
    }
}

function Invoke-EvidenceLocationDecision {
    <#
    .SYNOPSIS
        Parses the Claude Code tool-input JSON and returns an allow-or-block decision.
    .PARAMETER ToolInputRaw
        The raw JSON hook payload acquired by Read-ClaudeHookRawPayload. An envelope
        anomaly fails closed as a deny; a well-formed tool_input carrying no file_path
        remains an allow (non-file tool calls have no file_path).
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [AllowNull()]
        [AllowEmptyString()]
        [string] $ToolInputRaw
    )

    $payload = Resolve-ClaudeHookToolInput -Raw $ToolInputRaw
    if (-not $payload.IsValid) {
        return Get-EvidenceLocationAnomalyDecision -Anomaly $payload.Anomaly
    }

    $filePath = Get-ClaudeHookToolInputString -ToolInput $payload.Value -Name 'file_path'
    if (-not $filePath) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
    }

    if (Test-EvidenceLocationForbidden -FilePath $filePath) {
        return Get-EvidenceLocationBlockDecision -FilePath $filePath
    }

    return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
}

function Invoke-EvidenceLocationEntryPoint {
    <#
    .SYNOPSIS
        Runs the evidence-location decision and returns the process exit code.
    .DESCRIPTION
        Wraps the dispatch logic that the hook entry point performs so it can be
        exercised by unit tests. It acquires the payload through the shared reader
        unless the caller supplies one, writes the compact JSON decision to the output
        stream, and returns 0. It never returns 1: exit 1 is non-blocking for
        PreToolUse, so every envelope anomaly is already a deny decision by the time
        control reaches here. This function does not call exit; the thin entry-point
        wiring converts the returned code into a process exit.
    .PARAMETER ToolInputRaw
        Optional pre-acquired payload text. When omitted the ReadPayload seam runs.
    .PARAMETER ReadPayload
        Seam for payload acquisition, so tests can drive the empty-on-all-transports
        case without touching a console.
    #>
    [CmdletBinding()]
    [OutputType([int])]
    param(
        [AllowNull()]
        [AllowEmptyString()]
        [string] $ToolInputRaw,

        [scriptblock] $ReadPayload = { Read-ClaudeHookRawPayload }
    )

    if (-not $PSBoundParameters.ContainsKey('ToolInputRaw')) {
        $ToolInputRaw = [string](& $ReadPayload)
    }

    $decision = Invoke-EvidenceLocationDecision -ToolInputRaw $ToolInputRaw

    $decision | ConvertTo-Json -Compress -Depth 5 | Write-Output

    return 0
}

# Guard allows dot-sourcing in tests without executing the entrypoint.
if ($MyInvocation.InvocationName -eq '.') {
    return
}

# The entry point returns its [int] exit code as the last pipeline element and the
# decision JSON before it. `exit (<call>)` would capture BOTH into the exit
# expression and emit nothing, so the decision is written explicitly here first.
$entryPointResult = @(Invoke-EvidenceLocationEntryPoint)
if ($entryPointResult.Count -gt 1) {
    $entryPointResult[0..($entryPointResult.Count - 2)] | Write-Output
}

exit ([int]$entryPointResult[-1])