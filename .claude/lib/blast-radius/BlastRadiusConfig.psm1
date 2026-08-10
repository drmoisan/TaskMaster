<#
.SYNOPSIS
    Blast-radius input guards and truth-table resolution.

.DESCRIPTION
    Destination-runtime PowerShell port of the guard and truth-table half of
    scripts/dev_tools/_blast_radius_validation.py (require_text,
    require_str_tuple, require_mapping, config_string_list, config_modules,
    config_over_breadth_fraction, resolve_modules, resolve_shared_surfaces). The
    construction-time invariants of the BlastRadius dataclass are ported in the
    sibling module BlastRadiusValidation.psm1, which keeps every file inside the
    500-line limit.

    The Python modules remain the authoritative reference implementation. This
    module is one half of a two-language mirror; it never imports validator
    logic. Every function is pure: no filesystem, subprocess, network, or
    wall-clock access, and no input is mutated.

    Parity notes for maintainers:
      - The Python guards raise TypeError or ValueError; this port throws, which
        is the PowerShell analog of a fail-fast contract violation.
      - require_str_tuple rejects a bare string so Python cannot silently split
        it into characters. The port keeps that rejection even though PowerShell
        would not split, because the contract is "throw on malformed input" and
        the two languages must agree on which inputs are malformed.
      - A parsed truth table must be a dictionary. ConvertFrom-Json produces a
        PSCustomObject by default, so callers should either pass -AsHashtable or
        rely on the PSCustomObject conversion this module performs; no other
        shape is accepted.
      - Every returned collection is deduplicated and ordinally sorted.
#>

Set-StrictMode -Version Latest

Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'BlastRadiusExtraction.psm1') -Force
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'BlastRadiusGlob.psm1') -Force

# Keys read from the parsed config/blast-radius.json truth table.
$script:ConfigSharedSurfaceKey = 'shared_surfaces'
$script:ConfigSharedSurfaceGlobKey = 'shared_surface_globs'
$script:ConfigModuleKey = 'modules'
$script:ConfigOverBreadthKey = 'over_breadth_fraction'

# Numeric types a JSON or literal truth table may carry for the V3 threshold.
# Booleans are excluded explicitly because Python treats them as integers and the
# reference guard rejects them.
$script:NumericTypeName = @('System.Int32', 'System.Int64', 'System.Double',
    'System.Single', 'System.Decimal', 'System.Int16', 'System.Byte')


function Get-RequiredText {
    <#
    .SYNOPSIS
        Guard a caller-supplied value that must be a string.

    .DESCRIPTION
        Port of require_text. Returns the validated value unchanged so call sites
        can guard and assign in one expression.

    .PARAMETER Value
        Value of unknown runtime type.

    .PARAMETER FieldName
        Name used in the error message.

    .PARAMETER AllowEmpty
        When set, a blank string is accepted.

    .OUTPUTS
        System.String. The validated value, unchanged.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [AllowEmptyString()]
        [object] $Value,
        [Parameter(Mandatory = $true)]
        [string] $FieldName,
        [switch] $AllowEmpty
    )

    if ($Value -isnot [string]) {
        $actual = if ($null -eq $Value) { 'null' } else { $Value.GetType().Name }
        throw "$FieldName must be a string, got $actual."
    }
    if (-not $AllowEmpty -and [string]::IsNullOrWhiteSpace($Value)) {
        throw "$FieldName must not be empty."
    }

    return [string]$Value
}

function Get-RequiredStringList {
    <#
    .SYNOPSIS
        Guard a caller-supplied string collection and normalize its order.

    .DESCRIPTION
        Port of require_str_tuple. A bare string is rejected because the Python
        reference rejects it; accepting it here would make the two languages
        disagree about which inputs are malformed. Every entry is validated
        before sorting so an error names the offending value rather than a
        position in an already-reordered collection.

    .PARAMETER Value
        A collection of non-blank strings.

    .PARAMETER FieldName
        Name used in the error message.

    .OUTPUTS
        System.Object[]. Entries deduplicated and ordinally sorted for parity.
    #>
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Value,
        [Parameter(Mandatory = $true)]
        [string] $FieldName
    )

    if ($Value -is [string] -or $Value -isnot [System.Collections.IEnumerable]) {
        $actual = if ($null -eq $Value) { 'null' } else { $Value.GetType().Name }
        throw "$FieldName must be a list or tuple, got $actual."
    }

    $entry = [System.Collections.Generic.List[string]]::new()
    foreach ($item in $Value) {
        $entry.Add((Get-RequiredText -Value $item -FieldName "$FieldName entry"))
    }

    return @(Get-OrdinalSortedEntry -Entry $entry.ToArray())
}

