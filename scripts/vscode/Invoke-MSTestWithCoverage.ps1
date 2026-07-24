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
        Resolves the off-root CLI runsettings path and fails fast if absent.
    .DESCRIPTION
        The CLI runsettings (TaskMaster.cli.runsettings) lives alongside this script in
        scripts/vscode and is resolved deterministically from the script directory. It
        carries the MSTest parallelization only and no coverage data collector, so the
        inner vstest invocation never activates the Code Coverage collector; instrumentation
        comes solely from the outer dotnet-coverage --settings coverage.config path. Visual
        Studio continues to auto-detect the separate repo-root TaskMaster.runsettings (which
        carries the coverage exclusions). A clear, specific error is thrown when the file is missing.
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$ScriptRoot
    )

    $runSettingsPath = Join-Path $ScriptRoot 'TaskMaster.cli.runsettings'
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
        --settings path carries the effective instrumentation exclusions and remains
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

    # The outer dotnet-coverage --settings is the effective instrumentation-exclude
    # file; the inner vstest /Settings: is the MSTest runsettings.
    return @(
        'collect',
        '--output', $OutputPath,
        '--output-format', 'cobertura',
        '--settings', $CoverageConfig,
        '--', $VsTestPath
    ) + @($TestAssembly) + @("/Settings:$RunSettingsPath", '/InIsolation', '/TestCaseFilter:TestCategory!=LiveOutlook')
}

function ConvertTo-DerivedCoverageSettingsXml {
    <#
    .SYNOPSIS
        Adds the test-assembly instrumentation exclusion to coverage settings.
    .DESCRIPTION
        Parses canonical dotnet-coverage settings in memory, retains every
        existing module exclusion, and returns XML containing exactly one
        test-assembly exclusion. The canonical settings file is never written.
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$CanonicalSettingsXml
    )

    [xml]$settings = $CanonicalSettingsXml
    $excludeNode = $settings.SelectSingleNode('/Configuration/CodeCoverage/ModulePaths/Exclude')
    if ($null -eq $excludeNode) {
        throw 'Coverage settings do not contain Configuration/CodeCoverage/ModulePaths/Exclude.'
    }

    $testAssemblyPattern = '.*\.Test\.dll$'
    $existingTestExclusions = @(
        $excludeNode.SelectNodes('ModulePath') |
            Where-Object { $_.InnerText -ceq $testAssemblyPattern }
    )

    if ($existingTestExclusions.Count -gt 1) {
        throw "Coverage settings contain the test-assembly exclusion more than once: $testAssemblyPattern"
    }

    if ($existingTestExclusions.Count -eq 0) {
        $testAssemblyExclusion = $settings.CreateElement('ModulePath')
        $testAssemblyExclusion.InnerText = $testAssemblyPattern
        $null = $excludeNode.AppendChild($testAssemblyExclusion)
    }

    return $settings.OuterXml
}

function Get-DerivedCoverageSettingsPath {
    <#
    .SYNOPSIS
        Returns the effective settings path associated with a coverage output.
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$OutputPath
    )

    $resolvedOutputPath = [IO.Path]::GetFullPath($OutputPath)
    $outputDirectory = [IO.Path]::GetDirectoryName($resolvedOutputPath)
    if ([string]::IsNullOrWhiteSpace($outputDirectory)) {
        throw "Coverage output must have a parent directory: $OutputPath"
    }

    $outputName = [IO.Path]::GetFileName($resolvedOutputPath)
    return Join-Path $outputDirectory "$outputName.effective-coverage.config"
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

function Invoke-DotnetCoverageCollection {
    <#
    .SYNOPSIS
        Runs coverage with an output-adjacent effective settings file.
    .DESCRIPTION
        Reads the canonical settings without modifying them, writes one derived
        settings file beside the requested Cobertura output, and removes only
        that verified derived path in a finally block.
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

    $derivedSettingsPath = Get-DerivedCoverageSettingsPath -OutputPath $OutputPath
    $canonicalFullPath = [IO.Path]::GetFullPath($CoverageConfig)
    $derivedFullPath = [IO.Path]::GetFullPath($derivedSettingsPath)
    $outputDirectory = [IO.Path]::GetDirectoryName([IO.Path]::GetFullPath($OutputPath))
    $derivedDirectory = [IO.Path]::GetDirectoryName($derivedFullPath)

    if (-not [string]::Equals($outputDirectory, $derivedDirectory, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Derived coverage settings must be adjacent to the requested output: $derivedFullPath"
    }

    if ([string]::Equals($canonicalFullPath, $derivedFullPath, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Derived coverage settings path must differ from the canonical settings path.'
    }

    $shouldRemoveDerivedSettings = $false
    try {
        $canonicalSettingsXml = Get-Content -LiteralPath $canonicalFullPath -Raw -Encoding UTF8
        $derivedSettingsXml = ConvertTo-DerivedCoverageSettingsXml `
            -CanonicalSettingsXml $canonicalSettingsXml

        $shouldRemoveDerivedSettings = $true
        Set-Content `
            -LiteralPath $derivedFullPath `
            -Value $derivedSettingsXml `
            -Encoding UTF8 `
            -NoNewline

        $dotnetCoverageArgs = Get-DotnetCoverageArgumentList `
            -OutputPath $OutputPath `
            -CoverageConfig $derivedFullPath `
            -VsTestPath $VsTestPath `
            -TestAssembly $TestAssembly `
            -RunSettingsPath $RunSettingsPath

        $global:LASTEXITCODE = 0
        Invoke-DotnetCoverageExe -DotnetCoverageArgs $dotnetCoverageArgs
        $coverageExitCode = [int]$LASTEXITCODE
        if ($coverageExitCode -ne 0) {
            throw "MSTest with coverage failed with exit code $coverageExitCode"
        }
    } finally {
        if ($shouldRemoveDerivedSettings) {
            Remove-Item -LiteralPath $derivedFullPath -Force -ErrorAction SilentlyContinue
        }
    }
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

$runSettingsPath = Resolve-RunSettingsPath -ScriptRoot $PSScriptRoot

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

$testAssemblies = @(Get-ChildItem -Path $resolvedSearchRoot -Recurse -Filter '*.Test.dll' |
        Where-Object {
            $_.FullName -match "\\bin\\$Configuration\\" -and
            $_.FullName -notmatch '\\obj\\' -and
            $_.FullName -notmatch '\\ref\\'
        } |
            Select-Object -ExpandProperty FullName)

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

if ($NoExecute) {
    return
}

Invoke-DotnetCoverageCollection `
    -OutputPath $resolvedOutputPath `
    -CoverageConfig $coverageConfig `
    -VsTestPath $vstestPath `
    -TestAssembly $testAssemblies `
    -RunSettingsPath $runSettingsPath

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
