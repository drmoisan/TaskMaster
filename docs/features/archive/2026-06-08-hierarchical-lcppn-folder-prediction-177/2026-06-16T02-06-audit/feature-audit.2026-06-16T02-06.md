# Feature Audit: hierarchical-lcppn-folder-prediction (#177) — Cycle 3 Exit Reaudit

- Audit date: 2026-06-16
- Exit timestamp: 2026-06-16T02-06
- Branch: `TaskMaster-wt-2026-06-08-12-06`
- Work Mode: `full-feature` (AC sources: `spec.md` and `user-story.md`)
- Artifact naming confirmed: this policy-reviewer artifact is `feature-audit.<exit-ts>.md` per repository convention (not `feature-review.<exit-ts>.md`).

## Scope and Baseline

The audit scope is the full branch diff against the resolved base merge-base `c12aaf1c` (main). The cycle-3 production-migration delta evaluated here is `0b589c83..HEAD` (commits `cc769a05`, `c7ef085a`, `f4159154`). Only C# source files changed. Cycle 3 focuses on AC21, AC22, AC23, AC24, and re-verification of AC13 (flag-off parity); AC1–AC20 must remain satisfied. All AC verdicts below were verified against the live diff, the source files, and the recorded cycle-3 QA-gate and regression evidence under `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/`.

## Verdict

**Overall: PASS.** AC21–AC24 are satisfied, AC13 flag-off parity is re-verified, and AC1–AC20 remain satisfied. `blocking_count` contribution from this artifact: 0.

## Acceptance Criteria Inventory

Source files: `user-story.md` (AC1–AC24, all checked `[x]`), `spec.md`. Cycle-3 focus ACs: AC21, AC22, AC23, AC24; re-verified parity AC: AC13. AC1–AC20 are carried forward from the prior clean cycle-1/cycle-2 exit (`2026-06-12T17-14`) and re-confirmed not regressed by the containment evidence.

| AC | Title | Cycle-3 relevance |
|----|-------|-------------------|
| AC1–AC12 | Hierarchy/beam/abstention/shrinkage/incremental/new-leaf | Carried forward; unchanged source (`LcppnFolderPredictor.cs` zero diff in cycle 3) |
| AC13 | Backward compatibility (flat predictor) | Re-verified this cycle |
| AC14 | Shared `IFolderPredictor` seam | Carried forward; seam preserved |
| AC15 | Serialization round-trip (own file) | Reinforced this cycle |
| AC16–AC20 | Eval harness / test stack / coverage / toolchain / file-size | Carried forward; re-confirmed |
| AC21 | Production enablement, default ON via reachable config | Primary cycle-3 |
| AC22 | Safe fallback to flat | Primary cycle-3 |
| AC23 | Persistence and load-on-startup | Primary cycle-3 |
| AC24 | Containment and non-regression | Primary cycle-3 |

## Acceptance Criteria Evaluation

### AC21 — Production enablement, default ON via reachable config — PASS

- The setting is sourced from persistent config, not a hard-coded per-instance default: `TaskMaster/Properties/Settings.Designer.cs` adds `UseLcppnPredictor` with `[DefaultSettingValueAttribute("True")]`; `Settings.settings` and `app.config` carry `True`.
- It crosses the interface boundary as a resolved bool: `IAppAutoFileObjects.UseLcppnPredictor` (`UtilitiesCS/Interfaces/IGlobals/IAppAutoFileObjects.cs:53`) is implemented by `AppAutoFileObjects.UseLcppnPredictor => _defaults.UseLcppnPredictor` (`TaskMaster/AppGlobals/AppAutoFileObjects.FolderPredictorLoad.cs:29`, `_defaults = Properties.Settings.Default` at `AppAutoFileObjects.cs:101`).
- It is honored by production callers without per-call edits: `OlFolderClassifierGroup.FolderPredictorConfig` is a lazy getter resolving from `Globals.AF.UseLcppnPredictor` via `ResolveFolderPredictorConfigFromSettings()` (`OlFolderClassifierGroup.cs:44-66`). `EmailFiler.cs`, `SortEmail.cs`, and `FolderScorer.cs` have ZERO diff in `0b589c83..HEAD` (verified: `git diff --stat` empty), so no call site hand-sets the flag.
- Toggleable to OFF restoring flat-only: verified by `ToggleOff_ResolvesFlatOnly_PreservingAc13` and `ExplicitConfig_OverridesPersistedDefault` (`FolderPredictorSeam_DefaultOn_Tests.cs:127-166`).
- Evidence: `DefaultOn_NoExplicitFlag_SelectsLcppnWhenHeld` (`FolderPredictorSeam_DefaultOn_Tests.cs:84-102`); coverage 100% on the resolver/changed regions (`evidence/qa-gates/coverage-delta.2026-06-16T01-04.md`).

