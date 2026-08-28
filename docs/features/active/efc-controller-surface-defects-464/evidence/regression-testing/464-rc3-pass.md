# [P6-T14] RC3 pass-after evidence — #464 B, C and E

Timestamp: 2026-08-28T01-10
Task: [P6-T14]
Command: `& "<resolved vstest.console.exe>" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~AsyncVoidBoundary_WhenFaulted_LogsOnceAndDoesNotThrow|FullyQualifiedName~BoundaryErrorSink_DefaultDelegate_InvokesWithoutThrowing|FullyQualifiedName~PopulateFolderCombobox|FullyQualifiedName~ThrowInitializationFailure_PreservesOriginalStackTrace" "/Logger:trx;LogFileName=464-rc3-pass.trx" /ResultsDirectory:docs\features\active\efc-controller-surface-defects-464\evidence\regression-testing\p6-t14` under `pwsh -NoProfile` from the worktree root
EXIT_CODE: 0

## Preceding intermediate build

Command: `& "<resolved MSBuild.exe>" "QuickFiler.Test\QuickFiler.Test.csproj" /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo /v:m`
EXIT_CODE: 0

## Result — TRX `<Counters>`, verbatim

```
total="12" executed="12" passed="12" failed="0" error="0" timeout="0" aborted="0"
inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0"
warning="0" completed="0" inProgress="0" pending="0"
```

Total executed: **12** (non-zero, per the non-vacuity rule). Failed: **0**.

## Recorded discrepancy against the plan's projected count

`[P6-T14]` projects "the total executed count is 9 (the five data rows, the default-sink test, the two
`PopulateFolderCombobox` tests including the pre-existing one, and the stack-trace test)". The observed
count is **12**, not 9. The nine results the task enumerates are all present and all `Passed`; the
additional three are pre-existing tests in a **different test class** that the filter also matched.

Cause: vstest's `~` operator is **case-insensitive**, so the clause
`FullyQualifiedName~PopulateFolderCombobox` (lower-case `b`) also matches the `PopulateFolderComboBox`
(upper-case `B`) tests in `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs`, class
`QfcItemController_FolderHandlingTests`. That file is outside this feature's owned set and is untouched
by this feature; the three tests are present at the pre-Phase-5 HEAD `25924673` and pass both before and
after this phase. They are reported here rather than filtered out, because narrowing the command would
depart from the plan's stated command text.

The discrepancy is in the **projection**, not in the outcome: every named result the task requires is
green and nothing is red.

## Enumerated result names and outcomes — the nine the task names

| # | Result name | Outcome |
|---|---|---|
| 1 | `AsyncVoidBoundary_WhenFaulted_LogsOnceAndDoesNotThrow ("ButtonCancelClickAsync")` | Passed |
| 2 | `AsyncVoidBoundary_WhenFaulted_LogsOnceAndDoesNotThrow ("ButtonOkClickAsync")` | Passed |
| 3 | `AsyncVoidBoundary_WhenFaulted_LogsOnceAndDoesNotThrow ("ButtonRefreshClickAsync")` | Passed |
| 4 | `AsyncVoidBoundary_WhenFaulted_LogsOnceAndDoesNotThrow ("ButtonCreateClickAsync")` | Passed |
| 5 | `AsyncVoidBoundary_WhenFaulted_LogsOnceAndDoesNotThrow ("ButtonDeleteClickAsync")` | Passed |
| 6 | `BoundaryErrorSink_DefaultDelegate_InvokesWithoutThrowing` | Passed |
| 7 | `PopulateFolderCombobox_WhenDataModelFaults_LogsOnceAndDoesNotFault` | Passed |
| 8 | `PopulateFolderCombobox_WhenFormViewerIsNull_ReturnsWithoutTouchingDataModel` (pre-existing) | Passed |
| 9 | `ThrowInitializationFailure_PreservesOriginalStackTrace` | Passed |

## The three additional pre-existing results the case-insensitive filter also matched

| # | Result name | Owning class | Outcome |
|---|---|---|---|
| 10 | `PopulateFolderComboBox_WhenFactorySucceeds_LoadsHandlerAndAssignsComboFromViewer` | `QfcItemController_FolderHandlingTests` | Passed |
| 11 | `PopulateFolderComboBox_WhenInvokeRequired_MarshalsAssignFolderComboBoxViaInvoke` | `QfcItemController_FolderHandlingTests` | Passed |
| 12 | `PopulateFolderComboBoxAsync_WhenFactorySucceeds_DispatchesAssignFolderComboBoxThroughViewerDispatcher` | `QfcItemController_FolderHandlingTests` | Passed |

## What the pass demonstrates

- **#464 B.** All five extracted `internal async Task` boundary members now contain their fault and
  report it through `BoundaryErrorSink` exactly once. All five were recorded red in `464b-fail.md`.
- **#464 C.** `PopulateFolderCombobox` no longer returns a faulted task when its collaborator faults; it
  logs once through the sink and returns. It was recorded red in `464c-fail.md`. The pre-existing
  null-viewer early-return assertion (row 8) is unweakened: it stays inside the `try` and stays first.
- **#464 E.** `ThrowInitializationFailure` rethrows the same instance with its original stack trace
  intact, so the originating frame survives. It was recorded red in `464e-fail.md`.

## Artifacts

TRX: `docs/features/active/efc-controller-surface-defects-464/evidence/regression-testing/p6-t14/464-rc3-pass.trx`,
sanitised (worktree path to `<repo-root>`, account to `<user>`, machine to `<host>`). The `/InIsolation`
`Deploy_*` scratch tree written into that directory was deleted.

Output Summary: PASS. 12 executed, 12 passed, 0 failed, EXIT_CODE 0. All nine results the task enumerates
are green, each having been recorded red in its fail-before artifact. The count is 12 rather than the
projected 9 because vstest's `~` operator is case-insensitive and also matched three pre-existing
`PopulateFolderComboBox` tests in `QfcItemController_FolderHandlingTests`, a class outside this feature's
owned set; those three also pass and are recorded rather than filtered out.
