# [P1-T4] [expect-fail] Phase 1 regression tests observed failing on the unfixed tree

- Issue: #792
- Timestamp: 2026-09-17T19-01
- Command: CMD-OUTLOOK (`Get-Process -Name OUTLOOK`, printed `OUTLOOK-CLOSED: true`), then CMD-BUILD-PLAIN (`msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`, exit 0, `0 Warning(s)`, exact line `0 Error(s)`, `Build succeeded.`, 4 s incremental; `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll` rewritten at 19:01:15 so the three new Phase 1 files compiled), then CMD-VSTEST (vswhere resolved `vstest.console.exe`), then `& $vstest QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~WebView2BreadcrumbHostIssue792Tests|FullyQualifiedName~EfcFormControllerIssue792Tests|FullyQualifiedName~EfcViewerQueueIssue792Tests" "/ResultsDirectory:coverage/test-results/p1-t4" "/Logger:trx;LogFileName=p1-t4.trx"` (CMD-SCOPED-RUN with `<task>` = `p1-t4`; run from `coverage/plan792-helper.ps1` with the item worktree as the working directory; console captured to the gitignored `coverage/p1-t4-scoped.log`; TRX under the gitignored `coverage/test-results/p1-t4/`)
- EXIT_CODE: 1
- ExpectedExitCode: 1
- Output Summary: `Test Run Failed.`; `Total tests: 5`; `Passed: 1`; `Failed: 4`; `Skipped: 0 (omitted category)`; `Total time: 1.3937 Seconds`. The partition matches the plan's declaration exactly (5 / 1 / 4, the four declared tests failing, the declared control passing), and each failing test failed on its pre-predicted assertion. The filter discovered exactly five tests, so the run is not vacuous.

## Observed partition

Declared by the plan: `Total tests: 5`, `Passed: 1`, `Failed: 4`, failing set = the two host tests, `InitializeBreadcrumbHostAsync_WhenHostIsNull_ReportsThroughTheBoundarySinkToTheUser`, `ProductionBlockingPriorityScheduler_DefaultIsTheNamedUiDispatcherInvoke`; pass-before control = `PopulateFolderCombobox_WhenDataModelFaults_NotifiesTheUserThroughTheDefaultSink`.

Observed (console, in reported order):

- `Passed PopulateFolderCombobox_WhenDataModelFaults_NotifiesTheUserThroughTheDefaultSink [57 ms]`
- `Failed InitializeBreadcrumbHostAsync_WhenHostIsNull_ReportsThroughTheBoundarySinkToTheUser [66 ms]`
- `Failed ProductionBlockingPriorityScheduler_DefaultIsTheNamedUiDispatcherInvoke [136 ms]`
- `Failed InitializeAsync_PassesTheSharedFolderAndIncognitoArgumentToTheSeam [193 ms]`
- `Failed NavigateToString_BeforeCoreInitialization_WithNoDispatcher_DropsTheDocumentWithoutThrowing [8 ms]`

PARTITION-MATCHES-DECLARATION: true

## Fail-before evidence (first assertion message line per failed test)

FAIL-BEFORE: QuickFiler.Test.Viewers.WebView2BreadcrumbHostIssue792Tests.InitializeAsync_PassesTheSharedFolderAndIncognitoArgumentToTheSeam | Expected capturedOptions.AdditionalBrowserArguments to be "--incognito " because every WebView2 site must share the same incognito browser argument, but found <null>.

FAIL-BEFORE: QuickFiler.Test.Viewers.WebView2BreadcrumbHostIssue792Tests.NavigateToString_BeforeCoreInitialization_WithNoDispatcher_DropsTheDocumentWithoutThrowing | Did not expect any exception because a document navigated before core initialization must be dropped, not forwarded to a control with no core, but found System.InvalidOperationException: The instance of CoreWebView2 is uninitialized and unable to complete this operation. See EnsureCoreWebView2Async.

FAIL-BEFORE: QuickFiler.Controllers.Tests.EfcFormControllerIssue792Tests.InitializeBreadcrumbHostAsync_WhenHostIsNull_ReportsThroughTheBoundarySinkToTheUser | Expected captured to contain a single item because the final initialization failure must be reported to the user exactly once, but the collection is empty.

FAIL-BEFORE: QuickFiler.Test.HelperClasses.EfcViewerQueueIssue792Tests.ProductionBlockingPriorityScheduler_DefaultIsTheNamedUiDispatcherInvoke | Expected scheduler.Method.Name to be a match with the expectation because the blocking scheduler must be the named UI-dispatcher invoke, not a lambda, but it differs at index 0: (actual) "<.cctor>b__25_2" / (expected) "InvokeOnUiDispatcher"

PASS-BEFORE-CONTROL: EfcFormControllerIssue792Tests.PopulateFolderCombobox_WhenDataModelFaults_NotifiesTheUserThroughTheDefaultSink

## Correspondence to the plan's predictions

- Host test 1: the plan predicts the second assertion (the `AdditionalBrowserArguments` literal) fails because the parameterless options carry null. Observed: the folder assertion passed and the arguments assertion failed with `found <null>`. Matches.
- Host test 2: the plan predicts the inline forward reaches `WebView2.NavigateToString` on a control with no core and throws `InvalidOperationException`. Observed: `System.InvalidOperationException: The instance of CoreWebView2 is uninitialized`. Matches.
- `InitializeBreadcrumbHostAsync` test: the plan predicts the null host raises `NullReferenceException`, which the old catch logs only, so the list stays empty. Observed: the method did not throw (the `NotThrowAsync` assertion passed) and the captured list was empty. Matches.
- Scheduler test: the plan predicts the default is a lambda whose compiler-generated name is not `InvokeOnUiDispatcher`. Observed actual name `<.cctor>b__25_2` (compiler-generated, static-constructor lambda). Matches.

## Observed-failing map entries satisfied by this run

- AC-U3 UI-thread half: `ProductionBlockingPriorityScheduler_DefaultIsTheNamedUiDispatcherInvoke` observed failing.
- AC-U4 `InitializeBreadcrumbHostAsync` half: `InitializeBreadcrumbHostAsync_WhenHostIsNull_ReportsThroughTheBoundarySinkToTheUser` observed failing.
- AC-U6 site-1 seam test: `InitializeAsync_PassesTheSharedFolderAndIncognitoArgumentToTheSeam` observed failing.
- Host pre-initialization guard (D4): `NavigateToString_BeforeCoreInitialization_WithNoDispatcher_DropsTheDocumentWithoutThrowing` observed failing.

## Files created in Phase 1 (line counts by `(Get-Content -LiteralPath <path>).Count`)

- `QuickFiler.Test/Viewers/WebView2BreadcrumbHostIssue792Tests.cs`: 151 (ceiling 200)
- `QuickFiler.Test/Controllers/EfcFormControllerIssue792Tests.cs`: 148 (ceiling 260)
- `QuickFiler.Test/Helper Classes/EfcViewerQueueIssue792Tests.cs`: 42 (ceiling 120); contains no `DoNotParallelize` token and no `ResetProductionCoreDefaultsForTesting` token (positive control: the same search hits `ViewerQueueStaticWrapperTests.cs` at lines 11 and 18,20)
- `QuickFiler.Test/QuickFiler.Test.csproj`: three bare self-closing `<Compile Include>` items added (numstat `3 0` against `origin/main`), at lines 128, 219 and 233, each immediately after its named neighbour (127, 218, 232); CRLF preserved.

No production `.cs` file, no other `.csproj`, and no `.runsettings` file differs from `origin/main` after Phase 1.
