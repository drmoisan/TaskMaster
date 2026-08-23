# P9-T16 nonnumeric adapter final CSharpier

Timestamp: 2026-07-27T08-31
Command: C:\Users\DanMoisan\.dotnet\tools\csharpier.exe format QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs QuickFiler/Viewers/ItemViewer.Breadcrumb.cs QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs
Command: C:\Users\DanMoisan\.dotnet\tools\csharpier.exe check QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs QuickFiler/Viewers/ItemViewer.Breadcrumb.cs QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs
EXIT_CODE: 0

## Output Summary

CSharpier format and check exited 0. The formatter left four files hash-identical but changed QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs. The no-delta acceptance criterion therefore fails.

| Path | Before SHA-256 | After SHA-256 | Physical lines |
| --- | --- | --- | ---: |
| QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs | 32FC3630C813E14DF55C702876AE2D5FCB0B713B0314D666B703F6BCBD892F31 | 32FC3630C813E14DF55C702876AE2D5FCB0B713B0314D666B703F6BCBD892F31 | 430 |
| QuickFiler/Viewers/ItemViewer.Breadcrumb.cs | 187827EE6093B1B6797BBDB56CC4D92C6CC7778A3BA064E4C1ADFEAA99774170 | 187827EE6093B1B6797BBDB56CC4D92C6CC7778A3BA064E4C1ADFEAA99774170 | 258 |
| QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs | 0BE8FAAE1A774332A2B8E0B3A2C99292996D8C5165058D1A7D7B4717EFDD7F8D | A7CCB93C9F40D236A278DACD890807CECA371ECB886B343E50272AA4E054D108 | 434 |
| QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs | 7446D6EC089348E0ED2D03C7B4D158921F6490EE3AD13D1E231142E5C709EFF0 | 7446D6EC089348E0ED2D03C7B4D158921F6490EE3AD13D1E231142E5C709EFF0 | 200 |
| QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs | 37C6D4656AF85D2A03B3C2D2A43CF9BA11911822C01247A3E425F568AB78BB22 | 37C6D4656AF85D2A03B3C2D2A43CF9BA11911822C01247A3E425F568AB78BB22 | 182 |

All files remain at most 500 lines. The formatter output is retained. Result: FAIL; P9-T16 remains unchecked and returns to P9-T12. P9-T17 through P9-T21 were not run.