function Get-RequiredMapping {
    <#
    .SYNOPSIS
        Guard a caller-supplied mapping such as the parsed truth table.

    .DESCRIPTION
        Port of require_mapping. A dictionary is returned as a hashtable view; a
        PSCustomObject, which is what ConvertFrom-Json yields without
        -AsHashtable, is converted from its properties. The input is read only
        and never mutated.

    .PARAMETER Value
        Value of unknown runtime type.

    .PARAMETER FieldName
        Name used in the error message.

    .OUTPUTS
        System.Collections.Hashtable. The validated mapping.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Value,
        [Parameter(Mandatory = $true)]
        [string] $FieldName
    )

    if ($Value -is [hashtable]) {
        return $Value
    }

    # The two remaining accepted shapes are any other IDictionary (for example an
    # ordered dictionary) and the PSCustomObject ConvertFrom-Json produces; both
    # are copied key by key so the caller's object is never mutated.
    $copy = @{}
    if ($Value -is [System.Collections.IDictionary]) {
        foreach ($key in $Value.Keys) {
            $copy[$key] = $Value[$key]
        }
        return $copy
    }
    if ($Value -is [System.Management.Automation.PSCustomObject]) {
        foreach ($property in $Value.PSObject.Properties) {
            $copy[$property.Name] = $property.Value
        }
        return $copy
    }

    $actual = if ($null -eq $Value) { 'null' } else { $Value.GetType().Name }
    throw "$FieldName must be a mapping, got $actual."
}

function Get-ConfigStringList {
    <#
    .SYNOPSIS
        Read an optional list-of-strings entry from the truth table.

    .DESCRIPTION
        Port of config_string_list. An absent or null key yields an empty list so
        a minimal truth table stays usable.

    .PARAMETER Config
        Parsed config/blast-radius.json.

    .PARAMETER Key
        Truth-table key to read.

    .OUTPUTS
        System.Object[]. Entries sorted and deduplicated.
    #>
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Config,
        [Parameter(Mandatory = $true)]
        [string] $Key
    )

    $mapping = Get-RequiredMapping -Value $Config -FieldName 'config'
    if (-not $mapping.ContainsKey($Key) -or $null -eq $mapping[$Key]) {
        return @()
    }

    return @(Get-RequiredStringList -Value $mapping[$Key] -FieldName "config[""$Key""]")
}

function Get-ConfigRootSurface {
    <#
    .SYNOPSIS
        Read the separator-free subset of the configured shared surfaces.

    .DESCRIPTION
        Port of config_root_surfaces. This is the sole source of separator-free
        path acceptance (issue #452). The extraction module has no access to the
        truth table, so both entry points that must agree, Get-BlastRadius and
        Test-BlastRadius, call this reader on the same -Config value and forward
        the result as -RootSurface. Deriving the set from the shared_surfaces
        list rather than a second hardcoded list is what keeps extraction and
        surface resolution from desynchronizing.

        shared_surface_globs is deliberately not a source: a glob can never be an
        exact token match, and admitting one would classify as a glob rather than
        a concrete path.

    .PARAMETER Config
        Parsed config/blast-radius.json. Only the shared_surfaces key is read.

    .OUTPUTS
        System.Object[]. The shared_surfaces entries carrying no '/', ordinally
        sorted and deduplicated by the underlying reader. A config with no
        shared_surfaces key yields an empty array, which reproduces pre-change
        behavior.
    #>
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Config
    )

    $listed = @(Get-ConfigStringList -Config $Config -Key $script:ConfigSharedSurfaceKey)

    # Keep only the entries a bare inline-code token could match exactly. A
    # surface carrying a separator is already reachable through the ordinary
    # path-shape rules, so admitting it here would widen nothing; a
    # separator-free surface is the only kind the classifier's separator test
    # made unreachable.
    $rootSurface = [System.Collections.Generic.List[string]]::new()
    foreach ($surface in $listed) {
        if (-not $surface.Contains('/')) {
            $rootSurface.Add($surface)
        }
    }

    return @($rootSurface.ToArray())
}

function Get-ConfigModuleEntry {
    <#
    .SYNOPSIS
        Read the module map from the truth table as ordered name/glob pairs.

    .DESCRIPTION
        Port of config_modules. Each module is validated as it is read so a
        malformed truth table fails at the first offending module rather than
        producing a partial resolution. Pairs are returned ordered by module name
        so resolution is deterministic in both languages.

    .PARAMETER Config
        Parsed config/blast-radius.json.

    .OUTPUTS
        System.Object[]. Hashtables with keys name and globs, ordered by name.
    #>
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Config
    )

    $mapping = Get-RequiredMapping -Value $Config -FieldName 'config'
    if (-not $mapping.ContainsKey($script:ConfigModuleKey) -or
        $null -eq $mapping[$script:ConfigModuleKey]) {
        return @()
    }

    $moduleMap = Get-RequiredMapping -Value $mapping[$script:ConfigModuleKey] `
        -FieldName "config[""$script:ConfigModuleKey""]"

    # Validate every key before ordering so the error names the offending module
    # rather than a position in an already-sorted collection.
    $name = [System.Collections.Generic.List[string]]::new()
    foreach ($key in $moduleMap.Keys) {
        $name.Add((Get-RequiredText -Value $key -FieldName "config[""$script:ConfigModuleKey""] key"))
    }

    $pair = [System.Collections.Generic.List[hashtable]]::new()
    foreach ($module in @(Get-OrdinalSortedEntry -Entry $name.ToArray())) {
        $globs = @(Get-RequiredStringList -Value $moduleMap[$module] `
                -FieldName "config[""$script:ConfigModuleKey""][$module]")
        $pair.Add(@{ name = $module; globs = $globs })
    }

    return @($pair.ToArray())
}

