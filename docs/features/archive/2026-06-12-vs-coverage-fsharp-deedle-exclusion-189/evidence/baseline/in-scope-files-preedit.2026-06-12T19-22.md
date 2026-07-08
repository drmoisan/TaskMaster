# Phase 0 — In-Scope Files Pre-Edit Capture

Timestamp: 2026-06-12T19-22

Command: `Read` of each in-scope file (verbatim capture) + `coverage.config`

EXIT_CODE: 0

Output Summary:
- `TaskMaster.runsettings`: present, at committed baseline. Contains ONLY the `<MSTest><Parallelize>` block;
  NO `<DataCollectionRunSettings>` / `<DataCollectors>` block is present.
- `scripts/vscode/TaskMaster.cli.runsettings`: ABSENT (not yet created).
- `scripts/vscode/Invoke-MSTest.ps1`: `Resolve-RunSettingsPath` resolves repo-root `TaskMaster.runsettings`.
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1`: `Resolve-RunSettingsPath` resolves repo-root `TaskMaster.runsettings`.
- `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`: `$script:expectedRunSettings` = repo-root `TaskMaster.runsettings`;
  assertions target that path.
- `coverage.config`: seven `<ModulePath>` exclusions captured verbatim (see below).

## TaskMaster.runsettings (pre-edit, verbatim)

```xml
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <MSTest>
    <Parallelize>
      <Workers>0</Workers>
      <Scope>ClassLevel</Scope>
    </Parallelize>
  </MSTest>
</RunSettings>
```

State decision: at baseline (no `<DataCollectionRunSettings>` block present). Phase 1 P1-T2 therefore ADDS the
coverage Exclude block (no prior additive block to normalize).

## scripts/vscode/TaskMaster.cli.runsettings (pre-edit)

ABSENT — file does not exist. Will be created in P1-T1.

## scripts/vscode/Invoke-MSTest.ps1 — Resolve-RunSettingsPath (pre-edit, verbatim)

```powershell
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
```

Call site (line 92): `$runSettingsPath = Resolve-RunSettingsPath -RepoRoot $repoRoot`

## scripts/vscode/Invoke-MSTestWithCoverage.ps1 — Resolve-RunSettingsPath (pre-edit, verbatim)

```powershell
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
```

Call site (line 113): `$runSettingsPath = Resolve-RunSettingsPath -RepoRoot $repoRoot`

## tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 — expected-path + Describe assertions (pre-edit, verbatim)

`$script:expectedRunSettings` line (line 20):
```powershell
    $script:expectedRunSettings = Join-Path $script:repoRoot 'TaskMaster.runsettings'
```

`Resolve-RunSettingsPath` Describe block (lines 23-37):
```powershell
Describe 'Resolve-RunSettingsPath' {
    It 'resolves the repo-root TaskMaster.runsettings path when present' {
        $resolved = Resolve-RunSettingsPath -RepoRoot $script:repoRoot

        $resolved | Should -Be $script:expectedRunSettings
    }

    It 'fails fast with a specific error naming the missing path when absent' {
        $missingRoot = Join-Path $script:repoRoot 'does-not-exist-runsettings-root'
        $expectedMissing = Join-Path $missingRoot 'TaskMaster.runsettings'

        { Resolve-RunSettingsPath -RepoRoot $missingRoot } |
            Should -Throw -ExpectedMessage "Runsettings file not found: $expectedMissing"
    }
}
```

The `Get-VsTestArgumentList` and `Get-DotnetCoverageArgumentList` `/Settings:` assertions (lines 39-148)
all reference `$script:expectedRunSettings`.

## coverage.config — seven ModulePath exclusions (verbatim)

```xml
<ModulePath>.*Deedle.*</ModulePath>
<ModulePath>.*FSharp.*</ModulePath>
<ModulePath>.*Castle\.Core.*</ModulePath>
<ModulePath>.*FluentAssertions.*</ModulePath>
<ModulePath>.*Moq.*</ModulePath>
<ModulePath>.*Microsoft\.Testing.*</ModulePath>
<ModulePath>.*MSTest.*</ModulePath>
```