### AC22 — Safe fallback to flat — PASS

- `GetFolderPredictorAsync` returns the held LCPPN predictor only when `FolderPredictorConfig?.UseLcppnPredictor == true && Globals.AF.FolderPredictor is not null`; otherwise it awaits and returns the flat `Manager["Folder"]` group (`OlFolderClassifierGroup.cs:103-114`). The `is not null` guard makes the ON-but-unbuilt case fall back without throwing.
- Regression tests: `DefaultOn_NoHeldPredictor_FallsBackToFlat` (`FolderPredictorSeam_DefaultOn_Tests.cs:107-122`) asserts the flat group is returned, non-null, never thrown.
- Verdict: PASS.

### AC23 — Persistence and load-on-startup — PASS

- Own-file persistence: `LcppnFolderPredictorStore.FileName = "LcppnFolder.json"` (distinct from `Folder.json`), `BuildConfig` targets `AppData/Bayesian/LcppnFolder.json` (`LcppnFolderPredictorStore.cs:22-47`). On the build path, the predictor's `Config` is set and `Serialize()` is called before holding it (`OlFolderClassifierGroup.cs:303-309`).
- Round-trip is real, not a no-op: `BuildSettings` excludes the runtime-only `Config`/`Disk` via `DoNotSerializeContractResolver("Config")` (`LcppnFolderPredictorStore.cs:59-65`), which works around the pre-existing `FilePathHelper` deserialization re-entrancy. `RoundTrip_WithDedicatedConfig_PreservesContentAndFileName` (`LcppnFolderPredictorStore_Tests.cs:62-99`) asserts the serialized JSON does NOT contain `"Disk"` yet `Version`, `BeamWidth`, and `Nodes.Keys` round-trip losslessly.
- Startup rehydrate: `AppAutoFileObjects.LoadFolderPredictorAsync` is awaited from both `LoadParallel` and `LoadSequential` (`AppAutoFileObjects.cs` wiring lines). On a deserializable persisted predictor the holder is populated (`LoadFolderPredictorAsync_SettingOnWithPersistedFile_PopulatesHolder`).
- Fail-soft on missing/unreadable file (does not throw): a null deserialize result leaves the holder null (`LoadFolderPredictorAsync_SettingOnButFileMissing_LeavesHolderNull`), an IOException is caught and logged (`LoadFolderPredictorAsync_SettingOnButReadThrows_FailsSoftToNull`), and unresolved AppData short-circuits (`LoadFolderPredictorAsync_AppDataMissing_FailsSoftToNull`). All three assert `NotThrowAsync` and holder null (`AppAutoFileObjectsFolderPredictorTests.cs:71-159`).
- Coverage: store 100% (32/32), load partial 100% (10/10).
- Verdict: PASS.

### AC24 — Containment and non-regression — PASS

