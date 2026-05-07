Timestamp: 2026-05-05T11:05:41.7855415-04:00
Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }; vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Tests:TaskMaster.Test.AppGlobals.AppToDoObjectsTests.LoadProjInfoAsync_DoesNotReadOutlookApplicationFromWorkerThread; exit $LASTEXITCODE"
EXIT_CODE: 0
Failure: TaskMaster.Test.AppGlobals.AppToDoObjectsTests.LoadProjInfoAsync_DoesNotReadOutlookApplicationFromWorkerThread failed with System.InvalidOperationException: Outlook Application getter ran off the caller thread.
Output Summary:
- Nullable build succeeded with 0 warnings and 0 errors.
- Focused VSTest run discovered 1 test and `Test Run Failed.` for `LoadProjInfoAsync_DoesNotReadOutlookApplicationFromWorkerThread`.
- The failure stack shows `UtilitiesCS.IOlObjects.get_App()` reached from `TaskMaster.AppToDoObjects.LoadProjInfoAsync()` while `ProjectData.Rebuild(Parent.Ol.App)` executed on a background task.
- The focused rerun required clearing stale `vstest` and `testhost` processes that had locked `TaskMaster.Test.dll` before the successful evidence capture.
- The installed `vstest.console.exe` returned `EXIT_CODE: 0` even when the targeted test failed, so acceptance is satisfied through the recorded `Test Run Failed.` output and explicit failing test name.
Relevant Stack Frames:
- TaskMaster.Test.AppGlobals.AppToDoObjectsTests.<>c__DisplayClass18_0.<LoadProjInfoAsync_DoesNotReadOutlookApplicationFromWorkerThread>b__0()
- UtilitiesCS.IOlObjects.get_App()
- TaskMaster.AppToDoObjects.<LoadProjInfoAsync>b__21_0()
- TaskMaster.AppToDoObjects.<LoadProjInfoAsync>d__21.MoveNext()
- TaskMaster.Test.AppGlobals.AppToDoObjectsTests.<LoadProjInfoAsync_DoesNotReadOutlookApplicationFromWorkerThread>d__18.MoveNext()
Evidence Source Logs:
- c:\Users\DanMoisan\AppData\Roaming\Code - Insiders\User\workspaceStorage\653bf1e67920176c5d60164d7e4a0163\GitHub.copilot-chat\chat-session-resources\b0deb795-3f48-4148-99bd-e12833985b71\call_aEPaBd2nXnB4kWO0tT9N8WKH__vscode-1777946811202\content.txt
