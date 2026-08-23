# P5 collapsed-readiness harness diagnosis

Timestamp: `2026-07-22T08:18:00Z`

Command: `& { $paths = @('QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs','QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs','QuickFiler/Viewers/BreadcrumbMessengerHub.cs','QuickFiler/Viewers/BreadcrumbCollapsedSurfaceController.cs'); $records = foreach ($path in $paths) { $lines = @(Get-Content $path); $testNames = @(); for ($index = 0; $index -lt $lines.Count; $index++) { if ($lines[$index] -match '\\[TestMethod\\]') { for ($member = $index + 1; $member -lt [Math]::Min($index + 5, $lines.Count); $member++) { if ($lines[$member] -match 'public (?:async )?(?:Task|void) ([A-Za-z0-9_]+)\\(') { $testNames += $Matches[1]; break } } } }; [pscustomobject]@{ Path = $path; Lines = $lines.Count; SHA256 = (Get-FileHash -Algorithm SHA256 $path).Hash; TestNames = $testNames } }; $records | ConvertTo-Json -Compress }`

EXIT_CODE: `0`

Output Summary: `PASS. The defect is confined to the existing ViewerIntegrationHarness in BreadcrumbCollapsedSurfaceReadinessTests.cs. It installs a generic SynchronizationContext without a creator-thread pump, while BreadcrumbCollapsedAttachment.CompleteAsync intentionally captures the owner context. The two Viewer-integration tests await instead of pumping and can restore the prior context from another continuation thread. The existing CapturingSynchronizationContext supplies the required deterministic creator-thread queue and drain behavior. Zero production files and no new helper or test file require correction.`

## Inspected file inventory

| File | Physical lines | SHA-256 | Test count |
| --- | ---: | --- | ---: |
| `QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs` | 468 | `26D4EE038B99078B18534492D7598C52DD7501784235286DDDD549852C2297EA` | 10 |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs` | 480 | `5FD7983359427300F589C0D6A2E80FC00F028DB07613F8948465EB675E1D9AFC` | 4 |
| `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` | 462 | `8721539FB1CE08181F2AD616A061FE70DCC3CF8D6F20796188FFABCC5CA1BC53` | 0 |
| `QuickFiler/Viewers/BreadcrumbCollapsedSurfaceController.cs` | 308 | `92B24E477A20C49ADBD372B42E7A6F22AC7870276789139AA42700BF8AE5FBDE` | 0 |

### `BreadcrumbCollapsedSurfaceReadinessTests` cases

1. `AttachAsync_PendingAndUnrelatedNavigation_DefersReadyPublicationUntilExactSuccess`
2. `AttachAsync_ExactNavigationFailure_LeavesNoReadyMessenger`
3. `Reset_PendingNavigation_CancelsDetachesAndRejectsLateSuccess`
4. `Dispose_PendingNavigation_CancelsDetachesAndRejectsLateSuccess`
5. `LaterNavigation_InvalidatesEarlierGenerationAndPublishesOnlyCurrentMessenger`
6. `ViewerAttachment_PendingCachesAndReplaysCurrentStateExactlyOnce`
7. `ViewerAttachment_FailureResetReuseAndDisposalLeaveNoStaleAttachment`
8. `NavigationReadiness_UnrelatedCompletionCannotReleaseExactNavigation`
9. `NavigationReadiness_SynchronousSuccessDetachesBeforeNavigationReturns`
10. `NavigationReadiness_FailureAndSynchronousExceptionDetachEveryPath`

### `BreadcrumbSelectorToggleUiBoundaryTests` cases

1. `WorkerProviderAndSelectorToggle_MarshalPostsAndCallbackEntryToOwningBoundary`
2. `PopupHost_WorkerCompletions_RunOnlyWhenCreatorThreadDrainsBoundary`
3. `PopupHost_FocusFailureAfterShow_NativeClosesThenRetriesClosedSession`
4. `PopupHost_FirstSchedulingFailure_SettlesFalseThenRetriesAndObservesLifecycle`

## Root cause

`ViewerIntegrationHarness` saves `SynchronizationContext.Current`, installs a base `new SynchronizationContext()`, and later restores the saved instance from `Dispose`. The base context does not provide a deterministic creator-thread queue or an owner-thread pump.

`BreadcrumbCollapsedAttachment.CompleteAsync` intentionally uses `await _controller.AttachAsync(messenger, readiness)` without `ConfigureAwait(false)`. Its comment states that this preserves the `ItemViewer` synchronization context for the hub attachment and state replay. `BreadcrumbCollapsedSurfaceController` performs its readiness coordination off-context and completes a task whose continuation must return to the captured owner context before `BreadcrumbCollapsedAttachment` can attach the messenger and settle the outward completion.

The two affected Viewer-integration cases are:

- `ViewerAttachment_PendingCachesAndReplaysCurrentStateExactlyOnce`
- `ViewerAttachment_FailureResetReuseAndDisposalLeaveNoStaleAttachment`

Both cases complete navigation and then use `await ...ConfigureAwait(false)` on the outward attachment task. They never drive a creator-thread pump for the continuation captured inside `BreadcrumbCollapsedAttachment.CompleteAsync`. Because their own continuation explicitly does not recapture the installed context, the `using` statement can also call `ViewerIntegrationHarness.Dispose` on a different continuation thread, causing the prior synchronization context to be restored from a thread other than its creator.

`BreadcrumbSelectorToggleUiBoundaryTests.CapturingSynchronizationContext` already provides the required deterministic test boundary:

- it records `CreatorThreadId` at construction;
- `Post` enqueues callbacks instead of executing them inline;
- `DrainUntil` synchronously drains until an operation settles;
- `DrainOne` rejects any drain attempt from a non-creator thread;
- queued callbacks execute with the capturing context installed and their thread IDs are recorded;
- callback failures are retained in `ExceptionSnapshot`; and
- each callback restores the prior synchronization context in a `finally` block.

The existing helper can be reused directly. No new helper or test file is needed.

## Scope decision and production integrity

The correction requires zero production files and exactly one existing test file: `QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs`. The production behavior is correct because retaining the `ItemViewer` owner context is required for hub subscription and replay.

The two production files were hashed before and after the read-only inspection. Their hashes were identical:

- `BreadcrumbMessengerHub.cs`: `8721539FB1CE08181F2AD616A061FE70DCC3CF8D6F20796188FFABCC5CA1BC53`
- `BreadcrumbCollapsedSurfaceController.cs`: `92B24E477A20C49ADBD372B42E7A6F22AC7870276789139AA42700BF8AE5FBDE`

No production source, project file, package, runsettings, coverage configuration, exclusion, or additional test file was changed during this diagnosis.
