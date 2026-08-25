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

# Characters that make a command line statically unresolvable: shell interpolation and
# redirection (D4 row 12). Tested across the whole line rather than per operand, because a
# redirection anywhere in the line moves content the operand list cannot describe.
$script:UnresolvableCommandCharacters = [char[]]@('$', '`', '>', '<')

# Wildcards that make an operand a glob (D4 row 15). Only the literal prefix before the
# first of these is prefix-tested.
$script:PathspecWildcardCharacters = [char[]]@('*', '?', '[')

function Split-OrchestrationCommandLine {
    <#
    .SYNOPSIS
        Splits a command line into segments on chain operators outside quotes.
    .DESCRIPTION
        Realizes D4 row 13. Quote state is tracked while scanning so a chain operator
        inside a quoted span does not split. The returned `Balanced` flag reports whether
        the scan ended outside every quote; unbalanced text is not splittable and the
        caller denies (D4 rows 11 and 13). Empty and whitespace-only segments are dropped.
    .OUTPUTS
        System.Collections.Hashtable with keys `Balanced` (bool) and `Segments` (string[]).
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $CommandText)

    $segments = [System.Collections.Generic.List[string]]::new()
    $current = [System.Text.StringBuilder]::new()
    $openQuote = [char]0

    foreach ($character in $CommandText.ToCharArray()) {
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
        Realizes D4 rows 3, 9, 15, 16, 17, and 18. Backslashes normalize to forward slashes
        before the prefix test (row 18); pathspec magic, absolute spellings, parent-directory
        segments, and globs whose literal prefix escapes the exempt set all deny.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $Operand)

    if (-not $Operand) {
        return $false
    }

    # Row 3b and row 9: any leading colon is pathspec magic, which can escape or invert the
    # tree scope, so no colon-led operand is ever resolvable by a prefix test.
    if ($Operand.StartsWith(':')) {
        return $false
    }

    # Row 18: separator normalization precedes every prefix comparison below.
    $normalized = $Operand -replace '\\', '/'

    # Row 16: rooted, drive-lettered, and UNC spellings all deny; the exempt prefixes stay
    # repo-relative (this is the posture issue #516 later composes with).
    if ($normalized.StartsWith('/')) {
        return $false
    }
    if ($normalized -match '^[A-Za-z]:') {
        return $false
    }

    # Rows 15c and 17: any parent-directory component escapes the prefix.
    if (($normalized -split '/') -contains '..') {
        return $false
    }

    # Row 15: only the literal prefix before the first wildcard is prefix-tested, so a glob
    # whose wildcard occupies or truncates an ancestor segment cannot pass.
    $literalPrefix = $normalized
    $wildcardIndex = $normalized.IndexOfAny($script:PathspecWildcardCharacters)
    if ($wildcardIndex -ge 0) {
        $literalPrefix = $normalized.Substring(0, $wildcardIndex)
    }

    foreach ($tree in $script:OrchestrationBookkeepingTrees) {
        if ($literalPrefix.StartsWith($tree)) {
            return $true
        }
    }
    return $false
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
    param([Parameter(Mandatory)][AllowEmptyCollection()][string[]] $Token)

    if ($Token.Count -lt 2) {
        return $false
    }

    # Row 14: the command name leads and the subcommand follows immediately. Anything in
    # between - a relocating option or an env-style prefix - moves the pathspec base and is
    # rejected here rather than modelled.
    if ($Token[0] -cne 'git') {
        return $false
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
                # The message option is the only modelled option on either subcommand. Rows
                # 2, 5, 6, 8, and 10 all land here and deny, including a dash-leading
                # operand supplied without a preceding separator.
                if ($subcommand -cne 'commit') {
                    return $false
                }
                if ($candidate -ceq '-m' -or $candidate -ceq '--message') {
                    # The message value is the following token and is not a pathspec.
                    $index += 2
                    if ($index -gt $Token.Count) {
                        return $false
                    }
                    continue
                }
                if ($candidate.StartsWith('--message=') -or
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
        trees after balanced-quote stripping and separator normalization.

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

    # Row 12: interpolation and redirection are not statically resolvable, so the operand
    # list cannot be trusted to describe what the line actually touches.
    if ($CommandText.IndexOfAny($script:UnresolvableCommandCharacters) -ge 0) {
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

    foreach ($segment in $segments) {
        $tokens = @(ConvertTo-OrchestrationCommandToken -Segment $segment)
        if (-not (Test-ExemptOrchestrationSegmentToken -Token $tokens)) {
            return $false
        }
    }
    return $true
}
