param(
    [Parameter(Mandatory = $false)]
    [string]$SearchRoot,

    [Parameter(Mandatory = $false)]
    [string]$Configuration
)

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

& $vstestPath $testAssemblies /InIsolation
if ($LASTEXITCODE -ne 0) {
    throw "MSTest execution failed with exit code $LASTEXITCODE"
}
