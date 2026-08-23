# P9-T41 CSharpier Evidence (Method-Group Successor)

Timestamp: 2026-07-27T06:28:44-04:00

The earlier P9-T41 evidence is superseded by `nonnumeric-adapter-member-coverage-superseded.2026-07-27T06-26.md`. This record applies to the corrected `NavigationBinder` method-group source state.

## Commands and Results

```powershell
& 'C:\Users\DanMoisan\.dotnet\tools\csharpier.exe' format QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs QuickFiler/Viewers/ItemViewer.Breadcrumb.cs QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs
& 'C:\Users\DanMoisan\.dotnet\tools\csharpier.exe' check QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs QuickFiler/Viewers/ItemViewer.Breadcrumb.cs QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs
```

Format exit code: `0` (`Formatted 8 files in 2673ms.`)

Stable-check exit code: `0` (`Checked 8 files in 1580ms.`)

## Stable Scoped Files

| File | Physical lines | SHA-256 |
| --- | ---: | --- |
| `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | 481 | `32FC3630C813E14DF55C702876AE2D5FCB0B713B0314D666B703F6BCBD892F31` |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | 298 | `19811E252ED35AAA0292AB3942DDD02E7F2C5620066B81C256AA97A5F4F2F9DA` |
| `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | 494 | `1728E0A62E4B2B4775F20BD5460C5F365AFF8B097ED0AF6169F222A07ED86746` |
| `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` | 327 | `8EB6AB9FBA022EF16EF7D1A4FC00FB137F91170ADE37458DDB0D3D560659D3C3` |
| `QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs` | 302 | `3EE05089236DEE9CA591ED1282FC6EE3F14D694B2CF82C7E566D1C4CE167237A` |
| `QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs` | 486 | `601BCAD921C078F495096589C354C2FD4F247CD9C5A2F55BB6C662EBE24467DF` |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs` | 461 | `7BA48796D4AC13BC89E6F5285F628C606893C27564854E785DB3071D9F0472EF` |
| `QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs` | 487 | `1166374F7EB833D59EF877262CA2E603F5E33ACC1AFDD783812ABE8DDFC8AC63` |

All eight scoped files remain within the repository 500-line limit.
