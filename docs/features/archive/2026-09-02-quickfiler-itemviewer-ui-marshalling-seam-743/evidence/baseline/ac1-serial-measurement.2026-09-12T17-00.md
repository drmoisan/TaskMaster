# AC1 SERIAL-regime instrumented measurement (P1-T9)

Task: [P1-T9]
Timestamp: 2026-09-13T02-47
Command: `pwsh -Command '& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/Logger:trx;LogFileName=p1-t9-ac1-serial.trx" /ResultsDirectory:coverage\trx\p1-t9 "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~Transaction_SecondCallerCannotInstallUntilTheFirstRestores"'` Run from the item worktree root via Set-Location inside one pwsh invocation, with the Command Reference tool resolution prepended (inner quoting inverted to single quotes; semantics identical); console output redirected to the ignored path `coverage\p1-t9-vstest.log`. Run while holding the shared machine build lock for item 743, on an otherwise-idle machine with Outlook closed (the load condition declared in P0-T11 item (d)).
EXIT_CODE: 0
Output Summary:
- `Test Run Successful.` / `Total tests: 1394` / `Passed: 1394` / `Total time: 12.2609 Seconds`
- Newest `.trx` under `coverage\trx\p1-t9` sorted by `LastWriteTime`: `p1-t9-ac1-serial.trx` (the only file).
- REGIME: SERIAL (no /Settings: argument).
- EXCLUDED BY DESIGN: Transaction_SecondCallerCannotInstallUntilTheFirstRestores starts a second transaction while the first is held (fixture test file lines 220-238); its contended acquisition has a live holder and lies outside the observable.
- The exclusion was verified against the TRX: zero results carry that test name. The total of 1394 equals the P0-T10 baseline total (1394) plus the one new balance test minus the one excluded test.

## Transcription 1 — totals (`ResultSummary/Counters`)

`total=1394`, `passed=1394`, `failed=0`, `timeout=0` (`executed=1394`, outcome `Completed`).

## Transcription 2 — balance test outcome

`TransactionGate_WhileThisTestHoldsATransaction_HasExactlyOneUnreleasedAcquisition` | Passed | duration 00:00:00.0010778

## Transcription 3 — the single GATECOUNTERS line (from the balance test's `Output/StdOut`)

```
GATECOUNTERS acquisitions=11 releases=10 contended=0
```

acquisitions = 11, releases = 10, contended = 0. Difference at the moment of assertion: 11 - 10 = 1 (the balance test's own held transaction).

## Transcription 4 — pump-test duration table (every test whose name contains `ThroughThePumpHost`; six tests)

| Test | Outcome | duration |
|---|---|---|
| InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme | Passed | 00:00:00.0852421 |
| InitializeBool_ThroughThePumpHost_CompletesAndInitializesState | Passed | 00:00:00.0817954 |
| InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates | Passed | 00:00:00.0857493 |
| InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults | Passed | 00:00:00.1245081 |
| InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState | Passed | 00:00:00.1111364 |
| ResolveControlGroupsAsync_ThroughThePumpHost_PopulatesTipsAndControlGroups | Passed | 00:00:00.0681703 |

Largest duration: 124.5081 ms.

## Decision rule (fixed in advance in P1-T9 of the plan and reproduced verbatim in the P0-T11 declaration)

| Serial-run contended count | Serial-run balance test | Verdict |
|---|---|---|
| 0 | passed (difference equals 1) | H-LEAK REJECTED by direct observation; H-COST is the surviving mechanism |
| greater than 0 | any | H-LEAK OPERATIVE: a serial run found the permit held, which requires a leaked or late-released transaction |
| 0 | failed (difference greater than 1) | H-LEAK OPERATIVE: acquisitions exceed releases |

The measured serial-regime figures (contended = 0; balance test passed with difference 1) select the first row. The verdict is recorded in the P1-T11 artifact.

## Results directory cleanup

Command: `pwsh -Command 'Test-Path coverage\trx\p1-t9'` (after deleting the directory)
EXIT_CODE: 0
Output Summary: `False`. The raw TRX was discarded after transcription (D1).
