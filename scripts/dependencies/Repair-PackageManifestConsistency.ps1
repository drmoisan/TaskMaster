<#
.SYNOPSIS
    Repairs a repository so that every dependent element in a project file, and every binding
    redirect in an application configuration, agrees with the sibling manifest.

.DESCRIPTION
    The composition root and command-line entry point of the dependency-consistency tooling for
    issue #911. This script owns every filesystem dependency: the five modules it wires are pure
    over text and reach the disk only through the delegates supplied here, which is what lets the
    whole pipeline be driven over an in-memory fixture with no temporary file. The passes run in
    the order in which each consumes the previous one's output: asset-level compatibility gate over
    the candidate upgrades; version reconciliation; analyzer-item regeneration; binding-redirect
    reconciliation; normalisation; verification. An incompatible package is skipped with a recorded
    reason and the remaining upgrades proceed, so a skip is never a run failure.

    Two resolution rules confirm rather than select, because this script can observe what a
    restored package ships but cannot reproduce the assembly MSBuild will bind. A Reference
    assembly version that some assembly in the restored package already carries is preserved;
    only a version the package carries nowhere is rewritten, and then to the version in the
    asset folder the compatibility module selects. An analyzer item's folder segment is
    likewise preserved, the restored listing being enumerated solely to confirm the item still
    resolves; an item that no longer resolves is reported rather than guessed at. Binding
    redirects are reconciled for the packages this run upgraded, a redirect disagreeing with a
    package the run did not upgrade being pre-existing drift outside this pass's remit.

.PARAMETER RepositoryRoot
    The repository to repair. Defaults to the root two levels above this script.
.PARAMETER CandidateUpgrade
    Package identifier to target version, normally the upgrades the bot proposed. Empty by
    default, which makes the run a reconciliation and verification pass over the tree as it is.
.PARAMETER DirectoryLister
    A delegate returning candidate file paths. Defaults to a pruned walk of the repository.
.PARAMETER TextReader
    A delegate taking a path and returning that file's text.
.PARAMETER TextWriter
    A delegate taking a path and the replacement text.
.PARAMETER AssetFolderProvider
    A delegate taking an identifier and version, returning the asset folder names offered.
.PARAMETER AssemblyIdentityProvider
    A delegate taking an identifier and version, returning AssetFolder and Version records.
.PARAMETER AnalyzerListingProvider
    A delegate taking an identifier and version, returning the package's relative contents.

.OUTPUTS
    A Repair.Result record carrying the per-project reports, the skip records, the aggregate
    counts and the Body text the pull-request disclosure consumes.

.EXAMPLE
    .\Repair-PackageManifestConsistency.ps1 -WhatIf
    Reports what the repair passes would change, writing nothing.
#>
[CmdletBinding(SupportsShouldProcess = $true)]
[OutputType([pscustomobject])]
param(
    [ValidateNotNullOrEmpty()][string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $PSScriptRoot)),
    [ValidateNotNull()][hashtable]$CandidateUpgrade = @{},
    [scriptblock]$DirectoryLister = $null,
    [scriptblock]$TextReader = $null,
    [scriptblock]$TextWriter = $null,
    [scriptblock]$AssetFolderProvider = $null,
    [scriptblock]$AssemblyIdentityProvider = $null,
    [scriptblock]$AnalyzerListingProvider = $null
)

Set-StrictMode -Version Latest

Import-Module (Join-Path $PSScriptRoot 'PackageGraph.psm1')
Import-Module (Join-Path $PSScriptRoot 'PackageCompatibility.psm1')
Import-Module (Join-Path $PSScriptRoot 'AnalyzerItemRepair.psm1')
Import-Module (Join-Path $PSScriptRoot 'ProjectConsistency.psm1')
Import-Module (Join-Path $PSScriptRoot 'ConsistencyVerifier.psm1')

# Separators are built from their character codes rather than written as escaped literals: a
# doubled backslash can be collapsed in transit and leave a pattern that matches nothing.
$script:SeparatorChar = [char[]]@([char]92, [char]47)
$script:DirectorySeparator = [string][char]92
$script:PackageMarker = 'packages' + $script:DirectorySeparator
$script:PrunedDirectory = @('packages', 'bin', 'obj', 'node_modules', '.git', '.vs',
    'coverage', 'TestResults')
