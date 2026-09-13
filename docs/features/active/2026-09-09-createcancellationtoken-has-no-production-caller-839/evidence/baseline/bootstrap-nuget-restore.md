# Bootstrap: NuGet restore for the solution (issue #839)

Timestamp: 2026-09-13T02-45
Command: pwsh -NoProfile -Command 'Set-Location REPO-ROOT; & ./scripts/vscode/Invoke-Restore.ps1; "RESTORE_EXIT=$LASTEXITCODE"'
Command: pwsh -NoProfile -Command '"PACKAGE_DIRS=$(@(Get-ChildItem -Directory -LiteralPath packages).Count)"'
EXIT_CODE: 0

Output Summary:
- The restore script resolved MSBuild through vswhere and ran the Restore target with RestorePackagesConfig for the Debug/Any CPU solution configuration.
- Installed: 172 package(s) to packages.config projects.
- Build succeeded. 0 Warning(s), 0 Error(s). Time Elapsed 00:00:02.08.
- RESTORE_EXIT=0.
- PACKAGE_DIRS=172, which is at least 1 as the gate requires.
- The plan's CMD-RESTORE label is the -File form of this invocation. It was run as the -Command form with an explicit Set-Location to the worktree root because the executor's shell does not start at the worktree root and pwsh -File would otherwise resolve the relative script path against the session root. The script invoked and its arguments are otherwise unchanged.
- The restore ran while this item held the shared machine build lock, which was released immediately after the script returned.
