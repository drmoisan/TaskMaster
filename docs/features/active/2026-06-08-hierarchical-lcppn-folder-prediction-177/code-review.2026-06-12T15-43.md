# Code Review: hierarchical-lcppn-folder-prediction (#177)

**Review Date:** 2026-06-12
**Reviewer:** feature-reviewer agent
**Feature Folder:** `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177`
**Base Branch:** `main` (merge-base `742d4f1656367ddb1d43ea66e1bdd59776f1a287`)
**Head Branch:** `TaskMaster-wt-2026-06-08-12-06` (head `d06f5c00`)
**Review Type:** Initial review

---

## Executive Summary

This change adds a hierarchy-aware Local Classifier Per Parent Node (LCPPN) folder predictor for the TaskMaster Outlook add-in, introduced behind an `IFolderPredictor` seam and a default-off `UseLcppnPredictor` flag. The new production surface comprises eight types across the `Bayesian` and `Evaluation` namespaces (interface, hierarchy model, per-parent classifier, predictor, config, evaluator, and two value objects). Existing callers (`EmailFiler`, `SortEmail`, `FolderScorer`) are re-pointed from a direct `BayesianClassifierGroup` cast of `Manager["Folder"]` to the new seam accessor. The implementation is count-based and self-contained: no new NuGet dependency, no ML.NET, and the flat predictor and its `Folder.json` remain the default path.

**What changed:**
The flat `BayesianClassifierGroup` now declares `IFolderPredictor` (a two-line additive change with no method modification). `OlFolderClassifierGroup` gains a flag-gated seam (`GetFolderPredictorAsync`, `BuildLcppnPredictorAsync`, `SetLcppnPredictor`, and a `_lcppnPredictor` holder). New pure types implement hierarchy construction (`FolderHierarchyTree`/`FolderHierarchyNode`), per-parent hierarchical-shrinkage Naive Bayes with cold-start fallback (`PerParentClassifier`), beam-search descent with path-product probability and abstention (`LcppnFolderPredictor`), validated configuration (`LcppnFolderPredictorConfig`), and a deterministic offline F1 harness (`FolderPredictorEvaluator`/`EvaluationResult`). Eight new MSTest files cover these. The shared `ConcurrentObservableDictionary<string, AsyncLazy<BayesianClassifierGroup>>` value type and `ManagerAsyncLazy.cs` are unchanged.

**Top 3 risks:**
1. Latent seam wiring gap: the production callers construct a fresh `new OlFolderClassifierGroup(globals)` per call, so the flag-on LCPPN holder (instance state set at build time on a different instance) is never reached in production. With the flag off this is harmless, but the LCPPN path is not actually live for any caller as written.
2. Strict new-code line coverage for `FolderHierarchyTree`, `FolderHierarchyNode`, and `LcppnFolderPredictor` is below 90% (inclusive ≥ 91.4%); a few defensive branches are unexercised.
3. `BayesianClassifierGroup.cs` remains over the 500-line cap (515) after this change.

**PR readiness recommendation:** **Conditional Go** — the default-off behavior is sound and well-tested; the seam wiring gap should be resolved before the `UseLcppnPredictor` flag is enabled in production, and the coverage/file-size items are non-blocking follow-ups.

