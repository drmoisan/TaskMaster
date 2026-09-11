<#
.SYNOPSIS
    Structural command-invocation matcher shared by the Claude and Codex enforcement hooks.
.DESCRIPTION
    Realizes Piece 3 of the D2 behavior contract and the retrieval surface of D12 in
    docs/features/active/2026-08-25-enforcement-hook-trigger-matches-whole-command-text-545/spec.md.

    Answers, per segment produced by hook-command-scanner.ps1, whether a command line
    INVOKES a named command word with a named subcommand path, rather than merely
    containing those words adjacently. It also retrieves the operands and flag values of
    the matched invocation, taken from the matched segment only.

    Pure string logic only: no disk, process, network, clock, or environment access. It is
    dot-sourced as: . (Join-Path $PSScriptRoot 'hook-command-invocation.ps1')
#>

. (Join-Path $PSScriptRoot 'hook-command-scanner.ps1')

# The transparent-wrapper set of D2 Piece 3 step 2. These prefix a command without changing
# which command runs, so the structural matcher skips them. All five are also members of the
# scanner's wrapper carve-out set, deliberately, so 'env git worktree remove x' classifies by
# both mechanisms. Pinned by test through Get-CommandLineTransparentWrapperName (rule R6).
$script:CommandLineTransparentWrapperNames = @('command', 'env', 'nohup', 'time', 'timeout')

# The modeled global-option tables of D2 Piece 3 steps 3 and the D6/D10 gh surface. An option
# in WithArgument consumes the following token as its value, or carries the value inline in
# the '--name=value' form; an option in Standalone consumes only itself. A dash-leading token
# in neither list is UNMODELED, and an unmodeled token between the command word and the
# subcommand classifies as a match: over-classification only forces a checkpoint check,
# whereas under-classification is a bypass. Pinned by test through
# Get-CommandLineGlobalOption (rule R6).
$script:CommandLineGlobalOptions = @{
    git = [pscustomobject]@{
        WithArgument = @('-C', '-c', '--git-dir', '--work-tree', '--namespace', '--exec-path')
        Standalone   = @('-p', '--paginate', '--no-pager', '--literal-pathspecs', '--no-optional-locks', '--bare')
    }
    gh  = [pscustomobject]@{
        WithArgument = @('-R', '--repo')
        Standalone   = @()
    }
    npx = [pscustomobject]@{
        WithArgument = @('-p', '--package', '-c', '--call', '--node-arg', '--userconfig')
        Standalone   = @('-y', '--yes', '--no-install', '--ignore-existing', '-q', '--quiet')
    }
}

# Zero-argument flags whose arity is known at operand-collection time regardless of command
# word. Without '--force' here, 'git worktree remove --force <path>' would terminate operand
# collection at the flag and return no path, which is exactly the acceptance case AT-7
# requires to resolve. Every member takes no value, so consuming one token is always correct.
$script:CommandLineStandaloneFlagNames = @('--force', '-f', '--dry-run', '-n', '--quiet', '-q', '--verbose', '-v')

function Get-CommandLineTransparentWrapperName {
    <#
    .SYNOPSIS
        Return the transparent-wrapper set of D2 Piece 3 step 2.
    .OUTPUTS
        System.String[]
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param()

    return [string[]]$script:CommandLineTransparentWrapperNames
}

function Get-CommandLineGlobalOption {
    <#
    .SYNOPSIS
        Return the modeled global-option table for one command word.
    .DESCRIPTION
        Returns a record carrying two string arrays: WithArgument, whose members consume a
        following token or an inline '=value', and Standalone, whose members consume only
        themselves. An unrecognized command word yields a record with two empty arrays, so
        every dash-leading token for that command word is treated as unmodeled and therefore
        classifies - the fail-closed direction.
    .OUTPUTS
        System.Management.Automation.PSCustomObject
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param([Parameter(Mandatory)][ValidateNotNullOrEmpty()][string] $CommandWord)

    $key = $CommandWord.ToLowerInvariant()
    if ($script:CommandLineGlobalOptions.ContainsKey($key)) {
        return $script:CommandLineGlobalOptions[$key]
    }

    return [pscustomobject]@{ WithArgument = @(); Standalone = @() }
}

