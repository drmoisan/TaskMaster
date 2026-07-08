# `hierarchical-lcppn-folder-prediction` — User Story

- Issue: #177
- Owner: TBD
- Status: Draft
- Last Updated: 2026-06-08T09-23

## Story Statement

- As an Outlook user of the TaskMaster add-in, I want the suggested filing folder to account
  for the folder tree structure, so that near-sibling folders are not crowded out by unrelated
  distant leaves and the top suggestion is more often correct.
- As an Outlook user of the TaskMaster add-in, I want my corrections to be learned immediately
  and locally, so that the predictor improves without a slow full retrain and without changing
  unrelated folders.

## Problem / Why

TaskMaster predicts the destination Outlook folder with a flat Bayesian classifier that scores
every leaf folder independently, with no awareness that siblings share a parent. Distant leaves
compete directly with near siblings, and data-sparse leaves get noisy estimates. This limits
leaf-level prediction quality. Reconstructing the folder hierarchy from `FolderInfo.RelativePath`
and deciding per parent node (LCPPN) lets sibling folders share parent-informed statistics and
reduces each decision to a choice among direct children, which is expected to raise leaf-level F1
while preserving the current incremental-update and abstention behavior.

## Personas & Scenarios

- Persona: **Outlook add-in user (knowledge worker filing email)**
  - who the user is: a TaskMaster user who files large volumes of email into a deep, branching
    archive folder tree (e.g., `Projects\Alpha\2024`).
  - what they care about: the top auto-suggested folder being correct, and corrections being
    learned quickly.
  - their constraints: prefers not to wait for retraining; does not want filing behavior in one
    branch to disturb predictions in unrelated branches.
  - their goals and frustrations: wants fewer mis-suggestions among sibling folders; frustrated
    when an unrelated distant folder outranks the correct nearby one.
  - their context and motivations: works inside Outlook on Windows with the TaskMaster VSTO
    add-in; motivated to keep a clean, accurately filed archive with minimal manual effort.
- Scenario: **Filing an email with a hierarchy-aware suggestion**
  - who is acting: the add-in user moving or sorting an email.
  - what triggered the action: the user selects an email and invokes filing, or auto-sort runs.
  - what steps do they take: the predictor descends the folder tree from the root using beam
    search, combining per-step child probabilities into a path-product probability, and presents
    the top leaf folder plus ordered alternatives.
  - what obstacles or decisions occur: if no path clears `MinimumPathProbability` the predictor
    abstains (no suggestion) rather than offering a low-confidence guess. If the user corrects the
    suggestion, only the classifiers along the corrected root-to-leaf path are updated.
  - what outcome do they expect: a correct (or clearly ordered) folder suggestion most of the
    time, immediate learning from corrections, and no disturbance to unrelated branches.

## Acceptance Criteria

- [x] **AC1 — Hierarchy construction from RelativePath.** Given a set of `RelativePath` values,
      `FolderHierarchyTree` builds a parent→children map by splitting each path on backslash and
      recording each adjacent segment pair as a parent→child edge, with the root parent key being
      the empty string.
- [x] **AC2 — Single-segment path edge case.** A single-segment `RelativePath` (e.g., `"Inbox"`)
      produces exactly one edge `root → "Inbox"`, and that node is recorded as both a child of the
      root and a leaf with zero children.
- [x] **AC3 — Idempotent / duplicate-path construction.** Building the tree from a collection
      containing duplicate `RelativePath` values yields the same tree as the distinct set; per-parent
      child sets contain no duplicates.
- [x] **AC4 — New-leaf construction.** Adding a previously unseen leaf `parent\NewLeaf` adds the
      child to `parent`'s child set only and does not alter any other parent's children.
- [x] **AC5 — LCPPN beam-search descent returns a leaf with path-product probability.**
      `LcppnFolderPredictor.Classify(tokens)` descends from the root via beam search and returns a
      top leaf whose `Probability` equals the product of per-step conditional probabilities along its
      root-to-leaf path, together with an ordered list of alternative leaf predictions.
