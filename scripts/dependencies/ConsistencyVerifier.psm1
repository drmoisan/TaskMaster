<#
.SYNOPSIS
    Detects, counts and reports disagreements between a project file and the package
    versions its sibling manifest declares, and drives the repair passes over one project.

.DESCRIPTION
    The detection and reporting layer of the dependency-consistency tooling for issue #911.
    Reconciliation lives in ProjectConsistency.psm1 and the analyzer item repair in
    AnalyzerItemRepair.psm1; this module owns every finding class, every examined count, the
    repairs report and the failure-result type, so no count is ambiguous between two owners.

    Two classes are non-fatal by design and are counted and named rather than failing the
    run: a dependent element whose package is absent from the sibling manifest, and an
    <Analyzer Include> whose preserved Roslyn folder segment is absent from the restored
    listing. No exception is hard-coded for any package identifier. A failure result is
    reserved for a divergence no repair can resolve.

    Exported functions: Find-VersionDisagreement, Find-OrphanedHintPath,
    Test-ReferenceCompleteness, Find-PackageAbsentFromManifest,
    Get-MissingRoslynSegmentFinding, Get-ExaminedElementCount, Get-ConsistencyRepairsReport,
    Get-ConsistencyFailureResult, Invoke-ProjectConsistencyRepair.
#>

Set-StrictMode -Version Latest

# Imported without -Force deliberately. A nested Import-Module -Force removes the module
# from the whole session before re-importing it into this one, so a caller that had already
# imported ProjectConsistency loses it the moment this module loads.
Import-Module (Join-Path $PSScriptRoot 'PackageGraph.psm1')
Import-Module (Join-Path $PSScriptRoot 'ProjectConsistency.psm1')
Import-Module (Join-Path $PSScriptRoot 'AnalyzerItemRepair.psm1')

# The restore-path vocabulary (Get-RestorePackageFolder, Get-FolderPackageIdentity and
# Get-FolderBearingElement) lives in AnalyzerItemRepair.psm1, the module whose subject is
# restore paths, and is imported above.
$script:SeparatorChar = [char[]]@([char]92, [char]47)

# Private. Builds the record every detector in this module returns. ExaminedCount is what
# distinguishes a detector that never fired from a clean population, so every caller
# supplies it; ExaminedAnalyzerCount is separate because AC5 counts that population alone.
function Get-DetectionResult {
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$Finding,
        [Parameter(Mandatory = $true)][int]$ExaminedCount,
        [int]$ExaminedAnalyzerCount = 0
    )

    return [pscustomobject]@{
        PSTypeName            = 'ConsistencyVerifier.DetectionResult'
        Finding               = $Finding
        FindingCount          = @($Finding).Count
        ExaminedCount         = $ExaminedCount
        ExaminedAnalyzerCount = $ExaminedAnalyzerCount
    }
}

function Get-ManifestVersionMap {
    <#
    .SYNOPSIS
        Maps each manifest package identifier to the version it declares.
    .PARAMETER ManifestText
        The sibling packages.config text.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$ManifestText)

    $map = @{}
    if ($ManifestText -notmatch '<packages\b') { return $map }
    foreach ($package in @(ConvertFrom-PackagesConfigText -Text $ManifestText)) {
        $map[$package.Id] = $package.Version
    }
    return $map
}

