# Coverage Delta — Cycle 4, Issue #227

Timestamp: 2026-07-02T16-30

## Baseline vs post-change coverage

| Metric | Baseline (P0-T5) | Post-change (P2-T4) | Delta |
|---|---|---|---|
| Combined test pass count | 4440/4440 | 4442/4442 | +2 (the 2 new tests added in Phase 1) |
| Repo-wide (whole-process, all loaded modules) line coverage | 63.21% | 63.28% | +0.07pp, no regression |
| `UtilitiesCS.dll` line coverage | 85.86% | 85.96% | +0.10pp |
| `QuickFiler.dll` line coverage | 47.69% | 48.32% | +0.63pp (driven by the two `ToggleFocus` overload bodies now genuinely executed) |

No regression against the Phase 0 baseline on any metric.

## Confirmation: `ToggleFocus`/`ToggleFocus(Enums.ToggleState)` production lines are covered

A dedicated `QuickFiler.Test.dll`-only coverage run (`vstest.console.exe QuickFiler.Test.dll /EnableCodeCoverage /InIsolation`, 349/349 passed) was converted to XML (`Microsoft.CodeCoverage.Console.exe merge <file> -f xml`) and inspected for per-line `<range>` coverage against `source_file id="43"` (`QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs`):

- `ToggleFocus(Enums.ToggleState desiredState)` body — lines 27-67:
  - `<range source_id="43" start_line="28" end_line="28" ... covered="yes" />`
  - `<range source_id="43" start_line="29" end_line="66" ... covered="yes" />` (the entire delegate body, including both branches of the `if`/`else if` and the terminal `_themes[_activeTheme].SetQfcTheme(async: false)` call)
  - `<range source_id="43" start_line="67" end_line="67" ... covered="yes" />`
- `ToggleFocus()` parameterless overload body — lines 83-123:
  - `<range source_id="43" start_line="84" end_line="84" ... covered="yes" />`
  - `<range source_id="43" start_line="85" end_line="122" ... covered="yes" />` (the entire delegate body, both branches, terminal `SetQfcTheme` call)
  - `<range source_id="43" start_line="123" end_line="123" ... covered="yes" />`

All ranges spanning both overloads' full method bodies report `covered="yes"`. This confirms the 4 tests in `QfcItemController.FocusAndThemeTests.cs` (2 modified, 2 new) genuinely execute both branches of both `ToggleFocus` overloads, not merely the `Invoke` marshal.

## Changed/new test code coverage

Per policy, `*.Test` files are excluded from the application-code coverage metric. The 2 modified tests (`ToggleFocus_StateOverload_MarshalsThroughItemViewerInvoke`, `ToggleFocus_ParameterlessOverload_MarshalsThroughItemViewerInvoke`), the 2 new tests (`ToggleFocus_StateOverload_Off_FromActive_DeactivatesUiAndSwitchesToNormalTheme`, `ToggleFocus_ParameterlessOverload_FromActive_DeactivatesUiAndSwitchesToNormalTheme`), and the new `EnableHandlelessThemeInvoke`/`SetThemeField`/`SetThemeFieldViaActivator` helpers all executed successfully on every test run in this cycle (P1-T6, P2-T4) with zero failures, confirming they are themselves exercised (self-evidently, since they are the code under test execution).

Acceptance: no regression recorded; `ToggleFocus`/`ToggleFocus(Enums.ToggleState)` lines show as covered in the post-change report.
