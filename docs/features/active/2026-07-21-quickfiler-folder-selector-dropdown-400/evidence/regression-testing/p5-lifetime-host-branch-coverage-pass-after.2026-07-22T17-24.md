# P5-T199 — Focused pass-after run for `BreadcrumbPopupBoundaryCoverageTests` (batch N2)

Timestamp: 2026-07-22T17-24Z

Command: `$installation=& 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe' -latest -products * -property installationPath; $vstestPath=Join-Path $installation 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'; $asm=(Resolve-Path 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll').Path; & $vstestPath $asm '/InIsolation' '/TestCaseFilter:FullyQualifiedName~BreadcrumbPopupBoundaryCoverageTests'`

EXIT_CODE: 0

## Discovered case list (23 discovered, 23 passed, 0 failed, 0 skipped)

Eighteen pre-existing cases, all passed:

1. `Dispatcher_NullInputsAndThrowingSink_AreHandledByContract` — Passed
2. `Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction` — Passed
3. `Dispatcher_PostedFailure_ReportsOnceAndRestoresBoundary` — Passed
4. `ProductionFactoryCreate_ControlledContext_CapturesWithoutInvokingAdapters` — Passed
5. `InjectedFactory_Success_UsesOwnerBoundaryAndReturnsReadySurface` — Passed
6. `InjectedFactory_CreateFailure_ReportsOnceWithoutCleanup` — Passed
7. `InjectedFactory_InitializationFailure_DisposesControlOnce` — Passed
8. `InjectedFactory_CoreFailure_DisposesControlOnce` — Passed
9. `InjectedFactory_NavigationFailure_DisposesControlOnce` — Passed
10. `InjectedFactory_CleanupFailure_DoesNotReplacePrimaryFailure` — Passed
11. `Readiness_ConstructorGuardsBlankNameAndNullDetach` — Passed
12. `Readiness_BeginNavigationGuardsNullDuplicateAndTerminalRequests` — Passed
13. `Readiness_UnrelatedAndDuplicateNotifications_CompleteCapturedSuccessOnce` — Passed
14. `Readiness_Failure_NormalizesNullAndBlankStatuses` — Passed
15. `Readiness_CancelAndDispose_AreIdempotent` — Passed
16. `Readiness_DetachFailure_IsContainedAndCompletionSucceeds` — Passed
17. `CaptureCurrentOrTests_NullAndControlledContexts_SelectExpectedBoundaries` — Passed
18. `NormalizeFactory_SuccessAndNullResultPaths_PreserveContract` — Passed

Five new cases, all passed:

19. `OpenAsync_LeaseSupersededDuringInstall_DisposesInstalledSurfaceExactlyOnce` — Passed
20. `OpenAsync_CreationFailsAndCleanupSucceeds_DisposesOwnedSurfaceWithoutReport` — Passed
21. `OpenAsync_CleanupDispatchFails_ReportsSecondaryOnceAndPreservesPrimary` — Passed
22. `OpenAsync_RecoveryDispatchFails_ReportsOnceAndClearsStoredOpenTask` — Passed
23. `NativeClosedCallback_HostClosedBeforeDrain_PerformsNoLateCloseWork` — Passed

## Output Summary

`Test Run Successful. Total tests: 23, Passed: 23, Total time: 1.4410 Seconds`, exit code 0. Exactly 23 cases were
discovered, 23 passed, zero failed, zero skipped. The filter was not narrowed, no case was deleted, and no
assertion was weakened to obtain the pass.

In-batch correction disclosure: an earlier draft of case 19 asserted `ToolStripItem.IsDisposed` on the installed
control host and failed (22/23). Read-only diagnosis of the failure showed the production disposal had in fact
run — `ToolStripControlHost.Dispose()` releases and nulls its hosted `Control` rather than setting the
`ToolStripItem.IsDisposed` flag observable here — so the assertion was replaced with the equivalent, equally
specific disposal proof `((ToolStripControlHost)probe.AddedItem).Control.Should().BeNull(...)`, retained alongside
the unchanged `Surface.DisposeCount == 1`, `MessengerDisposeCount == 1`, and `DropDown.Items.Count == 0`
assertions. No assertion was removed, relaxed, or made conditional, no production code was changed, and no
timing, retry, skip, or exclusion was introduced.