function Find-VersionDisagreement {
    <#
    .SYNOPSIS
        Detects dependent elements whose package version disagrees with the manifest.
    .DESCRIPTION
        Every element kind carrying a package folder segment is covered, because the #908
        divergence is three-way and an analyzer-only detector reports half of it. Each
        finding carries its kind so the guard and analyzer disagreements stay separable. A
        <Reference> is outside this detector: its Include carries an assembly version, which
        need not track the package version, so a difference is not evidence of anything.
    .PARAMETER ProjectText
        The project file text.
    .PARAMETER ManifestText
        The sibling packages.config text.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$ProjectText,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$ManifestText
    )

    $map = Get-ManifestVersionMap -ManifestText $ManifestText
    $knownId = [string[]]@($map.Keys)
    $element = @(Get-FolderBearingElement -ProjectText $ProjectText)
    $finding = [System.Collections.Generic.List[pscustomobject]]::new()

    foreach ($record in $element) {
        $identity = Get-FolderPackageIdentity -Folder $record.PackageFolder -KnownId $knownId
        if ($null -eq $identity) { continue }
        $expected = [string]$map[$identity.Id]
        if ($identity.Version -eq $expected) { continue }
        $finding.Add([pscustomobject]@{
                PSTypeName = 'ConsistencyVerifier.VersionDisagreement'
                Kind = $record.Kind; LineNumber = $record.LineNumber
                PackageId = $identity.Id; FoundVersion = $identity.Version
                ExpectedVersion = $expected; Value = $record.Value
            })
    }

    return Get-DetectionResult -Finding $finding.ToArray() -ExaminedCount $element.Count `
        -ExaminedAnalyzerCount @($element | Where-Object { $_.Kind -eq 'Analyzer' }).Count
}

function Find-OrphanedHintPath {
    <#
    .SYNOPSIS
        Detects <HintPath> entries whose package folder no manifest entry declares.
    .PARAMETER ProjectText
        The project file text.
    .PARAMETER ManifestText
        The sibling packages.config text.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$ProjectText,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$ManifestText
    )

    $map = Get-ManifestVersionMap -ManifestText $ManifestText
    $declared = @($map.Keys | ForEach-Object { $_ + '.' + [string]$map[$_] })
    $element = @(Get-FolderBearingElement -ProjectText $ProjectText |
            Where-Object { $_.Kind -eq 'HintPath' })
    $finding = [System.Collections.Generic.List[pscustomobject]]::new()

    foreach ($record in $element) {
        if ($declared -contains $record.PackageFolder) { continue }
        $finding.Add([pscustomobject]@{
                PSTypeName = 'ConsistencyVerifier.OrphanedHintPath'
                Kind = 'HintPath'; LineNumber = $record.LineNumber
                PackageFolder = $record.PackageFolder; Value = $record.Value
            })
    }

    return Get-DetectionResult -Finding $finding.ToArray() -ExaminedCount $element.Count
}

function Test-ReferenceCompleteness {
    <#
    .SYNOPSIS
        Asserts a <Reference> with a matching <HintPath> exists for each consumable library
        asset resolved for each manifest package.
    .DESCRIPTION
        The check falsifies the assumption that the NuGet CLI adds references for newly
        introduced assemblies. A missing reference is not repairable here: the repair pass
        moves existing elements into agreement and never synthesises one.
    .PARAMETER ProjectText
        The project file text.
    .PARAMETER ManifestText
        The sibling packages.config text.
    .PARAMETER AssetProvider
        A delegate taking a package identifier and version and returning the library
        assembly file names that package resolves for the target framework.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$ProjectText,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$ManifestText,
        [Parameter(Mandatory = $true)][scriptblock]$AssetProvider
    )

    $hintPath = @(Get-FolderBearingElement -ProjectText $ProjectText |
            Where-Object { $_.Kind -eq 'HintPath' })
    $referenceName = @(ConvertFrom-ProjectFileText -Text $ProjectText |
            Where-Object { $_.Kind -eq 'Reference' } |
                ForEach-Object { ($_.Value -split ',')[0].Trim() })
    $finding = [System.Collections.Generic.List[pscustomobject]]::new()
    $examined = 0

    if ($ManifestText -match '<packages\b') {
        foreach ($package in @(ConvertFrom-PackagesConfigText -Text $ManifestText)) {
            $folder = $package.Id + '.' + $package.Version
            foreach ($asset in @(& $AssetProvider $package.Id $package.Version)) {
                if ([string]::IsNullOrWhiteSpace($asset)) { continue }
                $examined++
                $leaf = @($asset.Split($script:SeparatorChar))[-1]
                $hasHintPath = @($hintPath | Where-Object {
                        $_.PackageFolder -eq $folder -and
                        @($_.Value.Split($script:SeparatorChar))[-1] -eq $leaf
                    }).Count -gt 0
                $hasReference = $referenceName -contains [System.IO.Path]::GetFileNameWithoutExtension($leaf)
                if ($hasHintPath -and $hasReference) { continue }
                $finding.Add([pscustomobject]@{
                        PSTypeName   = 'ConsistencyVerifier.MissingReference'
                        PackageId = $package.Id; PackageVersion = $package.Version
                        AssetFileName = $leaf; HasHintPath = $hasHintPath
                        HasReference = $hasReference
                    })
            }
        }
    }

    return Get-DetectionResult -Finding $finding.ToArray() -ExaminedCount $examined
}

function Find-PackageAbsentFromManifest {
    <#
    .SYNOPSIS
        Detects dependent elements whose package is absent from the sibling manifest.
    .DESCRIPTION
        The class is non-fatal: counted and named in the report, never a failure result.
        QuickFiler.Test carries a live instance in two Exists() guarded <Import> elements
        naming an altcover package no manifest declares. No exception is hard-coded for any
        package identifier.
    .PARAMETER ProjectText
        The project file text.
    .PARAMETER ManifestText
        The sibling packages.config text.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$ProjectText,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$ManifestText
    )

    $knownId = [string[]]@((Get-ManifestVersionMap -ManifestText $ManifestText).Keys)
    $element = @(Get-FolderBearingElement -ProjectText $ProjectText)
    $finding = [System.Collections.Generic.List[pscustomobject]]::new()

    foreach ($record in $element) {
        if ($null -ne (Get-FolderPackageIdentity -Folder $record.PackageFolder -KnownId $knownId)) { continue }
        $finding.Add([pscustomobject]@{
                PSTypeName = 'ConsistencyVerifier.PackageAbsentFromManifest'
                Kind = $record.Kind; LineNumber = $record.LineNumber
                PackageFolder = $record.PackageFolder; Value = $record.Value
            })
    }

    return Get-DetectionResult -Finding $finding.ToArray() -ExaminedCount $element.Count
}

function Get-MissingRoslynSegmentFinding {
    <#
    .SYNOPSIS
        Aggregates and counts the missing-segment records AnalyzerItemRepair.psm1 returns.
    .DESCRIPTION
        The derivation belongs to AnalyzerItemRepair.psm1, which returns a record per
        affected item and builds no report. This function is the single owner of the
        reported class, which is non-fatal.
    .PARAMETER MissingSegmentRecord
        The records returned on an AnalyzerItemRepair result object.
    .PARAMETER ExaminedItemCount
        The analyzer items the repair examined, so a clean aggregation is distinguishable
        from an aggregation over nothing.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$MissingSegmentRecord,
        [int]$ExaminedItemCount = 0
    )

    $finding = @($MissingSegmentRecord | Where-Object { $null -ne $_ })
    return Get-DetectionResult -Finding $finding -ExaminedCount $ExaminedItemCount `
        -ExaminedAnalyzerCount $ExaminedItemCount
}

