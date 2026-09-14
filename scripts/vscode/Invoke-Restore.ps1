param(
    [Parameter(Mandatory = $false)]
    [string]$SolutionPath = 'TaskMaster.sln',

    [Parameter(Mandatory = $false)]
    [string]$Configuration = 'Debug',

    [Parameter(Mandatory = $false)]
    [string]$Platform = 'Any CPU'
)

function Get-RestoreMSBuildPath {
    <#
    .SYNOPSIS
        Resolves the MSBuild.exe path through vswhere.
    .DESCRIPTION
        Wrapper seam around the vswhere lookup. The name deliberately differs from the
        Get-MSBuildPath seam in Invoke-VSBuild.ps1: Pester runs every test container in one
        runspace, so two same-named functions dot-sourced from two production files shadow each
        other and the mock registered for one file would silently govern the other.

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

function Invoke-RestoreMSBuildExe {
    <#
    .SYNOPSIS
        Wrapper seam that splats the argument list into MSBuild.exe.
    .DESCRIPTION
        Single array parameter (MSBuildArgs, not Args) splatted into the resolved executable.
        The name deliberately differs from the Invoke-MSBuildExe seam in Invoke-VSBuild.ps1, for
        the single-runspace shadowing reason stated on Get-RestoreMSBuildPath above. The
        executable is supplied by the caller rather than hard-coded, and no check is made that it
        names a file on disk, so a test can drive this body against an in-process command.
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

function Invoke-RestoreMain {
    <#
    .SYNOPSIS
        Resolves MSBuild and restores the solution's packages.
    .DESCRIPTION
        Host-neutral entry-point body. Every external dependency is reached through a named seam
        (Get-RestoreMSBuildPath, Invoke-RestoreMSBuildExe), so the guards, messages, and ordering
        below are exercisable by Pester without a live Visual Studio installation. The top-level
        wiring at the bottom of this file forwards the script parameters here and does nothing
        else, per the Coverage Exclusion Policy in .claude/rules/general-unit-test.md, which
        requires logic to live in testable units rather than in an untestable host-bound script
        body.

        Guard order and guard messages are unchanged from the pre-extraction body, and every
        existing caller invokes this file with -File, for which $MyInvocation.InvocationName is
        the script path and never '.', so behaviour is preserved for all of them.
    #>
    param(
        [string]$SolutionPath = 'TaskMaster.sln',
        [string]$Configuration = 'Debug',
        [string]$Platform = 'Any CPU',
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

    $msbuildPath = Get-RestoreMSBuildPath -VsWherePath $vswherePath
    if (-not $msbuildPath) {
        throw 'MSBuild.exe not found via vswhere. Install Visual Studio MSBuild components.'
    }

    Write-Host "Using MSBuild: $msbuildPath"

    # Restore using MSBuild with RestorePackagesConfig=true to handle
    # both SDK-style (PackageReference) and legacy (packages.config) projects.
    $restoreArguments = @(
        $resolvedSolutionPath,
        '/t:Restore',
        "/p:Configuration=$Configuration",
        "/p:Platform=$Platform",
        '/p:RestorePackagesConfig=true',
        '/m'
    )

    Invoke-RestoreMSBuildExe -MSBuildPath $msbuildPath -MSBuildArgs $restoreArguments
    if ($LASTEXITCODE -ne 0) {
        throw "MSBuild Restore failed with exit code $LASTEXITCODE"
    }
}

if ($MyInvocation.InvocationName -ne '.') {
    Invoke-RestoreMain @PSBoundParameters
}
