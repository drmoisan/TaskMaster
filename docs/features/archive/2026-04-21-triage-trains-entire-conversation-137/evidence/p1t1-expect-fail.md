# P1-T1 Evidence: Wrong Tests Removed, Correct Regression Test Added

Timestamp: 2026-04-21T12:45:00
File modified: UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogicTests.cs

## Tests removed
- `TrainSelectionAsync_WhenSelectionContainsTwoMailItems_TotalEmailCountIncrementsByExactlyTwo`

## Test method names added
- `TrainSelectionAsync_WhenSelectionContainsTwoMailItems_TrainsOnlyFirstItem_TotalEmailCountIncrementsOnce`