function Get-ExaminedElementCount {
    <#
    .SYNOPSIS
        Reports how many dependent elements of each kind a project file carries.
    .PARAMETER ProjectText
        The project file text.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$ProjectText)

    $tally = [ordered]@{ Import = 0; Error = 0; Reference = 0; HintPath = 0; Analyzer = 0 }
    if (-not [string]::IsNullOrWhiteSpace($ProjectText)) {
        foreach ($element in @(ConvertFrom-ProjectFileText -Text $ProjectText)) {
            if ($tally.Contains($element.Kind)) { $tally[$element.Kind] = $tally[$element.Kind] + 1 }
        }
    }
    $total = 0
    foreach ($key in @($tally.Keys)) { $total += $tally[$key] }

    return [pscustomobject]@{
        PSTypeName = 'ConsistencyVerifier.ExaminedCount'
        Import = $tally['Import']; Error = $tally['Error']; Reference = $tally['Reference']
        HintPath = $tally['HintPath']; Analyzer = $tally['Analyzer']; Total = $total
    }
}

# Private. Reads one detection result out of a detection map, tolerating absence.
function Get-DetectionFinding {
    [CmdletBinding()]
    [OutputType([object[]])]
    param(
        [Parameter(Mandatory = $true)][hashtable]$Detection,
        [Parameter(Mandatory = $true)][string]$Name
    )

    if (-not $Detection.ContainsKey($Name) -or $null -eq $Detection[$Name]) { return @() }
    return @($Detection[$Name].Finding)
}

