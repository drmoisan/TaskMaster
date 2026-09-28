
<#
.SYNOPSIS
    Codex PreToolUse hook that blocks dangerous shell commands.

.DESCRIPTION
    Reads one Codex hook payload from stdin, validates tool_input.command, and emits the current
    PreToolUse deny envelope for destructive operations. Malformed hook input fails closed with
    exit code 2 and the reason on stderr. Safe commands emit no output and exit 0.

.NOTES
    Compatible with PowerShell 7+. This hook is read-only.
#>
[CmdletBinding()]
param()

# Shared command-line parser (issue #545). The denylist comparison below runs against the
# segment list rather than against the unsegmented command string, which is what keeps the
# two runtimes on one implementation of the same concern. This copy has no cd-chained leg
# and none is added.
. (Join-Path $PSScriptRoot 'hook-command-scanner.ps1')
. (Join-Path $PSScriptRoot 'hook-command-invocation.ps1')

function Get-BlockedBashPattern {
    [CmdletBinding()]
    [OutputType([string[]])]
    param()

    return [string[]]@(
        'rm -rf',
        'git push --force',
        'git push origin --force',
        'Remove-Item -Recurse -Force',
        'git reset --hard',
        'git push -f'
    )
}

function Test-BlockedPatternTokenRun {
    <#
    .SYNOPSIS
        Report whether a literal's token sequence occurs as a contiguous run in a token list.
    .DESCRIPTION
        Whole-token equality, element by element. This is the comparison primitive that
        replaces String.Contains. It is what makes '--force-with-lease' stop matching the
        literal 'git push --force': the two are different tokens, whereas one is a substring
        of the other.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][string[]] $Token,
        [Parameter(Mandatory)][ValidateNotNullOrEmpty()][string[]] $PatternToken
    )

    if ($PatternToken.Count -eq 0 -or $Token.Count -lt $PatternToken.Count) {
        return $false
    }

    for ($start = 0; $start -le $Token.Count - $PatternToken.Count; $start++) {
        $matched = $true
        for ($offset = 0; $offset -lt $PatternToken.Count; $offset++) {
            if ($Token[$start + $offset] -ne $PatternToken[$offset]) {
                $matched = $false
                break
            }
        }
        if ($matched) {
            return $true
        }
    }

    return $false
}

function Get-BlockedStructuralGitMatch {
    <#
    .SYNOPSIS
        Report the denylist literal a segment's structural git invocation stands for, or $null.
    .DESCRIPTION
        The flag conjunction is required rather than optional. Classifying on the bare
        subcommand would deny 'git push --force-with-lease origin HEAD', which spec Test
        Strategy row AT-8 requires to allow, and would deny 'git reset --soft HEAD~1'.
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)][AllowEmptyString()][string] $SegmentText,
        [Parameter(Mandatory)][AllowEmptyCollection()][string[]] $Token
    )

    if (Test-CommandLineInvocation -CommandText $SegmentText -CommandWord 'git' -SubcommandPath @('push')) {
        if ($Token -contains '--force') { return 'git push --force' }
        if ($Token -contains '-f') { return 'git push -f' }
    }
    if (Test-CommandLineInvocation -CommandText $SegmentText -CommandWord 'git' -SubcommandPath @('reset')) {
        if ($Token -contains '--hard') { return 'git reset --hard' }
    }

    return $null
}

