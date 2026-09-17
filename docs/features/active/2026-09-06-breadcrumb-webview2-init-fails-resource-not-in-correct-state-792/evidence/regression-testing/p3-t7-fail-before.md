# [P3-T7] [expect-fail] Whole-set regression tests observed failing on the unfixed tree

- Issue: #792
- Timestamp: 2026-09-17T19-47
- Command: CMD-OUTLOOK (`Get-Process -Name OUTLOOK`, printed `OUTLOOK-CLOSED: true`), then CMD-BUILD-PLAIN (`msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`, exit 0, `0 Warning(s)`, exact line `0 Error(s)`, `Build succeeded.`, 6 s incremental; the log contains 19 `CoreCompile:` target lines and 2 `csc.exe` lines both naming `QuickFiler.Test`, and `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll` was rewritten at 19:47:43, so the six Phase 3 files compiled and the build was not vacuous), then CMD-VSTEST (vswhere resolved `vstest.console.exe`), then `& $vstest QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~Issue792|FullyQualifiedName~WebView2EnvironmentContractTests" "/ResultsDirectory:coverage/test-results/p3-t7" "/Logger:trx;LogFileName=p3-t7.trx"` (CMD-SCOPED-RUN with `<task>` = `p3-t7`; run from a helper script with the item worktree as the working directory; console captured to the gitignored `coverage/p3-t7-scoped.log`; build console to the gitignored `coverage/p3-t7-build.log`; TRX under the gitignored `coverage/test-results/p3-t7/`)
- EXIT_CODE: 1
- ExpectedExitCode: 1
- Output Summary: `Test Run Failed.`; `Total tests: 31`; `Passed: 13`; `Failed: 18`; `Skipped: 0 (omitted category)`; `Total time: 1.6525 Seconds`. The partition matches the plan's declaration exactly (31 / 13 / 18; the eighteen declared tests fail, the thirteen declared controls pass), and each failing test failed on its pre-predicted assertion. The filter discovered exactly thirty-one tests, so the run is not vacuous.

## Observed partition

Declared by the plan: `Total tests: 31` (2 + 2 + 1 from Phase 1, 3 + 4 + 3 + 5 + 6 + 5 from Phase 3), `Passed: 13`, `Failed: 18`, with both sets enumerated by name in [P3-T7].

Observed: `Total tests: 31`, `Passed: 13`, `Failed: 18`. The TRX contains 31 `UnitTestResult` rows: 18 `Failed`, 13 `Passed`.

PARTITION-MATCHES-DECLARATION: true

## Fail-before evidence (first assertion message line per failed test; fully qualified from the TRX)

FAIL-BEFORE: QuickFiler.Test.Viewers.WebView2BreadcrumbHostIssue792Tests.InitializeAsync_PassesTheSharedFolderAndIncognitoArgumentToTheSeam | Expected capturedOptions.AdditionalBrowserArguments to be "--incognito " because every WebView2 site must share the same incognito browser argument, but found <null>.

FAIL-BEFORE: QuickFiler.Test.Viewers.WebView2BreadcrumbHostIssue792Tests.NavigateToString_BeforeCoreInitialization_WithNoDispatcher_DropsTheDocumentWithoutThrowing | Did not expect any exception because a document navigated before core initialization must be dropped, not forwarded to a control with no core, but found System.InvalidOperationException: The instance of CoreWebView2 is uninitialized and unable to complete this operation. See EnsureCoreWebView2Async.

FAIL-BEFORE: QuickFiler.Controllers.Tests.EfcFormControllerIssue792Tests.InitializeBreadcrumbHostAsync_WhenHostIsNull_ReportsThroughTheBoundarySinkToTheUser | Expected captured to contain a single item because the final initialization failure must be reported to the user exactly once, but the collection is empty.

FAIL-BEFORE: QuickFiler.Test.HelperClasses.EfcViewerQueueIssue792Tests.ProductionBlockingPriorityScheduler_DefaultIsTheNamedUiDispatcherInvoke | Expected scheduler.Method.Name to be a match with the expectation because the blocking scheduler must be the named UI-dispatcher invoke, not a lambda, but it differs at index 0: (actual) "<.cctor>b__26_2" / (expected) "InvokeOnUiDispatcher"

FAIL-BEFORE: QuickFiler.Test.Controllers.BreadcrumbBridgeRouterIssue792Tests.NotifyInitializationFailed_ClearsThePendingDocumentAndNavigatesTheErrorBanner | Expected _navigated to contain a single item because the failure must navigate exactly one document, the error banner, but the collection is empty.

