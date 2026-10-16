<#
.SYNOPSIS
    Pathspec classifier for the orchestration-bookkeeping staging exemption (issue #539).
.DESCRIPTION
    Pure string logic only: no disk, process, network, or environment access. The single
    entry predicate `Test-ExemptOrchestrationStagingCommand` answers one question - does
    this command text parse, in its entirety, as recognized staging or integration
    invocations whose every pathspec operand resolves inside an orchestration-bookkeeping
    tree? Every parse ambiguity answers false, so the caller's pre-change deny is the
    fallback for every unmodeled form.

    Shell-agnostic parsing (issue #735): the shell that executes a hooked command is
    undetermined per command, so every shape whose meaning differs between a POSIX shell
    and PowerShell denies - a backslash anywhere, and { } , ( ) @ outside quotes. Evidence:
    research/research.2026-10-08T14-00.md (Q1) in the issue #732 feature folder.

    The normative contract is the D4 fail-closed rule table in
    docs/features/active/2026-08-24-preimplementation-gate-blocks-planner-integration-commits-539/spec.md.
    Rule-table row numbers are cited inline against the code that realizes them.

    This file is dot-sourced by the sibling gate hook, following the headroom-split
    precedent set by enforce-pr-author-skill.ps1.
#>

# The five exempt orchestration-bookkeeping trees (D2). Directory prefixes, repo-relative,
# forward-slash spelled. No glob, no absolute entry, no literal-file entry.
$script:OrchestrationBookkeepingTrees = @(
    'docs/features/epics/'
    'docs/features/parallel/'
    'docs/features/active/'
    'docs/features/potential/'
    'artifacts/orchestration/'
)

# Characters that make a command line statically unresolvable (D4 row 12, as narrowed by
# issues #663, #713, #732, and #735): interpolation characters outside single quotes;
# outside-quote characters (`<`, `>`, the `#` comment introducer, and the shell-divergent
# `{ } , ( ) @`) outside any quote; and typographic quotes (U+2018 to U+201E) anywhere.
$script:InterpolationCommandCharacters = [char[]]@('$', '`')
$script:OutsideQuoteCommandCharacters = [char[]]@('>', '<', '#', '{', '}', ',', '(', ')', '@')
$script:TypographicQuoteCharacters = [char[]]@(0x2018, 0x2019, 0x201A, 0x201B, 0x201C, 0x201D, 0x201E)

# Wildcards a selector value may not carry (LACS L6). An operand carrying one fails the
# operand allowlist (issue #732).
$script:PathspecWildcardCharacters = [char[]]@('*', '?', '[')

# The single repository selector modelled between the command name and the subcommand: D4
# row 14 as narrowed by issue #671, the Lexical Absolute-Canonical Selector (LACS) rule in
# docs/features/active/2026-09-13-preimplementation-gate-worktree-selector-671/spec.md.
# Compared case-sensitively; no other relocating spelling is modelled.
$script:OrchestrationSelectorOptionName = '-C'

# Accepted widening (issue #671): a selector naming a nested subdirectory of a worktree
# passes L1 through L8 lexically yet relocates what a relative operand denotes, so
# `docs/features/active/X` under `-C <root>/tests/fixtures/resolve_execute_plan_prompt`
# stages `tests/fixtures/resolve_execute_plan_prompt/docs/features/active/X`. Measured
# exposure in this repository is seven Markdown test fixtures under the
# resolve_execute_plan_prompt fixture tree, all of which the gate's file_path leg already
# classifies as non-implementation. This module stays pure string logic; the epic's F1
# resolution module composes upstream to close the escape later without a schema change,
# the same posture D4 row 16 takes toward issue #516.

function Split-OrchestrationCommandLine {
    <#
    .SYNOPSIS
        Splits a command line into segments on chain operators outside quotes.
    .DESCRIPTION
        Realizes D4 row 13. Quote state and POSIX backslash escapes (issue #710) are tracked, so a quoted or
        escaped chain operator does not split. `Balanced` reports whether the scan ended outside every quote;
        the caller denies unbalanced text (D4 rows 11 and 13). Empty and whitespace-only segments are dropped.
    .OUTPUTS
        System.Collections.Hashtable with keys `Balanced` (bool) and `Segments` (string[]).
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $CommandText)

    $segments = [System.Collections.Generic.List[string]]::new()
    $current = [System.Text.StringBuilder]::new()
    $openQuote = [char]0
    $escaped = $false
    foreach ($character in $CommandText.ToCharArray()) {
        if ($escaped -or ($character -eq '\' -and $openQuote -ne "'")) { $escaped = -not $escaped; [void]$current.Append($character); continue }
        if ($openQuote -ne [char]0) {
            if ($character -eq $openQuote) {
                $openQuote = [char]0
            }
            [void]$current.Append($character)
            continue
        }

        if ($character -eq '"' -or $character -eq "'") {
            $openQuote = $character
            [void]$current.Append($character)
            continue
        }

        if ($character -eq ';' -or $character -eq '&' -or $character -eq '|' -or
            $character -eq "`n" -or $character -eq "`r") {
            $segments.Add($current.ToString())
            [void]$current.Clear()
            continue
        }

        [void]$current.Append($character)
    }
    $segments.Add($current.ToString())

    return @{
        Balanced = ($openQuote -eq [char]0)
        Segments = @($segments | Where-Object { $_.Trim() })
    }
}

function Test-OrchestrationCommandTextUnresolvable {
    <#
    .SYNOPSIS
        Reports whether a command line carries a statically unresolvable character.
    .DESCRIPTION
        Realizes D4 row 12 as narrowed by issues #663, #713, #732, and #735, with quote state
        tracked as in Split-OrchestrationCommandLine. `$` or backtick answers true outside
        quotes or inside double quotes (single quotes keep it literal); `<`, `>`, `#`, and
        `{ } , ( ) @` answer true only outside a quoted span. A typographic quote (U+2018 to
        U+201E) or a backslash answers true anywhere, because POSIX shells and PowerShell read
        them differently and the executing shell is undetermined (fail closed).
    .PARAMETER CommandText
        The full command line as the shell would receive it.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $CommandText)

    # A backslash or typographic quote is read differently by POSIX shells and PowerShell (#735).
    if ($CommandText.Contains('\') -or $CommandText.IndexOfAny($script:TypographicQuoteCharacters) -ge 0) {
        return $true
    }

    $openQuote = [char]0

    # Scan every character once, tracking whether it sits inside a quoted span so that
    # outside-quote characters are judged only where the shell would honour them.
    foreach ($character in $CommandText.ToCharArray()) {
        if ($openQuote -ne "'" -and $script:InterpolationCommandCharacters -contains $character) {
            return $true
        }

        # Decide by quote state: inside a span only the closing quote matters; outside a
        # span a quote opens one and an outside-quote character is unresolvable.
        if ($openQuote -ne [char]0) {
            if ($character -eq $openQuote) {
                $openQuote = [char]0
            }
        } elseif ($character -eq '"' -or $character -eq "'") {
            $openQuote = $character
        } elseif ($script:OutsideQuoteCommandCharacters -contains $character) {
            return $true
        }
    }
    return $false
}

function ConvertTo-OrchestrationCommandToken {
    <#
    .SYNOPSIS
        Splits one segment into whitespace-delimited tokens with balanced quotes stripped.
    .DESCRIPTION
        Realizes the quote handling of D4 row 11. A quoted span contributes to the token it
        sits in, so `-m "epic scaffold"` yields the two tokens `-m` and `epic scaffold`, and
        a quoted operand arrives at the prefix test unquoted. The caller has already
        rejected unbalanced text, so an unterminated quote simply ends the final token.
    .OUTPUTS
        System.String[]
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $Segment)

    $tokens = [System.Collections.Generic.List[string]]::new()
    $current = [System.Text.StringBuilder]::new()
    $hasToken = $false
    $openQuote = [char]0

    foreach ($character in $Segment.ToCharArray()) {
        if ($openQuote -ne [char]0) {
            if ($character -eq $openQuote) {
                $openQuote = [char]0
            } else {
                [void]$current.Append($character)
            }
            continue
        }

        if ($character -eq '"' -or $character -eq "'") {
            $openQuote = $character
            $hasToken = $true
            continue
        }

        if ([char]::IsWhiteSpace($character)) {
            if ($hasToken) {
                $tokens.Add($current.ToString())
                [void]$current.Clear()
                $hasToken = $false
            }
            continue
        }

        $hasToken = $true
        [void]$current.Append($character)
    }

    if ($hasToken) {
        $tokens.Add($current.ToString())
    }

    return $tokens.ToArray()
}

function Test-ExemptOrchestrationOperand {
    <#
    .SYNOPSIS
        Tests one pathspec operand against the five exempt orchestration trees.
    .DESCRIPTION
        Realizes D4 rows 3, 9, 15, 16, and 17 as narrowed by issues #732 and #735. An operand
        is exempt only when it matches the plain ASCII allowlist A-Z a-z 0-9 . _ / - (which
        excludes pathspec magic, drive letters, backslashes, globs, braces, and non-ASCII
        look-alikes), is not rooted, carries no '..' segment, and starts with an exempt tree.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $Operand)

    if ($Operand -cnotmatch '^[A-Za-z0-9._/-]+$' -or $Operand.StartsWith('/')) {
        return $false
    }
    if (($Operand -split '/') -contains '..') {
        return $false
    }
    foreach ($tree in $script:OrchestrationBookkeepingTrees) {
        if ($Operand.StartsWith($tree)) {
            return $true
        }
    }
    return $false
}

function Test-ExemptOrchestrationSelector {
    <#
    .SYNOPSIS
        Tests the repository selector of one tokenized segment against LACS L1 through L8.
    .DESCRIPTION
        Realizes D4 row 14 as narrowed by issue #671 (the Lexical Absolute-Canonical Selector
        rule in docs/features/active/2026-09-13-preimplementation-gate-worktree-selector-671/spec.md).
        Returns true only when the segment reads `git -C <value> <subcommand> ...` and the
        value is decidable from the command text alone: exactly one case-sensitive `-C`, at
        index 1 (L1, L2); a value at index 2 and the subcommand at index 3 (L3); a
        drive-lettered or rooted, non-UNC value after separator normalization (L4); no `.` or
        `..` segment (L5); no wildcard (L6); no colon other than the drive colon (L7); and a
        non-empty value (L8). Each rejection writes a diagnostic token through Write-Debug.
        The tokens are not contractual and change no decision.
        L3 checks only that a non-option token follows the value; the caller
        Test-ExemptOrchestrationSegmentToken rejects any subcommand other than add or commit.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][AllowEmptyCollection()][AllowEmptyString()][string[]] $Token)

    $selector = $script:OrchestrationSelectorOptionName
    if ($Token.Count -lt 2 -or $Token[1] -cne $selector) {
        Write-Debug 'PREIMPL_SELECTOR_UNMODELLED_OPTION: the token after the command name is not the -C selector.'
        return $false
    }
    if (@($Token | Where-Object { $_ -ceq $selector }).Count -gt 1) {
        Write-Debug 'PREIMPL_SELECTOR_REPEATED: the segment carries more than one -C selector.'
        return $false
    }
    if ($Token.Count -lt 4 -or $Token[3].StartsWith('-')) {
        Write-Debug 'PREIMPL_SELECTOR_MALFORMED: no subcommand immediately follows the selector value.'
        return $false
    }
    $value = $Token[2]
    if (-not $value) {
        Write-Debug 'PREIMPL_SELECTOR_MALFORMED: the selector value is empty.'
        return $false
    }

    $normalized = $value -replace '\\', '/'
    $isDriveRooted = $normalized -match '^[A-Za-z]:/'
    if ($normalized.StartsWith('//') -or -not ($isDriveRooted -or $normalized.StartsWith('/'))) {
        Write-Debug 'PREIMPL_SELECTOR_NOT_ROOTED: the selector value is relative or UNC.'
        return $false
    }
    $segments = $normalized -split '/'
    if ($segments -contains '..' -or $segments -contains '.') {
        Write-Debug 'PREIMPL_SELECTOR_TRAVERSAL: the selector value carries a dot segment.'
        return $false
    }
    $afterDrive = if ($isDriveRooted) { $normalized.Substring(2) } else { $normalized }
    if ($normalized.IndexOfAny($script:PathspecWildcardCharacters) -ge 0 -or $afterDrive.Contains(':')) {
        Write-Debug 'PREIMPL_SELECTOR_NOT_LITERAL: the selector value carries a wildcard or a stray colon.'
        return $false
    }
    return $true
}

