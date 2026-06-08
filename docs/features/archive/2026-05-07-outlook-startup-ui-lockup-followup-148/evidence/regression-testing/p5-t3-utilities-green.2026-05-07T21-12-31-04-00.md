# Phase 5 Utilities Green Regression Evidence

Timestamp: 2026-05-07T21:12:31.8572714-04:00
Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath UtilitiesCS.Test\UtilitiesCS.Test.csproj -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }; vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform,GetConversationDfAsync_CapturesConversationTableSnapshotBeforeBackgroundTransform,FromMailItemAsync_MaterializesComDataBeforeAsyncProjection,GetTableInViewAsync_ReturnsSnapshotWithoutTaskRunWrappedComAccess; exit $LASTEXITCODE"
EXIT_CODE: 0
Passing Tests:
- GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform
- GetConversationDfAsync_CapturesConversationTableSnapshotBeforeBackgroundTransform
- FromMailItemAsync_MaterializesComDataBeforeAsyncProjection
- GetTableInViewAsync_ReturnsSnapshotWithoutTaskRunWrappedComAccess
Output Summary:
- The exact Phase 5 Utilities verification command completed successfully.
- The project-scoped `UtilitiesCS.Test\UtilitiesCS.Test.csproj` build completed with referenced projects up to date.
- All four focused Utilities regressions passed on the green path.
- Source Transcript: c:\Users\DanMoisan\AppData\Roaming\Code - Insiders\User\workspaceStorage\a8da1f0c9dae597edcdc167a5b8b2c63\GitHub.copilot-chat\chat-session-resources\d9a71451-4603-4e69-8bbf-7ccf9627280e\call_6vLWE4VVcROREPLBNaa48azg__vscode-1778175287750\content.txt
