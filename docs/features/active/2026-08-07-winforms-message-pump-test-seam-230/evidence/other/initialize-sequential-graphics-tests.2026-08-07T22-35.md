# P3-T4 — InitializeSequentialAsync / InitializeGraphicsAsync De-Exemption Tests

Issue: #230
Task: [P3-T4]

## Step 1 — Build

- Timestamp: 2026-08-07T22-35
- Command: `MSBuild.exe QuickFiler.Test/QuickFiler.Test.csproj -t:Build -p:Configuration=Debug -p:Platform="AnyCPU" -v:m`
- EXIT_CODE: 0
- Output Summary: Build succeeded, 0 errors.

## Step 2 — Filtered test run (D6 command form)

- Timestamp: 2026-08-07T22-35
- Command:
  ```powershell
  $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
  $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
  & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~InitializationTests"
  ```
- EXIT_CODE: 0
- Output Summary: **Total tests: 6 — Passed: 6, Failed: 0.** Total time 3.1553
  seconds.

### Executed tests

```
Passed PrimaryConstructor_AssignsFieldsAndSetsControllerBackReference [332 ms]
Passed PredeterminedFolderConstructor_StoresPredeterminedFolder [2 ms]
Passed AsyncFlagConstructor_AssignsFieldsViaSaveParameters [< 1 ms]
Passed SaveParameters_AssignsAllFieldsAndResolvesCollaborators [1 ms]
Passed InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState [1 s]
Passed InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme [90 ms]
```

## Changes recorded by this phase

- **P3-T1** — added
  `InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState`, plus
  the shared `BuildPumpHarnessAsync` / `PumpHarness` fixture, to
  `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs`. The
  harness constructs the real `QuickFiler.ItemViewer` on the pump thread, and
  injects `_globals` (with `Ol.DarkMode`, `Ol.EmailPrefixToStrip`, `QfSettings`),
  `_homeController`, `_kbdHandler`, `_tokenSource`, `Token`, `Mail` (a Moq'd COM
  `MailItem`), `ItemNumber`, an inline-executing `IUiDispatcher`, and a mocked
  `IWebViewCoreInitializer`. Asserts `TableLayoutPanels`, `Buttons`, `_themes` and
  `ItemHelper`. The `_ = InitializeWebViewAsync()` tail is fire-and-forget and
  faults at the mocked seam, so the member returns normally (D13).
- **P3-T2** — added
  `InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme`,
  selecting the `darkMode: true` branch so the theme code path differs from the
  sequential test's light branch. Asserts `TableLayoutPanels`, `Buttons`,
  `_themes` and `_activeTheme`.
- **P3-T3** — removed the `[ExcludeFromCodeCoverage]` attributes from
  `InitializeSequentialAsync` and `InitializeGraphicsAsync` in
  `QuickFiler/Controllers/QfcItemController.Initialization.cs` in the same change,
  and replaced their residual-barrier comments with `#230` notes naming the
  covering tests. The remaining Initialization.cs attributes (9-arg `Initialize`,
  `Initialize(bool)`, `InitializeAsync`, `CreateAsync`, `CreateSequentialAsync`)
  were not touched in this phase.

## Implementation note recorded for the audit trail

`QfcTipsDetails.ToggleAsync` (reached via `ToggleTipsAsync`) marshals through the
process-wide static `UtilitiesCS.UiThread.Dispatcher`. In this test assembly that
static is either unset or holds the deliberately **parked** dispatcher seeded by
`QfcItemControllerTestSupport.EnsureUiThreadDispatcher`, neither of which can
complete an `InvokeAsync` — the first run of these two tests failed with a
`NullReferenceException` at `QfcTipsDetails.ToggleAsync`. The fixture therefore
points `UiThread._dispatcher` at the **pump thread's** WPF dispatcher
(`viewer.UiDispatcher`, serviced by the WinForms loop — the interop proven by
`WinFormsPumpHostTests.BothMarshalRoutes_*`) for the duration of the test, and
`PumpHarness.Restore()` restores the previous value in the test's `finally` block
so no process-wide state leaks. This is a save/restore of an existing static, not
a new one; the repository's own `EnsureUiThreadDispatcher` mutates the same field
without restoring it.

Exemption sites remaining in `QfcItemController.Initialization.cs` after this
phase: 5.

## D8 overflow rule applied (post-split re-verification)

After adding both tests the combined
`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs` measured 529
lines, exceeding the 500-line repository limit. Per D8 the file was split into
`partial` continuations of the same test class (no second `[TestClass]`
attribute), each wired into `QuickFiler.Test/QuickFiler.Test.csproj` with a
`<Compile Include>` entry in the same change:

| File | Post-format lines | Content |
|---|---|---|
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs` | 209 | pre-existing constructor/`SaveParameters` tests + `PumpTimeoutMs` |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | 257 | #230 shared pump fixture (`BuildPumpHarnessAsync`, `SwapUiThreadDispatcher`, mocks, `PumpHarness`) |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` | 118 | #230 pump-hosted de-exemption tests |
| `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` | 467 | includes the P2-T1 test |

- Timestamp: 2026-08-07T22-45
- Command: `MSBuild.exe QuickFiler.Test/QuickFiler.Test.csproj -t:Build ...` then the
  D6 vstest form with
  `/TestCaseFilter:"FullyQualifiedName~InitializationTests|FullyQualifiedName~ViewerSetupTests"`
- EXIT_CODE: 0
- Output Summary: **Total tests: 16 — Passed: 16, Failed: 0.** Confirms the split
  files compiled and all three #230 pump-hosted tests still execute.
