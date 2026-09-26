<#
.SYNOPSIS
    Derives the <Analyzer Include> path set a restored analyzer package implies, and
    rewrites the stale analyzer items of a project file under the preserve rule.

.DESCRIPTION
    AnalyzerItemRepair is the restore-path layer of the dependency-consistency tooling for
    issue #911, and the fix for issue #898. The NuGet CLI's project system contains no
    analyzer-item logic at all, so an upgrade leaves every <Analyzer Include> naming the
    previous package version while the manifest and the guards move on.

    The repair is governed by the preserve rule. It changes only the <Id>.<Version> segment
    of an item's path and leaves every following segment byte-identical, including the
    Roslyn-qualified folder. The restored package directory is enumerated through the
    injected listing delegate solely to confirm the preserved segment still exists in the
    new version's listing; it is never enumerated to select a folder, and the path is never
    computed from the package identifier.

    Selecting a folder is prohibited, and the reason is measured rather than stylistic.
    Neither analyzer family in this repository names the highest folder its package ships:
    Meziantou.Analyzer items sit at roslyn5.0 while the package ships roslyn4.14, roslyn4.8,
    roslyn5.0, roslyn5.6 and roslyn5.9, and Roslynator.Analyzers items sit at roslyn4.7
    while the package ships roslyn3.8, roslyn4.7 and roslyn5.0. A highest-folder rule would
    rewrite all 80 items in those two families rather than the 15 that are stale, and would
    stake the analyzer build on a Roslyn version the installed MSBuild may not support. The
    existing segment encodes a toolchain-compatibility choice the package's own install step
    made against the installed Visual Studio, and nothing here can re-make it.

    When the preserved segment is absent from the new version's listing the repair emits no
    guessed path: it leaves the item unmodified and returns a missing-segment record naming
    the project, the item, the missing segment and the segments the package does ship. This
    module builds no report and counts nothing for that class. ConsistencyVerifier.psm1
    aggregates the records, counts them and emits the non-fatal reported class, so exactly
    one module owns it.

    Every rewrite is a byte-exact replacement performed over the project file's own text in
    PowerShell. No external text-substitution executable is invoked: an analyzer item path
    carries several doubled backslashes, and a shell layer that collapses them produces a
    substitution matching nothing while still rewriting the file.

    This module is also the home of the restore-path vocabulary the consistency layer
    shares: extracting the package folder segment from a dependent element, splitting that
    segment into an identifier and a version, enumerating the elements that carry one, and
    rewriting the folder segment in place. Those belong with the module whose subject is
    restore paths, and Batch C's production-file cap admits no fourth file to hold them.

    Exported functions:
      - Get-RestorePackageFolder
      - Get-FolderPackageIdentity
      - Get-FolderBearingElement
      - Get-RewrittenPackageFolderLine
      - Get-AnalyzerAssemblyPath
      - Invoke-AnalyzerItemRepair
#>

Set-StrictMode -Version Latest

# Imported without -Force deliberately: a nested Import-Module -Force removes the module
# from the whole session before re-importing it here, which would strip PackageGraph from a
# caller that had already imported it.
Import-Module (Join-Path $PSScriptRoot 'PackageGraph.psm1')

# Separators are built from their character codes rather than written as literal escaped
# backslashes: a doubled backslash can be collapsed in transit between an author and the
# file, leaving a character class that matches nothing while the code still reads right.
$script:SeparatorClass = '[' + [regex]::Escape([string][char]92) + '/]'
$script:NonSeparatorClass = '[^' + [regex]::Escape([string][char]92) + '/]'
$script:SeparatorChar = [char[]]@([char]92, [char]47)
$script:DirectorySeparator = [string][char]92
$script:FolderBearingKind = @('Import', 'Error', 'HintPath', 'Analyzer')
$script:AnalyzerRoot = 'analyzers'
$script:ExcludedLanguageFolder = @('vb', 'fs')
$script:SatelliteSuffix = '.resources.dll'
$script:AssemblySuffix = '.dll'
$script:LineSplitPattern = '(\r?\n)'

