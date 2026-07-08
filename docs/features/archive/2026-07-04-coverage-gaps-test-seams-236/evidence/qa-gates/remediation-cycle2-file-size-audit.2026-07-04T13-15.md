# Remediation Cycle 2 File Size Audit

Timestamp: 2026-07-04T16:57:38.5510760-04:00
Task: P12-T11
Command: git diff --name-only merge-base..HEAD plus working tree; count changed production, test, and reusable script files
EXIT_CODE: 0

Output Summary:
- Audited files: 21
- Files over 500 lines: 0

| File | Lines | Result |
| --- | ---: | --- |
| QuickFiler.Test/Controllers/EfcHomeControllerDependenciesProductionFactoryTests.cs | 278 | PASS |
| QuickFiler.Test/Controllers/EfcHomeControllerDependenciesTests.cs | 435 | PASS |
| QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs | 387 | PASS |
| QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs | 203 | PASS |
| QuickFiler.Test/Controllers/EfcHomeControllerSeamTests.cs | 268 | PASS |
| QuickFiler.Test/Controllers/EfcHomeControllerTests.cs | 183 | PASS |
| QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs | 345 | PASS |
| QuickFiler.Test/Helper Classes/TlpCellStatesTests.cs | 119 | PASS |
| QuickFiler.Test/Helper Classes/ViewerQueueCoreTests.cs | 174 | PASS |
| QuickFiler.Test/Helper Classes/ViewerQueueStaticWrapperTests.cs | 308 | PASS |
| QuickFiler/Controllers/EfcHomeController.cs | 424 | PASS |
| QuickFiler/Controllers/EfcHomeController.Metrics.cs | 78 | PASS |
| QuickFiler/Controllers/EfcHomeController.Timing.cs | 39 | PASS |
| QuickFiler/Controllers/EfcHomeControllerDependencies.cs | 391 | PASS |
| QuickFiler/Controllers/EfcHomeControllerDependencyFactories.cs | 131 | PASS |
| QuickFiler/Helper Classes/EfcViewerQueue.cs | 88 | PASS |
| QuickFiler/Helper Classes/ItemViewerQueue.cs | 107 | PASS |
| QuickFiler/Helper Classes/QfcThemeControlSet.cs | 80 | PASS |
| QuickFiler/Helper Classes/QfcThemeHelper.cs | 353 | PASS |
| QuickFiler/Helper Classes/TlpCellSnapShot.cs | 192 | PASS |
| QuickFiler/Helper Classes/ViewerQueueCore.cs | 140 | PASS |
