<#
.SYNOPSIS
    Blast-radius record normalization and validation rules V1, V2, and V3.

.DESCRIPTION
    Destination-runtime PowerShell port of the rule half of
    scripts/dev_tools/_blast_radius_validation.py (validate_blast_radius,
    _coverage_findings, _shared_surface_findings, _over_breadth_findings and the
    RadiusFinding record), plus the construction-time invariants the BlastRadius
    dataclass in scripts/dev_tools/compute_blast_radius.py enforces in
    __post_init__ and from_dict.

    The Python module remains the authoritative reference implementation. This
    module is one half of a two-language mirror; it never imports validator
    logic from outside this library. Every function is pure: no filesystem,
    subprocess, network, or wall-clock access, and no input is mutated.

    Parity notes for maintainers:
      - V1 and V2 are Blocking, V3 Advisory with at most one finding. Findings
        are sorted by rule then subject, at most one per rule per subject.
      - The V2 touched-surface set is the union of the radius's own concrete
        paths and the plan's concrete paths, and enumeration is exact-path
        membership. Glob coverage in either paths or shared_surfaces is
        deliberately insufficient, so a surface reachable only through a wildcard
        still produces a finding. That is the fail-closed reading.
      - V3 applies the threshold by multiplication rather than division so the
        boundary is exact: a radius sitting exactly at the fraction does not
        trigger, and both languages compute the identical IEEE-754 comparison.
      - tracked_file_count must be an integer, and a boolean is rejected
        explicitly because the Python reference rejects it.
      - Finding message text is a contract literal shared with the Python
        reference and the cross-language fixture corpus; do not reword it.
    CONVENTION: this module fails fast at module scope and imports its siblings with -ErrorAction Stop.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'BlastRadiusExtraction.psm1') -Force -ErrorAction Stop
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'BlastRadiusGlob.psm1') -Force -ErrorAction Stop
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'BlastRadiusConfig.psm1') -Force -ErrorAction Stop
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'BlastRadiusNormalization.psm1') -Force -ErrorAction Stop

# Finding vocabulary. These strings are contract literals consumed by the
# downstream parallel schema and planner features.
$script:RuleCoverage = 'V1'
$script:RuleSharedSurface = 'V2'
$script:RuleOverBreadth = 'V3'
$script:SeverityBlocking = 'Blocking'
$script:SeverityAdvisory = 'Advisory'
$script:FindingRule = @('V1', 'V2', 'V3')
$script:FindingSeverity = @('Blocking', 'Advisory')

# The single subject the over-breadth rule reports against; the rule is about the
# whole paths level rather than any one entry.
$script:OverBreadthSubject = 'blast_radius.paths'

# Integer types a caller may supply for the tracked-file count. Boolean is
# excluded explicitly because Python treats it as an integer.
$script:IntegerTypeName = @('System.Int32', 'System.Int64', 'System.Int16', 'System.Byte')

# Serialized key set, in the order of the parallel manifest schema. Downstream
# features depend on these strings verbatim.
$script:RadiusKey = @('paths', 'modules', 'shared_surfaces', 'contracts', 'source', 'computed_at')

# Confidence sources: derived seeds cohorts provisionally, declared is
# planner-computed and authoritative for scheduling, and observed comes from an
# actual diff during drift correction.
$script:RadiusSource = @('derived', 'declared', 'observed')


function ConvertTo-NormalizedBlastRadius {
    <#
    .SYNOPSIS
        Validate a blast-radius record and normalize every collection it carries.

    .DESCRIPTION
        Port of the BlastRadius dataclass construction path: the exact key-set
        check from from_dict plus the field guards, source-vocabulary check, and
        sorted, deduplicated collections from __post_init__. An exact key-set
        check is deliberate: a missing key would silently narrow a radius and an
        unexpected key would silently drop data, and both failure modes
        under-report contention. PowerShell hashtable keys are matched case
        insensitively, which is the one place this port is more permissive than
        Python; a case-variant key is malformed input in both languages.

    .PARAMETER Radius
        A radius record carrying exactly the keys paths, modules,
        shared_surfaces, contracts, source, and computed_at.

    .OUTPUTS
        System.Collections.Hashtable. A new record with the same key set, every
        collection deduplicated and ordinally sorted.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Radius
    )

    $mapping = Get-RequiredMapping -Value $Radius -FieldName 'blast radius'

    $missing = @($script:RadiusKey | Where-Object { -not $mapping.ContainsKey($_) })
    if ($missing.Count -gt 0) {
        throw "blast radius record is missing keys $($missing -join ', ')."
    }
    $unexpected = [string[]]@(Get-OrdinalSortedEntry -Entry ([string[]]@(
                $mapping.Keys | Where-Object { $script:RadiusKey -notcontains $_ })))
    if ($unexpected.Count -gt 0) {
        throw "blast radius record has unexpected keys $($unexpected -join ', ')."
    }

    $source = Get-RequiredText -Value $mapping['source'] -FieldName 'source'
    if ($script:RadiusSource -cnotcontains $source) {
        throw "source must be one of $($script:RadiusSource -join ', ')."
    }

    return @{
        paths           = @(Get-RequiredStringList -Value $mapping['paths'] -FieldName 'paths')
        modules         = @(Get-RequiredStringList -Value $mapping['modules'] -FieldName 'modules')
        shared_surfaces = @(Get-RequiredStringList -Value $mapping['shared_surfaces'] -FieldName 'shared_surfaces')
        contracts       = @(Get-RequiredStringList -Value $mapping['contracts'] -FieldName 'contracts')
        source          = $source
        computed_at     = Get-RequiredText -Value $mapping['computed_at'] -FieldName 'computed_at'
    }
}


