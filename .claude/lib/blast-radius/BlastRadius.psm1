<#
.SYNOPSIS
    Blast-radius derivation and the fail-closed contention relation.

.DESCRIPTION
    Destination-runtime PowerShell facade for the blast-radius library, porting
    scripts/dev_tools/compute_blast_radius.py (derive_blast_radius,
    radius_from_observed_paths, _feature_folder_glob) and
    scripts/dev_tools/_blast_radius_conflicts.py (conflicts,
    _smallest_path_overlap, _smallest_common). It imports the extraction, glob,
    truth-table, and validation modules that sit beside it and re-exports the
    five functions the spec PowerShell surface fixes:

      - Get-PlanPaths                     port of extract_plan_paths
      - Get-BlastRadius                   port of derive_blast_radius
      - Get-BlastRadiusFromObservedPaths  port of radius_from_observed_paths
      - Test-BlastRadius                  port of validate_blast_radius
      - Test-BlastRadiusConflict          port of conflicts

    The Python modules remain the authoritative reference implementation. This
    module is one half of a two-language mirror; it never imports validator
    logic. Every function is pure: no filesystem, subprocess, network, or
    wall-clock access, and no input is mutated. computed_at is caller supplied,
    so the library never reads the clock.

    Parity notes for maintainers:
      - A radius is a hashtable whose key set is exactly paths, modules,
        shared_surfaces, contracts, source, and computed_at. Hashtable key order
        is not significant; the key set and the values are the contract.
      - Every function in this library that mirrors a Python tuple return writes
        its elements to the pipeline, following the repository's
        Get-PoshQCFileList convention. Callers must wrap such a call in @(...) to
        obtain an array, because a zero-element result writes nothing and a
        one-element result writes a single object. Test-BlastRadius is the one
        collection-returning function on this facade; Get-BlastRadius,
        Get-BlastRadiusFromObservedPaths, and Test-BlastRadiusConflict each
        return a single hashtable.
      - source is restricted to derived, declared, and observed; anything else,
        and any malformed input, throws.
      - The contention relation fails closed: a glob pair that cannot be proven
        disjoint counts as overlapping, because radius under-reporting is the
        dominant risk of the parallel design.
      - Reasons are reported in the fixed kind order path_overlap,
        module_overlap, shared_surface_overlap, contract_dependency, and each
        detail is order-normalized so the relation is observably symmetric in its
        two arguments.
      - Two empty radii, and an empty radius against a non-empty one, do not
        conflict. Under-reporting via emptiness is V1's problem at plan time, not
        the relation's.
    CONVENTION: this module fails fast at module scope and imports its siblings with -ErrorAction Stop.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'BlastRadiusExtraction.psm1') -Force -ErrorAction Stop
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'BlastRadiusGlob.psm1') -Force -ErrorAction Stop
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'BlastRadiusConfig.psm1') -Force -ErrorAction Stop
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'BlastRadiusNormalization.psm1') -Force -ErrorAction Stop
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'BlastRadiusValidation.psm1') -Force -ErrorAction Stop

# Feature-folder handling. Every radius contains its own feature folder, and a
# caller may pass either a bare folder name or an already-qualified path.
$script:FeatureFolderRoot = 'docs/features/active'
$script:FeatureFolderPrefix = 'docs/features/'

# The default confidence source for derivation, and the source a diff-derived
# radius always records.
$script:SourceDerived = 'derived'
$script:SourceObserved = 'observed'

# A contract identifier names something callable or referenceable, so it must
# carry at least one ASCII letter (issue #489). Get-NormalizedDeclaredRadius
# re-applies that rule to identifiers a pre-#489 extractor already recorded.
$script:ContractLetterPattern = [regex]::new('[A-Za-z]')

# Contention reason kinds, in the fixed order every result reports them. These
# strings are contract literals consumed by the downstream parallel schema.
$script:ConflictPathOverlap = 'path_overlap'
$script:ConflictModuleOverlap = 'module_overlap'
$script:ConflictSharedSurfaceOverlap = 'shared_surface_overlap'
$script:ConflictContractDependency = 'contract_dependency'

# Separator used in an overlapping-pair detail string. The pair is ordered
# ordinally before formatting so the detail is identical in both argument orders.
$script:PairDetailSeparator = ' ~ '


