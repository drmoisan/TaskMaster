# P2-T2 regression pass: service gate release during UI composition

Timestamp: 2026-08-04T20:26:00-04:00

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /TestCaseFilter:'FullyQualifiedName~FolderTreeService_WorkerComposition_DisposeDoesNotWaitForDispatcherWork'`

EXIT_CODE: 0

Output Summary: With synchronous composition outside `_folderTreeServiceGate`, the solution build passed with the existing six warnings and the regression passed. The worker released the gate while waiting for UI composition; `Dispose` discarded the uncommitted result and the worker observed `ObjectDisposedException`.
