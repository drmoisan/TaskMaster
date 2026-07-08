# Research: LCPPN Hierarchical Folder Prediction — Issue #177

- Date: 2026-06-08
- Author: task-researcher
- Issue: #177
- Status: Complete — feeds feature spec and atomic plan

---

## 1. Current Architecture (Verified)

### Flat predictor summary

The flat predictor is implemented across four cooperating types.

**`BayesianClassifierGroup`** (`UtilitiesCS/EmailIntelligence/Bayesian/BayesianClassifierGroup.cs`)
- Holds `ConcurrentDictionary<string, BayesianClassifierShared> Classifiers` keyed by full leaf-folder tag (e.g., `"Projects\\Alpha\\2024"`) (`BayesianClassifierGroup.cs:33-38`).
- Holds a `Corpus SharedTokenBase` — a flat `ConcurrentDictionary<string, int>` of all tokens across all tags, used as the "not-match" denominator for every per-tag classifier (`BayesianClassifierGroup.cs:40-46`).
- `TotalEmailCount` is the sum of all trained emails across all tags (`BayesianClassifierGroup.cs:48-53`).
- `Train(tag, tokens, count)` adds a new classifier via `GetOrAdd` if the tag is new; increments per-tag match counts and shared token counts (`BayesianClassifierGroup.cs:146-151`).
- `TrainMultiTag` also increments `TotalEmailCount` and `SharedTokenBase` once per call across all tags, then iterates to update each tag's match counts (`BayesianClassifierGroup.cs:164-187`).
- `Classify(tokenIncidence)` scores **every** classifier in `Classifiers` in parallel via `Chi2SpamProb`, filters by `MinimumProbability`, and returns results ordered by descending probability (`BayesianClassifierGroup.cs:236-248`). There is no tree traversal; all leaves are compared directly.

**`BayesianClassifierShared`** (`UtilitiesCS/EmailIntelligence/Bayesian/BayesianClassifierShared.cs`)
- Holds per-tag match counts in `Corpus Match` and a cached `ConcurrentDictionary<string, double> Prob` (`BayesianClassifierShared.cs:210-238`).
- References its parent `BayesianClassifierGroup` to access `SharedTokenBase` and `TotalEmailCount` (`BayesianClassifierShared.cs:240-244`).
- `Chi2SpamProb(tokens)`: SpamBayes chi-squared combiner. Identifies discriminating tokens via `GetClues` (selects up to `MaxDiscriminators=150` tokens by distance from 0.5), then computes a combined spam-vs-ham probability via sum-of-log products and `chi2Q` (`BayesianClassifierShared.cs:810-887`).
- `UpdateProbabilitySb(token, matchCount, notMatchCount)`: Robinson Bayesian adjustment — `prob = (S*x + n*p) / (S + n)` with `UnknownWordStrength=0.45`, `UnknownWordProb=0.5` (`BayesianClassifierShared.cs:405-438`).
- Incremental update: on `Train`, for each token the match count in `Match.TokenFrequency` is incremented, then `UpdateProbabilitySb` is called immediately, caching the new probability in `Prob` (`BayesianClassifierShared.cs:259-277`). No full retrain is needed.
- Abstention: governed by `MinimumProbability` in `BayesianClassifierGroup`; predictions below the threshold are filtered out (`BayesianClassifierGroup.cs:246`). If no prediction clears the threshold the caller receives an empty result, which the app treats as "no prediction" (abstain).

**`Corpus`** (`UtilitiesCS/EmailIntelligence/Bayesian/Corpus.cs`)
- `ConcurrentDictionary<string, int> TokenFrequency` — token to occurrence-count store (`Corpus.cs:89-94`).
- `AddOrSumTokenValue`, `SubtractOrRemoveValue` — thread-safe increment/decrement (`Corpus.cs:153-175`).
- Supports `Clone()` and `operator+/-` for set arithmetic (`Corpus.cs:177-309`).

**`Prediction<T>`** (`UtilitiesCS/EmailIntelligence/Bayesian/Prediction.cs`)
- Simple record: `Class` (the tag string) + `Probability` (double) + `IComparable<Prediction<T>>` (`Prediction.cs:9-44`).