$script:ProjectExtension = '.csproj'
$script:IdentityCache = @{}

# The default filesystem delegates. Each is defined at script scope and reads the locals below
# from that scope when a module invokes it. Only an assembly named for its package is reported
# by the identity delegate, that being the only one a Reference named for the package resolves.
$script:DefaultFileLister = {
    $directory = @(Get-ChildItem -LiteralPath $script:Root -Directory |
            Where-Object { $script:PrunedDirectory -notcontains $_.Name })
    $file = @(@($directory) + @(Get-Item -LiteralPath $script:Root) |
            ForEach-Object { Get-ChildItem -LiteralPath $_.FullName -File } |
                Where-Object {
                    $_.Name -eq 'packages.config' -or $_.Name -eq 'app.config' -or
                    $_.Extension -eq $script:ProjectExtension
                } | ForEach-Object { $_.FullName })
    # Discovery reaches the root and its immediate subdirectories only, so a project nested
    # deeper is skipped. This record does not prevent that; it makes the shortfall observable
    # in the run log, which is decision D4 for finding R9c. Widening the walk would change
    # which manifests the production pass discovers and no test covers that change.
    Write-Verbose ('Manifest discovery: enumerated directories {0}, returned files {1}' -f ($directory.Count + 1), $file.Count)
    return $file
}

$script:DefaultAssetFolder = {
    param([string]$PackageId, [string]$PackageVersion)
    $lib = Join-Path (Join-Path $script:RestoreRoot ($PackageId + '.' + $PackageVersion)) 'lib'
    if (-not (Test-Path -LiteralPath $lib)) { return @() }
    return @(Get-ChildItem -LiteralPath $lib -Directory | ForEach-Object { $_.Name })
}

# Restricted to the library directory, where an assembly a Reference resolves lives, and memoised:
# one package version's identity is asked for once per project that declares it.
$script:DefaultAssemblyIdentity = {
    param([string]$PackageId, [string]$PackageVersion)
    $key = $PackageId + '|' + $PackageVersion
    if ($script:IdentityCache.ContainsKey($key)) { return $script:IdentityCache[$key] }
    $lib = Join-Path (Join-Path $script:RestoreRoot ($PackageId + '.' + $PackageVersion)) 'lib'
    $identity = @()
    if (Test-Path -LiteralPath $lib) {
        $identity = @(Get-ChildItem -LiteralPath $lib -Recurse -File -Filter ($PackageId + '.dll') |
                ForEach-Object {
                    try { $name = [System.Reflection.AssemblyName]::GetAssemblyName($_.FullName) }
                    catch { Write-Verbose "Unreadable assembly: $($_.Exception.Message)"; return }
                    [pscustomobject]@{ AssetFolder = $_.Directory.Name; Version = $name.Version.ToString() }
                })
    }
    $script:IdentityCache[$key] = $identity
    return $identity
}

$script:DefaultAnalyzerListing = {
    param([string]$PackageId, [string]$PackageVersion)
    $directory = Join-Path $script:RestoreRoot ($PackageId + '.' + $PackageVersion)
    if (-not (Test-Path -LiteralPath $directory)) { return @() }
    $prefixLength = $directory.Length
    return @(Get-ChildItem -LiteralPath $directory -Recurse -File |
            ForEach-Object { $_.FullName.Substring($prefixLength).TrimStart($script:SeparatorChar) })
}

# Reduces a restore path to the part from the restore directory onwards, lower-cased and with
# separators normalised, so two spellings of one path compare equal.
$script:RestoreTail = {
    param([string]$Value)
    $normalised = $Value.Replace([string][char]47, $script:DirectorySeparator)
    $at = $normalised.IndexOf($script:PackageMarker, [System.StringComparison]::OrdinalIgnoreCase)
    if ($at -lt 0) { return '' }
    return $normalised.Substring($at + $script:PackageMarker.Length).ToLowerInvariant()
}

