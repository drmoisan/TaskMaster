# Fail-before exception dossier (cycle 1, P1-T7)

Timestamp: 2026-10-03T09-23
Command: sources cited: evidence/remediation-baseline/sinkguard-partial-baseline.md, evidence/remediation-baseline/coverage-baseline.md, evidence/regression-testing/cycle1-token-gates.md, evidence/regression-testing/cycle1-fixture-run.md, evidence/remediation-baseline/coordinator-tests-baseline.md
EXIT_CODE: 0
Output Summary: no failing run against the unmodified production code exists for R-1 or R-2; the false-before and true-after pairs are recorded below. The class-node half is appended by P2-T6.

WhyFailingRunImpossible: R-1 adds coverage of a null or empty engine key over production behaviour that is already correct, and R-2 adds assertions over behaviour that is already correct, so no test of either change can fail against the unmodified production code.

## Alternative proof (false before, true after)

- R1-NAME count before the edit: 0 (remediation-baseline/sinkguard-partial-baseline.md); the baseline fixture run reports `rows=0 passed=0` for it (remediation-baseline/coordinator-tests-baseline.md).
- BASELINE-MESSAGES-BRANCH-RATE: 0.5 and BASELINE-COORD-BRANCHES: 43/44 (remediation-baseline/coverage-baseline.md).
- TOKENS-R2 before and after (sinkguard-partial-baseline.md, then regression-testing/cycle1-token-gates.md): `harness\.Engines\.VerifyNoOtherCalls\(\);` 1 then 3; `harness\.Invalidations\.Should\(\)\.BeEmpty\("a refused click changes no state to display"\);` 1 then 3; `Engines\.VerifyNoOtherCalls` 1 then 3.
- TOKENS-R1 before and after: `\[DataTestMethod\]` 0 then 1; `\[DataRow\(null\)\]` 0 then 1; `\[DataRow\(""\)\]` 0 then 1; R1-NAME 0 then 1; `#region ` 2 then 3; `#endregion ` 2 then 3.
- FIXTURE-TOTAL: 45 (regression-testing/cycle1-fixture-run.md) against BASELINE-TOTAL: 43 (remediation-baseline/coordinator-tests-baseline.md); R1-NAME runs 2 rows, both passed.

## CLASS-NODE-PROOF

Messages class-node branch-rate: baseline value 0.5 (remediation-baseline/coverage-baseline.md, read from the pre-change document), final value 1 (qa-gates/cycle1-coverage.md, read from the post-change document; coordinator branches 44/44).
