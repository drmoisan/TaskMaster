# P2-T10 AC-4 reader search

Timestamp: 2026-10-01T12-16
Command: git grep -n -I -F ".csproj.bak" -- scripts .github .claude/lib "*.csproj" "*.sln" "*.targets"
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: Output is empty (exit 1 is git grep's no-match code, confirmed with an explicit exit-code print). Nothing in the searched paths reads the deleted files on the end-state tree.
