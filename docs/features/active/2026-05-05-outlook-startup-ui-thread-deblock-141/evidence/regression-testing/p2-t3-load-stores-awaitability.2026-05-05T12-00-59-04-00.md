Timestamp: 2026-05-05T12:00:59.3163791-04:00
Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }; vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Tests:TaskMaster.Test.AppGlobals.AppOlObjectsTests.LoadStoresAsync_DoesNotCompleteBeforeStoreRewireTaskFinishes; exit $LASTEXITCODE"
EXIT_CODE: 0
Failure: TaskMaster.Test.AppGlobals.AppOlObjectsTests.LoadStoresAsync_DoesNotCompleteBeforeStoreRewireTaskFinishes failed because `loadTask.IsCompleted` was `True` before the delayed store-rewire work finished.
Output Summary:
- Nullable build succeeded with 0 warnings and 0 errors.
- Focused VSTest discovered 1 test and `Test Run Failed.` for `LoadStoresAsync_DoesNotCompleteBeforeStoreRewireTaskFinishes`.
- The failing assertion was `Expected loadTask.IsCompleted to be False because LoadStoresAsync should await store rewire completion., but found True.`
- This proves `TaskMaster.AppOlObjects.LoadStoresAsync()` currently reports completion before the simulated store-rewire task finishes.
- The actual execution used the full Visual Studio `vstest.console.exe` path and pre-cleared stale `vstest` and `testhost` processes so the focused run could complete deterministically.
Relevant Stack Frames:
- TaskMaster.Test.AppGlobals.AppOlObjectsTests.<LoadStoresAsync_DoesNotCompleteBeforeStoreRewireTaskFinishes>d__9.MoveNext()
- FluentAssertions.Primitives.BooleanAssertions`1.BeFalse(String because, Object[] becauseArgs)
Evidence Source Logs:
- c:\Users\DanMoisan\AppData\Roaming\Code - Insiders\User\workspaceStorage\653bf1e67920176c5d60164d7e4a0163\GitHub.copilot-chat\chat-session-resources\b0deb795-3f48-4148-99bd-e12833985b71\call_jG9Z8mWSs7RAPFAjWRF8RBWR__vscode-1777946811237\content.txt
