# Coverage Comparison (Baseline vs Post-Change) — Cycle 2, Issue #218

Timestamp: 2026-06-28T17-31

Command: Parse root `line-rate`/`lines-covered`/`lines-valid` from the merge-base baseline `evidence/baseline/coverage-baseline-218.cobertura.xml` and the post-change `evidence/qa-gates/final-coverage-cycle2-218.cobertura.xml`.

EXIT_CODE: 0

| Artifact | line-rate | lines-covered | lines-valid | Repo-wide % |
|----------|-----------|---------------|-------------|-------------|
| Baseline (merge-base 1b8536b6) | 0.6202918410429243 | 100491 | 162006 | 62.02918410429243% |
| Post-change (cycle-2 final) | 0.6212100678830588 | 100846 | 162338 | 62.12100678830588% |

Delta: +0.09182268401345 percentage points (post-change minus baseline).

No-regression PASS/FAIL on changed lines: PASS. This remediation modified only test sources (QfcDatamodelTests.cs, QfcHomeControllerTests.cs) and the test csproj; no production `.cs` file was changed. Repo-wide line coverage increased (+0.0918 pp); production line coverage cannot regress from a test-only change. (Note: lines-valid grew by 332 because maintainer split 2637e4c1 added newly-counted production code such as EmailSorter and the partial files; lines-covered grew by 355.)

Positive-or-equal-delta PASS/FAIL: PASS (+0.09182268401345 pp, strictly positive).

Output Summary: Post-change repo-wide line coverage = 62.12100678830588% (100846/162338) vs baseline 62.02918410429243% (100491/162006); delta +0.0918 pp. No regression (PASS); positive delta (PASS). Changed-line (Finding 2) detail is in changed-line-coverage-final-cycle2-218.md; repo-wide raw-threshold disposition (Finding 3) is in repo-wide-coverage-exception-cycle2-218.md.
