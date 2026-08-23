# P5-T6 — CreateAsync / CreateSequentialAsync De-Exemption Tests

Issue: #230
Task: [P5-T6]

## Step 1 — Build

- Timestamp: 2026-08-07T23-15
- Command: `MSBuild.exe QuickFiler.Test/QuickFiler.Test.csproj -t:Build -p:Configuration=Debug -p:Platform="AnyCPU" -v:m`
- EXIT_CODE: 0
- Output Summary: Build succeeded, 0 errors. The additive seam parameters on both
  static factories compile with no change to any existing call site.

## Step 2 — Filtered test run (D6 command form)

- Timestamp: 2026-08-07T23-15
- Command:
  ```powershell
  $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
  $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
  & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~SeamFactoryTests"
  ```
- EXIT_CODE: 0
- Output Summary: **Total tests: 9 — Passed: 9, Failed: 0.** Total time 3.8839
  seconds.

### Executed tests

```
Passed PopulateConversation_UsesResolverFactoryAndRendersCount [491 ms]
Passed FlagAsTask_InvokesFactoryWithExpectedArguments [29 ms]
Passed FlagAsTaskAsync_InvokesFactoryThroughDispatcher [14 ms]
Passed MoveMailAsync_WhenItemHelperNull_DoesNotInvokeFactory [1 ms]
Passed MoveMailAsync_WhenOneDriveMissing_ReturnsWithoutInvokingFactory [12 ms]
Passed MoveMailAsync_WhenOneDrivePresent_InvokesFactoryWithConfigAndEnqueues [18 ms]
Passed WireIntentEvents_SubscribesEveryIntentEvent [17 ms]
Passed CreateSequentialAsync_WithInjectedSeams_ReturnsAnInitializedController [1 s]
Passed CreateAsync_WithFaultingWebViewSeam_FaultsWithThatExceptionAfterInitializing [349 ms]
```

## Changes recorded by this phase

- **P5-T1** — `CreateAsync` and `CreateSequentialAsync` in
  `QuickFiler/Controllers/QfcItemController.Initialization.cs` gained three optional
  seam parameters appended after `CancellationToken token`:
  `UtilitiesCS.Threading.IUiDispatcher uiDispatcher = null`,
  `QuickFiler.Viewers.IWebViewCoreInitializer webViewInitializer = null`,
  `Func<MailItem, ConversationResolver> conversationResolverFactory = null`. They
  are assigned to `controller._uiDispatcher`, `controller._webViewInitializer` and
  `controller._conversationResolverFactory` after construction and before
  `controller.SaveParameters(...)`, exactly mirroring the primary constructor's
  optional-seam pattern, so `SaveParameters`'s `??=` defaults preserve current
  behavior when a seam is left null.
- **P5-T3** — added
  `CreateSequentialAsync_WithInjectedSeams_ReturnsAnInitializedController` to
  `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs`. It drives the
  factory end-to-end through `WinFormsPumpHost` with the new seam parameters
  supplying a mocked `IWebViewCoreInitializer`, an inline-executing `IUiDispatcher`
  and a conversation-resolver factory, awaits the factory to **normal completion**
  (D13: the `InitializeSequentialAsync` tail is fire-and-forget), and asserts the
  returned controller's `Parent`, `ItemNumber`, `TableLayoutPanels`, `Buttons`,
  `_themes`, and that the injected `_webViewInitializer` survived
  `SaveParameters`'s `??=` defaults.
- **P5-T4** — added
  `CreateAsync_WithFaultingWebViewSeam_FaultsWithThatExceptionAfterInitializing`.
  The mocked `IWebViewCoreInitializer.CreateEnvironmentAsync` faults with the
  distinguishable `WebViewSentinelException`; the test asserts that **exact
  exception identity** on the awaited factory task, then asserts the observable
  state the preceding lines set (`ItemNumber` 6, `TableLayoutPanels`, `_themes`) via
  the viewer's `Controller` back-reference, which is the only handle a test has on
  the factory-built controller once the factory faults.

  Per D13, `CreateAsync` awaits `InitializeAsync`, whose final statement is
  `await InitializeWebViewAsync()`. Under the mocked seam that await always faults,
  so **`CreateAsync` can never reach its `return controller;` statement in a unit
  test**. Its per-member coverage is therefore partial by construction and the D5
  gate (c) bar for it is "> 0%", not "no uncovered lines". This is recorded in the
  test's XML documentation comment as well as here.
- **P5-T5** — removed the `[ExcludeFromCodeCoverage]` attributes from `CreateAsync`
  and `CreateSequentialAsync` in the same change, replacing their bucket-iii
  comments with `#230` notes naming the covering tests.
  `QfcItemController.Initialization.cs` now contains exactly **one** remaining
  exemption site (`InitializeAsync`).

## Harness additions required to reach the web-view seam (recorded for the audit trail)

`InitializeAsync` (driven by `CreateAsync`) additionally runs
`PopulateFolderComboBoxAsync`, which the sequential path does not. Three
successive failures were resolved by widening the **mock** graph only — no
production code was changed to accommodate a test:

1. `FolderScorer.AddConversationBasedSuggestions` read `_globals.AF.CtfMap` (null).
   Fixed with `Mock<IAppAutoFileObjects>.CtfMap => new CtfMap()` (empty, so
   `ContainsId` is false).
2. `OlFolderClassifierGroup.GetFolderPredictorAsync` fell through to
   `Globals.AF.Manager["Folder"]` (null). Fixed by selecting the existing LCPPN
   seam: `AF.UseLcppnPredictor => true` plus `AF.FolderPredictor =>
   Mock<IFolderPredictor>` whose `Classify` returns an empty ordered sequence. This
   keeps the entire flat Bayesian classifier stack out of the test.
3. `FolderPredictor.FolderArray` read `_globals.AF.RecentsList` (null). Fixed with
   `AF.RecentsList => new SloLinkedList<string>()`.

No live Outlook object, no persisted state, no temporary file, and no real
WebView2 runtime is involved at any point.

Exemption sites remaining in `QfcItemController.Initialization.cs` after this
phase: 1 (`InitializeAsync`).
