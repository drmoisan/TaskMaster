# Phase 6 — Final QA loop, step 4: whole-assembly SERIAL-regime test gate (P6-T5)

Task: [P6-T5]
Toolchain pass: 1

Timestamp: 2026-09-13T03-48
Command: `pwsh -Command '& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/Logger:trx;LogFileName=p6-t5-final-serial.trx" /ResultsDirectory:coverage\trx\p6-t5 "/TestCaseFilter:TestCategory!=LiveOutlook"'` Run from the item worktree root via Set-Location inside one pwsh invocation, with the Command Reference tool resolution prepended (inner quoting inverted to single quotes; semantics identical); console output redirected to the ignored path `coverage\p6-t5-vstest.log`. Run while holding the shared machine build lock for item 743 (acquired 03:47:54, released 03:48:19 immediately after the command returned). Outlook was closed; no induced load. The assembly executed is the one produced by the P6-T4 nullable Rebuild of this pass.
EXIT_CODE: 0
Output Summary:
- `Test Run Successful.` / `Total tests: 1400` / `Passed: 1400` / `Total time: 11.8342 Seconds`
- Newest `.trx` under `coverage\trx\p6-t5` sorted by `LastWriteTime`: `p6-t5-final-serial.trx` (the only file).
- Transcribed `ResultSummary/Counters`: `total=1400`, `passed=1400`, `failed=0`, `timeout=0` (`executed=1400`, `notExecuted=0`, outcome `Completed`).
- Acceptance arithmetic: P0-T10 recorded `total=1394`; 1394 + 6 = 1400; the recorded total 1400 is at least 1400. The six added tests are the five seam tests (P2-T6) and the one balance test (P1-T7).
- REGIME: SERIAL (no /Settings: argument; identical in this respect to the CI command at line 99 of the MSTest coverage workflow).
- Observation (not a gate of this task): the balance test emitted `GATECOUNTERS acquisitions=13 releases=12 contended=1`. The difference is exactly 1 while the balance test holds the permit. The single contended acquisition is the designed live-holder contention of `Transaction_SecondCallerCannotInstallUntilTheFirstRestores`, which this task's filter does not exclude (the AC1 serial measurement in P1-T9 and P4-T3 excludes it by design, per P0-T11 item (d)); it is not an AC1 measurement and does not bear on the P1-T11 verdict.
- The known-intermittent class `QfcInitEmailQueueZeroBatchTests` (P4-T3 parallel regime) passed in this serial run; no re-run was needed.

## Outcome of every test named in the spec section 7 disposition table

### 7.1 — MOVED to the seam: `QuickFiler.Test/Controllers/QfcItemController.SeamMarshallingTests.cs` (class `QfcItemController_SeamMarshallingTests`, 5 tests)

| Test | Outcome | duration |
|---|---|---|
| ResolveControlGroupsAsync_WithMockViewerAndSyncDispatcher_CompletesWithoutAConcreteViewer | Passed | 00:00:00.0128786 |
| ResolveControlGroupsAsync_WithMockViewer_PopulatesTipsAndControlGroups | Passed | 00:00:00.0040019 |
| ResolveControlGroupsAsync_FirstParameterType_IsTheViewerInterface | Passed | 00:00:00.0004783 |
| ResolveControlGroupsAsync_WithCancelledToken_ThrowsOperationCanceled | Passed | 00:00:00.0012288 |
| AssignControlsAsync_WithSyncDispatcherDouble_AssignsThroughTheInjectedSeam | Passed | 00:00:00.0057189 |

### 7.1 — MOVED to the seam: the deterministic mechanism regression test (Branch COST placed it in the fixture test file per P1-T7)

| Test | Outcome | duration |
|---|---|---|
| TransactionGate_WhileThisTestHoldsATransaction_HasExactlyOneUnreleasedAcquisition | Passed | 00:00:00.0010231 |

### 7.1 — RETAINED pump-hosted, edited additively: `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` (six existing gate tests)

| Test | Outcome | duration |
|---|---|---|
| EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt | Passed | 00:00:00.0049070 |
| EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose | Passed | 00:00:00.0006939 |
| EnsureDispatcher_ScopeDisposedTwice_IsIdempotent | Passed | 00:00:00.0007974 |
| Transaction_SecondCallerCannotInstallUntilTheFirstRestores | Passed | 00:00:00.0041398 |
| Transaction_DisposedTwice_DoesNotOverReleaseTheGate | Passed | 00:00:00.0028355 |
| Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException | Passed | 00:00:00.0028304 |

