Timestamp: 2026-05-07T20:48:05.2435805-04:00
Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath UtilitiesCS.Test\UtilitiesCS.Test.csproj -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }; vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:GetTableInViewAsync_ReturnsSnapshotWithoutTaskRunWrappedComAccess; exit $LASTEXITCODE"
EXIT_CODE: 1
Failure: The exact approved `P2-T9` command now reaches MSTest execution and exits red before the production fix. After the project-scoped nullable build succeeded and produced `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll`, the focused regression `GetTableInViewAsync_ReturnsSnapshotWithoutTaskRunWrappedComAccess` failed because `UtilitiesCS\OutlookObjects\Table\OlTableExtensions.cs` still uses `Task.Run(view.GetTable, token)` instead of returning a table snapshot without Task.Run-wrapped Outlook COM access.
Output Summary:
- The exact approved project-scoped nullable build command for `UtilitiesCS.Test\UtilitiesCS.Test.csproj` completed successfully with `Build succeeded.` and left `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` ready for MSTest execution.
- The composite exact command then exited with code `1`, which confirms the focused MSTest selection remained red.
- A direct rerun of `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:GetTableInViewAsync_ReturnsSnapshotWithoutTaskRunWrappedComAccess` produced the concise failure details for the same test selection.
- The failing assertion was `Did not expect methodSource ... to contain "Task.Run(" because GetTableInViewAsync should return a table snapshot without Task.Run-wrapped Outlook COM access.`
- The failing test method was `UtilitiesCS.Test.OutlookObjects.Table.OlTableExtensions_Tests.GetTableInViewAsync_ReturnsSnapshotWithoutTaskRunWrappedComAccess`.
Evidence Notes:
- This red regression proves that the current `GetTableInViewAsync(...)` implementation still crosses the Outlook COM boundary through `Task.Run` rather than through an explicit snapshot handoff.
- No production files were changed while capturing this evidence.