# Port of _feature_folder_glob. Accepting an already-qualified path avoids
# producing a doubled docs/features/active/docs/features/active/... entry when a
# caller passes the folder path it already holds.
function Get-FeatureFolderGlob {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $FeatureFolder
    )

    $trimmed = $FeatureFolder.Trim().Trim('/')
    if ($trimmed.StartsWith($script:FeatureFolderPrefix, [System.StringComparison]::Ordinal)) {
        return "$trimmed/**"
    }

    return "$script:FeatureFolderRoot/$trimmed/**"
}

function Get-BlastRadius {
    <#
    .SYNOPSIS
        Derive a blast radius from an approved plan and its feature spec.

    .DESCRIPTION
        Port of derive_blast_radius. Plan task bodies are the primary signal, the
        spec contributes the paths it cites in inline code, and the feature
        folder is always present because every work item writes its own documents
        and evidence. A plan and spec with no extractable paths still yield a
        radius containing the feature-folder glob.

    .PARAMETER PlanText
        Approved atomic-plan document text; may be empty.

    .PARAMETER SpecText
        Feature spec.md document text; may be empty.

    .PARAMETER FeatureFolder
        Bare feature folder name, or a path that already starts with
        docs/features/.

    .PARAMETER Config
        Parsed config/blast-radius.json.

    .PARAMETER Source
        Confidence source to record; derived by default and declared when a
        planner adopts the result as authoritative.

    .PARAMETER ComputedAt
        Caller-supplied ISO-8601 timestamp. The library never reads the clock.

    .OUTPUTS
        System.Collections.Hashtable. The derived radius, carrying exactly the
        keys paths, modules, shared_surfaces, contracts, source, and computed_at.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $PlanText,
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $SpecText,
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $FeatureFolder,
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Config,
        [string] $Source = $script:SourceDerived,
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $ComputedAt
    )

    [void](Get-RequiredText -Value $PlanText -FieldName 'plan_text' -AllowEmpty)
    [void](Get-RequiredText -Value $SpecText -FieldName 'spec_text' -AllowEmpty)

    # Both extraction calls read the separator-free root-surface set from the
    # same -Config value that resolves modules and shared surfaces below. Sharing
    # one reader with Test-BlastRadius is what preserves the invariant that a
    # derived radius always passes V1 and V2 against its own plan (issue #452).
    $rootSurface = [string[]]@(Get-ConfigRootSurface -Config $Config)

    $specLine = [string[]]@(ConvertTo-NormalizedLine -Text $SpecText)
    $entry = [System.Collections.Generic.List[string]]::new()
    $entry.AddRange([string[]]@(Get-PlanPaths -PlanText $PlanText -RootSurface $rootSurface))
    $entry.AddRange([string[]]@(Get-PathFromLine -Line $specLine -RootSurface $rootSurface))

    # Read-by-mandate citations are dropped before the feature folder is added: a
    # plan cites the policy rules because its author was told to read them, not
    # because the change will write them (issue #489). Test-BlastRadius applies
    # the same filter, which is what keeps the derived radius passing V1 and V2
    # against its own plan. The feature-folder glob is added afterwards so it can
    # never be excluded.
    $surviving = [System.Collections.Generic.List[string]]::new()
    $surviving.AddRange([string[]]@(Get-NonMandateReadEntry -Entry $entry.ToArray() `
                -MandateRead ([string[]]@(Get-ConfigMandateRead -Config $Config))))
    $surviving.Add((Get-FeatureFolderGlob -FeatureFolder (
                Get-RequiredText -Value $FeatureFolder -FieldName 'feature_folder')))

    $paths = [string[]]@(Get-OrdinalSortedEntry -Entry $surviving.ToArray())
    $concrete = [string[]]@(Get-ConcreteEntry -Entry $paths)

    return ConvertTo-NormalizedBlastRadius -Radius @{
        paths           = $paths
        modules         = @(Resolve-BlastRadiusModule -PathEntry $paths -Config $Config)
        shared_surfaces = @(Resolve-BlastRadiusSharedSurface -ConcretePath $concrete -Config $Config)
        contracts       = @(Get-ContractIdentifier -SpecText $SpecText)
        source          = $Source
        computed_at     = $ComputedAt
    }
}

function Get-NormalizedDeclaredRadius {
    <#
    .SYNOPSIS
        Re-apply the current extraction rules to an already-recorded radius.

    .DESCRIPTION
        Port of normalize_declared_radius. A radius recorded by an older
        extractor can carry entries the current rules reject: directory-shaped
        tokens, cross-corpus documentation globs, read-by-mandate citations, and
        letterless contract tokens. Re-deriving from the plan text is not always
        possible, so this function re-filters the recorded radius in place of a
        fresh derivation and re-resolves the levels that depend on the surviving
        paths (issue #489). source and computed_at are preserved and the input is
        never mutated.

    .PARAMETER Radius
        A derived or declared radius hashtable to re-filter.

    .PARAMETER Config
        Parsed config/blast-radius.json. The root-surface set, the mandate-read
        list, the module map, and the shared-surface list are all read from this
        one value.

    .OUTPUTS
        System.Collections.Hashtable. A new radius carrying the surviving paths,
        contracts re-filtered by the ASCII-letter rule, and modules and shared
        surfaces re-resolved from the surviving paths. Throws when the radius
        source is observed: an observed radius records a diff listing rather than
        a plan-text harvest, so the plan-text acceptance rules do not apply to it
        and re-filtering one would silently discard genuine evidence.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Radius,
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Config
    )

    $normalized = ConvertTo-NormalizedBlastRadius -Radius $Radius

    # Fail fast, before any other work, so the prohibition is unambiguous and no
    # partially-filtered value can escape.
    if ($normalized['source'] -eq $script:SourceObserved) {
        throw ("Get-NormalizedDeclaredRadius rejects a radius whose source is " +
            "'$($script:SourceObserved)': an observed radius records a diff listing, " +
            'not a plan-text harvest, so the plan-text acceptance rules must not ' +
            'be applied to it.')
    }

    $rootSurface = [string[]]@(Get-ConfigRootSurface -Config $Config)

    # Re-run the classifier over each recorded entry. An entry the current rules
    # reject is dropped; the mandate-read filter then removes the citations that
    # are evidence of a read rather than of a write.
    $accepted = [System.Collections.Generic.List[string]]::new()
    foreach ($entry in @($normalized['paths'])) {
        if ($null -ne (Get-PathTokenKind -Token $entry -RootSurface $rootSurface)) {
            $accepted.Add([string]$entry)
        }
    }

    $paths = [string[]]@(Get-NonMandateReadEntry -Entry $accepted.ToArray() `
            -MandateRead ([string[]]@(Get-ConfigMandateRead -Config $Config)))
    $concrete = [string[]]@(Get-ConcreteEntry -Entry $paths)

    $contract = [System.Collections.Generic.List[string]]::new()
    foreach ($identifier in @($normalized['contracts'])) {
        if ($script:ContractLetterPattern.IsMatch([string]$identifier)) {
            $contract.Add([string]$identifier)
        }
    }

    return ConvertTo-NormalizedBlastRadius -Radius @{
        paths           = $paths
        modules         = @(Resolve-BlastRadiusModule -PathEntry $paths -Config $Config)
        shared_surfaces = @(Resolve-BlastRadiusSharedSurface -ConcretePath $concrete -Config $Config)
        contracts       = @($contract.ToArray())
        source          = $normalized['source']
        computed_at     = $normalized['computed_at']
    }
}

