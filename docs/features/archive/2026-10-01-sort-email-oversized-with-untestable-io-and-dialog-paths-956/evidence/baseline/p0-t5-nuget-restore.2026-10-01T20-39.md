# P0-T5 NuGet restore

Timestamp: 2026-10-01T20-39
Command: pwsh -NoProfile -File scripts\vscode\Invoke-Restore.ps1 (the absolute script path was resolved at run time with Join-Path (Get-Location).Path after Set-Location to the worktree; MSBUILDDISABLENODEREUSE was 1; output teed to coverage\logs\p0-t5.restore.log; only the last 15 lines were echoed to the console, the full log is on disk and git-ignored)
EXIT_CODE: 0
Output Summary:
- Installed: 172 package(s) to packages.config projects
- Done Building Project "<repo-root>\TaskMaster.sln" (Restore target(s)).
- Build succeeded. 0 Warning(s) 0 Error(s). Time Elapsed 00:00:02.63
- RESTORE_EXIT_CODE: 0
- PACKAGE-DIR-COUNT: 172
Acceptance: EXIT_CODE 0 and PACKAGE-DIR-COUNT at least 1 (holds).
