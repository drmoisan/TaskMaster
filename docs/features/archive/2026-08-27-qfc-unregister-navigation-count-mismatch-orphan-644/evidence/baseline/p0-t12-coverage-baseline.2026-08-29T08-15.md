# Baseline — Repository coverage figure ([P0-T12])

- Issue: #644
- Task: `[P0-T12]`
- Timestamp: 2026-08-29T08-15

Command: `pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\coverage.cobertura.xml`
Working directory: repository root (`<repo-root>`)
EXIT_CODE: 0

## Branch taken

**Branch 1 — the script exited 0.** The figure was therefore read from the written file with
`([xml](Get-Content coverage\coverage.cobertura.xml -Raw)).coverage.'line-rate'`.

Neither of the other two branches applied:

- Branch 2 (below-threshold throw formatted at `Invoke-MSTestWithCoverage.Helpers.ps1` line 489)
  did not fire; the run completed and printed its `Done. Coverage artifact:` line.
- Branch 3 (collection-failure throw formatted at `Invoke-MSTestWithCoverage.ps1` line 236) did
  not fire, so the `REMEDIATION-REQUIRED` reporting branch this task authorizes was **not** taken
  and Phase 1 may proceed.

## The figure is the post-processed one, not the raw collector figure

`Invoke-MSTestWithCoverage.ps1` line 343 writes the post-processed document to disk with
`Set-Content` **after** `ConvertTo-KoverageCoberturaXml` recomputes the root `line-rate` at
`Invoke-MSTestWithCoverage.Helpers.ps1` line 442, having dropped the third-party `<package>`
elements. Because the script exited 0, that `Set-Content` ran, so the file on disk carries the
post-processed figure over the first-party denominator rather than the raw collector figure over
a different denominator. The post-processed document carries 9 `<package>` elements.

## Test run output tail (host paths redacted)

```
Test Run Successful.
Total tests: 6870
     Passed: 6870
 Total time: 1.0548 Minutes
Code coverage results: <repo-root>\coverage\coverage.cobertura.xml.
Post-processing coverage XML for Koverage compatibility...
Done. Coverage artifact: <repo-root>\coverage\coverage.cobertura.xml
```

## BASELINE COVERAGE PERCENT

| Attribute | Value |
|---|---|
| Root `line-rate` (decimal) | **0.853303** |
| Root `line-rate` (percentage, two places) | **85.33%** |
| `lines-covered` | 54800 |
| `lines-valid` | 64221 |
| Root `branch-rate` (decimal) | 0.793049 |
| `<package>` elements after post-processing | 9 |

**`BASELINE COVERAGE PERCENT` = 85.33%** (decimal `0.853303`). This is the value `[P4-T6]`
consumes for the no-regression comparison. It is a measured number read from the written
post-processed artifact, not a placeholder.

## Why this gate is not vacuously satisfied

`QfcCollectionController` carries `[ExcludeFromCodeCoverage]` on line 21 of
`QuickFiler/Controllers/QfcCollectionController.cs`, so this fix's changed production lines sit
**outside** the coverage denominator and cannot move the repository figure in either direction.
The coverage comparison is therefore a no-regression guard over the rest of the repository rather
than the instrument that proves this fix. The instrument that proves the fix is the six new
regression tests in `[P1-T1]`, demonstrated red in `[P1-T4]` and green in `[P2-T5]` and `[P4-T5]`.

Output Summary: Branch 1 taken — the coverage script exited 0 and wrote the post-processed
Cobertura document. All 6870 tests across the repository passed. **Post-processed root
`line-rate` = 0.853303, i.e. `BASELINE COVERAGE PERCENT` = 85.33%**, over 54800 covered of 64221
valid lines. `QfcCollectionController` carries `[ExcludeFromCodeCoverage]`, so this fix's
production lines are outside the denominator; this is stated so the `[P4-T6]` comparison is not
read as vacuously satisfied.
