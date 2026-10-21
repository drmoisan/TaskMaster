# P0-T5 NuGet Restore

Timestamp: 2026-09-29T08-56
Command: pwsh -NoProfile -File scripts\vscode\Invoke-Restore.ps1 (the payload resolved the absolute script path at run time through `Join-Path (Get-Location).Path`, created coverage\logs first, and set MSBUILDDISABLENODEREUSE to 1 before starting the script; output teed to the git-ignored coverage\logs\p0-t5.restore.log; the console view of the tee was limited to its last lines, which does not alter the log)
EXIT_CODE: 0

Output Summary:
- RESTORE_EXIT_CODE: 0
- Restore log tail: "172 package(s) to packages.config projects"; "Build succeeded."; "0 Warning(s)"; "0 Error(s)"
- PACKAGE-DIR-COUNT: 172

Acceptance: EXIT_CODE 0 and PACKAGE-DIR-COUNT at least 1; both hold.
