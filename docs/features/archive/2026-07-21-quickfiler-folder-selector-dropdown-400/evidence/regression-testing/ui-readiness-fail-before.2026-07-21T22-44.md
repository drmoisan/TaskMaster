# UI Dispatch and Collapsed Readiness Fail-Before

Timestamp: 2026-07-21T22-44Z
Command: `$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'; $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; if (-not $vstest -or -not (Test-Path -LiteralPath $vstest)) { throw 'vstest.console.exe not found via vswhere.' }; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbUiThreadDispatchTests|FullyQualifiedName~BreadcrumbCollapsedSurfaceReadinessTests|FullyQualifiedName~QfcItemControllerBreadcrumbDropDownTests" /Logger:"console;Verbosity=normal"`
EXIT_CODE: 1
Output Summary: Expected-failure gate accepted. Thirteen filtered tests were discovered in `QuickFiler.Test`; four existing controller controls passed and nine new failure-first tests failed. Distinct named assertions proved both required defects: worker-originated breadcrumb delivery bypassed the captured UI synchronization boundary, and the collapsed surface lacked a correlated readiness/controller lifetime before cached render, selector, and theme attachment/replay. No build, discovery, tool-resolution, environmental, or unrelated failure occurred.

## Resolution and totals

- Resolved VSTest: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.
- Assemblies matched: 1.
- Total tests: 13.
- Passed: 4.
- Failed: 9.
- Skipped: 0.
- Test time: 2.4526 seconds.

## UI-dispatch expected failures

1. `BreadcrumbUiThreadDispatchTests.SetSuggestionsAsync_WorkerProviderCompletion_SchedulesPostOnOwningContext` — failed because the captured context recorded 0 posts after worker-thread provider completion.
2. `BreadcrumbUiThreadDispatchTests.InboundWorkerMessage_SchedulesEveryPostAndCallbackOnOwningContext` — failed because the captured context recorded 0 posts for worker-originated render posts and `SelectionChanged` callback delivery.
3. `BreadcrumbUiThreadDispatchTests.DispatcherSchedulingFailure_IsReportedThroughObservableErrorSink` — failed because the host-neutral `BreadcrumbUiDispatcher` and observable error-sink boundary do not exist.

These named failures independently establish that current asynchronous delivery bypasses the required UI dispatcher and provides no focused observable scheduling-failure path.

## Correlated collapsed-readiness expected failures

1. `BreadcrumbCollapsedSurfaceReadinessTests.AttachAsync_PendingAndUnrelatedNavigation_DefersCachedReplayUntilExactSuccess` — failed because no correlated collapsed-surface controller exists to keep cached replay pending through unrelated navigation.
2. `BreadcrumbCollapsedSurfaceReadinessTests.AttachAsync_ExactNavigationFailure_LeavesNoAttachmentOrReplay` — failed at the same missing correlated controller boundary.
3. `BreadcrumbCollapsedSurfaceReadinessTests.Reset_PendingNavigation_CancelsAttachmentAndRejectsLateSuccess` — failed because no resettable pending collapsed-navigation lifetime exists.
4. `BreadcrumbCollapsedSurfaceReadinessTests.Dispose_PendingNavigation_CancelsAttachmentAndRejectsLateSuccess` — failed because no disposable pending collapsed-navigation lifetime exists.
5. `BreadcrumbCollapsedSurfaceReadinessTests.LaterNavigation_InvalidatesEarlierGenerationAndAttachesOnlyCurrentSurface` — failed because no generation-owned collapsed readiness controller exists.
6. `QfcItemControllerBreadcrumbDropDownTests.CollapsedAttachmentContract_IsAwaitableAndControllerOwnedForControllerSetup` — failed because `ItemViewer` has neither the planned controller field nor an awaitable collapsed attachment boundary.

These named failures establish the missing boundary required to prevent cached render, selector, and theme replay before the exact target document is ready.

## Existing controls that passed

1. `QfcItemControllerBreadcrumbDropDownTests.ConfigureBreadcrumbDropDown_PassesExistingEnvironmentAndDarkThemeLazily`.
2. `QfcItemControllerBreadcrumbDropDownTests.ConfigureBreadcrumbDropDown_LightThemeUsesSameControllerSetupSeam`.
3. `QfcItemControllerBreadcrumbDropDownTests.Cleanup_ResetsInjectedHostForPooledViewerReuse`.
4. `QfcItemControllerBreadcrumbDropDownTests.OnBreadcrumbUnhandledArrow_ForViewer_RoutesOnceToKeyboardHandler`.

## Current early-attachment call-chain inspection

Inspection: `Select-String` and numbered source inspection for `NavigateToString`, `WebView2Messenger`, hub attachment, and the controller setup call.
Inspection EXIT_CODE: 0

- `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:69` calls `NavigateToString(Properties.Resources.FolderBreadcrumb)`.
- `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:70` immediately constructs and attaches `WebView2Messenger`; no correlated `NavigationId` completion is awaited.
- `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:88` attaches the collapsed messenger to the hub.
- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:112` invokes the synchronous `AttachBreadcrumbWebView()` method.

The source call chain confirms that the missing controller assertion corresponds to the current early collapsed attachment/replay behavior rather than an unrelated structural expectation.
