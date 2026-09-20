<#
.SYNOPSIS
    Reconciles the dependent elements of a project file, and the binding redirects of an
    application configuration, with the package versions their sibling manifest declares.

.DESCRIPTION
    ProjectConsistency is the reconciliation layer of the dependency-consistency tooling
    for issue #911. It carries reconciliation only: detection, examined counts, the repairs
    report and the failure-result type live in ConsistencyVerifier.psm1, and the analyzer
    item repair lives in AnalyzerItemRepair.psm1. Keeping the three apart is what lets each
    file stay inside the repository line ceiling and gives every reported class exactly one
    owner.

    Every function is pure over text. Parsing is delegated to PackageGraph.psm1 rather than
    re-implemented here, so a manifest written inline and the same manifest reflowed across
    several lines reconcile identically.

    Every rewrite is a byte-exact replacement performed over the file's own text in
    PowerShell. No external text-substitution executable is invoked: the Windows paths these
    elements carry contain doubled backslashes, and a shell layer that collapses them
    produces a substitution that matches nothing while still rewriting the file. The text is
    split on its own line terminators with those terminators captured, so a file with mixed
    or non-native line endings is reassembled byte-for-byte outside the substituted spans.

    Exported functions:
      - Resolve-ReferenceAssemblyVersion
      - Invoke-VersionReconciliation
      - Invoke-BindingRedirectReconciliation
#>

Set-StrictMode -Version Latest

# Imported without -Force deliberately: a nested Import-Module -Force removes the module
# from the whole session before re-importing it here, which would strip the target from a
# caller that had already imported it. AnalyzerItemRepair.psm1 supplies the restore-path
# vocabulary, including the byte-exact folder-segment rewrite this module performs on three
# of its four element kinds; duplicating that rewrite here would give the repository two
# copies of the one substitution whose correctness gate rule 15 turns on.
Import-Module (Join-Path $PSScriptRoot 'PackageGraph.psm1')
Import-Module (Join-Path $PSScriptRoot 'AnalyzerItemRepair.psm1')
# PackageCompatibility supplies Select-CompatibleAssetFolder, which
# Resolve-ReferenceAssemblyVersion calls to pick the asset folder whose assembly version
# a Reference should declare. That module imports nothing, so no cycle is created.
Import-Module (Join-Path $PSScriptRoot 'PackageCompatibility.psm1')

$script:LineSplitPattern = '(\r?\n)'
$script:ReconciledKind = @('Import', 'Error', 'Reference', 'HintPath')

function Get-ReconciliationRepair {
    <#
    .SYNOPSIS
        Builds one repair record describing a single reconciled element.
    .PARAMETER Kind
        The dependent element kind that was reconciled.
    .PARAMETER PackageId
        The package the element depends on.
    .PARAMETER LineNumber
        The one-based line the element sits on.
    .PARAMETER From
        The element text before the substitution.
    .PARAMETER To
        The element text after the substitution.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)][string]$Kind,
        [Parameter(Mandatory = $true)][string]$PackageId,
        [Parameter(Mandatory = $true)][int]$LineNumber,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$From,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$To
    )

    return [pscustomobject]@{
        PSTypeName = 'ProjectConsistency.Repair'
        Kind       = $Kind
        PackageId  = $PackageId
        LineNumber = $LineNumber
        From       = $From
        To         = $To
    }
}

function Get-RewrittenReferenceVersionLine {
    <#
    .SYNOPSIS
        Returns the line with the assembly version inside a <Reference> Include rewritten.
    .DESCRIPTION
        The rewrite applies only when the Include's simple assembly name equals the package
        identifier. A package whose assemblies are named differently from the package is
        therefore left alone by this function, which is the correct outcome: this function
        has no evidence about which assembly such a package resolves.
    .PARAMETER Line
        The line text.
    .PARAMETER PackageId
        The package whose reference is rewritten.
    .PARAMETER AssemblyVersion
        The assembly version to write.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Line,
        [Parameter(Mandatory = $true)][string]$PackageId,
        [Parameter(Mandatory = $true)][string]$AssemblyVersion
    )

    $pattern = 'Include="(?<name>[^",]+)(?<between>,\s*Version=)(?<version>[^,"]+)'

    # Bound to locals before the evaluator closes over them. A parameter referenced only
    # inside a nested scriptblock reads as unused to static analysis.
    $targetName = $PackageId
    $replacement = $AssemblyVersion
    $evaluator = {
        param($match)
        if ($match.Groups['name'].Value.Trim() -ne $targetName) {
            return $match.Value
        }
        return 'Include="' + $match.Groups['name'].Value + $match.Groups['between'].Value + $replacement
    }

    return [regex]::Replace($Line, $pattern, $evaluator)
}

