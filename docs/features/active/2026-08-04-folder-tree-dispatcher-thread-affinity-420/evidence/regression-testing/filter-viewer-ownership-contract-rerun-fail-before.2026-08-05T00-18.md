Timestamp: 2026-08-05T00-18
Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' 'UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll' /TestCaseFilter:"FullyQualifiedName~CreateAsync_NullGlobals_DisposesFactoryViewer|FullyQualifiedName~CreateAsync_CompositionFault_DisposesFactoryViewerAndRethrowsOriginal"`
EXIT_CODE: 1
Output Summary: Expected red result: 2 targeted tests executed; 1 passed and 1 failed. The null-globals factory path creates and shows its viewer, preserves `ArgumentNullException.ParamName == "appGlobals"`, but fails ownership cleanup because `CloseCount=0` and `DisposeCount=0` rather than 1. The synchronous FormClosed-add fault path passed its ordering and exact-exception assertions: the FolderTreeService getter was acquired before the add callback threw the exact controlled exception, and it retained zero `SnapshotChanged` handlers.

Pre-run process check:
- No `vstest` or `testhost` process was active.

Replacement H2 red evidence:
- `CreateAsync_NullGlobals_DisposesFactoryViewer` failed at `FilterOlFoldersControllerRefreshDisposalTests.cs:149`: expected `CloseCount=1`, actual `CloseCount=0`.
- The independent ownership observation also recorded `DisposeCount=0`; construction validates `appGlobals` before assigning the factory-created viewer, so the failure cleanup has no owned viewer to close or dispose.
- `CreateAsync_CompositionFault_DisposesFactoryViewerAndRethrowsOriginal` passed: create/show counts were one, the service getter was acquired before the `FormClosed` add callback, the exact `InvalidOperationException` escaped without wrapping, and no `SnapshotChanged` handler remained.
- The test uses a recording fake viewer, Moq strict getters, `AssertionScope` observations, no real viewer, message loop, temporary file, sleep, timer, polling, retry, or global mutable test hook.

Formatting and compilation verification:
- `dotnet tool run csharpier format 'UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersControllerInitializationTests.cs'` exited 0.
- `dotnet tool run csharpier format 'UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersControllerRefreshDisposalTests.cs'` exited 0.
- `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` exited 0 with only pre-existing repository warnings, including CS8632 scheduled for P5-T37.
- Changed test-file counts: `FilterOlFoldersControllerRefreshDisposalTests.cs` is 498 lines and `FilterOlFoldersControllerInitializationTests.cs` is 486 lines.

Result: EXPECTED FAIL. The red result supplies the required replacement evidence for P5-T15; no acceptance criterion is marked complete.
