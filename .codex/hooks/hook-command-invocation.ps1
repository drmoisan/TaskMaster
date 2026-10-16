<#
.SYNOPSIS
    Structural command-invocation matcher shared by the Claude and Codex enforcement hooks.
.DESCRIPTION
    Realizes Piece 3 of the D2 behavior contract and the retrieval surface of D12 in
    docs/features/active/2026-08-25-enforcement-hook-trigger-matches-whole-command-text-545/spec.md,
    as revised by issue #824.

    Answers whether a command line INVOKES a named command word with a named subcommand
    path, rather than merely containing those words. Every record produced by
    Read-CommandLineInvocationSegment (top-level segments, wrapper payloads, substitution
    bodies, and xargs-injected commands) is classified Structural, Indeterminate, or not a
    match. An Indeterminate match is fail-closed for every consumer: it means the command may
    invoke the governed command and its target cannot be read. Classification never rests on
    substring containment; every presence test is whole-token (Test-CommandLineWordPresent).

    Pure string logic only: no disk, process, network, clock, or environment access. It is
    dot-sourced as: . (Join-Path $PSScriptRoot 'hook-command-invocation.ps1')
#>

. (Join-Path $PSScriptRoot 'hook-command-scanner.ps1')
. (Join-Path $PSScriptRoot 'hook-command-payload.ps1')
. (Join-Path $PSScriptRoot 'hook-command-payload-powershell.ps1')
. (Join-Path $PSScriptRoot 'hook-command-invocation-operands.ps1')

# The transparent-wrapper set of D2 Piece 3 step 2. These prefix a command without changing
# which command runs, so the structural matcher skips them. All five are also members of the
# scanner's wrapper carve-out set, deliberately, so 'env git worktree remove x' classifies by
# both mechanisms. Pinned by test through Get-CommandLineTransparentWrapperName (rule R6).
$script:CommandLineTransparentWrapperNames = @('command', 'env', 'nohup', 'time', 'timeout')

# The modeled global-option tables of D2 Piece 3 step 3, the D6/D10 gh surface, and the
# issue #824 Terminal lists. An option in WithArgument consumes the following token as its
# value, or carries the value inline in the '--name=value' form; an option in Standalone
# consumes only itself ('--exec-path=<value>' is one token, so it is Standalone); a bare
# option in Terminal makes the command print and exit, so the segment is a resolved
# non-match. A dash-leading token in none of the lists is UNMODELED: its arity is unknown, so
# an unmodeled token between the command word and the subcommand classifies the segment
# Indeterminate, which every consumer treats fail-closed. Pinned by test through
# Get-CommandLineGlobalOption (rule R6).
$script:CommandLineGlobalOptions = @{
    git = [pscustomobject]@{
        WithArgument = @('-C', '-c', '--git-dir', '--work-tree', '--namespace')
        Standalone   = @('-p', '--paginate', '--no-pager', '--literal-pathspecs', '--no-optional-locks', '--bare', '--exec-path')
        Terminal     = @('--version', '-v', '--help', '-h', '--html-path', '--man-path', '--info-path', '--exec-path')
    }
    gh  = [pscustomobject]@{
        WithArgument = @('-R', '--repo')
        Standalone   = @()
        Terminal     = @('--version', '--help')
    }
    npx = [pscustomobject]@{
        WithArgument = @('-p', '--package', '-c', '--call', '--node-arg', '--userconfig')
        Standalone   = @('-y', '--yes', '--no-install', '--ignore-existing', '-q', '--quiet')
        Terminal     = @()
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
        Returns a record carrying three string arrays: WithArgument, Standalone, and Terminal.
        An unrecognized command word yields three empty arrays, so every dash-leading token
        for that command word is unmodeled and classifies Indeterminate - the fail-closed
        direction.
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

    return [pscustomobject]@{ WithArgument = @(); Standalone = @(); Terminal = @() }
}

function Test-CommandLineTokenLiteral {
    <#
    .SYNOPSIS
        Report whether a token is literal: no '$' or backtick, and not led by '(', '@', or '{'.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $Token)

    if ($Token.Contains('$') -or $Token.Contains('`')) { return $false }
    return -not ($Token.StartsWith('(') -or $Token.StartsWith('@') -or $Token.StartsWith('{'))
}