function Test-ExemptOrchestrationSegmentToken {
    <#
    .SYNOPSIS
        Tests one already-tokenized segment as a recognized all-exempt invocation.
    .DESCRIPTION
        Realizes D4 rows 1, 2, 4, 5, 6, 7, 8, 10, 14, and 19. The command name must lead the
        segment and the subcommand must follow it immediately (row 14), the option table is
        modelled positively so any unmodelled dash-leading token denies (rows 2, 5, 6, 8, 10),
        tokens after the double-dash separator are pathspecs (row 7), at least one operand is
        required (rows 1, 4, 7), and every operand must pass (row 19).
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][AllowEmptyCollection()][AllowEmptyString()][string[]] $Token)

    if ($Token.Count -lt 2) {
        return $false
    }

    # Row 14, narrowed by issue #671: the command name leads and the subcommand follows it,
    # either immediately or after one lexically absolute `-C <value>` selector (LACS L1-L8).
    # Anything else in between - an unmodelled or unresolvable relocating option, or an
    # env-style prefix - moves the pathspec base undecidably and is rejected here.
    if ($Token[0] -cne 'git') {
        return $false
    }
    if ($Token[1] -cne 'add' -and $Token[1] -cne 'commit') {
        if (-not (Test-ExemptOrchestrationSelector -Token $Token)) {
            return $false
        }
        # Drop the accepted selector and its value so the subcommand is again at index 1
        # and operand collection below starts at index 2 exactly as before.
        $Token = @($Token[0]) + @($Token[3..($Token.Count - 1)])
    }
    $subcommand = $Token[1]
    if ($subcommand -cne 'add' -and $subcommand -cne 'commit') {
        return $false
    }

    $operands = [System.Collections.Generic.List[string]]::new()
    $afterSeparator = $false
    $index = 2

    while ($index -lt $Token.Count) {
        $candidate = $Token[$index]

        if (-not $afterSeparator) {
            if ($candidate -ceq '--') {
                # Row 7: every remaining token is a pathspec, dash-leading or not.
                $afterSeparator = $true
                $index++
                continue
            }

            if ($candidate.StartsWith('-')) {
                # Only the message and trailer options of commit are modelled (issue #713). Rows
                # 2, 5, 6, 8, and 10 all land here and deny, including a dash-leading
                # operand supplied without a preceding separator.
                if ($subcommand -cne 'commit') {
                    return $false
                }
                if ($candidate -ceq '-m' -or $candidate -ceq '--message' -or $candidate -ceq '--trailer') {
                    # The message or trailer value is the following token and is not a pathspec.
                    $index += 2
                    if ($index -gt $Token.Count) {
                        return $false
                    }
                    continue
                }
                if ($candidate.StartsWith('--message=') -or $candidate.StartsWith('--trailer=') -or
                    ($candidate.Length -gt 2 -and $candidate.StartsWith('-m'))) {
                    $index++
                    continue
                }
                return $false
            }
        }

        $operands.Add($candidate)
        $index++
    }

    # Rows 1, 4, and 7: an invocation with no pathspec operand claims no path, so there is
    # nothing for the prefix test to scope and the exemption is not available.
    if ($operands.Count -eq 0) {
        return $false
    }

    # Row 19: all-operands-exempt is the invariant; one non-exempt operand denies the set.
    foreach ($operand in $operands) {
        if (-not (Test-ExemptOrchestrationOperand -Operand $operand)) {
            return $false
        }
    }
    return $true
}

