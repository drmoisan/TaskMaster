# Batch C — DO-NOT-ALTER Constraint Verification (Scoring Engine Core)

Timestamp: 2026-07-19T02-00

Scope: T1 scoring engine core. Per-region confirmation that no scoring/corpus/probability math changed, no `Math.Max`/`Math.Min`/division/log/exp expression reordered, no `KnobList`/`LaplaceAlpha` constant altered, and no new `if (x is null) throw` guard was added on any scoring path.

## BayesianClassifierShared.cs (7 CS86xx closed)
- `#nullable enable`.
- `_parent` field `= null!` and `_tag` field `= null!` (both set via property/deserialization or by functional constructors; the compiler does not treat property-setter assignment as field init, hence CS8618 on the parameterless/partial constructors). No hot-path guard added; `Parent`/`Tag` remain non-null typed so `Train`/`UnTrain` hot paths are untouched.
- Three `Chi2SpamProb(...)` overloads' return type second element annotated nullable: `(double, List<(string word, double prob)>?)`, reflecting the existing `return (prob, null)` on the non-evidence path (line ~886). The chi-squared math (`chi2Q`, `S`/`H` products, `Math.Log`, `frexp`, `Math.Min(sum, 1.0)`) is byte-for-byte unchanged.
- `GetWordInfo` return annotated `WordInfo?` (it already `return null` when `m + nm == 0`); the `if (record is null)` branch in `GetWordDistance` is unchanged. `GetWordDistance` local and return-tuple `record` element and `GetClues` list/return `record` element annotated `WordInfo?` to carry the nullable through (consumers destructure but never dereference `record`). `UpdateProbabilitySb(record)` is called only in the `else` (non-null) branch — no `!` needed, no guard added.

DO-NOT-ALTER regions confirmed unchanged: Paul Graham/Robinson `UpdateProbability*`, `CombineProbabilities`, `Chi2SpamProb` arithmetic, `chi2Q`, `GetClues` selection/ordering (`OrderByDescending`/`Take(Knobs.MaxDiscriminators)`), `GetWordDistance` distance math, `KnobList` constants, `Train`/`UnTrain` count paths.

## BayesianClassifierGroup.cs (1 CS86xx closed)
- `#nullable enable`; `Globals` auto-property initialized `= null!` (injected after construction; keeps non-null posture at the `Tokenize`/`TokenizeAsync` hot-path call sites). No behavior change.

## PerParentClassifier.cs (1 CS86xx closed)
- `#nullable enable`; `BayesianClassifierGroup? group = null` default parameter (the existing `_group = group ?? new BayesianClassifierGroup()` already handles null). `ShrinkageLambda`/`MinColdStartExamples`/`ScoreChildren`/`ChildLogScore`/`LaplaceProbability`/`Normalize` untouched.

## FolderHierarchyTree.cs (3 CS86xx closed)
- `#nullable enable`; two `StringComparer? comparer = null` default parameters (ctor + `Build`; both use `comparer ?? StringComparer.Ordinal`); `GetNode` return annotated `FolderHierarchyNode?` (it already `return null` when the key is absent). No callers elsewhere in EmailIntelligence; dictionary `TryGetValue` flow unchanged.

Confirmation:
- No `System.Diagnostics.CodeAnalysis` post-condition attribute added.
- No arithmetic, comparison, constant, clamp, ordering, or control flow changed in any DO-NOT-ALTER region (AC3).
- Existing `probabilities is null` / `tokens is null` / `record is null` guards remain as-is; no new throwing guard added.
- Base/override virtual signatures (`UpdateProbability*`) unchanged, preserving the `SubBayesianClassifier`/`SubClassifierGroup`/`SubCorpus` test-double contracts and avoiding CS8765/CS8767 (AC5).
- The `BayesianClassifierShared.cs` (1008 lines) and `BayesianClassifierGroup.cs` (515 lines) files were NOT split.
