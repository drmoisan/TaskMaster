# [P4-T11] Coverage delta report

Timestamp: 2026-08-27T20-00
Command: comparison of the `[P0-T20]` baseline Cobertura figures against the `[P4-T8]` final Cobertura figures; both produced by `pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\coverage.cobertura.<baseline|final>.xml`
EXIT_CODE: 0
Output Summary: repository-wide line coverage moved from 85.04 to 85.13 percent (delta `+0.09`) and
branch coverage from 79.12 to 79.21 percent (delta `+0.09`). Both deltas are at or above `0.00`, so
the denominator-drift reconciliation branch is **not taken**. Both changed-file line rates rose.

## Repository-wide figures

| Figure | Baseline (`[P0-T20]`) | Final (`[P4-T8]`) | Signed delta |
| --- | --- | --- | --- |
| Line coverage percent | `BaselineLineCoveragePercent` = 85.04 | `FinalLineCoveragePercent` = 85.13 | **+0.09** |
| Branch coverage percent | `BaselineBranchCoveragePercent` = 79.12 | `FinalBranchCoveragePercent` = 79.21 | **+0.09** |
| Raw root `line-rate` | 0.850393 | 0.851295 | — |
| Raw root `branch-rate` | 0.791192 | 0.7921 | — |
| `lines-covered` | 54358 | 54402 | +44 |
| `branches-covered` | 12917 | 12935 | +18 |

## Measurable-line denominators

| Figure | Value |
| --- | --- |
| `BaselineMeasurableLines` (`lines-valid`) | 63921 |
| `FinalMeasurableLines` (`lines-valid`) | 63905 |
| Difference | -16 |

The two denominators are **not** equal. `dotnet-coverage` instruments every assembly loaded at run
time, so its denominator is not stable between two full runs; the 16-line difference is recorded for
completeness. It is not load-bearing here, because both repository-wide deltas are non-negative and
therefore no reconciliation is invoked.

## Per-file line rates for the two changed files

| File | Baseline `line-rate` | Final `line-rate` | Movement |
| --- | --- | --- | --- |
| `QuickFiler/Controllers/KbdActions.cs` | 0.9397590361445783 | 0.9897959183673469 | **higher** |
| `QuickFiler/Controllers/QfcItemController.Navigation.cs` | 0.90678 | 0.92126 | **higher** |

Neither changed-file line rate is lower than its recorded baseline.

## No coverage figure is attributed to `QfcCollectionController.cs`

**No coverage figure is attributed to `QuickFiler/Controllers/QfcCollectionController.cs`.** That
class carries a class-level `[ExcludeFromCodeCoverage]` attribute at its declaration (line 21,
confirmed by `[P0-T13]`), so its lines are outside every coverage denominator, in the baseline
document and in the final document alike (decision D-P4). An XPath query over `//class` filtered on
that filename returns no node in either document. The `[P2-T4]`/`[P2-T5]` edits to that file
therefore neither raise nor lower any coverage figure in this report, and the absence of a figure
for it is correct rather than a missing measurement.

## Repository-wide floors at the Phase 0 baseline (reported, not independently blocking)

The Phase 0 baseline **already met** both repository-wide floors: line 85.04 percent is at or above
the `.claude/rules/general-unit-test.md` and `quality-tiers.md` floor of 85 percent, and above
`CLAUDE.md` §UT2's 80 percent; branch 79.12 percent is above the 75 percent floor. This is recorded
as a reported figure. The binding condition for this feature is no regression against those
baseline figures, which is what the delta table above evaluates. The final figures also clear both
floors (85.13 and 79.21).

## Denominator-drift reconciliation branch

**Not taken.** Neither repository-wide delta is below `0.00`: line is `+0.09` and branch is `+0.09`.
`RECONCILED-DENOMINATOR-DRIFT` is not recorded, and the branch's preconditions are not evaluated,
because the condition that would invoke it (a negative delta) does not hold.

## Acceptance

- Either the repository-wide line delta and branch delta are each greater than or equal to `0.00`,
  or the reconciliation branch holds and is recorded — met on the **first** disjunct: `+0.09` and
  `+0.09`.
- The changed-file line rates are each not lower than their recorded baseline — met; both rose.
- The artifact carries the explicit no-attribution statement for `QfcCollectionController.cs` — met,
  in the section of that name above.