function Test-CommandLineAllWordPresent {
    <#
    .SYNOPSIS
        Report whether every word passes Test-CommandLineWordPresent on one text.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)][AllowEmptyString()][AllowNull()][string] $Text,
        [Parameter(Mandatory)][string[]] $Word
    )

    foreach ($current in $Word) {
        if (-not (Test-CommandLineWordPresent -RawText $Text -Word $current)) { return $false }
    }
    return $true
}

function Skip-CommandLineOption {
    <#
    .SYNOPSIS
        Absorb modeled options from a token list and report where they end.
    .DESCRIPTION
        Returns Index, the position of the first token that is not a modeled option; Terminal,
        set when a bare token (no '=') is in the Terminal list, which is tested before any
        other list; Unmodeled, set when a dash-leading token appears in no list; and Options,
        the ordered {Name, Value} pairs absorbed. A bare '-' and a bare '--' stop absorption.
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
    $options = [System.Collections.Generic.List[pscustomobject]]::new()
    while ($index -lt $Token.Count) {
        $current = $Token[$index]
        if (-not $current.StartsWith('-') -or $current -eq '-' -or $current -eq '--') { break }

        $name = $current
        $value = $null
        $split = $current.IndexOf('=')
        if ($split -gt 0) {
            $name = $current.Substring(0, $split)
            $value = $current.Substring($split + 1)
        } elseif (@($Option.Terminal) -contains $current) {
            return [pscustomobject]@{ Index = $index; Unmodeled = $false; Terminal = $true; Options = $options.ToArray() }
        }

        if ($Option.WithArgument -contains $name) {
            if ($null -eq $value) {
                $value = if ($index + 1 -lt $Token.Count) { $Token[$index + 1] } else { $null }
                $index++
            }
            $options.Add([pscustomobject]@{ Name = $name; Value = $value })
            $index++
            continue
        }
        if ($Option.Standalone -contains $name) {
            $options.Add([pscustomobject]@{ Name = $name; Value = $value })
            $index++
            continue
        }

        return [pscustomobject]@{ Index = $index; Unmodeled = $true; Terminal = $false; Options = $options.ToArray() }
    }

    return [pscustomobject]@{ Index = $index; Unmodeled = $false; Terminal = $false; Options = $options.ToArray() }
}

