<#
.SYNOPSIS
    Integration-cost scheduling layer over the blast-radius contention relation.

.DESCRIPTION
    Destination-runtime PowerShell mirror of
    scripts/dev_tools/_blast_radius_scheduling.py (issue #722). The unchanged
    relation Test-BlastRadiusConflict decides whether two radii contend; this
    module weighs the cost of integrating two concurrent branches against the
    parallelism benefit and records a conflict as an edge only when the cost
    exceeds the configured tolerance. Conflicts within tolerance are returned as
    tolerated overlaps. Each exported function names its Python counterpart.

    Edge rule: edge iff conflict AND (hard OR tolerance_percent is 0 OR
    cost * 100 > benefit * tolerance_percent). Hard means the reasons include
    shared_surface_overlap or contract_dependency. The recorded reason is the
    first kind of the relation's reason list, so the reason enum is unchanged.

    Parity notes for maintainers:
      - The Python module is the reference implementation; the committed
        scheduling fixtures bind the two runtimes together.
      - No overlap semantics are defined here. The entry-pair overlap test, the
        mergeable exclusion, and the mergeable matcher are the sibling modules'.
      - An absent (or null) conflict_tolerance key reads as tolerance 0 with unit
        weights and durations, so the edge set equals the detected-conflict set.
      - Test-BlastRadiusConflict lives in the facade BlastRadius.psm1, which
        imports this module, so the caller supplies it as the [scriptblock]
        -Relation; an omitted relation fails fast naming the facade.
      - Every function is pure: no filesystem, subprocess, network, or clock
        access, and no input is mutated.
    CONVENTION: this module fails fast at module scope and imports its siblings with -ErrorAction Stop.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'BlastRadiusGlob.psm1') -Force -ErrorAction Stop
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'BlastRadiusConfig.psm1') -Force -ErrorAction Stop
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'BlastRadiusConflict.psm1') -Force -ErrorAction Stop
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'BlastRadiusValidation.psm1') -Force -ErrorAction Stop

# Truth-table key and the label every reader error carries.
$script:ToleranceKey = 'conflict_tolerance'
$script:ToleranceLabel = 'config["conflict_tolerance"]'

# Member names of the key and of its two nested maps. Each must be carried
# exactly, so a misspelled name fails fast instead of reading as absent.
$script:MemberName = @('tolerance_percent', 'weights', 'band_durations', 'default_band', 'append_only_paths')
$script:WeightName = @('same_file', 'possible_overlap', 'append_only', 'module')
$script:BandName = @('C1', 'C2', 'C3', 'C4')

# Reason kinds that make a conflict an edge at every tolerance: a shared surface
# or a contract dependency cannot be reconciled by a merge step.
$script:HardReasonKind = @('shared_surface_overlap', 'contract_dependency')

# The fail-fast message for an omitted relation; the facade defines the relation.
$script:RelationRequired = '-Relation is required: pass ${function:Test-BlastRadiusConflict} from the facade module BlastRadius.psm1.'

# tolerance_percent is a percentage of the benefit.
$script:Percent = 100


# Reject a boolean or non-integer value, or one below Minimum. Booleans are
# rejected explicitly because the Python reference rejects them.
function Get-RequiredInteger {
    [CmdletBinding()]
    [OutputType([long])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Value,
        [Parameter(Mandatory = $true)]
        [string] $FieldName,
        [Parameter(Mandatory = $true)]
        [long] $Minimum
    )

    $isInteger = $Value -is [int] -or $Value -is [long] -or $Value -is [int16] -or $Value -is [byte]
    if ($Value -is [bool] -or -not $isInteger) {
        $actual = if ($null -eq $Value) { 'null' } else { $Value.GetType().Name }
        throw "$FieldName must be an integer, got $actual."
    }
    if ($Value -lt $Minimum) {
        throw "$FieldName must be >= $Minimum, got $Value."
    }

    return [long]$Value
}

# Require a mapping to carry exactly Names, reporting both directions of
# mismatch in one error. Names are compared ordinally.
function Assert-ExactMemberName {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [hashtable] $Mapping,
        [Parameter(Mandatory = $true)]
        [string[]] $Name,
        [Parameter(Mandatory = $true)]
        [string] $FieldName
    )

    $present = @($Mapping.Keys | ForEach-Object { [string]$_ })
    $missing = @($Name | Where-Object { $present -cnotcontains $_ })
    $unknown = @($present | Where-Object { $Name -cnotcontains $_ } | Sort-Object)
    if ($missing.Count -gt 0 -or $unknown.Count -gt 0) {
        throw "$FieldName must carry exactly $($Name -join ', '); missing [$($missing -join ', ')], unknown [$($unknown -join ', ')]."
    }
}

