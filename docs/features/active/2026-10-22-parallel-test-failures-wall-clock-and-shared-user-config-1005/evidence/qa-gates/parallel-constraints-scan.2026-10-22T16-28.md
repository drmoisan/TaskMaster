# Parallel-Constraints Scan (P6-T2)

Timestamp: 2026-10-22T16-28
Command: git diff <merge-base> -- "*.cs" "*.runsettings" "*.config" (added lines) plus every untracked .cs file from git status --porcelain --untracked-files=all, scanned with a case-sensitive substring match per token; git diff --name-only <merge-base> and git status --porcelain over coverage.config, TaskMaster.runsettings, scripts/vscode/TaskMaster.cli.runsettings
EXIT_CODE: 0
Output Summary: Every token has an added-line count of 0 in the diff and 0 in the 7 untracked .cs files. Removal lines: DoNotParallelize 2 (the class-level attributes removed from AppQuickFilerSettingsTests and RibbonControllerTests), all others 0. coverage.config, TaskMaster.runsettings, and scripts/vscode/TaskMaster.cli.runsettings are unchanged (empty diff and empty status).

MERGE-BASE: computed by `git merge-base origin/main HEAD` after `git fetch origin main` (recorded in `evidence/baseline/merge-base-scope.2026-10-22T16-06.md`).

## Scope scanned

- Diff added lines: 744 (removed lines: 389) across tracked `*.cs`, `*.runsettings`, `*.config`.
- Untracked `.cs` files (431 lines):
  - `TaskMaster.Test/AppGlobals/InMemorySettingsStore.cs`
  - `TaskMaster.Test/AppGlobals/SettingsSaveProbe.cs`
  - `TaskMaster/AppGlobals/AppAutoFileObjects.ScalarSettings.cs`
  - `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.QfcColumnsAsync.cs`
  - `UtilitiesCS.Test/HelperClasses/Settings/ApplicationSettingsStoreTests.cs`
  - `UtilitiesCS/HelperClasses/Settings/ApplicationSettingsStore.cs`
  - `UtilitiesCS/HelperClasses/Settings/ISettingsStore.cs`

## Token counts

| Token | Added (diff) | Untracked files | Removed (reported separately, allowed) |
| --- | --- | --- | --- |
| `DoNotParallelize` | 0 | 0 | 2 |
| `Workers` | 0 | 0 | 0 |
| `Retry` | 0 | 0 | 0 |
| `Thread.Sleep` | 0 | 0 | 0 |
| `Task.Delay` | 0 | 0 | 0 |
| `Timeout` | 0 | 0 | 0 |
| `GetTempPath` | 0 | 0 | 0 |
| `GetTempFileName` | 0 | 0 | 0 |
| `user.config` | 0 | 0 | 0 |
| `SettingsProvider` | 0 | 0 | 0 |
| `ExcludeFromCodeCoverage` | 0 | 0 | 0 |

## Configuration files

| File | Diff vs MERGE-BASE | Porcelain status |
| --- | --- | --- |
| `coverage.config` | none | clean |
| `TaskMaster.runsettings` | none | clean |
| `scripts/vscode/TaskMaster.cli.runsettings` | none | clean |

Result: PASS. Every added-line count is zero.
