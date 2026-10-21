# [P4-T11] Green gate: analyzer build, nullable build, and the pass-after scoped run

- Issue: #792
- Timestamp: 2026-09-17T20-10
- Command: `dotnet tool run csharpier check .` (formatter gate, read-only); CMD-OUTLOOK (`Get-Process -Name OUTLOOK`, printed `OUTLOOK-CLOSED: true` before each build); CMD-BUILD-ANALYZE (`msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`); CMD-BUILD-NULLABLE (`msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`); CMD-VSTEST (vswhere resolved `vstest.console.exe`); CMD-SCOPED-RUN `& $vstest QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~Issue792|FullyQualifiedName~WebView2EnvironmentContractTests|FullyQualifiedName~EfcFormControllerTests|FullyQualifiedName~EfcItemControllerTests|FullyQualifiedName~EfcDataModel|FullyQualifiedName~QfcCollectionControllerTests|FullyQualifiedName~ViewerQueueStaticWrapperTests|FullyQualifiedName~BreadcrumbBridgeRouterQueueTests|FullyQualifiedName~WebView2BreadcrumbHostTests|FullyQualifiedName~EfcHomeController|FullyQualifiedName~QfcItemController_InitializationTests" "/ResultsDirectory:coverage/test-results/p4-t11" "/Logger:trx;LogFileName=p4-t11.trx"` (`<task>` = `p4-t11`); all run from `coverage/plan792-helper.ps1` with the item worktree as the working directory; consoles captured to the gitignored `coverage/p4-t11-csharpier-check.log`, `coverage/p4-t11-analyze.log`, `coverage/p4-t11-nullable.log`, `coverage/p4-t11-scoped.log`; TRX under the gitignored `coverage/test-results/p4-t11/`
- EXIT_CODE: 0
- Output Summary: formatter `Checked 1658 files`, exit 0; analyzer Rebuild exit 0, `0 Warning(s)`, exact line `0 Error(s)`, `Build succeeded.`; nullable Rebuild exit 0, `0 Warning(s)`, exact line `0 Error(s)`, `Build succeeded.`; scoped run `Test Run Successful.`, `Total tests: 234`, `Passed: 234`, `Failed: 0 (omitted category)`, `Skipped: 0 (omitted category)`, `Total time: 2.9408 Seconds`, exit 0; all 32 Issue792/contract tests passed (31 from [P3-T7] plus the [P4-T4] test), so the 18 tests recorded failing in [P3-T7] now pass and the 13 controls still pass.

## Formatter gate

`dotnet tool run csharpier check .` printed `Checked 1658 files in 4681ms.` and exited 0. Each Phase 4 file had been passed through `dotnet tool run csharpier format <path>` (pinned 1.2.6) as it was edited; the whole-tree check confirms no drift anywhere.

## CMD-BUILD-ANALYZE (non-vacuity)

Exit 0; `0 Warning(s)`; `0 Error(s)`; `Build succeeded.`; 63 `CoreCompile:` target lines (unanchored count, per the `/m` node-prefix note), 36 `csc.exe` lines of which 3 name the `QuickFiler` production project and 2 name `QuickFiler.Test`, 17 `csc.exe` lines carry `/analyzer:` switches; `QuickFiler/bin/Debug/QuickFiler.dll` rewritten 20:04:57 to 20:09:17 and `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll` rewritten 20:04:58 to 20:09:20; no `error`/`warning` diagnostic line in the log; elapsed 15 s.

## CMD-BUILD-NULLABLE (non-vacuity)

Exit 0; `0 Warning(s)`; `0 Error(s)`; `Build succeeded.`; 66 `CoreCompile:` target lines, 36 `csc.exe` lines (3 `QuickFiler`, 2 `QuickFiler.Test`), 18 `csc.exe` lines carry `/warnaserror+`; `QuickFiler.dll` rewritten 20:09:17 to 20:09:54 and `QuickFiler.Test.dll` 20:09:20 to 20:09:57; no diagnostic line in the log; elapsed 15 s. No `/p:Nullable=enable` was passed.

## Scoped run

Console: `Test Run Successful.`; `Total tests: 234`; `Passed: 234`; no `Failed:` line and no `Skipped:` line (transcribed as `Failed: 0 (omitted category)`, `Skipped: 0 (omitted category)`); `Total time: 2.9408 Seconds`; exit 0. TRX counters: `total=234 executed=234 passed=234 failed=0`; 234 `UnitTestResult` rows, all `Passed`.

TOTAL-EQUALS-PASSED: true

`scripts/vscode/TaskMaster.cli.runsettings` is byte-identical to HEAD (`git status --porcelain` on that path prints nothing); the run used it with `/InIsolation`.

### PASS-AFTER lines (one per Issue792/contract test; fully qualified from the TRX; 32 lines)

