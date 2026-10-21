# [P1-T19] [expect-fail] `QfcDatamodelTeardownTests`, before the fix

Timestamp: 2026-09-06T14-47

Command:

```
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\791-p1t19' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:FullyQualifiedName~QfcDatamodelTeardownTests'
```

`$vstest` was re-bound inside this command block by the two R10 resolution lines; the resolved value
reduced per R3 is `<program-files>\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.

ExpectedExitCode: 1
EXIT_CODE: 1

Output Summary: `Total tests: 5, Failed: 5. Test Run Failed. Total time: 1.6116 Seconds.` All five
tests in the class are red, which is the required outcome for this task.

FAIL-BEFORE-COUNT: 5

## Failing tests, by fully qualified name, with exception types recorded

All names are in `QuickFiler.Controllers.Tests.QfcDatamodelTeardownTests`. No message below carries
a host path.

1. `TryQueueRemainingMailItemAsync_AfterCleanupNulledFields_ReturnsFalseWithoutThrowing`
   — exception type `System.ArgumentException`.
   `Did not expect any exception because a released field must be a refusal at the accept point, not
   a throw, but found System.ArgumentException: Delegate to an instance method cannot have null
   'this'.`
   This is character-for-character the failure mode `issue.md` records in the production log
   (`Delegate to an instance method cannot have null 'this'`, raised from
   `TryQueueRemainingMailItemAsync` while constructing `QfcRemainingQueueAdmission` over
   `_masterQueue.AddLast` and `_moveMonitor.HookItem`). The reported defect is reproduced
   deterministically and without Outlook. This is the named fail-before evidence AC3 requires.

2. `Cleanup_CalledTwice_DoesNotThrow`
   — exception type `System.NullReferenceException`.
   `Did not expect any exception because repeat teardown must be inert, not a fault on released
   fields, but found System.NullReferenceException: Object reference not set to an instance of an
   object.` The unguarded `_globals.Ol.App.NewMailEx -=` and `_moveMonitor.UnhookAll()` in
   `Cleanup()` raise on the first call once those fields are already released.

3. `QuiesceLoaderAsync_LoaderCompletes_ReturnsBeforeTimeout`
   — assertion failure: `Expected field not to be <null> because private field
   '_remainingLoadTask' should exist on QfcDatamodel.`

4. `QuiesceLoaderAsync_LoaderHangs_ReturnsAtBoundAndLogs`
   — assertion failure: `Expected field not to be <null> because private field
   '_remainingLoadTask' should exist on QfcDatamodel.`

5. `Worker_DoWork_CapturesRemainingLoadTask`
   — assertion failure: `Expected field not to be <null> because private field
   '_remainingLoadTask' should exist on QfcDatamodel.`

All five names appear in the failure set with the cause recorded, which is this task's acceptance.

## Divergence from the failure mode this task predicted

[P1-T19] states that the two `QuiesceLoaderAsync` tests would fail with `NotImplementedException`
from the [P1-T2] seam. They fail one step earlier than that, and the reason is structural rather
than a defect in either the plan or the tests.

Both tests inject `_remainingLoadTask` before calling `QuiesceLoaderAsync`, because
[P1-T14] specifies that injection as part of their arrangement — a completed task for the completion
case and a never-completing `TaskCompletionSource` task for the bound case. That field is added by
[P2-T4], not by the Phase 1 seams, so at the end of Phase 1 the reflective field lookup in the
shared `SetPrivateField` helper returns null and its fail-closed
`.Should().NotBeNull(...)` guard fires during Arrange. The seam's
`NotImplementedException` sits in Act, which is never reached.

`Worker_DoWork_CapturesRemainingLoadTask` is red for the same reason on its read side, which is what
[P1-T20] tags `SEAM-BLOCKED`.

The consequence is confined to which line of these two tests reports first. Both remain red before
the change and both must be green after it, so the fail-before/pass-after evidence they carry is
unaffected, and no acceptance condition of this task or of AC2 or AC3 depends on the exception type
being `NotImplementedException`. The divergence is recorded here rather than silently absorbed, and
is repeated in the execution report.
