# P8-T3 — Toolchain Step 2, Analyzer Gate

Timestamp: 2026-08-26T11-23

Pass number: **3** — the final pass.

Command: `pwsh -NoProfile -Command '$vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $mb = & $vsw -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $mb "TaskMaster.sln" /t:Rebuild /m "/p:Configuration=Debug" "/p:Platform=Any CPU" "/p:EnableNETAnalyzers=true" "/p:EnforceCodeStyleInBuild=true"; "EXIT_CODE: $LASTEXITCODE"'`

EXIT_CODE: 0

## Output Summary

- Exit code: **0** — the primary acceptance condition, met absolutely.
- MSBuild summary: **5 Warning(s), 0 Error(s)**. `Build succeeded.` Time elapsed 00:00:34.58.
- Warning count: **5**. Error count: **0**.

### Command-shape assertions

- The command contains `/t:Rebuild`. **It does not contain `/t:Build`**, so `CoreCompile` ran on every
  project and the analyzers actually executed; a warm `/t:Build` would have returned exit 0 having
  compiled nothing.
- The command does not contain `/p:Nullable=enable`, matching CI.

### Diagnostic breakdown

A scan of the full build log for the pattern `(warning|error) [A-Z]+[0-9]+` returned **zero matches**:
there is no coded compiler or analyzer diagnostic of any severity in the output. In particular there is
no `error CS0006`, so the `packages/Meziantou.Analyzer.3.0.156/` and
`packages/Roslynator.Analyzers.4.16.0/` provisioning documented in `p0-t13-analyzer-rebuild.md` is still
in place.

All five warnings are the identical uncoded `System.Reactive` packages.config advisory emitted by
`packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5)`, one each on
`QuickFiler`, `TaskMaster`, `ToDoModel`, `UtilitiesCS` and `UtilitiesCS.Test`. The advisory carries no
warning code, comes from a third-party targets file inside the gitignored `packages/` directory, and is
pre-existing on the integration branch. **No diagnostic names any file on the `P8-T1` target list**, or
any first-party source file at all.

The three tests appended to `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs` in the
`P8-T7` remediation introduced no analyzer diagnostic.

### Comparison with the `P0-T13` baseline

| Metric | `P0-T13` baseline | This run | Change |
|---|---:|---:|---|
| Exit code | 0 | 0 | none |
| Errors | 0 | 0 | none |
| Warnings | 5 | 5 | none |
| Projects with the advisory | 5 (same five) | 5 (same five) | none |

Exact parity with the baseline. Pass 1 produced the identical result (0 errors, 5 warnings, exit 0).

### Degradation status

The conditional degradation is permitted ONLY IF the `P0-T13` baseline recorded a non-zero exit code. It
recorded `EXIT_CODE: 0`, so the degradation branch is **unavailable**. The gate stood at its primary
condition `EXIT_CODE: 0` and met it. No `ExpectedExitCode:` is declared.
