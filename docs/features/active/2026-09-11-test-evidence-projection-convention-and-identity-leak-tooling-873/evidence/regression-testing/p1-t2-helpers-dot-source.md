# P1-T2 — Helpers Dot-Source Addition

Timestamp: 2026-09-13T05-23
Task: [P1-T2]

Command: git diff refs/base-anchor-873 --numstat -- scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
EXIT_CODE: 0

```
1	0	scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
```

Added lines: 1. Removed lines: 0.

Command: git status --porcelain --untracked-files=all
EXIT_CODE: 0

```
 M docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/baseline/p0-t8-msbuild-analyzer-baseline.md
 M docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/baseline/p0-t9-msbuild-nullable-baseline.md
 M scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
?? docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/regression-testing/p1-t1-part-file-structure.md
?? scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1
```

The untracked sibling the anchored name-listing diff cannot see is
`scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1`, the part file P1-T2's added line
dot-sources. The porcelain listing is paired with the diff for exactly that reason.

## Line count

```
POST_EDIT_LINE_COUNT: scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 471
PHASE0_BASELINE_LINE_COUNT: scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 470
```

471 is exactly one greater than the 470 recorded under `POST_FORMAT_BASELINE_LINE_COUNTS:` in the
Phase 0 format-baseline artifact, and 471 is at most 500.

## The added line, verbatim

```
. (Join-Path $PSScriptRoot 'Invoke-MSTestWithCoverage.Projection.ps1')
```

It sits immediately after the four pre-existing part-file dot-sources, which name the closure-filter,
package-rate, threshold and first-party part files, so the five dot-sources form one contiguous block
at the top of the file.

## Output Summary

EXIT_CODE: 0 for both commands. Exactly one line added and zero removed against
`refs/base-anchor-873`; the file measures 471 content lines, one more than the Phase 0 figure of 470
and at most 500. The added line is the dot-source of the projection part file.
