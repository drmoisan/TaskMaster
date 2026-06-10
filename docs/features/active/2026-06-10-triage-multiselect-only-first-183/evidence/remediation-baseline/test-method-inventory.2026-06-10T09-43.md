# Baseline — [TestMethod] Inventory of Triage_OlLogicTests.cs (Remediation Cycle 1, Issue #183 R1)

Timestamp: 2026-06-10T09-43

Command: `Select-String -Path 'UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogicTests.cs' -Pattern '\[TestMethod\]'`

EXIT_CODE: 0

## Output Summary

- Total `[TestMethod]`-decorated methods: 21 (matches expected count). The `[TestInitialize] Setup()` method is excluded — it is not a test method.

### Full inventory (21 test methods)

1. `Constructor_ShouldInitializeParent` (line 37) — STAYS
2. `FilterViewAsync_ShouldCallFilterView` (line 43) — STAYS
3. `FilterView_ShouldCallFilterViewWithTriageValues` (line 70) — STAYS
4. `FilterView_WithTriageValues_ShouldApplyFilter` (line 99) — STAYS
5. `ParseAndStripFilter_ShouldReturnStrippedFilter` (line 122) — STAYS
6. `ParseAndStripFilter_ShouldReturnStrippedFilter2` (line 133) — STAYS
7. `TrainSelectionAsync_ShouldTrainSelection` (line 153) — MOVE
8. `ParseAndStripFilter_WithEmptyString_ShouldReturnEmpty` (line 161) — STAYS
9. `ParseAndStripFilter_WithNoTriageReferences_ShouldReturnOriginal` (line 168) — STAYS
10. `ParseAndStripFilter_WithSingleTriageEquals_ShouldRemoveIt` (line 176) — STAYS
11. `StripFilter_WithNullParent_ShouldReturnNull` (line 186) — STAYS
12. `StripFilter_WithNoMatch_ShouldReturnOriginalTree` (line 197) — STAYS
13. `StripFilter_WithMatchAndParent_ShouldRemoveNode` (line 208) — STAYS
14. `FilterView_WithEmptyTriageValues_ShouldNotThrow` (line 224) — STAYS
15. `FilterView_WhenExplorerIsNull_ShouldReturnGracefully` (line 245) — STAYS
16. `ParseAndStripFilter_WithUnsupportedAndSupportedClauses_StripsTriagePreservesSupported` (line 261) — STAYS
17. `TrainSelectionAsync_WhenSelectionIsNull_SkipsWithoutThrowingOrTraining` (line 281) — MOVE
18. `TrainSelectionAsync_WhenSelectionContainsMailItem_TrainsClassifierWithExpectedLabel` (line 308) — MOVE
19. `TrainSelectionAsync_WhenSelectionContainsTwoMailItemsWithSameConversationId_TrainsOnlyOneItem_TotalEmailCountIncrementsOnce` (line 360) — MOVE
20. `TrainSelectionAsync_WhenSelectionContainsTwoMailItemsWithSameConversationId_TrainsOnlyOneItem_MatchEmailCountIncrementsOnce` (line 413) — MOVE
21. `TrainSelectionAsync_WhenSelectionContainsTwoMailItemsWithSameConversationId_WritesTriageUdfToEveryItem` (line 476, #183 regression) — MOVE

### Split summary

- STAYS in original file: 15 `[TestMethod]` methods + `[TestInitialize] Setup()`.
- MOVES to `Triage_OlLogicTests.TrainSelection.cs`: 6 `TrainSelectionAsync_*` methods.
- Total preserved: 15 + 6 = 21. This inventory is the verbatim-preservation reference for Phase 2.
