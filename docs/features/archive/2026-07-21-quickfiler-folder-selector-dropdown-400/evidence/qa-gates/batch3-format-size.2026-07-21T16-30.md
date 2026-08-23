# Batch 3 format and size

Timestamp: 2026-07-21T16-30Z

Format Command: `csharpier format QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs QuickFiler/Viewers/BreadcrumbMessengerHub.cs QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorProbabilityTests.cs QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs QuickFiler.Test/Viewers/BreadcrumbMessengerHubTests.cs`

Format EXIT_CODE: 0

Size Command: `Get-Item -LiteralPath 'QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs','QuickFiler/Viewers/BreadcrumbMessengerHub.cs','QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorProbabilityTests.cs','QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs','QuickFiler.Test/Viewers/BreadcrumbMessengerHubTests.cs' | ForEach-Object { [pscustomobject]@{ Path = $_.FullName; Lines = (Get-Content -LiteralPath $_.FullName).Count } }`

Size EXIT_CODE: 0

| File | Lines |
|---|---:|
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | 437 |
| `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` | 236 |
| `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorProbabilityTests.cs` | 162 |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs` | 205 |
| `QuickFiler.Test/Viewers/BreadcrumbMessengerHubTests.cs` | 158 |

Protected File Command: `git diff --exit-code df5ad49c909f6b739edef45d0336151f44e827a6 -- QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs`

Protected File EXIT_CODE: 0

Output Summary: CSharpier formatted five files, every batch file remains at or below 500 lines, and the protected existing `BreadcrumbBridgeCoordinatorTests.cs` file is unchanged from the baseline commit.
