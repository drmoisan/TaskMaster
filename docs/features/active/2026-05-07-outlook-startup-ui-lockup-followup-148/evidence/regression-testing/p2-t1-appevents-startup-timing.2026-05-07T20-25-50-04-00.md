Timestamp: 2026-05-07T20:25:50-04:00
Task: P2-T1
Test File: TaskMaster.Test/AppGlobals/AppEventsTests.cs
Test Name: LoadAsync_EmitsStartupInboxTimingEnvelopeBeforeDeferredProcessingWindow
Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.Test\TaskMaster.Test.csproj -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }; vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Tests:LoadAsync_EmitsStartupInboxTimingEnvelopeBeforeDeferredProcessingWindow; exit $LASTEXITCODE"
EXIT_CODE: 1
Output Summary:
- Project-scoped nullable build completed successfully.
- VSTest executed the single targeted MSTest from TaskMaster.Test\bin\Debug\TaskMaster.Test.dll.
- The regression test failed before any production fix, which is the expected red-state outcome for this task.
Failure:
- Failed test: LoadAsync_EmitsStartupInboxTimingEnvelopeBeforeDeferredProcessingWindow
- Assertion: Expected Regex.IsMatch(methodBody,@"\[Startup timing\][\s\S]*LoadAsync[\s\S]*deferred processing window[\s\S]*await\s+ProcessNewInboxItemsAsync\s*\(") to be True because LoadAsync should log a startup inbox timing envelope before entering the deferred processing window., but found False.
- Stack location: TaskMaster.Test.AppGlobals.AppEventsTests.LoadAsync_EmitsStartupInboxTimingEnvelopeBeforeDeferredProcessingWindow() in TaskMaster.Test/AppGlobals/AppEventsTests.cs:line 23
Build Result:
- Build succeeded.
- Warnings: 0
- Errors: 0
Test Result:
- Test Run Failed.
- Total tests: 1
- Failed: 1
- Total time: 0.1518 Seconds
Evidence Source:
- Transcript: c:\Users\DanMoisan\AppData\Roaming\Code - Insiders\User\workspaceStorage\a8da1f0c9dae597edcdc167a5b8b2c63\GitHub.copilot-chat\chat-session-resources\d9a71451-4603-4e69-8bbf-7ccf9627280e\call_3BQWuNJnTrnu2sVDP7vwaWvX__vscode-1778175287271\content.txt
