# Code Review: hierarchical-lcppn-folder-prediction (#177) — Cycle 1 Exit Reaudit

**Review Date:** 2026-06-12 (exit timestamp 2026-06-12T16-35 UTC)
**Reviewer:** feature-reviewer agent
**Feature Folder:** `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177`
**Base Branch:** `main` (merge-base `742d4f1656367ddb1d43ea66e1bdd59776f1a287`)
**Head Branch:** `TaskMaster-wt-2026-06-08-12-06` (head `e159bead`)
**Review Type:** Remediation cycle 1 end-of-cycle reaudit (cumulative diff of `0223bc60`, `d06f5c00`, `e159bead`)

---

## Executive Summary

This reaudit confirms that remediation cycle 1 resolved the two findings it was scoped to address and introduced one new file-size policy violation.

The prior cycle's Major finding (F1) — the flag-on LCPPN path was unreachable because the predictor was held in per-instance state while production callers construct a fresh `OlFolderClassifierGroup` per call — is resolved. The per-instance `_lcppnPredictor` field is removed entirely (no source references remain), and the built predictor is now held in a Folder-only `IFolderPredictor FolderPredictor { get; set; }` on the shared `IAppAutoFileObjects` surface (`Globals.AF.FolderPredictor`). It is set at the build/registration site (`OlFolderClassifierGroup.cs:281`) when `UseLcppnPredictor` is true and resolved in `GetFolderPredictorAsync` (`OlFolderClassifierGroup.cs:80-91`). Because all three callers share the same `globals`, every fresh per-call instance resolves the same held predictor. A new regression test constructs two independent instances over shared globals and asserts both return the same LCPPN predictor; a companion test confirms the flag-off path still returns the exact flat `Manager["Folder"]` instance.

The prior cycle's Minor coverage finding (F2) is resolved: independently re-parsed from `artifacts/csharp/coverage.xml`, `FolderHierarchyTree` is 100.00% strict and `LcppnFolderPredictor` is 97.71% strict, both above the 90% strict new-code target. Repo-wide UtilitiesCS.dll coverage is 85.45% strict with no regression.

**What changed (cycle-1 commit e159bead):**
The dead `_lcppnPredictor` instance field and its comment are removed. `IAppAutoFileObjects` gains `IFolderPredictor FolderPredictor { get; set; }` (implemented as an auto-property on `AppAutoFileObjects`). `GetFolderPredictorAsync` reads the shared holder; `BuildClassifiersAsync` and `SetLcppnPredictor` write it. Two regression tests are added to `FolderPredictorSeam_Tests.cs`; targeted branch-coverage tests are added to `FolderHierarchyTree_Tests.cs` and `LcppnFolderPredictor_Tests.cs`. The shared `Manager` value type and `ManagerAsyncLazy.cs` are unchanged, as are the out-of-scope classifier subsystems.

**Top risks:**
1. **New file-size violation (Major):** `LcppnFolderPredictor_Tests.cs` grew from 418 to 554 lines in this cycle, exceeding the 500-line cap. This is a new policy violation, not a pre-existing one, and AC20 explicitly forbids new test files over 500 lines.
2. **Pre-existing over-cap modified files (Minor, accepted):** `BayesianClassifierGroup.cs` (515), `FolderScorer.cs` (608), `SortEmail.cs` (1406) remain over cap; pre-existing, recorded out-of-scope for cycle 1.
3. None remaining for F1/F2: the seam is now live for the per-call caller pattern and the F2 coverage targets are met.

**PR readiness recommendation:** **Needs revision** — the F1/F2 substantive objectives are met and the toolchain is green, but the new over-cap test file is a FAIL-level policy item that should be split before merge.

