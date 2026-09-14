param(
    [Parameter(Mandatory = $false)]
    [string]$SolutionPath = 'TaskMaster.sln',

    [Parameter(Mandatory = $false)]
    [string]$Configuration = 'Debug',

    [Parameter(Mandatory = $false)]
    [string]$Platform = 'Any CPU',

    [Parameter(Mandatory = $false)]
    [ValidateSet('Build', 'Rebuild')]
    [string]$Target = 'Build',

    [Parameter(Mandatory = $false)]
    [string[]]$MSBuildProperty = @(),

    [Parameter(Mandatory = $false)]
    [switch]$EnableNETAnalyzers,

    [Parameter(Mandatory = $false)]
    [switch]$EnforceCodeStyleInBuild,

    # Deprecated and no-op. Retained so existing callers still bind. See CLAUDE.md C#1 item 3.
    [Parameter(Mandatory = $false)]
    [switch]$EnableNullable,

    [Parameter(Mandatory = $false)]
    [switch]$TreatWarningsAsErrors,

    [Parameter(Mandatory = $false)]
    [switch]$NoExecute
)

function ConvertTo-MSBuildPropertyArgument {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Property
    )

    if ([string]::IsNullOrWhiteSpace($Property)) {
        throw 'MSBuildProperty entries must not be empty.'
    }

    if ($Property.StartsWith('/p:')) {
        return $Property
    }

    return "/p:$Property"
}

function Get-MSBuildBuildArguments {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ResolvedSolutionPath,

        [Parameter(Mandatory = $true)]
        [string]$Configuration,

        [Parameter(Mandatory = $true)]
        [string]$Platform,

        [Parameter(Mandatory = $false)]
        [ValidateSet('Build', 'Rebuild')]
        [string]$Target = 'Build',

        [Parameter(Mandatory = $false)]
        [string[]]$MSBuildProperty = @()
    )

    $arguments = @(
        $ResolvedSolutionPath,
        "/t:$Target",
        "/p:Configuration=$Configuration",
        "/p:Platform=$Platform"
    )

    foreach ($property in $MSBuildProperty) {
        $arguments += ConvertTo-MSBuildPropertyArgument -Property $property
    }

    $arguments += '/m'

    return $arguments
}

function Get-RequestedMSBuildProperties {
    param(
        [Parameter(Mandatory = $false)]
        [string[]]$MSBuildProperty = @(),

        [Parameter(Mandatory = $false)]
        [switch]$EnableNETAnalyzers,

        [Parameter(Mandatory = $false)]
        [switch]$EnforceCodeStyleInBuild,

        # Deprecated and no-op. Retained so existing callers still bind. See CLAUDE.md C#1 item 3.
        [Parameter(Mandatory = $false)]
        [switch]$EnableNullable,

        [Parameter(Mandatory = $false)]
        [switch]$TreatWarningsAsErrors
    )

    $properties = @($MSBuildProperty)

    if ($EnableNETAnalyzers) {
        $properties += 'EnableNETAnalyzers=true'
    }

    if ($EnforceCodeStyleInBuild) {
        $properties += 'EnforceCodeStyleInBuild=true'
    }

    if ($EnableNullable) {
        Write-Warning 'The -EnableNullable switch is deprecated and has no effect. This repository enforces nullability per file via #nullable enable; /p:Nullable=enable is deliberately absent from CI and makes the gate unpassable. See CLAUDE.md C#1 item 3.'
    }

    if ($TreatWarningsAsErrors) {
        $properties += 'TreatWarningsAsErrors=true'
    }

    return $properties
}

function Get-MSBuildPath {
    <#
    .SYNOPSIS
        Resolves the MSBuild.exe path through vswhere.
    .DESCRIPTION
        Wrapper seam around the vswhere lookup, in the same style as the Get-VsTestConsolePath
        seam in Invoke-MSTest.ps1. The external-process invocation is confined to this one
        function so Invoke-VSBuildMain can be exercised by Pester without launching vswhere.exe.
        The executable is supplied by the caller rather than hard-coded, and no check is made
        that it names a file on disk, so a test can drive this body against an in-process
        command. Returns the first match, or nothing when vswhere reports no MSBuild component.
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$VsWherePath
    )

    return & $VsWherePath -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' |
        Select-Object -First 1
}

function Invoke-SyncPackageReferences {
    <#
    .SYNOPSIS
        Wrapper seam that runs the package-reference sync script against a solution root.
    .DESCRIPTION
        The sibling-script invocation is confined to this one function so Invoke-VSBuildMain can
        be exercised by Pester without executing Sync-PackageReferences.ps1 against the real
        repository, which writes .csproj files when a HintPath does not resolve and is the source
        of the non-deterministic coverage measurement issue #869 reports.
    #>
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseSingularNouns', '', Justification = 'The seam name is fixed by specification decision D3 of issue #869, which names the seam after the Sync-PackageReferences.ps1 script it wraps.')]
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$SyncScriptPath,

        [Parameter(Mandatory = $true)]
        [string]$SolutionRoot
    )

    & $SyncScriptPath -SolutionRoot $SolutionRoot
}