FAIL-BEFORE: QuickFiler.Test.Controllers.BreadcrumbBridgeRouterIssue792Tests.NotifyInitializationFailed_LeavesNoStashForALaterInitialization | Expected _navigated[0] "<!DOCTYPE html><html>... (5,000-character rendered folder document, elided) ..." to contain "Folder list unavailable" because the single navigation must be the error banner.

FAIL-BEFORE: QuickFiler.Test.Controllers.BreadcrumbOutboundQueueIssue792Tests.DiscardPending_ReturnsTheDiscardedCountAndLeavesZeroPending | Expected discarded to be 3 because the discarded count must equal the number buffered, but found 0 (difference of -3).

FAIL-BEFORE: QuickFiler.Test.Controllers.BreadcrumbOutboundQueueIssue792Tests.NotifyInitializationFailed_DiscardsTheOutboundQueueWithoutPosting | Expected queue.PendingCount to be 0 because a failed initialization must discard the buffered payloads, but found 2 (difference of 2).

FAIL-BEFORE: QuickFiler.Controllers.Tests.EfcFormControllerIssue792Tests.InitializeBreadcrumbHostAsync_RetriesUpToTheAttemptLimitThenReportsOnce | Expected initializer.Invocations to be 3 because the host initializer must be attempted exactly the limit of three times, but found 0 (difference of -3).

FAIL-BEFORE: QuickFiler.Controllers.Tests.EfcFormControllerIssue792Tests.InitializeBreadcrumbHostAsync_SucceedsOnALaterAttempt_ReportsNothing | Expected initializer.Invocations to be 2 because the loop must stop on the first successful attempt, but found 0 (difference of -2).

FAIL-BEFORE: QuickFiler.Controllers.Tests.EfcFormControllerIssue792Tests.InitializeBreadcrumbHostAsync_WhenCanceled_DoesNotRetryOrReport | Expected initializer.Invocations to be 1 because a canceled attempt must not be retried, but found 0 (difference of -1).

FAIL-BEFORE: QuickFiler.Controllers.Tests.EfcFormControllerIssue792Tests.InitializeBreadcrumbHostAsync_OnFinalFailure_ShowsTheErrorTextInTheFolderAreaLabel | Expected label.Text to be "Matched Folders: unavailable (breadcrumb initialization failed)" with a length of 63 because the folder-area label is the visible carrier of the final failure, but "" has a length of 0, differs near "" (index 0).

FAIL-BEFORE: QuickFiler.Controllers.Tests.EfcFormControllerIssue792Tests.InitializeBreadcrumbHostAsync_OnFinalFailure_NotifiesTheRouter | Expected navigated to contain a single item because the router must navigate exactly one document on failure, but the collection is empty.

FAIL-BEFORE: QuickFiler.Controllers.Tests.EfcDataModelIssue792CarryTests.TryAdoptCarriedFolderHandler_WithNullListAndConcretePredictor_Adopts | Expected adopts to be True because a concrete predictor with no explicit list must be adopted, but found False.

FAIL-BEFORE: QuickFiler.Controllers.Tests.EfcDataModelIssue792CarryTests.InitFolderHandlerAsync_WithCarriedPredictor_AdoptsItAndReleasesTheCarry | Test method QuickFiler.Controllers.Tests.EfcDataModelIssue792CarryTests.InitFolderHandlerAsync_WithCarriedPredictor_AdoptsItAndReleasesTheCarry threw exception: System.NullReferenceException: Object reference not set to an instance of an object. (thrown from `UtilitiesCS.FolderPredictor..ctor(IApplicationGlobals AppGlobals)` inside the `InitFolderHandlerAsync` closure)

FAIL-BEFORE: QuickFiler.Controllers.Tests.EfcDataModelIssue792CarryTests.InitFolderHandlerAsync_WithNonPredictorCarry_RunsTheExistingPathAndReleasesTheCarry | Expected model.CarriedFolderHandler to be <null> because the carry must be released after the existing path has run, but found Mock<IFolderSearchHandler:4>.Object.

FAIL-BEFORE: QuickFiler.Controllers.Tests.QfcCollectionControllerIssue792PopOutTests.ReadPopOutCarry_WithConcreteItemController_ReturnsHandlerAndHelper | Expected carry.FolderHandler to refer to Mock<IFolderSearchHandler:1>.Object because the concrete controller's folder handler must be carried, but found <null>.

FAIL-BEFORE: QuickFiler.Controllers.Tests.QfcCollectionControllerIssue792PopOutTests.ReadPopOutCarry_WithInterfaceOnlyController_ReturnsNullHandlerAndTheHelper | Expected carry.MailHelper to refer to Mock<MailItemHelper:2>.Object because the interface helper must be carried, but found <null>.

## Pass-before controls (one line per passing test; fully qualified from the TRX)

