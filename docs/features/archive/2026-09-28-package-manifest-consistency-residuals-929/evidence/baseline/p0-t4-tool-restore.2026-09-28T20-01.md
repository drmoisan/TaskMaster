# P0-T4 — dotnet tool restore

Timestamp: 2026-09-30T09-15
Command: pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; dotnet tool restore; "TOOL_RESTORE_EXIT=$LASTEXITCODE"'
EXIT_CODE: 0
Output Summary:
- "Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier"
- "Restore was successful."
- TOOL_RESTORE_EXIT=0
- csharpier 1.2.6 is the version the repository-root dotnet-tools.json pins.
