# Evidence: QA Analyzer Build

- **Timestamp:** 2026-03-27T08:14 UTC
- **Command:** `pwsh -NoProfile -ExecutionPolicy Bypass -Command "Set-Location 'c:\Users\DanMoisan\repos\TaskMaster-residual-clean'; pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild"`
- **EXIT_CODE:** 0
- **Output Summary:** Build succeeded. 39 warnings, 0 errors. Warnings are pre-existing CS0618 obsolescence warnings from AsyncEnumerable usage across QuickFiler and TaskMaster projects, plus one MSTEST0032 diagnostic in QuickFiler.Test. NuGet packages were restored as a one-time worktree setup step before this build. This matches the baseline warning profile (40 warnings on the mixed branch, 39 here — one fewer because the worktree contains less code scope).
