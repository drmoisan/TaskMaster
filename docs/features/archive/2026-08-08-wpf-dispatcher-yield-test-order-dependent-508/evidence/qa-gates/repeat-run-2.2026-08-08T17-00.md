# Repeat Run 2 of 3 — `UtilitiesCS.Test` full parallel run

Timestamp: 2026-08-08T17-00

Task: [P2-T8]

AC served: AC7.

## Command

Identical to P2-T7, rerun without modification:

Command: `<vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`

`<vstest>` = `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`
(resolved via `vswhere.exe`).

EXIT_CODE: 0

Nothing was rebuilt, edited, or reconfigured between run 1 and run 2. Class-level parallelization
(`Workers = 0`) is the assembly's own unmodified `[assembly: Parallelize]` attribute, so thread
assignment is free to differ between runs — which is exactly what makes repetition meaningful for
an order-dependence defect.

## Result

```
Test Run Successful.
Total tests: 4667
     Passed: 4667
```

Total 4667 / Passed 4667 / Failed 0 / Skipped 0. Identical counts to run 1.

## Per-test outcome for every `WpfDispatcherYieldTests` method

| # | Method | Outcome | Duration |
|---|---|---|---|
| 1 | `YieldAsync_CanceledToken_ThrowsBeforeDispatcherYield` | Passed | 2 ms |
| 2 | `YieldAsync_ThreadAffinitizedDispatcherPresent_YieldsWithoutFallback` | Passed | 13 ms |
| 3 | `YieldAsync_ThreadDispatcherAbsent_FallsBackToProcessGlobalDispatcher` | Passed | 21 ms |
| 4 | `YieldAsync_WithoutDispatcher_RemainsStrict` | Passed | 1 ms |

All four passed. Durations vary slightly from run 1 (normal scheduling jitter); outcomes do not.

Failure scan of the log for `^\s+Failed ` returned no lines.

Output Summary: PASS, EXIT_CODE 0. Second consecutive run of the identical command with no
intervening change: Total 4667, Passed 4667, Failed 0 — identical counts to run 1. All four
`WpfDispatcherYieldTests` methods passed again, including
`YieldAsync_WithoutDispatcher_RemainsStrict`. Run 2 of the three required for AC7.
