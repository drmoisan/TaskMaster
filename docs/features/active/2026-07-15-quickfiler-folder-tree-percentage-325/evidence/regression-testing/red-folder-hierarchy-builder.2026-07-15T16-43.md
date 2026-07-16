# Red — FolderHierarchyBuilderTests (P3-T2) [expect-fail]

Timestamp: 2026-07-16T10-05
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:FullyQualifiedName~FolderHierarchyBuilderTests
EXIT_CODE: 1 (expected failure — red phase)

Output Summary: All 5 FolderHierarchyBuilderTests FAIL against the unimplemented `Build` (stub throws
NotImplementedException). Expected red-phase outcome before P3-T3.
Total tests: 5 | Failed: 5 | Passed: 0.
Failing tests:
- Build_MultiSegmentSuggestion_SynthesizesAncestorAndAttachesLeafProbability
- Build_SiblingSuggestions_ShareFindOrAddAncestor
- Build_NonSuggestionRows_AreDepthZeroLeavesWithNoProbability
- Build_MixedRows_PreservesInputOrderInForest
- Build_SingleSegmentSuggestion_IsDepthZeroLeafWithProbability
