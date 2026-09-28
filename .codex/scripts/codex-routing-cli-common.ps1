#Requires -Version 7.0
<#
.SYNOPSIS
    Shared helpers for the Codex routing CLI wrappers (issue #697).

.DESCRIPTION
    Dot-sourced by Resolve-CodexTopology.ps1 and Resolve-CodexDeployment.ps1 so
    the two wrappers share one implementation of module-candidate resolution,
    GNU-style token parsing, and Python-compatible JSON serialization. The
    wrappers replace `poetry run python -m scripts.dev_tools.resolve_codex_*` in
    destinations that have no Python toolchain, and must print byte-identical
    receipts, so the serializer reproduces Python
    `json.dumps(receipt, indent=2, sort_keys=True)` with `ensure_ascii=True`.

    This file defines functions only and has no top-level side effects.
#>

function Get-CodexRoutingModuleCandidate {
    <#
    .SYNOPSIS
        Return the ordered module candidate paths for one codex-routing module.
    .DESCRIPTION
        The first candidate is the published destination location
        (.codex/lib/codex-routing beside .codex/scripts). The second is the
        self-hosting location in this repository (.claude/lib/codex-routing),
        used where no .codex/lib directory exists.
    .PARAMETER ScriptRoot
        Directory of the calling wrapper (its $PSScriptRoot).
    .PARAMETER ModuleName
        Module file name, for example CodexTopology.psm1.
    .OUTPUTS
        System.String[] - the two candidate paths in resolution order.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)][string] $ScriptRoot,
        [Parameter(Mandatory = $true)][string] $ModuleName
    )

    return [string[]]@(
        (Join-Path $ScriptRoot "../lib/codex-routing/$ModuleName"),
        (Join-Path $ScriptRoot "../../.claude/lib/codex-routing/$ModuleName")
    )
}

function Resolve-CodexRoutingModulePath {
    <#
    .SYNOPSIS
        Return the first existing module candidate, or $null when none exists.
    .PARAMETER Candidate
        Candidate module paths in resolution order.
    .OUTPUTS
        System.String - the first candidate that is an existing file, or $null.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]] $Candidate
    )

    # The first existing candidate wins, so a published destination module
    # always takes precedence over the self-hosting fallback.
    foreach ($path in $Candidate) {
        if (Test-Path -LiteralPath $path -PathType Leaf) {
            return $path
        }
    }
    return $null
}

function Test-CodexRoutingOptionValueToken {
    <#
    .SYNOPSIS
        Decide whether a token can be consumed as an option value.
    .DESCRIPTION
        Mirrors argparse: a token that looks like an option (a leading '-' that
        is not a negative number) is not a value, so the preceding option
        reports that it expected one argument.
    .PARAMETER Token
        The candidate value token.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string] $Token)

    if (-not $Token.StartsWith('-')) {
        return $true
    }
    return ($Token -match '^-\d+$|^-\d*\.\d+$')
}

