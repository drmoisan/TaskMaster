# Final QA Gate 4 — QuickFiler.Test with Coverage (issue #742, [P5-T4])

Timestamp: 2026-09-14T02-24

Command: `pwsh -NoProfile -Command './scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test -Configuration Debug -CoverageOutput "coverage\coverage.cobertura.xml"; Write-Output "EXITCODE=$LASTEXITCODE"'`

EXIT_CODE: 1

**Task acceptance NOT met.** [P5-T4] states its acceptance as `EXITCODE is 0`. The observed exit
code is 1. The task is therefore left unchecked in the plan, and the acceptance criterion it carries
is left unchecked in `spec.md`. The cause is recorded in full below; it is a property of the
coverage runner rather than a failure of this change.

## Test result

Transcribed from the runner's own trx document (`coverage\test-results\mstest-coverage-run.trx`,
written under the gitignored `coverage\` directory and not committed):

- outcome: `Completed`
- total 1434, executed 1434, **passed 1434, failed 0**
- error 0, timeout 0, aborted 0, inconclusive 0, notExecuted 0 (skipped 0)

The baseline run in [P0-T8] reported 1429 total / 1429 passed / 0 failed. The increase of exactly 5
is the five new tests this change adds. **Zero failures for `QuickFiler.Test`**, including the two
rewritten oracle tests and the five new tests, all seven of which are also confirmed individually
green in `../regression-testing/phase4-confirmation.2026-09-12T16-09.md`.

## Coverage figures, baseline versus final

Document totals: `line-rate` 0.243093, `lines-covered` 15047, `lines-valid` 61898
(baseline: 0.242996, 15040, 61894).

| Class | File | Baseline line-rate | Final line-rate | Change |
|---|---|---|---|---|
| `QuickFiler.Controllers.QfcHomeController` | `QuickFiler\Controllers\QfcHomeController.Metrics.cs` | 0.75969 | 0.766917 | +0.007227 |
| `QuickFiler.EfcHomeController` | `QuickFiler\Controllers\EfcHomeController.Metrics.cs` | 1 | 1 | unchanged |
| `QuickFiler.Controllers.QfcItemController` | `QuickFiler\Controllers\QfcItemController.ViewerSetup.cs` | 0.906103 | 0.910798 | +0.004695 |

**No regression on the lines this change touched.** Two of the three non-excluded classes rose and
the third was already at 1. Every line edited by this change is inside a method exercised by the new
tests, so each edited line is covered.

`QfcCollectionController` and `EfcItemController` carry a type-level `[ExcludeFromCodeCoverage]`
attribute ([P0-T10]) and remain absent from the report in both runs. Their quality signal is the
named-test pass/fail state, which is green: the two tests exercising their five edited call sites
pass in [P4-T4] and are part of the 1434 above.

Raw output deleted: `coverage\coverage.cobertura.xml` was removed after transcription
(`[System.IO.File]::Exists(...)` returned `False` afterwards). No `.trx` and no Cobertura XML was
written into the feature evidence tree.

## Why EXIT_CODE is 1

The runner's own post-run gate threw:

```
Exception: ...\scripts\vscode\Invoke-MSTestWithCoverage.Threshold.ps1:54
Cobertura line coverage 24.3093% is below the required 80% threshold.
```

`Assert-CoberturaLineCoverageThreshold` in `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`
reads the document-level `line-rate` of the `/coverage` element and throws when it is below 80
percent. The threshold is a hard-coded literal `80` in that function, and
`Invoke-MSTestWithCoverage.ps1` exposes only `-SearchRoot`, `-Configuration`, `-CoverageOutput` and
`-NoExecute`, so no parameter lowers or disables it.

The document-level denominator spans every instrumented assembly (61898 valid lines) while
`-SearchRoot QuickFiler.Test` executes only the QuickFiler test assembly, covering 15047 of them. A
single-assembly scoped run therefore cannot reach the document-level 80 percent threshold no matter
how many of its own tests pass.

The same exit code and the same 24.3 percent figure were observed on the **unfixed** tree in
[P0-T8], before any production edit, and are recorded in
`../baseline/vstest-coverage-baseline.2026-09-12T16-09.md`. The gate is therefore red identically
before and after this change, which is the evidence that this change did not cause it.

The scoping to `QuickFiler.Test` is not optional on this host: an unscoped run executes the
`UtilitiesCS.Test` shell-icon classes known to stall vstest here. The plan selected the scoping for
that reason and equated the runner's exit code with the test-failure count; on this tree those two
are not equivalent.

This is reported to the coordinator as a falsified plan premise rather than adapted. The command was
run exactly as the plan writes it, with no parameter added, removed, or altered.