### FolderInfo.RelativePath — verified structure

`IFolderWrapper.RelativePath` is a `string` property (`IFolderWrapper.cs:30`). `FolderWrapper.LoadRelativePath()` computes it by stripping the Outlook root path prefix from `OlFolder.FolderPath` and removing the leading backslash separator (`FolderWrapper.cs:193-222`). Concretely:
- Root path: `\\Mailbox - Dan\Archive`
- Folder path: `\\Mailbox - Dan\Archive\Projects\Alpha\2024`
- Resulting `RelativePath`: `Projects\Alpha\2024`

Delimiter is **backslash (`\`)**. There is no leading or trailing delimiter. A top-level folder directly under the archive root produces a single-segment path (e.g., `"Projects"` with no backslash). Depth is unbounded.

`OlFolderClassifierGroup.BuildFolderClassifiersAsync` groups `MinedMailInfo` by `FolderInfo.RelativePath` and creates one classifier per full path, ignoring all intermediate segments (`OlFolderClassifierGroup.cs:118`). The current code never parses the path into parts.

### Classifier registration and lifecycle

- After building, the classifier group is assigned to `Globals.AF.Manager["Folder"]` as an `AsyncLazy<BayesianClassifierGroup>` (`OlFolderClassifierGroup.cs:211`).
- `EmailFiler.TrainFolderAsync` calls `(await Manager["Folder"]).Train(destinationOlStem, tokens, 1)` on a user move (`EmailFiler.cs:378-386`).
- `EmailFiler.UnTrainFolderAsync` calls `.UnTrain(originOlStem, tokens, 1)` on undo (`EmailFiler.cs:374-376`).
- `SortEmail` calls `.Train(destinationOlStem, tokens, 1)` after auto-sort (`SortEmail.cs:250-253`).

All callers address `Manager["Folder"]` which returns a `BayesianClassifierGroup`. This is the **primary seam** for introducing the hierarchical predictor.

### Serialization

`BayesianClassifierGroup` extends `SmartSerializable<BayesianClassifierGroup>` (`BayesianClassifierGroup.cs:15`). `SmartSerializable<T>` uses Newtonsoft.Json with `TypeNameHandling.Auto` and `Formatting.Indented`. Serialization is triggered lazily via `RequestSerialization` with a 3-second debounce timer (`SmartSerializable.cs:523-530`). `ClassifierGroupUtilities` also writes and reads `BayesianClassifierGroup` JSON directly via `JsonConvert.DeserializeObject<T>` (`ClassifierGroupUtilities.cs:91-94`).

---

## 2. Hierarchy Construction

### How to build the parent → children tree

Given a `MinedMailInfo[]` corpus, the tree is built by parsing each `RelativePath` on backslash and recording each segment pair as a parent → child edge.

Algorithm:
1. For each distinct `RelativePath` value, call `path.Split('\\')`.
2. Walk the segments. For depth `d` from 0 to `segments.Length - 2`:
   - `parentKey = string.Join("\\", segments[0..d+1])` (or `""` / a sentinel root if `d == 0`)
   - `childKey = string.Join("\\", segments[0..d+2])`
   - Register `childKey` as a child of `parentKey`.
3. The root-level parent key is the empty string (or a dedicated `FolderTreeNode.Root` sentinel).

### Edge cases

| Scenario | Behavior |
|---|---|
| Single-segment path (`"Inbox"`) | Only one edge: `root → "Inbox"`. This node is both a parent-set entry (with zero children) and a leaf. |
| Path already seen | Idempotent: `HashSet<string>` per parent deduplicates children. |
| Reparented folder | On rebuild, the old parent's child set is replaced. Incremental reparenting is not supported (rare; treat as a full rebuild trigger). |
| New leaf under existing parent | Only the parent's child set changes; all other classifiers are unaffected. |
| Case sensitivity | Paths from Outlook COM are case-sensitive as received; treat comparisons as ordinal by default. A `StringComparer.OrdinalIgnoreCase` option should be configurable. |

---

## 3. Per-Parent Decision Model

### Candidate evaluation

**Hard constraints:**
1. Higher leaf-level F1 than the flat Bayesian predictor.
2. Incremental count-based update; no full retrain required.

---

#### (a) LCPPN + Hierarchical-Shrinkage Naive Bayes

**Description.** Each parent node `p` with children `C(p)` scores child `c` by the smoothed NB probability:

```
P(c | p, tokens) ∝ P_smoothed(c) * ∏_t P_smoothed(t | c)
```

where `P_smoothed(t | c) = λ * P_leaf(t | c) + (1-λ) * P_parent(t | p)`. The smoothing weight `λ` can be estimated from data or set to a fixed value (e.g., 0.7).

**Reuse of existing machinery.** Each per-parent classifier is essentially a `BayesianClassifierGroup` whose `SharedTokenBase` is the parent-scoped total-token corpus, and whose `Classifiers` dictionary is keyed by child name rather than full path. `BayesianClassifierShared` is reused without modification. The smoothing adds one new step: when computing per-token probability, blend the leaf's `Match.TokenFrequency[token]` with an aggregated parent count.

**Incremental update.** When a new email lands in leaf `c` under parent `p`, update `BayesianClassifierShared` for child `c` (match counts) and the shared parent corpus (total counts). Walk up to `p`'s parent and repeat. This is O(depth) updates, same asymptotic cost as the current single-tag update.

**F1 gain expectation.** Moderate. Addresses the data-sparsity problem (siblings share smoothed parent statistics) but remains a generative model with the conditional independence assumption. For well-separated sibling folders, F1 improvement over the flat model is real but bounded.

**C# implementation cost.** Low. The existing `BayesianClassifierGroup` / `BayesianClassifierShared` / `Corpus` types compose directly. The only new logic is the smoothing coefficient and the tree-structure wrapper that routes each email to the correct per-parent group.

**Key limitation.** Does not improve over a flat NB for siblings that are discriminated by complex feature combinations (e.g., subtle sender + subject patterns). Smoothing reduces variance but does not change the generative model family.

---

#### (b) LCPPN + NBSVM-at-each-parent

**Description.** Maintain NB log-count-ratio features from existing `BayesianClassifierShared.Prob` cache per sibling group, then feed them as input to a small per-parent linear classifier (binary one-vs-rest or multiclass softmax). The NB layer is not the final decision; it is a feature transformer.

**Reuse of existing machinery.** The NB count layer is unchanged. The added layer is a per-parent weight vector `w_c` for each child `c`. For multiclass, scores: `score(c) = w_c · phi(tokens)` where `phi_t = log(P(t|c) / P(t|¬c))` — directly available from `BayesianClassifierShared.Prob` cache.

**Incremental update.** The NB counts update as before. The linear weight layer requires either online gradient steps (SGD) or a periodic re-fit. With online SGD (gradient step on each correction), the linear layer can be updated without a full retrain. Without an online method the linear layer requires batch re-fit whenever new data arrives, which violates hard constraint 2.

**F1 gain expectation.** Higher than pure NB. NBSVM is a well-documented improvement over NB for text classification; the per-parent scoping keeps each linear problem small.

**C# implementation cost.** Medium-high. Requires implementing a per-parent online logistic or SVM weight vector update in C#. No existing repo type covers this. If ML.NET is introduced, the `SgdCalibratedTrainer` or `AveragedPerceptronTrainer` could serve this role, but they require a fixed feature space. The NB log-count-ratio feature space is token-indexed and grows over time, which conflicts with static feature spaces in ML.NET trainers without periodic re-registration. A self-contained implementation would require an `IDictionary<string, double>` sparse weight vector plus a Pegasos/SGD update rule.

**Risk.** The feature space is unbounded as new tokens arrive. This is manageable with a sparse representation, but it is a significant increase in state complexity compared to the current design.

---

#### (c) LCPPN + Online Linear/Logistic per-parent learner

**Description.** Each parent node runs an independent online multiclass classifier over its direct children. Inputs are the raw token bag-of-words or NB-transformed features. Parameters are updated by online gradient descent on each training example.

**Incremental update.** Genuine online update: on each user correction, execute one gradient step per classifier on the true path. Adding a new leaf requires re-initializing the parent classifier with the new class, which may require re-fitting all examples seen so far for that parent (a significant re-fit for busy parents).

**F1 gain expectation.** Highest asymptotically for parents with sufficient data per sibling. Discriminative models with an appropriate learning rate can model complex boundaries that NB cannot.

**C# implementation cost.** High. A correct online multiclass logistic/SGD implementation requires: a sparse gradient computation, a learning-rate schedule, class-count management when new siblings are added, and validation against overfitting. ML.NET `AveragedPerceptronTrainer` supports incremental warm-start retraining but requires a fixed feature dimension and periodic retrain batches, not true per-example updates without a retrain call.

**ML.NET justification assessment.** The repository prefers approved libraries. ML.NET is a Microsoft-supported package with documented retrainable trainers. However, introducing it for this feature would add a heavyweight dependency (ML.NET pulls in significant NuGet packages), requires adapting `IDataView` / feature pipelines for the dynamic token vocabulary, and does not provide true online (per-example) updates without a retrain call per batch. For the specific LCPPN use case the benefit does not justify the dependency cost. A self-contained sparse SGD implementation is feasible within the 500-line file constraint but introduces algorithmic risk.

**Risk.** Adding new leaves to a parent that has trained examples requires re-fitting that parent's classifier with all historical examples for that parent. Without persisting per-parent training corpora this is not possible incrementally. This violates hard constraint 2 unless per-parent training examples are retained (significant storage overhead) or the parent is re-fit from the full `MinedMailInfo` corpus (expensive full rebuild for that node).

---

### Recommendation

**Mainline: LCPPN + Hierarchical-Shrinkage Naive Bayes (option a).**

Justification:
- Fully satisfies hard constraint 2: all updates remain count-based. The shrinkage extension requires no new infrastructure beyond computing a weighted blend during scoring.
- Maximum reuse of existing `BayesianClassifierGroup`, `BayesianClassifierShared`, and `Corpus` types; lowest implementation risk.
- Directly addresses the root cause (siblings are independent in the flat model) by introducing parent-informed smoothing. Literature on hierarchical Bayesian text classification confirms meaningful F1 improvement over flat NB for hierarchical label structures with data-sparse leaves.
- Does not require new NuGet dependencies.
- The per-parent group structure also reduces the effective classification problem at each node to O(siblings) rather than O(all leaves), reducing variance regardless of smoothing.

**Cold-start fallback: flat NB (complement mode) per parent.**

For any parent node with fewer than `MinColdStartExamples` total examples across its children (configurable, suggested default 5), fall back to unsmoothed NB (the existing `BayesianClassifierShared` behavior). This is the current behavior and avoids noise from the smoothing weight when data is too sparse to estimate `λ` reliably.

**Rejected alternatives:**
- NBSVM-at-each-parent: higher F1 ceiling, but requires a self-contained online linear weight layer. The unbounded feature space and the need to maintain gradient state adds substantial complexity that is not justified before validating that hierarchical shrinkage NB does not meet the F1 target.
- Online linear/logistic: highest theoretical ceiling but violates hard constraint 2 for new-leaf scenarios without storing full per-parent training corpora, and requires either ML.NET (unjustified dependency) or a non-trivial custom SGD implementation.

---

## 4. Inference: Beam Search over Path Log-Probabilities

### Top-down descent with beam

At prediction time, the LCPPN predictor descends the folder tree from the root, at each node scoring the current beam's frontier entries' children using the per-parent NB classifier, and retaining the top-`B` candidates by cumulative log-probability.

Leaf probability: `log P(leaf | tokens) = Σ_{(p→c) on path} log P(c | p, tokens)`.

Per-node scoring reuses `BayesianClassifierShared.Chi2SpamProb` (or the smoothed variant) to produce a probability `P(c | p, tokens)` for each child `c` of parent `p`. The product of these along the path is the path probability.

### Beam width

Recommended default: `B = 3`. This is sufficient to recover from a single uncertain node decision without the O(B × branching_factor × depth) cost of wide beams. The beam width is exposed as a configurable integer parameter.

### Path-product probability and abstention

The path-product probability `P(leaf | tokens)` ranges (0, 1). Abstention is triggered when the top-scoring leaf's path probability falls below `MinimumPathProbability` (a new double field, analogous to the existing `MinimumProbability` on `BayesianClassifierGroup`). If no leaf clears the threshold, the predictor returns an empty result — this is "no prediction" (abstain).

**F1 accounting for abstentions.** Abstaining on a true-class example counts as a false negative for that class. Abstaining on a case where no class meets the threshold reduces precision denominator but increases recall denominator. For the offline evaluation harness (section 7), abstentions are treated as false negatives for the true class (no prediction issued = failure to predict correctly). This matches the existing flat predictor's abstention semantics.

---

## 5. Incremental Update Localization

### Single correction update path

When a user moves an email to leaf `L` with path `root → n₁ → n₂ → L`:
1. For each classifier on the path (classifier at root for child `n₁`, classifier at `n₁` for child `n₂`, classifier at `n₂` for child `L`):
   - Call the per-parent `BayesianClassifierGroup.Train(childSegment, tokens, 1)`.
   - This updates match counts in the child's `BayesianClassifierShared.Match` and shared parent counts in the group's `SharedTokenBase`.
2. If the email was previously filed to a different leaf `L'`, call `UnTrain` on each classifier along the path to `L'`.

This is O(depth) updates, each O(|tokens|). No global state is touched.

### New leaf addition

When a new folder is created at path `parent\NewLeaf`:
1. `parent`'s per-parent `BayesianClassifierGroup.Train("NewLeaf", [], 0)` is called with empty tokens and zero email count to register the new child. No data is available yet.
2. The first training example for `NewLeaf` triggers the cold-start fallback path (unsmoothed NB with `MinColdStartExamples` guard).
3. All classifiers at nodes other than `parent` are unaffected.

---

## 6. Backward Compatibility and Rollout

### Seam design

The existing callers address `Manager["Folder"]` and receive a `BayesianClassifierGroup`. All call sites use the following methods from that type:
- `Train(tag, tokens, emailCount)` — `SortEmail.cs:250`, `EmailFiler.cs:381`
- `UnTrain(tag, tokens, emailCount)` — `EmailFiler.cs:375`
- `Classify(tokenIncidence)` — implicit via `BayesianClassifierGroup.Classify`
- `Serialize()` — `EmailFiler.cs:369`

A backward-compatible introduction requires the hierarchical predictor to conform to the same public surface used by callers. Options:

**Option 1 (preferred): Introduce `IFolderPredictor` interface.**
Define a narrow interface with `Train`, `UnTrain`, `Classify`, and `Serialize`. Both `BayesianClassifierGroup` and the new `LcppnFolderPredictor` implement this interface. `Manager["Folder"]` is typed to `IFolderPredictor` (or a common base). A config flag `UseLcppnPredictor` selects which implementation is returned at startup.

This avoids subclassing `BayesianClassifierGroup` (which would complicate serialization) and makes the seam explicit. Callers require a one-line change to receive `IFolderPredictor` instead of `BayesianClassifierGroup`.

**Option 2: Subclass `BayesianClassifierGroup`.**
`LcppnFolderPredictor : BayesianClassifierGroup` overrides `Train`, `UnTrain`, `Classify`. This avoids any caller change but couples the hierarchical predictor to the flat type's serialized layout. Not recommended because `BayesianClassifierGroup`'s `Classifiers` dictionary is semantically meaningless for the LCPPN design, causing confusion and wasted deserialization.

**Recommendation: Option 1.** The interface approach is clean, testable, and does not carry forward the flat model's internal structure.

### Serialization strategy

The new `LcppnFolderPredictor` serializes as a separate JSON file using `SmartSerializable<LcppnFolderPredictor>` or `ClassifierGroupUtilities.SerializeAndSave`. Internal structure:
```json
{
  "Version": 1,
  "BeamWidth": 3,
  "MinimumPathProbability": 0.5,
  "Nodes": {
    "": { "Children": ["Projects", "Admin", ...], "Classifiers": { ... } },
    "Projects": { "Children": ["Alpha", ...], "Classifiers": { ... } },
    ...
  }
}
```

Each node's `Classifiers` is a `BayesianClassifierGroup` JSON subtree (already serializable). The `Nodes` dictionary key is the full relative path of the parent node (empty string for root). Newtonsoft.Json `TypeNameHandling.Auto` handles the polymorphic `BayesianClassifierGroup` entries.

The flat `BayesianClassifierGroup` model (`Folder.json`) is not deleted. When `UseLcppnPredictor = false`, the system loads `Folder.json` as before.

---

## 7. Evaluation Harness

### Approach: time-sliced offline evaluation

Inputs: a `MinedMailInfo[]` corpus sorted by a timestamp field (or corpus index as a proxy if timestamps are absent in the staged data).

Protocol:
1. Split by time: the first `TrainFraction` (e.g., 0.7) of examples are the training set; the remainder are the test set.
2. Build the flat `BayesianClassifierGroup` and the `LcppnFolderPredictor` from the training set.
3. For each test example, call `Classify` on both models, record the top prediction and path probability.
4. Compute leaf-level F1 (macro average over leaves appearing in the test set) for each model.
5. Record abstention rate separately (fraction of test examples where no prediction cleared the threshold).

**Abstention accounting.** An abstained example is counted as a false negative for its true class and as a true negative for all other classes (i.e., it does not inflate false positives). This matches the operational behavior.

**Determinism.** The split is deterministic (sorted index, not random). No external services, no temp files, no mutable global state. Fully unit-testable.

**Class:** `FolderPredictorEvaluator` in namespace `UtilitiesCS.EmailIntelligence.Evaluation`. Pure logic; accepts `IFolderPredictor` and `MinedMailInfo[]` as constructor arguments. No Outlook COM references.

---

## 8. Proposed C# Component Decomposition

All new files are under `UtilitiesCS/EmailIntelligence/` or `UtilitiesCS.Test/EmailIntelligence/`. All must pass CSharpier, .NET analyzers, nullable analysis, and MSTest at >= 90% coverage for new code.

### New production types

| Type | Namespace | Responsibility | Existing types reused |
|---|---|---|---|
| `IFolderPredictor` | `.Bayesian` | Narrow interface: `Train`, `UnTrain`, `Classify`, `Serialize` | None |
| `FolderHierarchyNode` | `.Bayesian` | Immutable record: `string NodeKey`, `string[] Children`. Serializable. | None |
| `FolderHierarchyTree` | `.Bayesian` | Builds and holds `Dictionary<string, FolderHierarchyNode>` from `RelativePath[]`. Pure logic; no I/O. | None |
| `PerParentClassifier` | `.Bayesian` | Wraps one `BayesianClassifierGroup` for a single parent node; exposes per-child scoring and count update. Manages the shrinkage smoothing coefficient. | `BayesianClassifierGroup`, `BayesianClassifierShared`, `Corpus` |
| `LcppnFolderPredictor` | `.Bayesian` | Implements `IFolderPredictor`. Holds `Dictionary<string, PerParentClassifier>`. Beam search descent, path-product probability, abstention logic, incremental update dispatch. Extends `SmartSerializable<LcppnFolderPredictor>`. | `FolderHierarchyTree`, `PerParentClassifier`, `Prediction<string>`, `SmartSerializable<T>` |
| `LcppnFolderPredictorConfig` | `.Bayesian` | Holds `BeamWidth`, `MinimumPathProbability`, `ShrinkageLambda`, `MinColdStartExamples`, `UseLcppnPredictor`. Serializable. | `NewSmartSerializableConfig` |
| `FolderPredictorEvaluator` | `.Evaluation` | Time-sliced F1 evaluation harness; pure logic. Accepts `IFolderPredictor`, `MinedMailInfo[]`, `EvaluationConfig`. Returns `EvaluationResult`. | `IFolderPredictor`, `MinedMailInfo` |
| `EvaluationResult` | `.Evaluation` | Value record: per-leaf precision/recall/F1, macro F1, abstention rate. | None |

### Existing types extended or unchanged

| Type | Change |
|---|---|
| `BayesianClassifierGroup` | Implement `IFolderPredictor` (additive; no breaking change to existing methods). |
| `OlFolderClassifierGroup` | Add `BuildLcppnPredictorAsync` method. Existing `BuildFolderClassifiersAsync` unchanged. |
| `EmailFiler` | Change `(await Manager["Folder"])` cast from `BayesianClassifierGroup` to `IFolderPredictor`. |
| `SortEmail` | Same cast change. |
| `Manager["Folder"]` internal type | Type parameter changes to `IFolderPredictor`. |

### File size constraint

- `LcppnFolderPredictor.cs`: estimated 350–450 lines (beam search, path scoring, incremental update, serialization setup).
- `PerParentClassifier.cs`: estimated 150–200 lines (smoothing blend, per-child probability delegation).
- `FolderHierarchyTree.cs`: estimated 80–120 lines.
- `FolderPredictorEvaluator.cs`: estimated 150–200 lines.

All under the 500-line limit.

### Test types

| Test class | Coverage focus |
|---|---|
| `FolderHierarchyTree_Tests` | Path parsing: single segment, multi-depth, duplicate paths, empty collection, case variants |
| `PerParentClassifier_Tests` | Smoothing blend correctness, cold-start fallback, incremental update (add token, remove token), probability normalization |
| `LcppnFolderPredictor_Tests` | Beam search descent, correct leaf returned, path probability computation, abstention behavior, incremental Train/UnTrain localizes to correct nodes, new-leaf addition |
| `FolderPredictorEvaluator_Tests` | F1 computation (precision/recall/macro), abstention counting as false negative, deterministic split |
| `LcppnFolderPredictor_Serialization_Tests` | Round-trip JSON via Newtonsoft, version field preserved, empty tree serializes cleanly |

---

## 9. Open Questions for Feature Spec

1. **Smoothing coefficient `λ`:** Should it be fixed (e.g., 0.7) or estimated from held-out data per node? A fixed value is simpler and avoids overfitting on small node counts. Recommend fixed with a configurable default.

2. **Root-node behavior:** Should the root node always predict (descend into the most-probable top-level folder) or can it abstain? Current flat model abstains globally. Recommend: root abstention is allowed; if the root-level prediction does not clear `MinimumPathProbability`, the predictor returns an empty result.

3. **CorpusInherit vs Corpus for per-parent shared token base:** The existing `CorpusInherit` has file-system serialization built in; `Corpus` does not. Per-parent classifiers should use `Corpus` (serialized as part of the `LcppnFolderPredictor` JSON tree) rather than separate `CorpusInherit` files, to avoid O(nodes) separate JSON files.

4. **Timestamp availability in `MinedMailInfo`:** The evaluation harness splits by index as a proxy for time. If a `ReceivedDate` or `SentDate` field is added to `MinedMailInfo`, the split should use it. This is a scope decision for the spec.

5. **Manager type parameter change:** Changing `Manager["Folder"]` from `BayesianClassifierGroup` to `IFolderPredictor` may require changes to the `ManagerAsyncLazy` type. The scope of that change needs confirmation.

6. **Feature flag delivery:** `LcppnFolderPredictorConfig.UseLcppnPredictor` should be configurable in the existing config infrastructure (wherever `IntelligenceConfig` or `loader.Config.ClassifierActivated` is set). Confirm the config write path before implementation.

---

## 10. Ranked Recommendation Summary

| Decision | Recommendation |
|---|---|
| Mainline decision model | LCPPN + Hierarchical-Shrinkage Naive Bayes (option a) |
| Cold-start fallback | Unsmoothed NB (existing `BayesianClassifierShared` behavior) when parent has < `MinColdStartExamples` total examples |
| Inference | Beam search over path log-probabilities, beam width `B = 3` (configurable) |
| Abstention | Path probability < `MinimumPathProbability` → empty result (false negative for true class in F1) |
| Seam/rollout | `IFolderPredictor` interface; `UseLcppnPredictor` config flag; existing flat model unchanged |
| Serialization | `SmartSerializable<LcppnFolderPredictor>` with Newtonsoft.Json; separate file from `Folder.json` |
| New NuGet dependencies | None |
| ML.NET | Not introduced; self-contained count-based implementation is sufficient |
