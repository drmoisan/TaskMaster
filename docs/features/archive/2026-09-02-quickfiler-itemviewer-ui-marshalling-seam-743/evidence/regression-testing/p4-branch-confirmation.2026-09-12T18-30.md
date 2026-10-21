# Phase 4 — Branch COST confirmation in both regimes (P4-T3)

Task: [P4-T3]
Branch selected in P4-T1: COST (no fixture file modified; counters and balance test retained). Every run below was executed from the item worktree root via Set-Location inside one pwsh invocation with the Command Reference tool resolution prepended, console output redirected to an ignored path under `coverage\`, and the shared machine build lock for item 743 acquired separately for each run and released immediately after it returned. Outlook was closed; no induced load.

## Run A — SERIAL regime

Timestamp: 2026-09-13T03-31
Command: `pwsh -Command '& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/Logger:trx;LogFileName=p4-t3-serial.trx" /ResultsDirectory:coverage\trx\p4-t3-serial "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~Transaction_SecondCallerCannotInstallUntilTheFirstRestores"'`
EXIT_CODE: 0
Output Summary: total=1399 executed=1399 passed=1399 failed=0 timeout=0 notExecuted=0 (TRX `p4-t3-serial.trx`, outcome=Completed); console `Total tests: 1399` / `Passed: 1399` / `Total time: 12.2704 Seconds`.

REGIME: SERIAL (no /Settings: argument).

- Balance test `TransactionGate_WhileThisTestHoldsATransaction_HasExactlyOneUnreleasedAcquisition`: **Passed** [00:00:00.0010079]
- `GATECOUNTERS acquisitions=11 releases=10 contended=0`
- EXCLUDED BY DESIGN: `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` (not present in the TRX, per the filter).
- Serial contended count is `0`, unchanged in kind from P1-T9 (`acquisitions=11 releases=10 contended=0`).
- The total is 1399 = the P1-T9 serial total of 1394 plus the five seam tests added in P2-T6.

## Run B — PARALLEL regime, first execution (recorded; superseded by the re-run below)

Timestamp: 2026-09-13T03-32
Command: `pwsh -Command '& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:TaskMaster.runsettings /InIsolation "/Logger:trx;LogFileName=p4-t3-parallel.trx" /ResultsDirectory:coverage\trx\p4-t3-parallel "/TestCaseFilter:TestCategory!=LiveOutlook"'`
EXIT_CODE: 1
Output Summary: total=1400 executed=1400 passed=1397 failed=3 timeout=0 (outcome=Failed); console `Total tests: 1400` / `Passed: 1397` / `Failed: 3` / `Total time: 15.4625 Seconds`.

REGIME: PARALLEL (/Settings:TaskMaster.runsettings, which declares Workers 0 and Scope ClassLevel).

- Balance test: **Passed** [00:00:00.0020824]
- `GATECOUNTERS acquisitions=19 releases=18 contended=15`
- The three failures are NOT the known-intermittent R4 test and are NOT gate-related; all three are in `QfcInitEmailQueueZeroBatchTests` (a class outside this item's Write Set, which this plan never edits) and carry the same message:
  - `InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing`
  - `InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop`
  - `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker`
  - Message: `System.TypeInitializationException: The type initializer for 'Deedle.Reflection' threw an exception. ---> System.TypeInitializationException: The type initializer for '<StartupCode$Deedle>.$FrameUtils' threw an exception. ---> System.IO.FileNotFoundException: Could not load file or assembly 'netstandard, Version=2.1.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51' or one of its dependencies. The system cannot find the file specified.`
  - Classification: an assembly-binding failure inside Deedle's static constructor under class-level parallelism (a type-initializer failure is sticky for the AppDomain once it occurs, so all three tests in the class fail together). It is environmental and independent of this item's changes: none of the seven Write Set files references Deedle, the same class passed in the serial run above and in the P1-T10 parallel run of run B (`failed=0` over 1395 tests), and it passed again in the re-run below. It is reported to the caller as an out-of-scope intermittent for follow-up; no file was edited in response.

## Run B' — PARALLEL regime, re-run (one re-run as a diagnostic micro-action; both executions transcribed)

Timestamp: 2026-09-13T03-33
Command: `pwsh -Command '& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:TaskMaster.runsettings /InIsolation "/Logger:trx;LogFileName=p4-t3-parallel.trx" /ResultsDirectory:coverage\trx\p4-t3-parallel "/TestCaseFilter:TestCategory!=LiveOutlook"'` (console output to `coverage\p4-t3-parallel-rerun.log`; the fixed `LogFileName` overwrote the first execution's TRX after it had been transcribed above)
EXIT_CODE: 0
Output Summary: total=1400 executed=1400 passed=1400 failed=0 timeout=0 (TRX `p4-t3-parallel.trx`, LastWriteTime 03:34:14, outcome=Completed); console `Total tests: 1400` / `Passed: 1400` / `Total time: 15.7217 Seconds`.

REGIME: PARALLEL (/Settings:TaskMaster.runsettings, which declares Workers 0 and Scope ClassLevel).

- Balance test `TransactionGate_WhileThisTestHoldsATransaction_HasExactlyOneUnreleasedAcquisition`: **Passed** [00:00:00.0020294]
- `GATECOUNTERS acquisitions=19 releases=18 contended=14`
- `Transaction_SecondCallerCannotInstallUntilTheFirstRestores`: Passed (included in this regime).
- The counters are unchanged in kind from P1-T10 (`acquisitions=19 releases=18 contended=14`); a contended count greater than zero in this regime is genuine live-holder queueing across concurrently running classes and does not by itself indicate a leak; only the serial-regime figure discriminates, and it is `0`.

## Branch COST confirmation

Both regimes' accepted runs (Run A and Run B') record `failed=0`, the balance test passed in both with difference exactly 1 (11 - 10 and 19 - 18), and the counters are unchanged in kind from P1-T9 and P1-T10. The retained instrumentation is confirmed clean under Branch COST.

## Results directories deleted

`coverage\trx\p4-t3-serial` and `coverage\trx\p4-t3-parallel` were deleted after transcription; `Test-Path` printed `False` for both. No raw `.trx` was written outside the ignored `coverage` directory (D1).
