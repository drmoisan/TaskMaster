# Fail-Before Exception Dossier — folder-probability-plumbing (#324)

Timestamp: 2026-07-16T03-32

## WhyFailingRunImpossible

This feature is a pure additive contract with no defect to reproduce. A meaningful runtime
"fail-before" run cannot be produced for two reasons:

1. The new-member tests (`FolderScoreTests`, `FolderScorerRegressionTests` scored-projection
   assertions, `FolderRowTests`) reference symbols that do not exist in the baseline —
   `FolderScore`, `FolderScorer.ToScoredArray`, `FolderRow`, `FolderRowKind`,
   `FolderPredictor.FolderRowArray`, and `FolderPredictor.FindFolderRows`. Against the pre-change
   tree these tests cannot compile (CS0246/CS0117), so they cannot be executed to produce a runtime
   red result. A non-compiling test assembly is not a valid fail-before signal.

2. The regression/characterization tests that CAN compile against the baseline
   (`ToArray_WithPopulatedScorer_ReturnsGoldenOrderingWithOrdinalTieBreak`,
   `ToArrayTopN_WithPopulatedScorer_ReturnsGoldenTopNSlice`, and the `FolderArray` / `FindFolder`
   Text-parity portions) are characterization tests. By design they must pass BOTH before and after
   the change, because the whole point of the feature is that the existing name-only outputs
   (`ToArray`, `ToArray(int)`, `FolderArray`, `FindFolder`) are preserved byte-for-byte. A
   characterization test that failed before the change would indicate the baseline behavior was
   already broken, which is not the case.

This is therefore an additive-contract feature, not a bug fix; the repository bugfix "failing test
first" workflow does not apply. This dossier satisfies the fail-before requirement per
`evidence-and-timestamp-conventions` (fail-before exception dossier accepted in lieu of a failing
run), and no `[expect-fail]` task is present in the plan.

## Alternative Proof (no behavior change on existing outputs)

The golden-baseline characterization tests demonstrate no behavior change on the protected existing
outputs:

- `FolderScorerRegressionTests.ToArray_WithPopulatedScorer_ReturnsGoldenOrderingWithOrdinalTieBreak`
  and `...ToArrayTopN_WithPopulatedScorer_ReturnsGoldenTopNSlice` lock `ToArray()` / `ToArray(int)`
  ordering and content, including the ordinal tie-break, and PASS against the refactored code
  (verified 2026-07-16).
- `FolderRowTests.FolderRowArray_WithSuggestionsAndRecents_MatchesFolderArrayTextAndTagsKinds`,
  `...FolderRowArray_DoesNotAlterFolderArrayOutput`, and
  `...FindFolderRows_WithMatchesSuggestionsAndRecents_MatchesFindFolderTextAndTagsKinds` prove the
  new row projections' `Text` sequence equals the legacy `FolderArray` / `FindFolder` output
  byte-for-byte, and that reading the new members does not mutate the cached `_folderList`.
- The structural parity mechanism (the shared private `FolderScorer.OrderedScores()` enumeration
  consumed by both `ToArray*` and `ToScoredArray*`) guarantees ordering identity by construction,
  not merely by test.

Result: all 14 Layer-1 tests and 3 Layer-2 tests pass against the changed code; the pre-existing
`FolderScorerTests` / `FolderPredictorTests` suites remain green. No existing behavior regressed.

## Evidence Schema

- Timestamp: 2026-07-16T03-32
- Command: "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:FullyQualifiedName~FolderScoreTests|FullyQualifiedName~FolderScorerRegressionTests|FullyQualifiedName~FolderRowTests
- EXIT_CODE: 0
- SearchScope: docs/features/active/2026-07-15-folder-probability-plumbing-324/evidence/regression-testing/
- SearchPatterns: fail-before-exception.*.md
- SearchResult: this dossier (fail-before-exception.2026-07-15T16-52.md)