function Invoke-CandidateUpgrade {
    <#
    .SYNOPSIS
        Applies each candidate upgrade whose package ships a consumable asset and skips the
        rest, carrying the reason the compatibility gate gave on every skip record.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$ManifestText,
        [Parameter(Mandatory = $true)][ValidateNotNull()][hashtable]$Candidate,
        [AllowNull()][scriptblock]$AssetProvider = $null
    )

    $applied = [System.Collections.Generic.List[pscustomobject]]::new()
    $skipped = [System.Collections.Generic.List[pscustomobject]]::new()
    $text = $ManifestText

    if ($Candidate.Count -gt 0 -and $ManifestText -match '<packages\b') {
        $package = @(ConvertFrom-PackagesConfigText -Text $ManifestText)
        foreach ($record in $package) {
            if (-not $Candidate.ContainsKey($record.Id)) { continue }
            $target = [string]$Candidate[$record.Id]
            if ([string]::IsNullOrWhiteSpace($target) -or $target -eq $record.Version) { continue }

            $offered = if ($null -ne $AssetProvider) { @(& $AssetProvider $record.Id $target) } else { @() }
            $decision = Test-PackageAssetCompatibility -PackageId $record.Id -AssetFolder ([string[]]$offered)
            if (-not $decision.IsCompatible) {
                $skipped.Add([pscustomobject]@{
                        PSTypeName = 'Repair.SkippedPackage'
                        PackageId = $record.Id; FromVersion = $record.Version
                        ToVersion = $target; Reason = $decision.Reason
                    })
                continue
            }

            $applied.Add([pscustomobject]@{
                    PSTypeName = 'Repair.AppliedUpgrade'
                    PackageId = $record.Id; FromVersion = $record.Version
                    ToVersion = $target; AssetFolder = $decision.SelectedAssetFolder
                })
            $record.Attribute['version'] = $target
            $record.Version = $target
        }
        if ($applied.Count -gt 0) { $text = ConvertTo-PackagesConfigText -Package $package }
    }

    return [pscustomobject]@{ Text = $text; Applied = $applied.ToArray(); Skipped = $skipped.ToArray() }
}

