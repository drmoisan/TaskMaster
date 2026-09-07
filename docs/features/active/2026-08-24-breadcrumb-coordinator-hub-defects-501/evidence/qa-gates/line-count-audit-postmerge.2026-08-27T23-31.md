# QA Gate — 500-line audit after the final formatting pass (P7-T8 re-run)

Timestamp: 2026-08-27T23-31

Command: `(Get-Content -LiteralPath <path>).Count` for each of the ten audited files

EXIT_CODE: 0

Output Summary: ten rows, every count at or below 500. Produced AFTER the final Phase 7 CSharpier run.

| Lines | File |
| ---: | --- |
| 378 | `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` |
| 353 | `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs` |
| 490 | `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` |
| 437 | `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` |
| 123 | `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Suggestions.cs` |
| 455 | `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` |
| 271 | `QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs` |
| 492 | `QuickFiler.Test/Viewers/BreadcrumbMessengerHubTests.cs` |
| 500 | `QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs` |
| 191 | `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorSupersessionTests.cs` |

Maximum observed: 500, at the limit and not over it. The instrument is
`(Get-Content -LiteralPath <path>).Count`, run under `pwsh -NoProfile`. No relocation was needed, so
P6-T1, P6-T4, P6-T5 and P6-T8 remain valid and the Phase 7 loop was not restarted for this audit.
