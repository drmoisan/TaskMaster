# Phase 0 — Baseline Exemption and Real-ItemViewer Counts

Timestamp: 2026-08-26T08-39
Task: [P0-T16]

## Count 1 — `ExcludeFromCodeCoverage` occurrences in the four owned production files

Command: `grep -c "ExcludeFromCodeCoverage" <the four owned production files>`
EXIT_CODE: 0

| File | Expected | Measured |
|---|---|---|
| `QuickFiler/Controllers/QfcItemController.EventWiring.cs` | 1 | **1** |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 2 | **2** |
| `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` | 0 | **0** |
| `QuickFiler/Controllers/QfcItemController.MailActions.cs` | 0 | **0** |
| **Total** | **3** | **3** |

This total and distribution are what constraint C5 requires to be unchanged at `[P7-T11]`.

## Count 2 — real `QuickFiler.ItemViewer` constructions in the five owned test files

Command: `grep -n "new QuickFiler.ItemViewer\|new ItemViewer(" <the five owned test files>`
EXIT_CODE: 0

```
QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs:236:                var viewer = new QuickFiler.ItemViewer();
QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs:327:                var viewer = new QuickFiler.ItemViewer();
QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs:395:                var viewer = new QuickFiler.ItemViewer();
QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs:433:                        new QuickFiler.ItemViewer()
```

| File | Expected | Measured | Lines |
|---|---|---|---|
| `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs` | 2 | **2** | 236, 327 |
| `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` | 2 | **2** | 395, 433 |
| `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` | 0 | **0** | — |
| `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` | (not enumerated; measured) | **0** | — |
| `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs` | (not enumerated; measured) | **0** | — |
| **Total** | **4** | **4** | |

`[P7-T10]` requires this total to be exactly 5 after the change: the baseline 4 plus the single
construction inside `UnwireControlTreeEvents_WithHeadlessItemViewer_DetachesKeyboardAndMouseHandlers`.

Output Summary: Both baselines match the plan's expectations exactly. Coverage exemptions: EventWiring 1,
ViewerSetup 2, FocusAndTheme 0, MailActions 0, total 3. Real `ItemViewer` constructions: EventWiringTests
2, ViewerSetupTests 2, TestSupport 0, total 4.
