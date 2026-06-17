# Feature Audit: hierarchical-lcppn-folder-prediction (#177) — Cycle 1 Exit Reaudit

**Audit Date:** 2026-06-12 (exit timestamp 2026-06-12T16-35 UTC)
**Feature Folder:** `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177`
**Base Branch:** `main` (merge-base `742d4f1656367ddb1d43ea66e1bdd59776f1a287`)
**Head Branch:** `TaskMaster-wt-2026-06-08-12-06` (head `e159bead`)
**Work Mode:** `full-feature`
**Audit Type:** Remediation cycle 1 end-of-cycle acceptance reaudit

---

## Scope and Baseline

- **Base branch:** `main` (merge-base commit `742d4f1656367ddb1d43ea66e1bdd59776f1a287`)
- **Head branch/commit:** `TaskMaster-wt-2026-06-08-12-06` (commit `e159bead`)
- **Merge base:** `742d4f1656367ddb1d43ea66e1bdd59776f1a287`
- **Cumulative branch commits audited:** `0223bc60`, `d06f5c00`, `e159bead` (cycle-1 remediation)
- **Evidence sources:**
  - Primary: full branch diff (`git diff <merge-base> HEAD`) and direct source inspection
  - Cycle-1 baseline: `evidence/baseline/coverage-p0/baseline.xml`
  - Feature evidence: `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/**`
  - Coverage: `artifacts/csharp/coverage.xml` (canonical, independently re-parsed)
  - Regression evidence: `evidence/regression-testing/f1-flag-on-reachability.2026-06-12T15-54.md`
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
| 2 | Single-segment edge case | PASS | `AddPath` one edge root→segment; `IsLeaf`. | Read | Branch now covered by cycle-1 F2 tests. |
| 3 | Idempotent duplicates | PASS | `ChildSet` HashSet dedupe. | Read | Unchanged. |
| 4 | New-leaf construction | PASS | `AddLeaf` mutates one parent set. | Read | Unchanged. |
| 5 | Beam descent + path-product + alternatives | PASS | `LcppnFolderPredictor.Classify`/`DescendBeam`. | Read | Unchanged. |
| 6 | Configurable beam width | PASS | `BeamWidth` default 3; `Validate >= 1`; width-recovery test. | Read | Beam-trim branch now covered (cycle-1 F2). |
| 7 | Abstention semantics | PASS | `Classify` empty when `exp(top.LogProbability) < MinimumPathProbability`. | Read | Abstention branch now covered (cycle-1 F2). |
| 8 | F1 abstention accounting | PASS | `FolderPredictorEvaluator.Evaluate` abstention → FN only. | Read | Unchanged. |
| 9 | Shrinkage lambda | PASS | `PerParentClassifier.ChildLogScore` blend; default 0.7; range validated. | Read | Unchanged. |
| 10 | Cold-start fallback | PASS | `IsColdStart`; leaf-only estimate below threshold. | Read | Unchanged. |
| 11 | Localized incremental update | PASS | `Train`/`UnTrain` iterate only root-to-leaf segments. | Read | Missing-parent UnTrain branch now covered (cycle-1 F2). |
| 12 | New-leaf addition local | PASS | `GetOrAddNode` + per-parent `Train`. | Read | Unchanged. |
| 13 | Backward compatibility (flag off) | PASS | `BayesianClassifierGroup.cs` diff is interface declaration only; `GetFolderPredictorAsync` returns `Manager["Folder"]` when flag off (`OlFolderClassifierGroup.cs:90`); new `GetFolderPredictorAsync_FlagOff_FreshPerCallInstance_ReturnsFlat` asserts `BeSameAs(flat)` for a fresh per-call instance. | `git diff ...BayesianClassifierGroup.cs`; Read tests | **Strengthened this cycle:** flag-off byte-for-byte preservation now proven for the production per-call pattern. |
| 14 | Shared IFolderPredictor seam | PASS | Both predictors implement `IFolderPredictor`; callers route through `GetFolderPredictorAsync`; new `GetFolderPredictorAsync_FlagOn_ReachableThroughFreshPerCallInstance` proves the held LCPPN predictor is reachable from two independent fresh per-call instances over shared globals. | Read tests; `git diff` callers | **Strengthened this cycle:** the prior Major wiring gap (flag-on path unreachable in production) is resolved; the seam is now live for the per-call caller pattern. |
| 15 | Serialization round-trip | PASS | `LcppnFolderPredictor : SmartSerializable<>`; `Version`; `[JsonIgnore]` Tree rebuilt; empty-tree test. | Read | Unchanged. |
| 16 | Deterministic evaluation harness | PASS | `FolderPredictorEvaluator` index split; per-leaf P/R/F1, macro F1, abstention rate; no COM/IO/temp. | Read | Unchanged. |
| 17 | Test stack and isolation | PASS | All tests MSTest + Moq + FluentAssertions; deterministic; no temp files/external deps; cycle-1 tests included. | grep; Read | New tests conform. |
| 18 | Coverage | PASS | Repo-wide UtilitiesCS.dll 85.45% strict (≥ 80%), no regression vs 85.31% baseline. F2 targets independently re-parsed: `FolderHierarchyTree` 100.00% strict, `LcppnFolderPredictor` 97.71% strict — both ≥ 90%. | per-type re-parse of `artifacts/csharp/coverage.xml` | **Upgraded to PASS this cycle:** the two in-scope strict-coverage shortfalls are resolved. `FolderHierarchyNode` 60.0% strict / 100.0% inclusive remains the accepted auto-generated-record exception (out-of-scope for cycle 1). |
| 19 | Toolchain | PASS | `final-{csharpier,analyzers,nullable,tests}.md`: each EXIT 0; tests 3904/3904 — single final pass. | final-step gates | Order respected. |
| 20 | File-size and separation | FAIL | New test file `LcppnFolderPredictor_Tests.cs` is **554 lines** (verified `awk END{NR}` = 554; was 418 at `d06f5c00`, crossed the cap in cycle-1 commit `e159bead`). AC20 forbids any new test file > 500 lines. Separation (logic pure/COM-free) still holds; new production files all < 500 (largest 363). | `awk END{NR}`; `git show d06f5c00:...` | **Regressed this cycle:** the cycle-1 F2 coverage additions pushed a new test file over the 500-line cap. AC20's "no new ... test ... file exceeds 500 lines" condition is violated. |

