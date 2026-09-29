# Bootstrap: manifest tool restore ([P0-T5])

Timestamp: 2026-09-29T08-53
Command: pwsh -NoProfile -Command 'dotnet tool restore; "TOOL_RESTORE_EXIT=$LASTEXITCODE"; dotnet tool list --local; dotnet tool run csharpier check --help | Out-Null; "CSHARPIER_HELP_EXIT=$LASTEXITCODE"'
EXIT_CODE: 0
Output Summary:
- Tool 'csharpier' (version '1.2.6') was restored. Restore was successful.
- TOOL_RESTORE_EXIT=0
- dotnet tool list --local row: Package Id csharpier, Version 1.2.6, Commands csharpier, Manifest REPO-ROOT\dotnet-tools.json
- CSHARPIER_HELP_EXIT=0
