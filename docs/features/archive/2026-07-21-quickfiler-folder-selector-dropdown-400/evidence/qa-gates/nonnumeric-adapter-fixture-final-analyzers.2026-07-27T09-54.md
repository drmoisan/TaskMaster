# P9-T31 non-numeric adapter fixture final analyzer gate

Timestamp: 2026-07-27T09:54Z

Command:

```powershell
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

EXIT_CODE: 0

Output Summary: Build succeeded with 0 errors. It reported six pre-existing warnings: five `System.Reactive` `packages.config` compatibility warnings and one duplicate `PercentageFormatterTests.cs` source warning in `UtilitiesCS.Test`.

## Assembly freshness

Resolved assembly: `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll`

Assembly `LastWriteTimeUtc`: `2026-07-27T09:53:41.5129109Z`

| Input | LastWriteTimeUtc | Assembly newer |
| --- | --- | --- |
| `QuickFiler/QuickFiler.csproj` | `2026-07-27T07:40:22.9454675Z` | Yes |
| `QuickFiler.Test/QuickFiler.Test.csproj` | `2026-07-27T07:42:52.7940519Z` | Yes |
| `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | `2026-07-27T08:17:37.4621297Z` | Yes |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | `2026-07-27T09:51:45.8571379Z` | Yes |
| `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | `2026-07-27T08:31:49.3833397Z` | Yes |
| `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` | `2026-07-27T08:17:38.3681165Z` | Yes |
| `QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs` | `2026-07-27T08:17:38.6850907Z` | Yes |
| `QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs` | `2026-07-27T09:51:47.1637583Z` | Yes |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs` | `2026-07-27T09:51:47.5335550Z` | Yes |
| `QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs` | `2026-07-27T09:51:47.9122476Z` | Yes |