function Get-BlockedPatternMatch {
    <#
    .SYNOPSIS
        Return the denylist literal a command matches, or $null.
    .DESCRIPTION
        The Codex sibling of the Claude copy's detector, with the same two legs in the same
        order over the segment list produced by Read-CommandLineSegment.

        Leg 1 (literal): each of the six byte-unchanged literals is split into its own
        whitespace-delimited token sequence and reported as a match when that sequence occurs
        as a CONTIGUOUS RUN inside a segment's Tokens, every element compared by whole-token
        equality. The value returned is the literal string itself, in declaration order.

        Leg 1 has a second condition, the wrapper carve-out of D2 Piece 2 together with the two
        other clauses under which the scanner selects raw scan text. A wrapper's quoted argument is
        a nested command line rather than inert data, and ConvertTo-CommandLineToken collapses a
        balanced quoted span into ONE token, so a multi-token literal can never form a contiguous
        token run inside it. The same masking hides a literal carried inside a quoted span or a
        heredoc that never closes. For a segment that is wrapper-led, that carries a live
        substitution, or whose quoting or heredoc did not close, the scanner already selects
        RawText as ScanText, and this leg reads that field with an Ordinal IndexOf, matching the
        culture-insensitive String.Contains it replaced. The three disjuncts here are exactly the
        three clauses of that ScanText selection, so no segment the scanner scans raw is left
        unscanned by this leg. That is what keeps both 'bash -c "rm -rf /tmp/x"' and
        'echo "rm -rf /tmp/x' denying. A segment matching none of the three is never scanned this
        way, so 'git commit -m "docs: explain why rm -rf is banned"' still allows.

        Leg 2 (structural): a relocating spelling such as 'git -C ../wt push --force origin
        HEAD' contains no literal as a token run, so it is classified structurally instead.

        Leg 1 is evaluated in full before any leg 2 evaluation, so 'git push origin --force'
        returns the literal 'git push origin --force' rather than the leg 2 value.

        Spec D11.3 rules that rule R2 governs the literal TEXT, not the comparison operator,
        so all six literals stay byte-unchanged; only the comparison primitive changed.
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $false)]
        [AllowEmptyString()]
        [AllowNull()]
        [string] $Command
    )

    if (-not $Command) {
        return $null
    }

    $segments = @(Read-CommandLineSegment -CommandText $Command)

    foreach ($pattern in (Get-BlockedBashPattern)) {
        $patternTokens = [string[]]@($pattern -split '\s+' | Where-Object { $_ })
        foreach ($segment in $segments) {
            if (Test-BlockedPatternTokenRun -Token @($segment.Tokens) -PatternToken $patternTokens) {
                return $pattern
            }
            if (($segment.IsWrapperLed -or $segment.HasLiveSubstitution -or $segment.Unbalanced) -and
                $segment.ScanText.IndexOf($pattern, [System.StringComparison]::Ordinal) -ge 0) {
                return $pattern
            }
        }
    }

    foreach ($segment in $segments) {
        $structural = Get-BlockedStructuralGitMatch -SegmentText $segment.RawText -Token @($segment.Tokens)
        if ($structural) {
            return $structural
        }
    }

    return $null
}

function Get-BashBlockReason {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $false)]
        [AllowEmptyString()]
        [AllowNull()]
        [string] $Command
    )

    $pattern = Get-BlockedPatternMatch -Command $Command
    if (-not $pattern) {
        return $null
    }

    return "Blocked dangerous command pattern detected: '$pattern'"
}

function Get-BashDenyDecision {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)]
        [string] $Reason
    )

    [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName            = 'PreToolUse'
            permissionDecision       = 'deny'
            permissionDecisionReason = $Reason
        }
    }
}

function Get-BashCommandToCheck {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $false)]
        [AllowEmptyString()]
        [AllowNull()]
        [string] $ToolInputRaw,

        [Parameter(Mandatory = $false)]
        [AllowEmptyString()]
        [AllowNull()]
        [string] $PositionalInput
    )

    if ($ToolInputRaw) {
        try {
            $parsed = $ToolInputRaw | ConvertFrom-Json -ErrorAction Stop
            if ($parsed.command) {
                return [string]$parsed.command
            }
        } catch {
            # If JSON parsing fails, treat the raw input as the command.
            return $ToolInputRaw
        }
    }

    if ($PositionalInput) {
        return $PositionalInput
    }

    return ''
}

function Invoke-ValidateBashDecision {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory = $false)]
        [AllowEmptyString()]
        [AllowNull()]
        [string] $ToolInputRaw,

        [Parameter(Mandatory = $false)]
        [AllowEmptyString()]
        [AllowNull()]
        [string] $PositionalInput
    )

    $commandToCheck = Get-BashCommandToCheck -ToolInputRaw $ToolInputRaw -PositionalInput $PositionalInput

    $reason = Get-BashBlockReason -Command $commandToCheck
    if ($reason) {
        return Get-BashDenyDecision -Reason $reason
    }

    return $null
}

function ConvertFrom-CodexBashHookPayload {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string] $PayloadRaw)

    if ([string]::IsNullOrWhiteSpace($PayloadRaw)) {
        throw 'validate-bash hook input is empty.'
    }
    try {
        $payload = $PayloadRaw | ConvertFrom-Json -ErrorAction Stop
    } catch {
        throw "validate-bash hook input is malformed JSON: $_"
    }
    if ($payload.PSObject.Properties.Name -notcontains 'tool_input' -or $null -eq $payload.tool_input) {
        throw 'validate-bash hook input is missing tool_input.'
    }
    if ([string]$payload.hook_event_name -ne 'PreToolUse' -or [string]$payload.tool_name -ne 'Bash') {
        throw 'validate-bash requires a PreToolUse Bash payload.'
    }
    return $payload
}

if ($MyInvocation.InvocationName -eq '.') {
    return
}

try {
    $payload = ConvertFrom-CodexBashHookPayload -PayloadRaw ([Console]::In.ReadToEnd())
    $toolInputRaw = $payload.tool_input | ConvertTo-Json -Compress -Depth 20
    $decision = Invoke-ValidateBashDecision -ToolInputRaw $toolInputRaw
    if ($null -ne $decision -and $decision.hookSpecificOutput.permissionDecision -eq 'deny') {
        $decision | ConvertTo-Json -Compress -Depth 5 | Write-Output
    }
    exit 0
} catch {
    [Console]::Error.WriteLine([string]$_)
    exit 2
}
