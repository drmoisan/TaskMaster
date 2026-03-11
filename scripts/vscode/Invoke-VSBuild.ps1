param(
    [Parameter(Mandatory = $false)]
    [string]$SolutionPath = 'TaskMaster.sln',

    [Parameter(Mandatory = $false)]
    [string]$Configuration = 'Debug',

    [Parameter(Mandatory = $false)]
    [string]$Platform = 'Any CPU'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$resolvedSolutionPath = Join-Path $repoRoot $SolutionPath

if (-not (Test-Path $resolvedSolutionPath)) {
    throw "Solution not found: $resolvedSolutionPath"
}

$vswherePath = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (-not (Test-Path $vswherePath)) {
    throw 'vswhere.exe was not found. Install Visual Studio 2022 (or Build Tools) with MSBuild components.'
}

$msbuildPath = & $vswherePath -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if (-not $msbuildPath) {
    throw 'MSBuild.exe not found via vswhere. Install Visual Studio MSBuild components.'
}

Write-Host "Using MSBuild: $msbuildPath"

& $msbuildPath $resolvedSolutionPath /t:Build "/p:Configuration=$Configuration" "/p:Platform=$Platform" /m
if ($LASTEXITCODE -ne 0) {
    throw "MSBuild failed with exit code $LASTEXITCODE"
}
