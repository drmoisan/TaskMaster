# Phase 2 — pass-after evidence for the #466 EfcViewer removals

Timestamp: 2026-08-28T00-19
Task: [P2-T6]
Command: `& "<resolved MSBuild.exe>" "QuickFiler.Test\QuickFiler.Test.csproj" /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU`, then `& "<resolved vstest.console.exe>" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~AreAbsentFromEfcViewerMetadata|FullyQualifiedName~EditFiltersMenuItemClick_IsAbsentFromEfcViewerMetadata|FullyQualifiedName~FormEditFiltersMenuItemClick_IsStillDeclaredOnEfcFormController" "/Logger:trx;LogFileName=466-viewer-pass.trx" /ResultsDirectory:docs\features\active\efc-controller-surface-defects-464\evidence\regression-testing\p2-t6`, both under `pwsh -NoProfile`
EXIT_CODE: 0

Build exit code: 0.

## Counters

```
total="3" executed="3" passed="3" failed="0" error="0" timeout="0" aborted="0" inconclusive="0"
passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0" warning="0" completed="0"
inProgress="0" pending="0"
```

Total executed: **3**. Failed: **0**.

## The three distinct results, all `Passed`

| Test | Outcome | Duration |
|---|---|---|
| `SetControllerAndFormControllerField_AreAbsentFromEfcViewerMetadata` | Passed | 30 ms |
| `EditFiltersMenuItemClick_IsAbsentFromEfcViewerMetadata` | Passed | 3 ms |
| `FormEditFiltersMenuItemClick_IsStillDeclaredOnEfcFormController` | Passed | < 1 ms |

The first two were **red** in `[P2-T2]` against the identical assertions, so the transition is
attributable to `[P2-T3]`'s deletion of `EfcViewer._formController`, `EfcViewer.SetController` and the
viewer-side `EfcViewer.EditFiltersMenuItem_Click`, and to nothing else.

The third was green before and after by design: it is the positive behaviour-preservation control that
pins the surviving live Edit Filters route on `EfcFormController`. Its remaining green after the deletion
is the evidence that removing the viewer-side duplicate did not disturb the working command.

## TRX artifact

`docs/features/active/efc-controller-surface-defects-464/evidence/regression-testing/p2-t6/466-viewer-pass.trx`

Sanitised in place: worktree paths replaced with `<repo-root>`, account and machine names replaced with
`<user>` and `<host>`; a case-insensitive search for either returns zero matches. The `/InIsolation`
`Deploy_*` scratch tree was removed.

Output Summary: 3 of 3 executed and passed, 0 failed, vstest exit code 0. The two absence assertions
flipped from red in [P2-T2] to green here; the live-route control stayed green throughout.
