<#
.SYNOPSIS
    Dot-sourced shape-and-derivation helpers for enforce-parallel-drift-gate.ps1.

.DESCRIPTION
    Provides the eight pure helpers the Layer 1 parallel drift gate uses to read checkpoint
    shape and derive which item keys have an unresolved latest drift event. They are split out
    of the parent hook, .claude/hooks/enforce-parallel-drift-gate.ps1, because that file had
    reached the 500-line limit with no headroom; the split is a pure move with no behaviour
    change (issue #446 remediation cycle 1, finding F8-N10).

      - Test-ParallelDriftGateItemKey: positive non-boolean integer issue_num test.
      - Test-ParallelDriftGateText: non-empty string test.
      - Test-ParallelDriftGateCanonicalTimestamp: canonical yyyy-MM-ddTHH-mm shape test.
      - Test-ParallelDriftGateEventRecord: per-entry drift_events[] shape test.
      - Get-ParallelDriftGateLatestEventMap: reduces drift_events[] to each item's latest at.
      - Get-ParallelDriftGateItemRadiusMap: indexes readable items[].blast_radius by item key.
      - Test-ParallelDriftGateEventResolved: applies the re-recorded-radius resolution disjunct.
      - Get-ParallelDriftGateUnresolvedState: derives the unresolved item keys.

    Resolution semantics are owned by Python, not reimplemented here. That module's
    unresolved_drift_item_keys derives resolution from two disjuncts: (a) the recorded radius
    widened to cover every escaped path, evaluated with F1's is_path_subsumed glob semantics; or
    (b) the radius was re-recorded from a later observed diff (source == 'observed' and
    computed_at strictly greater than the event's at). These helpers implement only the narrower
    check Layer 1 needs -- latest-event selection plus disjunct (b), which is ordinal string
    comparison and needs no glob matcher. Omitting disjunct (a) can only report unresolved where
    Python reports resolved, the fail-closed direction, and the parent hook's finding-file
    allowance keeps that from deadlocking review. The cross-runtime seam test in
    tests/scripts/claude-hooks/enforce-parallel-drift-gate-helpers.Tests.ps1 runs both runtimes
    over one shared checkpoint-state table and fails when they diverge.

    Disjunct (b)'s ordinal comparison is gated on both timestamps carrying the canonical
    yyyy-MM-ddTHH-mm shape, because an ungated ordinal comparison fails open: '-' (0x2D) sorts
    below ':' (0x3A), so a colon-bearing computed_at such as 2026-01-09T10:00:00Z compares
    greater than the hyphen-bearing at 2026-01-09T10-00 even though the two name the same
    instant, and would resolve the drift with no later diff (issue #446 remediation cycle 1,
    finding F8-N4). A non-conforming value on either side is unresolved.

    This script is dot-sourced by .claude/hooks/enforce-parallel-drift-gate.ps1. It contains no
    entrypoint logic, so dot-sourcing it in tests has no side effects.

.NOTES
    PowerShell 7+, no module dependencies. Parent hook:
    .claude/hooks/enforce-parallel-drift-gate.ps1.
#>
[CmdletBinding()]
param()

# The one blast_radius.source member the narrowed Layer 1 resolution disjunct accepts. Read
# only by Test-ParallelDriftGateEventResolved, so it travels with these helpers.
$script:ObservedRadiusSource = 'observed'

# The canonical timestamp shape both sides of the disjunct (b) comparison must carry. The
# pattern text is character-identical to CANONICAL_TIMESTAMP_RE in
# scripts/dev_tools/_parallel_drift_shape.py, so the two runtimes accept the same value set.
$script:CanonicalTimestampPattern = '^\d{4}-\d{2}-\d{2}T\d{2}-\d{2}$'

function Test-ParallelDriftGateItemKey {
    <#
    .SYNOPSIS
        Report whether a checkpoint value is a positive, non-boolean integer issue_num,
        mirroring is_positive_integer in scripts/dev_tools/_parallel_state_common.py.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([AllowNull()] $Value)

    # A boolean in a numeric slot is malformed data, not a value to coerce.
    if ($Value -is [bool] -or -not ($Value -is [int] -or $Value -is [long])) {
        return $false
    }
    return ([long]$Value -gt 0)
}

function Test-ParallelDriftGateText {
    <#
    .SYNOPSIS
        Report whether a checkpoint value is a string carrying a non-space character,
        mirroring is_non_empty_string in scripts/dev_tools/_parallel_state_common.py.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([AllowNull()] $Value)

    return ($Value -is [string]) -and (-not [string]::IsNullOrWhiteSpace([string]$Value))
}

function Test-ParallelDriftGateCanonicalTimestamp {
    <#
    .SYNOPSIS
        Report whether a checkpoint value carries the canonical yyyy-MM-ddTHH-mm timestamp
        shape, mirroring CANONICAL_TIMESTAMP_RE in scripts/dev_tools/_parallel_drift_shape.py.
    .DESCRIPTION
        The match is case-sensitive (-cmatch) so the literal 'T' separator is required exactly as
        Python's case-sensitive re.match requires it; a lowercase 't' is non-conforming in both
        runtimes. A non-string, blank, truncated, or differently punctuated value reports $false.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([AllowNull()] $Value)

    if (-not (Test-ParallelDriftGateText -Value $Value)) {
        return $false
    }
    return ([string]$Value -cmatch $script:CanonicalTimestampPattern)
}

function Test-ParallelDriftGateEventRecord {
    <#
    .SYNOPSIS
        Report whether one drift_events[] entry is well formed in the three fields this
        derivation reads.
    .DESCRIPTION
        item_key must resolve as an issue_num and at must be non-empty. escaped_paths must be a
        non-empty list of non-empty strings: F3 invariant 18 rejects a zero-escape event.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([AllowNull()] $Record)

    if ($null -eq $Record -or
        -not (Test-ParallelDriftGateItemKey -Value $Record.item_key) -or
        -not (Test-ParallelDriftGateText -Value $Record.at) -or
        $Record.escaped_paths -isnot [System.Collections.IList]) {
        return $false
    }
    $escaped = @($Record.escaped_paths)
    if ($escaped.Count -eq 0) {
        return $false
    }

    # One blank or non-string entry fails the whole list, matching is_string_list.
    foreach ($path in $escaped) {
        if (-not (Test-ParallelDriftGateText -Value $path)) {
            return $false
        }
    }
    return $true
}

function Get-ParallelDriftGateLatestEventMap {
    <#
    .SYNOPSIS
        Reduce drift_events[] to the latest event timestamp of each item key, returning an
        OrderedDictionary with Malformed (bool) and LatestAt (item key to at).
    .DESCRIPTION
        Latest means the greatest at, ordinally compared so the ranking matches Python's
        string ordering, with ties broken by append order so the later-appended record wins.
        One malformed entry anywhere makes the whole log unreadable, matching
        has_unresolved_drift's malformed-log verdict. A $null checkpoint is unreadable too.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param([AllowNull()] $Checkpoint)

    $latestAt = @{}
    if ($null -eq $Checkpoint) {
        return [ordered]@{ Malformed = $true; LatestAt = $latestAt }
    }

    # An absent drift_events key is the pre-drift checkpoint shape: no events, no drift. A
    # present but non-list value cannot be reduced, so it fails closed.
    if (@($Checkpoint.PSObject.Properties.Name) -notcontains 'drift_events') {
        return [ordered]@{ Malformed = $false; LatestAt = $latestAt }
    }
    if ($Checkpoint.drift_events -isnot [System.Collections.IList]) {
        return [ordered]@{ Malformed = $true; LatestAt = $latestAt }
    }

    # Walk the append-ordered log once so each item's latest event is resolved in a single
    # pass; a malformed entry aborts the whole derivation. Replacing the record on a comparison
    # of zero is what makes append order the tie-break, matching Python's (at, index) rank.
    foreach ($record in @($Checkpoint.drift_events)) {
        if (-not (Test-ParallelDriftGateEventRecord -Record $record)) {
            return [ordered]@{ Malformed = $true; LatestAt = @{} }
        }
        $key = [long]$record.item_key
        $atText = [string]$record.at
        $comparison = 1
        if ($latestAt.ContainsKey($key)) {
            $comparison = [string]::CompareOrdinal($atText, [string]$latestAt[$key])
        }
        if ($comparison -ge 0) {
            $latestAt[$key] = $atText
        }
    }
    return [ordered]@{ Malformed = $false; LatestAt = $latestAt }
}

function Get-ParallelDriftGateItemRadiusMap {
    <#
    .SYNOPSIS
        Index the readable blast_radius blocks of items[] by item key.
    .DESCRIPTION
        An item with an unreadable issue_num or a non-object blast_radius is skipped rather
        than rejected: the caller treats absence as unresolved (fail closed), and shape
        reporting belongs to the checkpoint validator.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param([AllowNull()] $Checkpoint)

    $radii = @{}
    if ($null -eq $Checkpoint -or (@($Checkpoint.PSObject.Properties.Name) -notcontains 'items')) {
        return $radii
    }

    # Collect only the item records whose key and radius are both readable.
    foreach ($item in @($Checkpoint.items)) {
        if ($null -eq $item -or -not (Test-ParallelDriftGateItemKey -Value $item.issue_num)) {
            continue
        }
        if ($item.blast_radius -is [System.Management.Automation.PSCustomObject]) {
            $radii[[long]$item.issue_num] = $item.blast_radius
        }
    }
    return $radii
}

function Test-ParallelDriftGateEventResolved {
    <#
    .SYNOPSIS
        Apply the re-recorded-radius resolution disjunct to one item's latest drift event.
    .DESCRIPTION
        The narrowed Layer 1 check described in the script header: only the disjunct that
        needs no glob matcher is evaluated, a radius re-recorded from a diff taken after the
        event (source == 'observed' and computed_at strictly greater than the event's at).
        Comparisons are ordinal and case-sensitive so the verdict matches Python's. A missing
        or unreadable radius is unresolved (fail closed).

        Both computed_at and At must satisfy the canonical yyyy-MM-ddTHH-mm pattern before the
        ordinal comparison runs. Without that gate the comparison fails open on a
        differently punctuated timestamp, as the script header records. Either value being
        non-conforming yields $false, matching is_later_canonical_timestamp in
        scripts/dev_tools/_parallel_drift_shape.py.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowNull()] $Radius,
        [Parameter(Mandatory)][AllowEmptyString()][string] $At
    )

    if ($null -eq $Radius -or (([string]$Radius.source) -cne $script:ObservedRadiusSource)) {
        return $false
    }

    # Gate the ordinal comparison on both sides conforming; a non-conforming value on either
    # side is unresolved rather than compared against a differently shaped string.
    if (-not (Test-ParallelDriftGateCanonicalTimestamp -Value $Radius.computed_at)) {
        return $false
    }
    if (-not (Test-ParallelDriftGateCanonicalTimestamp -Value $At)) {
        return $false
    }
    return ([string]::CompareOrdinal([string]$Radius.computed_at, $At) -gt 0)
}

function Get-ParallelDriftGateUnresolvedState {
    <#
    .SYNOPSIS
        Derive the item keys whose latest drift event is still unresolved, returning an
        OrderedDictionary with Malformed (bool), UnresolvedItemKeys (long[], ascending), and
        LatestAt (item key to the latest event's at).
    .DESCRIPTION
        The PowerShell counterpart of unresolved_drift_item_keys in
        scripts/dev_tools/parallel_drift_detection.py, narrowed as the script header records.
        Malformed reports the case the Python derivation reports by raising, which
        has_unresolved_drift treats as unresolved.

        LatestAt is surfaced rather than kept internal so the parent hook's decision path can
        bind a finding file to the CURRENT unresolved drift event without deriving the latest
        event a second time (issue #446 remediation cycle 1, finding F8-N3). It is the same map
        Get-ParallelDriftGateLatestEventMap produced, passed through unchanged, and it is empty
        whenever Malformed is $true because an unreadable log yields no trustworthy timestamp.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param([AllowNull()] $Checkpoint)

    $latestState = Get-ParallelDriftGateLatestEventMap -Checkpoint $Checkpoint
    if ($latestState.Malformed) {
        return [ordered]@{ Malformed = $true; UnresolvedItemKeys = [long[]]@(); LatestAt = @{} }
    }

    # Keep the drifted items the resolution disjunct does not clear, sorted so the result is
    # deterministic and directly comparable with the Python derivation.
    $radii = Get-ParallelDriftGateItemRadiusMap -Checkpoint $Checkpoint
    $unresolved = [System.Collections.Generic.List[long]]::new()
    foreach ($itemKey in @($latestState.LatestAt.Keys)) {
        $radius = if ($radii.ContainsKey($itemKey)) { $radii[$itemKey] } else { $null }
        if (-not (Test-ParallelDriftGateEventResolved -Radius $radius -At ([string]$latestState.LatestAt[$itemKey]))) {
            $unresolved.Add([long]$itemKey)
        }
    }
    return [ordered]@{
        Malformed          = $false
        UnresolvedItemKeys = [long[]]@($unresolved | Sort-Object)
        LatestAt           = $latestState.LatestAt
    }
}
