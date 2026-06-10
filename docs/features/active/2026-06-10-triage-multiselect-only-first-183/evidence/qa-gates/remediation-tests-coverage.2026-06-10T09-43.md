# Remediation QA — Tests with Coverage (Cycle 1, Issue #183 R1)

Timestamp: 2026-06-10T09-43

Command (canonical): `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage`
Command (executed, Moq-isolation form):
`MSYS_NO_PATHCONV=1 "<VS18>/Common7/IDE/Extensions/TestPlatform/vstest.console.exe" "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /InIsolation /EnableCodeCoverage`
Coverage conversion: `Microsoft.CodeCoverage.Console.exe merge <.coverage> -f xml -o coverage-post-remediation.xml`

EXIT_CODE: 1 (full assembly — one PRE-EXISTING unrelated failure, identical to baseline; see below)

## Output Summary

### Targeted Triage_OlLogic tests (`/TestCaseFilter:FullyQualifiedName~Triage_OlLogic`)
- All 21 `Triage_OlLogicTests` methods PASS (15 in `Triage_OlLogicTests.cs` + 6 in `Triage_OlLogicTests.TrainSelection.cs`).
- The targeted run reports 22 passed because the substring filter `~Triage_OlLogic` also matches one test in another class (`FilterView_WithJetFilter_AppendsParenthesizedTriageClause`); that test also passes. The 21 in-scope partial-class methods are all Passed:
  - Original file (15): Constructor_ShouldInitializeParent; FilterViewAsync_ShouldCallFilterView; FilterView_ShouldCallFilterViewWithTriageValues; FilterView_WithTriageValues_ShouldApplyFilter; ParseAndStripFilter_ShouldReturnStrippedFilter; ParseAndStripFilter_ShouldReturnStrippedFilter2; ParseAndStripFilter_WithEmptyString_ShouldReturnEmpty; ParseAndStripFilter_WithNoTriageReferences_ShouldReturnOriginal; ParseAndStripFilter_WithSingleTriageEquals_ShouldRemoveIt; StripFilter_WithNullParent_ShouldReturnNull; StripFilter_WithNoMatch_ShouldReturnOriginalTree; StripFilter_WithMatchAndParent_ShouldRemoveNode; FilterView_WithEmptyTriageValues_ShouldNotThrow; FilterView_WhenExplorerIsNull_ShouldReturnGracefully; ParseAndStripFilter_WithUnsupportedAndSupportedClauses_StripsTriagePreservesSupported.
  - Moved file (6): TrainSelectionAsync_ShouldTrainSelection; TrainSelectionAsync_WhenSelectionIsNull_SkipsWithoutThrowingOrTraining; TrainSelectionAsync_WhenSelectionContainsMailItem_TrainsClassifierWithExpectedLabel; TrainSelectionAsync_WhenSelectionContainsTwoMailItemsWithSameConversationId_TrainsOnlyOneItem_TotalEmailCountIncrementsOnce; TrainSelectionAsync_WhenSelectionContainsTwoMailItemsWithSameConversationId_TrainsOnlyOneItem_MatchEmailCountIncrementsOnce; TrainSelectionAsync_WhenSelectionContainsTwoMailItemsWithSameConversationId_WritesTriageUdfToEveryItem (#183 regression).

### Full UtilitiesCS.Test assembly (with coverage)
- Total tests: 3815; Passed: 3814; Failed: 1.
- The single failing test is `AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException`. It is the PRE-EXISTING, unrelated UI-thread/dispatcher timing test identified in remediation-inputs and the baseline (baseline EXIT_CODE was also 1 for this same single failure). It is the ONLY allowed pre-existing failure.
- Confirmation that it is pre-existing/timing-sensitive and unchanged: a re-run without coverage reported 3815 passed / 0 failed, and an isolated run of the test (`/TestCaseFilter:FullyQualifiedName~AddEntry_UseUiThreadTrue`) PASSED (1/1). The failure is a parallel-run timing artifact in the queue/dispatcher area, not in `Triage_OlLogic` and not touched by this remediation. Status unchanged from baseline.
- No in-scope test failed. No toolchain restart required.

### Coverage headline (post-remediation, first-party UtilitiesCS.dll)
- `UtilitiesCS.dll`: lines_covered=35057, lines_not_covered=5134 -> 87.23% line coverage (baseline-comparable metric: 35057 / (35057 + 5134) = 87.23%).
- Tool-reported `line_coverage` attribute = 85.33% (this attribute treats lines_partially_covered=893 as not-fully-covered; baseline reported the covered/not-covered ratio of 87.23% and that metric is preserved).
- >= 80% first-party repo gate satisfied.

Coverage XML: docs/features/active/2026-06-10-triage-multiselect-only-first-183/evidence/qa-gates/coverage-post-remediation.xml
