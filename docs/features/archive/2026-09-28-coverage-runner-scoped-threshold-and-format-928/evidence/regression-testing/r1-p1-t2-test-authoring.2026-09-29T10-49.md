# R1 P1-T2 Test Authoring

Timestamp: 2026-09-29T10-49
Task: P1-T2 (remediation-plan.2026-09-29T10-00.md)
Command: Edit tool (one insertion edit to tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1); Grep tool counts; git -C <repo-root> diff --numstat HEAD -- tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1; git -C <repo-root> status --porcelain -uall -- tests/scripts/vscode
EXIT_CODE: 0

## Insertions

- Insertion A (inside Describe `Test-CoverageRunIsScoped`, after It 8): It 15 to 18 at lines 132, 138, 144 and 150.
- Insertion B (new Describe `Assert-CoberturaCoverageThresholdForRun` between the predicate Describe and the entry-point Describe, with one `BeforeEach` registering the Write-Warning mock): It 19 to 22 at lines 163, 173, 186 and 196. Every call passes `-CoberturaXml`, `-RepoRoot $script:fixtureRoot` and `-ResolvedSearchRoot` as backtick-continued named arguments, one per line.

## Grep counts over the test file

- `^\s*It '`: 22
- Each of the eight new It names, count 1 (from the content listing): `throws when the repository root is an empty string` (132); `throws when the resolved search root is an empty string` (138); `throws when the repository root is a relative path` (144); `throws when the resolved search root is a relative path` (150); `does not throw on a scoped run whose document is below both floors` (163); `writes exactly one warning naming the scoped search root` (173); `throws the line threshold message on an unscoped run below the line floor` (186); `throws the branch threshold message on an unscoped run at the line floor and below the branch floor` (196)
- `^Describe '`: 4
- `^\s*Context '`: 2
- `Assert-CoberturaCoverageThresholdForRun`: 5 (the Describe title and four calls)
- `must be an absolute path`: 2
- `Mock Write-Warning`: 2
- `TestDrive`, `New-TemporaryFile`, `GetTempPath`, `Mock Assert-CoberturaLineCoverageThreshold`, `Mock Assert-CoberturaBranchCoverageThreshold`, `Mock Invoke-DotnetCoverageCollection`, `[^\x00-\x7F]`: 0 (one alternation search returned no match, so each is 0)
- `Set-StrictMode -Version Latest`: 1, on line 1

## Diff and status

- `git diff --numstat HEAD -- tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1`: `74	0	tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1` (at least 18 added; 0 deleted, insertion only; git also printed its line-ending conversion notice for the working copy)
- `git status --porcelain -uall -- tests/scripts/vscode`: exactly one line, ` M tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1`

Output Summary:
- Eight It blocks added exactly per the Test Specification; the fourteen existing It blocks are untouched (0 deleted lines).
- Every count condition met: 22 It, 4 Describe, 2 Context, 5 function-name references, 2 absolute-path messages, 2 Write-Warning mocks, no temporary-file or forbidden-mock token, pure ASCII.
