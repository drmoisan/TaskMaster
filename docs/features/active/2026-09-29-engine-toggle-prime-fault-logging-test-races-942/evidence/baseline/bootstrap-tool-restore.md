# Bootstrap: dotnet manifest tool restore (issue 942)

Timestamp: 2026-09-30T07-25
Task: P0-T5
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; dotnet tool restore; "RESTORE_EXIT=$LASTEXITCODE"; dotnet tool list --local; dotnet tool run csharpier check --help | Out-Null; "CHECK_HELP_EXIT=$LASTEXITCODE"'
EXIT_CODE: 0

Output Summary:
- dotnet tool restore: "Tool 'csharpier' (version '1.2.6') was restored." and "Restore was successful."
- RESTORE_EXIT=0
- dotnet tool list --local (Package Id and Version columns only; the Manifest column carries an absolute path and is not transcribed):
  - Package Id: csharpier | Version: 1.2.6
- CHECK_HELP_EXIT=0
