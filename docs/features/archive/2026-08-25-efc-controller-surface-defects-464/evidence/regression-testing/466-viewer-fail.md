# Phase 2 — fail-before evidence for the #466 EfcViewer removals

Timestamp: 2026-08-28T00-17
Task: [P2-T2] [expect-fail]
Command: `& "<resolved MSBuild.exe>" "QuickFiler.Test\QuickFiler.Test.csproj" /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU`, then `& "<resolved vstest.console.exe>" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~AreAbsentFromEfcViewerMetadata|FullyQualifiedName~EditFiltersMenuItemClick_IsAbsentFromEfcViewerMetadata|FullyQualifiedName~FormEditFiltersMenuItemClick_IsStillDeclaredOnEfcFormController" "/Logger:trx;LogFileName=466-viewer-fail.trx" /ResultsDirectory:docs\features\active\efc-controller-surface-defects-464\evidence\regression-testing\p2-t2`, both under `pwsh -NoProfile`
EXIT_CODE: 1
ExpectedExitCode: 1

A failing run is the intended outcome of this task. Build exit code was 0, so the two red results are
genuine assertion failures rather than a compile failure.

## Counters

```
total="3" executed="3" passed="1" failed="2" error="0" timeout="0" aborted="0" inconclusive="0"
passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0" warning="0" completed="0"
inProgress="0" pending="0"
```

Total executed: **3**, which is greater than zero, satisfying the non-vacuity rule.

## The three results

| Test | Outcome | Evidence it establishes |
|---|---|---|
| `SetControllerAndFormControllerField_AreAbsentFromEfcViewerMetadata` | **Failed** | Expected `setController` to be `<null>` ... but found `EfcViewer.SetController`. The dead member is still present. |
| `EditFiltersMenuItemClick_IsAbsentFromEfcViewerMetadata` | **Failed** | Expected `viewerSideHandlers` to be empty ... but found at least one item `{EfcViewer.EditFiltersMenuItem_Click}`. The dead viewer-side handler is still present. |
| `FormEditFiltersMenuItemClick_IsStillDeclaredOnEfcFormController` | **Passed** | The live form-controller handler already exists, so the Edit Filters command is functional **before** this feature touches anything. |

This is exactly the split `[P2-T2]` specifies: the two absence tests red, the live-route test green. Read
together they establish both halves of the RC11-A premise — the two viewer-side members are present and
dead, and the command they appear to serve is already delivered elsewhere. The third result is the
positive behaviour-preservation control that `spec.md` risk R-3 mitigation 6 requires, and it is green
both before and after the deletion by design.

## TRX artifact

`docs/features/active/efc-controller-surface-defects-464/evidence/regression-testing/p2-t2/466-viewer-fail.trx`

Sanitised in place: worktree paths replaced with `<repo-root>`, account and machine names replaced with
`<user>` and `<host>`; a case-insensitive search for either returns zero matches. The `Deploy_*` scratch
tree created by `/InIsolation` was removed.

Output Summary: 3 executed, 2 failed, 1 passed, vstest exit code 1 as expected. The two failures name
`EfcViewer.SetController` and `EfcViewer.EditFiltersMenuItem_Click` as still present; the passing control
confirms `EfcFormController.EditFiltersMenuItem_Click` already exists.