### 7.2 — Retained pump-hosted and unchanged: initialization tests in the Part3 file (class `QfcItemController_InitializationTests`, all tests in the class transcribed; the five `ThroughThePumpHost` tests plus the pump-harness tests are the pump-hosted ones)

| Test | Outcome | duration |
|---|---|---|
| InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState | Passed | 00:00:00.1114210 |
| InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme | Passed | 00:00:00.0807574 |
| InitializeBool_ThroughThePumpHost_CompletesAndInitializesState | Passed | 00:00:00.0822165 |
| InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates | Passed | 00:00:00.0850347 |
| InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults | Passed | 00:00:00.1099004 |
| InitializeWebViewGuardedAsync_WhenTheWebViewSeamFaults_ReportsToTheSinkAndDoesNotFault | Passed | 00:00:00.0011805 |
| WebViewInitializationErrorSink_DefaultDelegate_InvokesWithoutThrowing | Passed | 00:00:00.0002911 |
| InitializeBool_WhenTheWebViewSeamFaults_ObservesTheFaultThroughTheSink | Passed | 00:00:00.0804366 |
| BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread | Passed | 00:00:00.0651496 |
| BuildPumpHarness_DoesNotCreateTheWebViewChildHandles | Passed | 00:00:00.0717996 |
| InitializeWebViewGuardedAsync_WhenTheTokenIsAlreadyCanceled_DoesNotInvokeTheSink | Passed | 00:00:00.0033140 |
| PrimaryConstructor_AssignsFieldsAndSetsControllerBackReference | Passed | 00:00:00.0008887 |
| AsyncFlagConstructor_AssignsFieldsViaSaveParameters | Passed | 00:00:00.0004086 |
| SaveParameters_AssignsAllFieldsAndResolvesCollaborators | Passed | 00:00:00.0005500 |
| PredeterminedFolderConstructor_StoresPredeterminedFolder | Passed | 00:00:00.0005421 |

### 7.2 — Retained pump-hosted and unchanged: the two pump-hosted seam-factory tests (class `QfcItemController_SeamFactoryTests`; the whole class is transcribed, the two pump-hosted ones are the `Create*` tests)

| Test | Outcome | duration |
|---|---|---|
| CreateSequentialAsync_WithInjectedSeams_ReturnsAnInitializedController | Passed | 00:00:00.0770007 |
| CreateAsync_WithFaultingWebViewSeam_FaultsWithThatExceptionAfterInitializing | Passed | 00:00:00.0892169 |
| PopulateConversation_UsesResolverFactoryAndRendersCount | Passed | 00:00:00.0007870 |
| FlagAsTask_InvokesFactoryWithExpectedArguments | Passed | 00:00:00.0010673 |
| FlagAsTaskAsync_InvokesFactoryThroughDispatcher | Passed | 00:00:00.0014232 |
| MoveMailAsync_WhenItemHelperNull_DoesNotInvokeFactory | Passed | 00:00:00.0004006 |
| MoveMailAsync_WhenOneDriveMissing_ReturnsWithoutInvokingFactory | Passed | 00:00:00.0006212 |
| MoveMailAsync_WhenOneDrivePresent_InvokesFactoryWithConfigAndEnqueues | Passed | 00:00:00.0017812 |
| WireIntentEvents_SubscribesEveryIntentEvent | Passed | 00:00:00.0030040 |

### 7.2 — Retained pump-hosted and unchanged: the one pump-hosted test in the ViewerSetup test file (class `QfcItemController_ViewerSetupTests`; whole class transcribed)