# Read a nested map with exactly Names, each an integer >= 1.
function Get-RequiredIntegerMap {
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Value,
        [Parameter(Mandatory = $true)]
        [string[]] $Name,
        [Parameter(Mandatory = $true)]
        [string] $Member
    )

    $label = "$script:ToleranceLabel[""$Member""]"
    $mapping = Get-RequiredMapping -Value $Value -FieldName $label
    Assert-ExactMemberName -Mapping $mapping -Name $Name -FieldName $label
    $result = @{}
    foreach ($entry in $Name) {
        $result[$entry] = Get-RequiredInteger -Value $mapping[$entry] -FieldName "$label[""$entry""]" -Minimum 1
    }
    return $result
}

function Get-ConfigConflictTolerance {
    <#
    .SYNOPSIS
        Read and strictly validate the conflict_tolerance key of the truth table.

    .DESCRIPTION
        Port of config_conflict_tolerance. An absent or null key returns the
        strict semantics: tolerance 0, every weight 1, every band duration 1,
        default_band C1, and no append-only paths.

    .PARAMETER Config
        Parsed config/blast-radius.json.

    .OUTPUTS
        System.Collections.Hashtable. Keys tolerance_percent, weights,
        band_durations, default_band, and append_only_paths. Every error message
        names the conflict_tolerance key.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Config
    )

    $mapping = Get-RequiredMapping -Value $Config -FieldName 'config'
    $value = if ($mapping.ContainsKey($script:ToleranceKey)) { $mapping[$script:ToleranceKey] } else { $null }
    if ($null -eq $value) {
        $unit = @{}
        foreach ($entry in $script:WeightName) { $unit[$entry] = [long]1 }
        $band = @{}
        foreach ($entry in $script:BandName) { $band[$entry] = [long]1 }
        return @{ tolerance_percent = [long]0; weights = $unit; band_durations = $band; default_band = $script:BandName[0]; append_only_paths = [string[]]@() }
    }

    $member = Get-RequiredMapping -Value $value -FieldName $script:ToleranceLabel
    Assert-ExactMemberName -Mapping $member -Name $script:MemberName -FieldName $script:ToleranceLabel
    $defaultBand = $member['default_band']
    if ($defaultBand -isnot [string] -or $script:BandName -cnotcontains $defaultBand) {
        throw "$script:ToleranceLabel[""default_band""] must be one of $($script:BandName -join ', ')."
    }

    return @{
        tolerance_percent = Get-RequiredInteger -Value $member['tolerance_percent'] -FieldName "$script:ToleranceLabel[""tolerance_percent""]" -Minimum 0
        weights           = Get-RequiredIntegerMap -Value $member['weights'] -Name $script:WeightName -Member 'weights'
        band_durations    = Get-RequiredIntegerMap -Value $member['band_durations'] -Name $script:BandName -Member 'band_durations'
        default_band      = [string]$defaultBand
        append_only_paths = [string[]]@(Get-RequiredStringList -Value $member['append_only_paths'] -FieldName "$script:ToleranceLabel[""append_only_paths""]")
    }
}

# Weigh one overlapping path pair: append_only when a concrete entry of the pair
# is append-only (checked first, so a registry named by both items stays cheap),
# else same_file for two equal concrete entries, else possible_overlap.
function Get-PathPairWeight {
    [CmdletBinding()]
    [OutputType([long])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $EntryA,
        [Parameter(Mandatory = $true)]
        [string] $EntryB,
        [Parameter(Mandatory = $true)]
        [hashtable] $Tolerance
    )

    # Only a concrete entry can name an append-only file.
    $concrete = @(@($EntryA, $EntryB) | Where-Object { -not (Test-GlobEntry -Entry $_) })
    $appendOnly = [string[]]@($Tolerance['append_only_paths'])
    foreach ($entry in $concrete) {
        if (Test-MergeablePath -Entry $entry -MergeablePath $appendOnly) {
            return [long]$Tolerance['weights']['append_only']
        }
    }
    if ($concrete.Count -eq 2 -and [string]::Equals($EntryA, $EntryB, [System.StringComparison]::Ordinal)) {
        return [long]$Tolerance['weights']['same_file']
    }
    return [long]$Tolerance['weights']['possible_overlap']
}

