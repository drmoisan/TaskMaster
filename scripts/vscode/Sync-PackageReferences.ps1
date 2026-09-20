<#
.SYNOPSIS
    Repairs project hint paths that no longer resolve after a package version change.

.DESCRIPTION
    Visual Studio writes a new version into packages.config when a package is updated but
    does not always persist the matching hint-path change into the project file. This
    script finds every hint path that does not resolve on disk, works out which package
    the path refers to, and rewrites it against the version the manifest now declares.

    Framework selection is not this script's concern and is deliberately not expressed
    here. Every decision about which asset folder the target framework can consume is
    delegated to scripts/dependencies/PackageCompatibility.psm1, which excludes
    unconsumable frameworks outright rather than ranking them (issue #902). This script
    previously carried its own ordered preference array, which ranked an unconsumable
    framework last and therefore still selected it when nothing else was offered.

    Every filesystem interaction is confined to the delegate table Get-PackageSyncSeam
    returns, so the repair logic is exercisable in memory with an injected table and
    creates no temporary file.

.PARAMETER SolutionRoot
    The directory to search for manifests. Defaults to the repository root.

.EXAMPLE
    .\Sync-PackageReferences.ps1 -SolutionRoot C:\repos\TaskMaster
#>
param(
    [Parameter(Mandatory = $false)]
    [string]$SolutionRoot
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Import-Module (Join-Path $PSScriptRoot '..\dependencies\PackageCompatibility.psm1') -Force
Import-Module (Join-Path $PSScriptRoot '..\dependencies\PackageGraph.psm1') -Force

$script:HintPathPattern = [regex]'<HintPath>(\.\.\\packages\\(.+?)\\lib\\([^\\]+)\\([^<]+))</HintPath>'
$script:RestoreDirectoryPattern = '\\packages\\'
$script:ConflictMarkerPattern = '<{7}|>{7}|={7}'

function Get-PackageSyncSeam {
    <#
    .SYNOPSIS
        Returns the delegate table through which this script reaches the filesystem.
    .DESCRIPTION
        Every member is a scriptblock, so a caller can substitute an in-memory table and
        exercise the repair logic without touching disk and without creating a temporary
        file. The production table is the only place in this file that calls a filesystem
        cmdlet or a reflection API.
    .OUTPUTS
        A hashtable whose members are ListManifestPath, ListProjectPath, ListAssetFolder,
        TestPath, ReadText, WriteText and ReadAssemblyIdentity.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param()

    return @{
        ListManifestPath     = {
            param([string]$Root)
            @(Get-ChildItem -LiteralPath $Root -Filter 'packages.config' -Recurse -File |
                    ForEach-Object { $_.FullName })
        }
        ListProjectPath      = {
            param([string]$Directory)
            @(Get-ChildItem -LiteralPath $Directory -Filter '*.csproj' -File |
                    ForEach-Object { $_.FullName })
        }
        ListAssetFolder      = {
            param([string]$LibraryDirectory)
            @(Get-ChildItem -LiteralPath $LibraryDirectory -Directory |
                    ForEach-Object { $_.Name })
        }
        TestPath             = {
            param([string]$Path)
            [bool](Test-Path -LiteralPath $Path)
        }
        ReadText             = {
            param([string]$Path)
            [System.IO.File]::ReadAllText($Path)
        }
        WriteText            = {
            param([string]$Path, [string]$Text)
            [System.IO.File]::WriteAllText($Path, $Text)
        }
        ReadAssemblyIdentity = {
            param([string]$Path)
            $identity = [System.Reflection.AssemblyName]::GetAssemblyName($Path)
            [pscustomobject]@{ Name = $identity.Name; Version = $identity.Version.ToString() }
        }
    }
}

function Get-PackageVersionMap {
    <#
    .SYNOPSIS
        Builds a package-identifier to version lookup from manifest text.
    .DESCRIPTION
        Parsing is delegated to PackageGraph so that this script and the consistency
        tooling read a manifest the same way, including a manifest whose elements have
        been reflowed across several lines.
    .PARAMETER ManifestText
        The packages.config text.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$ManifestText
    )

    $map = @{}
    foreach ($record in @(ConvertFrom-PackagesConfigText -Text $ManifestText)) {
        $map[$record.Id] = $record.Version
    }
    return $map
}

function Resolve-ManifestPackageId {
    <#
    .SYNOPSIS
        Identifies which declared package a restore folder name belongs to.
    .DESCRIPTION
        A restore folder is named identifier-dot-version, and an identifier may itself
        contain dots, so the match is made by prefix with a digit required immediately
        after the separating dot.
    .PARAMETER FolderName
        The restore folder name taken from the unresolved hint path.
    .PARAMETER VersionMap
        The manifest lookup Get-PackageVersionMap produced.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)][string]$FolderName,
        [Parameter(Mandatory = $true)][hashtable]$VersionMap
    )

    foreach ($identifier in $VersionMap.Keys) {
        if (-not $FolderName.StartsWith("$identifier.", [StringComparison]::OrdinalIgnoreCase)) {
            continue
        }
        if ($FolderName.Substring($identifier.Length + 1) -match '^\d') {
            return [string]$identifier
        }
    }

    return ''
}

