<#
.SYNOPSIS
    Decides whether a candidate NuGet package ships an asset folder that the repository's
    target framework can consume, and which folder to select.

.DESCRIPTION
    PackageCompatibility is the framework-compatibility layer of the dependency-consistency
    tooling for issue #911. Every function is pure over the asset folder names supplied to
    it: nothing here touches the filesystem, so the whole module is exercisable in memory
    with no temporary file and no filesystem dependency. Callers that need the asset folders
    of a package on disk enumerate them through their own injected listing delegate and pass
    the resulting names in.

    The decision is asset-level. It is taken from the folder names a candidate package
    actually ships under its library directory, never from a declared target-framework
    attribute in a manifest or project file: a manifest attribute records what a project
    asked for, which is not evidence about what the package contains.

    Issue #902. The target framework is .NET Framework 4.8.1, which can load assets built
    for .NET Framework 4.8.1 and below and for .NET Standard 2.0 and below. It cannot load a
    netstandard2.1 asset at all, because .NET Framework implements no version of the .NET
    Standard above 2.0. That framework is therefore excluded outright rather than ranked
    below netstandard2.0: a ranking still selects it when nothing else is offered, which is
    precisely the defect #902 reports. Exclusion here is by non-membership of the consumable
    list below, so any framework absent from that list (a .NET-Core-era target such as
    net6.0, a Xamarin or UAP target, or a future .NET Standard revision) is rejected by the
    same rule rather than by an enumerated deny list that would need maintaining.

    Exported functions:
      - Select-CompatibleAssetFolder
      - Test-PackageAssetCompatibility
#>

Set-StrictMode -Version Latest

$script:TargetFramework = 'net481'

# Ordered by preference, most specific and most recent first. Membership in this collection
# is itself the compatibility rule: a folder name absent from it is not consumable by the
# target framework and is rejected. See the module DESCRIPTION for the issue #902 rationale
# behind exclusion-by-non-membership rather than ranking.
$script:ConsumableAssetFolder = @(
    'net481',
    'net48',
    'net472',
    'net471',
    'net47',
    'net462',
    'net461',
    'net46',
    'net452',
    'net451',
    'net45',
    'net40',
    'net35',
    'net20',
    'netstandard2.0',
    'netstandard1.6',
    'netstandard1.5',
    'netstandard1.4',
    'netstandard1.3',
    'netstandard1.2',
    'netstandard1.1',
    'netstandard1.0'
)

function Select-CompatibleAssetFolder {
    <#
    .SYNOPSIS
        Selects the best asset folder the target framework can consume from those offered.
    .DESCRIPTION
        The offered set is the list of folder names a candidate package ships under its
        library directory. Comparison is case-insensitive and ignores surrounding
        whitespace, because a directory listing and a manifest spell the same folder in
        different cases. An offered folder that is not a member of the consumable
        collection is never selected, whatever else is offered and whatever its position
        in the offered set.
    .PARAMETER AssetFolder
        The asset folder names the candidate package ships. May be empty or null.
    .OUTPUTS
        The selected folder name, or the empty string when the target framework can consume
        none of the offered folders.
    .EXAMPLE
        Select-CompatibleAssetFolder -AssetFolder @('net48', 'net20')
        Returns net48, the most preferred consumable member of the offered set.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [AllowEmptyCollection()]
        [string[]]$AssetFolder
    )

    $offered = @(@($AssetFolder) |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
                ForEach-Object { $_.Trim().ToLowerInvariant() })

    $selected = $script:ConsumableAssetFolder |
        Where-Object { $offered -contains $_ } |
            Select-Object -First 1

    if ($null -eq $selected) {
        return ''
    }

    return [string]$selected
}

function Test-PackageAssetCompatibility {
    <#
    .SYNOPSIS
        Decides whether a candidate package is consumable, and reports why when it is not.
    .DESCRIPTION
        Returns a decision record rather than a boolean, so a caller that skips a package
        can report the reason it skipped rather than reporting only that something was
        skipped. The rejection reason is always a non-empty string naming the package and
        the asset folders it offered; the acceptance always names the selected folder.
    .PARAMETER PackageId
        The candidate package identifier, used in the reason string.
    .PARAMETER AssetFolder
        The asset folder names the candidate package ships. May be empty or null.
    .OUTPUTS
        A PackageCompatibility.Decision record carrying IsCompatible, SelectedAssetFolder,
        OfferedAssetFolder and Reason.
    .EXAMPLE
        Test-PackageAssetCompatibility -PackageId 'Contoso' -AssetFolder @('net6.0')
        Returns a record whose IsCompatible is false and whose Reason names Contoso.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$PackageId,

        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [AllowEmptyCollection()]
        [string[]]$AssetFolder
    )

    $offered = @(@($AssetFolder) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    $selected = Select-CompatibleAssetFolder -AssetFolder $offered

    if ([string]::IsNullOrEmpty($selected)) {
        $offeredText = if ($offered.Count -eq 0) { '(none)' } else { $offered -join ', ' }
        return [pscustomobject]@{
            PSTypeName          = 'PackageCompatibility.Decision'
            PackageId           = $PackageId
            IsCompatible        = $false
            SelectedAssetFolder = ''
            OfferedAssetFolder  = $offered
            Reason              = "Package '$PackageId' ships no asset folder that $script:TargetFramework can consume. Offered: $offeredText."
        }
    }

    return [pscustomobject]@{
        PSTypeName          = 'PackageCompatibility.Decision'
        PackageId           = $PackageId
        IsCompatible        = $true
        SelectedAssetFolder = $selected
        OfferedAssetFolder  = $offered
        Reason              = ''
    }
}

Export-ModuleMember -Function @(
    'Select-CompatibleAssetFolder',
    'Test-PackageAssetCompatibility'
)