function Invoke-MSBuildExe {
    <#
    .SYNOPSIS
        Wrapper seam that splats the argument list into MSBuild.exe.
    .DESCRIPTION
        Single array parameter (MSBuildArgs, not Args) splatted into the resolved executable.
        This is the mockable seam used by Pester tests so the argument list can be asserted
        without launching the external executable. The executable is supplied by the caller
        rather than hard-coded, and no check is made that it names a file on disk, so a test can
        drive this body against an in-process command.
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$MSBuildPath,

        [Parameter(Mandatory = $true)]
        [string[]]$MSBuildArgs
    )

    & $MSBuildPath @MSBuildArgs
}

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Invoke-VSBuildMain {
    <#
    .SYNOPSIS
        Resolves MSBuild, syncs package references, and builds the solution.
    .DESCRIPTION
        Host-neutral entry-point body. Every external dependency is reached through a named seam
        (Get-MSBuildPath, Invoke-SyncPackageReferences, Invoke-MSBuildExe), so the guards,
        messages, and ordering below are exercisable by Pester without a live Visual Studio
        installation and without executing any sibling script. The top-level wiring at the bottom
        of this file forwards the script parameters here and does nothing else, per the Coverage
        Exclusion Policy in .claude/rules/general-unit-test.md, which requires logic to live in
        testable units rather than in an untestable host-bound script body.

        Guard order and guard messages are unchanged from the pre-extraction body, and every
        existing caller invokes this file with -File, for which $MyInvocation.InvocationName is
        the script path and never '.', so behaviour is preserved for all of them.
    #>
    param(
        [string]$SolutionPath = 'TaskMaster.sln',
        [string]$Configuration = 'Debug',
        [string]$Platform = 'Any CPU',
        [ValidateSet('Build', 'Rebuild')]
        [string]$Target = 'Build',
        [string[]]$MSBuildProperty = @(),
        [switch]$EnableNETAnalyzers,
        [switch]$EnforceCodeStyleInBuild,
        [switch]$EnableNullable,
        [switch]$TreatWarningsAsErrors,
        [switch]$NoExecute,
        [string]$ScriptRoot = $PSScriptRoot
    )

    $repoRoot = (Resolve-Path (Join-Path $ScriptRoot '..\..')).Path
    $resolvedSolutionPath = Join-Path $repoRoot $SolutionPath

    if (-not (Test-Path $resolvedSolutionPath)) {
        throw "Solution not found: $resolvedSolutionPath"
    }

    $vswherePath = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (-not (Test-Path $vswherePath)) {
        throw 'vswhere.exe was not found. Install Visual Studio 2022 (or Build Tools) with MSBuild components.'
    }

    $msbuildPath = Get-MSBuildPath -VsWherePath $vswherePath
    if (-not $msbuildPath) {
        throw 'MSBuild.exe not found via vswhere. Install Visual Studio MSBuild components.'
    }

    Write-Host "Using MSBuild: $msbuildPath"

    # Sync csproj HintPaths with packages.config versions before building.
    # This resolves mismatches created when NuGet updates packages.config in VS
    # but the csproj reference paths are not persisted to disk.
    $syncScript = Join-Path $ScriptRoot 'Sync-PackageReferences.ps1'
    if (Test-Path $syncScript) {
        Invoke-SyncPackageReferences -SyncScriptPath $syncScript -SolutionRoot $repoRoot
    }

    $requestedMSBuildProperties = Get-RequestedMSBuildProperties -MSBuildProperty $MSBuildProperty -EnableNETAnalyzers:$EnableNETAnalyzers -EnforceCodeStyleInBuild:$EnforceCodeStyleInBuild -EnableNullable:$EnableNullable -TreatWarningsAsErrors:$TreatWarningsAsErrors
    $msbuildArguments = Get-MSBuildBuildArguments -ResolvedSolutionPath $resolvedSolutionPath -Configuration $Configuration -Platform $Platform -Target $Target -MSBuildProperty $requestedMSBuildProperties

    if ($NoExecute) {
        return
    }

    Invoke-MSBuildExe -MSBuildPath $msbuildPath -MSBuildArgs $msbuildArguments
    if ($LASTEXITCODE -ne 0) {
        throw "MSBuild failed with exit code $LASTEXITCODE"
    }
}

if ($MyInvocation.InvocationName -ne '.') {
    Invoke-VSBuildMain @PSBoundParameters
}