function Get-RestorePackageFolder {
    <#
    .SYNOPSIS
        Extracts the package folder segment from a restore path, or the empty string.
    .PARAMETER Value
        The dependent element's primary value.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$Value)

    $pattern = 'packages' + $script:SeparatorClass + '(?<folder>' + $script:NonSeparatorClass +
    '+)' + $script:SeparatorClass
    $match = [regex]::Match($Value, $pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    if (-not $match.Success) { return '' }
    return $match.Groups['folder'].Value
}

function Get-FolderPackageIdentity {
    <#
    .SYNOPSIS
        Splits a package folder segment into identifier and version using a known
        identifier vocabulary.
    .DESCRIPTION
        The split cannot be taken on the last dot, because a version has several dots and an
        identifier may too. Candidates are tried longest first and the remainder must begin
        with a digit, which keeps Microsoft.Extensions.Configuration from claiming
        Microsoft.Extensions.Configuration.Binder.10.0.12. The ordering is over the supplied
        identifier vocabulary and never over anything derived from a directory listing.
    .PARAMETER Folder
        The package folder segment.
    .PARAMETER KnownId
        The package identifiers to try, normally those the manifest declares.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Folder,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]]$KnownId
    )

    foreach ($candidate in @($KnownId | Sort-Object -Property Length -Descending)) {
        if ($Folder.Length -le $candidate.Length + 1) { continue }
        if (-not $Folder.StartsWith($candidate + '.', [System.StringComparison]::OrdinalIgnoreCase)) { continue }
        $remainder = $Folder.Substring($candidate.Length + 1)
        if ($remainder -notmatch '^\d') { continue }
        return [pscustomobject]@{ Id = $candidate; Version = $remainder }
    }
    return $null
}

function Get-FolderBearingElement {
    <#
    .SYNOPSIS
        Returns the dependent elements that carry a restore path, each with its folder.
    .PARAMETER ProjectText
        The project file text.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject[]])]
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$ProjectText)

    if ([string]::IsNullOrWhiteSpace($ProjectText)) { return [pscustomobject[]]@() }
    $records = [System.Collections.Generic.List[pscustomobject]]::new()
    foreach ($element in @(ConvertFrom-ProjectFileText -Text $ProjectText)) {
        if ($script:FolderBearingKind -notcontains $element.Kind) { continue }
        $folder = Get-RestorePackageFolder -Value $element.Value
        if ([string]::IsNullOrEmpty($folder)) { continue }
        $records.Add([pscustomobject]@{
                Kind = $element.Kind; LineNumber = $element.LineNumber
                Value = $element.Value; PackageFolder = $folder
            })
    }
    return [pscustomobject[]]$records.ToArray()
}

function Get-RewrittenPackageFolderLine {
    <#
    .SYNOPSIS
        Returns the line with the package folder segment of every restore path rewritten.
    .DESCRIPTION
        Only the <Id>.<Version> segment moves. Every other segment of the path, and every
        other character on the line, is left byte-identical: the substitution is anchored on
        the separators either side of the segment and reuses the separator characters and
        the identifier casing the line already carries. A version segment starts with a
        digit and runs to the next separator, which is what keeps a package identifier from
        matching a longer sibling.
    .PARAMETER Line
        The line text.
    .PARAMETER PackageId
        The package whose folder segment is rewritten.
    .PARAMETER PackageVersion
        The version to write.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Line,
        [Parameter(Mandatory = $true)][string]$PackageId,
        [Parameter(Mandatory = $true)][string]$PackageVersion
    )

    $pattern = 'packages(?<s1>' + $script:SeparatorClass + ')(?<id>' + [regex]::Escape($PackageId) +
    ')\.(?<version>\d' + $script:NonSeparatorClass + '*?)(?<s2>' + $script:SeparatorClass + ')'

    # Bound to a local before the evaluator closes over it. A parameter referenced only
    # inside a nested scriptblock reads as unused to static analysis.
    $replacement = $PackageVersion
    $evaluator = {
        param($match)
        return 'packages' + $match.Groups['s1'].Value + $match.Groups['id'].Value + '.' +
        $replacement + $match.Groups['s2'].Value
    }

    return [regex]::Replace($Line, $pattern, $evaluator, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
}