function Get-BlastRadiusPairCost {
    <#
    .SYNOPSIS
        Compute the integer integration cost of running two items concurrently.

    .DESCRIPTION
        Port of pair_cost. Sums the weight of every overlapping path pair left
        after the mergeable exclusion, then adds the module weight times the
        number of shared modules. Mergeable paths therefore contribute 0.

    .PARAMETER RadiusA
        First radius record.

    .PARAMETER RadiusB
        Second radius record.

    .PARAMETER Config
        Parsed truth table; only mergeable_paths is read, for the exclusion the
        relation uses.

    .PARAMETER Tolerance
        A value returned by Get-ConfigConflictTolerance.

    .OUTPUTS
        System.Int64. The cost.
    #>
    [CmdletBinding()]
    [OutputType([long])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $RadiusA,
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $RadiusB,
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Config,
        [Parameter(Mandatory = $true)]
        [hashtable] $Tolerance
    )

    $left = ConvertTo-NormalizedBlastRadius -Radius $RadiusA
    $right = ConvertTo-NormalizedBlastRadius -Radius $RadiusB
    $mergeable = [string[]]@(Get-ConfigMergeablePath -Config $Config)
    $pathsA = @(Get-NonMergeablePathEntry -Entry ([string[]]@($left['paths'])) -MergeablePath $mergeable)
    $pathsB = @(Get-NonMergeablePathEntry -Entry ([string[]]@($right['paths'])) -MergeablePath $mergeable)

    # Enumerate the overlapping pairs with the relation's own indexed enumeration
    # (the Test-EntryOverlap pairs, issue #776), so the cost covers exactly the
    # pairs the relation could report.
    [long]$cost = 0
    foreach ($pair in @(Get-OverlappingPathPair -PathA $pathsA -PathB $pathsB)) {
        $cost += Get-PathPairWeight -EntryA $pair['EntryA'] -EntryB $pair['EntryB'] -Tolerance $Tolerance
    }
    $sharedModule = @(@($left['modules']) | Where-Object { @($right['modules']) -ccontains $_ })
    return $cost + ([long]$Tolerance['weights']['module'] * $sharedModule.Count)
}

function Get-BlastRadiusPairBenefit {
    <#
    .SYNOPSIS
        Compute the parallelism benefit: the shorter of the two band durations.

    .DESCRIPTION
        Port of pair_benefit. A missing band reads as default_band.

    .PARAMETER BandA
        First item's complexity band, or null.

    .PARAMETER BandB
        Second item's complexity band, or null.

    .PARAMETER Tolerance
        A value returned by Get-ConfigConflictTolerance.

    .OUTPUTS
        System.Int64. The benefit. Throws when a band is not a configured band.
    #>
    [CmdletBinding()]
    [OutputType([long])]
    param(
        [AllowNull()]
        [object] $BandA,
        [AllowNull()]
        [object] $BandB,
        [Parameter(Mandatory = $true)]
        [hashtable] $Tolerance
    )

    $duration = foreach ($band in @($BandA, $BandB)) {
        $resolved = if ($null -eq $band) { $Tolerance['default_band'] } else { $band }
        if ($resolved -isnot [string] -or $script:BandName -cnotcontains $resolved) {
            throw "band '$resolved' must be one of $($script:BandName -join ', ')."
        }
        [long]$Tolerance['band_durations'][$resolved]
    }
    return [long]([System.Math]::Min($duration[0], $duration[1]))
}

function Get-BlastRadiusPairDecision {
    <#
    .SYNOPSIS
        Decide whether one pair is an edge, a tolerated overlap, or neither.

    .DESCRIPTION
        Port of decide_pair. Reads the tolerance first (so a malformed key fails
        before the relation runs), calls the supplied -Relation exactly once
        with RadiusA first, and applies the edge rule. At tolerance 0 the edge
        flag equals the conflict verdict; cost and benefit are still reported.

    .PARAMETER RadiusA
        First radius record; passed first to the relation.

    .PARAMETER RadiusB
        Second radius record.

    .PARAMETER Config
        Parsed truth table.

    .PARAMETER BandA
        First item's complexity band, or null for default_band.

    .PARAMETER BandB
        Second item's complexity band, or null for default_band.

    .PARAMETER Relation
        The detection relation; pass ${function:Test-BlastRadiusConflict}.

    .OUTPUTS
        System.Collections.Hashtable. Keys conflict, edge, hard, cost, benefit,
        reason, and reasons. Without a conflict, edge and hard are false, cost
        and benefit are 0, reason is null, and reasons is empty.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $RadiusA,
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $RadiusB,
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Config,
        [AllowNull()]
        [object] $BandA,
        [AllowNull()]
        [object] $BandB,
        [scriptblock] $Relation
    )

    if ($null -eq $Relation) { throw $script:RelationRequired }
    $tolerance = Get-ConfigConflictTolerance -Config $Config
    $result = & $Relation -RadiusA $RadiusA -RadiusB $RadiusB -Config $Config
    if (-not $result['conflict']) {
        return @{ conflict = $false; edge = $false; hard = $false; cost = [long]0; benefit = [long]0; reason = $null; reasons = [string[]]@() }
    }

    # Project the reason records to their kinds, which keep canonical order.
    $reasons = [string[]]@($result['reasons'] | ForEach-Object { $_['kind'] })
    $hard = @($reasons | Where-Object { $script:HardReasonKind -ccontains $_ }).Count -gt 0
    $cost = Get-BlastRadiusPairCost -RadiusA $RadiusA -RadiusB $RadiusB -Config $Config -Tolerance $tolerance
    $benefit = Get-BlastRadiusPairBenefit -BandA $BandA -BandB $BandB -Tolerance $tolerance

    # Tolerance 0 is stated explicitly rather than left to the inequality, so
    # strict identity holds whatever the cost enumeration yields.
    $percent = [long]$tolerance['tolerance_percent']
    $edge = $hard -or $percent -eq 0 -or ($cost * $script:Percent -gt $benefit * $percent)
    return @{ conflict = $true; edge = $edge; hard = $hard; cost = $cost; benefit = $benefit; reason = $reasons[0]; reasons = $reasons }
}

