# Formatter baseline (P0-T9)

Timestamp: 2026-10-02T00-49
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; [System.IO.Directory]::SetCurrentDirectory((Get-Location).Path); Write-Output ("WORKTREE-LEAF: " + (Split-Path -Leaf (Get-Location).Path)); dotnet tool run csharpier check .; "CSHARPIER_EXIT_CODE: $LASTEXITCODE"'
Canonical command: dotnet tool run csharpier check .
EXIT_CODE: 0

Output Summary:
WORKTREE-LEAF: agent-a7805823735145ca4
Checked 1637 files in 5251ms.
CSHARPIER_EXIT_CODE: 0
The baseline is clean; no path was reported.
