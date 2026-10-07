<#
.SYNOPSIS
    Pure radius-drift detection for the parallel orchestration surface.

.DESCRIPTION
    Destination-runtime PowerShell port (issue #763) of parallel drift detection:
    the checkpoint readers of scripts/dev_tools/_parallel_drift_cli_io.py,
    detect_escaped_paths, build_drift_event, and recompute_conflicts_with_observed
    of scripts/dev_tools/parallel_drift_detection.py, the per-peer helpers of
    scripts/dev_tools/_parallel_drift_scheduling.py, build_observed_radius, and
    evaluate_drift of scripts/dev_tools/parallel_drift_detection_cli.py. The
    Python modules remain the repository authority and the parity reference; the
    shared corpus tests/fixtures/parallel_drift binds the two.

    Blast-radius logic is not re-derived here: subsumption, observed-radius
    construction, radius normalization, the contention relation, and the
    integration-cost pair decision are the functions of .claude/lib/blast-radius.
    Halt selection and the shape guards live in ParallelDriftHalt.psm1.

    Every function is pure: no filesystem, subprocess, network, or clock access,
    and no input is mutated. Collections are always returned as arrays, and every
    malformed input throws with a message naming the offending field. Each
    function's [OutputType] attribute states its return type.
    CONVENTION: this module fails fast at module scope and imports its siblings with -ErrorAction Stop.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# The halt module is imported first, then the blast-radius facade, then the two
# blast-radius modules whose functions the facade does not re-export.
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'ParallelDriftHalt.psm1') -Force -ErrorAction Stop
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath '../blast-radius/BlastRadius.psm1') -Force -ErrorAction Stop
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath '../blast-radius/BlastRadiusGlob.psm1') -Force -ErrorAction Stop
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath '../blast-radius/BlastRadiusValidation.psm1') -Force -ErrorAction Stop

# The F3-owned members this module emits or compares, and the three verdicts.
$script:ItemStateInFlight = 'in_flight'
$script:ActionRaisedBlockingFinding = 'raised_blocking_finding'
$script:ActionHaltedLaterStartedItem = 'halted_later_started_item'
$script:DriftAction = @($script:ActionRaisedBlockingFinding, $script:ActionHaltedLaterStartedItem)
$script:BandName = @('C1', 'C2', 'C3', 'C4')
$script:ResultNoEscape = 'no_escape'
$script:ResultNoNewConflict = 'no_new_conflict'
$script:ResultHaltRequired = 'halt_required'

# Numeric runtime types an issue_num may carry for the equality comparisons the
# Python readers make with ==. Boolean is excluded.
$script:NumericTypeName = @('System.Int16', 'System.Int32', 'System.Int64', 'System.Byte', 'System.SByte',
    'System.UInt16', 'System.UInt32', 'System.UInt64', 'System.Double', 'System.Single', 'System.Decimal')

function Test-ParallelDriftList {
    <#
    .SYNOPSIS
        Report whether a deserialized value is a JSON list (an IList that is not a string).
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([AllowNull()][object] $Value)

    return ($Value -is [System.Collections.IList]) -and ($Value -isnot [string])
}

function Test-ParallelDriftKeyEqual {
    <#
    .SYNOPSIS
        Compare a recorded issue_num (Value) with an item key (ItemKey) as the Python == does.
    .DESCRIPTION
        True only for a numeric, non-boolean value numerically equal to the key.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([AllowNull()][object] $Value, [long] $ItemKey)

    if ($null -eq $Value -or $script:NumericTypeName -notcontains $Value.GetType().FullName) { return $false }
    return [double]$Value -eq [double]$ItemKey
}

function Get-ParallelDriftCheckpointItem {
    <#
    .SYNOPSIS
        Read the checkpoint's items[] collection as object records, in checkpoint order.
    .DESCRIPTION
        Port of checkpoint_items. An unusable collection is rejected rather than
        filtered: a dropped peer would never be evaluated against the observed
        radius, which is the under-reporting failure drift detection exists to catch.
    .PARAMETER State
        The parsed checkpoint.
    #>
    [CmdletBinding()]
    [OutputType([object[]])]
    param([Parameter(Mandatory = $true)][System.Collections.IDictionary] $State)

    $items = $State['items']
    if (-not (Test-ParallelDriftList -Value $items)) { throw 'Parallel checkpoint items must be a list.' }
    $record = [System.Collections.Generic.List[object]]::new()
    # Reject, rather than skip, any entry that is not an object.
    foreach ($entry in $items) {
        if ($entry -isnot [System.Collections.IDictionary]) {
            throw "Parallel checkpoint items[] entries must be objects; found: $entry."
        }
        $record.Add($entry)
    }
    return , $record.ToArray()
}

