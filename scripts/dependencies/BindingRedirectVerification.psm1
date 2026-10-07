<#
.SYNOPSIS
    Detects application-configuration binding redirects whose newVersion names an assembly
    version that no project file Reference declares.

.DESCRIPTION
    BindingRedirectVerification is a detection layer beside the dependency-consistency
    tooling for issue #911: PackageGraph parses, ProjectConsistency reconciles and
    ConsistencyVerifier detects manifest and project-file faults. This module carries the
    one rule issue #953 adds. A bindingRedirect newVersion must equal a version that some
    project file declares in a Reference Include for the same assembly name, because that is
    the assembly version the build copies to the output directory. Package versions are not
    consulted: a package version and its assembly version are different quantities.

    Both functions are pure over text. The deployed-version source is an injected
    scriptblock, so the detector runs in memory with no file dependency; the repository-level
    test supplies a provider built from every project file.

    Exported functions:
      - ConvertTo-ReferenceVersionMap
      - Find-StaleBindingRedirect
 #>

Set-StrictMode -Version Latest

<# Imported without -Force deliberately, as ProjectConsistency.psm1 does: a nested
   Import-Module -Force removes the module from the whole session before re-importing it,
   which would strip the parser from a caller that had already imported it. #>
Import-Module (Join-Path $PSScriptRoot 'PackageGraph.psm1')

function ConvertTo-ReferenceVersionMap {
    <#
    .SYNOPSIS
        Builds a map from assembly name to the versions the supplied project files declare
        in Reference Include attributes.
    .DESCRIPTION
        Only an Include of the form "Name, Version=X.Y.Z.W, ..." contributes. A Reference
        without a Version is not evidence of a deployed version and is skipped. Versions are
        unioned across every supplied text, so an assembly two projects reference at two
        versions maps to both. An empty or whitespace-only element is accepted at parameter
        binding and then rejected by ConvertFrom-ProjectFileText, which throws for it.
    .PARAMETER ProjectText
        The project-file texts. An empty collection yields an empty map.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [AllowEmptyString()]
        [string[]]$ProjectText
    )

    $versions = @{}
    foreach ($text in $ProjectText) {
        $reference = @(ConvertFrom-ProjectFileText -Text $text | Where-Object { $_.Kind -eq 'Reference' })
        foreach ($record in $reference) {
            $match = [regex]::Match($record.Value, '^\s*(?<name>[^,]+?)\s*,\s*Version=(?<version>[^,\s]+)')
            if (-not $match.Success) { continue }
            $name = $match.Groups['name'].Value
            if (-not $versions.ContainsKey($name)) {
                $versions[$name] = [System.Collections.Generic.List[string]]::new()
            }
            $value = $match.Groups['version'].Value
            if (-not $versions[$name].Contains($value)) { $versions[$name].Add($value) }
        }
    }

    $map = @{}
    foreach ($key in $versions.Keys) { $map[$key] = [string[]]$versions[$key].ToArray() }
    return $map
}

function Find-StaleBindingRedirect {
    <#
    .SYNOPSIS
        Reports every bindingRedirect whose newVersion equals no deployed version of its
        assembly, with the examined-entry count and the unverifiable assembly names.
    .DESCRIPTION
        A dependentAssembly block with no bindingRedirect is not examined. An assembly for
        which the provider returns no version is listed as unverifiable and is not a finding,
        so a redirect for a transitively deployed assembly needs no allow list. Only
        newVersion is compared; the oldVersion range is a request filter, not a deployment
        claim. Empty or whitespace text examines zero entries. Text that is not an
        application configuration document is rejected by the parser.
    .PARAMETER AppConfigText
        The application configuration text.
    .PARAMETER DeployedVersionProvider
        A delegate taking an assembly name and returning its deployed version strings, or
        nothing when the name is unknown.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$AppConfigText,

        [Parameter(Mandatory = $true)]
        [scriptblock]$DeployedVersionProvider
    )

    $finding = [System.Collections.Generic.List[pscustomobject]]::new()
    $unverifiable = [System.Collections.Generic.List[string]]::new()
    $examined = 0

    if (-not [string]::IsNullOrWhiteSpace($AppConfigText)) {
        foreach ($record in @(ConvertFrom-AppConfigText -Text $AppConfigText)) {
            if ([string]::IsNullOrEmpty($record.NewVersion)) { continue }
            $examined++
            $deployed = @(& $DeployedVersionProvider $record.Name |
                    Where-Object { -not [string]::IsNullOrEmpty([string]$_) } |
                        ForEach-Object { [string]$_ })
            if ($deployed.Count -eq 0) {
                if (-not $unverifiable.Contains($record.Name)) { $unverifiable.Add($record.Name) }
                continue
            }
            if ($deployed -contains $record.NewVersion) { continue }
            $finding.Add([pscustomobject]@{
                    PSTypeName       = 'BindingRedirectVerification.StaleRedirect'
                    AssemblyName     = $record.Name
                    NewVersion       = $record.NewVersion
                    DeployedVersions = [string[]]$deployed
                })
        }
    }

    return [pscustomobject]@{
        PSTypeName    = 'BindingRedirectVerification.DetectionResult'
        Finding       = $finding.ToArray()
        Unverifiable  = $unverifiable.ToArray()
        ExaminedCount = $examined
    }
}

Export-ModuleMember -Function @(
    'ConvertTo-ReferenceVersionMap',
    'Find-StaleBindingRedirect'
)
