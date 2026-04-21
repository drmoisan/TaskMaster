# P1-T3 Evidence: Regression Confirmed (Both New Tests FAIL Before Fix)

Timestamp: 2026-04-21T12:50:00
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~TrainSelectionAsync_WhenSelectionContainsTwoMailItems"
EXIT_CODE: 1

## Output Summary

Total tests: 2
Failed: 2

### FAILED: TrainSelectionAsync_WhenSelectionContainsTwoMailItems_TrainsOnlyFirstItem_TotalEmailCountIncrementsOnce
- Error: Expected _triage.ClassifierGroup.TotalEmailCount to be 1, but found 2.
- Confirms: pre-fix code trains both items in the selection (increments by 2), but assertion expects +1.

### FAILED: TrainSelectionAsync_WhenSelectionContainsTwoMailItems_TrainsOnlyFirstItem_MatchEmailCountIncrementsOnce
- Error: Expected _triage.ClassifierGroup.Classifiers["A"].MatchEmailCount to be 1, but found 2.
- Confirms: pre-fix code trains both items in the selection (increments by 2), but assertion expects +1.

Regression reproduced. Both tests fail before the fix is applied. Proceeding to P1-T4.
