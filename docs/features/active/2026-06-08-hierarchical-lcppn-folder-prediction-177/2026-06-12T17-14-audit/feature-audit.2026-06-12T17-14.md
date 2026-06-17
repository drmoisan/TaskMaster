# Feature Audit: hierarchical-lcppn-folder-prediction (#177) — Cycle 2 Exit Reaudit

**Audit Date:** 2026-06-12 (exit timestamp 2026-06-12T17-14 UTC)
**Feature Folder:** `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177`
**Base Branch:** `main` (merge-base `742d4f1656367ddb1d43ea66e1bdd59776f1a287`)
**Head Branch:** `TaskMaster-wt-2026-06-08-12-06` (head `31cbb12e`)
**Work Mode:** `full-feature`
**Audit Type:** Remediation cycle 2 end-of-cycle acceptance reaudit

---

## Scope and Baseline

- **Base branch:** `main` (merge-base commit `742d4f1656367ddb1d43ea66e1bdd59776f1a287`)
- **Head branch/commit:** `TaskMaster-wt-2026-06-08-12-06` (commit `31cbb12e`)
- **Merge base:** `742d4f1656367ddb1d43ea66e1bdd59776f1a287`
- **Cumulative branch commits audited:** `0223bc60`, `d06f5c00`, `e159bead` (cycle-1 remediation), `f4ca954c` (cycle-1 exit audit), `31cbb12e` (cycle-2 remediation)
- **Evidence sources:**
  - Primary: full branch diff (`git diff <merge-base> HEAD`) and direct source inspection
  - Cycle-0 baseline: `evidence/baseline/coverage-p0/baseline.xml`
  - Feature evidence: `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/**`
  - Coverage: `artifacts/csharp/coverage.xml` (canonical, independently re-parsed)
  - Cycle-2 evidence: `evidence/qa-gates/split-verification.2026-06-12T16-45.md`, `evidence/qa-gates/coverage-delta.2026-06-12T16-45.md`, `evidence/qa-gates/containment.2026-06-12T16-45.md`, `evidence/qa-gates/final-toolchain.2026-06-12T16-45.md`
- **Feature folder used:** `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177`
- **Requirements source:** `spec.md` and `user-story.md` (full-feature mode; `issue.md:12` = `- Work Mode: full-feature`)
- **Work mode resolution note:** `full-feature` resolves AC sources to `spec.md` and `user-story.md`. The 20 enumerated, checkbox-backed AC live in `user-story.md`.
- **Scope note:** PR context summary/appendix artifacts were absent; scope was derived directly from the deterministic merge-base diff covering all branch commits.

---

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**
- `user-story.md` — primary (20 checkbox-backed criteria AC1-AC20)
- `spec.md` — secondary (Definition of Done; behavior and config contracts)

### Acceptance criteria (from user-story.md)