# Validate item keys (integers, booleans rejected, distinct) and return the
# items as records sorted by key.
function Get-OrderedSchedulingItem {
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [object[]] $Item
    )

    $seen = [System.Collections.Generic.HashSet[long]]::new()
    $record = foreach ($entry in $Item) {
        $mapping = Get-RequiredMapping -Value $entry -FieldName 'scheduling item'
        $key = $mapping['key']
        $isInteger = $key -is [int] -or $key -is [long] -or $key -is [int16] -or $key -is [byte]
        if ($key -is [bool] -or -not $isInteger) {
            throw "scheduling item key must be an integer: $key."
        }
        if (-not $seen.Add([long]$key)) {
            throw "scheduling item keys must be distinct: $key."
        }
        $band = if ($mapping.ContainsKey('band')) { $mapping['band'] } else { $null }
        @{ key = [long]$key; radius = $mapping['radius']; band = $band }
    }
    return @(@($record) | Where-Object { $null -ne $_ } | Sort-Object -Property { $_['key'] })
}

function Get-BlastRadiusConflictEdge {
    <#
    .SYNOPSIS
        Build the scheduling edges and tolerated overlaps of a set of items.

    .DESCRIPTION
        Port of schedule_conflict_edges. Visits every unordered pair once in
        ascending key order. A pair whose decision is an edge is recorded as an
        edge; a conflict within tolerance is recorded as a tolerated overlap; a
        pair that does not conflict appears in neither list.

    .PARAMETER Item
        The run's items: records carrying key (an integer issue number), radius,
        and an optional band.

    .PARAMETER Config
        Parsed truth table.

    .PARAMETER Relation
        The detection relation passed to every pair decision; pass
        ${function:Test-BlastRadiusConflict}.

    .OUTPUTS
        System.Collections.Hashtable. Keys edges (ordered records a, b, reason,
        hard, cost, benefit) and tolerated_overlaps (ordered records a, b,
        reasons, cost, benefit), each sorted by (a, b) with a < b.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [object[]] $Item,
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Config,
        [scriptblock] $Relation
    )

    if ($null -eq $Relation) { throw $script:RelationRequired }
    [void](Get-ConfigConflictTolerance -Config $Config)
    $ordered = @(Get-OrderedSchedulingItem -Item $Item)

    $edge = [System.Collections.Generic.List[object]]::new()
    $tolerated = [System.Collections.Generic.List[object]]::new()
    for ($first = 0; $first -lt $ordered.Count; $first++) {
        for ($second = $first + 1; $second -lt $ordered.Count; $second++) {
            $a = $ordered[$first]
            $b = $ordered[$second]
            $decision = Get-BlastRadiusPairDecision -RadiusA $a['radius'] -RadiusB $b['radius'] -Config $Config -BandA $a['band'] -BandB $b['band'] -Relation $Relation
            if ($decision['edge']) {
                $edge.Add([ordered]@{ a = $a['key']; b = $b['key']; reason = $decision['reason']; hard = $decision['hard']; cost = $decision['cost']; benefit = $decision['benefit'] })
            } elseif ($decision['conflict']) {
                $tolerated.Add([ordered]@{ a = $a['key']; b = $b['key']; reasons = $decision['reasons']; cost = $decision['cost']; benefit = $decision['benefit'] })
            }
        }
    }

    return @{ edges = @($edge.ToArray()); tolerated_overlaps = @($tolerated.ToArray()) }
}

Export-ModuleMember -Function `
    Get-ConfigConflictTolerance, `
    Get-BlastRadiusPairCost, `
    Get-BlastRadiusPairBenefit, `
    Get-BlastRadiusPairDecision, `
    Get-BlastRadiusConflictEdge