function Invoke-ProjectFileRepair {
    <#
    .SYNOPSIS
        Runs analyzer-item regeneration and version reconciliation over one project file,
        returning the repaired text, the repair records and any unrepairable analyzer item.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$ProjectName,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$ProjectText,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$ManifestText,
        [AllowNull()][scriptblock]$ListingProvider = $null,
        [AllowNull()][scriptblock]$IdentityProvider = $null
    )

    $text = $ProjectText
    $repair = [System.Collections.Generic.List[pscustomobject]]::new()
    $missing = [System.Collections.Generic.List[pscustomobject]]::new()

    if ($ManifestText -match '<packages\b') {
        $stale = @((Find-VersionDisagreement -ProjectText $text -ManifestText $ManifestText).Finding |
                Where-Object { $_.Kind -eq 'Analyzer' } | ForEach-Object { $_.PackageId })

        foreach ($record in @(ConvertFrom-PackagesConfigText -Text $ManifestText)) {
            if ($null -ne $ListingProvider -and $stale -contains $record.Id) {
                $identifier = $record.Id
                $version = $record.Version
                $lister = { & $ListingProvider $identifier $version }.GetNewClosure()
                $repaired = Invoke-AnalyzerItemRepair -ProjectName $ProjectName -ProjectText $text `
                    -PackageId $record.Id -ManifestVersion $record.Version -DirectoryLister $lister
                $text = $repaired.Text
                foreach ($item in @($repaired.RepairedItem)) { $repair.Add($item) }
                foreach ($item in @($repaired.MissingSegmentRecord)) { $missing.Add($item) }
            }

            $assemblyVersion = Resolve-ReferenceAssemblyVersion -PackageId $record.Id `
                -PackageVersion $record.Version -ProjectText $text -IdentityProvider $IdentityProvider
            $reconciled = Invoke-VersionReconciliation -ProjectText $text -PackageId $record.Id `
                -ManifestVersion $record.Version -AssemblyVersion $assemblyVersion
            $text = $reconciled.Text
            foreach ($item in @($reconciled.Repair)) { $repair.Add($item) }
        }
    }

    return [pscustomobject]@{
        Text = $text; Repair = $repair.ToArray(); MissingSegment = $missing.ToArray()
    }
}

function Get-ProjectVerification {
    <#
    .SYNOPSIS
        Confirms every analyzer item still resolves in its own package's restored listing, then
        builds the per-project repairs report from the post-repair text.
    .DESCRIPTION
        The confirmation is the measurement behind the missing-segment class: each item is
        compared against the path set the preserve rule derives, so a clean class is reported
        alongside the population that produced it. An orphaned HintPath is reported by its own
        class, so the absent-from-manifest class covers the remaining element kinds: an absent
        HintPath is necessarily orphaned too, and counting it twice would inflate both figures.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$ProjectName,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$ProjectText,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$ManifestText,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$Repair,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][object[]]$MissingSegment,
        [AllowNull()][scriptblock]$ListingProvider = $null,
        [ValidateNotNull()][hashtable]$ListingCache = @{}
    )

    $unresolved = [System.Collections.Generic.List[pscustomobject]]::new()
    $item = @(Get-FolderBearingElement -ProjectText $ProjectText | Where-Object { $_.Kind -eq 'Analyzer' })
    $knownId = [string[]]@()
    if ($ManifestText -match '<packages\b') {
        $knownId = [string[]]@(@(ConvertFrom-PackagesConfigText -Text $ManifestText) | ForEach-Object { $_.Id })
    }

    foreach ($element in $item) {
        if ($null -eq $ListingProvider) { break }
        $identity = Get-FolderPackageIdentity -Folder $element.PackageFolder -KnownId $knownId
        if ($null -eq $identity) { continue }

        $key = $identity.Id + '|' + $identity.Version
        if (-not $ListingCache.ContainsKey($key)) {
            $identifier = $identity.Id
            $version = $identity.Version
            $lister = { & $ListingProvider $identifier $version }.GetNewClosure()
            # Called without -PreservedSegment, so the result is a verification membership set: every consumable analyzer assembly in every Roslyn folder the package ships, consumed only by the -contains test below and never written to a project file.
            # A caller wanting a writable path supplies -PreservedSegment, which confines the result to the folder the item already names.
            $ListingCache[$key] = @(Get-AnalyzerAssemblyPath -PackageId $identity.Id `
                    -PackageVersion $identity.Version -DirectoryLister $lister |
                    ForEach-Object { & $script:RestoreTail $_ })
        }
        if (@($ListingCache[$key]) -contains (& $script:RestoreTail $element.Value)) { continue }
        $unresolved.Add([pscustomobject]@{
                PSTypeName = 'Repair.UnresolvedAnalyzerItem'
                ProjectName = $ProjectName; LineNumber = $element.LineNumber; Item = $element.Value
            })
    }

    $disagreement = Find-VersionDisagreement -ProjectText $ProjectText -ManifestText $ManifestText
    $absent = @((Find-PackageAbsentFromManifest -ProjectText $ProjectText -ManifestText $ManifestText).Finding |
            Where-Object { $_.Kind -ne 'HintPath' })
    $segment = @(@($MissingSegment) + @($unresolved.ToArray()) | Where-Object { $null -ne $_ })
    $detection = @{
        OrphanedHintPath     = (Find-OrphanedHintPath -ProjectText $ProjectText -ManifestText $ManifestText)
        AbsentFromManifest   = [pscustomobject]@{ Finding = $absent }
        MissingRoslynSegment = (Get-MissingRoslynSegmentFinding -MissingSegmentRecord $segment `
                -ExaminedItemCount $item.Count)
        Examined             = (Get-ExaminedElementCount -ProjectText $ProjectText)
    }

    # A disagreement on an item the repair deliberately left alone is excused: the missing-segment
    # class is non-fatal and emitting a guessed path in its place is prohibited.
    $excused = @($segment | ForEach-Object { $_.LineNumber })
    $residual = @($disagreement.Finding | Where-Object { $excused -notcontains $_.LineNumber })
    $failure = @(if ($residual.Count -gt 0) {
            Get-ConsistencyFailureResult -ProjectName $ProjectName `
                -Condition 'ResidualVersionDisagreement' -Detail $residual
        })

    return [pscustomobject]@{
        PSTypeName            = 'Repair.ProjectVerification'
        ProjectName           = $ProjectName
        Report                = (Get-ConsistencyRepairsReport -ProjectName $ProjectName -Repair $Repair -Detection $detection)
        ExaminedAnalyzerCount = $item.Count
        DisagreementCount     = $disagreement.FindingCount
        Failure               = $failure
    }
}

