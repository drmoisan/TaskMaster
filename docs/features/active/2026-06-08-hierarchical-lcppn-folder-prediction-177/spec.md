# hierarchical-lcppn-folder-prediction — Spec

- **Issue:** #177
- **Parent (optional):** none
- **Owner:** TBD
- **Last Updated:** 2026-06-08T09-23
- **Status:** Draft
- **Version:** 0.2

## Overview

Brief summary of the behavior and scope.

TaskMaster currently predicts the destination Outlook folder for an email with a flat
Bayesian classifier (`BayesianClassifierGroup`), which holds one independent
`BayesianClassifierShared` per full leaf-folder path and scores every leaf against every
other leaf with no awareness that siblings share a parent in the folder tree. This discards
the hierarchical structure encoded in `FolderInfo.RelativePath` and limits leaf-level
prediction quality, especially for data-sparse leaves.

This feature introduces a hierarchy-aware predictor based on the **Local Classifier Per
Parent Node (LCPPN)** strategy. The folder tree is reconstructed from `RelativePath`; each
internal node owns a multiclass decision over its direct children; leaf probability is the
product of per-step conditional probabilities along the root-to-leaf path. The per-parent
decision model is hierarchical-shrinkage Naive Bayes that reuses the existing
`BayesianClassifierGroup` / `BayesianClassifierShared` / `Corpus` token-count machinery.
The new predictor is introduced behind an `IFolderPredictor` seam and a `UseLcppnPredictor`
config flag; the existing flat predictor and its serialized `Folder.json` remain functional
and selected by default.

- Target users/personas and primary use cases: Outlook users of the TaskMaster add-in who
  rely on auto-suggested filing folders when moving or sorting email. Primary use cases are
  (1) suggesting a destination leaf folder for an incoming or selected email, and
  (2) incrementally learning from user corrections without a full retrain.
- Success metrics or expected impact: higher leaf-level macro F1 than the flat predictor on
  a deterministic offline evaluation, while preserving the existing count-based incremental
  update property and the existing all-or-nothing abstention behavior. No new NuGet
  dependencies and no breaking change to the default (flat) prediction path.

## Behavior

Describe how the feature should behave end-to-end.

