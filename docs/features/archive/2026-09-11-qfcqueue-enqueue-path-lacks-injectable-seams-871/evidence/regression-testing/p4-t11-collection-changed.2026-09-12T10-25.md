# P4-T11 — both arms of the collection-changed notification

Timestamp: 2026-09-13T16-25

Command: msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"

EXIT_CODE: 0

Output Summary:
- Build summary line as printed: `0 Error(s)`, read by an anchored regular expression.
- `0 Warning(s)`.

Command: & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName~QfcQueueEnqueueTests" "/Logger:trx;LogFileName=vstest-class-run.trx" /ResultsDirectory:TestResults\p4-t11

EXIT_CODE: 0

Output Summary:
- Counters element of TestResults\p4-t11\vstest-class-run.trx:
  `total=16 executed=16 passed=16 failed=0`
- New cases: `EnqueueAsync_WithSubscriber_RaisesExactlyOneAddNotification`, which subscribes and
  asserts exactly one event whose action is `NotifyCollectionChangedAction.Add`, and
  `EnqueueAsync_WithNoSubscriber_CompletesWithoutThrowing`, which runs the same flow with no
  subscriber attached and asserts nothing is thrown and the entry is still queued.
- The second case is the null-conditional arm of the event invocation: it is the only arm reached
  when no handler is attached, and it is the state every production caller starts in.