1. AC1 — Hierarchy construction from RelativePath (parent→children, root = empty string).
2. AC2 — Single-segment path edge case (one edge root→Inbox; node is child and leaf).
3. AC3 — Idempotent / duplicate-path construction (no duplicate children).
4. AC4 — New-leaf construction (adds child to one parent only).
5. AC5 — LCPPN beam-search descent returns a leaf with path-product probability + ordered alternatives.
6. AC6 — Configurable beam width (default 3; width recovers a branch greedy would discard; BeamWidth >= 1).
7. AC7 — Abstention semantics (below MinimumPathProbability → empty; root abstention allowed).
8. AC8 — F1 accounting for abstention (FN for true class, TN for others, no FP).
9. AC9 — Shrinkage smoothing with configurable lambda (default 0.7; 0 <= λ <= 1).
10. AC10 — Cold-start fallback (below MinColdStartExamples → unsmoothed NB).
11. AC11 — Localized incremental update (only path classifiers; prior-path UnTrain on reclassify).
12. AC12 — New-leaf addition is local (only that parent's PerParentClassifier).
13. AC13 — Backward compatibility (flag off → flat BayesianClassifierGroup unchanged; Folder.json as before).
14. AC14 — Shared IFolderPredictor seam (both predictors implement it; callers route through it).
15. AC15 — Serialization round-trip (separate file; preserves Version/tree/counts; empty tree; Corpus inline).
16. AC16 — Deterministic evaluation harness (index-proxy split; per-leaf P/R/F1, macro F1, abstention rate; no COM/IO/temp).
17. AC17 — Test stack and isolation (MSTest + Moq + FluentAssertions; deterministic; no temp files/external deps).
18. AC18 — Coverage (new modules >= 90%; repo-wide >= 80%; no changed-line regression).
19. AC19 — Toolchain (CSharpier → analyzers → nullable → MSTest, restart on failure/auto-fix).
20. AC20 — File-size and separation constraints (no new production/test/reusable script file > 500 lines; logic pure/testable without COM).

---

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|-----------|--------|----------|--------------------------|-------|
| 1 | Hierarchy construction | PASS | `FolderHierarchyTree.AddPath`/`AddLeaf`; `FolderHierarchyTree_Tests.cs`. | Read; `git diff` | Unchanged this cycle; still satisfied. |
| 2 | Single-segment edge case | PASS | `AddPath` one edge root→segment; `IsLeaf`. | Read | Covered by F2 tests; unchanged this cycle. |
| 3 | Idempotent duplicates | PASS | `ChildSet` HashSet dedupe. | Read | Unchanged. |
| 4 | New-leaf construction | PASS | `AddLeaf` mutates one parent set. | Read | Unchanged. |
| 5 | Beam descent + path-product + alternatives | PASS | `LcppnFolderPredictor.Classify`/`DescendBeam`; `Classify_*` cases now in `LcppnFolderPredictor_Classify_Tests.cs`. | Read; `diff` of test-method sets | Tests relocated by the cycle-2 split; behavior coverage preserved. |
| 6 | Configurable beam width | PASS | `BeamWidth` default 3; `Validate >= 1`; `Classify_WiderBeam_RecoversBranchGreedyWouldDiscard` (now in Classify file). | Read | Beam-trim/beam-width cases preserved across split. |
| 7 | Abstention semantics | PASS | `Classify` empty when below threshold; `Classify_BelowThreshold_ReturnsEmpty`/`Classify_NoRootChildren_ReturnsEmpty` (now in Classify file). | Read | Abstention cases preserved across split. |
| 8 | F1 abstention accounting | PASS | `FolderPredictorEvaluator.Evaluate` abstention → FN only. | Read | Unchanged. |
| 9 | Shrinkage lambda | PASS | `PerParentClassifier.ChildLogScore` blend; default 0.7; range validated. | Read | Unchanged. |
| 10 | Cold-start fallback | PASS | `IsColdStart`; leaf-only estimate below threshold. | Read | Unchanged. |
| 11 | Localized incremental update | PASS | `Train`/`UnTrain` iterate only root-to-leaf segments; `UnTrain_IntermediateParentMissing_SkipsMissingSegment` retained in `LcppnFolderPredictor_Tests.cs`. | Read | Unchanged. |
| 12 | New-leaf addition local | PASS | `GetOrAddNode` + per-parent `Train`. | Read | Unchanged. |
| 13 | Backward compatibility (flag off) | PASS | `BayesianClassifierGroup.cs` diff is interface declaration only; `GetFolderPredictorAsync` returns `Manager["Folder"]` when flag off; `GetFolderPredictorAsync_FlagOff_FreshPerCallInstance_ReturnsFlat` (FolderPredictorSeam_Tests.cs:266) asserts `BeSameAs(flat)`. | Read tests; `git diff` | No production change since cycle 1; F1 byte-identical. |
| 14 | Shared IFolderPredictor seam | PASS | Both predictors implement `IFolderPredictor`; callers route through `GetFolderPredictorAsync`; flag-on reachability via shared `Globals.AF.FolderPredictor` (`IAppAutoFileObjects.cs:45`, `AppAutoFileObjects.cs:617`); regression test present. `grep _lcppnPredictor` → no matches. | grep; Read; `git diff` | No production change since cycle 1; F1 fix intact. |
| 15 | Serialization round-trip | PASS | `LcppnFolderPredictor : SmartSerializable<>`; `Version`; `[JsonIgnore]` Tree rebuilt; empty-tree test in `LcppnFolderPredictor_Serialization_Tests.cs` (192 lines). | Read | Unchanged. |
| 16 | Deterministic evaluation harness | PASS | `FolderPredictorEvaluator` index split; per-leaf P/R/F1, macro F1, abstention rate; no COM/IO/temp. | Read | Unchanged. |
| 17 | Test stack and isolation | PASS | All tests MSTest + Moq + FluentAssertions; deterministic; no temp files/external deps. Split files: `temp/file-IO hits = 0`; both `using` MSTest + FluentAssertions. | grep; Read | New `Classify_Tests` file conforms (in-memory corpora, no temp/IO). |
| 18 | Coverage | PASS | Repo-wide UtilitiesCS.dll 85.46% strict (≥ 80%), no regression vs 85.31% baseline. F2 targets independently re-parsed: `FolderHierarchyTree` 100.00% strict, `LcppnFolderPredictor` 97.71% strict — both ≥ 90%. | per-type re-parse of `artifacts/csharp/coverage.xml` | Cycle-2 split is test-only; production-class coverage unchanged from cycle 1. `FolderHierarchyNode` 60.0% strict / 100.0% inclusive remains accepted auto-generated-record exception. |
| 19 | Toolchain | PASS | `final-toolchain.2026-06-12T16-45.md`: Steps 1-3 EXIT 0; Step 4 in-scope tests pass (3903), 1 out-of-scope pre-existing flake passes in isolation. Order respected; no restart needed for the excluded flake. | final-toolchain gate | In-scope toolchain clean in a single pass. |
| 20 | File-size and separation | PASS | `LcppnFolderPredictor_Tests.cs` now **316 lines**; new `LcppnFolderPredictor_Classify_Tests.cs` **287 lines**; both ≤ 500. All nine feature test files ≤ 500 (largest 334). New file registered in `UtilitiesCS.Test.csproj:119`. Separation (logic pure/COM-free) holds; new production files all < 500 (largest 363). | `awk END{NR}`; `git show`; csproj grep | **Resolved this cycle:** the cycle-1 554-line over-cap test file is split; AC20 now satisfied. |

---

## Summary

**Overall Feature Readiness:** PASS (all 20 AC satisfied)

**Criteria summary:**
- **PASS:** 20 criteria (AC1–AC20; AC20 resolved this cycle by the test-file split)
- **PARTIAL:** 0 criteria
- **UNVERIFIED:** 0 criteria
- **FAIL:** 0 criteria

**Cycle-2 objective outcome:**

1. **F3 resolved (AC20 upgraded to PASS).** The cycle-1 FAIL — `LcppnFolderPredictor_Tests.cs` at 554 lines, over the 500-line cap — is resolved by a test-only split: `LcppnFolderPredictor_Tests.cs` trimmed to 316 lines (12 cases) and a new `LcppnFolderPredictor_Classify_Tests.cs` added at 287 lines (9 `Classify_*` cases), registered in the csproj. A set-diff of the pre-split (`e159bead`) `[TestMethod]` names against the post-split union of the two files is empty: all 21 cases preserved, none dropped or renamed.

**No-regression confirmation:**

- The diff between cycle-1 head `e159bead` and HEAD touches only the two test files, the csproj, and documentation/evidence — zero production-code change — so F1 (AC13/AC14) and F2 (AC18) stand byte-identical and were re-verified PASS.
- Containment held across the whole branch: `ManagerAsyncLazy.cs`, `Triage.cs`, `SpamBayes.cs`, `CategoryClassifierGroup.cs`, `MulticlassEngine.cs` have zero changed files; the shared `Manager` value type is unchanged.
- No workflow files modified; `modified-workflow-needs-green-run` not triggered.

**Accepted out-of-scope items (re-confirmed, not failures):**

1. `FolderHierarchyNode` strict coverage 60.0% / inclusive 100.0% — auto-generated record members; every line exercised.
2. Pre-existing over-cap production files `BayesianClassifierGroup.cs` (515), `FolderScorer.cs` (608), `SortEmail.cs` (1406) — all over the cap before this feature; #177 did not push any from under to over.

**Recommended follow-up verification steps:**

1. The accepted out-of-scope items (FolderHierarchyNode strict coverage; the three pre-existing over-cap production files) remain follow-ups for a separate refactor; they do not gate #177.
2. The pre-existing `IdleAsyncQueue` flake (`AddEntry_UseUiThreadTrue...`, ci-flaky-test-isolation-176) is tracked separately and passes in isolation; it does not gate this feature.

---

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules:
- Criteria evaluated as **PASS** may be checked off in the authoritative source file(s).
- Criteria evaluated as **PARTIAL**, **FAIL**, or **UNVERIFIED** must remain unchecked.

All 20 AC in `user-story.md` are marked `[x]`. This reaudit confirms all 20 as PASS, including AC20, which was FAIL at the cycle-1 exit and is now resolved by the test-file split. The `[x]` state for all 20 is therefore accurate as of this cycle. This reviewer does not modify AC source-file text; no source/test/AC-text changes are made by this audit.

### AC Status Summary

- Source: `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/user-story.md`
- Total AC items: 20
- Confirmed PASS by this audit: 20 (AC1–AC20)
- Failing / to be treated as unchecked: 0
- Items remaining: none

| Source File | Total AC | Confirmed PASS | Failing/Unchecked | Notes |
|-------------|----------|----------------|-------------------|-------|
| `user-story.md` | 20 | 20 | 0 | AC20 resolved this cycle by the test-file split; all `[x]` states accurate. Source text not modified by this audit. |
| `spec.md` | Definition of Done (prose checklist) | n/a | n/a | Secondary source; behavior/config contracts verified above. No checkbox change made. |
