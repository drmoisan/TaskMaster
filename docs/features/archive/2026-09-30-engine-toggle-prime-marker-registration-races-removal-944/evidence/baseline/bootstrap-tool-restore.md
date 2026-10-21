# Bootstrap Tool Restore (P0-T10)

Timestamp: 2026-09-30T13-21
Command: pwsh -NoProfile -Command (dotnet tool restore; dotnet tool list --local; dotnet tool run csharpier check --help)
EXIT_CODE: 0
Output Summary: dotnet tool restore restored csharpier 1.2.6 (RESTORE_EXIT=0). Local tool list row: Package Id csharpier, Version 1.2.6. CHECK_HELP_EXIT=0.

## Observed

- RESTORE_EXIT=0 ("Tool 'csharpier' (version '1.2.6') was restored." / "Restore was successful.")
- Local tool list (Package Id and Version columns only; the Manifest column carries an absolute path and is omitted):

| Package Id | Version |
|---|---|
| csharpier | 1.2.6 |

- CHECK_HELP_EXIT=0
