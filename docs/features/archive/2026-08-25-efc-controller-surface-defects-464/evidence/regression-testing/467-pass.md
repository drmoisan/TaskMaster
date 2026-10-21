# [P8-T5] #467 pass-after evidence — both mnemonics restored

Timestamp: 2026-08-28T01-30
Task: [P8-T5]
Command: `& "<resolved vstest.console.exe>" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~ClaimsAltChord|FullyQualifiedName~EfcViewerTests" "/Logger:trx;LogFileName=467-pass.trx" /ResultsDirectory:docs\features\active\efc-controller-surface-defects-464\evidence\regression-testing\p8-t5` under `pwsh -NoProfile` from the worktree root
EXIT_CODE: 0

## Preceding intermediate build

Command: `& "<resolved MSBuild.exe>" "QuickFiler.Test\QuickFiler.Test.csproj" /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo /v:m`
EXIT_CODE: 0

## Result — TRX `<Counters>`, verbatim

```
total="8" executed="8" passed="8" failed="0" error="0" timeout="0" aborted="0"
inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0"
warning="0" completed="0" inProgress="0" pending="0"
```

Total executed: **8** (non-zero, per the non-vacuity rule). Failed: **0**.

## Enumerated result names and outcomes

| # | Result name | Outcome | Issue |
|---|---|---|---|
| 1 | `SetControllerAndFormControllerField_AreAbsentFromEfcViewerMetadata` | Passed | #466 A (Phase 2) |
| 2 | `EditFiltersMenuItemClick_IsAbsentFromEfcViewerMetadata` | Passed | #466 A (Phase 2) |
| 3 | `FormEditFiltersMenuItemClick_IsStillDeclaredOnEfcFormController` | Passed | #466 A, Edit Filters invariant |
| 4 | `ClaimsAltChord_WithBareAltAndHandler_ReturnsTrue` | Passed | #467 |
| 5 | `ClaimsAltChord_WithAltF_ReturnsFalse` | Passed | #467 |
| 6 | `ClaimsAltChord_WithAltM_ReturnsFalse` | Passed | #467 |
| 7 | `ClaimsAltChord_WithNonAltChord_ReturnsFalse` | Passed | #467 |
| 8 | `ClaimsAltChord_WithNullHandler_ReturnsFalse` | Passed | #467 |

Eight distinct result names, all `Passed`, matching the task's expected count exactly. Rows 5 and 6 were
both recorded `Failed` in `467-fail.md`, so the pair is a complete fail-before / pass-after set for the
two swallowed mnemonics. Rows 1 to 3 are the Phase 2 #466 tests, re-run here and still green.

## What the pass demonstrates

The narrowed predicate claims a chord only when the handler is non-null, the key data carries the
`Keys.Alt` flag, and the key-code portion (obtained by masking with `Keys.KeyCode`) is `Keys.Menu` or
`Keys.None`. Bare Alt still reaches `ToggleKeyboardDialogAsync`; `Alt+F` and `Alt+M` now fall through
to `base.ProcessCmdKey` and open their menus.

### The narrowing does not sever `CharActions` reachability

Feature #444 records that `CharActions` is read by `KeyboardHandler_KeyDown` and is reached **only**
from the Alt-key `ProcessCmdKey` path, and #444 deliberately widened Alt+`B` and Alt+`D` availability.
That route is preserved: the predicate is scoped to `EfcViewer` and narrows only what `EfcViewer`
claims for its own keyboard dialog. The bare-Alt claim — the gesture that opens the keyboard dialog from
which `CharActions` is serviced — still returns `true` (row 4).
`QuickFiler/Controllers/KeyboardHandler.cs` is owned by #498 and was not edited.

## Artifacts

TRX: `docs/features/active/efc-controller-surface-defects-464/evidence/regression-testing/p8-t5/467-pass.trx`,
sanitised (worktree path to `<repo-root>`, account to `<user>`, machine to `<host>`). The `/InIsolation`
`Deploy_*` scratch tree written into that directory was deleted.

Output Summary: PASS. 8 executed, 8 passed, 0 failed, EXIT_CODE 0. Both menu mnemonics are restored,
bare Alt is still claimed, and the three Phase 2 #466 tests remain green.
