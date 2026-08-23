# [P3-T8] Coverage delta — baseline versus post-change

Timestamp: 2026-08-11T01-58

## Source artifacts (cited by path)

- Baseline: `docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/baseline/coverage-collection.2026-08-11T00-30.md`
  (with `docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/baseline/coverage-baseline-extract.2026-08-11T00-30.md`)
- Post-change: `docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/qa-gates/coverage-collection.2026-08-11T01-56.md`
  (with `docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/qa-gates/coverage-final-extract.2026-08-11T01-56.md`)

Both runs executed against the post-#441 arithmetic, on the same branch, in the same worktree, with
the same 9 test assemblies, the same 6435 tests, and the same post-processing procedure. No other
change in this plan touches the coverage pipeline, so the deltas below are attributable solely to
`Remove-CoberturaExemptClosureCoverage`.

## Repository headline figures

| Measure | Baseline (`[P0-T11]`) | Post-change (`[P3-T7]`) | Delta |
|---|---|---|---|
| `lines-covered` | 53663 | 53375 | **−288** |
| `lines-valid` | 62873 | 62401 | **−472** |
| `line-rate` | 0.853514 | 0.855355 | **+0.001841** |
| `branches-covered` | 12609 | 12541 | **−68** |
| `branches-valid` | 15956 | 15872 | **−84** |
| `branch-rate` | 0.790236 | 0.790134 | **−0.000102** |

Interpretation, stated without inflation: 472 lines left the denominator and 288 of them were
**covered** lines that simultaneously left the numerator. The repository line rate rose by 0.18
percentage points, from 85.3514% to 85.5355%, because the removed population was less well covered
than the repository average (288/472 = 61.0% versus 85.4% overall). The branch rate fell very
slightly, by 0.01 percentage points, because the removed branch population was marginally better
covered than average (68/84 = 81.0% versus 79.0% overall).

## `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs`

| Measure | Baseline | Post-change | Delta |
|---|---|---|---|
| class `line-rate` | 0.906977 | 0.991453 | **+0.084476** |
| class `branch-rate` | 0.883333 | 0.896552 | **+0.013219** |
| count of `<line>` elements | 258 | 234 | **−24** |
| count of `<line>` elements with `hits` > 0 | 234 | 232 | **−2** |
| `<method>` count | 28 | 28 | 0 |
| `complexity` | 131 | 124 | −7 |

## `TaskVisualization/FlagTasks.cs`

| Measure | Baseline | Post-change | Delta |
|---|---|---|---|
| `<class>` node count | 1 | **0** | −1 |
| class `name` | `TaskVisualization.FlagTasks.<>c` | absent | — |
| class `line-rate` | 0 | absent | — |
| class `branch-rate` | 0 | absent | — |
| count of `<line>` elements | 10 | absent | −10 |
| count of `<line>` elements with `hits` > 0 | 0 | absent | 0 |

The file is absent from the post-change report entirely. Every member of the type is attributed, so
the only class for that filename was a closure class whose sole method resolved to an absent
declaring member; the filter removed the class and the filename disappeared with it. That is the
correct semantic for a wholly exempt file, and it is a visible change for any downstream consumer
that enumerates files.

## Every figure above is MEASURED, not derived

This section is required by `spec.md` and is the substance of AC 14.

The corrected per-file rate for `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` is **not**
`covered / (valid − 22)`, and no figure in this artifact is arithmetic performed on a pre-fix figure.
Every value in every table was read from the `<class>` and `<coverage>` elements of an actual
post-fix, post-#441 Cobertura document produced by the canonical runner.

The measurement proves the point concretely. Research recorded that
`<>c__DisplayClass42_0`, the closure of the exempt member `DisposeProductionSurface`, contributes two
**covered** lines. A correct fix removes those from the numerator as well as the denominator, and the
measurement confirms exactly that: the covered count fell from 234 to 232, a **−2** delta, at the
same time as the line count fell from 258 to 234.

Had the corrected rate been derived as `covered / (valid − 22)` from the pre-fix figures, it would
have been 234 / 236 = 0.991525. The measured rate is **0.991453** (232 / 234). The derived figure is
wrong in both numerator and denominator, and differs in the sixth decimal place. The 24-line
denominator reduction is also not the 22 that a naive derivation from the issue text would assume.

## Substantive gates

| Gate | Requirement | Measured | Result |
|---|---|---|---|
| 1 | post-change `<line>` count for `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` **strictly less than** the `[P0-T11]` baseline count | 234 < 258 | **PASS** |
| 2 | `TaskVisualization/FlagTasks.cs` **absent** from the post-change report | node count 0 (absent) | **PASS** |

Gate 2 is live rather than waived: `[P0-T11]` recorded `TaskVisualization/FlagTasks.cs` as **present**
(1 class node, 10 lines), so the conditional clause in the plan does not apply and the absence
requirement is enforced.

A zero delta on gate 1, or the continued presence of the file in gate 2, would be a failure of this
task rather than an observation to record. Neither occurred. Together these two gates establish that
the filter did real work rather than holding vacuously.

## Threshold note

The post-change repository line rate of 85.5355% is recorded here as a measurement only. The
comparison against documented thresholds, and any handoff arising from it, is `[P3-T9]`'s task. No
threshold is adjusted anywhere in this plan.

## Output Summary

Repository: `lines-covered` −288, `lines-valid` −472, `line-rate` +0.001841 (85.3514% -> 85.5355%);
`branches-covered` −68, `branches-valid` −84, `branch-rate` −0.000102.
`QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs`: 258 -> 234 lines, 234 -> 232 covered, `line-rate`
0.906977 -> 0.991453. `TaskVisualization/FlagTasks.cs`: present -> absent. Both substantive gates
PASS. No percentage in this artifact is derived from a pre-fix figure.