# Port of the RadiusFinding dataclass construction path: reject any finding
# outside the frozen rule and severity vocabulary, then return the record.
function Get-RadiusFinding {
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $Rule,
        [Parameter(Mandatory = $true)]
        [string] $Severity,
        [Parameter(Mandatory = $true)]
        [string] $Subject,
        [Parameter(Mandatory = $true)]
        [string] $Message
    )

    if ($script:FindingRule -cnotcontains $Rule) {
        throw "RadiusFinding rule must be one of $($script:FindingRule -join ', ')."
    }
    if ($script:FindingSeverity -cnotcontains $Severity) {
        throw "RadiusFinding severity must be one of $($script:FindingSeverity -join ', ')."
    }

    return @{
        rule     = Get-RequiredText -Value $Rule -FieldName 'RadiusFinding.rule'
        severity = Get-RequiredText -Value $Severity -FieldName 'RadiusFinding.severity'
        subject  = Get-RequiredText -Value $Subject -FieldName 'RadiusFinding.subject'
        message  = Get-RequiredText -Value $Message -FieldName 'RadiusFinding.message'
    }
}

# Port of _coverage_findings. Coverage is subsumption, not equality: an exact
# entry, a listed directory, or a glob in the radius all cover a plan path.
function Get-CoverageFinding {
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [hashtable] $Radius,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [AllowEmptyString()]
        [string[]] $PlanConcretePath
    )

    $finding = [System.Collections.Generic.List[hashtable]]::new()
    foreach ($path in $PlanConcretePath) {
        if (-not (Test-PathSubsumed -Path $path -CoveringPath ([string[]]@($Radius['paths'])))) {
            $finding.Add((Get-RadiusFinding -Rule $script:RuleCoverage `
                        -Severity $script:SeverityBlocking `
                        -Subject $path `
                        -Message "Plan path $path is not subsumed by blast_radius.paths."))
        }
    }

    return @($finding.ToArray())
}

# Port of _shared_surface_findings. The touched-surface source is the union of
# the radius's own concrete paths and the plan's concrete paths, so a radius that
# covers a surface only by glob is still caught.
function Get-SharedSurfaceFinding {
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [hashtable] $Radius,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [AllowEmptyString()]
        [string[]] $PlanConcretePath,
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Config
    )

    $touchedSource = [System.Collections.Generic.List[string]]::new()
    $touchedSource.AddRange([string[]]@(Get-ConcreteEntry -Entry ([string[]]@($Radius['paths']))))
    $touchedSource.AddRange($PlanConcretePath)

    $declared = [System.Collections.Generic.HashSet[string]]::new(
        [string[]]@($Radius['shared_surfaces']), [StringComparer]::Ordinal)

    $finding = [System.Collections.Generic.List[hashtable]]::new()
    foreach ($surface in @(Resolve-BlastRadiusSharedSurface -ConcretePath $touchedSource.ToArray() -Config $Config)) {
        if (-not $declared.Contains($surface)) {
            $finding.Add((Get-RadiusFinding -Rule $script:RuleSharedSurface `
                        -Severity $script:SeverityBlocking `
                        -Subject $surface `
                        -Message ("Shared surface $surface is touched but is not " +
                        'enumerated in blast_radius.shared_surfaces.')))
        }
    }

    return @($finding.ToArray())
}

