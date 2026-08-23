# P1-T1 fail-before: worker-held service gate

Timestamp: 2026-08-04T20:25:00-04:00

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /TestCaseFilter:'FullyQualifiedName~FolderTreeService_WorkerComposition_DisposeDoesNotWaitForDispatcherWork'`

EXIT_CODE: 1

Output Summary: After temporarily reconstructing only the reviewed defect by wrapping the synchronous `dispatcher.Invoke` in `_folderTreeServiceGate`, the solution build passed with the existing six warnings and the deterministic test failed: `Expected enteredGate to be True ... but found False.` The test released the blocked dispatcher in `finally`; the temporary hunk was immediately restored.
