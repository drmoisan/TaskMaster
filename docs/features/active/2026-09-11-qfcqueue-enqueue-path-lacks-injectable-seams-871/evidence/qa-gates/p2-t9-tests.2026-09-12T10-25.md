# P2-T9 — QuickFiler test assembly after the Phase 2 seams

Timestamp: 2026-09-13T15-33
Command: & <vstest.console.exe> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook" "/Logger:trx;LogFileName=vstest-run.trx" /ResultsDirectory:TestResults\p2-t9
EXIT_CODE: 0
Output Summary: Test Run Successful. The trx counters element reports total=1395, executed=1395,
passed=1395, failed=0. The total equals BASELINE_TEST_TOTAL, so the two seams added no test case and
removed none. This is the gate that proves the three existing QfcQueue test files still pass
unmodified.

## Counters read from the trx counters element (CMD-TRXCOUNTERS)

```
total=1395 executed=1395 passed=1395 failed=0
```

- total: 1395
- executed: 1395
- passed: 1395
- failed: 0

BASELINE_TEST_TOTAL from P0-T11: 1395. Observed total: 1395. The two are equal, which is this task's
acceptance condition, alongside failed=0.

## What this run establishes

Seam S1 replaced the single move-monitor field read in the enqueue path with a read through the new
`MoveMonitor` property, and seam S2 replaced the three marshalling bodies with forwards through the
new `UiIdleDispatcher` property. No existing test file was edited by either task. Six existing test
methods resolve the move-monitor backing field by reflection under its current name; the field was
retained rather than converted to an auto-property for exactly that reason, and all six still pass.

## Console tail

```
Test Run Successful.
Total tests: 1395
     Passed: 1395
 Total time: 13.1926 Seconds
```

No test failure appeared that was not present before this change: the run is fully green, as the
P0-T11 baseline and the P1-T7 run were.