---

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Major | `UtilitiesCS.Test/EmailIntelligence/Bayesian/LcppnFolderPredictor_Tests.cs` | whole file | NEW file is 554 lines, over the 500-line cap. It was 418 lines at the pre-cycle-1 head `d06f5c00` and crossed the cap in cycle-1 commit `e159bead` when P2-T2 added 136 lines of F2 coverage tests. | Split into two cohesive test files under 500 lines each (e.g., descent/abstention/beam tests vs. construction/serialization/branch-coverage tests) with matching `<Compile Include>` entries; re-run the toolchain and re-verify F2 coverage. | The 500-line cap applies to test code; an MSTest file is not a throwaway script or raw fixture, so no exception applies. AC20 explicitly forbids new test files over 500 lines. This is a regression introduced by this cycle. | `awk END{NR}` = 554; `wc -l` = 554; `git show d06f5c00:...LcppnFolderPredictor_Tests.cs \| awk END{NR}` = 418; `git cat-file -e <merge-base>:...` confirms file is new in branch. |
| Minor | `UtilitiesCS/EmailIntelligence/Bayesian/BayesianClassifierGroup.cs` | whole file | Modified file is 515 lines (baseline 513), over cap; +2 from the interface declaration. | Split in a separate refactor (out-of-scope for cycle 1). | Cap applies to changed files; overage is pre-existing, not created by this feature. | `awk END{NR}` head 515 vs base 513; recorded out-of-scope in `remediation-inputs.2026-06-12T15-54.md`. |
| Info | `UtilitiesCS/EmailIntelligence/Bayesian/FolderHierarchyNode.cs` | record members | Strict line coverage 60.0% / inclusive 100.0%; shortfall is auto-generated record members. | No action; accepted out-of-scope for cycle 1. | Every line is exercised (inclusive 100%); strict undercounts auto-generated members. | Per-type re-parse of `artifacts/csharp/coverage.xml`; cycle-1 remediation-inputs out-of-scope list. |
| Info | `UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/OlFolderClassifierGroup.cs`, `IAppAutoFileObjects.cs`, `AppAutoFileObjects.cs` | F1 seam | F1 fix is correct: `_lcppnPredictor` removed; shared `Globals.AF.FolderPredictor` holder set at build site and read by the accessor; null-guarded fallback to flat path. | No action; resolved. | Holder on shared `globals.AF` is reachable by every fresh per-call instance, closing the wiring gap with the smallest seam and without retyping `Manager`. | `grep -rn "_lcppnPredictor"` no matches; `OlFolderClassifierGroup.cs:80-91,281`; `IAppAutoFileObjects.cs:44`; `AppAutoFileObjects.cs:617`. |

No Blocker findings.

---

## Implementation Audit

### C# implementation audit

#### F1 fix correctness (verified)

- The dead per-instance holder is gone: `grep -rn "_lcppnPredictor" --include="*.cs" .` returns no matches across the repo.
- The shared holder is declared on the interface (`IAppAutoFileObjects.cs:44`) and implemented as a public auto-property on `AppAutoFileObjects` (`AppAutoFileObjects.cs:617`), defaulting to null. The XML docs state it is null when the flat path is active and that it does not alter the `Manager` value type.
- The registration site sets it inside the existing `UseLcppnPredictor == true` block after the flat `Manager["Folder"]` registration (`OlFolderClassifierGroup.cs:279-282`), leaving the flat registration unchanged.
- `GetFolderPredictorAsync` returns `Globals.AF.FolderPredictor` only when `FolderPredictorConfig?.UseLcppnPredictor == true && Globals.AF.FolderPredictor is not null`; otherwise it awaits and returns `Globals.AF.Manager["Folder"]` (`OlFolderClassifierGroup.cs:80-91`). The null guard preserves the flat fallback when the flag is on but no predictor has been built.
- `SetLcppnPredictor` now writes `Globals.AF.FolderPredictor` (`OlFolderClassifierGroup.cs:67-70`), so the test seam routes through the same shared holder as production.

#### Containment (verified)

- `git diff <merge-base> HEAD` for `ManagerAsyncLazy.cs`, `Triage.cs`, `SpamBayes.cs`, `CategoryClassifierGroup.cs`, `MulticlassEngine.cs` is empty.
- The shared `ConcurrentObservableDictionary<string, AsyncLazy<BayesianClassifierGroup>>` value type behind `Manager` is unchanged. The cycle added one Folder-specific member to the AF surface (distinct from `Manager["Folder"]`), consistent with the Manager-shared-seam constraint.

#### Type safety and API notes

- Nullable build is clean under TreatWarningsAsErrors (`final-nullable.md` EXIT 0). The new property is a nullable reference type with a null guard at the read site.
- The `FolderPredictor` holder is a narrow `IFolderPredictor`-typed seam; no broadening of the public surface beyond the single property.

---

## Test Quality Audit