- Main user flow (happy path):
  1. **Hierarchy construction.** From the mined corpus the predictor parses each distinct
     `FolderInfo.RelativePath` on the backslash (`\`) delimiter and records each adjacent
     segment pair as a parent → child edge. The root parent key is the empty string. Each
     internal node owns a `PerParentClassifier` (a `BayesianClassifierGroup` keyed by direct
     child segment rather than full path).
  2. **Beam-search descent.** At prediction time the predictor descends from the root. At
     each frontier node it scores that node's children with the per-parent NB classifier and
     retains the top `BeamWidth` partial paths by cumulative path log-probability. Descent
     continues until frontier entries reach leaf nodes.
  3. **Path-product probability.** The probability of a candidate leaf is
     `P(leaf | tokens) = ∏ over (parent→child) on the path of P(child | parent, tokens)`,
     computed in log space. The predictor returns the highest-probability leaf with its
     path-product probability and an ordered list of alternative leaves.
  4. **Incremental update.** When a user files or corrects an email to leaf `L` with path
     `root → n₁ → … → L`, only the classifiers on that path are updated (`Train` on each
     parent for its child segment). If the email previously belonged to a different leaf,
     `UnTrain` is applied along the prior path. Updates remain count-based; no full retrain.

- Alternate/edge flows:
  - **Single-segment path** (a top-level folder directly under the archive root, e.g.,
    `"Inbox"`): one edge `root → "Inbox"`; the node is both a root child and a leaf with zero
    children.
  - **New-leaf handling.** When a new folder `parent\NewLeaf` appears, only `parent`'s
    `PerParentClassifier` registers the new child; all other classifiers are unaffected. The
    first training examples for `NewLeaf` use the cold-start fallback.
  - **Cold-start fallback.** For any parent whose children hold fewer than
    `MinColdStartExamples` total examples, per-child scoring falls back to unsmoothed Naive
    Bayes (the existing `BayesianClassifierShared` behavior) instead of the shrinkage blend.
  - **Reparented folder.** Incremental reparenting is not supported. A moved subtree is
    handled by a full rebuild of the predictor from the corpus.

- Error handling and recovery behavior:
  - **Abstention.** If the top leaf's path-product probability is below
    `MinimumPathProbability`, the predictor returns an empty result ("no prediction").
  - **Root abstention is allowed.** If no root-level child clears the threshold, the
    predictor returns an empty result rather than forcing a top-level descent.
  - **F1 accounting.** In the offline evaluation harness an abstained example is counted as a
    false negative for its true class and a true negative for all other classes (it does not
    inflate false positives). This matches the operational behavior of the flat predictor.

## Inputs / Outputs

- Inputs (CLI flags, files, env vars):
  - Mined `MinedMailInfo[]` corpus where each example carries `FolderInfo.RelativePath`
    (backslash-delimited hierarchical folder label) and tokenized features.
  - Serialized predictor state (separate JSON file from `Folder.json`) on load.
- Outputs (artifacts, logs, telemetry):
  - A leaf-folder prediction: top `Class` (full relative path), path-product `Probability`,
    and an ordered list of alternative `Prediction<string>` entries; or an empty result on
    abstention.
  - Serialized `LcppnFolderPredictor` JSON (the per-parent tree and counts).
  - `EvaluationResult` from the offline harness (per-leaf precision/recall/F1, macro F1,
    abstention rate).
- Config keys and defaults (`LcppnFolderPredictorConfig`):
  - `UseLcppnPredictor` (bool, default **false**) — selects the LCPPN predictor; default
    preserves the flat predictor.
  - `BeamWidth` (int, default **3**) — beam width for path descent.
  - `MinimumPathProbability` (double, default **0.5**) — abstention threshold on the
    path-product probability.
  - `ShrinkageLambda` (double, default **0.7**) — weight on the leaf estimate in the
    parent-informed smoothing blend `λ·P_leaf(t|c) + (1-λ)·P_parent(t|p)`.
  - `MinColdStartExamples` (int, default **5**) — minimum total examples under a parent before
    the shrinkage blend is applied; below it, unsmoothed NB is used.
- Versioning or backward-compatibility constraints:
  - The flat `BayesianClassifierGroup` and its serialized `Folder.json` remain fully
    functional and are used when `UseLcppnPredictor = false`.
  - The LCPPN serialized state carries a `Version` field for forward migration.

## API / CLI Surface

List commands, flags, request/response shapes, and examples.

- **`IFolderPredictor` interface** (`UtilitiesCS.EmailIntelligence.Bayesian`). Narrow contract
  implemented by both predictors:
  - `Train(tag, tokens, emailCount)` — incremental count-based training for a tag.
  - `UnTrain(tag, tokens, emailCount)` — incremental count-based untraining for a tag.
  - `Classify(tokenIncidence)` — returns ordered predictions (or empty on abstention).
  - `Serialize()` — persists predictor state.
- New production types (per research §8 component table):
  - `FolderHierarchyNode` — immutable record: `string NodeKey`, `string[] Children`;
    serializable.
  - `FolderHierarchyTree` — builds and holds `Dictionary<string, FolderHierarchyNode>` from
    `RelativePath[]`; pure logic, no I/O.
  - `PerParentClassifier` — wraps one `BayesianClassifierGroup` for a single parent node;
    exposes per-child scoring with the shrinkage blend and count update; implements the
    cold-start fallback.
  - `LcppnFolderPredictor : SmartSerializable<LcppnFolderPredictor>` — implements
    `IFolderPredictor`; holds `Dictionary<string, PerParentClassifier>`; beam-search descent,
    path-product probability, abstention, and incremental-update dispatch.
  - `LcppnFolderPredictorConfig` — holds the config keys above; serializable.
  - `FolderPredictorEvaluator` (`.Evaluation`) — time-sliced offline F1 harness; pure logic;
    accepts `IFolderPredictor`, `MinedMailInfo[]`, `EvaluationConfig`.
  - `EvaluationResult` (`.Evaluation`) — value record: per-leaf precision/recall/F1, macro F1,
    abstention rate.
- Example invocations with expected outputs (concise):
  - `predictor.Classify(tokens)` on a corpus with `Projects\Alpha\2024` returns top
    `Class = "Projects\\Alpha\\2024"` with `Probability` equal to the product of
    `P(Projects|root)·P(Alpha|Projects)·P(2024|Alpha)`, plus ordered alternatives.
  - With `MinimumPathProbability = 0.5`, an input whose best path product is `0.4` returns an
    empty result (abstain).
- Contracts and validation rules:
  - `BeamWidth >= 1`; `0 < MinimumPathProbability < 1`; `0 <= ShrinkageLambda <= 1`;
    `MinColdStartExamples >= 0`. Construction validates these invariants and fails fast on
    violation.

## Data & State

Data flow, storage, or state changes introduced by this feature.

- Data transformations and invariants:
  - `RelativePath` strings are parsed into a parent→children hierarchy; the parsing is
    idempotent (duplicate paths deduplicate via a per-parent child set).
  - Each per-parent classifier's `Classifiers` dictionary is keyed by **direct child segment**,
    not full path. Its `SharedTokenBase` is the parent-scoped token corpus.
  - Path comparisons default to ordinal; an `OrdinalIgnoreCase` option is configurable.
- Caching or persistence details:
  - `LcppnFolderPredictor` serializes via `SmartSerializable<LcppnFolderPredictor>` (Newtonsoft.Json,
    `TypeNameHandling.Auto`) to a **separate file** from `Folder.json`. The serialized shape is a
    `Nodes` dictionary keyed by full parent path (empty string for root), each holding the
    node's children and its `BayesianClassifierGroup` subtree, plus top-level `Version`,
    `BeamWidth`, and `MinimumPathProbability`.
  - Per-parent shared token base uses `Corpus` (serialized inline as part of the predictor
    JSON), **not** `CorpusInherit`, to avoid producing O(nodes) separate JSON files.
- Migration or backfill requirements (if any):
  - None for the default path. The flat `Folder.json` is neither read nor modified by the
    LCPPN predictor. When `UseLcppnPredictor = true` and no LCPPN state exists, the predictor
    is built from the mined corpus.

## Constraints & Risks

Performance, compatibility, security, rollout constraints.

- Limits (latency/throughput/memory) and acceptable trade-offs:
  - No production, test, or reusable script file may exceed **500 lines**. Estimated sizes
    keep all new files within the limit (`LcppnFolderPredictor.cs` 350–450,
    `PerParentClassifier.cs` 150–200, `FolderHierarchyTree.cs` 80–120,
    `FolderPredictorEvaluator.cs` 150–200).
  - Beam-search descent cost is O(BeamWidth × branching_factor × depth) per prediction.
    `BeamWidth = 3` recovers from a single uncertain decision without wide-beam cost.
  - Incremental update is O(depth) per correction, each O(|tokens|) — the same asymptotic
    cost as the current single-tag update.
- Security/privacy considerations:
  - No new external services or network calls. Predictor state stays in the existing local
    serialized-model storage. No new PII surface beyond what the flat model already persists.
- Operational/rollout risks and mitigations:
  - **Pure logic must be separated from Outlook COM.** All new prediction and evaluation logic
    is testable without Outlook COM or external services; COM interaction stays in the existing
    `OlFolderClassifierGroup` build path.
  - **No temporary files in tests** (repository policy). Serialization round-trip tests use
    in-memory JSON.
  - **Reparenting** is handled by full rebuild, not incremental update; this is documented and
    treated as a rare rebuild trigger.
  - **Rollout behind a flag.** `UseLcppnPredictor` defaults to false; the flat predictor and
    `Folder.json` remain the default path so the change is non-breaking.

## Implementation Strategy

- Implementation scope (what changes, not sequencing):
  - Add the new production types listed under API / CLI Surface.
  - Extend `BayesianClassifierGroup` to implement `IFolderPredictor` (additive; no breaking
    change to existing methods).
  - Add `OlFolderClassifierGroup.BuildLcppnPredictorAsync`; leave `BuildFolderClassifiersAsync`
    unchanged.
  - Change the `(await Manager["Folder"])` cast in `EmailFiler` and `SortEmail` from
    `BayesianClassifierGroup` to `IFolderPredictor`; the `Manager["Folder"]` type parameter
    changes to `IFolderPredictor`.
- New classes/functions/commands to add or update:
  - New: `IFolderPredictor`, `FolderHierarchyNode`, `FolderHierarchyTree`, `PerParentClassifier`,
    `LcppnFolderPredictor`, `LcppnFolderPredictorConfig`, `FolderPredictorEvaluator`,
    `EvaluationResult`.
  - Reused unchanged: `BayesianClassifierShared`, `Corpus`, `Prediction<string>`,
    `SmartSerializable<T>`.
- Dependency changes (new/removed packages) and rationale:
  - **None.** ML.NET is explicitly not introduced; a self-contained count-based design is
    sufficient and avoids a heavyweight dependency with a dynamic token vocabulary mismatch.
- Logging/telemetry additions and locations:
  - Use the project's established logging pattern for build and load of the LCPPN predictor and
    for abstention/threshold decisions where the flat predictor already logs. No ad-hoc console
    output.
- Rollout plan (feature flags, staged deploys, fallback path):
  - Introduce behind `UseLcppnPredictor` (default false). The flat predictor remains the
    fallback. The `IFolderPredictor` seam selects the active implementation at startup so both
    paths coexist without breaking callers.

## Definition of Done

- [ ] Acceptance criteria documented and mapped to tests or demos
- [ ] Behavior matches acceptance criteria in all documented environments
- [ ] Tests updated/added (unit/integration as applicable) using MSTest + Moq + FluentAssertions
- [ ] Edge cases and error handling covered by tests (single-segment, new-leaf, cold-start,
      abstention, incremental-update localization)
- [ ] New modules/classes meet >= 90% coverage; repository-wide coverage stays >= 80%
- [ ] Docs updated (README, docs/features/active/... links)
- [ ] Telemetry/logging added or updated (if applicable)
- [ ] Full C# toolchain pass completed in order: CSharpier → .NET analyzers → nullable →
      MSTest (vstest with code coverage), restarting from the start on any failure or auto-fix
