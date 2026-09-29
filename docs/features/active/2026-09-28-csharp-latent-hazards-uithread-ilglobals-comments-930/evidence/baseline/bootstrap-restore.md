# Bootstrap: NuGet restore ([P0-T6])

Timestamp: 2026-09-29T08-54
Command: pwsh -NoProfile -Command 'pwsh -NoProfile -File scripts/vscode/Invoke-Restore.ps1; "RESTORE_EXIT=$LASTEXITCODE"; "PACKAGE_DIRS=$(@(Get-ChildItem -Directory -LiteralPath packages).Count)"' (script path passed as its absolute worktree path, REPO-ROOT/scripts/vscode/Invoke-Restore.ps1)
EXIT_CODE: 0
Output Summary:
- MSBuild 18.10.1 Restore target over TaskMaster.sln (Debug|Any CPU); packages.config packages restored.
- Build succeeded. 0 Warning(s), 0 Error(s).
- RESTORE_EXIT=0
- PACKAGE_DIRS=172
