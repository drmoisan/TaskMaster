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

# Sync csproj HintPaths with packages.config versions before building.
# This resolves mismatches created when NuGet updates packages.config in VS
# but the csproj reference paths are not persisted to disk.
$syncScript = Join-Path $PSScriptRoot 'Sync-PackageReferences.ps1'
if (Test-Path $syncScript) {
    & $syncScript -SolutionRoot $repoRoot
}

$requestedMSBuildProperties = Get-RequestedMSBuildProperties -MSBuildProperty $MSBuildProperty -EnableNETAnalyzers:$EnableNETAnalyzers -EnforceCodeStyleInBuild:$EnforceCodeStyleInBuild -EnableNullable:$EnableNullable -TreatWarningsAsErrors:$TreatWarningsAsErrors
$msbuildArguments = Get-MSBuildBuildArguments -ResolvedSolutionPath $resolvedSolutionPath -Configuration $Configuration -Platform $Platform -Target $Target -MSBuildProperty $requestedMSBuildProperties

if ($NoExecute) {
    return
}

& $msbuildPath @msbuildArguments
if ($LASTEXITCODE -ne 0) {
    throw "MSBuild failed with exit code $LASTEXITCODE"
}
