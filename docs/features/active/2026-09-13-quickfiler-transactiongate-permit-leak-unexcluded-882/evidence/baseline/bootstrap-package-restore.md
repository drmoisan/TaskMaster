# NuGet Package Restore (P0-T6)

Timestamp: 2026-09-29T08-52
Command: pwsh -NoProfile -File "<repo-root>/scripts/vscode/Invoke-Restore.ps1" (defaults: TaskMaster.sln, Debug, Any CPU; MSBuild /t:Restore /p:RestorePackagesConfig=true /m; console captured to the gitignored coverage/logs/bootstrap-package-restore.log) ; then pwsh -NoProfile -Command '"PACKAGE-DIRS=" + @(Get-ChildItem packages -Directory).Count; "MSTEST=" + (Test-Path packages/MSTest.TestFramework.4.4.1); "FA=" + (Test-Path packages/FluentAssertions.8.11.0)'
EXIT_CODE: 0
Output Summary:
- Script EXIT_CODE: 0 (SCRIPT-EXIT=0)
- Restore summary: 172 package(s) to packages.config projects; Build succeeded. 0 Warning(s), 0 Error(s)
- PACKAGE-DIRS=172
- MSTEST=True
- FA=True
- Note: both commands ran with the current directory at the worktree root; the script path was passed as an absolute worktree path (rendered here as <repo-root>) because the script resolves the repository root from its own location.