function Get-CommandLineStructuralWalk {
    <#
    .SYNOPSIS
        Walk one record's tokens for a command word and subcommand path (DC-10 rule 3).
    .DESCRIPTION
        Skips VAR=value prefixes and transparent wrappers, leaf-compares the command word, and
        absorbs modeled options before each subcommand element. Outcome is 'Match' (with
        OperandIndex and GlobalOptions), 'Terminal', 'Unmodeled', or 'NoMatch'. A NoMatch
        reports whether the command-word token or the subcommand-position token where the walk
        stopped was non-literal, and whether a transparent wrapper was skipped.
    .OUTPUTS
        System.Management.Automation.PSCustomObject
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][AllowEmptyString()][string[]] $Token,
        [Parameter(Mandatory)][string] $CommandWord,
        [Parameter(Mandatory)][string[]] $SubcommandPath,
        [Parameter(Mandatory)][pscustomobject] $Option
    )

    $result = [pscustomobject]@{
        Outcome = 'NoMatch'; OperandIndex = -1; GlobalOptions = [pscustomobject[]]@()
        SkippedWrapper = $false; CommandWordNonLiteral = $false; SubcommandNonLiteral = $false
    }
    $skip = Skip-CommandLineTransparentWrapper -Token $Token -Except $CommandWord
    $result.SkippedWrapper = $skip.SkippedWrapper
    $index = $skip.Index
    if ($index -ge $Token.Count) { return $result }
    if (-not (Test-CommandLineTokenLiteral -Token $Token[$index])) { $result.CommandWordNonLiteral = $true; return $result }
    if ((ConvertTo-CommandLineLeafWord -Word $Token[$index]) -ne $CommandWord) { return $result }
    $index++

    $globalOptions = [System.Collections.Generic.List[pscustomobject]]::new()
    foreach ($element in $SubcommandPath) {
        $skipOption = Skip-CommandLineOption -Token $Token -StartIndex $index -Option $Option
        foreach ($absorbed in @($skipOption.Options)) { $globalOptions.Add($absorbed) }
        if ($skipOption.Terminal) { $result.Outcome = 'Terminal'; return $result }
        if ($skipOption.Unmodeled) { $result.Outcome = 'Unmodeled'; return $result }
        $index = $skipOption.Index
        if ($index -ge $Token.Count) { return $result }
        if (-not (Test-CommandLineTokenLiteral -Token $Token[$index])) { $result.SubcommandNonLiteral = $true; return $result }
        if ($Token[$index] -ne $element) { return $result }
        $index++
    }

    $result.Outcome = 'Match'
    $result.OperandIndex = $index
    $result.GlobalOptions = $globalOptions.ToArray()
    return $result
}

function Get-CommandLineInvocationOperand {
    <#
    .SYNOPSIS
        Collect the operands after a structural match and report whether they are complete (DC-12).
    .DESCRIPTION
        OperandsComplete is $false for an argument-injected record, for any non-literal operand,
        for a redirection after the subcommand (a bare operator also consumes its target), and
        for an unmodeled dash token followed by another token.
    .OUTPUTS
        System.Management.Automation.PSCustomObject with Operands and Complete.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][AllowEmptyString()][string[]] $Token,
        [Parameter(Mandatory)][int] $StartIndex,
        [Parameter(Mandatory)][pscustomobject] $Option,
        [Parameter(Mandatory)][bool] $ArgumentInjected
    )

    $operands = [System.Collections.Generic.List[string]]::new()
    $complete = -not $ArgumentInjected
    $afterSeparator = $false
    $index = $StartIndex
    while ($index -lt $Token.Count) {
        $current = $Token[$index]
        if (-not $afterSeparator -and $current -match '^(<|>|\d*>|&>)') {
            $complete = $false
            $index += if ($current -match '^(\d*[<>]{1,2}&?|&>{1,2})$') { 2 } else { 1 }
            continue
        }
        if ($afterSeparator) { $operands.Add($current); $index++; continue }
        if ($current -eq '--') { $afterSeparator = $true; $index++; continue }
        if ($current.StartsWith('-') -and $current.Length -gt 1) {
            $name = $current
            $hasInlineValue = $false
            $split = $current.IndexOf('=')
            if ($split -gt 0) { $name = $current.Substring(0, $split); $hasInlineValue = $true }
            if ($Option.WithArgument -contains $name) { $index += if ($hasInlineValue) { 1 } else { 2 }; continue }
            if ($Option.Standalone -contains $name -or $script:CommandLineStandaloneFlagNames -contains $name) { $index++; continue }
            if ($index + 1 -lt $Token.Count) { $complete = $false }
            break
        }
        $operands.Add($current)
        $index++
    }
    foreach ($operand in $operands) {
        if (-not (Test-CommandLineTokenLiteral -Token $operand)) { $complete = $false }
    }
    return [pscustomobject]@{ Operands = [string[]]$operands.ToArray(); Complete = $complete }
}

