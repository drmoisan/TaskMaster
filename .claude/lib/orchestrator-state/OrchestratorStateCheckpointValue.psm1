<#
.SYNOPSIS
    Shared checkpoint-value primitives for the portable orchestrator-state parity checks.

.DESCRIPTION
    Sibling helper module for the orchestrator-state parity family, created under
    the plan's pre-authorized split so no parity module exceeds the repository's
    500-line file cap. It holds the primitives every ported check family needs and
    guarantees there is exactly ONE implementation of each:

      - Test-CheckpointObjectValue / Test-CheckpointListValue - the JSON shape
        predicates mirroring Python's isinstance(value, dict) / isinstance(value, list).
      - Get-CheckpointObjectMember - the strict-mode-safe member accessor that
        distinguishes an absent key from a present null, mirroring the Python
        distinction between `key not in mapping` and `mapping.get(key) is None`.
      - Get-CheckpointOrdinalSortedName - ordinal key ordering matching Python's
        sorted(), which PowerShell's culture-aware Sort-Object would not reproduce.
      - Test-PythonZeroEquivalent - Python's `value == 0` semantics, under which
        boolean False is zero-equivalent.
      - ConvertTo-PythonDisplayText / ConvertTo-PythonReprText - the str() and
        repr() interpolation renderers the inventory error templates require.

    Every function is pure: it reads no file, starts no process, and never mutates
    its input. The module imports nothing, so it is the leaf of the parity family's
    import graph and cannot participate in a load-order cycle.
#>

Set-StrictMode -Version Latest

# The numeric CLR types a JSON number can deserialize to, used by the Python
# zero-equivalence predicate so a value is compared numerically, not by type.
$script:JSON_NUMERIC_TYPES = @([int], [long], [double], [decimal], [single])


function Test-CheckpointObjectValue {
    <#
    .SYNOPSIS
        Report whether a deserialized JSON value is an object (mapping).
    .DESCRIPTION
        Shape predicate mirroring Python's isinstance(value, dict).
        ConvertFrom-Json materializes a JSON object as a PSCustomObject.
    .PARAMETER Value
        The deserialized JSON value to classify. May be $null.
    .OUTPUTS
        System.Boolean - $true only for a JSON object.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Value
    )

    return ($Value -is [System.Management.Automation.PSCustomObject])
}

function Test-CheckpointListValue {
    <#
    .SYNOPSIS
        Report whether a deserialized JSON value is an array (list).
    .DESCRIPTION
        Shape predicate mirroring Python's isinstance(value, list). A JSON string
        is deliberately not a list, matching Python, where a string is a sequence
        but not a list.
    .PARAMETER Value
        The deserialized JSON value to classify. May be $null.
    .OUTPUTS
        System.Boolean - $true only for a JSON array.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Value
    )

    return ($Value -is [System.Array])
}

function Get-CheckpointObjectMemberName {
    <#
    .SYNOPSIS
        List the member names of a deserialized JSON object.
    .DESCRIPTION
        Enumerating `$Owner.PSObject.Properties.Name` directly throws under
        Set-StrictMode when the object carries zero properties, because member
        enumeration over an empty collection has no Name member. This helper
        projects the names one property at a time so an empty JSON object ({})
        yields an empty name list instead of a terminating error.
    .PARAMETER Owner
        The deserialized JSON value expected to be an object. May be $null.
    .OUTPUTS
        System.String[] - the member names, empty for a non-object or empty object.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Owner
    )

    if (-not (Test-CheckpointObjectValue -Value $Owner)) { return [string[]]@() }
    return [string[]]@($Owner.PSObject.Properties | ForEach-Object { $_.Name })
}

function Get-CheckpointObjectMember {
    <#
    .SYNOPSIS
        Read a member from a deserialized JSON object, absent distinguished from null.
    .DESCRIPTION
        Accessor that safely reads a named property under Set-StrictMode, where
        touching an undefined property would otherwise throw. Returns both presence
        and value so callers can reproduce the Python distinction between
        `key not in mapping` and `mapping.get(key) is None`. A non-object owner
        reports the member as absent rather than throwing.
    .PARAMETER Owner
        The deserialized JSON value expected to be an object. May be $null.
    .PARAMETER Name
        The member name to read.
    .OUTPUTS
        System.Collections.Hashtable with keys Present (bool) and Value (object).
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Owner,

        [Parameter(Mandatory = $true)]
        [string] $Name
    )

    if (-not (Test-CheckpointObjectValue -Value $Owner)) {
        return @{ Present = $false; Value = $null }
    }

    $names = @(Get-CheckpointObjectMemberName -Owner $Owner)
    if ($names -contains $Name) {
        return @{ Present = $true; Value = $Owner.$Name }
    }
    return @{ Present = $false; Value = $null }
}

