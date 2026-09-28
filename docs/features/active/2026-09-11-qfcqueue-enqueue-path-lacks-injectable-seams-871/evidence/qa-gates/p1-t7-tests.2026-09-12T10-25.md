# P1-T7 — QuickFiler test assembly after the Phase 1 split

Timestamp: 2026-09-13T15-21
Command: & <vstest.console.exe> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook" "/Logger:trx;LogFileName=vstest-run.trx" /ResultsDirectory:TestResults\p1-t7
EXIT_CODE: 0
Output Summary: Test Run Successful. The trx counters element reports total=1395, executed=1395,
passed=1395, failed=0. The total equals BASELINE_TEST_TOTAL as recorded by P0-T11, so the split
neither added nor removed a test case. No test failed.

## Counters read from the trx counters element (CMD-TRXCOUNTERS)

```
total=1395 executed=1395 passed=1395 failed=0
```

- total: 1395
- executed: 1395
- passed: 1395
- failed: 0

BASELINE_TEST_TOTAL from P0-T11: 1395. Observed total: 1395. The two are equal, which is this task's
acceptance condition.

The counters element is the assertion target rather than a console phrase, because a green run of this
runner prints no failed or skipped line at all.

## Results file

TestResults\p1-t7\vstest-run.trx (the results directory is matched by a repository ignore pattern and
so never appears in a porcelain status that excludes ignored paths).

## Console tail

```
Test Run Successful.
Total tests: 1395
     Passed: 1395
 Total time: 13.5717 Seconds
```
