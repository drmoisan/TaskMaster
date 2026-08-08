# P6-T4 — InitializeAsync De-Exemption Tests

Issue: #230
Task: [P6-T4]

## Step 1 — Build

- Timestamp: 2026-08-07T23-25
- Command: `MSBuild.exe QuickFiler.Test/QuickFiler.Test.csproj -t:Build -p:Configuration=Debug -p:Platform="AnyCPU" -v:m`
- EXIT_CODE: 0
- Output Summary: Build succeeded, 0 errors.

## Step 2 — Filtered test run (D6 command form)

- Timestamp: 2026-08-07T23-25
- Command:
  ```powershell
  $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
  $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
  & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~InitializationTests|FullyQualifiedName~SeamFactoryTests"
  ```
  (The `SeamFactoryTests` clause is joined with `|` per D6 — `vstest` rejects `OR` —
  and is included because the P6 fixture change touches the shared
  `BuildPumpHarnessAsync` those tests also consume.)
- EXIT_CODE: 0
- Output Summary: **Total tests: 18 — Passed: 18, Failed: 0.** Total time 3.3211
  seconds.

### Executed tests

```
Passed PrimaryConstructor_AssignsFieldsAndSetsControllerBackReference [342 ms]
Passed PopulateConversation_UsesResolverFactoryAndRendersCount [341 ms]
Passed PredeterminedFolderConstructor_StoresPredeterminedFolder [2 ms]
Passed AsyncFlagConstructor_AssignsFieldsViaSaveParameters [< 1 ms]
Passed SaveParameters_AssignsAllFieldsAndResolvesCollaborators [1 ms]
Passed FlagAsTask_InvokesFactoryWithExpectedArguments [20 ms]
Passed FlagAsTaskAsync_InvokesFactoryThroughDispatcher [8 ms]
Passed MoveMailAsync_WhenItemHelperNull_DoesNotInvokeFactory [1 ms]
Passed MoveMailAsync_WhenOneDriveMissing_ReturnsWithoutInvokingFactory [11 ms]
Passed MoveMailAsync_WhenOneDrivePresent_InvokesFactoryWithConfigAndEnqueues [19 ms]
Passed WireIntentEvents_SubscribesEveryIntentEvent [14 ms]
Passed CreateSequentialAsync_WithInjectedSeams_ReturnsAnInitializedController [1 s]
Passed InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState [1 s]
Passed InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme [118 ms]
Passed InitializeBool_ThroughThePumpHost_CompletesAndInitializesState [127 ms]
Passed InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates [94 ms]
Passed CreateAsync_WithFaultingWebViewSeam_FaultsWithThatExceptionAfterInitializing [404 ms]
Passed InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults [89 ms]
```

## Changes recorded by this phase

- **P6-T1** — added
  `InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults` to
  `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs`. It
  awaits `InitializeAsync()` under the established pump harness and asserts the
  **controlled fault** from the mocked `IWebViewCoreInitializer` at the member's
  final `await InitializeWebViewAsync()` — the injected `WebViewSentinelException`
  identity specifically, which distinguishes "stopped at the seam" from a timeout,
  a hang, or an unrelated null-reference. It then asserts the observable state set
  by every preceding line: `TableLayoutPanels`, `Buttons`, `_themes` (SetupThemes),
  `ItemHelper` (PopulateControlsAsync) and `_folderHandler`
  (PopulateFolderComboBoxAsync). `[Timeout(60000)]`; host stopped in `finally`;
  static `UiThread.Dispatcher` restored via `PumpHarness.Restore()`.
- **P6-T2** — removed the `[ExcludeFromCodeCoverage]` attribute from
  `InitializeAsync` in `QuickFiler/Controllers/QfcItemController.Initialization.cs`
  in the same change, replacing its residual-barrier comment with a `#230` note.
  **Zero exemption sites now remain in `QfcItemController.Initialization.cs`.**
- **P6-T3** — comment-only update to the **retained**
  `[ExcludeFromCodeCoverage]` on `InitializeWebViewAsync`
  (`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`). The attribute itself
  is unchanged and still present. The new justification states: the pump barrier is
  resolved by the #230 `WinFormsPumpHost` seam and tests do reach the
  `IWebViewCoreInitializer` seam call; the residual barrier is the
  `((ItemViewer)_itemViewer).L0v2h2_WebView2.CoreWebView2` dependency on the real
  WebView2 runtime, an external process barred by the unit-test policy; and the
  separate concrete-accessor barrier is tracked per issue #230.

## Fixture change required by this phase (recorded for the audit trail)

The first run of the new test failed with a `NullReferenceException` in
`LoadFolderHandlerAsync` because `_folderPredictorFactory` was null: the pump
fixture had been injecting private fields one-by-one with `SetField` and therefore
never ran `SaveParameters`, whose `??=` block supplies the folder-predictor,
conversation-resolver and mail-actions defaults. `InitializeSequentialAsync`,
`InitializeGraphicsAsync` and `Initialize(bool)` do not reach that code, which is
why they passed; the P5 factory tests passed because the factories call
`SaveParameters` themselves.

`BuildPumpHarnessAsync` now injects only the two behavioral seams
(`_uiDispatcher`, `_webViewInitializer`) with `SetField` and then calls
`controller.SaveParameters(...)` — the production construction path — so the
remaining collaborators are the production defaults. This makes the fixture a
closer match to production and was re-verified against every test that consumes
it: all 18 pass.

Exemption sites remaining in `QfcItemController.Initialization.cs`: **0**.
Exemption sites remaining in `QfcItemController.ViewerSetup.cs`: 2
(`InitializeWebViewAsync`, retained with the updated justification;
`EnsureBreadcrumbPipeline`, out of scope per spec Non-Goal 2).
