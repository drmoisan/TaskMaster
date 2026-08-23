# P4-T4 — Initialize(bool) / Nine-Arg Initialize De-Exemption Tests

Issue: #230
Task: [P4-T4]

## Step 1 — Build

- Timestamp: 2026-08-07T22-55
- Command: `MSBuild.exe QuickFiler.Test/QuickFiler.Test.csproj -t:Build -p:Configuration=Debug -p:Platform="AnyCPU" -v:m`
- EXIT_CODE: 0
- Output Summary: Build succeeded, 0 errors.

## Step 2 — Filtered test run (D6 command form)

- Timestamp: 2026-08-07T22-55
- Command:
  ```powershell
  $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
  $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
  & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~InitializationTests"
  ```
- EXIT_CODE: 0
- Output Summary: **Total tests: 8 — Passed: 8, Failed: 0.** Total time 2.9479
  seconds.

### Executed tests

```
Passed PrimaryConstructor_AssignsFieldsAndSetsControllerBackReference [291 ms]
Passed PredeterminedFolderConstructor_StoresPredeterminedFolder [1 ms]
Passed AsyncFlagConstructor_AssignsFieldsViaSaveParameters [< 1 ms]
Passed SaveParameters_AssignsAllFieldsAndResolvesCollaborators [< 1 ms]
Passed InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState [1 s]
Passed InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme [79 ms]
Passed InitializeBool_ThroughThePumpHost_CompletesAndInitializesState [81 ms]
Passed InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates [77 ms]
```

## Changes recorded by this phase

- **P4-T1** — added `InitializeBool_ThroughThePumpHost_CompletesAndInitializesState`
  to `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs`.
  The whole synchronous orchestration is run on the pump thread via
  `host.InvokeAsync(() => controller.Initialize(async: false))`. Its tail,
  `_ = _itemViewer.UiDispatcher.InvokeAsync(InitializeWebViewAsync)`, dispatches
  through the pump thread's WPF dispatcher (interop proven by P1-T4) with the
  mocked `IWebViewCoreInitializer` faulting immediately, so no real WebView2
  initialization occurs. Per research section 9 the discarded task's fault path is
  deliberately not asserted. Asserts `TableLayoutPanels`, `Buttons`, `_themes` and
  `ItemHelper`.
- **P4-T2** — added
  `InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates`,
  exercising the private nine-argument `Initialize` overload through the existing
  `QfcItemControllerTestSupport.InvokeNonPublic` reflection helper on the pump
  thread. Asserts both the `SaveParameters` state it funnels (`Parent`,
  `ItemNumber` 9, `ItemNumberDigits` 2) and the state produced by the delegated
  `Initialize(bool)` body (`TableLayoutPanels`, `_themes`).
- **P4-T3** — removed the `[ExcludeFromCodeCoverage]` attributes from the nine-arg
  `Initialize` and `Initialize(bool async)` in
  `QuickFiler/Controllers/QfcItemController.Initialization.cs` in the same change,
  replacing their residual-barrier comments with `#230` notes naming the covering
  tests.

Both tests carry `[Timeout(60000)]`, dispose the host in `finally` via
`StopAsync`, and restore the static `UiThread.Dispatcher` through
`PumpHarness.Restore()`.

Exemption sites remaining in `QfcItemController.Initialization.cs` after this
phase: 3 (`InitializeAsync`, `CreateAsync`, `CreateSequentialAsync`).
