# Baseline: csharpier check (issue #968, task P0-T9)

Timestamp: 2026-10-03T02-43
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; [System.IO.Directory]::SetCurrentDirectory((Get-Location).Path); Write-Output ("WORKTREE-LEAF: " + (Split-Path -Leaf (Get-Location).Path)); dotnet tool run csharpier check .; "CSHARPIER_EXIT_CODE: $LASTEXITCODE"'
Canonical command: dotnet tool run csharpier check .
EXIT_CODE: 0
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229
- Checked 1637 files in 5261ms.
- CSHARPIER_EXIT_CODE: 0 (format baseline clean; no path reported)
