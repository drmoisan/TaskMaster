<#
.SYNOPSIS
    SubagentStop hook that verifies the pr-author agent reported a created or updated PR.

.DESCRIPTION
    Invoked by the Claude Code SubagentStop hook for the pr-author matcher. Reads the SubagentStop
    envelope JSON from stdin through the shared envelope helper (subagentstop-envelope.ps1) and
    validates its last_assistant_message field, the pr-author agent's last assistant message. The
    hook allows (exit 0) when that message contains:
      - a GitHub PR URL (github.com/<owner>/<repo>/pull/<n>), or
      - a PR reference of the form "PR #<n>", or
      - a "gh pr create"/"gh pr edit" confirmation that includes a PR number.
    The hook blocks, writing the reason to stderr and exiting 2 through the shared envelope
    helper, when:
      - the envelope is empty, is not valid JSON, is not a JSON object, or carries no string
        last_assistant_message (reason prefix "PR_AUTHOR_OUTPUT_MALFORMED: payload anomaly"),
      - the last assistant message is blank (PR_AUTHOR_OUTPUT_EMPTY), or
      - the last assistant message contains no PR URL or PR number (PR_AUTHOR_OUTPUT_NO_PR).
    A hook dependency that fails to load blocks with exit 2 and the PR_AUTHOR_OUTPUT_MISSING:
    prefix.

.NOTES
    Compatible with PowerShell 7+. No external module dependencies.

    Enforcement strength: this validator is a policy guardrail, not a cryptographic or security
    control. It confirms that the pr-author agent's final output references a PR; it cannot verify
    that the referenced PR actually exists on GitHub, and the output text is forgeable by any actor
    that controls the agent transcript. It MUST NOT be described as tamper-proof or as a security
    boundary.
    An unhandled error during validation blocks with exit 2 and a reason (issue #928).
#>
[CmdletBinding()]
param()
$script:HookDependencyGuardLoadFailed = $false; try { . (Join-Path $PSScriptRoot 'hook-dependency-guard.ps1') } catch { $script:HookDependencyGuardLoadFailed = $true }
try { . (Join-Path $PSScriptRoot 'subagentstop-envelope.ps1') } catch { Add-HookDependencyFailure -Name 'subagentstop-envelope.ps1' -ErrorRecord $_ }

function Test-PrAuthorOutputReportsPr {
    <#
    .SYNOPSIS
        Return $true when the supplied output text reports a created or updated PR.
    .DESCRIPTION
        Detection helper (injectable boundary for tests). Matches a GitHub PR URL, a "PR #<n>"
        reference, or a "gh pr create"/"gh pr edit" confirmation that contains a PR number.
    .PARAMETER OutputText
        The pr-author agent's last assistant message text.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [string] $OutputText
    )

    if ([string]::IsNullOrWhiteSpace($OutputText)) {
        return $false
    }

    # GitHub PR URL, e.g. https://github.com/owner/repo/pull/123
    if ($OutputText -match '(?i)github\.com/[^\s/]+/[^\s/]+/pull/\d+') {
        return $true
    }

    # Explicit PR number reference, e.g. "PR #123".
    if ($OutputText -match '(?i)\bPR\s*#\d+') {
        return $true
    }

    # gh pr create / gh pr edit confirmation that includes a PR number anywhere in the output.
    if (($OutputText -match '(?i)\bgh\s+pr\s+(create|edit)\b') -and ($OutputText -match '#\d+')) {
        return $true
    }

    return $false
}

function Get-PrAuthorOutputDecision {
    <#
    .SYNOPSIS
        Read the SubagentStop envelope and return an allow-or-block decision.
    .DESCRIPTION
        Returns an ordered dictionary with 'allowed' (bool) and 'reason' (string). Allowed is $true
        only when the envelope's last assistant message reports a PR per
        Test-PrAuthorOutputReportsPr. An invalid envelope (payload anomaly), a blank last assistant
        message, and a message without a PR reference all yield allowed = $false.
    .PARAMETER InputText
        The SubagentStop envelope JSON. When the parameter is not bound, Read-SubagentStopEnvelope
        reads the envelope from stdin.
    .OUTPUTS
        System.Collections.Specialized.OrderedDictionary
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [AllowNull()] [AllowEmptyString()] [string] $InputText
    )
    $dependencyDecision = Get-HookDependencyFailureDecision -HookEvent SubagentStop -ReasonPrefix 'PR_AUTHOR_OUTPUT_MISSING:'
    if ($null -ne $dependencyDecision) { return $dependencyDecision }
    $envelope = if ($PSBoundParameters.ContainsKey('InputText')) { Read-SubagentStopEnvelope -InputText $InputText } else { Read-SubagentStopEnvelope }
    if (-not $envelope.IsValid) { return [ordered]@{ allowed = $false; reason = "PR_AUTHOR_OUTPUT_MALFORMED: payload anomaly - $(Get-SubagentStopEnvelopeAnomalyReason -Anomaly $envelope.Anomaly)" } }
    $assistantMessage = [string]$envelope.AssistantText

    if ([string]::IsNullOrWhiteSpace($assistantMessage)) {
        return [ordered]@{
            allowed = $false
            reason  = 'PR_AUTHOR_OUTPUT_EMPTY: the pr-author last assistant message is empty; it must report the created or updated PR URL or number.'
        }
    }

    if (Test-PrAuthorOutputReportsPr -OutputText $assistantMessage) {
        return [ordered]@{ allowed = $true }
    }

    return [ordered]@{
        allowed = $false
        reason  = 'PR_AUTHOR_OUTPUT_NO_PR: the pr-author last assistant message does not reference a PR URL or PR number; it must report the created or updated pull request.'
    }
}

# Allow dot-sourcing in tests without executing the entrypoint.
if ($MyInvocation.InvocationName -eq '.') {
    return
}
if ($script:HookDependencyGuardLoadFailed) { [Console]::Error.WriteLine('PR_AUTHOR_OUTPUT_MISSING: hook-dependency-guard.ps1 failed to load; the gate fails closed.'); exit 2 }
$dependencyDecision = Get-HookDependencyFailureDecision -HookEvent SubagentStop -ReasonPrefix 'PR_AUTHOR_OUTPUT_MISSING:'
if ($null -ne $dependencyDecision) { [Console]::Error.WriteLine($dependencyDecision.Reason); exit $dependencyDecision.ExitCode }

try { $decision = Get-PrAuthorOutputDecision }
catch { Exit-SubagentStopBlock -Message ('PR_AUTHOR_OUTPUT_MISSING: validator error: ' + ([string]$_.Exception.Message -split "`r?`n")[0] + '; the gate fails closed.' + "`n" + 'This condition is not repairable by the subagent. Report it to the operator and halt.') }
if (-not $decision['allowed']) { Exit-SubagentStopBlock -Message $decision['reason'] }
exit 0
