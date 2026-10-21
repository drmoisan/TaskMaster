# Bootstrap: manifest tool restore (P0-T5)

Timestamp: 2026-10-02T00-49
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; [System.IO.Directory]::SetCurrentDirectory((Get-Location).Path); Write-Output ("WORKTREE-LEAF: " + (Split-Path -Leaf (Get-Location).Path)); dotnet tool restore; "RESTORE_EXIT=$LASTEXITCODE"; dotnet tool list --local; dotnet tool run csharpier check --help | Out-Null; "CHECK_HELP_EXIT=$LASTEXITCODE"'
Canonical command: dotnet tool restore
EXIT_CODE: 0

Output Summary:
WORKTREE-LEAF: agent-a7805823735145ca4
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier
Restore was successful.
RESTORE_EXIT=0
Local tool row (Package Id, Version): csharpier, 1.2.6
CHECK_HELP_EXIT=0