function Get-ParallelDriftCheckpointEdge {
    <#
    .SYNOPSIS
        Read the checkpoint's conflict_edges[] collection, read-only.
    .DESCRIPTION
        Port of conflict_edges. A non-object edge is omitted, which leaves any
        conflict over that pair reportable as new (fail closed).
    .PARAMETER State
        The parsed checkpoint.
    #>
    [CmdletBinding()]
    [OutputType([object[]])]
    param([Parameter(Mandatory = $true)][System.Collections.IDictionary] $State)

    $edges = $State['conflict_edges']
    if (-not (Test-ParallelDriftList -Value $edges)) { throw 'Parallel checkpoint conflict_edges must be a list.' }
    return , @($edges | Where-Object { $_ -is [System.Collections.IDictionary] })
}

function Get-ParallelDriftItemRecord {
    <#
    .SYNOPSIS
        Find the item record (among Item) carrying the issue_num ItemKey.
    .DESCRIPTION
        Port of item_by_key. A key the checkpoint does not track is an error.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.IDictionary])]
    param([Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]] $Item, [Parameter(Mandatory = $true)][long] $ItemKey)

    # Scan in checkpoint order; issue numbers are unique, so the first match wins.
    foreach ($record in $Item) {
        if (Test-ParallelDriftKeyEqual -Value $record['issue_num'] -ItemKey $ItemKey) { return $record }
    }
    throw "Parallel checkpoint records no items[] entry with issue_num $ItemKey."
}

function Get-ParallelDriftDeclaredPath {
    <#
    .SYNOPSIS
        Read one item record's declared blast_radius.paths verbatim.
    .DESCRIPTION
        Port of declared_paths. Entry-level shape is checked later by the escape
        detection, which rejects a blank or non-string entry.
    .PARAMETER Item
        The item record.
    #>
    [CmdletBinding()]
    [OutputType([object[]])]
    param([Parameter(Mandatory = $true)][System.Collections.IDictionary] $Item)

    $radius = $Item['blast_radius']
    if ($radius -isnot [System.Collections.IDictionary]) {
        throw "Parallel checkpoint items[] blast_radius must be an object; found: $radius."
    }
    $paths = $radius['paths']
    if (-not (Test-ParallelDriftList -Value $paths)) {
        throw "Parallel checkpoint items[] blast_radius.paths must be a list; found: $paths."
    }
    return , @($paths)
}

function Get-ParallelDriftEscapedPath {
    <#
    .SYNOPSIS
        Return the changed paths no declared radius entry covers, deduplicated and ordinally sorted.
    .DESCRIPTION
        Port of detect_escaped_paths, using the blast-radius subsumption relation
        Test-PathSubsumed so plan-time coverage and execution-time drift apply one
        predicate.
    .PARAMETER ChangedPath
        Observed repository-relative paths; may be empty.
    .PARAMETER DeclaredPath
        The declared blast_radius.paths; may be empty, which covers nothing.
    #>
    [CmdletBinding()]
    [OutputType([string[]], [System.Object[]])]
    param(
        [Parameter(Mandatory = $true)][AllowNull()][AllowEmptyCollection()][object] $ChangedPath,
        [Parameter(Mandatory = $true)][AllowNull()][AllowEmptyCollection()][object] $DeclaredPath
    )

    $changed = Assert-ParallelDriftPathList -Value $ChangedPath -FieldName 'changed' -AllowEmpty
    $declared = Assert-ParallelDriftPathList -Value $DeclaredPath -FieldName 'declared' -AllowEmpty
    $escaped = [System.Collections.Generic.List[string]]::new()
    # Test each changed path independently against the whole declared collection.
    foreach ($path in $changed) {
        if (-not (Test-PathSubsumed -Path $path -CoveringPath $declared)) { $escaped.Add($path) }
    }
    return , [string[]]$escaped.ToArray()
}