function Test-ExemptOrchestrationStagingCommand {
    <#
    .SYNOPSIS
        Reports whether a command line is an orchestration-bookkeeping staging invocation.
    .DESCRIPTION
        The entry predicate of the issue #539 exemption. Returns true only when the whole
        command line splits cleanly into segments and EVERY segment parses as a complete,
        recognized staging or integration invocation carrying at least one pathspec operand,
        with every operand resolving inside one of the five exempt orchestration-bookkeeping
        trees after balanced-quote stripping.

        The all-segments reading is deliberate and fail-closed: a chained line denies unless
        each of its segments is independently a recognized all-exempt invocation, so a
        relocating spelling or a prose fragment anywhere in the line withholds the exemption
        even when another segment on the same line would have qualified on its own.

        Consumed allow-side only. A false result restores the caller's unchanged
        classification; it never suppresses the trigger.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $CommandText)

    if (-not $CommandText) {
        return $false
    }

    # Row 12: `$` or backtick outside single quotes, `< > # { } , ( ) @` outside quotes, and
    # any typographic quote or backslash make the operand list untrustworthy (#732, #735).
    if (Test-OrchestrationCommandTextUnresolvable -CommandText $CommandText) {
        return $false
    }

    $split = Split-OrchestrationCommandLine -CommandText $CommandText
    if (-not $split.Balanced) {
        # Rows 11 and 13: unbalanced quoting makes the segment boundaries ambiguous.
        return $false
    }

    $segments = @($split.Segments)
    if ($segments.Count -eq 0) {
        return $false
    }

    # Fail closed (issue #671): an error raised while classifying any segment is a parse
    # ambiguity, so it answers false instead of letting the caller continue past it.
    try {
        foreach ($segment in $segments) {
            $tokens = @(ConvertTo-OrchestrationCommandToken -Segment $segment)
            if (-not (Test-ExemptOrchestrationSegmentToken -Token $tokens)) {
                return $false
            }
        }
    } catch {
        return $false
    }
    return $true
}
