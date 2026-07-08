# Phase 1 (R1+R3) Verification — Cycle 5

- **Timestamp:** 2026-07-02T17-00
- **Task:** [P1-T7]

## Test run

- **Command:** `MSYS_NO_PATHCONV=1 "C:/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" "/Tests:ResolveControlGroups_WithHeadlessItemViewer_PopulatesConcreteControlCollections,WireControlTreeEvents_WithHeadlessItemViewer_WiresKeyboardAndMouseHandlers,WireEvents_WithHeadlessItemViewer_WiresBothControlTreeAndIntentEvents"`
- **EXIT_CODE:** 0
- **Output Summary:** Total tests: 3. Passed: 3. Failed: 0. All three new de-exemption tests pass, each genuinely constructing and exercising a real headless `ItemViewer` (no control-tree mocking).

## Exemption count

- **Command:** `grep -rnE "ExcludeFromCodeCoverage\]" QuickFiler/Controllers/QfcItemController*.cs UtilitiesCS/Threading/WpfUiDispatcher.cs QuickFiler/Viewers/WebView2CoreInitializer.cs QuickFiler/Interfaces/MailItemActionsAdapter.cs | wc -l`
- **Result:** 21 (24 baseline minus the 3 attributes removed in P1-T1/P1-T2/P1-T3).

## File-size check (≤ 500 lines)

| File | Lines |
|---|---:|
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 282 |
| `QuickFiler/Controllers/QfcItemController.EventWiring.cs` | 389 |
| `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` | 407 |
| `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs` | 374 |

All four files are within the 500-line cap.