PASS-AFTER: QuickFiler.Controllers.Tests.EfcDataModelIssue792CarryTests.InitFolderHandlerAsync_WithCarriedPredictor_AdoptsItAndReleasesTheCarry
PASS-AFTER: QuickFiler.Controllers.Tests.EfcDataModelIssue792CarryTests.InitFolderHandlerAsync_WithNonPredictorCarry_RunsTheExistingPathAndReleasesTheCarry
PASS-AFTER: QuickFiler.Controllers.Tests.EfcDataModelIssue792CarryTests.TryAdoptCarriedFolderHandler_WithExplicitListAndPredictor_DoesNotAdopt
PASS-AFTER: QuickFiler.Controllers.Tests.EfcDataModelIssue792CarryTests.TryAdoptCarriedFolderHandler_WithNonPredictorHandler_DoesNotAdopt
PASS-AFTER: QuickFiler.Controllers.Tests.EfcDataModelIssue792CarryTests.TryAdoptCarriedFolderHandler_WithNullCarry_DoesNotAdopt
PASS-AFTER: QuickFiler.Controllers.Tests.EfcDataModelIssue792CarryTests.TryAdoptCarriedFolderHandler_WithNullListAndConcretePredictor_Adopts
PASS-AFTER: QuickFiler.Controllers.Tests.EfcFormControllerIssue792Tests.InitializeBreadcrumbHostAsync_OnFinalFailure_NotifiesTheRouter
PASS-AFTER: QuickFiler.Controllers.Tests.EfcFormControllerIssue792Tests.InitializeBreadcrumbHostAsync_OnFinalFailure_ShowsTheErrorTextInTheFolderAreaLabel
PASS-AFTER: QuickFiler.Controllers.Tests.EfcFormControllerIssue792Tests.InitializeBreadcrumbHostAsync_RetriesUpToTheAttemptLimitThenReportsOnce
PASS-AFTER: QuickFiler.Controllers.Tests.EfcFormControllerIssue792Tests.InitializeBreadcrumbHostAsync_SucceedsOnALaterAttempt_ReportsNothing
PASS-AFTER: QuickFiler.Controllers.Tests.EfcFormControllerIssue792Tests.InitializeBreadcrumbHostAsync_WhenCanceled_DoesNotRetryOrReport
PASS-AFTER: QuickFiler.Controllers.Tests.EfcFormControllerIssue792Tests.InitializeBreadcrumbHostAsync_WhenHostIsNull_ReportsThroughTheBoundarySinkToTheUser
PASS-AFTER: QuickFiler.Controllers.Tests.EfcFormControllerIssue792Tests.PopulateFolderCombobox_WhenDataModelFaults_NotifiesTheUserThroughTheDefaultSink
PASS-AFTER: QuickFiler.Controllers.Tests.QfcCollectionControllerIssue792PopOutTests.EfcHomeController_DepositsTheCarryOnTheDataModelBeforeConstructingTheFormController
PASS-AFTER: QuickFiler.Controllers.Tests.QfcCollectionControllerIssue792PopOutTests.PopOutHomeControllerFactory_DefaultIsTheNamedProductionFactory
PASS-AFTER: QuickFiler.Controllers.Tests.QfcCollectionControllerIssue792PopOutTests.ReadPopOutCarry_WithConcreteItemController_ReturnsHandlerAndHelper
PASS-AFTER: QuickFiler.Controllers.Tests.QfcCollectionControllerIssue792PopOutTests.ReadPopOutCarry_WithInterfaceOnlyController_ReturnsNullHandlerAndTheHelper
PASS-AFTER: QuickFiler.Controllers.Tests.QfcCollectionControllerIssue792PopOutTests.ReadPopOutCarry_WithNullController_ReturnsNulls
PASS-AFTER: QuickFiler.Test.Controllers.BreadcrumbBridgeRouterIssue792Tests.NotifyCoreInitialized_AfterAnEarlierStash_StillNavigatesIt
PASS-AFTER: QuickFiler.Test.Controllers.BreadcrumbBridgeRouterIssue792Tests.NotifyInitializationFailed_ClearsThePendingDocumentAndNavigatesTheErrorBanner
PASS-AFTER: QuickFiler.Test.Controllers.BreadcrumbBridgeRouterIssue792Tests.NotifyInitializationFailed_LeavesNoStashForALaterInitialization
PASS-AFTER: QuickFiler.Test.Controllers.BreadcrumbBridgeRouterIssue792Tests.NotifyInitializationFailed_WithNullFailure_Throws
PASS-AFTER: QuickFiler.Test.Controllers.BreadcrumbOutboundQueueIssue792Tests.DiscardPending_OnAnEmptyQueue_ReturnsZero
PASS-AFTER: QuickFiler.Test.Controllers.BreadcrumbOutboundQueueIssue792Tests.DiscardPending_ReturnsTheDiscardedCountAndLeavesZeroPending
PASS-AFTER: QuickFiler.Test.Controllers.BreadcrumbOutboundQueueIssue792Tests.NotifyInitializationFailed_DiscardsTheOutboundQueueWithoutPosting
PASS-AFTER: QuickFiler.Test.HelperClasses.EfcViewerQueueIssue792Tests.ProductionBlockingPriorityScheduler_DefaultIsTheNamedUiDispatcherInvoke
PASS-AFTER: QuickFiler.Test.Viewers.WebView2BreadcrumbHostIssue792Tests.InitializeAsync_PassesTheSharedFolderAndIncognitoArgumentToTheSeam
PASS-AFTER: QuickFiler.Test.Viewers.WebView2BreadcrumbHostIssue792Tests.NavigateToString_BeforeCoreInitialization_WithNoDispatcher_DropsTheDocumentWithoutThrowing
PASS-AFTER: QuickFiler.Test.Viewers.WebView2EnvironmentContractTests.AdditionalBrowserArguments_IsAsciiDoubleHyphenIncognitoWithTrailingSpace
PASS-AFTER: QuickFiler.Test.Viewers.WebView2EnvironmentContractTests.CreateOptions_CarriesTheSharedArgumentsOnAFreshInstance
PASS-AFTER: QuickFiler.Test.Viewers.WebView2EnvironmentContractTests.EfcItemController_InitializeWebViewAsync_PassesTheContractValuesThroughTheSeam
PASS-AFTER: QuickFiler.Test.Viewers.WebView2EnvironmentContractTests.ResolveUserDataFolder_CombinesLocalApplicationDataWithTheSharedLeafName

