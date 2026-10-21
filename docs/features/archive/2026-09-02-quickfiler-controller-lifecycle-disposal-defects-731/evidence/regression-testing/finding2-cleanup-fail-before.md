# Finding 2 — undo-queue disposal ordering, failing run before the fix

Timestamp: 2026-09-03T14-15

Task: [P2-T3] [expect-fail]
Issue: #731

## Command

1. Build:

```
msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
```

MSBuild executable actually invoked: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`. Recording this absolute path in full is the narrow exception the Evidence path-hygiene rule states for an external build-tool executable outside this worktree, under `Program Files`, containing no account name.

2. Filtered test run:

```
<vstest> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcFormControllerCleanupTests"
```

vstest console: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe` (VSTest version 18.9.0, x64), resolved through the standard vswhere invocation.

EXIT_CODE: 1

ExpectedExitCode: 1

The build exited 0. The test run exited 1, which is the expected outcome for this task: the fix has not been applied yet.

The build succeeding at this point is itself part of the design. `Cleanup_WithFaultedConsumer_ObservesAndLogsTheFault` reaches the not-yet-existing `_undoQueueDisposal` field through a string-keyed reflection lookup rather than a compile-time member reference, so the field's absence is a runtime failure rather than a compile error.

## Output Summary

```
Total tests: 7
     Passed: 4
     Failed: 3
Test Run Failed.
```

All absolute paths inside the quoted diagnostics below have been rewritten to their repository-relative remainder under the Evidence path-hygiene rule. Exception type names and exception messages are left intact, so the `ObjectDisposedException` this task gates on remains present in the recorded text.

### Failed: `Cleanup_WithRunningConsumer_ConsumerReachesRanToCompletion`

Observed failure message:

```
Expected consumer.Status to be TaskStatus.RanToCompletion {value: 5} because issue #731 finding 2
requires the consumer to reach RanToCompletion; observed fault was System.AggregateException: One or
more errors occurred. ---> System.ObjectDisposedException: The collection has been disposed.
Object name: 'BlockingCollection'.
   at System.Collections.Concurrent.BlockingCollection`1.CheckDisposed()
   at System.Collections.Concurrent.BlockingCollection`1.get_IsCompleted()
   at QuickFiler.Controllers.QfcFormController.<UndoConsumer>d__102.MoveNext() in
QuickFiler\Controllers\QfcFormController.Actions.cs:line 322
   --- End of inner exception stack trace ---
, but found TaskStatus.Faulted {value: 7}.
```

This is the defect issue #731 finding 2 describes, reproduced exactly. `Cleanup()` disposes `_undoQueue` outright at `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs:218` while the consumer is parked, and the consumer's next evaluation of `_undoQueue.IsCompleted` at `QuickFiler/Controllers/QfcFormController.Actions.cs:322` throws `ObjectDisposedException`, faulting the consumer task instead of letting it exit its loop normally.

### Failed: `Cleanup_WithRunningConsumer_CompletesAddingBeforeDisposing`

Observed failure message:

```
Test method QuickFiler.Controllers.Tests.QfcFormControllerCleanupTests.
Cleanup_WithRunningConsumer_CompletesAddingBeforeDisposing threw exception:
System.ObjectDisposedException: The collection has been disposed.
Object name: 'BlockingCollection'.
   at System.Collections.Concurrent.BlockingCollection`1.CheckDisposed()
   at System.Collections.Concurrent.BlockingCollection`1.get_IsAddingCompleted()
```

Reading `IsAddingCompleted` immediately after `Cleanup()` returns throws, because the queue has already been disposed. There is no state in which `CompleteAdding()` ran before disposal on the pre-fix tree, since `CompleteAdding()` is never called at all.

### Failed: `Cleanup_WithFaultedConsumer_ObservesAndLogsTheFault`

Observed failure message:

```
Expected field not to be <null> because issue #731 finding 2 requires the private field
_undoQueueDisposal to exist on QfcFormController.
```

The deferred-disposal handle does not exist on the pre-fix tree, so the continuation whose fault-logging branch this test exercises does not exist either.

## Recorded, not gated: the four forward guards

These four are forward guards rather than reproductions, and the plan records their state here without gating on it.

```
  Passed Cleanup_WithNullConsumerTask_DisposesQueueAndDoesNotThrow [4 ms]
  Passed Cleanup_WithParkedConsumer_ReturnsWithoutWaiting [1 ms]
  Passed Cleanup_CalledTwice_DoesNotThrow [< 1 ms]
  Passed Cleanup_SourceContainsNoSynchronousWait [1 ms]
```

All four passed in this pre-fix run, which is expected: the pre-fix `Cleanup()` also disposes the queue when no consumer is in flight, also returns without blocking, is also safe to call twice because `BlockingCollection.Dispose()` is idempotent and every other statement is null-guarded, and already contains none of the four synchronous-wait literals.

## Verdict

The reproduction is genuine. The three tests the plan requires to fail did fail, and the first carries an `ObjectDisposedException` in its recorded diagnostic text. The fix at [P2-T4] may proceed.
