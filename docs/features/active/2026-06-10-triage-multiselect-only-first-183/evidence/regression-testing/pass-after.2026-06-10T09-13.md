# Regression — Pass-After (Issue #183, AC2/AC3)

Timestamp: 2026-06-10T09-13

Command (canonical): `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll`
Command (executed): `MSYS_NO_PATHCONV=1 "C:/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe" "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /InIsolation /TestCaseFilter:"FullyQualifiedName~Triage_OlLogic"`

EXIT_CODE: 0

## Output Summary

After applying the minimal decoupling fix in `Triage_OlLogic.TrainSelectionAsync`, the full Triage_OlLogic test set passes.

- Total tests: 22; Passed: 22; Failed: 0.
- The new regression test now PASSES:
  - `TrainSelectionAsync_WhenSelectionContainsTwoMailItemsWithSameConversationId_WritesTriageUdfToEveryItem` — PASS (both `mockMailItem1.Save()` and `mockMailItem2.Save()` invoked once; `TotalEmailCount` incremented by exactly 1).
- The four pre-existing tests all PASS unchanged:
  - `TrainSelectionAsync_WhenSelectionContainsTwoMailItemsWithSameConversationId_TrainsOnlyOneItem_TotalEmailCountIncrementsOnce` — PASS
  - `TrainSelectionAsync_WhenSelectionContainsTwoMailItemsWithSameConversationId_TrainsOnlyOneItem_MatchEmailCountIncrementsOnce` — PASS
  - `TrainSelectionAsync_WhenSelectionContainsMailItem_TrainsClassifierWithExpectedLabel` — PASS
  - `TrainSelectionAsync_WhenSelectionIsNull_SkipsWithoutThrowingOrTraining` — PASS

This confirms AC1 (UDF written to every same-conversation item), AC2 (training still dedups by ConversationID — TotalEmailCount/MatchEmailCount increment exactly once), and AC3 (deterministic MSTest regression test passes; #137 training-dedup tests unchanged).
