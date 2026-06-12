param(
    [Parameter(Mandatory = $false)]
    [string]$SearchRoot,

    [Parameter(Mandatory = $false)]
    [string]$Configuration,

    [Parameter(Mandatory = $false)]
    [string]$CoverageOutput = "coverage\coverage.cobertura.xml",

    [Parameter(Mandatory = $false)]
    [switch]$NoExecute
)

function Resolve-RunSettingsPath {
    <#
    .SYNOPSIS
        Resolves the repo-root TaskMaster.runsettings path and fails fast if absent.
    .DESCRIPTION
        The runsettings path is resolved deterministically from the repository root so
        VS Code coverage runs apply the same MSTest parallelization that Visual Studio
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

function Get-DotnetCoverageArgumentList {
    <#
    .SYNOPSIS
        Builds the dotnet-coverage argument list, including the inner vstest /Settings:.
    .DESCRIPTION
        Returns the full argument array for dotnet-coverage collect. The outer
        --settings <coverage.config> (instrumentation excludes) is preserved and remains
        distinct from the inner vstest /Settings:<TaskMaster.runsettings> applied after
        the -- separator and the vstest executable path. Pure function; no I/O or execution.
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$OutputPath,

        [Parameter(Mandatory = $true)]
        [string]$CoverageConfig,

        [Parameter(Mandatory = $true)]
        [string]$VsTestPath,

        [Parameter(Mandatory = $true)]
        [string[]]$TestAssembly,

        [Parameter(Mandatory = $true)]
        [string]$RunSettingsPath
    )

    # The outer dotnet-coverage --settings is the instrumentation-exclude file
    # (coverage.config); the inner vstest /Settings: is the MSTest runsettings.
    return @(
        'collect',
        '--output', $OutputPath,
        '--output-format', 'cobertura',
        '--settings', $CoverageConfig,
        '--', $VsTestPath
    ) + @($TestAssembly) + @("/Settings:$RunSettingsPath", '/InIsolation')
}

function Invoke-DotnetCoverageExe {
    <#
    .SYNOPSIS
        Wrapper seam that splats the argument list into dotnet-coverage.
    .DESCRIPTION
        Single array parameter (DotnetCoverageArgs, not Args) splatted into the
        dotnet-coverage executable. This is the mockable seam used by Pester tests so
        the constructed argument list can be asserted without launching the executable
        or the inner vstest.console.exe.
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$DotnetCoverageArgs
    )

    & dotnet-coverage @DotnetCoverageArgs
}

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'Invoke-MSTestWithCoverage.Helpers.ps1')

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

if (-not (Get-Command 'dotnet-coverage' -ErrorAction SilentlyContinue)) {
    throw "dotnet-coverage not found. Install it with: dotnet tool install --global dotnet-coverage"
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

$resolvedOutputPath = Join-Path $repoRoot $CoverageOutput
$outputDir = Split-Path $resolvedOutputPath -Parent
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir | Out-Null
}

Write-Output "Using vstest.console: $vstestPath"
Write-Output "Discovered $($testAssemblies.Count) test assemblies."
Write-Output "Coverage output: $resolvedOutputPath"

# Resolve the coverage settings file (excludes third-party/F# assemblies to prevent
# instrumentation from breaking tests like those using Deedle/FSharp.Core).
$coverageConfig = Join-Path $repoRoot 'coverage.config'

# Pass -- to dotnet-coverage to signal the start of the test runner command and its arguments.
$dotnetCoverageArgs = Get-DotnetCoverageArgumentList `
    -OutputPath $resolvedOutputPath `
    -CoverageConfig $coverageConfig `
    -VsTestPath $vstestPath `
    -TestAssembly $testAssemblies `
    -RunSettingsPath $runSettingsPath

if ($NoExecute) {
    return
}

Invoke-DotnetCoverageExe -DotnetCoverageArgs $dotnetCoverageArgs
if ($LASTEXITCODE -ne 0) {
    throw "MSTest with coverage failed with exit code $LASTEXITCODE"
}

# Post-process the Cobertura XML for Koverage compatibility:
#   1. Rewrite absolute paths to workspace-relative paths using native separators.
#   2. Inject <sources><source>.</source></sources> (required by cobertura-parse).
#   3. Remove <package> elements for third-party assemblies that are not part
#      of the solution (dotnet-coverage instruments all loaded DLLs at runtime).
Write-Output "Post-processing coverage XML for Koverage compatibility..."
$xmlContent = Get-Content $resolvedOutputPath -Raw -Encoding UTF8
$processedXmlContent = ConvertTo-KoverageCoberturaXml -XmlContent $xmlContent -RepoRoot $repoRoot

Set-Content -Path $resolvedOutputPath -Value $processedXmlContent -Encoding UTF8 -NoNewline
Write-Output "Done. Coverage artifact: $resolvedOutputPath"
