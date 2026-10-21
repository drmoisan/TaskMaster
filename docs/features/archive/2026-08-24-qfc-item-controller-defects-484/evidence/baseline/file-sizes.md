# Phase 0 — Baseline Line Counts of the Nine Owned Files

Timestamp: 2026-08-26T08-38
Task: [P0-T15]

Command: `wc -l <the nine owned file paths>`
EXIT_CODE: 0

## Rows

| # | File | Expected (constraint C2) | Measured | Headroom to 500 | Match |
|---|---|---|---|---|---|
| 1 | `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` | 326 | **326** | 174 | yes |
| 2 | `QuickFiler/Controllers/QfcItemController.EventWiring.cs` | 391 | **391** | 109 | yes |
| 3 | `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 430 | **430** | 70 | yes |
| 4 | `QuickFiler/Controllers/QfcItemController.MailActions.cs` | 224 | **224** | 276 | yes |
| 5 | `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` | 497 | **497** | 3 | yes |
| 6 | `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs` | 374 | **374** | 126 | yes |
| 7 | `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` | 474 | **474** | 26 | yes |
| 8 | `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs` | 184 | **184** | 316 | yes |
| 9 | `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` | 365 | **365** | 135 | yes |

Total: 3265 lines across the nine files.

## Discrepancies

**None.** All nine measured values equal the values recorded in the plan's constraint C2 baseline table.
No discrepancy needs to be reported before Phase 1 begins.

Output Summary: Nine rows recorded; every measured line count matches the constraint C2 expectation
exactly (326, 391, 430, 224, 497, 374, 474, 184, 365).