PASS-BEFORE-CONTROL: QuickFiler.Controllers.Tests.EfcFormControllerIssue792Tests.PopulateFolderCombobox_WhenDataModelFaults_NotifiesTheUserThroughTheDefaultSink

PASS-BEFORE-CONTROL: QuickFiler.Test.Viewers.WebView2EnvironmentContractTests.AdditionalBrowserArguments_IsAsciiDoubleHyphenIncognitoWithTrailingSpace

PASS-BEFORE-CONTROL: QuickFiler.Test.Viewers.WebView2EnvironmentContractTests.ResolveUserDataFolder_CombinesLocalApplicationDataWithTheSharedLeafName

PASS-BEFORE-CONTROL: QuickFiler.Test.Viewers.WebView2EnvironmentContractTests.CreateOptions_CarriesTheSharedArgumentsOnAFreshInstance

PASS-BEFORE-CONTROL: QuickFiler.Test.Controllers.BreadcrumbBridgeRouterIssue792Tests.NotifyInitializationFailed_WithNullFailure_Throws

PASS-BEFORE-CONTROL: QuickFiler.Test.Controllers.BreadcrumbBridgeRouterIssue792Tests.NotifyCoreInitialized_AfterAnEarlierStash_StillNavigatesIt

PASS-BEFORE-CONTROL: QuickFiler.Test.Controllers.BreadcrumbOutboundQueueIssue792Tests.DiscardPending_OnAnEmptyQueue_ReturnsZero

PASS-BEFORE-CONTROL: QuickFiler.Controllers.Tests.EfcDataModelIssue792CarryTests.TryAdoptCarriedFolderHandler_WithNullCarry_DoesNotAdopt

PASS-BEFORE-CONTROL: QuickFiler.Controllers.Tests.EfcDataModelIssue792CarryTests.TryAdoptCarriedFolderHandler_WithNonPredictorHandler_DoesNotAdopt

PASS-BEFORE-CONTROL: QuickFiler.Controllers.Tests.EfcDataModelIssue792CarryTests.TryAdoptCarriedFolderHandler_WithExplicitListAndPredictor_DoesNotAdopt

PASS-BEFORE-CONTROL: QuickFiler.Controllers.Tests.QfcCollectionControllerIssue792PopOutTests.ReadPopOutCarry_WithNullController_ReturnsNulls

PASS-BEFORE-CONTROL: QuickFiler.Controllers.Tests.QfcCollectionControllerIssue792PopOutTests.PopOutHomeControllerFactory_DefaultIsTheNamedProductionFactory

PASS-BEFORE-CONTROL: QuickFiler.Controllers.Tests.QfcCollectionControllerIssue792PopOutTests.EfcHomeController_DepositsTheCarryOnTheDataModelBeforeConstructingTheFormController

## Correspondence to the plan's predictions