ISSUE792-AND-CONTRACT-COUNT: 32 (declared 32)

### Pass for the reason the fix supplies (no assertion weakened)

`git diff --stat HEAD -- QuickFiler.Test` lists exactly one file, `QuickFiler.Test/Viewers/WebView2EnvironmentContractTests.cs` (84 insertions, 0 deletions: the [P4-T4] test and its `SetPrivateField` helper). No test recorded failing in [P3-T7] was edited, so each passes on the clause it failed on:

- `InitializeAsync_PassesTheSharedFolderAndIncognitoArgumentToTheSeam` (failed on `AdditionalBrowserArguments ... found <null>`): the host now passes `WebView2EnvironmentContract.CreateOptions()` [P4-T1].
- `NavigateToString_BeforeCoreInitialization_WithNoDispatcher_DropsTheDocumentWithoutThrowing` (failed on `InvalidOperationException: The instance of CoreWebView2 is uninitialized`): the inline path now reads `CoreWebView2`, logs `document dropped.` and returns [P4-T1].
- `NotifyInitializationFailed_LeavesNoStashForALaterInitialization` (failed on the CONTENT clause while `HaveCount(1)` already passed): the single navigation is now the banner document containing `Folder list unavailable` and not `Alpha`, and the stash is null so `NotifyCoreInitialized` replays nothing [P4-T6]; the test still asserts both count and content.
- `NotifyInitializationFailed_ClearsThePendingDocumentAndNavigatesTheErrorBanner` (failed on `_navigated` empty): one banner navigation, selection cleared, subscriber notified with null [P4-T6].
- `ProductionBlockingPriorityScheduler_DefaultIsTheNamedUiDispatcherInvoke` (failed on `<.cctor>b__26_2` versus `InvokeOnUiDispatcher`): the default is now the method group [P4-T10].
- `DiscardPending_ReturnsTheDiscardedCountAndLeavesZeroPending` (failed on `expected 3, found 0`) and `NotifyInitializationFailed_DiscardsTheOutboundQueueWithoutPosting` (failed on `PendingCount expected 0, found 2`): `DiscardPending` clears and reports the count [P4-T5]; the router calls it [P4-T6].
- The five `InitializeBreadcrumbHostAsync_*` tests plus `InitializeBreadcrumbHostAsync_WhenHostIsNull_ReportsThroughTheBoundarySinkToTheUser` (failed on invocation counts 0 against 3/2/1, empty label text, no router navigation, empty capture): the bounded loop over the seam, the `OperationCanceledException` stop, `ShowFolderAreaError`, `_router?.NotifyInitializationFailed` and the single `TryReportBoundaryFault` naming `after 3 attempts` [P4-T7].
- `TryAdoptCarriedFolderHandler_WithNullListAndConcretePredictor_Adopts` (failed on `False`), `InitFolderHandlerAsync_WithCarriedPredictor_AdoptsItAndReleasesTheCarry` (threw `NullReferenceException` from the predictor constructor), `InitFolderHandlerAsync_WithNonPredictorCarry_RunsTheExistingPathAndReleasesTheCarry` (failed on `CarriedFolderHandler` still holding the mock): adoption, `MailInfo ?? CarriedMailHelper`, and `ReleaseCarry()` on both branches [P4-T8].
- `ReadPopOutCarry_WithConcreteItemController_ReturnsHandlerAndHelper` (failed on null handler) and `ReadPopOutCarry_WithInterfaceOnlyController_ReturnsNullHandlerAndTheHelper` (failed on null helper): the pattern-matched read [P4-T9].

