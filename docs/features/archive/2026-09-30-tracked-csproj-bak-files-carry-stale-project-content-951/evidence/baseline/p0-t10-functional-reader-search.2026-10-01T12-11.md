# P0-T10 functional-reader search

Timestamp: 2026-10-01T12-11
Command: git grep -n -I -F ".csproj.bak" origin/main -- scripts .github .claude/lib "*.csproj" "*.sln" "*.targets"
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: Output is empty (exit 1 is git grep's no-match code, confirmed with an explicit exit-code print). No script, workflow, library, project, solution or targets file reads the backups.
