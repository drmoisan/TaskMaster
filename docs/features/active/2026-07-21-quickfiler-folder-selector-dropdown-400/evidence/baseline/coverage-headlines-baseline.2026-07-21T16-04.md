# Coverage headlines baseline

Timestamp: 2026-07-21T16-04Z

Command:

```powershell
$coveragePath = 'docs\features\active\2026-07-21-quickfiler-folder-selector-dropdown-400\evidence\baseline\coverage-baseline.2026-07-21T16-00.cobertura.xml'; [xml]$coverage = Get-Content -LiteralPath $coveragePath -Raw; $classes = @($coverage.coverage.packages.package.classes.class); function Measure-Coverage([object[]]$selected) { $map = @{}; foreach ($class in $selected) { foreach ($line in @($class.lines.line)) { $key = "$($class.filename):$($line.number)"; if (-not $map.ContainsKey($key) -or [int]$line.hits -gt $map[$key]) { $map[$key] = [int]$line.hits } } }; $valid = $map.Count; $covered = @($map.Values | Where-Object { $_ -gt 0 }).Count; $rate = if ($valid -eq 0) { 0.0 } else { $covered / $valid }; return [pscustomobject]@{ Covered=$covered; Valid=$valid; Rate=$rate } }; foreach ($prefix in 'UtilitiesCS.OutlookObjects.Folder','QuickFiler.Viewers') { $m = Measure-Coverage @($classes | Where-Object { $_.name -eq $prefix -or $_.name -like "$prefix.*" }); '{0}|{1}|{2}|{3:F6}' -f $prefix,$m.Covered,$m.Valid,$m.Rate }; $files = @('UtilitiesCS\OutlookObjects\Folder\BreadcrumbStateModel.cs','UtilitiesCS\OutlookObjects\Folder\BreadcrumbRenderProjection.cs','UtilitiesCS\OutlookObjects\Folder\FolderBreadcrumbBridgeRouter.cs','QuickFiler\Viewers\BreadcrumbBridgeCoordinator.cs','QuickFiler\Viewers\ItemViewer.Breadcrumb.cs','QuickFiler\Viewers\ItemViewer.FolderSearch.cs','QuickFiler\Controllers\QfcItemController.ViewerSetup.cs'); foreach ($file in $files) { $m = Measure-Coverage @($classes | Where-Object filename -eq $file); '{0}|{1}|{2}|{3:F6}' -f $file,$m.Covered,$m.Valid,$m.Rate }
```

EXIT_CODE: 0

## Repository headline

- Lines covered: 87397
- Lines valid: 104178
- Line rate: 0.838920

## Required namespace aggregates

| Namespace | Covered lines | Valid lines | Line rate |
|---|---:|---:|---:|
| `UtilitiesCS.OutlookObjects.Folder` | 2272 | 2329 | 0.975526 |
| `QuickFiler.Viewers` | 530 | 573 | 0.924956 |

## Existing planned production files

| File | Covered lines | Valid lines | Line rate |
|---|---:|---:|---:|
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` | 160 | 160 | 1.000000 |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs` | 113 | 113 | 1.000000 |
| `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` | 199 | 204 | 0.975490 |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | 109 | 112 | 0.973214 |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | 0 | 0 | 0.000000 |
| `QuickFiler/Viewers/ItemViewer.FolderSearch.cs` | 0 | 0 | 0.000000 |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 96 | 140 | 0.685714 |

The two existing `ItemViewer` partial files have numeric zero baselines because the post-processed Cobertura document contains no class entry for those source filenames.

## Planned new types not present at baseline

| Planned type | Executable lines | Covered lines | Line rate |
|---|---:|---:|---:|
| `BreadcrumbSelectionSession` | 0 | 0 | 0.000000 |
| `BreadcrumbSelectorMessages` | 0 | 0 | 0.000000 |
| `BreadcrumbMessengerHub` | 0 | 0 | 0.000000 |
| `BreadcrumbPopupPlacement` | 0 | 0 | 0.000000 |
| `IBreadcrumbDropDownHost` | 0 | 0 | 0.000000 |
| `BreadcrumbDropDownHost` | 0 | 0 | 0.000000 |

Output Summary: The Cobertura root, package, class, and line attributes were parsed without modifying production or test files. All required baseline values are numeric.
