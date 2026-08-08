# Repeat Run 3 of 3 — `UtilitiesCS.Test` full parallel run

Timestamp: 2026-08-08T17-02

Task: [P2-T9]

AC served: AC7.

## Command

Identical to P2-T7 and P2-T8, rerun without modification:

Command: `<vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`

`<vstest>` = `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`
(resolved via `vswhere.exe`).

EXIT_CODE: 0

Nothing was rebuilt, edited, or reconfigured between runs 1, 2, and 3.

## Result

```
Test Run Successful.
Total tests: 4667
     Passed: 4667
```

Total 4667 / Passed 4667 / Failed 0 / Skipped 0. Identical counts to runs 1 and 2.

## Per-test outcome for every `WpfDispatcherYieldTests` method

| # | Method | Outcome | Duration |
|---|---|---|---|
| 1 | `YieldAsync_CanceledToken_ThrowsBeforeDispatcherYield` | Passed | 6 ms |
| 2 | `YieldAsync_ThreadAffinitizedDispatcherPresent_YieldsWithoutFallback` | Passed | 8 ms |
| 3 | `YieldAsync_ThreadDispatcherAbsent_FallsBackToProcessGlobalDispatcher` | Passed | 33 ms |
| 4 | `YieldAsync_WithoutDispatcher_RemainsStrict` | Passed | 1 ms |

All four passed. Durations differ from runs 1 and 2 (3/13/12/1 ms and 2/13/21/1 ms respectively),
confirming genuinely different scheduling across runs while outcomes stayed constant. That variance
is the point: under class-level parallelization the thread each class lands on differs between runs,
and the result no longer changes with it.

Failure scan of the log for `^\s+Failed ` returned no lines.

Output Summary: PASS, EXIT_CODE 0. Third consecutive run of the identical command with no
intervening change: Total 4667, Passed 4667, Failed 0 — identical counts to runs 1 and 2. All four
`WpfDispatcherYieldTests` methods passed, with per-test durations varying across the three runs
while outcomes remained constant. Run 3 of the three required for AC7; the AC7 threshold is now met.