function ConvertFrom-CodexRoutingArgument {
    <#
    .SYNOPSIS
        Parse GNU-style CLI tokens against a wrapper specification.
    .DESCRIPTION
        Supports `--flag value` and `--flag=value`. An `append` option collects
        every occurrence; a `switch` option takes no value; for other options
        the last occurrence wins. Error messages follow argparse wording so the
        wrappers report the same failures as the Python CLIs.
    .PARAMETER Arguments
        The CLI tokens.
    .PARAMETER Specification
        Ordered dictionary from option name (for example '--language') to a
        hashtable with Key, Kind ('value', 'append', 'switch', or 'int'),
        Required, optional Choices, and Default.
    .OUTPUTS
        PSCustomObject with Values (hashtable keyed by each option's Key) and
        Error ($null, or an argparse-style 'error: ...' message).
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][AllowEmptyString()][string[]] $Arguments,
        [Parameter(Mandatory = $true)][System.Collections.IDictionary] $Specification
    )

    $values = @{}
    $appended = @{}
    $seen = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    $unrecognized = [System.Collections.Generic.List[string]]::new()

    # Seed defaults so every key is present even when its option is absent.
    foreach ($name in $Specification.Keys) {
        $entry = $Specification[$name]
        if ($entry.Kind -eq 'append') {
            $appended[$entry.Key] = [System.Collections.Generic.List[string]]::new()
        } else {
            $values[$entry.Key] = $entry.Default
        }
    }

    # Consume tokens left to right. Value errors stop immediately, as argparse
    # does; unknown options and positional tokens are reported after the
    # required-option check, matching argparse's reporting order.
    $index = 0
    while ($index -lt $Arguments.Count) {
        $token = $Arguments[$index]
        $index++
        if ($token -eq '--') {
            # Every token after the separator is positional, and neither
            # wrapper accepts positional tokens.
            for ($rest = $index; $rest -lt $Arguments.Count; $rest++) {
                $unrecognized.Add($Arguments[$rest])
            }
            break
        }
        if (-not $token.StartsWith('--') -or $token.Length -le 2) {
            $unrecognized.Add($token)
            continue
        }

        $separator = $token.IndexOf('=')
        $name = if ($separator -ge 0) { $token.Substring(0, $separator) } else { $token }
        $inlineValue = if ($separator -ge 0) { $token.Substring($separator + 1) } else { $null }
        if (-not $Specification.Contains($name)) {
            $unrecognized.Add($token)
            continue
        }
        $entry = $Specification[$name]
        [void]$seen.Add($name)

        # A switch takes no value; every other kind requires exactly one.
        if ($entry.Kind -eq 'switch') {
            if ($null -ne $inlineValue) {
                return [pscustomobject]@{ Values = $values; Error = "error: argument ${name}: ignored explicit argument '$inlineValue'" }
            }
            $values[$entry.Key] = $true
            continue
        }
        if ($null -ne $inlineValue) {
            $value = $inlineValue
        } elseif ($index -lt $Arguments.Count -and (Test-CodexRoutingOptionValueToken -Token $Arguments[$index])) {
            $value = $Arguments[$index]
            $index++
        } else {
            return [pscustomobject]@{ Values = $values; Error = "error: argument ${name}: expected one argument" }
        }

        if ($null -ne $entry.Choices -and @($entry.Choices) -cnotcontains $value) {
            $choices = (@($entry.Choices) | ForEach-Object { "'$_'" }) -join ', '
            return [pscustomobject]@{ Values = $values; Error = "error: argument ${name}: invalid choice: '$value' (choose from $choices)" }
        }
        if ($entry.Kind -eq 'int') {
            $number = 0
            $parsedNumber = [int]::TryParse(
                $value,
                [System.Globalization.NumberStyles]::Integer,
                [System.Globalization.CultureInfo]::InvariantCulture,
                [ref] $number)
            if (-not $parsedNumber) {
                return [pscustomobject]@{ Values = $values; Error = "error: argument ${name}: invalid int value: '$value'" }
            }
            $values[$entry.Key] = $number
        } elseif ($entry.Kind -eq 'append') {
            $appended[$entry.Key].Add($value)
        } else {
            $values[$entry.Key] = $value
        }
    }

    $missing = @($Specification.Keys | Where-Object { $Specification[$_].Required -and -not $seen.Contains($_) })
    if ($missing.Count -gt 0) {
        return [pscustomobject]@{ Values = $values; Error = "error: the following arguments are required: $($missing -join ', ')" }
    }
    if ($unrecognized.Count -gt 0) {
        return [pscustomobject]@{ Values = $values; Error = "error: unrecognized arguments: $($unrecognized -join ' ')" }
    }

    # Append options surface as string arrays, empty when never supplied.
    foreach ($key in $appended.Keys) {
        $values[$key] = [string[]]$appended[$key].ToArray()
    }
    return [pscustomobject]@{ Values = $values; Error = $null }
}

function ConvertTo-CodexRoutingJsonString {
    <#
    .SYNOPSIS
        Encode one string as a JSON literal the way Python json.dumps does.
    .DESCRIPTION
        Escapes quote, backslash, and the short control escapes; every other
        character below 0x20 or above 0x7E becomes a lowercase \uXXXX escape per
        UTF-16 code unit (ensure_ascii=True), so astral characters produce two.
    .PARAMETER Value
        The string to encode.
    .OUTPUTS
        System.String - the quoted JSON literal.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string] $Value)

    $builder = [System.Text.StringBuilder]::new()
    [void]$builder.Append('"')
    foreach ($character in $Value.ToCharArray()) {
        $code = [int]$character
        switch ($code) {
            0x22 { [void]$builder.Append('\"'); break }
            0x5C { [void]$builder.Append('\\'); break }
            0x08 { [void]$builder.Append('\b'); break }
            0x0C { [void]$builder.Append('\f'); break }
            0x0A { [void]$builder.Append('\n'); break }
            0x0D { [void]$builder.Append('\r'); break }
            0x09 { [void]$builder.Append('\t'); break }
            default {
                if ($code -lt 0x20 -or $code -gt 0x7E) {
                    [void]$builder.Append('\u').Append($code.ToString('x4', [System.Globalization.CultureInfo]::InvariantCulture))
                } else {
                    [void]$builder.Append($character)
                }
            }
        }
    }
    [void]$builder.Append('"')
    return $builder.ToString()
}

