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
        Resolves the off-root CLI runsettings path and fails fast if absent.
    .DESCRIPTION
        The CLI runsettings (TaskMaster.cli.runsettings) lives alongside this script in
        scripts/vscode and is resolved deterministically from the script directory. It
        carries the MSTest parallelization only and no coverage data collector, so a plain
        CLI run never activates coverage. Visual Studio continues to auto-detect the
        separate repo-root TaskMaster.runsettings (which carries the coverage exclusions).
        A clear, specific error is thrown when the file is missing.
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

    return @($TestAssembly) + @("/Settings:$RunSettingsPath", '/InIsolation', '/TestCaseFilter:TestCategory!=LiveOutlook')
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

function Get-VsTestConsolePath {
    <#
    .SYNOPSIS
        Resolves the vstest.console.exe path through vswhere.
    .DESCRIPTION
        Wrapper seam around the vswhere lookup, in the same style as the Invoke-VsTestExe
        seam above and the Invoke-VsWhereExe seam in Invoke-MSTestWithCoverage.ps1. The
        external-process invocation is confined to this one function so Invoke-MSTestMain
        can be exercised by Pester without launching vswhere.exe. Returns the first match,
        or nothing when vswhere reports no Test Platform component.
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$VsWherePath
    )

    return & $VsWherePath -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' |
        Select-Object -First 1
}

function Get-MSTestAssemblyPathList {
    <#
    .SYNOPSIS
        Discovers the built test assembly paths beneath a search root for one configuration.
    .DESCRIPTION
        Returns the discovery pipeline wrapped in @(...), so the result is an array at every
        cardinality. Left unwrapped, a zero-match run yields $null and a single-match run yields
        a bare string, and every downstream array member access on those shapes is unsafe under
        Set-StrictMode -Version Latest (issue #733 finding 7). This mirrors the equivalent,
        already-wrapped discovery block in Invoke-MSTestWithCoverage.ps1, whose @(...) sits at an
        assignment site. A function return enumerates its output, which would unwrap the array
        again, so the unary comma below is what delivers the same array shape to the caller.
    #>
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [string]$SearchRoot,

        [Parameter(Mandatory = $true)]
        [string]$Configuration
    )

    return , @(Get-ChildItem -Path $SearchRoot -Recurse -Filter '*.Test.dll' |
            Where-Object {
                $_.FullName -match "\\bin\\$Configuration\\" -and
                $_.FullName -notmatch '\\obj\\' -and
                $_.FullName -notmatch '\\ref\\'
            } |
                Select-Object -ExpandProperty FullName)
}

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Invoke-MSTestMain {
    <#
    .SYNOPSIS
        Resolves the toolchain, discovers test assemblies, and runs vstest.console.exe.
    .DESCRIPTION
        Host-neutral entry-point body. Every external dependency is reached through a
        named seam (Resolve-RunSettingsPath, Get-VsTestConsolePath, Get-MSTestAssemblyPathList,
        Invoke-VsTestExe), so the guards, messages, and ordering below are exercisable by
        Pester without a live Visual Studio installation. The top-level wiring at the bottom
        of this file forwards the script parameters here and does nothing else, per the
        Coverage Exclusion Policy in .claude/rules/general-unit-test.md, which requires logic
        to live in testable units rather than in an untestable host-bound script body.
    #>
    param(
        [string]$SearchRoot,
        [string]$Configuration,
        [switch]$NoExecute,
        [string]$ScriptRoot = $PSScriptRoot
    )

    if ([string]::IsNullOrWhiteSpace($SearchRoot)) {
        $SearchRoot = '.'
    }

    if ([string]::IsNullOrWhiteSpace($Configuration)) {
        $Configuration = 'Debug'
    }

    $repoRoot = (Resolve-Path (Join-Path $ScriptRoot '..\..')).Path
    $resolvedSearchRoot = Join-Path $repoRoot $SearchRoot

    if (-not (Test-Path $resolvedSearchRoot)) {
        throw "Search root not found: $resolvedSearchRoot"
    }

    $runSettingsPath = Resolve-RunSettingsPath -ScriptRoot $ScriptRoot

    $vswherePath = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (-not (Test-Path $vswherePath)) {
        throw 'vswhere.exe was not found. Install Visual Studio 2022 (or Build Tools) with Test Platform components.'
    }

    $vstestPath = Get-VsTestConsolePath -VsWherePath $vswherePath
    if (-not $vstestPath) {
        throw 'vstest.console.exe not found via vswhere. Install Visual Studio Test Platform components.'
    }

    $testAssemblies = Get-MSTestAssemblyPathList -SearchRoot $resolvedSearchRoot -Configuration $Configuration

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
}

if ($MyInvocation.InvocationName -ne '.') {
    Invoke-MSTestMain @PSBoundParameters
}