# Private. Returns the directory path an analyzer asset sits under, relative to the package's
# analyzer root, or $null when the entry is not a consumable analyzer assembly. An entry
# directly in the analyzer root yields the empty string, which is a valid segment.
function Get-AnalyzerDirectorySegment {
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$RelativePath)

    $segment = @($RelativePath.Split($script:SeparatorChar) | Where-Object { $_ -ne '' })
    if ($segment.Count -lt 2) { return $null }
    if ($segment[0] -ne $script:AnalyzerRoot) { return $null }
    $leaf = $segment[-1]
    if (-not $leaf.EndsWith($script:AssemblySuffix, [System.StringComparison]::OrdinalIgnoreCase)) { return $null }
    if ($leaf.EndsWith($script:SatelliteSuffix, [System.StringComparison]::OrdinalIgnoreCase)) { return $null }

    $directory = if ($segment.Count -gt 2) { @($segment[1..($segment.Count - 2)]) } else { @() }
    foreach ($part in $directory) {
        if ($script:ExcludedLanguageFolder -contains $part.ToLowerInvariant()) { return $null }
    }
    return ($directory -join $script:DirectorySeparator)
}

function Get-AnalyzerAssemblyPath {
    <#
    .SYNOPSIS
        Derives the analyzer item paths a restored package's listing implies.
    .DESCRIPTION
        Only entries under the package's analyzer root are considered; non-C-sharp language
        folders and satellite resource assemblies are excluded. When a preserved segment is
        supplied, only entries sitting directly under that segment are returned, which is
        how the preserve rule reaches the derivation: the listing confirms the segment, it
        does not choose one. A package whose listing contains no analyzer directory
        contributes no items, which is an empty result rather than an error.
    .PARAMETER PackageId
        The analyzer package identifier.
    .PARAMETER PackageVersion
        The version the manifest declares, which forms the package folder segment.
    .PARAMETER DirectoryLister
        A delegate returning the restored package's contents as relative paths.
    .PARAMETER PreservedSegment
        The directory path, relative to the analyzer root, that the existing items already
        name. Omitted when deriving the full set.
    .EXAMPLE
        Get-AnalyzerAssemblyPath -PackageId 'Contoso' -PackageVersion '2.0.0' -DirectoryLister $lister
        Returns one item path per consumable analyzer assembly the listing offers.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$PackageId,

        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$PackageVersion,

        [Parameter(Mandatory = $true)]
        [scriptblock]$DirectoryLister,

        [AllowEmptyString()]
        [AllowNull()]
        [string]$PreservedSegment = $null
    )

    $prefix = '..' + $script:DirectorySeparator + 'packages' + $script:DirectorySeparator +
    $PackageId + '.' + $PackageVersion + $script:DirectorySeparator
    $filtered = $PSBoundParameters.ContainsKey('PreservedSegment')
    $derived = [System.Collections.Generic.List[string]]::new()

    foreach ($entry in @(& $DirectoryLister)) {
        if ($null -eq $entry) { continue }
        $relative = [string]$entry
        $segment = Get-AnalyzerDirectorySegment -RelativePath $relative
        if ($null -eq $segment) { continue }
        if ($filtered -and $segment -ne $PreservedSegment) { continue }
        $derived.Add($prefix + $relative)
    }

    return [string[]]$derived.ToArray()
}

