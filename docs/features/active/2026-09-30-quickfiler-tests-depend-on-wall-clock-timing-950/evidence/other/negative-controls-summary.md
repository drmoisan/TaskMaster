# Negative-control summary (P5-T25, AC15)

Timestamp: 2026-10-02T01-16

Every value below is copied from the cited artifact. All runs used the repository runsettings (scripts\vscode\TaskMaster.cli.runsettings, Workers=0, Scope=ClassLevel) with the hang-dump blame switch; no run produced a Sequence file, a Timeout, an Aborted or a NotExecuted outcome.

## AC15 table (one row per test named in AC6 to AC13, in that order)

| Test | Mechanism (spec Test Strategy) | Edit applied | Observed outcome | Failure message fragment | Duration | Source artifact |
|---|---|---|---|---|---|---|
| QfcDatamodelLivenessTests.DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle (AC6) | assign a no-op WorkerStarter | A1 | Failed | the synchronous starter must reach the injected RemainingEmailLoader | 00:00:00.1859136 | regression-testing/negative-controls-batch-a.md (P5-T11) |
| QfcDatamodelLivenessTests.RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces (AC7) | no-op WorkerStarter | B1 | Failed | the synchronous starter must reach the injected loader before returning | 00:00:00.2592311 | regression-testing/negative-controls-batch-b.md (P5-T16) |
| QfcDatamodelLivenessTests.RemainingLoadActive_AfterLoaderCompletes_BecomesFalse (AC8) | never set release, then Drain() | A2 | Failed | the finally around the awaited loader must clear the flag once it completes | 00:00:00.0085546 | regression-testing/negative-controls-batch-a.md (P5-T11) |
| QfcDatamodelLivenessTests.RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally (AC9) | never set release, then Drain() | A3 | Failed | the finally must clear the flag on the throwing path too | 00:00:00.0017485 | regression-testing/negative-controls-batch-a.md (P5-T11) |
| QfcDatamodelTeardownTests.Worker_DoWork_CapturesRemainingLoadTask (AC10) | no-op WorkerStarter | A4 | Failed | the synchronous starter must reach the injected RemainingEmailLoader | 00:00:00.1862274 | regression-testing/negative-controls-batch-a.md (P5-T11) |
| QfcInitEmailQueueZeroBatchTests.InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker (AC11) | no-op WorkerStarter | A5 | Failed | the injected RemainingEmailLoader must be invoked by the started worker | 00:00:00.0077444 | regression-testing/negative-controls-batch-a.md (P5-T11) |
| QfcInitEmailQueueZeroBatchTests.InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing (AC12) | remove the WorkerStarter assignment | A6 | Failed | NullReferenceException (at the start site in InitEmailQueue) | 00:00:00.1859851 | regression-testing/negative-controls-batch-a.md (P5-T11) |
| QfcInitEmailQueueZeroBatchTests.InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop (AC12) | remove the WorkerStarter assignment | A7 | Failed | NullReferenceException | 00:00:00.2862076 | regression-testing/negative-controls-batch-a.md (P5-T11) |
| QfcItemController_UiThreadDispatcherFixtureTests.Transaction_SecondCallerCannotInstallUntilTheFirstRestores (AC13) | pre-fix shape plus one injected discarded EnsureUiThreadDispatcher() between the original read and Install(liveA), run alone; then the pinned shape with the same injected call | R-INJECT on the unmodified file (P1-T6); A8 on the pinned file (P5-T11) | Failed (pre-fix, P1-T6); Passed (pinned, P5-T11) | P1-T6: Expected observedByB to refer to null ... Name = "UiThreadDispatcherFixture.ParkedDispatcher"; P5-T11: none (passed) | 00:00:00.1933723 (P1-T6); 00:00:00.0825982 (P5-T11) | regression-testing/r4-fail-before.md (P1-T6); regression-testing/negative-controls-batch-a.md (P5-T11) |

## Drain-dependency controls

Supplements the AC15 table; it does not replace the Test Strategy mechanism above.

| Test | Mechanism (spec Test Strategy) | Edit applied | Observed outcome | Failure message fragment | Duration | Source artifact |
|---|---|---|---|---|---|---|
| QfcDatamodelLivenessTests.RemainingLoadActive_AfterLoaderCompletes_BecomesFalse | release set, Drain() removed (shows the flag clears only through Drain) | C1 | Failed | the finally around the awaited loader must clear the flag once it completes | 00:00:00.1620202 | regression-testing/negative-controls-batch-c.md (P5-T22) |
| QfcDatamodelLivenessTests.RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally | release set, Drain() removed | C2 | Failed | the finally must clear the flag on the throwing path too | 00:00:00.0021400 | regression-testing/negative-controls-batch-c.md (P5-T22) |

Every control that is meant to fail did so at once with an assertion or an exception; none hung.
