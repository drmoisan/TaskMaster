Timestamp: 2026-08-05T05:15:00-04:00

Status: Passed. This replaces neither the historical failed run nor its evidence at `remediation-cycle3-pass3-targeted-regressions.2026-08-05T05-09-21.md`.

Change Under Test: When a captured dispatcher exists, disposal queues cleanup through `InvokeAsync(Action)` only while traversal is active. Otherwise it synchronously marshals cleanup through `IUiDispatcher.Invoke(Action)`. A synchronous dispatch failure is reported once through the captured cleanup observer and does not run cleanup off-dispatcher.

Pre-Run Process Check: `Get-Process -Name vstest,testhost -ErrorAction SilentlyContinue` returned no processes.

Post-Run Process Check: `Get-Process -Name vstest,testhost -ErrorAction SilentlyContinue` returned no processes.

Serialization Setting: `docs/features/archive/2026-07-16-progress-viewer-cancel-button-339/evidence/other/p2-t4-single-worker-classlevel.2026-07-16T15-49.runsettings` exists and specifies MSTest `Workers=1`, `Scope=ClassLevel`.

Command: `vstest.console.exe TaskMaster.Test\\bin\\Debug\\TaskMaster.Test.dll UtilitiesCS.Test\\bin\\Debug\\UtilitiesCS.Test.dll /TestCaseFilter:"FullyQualifiedName~TaskMaster.Test.AppGlobals.AppOlObjectsFolderTreeServiceTests|FullyQualifiedName~TaskMaster.Test.AppGlobals.AppOlObjectsFolderTreeServiceLifecycleTests|FullyQualifiedName~TaskMaster.Test.Ribbon.TryFunctionalityInConstructionTests|FullyQualifiedName~UtilitiesCS.Test.EmailIntelligence.FilterOlFoldersController_Tests|FullyQualifiedName~UtilitiesCS.Test.EmailIntelligence.FilterOlFoldersControllerInitializationTests|FullyQualifiedName~UtilitiesCS.Test.EmailIntelligence.FilterOlFoldersControllerRefreshDisposalTests|FullyQualifiedName~UtilitiesCS.Test.Threading.WpfUiDispatcherTests|FullyQualifiedName~UtilitiesCS.Test.OutlookObjects.Folder.FolderTreeSnapshotBuilderYieldTests|FullyQualifiedName~UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderHierarchyReaderTests|FullyQualifiedName~UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderTreeServiceConcurrencyTests|FullyQualifiedName~UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderTreeServiceDisposalTests|FullyQualifiedName~UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderTreeServiceInvalidationTests|FullyQualifiedName~UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderTreeServiceTraversalCancellationTests|FullyQualifiedName~UtilitiesCS.Test.OutlookObjects.Folder.WpfDispatcherYieldTests" /Settings:docs\\features\\archive\\2026-07-16-progress-viewer-cancel-button-339\\evidence\\other\\p2-t4-single-worker-classlevel.2026-07-16T15-49.runsettings /InIsolation /Logger:"console;verbosity=normal"`

EXIT_CODE: 0

Assemblies:

- `TaskMaster.Test\\bin\\Debug\\TaskMaster.Test.dll`
- `UtilitiesCS.Test\\bin\\Debug\\UtilitiesCS.Test.dll`

Fully Qualified Test Classes:

- `TaskMaster.Test.AppGlobals.AppOlObjectsFolderTreeServiceTests`
- `TaskMaster.Test.AppGlobals.AppOlObjectsFolderTreeServiceLifecycleTests`
- `TaskMaster.Test.Ribbon.TryFunctionalityInConstructionTests`
- `UtilitiesCS.Test.EmailIntelligence.FilterOlFoldersController_Tests`
- `UtilitiesCS.Test.EmailIntelligence.FilterOlFoldersControllerInitializationTests`
- `UtilitiesCS.Test.EmailIntelligence.FilterOlFoldersControllerRefreshDisposalTests` (includes the original and lifecycle-races partial files)
- `UtilitiesCS.Test.Threading.WpfUiDispatcherTests`
- `UtilitiesCS.Test.OutlookObjects.Folder.FolderTreeSnapshotBuilderYieldTests`
- `UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderHierarchyReaderTests`
- `UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderTreeServiceConcurrencyTests`
- `UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderTreeServiceDisposalTests`
- `UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderTreeServiceInvalidationTests`
- `UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderTreeServiceTraversalCancellationTests`
- `UtilitiesCS.Test.OutlookObjects.Folder.WpfDispatcherYieldTests`

