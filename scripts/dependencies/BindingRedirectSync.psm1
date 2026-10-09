<#
.SYNOPSIS
    Synchronises application-configuration binding redirects with the assembly versions the
    solution's project files reference.

.DESCRIPTION
    Detection of a stale redirect lives in BindingRedirectVerification.psm1. This module is the
    writer that keeps every bindingRedirect newVersion inside the solution-wide csproj Reference
    version map, covering assemblies a project receives only transitively: such a project
    declares the assembly in neither its manifest nor its project file, so neither the package
    updater nor the per-manifest repair pass ever edits its redirect (issue #985).

    The pass is stale-only. A redirect whose newVersion is already a deployed version is never
    modified, which preserves curated ranges. A stale redirect is rewritten to the project's own
    Reference version when that is unambiguous, and otherwise to the highest deployed version by
    numeric comparison, mirroring the conflict resolution the build applies. A name with no
    deployed version is reported as unverifiable, and a name whose deployed versions do not all
    parse is reported as unresolvable; neither is changed. Rewrites reuse
    Invoke-BindingRedirectReconciliation, so only attribute values inside the matched block change.

    Exported functions:
      - Invoke-BindingRedirectSync
      - Invoke-SolutionBindingRedirectSync
      - Format-BindingRedirectSyncReport
#>

Set-StrictMode -Version Latest

<# Imported without the force switch deliberately, as BindingRedirectVerification.psm1 does: a
   forced nested import removes the module from the whole session before re-importing it, which
   would strip the parser from a caller that had already imported it. #>
Import-Module (Join-Path $PSScriptRoot 'PackageGraph.psm1')
Import-Module (Join-Path $PSScriptRoot 'BindingRedirectVerification.psm1')
Import-Module (Join-Path $PSScriptRoot 'ProjectConsistency.psm1')

# Separators are built from their character codes rather than written as escaped literals: a
# doubled backslash can be collapsed in transit and leave a pattern that matches nothing.
$script:DirectorySeparator = [char]92
$script:PathSeparator = [char]47
$script:ExcludedSegment = @('packages', 'bin', 'obj', 'node_modules')

# Private. Emits the distinct non-empty version strings a provider yields for a name, and nothing
# when no provider was supplied; callers collect the output with @().
function Get-ProviderVersion {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [AllowNull()][scriptblock]$Provider,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Name
    )

    if ($null -eq $Provider) { return }
    & $Provider $Name | Where-Object { -not [string]::IsNullOrEmpty([string]$_) } |
        ForEach-Object { [string]$_ } | Select-Object -Unique
}

# Private. True for a project-file path outside every restore and build output directory, the
# same exclusion Get-PackageManifestPath applies to manifests.
function Test-SyncProjectPath {
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory = $true)][string]$Path)

    $extension = [System.IO.Path]::GetExtension($Path)
    if (-not [string]::Equals($extension, '.csproj', [System.StringComparison]::OrdinalIgnoreCase)) {
        return $false
    }
    $segment = $Path.Replace($script:DirectorySeparator, $script:PathSeparator).Split($script:PathSeparator)
    if ($segment.Count -lt 2) { return $true }
    $parent = $segment[0..($segment.Count - 2)]
    return (@($parent | Where-Object { $script:ExcludedSegment -contains $_ }).Count -eq 0)
}