---

## Summary

**Overall Feature Readiness:** FAIL (one AC FAIL — AC20 file-size)

**Criteria summary:**
- **PASS:** 19 criteria (AC1–AC19; AC13/AC14/AC18 strengthened this cycle)
- **PARTIAL:** 0 criteria
- **UNVERIFIED:** 0 criteria
- **FAIL:** 1 criterion (AC20 — new test file over the 500-line cap)

**Cycle-1 objective outcomes:**

1. **F1 resolved (AC13/AC14 strengthened to verified PASS).** The flag-on LCPPN path is now reachable through a fresh per-call `OlFolderClassifierGroup` instance via the shared `Globals.AF.FolderPredictor` holder; proven by `GetFolderPredictorAsync_FlagOn_ReachableThroughFreshPerCallInstance`. Flag-off behavior is byte-for-byte preserved, proven by `GetFolderPredictorAsync_FlagOff_FreshPerCallInstance_ReturnsFlat`. The dead `_lcppnPredictor` field is fully removed.
2. **F2 resolved (AC18 upgraded to PASS).** Both in-scope target types exceed the 90% strict new-code gate (100.00% / 97.71%, independently re-parsed); repo-wide 85.45% strict with no regression.

**Top gaps preventing full PASS:**

1. **AC20 FAIL:** `LcppnFolderPredictor_Tests.cs` is a new test file at 554 lines, over the 500-line cap. This is a new violation introduced by the cycle-1 F2 coverage work, distinct from the pre-existing over-cap files. It must be split before merge.

**Recommended follow-up verification steps:**

1. Split `LcppnFolderPredictor_Tests.cs` into two cohesive test files each under 500 lines, with matching `<Compile Include>` entries; re-run the C# toolchain and re-verify F2 strict coverage is preserved.
2. Pre-existing over-cap modified files (`BayesianClassifierGroup.cs`, `FolderScorer.cs`, `SortEmail.cs`) and the `FolderHierarchyNode` strict shortfall remain accepted out-of-scope follow-ups.

---

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules:
- Criteria evaluated as **PASS** may be checked off in the authoritative source file(s).
- Criteria evaluated as **PARTIAL**, **FAIL**, or **UNVERIFIED** must remain unchecked.

All 20 AC in `user-story.md` are currently marked `[x]` (the executor checked them off in prior work). This reaudit confirms 19 as PASS. **AC20 is now evaluated FAIL** because the cycle-1 F2 work introduced a new 554-line test file in violation of AC20's "no new test file exceeds 500 lines" condition; its `[x]` in `user-story.md` is therefore stale. This reviewer does not modify AC source-file text (no source/test/AC-text changes are made by this audit) but records the discrepancy here so the orchestrator and the next remediation cycle treat AC20 as unchecked/failing until the over-cap test file is split. No criterion text was modified.

### AC Status Summary

- Source: `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/user-story.md`
- Total AC items: 20
- Confirmed PASS by this audit: 19 (AC1–AC19)
- Failing / to be treated as unchecked: 1 (AC20 — new test file over 500-line cap)
- Items remaining: AC20 (file-size: new test file `LcppnFolderPredictor_Tests.cs` is 554 lines)

| Source File | Total AC | Confirmed PASS | Failing/Unchecked | Notes |
|-------------|----------|----------------|-------------------|-------|
| `user-story.md` | 20 | 19 | 1 (AC20) | AC20 marked `[x]` in source is stale; evaluated FAIL here. Source text not modified by this audit. |
| `spec.md` | Definition of Done (prose checklist) | n/a | n/a | Secondary source; behavior/config contracts verified above. No checkbox change made. |
