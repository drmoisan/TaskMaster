# R1 P1-T4 Scope Part File Edit

Timestamp: 2026-09-29T10-51
Task: P1-T4 (remediation-plan.2026-09-29T10-00.md)
Command: Write tool (scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1 rewritten exactly per the Production Specification; LF line endings and no byte-order mark, matching the committed file); Grep tool counts; Read tool; git -C <repo-root> status --porcelain -uall -- scripts/vscode
EXIT_CODE: 0

## Edits applied

- Header comment lines 3 to 6 replaced by the four specified lines.
- Predicate `.DESCRIPTION`: last sentence (entry point performs the skip) removed; absolute-input sentence added (five lines).
- `[ValidateNotNullOrEmpty()]` on both predicate parameters; two `IsPathRooted` fail-fast guards before the three existing assignments.
- New function `Assert-CoberturaCoverageThresholdForRun` appended after one empty line; it ends the file.

## Grep counts

- `^function Test-CoverageRunIsScoped \{`: 1 (line 8)
- `^function Assert-CoberturaCoverageThresholdForRun \{`: 1 (line 59)
- `\[ValidateNotNullOrEmpty\(\)\]`: 2 (lines 33, 37)
- `IsPathRooted`: 2 (lines 41, 45)
- `must be an absolute path`: 2 (lines 42, 46)
- `Test-CoverageRunIsScoped -RepoRoot \$RepoRoot -ResolvedSearchRoot \$ResolvedSearchRoot`: 1 (line 96)
- `Coverage threshold assertions skipped`: 1 (line 97)
- `Assert-CoberturaLineCoverageThreshold -CoberturaXml \$CoberturaXml`: 1 (line 102)
- `Assert-CoberturaBranchCoverageThreshold -CoberturaXml \$CoberturaXml`: 1 (line 103, immediately after the line assertion)
- `\[Parameter\(Mandatory = \$true\)\]`: 5 (lines 32, 36, 86, 89, 92)
- `three-file`, `fourth production file`, `Get-ChildItem`, `Test-Path`, `Resolve-Path`, `Get-Content`, `Set-Content`, `[^\x00-\x7F]`: 0 each (one alternation search returned no match)

## File shape

- Line 1: `Set-StrictMode -Version Latest`.
- Read renders 105 numbered lines with line 105 empty (104 newline-terminated; ceiling 120).
- `git status --porcelain -uall -- scripts/vscode`: exactly one line, ` M scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1`.

Output Summary:
- Scope part file edited per specification: predicate hardened (CR-2) and the scoped-run gate function added (R-1).
- Every Grep count matches; the file has no filesystem I/O token and is pure ASCII; 104 lines against a ceiling of 120.
