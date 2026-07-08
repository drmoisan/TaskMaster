# Phase 4 — Test-File Size Cap (Cycle 3, #177)

Timestamp: 2026-06-16T01-04
Command: wc -l <new/modified test files>
EXIT_CODE: 0

Output Summary (all <= 500):
- UtilitiesCS.Test/EmailIntelligence/FolderPredictorSeam_Tests.cs: 285 (unmodified; existing AC13/AC14 tests)
- UtilitiesCS.Test/EmailIntelligence/FolderPredictorSeam_DefaultOn_Tests.cs (new): 168 (AC21/AC22)
- UtilitiesCS.Test/EmailIntelligence/LcppnFolderPredictorStore_Tests.cs (new): 101 (AC23 store + round-trip)
- TaskMaster.Test/AppGlobals/AppAutoFileObjectsFolderPredictorTests.cs (new): 161 (AC23 load-path + fail-soft)
- UtilitiesCS.Test/EmailIntelligence/Bayesian/LcppnFolderPredictor_Serialization_Tests.cs: 192 (unmodified)

New tests split across dedicated sibling files to stay well under the 500-line cap. FolderPredictorSeam_Tests.cs
(285) was left unmodified so the existing AC13 flag-off parity tests remain present and unweakened (P4-T7).
