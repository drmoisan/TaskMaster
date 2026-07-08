# Acceptance-Criteria Status (Cycle 3, #177)

Timestamp: 2026-06-16T01-04
AC source: docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/user-story.md
PostedAs: unknown (local mirror only; no GitHub posting performed by the executor)

## Cycle-3 AC → evidence mapping

| AC | Status | Proving evidence |
|----|--------|------------------|
| AC21 — Production enablement, default ON via reachable config | PASS | Persistent setting `UseLcppnPredictor` (default True) in Settings.settings/.Designer.cs/app.config; exposed on `IAppAutoFileObjects.UseLcppnPredictor`, backed by `Properties.Settings.Default`; `OlFolderClassifierGroup.FolderPredictorConfig` resolved from it (no per-call hand-set). Tests: `FolderPredictorSeam_DefaultOn_Tests.DefaultOn_NoExplicitFlag_SelectsLcppnWhenHeld`, `ToggleOff_ResolvesFlatOnly_PreservingAc13`, `ExplicitConfig_OverridesPersistedDefault`. Evidence: qa-gates/final-test-coverage, regression-testing/ac13-final. |
| AC22 — Safe fallback to flat | PASS | `GetFolderPredictorAsync` returns flat `Manager["Folder"]` when ON but holder null (no throw). Tests: `DefaultOn_NoHeldPredictor_FallsBackToFlat`; load fail-soft tests `LoadFolderPredictorAsync_SettingOnButFileMissing_LeavesHolderNull`, `_SettingOnButReadThrows_FailsSoftToNull`, `_AppDataMissing_FailsSoftToNull`. Evidence: qa-gates/final-test-coverage. |
| AC23 — Persistence and load-on-startup | PASS | Predictor serialized to dedicated `LcppnFolder.json` (distinct from `Folder.json`) via `LcppnFolderPredictorStore`; rehydrated on startup by `AppAutoFileObjects.LoadFolderPredictorAsync` (wired into LoadParallel/LoadSequential); fail-soft on missing/unreadable. Tests: `LcppnFolderPredictorStore_Tests.*` (dedicated name, BuildConfig path, round-trip), `AppAutoFileObjectsFolderPredictorTests.LoadFolderPredictorAsync_SettingOnWithPersistedFile_PopulatesHolder`. Evidence: qa-gates/final-test-coverage. |
| AC24 — Containment and non-regression | PASS | Zero diff in SpamBayes.cs/Triage.cs/CategoryClassifierGroup.cs/MulticlassEngine.cs; ManagerAsyncLazy value typing unchanged (`AsyncLazy<BayesianClassifierGroup>`); full toolchain green; new/changed code >= 90% strict; repo figure governed by documented COM/VSTO exemption. Evidence: qa-gates/containment-diff, final-csharpier, final-analyzers, final-nullable, final-test-coverage, coverage-delta, final-filesize. |
| AC13 (re-verify) — Backward compatibility (flag-off flat parity) | PASS | 4 `GetFolderPredictorAsync_FlagOff_*` tests green at baseline and final. Evidence: baseline/ac13-baseline, regression-testing/ac13-final. |

## Summary
- AC1–AC20: remain satisfied (unchanged; no contained-subsystem or shared-typing changes; full
  toolchain green).
- AC21, AC22, AC23, AC24: delivered and verified this cycle (checked off in user-story.md).
- AC13: re-verified green.

Outcome: all cycle-3 acceptance criteria map to passing evidence artifacts.
