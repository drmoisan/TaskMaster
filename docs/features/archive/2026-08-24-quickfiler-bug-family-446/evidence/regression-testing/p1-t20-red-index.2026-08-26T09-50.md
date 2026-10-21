# [P1-T20] Phase 1 RED-State Commit and `[expect-fail]` TRX Index

Timestamp: 2026-08-26T09-50

Task: [P1-T20]
Feature: docs/features/active/quickfiler-bug-family-446

## Commands

Command: `git add "QuickFiler" "QuickFiler.Test" "docs/features/active/quickfiler-bug-family-446"`
EXIT_CODE: 0

Command: `git commit -m "test(446): failing-first regression tests for 446, 426 and 427-A producer side"`
EXIT_CODE: 0

Resulting HEAD sha: **`68d525a2d9219ee0d00f08418ba0b46d8fc68187`**
(`68d525a2 test(446): failing-first regression tests for 446, 426 and 427-A producer side`,
one commit ahead of `3d4e8e9d chore(446): capture phase 0 policy reads, bootstrap and baselines`.)

Command: `git status --porcelain -- "QuickFiler" "QuickFiler.Test"`
EXIT_CODE: 0
Output: zero lines.

The untracked `.claude/state/` directory was deliberately left unstaged; the `git add` pathspec
names only `QuickFiler`, `QuickFiler.Test` and the feature folder.

## Index of the nine `[expect-fail]` TRX artifacts

| # | Task | Test | TRX path |
| --- | --- | --- | --- |
| 1 | `[P1-T2]` | `DequeueAsync_BelowThresholdCandidate_InvokesOnRejectedOnce` | `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p1-t2/p1-t2.trx` |
| 2 | `[P1-T3]` | `DequeueAsync_OnRejectedThrows_ScanContinues` | `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p1-t3/p1-t3.trx` |
| 3 | `[P1-T6]` | `DequeueNextItemGroupAsync_HighConfidenceRejectedItem_UnhooksFromMoveMonitor` | `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p1-t6/p1-t6.trx` |
| 4 | `[P1-T9]` | `DequeueAsync_DeadlineExpiresWithZeroAccepted_ReportsDeadlineExpiredStop` | `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p1-t9/p1-t9.trx` |
| 5 | `[P1-T10]` | `DequeueAsync_SourceDrained_ReportsSourceExhaustedStop` | `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p1-t10/p1-t10.trx` |
| 6 | `[P1-T12]` | `ScoreRemainingQueueMailItemAsync_ReturnsScoreAndTopFolder` | `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p1-t12/p1-t12.trx` |
| 7 | `[P1-T13]` | `DequeueAsync_AcceptedCandidate_CarriesTopFolderInPreScoredResult` | `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p1-t13/p1-t13.trx` |
| 8 | `[P1-T16]` | `IterateQueueAsync_EmptyBatchWithDeadlineExpired_DoesNotCompleteAdding` | `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p1-t16/p1-t16.trx` |
| 9 | `[P1-T18]` | `DequeueNextItemGroupWithOutcomeAsync_DeadlineExpiredGate_ReportsDeadlineExpiredStop` | `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p1-t18/p1-t18.trx` |

Nine TRX paths, one per `[expect-fail]` task in Phase 1. Every one records its test as **Failed**
with an assertion-failure message (FluentAssertions for eight of them, a Moq `Verify` assertion for
`[P1-T16]`); none failed with a build error or an unhandled exception.

## Committed change set

Production (5 files): `QuickFiler/Controllers/QfcDatamodel.cs`,
`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`,
`QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`,
`QuickFiler/Controllers/QfcHomeController.Iteration.cs`,
`QuickFiler/Interfaces/IQfcDatamodel.cs`.

Test (6 files): `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs`,
`...Part2.cs`, `...Part3.cs`, `QuickFiler.Test/Controllers/QfcDatamodelTests.cs`,
`QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs`,
`QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs`.

No new file was added to either project and neither `.csproj` was edited, per D2 and D4.
`QuickFiler.Test/Controllers/QfcFormControllerTests.cs` is untouched, per D-Plan-2.

## Output Summary

Phase 1 RED state committed at `68d525a2`. `git add` and `git commit` both EXIT_CODE 0;
`git status --porcelain -- "QuickFiler" "QuickFiler.Test"` produces zero output lines. The index
names nine `[expect-fail]` TRX paths, one per Phase 1 `[expect-fail]` task.
