# All-eight test-assembly isolation diagnosis

- Timestamp (UTC): 2026-07-27T04:37Z
- Scope: P8-T66 diagnosis only. No source, test, project, coverage, settings, filter, exclusion, threshold, or postprocessor change was made.

## Original failure and captured-repeat status

- The first detailed direct all-eight run returned `EXIT_CODE=1`, 6,056 total, 6,055 passed, and 1 failed. Its console stream was truncated before the failure identity, assertion, or stack trace was retained. The exact failing assembly, test identity, and error are therefore unavailable.
- A PowerShell-buffered TRX attempt hung and was terminated after more than five minutes without a TRX. The unbuffered captured all-eight retry completed successfully: `EXIT_CODE=0`, 6,056 total, 6,056 passed, 0 failed, 0 skipped; see `member-coverage-all-eight-determinism-attempt-2.2026-07-27T04-31.trx`.
- The buffered hang does not identify a failing test. It is not classified as a product or test failure because the equivalent unbuffered command completed under the same VSTest arguments.

## Per-assembly isolation

Each assembly was run separately with `scripts/vscode/TaskMaster.cli.runsettings`, `/InIsolation`, `TestCategory!=LiveOutlook`, detailed console logging, and canonical TRX capture.

| Assembly | Passed / total | TRX |
| --- | ---: | --- |
| QuickFiler.Test | 796 / 796 | `member-coverage-isolation-QuickFiler.Test.2026-07-27T04-33.trx` |
| Tags.Test | 65 / 65 | `member-coverage-isolation-Tags.Test.2026-07-27T04-34.trx` |
| TaskMaster.Test | 250 / 250 | `member-coverage-isolation-TaskMaster.Test.2026-07-27T04-34.trx` |
| TaskTree.Test | 51 / 51 | `member-coverage-isolation-TaskTree.Test.2026-07-27T04-34.trx` |
| TaskVisualization.Test | 163 / 163 | `member-coverage-isolation-TaskVisualization.Test.2026-07-27T04-34.trx` |
| ToDoModel.Test | 122 / 122 | `member-coverage-isolation-ToDoModel.Test.2026-07-27T04-35.trx` |
| UtilitiesCS.Test | 4,608 / 4,608 | `member-coverage-isolation-UtilitiesCS.Test.2026-07-27T04-35.trx` |
| VBFunctions.Test | 1 / 1 | `member-coverage-isolation-VBFunctions.Test.2026-07-27T04-36.trx` |

The isolated total is 6,056 / 6,056. No assembly failed or hung; VSTest blame or hang diagnostics were therefore not invoked.

## New and augmented QuickFiler coverage tests

The QuickFiler TRX explicitly records `Passed` outcomes for all seven new P8-T65 methods and for `Host_DisposeAndUseAfterDispose_FollowDeterministicContract`:

- `ArgumentGuards_NullInputsThrowArgumentNullException`
- `RunSynchronous_FailureAbandonsLinkedLeaseAndReportsCancellationFailure`
- `RunAsync_SupersededCancellationIsSwallowedAndSettled`
- `Disposal_RepeatedLifetimeDisposeIsSafeAndLeaseDisposeFailureIsReported`
- `Reset_HostAlreadyClosedWithOpenSelector_CancelsExactlyOnce`
- `SetDroppedDown_CloseThrows_ReportsOnceAndAllowsRetry`
- `PostRenderAndSelectorAsync_StaleLeaseReturnsCompletedWithoutPublishing`
- `Host_DisposeAndUseAfterDispose_FollowDeterministicContract`

## Required plan revision delta

P8-T66 cannot be completed from an unclassified failed first run. Insert the following task immediately after P8-T66 and before Phase 9:

`- [ ] [P8-T67] Preserve the unclassified first P8-T66 failure and the P8-T66 isolation evidence. Run the exact direct eight-assembly command twice without PowerShell output buffering, each with detailed console logging plus a canonical TRX under evidence/regression-testing/. Require both runs to report exactly 6,056 discovered and passed, zero failed/skipped, and require each TRX to contain Passed outcomes for the seven P8-T65 methods plus Host_DisposeAndUseAfterDispose_FollowDeterministicContract. On any failed or hung run, keep P8-T66 and P8-T67 unchecked, rerun only the affected assembly with VSTest blame/hang diagnostics, and produce a new source-fix task before Phase 9. Do not alter coverage policy.`

No exact failing test or error can be reported for the initial run because that output was not retained. The evidence above establishes the current diagnostic boundary but does not classify the original failure as transient.
