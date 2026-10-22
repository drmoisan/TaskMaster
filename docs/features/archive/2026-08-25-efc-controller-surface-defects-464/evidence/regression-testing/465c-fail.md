# [P7-T5] #465 C fail-before evidence — the trash row accumulates on a repeated delete gesture

Timestamp: 2026-08-28T01-17
Task: [P7-T5] [expect-fail]
Command: `& "<resolved vstest.console.exe>" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~WithTrashRow_AppliedTwice_YieldsExactlyOneTrashRow|FullyQualifiedName~ActionDeleteAsync_AwaitedTwice_LeavesExactlyOneTrashRowInFolderRows" "/Logger:trx;LogFileName=465c-fail.trx" /ResultsDirectory:docs\features\active\efc-controller-surface-defects-464\evidence\regression-testing\p7-t5` under `pwsh -NoProfile` from the worktree root
EXIT_CODE: 1
ExpectedExitCode: 1

## Preceding intermediate build

Command: `& "<resolved MSBuild.exe>" "QuickFiler.Test\QuickFiler.Test.csproj" /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo /v:m`
EXIT_CODE: 0

## Result — TRX `<Counters>`, verbatim

```
total="2" executed="2" passed="0" failed="2" error="0" timeout="0" aborted="0"
inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0"
warning="0" completed="0" inProgress="0" pending="0"
```

Total executed: **2** (non-zero, per the non-vacuity rule). Failed: **2**.

## Enumerated result names and outcomes

| # | Result name | Outcome | Failure message (verbatim) |
|---|---|---|---|
| 1 | `WithTrashRow_AppliedTwice_YieldsExactlyOneTrashRow` | **Failed** | `Expected twice.Where(row => row == EfcFormController.TrashRowText) to contain 1 item(s) because the trash row must not accumulate on a repeated delete gesture, but found 2: {"Trash to Delete", "Trash to Delete"}.` |
| 2 | `ActionDeleteAsync_AwaitedTwice_LeavesExactlyOneTrashRowInFolderRows` | **Failed** | `Expected folderRows.Where(row => row == EfcFormController.TrashRowText) to contain 1 item(s) because a repeated delete gesture must not accumulate trash rows, but found 2: {"Trash to Delete", "Trash to Delete"}.` |

Row 2 **reports two trash rows where one was expected**, exactly as the task requires. That is the
fail-before evidence for the repeated delete gesture.

Row 2 drives the criterion's literal instrument: it awaits `ActionDeleteAsync()` itself, twice, and does
not call `ApplyDeleteGesture` directly. Three mechanics make it runnable headlessly:

1. A `new SynchronizationContext()` injected into the viewer's private `_context` field satisfies the
   `SynchronizationContextAwaiter` null guard, so `await _formViewer.UiSyncContext` completes.
2. The `EfcViewer` was produced by `FormatterServices.GetUninitializedObject`, so it runs no
   constructor and has no handle, no controls and no message pump. No `Form` is constructed, `Show()` is
   never called and `Handle` is never read by either test.
3. `_router` is left null, so `BindFolderRows` returns at its guard without constructing a router or
   touching the breadcrumb host, while `ApplyDeleteGesture` has already assigned `_folderRows` — which
   is what makes the accumulation observable.

## Artifacts

TRX: `docs/features/active/efc-controller-surface-defects-464/evidence/regression-testing/p7-t5/465c-fail.trx`,
sanitised (worktree path to `<repo-root>`, account to `<user>`, machine to `<host>`). The `/InIsolation`
`Deploy_*` scratch tree written into that directory was deleted.

Output Summary: EXPECT-FAIL SATISFIED. 2 executed, 2 failed, EXIT_CODE 1 against ExpectedExitCode 1. Both
tests report two trash rows where one was expected, against the defect-preserving unconditional
`WithTrashRow`.