- Phase 1 tests (four failures, one control): identical outcomes and identical first assertions to [P1-T4], with one cosmetic difference. The scheduler test's actual lambda name is now `<.cctor>b__26_2` where [P1-T4] recorded `<.cctor>b__25_2`: the [P2-T12] declaration of `InvokeOnUiDispatcher` shifted the compiler-generated ordinal of the unchanged inline lambda. The default is still a lambda, so the assertion still discriminates.
- [P3-T2] `NotifyInitializationFailed_ClearsThePendingDocumentAndNavigatesTheErrorBanner`: the plan predicts the `_navigated` assertion fails first, before either selection assertion is reached. Observed: `Expected _navigated to contain a single item ... but the collection is empty.` Matches.
- [P3-T2] `NotifyInitializationFailed_LeavesNoStashForALaterInitialization`: the plan predicts the only navigation is the stale stash, which contains `Alpha`, so the count assertion passes and the content assertion fails. Observed: the `HaveCount(1)` assertion passed; the failing clause is `to contain "Folder list unavailable"` against an actual value that is the rendered folder document and contains `Alpha`. Matches, and it is the count-only trap the plan warns about: a count-only form would have passed on the unfixed tree.
- [P3-T3] `DiscardPending_ReturnsTheDiscardedCountAndLeavesZeroPending`: the stub returns 0 for three buffered payloads. Matches. `NotifyInitializationFailed_DiscardsTheOutboundQueueWithoutPosting`: two payloads remain pending. Matches.
- [P3-T4] five tests: the plan predicts the seam is not consulted before the fix. Observed invocation counts of 0 against expected 3, 2 and 1; the label text is empty; the router navigated nothing. Matches on all five.
- [P3-T5] `TryAdoptCarriedFolderHandler_WithNullListAndConcretePredictor_Adopts`: the stub returns false. Matches. `InitFolderHandlerAsync_WithCarriedPredictor_AdoptsItAndReleasesTheCarry`: the plan allows either an assertion failure or the exception the unguarded construction path raises against null `Globals`; observed `NullReferenceException` from `FolderPredictor..ctor(IApplicationGlobals)` inside the `InitFolderHandlerAsync` closure, which is the second allowed outcome and is recorded as the fail-before. `InitFolderHandlerAsync_WithNonPredictorCarry_RunsTheExistingPathAndReleasesTheCarry`: the plan predicts the existing null-list path runs (a predictor is constructed over the mocked globals) and the carry is never released, so the release assertion fails. Observed: the `FolderHelper` not-null and not-same-as assertions passed and the release assertion failed with the carried mock still present. Matches; the carry is *not consulted* rather than declined, as the plan states.
- [P3-T6] `ReadPopOutCarry_WithConcreteItemController_ReturnsHandlerAndHelper` and `ReadPopOutCarry_WithInterfaceOnlyController_ReturnsNullHandlerAndTheHelper`: the stub returns a null pair. Matches (the first fails on the handler, the second on the helper, each being that test's first discriminating assertion).
- Thirteen controls: all passed, as declared.

## Observed-failing map entries satisfied by this run

- AC-U1: retry, label and router tests observed failing (`InitializeBreadcrumbHostAsync_*` five tests, `NotifyInitializationFailed_ClearsThePendingDocumentAndNavigatesTheErrorBanner`).
- AC-U2: pending-cleared tests observed failing (`NotifyInitializationFailed_LeavesNoStashForALaterInitialization`, `NotifyInitializationFailed_ClearsThePendingDocumentAndNavigatesTheErrorBanner`).
- AC-U3: adoption and carry-read tests observed failing (`TryAdoptCarriedFolderHandler_WithNullListAndConcretePredictor_Adopts`, both `InitFolderHandlerAsync_*` tests, both `ReadPopOutCarry_With*Controller_*` tests); the UI-thread half was observed failing at [P1-T4] and again here.
- AC-U7: queue tests observed failing (`DiscardPending_ReturnsTheDiscardedCountAndLeavesZeroPending`, `NotifyInitializationFailed_DiscardsTheOutboundQueueWithoutPosting`).

## Files created or edited in Phase 3 (line counts by `(Get-Content -LiteralPath <path>).Count`; every file UTF-8 without BOM, CRLF)

- `QuickFiler.Test/Viewers/WebView2EnvironmentContractTests.cs`: 108 (ceiling 120)
- `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue792Tests.cs`: 196 (ceiling 200; the first formatted draft measured 215 and was shortened by trimming doc comments and `because` strings only)
- `QuickFiler.Test/Controllers/BreadcrumbOutboundQueueIssue792Tests.cs`: 113 (ceiling 150)
- `QuickFiler.Test/Controllers/EfcFormControllerIssue792Tests.cs`: 348 (ceiling 470; was 148 after Phase 1); contains no `DoNotParallelize` token and no `Thread.Sleep` or `Task.Delay` token (positive controls: the same searches hit `ViewerQueueStaticWrapperTests.cs` once and six lines elsewhere under `QuickFiler.Test`)
- `QuickFiler.Test/Controllers/EfcDataModelIssue792CarryTests.cs`: 175 (ceiling 230)
- `QuickFiler.Test/Controllers/QfcCollectionControllerIssue792PopOutTests.cs`: 213 (ceiling 260)
- `QuickFiler.Test/QuickFiler.Test.csproj`: five bare self-closing `<Compile Include>` items added (numstat `5 0` against HEAD `11f5aa598`), at lines 60 and 61 (after `BreadcrumbBridgeRouterQueueTests.Part2.cs`, line 59), 128 (after `EfcDataModelArchiveRootTests.cs`, line 127), 172 (after `QfcCollectionControllerTests.Part2.cs`, line 171) and 225 (after `WebView2BreadcrumbHostIssue792Tests.cs`, line 224); the plan's anchor citations 125 and 167 were authored before the Phase 1 and Phase 3 insertions above them and resolve to the same named items.

All six `.cs` files were passed through `dotnet tool run csharpier format` (pinned 1.2.6) before the run; a second pass rewrote nothing.

## Determinism note

`InitializeBreadcrumbHostAsync_OnFinalFailure_ShowsTheErrorTextInTheFolderAreaLabel` constructs a real `System.Windows.Forms.Label` (required by the plan), which installs `WindowsFormsSynchronizationContext` on the test thread; the test clears that context immediately after construction so no continuation can be posted to an unpumped thread. No sleep, no pump, no `[DoNotParallelize]`; `scripts/vscode/TaskMaster.cli.runsettings` is unchanged.

No production `.cs` file, no other `.csproj`, and no `.runsettings` file differs from HEAD after Phase 3.
