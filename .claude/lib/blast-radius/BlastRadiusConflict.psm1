<#
.SYNOPSIS
    Mechanically-mergeable path exclusion and the shared overlap helpers.

.DESCRIPTION
    Destination-runtime PowerShell mirror of
    scripts/dev_tools/_blast_radius_mergeable.py (config_mergeable_paths,
    matches_mergeable_path, exclude_mergeable_paths), plus the two overlap helpers
    the relation calls (ports of _smallest_path_overlap and _smallest_common,
    relocated here from BlastRadius.psm1).

    The mergeable path class names the project-file shapes whose overlap a merge
    step can reconcile without re-delegating the work (issue #643). Two items
    touching the same .csproj are not genuinely in contention, so the class is
    removed from both radii immediately before the relation compares them.

    Parity notes for maintainers:
      - The exclusion is applied ONLY inside Test-BlastRadiusConflict and changes
        no radius record: a radius still lists every project file it cited, so
        drift detection and validation see the paths they saw before this key.
      - The key is optional and fail-closed: an absent mergeable_paths key and an
        empty list both exclude nothing and reproduce pre-change behaviour.
      - Every comparison is ordinal, matching the Python mirror's str equality.
    CONVENTION: this module fails fast at module scope and imports its siblings with -ErrorAction Stop.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'BlastRadiusGlob.psm1') -Force -ErrorAction Stop
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'BlastRadiusConfig.psm1') -Force -ErrorAction Stop
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'BlastRadiusNormalization.psm1') -Force -ErrorAction Stop

# Truth-table key naming the class, declared as a constant so the reader, its
# tests, and the Python mirror all name one string rather than a literal.
$script:ConfigMergeablePathKey = 'mergeable_paths'

# Prefix making a configured pattern match at any depth. It is stripped in the
# third matching step so a root-level file can satisfy it (Test-MergeablePath).
$script:AnyDepthPrefix = '**/'

# Separator used in an overlapping-pair detail string. The pair is ordered
# ordinally before formatting so the detail is identical in both argument orders.
$script:PairDetailSeparator = ' ~ '


