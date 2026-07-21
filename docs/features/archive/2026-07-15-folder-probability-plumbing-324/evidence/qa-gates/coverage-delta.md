# Coverage Delta and Threshold Verification (AC12)

Timestamp: 2026-07-16T03-32

Regime applied: the stricter of CLAUDE.md (80% floor, >= 90% for new members) and
`.claude/rules/general-unit-test.md` (>= 85% line, >= 75% branch). Target: >= 90% line on all new
members, branch coverage of the empty / all-zero / tie / topN paths, no reduction on changed lines,
no production file excluded.

## Baseline vs Post-Change (readable Cobertura, dotnet-coverage)

| Scope | Baseline line | Post-change line | Baseline branch | Post-change branch | Verdict |
|---|---|---|---|---|---|
| Repository (merged, all assemblies) | 59.35% | 59.42% | 30.28% | 30.37% | No regression (slight increase) |
| FolderScorer (primary class) | 97.75% | 97.85% | 94.20% | 94.20% | No reduction; improved |
| FolderPredictor (primary class) | 86.71% | 88.86% | 86.27% | 88.28% | No reduction; improved |
| FolderScore.cs (new file) | n/a | 100% | n/a | 100% | New member >= 90% |
| FolderRow.cs (new file) | n/a | 100% | n/a | 100% | New member >= 90% |

Baseline figures: evidence/baseline/baseline-vstest-coverage.md. Post-change figures:
evidence/qa-gates/qc-vstest-coverage.md.

## New / Changed Member Line Coverage (all >= 90%)

New members:
- FolderScore struct (ctor + FolderPath/Score/Probability): 100%.
- FolderRow struct + FolderRowKind enum: 100%.
- FolderScorer.OrderedScores(): 100% (1/1).
- FolderScorer.ToScoredArray(): 100% (1/1).
- FolderScorer.ToScoredArray(int): 100% (1/1).
- FolderScorer.BuildScoredArray(): 100% (11/11), branch 100%.
- FolderPredictor.FolderRowArray (get): 100% (12/12).
- FolderPredictor.FindFolderRows(): 95.7% (22/23), branch 100%. (The single uncovered line is the
  closing brace of the recalcSuggestions block; the branch itself is covered by
  FindFolderRows_WithRecalcSuggestionsAndUnresolvableItem_ThrowsArgumentException.)
- FolderPredictor.AddMatchRows(): 100% (13/13).
- FolderPredictor.AddSuggestionRows(): 100% (9/9).
- FolderPredictor.AddRecentRows(): 100% (16/16).

Changed members (refactored to route through OrderedScores()):
- FolderScorer.ToArray() and ToArray(int): covered within the 97.85% FolderScorer class rate;
  golden-baseline regression tests lock their output unchanged.

## Branch-Path Coverage (empty / all-zero / tie / topN)

- Empty scorer: FolderScoreTests.ToScoredArray_EmptyScorer_ReturnsEmptyArrayWithoutDivideByZero
  (Array.Empty path in BuildScoredArray).
- All-zero seeds: FolderScoreTests.ToScoredArray_AllZeroSeeds_YieldsZeroProbabilityForEveryRow
  (maxScore == 0 zero-guard path).
- Tie (ordinal tie-break): FolderScorerRegressionTests.ToScoredArray_WithTie_PreservesIdenticalOrdinalTieBreakAsToArray
  and ToArray_WithPopulatedScorer_ReturnsGoldenOrderingWithOrdinalTieBreak.
- topN: ToScoredArrayTopN_WhenTopNExceedsCount_ReturnsAllRows and
  ToScoredArrayTopN_FolderPathOrdering_EqualsToArrayTopNOrdering (topN < 0 vs Take(topN) branches).
BuildScoredArray branch-rate = 100% confirms these paths are exercised.

## Exclusion Check

No production file is excluded from measurement. FolderScore.cs and FolderRow.cs are both present in
the Cobertura report with measured coverage; no [ExcludeFromCodeCoverage] attribute or coverage.config
exclude was added by this feature.

## Verdict

AC12 MET: every new member is >= 90% line; branch coverage of empty/all-zero/tie/topN paths is
present (BuildScoredArray branch 100%); no changed-line coverage reduction (both touched classes
improved); no production file excluded. Outcome: PASS.
