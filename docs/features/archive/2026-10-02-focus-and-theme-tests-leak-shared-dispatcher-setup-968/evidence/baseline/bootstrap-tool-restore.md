# Bootstrap: dotnet tool restore (issue #968, task P0-T5)

Timestamp: 2026-10-03T02-43
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; [System.IO.Directory]::SetCurrentDirectory((Get-Location).Path); Write-Output ("WORKTREE-LEAF: " + (Split-Path -Leaf (Get-Location).Path)); dotnet tool restore; "RESTORE_EXIT=$LASTEXITCODE"; dotnet tool list --local; dotnet tool run csharpier check --help | Out-Null; "CHECK_HELP_EXIT=$LASTEXITCODE"'
Canonical command: dotnet tool restore (manifest dotnet-tools.json at the worktree root)
EXIT_CODE: 0
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229
- Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier
- Restore was successful.
- RESTORE_EXIT=0
- Local tool row (Package Id, Version only; the Manifest column carries an absolute path and is not transcribed): csharpier 1.2.6
- CHECK_HELP_EXIT=0
