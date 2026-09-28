# [P6-T2] Advisory line-count audit and AC-U9 measurement (post-change)

- Issue: #792
- Timestamp: 2026-09-17T20-53
- Command: `(Get-Content -LiteralPath <path>).Count` per write-set `.cs` path (convention 3: total lines, never `Measure-Object -Line`, never non-blank counts), run from `coverage/plan792-helper.ps1` with the item worktree as the working directory (the helper's opening branch assertion `bug/breadcrumb-webview2-init-fails-resource-not-in-correct-state-792` is the worktree proof and passed; HEAD `9ac987a969d1f4786d296fee25817c8e5dde9233`)
- EXIT_CODE: 0
- Output Summary: 29 write-set `.cs` paths measured (20 production, 9 test), `MISSING-COUNT: 0`; `OVER-CEILING-AFTER: 2` (`EfcItemController.cs` 1076, `QfcCollectionController.cs` 2306, both pre-existing); `UNEXPECTED-OVER-CEILING: 0`; retained `EfcFormController.cs` is 266 (`OVER-CEILING: false`). This audit is advisory; [P7-T3] is authoritative.

## Production write-set `.cs` files (total line count)

| Path | Lines | OVER-CEILING |
|---|---|---|
| `QuickFiler/Viewers/WebView2EnvironmentContract.cs` | 53 | false |
| `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` | 382 | false |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 474 | false |
| `QuickFiler/Controllers/EfcItemController.cs` | 1076 | true (pre-existing, allowed by the [P6-T2] acceptance) |
| `QuickFiler/Controllers/EfcItemController.WebViewEnvironment.cs` | 56 | false |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 453 | false |
| `QuickFiler/Controllers/BreadcrumbOutboundQueue.cs` | 80 | false |
| `QuickFiler/Controllers/EfcFormController.cs` | 266 | false |
| `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs` | 178 | false |
| `QuickFiler/Controllers/EfcFormController.SetupAndProperties.cs` | 243 | false |
| `QuickFiler/Controllers/EfcFormController.EventHandlers.cs` | 383 | false |
| `QuickFiler/Controllers/EfcFormController.Actions.cs` | 184 | false |
| `QuickFiler/Controllers/EfcFormController.Helpers.cs` | 270 | false |
| `QuickFiler/Controllers/QfcCollectionController.cs` | 2306 | true (pre-existing, allowed by the [P6-T2] acceptance) |
| `QuickFiler/Controllers/QfcCollectionController.PopOut.cs` | 111 | false |
| `QuickFiler/Controllers/EfcHomeController.cs` | 464 | false |
| `QuickFiler/Controllers/EfcDataModel.cs` | 464 | false |
| `QuickFiler/Controllers/EfcDataModel.Carry.cs` | 100 | false |
| `QuickFiler/Controllers/QfcItemController.cs` | 340 | false |
| `QuickFiler/Helper Classes/EfcViewerQueue.cs` | 108 | false |

## Test write-set `.cs` files (total line count)

| Path | Lines | OVER-CEILING |
|---|---|---|
| `QuickFiler.Test/Viewers/WebView2BreadcrumbHostIssue792Tests.cs` | 153 | false |
| `QuickFiler.Test/Viewers/WebView2EnvironmentContractTests.cs` | 192 | false |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue792Tests.cs` | 196 | false |
| `QuickFiler.Test/Controllers/BreadcrumbOutboundQueueIssue792Tests.cs` | 113 | false |
| `QuickFiler.Test/Controllers/EfcFormControllerIssue792Tests.cs` | 348 | false |
| `QuickFiler.Test/Controllers/QfcCollectionControllerIssue792PopOutTests.cs` | 213 | false |
| `QuickFiler.Test/Controllers/EfcDataModelIssue792CarryTests.cs` | 175 | false |
| `QuickFiler.Test/Helper Classes/EfcViewerQueueIssue792Tests.cs` | 40 | false |
| `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` | 485 | false (unchanged from the [P0-T8] baseline of 485; zero-edit by design) |

PRODUCTION-ROWS: 20
TEST-ROWS: 9
MISSING-COUNT: 0

OVER-CEILING-AFTER: 2

UNEXPECTED-OVER-CEILING: 0 (every write-set `.cs` file except `QuickFiler/Controllers/EfcItemController.cs` and `QuickFiler/Controllers/QfcCollectionController.cs` is at most 500)

## AC-U9 statement

PRE-EXISTING-DEBT: EfcItemController.cs before 1122 after 1076; QfcCollectionController.cs before 2333 after 2306

The remaining over-ceiling size of these two files is pre-existing debt that this change neither introduces nor resolves. Both files were over the 500-line ceiling at the [P0-T8] baseline (`OVER-CEILING-BEFORE: 3`, listed there as 2333, 1321 and 1122), and both shrank in this change because members moved out to new partials (`EfcItemController.WebViewEnvironment.cs`, `QfcCollectionController.PopOut.cs`); neither is brought under the ceiling, and no new file exceeds it. The measured values fall inside the plan's expected ranges (1076 or 1077; 2306 or 2307).

`EfcFormController.cs`: before 1321 (from [P0-T8]), after 266; retained file `OVER-CEILING: false`. The third file over the ceiling at baseline is therefore resolved by the six-way split (D9), which is why `OVER-CEILING-BEFORE: 3` becomes `OVER-CEILING-AFTER: 2`.

## Positive control

`CONTROL-KNOWN-OVER: true` — the same `(Get-Content -LiteralPath ...).Count -gt 500` expression applied to `QuickFiler/Controllers/QfcCollectionController.cs` returned true, so the `OVER-CEILING: false` rows are produced by a comparison that does fire when a file is over the ceiling. `MISSING-COUNT: 0` was computed by `Test-Path -LiteralPath` over all 29 paths, so no row is a count over an absent file.
