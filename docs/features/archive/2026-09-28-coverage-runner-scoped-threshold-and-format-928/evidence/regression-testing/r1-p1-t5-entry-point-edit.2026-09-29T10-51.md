# R1 P1-T5 Entry-Point Edit

Timestamp: 2026-09-29T10-51
Task: P1-T5 (remediation-plan.2026-09-29T10-00.md)
Command: Edit tool (two edits to scripts/vscode/Invoke-MSTestWithCoverage.ps1); Grep tool counts; Read tool; git -C <repo-root> diff --numstat 177b6d78e -- scripts/vscode/Invoke-MSTestWithCoverage.ps1; git -C <repo-root> diff --numstat 177b6d78e -- scripts/vscode
EXIT_CODE: 0

## Edits applied

1. Lines 311 to 312 (the comment above the Scope part-file dot-source) replaced by the two specified lines; line 313 (the dot-source) unchanged.
2. Lines 404 to 414 (the eleven-line conditional block) replaced by the two-line comment and the four-line backtick-continued call to `Assert-CoberturaCoverageThresholdForRun` (now lines 404 to 409); the first-party report line follows at line 410. The help block (lines 275 to 292) is not edited.

## Grep counts over the entry point

- `Assert-CoberturaCoverageThresholdForRun`: 1 (line 406)
- `Assert-CoberturaLineCoverageThreshold`: 0
- `Assert-CoberturaBranchCoverageThreshold`: 0
- `Test-CoverageRunIsScoped`: 0
- `Coverage threshold assertions skipped`: 0
- `three production files`: 0
- `Write-Warning`: 1 (line 436, the pre-existing test-result summary warning)
- `\.PARAMETER SearchRoot`: 1 (line 284)
- `Invoke-MSTestWithCoverage.Scope.ps1`: 1 (line 313)
- `\.DESCRIPTION`: 6 (unchanged)

(The counts come from one content-mode alternation search whose listing names each matching line.)

## Diff

- `git diff --numstat 177b6d78e -- scripts/vscode/Invoke-MSTestWithCoverage.ps1`: `24	2	scripts/vscode/Invoke-MSTestWithCoverage.ps1`
- `git diff --numstat 177b6d78e -- scripts/vscode`: exactly two paths, `104	0	scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1` and `24	2	scripts/vscode/Invoke-MSTestWithCoverage.ps1` (git also printed its line-ending conversion notice for the Scope part file's working copy)
- Read renders 462 numbered lines with line 462 empty (461 newline-terminated).

Output Summary:
- The eleven-line conditional block is replaced by one unconditional call into the path-loaded Scope part file; the dot-source comment is reworded to drop the change-budget rationale (CR-3).
- Every Grep count, the 24/2 numstat, the two-path listing and the 462-line render match the acceptance conditions.
