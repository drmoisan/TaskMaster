# P1-T6 fail-before: legacy ribbon wrapper

Timestamp: 2026-08-04T20:22:00-04:00

Command: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /TestCaseFilter:'FullyQualifiedName~TryLoadFolderFilter_PropagatesControlledInitializationFault'`

EXIT_CODE: 1

Output Summary: After temporarily restoring only the legacy `void TryLoadFolderFilter` wrapper that discards `TryLoadFolderFilterAsync`, the fault-propagation regression failed. Reflection returned no task and the assertion observed a `NullReferenceException` instead of the controlled `InvalidOperationException`, demonstrating that the fault was not observable through the legacy path. The task-returning wrapper was restored immediately.
