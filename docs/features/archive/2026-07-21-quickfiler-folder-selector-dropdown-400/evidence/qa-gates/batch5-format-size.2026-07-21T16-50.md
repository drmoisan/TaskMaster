# Phase 5 formatting and size evidence

Timestamp: 2026-07-21T16:50:30.1321481Z

## Formatter

```powershell
csharpier format QuickFiler/Viewers/ItemViewer.Breadcrumb.cs QuickFiler/Viewers/ItemViewer.FolderSearch.cs QuickFiler/Controllers/QfcItemController.ViewerSetup.cs QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs
```

EXIT_CODE: 0

Output summary: `Formatted 6 files in 1882ms.`

## Line-count command

```powershell
Get-Item -LiteralPath 'QuickFiler/Viewers/ItemViewer.Breadcrumb.cs','QuickFiler/Viewers/ItemViewer.FolderSearch.cs','QuickFiler/Controllers/QfcItemController.ViewerSetup.cs','QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs','QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs','QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs' | ForEach-Object { [pscustomobject]@{ Path = $_.FullName; Lines = (Get-Content -LiteralPath $_.FullName).Count } }
```

EXIT_CODE: 0

| Path | Lines |
| --- | ---: |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | 392 |
| `QuickFiler/Viewers/ItemViewer.FolderSearch.cs` | 74 |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 409 |
| `QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs` | 100 |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs` | 357 |
| `QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs` | 190 |

All six files are at most 500 lines.

## Protected Designer verification

```powershell
git diff --exit-code df5ad49c909f6b739edef45d0336151f44e827a6 -- QuickFiler/Viewers/ItemViewer.Designer.cs
```

EXIT_CODE: 0

`QuickFiler/Viewers/ItemViewer.Designer.cs` is byte-for-byte unchanged from the Phase 0 baseline commit.
