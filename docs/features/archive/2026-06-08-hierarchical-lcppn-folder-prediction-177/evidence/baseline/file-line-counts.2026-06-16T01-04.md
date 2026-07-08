# Phase 0 — Baseline File Line Counts (Cycle 3, #177)

Timestamp: 2026-06-16T01-04
Command: wc -l <each in-scope file>
EXIT_CODE: 0

Output Summary (per-file line counts at cycle-3 entry, head eebcc910):

| Lines | File |
|------:|------|
| 121 | UtilitiesCS/EmailIntelligence/Bayesian/LcppnFolderPredictorConfig.cs |
| 317 | UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/OlFolderClassifierGroup.cs |
| 363 | UtilitiesCS/EmailIntelligence/Bayesian/LcppnFolderPredictor.cs |
| 847 | TaskMaster/AppGlobals/AppAutoFileObjects.cs (already over 500 cap — pre-existing) |
| 453 | UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs |
| 343 | UtilitiesCS/EmailIntelligence/ClassifierGroups/ManagerAsyncLazy.cs |
| 608 | UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs (already over 500 cap — pre-existing) |
| 1406 | UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs (already over 500 cap — pre-existing) |
| 515 | UtilitiesCS/EmailIntelligence/Bayesian/BayesianClassifierGroup.cs (already over 500 cap — pre-existing) |
| 285 | UtilitiesCS.Test/EmailIntelligence/FolderPredictorSeam_Tests.cs |
| 192 | UtilitiesCS.Test/EmailIntelligence/Bayesian/LcppnFolderPredictor_Serialization_Tests.cs |
| 287 | UtilitiesCS.Test/EmailIntelligence/Bayesian/LcppnFolderPredictor_Classify_Tests.cs |
| 316 | UtilitiesCS.Test/EmailIntelligence/Bayesian/LcppnFolderPredictor_Tests.cs |

Note: the plan referenced the seam test file as
`UtilitiesCS.Test/EmailIntelligence/FolderPredictorSeam_Tests.cs`; the actual repository path is
`UtilitiesCS.Test/EmailIntelligence/FolderPredictorSeam_Tests.cs` (confirmed). The file exists and
contains the AC13 flag-off parity tests (`GetFolderPredictorAsync_FlagOff_*`).

Confirmed expected counts from the plan: AppAutoFileObjects.cs=847, FolderScorer.cs=608,
SortEmail.cs=1406, BayesianClassifierGroup.cs=515.
