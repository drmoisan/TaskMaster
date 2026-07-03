# Baseline — 500-Line-Cap File Inventory (P0-T6)

Timestamp: 2026-06-29T10-52

Measured line counts (wc -l):
- QuickFiler/Controllers/QfcItemController.cs = 2498 (expected 2498) — OVER the 500-line cap; to be split in Phase 1.
- QuickFiler/Viewers/IItemViewer.cs = 73 (expected 73)
- QuickFiler/Viewers/ItemViewer.cs = 436 (expected 436)
- QuickFiler/Helper Classes/QfcThemeHelper.cs = 342
- QuickFiler/Controllers/QfcCollectionController.cs = 2296 (expected ~2300) — [ExcludeFromCodeCoverage]; pre-existing debt.
- QuickFiler.Test/Controllers/QfcItemControllerTests.cs = 377 (expected 377)

Disposition statements:
(a) QfcCollectionController.cs (2296 lines) is pre-existing debt receiving at most a net-neutral edit this cycle and is NOT to be split (Non-Goal per spec §Non-Goals). Its line-140 `grp.ItemViewer.LblItemNumber.Text` access stays on the concrete ItemViewer type.
(b) QfcItemControllerTests.cs (377 lines) is held net-neutral; all new tests are routed to new per-cluster test files (Phase 7), each under 500 lines.

Output Summary: All baseline counts recorded. Only QfcItemController.cs (2498) exceeds the 500-line cap and is the split target for Phase 1. QfcCollectionController.cs not-split disposition and QfcItemControllerTests.cs net-neutral disposition both recorded.
