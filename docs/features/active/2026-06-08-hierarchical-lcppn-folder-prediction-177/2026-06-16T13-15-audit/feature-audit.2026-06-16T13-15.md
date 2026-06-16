# Feature Audit: Hierarchical LCPPN Folder Prediction — Cycle 4 Exit Re-audit (#177)

**Audit Date:** 2026-06-16
**Feature Folder:** `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177`
**Base Branch:** `main` (merge-base `c12aaf1c`)
**Head Branch:** `TaskMaster-wt-2026-06-08-12-06` (HEAD `ac3d6b53`)
**Work Mode:** `full-feature`
**Audit Type:** Post-remediation acceptance verification (cycle-4 close, no-fix-required)

---

## Scope and Baseline

- **Base branch:** `main` (commit `c12aaf1c` merge-base)
- **Head branch/commit:** `TaskMaster-wt-2026-06-08-12-06` (commit `ac3d6b53`)
- **Merge base:** `c12aaf1c`
- **Cycle-3 exit baseline:** commit `ac3d6b53` (audit `2026-06-16T02-06`). Cycle 4 added no commits; HEAD remains `ac3d6b53`.
- **Evidence sources:**
  - Primary: full branch `git diff` against `c12aaf1c`; cycle-4 diff against `ac3d6b53`.
  - Cycle-4 investigation: `artifacts/research/2026-06-16-lcppn-deserialize-nre-research.md`
  - Feature evidence: `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/baseline/phase0-*.2026-06-16T10-26.md`, `.../evidence/regression-testing/fail-before-exception.2026-06-16T10-26.md`
  - Cycle-3 exit audit: `.../2026-06-16T02-06-audit/feature-audit.2026-06-16T02-06.md`