---

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Major | `UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/OlFolderClassifierGroup.cs` | lines 38, 78-90, 274-282 + callers | `_lcppnPredictor` and `FolderPredictorConfig` are per-instance state; production callers (`EmailFiler.GetFolderPredictorAsync`, `SortEmail`, `FolderScorer`) call `new OlFolderClassifierGroup(globals).GetFolderPredictorAsync()` on a fresh instance with the default flag-off config and a null holder, so the flag-on LCPPN predictor is unreachable in production. | Persist the config and the built predictor where the callers can resolve them (e.g., hold the predictor in the `Manager` or a shared singleton), or have callers reuse the same group instance that ran the build. | The wired LCPPN path cannot activate for real callers; enabling the flag would silently keep using the flat predictor. | Diff of the three callers vs base; `OlFolderClassifierGroup.cs` lines 38/85-87; grep of `_lcppnPredictor`/`FolderPredictorConfig` shows no production setter outside the build site. |
| Minor | `UtilitiesCS/EmailIntelligence/Bayesian/BayesianClassifierGroup.cs` | whole file | Modified file is 515 lines (baseline 513), over the 500-line cap; the +2 came from the interface declaration. | Split the class in a separate refactor to bring it under the cap. | Repo policy applies the 500-line cap to changed files; overage is pre-existing but now touched. | `awk END{NR}` head 515 vs `git show base:` 513. |
| Minor | `UtilitiesCS/EmailIntelligence/Bayesian/FolderHierarchyTree.cs`, `LcppnFolderPredictor.cs`, `FolderHierarchyNode.cs` | n/a | Strict new-code line coverage below 90% (86.4% / 89.1% / 60.0%); inclusive ≥ 91.4%. | Add tests for `GetChildren`/`NodeKeys` accessors and the uncovered descent branches; record-member coverage on `FolderHierarchyNode` is auto-generated. | New modules target ≥ 90% per policy; strict metric is short. | Per-type aggregation of `artifacts/csharp/coverage.xml`; `coverage-comparison.md`. |
| Info | `UtilitiesCS/EmailIntelligence/EmailParsingSorting/{EmailFiler,SortEmail}.cs`, `OutlookObjects/Folder/FolderScorer.cs` | seam call sites | Each caller allocates a new `OlFolderClassifierGroup` per prediction/train call. With the flag off this only defers to `Manager["Folder"]`, so behavior is unchanged, but it adds a small per-call allocation. | Acceptable for the default path; tie to the Major-finding fix. | No functional change at flag-off; minor allocation. | Diffs of the three callers. |

No Blocker findings.

---

## Implementation Audit

### C# implementation audit

#### What changed well

- Strong separation of pure logic from Outlook COM: `FolderHierarchyTree`, `PerParentClassifier`, `LcppnFolderPredictor`, and the evaluation harness contain no `Microsoft.Office.Interop.Outlook` types and are fully testable in-memory. COM interaction stays in `OlFolderClassifierGroup`.
- Maximum reuse: `PerParentClassifier` wraps an existing `BayesianClassifierGroup` keyed by direct child segment and reuses `BayesianClassifierShared`/`Corpus` without modification, satisfying the "reuse count machinery" intent.
- Numerically careful scoring: `PerParentClassifier.ScoreChildren` computes log-scores then a stable softmax (subtracting the max) and guards `sum <= 0` with a uniform fallback; `DescendBeam` uses `Math.Log(Math.Max(score, double.Epsilon))` to avoid `log(0)`.
- Backward compatibility is structurally guaranteed: the only change to `BayesianClassifierGroup` is the interface declaration; `GetFolderPredictorAsync` returns the unchanged `Manager["Folder"]` when the flag is off, so the flat path is byte-for-byte unchanged.
- Serialization design is documented and justified: `Nodes` is serialized inline via `Corpus` (not `CorpusInherit`) to avoid O(nodes) JSON files; `Tree` is `[JsonIgnore]` and rebuilt via `OnDeserialized`, avoiding a redundant structure copy.

#### Type safety and API notes

- Nullable build is clean (TreatWarningsAsErrors). Guard clauses and null-coalescing are used at every public boundary (`Build`, `Train`, `Classify`, evaluator constructor).
- The `IFolderPredictor` interface is intentionally narrow (exactly the four members callers use) with signatures matched to the flat predictor so it satisfies the interface without behavior change.
- Construction-time invariant validation is consistent: `LcppnFolderPredictorConfig.Validate`, `PerParentClassifier.ValidateInvariants` (re-run on deserialization), and `EvaluationConfig` constructor all fail fast with `ArgumentOutOfRangeException` and explicit messages.

#### Error handling and logging

- Exceptions are specific and fail fast; no broad `catch (Exception)` was introduced.
- No ad-hoc console output added; the existing log4net logger field is retained in `BayesianClassifierGroup`.
- Resource/lifecycle: async build uses `Task.Run`; no new disposable resources are introduced.

---

## Test Quality Audit