function Get-BlastRadiusFromObservedPaths {
    <#
    .SYNOPSIS
        Build an observed-source radius from an already-collected path list.

    .DESCRIPTION
        Port of radius_from_observed_paths. Drift detection supplies the output
        of a diff listing; the library performs no subprocess call of its own, so
        the paths arrive as plain strings and are taken verbatim rather than
        re-classified by the plan-text heuristic. contracts is empty because a
        diff carries no interface-section text.

    .PARAMETER ObservedPaths
        Repository-relative paths from a diff, as a collection. A bare string is
        rejected, matching the Python reference guard.

    .PARAMETER Config
        Parsed config/blast-radius.json.

    .PARAMETER ComputedAt
        Caller-supplied ISO-8601 timestamp. The library never reads the clock.

    .OUTPUTS
        System.Collections.Hashtable. A radius whose source is observed and whose
        modules and shared surfaces are resolved by the derivation rules.
    #>
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseSingularNouns', '', Justification = 'The exported name is fixed by the spec PowerShell surface contract for issue #447 and mirrors radius_from_observed_paths.')]
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $ObservedPaths,
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Config,
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $ComputedAt
    )

    $paths = [string[]]@(Get-RequiredStringList -Value $ObservedPaths -FieldName 'observed_paths')
    $concrete = [string[]]@(Get-ConcreteEntry -Entry $paths)

    return ConvertTo-NormalizedBlastRadius -Radius @{
        paths           = $paths
        modules         = @(Resolve-BlastRadiusModule -PathEntry $paths -Config $Config)
        shared_surfaces = @(Resolve-BlastRadiusSharedSurface -ConcretePath $concrete -Config $Config)
        contracts       = @()
        source          = $script:SourceObserved
        computed_at     = $ComputedAt
    }
}

