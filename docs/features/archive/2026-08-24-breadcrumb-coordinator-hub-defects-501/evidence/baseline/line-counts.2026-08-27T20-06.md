# Baseline — Line Counts of Every File This Plan May Touch (P0-T16)

Timestamp: 2026-08-27T20-06

Instrument: `(Get-Content -LiteralPath <path>).Count` — the physical line count including blank lines.

The `Get-Content -LiteralPath <path> | Measure-Object -Line` form was NOT used. `Measure-Object -Line`
drops blank lines and undercounts: it reported 436 against 487 actual physical lines on
`QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`.

| # | Path | Lines | Headroom to 500 |
| ---: | --- | ---: | ---: |
| 1 | `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` | 355 | 145 |
| 2 | `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs` | 309 | 191 |
| 3 | `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` | 456 | 44 |
| 4 | `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | 487 | 13 |
| 5 | `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` | 381 | 119 |
| 6 | `QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs` | 122 | 378 |
| 7 | `QuickFiler.Test/Viewers/BreadcrumbMessengerHubTests.cs` | 414 | 86 |
| 8 | `QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs` | 434 | 66 |

Eight rows with numeric counts. Every count is at or below 500. PASS.

Every figure matches the research document's §7.1 and §7.3 tables exactly, confirming no line-number
drift between research time and `BASELINE_SHA`. Row 4 confirms the SR-1 premise: with 13 lines of
headroom and a #502 call-site change estimated at +13 to +17 lines, the partial split must land before
the Phase 4 edit.
