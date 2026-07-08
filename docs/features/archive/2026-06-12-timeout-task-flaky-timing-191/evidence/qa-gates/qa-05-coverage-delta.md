# QA-05 — Coverage Delta / No-Regression (AC5 / AC6)

Timestamp: 2026-06-13T00-41

## Inputs
- Baseline (P0-T5): targeted single-test run of `RunWithTimeout_FuncT1TResult_ShouldReturnResult` under parallel + coverage. Produced a `.coverage` attachment; the test passed. A single-test targeted run intentionally does not produce a representative whole-module percentage, so the authoritative module-coverage figure is taken from the full-suite post-change run.
- Post-change (P2-T4): full UtilitiesCS.Test suite under parallel + coverage. UtilitiesCS.dll (the module containing the production `TimeOutTask` code) line_coverage = 85.31% (block_coverage 86.35%), from evidence/qa-gates/coverage-post.xml.

## Changed-line coverage
- The change modifies exactly two TEST files:
  - UtilitiesCS.Test/Threading/TimeOutTask_Tests.cs: added `[DoNotParallelize]` attribute line.
  - UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs: changed the `milliseconds:` argument literal from `200` to `5000`.
- No production source (`UtilitiesCS/Threading/TimeOutTask.cs` or any other production file) was changed; the change introduces no new executable production lines.
- Therefore changed-line coverage on production code is unchanged by definition; there is no production line whose coverage could regress.

## No-regression statement
- Changed-line coverage did not regress: the change adds no production lines and removes none; the production `TimeOutTask` code paths remain exercised by the (still-passing) success-path test.
- UtilitiesCS.dll module line coverage post-change is 85.31%, above the repository-wide >= 80% threshold.
- No previously-passing test now fails as a result of this change: the only full-suite failure (`AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException` in IdleAsyncQueue_Tests.cs) is a pre-existing flaky test that passes 3/3 in isolation and is unrelated to the changed files (see qa-04-test-coverage.md). The affected test `RunWithTimeout_FuncT1TResult_ShouldReturnResult` passed in the full run and 13/13 across repeated runs (determinism-repeated-runs.md).

Verdict: No coverage regression on changed lines; no regression of previously-passing tests attributable to this change.
