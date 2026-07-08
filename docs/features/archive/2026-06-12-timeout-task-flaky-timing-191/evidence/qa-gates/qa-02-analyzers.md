# QA-02 — .NET Analyzer Build

Timestamp: 2026-06-13T00-36

Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
(msbuild resolved to "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe"; run with dash-switch syntax under git-bash.)

EXIT_CODE: 0

Output Summary:
- Build succeeded: 0 Error(s), 35 Warning(s).
- All 35 warnings are pre-existing diagnostics in unrelated test files (CS8632 nullable-annotation-context warnings in ApplicationGlobalsTests.cs, StoresWrapperTests.cs, AppToDoObjectsTests.cs, ManualFireTimerWrapper.cs, OlTableExtensions_Tests.cs, ProgressTracker_Tests.cs, ConversationHelper_ExtendedTests.cs; CS0067 unused-event warnings in SmartSerializable_Tests.cs, SmartSerializableBase_Tests.cs, StoreWrapperControllerTests.cs).
- No new analyzer diagnostics on the changed files. A grep of the full build log for `TimeOutTask_Tests.cs` / `TimeOutTask_AdditionalTests.cs` returned NO_WARNINGS_IN_CHANGED_FILES.
