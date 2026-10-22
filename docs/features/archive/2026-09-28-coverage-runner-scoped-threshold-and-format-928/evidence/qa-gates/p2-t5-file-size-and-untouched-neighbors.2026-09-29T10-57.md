# P2-T5 File Size and Untouched Neighbors (Remediation Cycle 1)

Timestamp: 2026-09-29T10-57
Task: P2-T5 (remediation-plan.2026-09-29T10-00.md; refreshes the original P2-T5 stem)
Command: git -C <repo-root> diff --numstat 177b6d78e -- scripts/vscode tests/scripts/vscode; Grep tool over scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1
EXIT_CODE: 0

## Numstat (verbatim; git also printed its line-ending conversion notices for the two LF working copies)

```
104	0	scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1
24	2	scripts/vscode/Invoke-MSTestWithCoverage.ps1
306	0	tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1
```

Every listed path is a Write Set PowerShell file. scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1 and scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 are absent.

## Derived line counts

- Entry point: 439 + 24 - 2 = 461 (ceiling 500; 461 expected)
- Scope part file: 104 (equal to its added count; ceiling 120; 104 expected)
- Scope test file: 306 (equal to its added count; ceiling 500; about 304 expected)
- Invoke-MSTest.ps1: absent from the listing, recorded at its base count 262 (262 + 0 - 0 = 262; ceiling 500)

## AC3 literals in the Threshold part file

- `-lt 80`: 1 (line 52)
- `required 80% threshold`: 1 (line 54)
- `-lt 75`: 1 (line 122)
- `required 75% threshold`: 1 (line 124)

Output Summary:
- PASS. Three files changed, all in the Write Set; the Threshold and Helpers part files are untouched.
- Line counts: entry point 461, Scope part file 104, Scope test file 306, Invoke-MSTest.ps1 262; all within their ceilings.
- The 80 and 75 literals and messages are unchanged (one each).
