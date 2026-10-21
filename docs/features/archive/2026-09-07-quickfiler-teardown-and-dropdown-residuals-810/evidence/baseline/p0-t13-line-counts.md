# [P0-T13] Pre-Change Line Counts of the Write Set

Timestamp: 2026-09-08T09-28
Command: Read tool, one measurement per file (the reported final line number of each file)
EXIT_CODE: 0
Output Summary: Twelve files were measured. Three sit at or above the 480-line threshold and are recorded with their remaining headroom to the 500-line ceiling. None exceeds 500 before any task of this plan edits it.

LINES: 150 QuickFiler/Controllers/QfcFormController.Deactivate.cs
LINES: 490 QuickFiler/Controllers/QfcFormController.EventHandlers.cs
LINES: 265 QuickFiler/Controllers/QfcFormController.SetupDisposal.cs
LINES: 496 QuickFiler/Controllers/QfcHomeController.cs
LINES: 317 QuickFiler/Controllers/QfcItemController.EventHandlers.cs
LINES: 496 QuickFiler/Viewers/BreadcrumbDropDownHost.cs
LINES: 131 QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs
LINES: 332 QuickFiler/Viewers/QfcFormViewer.cs
LINES: 393 QuickFiler.Test/Controllers/QfcFormControllerCancelTeardownTests.cs
LINES: 399 QuickFiler.Test/Controllers/QfcFormControllerCleanupTests.cs
LINES: 118 QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs
LINES: 290 QuickFiler.Test/Viewers/BreadcrumbDropDownCloseOrderingTests.cs

CEILING-HEADROOM:
- `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` — 490 lines, 10 lines of headroom. Grown by [P1-T6]: the rewritten `RunTeardownStage` call exceeds 100 columns at its indentation, so CSharpier splits it into a four-line argument list for a net growth of three lines. Projected 493, which leaves 7 lines of headroom.
- `QuickFiler/Controllers/QfcHomeController.cs` — 496 lines, 4 lines of headroom. Grown by [P2-T4] by two lines (`_tokenSource = null;` and `_datamodel = null;`). Projected 498, which leaves 2 lines of headroom.
- `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` — 496 lines, 4 lines of headroom. Grown by [P4-T4], which adds a fourth `CompleteAll` operation with a short comment and replaces one comment line with a multi-line corrected block. This is the file whose measured post-edit count decides Design A versus Design B at [P4-T7] and [P4-T8].

The threshold is at least 480 rather than greater than 490 because `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` is exactly 490 lines, and a strictly-greater test would have left the file this plan is about to grow out of the record.

## Files not near the ceiling

The remaining nine files each have at least 101 lines of headroom. `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` at 265 is grown by [P3-T4]'s try/finally restructure by a small number of lines and remains far below the ceiling. `QuickFiler/Viewers/QfcFormViewer.cs` at 332 is reduced by [P6-T6]. The three test files edited in Phases 1 through 4 grow by one test each and stay below 500; their post-change counts are asserted by their own task acceptance clauses and again at [P7-T7].
