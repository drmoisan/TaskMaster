# Bootstrap Tool Restore (P0-T9)

Timestamp: 2026-10-01T23-04
Command: dotnet tool restore (manifest dotnet-tools.json); dotnet tool list --local; dotnet tool run csharpier check --help
EXIT_CODE: 0
Output Summary: RESTORE_EXIT=0; `Tool 'csharpier' (version '1.2.6') was restored.`; local tool list row csharpier 1.2.6; CHECK_HELP_EXIT=0.

Transcript (Package Id and Version columns only; the Manifest column carries an absolute path and is omitted):

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier
Restore was successful.
RESTORE_EXIT=0
Package Id      Version
csharpier       1.2.6
CHECK_HELP_EXIT=0
```