The feature ships 77 MSTest tests across eight files. The QA-gate evidence reports all four toolchain steps clean in a single final pass and the feature suite passing deterministically across repeated runs.

### Reviewed test and QA artifacts

- `UtilitiesCS.Test/EmailIntelligence/Bayesian/LcppnFolderPredictor_Tests.cs` — beam descent, configurable beam width recovering a branch greedy would drop, abstention, localized update; deterministic, in-memory.
- `UtilitiesCS.Test/EmailIntelligence/Bayesian/PerParentClassifier_Tests.cs` — shrinkage blend vs cold-start fallback, construction validation, local new-child registration.
- `UtilitiesCS.Test/EmailIntelligence/Bayesian/LcppnFolderPredictor_Serialization_Tests.cs` — in-memory JSON round-trip preserving Version/tree/counts and empty-tree handling.
- `UtilitiesCS.Test/EmailIntelligence/FolderPredictorSeam_Tests.cs` — flag-off returns flat predictor; flag-on with held predictor returns LCPPN; both reachable as `IFolderPredictor`; flag-on-no-holder falls back to flat.
- `UtilitiesCS.Test/EmailIntelligence/Evaluation/FolderPredictorEvaluator_Tests.cs` — deterministic index split, abstention F1 accounting.
- `docs/.../evidence/qa-gates/2026-06-12T15-26/{QA-GATE,coverage-comparison,test-stack-audit}.md` — toolchain results and coverage comparison.
- `artifacts/csharp/coverage.xml` — canonical post-change coverage (independently re-parsed: UtilitiesCS.dll 85.40% strict).

### Quality assessment prompts

- **Determinism:** No randomness, time, or I/O in new code; evaluator split is index-based; serialization is in-memory. Confirmed by grep for temp-file/network/process APIs (no matches).
- **Isolation:** Each test targets a single behavior; fixtures are constructed per test.
- **Speed:** In-memory; part of a 3890-test suite within the established run window.
- **Diagnostics:** FluentAssertions with `because` reasons gives clear failure messages.

The seam tests verify the flag-on path only on a single instance that both holds the predictor and serves the accessor; they do not exercise the production caller pattern (fresh per-call instance), which is why the Major wiring-gap finding is not caught by the test suite.

---

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets in code | ✅ PASS | No credentials/keys in the diff; count-based model only. |
| No unsafe subprocess or command construction | ✅ PASS | No `Process.Start` or shell invocation in new code. |
| Input validation at boundaries | ✅ PASS | Null/empty/range guards on all public entry points. |
| Error handling remains explicit | ✅ PASS | Specific exceptions; no broad catch introduced. |
| Configuration / path handling is safe | ✅ PASS | Path parsing splits on backslash and filters empty segments; no filesystem path traversal; serialized state is a separate JSON file from `Folder.json`. |
| Containment (Option B) | ✅ PASS | `ManagerAsyncLazy.cs` zero diff; `Triage.cs`/`SpamBayes.cs`/`CategoryClassifierGroup.cs`/`MulticlassEngine.cs` unchanged; shared dictionary value type unchanged. |
| Tonality of evidence/docs | ✅ PASS | Evidence and feature docs use neutral, factual language consistent with the Tonality Policy. |

---

## Research Log

No external research was required. Review was based on diff inspection against the resolved merge-base, direct reading of the changed source files, the feature requirement documents, and re-parsing of the canonical coverage artifact.

---

## Verdict

The implementation is well-structured, reuses existing machinery, preserves the default flat path, and is supported by deterministic tests and a clean toolchain pass. The default-off behavior is correct and low-risk, so the change is suitable for normal PR flow. One Major item should be resolved before the `UseLcppnPredictor` flag is turned on in production: as written, the production callers construct a fresh `OlFolderClassifierGroup` per call and therefore never see the flag-on LCPPN holder, so enabling the flag would not actually activate the LCPPN predictor. The strict-coverage shortfall on three new types and the over-cap line count of `BayesianClassifierGroup.cs` are non-blocking follow-ups. Recommendation: Conditional Go, consistent with the Findings Table.
