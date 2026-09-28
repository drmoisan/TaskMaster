# P4-T12 — move-monitor hook loop under a strict mock

Timestamp: 2026-09-13T16-27

Command: msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"

EXIT_CODE: 0

Output Summary:
- Build summary line as printed: `0 Error(s)`, read by an anchored regular expression.
- `0 Warning(s)`.

Command: & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName~QfcQueueEnqueueTests" "/Logger:trx;LogFileName=vstest-class-run.trx" /ResultsDirectory:TestResults\p4-t12

EXIT_CODE: 0

Output Summary:
- Counters element of TestResults\p4-t12\vstest-class-run.trx:
  `total=17 executed=17 passed=17 failed=0`
- New case: `EnqueueAsync_WithStrictMoveMonitor_HooksEachItemExactlyOnce`. It assigns a
  `MockBehavior.Strict` mock of the move-monitor interface through the `MoveMonitor` seam, enqueues
  two items, and verifies the hook member was called exactly once per item with that item and a
  non-null action delegate. Strict behaviour additionally proves the enqueue path calls no other
  member of the interface.
- The captured delegates are asserted non-null and are never invoked. Each is the async-void lambda
  the production hook loop supplies; invoking it would start unobservable work on the calling
  thread and its failures could not be surfaced to the test.
