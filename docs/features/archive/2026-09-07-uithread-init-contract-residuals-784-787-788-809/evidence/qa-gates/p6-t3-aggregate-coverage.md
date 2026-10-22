# [P6-T3] Aggregate first-party coverage against the Phase 0 baseline

Timestamp: 2026-09-08T03-06

Command: read `BASELINE_FIRSTPARTY_LINES_VALID:` from `docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/evidence/baseline/p0-t13-coverage.md` and `FINAL_FIRSTPARTY_LINES_VALID:` from `docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/evidence/qa-gates/p5-t5-tests-coverage.md`, then compare the two aggregates produced by the pinned counting method.

EXIT_CODE: 0

## Denominator comparability test

| Quantity | Value |
|---|---|
| `BASELINE_FIRSTPARTY_LINES_VALID:` | 134023 |
| `FINAL_FIRSTPARTY_LINES_VALID:` | 134111 |
| Absolute difference | 88 |

DENOMINATOR_DELTA_PCT: 0.0657

That is the absolute difference expressed as a percentage of the baseline denominator, to four decimal places. It is at most `1.0000`, so Outcome A applies.

## Outcome A

COVERAGE COMPARISON: COMPARABLE

Both gate conditions are evaluated below.

| Quantity | Baseline | Final | Change | Permitted floor | Met |
|---|---|---|---|---|---|
| First-party line % | 84.58 | 84.62 | +0.04 | at least 84.08, the baseline less 0.50 percentage points | Yes |
| First-party branch % | 79.34 | 79.38 | +0.04 | at least 78.84, the baseline less 0.50 percentage points | Yes |

`FINAL_FIRSTPARTY_LINE_PCT` of 84.62 is at least `BASELINE_FIRSTPARTY_LINE_PCT` of 84.58 minus 0.50 percentage points. `FINAL_FIRSTPARTY_BRANCH_PCT` of 79.38 is at least `BASELINE_FIRSTPARTY_BRANCH_PCT` of 79.34 minus 0.50 percentage points. Both aggregates rose rather than fell, so the no-regression obligation is met with margin rather than by tolerance.

## Supporting figures

| Quantity | Baseline | Final |
|---|---|---|
| First-party lines covered | 113361 | 113481 |
| First-party lines valid | 134023 | 134111 |
| First-party branches covered | 26880 | 26920 |
| First-party branches valid | 33880 | 33912 |

The denominator grew by 88 lines and 32 branches, which is consistent with the 154 lines this delivery added to three production files, of which 48 are executable, plus the compiler-generated lines the new lambda and property members contribute. The numerator grew by 120 lines and 40 branches, which exceeds the denominator growth and is why both percentages rose.

Both figures were produced by the pinned counting method: the all-descendant `.//line` selection over each first-party `<package>`, with the two narrower selections rejected by name; the same nine-name first-party allowlist; and the same `condition-coverage` branch summation. Baseline and final are therefore commensurable by construction, and the denominator test above confirms it numerically.

Outcome B is not recorded, because `DENOMINATOR_DELTA_PCT` did not exceed `1.0000`. Both outcomes are gated; neither is a waiver. The file-scoped measurements in `p6-t1-uithread-file-coverage.md` and `p6-t2-changed-line-coverage.md` stand independently and corroborate this result rather than substituting for it.
