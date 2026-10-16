<#
.SYNOPSIS
    Operand, flag, and removal-target readers over the structural invocation matcher.
.DESCRIPTION
    Issue #824. Holds Get-CommandLineOperand, Get-CommandLineFlagValue, and
    Test-CommandLineFlag, moved out of hook-command-invocation.ps1 to keep that file under the
    500-line limit, and Resolve-CommandLineInvocationTarget, the target resolver the
    worktree-removal gates consume. Every reader takes its tokens from the FIRST Structural
    match Get-CommandLineInvocation returns; inside a wrapper payload that is the payload
    record's tokens. An Indeterminate match never yields an operand or a flag.

    Pure string logic only: no disk, process, network, clock, or environment access. It is
    dot-sourced by hook-command-invocation.ps1.
#>

function Get-CommandLineStructuralMatch {
    <#
    .SYNOPSIS
        Return the first Structural match for a command word and subcommand path, or $null.
    .OUTPUTS
        System.Management.Automation.PSCustomObject or $null
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory)][AllowEmptyString()][AllowNull()][string] $CommandText,
        [Parameter(Mandatory)][string] $CommandWord,
        [Parameter(Mandatory)][ValidateNotNullOrEmpty()][string[]] $SubcommandPath
    )

    foreach ($match in @(Get-CommandLineInvocation -CommandText $CommandText -CommandWord $CommandWord -SubcommandPath $SubcommandPath)) {
        if ($match.Status -eq 'Structural') { return $match }
    }
    return $null
}

function Get-CommandLineOperand {
    <#
    .SYNOPSIS
        Return the positional operands of the first Structural match, in source order.
    .DESCRIPTION
        Operand collection follows the issue #545 rules: a modeled option-with-argument pair
        is consumed, a known zero-argument flag such as '--force' contributes nothing, a token
        after a bare '--' is always an operand, a redirection is not an operand, and an
        unmodeled dash token ends collection. The operands come from the matched record only,
        so a chained 'cd <path> &&' segment contributes nothing (issue #591). With no
        Structural match the result is an empty array.
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

    $match = Get-CommandLineStructuralMatch -CommandText $CommandText -CommandWord $CommandWord -SubcommandPath $SubcommandPath
    if ($null -eq $match) { return [string[]]@() }
    return [string[]]@($match.Operands)
}

function Get-CommandLineFlagValue {
    <#
    .SYNOPSIS
        Return the value of a named flag belonging to the first Structural match.
    .DESCRIPTION
        Recognizes the separated form ('--merge 688') and the equals form ('--merge=688').
        Returns $null when no Structural match exists, when the flag is absent, when it has no
        following value token, and when the following token is itself dash-leading. The
        $null-on-missing-value contract is pinned by issue #591: a bare 'gh pr merge --merge'
        must yield $null, never 0 and never ''.
    .PARAMETER CommandText
        The raw Bash command text.
    .PARAMETER CommandWord
        The command name.
    .PARAMETER SubcommandPath
        The ordered subcommand tokens.
    .PARAMETER FlagName
        The flag as written, including leading dashes, e.g. '--merge' or '--body-file'.
    .OUTPUTS
        System.String or $null. Quotes are already stripped.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)][AllowEmptyString()][AllowNull()][string] $CommandText,
        [Parameter(Mandatory)][string] $CommandWord,
        [Parameter(Mandatory)][ValidateNotNullOrEmpty()][string[]] $SubcommandPath,
        [Parameter(Mandatory)][ValidateNotNullOrEmpty()][string] $FlagName
    )

    $match = Get-CommandLineStructuralMatch -CommandText $CommandText -CommandWord $CommandWord -SubcommandPath $SubcommandPath
    if ($null -eq $match) { return $null }

    $tokens = @($match.Segment.Tokens)
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
        Report whether a named flag is present on the first Structural match, with or without
        a value.
    .DESCRIPTION
        Matches both '--flag' and '--flag=value' by exact token comparison after quote
        stripping, so '--body' does not match '--body-file'. With no Structural match the
        result is $false.
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

    $match = Get-CommandLineStructuralMatch -CommandText $CommandText -CommandWord $CommandWord -SubcommandPath $SubcommandPath
    if ($null -eq $match) { return $false }

    $prefix = $FlagName + '='
    foreach ($current in @($match.Segment.Tokens)) {
        if ($current -eq $FlagName) { return $true }
        if ($current.StartsWith($prefix, [System.StringComparison]::OrdinalIgnoreCase)) { return $true }
    }

    return $false
}

function Resolve-CommandLineInvocationTarget {
    <#
    .SYNOPSIS
        Resolve the targets of every invocation of a command word and subcommand path.
    .DESCRIPTION
        Returns Status and Targets:
          NoMatch        no invocation; Targets is empty.
          Indeterminate  any match is Indeterminate, has OperandsComplete = $false, or has
                         other than exactly one literal operand; Targets is empty.
          Targets        every match is Structural with exactly one literal operand; Targets
                         holds the distinct operands (ordinal comparison) in source order and
                         is never empty.
        The worktree-removal gates authorize each target and deny on Indeterminate, so no
        unreadable operand routes to an allow.
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

    $matchList = @(Get-CommandLineInvocation -CommandText $CommandText -CommandWord $CommandWord -SubcommandPath $SubcommandPath)
    if ($matchList.Count -eq 0) {
        return [pscustomobject]@{ Status = 'NoMatch'; Targets = [string[]]@() }
    }

    $targets = [System.Collections.Generic.List[string]]::new()
    foreach ($match in $matchList) {
        $operands = @($match.Operands)
        if ($match.Status -ne 'Structural' -or -not $match.OperandsComplete -or $operands.Count -ne 1 -or
            -not (Test-CommandLineTokenLiteral -Token $operands[0])) {
            return [pscustomobject]@{ Status = 'Indeterminate'; Targets = [string[]]@() }
        }
        if (-not $targets.Contains($operands[0])) { $targets.Add($operands[0]) }
    }
    return [pscustomobject]@{ Status = 'Targets'; Targets = [string[]]$targets.ToArray() }
}
