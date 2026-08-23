# P9-T14 nonnumeric adapter focused build

Timestamp: 2026-07-27T08-12
Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU'
EXIT_CODE: 0

## Output Summary

The Debug/Any CPU solution build completed with zero errors. The six warnings are existing System.Reactive packages.config compatibility warnings and the existing UtilitiesCS.Test duplicate PercentageFormatterTests source warning.

Resolved assembly: QuickFiler.Test/bin/Debug/QuickFiler.Test.dll.
Assembly LastWriteTimeUtc: 2026-07-27T08:12:14.3543839Z.

## Freshness proof

| Input | LastWriteTimeUtc | Assembly newer |
| --- | --- | --- |
| QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs | 2026-07-27T08:09:10.5696204Z | True |
| QuickFiler/Viewers/ItemViewer.Breadcrumb.cs | 2026-07-27T07:46:35.8358724Z | True |
| QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs | 2026-07-27T08:00:25.1072689Z | True |
| QuickFiler/QuickFiler.csproj | 2026-07-27T07:40:22.9454675Z | True |
| QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs | 2026-07-27T07:47:08.7751009Z | True |
| QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs | 2026-07-27T07:44:09.1636821Z | True |
| QuickFiler.Test/QuickFiler.Test.csproj | 2026-07-27T07:42:52.7940519Z | True |

Result: PASS. The assembly is current and authorizes the P9-T14 focused VSTest selection. The 2026-07-27T07-56 failed and 2026-07-27T08-02 superseded build artifacts remain historical.
