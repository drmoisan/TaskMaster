<#
.SYNOPSIS
    Halt selection and input shape guards for parallel radius-drift detection.

.DESCRIPTION
    Destination-runtime PowerShell port (issue #763) of the halt-selection half of
    parallel drift detection. It ports select_halted_item and _start_rank from
    scripts/dev_tools/parallel_drift_halt.py, the halted_item_keys selection from
    scripts/dev_tools/parallel_drift_detection_cli.py, and the guards
    require_item_key, require_text, require_paths, require_enum_member,
    as_item_key, and canonical_pair from scripts/dev_tools/_parallel_drift_shape.py.
    The Python modules remain the repository authority and the parity reference.

    Every function is pure: no filesystem, subprocess, network, or clock access,
    and no input is mutated. A guard failure throws with a message naming the
    offending field. Booleans are never accepted where an integer is required,
    matching the Python reference, which rejects bool although it subclasses int.
    A function that returns an array uses the unary comma so a one-element or
    empty array is not unrolled; it declares both the array type and
    System.Object[] as output types.
    CONVENTION: this module fails fast at module scope and imports its siblings with -ErrorAction Stop.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Integer runtime types a deserialized checkpoint value may carry. Boolean is
# deliberately absent, so a boolean in an integer slot is malformed.
$script:IntegerTypeName = @('System.Int16', 'System.Int32', 'System.Int64', 'System.Byte',
    'System.SByte', 'System.UInt16', 'System.UInt32', 'System.UInt64')

function Test-ParallelDriftIntegerValue {
    <#
    .SYNOPSIS
        Report whether a value is a non-boolean integer.
    .PARAMETER Value
        Any deserialized value.
    .OUTPUTS
        System.Boolean.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([AllowNull()][object] $Value)

    return ($null -ne $Value) -and ($script:IntegerTypeName -contains $Value.GetType().FullName)
}

function Format-ParallelDriftValue {
    <#
    .SYNOPSIS
        Render a value for an error message; null renders as None, as in Python.
    .PARAMETER Value
        Any value.
    .OUTPUTS
        System.String.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param([AllowNull()][object] $Value)

    if ($null -eq $Value) { return 'None' }
    return [string]$Value
}

function ConvertTo-ParallelDriftSortedDistinct {
    <#
    .SYNOPSIS
        Deduplicate and ordinally sort a string collection.
    .DESCRIPTION
        Port of the tuple(sorted(set(...))) idiom. Ordinal comparison matches
        Python's code-point ordering; culture-sensitive sorting would not.
    .PARAMETER Entry
        The validated entries.
    .OUTPUTS
        System.String[]. Always an array, including when empty.
    #>
    [CmdletBinding()]
    [OutputType([string[]], [System.Object[]])]
    param([Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]] $Entry)

    $unique = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    # Collapse duplicates before sorting so each path appears once.
    foreach ($item in $Entry) { [void]$unique.Add($item) }
    $sorted = [System.Collections.Generic.List[string]]::new($unique)
    $sorted.Sort([System.StringComparer]::Ordinal)
    return , [string[]]$sorted.ToArray()
}

function Assert-ParallelDriftItemKey {
    <#
    .SYNOPSIS
        Require an issue_num primary key: a positive, non-boolean integer.
    .DESCRIPTION
        Port of require_item_key.
    .PARAMETER Value
        The candidate key.
    .PARAMETER FieldName
        Field label rendered into the error message.
    .OUTPUTS
        System.Int64. The validated key.
    #>
    [CmdletBinding()]
    [OutputType([long])]
    param(
        [Parameter(Mandatory = $true)][AllowNull()][object] $Value,
        [Parameter(Mandatory = $true)][string] $FieldName
    )

    if (-not (Test-ParallelDriftIntegerValue -Value $Value) -or [long]$Value -le 0) {
        throw "$FieldName must be a positive integer issue_num; found: $(Format-ParallelDriftValue -Value $Value)."
    }
    return [long]$Value
}

function Assert-ParallelDriftText {
    <#
    .SYNOPSIS
        Require a string that carries at least one non-space character.
    .DESCRIPTION
        Port of require_text.
    .PARAMETER Value
        The candidate string.
    .PARAMETER FieldName
        Field label rendered into the error message.
    .OUTPUTS
        System.String. The validated value, unchanged.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)][AllowNull()][AllowEmptyString()][object] $Value,
        [Parameter(Mandatory = $true)][string] $FieldName
    )

    if ($Value -isnot [string] -or [string]::IsNullOrWhiteSpace($Value)) {
        throw "$FieldName must be a non-empty string; found: $(Format-ParallelDriftValue -Value $Value)."
    }
    return $Value
}

