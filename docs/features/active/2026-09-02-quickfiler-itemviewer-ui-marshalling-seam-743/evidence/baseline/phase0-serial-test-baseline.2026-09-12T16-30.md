# Phase 0 — Whole-assembly SERIAL-regime test baseline (P0-T10)

Task: [P0-T10]
Timestamp: 2026-09-13T02-31
Command: `pwsh -Command '& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/Logger:trx;LogFileName=p0-t10-serial-baseline.trx" /ResultsDirectory:coverage\trx\p0-t10 "/TestCaseFilter:TestCategory!=LiveOutlook"'` Run from the item worktree root via Set-Location inside one pwsh invocation, with the Command Reference tool resolution prepended (inner quoting inverted to single quotes; semantics identical); console output redirected to the ignored path `coverage\p0-t10-vstest.log`. Run while holding the shared machine build lock for item 743.
EXIT_CODE: 0
Output Summary:
- `Test Run Successful.` / `Total tests: 1394` / `Passed: 1394` / `Total time: 12.9130 Seconds`
- Newest `.trx` under `coverage\trx\p0-t10` sorted by `LastWriteTime`: `p0-t10-serial-baseline.trx` (the only file).
- Transcribed `ResultSummary/Counters`: `total=1394`, `passed=1394`, `failed=0`, `timeout=0` (`executed=1394`, outcome `Completed`).
- REGIME: SERIAL (no /Settings: argument; identical in this respect to the CI command at line 99 of the MSTest coverage workflow).

## Pump-test duration table (every test whose name contains `ThroughThePumpHost`; six tests)

| Test | Outcome | duration |
|---|---|---|
| ResolveControlGroupsAsync_ThroughThePumpHost_PopulatesTipsAndControlGroups | Passed | 00:00:00.0746805 |
| InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState | Passed | 00:00:00.1067123 |
| InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme | Passed | 00:00:00.0878174 |
| InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults | Passed | 00:00:00.1200752 |
| InitializeBool_ThroughThePumpHost_CompletesAndInitializesState | Passed | 00:00:00.0807472 |
| InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates | Passed | 00:00:00.0828162 |

The first row is the retained test in the ViewerSetup test file; the other five are the Part3 initialization test file's tests. Largest duration: 120.0752 ms.

## Results directory cleanup

Command: `pwsh -Command 'Test-Path coverage\trx\p0-t10'` (after deleting the directory)
EXIT_CODE: 0
Output Summary: `False`. The raw TRX was discarded after transcription (D1).