function Get-ParallelDriftEvent {
    <#
    .SYNOPSIS
        Build one drift_events[] record in the six-key design shape.
    .DESCRIPTION
        Port of build_drift_event. ItemKey must be a positive integer; the three
        path collections (DeclaredPath, ObservedPath, EscapedPath) are rendered
        deduplicated and ordinally sorted, and EscapedPath must be non-empty; At
        must be non-blank; Action must be raised_blocking_finding or
        halted_later_started_item.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory = $true)][AllowNull()][object] $ItemKey,
        [Parameter(Mandatory = $true)][AllowNull()][AllowEmptyCollection()][object] $DeclaredPath,
        [Parameter(Mandatory = $true)][AllowNull()][AllowEmptyCollection()][object] $ObservedPath,
        [Parameter(Mandatory = $true)][AllowNull()][AllowEmptyCollection()][object] $EscapedPath,
        [Parameter(Mandatory = $true)][AllowNull()][AllowEmptyString()][object] $At,
        [Parameter(Mandatory = $true)][AllowNull()][object] $Action
    )

    return [ordered]@{
        item_key      = Assert-ParallelDriftItemKey -Value $ItemKey -FieldName 'item_key'
        declared      = Assert-ParallelDriftPathList -Value $DeclaredPath -FieldName 'declared' -AllowEmpty
        observed      = Assert-ParallelDriftPathList -Value $ObservedPath -FieldName 'observed' -AllowEmpty
        escaped_paths = Assert-ParallelDriftPathList -Value $EscapedPath -FieldName 'escaped_paths'
        at            = Assert-ParallelDriftText -Value $At -FieldName 'at'
        action        = Assert-ParallelDriftEnumMember -Value $Action -Vocabulary $script:DriftAction -FieldName 'action'
    }
}

function Get-ParallelDriftExistingEdgePair {
    <#
    .SYNOPSIS
        Collect the canonical pairs already recorded as conflict edges.
    .DESCRIPTION
        Port of existing_edge_pairs. Returns distinct two-element key arrays,
        lower key first. An edge with unreadable or identical endpoints is
        omitted, leaving a conflict over that pair reportable as new.
    .PARAMETER Edge
        The object-shaped conflict_edges[] records.
    #>
    [CmdletBinding()]
    [OutputType([object[]])]
    param([Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]] $Edge)

    $seen = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    $pairs = [System.Collections.Generic.List[object]]::new()
    # Normalize before collecting so a reversed pair is never misread as new.
    foreach ($record in $Edge) {
        $first = ConvertTo-ParallelDriftItemKey -Value $record['a']
        $second = ConvertTo-ParallelDriftItemKey -Value $record['b']
        if ($null -eq $first -or $null -eq $second -or $first -eq $second) { continue }
        $pair = Get-ParallelDriftCanonicalPair -First $first -Second $second
        if ($seen.Add("$($pair[0]),$($pair[1])")) { $pairs.Add($pair) }
    }
    return , $pairs.ToArray()
}

function Get-ParallelDriftItemBand {
    <#
    .SYNOPSIS
        Read the complexity band of the record (among Item) carrying ItemKey.
    .DESCRIPTION
        Port of item_band. A band is a benefit estimate, not a safety input, so an
        absent or unreadable band yields null and the scheduling rule applies
        default_band.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]] $Item, [Parameter(Mandatory = $true)][long] $ItemKey)

    # Return the first matching item's band; issue numbers are unique.
    foreach ($record in $Item) {
        if (-not (Test-ParallelDriftKeyEqual -Value $record['issue_num'] -ItemKey $ItemKey)) { continue }
        $band = $record['complexity_band']
        if ($band -is [string] -and $script:BandName -ccontains $band) { return $band }
        return $null
    }
    return $null
}