function Resolve-ReferenceAssemblyVersion {
    <#
    .SYNOPSIS
        Resolves the assembly version a Reference for one package should declare. A declared
        version the restored package carries somewhere is confirmed and preserved; a version
        the package carries nowhere is rewritten to the one in the selected asset folder.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$PackageId,
        [Parameter(Mandatory = $true)][ValidateNotNullOrEmpty()][string]$PackageVersion,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$ProjectText,
        [AllowNull()][scriptblock]$IdentityProvider = $null
    )

    $declared = ''
    $match = [regex]::Match($ProjectText,
        'Include="' + [regex]::Escape($PackageId) + ',\s*Version=(?<value>[^,"]+)')
    if ($match.Success) { $declared = $match.Groups['value'].Value }
    if ($null -eq $IdentityProvider) { return $declared }

    $identity = @(& $IdentityProvider $PackageId $PackageVersion | Where-Object { $null -ne $_ })
    if ($identity.Count -eq 0) { return $declared }
    if (-not [string]::IsNullOrEmpty($declared) -and
        @($identity | ForEach-Object { $_.Version }) -contains $declared) {
        return $declared
    }

    $folder = Select-CompatibleAssetFolder -AssetFolder @($identity | ForEach-Object { $_.AssetFolder })
    if ([string]::IsNullOrEmpty($folder)) { return $declared }
    $selected = @($identity | Where-Object { $_.AssetFolder.ToLowerInvariant() -eq $folder })
    if ($selected.Count -eq 0) { return $declared }
    return [string]$selected[0].Version
}