function ConvertTo-CommandLineIndeterminateMatch {
    <#
    .SYNOPSIS
        Build an Indeterminate match record: no operand position, no operands, incomplete.
    .OUTPUTS
        System.Management.Automation.PSCustomObject
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory)][pscustomobject] $Record,
        [Parameter(Mandatory)][string] $Reason
    )

    return [pscustomobject]@{
        Status = 'Indeterminate'; Reason = $Reason; Segment = $Record; OperandIndex = -1
        GlobalOptions = [pscustomobject[]]@(); Operands = [string[]]@(); OperandsComplete = $false
    }
}

function Get-CommandLineInvocation {
    <#
    .SYNOPSIS
        Return every match of a command word and subcommand path, in source order.
    .DESCRIPTION
        For each iterator record the first applicable rule wins (DC-10): Unbalanced, decode
        failure, and depth limit are Indeterminate; a structural walk that reaches a Terminal
        option is no match, one that meets an unmodeled option is Indeterminate (Opaque), and
        one that matches the full path is Structural. Otherwise a wrapper with no payload or a
        transparent-wrapper-led record whose own RawText carries every word as a whole token
        is Indeterminate (Opaque); a non-literal command word or subcommand token is
        Indeterminate (DynamicPosition) when the whole command text carries every word; and a
        payload, substitution, or injected record is Indeterminate (NotProvenInert) when its
        PresenceText carries every word and its root's payload is not proven inert, at most
        once per root.
    .OUTPUTS
        System.Management.Automation.PSCustomObject[] - each carries Status ('Structural' or
        'Indeterminate'), Reason ('', 'Unbalanced', 'Opaque', 'DynamicPosition',
        'NotProvenInert', 'DepthLimit'), Segment (the iterator record), OperandIndex (-1 when
        Indeterminate), GlobalOptions, Operands, and OperandsComplete.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject[]])]
    param(
        [Parameter(Mandatory)][AllowEmptyString()][AllowNull()][string] $CommandText,
        [Parameter(Mandatory)][string] $CommandWord,
        [Parameter(Mandatory)][ValidateNotNullOrEmpty()][string[]] $SubcommandPath
    )

    $matchList = [System.Collections.Generic.List[pscustomobject]]::new()
    $records = @(Read-CommandLineInvocationSegment -CommandText $CommandText)
    if ($records.Count -eq 0) { return [pscustomobject[]]$matchList.ToArray() }
    $option = Get-CommandLineGlobalOption -CommandWord $CommandWord
    $words = [string[]](@($CommandWord) + $SubcommandPath)
    $normalized = ConvertTo-CommandLineNormalizedText -CommandText $CommandText
    $inertByRoot = @{}
    $presenceRoots = [System.Collections.Generic.HashSet[int]]::new()

    foreach ($record in $records) {
        if ($record.Unbalanced) { $matchList.Add((ConvertTo-CommandLineIndeterminateMatch -Record $record -Reason 'Unbalanced')); continue }
        if ($record.Opaque -and $record.OpaqueReason -eq 'DecodeFailure') { $matchList.Add((ConvertTo-CommandLineIndeterminateMatch -Record $record -Reason 'Opaque')); continue }
        if ($record.Opaque -and $record.OpaqueReason -eq 'DepthLimit') { $matchList.Add((ConvertTo-CommandLineIndeterminateMatch -Record $record -Reason 'DepthLimit')); continue }

        $tokens = [string[]]@($record.Tokens)
        $walk = Get-CommandLineStructuralWalk -Token $tokens -CommandWord $CommandWord -SubcommandPath $SubcommandPath -Option $option
        if ($walk.Outcome -eq 'Terminal') { continue }
        if ($walk.Outcome -eq 'Unmodeled') { $matchList.Add((ConvertTo-CommandLineIndeterminateMatch -Record $record -Reason 'Opaque')); continue }
        if ($walk.Outcome -eq 'Match') {
            $collected = Get-CommandLineInvocationOperand -Token $tokens -StartIndex $walk.OperandIndex -Option $option -ArgumentInjected ([bool]$record.ArgumentInjected)
            $matchList.Add([pscustomobject]@{
                    Status = 'Structural'; Reason = ''; Segment = $record; OperandIndex = $walk.OperandIndex
                    GlobalOptions = $walk.GlobalOptions; Operands = $collected.Operands; OperandsComplete = $collected.Complete
                })
            continue
        }

        # A transparent-wrapper-led record that expanded a payload is judged by its payload records.
        $hasChild = $record.Index + 1 -lt $records.Count -and $records[$record.Index + 1].Depth -gt $record.Depth
        $opaqueLed = ($record.Opaque -and $record.OpaqueReason -eq 'NoPayload') -or ($walk.SkippedWrapper -and -not $hasChild)
        if ($opaqueLed -and (Test-CommandLineAllWordPresent -Text $record.RawText -Word $words)) {
            $matchList.Add((ConvertTo-CommandLineIndeterminateMatch -Record $record -Reason 'Opaque'))
            continue
        }
        if (($walk.CommandWordNonLiteral -or $walk.SubcommandNonLiteral) -and (Test-CommandLineAllWordPresent -Text $normalized -Word $words)) {
            $matchList.Add((ConvertTo-CommandLineIndeterminateMatch -Record $record -Reason 'DynamicPosition'))
            continue
        }
        if ($record.Origin -eq 'TopLevel' -or $presenceRoots.Contains([int]$record.RootIndex)) { continue }
        if (-not (Test-CommandLineAllWordPresent -Text $record.PresenceText -Word $words)) { continue }
        if (-not $inertByRoot.ContainsKey($record.RootIndex)) {
            $inertByRoot[$record.RootIndex] = Test-CommandLinePayloadInert -Record $records -RootIndex $record.RootIndex
        }
        if (-not $inertByRoot[$record.RootIndex]) {
            [void]$presenceRoots.Add([int]$record.RootIndex)
            $matchList.Add((ConvertTo-CommandLineIndeterminateMatch -Record $record -Reason 'NotProvenInert'))
        }
    }

    return [pscustomobject[]]$matchList.ToArray()
}