function Test-CommandLineRawContainment {
    <#
    .SYNOPSIS
        Report whether a raw text contains the command word and every subcommand element.
    .DESCRIPTION
        The "in any arrangement" test D12 specifies for a wrapper-led or live-substitution
        segment. Comparison is ordinal case-insensitive containment, which is deliberately
        loose: a false positive only forces a checkpoint check, a false negative is a bypass.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)][AllowEmptyString()][string] $RawText,
        [Parameter(Mandatory)][string] $CommandWord,
        [Parameter(Mandatory)][ValidateNotNullOrEmpty()][string[]] $SubcommandPath
    )

    $comparison = [System.StringComparison]::OrdinalIgnoreCase
    if ($RawText.IndexOf($CommandWord, $comparison) -lt 0) { return $false }
    foreach ($element in $SubcommandPath) {
        if ($RawText.IndexOf($element, $comparison) -lt 0) { return $false }
    }
    return $true
}

function Skip-CommandLineOption {
    <#
    .SYNOPSIS
        Absorb modeled options from a token list and report where they end.
    .DESCRIPTION
        Returns Index, the position of the first token that is not a modeled option, and
        Unmodeled, set when a dash-leading token appears in neither list of the supplied
        table. A bare '-' and a bare '--' are not options and stop the absorption.
    .OUTPUTS
        System.Management.Automation.PSCustomObject
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][AllowEmptyString()][string[]] $Token,
        [Parameter(Mandatory)][int] $StartIndex,
        [Parameter(Mandatory)][pscustomobject] $Option
    )

    $index = $StartIndex
    while ($index -lt $Token.Count) {
        $current = $Token[$index]
        if (-not $current.StartsWith('-') -or $current -eq '-' -or $current -eq '--') { break }

        $name = $current
        $hasInlineValue = $false
        $split = $current.IndexOf('=')
        if ($split -gt 0) {
            $name = $current.Substring(0, $split)
            $hasInlineValue = $true
        }

        if ($Option.WithArgument -contains $name) {
            $index += if ($hasInlineValue) { 1 } else { 2 }
            continue
        }
        if ($Option.Standalone -contains $name) {
            $index++
            continue
        }

        return [pscustomobject]@{ Index = $index; Unmodeled = $true }
    }

    return [pscustomobject]@{ Index = $index; Unmodeled = $false }
}

function Resolve-CommandLineInvocation {
    <#
    .SYNOPSIS
        Locate the first segment that invokes a command word with a subcommand path.
    .DESCRIPTION
        Implements the four mandatory fail-closed rules of D12 in order, per segment:
        an Unbalanced segment classifies because its structure could not be resolved; a
        wrapper-led or live-substitution segment classifies when its RawText contains the
        command word and every subcommand element in any arrangement; an unmodeled
        dash-leading token between the command word and the subcommand classifies; and a
        non-dash token that is not the next expected subcommand element terminates that
        segment's scan without a match, so 'git log --grep add' does not classify.

        Returns $null when no segment matched. Otherwise returns Segment, the matched record,
        and OperandIndex, the token position just past the subcommand path - or -1 when the
        match was reached by a non-structural rule, in which case there is no operand
        position to report.
    .OUTPUTS
        System.Management.Automation.PSCustomObject
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory)][AllowEmptyString()][AllowNull()][string] $CommandText,
        [Parameter(Mandatory)][string] $CommandWord,
        [Parameter(Mandatory)][ValidateNotNullOrEmpty()][string[]] $SubcommandPath
    )

    $option = Get-CommandLineGlobalOption -CommandWord $CommandWord
    $transparent = $script:CommandLineTransparentWrapperNames

    foreach ($segment in @(Read-CommandLineSegment -CommandText $CommandText)) {
        if ($segment.Unbalanced) {
            return [pscustomobject]@{ Segment = $segment; OperandIndex = -1 }
        }

        if (($segment.IsWrapperLed -or $segment.HasLiveSubstitution) -and
            (Test-CommandLineRawContainment -RawText $segment.RawText -CommandWord $CommandWord -SubcommandPath $SubcommandPath)) {
            return [pscustomobject]@{ Segment = $segment; OperandIndex = -1 }
        }

        $tokens = @($segment.Tokens)
        $index = 0
        while ($index -lt $tokens.Count -and (
                $tokens[$index] -match '^[A-Za-z_][A-Za-z0-9_]*=' -or
                ($transparent -contains $tokens[$index] -and $tokens[$index] -ne $CommandWord))) {
            $index++
        }

        if ($index -ge $tokens.Count -or $tokens[$index] -ne $CommandWord) { continue }
        $index++

        $matched = $true
        foreach ($element in $SubcommandPath) {
            $skip = Skip-CommandLineOption -Token $tokens -StartIndex $index -Option $option
            if ($skip.Unmodeled) {
                return [pscustomobject]@{ Segment = $segment; OperandIndex = -1 }
            }
            $index = $skip.Index
            if ($index -ge $tokens.Count -or $tokens[$index] -ne $element) {
                $matched = $false
                break
            }
            $index++
        }

        if ($matched) {
            return [pscustomobject]@{ Segment = $segment; OperandIndex = $index }
        }
    }

    return $null
}