function Invoke-VersionReconciliation {
    <#
    .SYNOPSIS
        Forces the four dependent element kinds in a project file to agree with the
        version its sibling manifest declares for one package.
    .DESCRIPTION
        The four kinds are <Import>, <Error>, <Reference> and <HintPath>. The first, second
        and fourth carry the package folder segment ..\packages\<Id>.<Version>\, which is
        rewritten to the manifest version. The third carries an assembly version in its
        Include attribute, which is a different quantity: an assembly version is not
        required to track its package version, so the caller supplies the resolved value
        through -AssemblyVersion and the manifest version is used only as the fallback.

        <Analyzer Include> is deliberately not reconciled here. That item is repaired by
        AnalyzerItemRepair.psm1 under the preserve rule, which confirms against the restored
        package listing that the item's existing folder segment still exists before moving
        the version. Rewriting the version segment of an analyzer item from this function
        would bypass that confirmation and could emit a path the package does not ship.
    .PARAMETER ProjectText
        The project file text.
    .PARAMETER PackageId
        The package whose dependent elements are reconciled.
    .PARAMETER ManifestVersion
        The version the sibling manifest declares, which is the single source of truth.
    .PARAMETER AssemblyVersion
        The assembly version to write into a matching <Reference> Include attribute. When
        omitted it is derived from the manifest version.
    .OUTPUTS
        A ProjectConsistency.ReconciliationResult carrying the reconciled text, the repair
        records and the examined element count.
    .EXAMPLE
        Invoke-VersionReconciliation -ProjectText $text -PackageId 'Contoso' -ManifestVersion '2.0.0'
        Returns the text with every Contoso restore path naming 2.0.0.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$ProjectText,

        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$PackageId,

        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$ManifestVersion,

        [AllowEmptyString()]
        [string]$AssemblyVersion = ''
    )

    if ([string]::IsNullOrWhiteSpace($ProjectText)) {
        return [pscustomobject]@{
            PSTypeName    = 'ProjectConsistency.ReconciliationResult'
            Text          = $ProjectText
            Repair        = @()
            ExaminedCount = 0
        }
    }

    $resolvedAssemblyVersion = if ([string]::IsNullOrWhiteSpace($AssemblyVersion)) {
        $ManifestVersion
    }
    else {
        $AssemblyVersion
    }

    $element = @(ConvertFrom-ProjectFileText -Text $ProjectText |
            Where-Object { $script:ReconciledKind -contains $_.Kind })

    # Splitting with the terminator captured keeps every line ending exactly as the file
    # carries it. Content sits at the even indices and terminators at the odd ones, so the
    # one-based line number PackageGraph reports maps to index (number - 1) * 2.
    $part = [regex]::Split($ProjectText, $script:LineSplitPattern)
    $repair = [System.Collections.Generic.List[pscustomobject]]::new()

    foreach ($record in $element) {
        $index = ($record.LineNumber - 1) * 2
        if ($index -lt 0 -or $index -ge $part.Count) { continue }
        $before = $part[$index]
        $after = if ($record.Kind -eq 'Reference') {
            Get-RewrittenReferenceVersionLine -Line $before -PackageId $PackageId -AssemblyVersion $resolvedAssemblyVersion
        }
        else {
            Get-RewrittenPackageFolderLine -Line $before -PackageId $PackageId -PackageVersion $ManifestVersion
        }
        if ($after -ceq $before) { continue }
        $part[$index] = $after
        $repair.Add((Get-ReconciliationRepair -Kind $record.Kind -PackageId $PackageId `
                    -LineNumber $record.LineNumber -From $before.Trim() -To $after.Trim()))
    }

    return [pscustomobject]@{
        PSTypeName    = 'ProjectConsistency.ReconciliationResult'
        Text          = ($part -join '')
        Repair        = $repair.ToArray()
        ExaminedCount = $element.Count
    }
}

function Invoke-BindingRedirectReconciliation {
    <#
    .SYNOPSIS
        Reconciles an application configuration binding redirect to a resolved assembly
        version.
    .DESCRIPTION
        The resolved version is written into both the upper bound of oldVersion and into
        newVersion. An application configuration that carries no redirect for the named
        assembly is returned unchanged, because adding a redirect for an assembly the
        project never redirected is a new decision rather than a reconciliation.

        An oldVersion written as a range has its upper bound replaced and its lower bound
        left alone. An oldVersion written as a single version is replaced outright, there
        being no bound to preserve.
    .PARAMETER AppConfigText
        The application configuration text.
    .PARAMETER AssemblyName
        The assembly identity whose redirect is reconciled.
    .PARAMETER AssemblyVersion
        The assembly version resolved for that identity.
    .OUTPUTS
        A ProjectConsistency.ReconciliationResult carrying the reconciled text, the repair
        records and the examined element count.
    .EXAMPLE
        Invoke-BindingRedirectReconciliation -AppConfigText $text -AssemblyName 'Contoso' -AssemblyVersion '2.0.0.0'
        Returns the text with the Contoso redirect naming 2.0.0.0 in both positions.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$AppConfigText,

        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$AssemblyName,

        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$AssemblyVersion
    )

    if ($AppConfigText -notmatch '<configuration\b') {
        return [pscustomobject]@{
            PSTypeName    = 'ProjectConsistency.ReconciliationResult'
            Text          = $AppConfigText
            Repair        = @()
            ExaminedCount = 0
        }
    }

    # Bound to locals before the scriptblocks below close over them. A parameter referenced
    # only inside a nested scriptblock reads as unused to static analysis.
    $targetName = $AssemblyName
    $resolvedVersion = $AssemblyVersion

    $redirect = @(ConvertFrom-AppConfigText -Text $AppConfigText)
    $examined = $redirect.Count
    $target = @($redirect | Where-Object { $_.Name -eq $targetName -and -not [string]::IsNullOrEmpty($_.NewVersion) })
    if ($target.Count -eq 0) {
        return [pscustomobject]@{
            PSTypeName    = 'ProjectConsistency.ReconciliationResult'
            Text          = $AppConfigText
            Repair        = @()
            ExaminedCount = $examined
        }
    }

    $repair = [System.Collections.Generic.List[pscustomobject]]::new()
    $blockEvaluator = {
        param($match)
        $block = $match.Value
        if ($block -notmatch ('name="' + [regex]::Escape($targetName) + '"')) { return $block }

        $rewritten = [regex]::Replace($block, 'oldVersion="(?<value>[^"]*)"', {
                param($inner)
                $value = $inner.Groups['value'].Value
                $dash = $value.LastIndexOf('-')
                $bound = if ($dash -ge 0) {
                    $value.Substring(0, $dash + 1) + $resolvedVersion
                }
                else {
                    $resolvedVersion
                }
                return 'oldVersion="' + $bound + '"'
            })
        $rewritten = [regex]::Replace($rewritten, 'newVersion="[^"]*"', 'newVersion="' + $resolvedVersion + '"')

        if ($rewritten -cne $block) {
            $repair.Add((Get-ReconciliationRepair -Kind 'BindingRedirect' -PackageId $targetName `
                        -LineNumber 0 -From $block.Trim() -To $rewritten.Trim()))
        }
        return $rewritten
    }

    $text = [regex]::Replace($AppConfigText, '(?s)<dependentAssembly>.*?</dependentAssembly>', $blockEvaluator)

    return [pscustomobject]@{
        PSTypeName    = 'ProjectConsistency.ReconciliationResult'
        Text          = $text
        Repair        = $repair.ToArray()
        ExaminedCount = $examined
    }
}

Export-ModuleMember -Function @(
    'Resolve-ReferenceAssemblyVersion',
    'Invoke-VersionReconciliation',
    'Invoke-BindingRedirectReconciliation'
)