Coverage Inventory: The filter includes P1-T1 through P1-T14; P5-T6 through P5-T37; P5-T9 dedicated-STA; P5-T11 captured-dispatcher; AppOlObjects shutdown, setup, and M1; controller ownership, reentrancy, and barriers; H4; M2; and M3. No named class is omitted.

Focused Regression Command: `vstest.console.exe UtilitiesCS.Test\\bin\\Debug\\UtilitiesCS.Test.dll /TestCaseFilter:"FullyQualifiedName~UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderTreeServiceDisposalTests|FullyQualifiedName~UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderTreeServiceInvalidationTests" /InIsolation /Logger:"console;verbosity=normal"`

Focused Regression Result: EXIT_CODE 0; 10 passed. This includes `Dispose_WhenSynchronousCleanupDispatchFails_ReportsOnceWithoutFallbackCleanup`, `NotificationRefreshAndDispose_RunOnTheCapturedDispatcher`, and `NotificationRefresh_RunsOnCapturedDispatcher`.

Toolchain Results: CSharpier check passed for `OutlookFolderTreeService.cs` and `OutlookFolderTreeServiceDisposalTests.cs`. Analyzer build passed with existing System.Reactive packages.config warnings and the existing `PercentageFormatterTests.cs` CS2002 duplicate-source warning. Nullable build passed with existing System.Reactive packages.config warnings.

Output Summary: 91 total tests, 91 passed. The prior run contained 90 selected tests; the current total includes the added synchronous dispatch-failure regression.

No-Live-Outlook-or-UI Proof: The selected tests use deterministic fake folder services, notification sinks, dispatchers, viewers, adapters, and dedicated STA dispatcher hosts. The run opened no Outlook application and no production UI viewer or message loop; `/InIsolation` started isolated test hosts only.

Changed C# Line Counts: All 26 tracked-or-untracked changed C# files are at or below 500 lines.

- `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs`: 490
- `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs`: 490
- `TaskMaster.Test/Ribbon/TryFunctionalityInConstructionTests.cs`: 188
- `TaskMaster/AppGlobals/AppOlObjects.cs`: 448
- `TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs`: 418
- `TaskMaster/Ribbon/RibbonViewer.cs`: 487
- `TaskMaster/Ribbon/TryFunctionalityInConstruction.cs`: 296
- `UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersController_Tests.cs`: 489
- `UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersControllerInitializationTests.cs`: 492
- `UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersControllerRefreshDisposalTests.cs`: 497
- `UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersControllerRefreshDisposalTests.LifecycleRaces.cs`: 234
- `UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeSnapshotBuilderYieldTests.cs`: 149
- `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyReaderTests.cs`: 433
- `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceConcurrencyTests.cs`: 190
- `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceDisposalTests.cs`: 500
- `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceInvalidationTests.cs`: 435
- `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceTraversalCancellationTests.cs`: 500
- `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs`: 39
- `UtilitiesCS.Test/Threading/WpfUiDispatcherTests.cs`: 171
- `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersController.cs`: 191
- `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersController.Lifecycle.cs`: 481
- `UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotBuilder.cs`: 84
- `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyReader.cs`: 274
- `UtilitiesCS/OutlookObjects/Folder/OutlookFolderTreeService.cs`: 497
- `UtilitiesCS/Threading/IUiDispatcher.cs`: 43
- `UtilitiesCS/Threading/WpfUiDispatcher.cs`: 63