function Get-ConfigMergeablePath {
    <#
    .SYNOPSIS
        Read the mechanically-mergeable path list from the truth table.

    .DESCRIPTION
        Port of config_mergeable_paths. The entries name project-file shapes,
        normally anchored globs such as **/*.csproj (issue #643).

    .PARAMETER Config
        Parsed config/blast-radius.json. Only the mergeable_paths key is read.

    .OUTPUTS
        System.Object[]. Entries sorted and deduplicated by the underlying reader.
        A config with no mergeable_paths key yields an empty array.
    #>
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Config
    )

    return @(Get-ConfigStringList -Config $Config -Key $script:ConfigMergeablePathKey)
}

function Test-MergeablePath {
    <#
    .SYNOPSIS
        Report whether one radius entry belongs to the mergeable path class.

    .DESCRIPTION
        Port of matches_mergeable_path. Three comparisons apply in order. The
        first two are the read-by-mandate rules, delegated to Test-MandateRead
        rather than restated: ordinal equality, the only rule that can settle a
        glob entry, then glob containment for a concrete entry only. The third is
        specific to this class, because an anchored pattern requires a separator
        and so excludes a root-level file; retesting a concrete entry with the
        anchor removed admits packages.config at the repository root.

    .PARAMETER Entry
        One radius paths entry: a concrete repository-relative path or a glob.

    .PARAMETER MergeablePath
        Configured patterns from Get-ConfigMergeablePath. An empty collection
        matches nothing.

    .OUTPUTS
        System.Boolean. True when the entry belongs to the mergeable class and is
        therefore excluded from the comparison. A glob entry is never mergeable
        unless it equals a configured pattern character for character.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $Entry,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [AllowEmptyString()]
        [string[]] $MergeablePath
    )

    # Steps one and two are the mandate-read rules verbatim, reused rather than
    # duplicated: a divergence would silently split two exclusions that share a
    # vocabulary.
    if (Test-MandateRead -Entry $Entry -MandateRead $MergeablePath) {
        return $true
    }

    # A glob entry that did not match exactly is left alone. Only a concrete path
    # reaches the anchor-stripping step.
    if (Test-GlobEntry -Entry $Entry) {
        return $false
    }

    # Retest against each anchored pattern with its anchor removed, which is the
    # only way a root-level file can satisfy an anchored pattern.
    foreach ($pattern in $MergeablePath) {
        if (-not $pattern.StartsWith($script:AnyDepthPrefix, [System.StringComparison]::Ordinal)) {
            continue
        }

        $stripped = $pattern.Substring($script:AnyDepthPrefix.Length)
        # A stripped pattern that still carries a wildcard is a glob and is
        # matched as one; a wildcard-free remainder can only be compared for
        # ordinal equality.
        if (Test-GlobEntry -Entry $stripped) {
            if (Test-GlobMatch -Pattern $stripped -Candidate $Entry) {
                return $true
            }
        } elseif ([string]::Equals($stripped, $Entry, [System.StringComparison]::Ordinal)) {
            return $true
        }
    }

    return $false
}

function Get-NonMergeablePathEntry {
    <#
    .SYNOPSIS
        Drop every mechanically-mergeable entry from a collection of paths.

    .DESCRIPTION
        Port of exclude_mergeable_paths. The caller passes the content by value,
        so the radius record is never rewritten; only the contention comparison
        sees the filtered collection.

    .PARAMETER Entry
        Radius paths entries to filter. An empty collection is accepted.

    .PARAMETER MergeablePath
        Configured patterns from Get-ConfigMergeablePath. An empty collection
        excludes nothing, so the returned content equals the input.

    .OUTPUTS
        System.Object[]. Surviving entries, deduplicated and ordinally sorted.
    #>
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [AllowEmptyString()]
        [string[]] $Entry,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [AllowEmptyString()]
        [string[]] $MergeablePath
    )

    $survivor = [System.Collections.Generic.List[string]]::new()
    foreach ($candidate in $Entry) {
        # Deduplication and ordering are the sorter's job, matching the Python
        # port's set-then-sorted construction.
        if (-not (Test-MergeablePath -Entry $candidate -MergeablePath $MergeablePath)) {
            $survivor.Add($candidate)
        }
    }

    return @(Get-OrdinalSortedEntry -Entry $survivor.ToArray())
}

function Get-SmallestPathOverlap {
    <#
    .SYNOPSIS
        Return the ordinally smallest overlapping path pair, or $null.

    .DESCRIPTION
        Port of _smallest_path_overlap. Each overlapping pair is ordered before
        it is recorded, so the minimum is taken over a set that does not depend
        on argument order; that is what makes the reported detail symmetric.

    .PARAMETER PathA
        First radius path collection. An empty collection is accepted.

    .PARAMETER PathB
        Second radius path collection. An empty collection is accepted.

    .OUTPUTS
        System.String. The smallest joined pair, or $null when nothing overlaps.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [AllowEmptyString()]
        [string[]] $PathA,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [AllowEmptyString()]
        [string[]] $PathB
    )

    $detail = [System.Collections.Generic.List[string]]::new()
    foreach ($entryA in $PathA) {
        foreach ($entryB in $PathB) {
            if (-not (Test-EntryOverlap -EntryA $entryA -EntryB $entryB)) {
                continue
            }
            $ordered = if ([string]::CompareOrdinal($entryA, $entryB) -le 0) {
                @($entryA, $entryB)
            } else {
                @($entryB, $entryA)
            }
            $detail.Add($ordered -join $script:PairDetailSeparator)
        }
    }

    return (Get-OrdinalSmallestEntry -Entry $detail.ToArray())
}

function Get-SmallestCommonEntry {
    <#
    .SYNOPSIS
        Return the ordinally smallest entry present in both collections.

    .DESCRIPTION
        Port of _smallest_common. Two empty collections share nothing, so the
        result is $null and the level contributes no reason.

    .PARAMETER Left
        First collection. An empty collection is accepted.

    .PARAMETER Right
        Second collection. An empty collection is accepted.

    .OUTPUTS
        System.String. The smallest common entry, or $null when there is none.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [AllowEmptyString()]
        [string[]] $Left,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [AllowEmptyString()]
        [string[]] $Right
    )

    $rightSet = [System.Collections.Generic.HashSet[string]]::new($Right, [StringComparer]::Ordinal)
    $common = [System.Collections.Generic.List[string]]::new()
    foreach ($entry in $Left) {
        if ($rightSet.Contains($entry)) {
            $common.Add($entry)
        }
    }

    return (Get-OrdinalSmallestEntry -Entry $common.ToArray())
}

Export-ModuleMember -Function `
    Get-ConfigMergeablePath, `
    Test-MergeablePath, `
    Get-NonMergeablePathEntry, `
    Get-SmallestPathOverlap, `
    Get-SmallestCommonEntry
