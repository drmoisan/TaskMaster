# P1-T2 Test Authoring

Timestamp: 2026-09-29T09-12
Task: P1-T2
Command: Write tool (new file tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1); git add -N -- tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1; Grep (count mode) over the new file; git status --porcelain -uall -- tests/scripts/vscode
EXIT_CODE: 0

Output Summary:
- New file authored per the Test Specification: three Describe blocks, two Context blocks, fourteen It blocks, the guarded part-file dot-source, the four fixtures, and the BeforeEach mock set. No existing test file was edited.
- Grep `^\s*It '` count: 14 (expected 14).
- Grep `^Describe '` count: 3 (expected 3).
- Grep `^\s*Context '` count: 2 (expected 2).
- Each of the fourteen It names: exactly 1 matching line each (lines 76, 81, 89, 96, 103, 110, 117, 125, 165, 175, 185, 202, 209, 221).
- Grep count 0 for each of `TestDrive`, `New-TemporaryFile`, `GetTempPath`, `Mock Assert-CoberturaLineCoverageThreshold`, `Mock Assert-CoberturaBranchCoverageThreshold`, `Mock Invoke-DotnetCoverageCollection` (a single alternation over all six tokens returned 0 matches, so each is 0).
- Grep `Coverage threshold assertions skipped` count: 1 (expected 1).
- `Set-StrictMode -Version Latest`: count 1, on line 1.
- Grep pattern `[^\x00-\x7F]` count: 0 (pure ASCII).
- git add -N: exit 0, no output.
- git status --porcelain -uall -- tests/scripts/vscode (verbatim, one line):
  ` A tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1`

## It inventory (Test Specification numbering)

1. returns false when the resolved search root equals the repository root
2. returns false for a dot search root beneath the repository root
3. returns false for a dot-backslash search root beneath the repository root
4. returns false when only a trailing separator differs
5. returns false when only letter case differs
6. returns true for a subdirectory search root
7. returns true for a sibling directory whose name extends the repository root name
8. returns true for the parent directory of the repository root
9. completes without error on a scoped run whose post-processed document is below both floors
10. writes exactly one warning naming the skipped assertions and the scoped search root
11. still terminates with an error when collection returns a non-zero exit code on a scoped run
12. throws the line threshold message when the search root is omitted and the line rate is below 80 percent
13. throws the branch threshold message for a dot search root when the branch rate is below 75 percent
14. documents the scoped-run behavior on the SearchRoot parameter
