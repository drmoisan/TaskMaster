# P3-T11 — scoped test run after the Phase 3 seams

Timestamp: 2026-09-13T15-46

Command: & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook" "/Logger:trx;LogFileName=vstest-run.trx" /ResultsDirectory:TestResults\P3-T11

EXIT_CODE: 0

Output Summary:
- `Test Run Successful.` with `Total tests: 1395` and `Passed: 1395`.
- Counters element of TestResults\P3-T11\vstest-run.trx: total=1395 executed=1395 passed=1395
  failed=0.
- `total` equals BASELINE_TEST_TOTAL, which P0-T11 recorded as 1395 and which P1-T7 and P2-T9 each
  re-observed as 1395.
- `failed=0`.
- The counters element is the assertion target rather than a console phrase, because a green run of
  this runner prints no failed or skipped line at all.
- Total time 14.4584 seconds.

Counters line as read by CMD-TRXCOUNTERS:

```
total=1395 executed=1395 passed=1395 failed=0
```

BASELINE_TEST_TOTAL: 1395
ObservedTotal: 1395
ObservedExecuted: 1395
ObservedPassed: 1395
ObservedFailed: 0

This is the gate that proves the three existing QfcQueue test files, and every other test in the
assembly, still pass unmodified after the four Phase 3 seams and the two call-site substitutions.
No test failure was observed that had not been seen before; the run is identical in shape to the
P2-T9 run.