# Port of _smallest_path_overlap. Each overlapping pair is ordered before it is
# recorded, so the minimum is taken over a set that does not depend on argument
# order; that is what makes the reported detail symmetric.
function Get-SmallestPathOverlap {
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

# Port of _smallest_common. Two empty collections share nothing, so the result is
# $null and the level contributes no reason.
function Get-SmallestCommonEntry {
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

function Test-BlastRadiusConflict {
    <#
    .SYNOPSIS
        Decide whether two radii contend, and report every triggered disjunct.

    .DESCRIPTION
        Port of conflicts. Evaluates the four disjuncts and returns the verdict
        plus one reason per triggered level in the fixed kind order path_overlap,
        module_overlap, shared_surface_overlap, contract_dependency. The three
        set-intersection levels differ only in which collection they read, so one
        pass over the level table keeps them in the required order without
        repeating the intersection logic.

    .PARAMETER RadiusA
        First radius record.

    .PARAMETER RadiusB
        Second radius record.

    .PARAMETER Config
        Parsed config/blast-radius.json. The relation reads no key from it today;
        it is validated and kept in the signature because the contract is frozen
        for downstream consumers.

    .OUTPUTS
        System.Collections.Hashtable. Keys conflict (a boolean) and reasons (an
        array of hashtables with keys kind and detail).

        Read the verdict from the conflict key of the returned hashtable.
        Do not test the returned object itself: the hashtable is
        unconditionally truthy under PowerShell boolean coercion, so
        'if ($result)' treats every pair as contending.
        System.Collections.Hashtable implements IDictionary and ICollection but
        not IList, and the count-based truthiness rule applies only to IList
        implementations, so a hashtable falls under the rule for any other
        non-collection type and is always $true. The Python port agrees with its
        own verdict; this mirror provably cannot, because PowerShell exposes no
        hook by which a type can decline or change the conversion.
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
        [object] $Config
    )

    [void](Get-RequiredMapping -Value $Config -FieldName 'config')
    $left = ConvertTo-NormalizedBlastRadius -Radius $RadiusA
    $right = ConvertTo-NormalizedBlastRadius -Radius $RadiusB

    $reason = [System.Collections.Generic.List[hashtable]]::new()
    $pathDetail = Get-SmallestPathOverlap -PathA ([string[]]@($left['paths'])) `
        -PathB ([string[]]@($right['paths']))
    if ($null -ne $pathDetail) {
        $reason.Add(@{ kind = $script:ConflictPathOverlap; detail = $pathDetail })
    }

    $level = @(
        @{ kind = $script:ConflictModuleOverlap; key = 'modules' },
        @{ kind = $script:ConflictSharedSurfaceOverlap; key = 'shared_surfaces' },
        @{ kind = $script:ConflictContractDependency; key = 'contracts' }
    )
    foreach ($entry in $level) {
        $shared = Get-SmallestCommonEntry -Left ([string[]]@($left[$entry['key']])) `
            -Right ([string[]]@($right[$entry['key']]))
        if ($null -ne $shared) {
            $reason.Add(@{ kind = $entry['kind']; detail = $shared })
        }
    }

    return @{
        conflict = ($reason.Count -gt 0)
        reasons  = @($reason.ToArray())
    }
}

Export-ModuleMember -Function `
    Get-PlanPaths, `
    Get-BlastRadius, `
    Get-NormalizedDeclaredRadius, `
    Get-BlastRadiusFromObservedPaths, `
    Test-BlastRadius, `
    Test-BlastRadiusConflict
