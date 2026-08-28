# [P6-T8] #464 C fail-before evidence — `PopulateFolderCombobox` returns a faulted Task

Timestamp: 2026-08-28T01-07
Task: [P6-T8] [expect-fail]
Command: `& "<resolved vstest.console.exe>" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~PopulateFolderCombobox_WhenDataModelFaults_LogsOnceAndDoesNotFault" "/Logger:trx;LogFileName=464c-fail.trx" /ResultsDirectory:docs\features\active\efc-controller-surface-defects-464\evidence\regression-testing\p6-t8` under `pwsh -NoProfile` from the worktree root
EXIT_CODE: 1
ExpectedExitCode: 1

## Preceding intermediate build

Command: `& "<resolved MSBuild.exe>" "QuickFiler.Test\QuickFiler.Test.csproj" /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo /v:m`
EXIT_CODE: 0

## Result — TRX `<Counters>`, verbatim

```
total="1" executed="1" passed="0" failed="1" error="0" timeout="0" aborted="0"
inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0"
warning="0" completed="0" inProgress="0" pending="0"
```

Total executed: **1** (non-zero, per the non-vacuity rule). Failed: **1**.

## Enumerated result name and outcome

| # | Result name | Outcome | Failure reason |
|---|---|---|---|
| 1 | `PopulateFolderCombobox_WhenDataModelFaults_LogsOnceAndDoesNotFault` | **Failed** | the `NotThrowAsync` assertion failed: **the returned task faulted**, because the method body carries no `try`/`catch` and the first collaborator call reaches a null `_dataModel` |

This is the fail-before evidence for #464 C. The method is invoked fire-and-forget from
`EfcFormController.cs:95` and `:115` with a discarded result, so nothing on the call path can observe
the faulted task; the fault must be contained inside the method.

The injected `EfcViewer` was produced by `FormatterServices.GetUninitializedObject`, so it runs no
constructor and has no handle, no controls and no message pump. It is never shown, its `Handle` is never
read, and `CreateControl()` is never called. Its only purpose is to get past the pre-existing
null-viewer early return so the fault reaches the collaborator call.

The pre-existing test `PopulateFolderCombobox_WhenFormViewerIsNull_ReturnsWithoutTouchingDataModel`
(`EfcFormControllerTests.cs:40`) is unmodified.

## Artifacts

TRX: `docs/features/active/efc-controller-surface-defects-464/evidence/regression-testing/p6-t8/464c-fail.trx`,
sanitised (worktree path to `<repo-root>`, account to `<user>`, machine to `<host>`). The `/InIsolation`
`Deploy_*` scratch tree written into that directory was deleted.

Output Summary: EXPECT-FAIL SATISFIED. 1 executed, 1 failed, EXIT_CODE 1 against ExpectedExitCode 1. The
task returned by `PopulateFolderCombobox` faulted, which is the defect #464 C names.
