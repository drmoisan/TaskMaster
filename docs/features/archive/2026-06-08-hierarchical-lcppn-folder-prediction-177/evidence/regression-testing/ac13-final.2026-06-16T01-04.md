# Phase 5 — AC13 Flag-Off Parity Final Re-Verification (Cycle 3, #177)

Timestamp: 2026-06-16T01-04
Command: vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~FolderPredictorSeam_Tests.GetFolderPredictorAsync_FlagOff"
EXIT_CODE: 0

Output Summary: Total tests 4, Passed 4, Failed 0. AC13 flag-off flat-parity remains green after all
cycle-3 changes:
- GetFolderPredictorAsync_FlagOff_ReturnsFlatManagerGroup — Passed
- GetFolderPredictorAsync_FlagOff_ClassifyUnchanged — Passed
- GetFolderPredictorAsync_FlagOff_TrainAndUnTrainAffectFlatGroup — Passed
- GetFolderPredictorAsync_FlagOff_FreshPerCallInstance_ReturnsFlat — Passed

These tests were not modified; flag-off (the persisted setting OFF, resolved through the mocked
IAppAutoFileObjects.UseLcppnPredictor returning the Moq default false) returns the same flat
BayesianClassifierGroup instance from Manager["Folder"], byte-for-byte unchanged. The new
ToggleOff_ResolvesFlatOnly_PreservingAc13 and ExplicitConfig_OverridesPersistedDefault tests
(FolderPredictorSeam_DefaultOn_Tests) additionally confirm OFF restores flat-only selection.