function Get-CheckpointOrdinalSortedName {
    <#
    .SYNOPSIS
        Sort key names by ordinal comparison, matching Python's sorted().
    .DESCRIPTION
        Python's sorted() over strings compares code points; PowerShell's
        Sort-Object is culture-aware and case-insensitive by default, which would
        reorder mixed-case key names differently. This helper pins the comparison
        to StringComparer.Ordinal so unsupported-key error ordering is identical
        across the two runtimes.
    .PARAMETER Name
        The key names to sort. May be empty.
    .OUTPUTS
        System.String[] - the ordinally sorted names.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [string[]] $Name
    )

    $sorted = [string[]]@($Name)
    [Array]::Sort($sorted, [System.StringComparer]::Ordinal)
    return $sorted
}

function Test-PythonZeroEquivalent {
    <#
    .SYNOPSIS
        Report whether a value compares equal to Python's integer 0.
    .DESCRIPTION
        Reproduces Python's `value == 0` for deserialized JSON values: integer 0
        and float 0.0 compare equal, and so does boolean False because Python
        treats False as 0. None, a non-zero number, True, a string, and any
        structure do not.
    .PARAMETER Value
        The deserialized JSON value to compare. May be $null.
    .OUTPUTS
        System.Boolean - $true when the value equals Python's 0.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Value
    )

    if ($null -eq $Value) { return $false }

    # Python's False == 0 holds, so False is zero-equivalent and True is not. The
    # boolean branch precedes the numeric branch because a CLR boolean is not one
    # of the JSON numeric types.
    if ($Value -is [bool]) { return (-not $Value) }

    # Compare the exact CLR type rather than using -is so no boxed value satisfies
    # a numeric test through an implicit conversion.
    foreach ($numericType in $script:JSON_NUMERIC_TYPES) {
        if ($Value.GetType() -eq $numericType) { return ([double]$Value -eq 0.0) }
    }
    return $false
}

function Test-PythonValueEqual {
    <#
    .SYNOPSIS
        Compare two deserialized JSON values the way Python's == would.
    .DESCRIPTION
        Shared equality predicate for the resolved-key comparisons in the codex
        receipt families, where a checkpoint value is compared against a resolver
        output. It reproduces Python's value equality over the JSON value space:
        None equals only None, booleans compare by value and never to a string,
        strings compare ordinally (Python is case-sensitive where PowerShell's
        default -eq is not), numbers compare numerically, and lists compare
        element-wise in order. A mapping compares by member name and value.
    .PARAMETER Actual
        The value read from the checkpoint. May be $null.
    .PARAMETER Expected
        The value produced by the resolver. May be $null.
    .OUTPUTS
        System.Boolean - $true when the two values are Python-equal.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Actual,

        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Expected
    )

    if ($null -eq $Expected) { return ($null -eq $Actual) }
    if ($null -eq $Actual) { return $false }

    # Booleans are compared first and never cross-compare with strings, because a
    # resolver boolean must not match the string "True" recorded in a checkpoint.
    if ($Expected -is [bool] -or $Actual -is [bool]) {
        if (-not ($Expected -is [bool]) -or -not ($Actual -is [bool])) { return $false }
        return ([bool]$Expected -eq [bool]$Actual)
    }

    # Strings compare ordinally so a case difference is a real difference, which
    # PowerShell's default case-insensitive -eq would hide.
    if ($Expected -is [string] -or $Actual -is [string]) {
        if (-not ($Expected -is [string]) -or -not ($Actual -is [string])) { return $false }
        return ([string]::Equals([string]$Expected, [string]$Actual, [System.StringComparison]::Ordinal))
    }

    # Lists compare element-wise in order; length differs, values differ.
    if ((Test-CheckpointListValue -Value $Expected) -or (Test-CheckpointListValue -Value $Actual)) {
        if (-not (Test-CheckpointListValue -Value $Expected) -or -not (Test-CheckpointListValue -Value $Actual)) { return $false }
        $expectedItems = @($Expected)
        $actualItems = @($Actual)
        if ($expectedItems.Count -ne $actualItems.Count) { return $false }
        for ($i = 0; $i -lt $expectedItems.Count; $i++) {
            if (-not (Test-PythonValueEqual -Actual $actualItems[$i] -Expected $expectedItems[$i])) { return $false }
        }
        return $true
    }

    # Mappings compare by member name and value, order-independently.
    if ((Test-CheckpointObjectValue -Value $Expected) -or (Test-CheckpointObjectValue -Value $Actual)) {
        if (-not (Test-CheckpointObjectValue -Value $Expected) -or -not (Test-CheckpointObjectValue -Value $Actual)) { return $false }
        $expectedNames = @(Get-CheckpointObjectMemberName -Owner $Expected)
        $actualNames = @(Get-CheckpointObjectMemberName -Owner $Actual)
        if ($expectedNames.Count -ne $actualNames.Count) { return $false }
        foreach ($name in $expectedNames) {
            if ($actualNames -notcontains $name) { return $false }
            $pair = @{
                Expected = (Get-CheckpointObjectMember -Owner $Expected -Name $name).Value
                Actual   = (Get-CheckpointObjectMember -Owner $Actual -Name $name).Value
            }
            if (-not (Test-PythonValueEqual -Actual $pair.Actual -Expected $pair.Expected)) { return $false }
        }
        return $true
    }

    # Everything remaining is a scalar number, compared numerically.
    return ($Expected -eq $Actual)
}