# Renders the disclosure blocks the pull-request body consumes. The skipped block is present
# only when a skip was recorded, so a body carrying it is evidence of a skip, not boilerplate.
$script:ReportBody = {
    param([object[]]$Verification, [object[]]$Skipped)

    $line = [System.Collections.Generic.List[string]]::new()
    $line.Add('## Repairs applied')
    $repaired = @($Verification | Where-Object { $_.Report.RepairCount -gt 0 })
    if ($repaired.Count -eq 0) { $line.Add('No repairs were applied.') }
    foreach ($project in $repaired) {
        $line.Add('- ' + $project.ProjectName + ': ' + $project.Report.RepairCount + ' repair(s)')
        foreach ($record in @($project.Report.Repair)) {
            $line.Add('  - ' + $record.Kind + ' ' + $record.PackageId + ' line ' + $record.LineNumber)
        }
    }

    if (@($Skipped).Count -gt 0) {
        $line.Add('')
        $line.Add('## Packages skipped')
        foreach ($record in @($Skipped)) {
            $line.Add('- ' + $record.PackageId + ' ' + $record.FromVersion + ' to ' +
                $record.ToVersion + ': ' + $record.Reason)
        }
    }

    return ($line -join [System.Environment]::NewLine)
}

$script:Root = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$script:RestoreRoot = Join-Path $script:Root 'packages'
$lister = if ($null -ne $DirectoryLister) { $DirectoryLister } else { $script:DefaultFileLister }
$reader = if ($null -ne $TextReader) { $TextReader } else { { param($Path) [System.IO.File]::ReadAllText($Path) } }
$writer = if ($null -ne $TextWriter) { $TextWriter } else { { param($Path, $Text) [System.IO.File]::WriteAllText($Path, $Text) } }
$assetProvider = if ($null -ne $AssetFolderProvider) { $AssetFolderProvider } else { $script:DefaultAssetFolder }
$identityProvider = if ($null -ne $AssemblyIdentityProvider) { $AssemblyIdentityProvider } else { $script:DefaultAssemblyIdentity }
$listingProvider = if ($null -ne $AnalyzerListingProvider) { $AnalyzerListingProvider } else { $script:DefaultAnalyzerListing }

$candidatePath = @(& $lister)
$appConfigPath = @(Get-PackageManifestPath -Kind 'AppConfig' -DirectoryLister $lister)
$listingCache = @{}
$verification = [System.Collections.Generic.List[pscustomobject]]::new()
$skipped = [System.Collections.Generic.List[pscustomobject]]::new()
$upgraded = [System.Collections.Generic.List[pscustomobject]]::new()
$written = [System.Collections.Generic.List[string]]::new()