function Get-ConsistencyRepairsReport {
    <#
    .SYNOPSIS
        Builds the per-project repairs report consumed by the pull-request body.
    .PARAMETER ProjectName
        The project the report describes.
    .PARAMETER Repair
        The repair records the reconciliation passes produced.
    .PARAMETER Detection
        The detection results keyed by class name. Recognised keys are AbsentFromManifest,
        MissingRoslynSegment, OrphanedHintPath, MissingReference and Examined.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$ProjectName,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$Repair,
        [Parameter(Mandatory = $true)][hashtable]$Detection
    )

    $absent = Get-DetectionFinding -Detection $Detection -Name 'AbsentFromManifest'
    $segment = Get-DetectionFinding -Detection $Detection -Name 'MissingRoslynSegment'

    return [pscustomobject]@{
        PSTypeName                = 'ConsistencyVerifier.RepairsReport'
        ProjectName               = $ProjectName
        Repair                    = @($Repair)
        RepairCount               = @($Repair).Count
        AbsentFromManifest        = $absent
        AbsentFromManifestCount   = @($absent).Count
        MissingRoslynSegment      = $segment
        MissingRoslynSegmentCount = @($segment).Count
        OrphanedHintPath          = (Get-DetectionFinding -Detection $Detection -Name 'OrphanedHintPath')
        MissingReference          = (Get-DetectionFinding -Detection $Detection -Name 'MissingReference')
        Examined                  = $(if ($Detection.ContainsKey('Examined')) { $Detection['Examined'] } else { $null })
    }
}

function Get-ConsistencyFailureResult {
    <#
    .SYNOPSIS
        Builds the failure result returned when a divergence no repair can resolve remains.
    .PARAMETER ProjectName
        The project carrying the residual divergence.
    .PARAMETER Condition
        The specific condition, named rather than described.
    .PARAMETER Detail
        The elements the condition names.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$ProjectName,
        [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$Condition,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$Detail
    )

    return [pscustomobject]@{
        PSTypeName  = 'ConsistencyVerifier.Failure'
        ProjectName = $ProjectName
        Condition   = $Condition
        Detail      = $Detail
    }
}

