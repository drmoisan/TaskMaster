# Issue #608 orchestration-level diagnostic classification correction

Timestamp: 2026-08-25T13-32

## Immutable diagnostic result retained

The cycle-2 executor's `DETERMINISTIC_NON_608_FAILURE` receipt remains unchanged. Its canonical TRX, repetition, coverage-equivalence, classification, and terminal-decision evidence remain valid inputs to this correction.

## Corrected classification

The orchestration-level classification is `DETERMINISTIC_608_FAILURE`.

The repeated failing test is `QuickFiler.Controllers.Tests.QfcStreamingDequeueConfidenceGateTests.DequeueAsync_DeadlineExpiresDuringInFlightScore_IncludesFinalAcceptedItem` in `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs:184`. It asserts the exact pre-fix behavior that Issue #608 changes: return one non-empty accepted prefix at the deadline and stop taking further available qualifying items. Issue #608 requires continued scanning after a non-empty accepted prefix until the requested quantity is met or the source is exhausted. The test is in the same confidence-gate test class as the Issue #608 regressions.

## Scope reconciliation

- Revised budget: one production file and two existing test files.
- The change remains within the C# three-test-file batch cap and the user-authorized Issue #608 defect scope.
- The correction is test-only in `QfcStreamingDequeueConfidenceGateTests.Part2.cs`: preserve the in-flight-score assertion while changing the expected queue result to the fill-or-exhaust outcome.
- Do not change production code, wrappers, policy, project/configuration files, or the distinct #446 worktree.

## Next action

Create and preflight a separate cycle-3 correction-and-QA plan. It must retain all prior evidence, correct only the obsolete Part2 assertion, run complete C# QA, update `spec.md` acceptance criteria from evidence, then perform delegated feature review, PR authoring, and CI continuation.

