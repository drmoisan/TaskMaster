# Code Review — folder-probability-plumbing (#324)

- Timestamp: 2026-07-16T00-19
- Feature branch: feature/folder-probability-plumbing-324 @ d9bfe081
- Base: origin/epic/folder-tree-percentage-ui-integration
- Scope: full branch diff

## Executive Summary

The change is a clean, additive contract with strong separation of concerns. The structural
ordering-parity mechanism (a single private `OrderedScores()` enumeration shared by the name-only
and scored projections) is the correct design: it guarantees ordering identity by construction
rather than relying on tests alone. New value types are immutable net48-safe `readonly struct`s with
thorough XML documentation. The row-model mirrors on FolderPredictor faithfully reproduce the legacy
string sequences while classifying rows by `Kind`, removing the need for downstream
`.StartsWith("====")` string matching. Test coverage of the new members is high and exercises the
required edge branches. Code quality is acceptable for merge. One non-blocking file-size deviation
and two minor observations are recorded.

Blocking findings: 0.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Major (non-blocking) | UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs; FolderScorer.cs | whole file | Files exceed the 500-line limit (974 / 663) after this additive change; both were already over at baseline (823 / 617). | Track partial-class extraction as tech-debt; do not block this additive feature. | Pre-existing overage; new members are cohesive instance methods over private state; refactor risks the byte-for-byte compat guarantee. | git baseline vs head line counts; general-code-change.md file-size rule. |
| Minor (non-blocking) | UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs | BuildScoredArray, `maxScore = ordered[0].Value` and `Probability = Score / maxScore` | Max-normalization assumes non-negative scores. If a negative accumulated score ever occurred, Probability could fall outside [0,1]. | Optionally clamp or document the non-negative-score precondition. | All three sources accumulate non-negative weighted integers / probability*1000; negative scores are not a realistic input today. | FolderScorer.cs diff; spec Probability Semantics section. |
| Minor (non-blocking) | UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs | FindFolderRows recalcSuggestions block | One line (closing brace of the recalcSuggestions block) is uncovered (95.7% line); the branch itself is exercised. | No action required; branch coverage is 100%. | The uncovered line is a non-behavioral brace; the recalc branch is tested via FindFolderRows_WithRecalcSuggestionsAndUnresolvableItem_ThrowsArgumentException. | evidence/qa-gates/coverage-delta.md. |

## Design and Correctness Notes

- Structural parity: `OrderedScores()` returns
  `_folderNameScores.OrderByDescending(x => x.Value).ThenBy(x => x.Key, StringComparer.Ordinal)`.
  `ToArray*` project `.Select(x => x.Key)`; `ToScoredArray*` route through `BuildScoredArray`, which
  computes `maxScore` once over the full ordered set before applying any `topN`. This makes
  per-folder `Probability` stable regardless of `topN`, as the spec requires, and guarantees ordering
  identity with the name-only outputs.
- Zero-guard: `BuildScoredArray` returns `Array.Empty<FolderScore>()` for an empty scorer and sets
  `Probability = 0` when `maxScore == 0`; no divide-by-zero path exists.
- Non-mutation: FolderRowArray and FindFolderRows build a local `List<FolderRow>` and never touch the
  cached `_folderList`, so the legacy FolderArray/FindFolder outputs are unaffected. FindFolderRows
  mirrors FindFolder's unconditional AddSuggestions call and its recalcSuggestions gate faithfully.
- Row classification: separators, search results, suggestions, and recents are tagged by
  `FolderRowKind`; only Suggestion rows carry a non-null `Score`. `Text` equals the legacy string
  byte-for-byte, so consumers may adopt the new contract incrementally.
- Tests: MSTest + Moq + FluentAssertions, AAA structure, deterministic, no temp files. The golden
  baseline in FolderScorerRegressionTests hard-codes expected ordering including the 850-score tie
  (Finance before HR ordinally), which locks the tie-break behavior.

## Verdict

Code quality: PASS. No blocking findings; three documented non-blocking items.