function Test-ParallelDriftObservedPairEdge {
    <#
    .SYNOPSIS
        Decide whether a drifting item's observed radius makes a peer pair an edge.
    .DESCRIPTION
        Port of observed_pair_is_edge. A PeerRadius that is not an object, or that
        the blast-radius normalizer rejects, cannot be evaluated and counts as an
        edge (fail closed), because the relation reports no conflict for an empty
        radius. Otherwise the pair goes through the integration-cost decision with
        ObservedRadius first, the parsed Config, both bands (null applies
        default_band), and the contention relation passed explicitly; the edge key
        of the returned hashtable is read, and the hashtable itself is never
        tested for truthiness.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)][System.Collections.IDictionary] $ObservedRadius,
        [Parameter(Mandatory = $true)][AllowNull()][object] $PeerRadius,
        [Parameter(Mandatory = $true)][AllowNull()][object] $Config,
        [AllowNull()][object] $ObservedBand,
        [AllowNull()][object] $PeerBand
    )

    if ($PeerRadius -isnot [System.Collections.IDictionary]) { return $true }
    # Any normalization failure means the peer radius cannot be evaluated; the
    # Python reference catches TypeError and ValueError here for the same reason.
    try {
        $peer = ConvertTo-NormalizedBlastRadius -Radius $PeerRadius
    } catch {
        return $true
    }
    $decision = Get-BlastRadiusPairDecision -RadiusA $ObservedRadius -RadiusB $peer -Config $Config `
        -BandA $ObservedBand -BandB $PeerBand -Relation ${function:Test-BlastRadiusConflict}
    return [bool]$decision['edge']
}

function Get-ParallelDriftObservedRadius {
    <#
    .SYNOPSIS
        Build the observed-source radius from a changed-path set.
    .DESCRIPTION
        Port of build_observed_radius: ObservedPath (may be empty) and ComputedAt
        are validated here and the six-key radius is built from them and the
        parsed Config by the blast-radius library, never by hand.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)][AllowNull()][AllowEmptyCollection()][object] $ObservedPath,
        [Parameter(Mandatory = $true)][AllowNull()][object] $Config,
        [Parameter(Mandatory = $true)][AllowNull()][AllowEmptyString()][object] $ComputedAt
    )

    $paths = Assert-ParallelDriftPathList -Value $ObservedPath -FieldName 'observed_paths' -AllowEmpty
    $timestamp = Assert-ParallelDriftText -Value $ComputedAt -FieldName 'computed_at'
    return Get-BlastRadiusFromObservedPaths -ObservedPaths $paths -Config $Config -ComputedAt $timestamp
}

function Get-ParallelDriftNewConflictPair {
    <#
    .SYNOPSIS
        Return the pairs that newly conflict once the observed radius is used.
    .DESCRIPTION
        Port of recompute_conflicts_with_observed. The observed radius of
        DriftingItemKey, built from ObservedPath and stamped ComputedAt, is
        evaluated against every other in-flight record of Item through the
        scheduling rule with both items' bands and the parsed Config; a pair
        already recorded among the conflict_edges[] records in Edge is skipped.
        Every item's issue_num is validated, in flight or not. Returns canonical
        two-element key arrays in ascending order.
    #>
    [CmdletBinding()]
    [OutputType([object[]])]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]] $Item,
        [Parameter(Mandatory = $true)][AllowNull()][object] $DriftingItemKey,
        [Parameter(Mandatory = $true)][AllowNull()][AllowEmptyCollection()][object] $ObservedPath,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]] $Edge,
        [Parameter(Mandatory = $true)][AllowNull()][object] $Config,
        [Parameter(Mandatory = $true)][AllowNull()][AllowEmptyString()][object] $ComputedAt
    )

    $drifting = Assert-ParallelDriftItemKey -Value $DriftingItemKey -FieldName 'drifting_item_key'
    $observed = Get-ParallelDriftObservedRadius -ObservedPath $ObservedPath -Config $Config -ComputedAt $ComputedAt
    $existing = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    # Index the recorded edges by canonical identity for constant-time skips.
    foreach ($pair in (Get-ParallelDriftExistingEdgePair -Edge $Edge)) { [void]$existing.Add("$($pair[0]),$($pair[1])") }
    $observedBand = Get-ParallelDriftItemBand -Item $Item -ItemKey $drifting

    $found = [System.Collections.Generic.List[object]]::new()
    # Evaluate every other in-flight peer; only newly introduced contention is kept.
    for ($index = 0; $index -lt $Item.Count; $index++) {
        $record = $Item[$index]
        $itemKey = Assert-ParallelDriftItemKey -Value $record['issue_num'] -FieldName "items[$index].issue_num"
        $state = $record['state']
        $isInFlight = ($state -is [string]) -and ($state -ceq $script:ItemStateInFlight)
        if ($itemKey -eq $drifting -or -not $isInFlight) { continue }
        $pair = Get-ParallelDriftCanonicalPair -First $drifting -Second $itemKey
        if ($existing.Contains("$($pair[0]),$($pair[1])")) { continue }
        $peerBand = Get-ParallelDriftItemBand -Item $Item -ItemKey $itemKey
        if (Test-ParallelDriftObservedPairEdge -ObservedRadius $observed -PeerRadius $record['blast_radius'] -Config $Config `
                -ObservedBand $observedBand -PeerBand $peerBand) {
            $found.Add($pair)
        }
    }
    return , @($found | Sort-Object -Property @{ Expression = { $_[0] } }, @{ Expression = { $_[1] } })
}

