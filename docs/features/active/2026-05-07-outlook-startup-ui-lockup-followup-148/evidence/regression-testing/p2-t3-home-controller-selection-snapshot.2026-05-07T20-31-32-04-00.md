# P2-T3 Red Regression Evidence

- Timestamp: 2026-05-07T20:31:32.5518865-04:00
- Command: pwsh -NoProfile -ExecutionPolicy Bypass -Command "pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath QuickFiler.Test\QuickFiler.Test.csproj -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }; vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Tests:HandleSelectionChangedAsync_CapturesSelectionSnapshotBeforeBackgroundModelLoad; exit $LASTEXITCODE"
- EXIT_CODE: 1
- Output Summary:
  - Project-scoped nullable build for `QuickFiler.Test\QuickFiler.Test.csproj` succeeded with `0 Warning(s)` and `0 Error(s)`.
  - `vstest.console.exe` executed the single targeted MSTest `HandleSelectionChangedAsync_CapturesSelectionSnapshotBeforeBackgroundModelLoad`.
  - The targeted regression failed as expected before the production fix.
- Failure:
  - Expected `Regex.IsMatch(source,@"HandleSelectionChangedAsync[\s\S]*selection snapshot[\s\S]*EfcDataModel\.CreateAsync")` to be `True` because first-selection handling should capture a selection snapshot before background model loading begins, behind an explicit `HandleSelectionChangedAsync` stage boundary, but found `False`.
- Source Transcript:
  - `c:\Users\DanMoisan\AppData\Roaming\Code - Insiders\User\workspaceStorage\a8da1f0c9dae597edcdc167a5b8b2c63\GitHub.copilot-chat\chat-session-resources\d9a71451-4603-4e69-8bbf-7ccf9627280e\call_dgKL28MIcbkshJHMR4PvlK6w__vscode-1778175287335\content.txt`