function Resolve-CommandLineInvocation {
    <#
    .SYNOPSIS
        Return the first match of a command word and subcommand path, or $null.
    .DESCRIPTION
        Returns Segment (the matched iterator record), OperandIndex (the token position just
        past the subcommand path, or -1 when Status is 'Indeterminate'), and Status
        ('Structural' or 'Indeterminate') for the first record Get-CommandLineInvocation
        matched. An Indeterminate match means the record may invoke the command and its
        structure could not be resolved; every consumer treats it fail-closed.
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

    $first = @(Get-CommandLineInvocation -CommandText $CommandText -CommandWord $CommandWord -SubcommandPath $SubcommandPath) | Select-Object -First 1
    if ($null -eq $first) { return $null }
    return [pscustomobject]@{ Segment = $first.Segment; OperandIndex = $first.OperandIndex; Status = $first.Status }
}

function Test-CommandLineInvocation {
    <#
    .SYNOPSIS
        Report whether a command line invokes a named command word with a named
        subcommand path, structurally rather than by raw-text adjacency.
    .DESCRIPTION
        True for any match, Structural or Indeterminate, as Get-CommandLineInvocation
        defines them. This predicate is the replacement for every raw-text trigger regex
        that asked whether a governed command runs.
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

    return @(Get-CommandLineInvocation -CommandText $CommandText -CommandWord $CommandWord -SubcommandPath $SubcommandPath).Count -gt 0
}

function Test-CommandLineMention {
    <#
    .SYNOPSIS
        Report whether a command line merely MENTIONS a command word and subcommand path
        without invoking it.
    .DESCRIPTION
        True when Test-CommandLineInvocation returns false and every word occurs in the
        command text as a whole token (Test-CommandLineWordPresent). Purely informational;
        no hook makes a deny decision from this predicate.
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

    return (Test-CommandLineAllWordPresent -Text ([string]$CommandText) -Word ([string[]](@($CommandWord) + $SubcommandPath)))
}