function Assert-ParallelDriftPathList {
    <#
    .SYNOPSIS
        Require a collection of non-blank repository paths.
    .DESCRIPTION
        Port of require_paths. A bare string is rejected rather than iterated,
        because iterating one would treat each character as a path. A blank or
        non-string entry is rejected rather than dropped, because dropping it
        would narrow the compared set and under-report drift.
    .PARAMETER Value
        The candidate collection.
    .PARAMETER FieldName
        Field label rendered into the error messages.
    .PARAMETER AllowEmpty
        Accept an empty collection. It is not accepted for escaped paths, where
        emptiness means there was no drift.
    .OUTPUTS
        System.String[]. The entries deduplicated and ordinally sorted.
    #>
    [CmdletBinding()]
    [OutputType([string[]], [System.Object[]])]
    param(
        [Parameter(Mandatory = $true)][AllowNull()][AllowEmptyCollection()][object] $Value,
        [Parameter(Mandatory = $true)][string] $FieldName,
        [switch] $AllowEmpty
    )

    if ($Value -is [string]) {
        throw "$FieldName must be a collection of paths, not a single string; found: '$Value'."
    }
    if ($Value -isnot [System.Collections.IEnumerable] -or $Value -is [System.Collections.IDictionary]) {
        throw "$FieldName must be a collection of paths; found: $(Format-ParallelDriftValue -Value $Value)."
    }

    $entry = [System.Collections.Generic.List[string]]::new()
    # Validate every entry before normalizing so an error names the offending value.
    foreach ($item in $Value) {
        if ($item -isnot [string] -or [string]::IsNullOrWhiteSpace($item)) {
            throw "$FieldName entries must be non-empty strings; found: $(Format-ParallelDriftValue -Value $item)."
        }
        $entry.Add($item)
    }
    if ($entry.Count -eq 0 -and -not $AllowEmpty) {
        throw "$FieldName must not be empty."
    }
    return , (ConvertTo-ParallelDriftSortedDistinct -Entry $entry.ToArray())
}

function Assert-ParallelDriftEnumMember {
    <#
    .SYNOPSIS
        Require a member of a fixed vocabulary.
    .DESCRIPTION
        Port of require_enum_member. Matching is case-sensitive.
    .PARAMETER Value
        The candidate member.
    .PARAMETER Vocabulary
        The permitted members.
    .PARAMETER FieldName
        Field label rendered into the error message.
    .OUTPUTS
        System.String. The validated member, unchanged.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)][AllowNull()][object] $Value,
        [Parameter(Mandatory = $true)][string[]] $Vocabulary,
        [Parameter(Mandatory = $true)][string] $FieldName
    )

    if ($Value -isnot [string] -or $Vocabulary -cnotcontains $Value) {
        throw "$FieldName must be one of $($Vocabulary -join ', '); found: $(Format-ParallelDriftValue -Value $Value)."
    }
    return $Value
}

function ConvertTo-ParallelDriftItemKey {
    <#
    .SYNOPSIS
        Read a value as an item key without throwing.
    .DESCRIPTION
        Port of as_item_key.
    .PARAMETER Value
        Any deserialized checkpoint value.
    .OUTPUTS
        System.Int64, or $null when the value is not a positive, non-boolean integer.
    #>
    [CmdletBinding()]
    [OutputType([long])]
    param([AllowNull()][object] $Value)

    if ((Test-ParallelDriftIntegerValue -Value $Value) -and [long]$Value -gt 0) {
        return [long]$Value
    }
    return $null
}

function Get-ParallelDriftCanonicalPair {
    <#
    .SYNOPSIS
        Normalize two item keys into the canonical a < b edge identity.
    .DESCRIPTION
        Port of canonical_pair, so an edge recorded in either order has one identity.
    .PARAMETER First
        One item key.
    .PARAMETER Second
        The other item key.
    .OUTPUTS
        System.Int64[]. The two keys in ascending order.
    #>
    [CmdletBinding()]
    [OutputType([long[]], [System.Object[]])]
    param([Parameter(Mandatory = $true)][long] $First, [Parameter(Mandatory = $true)][long] $Second)

    if ($First -lt $Second) { return , [long[]]@($First, $Second) }
    return , [long[]]@($Second, $First)
}

function Get-ParallelDriftStartRank {
    <#
    .SYNOPSIS
        Render a start marker as the comparable rank halt selection maximizes.
    .DESCRIPTION
        Port of _start_rank plus the ItemStart construction checks. The rank is
        (Unknown, Timestamp, ItemKey); Unknown is 1 when no usable timestamp
        exists, which ranks an item of unknown start above every timestamped
        item and so makes the timestamped item earlier-started.
    .PARAMETER Start
        A hashtable carrying ItemKey and WorktreeCreatedAt (null, blank, or a
        timestamp string).
    .OUTPUTS
        System.Management.Automation.PSCustomObject with Unknown, Timestamp, ItemKey.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param([Parameter(Mandatory = $true)][System.Collections.IDictionary] $Start)

    $itemKey = Assert-ParallelDriftItemKey -Value $Start['ItemKey'] -FieldName 'ItemStart.item_key'
    $timestamp = $Start['WorktreeCreatedAt']
    if ($null -ne $timestamp -and $timestamp -isnot [string]) {
        throw "ItemStart.worktree_created_at must be a string or None; found: $timestamp."
    }
    if ([string]::IsNullOrWhiteSpace($timestamp)) {
        return [pscustomobject]@{ Unknown = 1; Timestamp = ''; ItemKey = $itemKey }
    }
    return [pscustomobject]@{ Unknown = 0; Timestamp = $timestamp; ItemKey = $itemKey }
}

