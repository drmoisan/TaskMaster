param(
    [Parameter(Mandatory = $false)]
    [string]$SearchRoot,

    [Parameter(Mandatory = $false)]
    [string]$Configuration,

    [Parameter(Mandatory = $false)]
    [string]$CoverageOutput = "coverage\coverage.cobertura.xml"
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

Write-Host "Using vstest.console: $vstestPath"
Write-Host "Discovered $($testAssemblies.Count) test assemblies."
Write-Host "Coverage output: $resolvedOutputPath"

# Resolve the coverage settings file (excludes third-party/F# assemblies to prevent
# instrumentation from breaking tests like those using Deedle/FSharp.Core).
$coverageConfig = Join-Path $repoRoot 'coverage.config'

# Pass -- to dotnet-coverage to signal the start of the test runner command and its arguments.
$dotnetCoverageArgs = @('collect', '--output', $resolvedOutputPath, '--output-format', 'cobertura', '--settings', $coverageConfig, '--', $vstestPath) + $testAssemblies + @('/InIsolation')
& dotnet-coverage @dotnetCoverageArgs
if ($LASTEXITCODE -ne 0) {
    throw "MSTest with coverage failed with exit code $LASTEXITCODE"
}

# Post-process the Cobertura XML for Koverage compatibility:
#   1. Rewrite absolute Windows paths to workspace-relative forward-slash paths.
#   2. Inject <sources><source>.</source></sources> (required by cobertura-parse).
#   3. Remove <package> elements for third-party assemblies that are not part
#      of the solution (dotnet-coverage instruments all loaded DLLs at runtime).
Write-Host "Post-processing coverage XML for Koverage compatibility..."

# Allowlist: only keep packages whose name matches a project in this solution.
$projectNames = @(
    'QuickFiler', 'QuickFiler.Test',
    'SVGControl', 'SVGControl.Test',
    'Tags', 'Tags.Test',
    'TaskMaster', 'TaskMaster.Test',
    'TaskTree',
    'TaskVisualization', 'TaskVisualization.Test',
    'TaskVisualizer',
    'ToDoModel', 'ToDoModel.Test',
    'UtilitiesCS', 'UtilitiesCS.Test',
    'UtilitiesSwordfish', 'UtilitiesSwordfish.Test',
    'VBFunctions', 'VBFunctions.Test'
)

[xml]$xml = Get-Content $resolvedOutputPath -Encoding UTF8

# Strip non-project packages.
$packagesNode = $xml.SelectSingleNode('//packages')
$removed = @()
foreach ($pkg in @($packagesNode.ChildNodes)) {
    if ($pkg.NodeType -eq 'Element' -and $pkg.name -and $pkg.name -notin $projectNames) {
        $removed += $pkg.name
        $packagesNode.RemoveChild($pkg) | Out-Null
    }
}
if ($removed.Count -gt 0) {
    Write-Host "Removed $($removed.Count) third-party packages: $($removed -join ', ')"
}

# Recompute root-level coverage attributes from remaining packages.
$totalLines = 0; $coveredLines = 0; $totalBranches = 0; $coveredBranches = 0
foreach ($pkg in $packagesNode.ChildNodes) {
    if ($pkg.NodeType -ne 'Element') { continue }
    foreach ($cls in $pkg.SelectNodes('.//class')) {
        foreach ($line in $cls.SelectNodes('.//lines/line')) {
            $totalLines++
            if ([int]$line.hits -gt 0) { $coveredLines++ }
            if ($line.branch -eq 'True') {
                # Parse condition-coverage attribute like "50% (1/2)"
                if ($line.HasAttribute('condition-coverage') -and $line.'condition-coverage' -match '\(([0-9]+)/([0-9]+)\)') {
                    $coveredBranches += [int]$Matches[1]
                    $totalBranches += [int]$Matches[2]
                }
            }
        }
    }
}
$xml.coverage.'line-rate' = if ($totalLines -gt 0) { [string]([math]::Round($coveredLines / $totalLines, 6)) } else { '0' }
$xml.coverage.'branch-rate' = if ($totalBranches -gt 0) { [string]([math]::Round($coveredBranches / $totalBranches, 6)) } else { '0' }
$xml.coverage.'lines-covered' = [string]$coveredLines
$xml.coverage.'lines-valid' = [string]$totalLines
$xml.coverage.'branches-covered' = [string]$coveredBranches
$xml.coverage.'branches-valid' = [string]$totalBranches

# Save as text so we can do string-level path fixups.
$sw = [System.IO.StringWriter]::new()
$xw = [System.Xml.XmlTextWriter]::new($sw)
$xw.Formatting = [System.Xml.Formatting]::Indented
$xml.WriteTo($xw)
$xw.Flush()
$xmlContent = $sw.ToString()

# Rewrite absolute paths to workspace-relative forward-slash paths.
$repoRootPrefix = $repoRoot.TrimEnd('\') + '\'
$xmlContent = $xmlContent.Replace($repoRootPrefix, '').Replace('\', '/')

# Inject <sources> if missing (required by cobertura-parse in Koverage).
if ($xmlContent -notmatch '<sources>') {
    $xmlContent = $xmlContent.Replace('<packages>', "<sources>`n    <source>.</source>`n  </sources>`n  <packages>")
}

Set-Content -Path $resolvedOutputPath -Value $xmlContent -Encoding UTF8 -NoNewline
Write-Host "Done. Coverage artifact: $resolvedOutputPath"
