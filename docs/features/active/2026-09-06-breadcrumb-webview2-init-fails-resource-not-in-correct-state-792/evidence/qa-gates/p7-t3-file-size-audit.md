# [P7-T3] Authoritative post-format file-size audit and AC-U9 figures

- Issue: #792
- Timestamp: 2026-09-17T21-06
- PASS-NUMBER: 1
- Command: `(Get-Content -LiteralPath <path>).Count` per write-set `.cs` path (convention 3: total lines, never `Measure-Object -Line`, never non-blank counts), measured AFTER the [P7-T2] format step of this pass; run from `coverage/plan792-helper.ps1 -Step sizes -PassNumber 1` with the item worktree as the working directory (the helper's opening branch assertion `bug/breadcrumb-webview2-init-fails-resource-not-in-correct-state-792` is the worktree proof and passed; HEAD `c9b457bda44ef856306a1bc96c94683dc528993c`)
- EXIT_CODE: 0
- Output Summary: 29 write-set `.cs` paths measured (20 production, 9 test), `MISSING-COUNT: 0`; `OVER-CEILING-AFTER: 2` (`EfcItemController.cs` 1076, `QfcCollectionController.cs` 2306, both pre-existing and exempted by the task text); `UNEXPECTED-OVER-CEILING: 0`; every figure equals the [P6-T2] advisory figure, consistent with the formatter rewriting no write-set file in [P7-T2].

## Production write-set `.cs` files (total line count, post-format)

| Path | Lines | OVER-CEILING |
|---|---|---|
| `QuickFiler/Viewers/WebView2EnvironmentContract.cs` | 53 | false |
| `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` | 382 | false |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 474 | false |
| `QuickFiler/Controllers/EfcItemController.cs` | 1076 | true (pre-existing; excepted by the [P7-T3] acceptance) |
| `QuickFiler/Controllers/EfcItemController.WebViewEnvironment.cs` | 56 | false |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 453 | false |
| `QuickFiler/Controllers/BreadcrumbOutboundQueue.cs` | 80 | false |
| `QuickFiler/Controllers/EfcFormController.cs` | 266 | false |
| `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs` | 178 | false |
| `QuickFiler/Controllers/EfcFormController.SetupAndProperties.cs` | 243 | false |
| `QuickFiler/Controllers/EfcFormController.EventHandlers.cs` | 383 | false |
| `QuickFiler/Controllers/EfcFormController.Actions.cs` | 184 | false |
| `QuickFiler/Controllers/EfcFormController.Helpers.cs` | 270 | false |
| `QuickFiler/Controllers/QfcCollectionController.cs` | 2306 | true (pre-existing; excepted by the [P7-T3] acceptance) |
| `QuickFiler/Controllers/QfcCollectionController.PopOut.cs` | 111 | false |
| `QuickFiler/Controllers/EfcHomeController.cs` | 464 | false |
| `QuickFiler/Controllers/EfcDataModel.cs` | 464 | false |
| `QuickFiler/Controllers/EfcDataModel.Carry.cs` | 100 | false |
| `QuickFiler/Controllers/QfcItemController.cs` | 340 | false |
| `QuickFiler/Helper Classes/EfcViewerQueue.cs` | 108 | false |

## Test write-set `.cs` files (total line count, post-format)

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

UNEXPECTED-OVER-CEILING: 0 (every write-set `.cs` file except `QuickFiler/Controllers/EfcItemController.cs` and `QuickFiler/Controllers/QfcCollectionController.cs` is at most 500 lines measured after [P7-T2])

## AC-U9 statement (post-format figures)

PRE-EXISTING-DEBT: EfcItemController.cs before 1122 after 1076; QfcCollectionController.cs before 2333 after 2306

The remaining over-ceiling size of these two files is pre-existing debt that this change neither introduces nor resolves. Both were over the 500-line ceiling at the [P0-T8] baseline (`OVER-CEILING-BEFORE: 3`, listed there as 2333, 1321 and 1122), and both shrank because members moved out to new partials (`EfcItemController.WebViewEnvironment.cs`, `QfcCollectionController.PopOut.cs`); neither is brought under the ceiling, and no new file exceeds it. `EfcFormController.cs` (1321 at baseline) is 266 after the six-way split (D9), which is why `OVER-CEILING-BEFORE: 3` becomes `OVER-CEILING-AFTER: 2`.

## Positive control

`CONTROL-KNOWN-OVER: true` — the same `(Get-Content -LiteralPath ...).Count -gt 500` expression applied to `QuickFiler/Controllers/QfcCollectionController.cs` returned true, so the `over=false` rows come from a comparison that fires when a file is over the ceiling. `MISSING-COUNT: 0` was computed by `Test-Path -LiteralPath` over all 29 paths, so no row is a count over an absent file.
