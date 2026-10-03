# Coverage comparison (issue #968, task P8-T6)

Timestamp: 2026-10-03T03-31
Sources: FEATURE/evidence/baseline/coverage-summary.md (P0-T17) and FEATURE/evidence/qa-gates/coverage-summary.md (P8-T5, ITERATION 1)

## First-party coverage

- Baseline: First-party coverage: lines 56206/65855 (85.35%), branches 13617/17078 (79.73%)
- Post-change: First-party coverage: lines 56211/65855 (85.36%), branches 13620/17078 (79.75%)
- FIRST-PARTY-LINE-DELTA: +0.01
- FIRST-PARTY-BRANCH-DELTA: +0.02
- AC23-STATUS: MET (both deltas are at least 0.00)

## Root counters

- Baseline: ROOT line-rate=0.853481 branch-rate=0.797342 lines-covered=56206 lines-valid=65855 branches-covered=13617 branches-valid=17078
- Post-change: ROOT line-rate=0.853557 branch-rate=0.797517 lines-covered=56211 lines-valid=65855 branches-covered=13620 branches-valid=17078

## Repository-wide comparison

BRANCH A: the two `lines-valid` figures are equal (65855 and 65855; a difference of 0, within 1 percent of the baseline figure). The post-change line rate (0.853557) is not lower than the baseline line rate (0.853481), so it is within the 0.5 percentage-point tolerance. No COVERAGE REGRESSION.

## Changed-code coverage

CHANGED-CODE-COVERAGE: NOT MEASURED (TEST ASSEMBLY EXCLUDED; QFCDATAMODEL EXCLUDED BY ATTRIBUTE)

- TEST_ASSEMBLY_PACKAGES: 0 at baseline and 0 post-change (no test assembly was instrumented, so no changed test line has a coverage figure)
- QFCDATAMODEL_CLASS_ENTRIES: 0 at baseline and 0 post-change (the `[ExcludeFromCodeCoverage]` type `QfcDatamodel` is absent from the report at both stages, so the removed production lines were in no measured denominator; the AC29 reading)

## Test outcomes

- BASELINE-FAILED-SET: (empty)
- FINAL-FAILED-SET: (empty)
- NEW-FAILURES: NONE
