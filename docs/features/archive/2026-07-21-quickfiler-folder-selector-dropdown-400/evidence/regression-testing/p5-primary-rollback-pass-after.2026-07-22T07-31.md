# P5 primary rollback regression pass

Timestamp: 2026-07-22T07:31:38.7395870Z

Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation '/TestCaseFilter:FullyQualifiedName~BreadcrumbDropDownCoverageThresholdTests' '/Logger:console;Verbosity=detailed'`

EXIT_CODE: 0

Result: 7 discovered, 7 passed, 0 failed, 0 skipped. Total time: 1.3676 seconds.

Passed tests:

- `OpenAsync_RollbackCallbackFailsOnce_OuterPipelineCompletesRecovery`
- `OpenAsync_ReadyHandlerResetsLifecycle_RejectsInstalledSurface`
- `OpenAsync_ShowCallbackResetsLifecycle_StopsBeforeFocus`
- `OpenAsync_FocusCallbackFailsAfterShow_ClosesThenPermitsRetry`
- `OpenAsync_ShowCallbackResetsThenThrows_DoesNotOverwriteCurrentLifecycle`
- `OpenAsync_ResetWhileReadinessPending_CancellationRejectsSurface`
- `OpenAsync_LegacyFactoryReturnsNull_ReportsNoSurfaceAndRollsBack`

Contract Summary: The rollback case retained the initiating failure, attempted cancel and anchor focus once each, and observed both rollback secondaries exactly once. The placement case retained and reported the no-space primary, observed the rollback secondary, and did not repeat rollback. Pre-show failures did not invoke native close; the post-show focus failure invoked native close and anchor focus once and permitted a retry. Ready-handler reset and readiness cancellation disposed late controls and messengers once before shared open completion, returned authoritative closed state, and permitted a fresh retry.
