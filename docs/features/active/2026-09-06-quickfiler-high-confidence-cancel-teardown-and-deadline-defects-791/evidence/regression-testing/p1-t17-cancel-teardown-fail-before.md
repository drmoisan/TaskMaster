# [P1-T17] [expect-fail] `QfcFormControllerCancelTeardownTests`, before the fix

Timestamp: 2026-09-06T14-47

Command:

```
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\791-p1t17' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:FullyQualifiedName~QfcFormControllerCancelTeardownTests'
```

`$vstest` was re-bound inside this command block by the two R10 resolution lines; the resolved value
reduced per R3 is `<program-files>\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.

ExpectedExitCode: 1
EXIT_CODE: 1

Output Summary: `Total tests: 8, Passed: 2, Failed: 6. Test Run Failed. Total time: 1.7859 Seconds.`

FAIL-BEFORE-COUNT: 6

## Failing tests, by fully qualified name, with failure messages reduced per R3

All names are in `QuickFiler.Controllers.Tests.QfcFormControllerCancelTeardownTests`. No message
below contains a host path, a user profile segment or a machine name.

1. `ActionCancelAsync_UnregistersHandlersBeforeGroupsCleanup`
   — `Expected FirstIndexOf(MarkerUnregisterNavigation) to be greater than or equal to 0 because the
   navigation ledger must be drained on Cancel, but found -1 (difference of -1).` The marker index
   is -1 because `IQfcCollectionController.UnregisterNavigation()` is never called on the Cancel
   path at all today. This is the test [P1-T17]'s acceptance names.
2. `ActionCancelAsync_ResetsKbdActive_WhenKeyboardDialogActive`
   — `Moq.MockException: an active keyboard dialog must be toggled off before the form goes away /
   Expected invocation on the mock once, but was 0 times: x => x.ToggleKeyboardDialog()`.
3. `ActionCancelAsync_ParksFocusAndCancelsBreadcrumbSelectors`
   — `Moq.MockException: Expected invocation on the mock once, but was 0 times:
   x => x.ParkFocusOffWebView2()`.
4. `ActionCancelAsync_AwaitsLoaderQuiesceBeforeGroupsCleanup`
   — `Moq.MockException: Expected invocation on the mock once, but was 0 times:
   x => x.QuiesceLoaderAsync(It.IsAny<TimeSpan>())`.
5. `ActionCancelAsync_GroupsCleanupThrows_StillInvokesParentCleanup`
   — `Did not expect any exception because a failing stage must not abort the teardown, but found
   System.InvalidOperationException: groups cleanup failed`. The throw from `_groups.Cleanup()`
   escapes `ActionCancelAsync` today, so `Cleanup()` — and through it the ribbon release callback —
   never runs.
6. `ButtonCancel_Click_ActionThrows_DoesNotRethrow`
   — `Expected capturing.Captured to be empty because a teardown failure must be logged, not
   rethrown into the Outlook UI thread, but found at least one item {System.NullReferenceException:
   Object reference not set to an instance of an object.` This confirms D12 directly: the throw
   originates inside the handler's own `try`, and the `throw;` at the end of the catch re-raises it.
   Because the handler is `async void`, the re-raise is posted to the captured
   `SynchronizationContext` rather than returned to the caller, which is precisely why the test
   installs a capturing context: without it the escape would land on the thread pool and the
   assertion could not observe it.

## Passing tests in this run, and why that is correct

- `ActionCancelAsync_DoesNotToggle_WhenInactive` — a negative control. `ToggleKeyboardDialog()` is
  not called today for any reason, so `Times.Never` holds vacuously before the fix and
  substantively after it. It is the control that keeps test 2 from being satisfiable by an
  unconditional toggle.
- `ActionCancelAsync_CalledTwice_InvokesParentCleanupOnce` — the capture pin for D6. Repeat
  invocation is already inert today, because the first pass nulls `_parent`, `_groups`,
  `_formViewer` and `_parentCleanup`. The test exists to pin that property so the Phase 2 rewrite
  cannot lose it, not to describe a defect, so it is green on both sides by design. This is the
  claim D6 says is pinned by an added test rather than asserted.
