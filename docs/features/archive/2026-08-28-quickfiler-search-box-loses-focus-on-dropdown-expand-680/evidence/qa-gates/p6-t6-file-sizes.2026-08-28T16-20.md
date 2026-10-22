# P6-T6 — Post-Format File-Size Audit (500-line repo limit)

Timestamp: 2026-08-28T16-37

Command: `(Get-Content <file>).Count` for every file edited or created by this plan, run AFTER the
final clean P6-T1 format pass.

EXIT_CODE: 0

## Per-file line counts

| Lines | File | Limit |
|---|---|---|
| 479 | `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | <= 500 PASS |
| 83 | `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` | <= 500 PASS |
| 460 | `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` | <= 500 PASS |
| 263 | `QuickFiler/Controllers/QfcItemController.EventHandlers.cs` | <= 500 PASS |
| 486 | `QuickFiler/Controllers/QfcItemController.EventWiring.cs` | <= 500 PASS |
| 192 | `QuickFiler/Viewers/IItemViewer.cs` | <= 500 PASS |
| 93 | `QuickFiler/Viewers/ItemViewer.FolderSearch.cs` | <= 500 PASS |
| 385 | `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part2.cs` | <= 500 PASS |
| 174 | `QuickFiler.Test/Controllers/QfcItemController.SearchDismissalTests.cs` | <= 500 PASS |
| 150 | `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs` | <= 500 PASS |
| 95 | `QuickFiler.Test/Viewers/ItemViewerSearchDismissalContractTests.cs` | <= 500 PASS |

Files audited: 11. Maximum count: 486 (`QfcItemController.EventWiring.cs`).

## DR-5 ceiling notes

- `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.cs` sits at 499 lines and was **not edited**
  by this plan (established byte-for-byte by the P4-T1 diff gate). The six new host-seam tests went
  into its `.Part2.cs` partial instead, which shares the primary file's `Harness`.
- `QuickFiler/Controllers/QfcItemController.EventWiring.cs` has the least headroom at 486 of 500
  after adding two subscription lines.

Acceptance: satisfied — every count is `<= 500`.