foreach ($manifest in @(Get-PackageManifestPath -Kind 'PackagesConfig' -DirectoryLister $lister)) {
    $directory = Split-Path -Parent $manifest
    $manifestText = [string](& $reader $manifest)

    $upgrade = Invoke-CandidateUpgrade -ManifestText $manifestText -Candidate $CandidateUpgrade `
        -AssetProvider $assetProvider
    foreach ($record in @($upgrade.Skipped)) { $skipped.Add($record) }
    foreach ($record in @($upgrade.Applied)) { $upgraded.Add($record) }
    if ($upgrade.Text -cne $manifestText) {
        if ($PSCmdlet.ShouldProcess($manifest, 'Apply candidate upgrades')) {
            & $writer $manifest $upgrade.Text
            $written.Add($manifest)
        }
        $manifestText = $upgrade.Text
    }

    $project = @($candidatePath | Where-Object {
            (Split-Path -Parent $_) -eq $directory -and
            [System.IO.Path]::GetExtension($_) -eq $script:ProjectExtension
        })
    if ($project.Count -eq 0) {
        Write-Verbose "No project file sits beside '$manifest'; the manifest was repaired alone."
        continue
    }

    $projectName = [System.IO.Path]::GetFileName($project[0])
    $projectText = [string](& $reader $project[0])
    $repaired = Invoke-ProjectFileRepair -ProjectName $projectName -ProjectText $projectText `
        -ManifestText $manifestText -ListingProvider $listingProvider -IdentityProvider $identityProvider
    if ($repaired.Text -cne $projectText -and
        $PSCmdlet.ShouldProcess($project[0], 'Reconcile dependent elements')) {
        & $writer $project[0] $repaired.Text
        $written.Add($project[0])
    }

    $verification.Add((Get-ProjectVerification -ProjectName $projectName -ProjectText $repaired.Text `
                -ManifestText $manifestText -Repair @($repaired.Repair) `
                -MissingSegment @($repaired.MissingSegment) -ListingProvider $listingProvider `
                -ListingCache $listingCache))

    $appConfig = @($appConfigPath | Where-Object { (Split-Path -Parent $_) -eq $directory })
    if ($appConfig.Count -eq 0 -or @($upgrade.Applied).Count -eq 0) { continue }

    $appConfigText = [string](& $reader $appConfig[0])
    $redirectText = $appConfigText
    foreach ($record in @($upgrade.Applied)) {
        $assemblyVersion = Resolve-ReferenceAssemblyVersion -PackageId $record.PackageId `
            -PackageVersion $record.ToVersion -ProjectText $repaired.Text -IdentityProvider $identityProvider
        if ([string]::IsNullOrWhiteSpace($assemblyVersion)) { continue }
        $redirectText = (Invoke-BindingRedirectReconciliation -AppConfigText $redirectText `
                -AssemblyName $record.PackageId -AssemblyVersion $assemblyVersion).Text
    }
    if ($redirectText -cne $appConfigText -and
        $PSCmdlet.ShouldProcess($appConfig[0], 'Reconcile binding redirects')) {
        & $writer $appConfig[0] $redirectText
        $written.Add($appConfig[0])
    }
}

# -WhatIf is passed explicitly: a preference variable set on this script does not reach a module's
# own session state, so a normalisation called without it writes during a what-if run.
$normalisation = Invoke-ManifestNormalization -DirectoryLister $lister -TextReader $reader `
    -TextWriter $writer -WhatIf:$WhatIfPreference
foreach ($path in @($normalisation.ChangedPath)) { $written.Add($path) }

# Summed across the per-project records rather than accumulated during the loop, so every
# aggregate reads from the same source the per-project report already published.
$total = { param($Selector) [int](@($verification | ForEach-Object $Selector | Measure-Object -Sum).Sum) }
$failure = @($verification | ForEach-Object { $_.Failure } | Where-Object { $null -ne $_ })

[pscustomobject]@{
    PSTypeName                = 'Repair.Result'
    RepositoryRoot            = $script:Root
    IsSuccess                 = ($failure.Count -eq 0)
    Verification              = $verification.ToArray()
    Skipped                   = $skipped.ToArray()
    Upgraded                  = $upgraded.ToArray()
    WrittenPath               = $written.ToArray()
    RepairCount               = (& $total { $_.Report.RepairCount })
    ExaminedProjectCount      = $verification.Count
    ExaminedElementCount      = (& $total { $_.Report.Examined.Total })
    ExaminedAnalyzerItemCount = (& $total { $_.ExaminedAnalyzerCount })
    AnalyzerProjectCount      = @($verification | Where-Object { $_.ExaminedAnalyzerCount -gt 0 }).Count
    VersionDisagreementCount  = (& $total { $_.DisagreementCount })
    AbsentFromManifestCount   = (& $total { $_.Report.AbsentFromManifestCount })
    MissingRoslynSegmentCount = (& $total { $_.Report.MissingRoslynSegmentCount })
    OrphanedHintPathCount     = (& $total { @($_.Report.OrphanedHintPath).Count })
    ExaminedManifestCount     = $normalisation.ExaminedPackagesConfig
    ExaminedAppConfigCount    = $normalisation.ExaminedAppConfig
    Failure                   = $failure
    Body                      = (& $script:ReportBody $verification.ToArray() $skipped.ToArray())
}
