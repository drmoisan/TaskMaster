# P4-T9 — running-jobs counter bookkeeping

Timestamp: 2026-09-13T16-23

Command: msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"

EXIT_CODE: 0

Output Summary:
- Build summary line as printed: `0 Error(s)`, read by an anchored regular expression.
- `1 Warning(s)`, down from 2: this task assigns `_itemGroupObserver`, clearing one of the two
  `CS0649` diagnostics. The remaining one is `_itemGroupFailure`, which P4-T10 assigns.

Command: & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName~QfcQueueEnqueueTests" "/Logger:trx;LogFileName=vstest-class-run.trx" /ResultsDirectory:TestResults\p4-t9

EXIT_CODE: 0

Output Summary:
- Counters element of TestResults\p4-t9\vstest-class-run.trx:
  `total=12 executed=12 passed=12 failed=0`
- New case: `EnqueueAsync_WhenItRuns_IncrementsRunningJobsAndDecrementsOnCompletion`. It installs an
  observer that the substituted item-group factory invokes mid-flight, samples `JobsRunning` from
  inside that callback and asserts it reads 1, which proves the increment took effect before the
  loader ran, then asserts `JobsRunning` reads 0 after the call returns, which proves the finally
  block decremented it.