The cycle added two F1 regression tests and several F2 branch tests; all run within the 3904-test suite, which the final QA gate reports green in a single pass.

### Reviewed test and QA artifacts

- `FolderPredictorSeam_Tests.cs:232-260` — `GetFolderPredictorAsync_FlagOn_ReachableThroughFreshPerCallInstance`: sets `Globals.AF.FolderPredictor` to a built LCPPN predictor on shared mock globals, constructs two independent `new OlFolderClassifierGroup(globals)` instances with the flag on, and asserts both resolve the same held predictor (`BeSameAs(lcppn)` and `firstPredictor.Should().BeSameAs(secondPredictor)`). This exercises the production per-call pattern that the prior test suite did not, which is exactly why the Major wiring gap was previously missed.
- `FolderPredictorSeam_Tests.cs:266-283` — `GetFolderPredictorAsync_FlagOff_FreshPerCallInstance_ReturnsFlat`: asserts a fresh flag-off instance returns the exact flat `Manager["Folder"]` instance (`BeSameAs(flat)`), confirming AC13 byte-for-byte preservation.
- `FolderPredictorSeam_Tests.cs:84` — the shared mock setup uses `mockAf.SetupProperty(x => x.FolderPredictor)` so the holder has a real backing store matching the production auto-property semantics; this is correct mocking, not a stub that would mask the bug.
- `FolderHierarchyTree_Tests.cs` / `LcppnFolderPredictor_Tests.cs` — F2 branch tests that lifted strict coverage to 100.00% / 97.71% (independently re-parsed).
- `artifacts/csharp/coverage.xml` — canonical post-change coverage, independently re-parsed (UtilitiesCS.dll 85.45% strict).

### Quality assessment

- **Determinism:** No randomness, time, or I/O in new tests; in-memory mocks only.
- **Isolation:** Each test targets one behavior; fixtures constructed per test.
- **Diagnostics:** FluentAssertions with `because` reasons.
- **Adequacy of the F1 proof:** The two-instance assertion directly models the caller pattern named in the original Major finding, so the regression is genuinely covered rather than asserted on a single instance that both holds and serves the predictor.

The one test-quality concern is structural, not behavioral: `LcppnFolderPredictor_Tests.cs` is now 554 lines, over the 500-line cap (see Findings Table).

---

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets in code | ✅ PASS | No credentials/keys in the diff; count-based model only. |
| No unsafe subprocess or command construction | ✅ PASS | No `Process.Start` or shell invocation in new code. |
| Input validation at boundaries | ✅ PASS | Null guard on the holder read; `collection.ThrowIfNull()` in the build path. |
| Error handling remains explicit | ✅ PASS | Specific exceptions; no broad catch introduced. |
| Configuration / path handling is safe | ✅ PASS | No filesystem path traversal; serialized state is a separate JSON file from `Folder.json`. |
| Containment (Option B) | ✅ PASS | `ManagerAsyncLazy.cs` zero diff; out-of-scope classifiers unchanged; shared dictionary value type unchanged. |
| No workflow files modified | ✅ PASS | `git diff --name-only <merge-base> HEAD -- .github/` is empty. |
| Tonality of evidence/docs | ✅ PASS | Evidence and feature docs use neutral, factual language. |

---

## Research Log

No external research was required. The review was based on diff inspection against the resolved merge-base, direct reading of the changed source and test files, re-parsing of the canonical coverage artifact, and inspection of the Phase 3 QA-gate evidence.

---

## Verdict

Remediation cycle 1 resolved both in-scope findings. F1 is fixed correctly: the per-instance holder is removed, the predictor is held on the shared AF surface reachable by every fresh per-call caller instance, and a regression test proves reachability through the production per-call pattern while a companion test preserves flag-off behavior byte-for-byte. F2 is resolved with both target types above the 90% strict new-code gate (100.00% and 97.71%, independently re-parsed). Containment held, no workflow files changed, and the C# toolchain is green in a single final pass.

One new FAIL-level item must be addressed: the cycle's F2 coverage additions pushed `LcppnFolderPredictor_Tests.cs` to 554 lines, over the 500-line cap, which AC20 forbids for new test files. Recommendation: Needs revision — split the over-cap test file, re-run the toolchain, and re-verify coverage. The pre-existing over-cap modified files and the `FolderHierarchyNode` strict shortfall remain accepted out-of-scope items.