function ConvertTo-CodexRoutingJsonText {
    <#
    .SYNOPSIS
        Recursively render one value at an indentation level.
    .PARAMETER Value
        Null, boolean, string, integer, dictionary, or enumerable.
    .PARAMETER Level
        Current nesting depth; each level indents by two spaces.
    .OUTPUTS
        System.String
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)][AllowNull()] $Value,
        [Parameter(Mandatory = $true)][int] $Level
    )

    $inner = ' ' * (2 * ($Level + 1))
    $outer = ' ' * (2 * $Level)
    # Routing table: scalars first, then strings before enumerables (a string
    # is enumerable), then objects and arrays; anything else is rejected.
    if ($null -eq $Value) {
        return 'null'
    }
    if ($Value -is [bool]) {
        return $(if ($Value) { 'true' } else { 'false' })
    }
    if ($Value -is [string]) {
        return ConvertTo-CodexRoutingJsonString -Value $Value
    }
    if ($Value -is [int] -or $Value -is [long] -or $Value -is [int16] -or $Value -is [byte]) {
        return $Value.ToString([System.Globalization.CultureInfo]::InvariantCulture)
    }
    if ($Value -is [System.Collections.IDictionary]) {
        $keys = [string[]]@($Value.Keys)
        if ($keys.Count -eq 0) {
            return '{}'
        }
        [Array]::Sort($keys, [System.StringComparer]::Ordinal)
        $members = foreach ($key in $keys) {
            $inner + (ConvertTo-CodexRoutingJsonString -Value $key) + ': ' + (ConvertTo-CodexRoutingJsonText -Value $Value[$key] -Level ($Level + 1))
        }
        return "{`n" + ($members -join ",`n") + "`n$outer}"
    }
    if ($Value -is [System.Collections.IEnumerable]) {
        $items = @(foreach ($item in $Value) {
                $inner + (ConvertTo-CodexRoutingJsonText -Value $item -Level ($Level + 1))
            })
        if ($items.Count -eq 0) {
            return '[]'
        }
        return "[`n" + ($items -join ",`n") + "`n$outer]"
    }
    throw [System.ArgumentException]::new("unsupported JSON value type: $($Value.GetType().FullName)")
}

function ConvertTo-CodexRoutingJson {
    <#
    .SYNOPSIS
        Serialize a receipt like Python json.dumps(indent=2, sort_keys=True).
    .DESCRIPTION
        Keys are sorted with the ordinal comparer, members are separated by a
        comma and newline, and key and value are separated by ': '. The result
        carries no trailing newline; the wrapper appends one as print() does.
    .PARAMETER InputObject
        The value to serialize.
    .OUTPUTS
        System.String
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory = $true)][AllowNull()] $InputObject)

    return ConvertTo-CodexRoutingJsonText -Value $InputObject -Level 0
}

function New-CodexRoutingCliResult {
    <#
    .SYNOPSIS
        Build the result object a wrapper returns: exit code and both streams.
    .DESCRIPTION
        Creates an in-memory object only; it changes no system state. It
        declares ShouldProcess support to satisfy the repository analyzer
        settings for the New verb and returns the object under -WhatIf too.
    .PARAMETER ExitCode
        Process exit code: 0 success, 1 resolver error, 2 usage error.
    .PARAMETER Stdout
        Standard output text.
    .PARAMETER Stderr
        Standard error text.
    .OUTPUTS
        PSCustomObject with ExitCode, Stdout, and Stderr.
    #>
    [CmdletBinding(SupportsShouldProcess = $true)]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)][int] $ExitCode,
        [AllowEmptyString()][string] $Stdout = '',
        [AllowEmptyString()][string] $Stderr = ''
    )

    [void]$PSCmdlet.ShouldProcess('codex routing CLI result', 'Create')
    return [pscustomobject]@{
        ExitCode = $ExitCode
        Stdout   = $Stdout
        Stderr   = $Stderr
    }
}
