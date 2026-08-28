# Issue #483 — Propagation and Cancellation Tests Pass After the Fix

Timestamp: 2026-08-26T09-36
Task: [P3-T7]

## Build step

Command (under `pwsh -NoProfile`, `[P0-T4]`-resolved MSBuild):

```
MSBuild.exe QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU
```

EXIT_CODE: 0 (3 warnings, 0 errors). Not an analyzer or nullable gate (decision D2).

## Test step

Command (under `pwsh -NoProfile`, `[P0-T4]`-resolved `vstest.console.exe`):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~MoveMailAsync|FullyQualifiedName~FlagAsTaskAsync|FullyQualifiedName~EnumerateConversation" "/Logger:trx;LogFileName=483-pass.trx" /ResultsDirectory:docs\features\active\qfc-item-controller-defects-484\evidence\regression-testing\483-pass
```

EXIT_CODE: **0**

## Counts

| Metric | Value |
|---|---|
| Total | 12 |
| Passed | 12 |
| **Failed** | **0** |

```
Test Run Successful.
Total tests: 12
     Passed: 12
```

## The six new tests

| Test | Outcome |
|---|---|
| `QfcItemController_MailActionsTests.MoveMailAsync_WhenFilerFactoryThrows_WrapsAndRethrowsWithInnerException` | **Passed** |
| `QfcItemController_MailActionsTests.MoveMailAsync_WhenEnqueueThrows_WrapsArgumentNullException` | **Passed** |
| `QfcItemController_MailActionsTests.MoveMailAsync_WithUiDispatcher_MarshalsNotificationThroughDispatcher` | **Passed** |
| `QfcItemController_MailActionsTests.MoveMailAsync_WhenTokenAlreadyCancelled_ThrowsAndNeverInvokesFilerFactory` | **Passed** |
| `QfcItemController_MailActionsTests.FlagAsTaskAsync_WhenTokenAlreadyCancelled_Throws` | **Passed** |
| `QfcItemController_MailActionsTests.EnumerateConversationAsync_WhenTokenAlreadyCancelled_Throws` | **Passed** |

## The three pre-existing `MoveMailAsync` tests in `QfcItemController.SeamFactoryTests.cs`

| Test | Outcome |
|---|---|
| `QfcItemController_SeamFactoryTests.MoveMailAsync_WhenItemHelperNull_DoesNotInvokeFactory` | **Passed** |
| `QfcItemController_SeamFactoryTests.MoveMailAsync_WhenOneDriveMissing_ReturnsWithoutInvokingFactory` | **Passed** |
| `QfcItemController_SeamFactoryTests.MoveMailAsync_WhenOneDrivePresent_InvokesFactoryWithConfigAndEnqueues` | **Passed** |

Three further pre-existing tests that the filter also selected are green as well:
`QfcItemController_MailActionsTests.EnumerateConversation_TogglesUnGroupWithResolverEntryIdAndCount`,
`QfcItemController_SeamDispatcherTests.EnumerateConversationAsync_RunsEnumerateThroughDispatcher`, and
`QfcItemController_SeamFactoryTests.FlagAsTaskAsync_InvokesFactoryThroughDispatcher`. The last two are
the reason the three new `Token.ThrowIfCancellationRequested()` calls are safe for existing callers:
`Token` defaults to `default(CancellationToken)`, on which the call is a no-op.

## Behaviour delivered

- `[P3-T5]`: the `catch` in `MoveMailAsync` now logs at error level through the existing static
  `logger`, calls `NotifyMoveFailure`, and ends in
  `throw new System.InvalidOperationException($"Failed to file mail '{ItemHelper.Subject}' to '{SelectedFolder}'.", e)`,
  carrying the original as `InnerException`. The return type is unchanged at `Task`.
- `[P3-T6]`: `Token.ThrowIfCancellationRequested()` is the first body statement of `MoveMailAsync`
  (outside the `try`, before the `ItemHelper` null test), of `FlagAsTaskAsync` (before the COM `Mail`
  read), and of `EnumerateConversationAsync` (before the dispatcher call).
- The notification is routed through `_uiDispatcher.Invoke` when the dispatcher is non-null and invoked
  directly when it is null, which is why the pre-existing `SeamFactoryTests` `MoveMailAsync` tests that
  never set `_uiDispatcher` still pass.

TRX:
`docs/features/active/qfc-item-controller-defects-484/evidence/regression-testing/483-pass/483-pass.trx`

Output Summary: `EXIT_CODE: 0`, 12 total, 12 passed, 0 failed. All six new #483 tests pass and the three
pre-existing `MoveMailAsync` tests in `QfcItemController.SeamFactoryTests.cs` remain green.
