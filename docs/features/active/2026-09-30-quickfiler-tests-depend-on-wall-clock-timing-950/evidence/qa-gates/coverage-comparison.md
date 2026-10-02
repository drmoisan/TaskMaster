# Coverage and outcome comparison (P6-T6)

Timestamp: 2026-10-02T01-22
Sources: FEATURE/evidence/baseline/coverage-baseline.md (P0-T16) and FEATURE/evidence/qa-gates/coverage-post-change.md (P6-T5). Both runs used the DIRECT route with the identical four-class exclusion filter.
Command: none (comparison of two recorded artifacts)
EXIT_CODE: 0

Output Summary:

## First-party coverage
- Baseline:    First-party coverage: lines 56204/65855 (85.35%), branches 13618/17078 (79.74%)
- Post-change: First-party coverage: lines 56212/65855 (85.36%), branches 13620/17078 (79.75%)

## Root counters
- Baseline:    ROOT line-rate=0.853451 branch-rate=0.7974 lines-covered=56204 lines-valid=65855 branches-covered=13618 branches-valid=17078
- Post-change: ROOT line-rate=0.853572 branch-rate=0.797517 lines-covered=56212 lines-valid=65855 branches-covered=13620 branches-valid=17078

## Repository-wide comparison
BRANCH A: the two lines-valid figures are equal (65855 and 65855, a difference of 0, within 1 percent of the baseline). The post-change line rate (85.36%) is not lower than the baseline line rate (85.35%); the 0.5 percentage-point tolerance is not needed. Branch rate 79.74% to 79.75%. No COVERAGE REGRESSION.
The small rise (8 lines, 2 branches, all in the UtilitiesCS package) is in code this plan did not change; it is run-to-run variance of the merged rate, which D-7 records as not reproducible across runs of an identical tree.

## New and changed code
CHANGED-PRODUCTION-COVERAGE: NOT MEASURED. Reason: QfcDatamodel carries a type-level [ExcludeFromCodeCoverage] at QuickFiler/Controllers/QfcDatamodel.cs line 25, so the file has no class element in the Cobertura document (QFCDATAMODEL-CLASS-NODES: 0 at both stages). The changed test files are outside the instrumented set (test assemblies are excluded by the derived coverage settings).

## Test outcomes
BASELINE-FAILED-SET: (empty)
FINAL-FAILED-SET: (empty)
NEW-FAILURES: NONE
