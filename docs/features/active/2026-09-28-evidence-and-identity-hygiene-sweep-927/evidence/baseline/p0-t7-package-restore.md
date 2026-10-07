# P0-T7 NuGet package restore

Timestamp: 2026-09-29T08-54
Command: pwsh -NoProfile -Command '& ./scripts/vscode/Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" 2>&1 | Tee-Object -FilePath coverage/logs/927-restore.log; "PACKAGE-DIRS=" + @(Get-ChildItem -Path packages -Directory -ErrorAction SilentlyContinue).Count'; then git status --porcelain -- "*.csproj"
EXIT_CODE: 0
Output Summary:
- The payload additionally printed "RESTORE-LASTEXIT=" + $LASTEXITCODE immediately after the restore invocation so that the restore's own exit code is observable; it read RESTORE-LASTEXIT=0, which is the EXIT_CODE row above.
- Restore log (coverage/logs/927-restore.log, ignored): "Build succeeded.", "0 Warning(s)", "0 Error(s)", "Time Elapsed 00:00:02.69". The resolved build tool leaf was MSBuild.exe.
- PACKAGE-DIRS=172 (greater than 0).
- git status --porcelain -- "*.csproj" printed no line: the restore rewrote no project file (no STOP: RESTORE REWROTE PROJECT FILE).