function ConvertTo-PythonDisplayText {
    <#
    .SYNOPSIS
        Render a deserialized JSON value the way Python's str() would.
    .DESCRIPTION
        Renderer for the inventory error templates that interpolate a raw value
        ({value}). Reproduces Python's str() over the JSON value space: None,
        True/False, bare strings, invariant-culture numbers, list literals, and
        dict literals. Container elements render with repr(), matching Python,
        where str() of a container calls repr() on its members.
    .PARAMETER Value
        The deserialized JSON value to render. May be $null.
    .OUTPUTS
        System.String - the rendered text.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Value
    )

    if ($null -eq $Value) { return 'None' }
    if ($Value -is [bool]) { if ($Value) { return 'True' } else { return 'False' } }
    if ($Value -is [string]) { return [string]$Value }

    # A JSON array renders as a Python list literal whose elements use repr().
    if (Test-CheckpointListValue -Value $Value) {
        $items = @(foreach ($item in $Value) { ConvertTo-PythonReprText -Value $item })
        return '[' + ($items -join ', ') + ']'
    }

    # A JSON object renders as a Python dict literal with repr() keys and values.
    if (Test-CheckpointObjectValue -Value $Value) {
        $pairs = @(foreach ($property in $Value.PSObject.Properties) {
                (ConvertTo-PythonReprText -Value $property.Name) + ': ' + (ConvertTo-PythonReprText -Value $property.Value)
            })
        return '{' + ($pairs -join ', ') + '}'
    }

    # Numbers and any remaining scalar render culture-invariantly so a non-US host
    # does not emit a comma decimal separator into a parity error string.
    return [string]::Format([System.Globalization.CultureInfo]::InvariantCulture, '{0}', $Value)
}

function ConvertTo-PythonReprText {
    <#
    .SYNOPSIS
        Render a deserialized JSON value the way Python's repr() would.
    .DESCRIPTION
        Renderer for the inventory templates that interpolate {value!r}. Only
        strings differ from str(): repr() quotes them and escapes backslashes and
        the quote character. Known divergence, recorded repo-wide at
        docs/features/potential/2026-08-07-python-repr-quote-selection-divergence.md:
        this renderer always selects single quotes, whereas CPython switches to
        double quotes when the string contains a single quote and no double quote.
    .PARAMETER Value
        The deserialized JSON value to render. May be $null.
    .OUTPUTS
        System.String - the rendered text.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Value
    )

    if ($Value -is [string]) {
        $escaped = ([string]$Value).Replace('\', '\\').Replace("'", "\'")
        return "'" + $escaped + "'"
    }
    return (ConvertTo-PythonDisplayText -Value $Value)
}

# Every primitive is exported so each sibling parity module consumes one shared
# implementation of the shape predicates, the member accessor, the ordinal sort,
# the Python zero-equivalence rule, and the str()/repr() renderers.
Export-ModuleMember -Function `
    Test-CheckpointObjectValue, `
    Test-CheckpointListValue, `
    Get-CheckpointObjectMemberName, `
    Get-CheckpointObjectMember, `
    Get-CheckpointOrdinalSortedName, `
    Test-PythonZeroEquivalent, `
    Test-PythonValueEqual, `
    ConvertTo-PythonDisplayText, `
    ConvertTo-PythonReprText