function Invoke-BindingRedirectSync {
    <#
    .SYNOPSIS
        Rewrites every stale binding redirect in one application configuration to a deployed
        assembly version.
    .DESCRIPTION
        Pure over text. A dependentAssembly block with no bindingRedirect is not examined, and a
        name already handled in this call is skipped. Empty or whitespace text examines zero
        entries; text that is not an application configuration document is rejected by the
        parser, whose exception propagates.
    .PARAMETER AppConfigText
        The application configuration text.
    .PARAMETER DeployedVersionProvider
        A delegate taking an assembly name and returning every version the solution's project
        files reference for it, or nothing when the name is unknown.
    .PARAMETER PreferredVersionProvider
        An optional delegate taking an assembly name and returning the versions the sibling
        project file references for it.
    .OUTPUTS
        A BindingRedirectSync.Result carrying the text, the repair records, the unverifiable and
        unresolvable names and the examined count.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$AppConfigText,

        [Parameter(Mandatory = $true)]
        [scriptblock]$DeployedVersionProvider,

        [AllowNull()]
        [scriptblock]$PreferredVersionProvider = $null
    )

    $text = $AppConfigText
    $repair = [System.Collections.Generic.List[pscustomobject]]::new()
    $unverifiable = [System.Collections.Generic.List[string]]::new()
    $unresolvable = [System.Collections.Generic.List[string]]::new()
    $handled = [System.Collections.Generic.List[string]]::new()
    $examined = 0

    if (-not [string]::IsNullOrWhiteSpace($AppConfigText)) {
        foreach ($record in @(ConvertFrom-AppConfigText -Text $AppConfigText)) {
            if ([string]::IsNullOrEmpty($record.NewVersion)) { continue }
            $examined++
            if ($handled.Contains($record.Name)) { continue }
            $handled.Add($record.Name)

            $deployed = @(Get-ProviderVersion -Provider $DeployedVersionProvider -Name $record.Name)
            if ($deployed.Count -eq 0) { $unverifiable.Add($record.Name); continue }
            if ($deployed -contains $record.NewVersion) { continue }

            $preferred = @(Get-ProviderVersion -Provider $PreferredVersionProvider -Name $record.Name)
            if ($preferred.Count -eq 1 -and $deployed -contains $preferred[0]) {
                $target = $preferred[0]
                $rule = 'OwnReference'
            }
            else {
                $target = ''
                $highest = $null
                $resolvable = $true
                foreach ($candidate in $deployed) {
                    $parsed = $null
                    if (-not [System.Version]::TryParse($candidate, [ref]$parsed)) { $resolvable = $false; break }
                    if ($null -eq $highest -or $parsed -gt $highest) { $highest = $parsed; $target = $candidate }
                }
                if (-not $resolvable) { $unresolvable.Add($record.Name); continue }
                $rule = 'HighestDeployed'
            }

            $text = (Invoke-BindingRedirectReconciliation -AppConfigText $text -AssemblyName $record.Name `
                    -AssemblyVersion $target).Text
            $repair.Add([pscustomobject]@{
                    PSTypeName   = 'BindingRedirectSync.Repair'
                    Kind         = 'BindingRedirectSync'
                    AssemblyName = $record.Name
                    From         = $record.NewVersion
                    To           = $target
                    Rule         = $rule
                })
        }
    }

    return [pscustomobject]@{
        PSTypeName    = 'BindingRedirectSync.Result'
        Text          = $text
        Repair        = $repair.ToArray()
        Unverifiable  = $unverifiable.ToArray()
        Unresolvable  = $unresolvable.ToArray()
        ExaminedCount = $examined
    }
}

function Invoke-SolutionBindingRedirectSync {
    <#
    .SYNOPSIS
        Synchronises the binding redirects of every application configuration in a tree with
        the Reference versions of every project file in it.
    .DESCRIPTION
        Discovery, reading and writing are supplied as delegates, as for
        Invoke-ManifestNormalization, so the pass runs in memory. The deployed-version map is
        built over every project file; a project text supplied in ProjectTextOverride is used in
        place of the text the reader returns, which lets a caller that has repaired a project in
        memory, or in a what-if run, synchronise against the repaired references. Only changed
        text is written, and only when ShouldProcess approves. Unverifiable and unresolvable
        names are reported in the result and are not failures.
    .PARAMETER DirectoryLister
        A delegate returning candidate paths.
    .PARAMETER TextReader
        A delegate taking a path and returning that file's text.
    .PARAMETER TextWriter
        A delegate taking a path and the replacement text.
    .PARAMETER ProjectTextOverride
        Project-file path to post-repair text. Empty by default.
    .OUTPUTS
        A BindingRedirectSync.SolutionResult carrying the changed paths, the repair records, the
        distinct unverifiable and unresolvable names and the examined application configuration
        count.
    #>
    [CmdletBinding(SupportsShouldProcess = $true)]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)][scriptblock]$DirectoryLister,
        [Parameter(Mandatory = $true)][scriptblock]$TextReader,
        [Parameter(Mandatory = $true)][scriptblock]$TextWriter,
        [ValidateNotNull()][hashtable]$ProjectTextOverride = @{}
    )

    # The listing is taken once and replayed, so a lister that records its own discovery is
    # invoked once per pass rather than once per consumer.
    $listing = @(& $DirectoryLister | Where-Object { $null -ne $_ } | ForEach-Object { [string]$_ })
    $replay = { $listing }.GetNewClosure()

    $projectPath = @($listing | Where-Object { Test-SyncProjectPath -Path $_ } | Sort-Object)
    $projectText = @{}
    foreach ($path in $projectPath) {
        $projectText[$path] = if ($ProjectTextOverride.ContainsKey($path)) {
            [string]$ProjectTextOverride[$path]
        }
        else {
            [string](& $TextReader $path)
        }
    }

    $map = ConvertTo-ReferenceVersionMap -ProjectText ([string[]]@($projectPath | ForEach-Object { $projectText[$_] }))
    $deployedProvider = { param($Name) $map[$Name] }.GetNewClosure()

    $changed = [System.Collections.Generic.List[string]]::new()
    $repair = [System.Collections.Generic.List[pscustomobject]]::new()
    $unverifiable = [System.Collections.Generic.List[string]]::new()
    $unresolvable = [System.Collections.Generic.List[string]]::new()
    $examined = 0

    foreach ($path in @(Get-PackageManifestPath -Kind 'AppConfig' -DirectoryLister $replay)) {
        $examined++
        $directory = Split-Path -Parent $path
        $sibling = @($projectPath | Where-Object { (Split-Path -Parent $_) -eq $directory })
        $ownMap = if ($sibling.Count -gt 0) {
            ConvertTo-ReferenceVersionMap -ProjectText ([string[]]@($sibling | ForEach-Object { $projectText[$_] }))
        }
        else {
            @{}
        }
        $preferredProvider = { param($Name) $ownMap[$Name] }.GetNewClosure()

        $original = [string](& $TextReader $path)
        $result = Invoke-BindingRedirectSync -AppConfigText $original -DeployedVersionProvider $deployedProvider `
            -PreferredVersionProvider $preferredProvider
        $projectDirectory = Split-Path -Leaf $directory
        foreach ($record in @($result.Repair)) {
            $record | Add-Member -NotePropertyName 'Path' -NotePropertyValue $path
            $record | Add-Member -NotePropertyName 'ProjectDirectory' -NotePropertyValue $projectDirectory
            $repair.Add($record)
        }
        foreach ($name in @($result.Unverifiable)) { $unverifiable.Add($name) }
        foreach ($name in @($result.Unresolvable)) { $unresolvable.Add($name) }

        if ($result.Text -cne $original -and $PSCmdlet.ShouldProcess($path, 'Synchronise binding redirects')) {
            & $TextWriter $path $result.Text
            $changed.Add($path)
        }
    }

    $distinctUnverifiable = [string[]]@($unverifiable | Sort-Object -Unique)
    $distinctUnresolvable = [string[]]@($unresolvable | Sort-Object -Unique)
    $summary = 'Binding redirect sync: examined {0} application configuration file(s), synchronised {1} redirect(s), unverifiable {2}, unresolvable {3}'
    Write-Information ($summary -f $examined, $repair.Count, $distinctUnverifiable.Count, $distinctUnresolvable.Count) `
        -InformationAction Continue

    return [pscustomobject]@{
        PSTypeName        = 'BindingRedirectSync.SolutionResult'
        ChangedPath       = $changed.ToArray()
        Repair            = $repair.ToArray()
        Unverifiable      = $distinctUnverifiable
        Unresolvable      = $distinctUnresolvable
        ExaminedAppConfig = $examined
    }
}

function Format-BindingRedirectSyncReport {
    <#
    .SYNOPSIS
        Renders the synchronised redirects as the disclosure block the pull-request body carries.
    .PARAMETER Repair
        The repair records Invoke-SolutionBindingRedirectSync returned.
    .OUTPUTS
        The block text, or an empty string when there are no repairs.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [object[]]$Repair
    )

    if (@($Repair).Count -eq 0) { return '' }
    $line = [System.Collections.Generic.List[string]]::new()
    $line.Add('## Binding redirects synchronised')
    foreach ($record in $Repair) {
        $line.Add(('- {0}: {1} {2} to {3} ({4})' -f $record.ProjectDirectory, $record.AssemblyName,
                $record.From, $record.To, $record.Rule))
    }
    return ($line -join [System.Environment]::NewLine)
}

Export-ModuleMember -Function @(
    'Invoke-BindingRedirectSync',
    'Invoke-SolutionBindingRedirectSync',
    'Format-BindingRedirectSyncReport'
)
