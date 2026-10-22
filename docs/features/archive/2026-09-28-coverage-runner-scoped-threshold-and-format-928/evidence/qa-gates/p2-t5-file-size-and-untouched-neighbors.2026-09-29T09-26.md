# P2-T5 File Sizes and Untouched Neighbors

Timestamp: 2026-09-29T09-26
Task: P2-T5
Command: git diff --numstat 177b6d78e -- scripts/vscode tests/scripts/vscode; Grep over scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1 for the four AC3 literals
EXIT_CODE: 0

Numstat (verbatim):

```
49	0	scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1
29	2	scripts/vscode/Invoke-MSTestWithCoverage.ps1
232	0	tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1
```

Commit-state reading: the two new files are visible to the anchored numstat because they are committed at 313d0b918, not through the P1-T2 and P1-T4 intent-to-add registrations the task text names. The anchored diff reads the same content either way.

Path check: every listed path is a Write Set PowerShell file; Invoke-MSTestWithCoverage.Threshold.ps1 and Invoke-MSTestWithCoverage.Helpers.ps1 are absent.

Derived line counts:

- Invoke-MSTestWithCoverage.ps1: 439 + 29 - 2 = 466 (at most 500: met)
- Invoke-MSTestWithCoverage.Scope.ps1: added count 49 (at most 60: met)
- Invoke-MSTestWithCoverage.Scope.Tests.ps1: added count 232 (at most 500: met)
- Invoke-MSTest.ps1: absent from the listing, recorded at its base count 262 (at most 500: met)

AC3 literal counts in Invoke-MSTestWithCoverage.Threshold.ps1 (Grep, content mode):

- `-lt 80`: 1 (line 52)
- `-lt 75`: 1 (line 122)
- `required 80% threshold`: 1 (line 54)
- `required 75% threshold`: 1 (line 124)

Output Summary: PASS. Numstat lists only the entry point, the new part file and the new test file; the threshold and helpers files are untouched. Entry point 466 lines, part file 49, test file 232, Invoke-MSTest.ps1 262 (unchanged); all at or under their ceilings. The four AC3 literals each occur exactly once at their base-anchor lines.
