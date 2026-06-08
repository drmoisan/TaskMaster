# P1-T2 Evidence: Wrong Test Removed, Correct Regression Test Added

Timestamp: 2026-04-21T12:45:00
File modified: UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogicTests.cs

## Test removed
- `TrainSelectionAsync_WhenSelectionContainsTwoMailItems_MatchEmailCountForLabelIncrementsByTwo`

## Test method name added
- `TrainSelectionAsync_WhenSelectionContainsTwoMailItems_TrainsOnlyFirstItem_MatchEmailCountIncrementsOnce`