### Composition of the 234-test run (per class, from the TRX)

EfcDataModelIssue792CarryTests 6; EfcDataModelTests 6; EfcFormControllerIssue792Tests 7; EfcFormControllerTests 32; EfcHomeControllerDependenciesTests 9; EfcHomeControllerDependenciesTestsProductionFactory 5; EfcHomeControllerExecuteMovesTests 7; EfcHomeControllerLifecycleTests 11; EfcHomeControllerMetricsTests 15; EfcHomeControllerSeamTests 4; EfcHomeControllerTests 6; EfcItemControllerTests 10; QfcCollectionControllerIssue792PopOutTests 5; QfcCollectionControllerTests 13; QfcItemController_InitializationTests 15; BreadcrumbBridgeRouterIssue792Tests 4; BreadcrumbBridgeRouterQueueTests 26; BreadcrumbOutboundQueueIssue792Tests 3; EfcDataModelArchiveRootTests 11; EfcDataModelIssue614Tests 8; EfcDataModelIssue637Tests 8; EfcViewerQueueIssue792Tests 1; ViewerQueueStaticWrapperTests 8; WebView2BreadcrumbHostIssue792Tests 2; WebView2BreadcrumbHostTests 8; WebView2EnvironmentContractTests 4. Every one of the eleven filter alternatives matched at least one class, so no alternative was silently empty.

## Phase 4 file footprint (line counts by `(Get-Content -LiteralPath <path>).Count`; every file CRLF; BOM state preserved as at HEAD)

- `QuickFiler/Viewers/WebView2BreadcrumbHost.cs`: 382 (ceiling 390); no BOM
- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`: 474 (ceiling 476); BOM preserved; numstat 3/8 against BASE-SHA
- `QuickFiler/Controllers/EfcItemController.WebViewEnvironment.cs`: 56 (ceiling 90); no BOM
- `QuickFiler.Test/Viewers/WebView2EnvironmentContractTests.cs`: 192 (ceiling 200); no BOM
- `QuickFiler/Controllers/BreadcrumbOutboundQueue.cs`: 80 (ceiling 90); no BOM
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`: 453 (ceiling 470); BOM preserved; numstat 46/0 against BASE-SHA and 32/0 against HEAD (deletions 0, `NotifyCoreInitialized` untouched at lines 320-329)
- `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs`: 178 (ceiling 230); no BOM; `ConfigureBreadcrumbControl` byte-identical to its moved form (1059 characters at HEAD and now)
- `QuickFiler/Controllers/EfcDataModel.Carry.cs`: 100 (ceiling 120); no BOM
- `QuickFiler/Controllers/QfcCollectionController.PopOut.cs`: 111 (ceiling 140); no BOM
- `QuickFiler/Helper Classes/EfcViewerQueue.cs`: 108 (ceiling 115); no BOM; numstat 4/4 against HEAD (the two scheduler lines and the two summary lines of `InvokeOnUiDispatcher`)

## Deviations recorded for the caller (plan text versus the tree; no task text was changed)

1. [P4-T3] `Select-String -SimpleMatch 'IncognitoArgument = WebView2EnvironmentContract.AdditionalBrowserArguments;'` returns 0: CSharpier breaks the 105-column declaration after `=`. Verified instead by `internal const string IncognitoArgument =` (1) immediately followed by `WebView2EnvironmentContract.AdditionalBrowserArguments;` (1); multiline regex match count 1. Detailed in `p4-t4-site3-mutation.md`.
2. [P4-T4] `git diff --numstat HEAD -- QuickFiler/Controllers/EfcItemController.WebViewEnvironment.cs` cannot read `0 0` before [P4-T12] because HEAD predates the [P4-T3] rewrite (observed `18 32`); restoration proven by SHA-256 identity and a no-index diff against the pre-mutation snapshot. Detailed in `p4-t4-site3-mutation.md`.
3. [P4-T7] `Select-String -SimpleMatch 'catch (OperationCanceledException)'` returns 2, not 1: the moved `BindBreadcrumbRowsAsync` already carried one such catch at HEAD line 115 (positive control: HEAD count 1), and the retry loop adds the second. The plan's count omitted the pre-existing catch.
