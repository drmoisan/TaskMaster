Timestamp: 2026-05-05T12:07:31.7175611-04:00
Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }; vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Tests:TaskMaster.Test.AppGlobals.ApplicationGlobalsTests.LoadSequentialAsync_KeepsComPhasesOnCallerThreadAndYieldsBetweenHeavyPhases; exit $LASTEXITCODE"
EXIT_CODE: 1
Failure: TaskMaster.Test.AppGlobals.ApplicationGlobalsTests.LoadSequentialAsync_KeepsComPhasesOnCallerThreadAndYieldsBetweenHeavyPhases failed because ApplicationGlobals.LoadSequentialAsync() currently contains no cooperative Task.Yield boundary between heavy startup phases.
Output Summary:
- Nullable build succeeded with 0 warnings and 0 errors.
- Focused VSTest discovered 1 test and `Test Run Failed.` for `LoadSequentialAsync_KeepsComPhasesOnCallerThreadAndYieldsBetweenHeavyPhases`.
- The failing assertion was `Expected yieldMatches.Count to be greater than 0 because the sequential startup coordinator should yield between heavy phases so Outlook can repaint and accept input., but found 0.`
- The test also confirmed the current source still keeps `_olObjects.LoadAsync()` and `_events.LoadAsync()` as direct awaits rather than wrapping them in `Task.Run`.
- The actual execution used the full Visual Studio `vstest.console.exe` path because `vstest.console.exe` was not directly resolvable from the shell PATH during artifact capture.
Relevant Stack Frames:
- TaskMaster.Test.AppGlobals.ApplicationGlobalsTests.LoadSequentialAsync_KeepsComPhasesOnCallerThreadAndYieldsBetweenHeavyPhases()
- FluentAssertions.Numeric.NumericAssertionsBase`3.BeGreaterThan(T expected, String because, Object[] becauseArgs)
Evidence Source Logs:
- c:\Users\DanMoisan\AppData\Roaming\Code - Insiders\User\workspaceStorage\653bf1e67920176c5d60164d7e4a0163\GitHub.copilot-chat\chat-session-resources\b0deb795-3f48-4148-99bd-e12833985b71\call_y1aehrJyMr4mRwzShAJtwfMU__vscode-1777946811315\content.txt
