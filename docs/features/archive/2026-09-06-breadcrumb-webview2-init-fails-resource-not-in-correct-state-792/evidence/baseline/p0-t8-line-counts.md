# [P0-T8] Baseline line counts and new-file absence

- Issue: #792
- Timestamp: 2026-09-17T18-39
- Command: `(Get-Content -LiteralPath <path>).Count` per existing write-set `.cs` path and `Test-Path -LiteralPath <path>` per new path, run from `coverage/plan792-helper.ps1` with the item worktree as the working directory (the helper's opening branch assertion is the worktree proof)
- EXIT_CODE: 0
- Output Summary: 12 existing rows measured, `MISMATCH-COUNT: 0` against the plan's expected values; 17 new paths checked, `NEW-PATHS-PRESENT: 0`; `OVER-CEILING-BEFORE: 3`; positive control `Test-Path` on an existing path returned true.

## Existing write-set `.cs` files (total line count)

| Path | Lines | Expected | Match |
|---|---|---|---|
| `QuickFiler/Controllers/EfcFormController.cs` | 1321 | 1321 | true |
| `QuickFiler/Controllers/EfcItemController.cs` | 1122 | 1122 | true |
| `QuickFiler/Controllers/QfcCollectionController.cs` | 2333 | 2333 | true |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 479 | 479 | true |
| `QuickFiler/Controllers/EfcDataModel.cs` | 499 | 499 | true |
| `QuickFiler/Controllers/EfcHomeController.cs` | 447 | 447 | true |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 407 | 407 | true |
| `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` | 368 | 368 | true |
| `QuickFiler/Controllers/QfcItemController.cs` | 334 | 334 | true |
| `QuickFiler/Helper Classes/EfcViewerQueue.cs` | 101 | 101 | true |
| `QuickFiler/Controllers/BreadcrumbOutboundQueue.cs` | 67 | 67 | true |
| `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` | 485 | 485 | true |

EXISTING-ROWS: 12 (eleven production, one test)
MISMATCH-COUNT: 0

OVER-CEILING-BEFORE: 3 — `QuickFiler/Controllers/QfcCollectionController.cs` (2333), `QuickFiler/Controllers/EfcFormController.cs` (1321), `QuickFiler/Controllers/EfcItemController.cs` (1122).

## Seventeen new write-set paths (must be absent on the unfixed tree)

| Path | ABSENT |
|---|---|
| `QuickFiler/Viewers/WebView2EnvironmentContract.cs` | true |
| `QuickFiler/Controllers/EfcItemController.WebViewEnvironment.cs` | true |
| `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs` | true |
| `QuickFiler/Controllers/EfcFormController.SetupAndProperties.cs` | true |
| `QuickFiler/Controllers/EfcFormController.EventHandlers.cs` | true |
| `QuickFiler/Controllers/EfcFormController.Actions.cs` | true |
| `QuickFiler/Controllers/EfcFormController.Helpers.cs` | true |
| `QuickFiler/Controllers/QfcCollectionController.PopOut.cs` | true |
| `QuickFiler/Controllers/EfcDataModel.Carry.cs` | true |
| `QuickFiler.Test/Viewers/WebView2BreadcrumbHostIssue792Tests.cs` | true |
| `QuickFiler.Test/Viewers/WebView2EnvironmentContractTests.cs` | true |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue792Tests.cs` | true |
| `QuickFiler.Test/Controllers/BreadcrumbOutboundQueueIssue792Tests.cs` | true |
| `QuickFiler.Test/Controllers/EfcFormControllerIssue792Tests.cs` | true |
| `QuickFiler.Test/Controllers/QfcCollectionControllerIssue792PopOutTests.cs` | true |
| `QuickFiler.Test/Controllers/EfcDataModelIssue792CarryTests.cs` | true |
| `QuickFiler.Test/Helper Classes/EfcViewerQueueIssue792Tests.cs` | true |

NEW-PATHS-CHECKED: 17
NEW-PATHS-PRESENT: 0

Positive control for the absence claim: the same `Test-Path -LiteralPath` form against `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` returned true (`CONTROL-EXISTING-PATH-TEST: true`), so a false `Test-Path` on the seventeen paths is a genuine absence rather than a mis-scoped probe.

This artifact records the AC-U8 gate in its FAIL state (the seventeen paths absent and the pre-change counts) per the plan's observed-failing map.
