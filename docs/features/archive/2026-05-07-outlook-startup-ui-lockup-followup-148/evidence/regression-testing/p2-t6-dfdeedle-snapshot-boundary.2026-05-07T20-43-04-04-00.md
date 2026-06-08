Timestamp: 2026-05-07T20:43:04.6703907-04:00
Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath UtilitiesCS.Test\UtilitiesCS.Test.csproj -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }; vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform; exit $LASTEXITCODE"
EXIT_CODE: 1
Failure: The focused regression remains red before the production fix. After the exact approved project-scoped nullable build command produced UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll, the targeted MSTest `GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform` failed with a FluentAssertions assertion because `UtilitiesCS\Extensions\DfDeedle.cs` does not yet expose the required snapshot-boundary contract inside `GetEmailDataInViewAsync`.
Output Summary:
- The exact approved `P2-T6` command was rerun after narrow `UtilitiesCS.Test` compatibility fixes were applied to the nullable gate.
- The project-scoped nullable build completed far enough to produce `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll`, which confirms the command is no longer blocked by unrelated `UtilitiesCS.Test` nullable diagnostics.
- A direct rerun of the same `vstest.console.exe` test selection against the produced assembly failed with `FluentAssertions.Execution.AssertionFailedException`.
- The failure message was: `Expected Regex.IsMatch(source,@"GetEmailDataInViewAsync[\s\S]*table snapshot[\s\S]*dataframe transform") to be True because GetEmailDataInViewAsync should capture a table snapshot before it begins the background dataframe transform., but found False.`
- The failing test method was `UtilitiesCS.Test.Extensions.DfDeedle_COM_Tests.GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform`.
Evidence Notes:
- The exact approved build-plus-test command was executed from the repository root during this session after the allowlisted nullable gate was activated for `UtilitiesCS.Test`.
- The concise failing regression output was captured by running `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform` immediately after the exact approved command produced the test assembly.
- This artifact supersedes the earlier blocked-state `P2-T6` artifact by recording that the approved command now reaches MSTest execution and the regression fails for the intended pre-fix reason.
