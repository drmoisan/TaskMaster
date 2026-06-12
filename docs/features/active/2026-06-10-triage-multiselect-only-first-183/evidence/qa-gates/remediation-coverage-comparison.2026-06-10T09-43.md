# Remediation QA — Coverage No-Regression Comparison (Cycle 1, Issue #183 R1)

Timestamp: 2026-06-10T09-43

Baseline source: `docs/features/active/2026-06-10-triage-multiselect-only-first-183/evidence/baseline/tests-coverage.2026-06-10T09-13.md`
Post-change source: `docs/features/active/2026-06-10-triage-multiselect-only-first-183/evidence/qa-gates/remediation-tests-coverage.2026-06-10T09-43.md`
(post-change coverage XML: `evidence/qa-gates/coverage-post-remediation.xml`)

## First-party UtilitiesCS.dll line coverage

| Metric | Baseline (09-13) | Post-remediation (09-43) | Delta |
|---|---|---|---|
| lines_covered | 35056 | 35057 | +1 |
| lines_not_covered | 5134 | 5134 | 0 |
| Line coverage (covered / (covered+not_covered)) | 87.23% | 87.23% | 0.00% |

## Verdict

- Repository-wide first-party line coverage remains >= 80% (87.23%). PASS.
- No coverage regression: covered lines did not decrease (35056 -> 35057), not-covered lines unchanged (5134). The change is a test-organization split (move six test methods into a sibling partial-class file) with no production-code change, so production coverage is preserved by construction; the measured figures confirm this.
- `TrainSelectionAsync` method-level coverage is unchanged (the same six tests still execute it; all pass).

VERDICT: PASS — coverage held; no regression.