# Port of _over_breadth_findings. An over-broad radius is safe but serializes the
# batch, so the rule only reports and emits at most one Advisory finding.
function Get-OverBreadthFinding {
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [hashtable] $Radius,
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Config,
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $TrackedFileCount
    )

    if ($TrackedFileCount -is [bool] -or $null -eq $TrackedFileCount -or
        $script:IntegerTypeName -notcontains $TrackedFileCount.GetType().FullName) {
        throw 'tracked_file_count must be an integer.'
    }
    $count = [long]$TrackedFileCount
    if ($count -le 0) {
        throw 'tracked_file_count must be a positive integer.'
    }

    $threshold = Get-ConfigOverBreadthFraction -Config $Config
    $covered = @(Get-ConcreteEntry -Entry ([string[]]@($Radius['paths']))).Count
    if ($covered -le ($threshold * $count)) {
        return @()
    }

    return @(
        Get-RadiusFinding -Rule $script:RuleOverBreadth `
            -Severity $script:SeverityAdvisory `
            -Subject $script:OverBreadthSubject `
            -Message ("Radius covers $covered of $count tracked files, " +
            'which exceeds the configured over-breadth fraction.')
    )
}

# Port of the findings.sort(key=(rule, subject)) call. A stable insertion sort is
# used because Python's sort is stable and because ordinal comparison must not be
# delegated to the culture-sensitive Sort-Object cmdlet.
function Get-SortedRadiusFinding {
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [hashtable[]] $Finding
    )

    $sorted = [System.Collections.Generic.List[hashtable]]::new()
    foreach ($candidate in $Finding) {
        $position = $sorted.Count
        while ($position -gt 0) {
            $previous = $sorted[$position - 1]
            $ruleOrder = [string]::CompareOrdinal([string]$previous['rule'], [string]$candidate['rule'])
            $order = if ($ruleOrder -ne 0) {
                $ruleOrder
            } else {
                [string]::CompareOrdinal([string]$previous['subject'], [string]$candidate['subject'])
            }
            if ($order -le 0) {
                break
            }
            $position -= 1
        }
        $sorted.Insert($position, $candidate)
    }

    return @($sorted.ToArray())
}

function Test-BlastRadius {
    <#
    .SYNOPSIS
        Apply validation rules V1, V2, and V3 to a radius against its plan.

    .DESCRIPTION
        Port of validate_blast_radius. The radius is normalized on entry, which
        reproduces the sorted, deduplicated invariant the Python BlastRadius
        dataclass guarantees by construction and makes the result independent of
        the order the caller happened to serialize its collections in.

    .PARAMETER Radius
        Radius record under validation, carrying exactly the keys paths,
        modules, shared_surfaces, contracts, source, and computed_at.

    .PARAMETER PlanText
        Approved atomic-plan text the radius claims to cover; may be empty.

    .PARAMETER Config
        Parsed config/blast-radius.json.

    .PARAMETER TrackedFileCount
        Files tracked in the repository, a caller input so the library performs
        no subprocess call. Must be a positive integer.

    .OUTPUTS
        System.Object[]. Finding hashtables with keys rule, severity, subject,
        and message, sorted by rule then subject. An empty array means the radius
        is valid.
    #>
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Radius,
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $PlanText,
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Config,
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $TrackedFileCount
    )

    $normalized = ConvertTo-NormalizedBlastRadius -Radius $Radius
    [void](Get-RequiredText -Value $PlanText -FieldName 'plan_text' -AllowEmpty)
    # The root-surface set comes from the same -Config value that V1 and V2 use
    # below to resolve modules and shared surfaces, and from the same reader
    # Get-BlastRadius calls. That shared source is what keeps a derived radius
    # passing V1 and V2 against its own plan (issue #452). The mandate-read
    # exclusion is applied here for the same reason: the derivation harvest drops
    # those citations, so V1 and V2 must not then demand that the radius cover
    # them (issue #489).
    $planPath = [string[]]@(Get-NonMandateReadEntry -MandateRead (
            [string[]]@(Get-ConfigMandateRead -Config $Config)) -Entry (
            [string[]]@(Get-PlanPaths -PlanText $PlanText `
                    -RootSurface ([string[]]@(Get-ConfigRootSurface -Config $Config)))))
    $planConcrete = [string[]]@(Get-ConcreteEntry -Entry $planPath)

    $finding = [System.Collections.Generic.List[hashtable]]::new()
    $finding.AddRange([hashtable[]]@(Get-CoverageFinding -Radius $normalized -PlanConcretePath $planConcrete))
    $finding.AddRange([hashtable[]]@(
            Get-SharedSurfaceFinding -Radius $normalized -PlanConcretePath $planConcrete -Config $Config))
    $finding.AddRange([hashtable[]]@(
            Get-OverBreadthFinding -Radius $normalized -Config $Config -TrackedFileCount $TrackedFileCount))

    return @(Get-SortedRadiusFinding -Finding $finding.ToArray())
}

Export-ModuleMember -Function ConvertTo-NormalizedBlastRadius, Test-BlastRadius
