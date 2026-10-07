# Bootstrap: NuGet restore (issue #968, task P0-T6)

Timestamp: 2026-10-03T02-43
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; [System.IO.Directory]::SetCurrentDirectory((Get-Location).Path); Write-Output ("WORKTREE-LEAF: " + (Split-Path -Leaf (Get-Location).Path)); $env:MSBUILDDISABLENODEREUSE = "1"; & (Join-Path (Get-Location).Path "scripts\vscode\Invoke-Restore.ps1"); "RESTORE_EXIT=$LASTEXITCODE"; "PACKAGE_DIRS=$(@(Get-ChildItem -LiteralPath packages -Directory -ErrorAction SilentlyContinue).Count)"'
Canonical command: scripts/vscode/Invoke-Restore.ps1 (msbuild TaskMaster.sln /t:Restore)
EXIT_CODE: 0
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229
- MSBuild version 18.10.1-1.26427.6+3cd27c13e for .NET Framework (resolved MSBuild path: REDACTED-PATH)
- Restore target: Build succeeded. 0 Warning(s), 0 Error(s)
- RESTORE_EXIT=0
- PACKAGE_DIRS=172