function Get-ParallelDriftResult {
    <#
    .SYNOPSIS
        Run the detection steps over already-loaded data and build the nine-key payload.
    .DESCRIPTION
        Port of evaluate_drift. State (the parsed checkpoint, read only) supplies
        items[] and conflict_edges[]; Config is the parsed truth table; ItemKey is
        the evaluated item; ChangedPath is the observed set (may be empty); At is
        recorded on the drift event and ComputedAt on the observed radius.

        An escape is the precondition for every later step; with none,
        drift_event and observed_radius are null. Otherwise the pairs are
        recomputed, the halted keys selected, the observed radius built, and one
        drift event recorded carrying the strongest action taken.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory = $true)][System.Collections.IDictionary] $State,
        [Parameter(Mandatory = $true)][AllowNull()][object] $Config,
        [Parameter(Mandatory = $true)][long] $ItemKey,
        [Parameter(Mandatory = $true)][AllowNull()][AllowEmptyCollection()][object] $ChangedPath,
        [Parameter(Mandatory = $true)][AllowNull()][AllowEmptyString()][object] $At,
        [Parameter(Mandatory = $true)][AllowNull()][AllowEmptyString()][object] $ComputedAt
    )

    $items = Get-ParallelDriftCheckpointItem -State $State
    $declared = Get-ParallelDriftDeclaredPath -Item (Get-ParallelDriftItemRecord -Item $items -ItemKey $ItemKey)
    $escaped = Get-ParallelDriftEscapedPath -ChangedPath $ChangedPath -DeclaredPath $declared

    $pairs = @()
    $halted = [long[]]@()
    $driftEvent = $null
    $observedRadius = $null
    $result = $script:ResultNoEscape
    # Only an escape leads to recomputation, halting, an observed radius, and an event.
    if ($escaped.Count -gt 0) {
        $pairs = Get-ParallelDriftNewConflictPair -Item $items -DriftingItemKey $ItemKey -ObservedPath $ChangedPath `
            -Edge (Get-ParallelDriftCheckpointEdge -State $State) -Config $Config -ComputedAt $ComputedAt
        $halted = Get-ParallelDriftHaltedItemKey -Item $items -Pair $pairs -DriftingItemKey $ItemKey
        $observedRadius = Get-ParallelDriftObservedRadius -ObservedPath $ChangedPath -Config $Config -ComputedAt $ComputedAt
        $action = $script:ActionRaisedBlockingFinding
        $result = $script:ResultNoNewConflict
        if ($pairs.Count -gt 0) {
            $action = $script:ActionHaltedLaterStartedItem
            $result = $script:ResultHaltRequired
        }
        $driftEvent = Get-ParallelDriftEvent -ItemKey $ItemKey -DeclaredPath $declared -ObservedPath $ChangedPath `
            -EscapedPath $escaped -At $At -Action $action
    }

    return [ordered]@{
        result                  = $result
        item_key                = $ItemKey
        at                      = $At
        computed_at             = $ComputedAt
        escaped_paths           = [string[]]$escaped
        newly_conflicting_pairs = [object[]]$pairs
        halted_item_keys        = [long[]]$halted
        drift_event             = $driftEvent
        observed_radius         = $observedRadius
    }
}

Export-ModuleMember -Function `
    Get-ParallelDriftCheckpointItem, `
    Get-ParallelDriftCheckpointEdge, `
    Get-ParallelDriftItemRecord, `
    Get-ParallelDriftDeclaredPath, `
    Get-ParallelDriftEscapedPath, `
    Get-ParallelDriftEvent, `
    Get-ParallelDriftExistingEdgePair, `
    Get-ParallelDriftItemBand, `
    Test-ParallelDriftObservedPairEdge, `
    Get-ParallelDriftObservedRadius, `
    Get-ParallelDriftNewConflictPair, `
    Get-ParallelDriftResult
