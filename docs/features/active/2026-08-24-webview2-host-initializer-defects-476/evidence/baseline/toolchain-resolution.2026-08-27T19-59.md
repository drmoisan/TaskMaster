# Toolchain Resolution ([P0-T6])

Timestamp: 2026-08-27T19-59

Command:
```
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
Get-Command dotnet-coverage
Test-Path coverage.config
Test-Path scripts\vscode\TaskMaster.cli.runsettings
```
(run through `pwsh -NoProfile` from the workspace root)

EXIT_CODE: 0

## Resolved items

`<user>` below stands for the interactive account name, which is not written into committed
artifacts. Every path was confirmed present with `Test-Path -LiteralPath` at resolution time.

| # | Item | Resolved absolute path | Exists |
| --- | --- | --- | --- |
| 1 | `vswhere.exe` | `C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe` | True |
| 2 | `MSBuild.exe` | `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe` | True |
| 3 | `vstest.console.exe` | `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe` | True |
| 4 | `dotnet-coverage` | `C:\Users\<user>\.dotnet\tools\dotnet-coverage.exe` | True |
| 5 | `coverage.config` | `<repo-root>\coverage.config` | True |

Additional item confirmed (required by every scoped test run in this plan, though not one of the
five the acceptance enumerates):

| Item | Path | Exists |
| --- | --- | --- |
| vstest run settings | `<repo-root>\scripts\vscode\TaskMaster.cli.runsettings` | True |

## Output Summary

- All five items required by the acceptance resolved, and each resolved path exists on disk.
- MSBuild resolves to the Visual Studio **18** Community full-framework MSBuild, which is the
  correct host for this solution's legacy non-SDK `net481` projects.
- `vstest.console.exe` resolves from the same Visual Studio 18 installation. `dotnet-coverage`
  reports version `18.5.2+6e39b75eaf98f2691cf62dbf259669cc13851fd3` and is on `PATH` as a global
  tool, which satisfies the precondition
  `scripts/vscode/Invoke-MSTestWithCoverage.ps1` enforces (it throws `dotnet-coverage not found`
  when absent).
- `coverage.config` and `scripts/vscode/TaskMaster.cli.runsettings` both exist, so the coverage
  runner and every scoped `vstest` invocation in this plan can supply the mandated
  `/Settings:` argument.
