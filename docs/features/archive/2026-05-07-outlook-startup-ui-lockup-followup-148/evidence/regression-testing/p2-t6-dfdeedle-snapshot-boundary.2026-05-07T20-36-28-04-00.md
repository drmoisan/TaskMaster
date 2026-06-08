Timestamp: 2026-05-07T20:36:28.8429424-04:00
Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath UtilitiesCS.Test\UtilitiesCS.Test.csproj -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }; vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform; exit $LASTEXITCODE"
EXIT_CODE: 1
Failure: The required project-scoped nullable build did not reach MSTest execution because unrelated pre-existing nullable diagnostics in UtilitiesCS.Test.csproj still fail under -EnableNullable -TreatWarningsAsErrors. The command therefore cannot yet prove the new regression test failure via vstest.
Output Summary:
- Build reached UtilitiesCS.Test.csproj after the Any CPU platform alias was added.
- The nullable build failed before test execution with pre-existing diagnostics including CS8625 and CS8765 in unrelated files such as UtilitiesCS.Test\OutlookObjects\Attachment\AttachmentHelperTests.cs, UtilitiesCS.Test\NewtonsoftHelpers\MonoExtension_Tests.cs, UtilitiesCS.Test\NewtonsoftHelpers\NonRecursiveConverter_Tests.cs, and multiple UtilitiesCS.Test\OutlookObjects\Folder\*.cs files.
- Because the command exits immediately on the non-zero build, vstest.console.exe did not run the focused test GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform.
- The newly added regression remains red by source intent: UtilitiesCS\Extensions\DfDeedle.cs does not currently contain the required snapshot-boundary contract text matching "table snapshot" followed by "dataframe transform" inside GetEmailDataInViewAsync.
Evidence Notes:
- Command output captured in VS Code session resource content.txt for the terminal invocation at 2026-05-07T20:35 local time.
- This artifact records the blocked red-run state; P2-T6 remains unchecked until the exact command can execute far enough to fail in MSTest for the intended reason.
