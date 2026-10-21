# Phase 0 — Session Helper Self-Check (Issue #797)

Timestamp: 2026-09-07T09-14

Command: `pwsh -NoProfile -File coverage/plan797-helpers.ps1 -Mode SelfCheck`

EXIT_CODE: 0

## Observed output

```text
VSTEST-RESOLVED=C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe
MSBUILD-RESOLVED=C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe
HELPER-FUNCTIONS=Resolve-VsWherePath,Resolve-VsTestConsolePath,Resolve-MsBuildPath,Get-NamedTestCaseFilter,Get-DerivedCoverageSettingsXml,Invoke-ScopedVsTest,Invoke-ScopedCoverage,Get-CoberturaRootCounters,Get-TrxSummary,Get-TrxPassedNames,Get-CoberturaHitMap,Get-ChangedLineCoverage,Get-MeasurableClassFiles
```

Both resolved paths were produced by the Visual Studio installer's vswhere executable using its
`-find` switch, which enumerates existing files only, so each reported path exists. Thirteen function
names are listed, above the required minimum of six.

## Helper properties required by rule R2

- The helper lives at the single fixed path coverage/plan797-helpers.ps1, inside a git-ignored
  directory, so it never appears in a porcelain or diff scope gate.
- It builds its own dotnet-coverage argument list rather than calling the repository coverage runner
  end to end. It supplies its own two-assembly list, its own combined test-case filter and its own
  output path, and it performs no post-processing.
- It reproduces the repository runner's in-memory settings derivation, adding one module exclusion
  matching any module name ending in `.Test.dll`, so both test assemblies are excluded from
  instrumentation and the denominator holds production code only. The canonical coverage.config file
  is read and never written.
- It reuses the repository runner's shape: `dotnet-coverage collect`, the cobertura output format, the
  off-root CLI runsettings at scripts/vscode/TaskMaster.cli.runsettings, and the `/InIsolation` switch.
- No repository script under the vscode scripts directory is modified.
- It is a session-scoped throwaway created in Phase 0 and deleted in P6-T11, so no Pester test is
  authored for it and it is not a production PowerShell file for coverage or budget purposes.

## The three backslash-bearing literals authored verbatim into the helper

```text
${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe
Common7\IDE\Extensions\TestPlatform\vstest.console.exe
MSBuild\**\Bin\MSBuild.exe
```

Output Summary: The helper self-check exited 0 and printed one `VSTEST-RESOLVED=` line, one
`MSBUILD-RESOLVED=` line and one `HELPER-FUNCTIONS=` line naming thirteen functions. Both resolved
executables are present on this workstation under Visual Studio 18 Community.
