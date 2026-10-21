# Manifest Tool Restore (P0-T5)

Timestamp: 2026-09-29T08-52
Command: pwsh -NoProfile -Command 'Set-Location "<repo-root>"; dotnet tool restore; "EXIT=$LASTEXITCODE"; dotnet tool list --local'
EXIT_CODE: 0
Output Summary:
- Tool 'csharpier' (version '1.2.6') was restored. Restore was successful.
- EXIT=0
- dotnet tool list --local row: csharpier | 1.2.6 | csharpier | <repo-root>\dotnet-tools.json
