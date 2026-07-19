# QC Coverage Delta (P12-T5) — AC6

Timestamp: 2026-07-19T11-57

Method: compare the baseline Cobertura (P0-T3, evidence/baseline/coverage-baseline.cobertura.xml)
against the post-change Cobertura (P12-T4, evidence/qa-gates/coverage-postchange.cobertura.xml).

EXIT_CODE: 0

Output Summary:

| Metric | Baseline (P0-T3) | Post-change (P12-T4) | Delta |
|---|---|---|---|
| Tests passed | 4511 | 4511 | 0 |
| Tests failed | 0 | 0 | 0 |
| UtilitiesCS line-rate | 0.8874674813 (88.75%) | 0.8875250262 (88.75%) | +0.0000575 (neutral, marginally up) |
| UtilitiesCS branch-rate | 0.8251334859 (82.51%) | 0.8251334859 (82.51%) | 0.000000 (identical) |
| Root aggregate line-rate | 0.65299 (65.30%) | 0.653541 (65.35%) | +0.00055 (neutral) |
| Root aggregate branch-rate | 0.613274 (61.33%) | 0.613274 (61.33%) | 0.000000 (identical) |

Conclusion (AC6): No test regressions and no coverage regression on changed lines are attributable to
this child. The edits are annotation-only (`#nullable enable` pragma, `?`, `= null!`, `!`,
`#nullable disable/enable` region markers): they introduce no new executable runtime lines, so coverage
is neutral (the marginal positive delta reflects small denominator shifts, not new uncovered code). The
branch-rate is byte-identical, confirming no new runtime branches were added.

Threshold-source note: the no-regression-on-changed-lines requirement is uniform across CLAUDE.md and
`.claude/rules/general-unit-test.md`. The 80/90 (CLAUDE.md) vs 85/75 (`.claude/rules/general-unit-test.md`)
threshold-source difference is a pre-existing repository conflict flagged to the maintainer at the epic
level and is not resolved by this child.
