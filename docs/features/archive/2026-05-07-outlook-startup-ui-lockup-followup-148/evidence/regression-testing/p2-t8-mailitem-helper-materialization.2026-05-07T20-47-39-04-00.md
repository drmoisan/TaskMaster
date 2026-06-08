Timestamp: 2026-05-07T20:47:39.0330232-04:00
Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath UtilitiesCS.Test\UtilitiesCS.Test.csproj -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }; vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:FromMailItemAsync_MaterializesComDataBeforeAsyncProjection; exit $LASTEXITCODE"
EXIT_CODE: 1
Failure: The exact approved `P2-T8` command now reaches MSTest execution and exits red before the production fix. After the project-scoped nullable build succeeded and produced `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll`, the focused regression `FromMailItemAsync_MaterializesComDataBeforeAsyncProjection` failed because `UtilitiesCS\OutlookObjects\MailItem\MailItemHelper.cs` still returns a helper built from live COM-backed state without first capturing the expected pure projection snapshot.
Output Summary:
- The exact approved project-scoped nullable build command for `UtilitiesCS.Test\UtilitiesCS.Test.csproj` completed successfully with `Build succeeded.` and left `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` ready for MSTest execution.
- The composite exact command then exited with code `1`, which confirms the focused MSTest selection remained red.
- A direct rerun of `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:FromMailItemAsync_MaterializesComDataBeforeAsyncProjection` produced the concise failure details for the same test selection.
- The failing assertion was `Expected methodSource ... to contain "TryProjectMailItemMembers" because FromMailItemAsync should capture a pure projection snapshot before later async work consumes mail item data.`
- The failing test method was `UtilitiesCS.Test.OutlookObjects.MailItem.MailItemHelperCoreTests.FromMailItemAsync_MaterializesComDataBeforeAsyncProjection`.
Evidence Notes:
- This red regression proves that the current `FromMailItemAsync(...)` path still lacks the explicit pre-projection snapshot boundary required by issue `#148`.
- No production files were changed while capturing this evidence.