function Resolve-PackageAssetFolder {
    <#
    .SYNOPSIS
        Selects the asset folder to bind to, through the shared compatibility module.
    .DESCRIPTION
        This function declares no framework ordering of its own. It enumerates the asset
        folders a restored package actually ships, optionally narrows them to those that
        contain the required file, and hands the resulting set to
        Select-CompatibleAssetFolder. A framework the target cannot consume is therefore
        never selected, whatever else is or is not offered.
    .PARAMETER LibraryDirectory
        The restored package's lib directory.
    .PARAMETER Seam
        The filesystem delegate table.
    .PARAMETER RequiredFile
        When supplied, only asset folders containing this file are offered.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)][string]$LibraryDirectory,
        [Parameter(Mandatory = $true)][hashtable]$Seam,
        [Parameter(Mandatory = $false)][AllowEmptyString()][string]$RequiredFile = ''
    )

    if (-not (& $Seam.TestPath $LibraryDirectory)) {
        return ''
    }

    $offered = @(& $Seam.ListAssetFolder $LibraryDirectory)
    if (-not [string]::IsNullOrEmpty($RequiredFile)) {
        $offered = @($offered | Where-Object {
                & $Seam.TestPath (Join-Path $LibraryDirectory ('{0}\{1}' -f $_, $RequiredFile))
            })
    }

    return [string](Select-CompatibleAssetFolder -AssetFolder $offered)
}

function Get-HintPathRepair {
    <#
    .SYNOPSIS
        Produces one repair record per hint path that does not resolve.
    .DESCRIPTION
        A hint path that already resolves is left alone, and so is one whose restore
        folder belongs to no declared package. When the manifest version differs, the
        existing asset folder is preferred at the corrected version; only when that does
        not resolve is a different asset folder selected, and then only through the
        shared compatibility module.
    .PARAMETER ProjectText
        The project file text.
    .PARAMETER ProjectDirectory
        The directory the project file sits in, used to resolve relative hint paths.
    .PARAMETER PackagesDirectory
        The solution's restore directory.
    .PARAMETER VersionMap
        The manifest lookup Get-PackageVersionMap produced.
    .PARAMETER Seam
        The filesystem delegate table.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$ProjectText,
        [Parameter(Mandatory = $true)][string]$ProjectDirectory,
        [Parameter(Mandatory = $true)][string]$PackagesDirectory,
        [Parameter(Mandatory = $true)][hashtable]$VersionMap,
        [Parameter(Mandatory = $true)][hashtable]$Seam
    )

    $repairs = [System.Collections.Generic.List[pscustomobject]]::new()

    foreach ($match in $script:HintPathPattern.Matches($ProjectText)) {
        $currentValue = $match.Groups[1].Value
        $restoreFolder = $match.Groups[2].Value
        $assetFolder = $match.Groups[3].Value
        $fileName = $match.Groups[4].Value

        if (& $Seam.TestPath (Join-Path $ProjectDirectory $currentValue)) {
            continue
        }

        $identifier = Resolve-ManifestPackageId -FolderName $restoreFolder -VersionMap $VersionMap
        if ([string]::IsNullOrEmpty($identifier)) {
            continue
        }

        $targetFolder = '{0}.{1}' -f $identifier, $VersionMap[$identifier]
        $candidate = '..\packages\{0}\lib\{1}\{2}' -f $targetFolder, $assetFolder, $fileName

        if (-not (& $Seam.TestPath (Join-Path $ProjectDirectory $candidate))) {
            $libraryDirectory = Join-Path $PackagesDirectory ('{0}\lib' -f $targetFolder)
            $selected = Resolve-PackageAssetFolder -LibraryDirectory $libraryDirectory -Seam $Seam -RequiredFile $fileName
            if ([string]::IsNullOrEmpty($selected)) {
                Write-Warning "Cannot resolve $fileName from $targetFolder; no asset folder the target framework can consume ships it."
                continue
            }
            $candidate = '..\packages\{0}\lib\{1}\{2}' -f $targetFolder, $selected, $fileName
        }

        $repairs.Add([pscustomobject]@{
                PSTypeName = 'SyncPackageReferences.HintPathRepair'
                OldElement = $match.Value
                NewElement = "<HintPath>$candidate</HintPath>"
                HintPath   = $candidate
            })
    }

    return $repairs.ToArray()
}

function Repair-ProjectReferenceVersion {
    <#
    .SYNOPSIS
        Rewrites the assembly version inside a reference Include attribute.
    .DESCRIPTION
        Pure over text. Returns the input unchanged when the reference is absent or
        already carries the resolved version, so applying it twice is a no-op.
    .PARAMETER ProjectText
        The project file text.
    .PARAMETER AssemblyName
        The simple assembly name read from the resolved file.
    .PARAMETER AssemblyVersion
        The four-part assembly version read from the resolved file.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$ProjectText,
        [Parameter(Mandatory = $true)][string]$AssemblyName,
        [Parameter(Mandatory = $true)][string]$AssemblyVersion
    )

    $pattern = '(Include="{0},\s*Version=)(\d+\.\d+\.\d+\.\d+)' -f [regex]::Escape($AssemblyName)
    $match = [regex]::Match($ProjectText, $pattern)
    if (-not $match.Success) {
        return $ProjectText
    }
    if ($match.Groups[2].Value -eq $AssemblyVersion) {
        return $ProjectText
    }

    return $ProjectText.Replace($match.Value, $match.Groups[1].Value + $AssemblyVersion)
}

