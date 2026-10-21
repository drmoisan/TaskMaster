# Issue #483 — Propagation and Cancellation Tests Fail Against the Unfixed Code

Timestamp: 2026-08-26T09-31
Task: [P3-T4] [expect-fail]

ExpectedExitCode: 1

## Build step

Command (under `pwsh -NoProfile`, `[P0-T4]`-resolved MSBuild):

```
MSBuild.exe QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU
```

EXIT_CODE: 0 (3 warnings, 0 errors). Not an analyzer or nullable gate (decision D2).

## Test step

Command (under `pwsh -NoProfile`, `[P0-T4]`-resolved `vstest.console.exe`):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~MoveMailAsync_When|FullyQualifiedName~MoveMailAsync_With|FullyQualifiedName~FlagAsTaskAsync_When|FullyQualifiedName~EnumerateConversationAsync_When" "/Logger:trx;LogFileName=483-fail.trx" /ResultsDirectory:docs\features\active\qfc-item-controller-defects-484\evidence\regression-testing\483-fail
```

EXIT_CODE: **1**

```
Total tests: 9
     Passed: 3
     Failed: 6
Test Run Failed.
```

## Results

| Test | Outcome | Failure reason |
|---|---|---|
| `MoveMailAsync_WhenFilerFactoryThrows_WrapsAndRethrowsWithInnerException` | **Failed** | `Expected a <System.InvalidOperationException> to be thrown, but no exception was thrown.` |
| `MoveMailAsync_WhenEnqueueThrows_WrapsArgumentNullException` | **Failed** | `Expected a <System.InvalidOperationException> to be thrown, but no exception was thrown.` |
| `MoveMailAsync_WithUiDispatcher_MarshalsNotificationThroughDispatcher` | **Failed** | `Expected a <System.InvalidOperationException> to be thrown, but no exception was thrown.` |
| `MoveMailAsync_WhenTokenAlreadyCancelled_ThrowsAndNeverInvokesFilerFactory` | **Failed** | `Expected a <System.OperationCanceledException> to be thrown, but no exception was thrown.` |
| `FlagAsTaskAsync_WhenTokenAlreadyCancelled_Throws` | **Failed** | `Expected a <System.OperationCanceledException> to be thrown, but found <System.NullReferenceException>` |
| `EnumerateConversationAsync_WhenTokenAlreadyCancelled_Throws` | **Failed** | `Expected a <System.OperationCanceledException> to be thrown, but found <System.NullReferenceException>` |
| `MoveMailAsync_WhenItemHelperNull_DoesNotInvokeFactory` (pre-existing) | Passed | — |
| `MoveMailAsync_WhenOneDriveMissing_ReturnsWithoutInvokingFactory` (pre-existing) | Passed | — |
| `MoveMailAsync_WhenOneDrivePresent_InvokesFactoryWithConfigAndEnqueues` (pre-existing) | Passed | — |

The acceptance text requires `MoveMailAsync_WhenFilerFactoryThrows_WrapsAndRethrowsWithInnerException`
and all three cancellation tests to be recorded with outcome `Failed`. All four are, and the remaining
two new tests fail as well.

## Interpretation

- **The swallow.** The three `MoveMailAsync` failure-path tests report that *no* exception was thrown.
  That is the #483 defect exactly: the broad `catch (System.Exception e)` logs, shows the message, and
  returns normally, so the faulted move is indistinguishable from a successful one at the caller. The
  `MoveFailureNotifier` seam introduced defect-preservingly in `[P3-T1]` is already in place, which is why
  the tests reach their assertion rather than opening a modal dialog.
- **The missing cancellation checks.** `MoveMailAsync` reports no exception, because with a cancelled
  `Token` and no check it simply runs the whole method. `FlagAsTaskAsync` and
  `EnumerateConversationAsync` report `NullReferenceException` instead of
  `OperationCanceledException`, because with no cancellation check the first statement they reach
  dereferences a collaborator (`Mail` and `_uiDispatcher` respectively) that the test deliberately leaves
  unset. In both cases the absent `Token.ThrowIfCancellationRequested()` is what allows execution to
  proceed at all.
- **No regression in the pre-existing tests.** The three live `MoveMailAsync` tests in
  `QfcItemController.SeamFactoryTests.cs` remain `Passed` in the same run, confirming the
  `[P3-T1]` seam introduction changed no behaviour.

TRX:
`docs/features/active/qfc-item-controller-defects-484/evidence/regression-testing/483-fail/483-fail.trx`

Output Summary: `EXIT_CODE: 1`, 9 total, 6 failed. All six new #483 tests fail against the unfixed code
and the three pre-existing `MoveMailAsync` tests stay green. This is the fail-before evidence for
issue #483.