function Test-CommandLineInvocation {
    <#
    .SYNOPSIS
        Report whether a command line invokes a named command word with a named
        subcommand path, structurally rather than by raw-text adjacency.
    .DESCRIPTION
        Scans every segment produced by Read-CommandLineSegment and applies the four
        mandatory fail-closed rules of D12, which are stated in full on
        Resolve-CommandLineInvocation. This predicate is the replacement for every
        raw-text trigger regex that asked whether a governed command runs.
    .PARAMETER CommandText
        The raw Bash command text.
    .PARAMETER CommandWord
        The command name, e.g. 'git' or 'gh'. Compared case-insensitively.
    .PARAMETER SubcommandPath
        One or more ordered subcommand tokens, e.g. @('worktree','remove'), @('pr','merge'),
        @('issue','create'), @('add'). Compared case-insensitively.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)][AllowEmptyString()][AllowNull()][string] $CommandText,
        [Parameter(Mandatory)][string] $CommandWord,
        [Parameter(Mandatory)][ValidateNotNullOrEmpty()][string[]] $SubcommandPath
    )

    return $null -ne (Resolve-CommandLineInvocation -CommandText $CommandText -CommandWord $CommandWord -SubcommandPath $SubcommandPath)
}

function Get-CommandLineOperand {
    <#
    .SYNOPSIS
        Return the positional operands of a matched invocation, in source order.
    .DESCRIPTION
        Locates the first segment in which CommandWord + SubcommandPath match STRUCTURALLY,
        then returns the non-option tokens that follow the subcommand path in that segment
        only. Modeled option-with-argument pairs are consumed, so '-C <arg>' consumes both
        tokens; known zero-argument flags such as '--force' contribute nothing; a token after
        a bare '--' separator is always an operand; and a dash-leading token that is not
        modeled terminates collection, because its arity is unknown and a wrong guess would
        silently return a flag as a path.

        The operand list comes from the MATCHED segment only, so a 'cd <path>' segment chained
        before the invocation contributes nothing. That is the direct fix for issue #591. A
        match reached by a non-structural fail-closed rule reports no operand position, and
        yields no operands.

        Callers receive the result through the @(...) array form used at every call site, so
        the no-operand case arrives as an empty array rather than as $null.
    .PARAMETER CommandText
        The raw Bash command text.
    .PARAMETER CommandWord
        The command name.
    .PARAMETER SubcommandPath
        The ordered subcommand tokens.
    .OUTPUTS
        System.String[] - operands with balanced quotes already stripped, in source order.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory)][AllowEmptyString()][AllowNull()][string] $CommandText,
        [Parameter(Mandatory)][string] $CommandWord,
        [Parameter(Mandatory)][ValidateNotNullOrEmpty()][string[]] $SubcommandPath
    )

    $operands = [System.Collections.Generic.List[string]]::new()
    $resolved = Resolve-CommandLineInvocation -CommandText $CommandText -CommandWord $CommandWord -SubcommandPath $SubcommandPath
    if ($null -eq $resolved -or $resolved.OperandIndex -lt 0) {
        return [string[]]$operands.ToArray()
    }

    $option = Get-CommandLineGlobalOption -CommandWord $CommandWord
    $tokens = @($resolved.Segment.Tokens)
    $index = $resolved.OperandIndex
    $afterSeparator = $false

    while ($index -lt $tokens.Count) {
        $current = $tokens[$index]

        if ($afterSeparator) {
            $operands.Add($current)
            $index++
            continue
        }
        if ($current -eq '--') {
            $afterSeparator = $true
            $index++
            continue
        }

        if ($current.StartsWith('-') -and $current.Length -gt 1) {
            $name = $current
            $hasInlineValue = $false
            $split = $current.IndexOf('=')
            if ($split -gt 0) {
                $name = $current.Substring(0, $split)
                $hasInlineValue = $true
            }

            if ($option.WithArgument -contains $name) {
                $index += if ($hasInlineValue) { 1 } else { 2 }
                continue
            }
            if ($option.Standalone -contains $name -or $script:CommandLineStandaloneFlagNames -contains $name) {
                $index++
                continue
            }
            break
        }

        $operands.Add($current)
        $index++
    }

    return [string[]]$operands.ToArray()
}

