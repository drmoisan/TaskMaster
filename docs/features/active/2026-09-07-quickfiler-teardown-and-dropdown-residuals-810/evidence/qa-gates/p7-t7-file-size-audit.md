# [P7-T7] 500-Line Ceiling Audit Across the Write Set

Timestamp: 2026-09-08T10-25

Measured with the Read tool on the tree as it stands after the final [P7-T1] format pass. All nine production `.cs` paths and all five test `.cs` paths of the Write Set are covered, which is fourteen files.

LINES: 157 QuickFiler/Controllers/QfcFormController.Deactivate.cs
LINES: 493 QuickFiler/Controllers/QfcFormController.EventHandlers.cs
LINES: 277 QuickFiler/Controllers/QfcFormController.SetupDisposal.cs
LINES: 498 QuickFiler/Controllers/QfcHomeController.cs
LINES: 311 QuickFiler/Controllers/QfcItemController.EventHandlers.cs
LINES: 459 QuickFiler/Viewers/BreadcrumbDropDownHost.cs
LINES: 178 QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs
LINES: 61 QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs
LINES: 325 QuickFiler/Viewers/QfcFormViewer.cs
LINES: 422 QuickFiler.Test/Controllers/QfcFormControllerCancelTeardownTests.cs
LINES: 455 QuickFiler.Test/Controllers/QfcFormControllerCleanupTests.cs
LINES: 159 QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs
LINES: 337 QuickFiler.Test/Viewers/BreadcrumbDropDownCloseOrderingTests.cs
LINES: 175 QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs

MAX-LINES: 498

CEILING: MET

`MAX-LINES` is 498, which is at most 500. The largest file is `QuickFiler/Controllers/QfcHomeController.cs`, which [P0-T13] measured at 496 with 4 lines of headroom and which [P2-T4] grew by exactly the two field-nulling statements AC3 requires, leaving 2 lines of headroom. The optional rationale comment that [P2-T4] permits was deliberately not added, because the [P0-T13] projection budgeted exactly two added lines for this file and any comment block would have consumed headroom the budget did not have.

## The three files [P0-T13] flagged, and where they finished

| File | [P0-T13] baseline | Final | Movement |
| --- | --- | --- | --- |
| `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` | 490 | 493 | Grew by 3, exactly the projection: the rewritten `RunTeardownStage` call exceeds 100 columns at its indentation, so it is a four-line argument list. |
| `QuickFiler/Controllers/QfcHomeController.cs` | 496 | 498 | Grew by 2, exactly the projection. |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | 496 | 459 | Grew to 504 under Design A, which overran the ceiling and selected branch B at [P4-T8]; the relocation then removed 45 lines. |

## `QuickFiler.Test/QuickFiler.Test.csproj`

That file is 533 lines and therefore exceeds 500. It is recorded here as a pre-existing condition outside the rule's stated scope rather than as a new violation: the 500-line rule covers production code, test code and reusable script files, and a legacy non-SDK project file is none of those. This plan's [P6-T2] added one `<Compile Include>` line to it, taking it from 532 to 533; the file was already over the ceiling before this plan began and no task here could have brought it under without deleting compile items.