| Test | Outcome | duration |
|---|---|---|
| ResolveControlGroupsAsync_ThroughThePumpHost_PopulatesTipsAndControlGroups | Passed | 00:00:00.0652283 |
| AssignControlsAsync_DispatchesAssignThroughViewerDispatcher | Passed | 00:00:00.0012583 |
| AssignControls_WhenInvokeRequired_MarshalsViaInvoke | Passed | 00:00:00.0005913 |
| AssignControls_WhenNotInvokeRequired_WritesAllIntentMembersFromSettings | Passed | 00:00:00.0014411 |
| AssignControls_WhenTaskFlagUnset_SetsCancelDialogResult | Passed | 00:00:00.0003723 |
| Cleanup_NullsMailActions_AndSaveParametersRebindsIt | Passed | 00:00:00.0015992 |
| Cleanup_NullsTrackedPrivateFields | Passed | 00:00:00.0002977 |
| PopulateControls_WithHelper_StoresHelperAndAssignsViewerFields | Passed | 00:00:00.0016483 |
| PopulateControls_WithMailItem_ConstructsHelperAndAssignsControls | Passed | 00:00:00.0032958 |
| PopulateControlsAsync_WithMailItem_LoadsHelperViaFromMailItemAsyncAndAssignsControls | Passed | 00:00:00.0021822 |
| ResolveControlGroups_WithHeadlessItemViewer_PopulatesConcreteControlCollections | Passed | 00:00:00.0559813 |

### 7.2 — Retained pump-hosted and unchanged: the eight breadcrumb-host tests (class `WebView2BreadcrumbHostTests`, under the Viewers folder)

| Test | Outcome | duration |
|---|---|---|
| PostMessageJson_PostsExactlyOnceToTheUiContext | Passed | 00:00:00.0068687 |
| NavigateToString_PostsExactlyOnceToTheUiContext | Passed | 00:00:00.0040991 |
| SecondHost_DetachesThePredecessorAndTakesOwnership | Passed | 00:00:00.0035504 |
| PredecessorDetach_ToleratesNullCoreWebView2 | Passed | 00:00:00.0032852 |
| ControlDisposed_DetachesTheHost | Passed | 00:00:00.0029858 |
| InitializeAsync_InstallsUiDispatcherFromUiSyncContext | Passed | 00:00:00.0077193 |
| InitializeAsync_PreservesAnInjectedDispatcher | Passed | 00:00:00.0040758 |
| PostMessageJson_WithNoDispatcher_ExecutesInlineAndDropsThePayload | Passed | 00:00:00.0041149 |

### 7.2 — Retained: the reflection contract tests (class `ItemViewerBreadcrumbDropDownContractTests`, 14 tests)

| Test | Outcome | duration |
|---|---|---|
| ExistingAnchor_RemainsTheDesignerWebViewClosedSurface | Passed | 00:00:00.0001685 |
| ProductionConfiguration_AcceptsExistingEnvironmentAndInitializer | Passed | 00:00:00.0001951 |
| InjectedConfiguration_AcceptsHostAndScreenGeometryProviders | Passed | 00:00:00.0001116 |
| ExistingFolderEventsAndDropDownIntentSignatures_AreUnchanged | Passed | 00:00:00.0002999 |
| HostNeutralPopupOpenOrchestration_IsOwnedByInstrumentedCoordinator | Passed | 00:00:00.0003191 |
| ItemViewer_DeclaresNoMenuItemCheckedChangedMembers | Passed | 00:00:00.0002533 |
| ItemViewer_DeclaresNoMoveOptionsMenuClickHandler | Passed | 00:00:00.0001565 |
| ItemViewer_DeclaresNoParentChangedHandler | Passed | 00:00:00.0004193 |
| ItemViewerExpanded_DeclaresNoParentChangedHandler | Passed | 00:00:00.0002316 |
| IItemViewer_DeclaresNoUiSchedulerMember | Passed | 00:00:00.0001396 |
| IItemViewer_StillDeclaresUiDispatcher | Passed | 00:00:00.0001652 |
| IItemViewer_StillDeclaresUiSyncContext | Passed | 00:00:00.0001878 |
| IItemViewer_DeclaresAddFolderItemsAndNotSetFolderItems | Passed | 00:00:00.0001878 |
| IItemViewer_FocusSubjectReturnsBool | Passed | 00:00:00.0000840 |

Every test named in the section 7 disposition table exists in the post-change tree and passed. The largest pump-test duration in this run is 111.4210 ms (`InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState`).

## Results directory cleanup

Command: `pwsh -Command 'Test-Path coverage\trx\p6-t5'` (after deleting the directory through the .NET `Directory.Delete` API)
EXIT_CODE: 0
Output Summary: `False`. The raw TRX was discarded after transcription (D1).