- [x] **AC6 — Configurable beam width.** `BeamWidth` is configurable (default 3). With a beam wide
      enough to retain a branch that a greedy (width-1) descent would discard, the predictor returns
      the correct leaf in a constructed case where width-1 would not; construction validates
      `BeamWidth >= 1`.
- [x] **AC7 — Abstention semantics.** When the top leaf's path-product probability is below
      `MinimumPathProbability`, `Classify` returns an empty result (no prediction). Root abstention is
      allowed: if no root-level child clears the threshold, the result is empty.
- [x] **AC8 — F1 accounting for abstention.** In `FolderPredictorEvaluator`, an abstained test
      example is counted as a false negative for its true class and a true negative for all other
      classes (it does not increment any false-positive count).
- [x] **AC9 — Shrinkage smoothing with configurable lambda.** Per-child scoring blends the leaf and
      parent token estimates as `λ·P_leaf(t|c) + (1-λ)·P_parent(t|p)` with `ShrinkageLambda` (default
      0.7); construction validates `0 <= ShrinkageLambda <= 1`.
- [x] **AC10 — Cold-start fallback.** When the total examples under a parent are fewer than
      `MinColdStartExamples` (default 5), per-child scoring uses unsmoothed Naive Bayes (the existing
      `BayesianClassifierShared` behavior) instead of the shrinkage blend.
- [x] **AC11 — Localized incremental update.** Training a corrected example on leaf `L` with path
      `root → n₁ → … → L` updates only the classifiers on that path; classifiers at nodes not on the
      path have unchanged counts and probabilities. If the example previously belonged to a different
      leaf, `UnTrain` is applied along the prior path only.
- [x] **AC12 — New-leaf addition is local.** Registering a new leaf under an existing parent modifies
      only that parent's `PerParentClassifier`; all other per-parent classifiers are unchanged.
- [x] **AC13 — Backward compatibility (flat predictor).** When `UseLcppnPredictor = false`, the
      existing flat `BayesianClassifierGroup` is used, its `Train` / `UnTrain` / `Classify` / `Serialize`
      behavior is unchanged, and `Folder.json` is loaded and written exactly as before.
- [x] **AC14 — Shared `IFolderPredictor` seam.** Both `BayesianClassifierGroup` and
      `LcppnFolderPredictor` implement `IFolderPredictor`, and `Manager["Folder"]` callers
      (`EmailFiler`, `SortEmail`) operate through that interface; the flat implementation requires no
      change to its existing method behavior.
- [x] **AC15 — Serialization round-trip.** `LcppnFolderPredictor` serializes via
      `SmartSerializable<LcppnFolderPredictor>` (Newtonsoft.Json) to a file separate from `Folder.json`
      and round-trips losslessly, preserving the `Version` field, the per-parent tree, and counts; an
      empty tree serializes and deserializes cleanly. Per-parent shared token base uses `Corpus`
      serialized inline (not `CorpusInherit`).
- [x] **AC16 — Deterministic evaluation harness.** `FolderPredictorEvaluator` performs a time-sliced
      (index-proxy) split into train/test, builds the predictor from the train slice, evaluates the
      test slice, and produces per-leaf precision/recall/F1, macro F1, and abstention rate. The split
      and result are deterministic for the same input, with no Outlook COM, no external services, and
      no temporary files.
- [x] **AC17 — Test stack and isolation.** All new tests use MSTest with Moq and FluentAssertions,
      are independent and deterministic, create no temporary files, and depend on no external services.
- [x] **AC18 — Coverage.** New modules/classes reach >= 90% line coverage, and repository-wide line
      coverage remains >= 80%; coverage for changed lines does not regress.
- [x] **AC19 — Toolchain.** The full C# toolchain passes in order — CSharpier formatting, .NET
      analyzers, nullable analysis (TreatWarningsAsErrors), and MSTest via vstest with code coverage —
      restarting from the start on any failure or auto-fix.
- [x] **AC20 — File-size and separation constraints.** No new production, test, or reusable script
      file exceeds 500 lines, and all new prediction and evaluation logic is pure and testable without
      Outlook COM.

### Cycle 3 — production migration (added 2026-06-16; option B, default-ON)