function Invoke-AnalyzerItemRepair {
    <#
    .SYNOPSIS
        Rewrites the stale <Analyzer Include> items of one project under the preserve rule.
    .DESCRIPTION
        Every analyzer item group in the project is visited, not only the first, and each
        group's sibling elements and preceding explanatory comment are left untouched
        because the rewrite is a same-line substitution of the version segment alone. An
        item already naming the manifest version is left alone, so the pass is a provable
        no-op on an already-agreeing project. An item whose preserved segment the new
        version's listing does not offer is left unmodified and reported as a record. A
        listing that is empty altogether means the restored directory does not exist, and
        the function throws rather than emitting a guessed path.
    .PARAMETER ProjectName
        The project being repaired, recorded on any missing-segment record.
    .PARAMETER ProjectText
        The project file text.
    .PARAMETER PackageId
        The analyzer package whose items are repaired.
    .PARAMETER ManifestVersion
        The version the sibling manifest declares.
    .PARAMETER DirectoryLister
        A delegate returning the restored package's contents as relative paths.
    .OUTPUTS
        An AnalyzerItemRepair.Result carrying the rewritten text, the repaired items, the
        missing-segment records and the examined item count.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$ProjectName,

        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$ProjectText,

        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$PackageId,

        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$ManifestVersion,

        [Parameter(Mandatory = $true)]
        [scriptblock]$DirectoryLister
    )

    $listing = @(& $DirectoryLister | Where-Object { $null -ne $_ })
    if ($listing.Count -eq 0) {
        throw ("The restored directory for package '$PackageId' version '$ManifestVersion' " +
            'contains no entries, so no analyzer item path can be derived. A repair that ' +
            'cannot be derived is reported rather than guessed.')
    }

    # The segments the package actually ships. This set is used only to confirm the segment
    # an existing item already names; nothing selects from it and nothing orders it.
    $offeredSegment = [System.Collections.Generic.List[string]]::new()
    foreach ($entry in $listing) {
        $segment = Get-AnalyzerDirectorySegment -RelativePath ([string]$entry)
        if ($null -eq $segment) { continue }
        if (-not $offeredSegment.Contains($segment)) { $offeredSegment.Add($segment) }
    }

    $owned = @(Get-FolderBearingElement -ProjectText $ProjectText |
            Where-Object { $_.Kind -eq 'Analyzer' } |
                ForEach-Object {
                    $identity = Get-FolderPackageIdentity -Folder $_.PackageFolder -KnownId @($PackageId)
                    if ($null -eq $identity) { return }
                    [pscustomobject]@{ Element = $_; FoundVersion = $identity.Version }
                })

    $part = [regex]::Split($ProjectText, $script:LineSplitPattern)
    $repaired = [System.Collections.Generic.List[pscustomobject]]::new()
    $missing = [System.Collections.Generic.List[pscustomobject]]::new()

    foreach ($candidate in $owned) {
        $element = $candidate.Element
        if ($candidate.FoundVersion -eq $ManifestVersion) { continue }

        $remainder = $element.Value.Substring(
            $element.Value.IndexOf($element.PackageFolder, [System.StringComparison]::OrdinalIgnoreCase) +
            $element.PackageFolder.Length).TrimStart($script:SeparatorChar)
        $preserved = Get-AnalyzerDirectorySegment -RelativePath $remainder
        if ($null -eq $preserved -or -not $offeredSegment.Contains($preserved)) {
            $missing.Add([pscustomobject]@{
                    PSTypeName     = 'AnalyzerItemRepair.MissingSegment'
                    ProjectName = $ProjectName; LineNumber = $element.LineNumber
                    Item = $element.Value; MissingSegment = $preserved
                    OfferedSegment = $offeredSegment.ToArray()
                })
            continue
        }

        $index = ($element.LineNumber - 1) * 2
        if ($index -lt 0 -or $index -ge $part.Count) { continue }
        $before = $part[$index]
        $after = Get-RewrittenPackageFolderLine -Line $before -PackageId $PackageId -PackageVersion $ManifestVersion
        if ($after -ceq $before) { continue }
        $part[$index] = $after
        $repaired.Add([pscustomobject]@{
                PSTypeName = 'AnalyzerItemRepair.Repair'
                Kind = 'Analyzer'; PackageId = $PackageId
                LineNumber = $element.LineNumber; From = $before.Trim(); To = $after.Trim()
            })
    }

    return [pscustomobject]@{
        PSTypeName           = 'AnalyzerItemRepair.Result'
        ProjectName          = $ProjectName
        Text                 = ($part -join '')
        RepairedItem         = $repaired.ToArray()
        MissingSegmentRecord = $missing.ToArray()
        ExaminedItemCount    = @($owned).Count
    }
}

Export-ModuleMember -Function @(
    'Get-RestorePackageFolder',
    'Get-FolderPackageIdentity',
    'Get-FolderBearingElement',
    'Get-RewrittenPackageFolderLine',
    'Get-AnalyzerAssemblyPath',
    'Invoke-AnalyzerItemRepair'
)
