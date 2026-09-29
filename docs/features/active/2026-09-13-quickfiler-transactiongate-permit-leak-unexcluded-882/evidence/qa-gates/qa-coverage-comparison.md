# QA Coverage Comparison (P4-T8)

Timestamp: 2026-09-29T09-18
Command: none (reads evidence/baseline/coverage-summary.md, evidence/baseline/baseline-coverage-test-run.md, evidence/qa-gates/coverage-summary.md and evidence/qa-gates/qa-coverage-test-run.md)
Output Summary:
- BASELINE-FIRST-PARTY-LINE-PERCENT: 24.40 (lines 15170/62182)
- FINAL-FIRST-PARTY-LINE-PERCENT: 24.42 (lines 15182/62182)
- BASELINE-FIRST-PARTY-BRANCH-PERCENT: 23.20 (branches 3763/16222)
- FINAL-FIRST-PARTY-BRANCH-PERCENT: 23.20 (branches 3763/16222)
- BASELINE-TOTAL: 1468
- FINAL-TOTAL: 1469 (equal to BASELINE-TOTAL plus 1)
- BASELINE LINE-FLOOR-80-OBSERVATION=FAIL: Cobertura line coverage 24.3961% is below the required 80% threshold.
- BASELINE BRANCH-FLOOR-75-OBSERVATION=FAIL: Cobertura branch coverage 23.1969% is below the required 75% threshold.
- FINAL LINE-FLOOR-80-OBSERVATION=FAIL: Cobertura line coverage 24.4154% is below the required 80% threshold.
- FINAL BRANCH-FLOOR-75-OBSERVATION=FAIL: Cobertura branch coverage 23.1969% is below the required 75% threshold.

Statements:
- The QuickFiler.Test assembly is outside the first-party coverage denominator (Get-KoverageProjectAllowlist drops every assembly whose name ends in .Test), so the change produces no first-party coverage movement: both edited files live in that assembly, and no first-party production file is modified.
- The first-party figures above are the QuickFiler.Test-scoped observation (D1), not the repository-wide floor measurement. The floor outcomes are observations of the single-assembly denominator and are not gates of this plan.
- No coverage delta is asserted.
- New-code coverage for this change is not measurable by line coverage, because every added line lives in the test assembly, which is excluded from instrumentation.
- Observation recorded without inference: the denominator is identical in both runs (62182 lines, 16222 branches), and the covered-line numerator differs by 12 (15170 to 15182). No instrumented file changed between the runs, so the difference does not come from changed code. The cause was not investigated; this plan asserts no delta.