- [x] **AC21 — Production enablement, default ON via reachable config.** The `UseLcppnPredictor`
      setting is sourced from the application's persistent settings/config rather than a hard-coded
      per-instance default, defaults to ON (`true`), and is honored by the production callers
      (`EmailFiler`, `SortEmail`, `FolderScorer`) so the LCPPN predictor is selected at runtime in
      production. The setting remains toggleable to OFF, which restores flat-only behavior (AC13
      preserved). No per-call site is required to hand-set the flag.
- [x] **AC22 — Safe fallback to flat.** When the setting is ON but `Globals.AF.FolderPredictor` is
      null or not yet built (first run before build, or load failure), `GetFolderPredictorAsync`
      returns the flat `BayesianClassifierGroup` without throwing. Covered by a regression test.
- [x] **AC23 — Persistence and load-on-startup.** `LcppnFolderPredictor` is serialized to its own
      file (distinct from `Folder.json`) and is rehydrated into `Globals.AF.FolderPredictor` at
      application startup (via the `AppAutoFileObjects` load path / `Manager.Configuration`
      registration), so it survives an application restart without requiring a manual
      `BuildClassifiersAsync` rerun. If the persisted file is absent or unreadable, the holder stays
      null and the accessor falls back to flat (AC22). Covered by serialization round-trip and
      load-path tests.
- [x] **AC24 — Containment and non-regression.** Spam/triage/category/actionable subsystems and the
      `ManagerAsyncLazy` value typing remain unchanged (zero diff); AC1–AC20 remain satisfied; new and
      changed lines meet coverage policy (new code >= 90% strict, repository-wide >= 80%); the full C#
      toolchain passes in order in a single final pass.

### Cycle 4 — root-cause hardening (added 2026-06-16; closed no-fix-required)

- [x] **AC25 — FilePathHelper deserialize-safe.** SATISFIED ON HEAD WITH NO CODE CHANGE REQUIRED.
      Investigation (two independent confirmations — an empirical executor probe across five document
      orderings and a code-path trace, recorded in
      `artifacts/research/2026-06-16-lcppn-deserialize-nre-research.md`) established that the
      previously-reported `FilePathHelper` deserialize `NullReferenceException` is **not reproducible on
      HEAD**: (a) `StemInitialized()` never returns true while `_fileExtension` is null because
      `TryParseFileName()` self-heals the stem backing fields before the `AdjustForMaxPath()`
      dereference; and (b) the production LCPPN load path excludes `Config` entirely via the cycle-3
      `DoNotSerializeContractResolver("Config")` in `LcppnFolderPredictorStore`, so `FilePathHelper` is
      never instantiated by Newtonsoft on that path. The cycle-3 throw was a stale-document/transient
      already neutralized by that (contract-correct) exclusion, which is retained. A proposed
      `AdjustForMaxPath()` null-guard would be unfalsifiable defensive hardening (no honest
      red-before-green test is achievable), so per the repository bugfix discipline no production change
      was made. Deserialize-safety is therefore met on HEAD. Cycle 4 closed as no-fix-required; AC1–AC24
      unchanged and not regressed.

## Non-Goals

Call out what is explicitly excluded from this feature.

- No ML.NET (or any new NuGet dependency) is introduced; the implementation is self-contained and
  count-based.
- No online discriminative learner (NBSVM or online logistic/SGD per-parent classifier) in this
  iteration; only hierarchical-shrinkage Naive Bayes with a cold-start NB fallback.
- No incremental reparenting; a moved folder subtree is handled by a full rebuild.
- No embedding-based or learned feature representations; only the existing token-count features.
- No removal of or breaking change to the flat predictor or its serialized `Folder.json`. As of the
  cycle-3 migration (option B), the flat predictor is retained as the runtime fallback path rather
  than the default selection; LCPPN becomes the default-ON selection per AC21, and flat-only behavior
  is still reachable by toggling the setting OFF (AC13).
- Extending LCPPN to non-folder classifiers (spam, triage, category/multiclass, actionable) remains
  out of scope.
- Retiring the always-on flat rebuild in `BuildClassifiersAsync` is out of scope; the flat group is
  intentionally still built and serialized to serve as the fallback.
