# Bootstrap: NuGet restore (P0-T6)

Timestamp: 2026-10-02T00-49
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; [System.IO.Directory]::SetCurrentDirectory((Get-Location).Path); Write-Output ("WORKTREE-LEAF: " + (Split-Path -Leaf (Get-Location).Path)); $env:MSBUILDDISABLENODEREUSE = "1"; & (Join-Path (Get-Location).Path "scripts\vscode\Invoke-Restore.ps1"); "RESTORE_EXIT=$LASTEXITCODE"; "PACKAGE_DIRS=$(@(Get-ChildItem -LiteralPath packages -Directory -ErrorAction SilentlyContinue).Count)"'
Canonical command: scripts\vscode\Invoke-Restore.ps1
EXIT_CODE: 0

Output Summary:
WORKTREE-LEAF: agent-a7805823735145ca4
Installed: 172 package(s) to packages.config projects
Build succeeded. 0 Warning(s), 0 Error(s)
RESTORE_EXIT=0
PACKAGE_DIRS=172

Transcription note: the console tail was trimmed for display by a second pwsh process reading the first one's output; the payload itself ran unchanged. Lines naming NuGet config files and feeds carried absolute paths and are omitted.
