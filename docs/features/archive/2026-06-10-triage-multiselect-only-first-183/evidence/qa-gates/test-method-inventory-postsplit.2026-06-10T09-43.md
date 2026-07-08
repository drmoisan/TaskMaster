# Post-Split — Combined [TestMethod] Inventory (Remediation Cycle 1, Issue #183 R1)

Timestamp: 2026-06-10T09-43

Command: `Select-String -Path '<each file>' -Pattern '\[TestMethod\]' | Measure-Object`

EXIT_CODE: 0

## Output Summary

- `Triage_OlLogicTests.cs` (original file, post-split): 15 `[TestMethod]` methods.
- `Triage_OlLogicTests.TrainSelection.cs` (new partial file): 6 `[TestMethod]` methods.
- Combined `[TestMethod]` count: 15 + 6 = 21. This matches the corrected Phase 0 inventory (P0-T4).

### Six methods moved to Triage_OlLogicTests.TrainSelection.cs

1. `TrainSelectionAsync_ShouldTrainSelection`
2. `TrainSelectionAsync_WhenSelectionIsNull_SkipsWithoutThrowingOrTraining`
3. `TrainSelectionAsync_WhenSelectionContainsMailItem_TrainsClassifierWithExpectedLabel`
4. `TrainSelectionAsync_WhenSelectionContainsTwoMailItemsWithSameConversationId_TrainsOnlyOneItem_TotalEmailCountIncrementsOnce`
5. `TrainSelectionAsync_WhenSelectionContainsTwoMailItemsWithSameConversationId_TrainsOnlyOneItem_MatchEmailCountIncrementsOnce`
6. `TrainSelectionAsync_WhenSelectionContainsTwoMailItemsWithSameConversationId_WritesTriageUdfToEveryItem` (#183 regression)

All six moved verbatim (signatures, bodies, comments, assertions unchanged). `[TestInitialize] Setup()` and the shared fields `_mockGlobals`, `_triage`, `_triageOlLogic` remain in the original file and are shared via the partial class.