function Get-ConfigOverBreadthFraction {
    <#
    .SYNOPSIS
        Read the V3 over-breadth threshold from the truth table.

    .DESCRIPTION
        Port of config_over_breadth_fraction. Booleans are rejected explicitly
        because Python treats them as integers, and the value must lie within
        (0, 1].

    .PARAMETER Config
        Parsed config/blast-radius.json.

    .OUTPUTS
        System.Double. The fraction of tracked files above which a radius is
        over-broad.
    #>
    [CmdletBinding()]
    [OutputType([double])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Config
    )

    $mapping = Get-RequiredMapping -Value $Config -FieldName 'config'
    $value = $null
    if ($mapping.ContainsKey($script:ConfigOverBreadthKey)) {
        $value = $mapping[$script:ConfigOverBreadthKey]
    }

    if ($value -is [bool] -or $null -eq $value -or
        $script:NumericTypeName -notcontains $value.GetType().FullName) {
        throw "config[""$script:ConfigOverBreadthKey""] must be a number in (0, 1]."
    }

    $fraction = [double]$value
    if (-not ($fraction -gt 0 -and $fraction -le 1)) {
        throw "config[""$script:ConfigOverBreadthKey""] must be within (0, 1]."
    }

    return $fraction
}

function Resolve-BlastRadiusModule {
    <#
    .SYNOPSIS
        Resolve path entries to the module names of the truth-table map.

    .DESCRIPTION
        Port of resolve_modules. A module joins the radius as soon as one of its
        globs covers one entry, so the search stops at the first hit per module.
        A path matching no glob resolves to no module.

    .PARAMETER PathEntry
        Concrete paths and globs of a radius. An empty collection is accepted.

    .PARAMETER Config
        Parsed config/blast-radius.json.

    .OUTPUTS
        System.Object[]. Matched module names, deduplicated and ordinally sorted.
    #>
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [AllowEmptyString()]
        [string[]] $PathEntry,
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Config
    )

    $matched = [System.Collections.Generic.List[string]]::new()
    foreach ($pair in @(Get-ConfigModuleEntry -Config $Config)) {
        foreach ($pattern in $pair['globs']) {
            $hit = $false
            foreach ($entry in $PathEntry) {
                if (Test-GlobMatch -Pattern $pattern -Candidate $entry) {
                    $matched.Add([string]$pair['name'])
                    $hit = $true
                    break
                }
            }
            if ($hit) {
                break
            }
        }
    }

    return @(Get-OrdinalSortedEntry -Entry $matched.ToArray())
}

function Resolve-BlastRadiusSharedSurface {
    <#
    .SYNOPSIS
        Select the concrete paths that are shared surfaces.

    .DESCRIPTION
        Port of resolve_shared_surfaces. Membership has two independent sources,
        the literal truth-table list and the membership globs. A glob hit counts
        even when the path is absent from the literal list, which is the
        fail-closed direction.

    .PARAMETER ConcretePath
        Wildcard-free paths of a radius or plan. An empty collection is accepted.

    .PARAMETER Config
        Parsed config/blast-radius.json.

    .OUTPUTS
        System.Object[]. Touched surfaces, deduplicated and ordinally sorted.
    #>
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [AllowEmptyString()]
        [string[]] $ConcretePath,
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Config
    )

    $listed = [System.Collections.Generic.HashSet[string]]::new(
        [string[]]@(Get-ConfigStringList -Config $Config -Key $script:ConfigSharedSurfaceKey),
        [StringComparer]::Ordinal)
    $surfaceGlob = [string[]]@(Get-ConfigStringList -Config $Config -Key $script:ConfigSharedSurfaceGlobKey)

    $touched = [System.Collections.Generic.List[string]]::new()
    foreach ($path in $ConcretePath) {
        if ($listed.Contains($path)) {
            $touched.Add($path)
            continue
        }
        foreach ($pattern in $surfaceGlob) {
            if (Test-GlobMatch -Pattern $pattern -Candidate $path) {
                $touched.Add($path)
                break
            }
        }
    }

    return @(Get-OrdinalSortedEntry -Entry $touched.ToArray())
}

Export-ModuleMember -Function `
    Get-RequiredText, `
    Get-RequiredStringList, `
    Get-RequiredMapping, `
    Get-ConfigStringList, `
    Get-ConfigRootSurface, `
    Get-ConfigModuleEntry, `
    Get-ConfigOverBreadthFraction, `
    Resolve-BlastRadiusModule, `
    Resolve-BlastRadiusSharedSurface
