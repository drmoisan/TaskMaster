# AC1 PARALLEL-regime instrumented measurement (P1-T10)

Task: [P1-T10]
Timestamp: 2026-09-13T02-49
Command: `pwsh -Command '& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:TaskMaster.runsettings /InIsolation "/Logger:trx;LogFileName=p1-t10-ac1-parallel.trx" /ResultsDirectory:coverage\trx\p1-t10 "/TestCaseFilter:TestCategory!=LiveOutlook"'` Run from the item worktree root via Set-Location inside one pwsh invocation, with the Command Reference tool resolution prepended (inner quoting inverted to single quotes; semantics identical); console output redirected to the ignored path `coverage\p1-t10-vstest.log`. Run while holding the shared machine build lock for item 743, on an otherwise-idle machine with Outlook closed (the load condition declared in P0-T11 item (d)).
EXIT_CODE: 0
Output Summary:
- `Test Run Successful.` / `Total tests: 1395` / `Passed: 1395` / `Total time: 14.1769 Seconds`
- Newest `.trx` under `coverage\trx\p1-t10` sorted by `LastWriteTime`: `p1-t10-ac1-parallel.trx` (the only `.trx`; the root runsettings also emitted one Code Coverage attachment under the same results directory, which was discarded with it).
- REGIME: PARALLEL (/Settings:TaskMaster.runsettings, which declares Workers 0 and Scope ClassLevel).
- `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` is included in this regime (Passed, 00:00:00.5101996); the total of 1395 equals the P0-T10 baseline total plus the one new balance test.

## Transcription 1 — totals (`ResultSummary/Counters`)

`total=1395`, `passed=1395`, `failed=0`, `timeout=0` (`executed=1395`, outcome `Completed`).

## Transcription 2 — balance test outcome

`TransactionGate_WhileThisTestHoldsATransaction_HasExactlyOneUnreleasedAcquisition` | Passed | duration 00:00:00.0017438

## Transcription 3 — the single GATECOUNTERS line (from the balance test's `Output/StdOut`)

```
GATECOUNTERS acquisitions=19 releases=18 contended=14
```

acquisitions = 19, releases = 18, contended = 14. Difference at the moment of assertion: 19 - 18 = 1.

Note: a contended count greater than zero in this regime is the expected consequence of genuine queueing — distinct `[TestClass]` types run concurrently under Workers 0 / Scope ClassLevel and their transactions queue on the one-permit gate with a live holder — and does not by itself indicate a leak. Only the serial-regime figure (P1-T9: contended = 0) discriminates.

## Transcription 4 — pump-test duration table (every test whose name contains `ThroughThePumpHost`; six tests)

| Test | Outcome | duration |
|---|---|---|
| InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme | Passed | 00:00:01.0538215 |
| ResolveControlGroupsAsync_ThroughThePumpHost_PopulatesTipsAndControlGroups | Passed | 00:00:01.9458362 |
| InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates | Passed | 00:00:00.2230774 |
| InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState | Passed | 00:00:06.4603800 |
| InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults | Passed | 00:00:00.1238651 |
| InitializeBool_ThroughThePumpHost_CompletesAndInitializesState | Passed | 00:00:00.5217196 |

Largest duration: 6460.3800 ms (InitializeSequentialAsync), against 111.1364 ms for the same test in the serial run — a 58x elongation under class-level parallelism on an otherwise-idle machine.

## Results directory cleanup

Command: `pwsh -Command 'Test-Path coverage\trx\p1-t10'` (after deleting the directory)
EXIT_CODE: 0
Output Summary: `False`. The raw TRX and the coverage attachment were discarded after transcription (D1).
