# P9-T44 Focused 19-Case Pass-After Evidence

Timestamp: 2026-07-27T10:31:41.3083102Z to 2026-07-27T10:31:47.1115118Z

## Command

The owning unbuffered runner directly redirected stdout and stderr from process start and invoked:

```text
C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:<17-method FullyQualifiedName OR filter> /Logger:Console;Verbosity=Detailed /ResultsDirectory:docs\features\active\2026-07-21-quickfiler-folder-selector-dropdown-400\evidence\regression-testing /Logger:trx;LogFileName=nonnumeric-adapter-member-coverage-focused-pass-after.2026-07-27T06-31.trx
```

The exact filter was the OR of the five P9-T39 methods, four P9-T40 methods, and these eight P9-T33 method names: `SurfaceFactory_ReadinessFailure_ReportsOnceThenDisposesSurface`, `SurfaceFactory_NavigationActionFailure_ReportsOnceAndCleansUp`, `SurfaceFactory_WorkerCompletion_DispatchesEveryStageAndCleanup`, `SurfaceFactory_InvalidNavigationResult_ReportsOnceAndCleansUp`, `MouseToggle_FirstOpenFaultsAfterAwait_SecondClickRetriesCleanly`, `SetFolderDroppedDownTrue_UsesSameOpenRequestAsMouseSelectorToggle`, `ViewerAttachment_PendingCachesAndReplaysCurrentStateExactlyOnce`, and `ViewerAttachment_FailureResetReuseAndDisposalLeaveNoStaleAttachment`. The invalid-navigation data test contributed its three rows, for 19 discovered cases.

## Result

- Exit code: `0`
- Timed out: `False`
- Total: `19`
- Passed: `19`
- Failed: `0`
- Skipped: `0`
- Residual issue-#400 VSTest/testhost/dotnet processes: `0`

Passed results: `SetBridgeCoordinator_SameReference_DoesNotDuplicateSubscriptions`; `AttachCollapsedMessenger_Null_ThrowsArgumentNullException`; `AttachCollapsedMessenger_SameReference_ReusesHubAttachment`; `AttachCollapsedMessenger_ReplacementDetachesPrevious`; `DisposedCoordinator_SetBridgeCoordinatorThrows`; `NavigateToDocument_NullDispatcher_ThrowsArgumentNullException`; `NavigateToDocument_NullCore_ThrowsArgumentNullException`; `NavigateToDocument_NullOwner_ThrowsArgumentNullException`; `NavigateToDocumentCore_InjectedBinderReturnsReadiness`; `SurfaceFactory_WorkerCompletion_DispatchesEveryStageAndCleanup`; `SurfaceFactory_NavigationActionFailure_ReportsOnceAndCleansUp`; `SurfaceFactory_ReadinessFailure_ReportsOnceThenDisposesSurface`; `SurfaceFactory_InvalidNavigationResult_ReportsOnceAndCleansUp (0)`; `SurfaceFactory_InvalidNavigationResult_ReportsOnceAndCleansUp (1)`; `SurfaceFactory_InvalidNavigationResult_ReportsOnceAndCleansUp (2)`; `MouseToggle_FirstOpenFaultsAfterAwait_SecondClickRetriesCleanly`; `SetFolderDroppedDownTrue_UsesSameOpenRequestAsMouseSelectorToggle`; `ViewerAttachment_PendingCachesAndReplaysCurrentStateExactlyOnce`; and `ViewerAttachment_FailureResetReuseAndDisposalLeaveNoStaleAttachment`.

## Process and Artifact Receipts

- Runner process ID: `268556`
- VSTest process ID: `164892`
- Observed child processes: `testhost.exe` `272584`; `conhost.exe` `213572`
- Pre-run related processes: none
- Post-run related processes: none
- TRX: `nonnumeric-adapter-member-coverage-focused-pass-after.2026-07-27T06-31.trx` — SHA-256 `77900C27813D10C68E298B637C04BAEEBB3CCDE69AB42B2E1C669AA8D7629B76`
- Stdout: `nonnumeric-adapter-member-coverage-focused-pass-after.2026-07-27T06-31.stdout.txt` — SHA-256 `8FBF25ACE4B0025DB719EC99FA9631AFBDB792244D451FDD34D406B85880464C`
- Stderr: `nonnumeric-adapter-member-coverage-focused-pass-after.2026-07-27T06-31.stderr.txt` — SHA-256 `E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855`
