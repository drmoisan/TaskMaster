# P4-T10 — both catch paths of the enqueue member

Timestamp: 2026-09-13T16-24

Command: msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"

EXIT_CODE: 0

Output Summary:
- Build summary line as printed: `0 Error(s)`, read by an anchored regular expression.
- `0 Warning(s)`. This task assigns `_itemGroupFailure`, clearing the second and last `CS0649`, so
  the build is back at the P0-T9 baseline of 0 errors and 0 warnings.

Command: & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName~QfcQueueEnqueueTests" "/Logger:trx;LogFileName=vstest-class-run.trx" /ResultsDirectory:TestResults\p4-t10

EXIT_CODE: 0

Output Summary:
- Counters element of TestResults\p4-t10\vstest-class-run.trx:
  `total=14 executed=14 passed=14 failed=0`
- New cases: `EnqueueAsync_WhenLoaderIsCancelled_SwallowsAndLeavesNothingQueued`, which raises
  `OperationCanceledException`, and `EnqueueAsync_WhenLoaderFails_SwallowsAndLeavesNothingQueued`,
  which raises `InvalidOperationException`. Each asserts the enqueue member does not propagate, the
  queue count stays 0 and the running-jobs count returns to 0.
- Both throws are raised from the substituted item-group factory, which the loader calls from
  inside the enqueue member's try block, so the finally decrement runs normally. Neither test
  raises from the background-template factory or from the hook loop: both of those lie before the
  try block, and a throw from there would require the separately promoted counter-leak behaviour to
  be treated as expected, which this item forbids.
