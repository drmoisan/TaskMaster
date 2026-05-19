# P2-T5 Red Regression Evidence

- Timestamp: 2026-05-07T20:33:36.6285928-04:00
- Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath QuickFiler.Test\QuickFiler.Test.csproj -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }; vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Tests:LoadDfAsync_ConsumesConversationSnapshotsWithoutRepeatedUiPublishes; exit $LASTEXITCODE"
- EXIT_CODE: 1
- Output Summary:
  - Project-scoped nullable build for `QuickFiler.Test\QuickFiler.Test.csproj` succeeded with `0 Warning(s)` and `0 Error(s)`.
  - `vstest.console.exe` executed the single targeted MSTest `LoadDfAsync_ConsumesConversationSnapshotsWithoutRepeatedUiPublishes`.
  - The targeted regression failed as expected before the production fix.
- Failure:
  - Expected `Regex.IsMatch(source,@"LoadDfAsync[\s\S]*conversation snapshots[\s\S]*repeated ui publishes")` to be `True` because `LoadDfAsync` should consume conversation snapshots before background transforms and avoid repeated UI publishes while that work completes, but found `False`.
- Source Transcript:
  - `c:\Users\DanMoisan\AppData\Roaming\Code - Insiders\User\workspaceStorage\a8da1f0c9dae597edcdc167a5b8b2c63\GitHub.copilot-chat\chat-session-resources\d9a71451-4603-4e69-8bbf-7ccf9627280e\call_KYVlzDpcxQxX6s14VDFu6GxF__vscode-1778175287364\content.txt`
