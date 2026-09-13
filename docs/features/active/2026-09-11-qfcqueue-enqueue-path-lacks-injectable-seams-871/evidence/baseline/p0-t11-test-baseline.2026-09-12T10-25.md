# P0-T11 — Test baseline for the QuickFiler test assembly

Timestamp: 2026-09-13T04-59
Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook" "/Logger:trx;LogFileName=vstest-run.trx" /ResultsDirectory:TestResults\p0-t11
EXIT_CODE: 0

Test platform resolved through vswhere: Visual Studio 18 Community, VSTest version 18.10.0 (x64).

## Console tail, verbatim

```
Results File: <worktree-root>\TestResults\p0-t11\vstest-run.trx

Test Run Successful.
Total tests: 1394
     Passed: 1394
 Total time: 13.1292 Seconds
VSTEST-EXIT: 0
```

## Counters element of the trx, the assertion target

Command: CMD-TRXCOUNTERS over the newest trx under the results directory for this task

```
total=1394 executed=1394 passed=1394 failed=0
```

TrxTotal: 1394
TrxExecuted: 1394
TrxPassed: 1394
TrxFailed: 0

BASELINE_TEST_TOTAL: 1394

The counters element rather than a console phrase is the assertion target, because a green run of
this runner prints no failed line and no skipped line at all, so an assertion phrased over console
text would have nothing to read on the passing case.

## Notes on the command shape

The run names exactly one test assembly, as the catalogue requires. A whole-solution local run pulls
in four shell-icon test classes in another assembly that stall the runner on this host; that is an
environmental property of this machine rather than a regression, and the repository pipeline covers
those classes. This command passes no settings file, which is the same configuration the repository
pipeline uses.

The results directory lies under the repository TestResults directory, which is matched by the
repository ignore file at ignore-file line 39, verified with `git check-ignore -v`. The trx itself is
therefore never a candidate for staging, which is consistent with the repository rule that only
projections of a test run may be committed.

Output Summary: 1394 of 1394 tests passed, 0 failed, runner exit code 0. BASELINE_TEST_TOTAL is 1394.
Both clauses of the acceptance condition are met: the exit code is 0 and the trx counters element
reports `failed=0`. The command was run while this item held the shared build lock, which was
released immediately after it returned.
