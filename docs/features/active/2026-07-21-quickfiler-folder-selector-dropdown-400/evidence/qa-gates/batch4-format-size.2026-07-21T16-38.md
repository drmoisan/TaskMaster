# Batch 4 format and size

Timestamp: 2026-07-21T16-38Z

Format Command: `csharpier format QuickFiler/Viewers/BreadcrumbPopupPlacement.cs QuickFiler/Viewers/IBreadcrumbDropDownHost.cs QuickFiler/Viewers/BreadcrumbDropDownHost.cs QuickFiler.Test/Viewers/BreadcrumbPopupPlacementTests.cs QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.cs QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleTests.cs`

Format EXIT_CODE: 0

Size Command: `Get-Item -LiteralPath 'QuickFiler/Viewers/BreadcrumbPopupPlacement.cs','QuickFiler/Viewers/IBreadcrumbDropDownHost.cs','QuickFiler/Viewers/BreadcrumbDropDownHost.cs','QuickFiler.Test/Viewers/BreadcrumbPopupPlacementTests.cs','QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.cs','QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleTests.cs' | ForEach-Object { [pscustomobject]@{ Path = $_.FullName; Lines = (Get-Content -LiteralPath $_.FullName).Count } }`

Size EXIT_CODE: 0

| File | Lines |
|---|---:|
| `QuickFiler/Viewers/BreadcrumbPopupPlacement.cs` | 87 |
| `QuickFiler/Viewers/IBreadcrumbDropDownHost.cs` | 42 |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | 392 |
| `QuickFiler.Test/Viewers/BreadcrumbPopupPlacementTests.cs` | 169 |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.cs` | 235 |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleTests.cs` | 277 |

Output Summary: CSharpier formatted all six batch files and every file remains at or below 500 lines.
