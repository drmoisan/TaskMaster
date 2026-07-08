# Split Verification (Cycle 2 / F3)

Timestamp: 2026-06-12T17:02Z

Command:
- wc -l on both resulting files
- grep -oE "public void [A-Za-z_]+" on both files

EXIT_CODE: 0

Output Summary:

(a) Line-count cap (each <= 500):
- LcppnFolderPredictor_Tests.cs = 316 lines
- LcppnFolderPredictor_Classify_Tests.cs = 287 lines
Both are <= 500. (Original was 554, over cap.)

(b) Test-preservation — union of method names across both files equals the original
21-case set (14 in File A + 9 in File B = 23 method declarations; the two
[DataTestMethod]s — Config_InvalidMinimumPathProbability_Throws (4 rows),
Config_InvalidShrinkageLambda_Throws (2 rows) — remain in File A, accounting for the
21 distinct test cases). No test dropped, none renamed:

File A (14): Config_BeamWidthBelowOne_Throws, Config_Defaults_MatchSpecification,
Config_InvalidMinimumPathProbability_Throws, Config_InvalidShrinkageLambda_Throws,
Config_NegativeMinColdStartExamples_Throws, Train_Leaf_UpdatesOnlyPathClassifiers,
UnTrain_PriorLeaf_DecrementsOnlyPathClassifiers, Train_NewLeaf_ModifiesOnlyTargetParentClassifier,
TrainAndUnTrain_EmptyTag_AreNoOps, UnTrain_IntermediateParentMissing_SkipsMissingSegment,
LcppnFolderPredictor_IsAssignableToIFolderPredictor, Build_NullCorpus_Throws,
Build_SkipsEntriesWithEmptyRelativePathAndNullTokens, Build_NullConfig_Throws.

File B (9): Classify_ConstructedCorpus_ReturnsLeafWithPathProductProbability,
Classify_ConstructedCorpus_ResultsAreOrderedDescending,
Classify_WiderBeam_RecoversBranchGreedyWouldDiscard, Classify_BelowThreshold_ReturnsEmpty,
Classify_NoRootChildren_ReturnsEmpty, Classify_DeepWideHierarchy_TruncatesFrontierToBeamWidth,
Classify_FrontierNodeWithoutClassifier_EmitsTerminalLeaf,
Classify_FrontierNodeWithNoChildScores_EmitsTerminalLeaf,
Classify_FrontierExceedsBeamWidth_TrimsToBeamWidth.

(c) Concern partition: count of `Classify_*` in File A = 0; count of non-`Classify_*`
`public void` methods in File B = 0. Partition is clean.