- ZERO diff in spam/triage/category/actionable subsystems: `SpamBayes.cs`, `Triage.cs`, `CategoryClassifierGroup.cs`, `MulticlassEngine.cs` all show empty `git diff --stat 0b589c83..HEAD` (independently verified). `Manager["Actionable"]` usage unchanged.
- `ManagerAsyncLazy` value typing unchanged: `ManagerAsyncLazy.cs` zero diff; value type remains `AsyncLazy<BayesianClassifierGroup>` (`evidence/qa-gates/containment-diff.2026-06-16T01-04.md`).
- Flat rebuild retained: the always-on flat `Manager["Folder"]` build/serialize is left in place; the LCPPN serialize is additive inside the same build method.
- AC1–AC20 remain satisfied (see below); new/changed code meets coverage policy (new code 100% >= 90% strict; repo-wide governed by the documented COM/VSTO exemption); full C# toolchain passes in a single final pass (csharpier/analyzers/nullable/tests all exit 0).
- Verdict: PASS.

### AC13 — Backward compatibility (flat predictor), re-verified — PASS

- With the persisted setting OFF (resolved via the mocked `IAppAutoFileObjects.UseLcppnPredictor` returning false), `GetFolderPredictorAsync` returns the same flat `BayesianClassifierGroup` instance from `Manager["Folder"]`, byte-for-byte unchanged. Four unchanged parity tests pass (`evidence/regression-testing/ac13-final.2026-06-16T01-04.md`, exit 0): `GetFolderPredictorAsync_FlagOff_ReturnsFlatManagerGroup`, `_ClassifyUnchanged`, `_TrainAndUnTrainAffectFlatGroup`, `_FreshPerCallInstance_ReturnsFlat`.
- Two new tests additionally confirm OFF restores flat-only selection (`ToggleOff_ResolvesFlatOnly_PreservingAc13`) and explicit OFF config wins over persisted ON (`ExplicitConfig_OverridesPersistedDefault`).
- The class-level `LcppnFolderPredictorConfig.UseLcppnPredictor` default stays `false`, so configs constructed directly in AC13 tests are not masked.
- Verdict: PASS.

### AC1–AC12, AC14–AC20 — carried forward — PASS (not regressed)

- `LcppnFolderPredictor.cs` (the hierarchy/beam/abstention/shrinkage/incremental engine) has ZERO diff in `0b589c83..HEAD`; its coverage remains 100% (344/344). The `IFolderPredictor` seam (AC14) is preserved. AC16 eval harness unchanged. AC17 test-stack (MSTest/Moq/FluentAssertions, no temp files) holds for the new tests. AC18 coverage and AC19 toolchain re-confirmed this cycle. AC20 file-size: all new files <= 500 (Store 67, FolderPredictorLoad 102, tests 168/101/161); over-cap callers untouched.
- These ACs were verified PASS at the cycle-1/cycle-2 clean exit (`2026-06-12T17-14`) and the containment evidence confirms cycle 3 did not regress them.
- Verdict: PASS.

## Acceptance Criteria Check-off

All AC items in `user-story.md` are already marked `[x]`. The cycle-3 focus ACs (AC21, AC22, AC23, AC24) and the re-verified AC13 are confirmed PASS by this audit; their `[x]` state is correct and is left as-is. No item required reverting to `[ ]`. No phantom criteria were added.

### Acceptance Criteria Status
- Source: `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/user-story.md` (and `spec.md`)
- Total AC items: 24
- Checked off (delivered): 24
- Remaining (unchecked): 0
- Items remaining: none

## Summary

Cycle 3 delivers the Option-B production migration: LCPPN is default-ON via a persisted, interface-boundary-resolved setting honored by all production callers without per-call edits; it persists to a dedicated `LcppnFolder.json` and rehydrates at startup with a verified (non-no-op) round-trip; the load path is fail-soft on missing/unreadable file; and flat-only behavior is preserved when toggled OFF. Containment is held (zero diff in spam/triage/category/multiclass, `ManagerAsyncLazy` typing unchanged, flat rebuild retained), over-cap caller files are untouched, all new files are <= 500 lines, and the full C# toolchain is green in a single final pass with new/changed code at 100% coverage. The pre-existing `FilePathHelper` deserialization defect is correctly out of scope (zero diff this cycle) and worked around via the `Config` exclusion. All 24 acceptance criteria are satisfied. `blocking_count` contribution: 0. Exit recommendation: PASS — ready to proceed to PR.
