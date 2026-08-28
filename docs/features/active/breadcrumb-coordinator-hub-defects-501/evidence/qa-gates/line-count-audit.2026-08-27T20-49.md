# QA Gate — 500-Line Cap Audit, PRE-FORMAT LEG (P6-T1)

Timestamp: 2026-08-27T20-49

Instrument used: `(Get-Content -LiteralPath <path>).Count`

The `Get-Content -LiteralPath <path> | Measure-Object -Line` form was NOT used. `Measure-Object -Line`
drops blank lines and undercounts: it reported 436 against 487 actual physical lines on
`QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` at baseline.

## Leg identification

This is the **PRE-FORMAT** leg of the AC-25 audit. P7-T8 is the post-format leg and its artifact
(`FF/evidence/qa-gates/line-count-audit-postformat.<TS>.md`) is the artifact of record for AC-25's
"after the change" condition. Both legs audit the same ten files with the same instrument.

## Rows — every added and modified `.cs` file

| # | Path | Baseline | Now | Headroom | At or below 500 |
| ---: | --- | ---: | ---: | ---: | --- |
| 1 | `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` | 355 | 378 | 122 | yes |
| 2 | `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs` | 309 | 353 | 147 | yes |
| 3 | `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` | 456 | 490 | 10 | yes |
| 4 | `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | 487 | 437 | 63 | yes |
| 5 | `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Suggestions.cs` | n/a (new) | 108 | 392 | yes |
| 6 | `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` | 381 | 455 | 45 | yes |
| 7 | `QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs` | 122 | 272 | 228 | yes |
| 8 | `QuickFiler.Test/Viewers/BreadcrumbMessengerHubTests.cs` | 414 | 492 | 8 | yes |
| 9 | `QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs` | 434 | 500 | 0 | yes |
| 10 | `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorSupersessionTests.cs` | n/a (new) | 140 | 360 | yes |

Ten rows. Every count is at or below 500. **PASS.**

Note on row 4: the primary bridge-coordinator file DECREASED from 487 to 437 because the SR-1 split
relocated four members out of it. That is the whole purpose of SR-1 — without it, the #502 call-site
change would have pushed the file to roughly 500-504.

Note on row 9: `BreadcrumbSelectorCoordinatorTests.cs` sits exactly at the cap with zero headroom. It
was drafted at 531 lines and compacted in place to 500. `csharpier check` reports it already formatted,
so the Phase 7 formatting pass cannot add lines to it; P7-T8 re-measures to confirm.
