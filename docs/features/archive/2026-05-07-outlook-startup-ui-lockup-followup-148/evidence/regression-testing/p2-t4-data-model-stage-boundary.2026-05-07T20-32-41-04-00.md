# P2-T4 Red Regression Evidence

- Timestamp: 2026-05-07T20:32:41.5591672-04:00
- Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath QuickFiler.Test\QuickFiler.Test.csproj -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }; vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Tests:CreateAsync_StagesSnapshotLoadBeforeBackgroundInitialization; exit $LASTEXITCODE"
- EXIT_CODE: 1
- Output Summary:
  - Project-scoped nullable build for `QuickFiler.Test\QuickFiler.Test.csproj` succeeded with `0 Warning(s)` and `0 Error(s)`.
  - `vstest.console.exe` executed the single targeted MSTest `CreateAsync_StagesSnapshotLoadBeforeBackgroundInitialization`.
  - The targeted regression failed as expected before the production fix.
- Failure:
  - Expected `Regex.IsMatch(source,@"CreateAsync[\s\S]*snapshot load[\s\S]*background initialization")` to be `True` because `CreateAsync` should define an explicit snapshot-load stage before any background initialization stage begins, but found `False`.
- Source Transcript:
  - `c:\Users\DanMoisan\AppData\Roaming\Code - Insiders\User\workspaceStorage\a8da1f0c9dae597edcdc167a5b8b2c63\GitHub.copilot-chat\chat-session-resources\d9a71451-4603-4e69-8bbf-7ccf9627280e\call_ruUB7Z5J7UvJL8yW4CGCWOCB__vscode-1778175287353\content.txt`