function Invoke-ProjectConsistencyRepair {
    <#
    .SYNOPSIS
        Runs the repair passes over one project and verifies the post-repair state.
    .DESCRIPTION
        The entry point repairs freely and fails only on residual inconsistency, returning
        either a success result whose report enumerates the repairs performed or a failure
        result naming the condition and project. Analyzer-item regeneration runs only where
        a disagreement is detected for that package in that project.
    .PARAMETER ProjectName
        The project being repaired, used in the report and in any failure result.
    .PARAMETER ProjectText
        The project file text.
    .PARAMETER ManifestText
        The sibling packages.config text.
    .PARAMETER AnalyzerListingProvider
        A delegate taking a package identifier and version and returning the restored
        package directory's contents, relative to that directory.
    .PARAMETER AssetProvider
        A delegate taking a package identifier and version and returning the library
        assembly file names it resolves. Reference completeness is evaluated only when this
        delegate is supplied.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$ProjectName,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$ProjectText,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$ManifestText,
        [scriptblock]$AnalyzerListingProvider = $null,
        [scriptblock]$AssetProvider = $null
    )

    $text = $ProjectText
    $repair = [System.Collections.Generic.List[pscustomobject]]::new()
    $missingSegment = [System.Collections.Generic.List[pscustomobject]]::new()
    $examinedAnalyzerItem = 0

    $staleAnalyzerId = @((Find-VersionDisagreement -ProjectText $text -ManifestText $ManifestText).Finding |
            Where-Object { $_.Kind -eq 'Analyzer' } | ForEach-Object { $_.PackageId })
    $package = if ($ManifestText -match '<packages\b') {
        @(ConvertFrom-PackagesConfigText -Text $ManifestText)
    }
    else { @() }

    foreach ($entry in $package) {
        if ($null -ne $AnalyzerListingProvider -and $staleAnalyzerId -contains $entry.Id) {
            $identifier = $entry.Id
            $version = $entry.Version
            $lister = { & $AnalyzerListingProvider $identifier $version }.GetNewClosure()
            $repaired = Invoke-AnalyzerItemRepair -ProjectName $ProjectName -ProjectText $text `
                -PackageId $entry.Id -ManifestVersion $entry.Version -DirectoryLister $lister
            $text = $repaired.Text
            foreach ($item in @($repaired.RepairedItem)) { $repair.Add($item) }
            foreach ($item in @($repaired.MissingSegmentRecord)) { $missingSegment.Add($item) }
            $examinedAnalyzerItem += $repaired.ExaminedItemCount
        }
        $reconciled = Invoke-VersionReconciliation -ProjectText $text -PackageId $entry.Id `
            -ManifestVersion $entry.Version
        $text = $reconciled.Text
        foreach ($item in @($reconciled.Repair)) { $repair.Add($item) }
    }

    $detection = @{
        Disagreement         = (Find-VersionDisagreement -ProjectText $text -ManifestText $ManifestText)
        OrphanedHintPath     = (Find-OrphanedHintPath -ProjectText $text -ManifestText $ManifestText)
        AbsentFromManifest   = (Find-PackageAbsentFromManifest -ProjectText $text -ManifestText $ManifestText)
        MissingRoslynSegment = (Get-MissingRoslynSegmentFinding -MissingSegmentRecord $missingSegment.ToArray() -ExaminedItemCount $examinedAnalyzerItem)
        Examined             = (Get-ExaminedElementCount -ProjectText $text)
        MissingReference     = $(if ($null -ne $AssetProvider) {
                Test-ReferenceCompleteness -ProjectText $text -ManifestText $ManifestText -AssetProvider $AssetProvider
            }
            else { Get-DetectionResult -Finding @() -ExaminedCount 0 })
    }

    $failure = [System.Collections.Generic.List[pscustomobject]]::new()
    if ($detection['MissingReference'].FindingCount -gt 0) {
        $failure.Add((Get-ConsistencyFailureResult -ProjectName $ProjectName `
                    -Condition 'MissingReference' -Detail $detection['MissingReference'].Finding))
    }

    # A disagreement on an item the repair deliberately left alone is excused: the
    # missing-segment class is non-fatal and emitting a guessed path instead is prohibited.
    $excusedLine = @($missingSegment | ForEach-Object { $_.LineNumber })
    $residual = @($detection['Disagreement'].Finding | Where-Object { $excusedLine -notcontains $_.LineNumber })
    if ($residual.Count -gt 0) {
        $failure.Add((Get-ConsistencyFailureResult -ProjectName $ProjectName `
                    -Condition 'ResidualVersionDisagreement' -Detail $residual))
    }

    return [pscustomobject]@{
        PSTypeName  = 'ConsistencyVerifier.Result'
        ProjectName = $ProjectName
        IsSuccess   = ($failure.Count -eq 0)
        ProjectText = $text
        Report      = (Get-ConsistencyRepairsReport -ProjectName $ProjectName -Repair $repair.ToArray() -Detection $detection)
        Failure     = $failure.ToArray()
    }
}

Export-ModuleMember -Function @(
    'Find-VersionDisagreement',
    'Find-OrphanedHintPath',
    'Test-ReferenceCompleteness',
    'Find-PackageAbsentFromManifest',
    'Get-MissingRoslynSegmentFinding',
    'Get-ExaminedElementCount',
    'Get-ConsistencyRepairsReport',
    'Get-ConsistencyFailureResult',
    'Invoke-ProjectConsistencyRepair'
)
