# [P7-T1] Post-change repository coverage (Issue 638)

Timestamp: 2026-08-29T12-41

Command: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage/coverage.cobertura.xml`

Run through `Start-Process -Wait -NoNewWindow`, with output redirected under `TestResults\`
(gitignored under `.gitignore:39`).

EXIT_CODE: 0

`ExpectedExitCode:` is omitted: the run exited 0, so the non-zero branch does not apply and
this task is not REMEDIATION-REQUIRED.

Output Summary:

## Test outcome

```
Total tests: 6870
     Passed: 6870
 Total time: 44.4908 Seconds
```

The run completed through post-processing, emitting
`Post-processing coverage XML for Koverage compatibility...` and then `Done.`.

## Root `coverage` element attributes

Read from `coverage/coverage.cobertura.xml`:

```
line-rate      = 0.853335
branch-rate    = 0.79311
lines-covered  = 54802
lines-valid    = 64221
package count  = 9
```

POSTCHANGE_REPO_LINE_COVERAGE_PERCENT: 85.33

POSTCHANGE_REPO_BRANCH_COVERAGE_PERCENT: 79.31

COVERAGE_XML_MODE: koverage-processed

The script exited 0, so per the same rule [P0-T12] applies, the mode is
`koverage-processed`. `scripts/vscode/Invoke-MSTestWithCoverage.ps1:341` asserted the
80 percent threshold on the post-processed content and `:343` wrote that content back, so
the file on disk carries the first-party root attributes recomputed by
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:442-445` after `:417-421` removed
every non-allowlisted `<package>`, including every `.Test` assembly. The package count is
9, against the 14 the raw baseline file carried.

## Note on comparability with the baseline

[P0-T12] recorded `COVERAGE_XML_MODE: raw` because that run terminated on a failing test
before reaching post-processing. The two figures are therefore computed over different
denominators. [P7-T3] records that mode mismatch and its consequence.
