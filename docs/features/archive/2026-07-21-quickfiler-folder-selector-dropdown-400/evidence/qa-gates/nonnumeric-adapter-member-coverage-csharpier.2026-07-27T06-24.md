# P9-T41 CSharpier Evidence

Timestamp: 2026-07-27T06:24:05-04:00

## Commands and Results

```powershell
& 'C:\Users\DanMoisan\.dotnet\tools\csharpier.exe' format QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs QuickFiler/Viewers/ItemViewer.Breadcrumb.cs QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs
```

Format exit code: `0` (`Formatted 8 files in 2726ms.`)

```powershell
& 'C:\Users\DanMoisan\.dotnet\tools\csharpier.exe' check QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs QuickFiler/Viewers/ItemViewer.Breadcrumb.cs QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs
```

Stable-check exit code: `0` (`Checked 8 files in 1590ms.`)

## Stable Scoped Files

| File | Physical lines | SHA-256 |
| --- | ---: | --- |
| `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | 481 | `32FC3630C813E14DF55C702876AE2D5FCB0B713B0314D666B703F6BCBD892F31` |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | 298 | `19811E252ED35AAA0292AB3942DDD02E7F2C5620066B81C256AA97A5F4F2F9DA` |
| `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | 494 | `709E50E6578FD8F97FD8CD56BCCE0A1B8E269A604E01F4B67E282AE7A9388760` |
| `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` | 327 | `8EB6AB9FBA022EF16EF7D1A4FC00FB137F91170ADE37458DDB0D3D560659D3C3` |
| `QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs` | 302 | `0D92BBF92237FDC98FBD3A98F7AAC7C004E23D7A9784FF6023689D19EF578E14` |
| `QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs` | 486 | `601BCAD921C078F495096589C354C2FD4F247CD9C5A2F55BB6C662EBE24467DF` |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs` | 461 | `7BA48796D4AC13BC89E6F5285F628C606893C27564854E785DB3071D9F0472EF` |
| `QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs` | 487 | `1166374F7EB833D59EF877262CA2E603F5E33ACC1AFDD783812ABE8DDFC8AC63` |

All eight scoped files remain within the repository 500-line limit. The stable check did not require a return to P9-T39 or P9-T40.
