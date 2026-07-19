# utilitiescs-nullable-email-classifier — Research Findings

- **Issue:** #372
- **Parent:** Epic `utilitiescs-nullable-remediation` (Wave 1)
- **Upstream contract dependency:** #363 (`utilitiescs-nullable-extensions`, Wave 0)
- **Integration branch:** `epic/utilitiescs-nullable-remediation-integration` (tip `f377c856a84958f865c72d194634eedaa3e6db02`)
- **Author:** task-researcher
- **Timestamp:** 2026-07-18T21-30
- **Status:** Research only (no plan, no spec, no source change)

Scope: NULL-ANNOTATION and NULL-SAFETY remediation ONLY under a per-file `#nullable enable`
opt-in architecture across three clusters of the EmailIntelligence classifier stack. STRICTLY NO
change to classifier scoring, model logic, corpus/probability math, or any observable behavior.
These are T1 (Critical) classifier-engine modules per `.claude/rules/quality-tiers.md`; existing
golden/property/characterization tests must remain unchanged.

---

## 0. Upstream Contract Dependency (#363) — What This Feature Consumes

Sources read and cited:
- `docs/features/active/utilitiescs-nullable-extensions/spec.md`
- `docs/features/active/utilitiescs-nullable-extensions/plan.2026-07-18T21-20.md`
- `UtilitiesCS/Extensions/NullExtensions.cs` (already `#nullable enable`; verify-only in #363)

The classifier cluster consumes the following #363-annotated extension methods. Annotation choices
in this feature must HONOR these published contracts rather than re-deriving them:

| Extension method | #363 source file / batch | Consumed by (in-scope cluster files) |
|---|---|---|
| `ThrowIfNull<T>(this T?) where T : notnull → T` | `NullExtensions.cs` (verify-only) | `MulticlassEngine`, `TristateEngine`, `ConditionalItemEngine` |
| `ThrowIfNullOrEmpty` (IEnumerable/ICollection/string overloads) | `NullExtensions.cs` (verify-only) | `MulticlassEngine` (`EngineName`, `collection`) |
| `IsNullOrEmpty<T>(this IEnumerable<T>)` / string | `NullExtensions.cs` / `StringExtensions.cs` (Batch B) | `MulticlassEngine.GetOlItemString`, `SpamBayes.Conditions.GetOlItemString` |
| `ForEach` (enumerable / dictionary) | `IEnumerableExtensions.cs` (Batch C) | `Corpus`, `BayesianClassifierShared`, `PerParentClassifier`, `MulticlassEngine` |
| `GroupAndCount` / `GroupAndCountAsync` | `IEnumerableExtensions.cs` (Batch C) | `BayesianClassifierShared`, `SpamBayes`, `MulticlassEngine` |
| `ToDictionary` (custom no-arg overload) | `IEnumerableExtensions.cs` (Batch C) | `BayesianClassifierShared`, `MulticlassEngine` |
| `UpdateOrRemove(..., out TValue?)` | `DictionaryExtensions.cs` (Batch C) | `Corpus.SubtractOrRemoveValue`, `BayesianClassifierShared.UnTrain` |
| `StringJoin` | `IEnumerableExtensions.cs` / `StringExtensions.cs` (Batch B/C) | `BayesianClassifierShared.ValidateParameters` |
| `ToFormattedText` | `StringExtensions.cs` (Batch B) | `BayesianClassifierShared.GetMatchProbability` |
| `ToLazy` / `ToAsyncLazy` | `LazyExtension.cs` (Batch B) | `MulticlassEngine` (`classifierGroup.ToAsyncLazy()`) |
| `DeepCopy` | Extensions cluster (Batch B/C) | `MulticlassEngine` (`loader.Config.DeepCopy()`) |
| `SubtractThreadSafe` (int) | Extensions cluster | `BayesianClassifierShared` (`_matchEmailCount.SubtractThreadSafe`) |

### Load-bearing contract consequence: `ThrowIfNull` gives NO ambient null-narrowing

Verified in `NullExtensions.cs` (lines 16–42): `ThrowIfNull<T>` is `where T : notnull` and returns
the non-null value; it uses **no** `[NotNull]` post-condition attribute (correctly, because those
attributes are not available on net481 — see §5). Therefore the null-state benefit is realized
**only through the return value**, not the side effect. Cluster call sites that invoke it as a bare
statement and discard the result — for example `Globals.ThrowIfNull();` (`MulticlassEngine.cs:55,114`),
`EngineName.ThrowIfNullOrEmpty();` (`MulticlassEngine.cs:113`), `collection.ThrowIfNullOrEmpty();`
(`MulticlassEngine.cs:324`), `item.ThrowIfNull(...)` (`TristateEngine.cs:102,118`) — will NOT narrow
the variable to non-null under `#nullable enable`. Subsequent dereferences can still emit CS8602.

Remediation implication (annotation-only): the executor must reach zero CS86xx by capturing the
return value, adding a justified `!` with a `// why` comment, or annotating the field/property as
non-null where an invariant already guarantees it — and must NOT add a `[NotNull]` polyfill and must
NOT rewrite these into new `if (x is null) throw` guard statements (that would add uncovered
executable lines under AC4 and risk an AC3 behavior change).

---

## 1. File Inventory and Remediation-Set Determination

Definitive note: the DEFINITIVE CS86xx set is measured at execution time by the executor's per-file
`/t:Rebuild /p:TreatWarningsAsErrors=true` build. task-researcher cannot compile; the classification
below is a **static estimate** to be confirmed at Phase 0 / per-batch build. `#nullable enable` is
absent from every in-scope production file (the only match in the tree is `Obsolete/DedicatedToken.cs`,
which is dead code). Line counts are from a newline count and are approximate.

### Bayesian/ (root)

| File | Lines | Class | Notes |
|---|---|---|---|
| `BayesianClassifierExtensions.cs` | 96 | REMEDIATE | extension helpers over classifier types |
| `BayesianClassifierGroup.cs` | 515 | REMEDIATE | core contract (`Classify`, `RebuildClassifier`); pre-existing >500 lines, do NOT split |
| `BayesianClassifierShared.cs` | 1008 | REMEDIATE | **core scoring engine** (see §4 guard list); pre-existing >500 lines, do NOT split |
| `Corpus.cs` | 313 | REMEDIATE | `SubtractAsync(..., SegmentStopWatch sw = null)`, `Clone()`/`as Corpus` null flow, operator overloads |
| `CorpusInherit.cs` | 297 | REMEDIATE | `dictionary = null` locals, `DeserializeJson` may return null, `_timer`/`_id` uninitialized fields |
| `DoNotSerializeContractResolver.cs` | 34 | REMEDIATE | small; Newtonsoft `CreateProperty` override returns nullable |
| `FolderHierarchyNode.cs` | 43 | REMEDIATE (low-risk) | already a `record` with `?? throw` guards; needs pragma only. **Already `record` — do NOT add `init`** (see §5) |
| `FolderHierarchyTree.cs` | 235 | REMEDIATE | tree build from paths; dictionary lookups |
| `IFolderPredictor.cs` | 45 | EXCLUDE (interface-only) | co-annotate only if implementer nullability forces CS8767/CS8766 (see below) |
| `LcppnFolderPredictor.cs` | 363 | REMEDIATE | prediction path (`Classify`); consumes probability contract |
| `LcppnFolderPredictorConfig.cs` | 125 | REMEDIATE | config DTO; nullable properties |
| `PerParentClassifier.cs` | 319 | REMEDIATE (low-risk) | **scoring** (`ScoreChildren`/`ChildLogScore`, see §4); already thorough guards + `group = null` default |
| `Prediction.cs` | 45 | REMEDIATE | `Prediction<T>`: unconstrained `T Class`, `CompareTo(Prediction<T> other)` null check → `IComparable<Prediction<T>?>`, `T?` |
| `SpamBayes.cs` (Bayesian) | 10 | EXCLUDE | empty stub `internal class SpamBayes { }`; no CS86xx possible |

### Bayesian/Obsolete/ — all EXCLUDE (dead code)

`BayesianClassifier.cs` (646), `BayesianFilter.cs` (346), `ClassifierGroup.cs` (396),
`CorpusExample.cs` (104), `CorpusVectorized_badidea.cs` (222), `DedicatedToken.cs` (59, already
`#nullable enable`). Reason: dead `Obsolete/` code; excluded from remediation scope.

### Bayesian/Performance/

| File | Lines | Class | Notes |
|---|---|---|---|
| `BayesianMetricTypes.cs` | 198 | REMEDIATE (scope-boundary) | metric data types; measurement tooling, not scoring |
| `BayesianPerformanceMeasurement.cs` | 1537 | REMEDIATE (scope-boundary, heavy) | benchmarking/measurement tooling; pre-existing >500; large remediation surface — flag for scope confirmation |
| `BayesianSerializationHelper.cs` | 351 | REMEDIATE | serialization I/O helper |
| `ConfusionViewer.cs` | 20 | EXCLUDE | WinForms `Form`-derived |
| `ConfusionViewer.Designer.cs` | 46 | EXCLUDE | Designer-generated |
| `MetricChartViewer.cs` | 20 | EXCLUDE | WinForms `Form`-derived |
| `MetricChartViewer.Designer.cs` | 110 | EXCLUDE | Designer-generated |

### ClassifierGroups/ (root + subfolders)

| File | Lines | Class | Notes |
|---|---|---|---|
| `ClassifierGroupUtilities.cs` | 474 | REMEDIATE | group build/load utilities |
| `ConditionalItemEngine.cs` | 46 | REMEDIATE | `ConditionalItemEngine<T>`: nullable delegate props, unconstrained `T TypedItem` |
| `ManagerAsyncLazy.cs` | 343 | REMEDIATE | async-lazy manager; dictionary `TryGetValue` flow |
| `MulticlassEngine.cs` | 458 | REMEDIATE | `InitAsync` returns `default`, `LoadStagingData` returns `default`, nullable delegate props, `ThrowIfNull` bare-statement sites (see §0) |
| `TristateEngine.cs` | 144 | REMEDIATE | many uninitialized `Func<...>`/`Action<...>` delegate fields; `bool?` tristate; `Threshhold` |
| `Actionable/ActionableClassifierGroup.cs` | 149 | REMEDIATE | derived engine |
| `Categories/CategoryClassifierGroup.cs` | 523 | REMEDIATE | derived engine; pre-existing >500 |
| `OlFolder/LcppnFolderPredictorStore.cs` | 67 | REMEDIATE | store wrapper |
| `OlFolder/OlFolderClassifierGroup.cs` | 346 | REMEDIATE | Outlook-interop-bound; annotation-only still applies (see §8 risk) |
| `SpamBayes/SpamBayes.cs` | 446 | REMEDIATE | **partial** (core); `TristateEngine`-derived |
| `SpamBayes/SpamBayes.Actions.cs` | 117 | REMEDIATE | **partial** |
| `SpamBayes/SpamBayes.Classify.cs` | 121 | REMEDIATE | **partial**; `as MailItem is null` flow, `[]` returns |
| `SpamBayes/SpamBayes.Conditions.cs` | 100 | REMEDIATE | **partial**; `UserProperties.Find(...) is not null` flow |
| `SpamBayes/SpamInitTimingProbe.cs` | 81 | REMEDIATE | timing probe |
| `Triage/Triage.cs` | 453 | REMEDIATE | **partial** (core) |
| `Triage/Triage_OlLogic.cs` | 269 | REMEDIATE | **partial** |

### Flags/

| File | Lines | Class | Notes |
|---|---|---|---|
| `FlagClassNoItem.cs` | 239 | REMEDIATE | |
| `FlagConsolidator.cs` | 135 | REMEDIATE | |
| `FlagDetails.cs` | 217 | REMEDIATE | |
| `FlagParser.cs` | 633 | REMEDIATE | pre-existing >500, do NOT split |
| `FlagTranslator.cs` | 90 | REMEDIATE | |
| `IFlagTranslator.cs` | 21 | EXCLUDE (interface-only) | co-annotate only if implementer nullability forces mismatch warnings |

### Interface co-annotation note

`IFolderPredictor.cs` and `IFlagTranslator.cs` are interface-only and emit no CS86xx on their own
(interfaces carry no null-flow). They are EXCLUDE for standalone remediation. However, if a
remediated implementer (`BayesianClassifierGroup`, `LcppnFolderPredictor`, `FlagTranslator`) annotates
a parameter or return as nullable, the compiler will emit a nullability-mismatch warning
(CS8767/CS8766) on the implementing member unless the interface signature is co-annotated. In that
case the interface must be annotated **in the same batch** as its implementer to keep the contract
consistent. This is annotation-only and does not change the interface's behavior.

### Reconciliation against the epic's ~18 estimate

The epic estimate is ~18 remediation files. My static REMEDIATE candidate set is substantially
larger: **~33 files** when Obsolete (6), Performance viewers/Designers (4), interfaces (2), and the
empty `SpamBayes.cs` stub are excluded (or **~30** if the three `Performance/` non-viewer files are
also treated as out of scope). The gap is expected and is resolved at Phase 0, for two reasons:

1. task-researcher cannot compile. The ~18 figure most plausibly reflects files that will **actually
   emit CS86xx** after the pragma. Several REMEDIATE-classified files are small and already
   null-guarded (`PerParentClassifier`, `FolderHierarchyNode`, `Prediction`, `ConditionalItemEngine`,
   `SpamBayes.Conditions/Actions`) and may need only a pragma line with zero or near-zero code change,
   or may prove already null-clean.
2. Scope-boundary ambiguity. `Performance/` (measurement/benchmark tooling, including the 1537-line
   `BayesianPerformanceMeasurement.cs`) and the entire `Flags/` subfolder are the two most likely
   candidates the epic may have intended to defer or exclude from this specific child. Both are
   flagged as open questions in §8.

Recommendation: treat the ~18 as a target for **files requiring code edits**, confirm the exact
CS86xx-emitting set at Phase 0 via the per-file rebuild, and confirm the `Performance/` and `Flags/`
scope boundary with the maintainer before batching.

---

## 2. Partial-Class and Co-Remediation Constraints

Partial-class groups (verified via `partial class` declarations) that MUST be remediated together in
one batch because members are shared across files:

- **SpamBayes partial set** (`namespace ...ClassifierGroups`, `public partial class SpamBayes : TristateEngine, IConditionalEngine<MailItemHelper>`):
  - `ClassifierGroups/SpamBayes/SpamBayes.cs`
  - `ClassifierGroups/SpamBayes/SpamBayes.Actions.cs`
  - `ClassifierGroups/SpamBayes/SpamBayes.Classify.cs`
  - `ClassifierGroups/SpamBayes/SpamBayes.Conditions.cs`
  - Note: `SpamBayes` derives from `TristateEngine` (abstract base) and overrides `TrainAsync(string[], bool)`. The base `TristateEngine.cs` should be annotated **before or with** this group (see §3 ordering). The 10-line `Bayesian/SpamBayes.cs` stub is a **different, unrelated** `internal class SpamBayes {}` in namespace `...Bayesian` — not part of this partial set; EXCLUDE.

- **Triage partial set** (`public partial class Triage`):
  - `ClassifierGroups/Triage/Triage.cs`
  - `ClassifierGroups/Triage/Triage_OlLogic.cs`

- **Bayesian partials:** none found in scope. `BayesianClassifierShared`, `BayesianClassifierGroup`,
  `Corpus`, `CorpusInherit`, `PerParentClassifier`, `Prediction` are each single-file
  non-partial types. (The only partial classes elsewhere under EmailIntelligence — `EmailDataMiner`,
  `SubjectMapSco` — are outside the three target clusters.)

No `record struct` or `record` partial pairing exists in scope. `FolderHierarchyNode` is a single-file
sealed `record` (§5).

---

## 3. Leaf-First Batch Grouping (Contract-Core Before Consumers)

Proposed ordered batch plan, analogous to #363's Batch A..E structure. Ordering respects:
partial-class co-remediation; shared/base types before consumers; subfolder grouping. The definitive
task-by-task sequencing belongs to the atomic plan, not this research.

- **Batch A — pure data/contract leaves (no in-cluster dependents):**
  `Prediction.cs`, `FolderHierarchyNode.cs`, `LcppnFolderPredictorConfig.cs`,
  `DoNotSerializeContractResolver.cs`, `BayesianClassifierExtensions.cs`.
  Rationale: `Prediction<T>` is consumed by `IFolderPredictor`/`LcppnFolderPredictor`; annotate it
  first so `Prediction<T>?` null-state is fixed before consumers reference it.

- **Batch B — corpus/count core (consumed by all scoring):**
  `Corpus.cs`, `CorpusInherit.cs`.
  Rationale: `Corpus` is the token-frequency substrate referenced by `BayesianClassifierShared`,
  `PerParentClassifier`, and `BayesianClassifierGroup`.

- **Batch C — scoring engine core:**
  `BayesianClassifierShared.cs`, then `BayesianClassifierGroup.cs`, then `PerParentClassifier.cs`,
  `FolderHierarchyTree.cs`.
  Rationale: `BayesianClassifierShared` holds all the probability math (§4) and is referenced by the
  group and per-parent classifiers; annotate the shared engine before its aggregators. This is the
  highest-risk batch for the DO-NOT-ALTER guard list.

- **Batch D — engine base + generic engines (base before derived):**
  `TristateEngine.cs`, `ConditionalItemEngine.cs`, `MulticlassEngine.cs`, `ManagerAsyncLazy.cs`,
  `ClassifierGroupUtilities.cs`.
  Rationale: `TristateEngine` (abstract) and `MulticlassEngine<T>` (abstract) are the bases for
  `SpamBayes`/`Triage`/`ActionableClassifierGroup`/`CategoryClassifierGroup`; annotate bases first so
  derived overrides inherit consistent nullability and avoid CS8765/CS8767 override-mismatch.

- **Batch E — derived engines + predictors (consume Batches C/D):**
  SpamBayes partial set (4 files, together), Triage partial set (2 files, together),
  `ActionableClassifierGroup.cs`, `CategoryClassifierGroup.cs`, `LcppnFolderPredictor.cs`,
  `OlFolder/LcppnFolderPredictorStore.cs`, `OlFolder/OlFolderClassifierGroup.cs`,
  `SpamInitTimingProbe.cs`. Co-annotate `IFolderPredictor.cs` here if implementer nullability forces
  it.

- **Batch F — Flags subfolder (if in scope, see §8):**
  `FlagDetails.cs`, `FlagClassNoItem.cs`, `FlagConsolidator.cs`, `FlagTranslator.cs`,
  `FlagParser.cs`; co-annotate `IFlagTranslator.cs` if forced.

- **Batch G — Performance tooling (if in scope, see §8):**
  `BayesianMetricTypes.cs`, `BayesianSerializationHelper.cs`, `BayesianPerformanceMeasurement.cs`.
  Deferred/last because it is measurement tooling, not scoring, and `BayesianPerformanceMeasurement.cs`
  is a heavy (1537-line) surface.

Cross-file ordering constraints (which files publish contracts consumed by other in-scope files):
`Prediction` → predictors; `Corpus`/`CorpusInherit` → scoring engines; `BayesianClassifierShared` →
`BayesianClassifierGroup`/`PerParentClassifier`; `TristateEngine`/`MulticlassEngine` (base) →
`SpamBayes`/`Triage`/`Actionable`/`Category` (derived); `IFolderPredictor`/`IFlagTranslator` co-batch
with their implementers.

---

## 4. Scoring / Model / Corpus Math Protection — DO NOT ALTER Guard List

The executor must annotate **around** these regions without changing any arithmetic, comparison,
constant, clamp, ordering, or control flow. Reaching zero CS86xx MUST NOT introduce a new
`if (x is null) throw` on any scoring path, must not reorder operations, and must not change a
`Math.Max`/`Math.Min`/division/log/exp expression. Where the compiler flags a possible-null on a
scoring path, prefer annotation + a justified `!` (with a `// why` comment) or `where T : notnull`.

### `BayesianClassifierShared.cs` (the core engine)

- `UpdateProbability(string, int, int)` (lines ~348–403) — Paul Graham probability incl.
  `Knobs.MinScore`/`MaxScore` clamps and the `nm == 0` special case. DO NOT ALTER.
- `UpdateProbabilitySb(string, int, int)` (lines ~405–438) — Robinson Bayesian adjustment
  (`(StimesX + n*prob)/(S + n)`). DO NOT ALTER.
- `UpdateProbabilitySb(WordInfo)` (lines ~440–462) and `UpdateProbabilitySb(string)` (lines ~473–513).
  DO NOT ALTER. Note: `GetWordInfo` returns `WordInfo` that may be `null` (line ~961); this is a
  legitimate nullable return — annotate `WordInfo?` and let `GetWordDistance` (line ~933) keep its
  existing `if (record is null)` branch. Do NOT convert the existing branch to a throw.
- `CombineProbabilities(SortedList<string,double>)` (lines ~574–610) — chi/Graham product combine.
  Keep the existing `if (probabilities is null) throw` and `Count == 0` early returns as-is
  (existing guards stay). DO NOT ALTER the `mult/comb` math.
- `GetInterestingList` (lines ~612–673), `GetMatchProbability` (overloads, lines ~675–737),
  `GetProbabilityDrivers` (lines ~684–706), `MergeProb`/`GetNotMatchIncidence`. DO NOT ALTER
  selection/sort/`interestingKey` formatting.
- `Chi2SpamProb(...)` overloads and `Chi2SpamProb(string[], bool)` (lines ~784–887) — chi-squared
  with `frexp` underflow handling (`math.frexp`), `1e-200` thresholds, `Math.Log(2)` scaling.
  DO NOT ALTER. Note the `evidence == false` path returns `(prob, null)` (line ~885) — this is a
  legitimate nullable tuple element; annotate the return type's list element as nullable rather than
  changing the return.
- `chi2Q(double, int)` (lines ~894–906), `GetClues(HashSet<string>)` (lines ~911–931),
  `GetWordDistance` (lines ~933–950). DO NOT ALTER.
- `KnobList` constants (lines ~976–992: `MinScore=0.011`, `MaxScore=0.99`, `LikelyMatchScore`,
  `CertainMatchScore`, `UnknownWordProb=0.5`, `UnknownWordStrength=0.45`, `MaxDiscriminators=150`,
  etc.). DO NOT ALTER any value.
- `Train`/`TrainMultiTag`/`UnTrain`/`UnTrainMultiTag` (lines ~259–337) — count-update paths using
  `Interlocked`, `AddOrUpdate`, and `UpdateOrRemove(..., out int)`. DO NOT ALTER count math. `_parent`
  is nullable-by-construction (`protected BayesianClassifierGroup _parent;`); annotate the
  `Parent`/`_parent` null-state to satisfy the compiler without adding a runtime guard on the hot path.

### `PerParentClassifier.cs` (hierarchical-shrinkage Naive Bayes)

- `ScoreChildren` (lines ~179–212), `ChildLogScore` (lines ~215–265) — shrinkage blend
  `λ·P_leaf + (1-λ)·P_parent`, softmax `Normalize`. DO NOT ALTER.
- `LaplaceProbability` (lines ~267–271) — add-one smoothing `(count+α)/(total+α·max(vocab,1))`.
  DO NOT ALTER. `LaplaceAlpha = 1.0` constant (line 29) — DO NOT ALTER.
- `Normalize` (lines ~273–290) — numerically stable softmax incl. the `sum <= 0` uniform fallback.
  DO NOT ALTER.
- Keep existing guards (`ValidateInvariants`, `RequireChildSegment`, `tokens is null` throws,
  `GroupAndCount`'s `token is null` continue) as-is. The `group = null` default param already exists;
  annotate as `BayesianClassifierGroup? group = null` — this matches the existing `?? new(...)` flow.

### `Corpus.cs`

- Operator `+` (lines ~189–202), operator `-` (lines ~253–271), `SubtractAsync` (lines ~204–251),
  `SubtractFilter` (lines ~273–309) — token-frequency set arithmetic incl. `negTokenWt`/`minCt`
  thresholds and `TryUpdate`/`TryRemove` flow. DO NOT ALTER. `Clone()` uses `as Corpus` (nullable
  result); annotate/`!` rather than adding a throw. `SubtractAsync(..., SegmentStopWatch sw = null)`
  → `SegmentStopWatch? sw = null` (the `sw ??= new(...)` already handles it).

### `Prediction.cs`

- `CompareTo(Prediction<T> other)` (lines ~33–43) — keep the `other is null → return 1` contract
  (`IComparable`); annotate parameter as `Prediction<T>?` to match the existing null check. DO NOT
  ALTER the `_probability.CompareTo` ordering.

### `TristateEngine.cs`

- `GetTristate(double)` (lines ~127–136) — threshold decision (`> MinimumTrue` → true,
  `< MaximumFalse` → false, else null). This is a decision boundary; DO NOT ALTER the comparisons.
  `bool?` is a nullable value type (fine under pragma). The delegate fields
  (`_tokenize`, `_calculateProbability`, `_getTristateAsync`, `_callback`, `_threshhold`, ...) are
  null-by-default; annotate as `Func<...>?` / `Action<...>?` / `TristateThreshhold?`. Keep existing
  `ThrowIfNull(...)` guard calls (but note §0 — they do not narrow; use return value or `!`).

### `MulticlassEngine.cs` / `BayesianClassifierGroup.cs`

- `MulticlassEngine.InitAsync` returns `default` (line ~69) and `LoadStagingData` returns `default`
  (line ~321) for `T`/`MinedMailInfo[]` — annotate the return as nullable (`Task<T?>` /
  `MinedMailInfo[]?`) to reflect the true behavior rather than adding a throw. `Condition`/scoring
  gate logic (`Condition`, `GetOlItemString`) is behavioral filtering — DO NOT ALTER the
  message-class/`IPM.Note` checks. `ProbabilityThreshold = 0.8` default — DO NOT ALTER.
- `BayesianClassifierGroup.Classify(string[])` and `RebuildClassifier` (not fully read here) —
  treat all probability/aggregation math as DO NOT ALTER; annotate signatures and dictionary
  `TryGetValue` flow only. Confirm exact regions when the file is opened for edit.

General DO-NOT-ALTER temptations to avoid: (a) adding `if (x is null) throw` on any of the above
hot paths to silence CS8602 — use annotation + justified `!` instead; (b) changing an existing
`null`-returning method (`GetWordInfo`, `Chi2SpamProb` non-evidence path) into a throwing method;
(c) reordering `Math.Max`/`Math.Min` clamps to "simplify" null flow.

---

## 5. net481 / C# 12 Constraints

Verified from `UtilitiesCS/UtilitiesCS.csproj`:
- `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>` (net481).
- `<LangVersion>12.0</LangVersion>` (C# 12).
- No `<Nullable>` element present. AC: this feature MUST NOT add one; enforcement is per-file pragma
  only (matches #363 AC2).

Constraints that carry into this feature:

- **Nullable post-condition attributes are NOT available/polyfilled and MUST NOT be used or added:**
  `[NotNullWhen]`, `[MaybeNullWhen]`, `[NotNullIfNotNull]`, `[MaybeNull]`, `[AllowNull]`,
  `[DisallowNull]`, `[DoesNotReturn]`, `[MemberNotNull]`. Zero CS86xx is reachable without them
  (proven by #363's already-enabled `NullExtensions.cs`) using plain `?`, `where T : notnull`,
  unconstrained `T?` returns/`out`, guard clauses, and justified `!`. Do NOT add a
  `System.Diagnostics.CodeAnalysis` polyfill — that is new production surface and out of scope.
  This is directly relevant because `NullExtensions.ThrowIfNull` deliberately avoids `[NotNull]`
  (see §0), so consumers cannot rely on ambient narrowing.

- **No `init` / `record` / `record struct` may be INTRODUCED:** net481 lacks `IsExternalInit`, so
  `init` accessors, positional records, and `record struct` fail CS0518. Existing types that already
  compile are fine:
  - `FolderHierarchyNode` is an existing `sealed record` with **get-only** auto-properties set in a
    constructor (no `init` accessor), so it compiles on net481. When remediating, DO NOT add an
    `init` accessor or convert it to a positional record; keep the get-only + constructor shape.
  - `WordStream` (`BayesianClassifierShared.cs:778`) and `WordInfo` (line ~772) use primary
    constructors with public fields assigned in the constructor — no `= default` reference field
    initializer requiring `= default!`. `TristateThreshhold` (`TristateEngine.cs:139`) similarly uses
    a primary constructor with `double` fields.
  - `FolderStruct` and `SpamBayesOptions` structs exist but are **outside** the three target clusters
    (`EmailParsingSorting`), so they are not in scope.

- **Struct-with-`= default` reference fields in scope:** none identified in the in-scope files. If the
  per-file build surfaces one (for example inside `BayesianPerformanceMeasurement.cs`, not fully read),
  apply `= default!` or type the field non-nullable and initialize it in the constructor, mirroring
  #363's `DfDeedle.EmailRecord` treatment. Confirm at edit time.

---

## 6. Existing Test Coverage

The `UtilitiesCS.Test` project (`UtilitiesCS.Test/UtilitiesCS.Test.csproj`, MSTest) has dense
coverage across the cluster. Golden/property/characterization suites that MUST remain unchanged
(no scoring path altered) include, among ~90 matching files:

- Scoring engine: `EmailIntelligence/Bayesian/BayesianClassifierShared_Tests.cs`,
  `BayesianClassifierSharedTests.cs`, `BayesianClassifierTests.cs`, `Corpus_Tests.cs`,
  `CorpusInherit_Tests.cs`, `PerParentClassifier_Tests.cs`, `Prediction_Tests.cs`,
  `EmailIntelligence/Bayesian/SampleTestSets.cs` (shared fixtures).
- Group/predictor: `BayesianClassifierGroup_Tests.cs`,
  `BayesianClassifierGroup_FlatPathUnchanged_Tests.cs`, `IFolderPredictor_Tests.cs`,
  `LcppnFolderPredictor_Classify_Tests.cs`, `LcppnFolderPredictor_Serialization_Tests.cs`,
  `FolderHierarchyTree_Tests.cs`.
- Engines: `ClassifierGroups/MulticlassEngine_Tests.cs`, `EngineTests.cs`,
  `ConditionalItemEngine_Tests.cs`, `ClassifierGroups/Triage/*`, `Triage_Tests.cs`,
  `SpamBayes_Tests.cs`, `SpamInitTimingProbeTests.cs`,
  `ClassifierGroups/ClassifierGroupUtilities_Tests.cs`, `OlFolderClassifierGroup_Tests.cs`,
  `ManagerAsyncLazy_Tests.cs`, `ActionableClassifierGroup_Tests.cs`.
- Flags: `FlagClassNoItem_Tests.cs`, `FlagDetails_Tests.cs`, `FlagTranslator_Tests.cs`,
  `Flags/FlagParserTests.cs`.
- Performance: `Bayesian/BayesianPerformanceMeasurement_Tests.cs`,
  `Bayesian/BayesianSerializationHelper_Tests.cs`, `BayesianMetricTypes_Tests.cs`.
- Subclass test doubles that pin protected/virtual scoring seams:
  `Bayesian/SubBayesianClassifier.cs`, `SubClassifierGroup.cs`, `SubCorpus.cs`. These override
  `protected internal virtual` members (`UpdateProbability*`); any signature-nullability change to
  those virtuals must keep the override contract intact (annotate base and override consistently to
  avoid CS8765/CS8767) — treat these test doubles as part of the spec.

Coverage baseline concern (AC4): the change must not regress changed-line coverage. Because these are
annotation-only edits, the safest posture is to prefer nullable annotations and justified `!` over new
runtime guard statements, so no new uncovered executable lines are introduced. A `#nullable enable`
pragma line and `?`/`!` annotations are non-executable and do not add to the coverage denominator; a
new `if (x is null) throw` block does, and would need a new test to cover the throw. The plan should
capture a coverage baseline at Phase 0 and compare changed-line coverage per batch.

---

## 7. Toolchain and Verification Note

Per-file pragma gate (the nullable verification step). Per the epic convention and #363 plan, the
narrowest correct form targets the project:

```
msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true
```

The solution-wide form (`msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true`) is used
for the final QC pass. `/t:Rebuild` is mandatory (per PR #361) so the compiler performs a genuine
recompile rather than a silently-skipped incremental build.

CLAUDE.md toolchain order (run per batch, restart on any change):
1. `dotnet tool run csharpier .` (adding a pragma + `?` reformats; run first).
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (analyzers / code style).
3. Per-file nullable pragma rebuild gate (the `/t:Rebuild ... /p:TreatWarningsAsErrors=true` command above).
4. `vstest.console.exe <UtilitiesCS.Test assembly> /EnableCodeCoverage` (regression + coverage).

Critical: do NOT pass `/p:Nullable=enable` globally for this feature's verification. The global flag
forces nullable solution-wide and surfaces the full pre-existing debt (the #363 plan cites a ~2131
whole-repo diagnostic count), drowning this child's signal. The per-file pragma keeps non-opted files
silent while `TreatWarningsAsErrors` turns any CS86xx in an opted-in file into an error. The
global-flag-versus-per-file-pragma conflict is deferred to the Wave-2 CI capstone; do not resolve it
here and do not edit `.claude/rules/*`.

---

## 8. Open Questions / Risks for the Maintainer

1. **Scope of `Performance/` (Bayesian).** `BayesianMetricTypes.cs`, `BayesianSerializationHelper.cs`,
   and especially the 1537-line `BayesianPerformanceMeasurement.cs` are measurement/benchmark tooling,
   not scoring. Confirm whether they are in scope for #372 or deferred. Including
   `BayesianPerformanceMeasurement.cs` roughly triples the largest single-file remediation surface.
2. **Scope of the `Flags/` cluster.** `Flags/` (6 files, incl. the 633-line `FlagParser.cs`) is listed
   as a target directory, but its inclusion is the single largest driver of the gap between the epic's
   ~18 estimate and the ~33 static candidate count. Confirm `Flags/` is intended for this child.
3. **The ~18 vs ~33 count gap.** Resolve at Phase 0 by capturing the exact CS86xx-emitting set from
   the per-file rebuild before batching. Report the measured set against the ~18 target.
4. **Interface co-annotation timing.** `IFolderPredictor` / `IFlagTranslator` may need co-annotation
   with their implementers to avoid CS8767/CS8766. Confirm the batch owns both files when that occurs.
5. **`OlFolderClassifierGroup.cs` COM binding.** This file is Outlook-interop-bound. Annotation-only
   work is still valid, but the executor should watch for COM-returned reference types (which the SDK
   surfaces as non-nullable) that are actually null at runtime; annotate to the true behavior and use
   justified `!` where the COM contract guarantees non-null. No behavior change.
6. **Files over 500 lines (pre-existing).** `BayesianClassifierShared.cs` (1008),
   `BayesianClassifierGroup.cs` (515), `CategoryClassifierGroup.cs` (523), `FlagParser.cs` (633),
   `BayesianPerformanceMeasurement.cs` (1537) exceed the 500-line limit. This is pre-existing; the
   annotation-only rule forbids splitting them here. Flag for a future refactor issue; do not fix now.
7. **`ThrowIfNull` no-narrowing friction (§0).** Numerous cluster call sites invoke `ThrowIfNull()` as
   a bare statement. Under the pragma these will not narrow, so the executor will need targeted
   annotation/`!` at each dereference. This is the most repetitive remediation pattern and the most
   likely place a well-meaning "add a guard" edit would violate AC3/AC4; the plan should call it out
   explicitly.

---

## Automation Feasibility

This is a source-only C# change (annotations, generic constraints, and justified `!` on existing
`.cs` files), with no third-party UI interaction, no external service, no network, no filesystem
mutation beyond the tracked source tree, and no credentials. Verification is fully scripted
(csharpier, msbuild, vstest). No human-interaction step is required; the work is fully automatable by
the executor. The only human decisions are the scope-boundary confirmations in §8, which are planning
inputs, not execution-time interactions.
