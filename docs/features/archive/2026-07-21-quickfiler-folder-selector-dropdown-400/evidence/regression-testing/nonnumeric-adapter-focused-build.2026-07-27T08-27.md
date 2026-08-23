# P9-T14 nonnumeric adapter focused build

Timestamp: 2026-07-27T08-27
Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU'
EXIT_CODE: 0

## Output Summary

The Debug/Any CPU solution build completed with zero errors. The six warnings are existing System.Reactive packages.config compatibility warnings and the existing UtilitiesCS.Test duplicate PercentageFormatterTests source warning.

Resolved assembly: QuickFiler.Test/bin/Debug/QuickFiler.Test.dll.
Assembly LastWriteTimeUtc: 2026-07-27T08:27:46.3473530Z.

## Freshness proof

| Input | LastWriteTimeUtc | Assembly newer |
| --- | --- | --- |
| QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs | 2026-07-27T08:17:37.4621297Z | True |
| QuickFiler/Viewers/ItemViewer.Breadcrumb.cs | 2026-07-27T08:17:37.7605571Z | True |
| QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs | 2026-07-27T08:23:20.8120568Z | True |
| QuickFiler/QuickFiler.csproj | 2026-07-27T07:40:22.9454675Z | True |
| QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs | 2026-07-27T08:17:38.3681165Z | True |
| QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs | 2026-07-27T08:17:38.6850907Z | True |
| QuickFiler.Test/QuickFiler.Test.csproj | 2026-07-27T07:42:52.7940519Z | True |

Result: PASS. The assembly is current and authorizes the P9-T14 focused VSTest selection. Earlier focused-build artifacts remain historical.
