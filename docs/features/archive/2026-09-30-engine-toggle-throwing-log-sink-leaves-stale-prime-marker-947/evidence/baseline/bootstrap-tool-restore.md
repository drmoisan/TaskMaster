# Bootstrap: Manifest Tool Restore (P0-T6)

Timestamp: 2026-10-01T17-37
Task: P0-T6
Command: dotnet tool restore; dotnet tool list --local; dotnet tool run csharpier check --help
EXIT_CODE: 0

Output Summary:
- "Tool 'csharpier' (version '1.2.6') was restored." / "Restore was successful."
- RESTORE_EXIT=0
- Local tool list (Package Id and Version columns only; the Manifest column carries an absolute path and is omitted):
  - Package Id: csharpier | Version: 1.2.6
- CHECK_HELP_EXIT=0
- Result: P0-T6 acceptance holds.
