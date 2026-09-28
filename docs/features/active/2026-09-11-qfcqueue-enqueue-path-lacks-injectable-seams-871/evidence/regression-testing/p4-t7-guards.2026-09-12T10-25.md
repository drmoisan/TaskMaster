# P4-T7 — enqueue argument guards

Timestamp: 2026-09-13T16-04

Command: msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"

EXIT_CODE: 0

Output Summary:
- Build summary line as printed: `0 Error(s)`, read by an anchored regular expression.
- `2 Warning(s)`, unchanged.

Command: & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName~QfcQueueEnqueueTests" "/Logger:trx;LogFileName=vstest-class-run.trx" /ResultsDirectory:TestResults\p4-t7

EXIT_CODE: 0

Output Summary:
- Counters element of TestResults\p4-t7\vstest-class-run.trx:
  `total=10 executed=10 passed=10 failed=0`
- New cases: `EnqueueAsync_WithNullItemList_ThrowsArgumentNullException` and
  `EnqueueAsync_WithEmptyItemList_ThrowsArgumentException`.
- Both use the FluentAssertions asynchronous throw assertion (`Awaiting(...).Should().ThrowAsync<T>()`)
  rather than a synchronous one, because the member under test returns a task.
