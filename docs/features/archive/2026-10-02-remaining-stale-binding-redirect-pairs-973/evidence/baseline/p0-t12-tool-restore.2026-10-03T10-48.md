# P0-T12 manifest tool restore (issue #973)

Timestamp: 2026-10-03T10-48
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "<execution-worktree-root>"; dotnet tool restore; "TOOL-RESTORE-EXIT: " + $LASTEXITCODE; dotnet tool list'
EXIT_CODE: 0
Output Summary: csharpier 1.2.6 restored from dotnet-tools.json; TOOL-RESTORE-EXIT 0; dotnet tool list shows the csharpier row at 1.2.6.

Output (C4):
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier
Restore was successful.
TOOL-RESTORE-EXIT: 0
Package Id      Version      Commands       Manifest
csharpier       1.2.6        csharpier      <execution-worktree-root>\dotnet-tools.json
