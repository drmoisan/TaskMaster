param(
    [Parameter(Mandatory = $false)]
    [string]$SearchRoot,

    [Parameter(Mandatory = $false)]
    [string]$Configuration,

    [Parameter(Mandatory = $false)]
    [switch]$NoExecute
)

function Resolve-RunSettingsPath {
    <#
    .SYNOPSIS
        Resolves the repo-root TaskMaster.runsettings path and fails fast if absent.
    .DESCRIPTION
        The runsettings path is resolved deterministically from the repository root so
        VS Code test runs apply the same MSTest parallelization that Visual Studio
        auto-detects. A clear, specific error is thrown when the file is missing.
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepoRoot
    )

    $runSettingsPath = Join-Path $RepoRoot 'TaskMaster.runsettings'
    if (-not (Test-Path $runSettingsPath)) {
        throw "Runsettings file not found: $runSettingsPath"
    }

    return $runSettingsPath
}

function Get-VsTestArgumentList {
    <#
    .SYNOPSIS
        Builds the vstest.console.exe argument list including the /Settings: runsettings.
    .DESCRIPTION
        Returns the full argument array passed to vstest.console.exe: the discovered
        test assemblies, the /Settings: argument pointing at the repo-root
        TaskMaster.runsettings, and /InIsolation. Pure function; no I/O or execution.
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$TestAssembly,

        [Parameter(Mandatory = $true)]
        [string]$RunSettingsPath
    )

    return @($TestAssembly) + @("/Settings:$RunSettingsPath", '/InIsolation')
}

function Invoke-VsTestExe {
    <#
    .SYNOPSIS
        Wrapper seam that splats the argument list into vstest.console.exe.
    .DESCRIPTION
        Single array parameter (VsTestArgs, not Args) splatted into the resolved
        executable. This is the mockable seam used by Pester tests so the argument
        list can be asserted without launching the external executable.
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$VsTestPath,

        [Parameter(Mandatory = $true)]
        [string[]]$VsTestArgs
    )

    & $VsTestPath @VsTestArgs
}

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($SearchRoot)) {
    $SearchRoot = '.'
}

if ([string]::IsNullOrWhiteSpace($Configuration)) {
    $Configuration = 'Debug'
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$resolvedSearchRoot = Join-Path $repoRoot $SearchRoot

if (-not (Test-Path $resolvedSearchRoot)) {
    throw "Search root not found: $resolvedSearchRoot"
}

$runSettingsPath = Resolve-RunSettingsPath -RepoRoot $repoRoot

$vswherePath = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (-not (Test-Path $vswherePath)) {
    throw 'vswhere.exe was not found. Install Visual Studio 2022 (or Build Tools) with Test Platform components.'
}

$vstestPath = & $vswherePath -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
if (-not $vstestPath) {
    throw 'vstest.console.exe not found via vswhere. Install Visual Studio Test Platform components.'
}

$testAssemblies = Get-ChildItem -Path $resolvedSearchRoot -Recurse -Filter '*.Test.dll' |
    Where-Object {
        $_.FullName -match "\\bin\\$Configuration\\" -and
        $_.FullName -notmatch '\\obj\\' -and
        $_.FullName -notmatch '\\ref\\'
    } |
        Select-Object -ExpandProperty FullName

if (-not $testAssemblies -or $testAssemblies.Count -eq 0) {
    throw "No test assemblies found under '$resolvedSearchRoot' for configuration '$Configuration'. Build first."
}

Write-Host "Using vstest.console: $vstestPath"
Write-Host "Discovered $($testAssemblies.Count) test assemblies."

$vsTestArguments = Get-VsTestArgumentList -TestAssembly $testAssemblies -RunSettingsPath $runSettingsPath

if ($NoExecute) {
    return
}

Invoke-VsTestExe -VsTestPath $vstestPath -VsTestArgs $vsTestArguments
if ($LASTEXITCODE -ne 0) {
    throw "MSTest execution failed with exit code $LASTEXITCODE"
}
