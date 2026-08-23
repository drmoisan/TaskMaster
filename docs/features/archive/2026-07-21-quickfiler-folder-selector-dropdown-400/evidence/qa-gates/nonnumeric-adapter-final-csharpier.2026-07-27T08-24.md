# P9-T16 nonnumeric adapter final CSharpier

Timestamp: 2026-07-27T08-24
Command: C:\Users\DanMoisan\.dotnet\tools\csharpier.exe format QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs QuickFiler/Viewers/ItemViewer.Breadcrumb.cs QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs
Command: C:\Users\DanMoisan\.dotnet\tools\csharpier.exe check QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs QuickFiler/Viewers/ItemViewer.Breadcrumb.cs QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs
EXIT_CODE: 0

## Output Summary

CSharpier format exited 0 and check exited 0, but the mutating format command changed all five authorized files. The stable-check condition therefore passes while the no-delta acceptance condition fails.

| Path | Before SHA-256 | After SHA-256 | Physical lines |
| --- | --- | --- | ---: |
| QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs | 4621CE1D3038BC536E848EC7DD482EEC0FB35C5BEE4AF344F8FAA6F6C9CFCA15 | 32FC3630C813E14DF55C702876AE2D5FCB0B713B0314D666B703F6BCBD892F31 | 430 |
| QuickFiler/Viewers/ItemViewer.Breadcrumb.cs | 9AF06BD370CEDC9641AA1EC93DB802D1BA0573B48A6D411B49A14C5CCABAA48B | 187827EE6093B1B6797BBDB56CC4D92C6CC7778A3BA064E4C1ADFEAA99774170 | 258 |
| QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs | 04DDB1340CB867D0C96C97FE5677B7CE2EE20A1EE0C2D8DD7896F6DD472A6B68 | 9964C68C70D66F287D5A3CEDF88362CD7B70CE16F33BC24027BD28BD3699AFF4 | 417 |
| QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs | C5B39D0811FD5ACCEF8C04318C8332EDB91A83091E5A1CF349F25447482240A8 | 7446D6EC089348E0ED2D03C7B4D158921F6490EE3AD13D1E231142E5C709EFF0 | 200 |
| QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs | 389BAF4FF308F5051CA8F832A027C9F4120EA8C429081F6AECA25936AD7E3675 | 37C6D4656AF85D2A03B3C2D2A43CF9BA11911822C01247A3E425F568AB78BB22 | 182 |

All files remain at most 500 physical lines. The formatter output is retained. P9-T16 is not complete because the first stable format changed source; execution returns to P9-T12 and must restart P9-T16 after the bounded correction is reconciled. P9-T17 through P9-T21 were not run.
