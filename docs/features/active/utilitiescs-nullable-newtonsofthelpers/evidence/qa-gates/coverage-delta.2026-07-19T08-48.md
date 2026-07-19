# Coverage Delta and Changed-Line No-Regression (P9-T6)

- Timestamp: 2026-07-19T08-48
- Inputs: baseline Cobertura `evidence/baseline/coverage-baseline.2026-07-19T08-48.cobertura.xml` (P0-T5) and post-change Cobertura `evidence/qa-gates/final-coverage.2026-07-19T08-48.cobertura.xml` (P9-T4). Same scope/method (UtilitiesCS.Test, `coverage.config` excludes).

## Overall (UtilitiesCS.Test scope)

| Metric | Baseline | Post-change | Delta |
|---|---|---|---|
| Line-rate | 0.7206858 (72.07%) | 0.7207039 (72.07%) | +0.0000181 |
| Branch-rate | 0.4844674 (48.45%) | 0.4845462 (48.45%) | +0.0000788 |
| Lines covered / valid | 98272 / 136359 | 98286 / 136375 | +14 / +16 |

## Targeted UtilitiesCS/NewtonsoftHelpers production (19 in-scope files, dedup)

| Metric | Baseline | Post-change | Delta |
|---|---|---|---|
| Line-rate | 0.9371 (93.71%) | 0.9381 (93.81%) | +0.0010 |
| Lines covered / valid | 1876 / 2002 | 1893 / 2018 | +17 / +16 |

## Changed-line no-regression assessment

- Result: NO coverage regression. The overall line-rate and branch-rate each increased marginally, and the targeted NewtonsoftHelpers production line-rate increased from 93.71% to 93.81%.
- The changed lines are ANNOTATION-ONLY (`#nullable enable` pragmas, `?` annotations, `!` operators, `= null!` field initializers, and `// why` comments). Pragma and comment lines are non-executable and do not enter the coverage denominator. The only structural edit is a behavior-preserving pattern-match tightening in `FilePathHelperConverter.GetErrorMessage` (`if (reader is JsonTextReader)` + separate `as` cast -> `if (reader is JsonTextReader textReader)`), which preserves the same single branch and is exercised by the existing tests.
- The small increases in valid/covered line counts reflect the added annotation lines (e.g., `= null!` initializers and split-out `!`-bearing expressions) that are on already-covered code paths, so they are counted as covered — confirming the changed executable lines are covered.
- All 4511 UtilitiesCS tests pass, and every in-scope batch's tests were green and behavior-identical (Phases 1-8). Outcome: PASS (no remediation required).