function Invoke-ProjectReferenceSync {
    <#
    .SYNOPSIS
        Applies every hint-path repair for one manifest's sibling project file.
    .PARAMETER ManifestPath
        The packages.config path.
    .PARAMETER PackagesDirectory
        The solution's restore directory.
    .PARAMETER Seam
        The filesystem delegate table.
    #>
    [CmdletBinding(SupportsShouldProcess = $true)]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)][string]$ManifestPath,
        [Parameter(Mandatory = $true)][string]$PackagesDirectory,
        [Parameter(Mandatory = $true)][hashtable]$Seam
    )

    $projectDirectory = Split-Path -Path $ManifestPath -Parent
    $projectName = Split-Path -Path $projectDirectory -Leaf
    $result = [pscustomobject]@{
        PSTypeName  = 'SyncPackageReferences.ProjectResult'
        ProjectName = $projectName
        FixedCount  = 0
        Skipped     = $true
    }

    $projectPaths = @(& $Seam.ListProjectPath $projectDirectory |
            Where-Object { (Split-Path -Path $_ -Leaf) -notmatch '_(BASE|LOCAL|REMOTE)_' })
    if ($projectPaths.Count -eq 0) {
        return $result
    }

    $projectPath = $projectPaths[0]
    $projectText = [string](& $Seam.ReadText $projectPath)
    if ($projectText -match $script:ConflictMarkerPattern) {
        Write-Warning "  [$projectName] Merge conflict markers detected, skipping"
        return $result
    }

    $versionMap = Get-PackageVersionMap -ManifestText ([string](& $Seam.ReadText $ManifestPath))
    $repairs = @(Get-HintPathRepair -ProjectText $projectText -ProjectDirectory $projectDirectory -PackagesDirectory $PackagesDirectory -VersionMap $versionMap -Seam $Seam)

    $result.Skipped = $false
    if ($repairs.Count -eq 0) {
        return $result
    }

    foreach ($repair in $repairs) {
        $projectText = $projectText.Replace($repair.OldElement, $repair.NewElement)
    }

    foreach ($repair in $repairs) {
        $resolved = Join-Path $projectDirectory $repair.HintPath
        if (-not (& $Seam.TestPath $resolved)) {
            continue
        }
        $identity = & $Seam.ReadAssemblyIdentity $resolved
        $projectText = Repair-ProjectReferenceVersion -ProjectText $projectText -AssemblyName $identity.Name -AssemblyVersion $identity.Version
    }

    if ($PSCmdlet.ShouldProcess($projectPath, 'Rewrite unresolved hint paths')) {
        & $Seam.WriteText $projectPath $projectText
        $result.FixedCount = $repairs.Count
        Write-Information "  [$projectName] Fixed $($repairs.Count) broken HintPath(s)" -InformationAction Continue
    }

    return $result
}

function Invoke-PackageReferenceSync {
    <#
    .SYNOPSIS
        Repairs unresolved hint paths across every manifest under a solution root.
    .PARAMETER SolutionRoot
        The directory to search. Defaults to the repository root.
    .PARAMETER Seam
        The filesystem delegate table. Defaults to the production table.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $false)][AllowEmptyString()][string]$SolutionRoot = '',
        [Parameter(Mandatory = $false)][hashtable]$Seam
    )

    if ([string]::IsNullOrWhiteSpace($SolutionRoot)) {
        $SolutionRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
    }
    if ($null -eq $Seam) {
        $Seam = Get-PackageSyncSeam
    }

    $packagesDirectory = Join-Path $SolutionRoot 'packages'
    $examined = 0
    $totalFixed = 0

    foreach ($manifestPath in @(& $Seam.ListManifestPath $SolutionRoot)) {
        if ($manifestPath -match $script:RestoreDirectoryPattern) {
            continue
        }
        $examined++
        $projectResult = Invoke-ProjectReferenceSync -ManifestPath $manifestPath -PackagesDirectory $packagesDirectory -Seam $Seam
        $totalFixed += $projectResult.FixedCount
    }

    if ($totalFixed -gt 0) {
        Write-Information "Sync-PackageReferences: Fixed $totalFixed HintPath(s) total" -InformationAction Continue
    }
    else {
        Write-Information 'Sync-PackageReferences: All HintPaths are up to date' -InformationAction Continue
    }

    return [pscustomobject]@{
        PSTypeName    = 'SyncPackageReferences.Summary'
        SolutionRoot  = $SolutionRoot
        ExaminedCount = $examined
        FixedCount    = $totalFixed
    }
}

if ($MyInvocation.InvocationName -ne '.') {
    $null = Invoke-PackageReferenceSync @PSBoundParameters
}