- **Feature folder used:** `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177`
- **Requirements source:** `spec.md` and `user-story.md` (full-feature work mode; AC25 added to `user-story.md` in cycle 4).
- **Work mode resolution note:** `issue.md` marker `- Work Mode: full-feature`. AC sources are therefore `spec.md` and `user-story.md`.
- **Scope note:** Cycle 4 produced zero production/test source diff. The two named files are byte-for-byte unchanged from `ac3d6b53`. The only tracked changes are documentation/evidence and the AC25 disposition text in `user-story.md`. The audit scope is the full branch diff (C# only, 30 `.cs` files), not the cycle-4 task subset.

---

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**
- `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/user-story.md` — primary (AC1–AC25, checkbox-backed)
- `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/spec.md` — secondary source

### Acceptance criteria

AC1–AC24 were inventoried and evaluated at the cycle-3 exit (`2026-06-16T02-06`) and all are marked `[x]` in `user-story.md`. This cycle-4 re-audit confirms they are unchanged and not regressed since `ac3d6b53`, and adds AC25.

- AC1–AC12: LCPPN hierarchy / beam / abstention / shrinkage / incremental update / new-leaf behavior.
- AC13: Backward compatibility (flat predictor preserved when toggled OFF).
- AC14: Shared `IFolderPredictor` seam.
- AC15: Serialization round-trip (dedicated own file).
- AC16–AC20: Evaluation harness / test stack / coverage / toolchain / file-size limit.
- AC21: Production enablement, default ON via reachable persisted config.
- AC22: Safe fallback to flat predictor.
- AC23: Persistence and load-on-startup (fail-soft).
- AC24: Containment and non-regression.
- AC25 (added cycle 4): FilePathHelper deserialize-safe. Verbatim from `user-story.md`: "SATISFIED ON HEAD WITH NO CODE CHANGE REQUIRED. Investigation … established that the previously-reported `FilePathHelper` deserialize `NullReferenceException` is not reproducible on HEAD: (a) `StemInitialized()` never returns true while `_fileExtension` is null because `TryParseFileName()` self-heals the stem backing fields before the `AdjustForMaxPath()` dereference; and (b) the production LCPPN load path excludes `Config` entirely via the cycle-3 `DoNotSerializeContractResolver("Config")` in `LcppnFolderPredictorStore`, so `FilePathHelper` is never instantiated by Newtonsoft on that path. … per the repository bugfix discipline no production change was made. Deserialize-safety is therefore met on HEAD. Cycle 4 closed as no-fix-required; AC1–AC24 unchanged and not regressed."

---

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|-----------|--------|----------|--------------------------|-------|
| AC1–AC12 | LCPPN hierarchy/beam/abstention/shrinkage/incremental engine | PASS | `LcppnFolderPredictor.cs` zero diff vs `ac3d6b53`; verified PASS at cycle-3 exit. | `git diff ac3d6b53 -- UtilitiesCS/EmailIntelligence/Bayesian/LcppnFolderPredictor.cs` (empty) | Carried forward; not regressed. |
| AC13 | Backward compatibility (flat predictor when OFF) | PASS | No diff since `ac3d6b53`; re-verified at cycle-3 exit (`feature-audit.2026-06-16T02-06.md`). | `git diff ac3d6b53` (no source files) | Unchanged. |
| AC14 | Shared `IFolderPredictor` seam | PASS | `IFolderPredictor.cs` unchanged since `ac3d6b53`. | `git diff ac3d6b53 -- UtilitiesCS/EmailIntelligence/Bayesian/IFolderPredictor.cs` (empty) | Carried forward. |
| AC15 | Serialization round-trip (dedicated file) | PASS | AC23 suite 10/10 includes `RoundTrip_WithDedicatedConfig_PreservesContentAndFileName`. | `phase0-ac23-baseline.2026-06-16T10-26.md` | Unchanged. |
| AC16–AC20 | Eval harness / test stack / coverage / toolchain / file-size | PASS | Cycle-4 Phase 0 baseline: csharpier exit 0; analyzers 0W/0E; nullable/TWAE 0W/0E; tests 3912/3912. | `phase0-csharpier`, `phase0-analyzers`, `phase0-nullable`, `phase0-tests-coverage` (all `.2026-06-16T10-26.md`) | Re-confirmed green. |
| AC21 | Production enablement, default ON via reachable config | PASS | Unchanged since `ac3d6b53`; verified PASS at cycle-3 exit. | `git diff ac3d6b53` (no source files) | Carried forward. |
| AC22 | Safe fallback to flat | PASS | Unchanged since `ac3d6b53`; verified PASS at cycle-3 exit. | `git diff ac3d6b53` (no source files) | Carried forward. |
| AC23 | Persistence and load-on-startup (fail-soft) | PASS | AC23 suite 10/10 with `DoNotSerializeContractResolver("Config")` present at `LcppnFolderPredictorStore.cs:63`. | `phase0-ac23-baseline.2026-06-16T10-26.md`; inspected line 63 | Config exclusion (INV-1) retained. |
| AC24 | Containment and non-regression | PASS | Zero source diff in cycle 4; full toolchain green; AC1–AC23 intact. | `git diff ac3d6b53` (no `.cs`/`.ps1`/`.ts`/`.py`); `phase0-*` baselines | No containment regression. |
| AC25 | FilePathHelper deserialize-safe | PASS | (a) `StemInitialized()` self-heals via `TryParseFileName()` before `AdjustForMaxPath()` dereference (inspected `FilePathHelper.cs:183-308`); (b) `DoNotSerializeContractResolver("Config")` excludes `Config`/`Disk` from the LCPPN load path (inspected `LcppnFolderPredictorStore.cs:63`); NRE not reproducible. | Inspected `FilePathHelper.cs:183-308`, `LcppnFolderPredictorStore.cs:63`; `artifacts/research/2026-06-16-lcppn-deserialize-nre-research.md`; `fail-before-exception.2026-06-16T10-26.md` | Satisfied on HEAD with no code change. No-fix-required correctly applied: no honest red-before-green test achievable because the throw is structurally unreachable; a defensive null-guard would be unfalsifiable. |

---

## Summary

**Overall Feature Readiness:** PASS

**Criteria summary:**
- **PASS:** 25 criteria (AC1–AC25)
- **PARTIAL:** 0 criteria
- **UNVERIFIED:** 0 criteria
- **FAIL:** 0 criteria

**Top gaps preventing PASS:**

1. None.

**Recommended follow-up verification steps:**

1. None required for this feature; the repo-wide C# coverage gate of record is the PR CI run (the local full-assembly Cobertura run is constrained per documented agent memory).
2. On PR merge, confirm CI green run for the C# toolchain as the standing gate.

AC25 is satisfied on HEAD with no code change. The conclusion is corroborated by two independent mechanisms confirmed in source: the `StemInitialized()`/`TryParseFileName()` self-heal of the stem backing fields, and the `DoNotSerializeContractResolver("Config")` exclusion that keeps `FilePathHelper` off the LCPPN deserialize path. The non-reproducibility of the defect means a failing regression test is not achievable, so declining a code change is consistent with the repository bugfix discipline. AC1–AC24 are unchanged since the cycle-3 exit `ac3d6b53` and are not regressed.

---

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules:
- Criteria evaluated as **PASS** may be checked off in the authoritative source file(s) if represented as markdown checkboxes and not already checked.
- Criteria evaluated as **PARTIAL**, **FAIL**, or **UNVERIFIED** remain unchecked.

All AC items (AC1–AC25) in `user-story.md` are already marked `[x]`. AC25 was checked off in cycle 4 when the disposition text was added (verified in the `user-story.md` diff vs `ac3d6b53`). No item required reverting to `[ ]`. No phantom criteria were added by this audit.

### AC Status Summary

- Source: `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/user-story.md` (AC1–AC25), `spec.md` (secondary, prose)
- Total AC items: 25
- Checked off (delivered): 25
- Remaining (unchecked): 0
- Items remaining: None.

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|-------------|----------|----------------|-----------|-------|
| `user-story.md` | 25 | 25 | 0 | Checkbox-backed; AC25 added and checked in cycle 4 |
| `spec.md` | n/a | n/a | n/a | Prose secondary source; no checkbox items to toggle |

No source-file checkbox change was made by this audit: all items were already `[x]` on HEAD.
