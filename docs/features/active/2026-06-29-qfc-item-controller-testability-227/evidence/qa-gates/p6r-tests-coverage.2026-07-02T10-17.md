# Phase 6 Gate — Tests + Coverage (P6-T17)

Timestamp: 2026-07-02T10-17
Command (tests, canonical): vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation
Command (numeric coverage): dotnet-coverage collect --output artifacts\csharp\coverage-r2-p6.cobertura.xml --output-format cobertura --settings coverage.config -- vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation
EXIT_CODE: 0

## Test result (regression guard)

- Total tests: 328
- Passed: 328
- Failed: 0
- Regression baseline (P0-T5 = 233; Phase 5 = 289) preserved; 39 new Phase 6 tests added on top of 289
  (289 + 39 = 328). All pass. Two existing Phase 5 tests were updated (not added) to inject the new
  `_uiDispatcher` / `_mailActions` seams instead of the pre-seam static-dispatcher / raw-`Mail`
  behavior.

## Coverage mechanism note

The `[ExcludeFromCodeCoverage]`-annotated members are still emitted (as hits=0) by dotnet-coverage in
the Cobertura output, so per-partial line-rates are computed by excluding the annotated member
source-line spans from the per-line hit data (identical methodology to the Phase 5 gate). Coverage is
collected with the repository's established `dotnet-coverage collect --output-format cobertura
--settings coverage.config` mechanism; all 328 tests pass under instrumentation.

## Numeric coverage (affected QfcItemController non-exempt denominator)

| Partial | non-exempt covered/total | % |
|---|---:|---:|
| QfcItemController.cs | 72/73 | 98.63% |
| QfcItemController.Conversation.cs | 80/97 | 82.47% |
| QfcItemController.EventHandlers.cs | 58/73 | 79.45% |
| QfcItemController.EventWiring.cs | 206/260 | 79.23% |
| QfcItemController.FocusAndTheme.cs | 86/129 | 66.67% |
| QfcItemController.FolderHandling.cs | 26/29 | 89.66% |
| QfcItemController.Initialization.cs | 112/113 | 99.12% |
| QfcItemController.MailActions.cs | 96/125 | 76.80% |
| QfcItemController.Navigation.cs | 93/95 | 97.89% |
| QfcItemController.ViewerSetup.cs | 56/57 | 98.25% |
| AGGREGATE | 885/1051 | 84.21% |

- Affected testable non-exempt denominator: 885/1051 = **84.21%** (>= 80% AC5 floor met; up from the
  Phase 5 figure of 83.71%).
- New/extracted controller code (all covered by >= 1 passing Phase 6 test): `WireIntentEvents`,
  `BtnPopOutCore`, `BtnReplyCore`, `BtnReplyAllCore`, `BtnForwardCore`, `TxtboxBodyDoubleClickCore`,
  `HandleWebViewInitializedAsync`, plus the de-exempted dispatcher/COM/factory members. Each extracted
  method is exercised at 100% by its dedicated `Seam*Tests` test (SeamDispatcherTests, SeamCoreTests,
  SeamFactoryTests each report 100% line coverage on their own bodies, confirming the extracted logic
  is fully driven).
- New seam files: `IUiDispatcher` / `IWebViewCoreInitializer` / `IMailItemActions` are declaration-only
  interfaces (no executable lines). `WpfUiDispatcher` / `WebView2CoreInitializer` /
  `MailItemActionsAdapter` are `[ExcludeFromCodeCoverage]` thin forwarding adapters (DI-seam adapter
  tier); each carries a construction/forwarding smoke test (`WpfUiDispatcherTests`,
  `WebView2CoreInitializerTests`, `MailItemActionsAdapterTests`).

## Exemption count

- Starting Phase 6 (Phase 5 end): 57 (in the QfcItemController partials).
- After Phase 6: **42** (in the QfcItemController partials) — 15 members de-exempted this phase, each
  covered by >= 1 passing test.
- Plus 3 NEW legitimate adapter-shim exemptions (`WpfUiDispatcher`, `WebView2CoreInitializer`,
  `MailItemActionsAdapter`), each a thin forwarding shim over a static/third-party/COM boundary
  (DI-seam adapter tier) — these are the intended, individually-justified residual introduced by the
  seams, not blanket exemptions.

## Scope deviations recorded (carried to Phase 7 residual boundary)

- `InitializeWebViewAsync` (P6-T5): the WebView2 SDK calls were routed through the new
  `IWebViewCoreInitializer` seam (isolating the SDK dependency into the exempt `WebView2CoreInitializer`
  adapter), but the method retains its `[ExcludeFromCodeCoverage]` as a justified bucket-(iii) residual.
  It still performs the concrete-bound `((ItemViewer)_itemViewer).L0v2h2_WebView2` access and awaits
  `_itemViewer.UiSyncContext`; `IItemViewer` intentionally exposes no WebView-core-init intent member
  (cycle-1 narrowing retained the raw control here), so the concrete cast cannot execute against a
  `Mock<IItemViewer>`. Reclassified as a residual (not de-exempted) — noted explicitly.
- `ApplyReadEmailFormat` (part of P6-T7): the direct `Mail.UnRead`/`Mail.Save()` COM writes were routed
  through the `IMailItemActions` seam, but the method retains its exemption as a justified bucket-(iii)
  residual because it also calls `_themes[_activeTheme].SetMailRead(async: true)`, which unconditionally
  invokes `_lblSender.BeginInvoke` on a live WinForms control (Theme throws when the control lacks a
  window handle) — the same out-of-scope Theme barrier as the Phase 5 `ToggleFocus` residual. No Theme
  seam this cycle (Option A). Reclassified as a residual — noted explicitly.

Output Summary: 328/328 tests pass (289 regression baseline preserved + 39 new). Affected non-exempt
denominator 885/1051 = 84.21% (>= 80%). Exemption count reduced 57 -> 42 in the partials (15
de-exempted, each covered), plus 3 new adapter-shim residuals. Two members (`InitializeWebViewAsync`,
`ApplyReadEmailFormat`) reclassified as justified bucket-(iii) residuals (SDK/COM dependency seamed;
concrete-control / Theme barrier remains).
