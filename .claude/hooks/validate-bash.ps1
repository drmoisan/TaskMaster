<#
.SYNOPSIS
    Pre-tool-use hook for Claude Code that blocks dangerous Bash commands.

.DESCRIPTION
    This script is invoked by the Claude Code PreToolUse hook before any Bash
    command is executed. It acquires the hook payload through the shared reader
    (.claude/lib/hook-payload/HookPayload.psm1: stdin first, then the two
    environment-variable fallbacks) and reads the proposed command string from the
    envelope's nested tool_input.command, or falls back to the first positional
    argument. If the command matches any blocked pattern (destructive operations
    such as forced deletions, forced pushes, or hard resets), the script writes a
    PreToolUse deny decision to stdout and exits with code 0. The deny decision
    uses the Claude Code PreToolUse schema:

        {"hookSpecificOutput":{"hookEventName":"PreToolUse",
         "permissionDecision":"deny","permissionDecisionReason":"<reason>"}}

    Safe commands emit no decision and exit with code 0 (an absent decision is a
    valid allow at PreToolUse). The legacy top-level decision/block form and the
    deny-path 'exit 1' are intentionally NOT used: PreToolUse fail-opens on both,
    so they would silently fail to block.

    Deliberate exception to the fail-closed envelope policy (issue #501, AC-5): this
    hook is a dangerous-command denylist, not a receipt gate, so an empty payload
    remains an allow and unparseable raw text is still treated as the command text
    for denylist matching. Both behaviours preserve the documented manual/CLI usage
    'pwsh -NoProfile -File validate-bash.ps1 "<command>"'. Every other PreToolUse
    hook denies on those two conditions.

.NOTES
    Compatible with PowerShell 7+.
    This script must not modify any state; it is a read-only validation gate.
#>
[CmdletBinding()]
param(
    [Parameter(Position = 0, Mandatory = $false)]
    [string]$CommandInput
)

Import-Module (Join-Path $PSScriptRoot '../lib/hook-payload/HookPayload.psm1') -Force
# Shared command-line parser (issue #545). Both detectors below run against the segment
# list rather than against the unsegmented command string, so a dangerous phrase quoted
# inside a message body is no longer a match and a relocating spelling no longer escapes.
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

        When a segment carries both '--force' and '-f' the '--force' leg is evaluated first,
        so the returned value is 'git push --force'.
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
        Two legs, in this order, over the segment list produced by Read-CommandLineSegment.

        Leg 1 (literal): each of the six byte-unchanged literals from Get-BlockedBashPattern
        is split into its own whitespace-delimited token sequence and reported as a match
        when that sequence occurs as a CONTIGUOUS RUN inside a segment's Tokens, every
        element compared by whole-token equality. The value returned is the literal string
        itself, and the literals are tested in their existing declaration order. Because a
        quoted span is one token, 'git commit -m "why rm -rf is banned"' no longer matches.

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

        Leg 1 is evaluated IN FULL, over every segment and every literal, before any leg 2
        evaluation. That ordering is what keeps 'git push origin --force' returning the
        literal 'git push origin --force' rather than the leg 2 value 'git push --force',
        which is what the existing case at tests/scripts/claude-hooks/validate-bash.Tests.ps1
        line 29 asserts.

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

# File-reading commands Claude Code's Bash permission engine resolves against
# Read() rules rather than Bash() prefix rules. Measured empirically (2026-09):
# once one of these co-occurs with a preceding `cd` in the same command line
# (chained via `&&` or `;`), the engine always requires manual approval -
# regardless of any Read() or Bash() allow rule present, and regardless of
# whether the read command's own path argument is relative or absolute. No
# settings.json configuration can suppress that prompt; the only fix is to
# never chain `cd` with one of these in the same command.
$script:CdChainedReadCommandPattern = 'cd\s+\S.*?(&&|;)\s*(grep|cat|head|tail|less|more|awk|sed\s+-n)\b'

# The read-command family the pattern above enumerates, as command words. Retained
# alongside the pattern so the rule R2 literal-text obligation stays visibly discharged:
# the pattern string is byte-unchanged and is kept as a declared constant, while the
# evaluation moves to the segment list.
$script:CdChainedReadCommandWords = @('grep', 'cat', 'head', 'tail', 'less', 'more', 'awk', 'sed')

function Get-CdChainedReadCommandMatch {
    <#
    .SYNOPSIS
        Return the read command chained after a cd on the same command line, or $null.
    .DESCRIPTION
        Walks the segment list in order and reports a match when a segment whose CommandWord
        is 'cd' and whose Tokens count is at least two is followed, at ANY later position in
        the segment list, by a segment whose CommandWord is one of the read-command family.
        The returned value is that later segment's CommandWord, except that a 'sed' segment
        matches only when its second token is '-n' and then returns 'sed -n'. When more than
        one later segment qualifies, the earliest supplies the return value.

        The regex could not simply be evaluated per segment: '&&' and ';' are segment
        delimiters under spec D2 Piece 1, so no single segment ever contains both sides of
        the chain.

        A second leg therefore runs first, before the walk: the retained regex constant is
        evaluated against the ScanText of each segment the scanner already reads raw, which
        is where a wrapper's quoted argument collapses a whole nested chain into one segment
        the walk cannot decompose. The two legs are complementary and neither replaces the
        other, so the ordering between them changes only which leg reports a chain that both
        could see, never whether a chain is reported at all.

        Adjacency is deliberately NOT required. The retained pattern places a lazy '.*?'
        between the cd argument and the delimiter, so 'cd /x && npm test && grep foo bar.txt'
        is denied today; requiring the read segment to follow the cd segment immediately
        would turn that existing denial into an allow, which acceptance criterion 9 forbids.
        This formulation also widens the delimiter set from '&&' and ';' to every segment
        delimiter, so every command line in which a real cd segment precedes a real read
        segment and is denied today is still denied.

        One narrowing is intended and is stated rather than left implicit: a cd-then-read
        phrase occurring only inside a quoted span or a heredoc body is denied today and
        allows after this change, because the scanner masks that span before the segment list
        is built. That narrowing is the over-match fix this issue exists to deliver.
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

    # D12 call-site row for line 105: the retained pattern, evaluated per segment against
    # ScanText. Complementary to the CommandWord walk below rather than a replacement for it.
    # '&&' and ';' delimit segments, so an unquoted chain never puts both halves in one
    # segment and only the walk can see it; a wrapper-led chain sits entirely inside one
    # segment whose command word is the wrapper, and only this leg can see that. Restricting
    # the leg to segments the scanner already scans raw preserves the intended narrowing:
    # echo "cd /x && head f" is masked and still allows.
    foreach ($segment in $segments) {
        if ((Test-CommandLineSegmentRawScan -Segment $segment) -and
            $segment.ScanText -match $script:CdChainedReadCommandPattern) {
            return ($Matches[2] -replace '\s+', ' ')
        }
    }

    $seenCd = $false

    foreach ($segment in $segments) {
        $tokens = @($segment.Tokens)
        $word = [string]$segment.CommandWord

        if (-not $seenCd) {
            if ($word -eq 'cd' -and $tokens.Count -ge 2) {
                $seenCd = $true
            }
            continue
        }

        if ($script:CdChainedReadCommandWords -notcontains $word) {
            continue
        }
        if ($word -eq 'sed') {
            if ($tokens.Count -ge 2 -and $tokens[1] -eq '-n') {
                return 'sed -n'
            }
            continue
        }
        return $word
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
    if ($pattern) {
        return "Blocked dangerous command pattern detected: '$pattern'"
    }

    $readOp = Get-CdChainedReadCommandMatch -Command $Command
    if ($readOp) {
        return "Forbidden Bash pattern: 'cd ... && $readOp' (or ';'-chained). Claude Code's Bash permission engine cannot resolve a file-reading command ($readOp) against Read() rules once a preceding 'cd' has changed the working directory in the same command line - it always requires manual approval, regardless of any Read() or Bash() allow rule, and regardless of whether the path argument is relative or absolute. Rewrite as a single command using an absolute path instead of 'cd'-ing first, e.g. run $readOp directly against the absolute file path, with no leading 'cd'."
    }

    return $null
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
        $parsed = ConvertFrom-ClaudeHookEnvelope -Raw $ToolInputRaw
        if (-not $parsed.IsValid) {
            # AC-5 exception: unparseable raw text is still treated as the command
            # text, which is what the documented manual/CLI usage supplies.
            if ($parsed.Anomaly -eq 'UnparseableJson') {
                return [string]$ToolInputRaw
            }
        } else {
            $extracted = Get-ClaudeHookToolInput -Envelope $parsed.Value
            if ($extracted.IsValid) {
                $command = Get-ClaudeHookToolInputString -ToolInput $extracted.Value -Name 'command'
                if ($command) {
                    return $command
                }
            }
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

if ($MyInvocation.InvocationName -eq '.') {
    return
}

$toolInputRaw = Read-ClaudeHookRawPayload

$decision = Invoke-ValidateBashDecision -ToolInputRaw $toolInputRaw -PositionalInput $CommandInput
if ($null -ne $decision -and $decision.hookSpecificOutput.permissionDecision -eq 'deny') {
    $decision | ConvertTo-Json -Compress -Depth 5 | Write-Output
}

exit 0