function Select-ParallelDriftHaltedItem {
    <#
    .SYNOPSIS
        Return the issue_num of the later-started item of a pair.
    .DESCRIPTION
        Port of select_halted_item: argmax over (start unknown, ordinal start
        timestamp, item key). Equal timestamps halt the larger key; a start
        present on exactly one item makes that item earlier-started; both
        unknown falls through to the item-key tie-break.
    .PARAMETER First
        One start marker.
    .PARAMETER Second
        The other start marker.
    .OUTPUTS
        System.Int64. The halted item key.
    #>
    [CmdletBinding()]
    [OutputType([long])]
    param(
        [Parameter(Mandatory = $true)][System.Collections.IDictionary] $First,
        [Parameter(Mandatory = $true)][System.Collections.IDictionary] $Second
    )

    $left = Get-ParallelDriftStartRank -Start $First
    $right = Get-ParallelDriftStartRank -Start $Second
    if ($left.ItemKey -eq $right.ItemKey) {
        throw "select_halted_item requires two distinct items; the pair names one item twice (item_key $($left.ItemKey))."
    }

    # Compare the ranks field by field in priority order; the larger rank is halted.
    $order = $left.Unknown.CompareTo($right.Unknown)
    if ($order -eq 0) { $order = [string]::CompareOrdinal($left.Timestamp, $right.Timestamp) }
    if ($order -eq 0) { $order = $left.ItemKey.CompareTo($right.ItemKey) }
    if ($order -gt 0) { return $left.ItemKey }
    return $right.ItemKey
}

function Get-ParallelDriftHaltedItemKey {
    <#
    .SYNOPSIS
        Select the halted item of every newly conflicting pair, never the drifter.
    .DESCRIPTION
        Port of halted_item_keys. The drifting key is dropped from each pair's
        candidates before the comparator runs, because halting the drifting item
        would deadlock the remediation that resolves the drift. A canonical pair
        holds two distinct keys, so one or two candidates remain: one is halted
        directly, two go through Select-ParallelDriftHaltedItem.
    .PARAMETER Item
        The checkpoint's item records, read for issue_num and worktree_created_at.
        Every record's start marker is validated, as the Python reference does.
    .PARAMETER Pair
        The canonical newly conflicting pairs, each a two-element key array.
    .PARAMETER DriftingItemKey
        The issue_num of the item whose diff escaped; never returned.
    .OUTPUTS
        System.Int64[]. The halted keys, deduplicated and ascending; always an array.
    #>
    [CmdletBinding()]
    [OutputType([long[]], [System.Object[]])]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]] $Item,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]] $Pair,
        [Parameter(Mandatory = $true)][long] $DriftingItemKey
    )

    if ($Pair.Count -eq 0) { return , [long[]]@() }

    $start = @{}
    # Build and validate one start marker per item so a pair resolves by key lookup.
    foreach ($record in $Item) {
        $marker = @{ ItemKey = $record['issue_num']; WorktreeCreatedAt = $record['worktree_created_at'] }
        $rank = Get-ParallelDriftStartRank -Start $marker
        $start[$rank.ItemKey] = $marker
    }

    $halted = [System.Collections.Generic.SortedSet[long]]::new()
    # Apply the rule to each pair independently; the set collapses an item that is
    # the halted member of several pairs.
    foreach ($entry in $Pair) {
        $candidate = @($entry | Where-Object { $_ -ne $DriftingItemKey })
        if ($candidate.Count -eq 1) {
            [void]$halted.Add([long]$candidate[0])
        } else {
            [void]$halted.Add((Select-ParallelDriftHaltedItem -First $start[[long]$candidate[0]] -Second $start[[long]$candidate[1]]))
        }
    }
    return , [long[]]@($halted)
}

Export-ModuleMember -Function `
    Assert-ParallelDriftItemKey, `
    Assert-ParallelDriftText, `
    Assert-ParallelDriftPathList, `
    Assert-ParallelDriftEnumMember, `
    ConvertTo-ParallelDriftItemKey, `
    Get-ParallelDriftCanonicalPair, `
    Get-ParallelDriftStartRank, `
    Select-ParallelDriftHaltedItem, `
    Get-ParallelDriftHaltedItemKey
