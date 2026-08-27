Timestamp: 2026-08-27T03-21-45Z
Command: `& $vstest TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.AppGlobals.AppFileSystemFolderPathsOneDriveResolutionTests" "/Logger:trx;LogFileName=p2-t3.trx" "/ResultsDirectory:coverage\trx\p2-t3"`
EXIT_CODE: 0
Output Summary: The dedicated OneDrive-resolution contract suite passed 7/7, including `ResolveOneDriveRoot_NoVariableSet_FailsExplicitlyWithARedactedDiagnostic`.

Existing one- and two-argument `ApplicationGlobals` constructors chain with no injected reader, so `LoadBasicMethod` continues to select the public default `AppFileSystemFolderPaths` constructor and the real-environment D7 fail-fast behavior. `TaskMaster/ThisAddIn.cs` is unchanged.
