# P0-T5 — Cold NuGet package restore

Timestamp: 2026-09-30T09-16
Command: pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; $before = @(Get-ChildItem -Path ".\packages" -Directory -ErrorAction SilentlyContinue).Count; "PACKAGE_DIRS_BEFORE=$before"; & ".\scripts\vscode\Invoke-Restore.ps1"; "RESTORE_EXIT=$LASTEXITCODE"; $after = @(Get-ChildItem -Path ".\packages" -Directory).Count; "PACKAGE_DIRS_AFTER=$after"; "ALTCOVER_RESTORED=" + (Test-Path "packages\altcover.8.6.45")'
EXIT_CODE: 0
Output Summary:
- PACKAGE_DIRS_BEFORE=0 (fresh worktree; the cold state AC7 names)
- "Using MSBuild: <program-files>\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe"
- Restore target over TaskMaster.sln; output ends "Build succeeded." / "0 Warning(s)" / "0 Error(s)" / "Time Elapsed 00:00:02.75"
- RESTORE_EXIT=0
- PACKAGE_DIRS_AFTER=172 (greater than 100 and not lower than PACKAGE_DIRS_BEFORE)
- ALTCOVER-RESTORED: False (no manifest declares altcover, so the restore did not produce packages\altcover.8.6.45)
