Timestamp: 2026-05-05T09:41:11.7205058-04:00
Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }; vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Tests:TaskMaster.Test.AppGlobals.AppToDoObjectsTests.LoadIdListAsync_DoesNotReadOutlookApplicationFromWorkerThread; exit $LASTEXITCODE"
EXIT_CODE: 0
Failure: TaskMaster.Test.AppGlobals.AppToDoObjectsTests.LoadIdListAsync_DoesNotReadOutlookApplicationFromWorkerThread failed with System.InvalidOperationException: Outlook Application getter ran off the caller thread.
Output Summary:
- Nullable build succeeded with 0 warnings and 0 errors.
- Focused VSTest run discovered 1 test and `Test Run Failed.` for `LoadIdListAsync_DoesNotReadOutlookApplicationFromWorkerThread`.
- The failure stack shows `UtilitiesCS.IOlObjects.get_App()` reached from `TaskMaster.AppToDoObjects.LoadIDList()` inside `LoadIdListAsync()` on a background task.
- Both installed `vstest.console.exe` binaries returned EXIT_CODE=0 even when the targeted test failed, so the plan acceptance clause requiring a non-zero exit code is not yet satisfied.
Relevant Stack Frames:
- TaskMaster.Test.AppGlobals.AppToDoObjectsTests.<>c__DisplayClass15_0.<LoadIdListAsync_DoesNotReadOutlookApplicationFromWorkerThread>b__0()
- UtilitiesCS.IOlObjects.get_App()
- TaskMaster.AppToDoObjects.LoadIDList()
- TaskMaster.AppToDoObjects.<LoadIdListAsync>b__39_0()
- TaskMaster.AppToDoObjects.<LoadIdListAsync>d__39.MoveNext()
Evidence Source Logs:
- c:\Users\DanMoisan\AppData\Roaming\Code - Insiders\User\workspaceStorage\653bf1e67920176c5d60164d7e4a0163\GitHub.copilot-chat\chat-session-resources\b0deb795-3f48-4148-99bd-e12833985b71\call_fo2JFkD0IbHLQxS35h1Kjwc2__vscode-1777946811082\content.txt
- c:\Users\DanMoisan\AppData\Roaming\Code - Insiders\User\workspaceStorage\653bf1e67920176c5d60164d7e4a0163\GitHub.copilot-chat\chat-session-resources\b0deb795-3f48-4148-99bd-e12833985b71\call_VTphHmFfJplw6cmaFltDHjhk__vscode-1777946811084\content.txt
- c:\Users\DanMoisan\AppData\Roaming\Code - Insiders\User\workspaceStorage\653bf1e67920176c5d60164d7e4a0163\GitHub.copilot-chat\chat-session-resources\b0deb795-3f48-4148-99bd-e12833985b71\call_Up5cBOwOH1GchC4dpYoUEWlB__vscode-1777946811087\content.txt