function Get-CommandLineFlagValue {
    <#
    .SYNOPSIS
        Return the value of a named flag belonging to a matched invocation.
    .DESCRIPTION
        Locates the matched segment as Test-CommandLineInvocation does, then searches that
        segment's tokens for FlagName. Recognizes both the separated form ('--merge 688') and
        the equals form ('--merge=688'). Returns $null when the flag is absent, when the flag
        is present with no following value token, and when the following token is itself
        dash-leading.

        The $null-on-missing-value contract is mandatory and is pinned by issue #591's first
        constraint: downstream logic treats a missing explicit pull-request number as a
        fail-closed condition, so a bare 'gh pr merge --merge' must yield $null, never 0 and
        never ''.
    .PARAMETER CommandText
        The raw Bash command text.
    .PARAMETER CommandWord
        The command name.
    .PARAMETER SubcommandPath
        The ordered subcommand tokens.
    .PARAMETER FlagName
        The flag as written, including leading dashes, e.g. '--merge' or '--body-file'.
        Compared case-insensitively.
    .OUTPUTS
        System.String or $null. Quotes are already stripped. Callers that need an integer cast
        the result themselves after a $null check.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)][AllowEmptyString()][AllowNull()][string] $CommandText,
        [Parameter(Mandatory)][string] $CommandWord,
        [Parameter(Mandatory)][ValidateNotNullOrEmpty()][string[]] $SubcommandPath,
        [Parameter(Mandatory)][ValidateNotNullOrEmpty()][string] $FlagName
    )

    $resolved = Resolve-CommandLineInvocation -CommandText $CommandText -CommandWord $CommandWord -SubcommandPath $SubcommandPath
    if ($null -eq $resolved) { return $null }

    $tokens = @($resolved.Segment.Tokens)
    $prefix = $FlagName + '='
    for ($index = 0; $index -lt $tokens.Count; $index++) {
        $current = $tokens[$index]

        if ($current -eq $FlagName) {
            if ($index + 1 -ge $tokens.Count) { return $null }
            $value = $tokens[$index + 1]
            if ($value.StartsWith('-') -and $value.Length -gt 1) { return $null }
            return $value
        }

        if ($current.StartsWith($prefix, [System.StringComparison]::OrdinalIgnoreCase)) {
            $value = $current.Substring($prefix.Length)
            if ([string]::IsNullOrEmpty($value)) { return $null }
            return $value
        }
    }

    return $null
}

function Test-CommandLineFlag {
    <#
    .SYNOPSIS
        Report whether a named flag is present on a matched invocation, regardless of whether
        it carries a value.
    .DESCRIPTION
        Presence-only companion to Get-CommandLineFlagValue, for boolean flags such as
        '--merge', '--force', and '--body'. Matches both '--flag' and '--flag=value'. Exact
        token comparison after quote stripping: '--body' does not match '--body-file', which
        is the distinction the pr-author hook's negative lookahead currently expresses as
        '--body(?!-file)\b'.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)][AllowEmptyString()][AllowNull()][string] $CommandText,
        [Parameter(Mandatory)][string] $CommandWord,
        [Parameter(Mandatory)][ValidateNotNullOrEmpty()][string[]] $SubcommandPath,
        [Parameter(Mandatory)][ValidateNotNullOrEmpty()][string] $FlagName
    )

    $resolved = Resolve-CommandLineInvocation -CommandText $CommandText -CommandWord $CommandWord -SubcommandPath $SubcommandPath
    if ($null -eq $resolved) { return $false }

    $prefix = $FlagName + '='
    foreach ($current in @($resolved.Segment.Tokens)) {
        if ($current -eq $FlagName) { return $true }
        if ($current.StartsWith($prefix, [System.StringComparison]::OrdinalIgnoreCase)) { return $true }
    }

    return $false
}

function Test-CommandLineMention {
    <#
    .SYNOPSIS
        Report whether a command line merely MENTIONS a command word and subcommand path
        without invoking it.
    .DESCRIPTION
        Convenience inverse used by hooks that want to log or explain an allow. True when the
        raw text contains the command word and every subcommand element but
        Test-CommandLineInvocation returns false. Purely informational; no hook makes a deny
        decision from this predicate.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)][AllowEmptyString()][AllowNull()][string] $CommandText,
        [Parameter(Mandatory)][string] $CommandWord,
        [Parameter(Mandatory)][ValidateNotNullOrEmpty()][string[]] $SubcommandPath
    )

    if (Test-CommandLineInvocation -CommandText $CommandText -CommandWord $CommandWord -SubcommandPath $SubcommandPath) {
        return $false
    }

    return (Test-CommandLineRawContainment -RawText ([string]$CommandText) -CommandWord $CommandWord -SubcommandPath $SubcommandPath)
}
