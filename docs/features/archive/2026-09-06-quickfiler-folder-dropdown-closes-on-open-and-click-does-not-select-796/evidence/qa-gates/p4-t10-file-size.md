# P4-T10 — File-size gate for Phase 4

Timestamp: 2026-09-07T14-17
Task: [P4-T10]
Issue: #796
Channel used: A

Command:

```
pwsh -NoProfile -Command '@("QuickFiler\Controllers\QfcFormController.Deactivate.cs","QuickFiler\Interfaces\IQfcFormViewer.cs","QuickFiler\Viewers\QfcFormViewer.cs","QuickFiler\Viewers\ItemViewer.Breadcrumb.cs","QuickFiler.Test\Controllers\QfcFormControllerDeactivateTests.cs") | ForEach-Object { $_ + " " + (Get-Content -LiteralPath $_).Count }'
```

EXIT_CODE: 0

LINE-COUNT-IDIOM: (Get-Content -LiteralPath $_).Count

This is the idiom recorded on the `LINE-COUNT-IDIOM:` line of
evidence/baseline/p0-t12-file-size-baseline.md and no other. The prohibited
`Measure-Object -Line` idiom was not used.

## Measured physical line counts

| Path | Physical lines | Ceiling | Verdict |
|---|---|---|---|
| QuickFiler/Controllers/QfcFormController.Deactivate.cs | 144 | 500 | within |
| QuickFiler/Interfaces/IQfcFormViewer.cs | 88 | 500 | within |
| QuickFiler/Viewers/QfcFormViewer.cs | 332 | 500 | within |
| QuickFiler/Viewers/ItemViewer.Breadcrumb.cs | 460 | 460 | at the plan ceiling, within |
| QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs | 305 | 500 | within |

Every recorded physical count is at most 500, and the count for
QuickFiler/Viewers/ItemViewer.Breadcrumb.cs is at most 460.

## Pre-P4-T11 statement

The count recorded above for `QuickFiler/Controllers/QfcFormController.Deactivate.cs` is the
PRE-P4-T11 value. That file changes twice in this phase: once at P4-T8, which this measurement
covers, and once again at P4-T11, the documentation repair that follows this task. Task P4-T11
appends its own re-measurement of that one path to THIS artifact under the heading
`POST-P4-T11 RE-MEASUREMENT:`. No second file-size artifact is created for the repair.

Output Summary: five paths measured with the recorded idiom; all at most 500; the item viewer at
460 against its 460 ceiling; the deactivate handler figure is the pre-P4-T11 value.

---

## POST-P4-T11 RE-MEASUREMENT:

Timestamp: 2026-09-07T14-19

Command (same P0-T12 form, same recorded idiom):

```
pwsh -NoProfile -Command '(Get-Content -LiteralPath QuickFiler\Controllers\QfcFormController.Deactivate.cs).Count'
```

EXIT_CODE: 0

| Path | Pre-P4-T11 | Post-P4-T11 | Ceiling | Verdict |
|---|---|---|---|---|
| QuickFiler/Controllers/QfcFormController.Deactivate.cs | 144 | 150 | 500 | within |

The six-line increase is the net effect of a comment-only repair: the stranded ten-line block was
removed from one position and reinserted at another, which is line-neutral, and the corrected
`activeFormIsNull` parameter description is six lines longer than the refuted one it replaces.
