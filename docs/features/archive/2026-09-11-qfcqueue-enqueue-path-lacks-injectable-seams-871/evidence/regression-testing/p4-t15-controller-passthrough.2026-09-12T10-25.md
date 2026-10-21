# P4-T15 — item-controller argument pass-through and initialize count

Timestamp: 2026-09-13T16-30

Command: msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"

EXIT_CODE: 0

Output Summary:
- Build summary line as printed: `0 Error(s)`, read by an anchored regular expression.
- `0 Warning(s)`.

Command: & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName~QfcQueueEnqueueTests" "/Logger:trx;LogFileName=vstest-class-run.trx" /ResultsDirectory:TestResults\p4-t15

EXIT_CODE: 0

Output Summary:
- Counters element of TestResults\p4-t15\vstest-class-run.trx:
  `total=24 executed=24 passed=24 failed=0`
- New case: `EnqueueAsync_WithOneItem_PassesEveryControllerArgumentThrough`, asserting all nine
  captured arguments: the globals instance the queue was constructed with (by reference), the home
  controller value the queue holds (null, the literal the harness constructs with), the collection
  controller passed to the enqueue call (by reference), the viewer (null, because the recording
  item-group factory leaves it null), the one-based position (1), the digits value (1), the mail
  item (by reference), the value of the panel-cell states property (by reference), and the carried
  handler (null).
- New case: `EnqueueAsync_WithThreeItems_AwaitsInitializeOncePerRow`, which verifies the item
  controller's initialize member is called exactly once on each of the three mocks the recording
  factory returned.
