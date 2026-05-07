# Targeted Regression Evidence

Timestamp: 2026-05-05T13:26:31.7404012-04:00
Source Artifact: docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/evidence/qa-gates/csharp-mstest-coverage.2026-05-05T13-22-10.md
Verified Test Files:
- TaskMaster.Test/AppGlobals/AppToDoObjectsTests.cs
- TaskMaster.Test/AppGlobals/AppOlObjectsTests.cs
- TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs
- TaskMaster.Test/OutlookObjects/Store/StoresWrapperTests.cs
Verified Test Names:
- LoadIdListAsync_DoesNotReadOutlookApplicationFromWorkerThread
- LoadProjInfoAsync_DoesNotReadOutlookApplicationFromWorkerThread
- LoadStoresAsync_DoesNotCompleteBeforeStoreRewireTaskFinishes
- LoadSequentialAsync_KeepsComPhasesOnCallerThreadAndYieldsBetweenHeavyPhases
- RewireOlObjectsAsync_PreservesStoreOrderAcrossYieldedIterations
