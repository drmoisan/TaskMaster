# Phase 0 — AC13 Flag-Off Parity Regression Baseline (Cycle 3, #177)

Timestamp: 2026-06-16T01-04
Command: vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~FolderPredictorSeam_Tests"
EXIT_CODE: 0

Output Summary: Total tests 8, Passed 8, Failed 0. AC13 flag-off parity tests (in
UtilitiesCS.Test/EmailIntelligence/FolderPredictorSeam_Tests.cs) all pass at baseline:
- GetFolderPredictorAsync_FlagOff_ReturnsFlatManagerGroup — Passed
- GetFolderPredictorAsync_FlagOff_ClassifyUnchanged — Passed
- GetFolderPredictorAsync_FlagOff_TrainAndUnTrainAffectFlatGroup — Passed
- GetFolderPredictorAsync_FlagOff_FreshPerCallInstance_ReturnsFlat — Passed
(The remaining 4 tests in the class are flag-on / IFolderPredictor reachability tests, also passing.)
