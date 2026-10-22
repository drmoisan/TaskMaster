# P2-T11 AC-4 search control

Timestamp: 2026-10-01T12-16
Command: git grep -n -I -F ".csproj.bak" -- .gitignore scripts .github .claude/lib "*.csproj" "*.sln" "*.targets"
EXIT_CODE: 0
Output Summary: Exactly one line, from the file `.gitignore`: `.gitignore:258:*.csproj.bak`. This proves the P2-T10 search shape can match and that the only hit across the extended path list is the new ignore rule.
