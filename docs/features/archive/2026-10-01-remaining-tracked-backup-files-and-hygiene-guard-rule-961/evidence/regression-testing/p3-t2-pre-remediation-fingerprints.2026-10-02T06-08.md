Timestamp: 2026-10-02T06-08
Command: git -C <worktree-root> hash-object -- tests/scripts/hygiene/Test-RepositoryHygiene.Tests.ps1 tests/scripts/hygiene/Test-RepositoryHygiene.Rules.Tests.ps1 .gitignore docs/features/active/2026-10-01-remaining-tracked-backup-files-and-hygiene-guard-rule-961/issue.md; Grep count mode of `^` and `\r$` on .gitignore; Grep count mode of `^` on each of the two test files
EXIT_CODE: 0
Output Summary: Four 40-hex hashes recorded; .gitignore has 370 lines and 370 CRLF line endings (equal); Test-RepositoryHygiene.Tests.ps1 has 204 lines; Test-RepositoryHygiene.Rules.Tests.ps1 has 288 lines (both at most 500).

Hash Before:
- tests/scripts/hygiene/Test-RepositoryHygiene.Tests.ps1: bef00db2d06accf646e611816f8212ed4891ff19
- tests/scripts/hygiene/Test-RepositoryHygiene.Rules.Tests.ps1: db1ded33017df6e04f1b0c42e400a6a1b98c64f5
- .gitignore: e0c040d97fa637a45567f032e9698074bef61b17
- docs/features/active/2026-10-01-remaining-tracked-backup-files-and-hygiene-guard-rule-961/issue.md: 1afdb760bb1a20bdf67362fe40c6a9b9e49a6704

Counts:
- .gitignore `^`: 370
- .gitignore `\r$`: 370
- Test-RepositoryHygiene.Tests.ps1 `^`: 204
- Test-RepositoryHygiene.Rules.Tests.ps1 `^`: 288
