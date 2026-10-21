# Baseline QuickFiler.Test Run and Coverage (issue #742, [P0-T8])

Timestamp: 2026-09-14T02-04

Command: `pwsh -NoProfile -Command './scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test -Configuration Debug -CoverageOutput "coverage\coverage.cobertura.xml"; Write-Output "EXITCODE=$LASTEXITCODE"'`

EXIT_CODE: 1

Output Summary:

- Test outcome: `Completed`. Counters transcribed from the runner's own trx document
  (`coverage\test-results\mstest-coverage-run.trx`, gitignored and not committed):
  total 1429, executed 1429, **passed 1429, failed 0**, error 0, timeout 0, aborted 0,
  inconclusive 0, notExecuted 0 (skipped 0).
- Coverage document totals: `line-rate` 0.242996, `lines-covered` 15040, `lines-valid` 61894.
- Transcribed class-level `line-rate` figures for the three non-excluded classes named by the task:
  - `QuickFiler.Controllers.QfcHomeController` — `QuickFiler\Controllers\QfcHomeController.Metrics.cs` — line-rate 0.75969, branch-rate 0.791667
  - `QuickFiler.EfcHomeController` — `QuickFiler\Controllers\EfcHomeController.Metrics.cs` — line-rate 1, branch-rate 1
  - `QuickFiler.Controllers.QfcItemController` — `QuickFiler\Controllers\QfcItemController.ViewerSetup.cs` — line-rate 0.906103, branch-rate 0.829268
- `QfcCollectionController` and `EfcItemController` carry a type-level `[ExcludeFromCodeCoverage]`
  attribute (confirmed separately in [P0-T10]) and are **absent** from the report: a query for class
  nodes whose `filename` matches either name returned 0 nodes. Their baseline quality signal is
  therefore the named-test pass/fail state captured above, not a coverage percentage.
- Raw output deleted: `coverage\coverage.cobertura.xml` was removed after transcription
  (`[System.IO.File]::Exists(...)` returned `False` afterwards). No `.trx` and no Cobertura XML was
  written into the feature evidence tree.

Acceptance: none stated by the task; this is a baseline capture only.

## Why EXIT_CODE is 1 despite zero test failures (recorded for [P5-T4])

The non-zero exit code is not a test failure. The runner's own post-run gate threw:

```
Exception: ...\scripts\vscode\Invoke-MSTestWithCoverage.Threshold.ps1:54
Cobertura line coverage 24.3028% is below the required 80% threshold.
```

`Assert-CoberturaLineCoverageThreshold` in `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`
reads the document-level `line-rate` of the `/coverage` element and throws when it is below 80
percent. The threshold is a hard-coded literal `80` in that function; `Invoke-MSTestWithCoverage.ps1`
exposes only `-SearchRoot`, `-Configuration`, `-CoverageOutput` and `-NoExecute`, so no parameter
lowers or disables it.

The document-level denominator covers every instrumented assembly (61894 valid lines), while
`-SearchRoot QuickFiler.Test` executes only the QuickFiler test assembly. A scoped single-assembly
run therefore cannot reach the document-level 80 percent threshold regardless of how many of its own
tests pass. This is a property of the runner on the current tree, independent of this change.

This observation is recorded here because [P5-T4] states its acceptance as `EXITCODE is 0 (zero
failures for QuickFiler.Test ...)`, equating the runner's exit code with the test-failure count. On
this tree those two are not equivalent.
