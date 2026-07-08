# Remediation Inputs: hierarchical-lcppn-folder-prediction (#177)

**Generated:** 2026-06-12T15-43 (UTC)
**Base:** `main` (merge-base `742d4f1656367ddb1d43ea66e1bdd59776f1a287`)
**Head:** `TaskMaster-wt-2026-06-08-12-06` (`d06f5c00`)

These findings are non-blocking for merge of the default-off feature but are recorded for remediation. None is a hard policy FAIL; the overall verdict is PARTIALLY COMPLIANT / PASS-with-PARTIALs. Remediation is recommended before the `UseLcppnPredictor` flag is enabled in production.

## Source artifacts

- `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/policy-audit.2026-06-12T15-43.md`
- `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/code-review.2026-06-12T15-43.md`
- `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/feature-audit.2026-06-12T15-43.md`

## Remediation-required findings

1. **[Major] Seam wiring gap — flag-on LCPPN path unreachable in production.**
   - File: `UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/OlFolderClassifierGroup.cs` (lines 38, 78-90, 274-282) and callers `EmailFiler.cs`, `SortEmail.cs`, `FolderScorer.cs`.
   - Problem: `_lcppnPredictor` and `FolderPredictorConfig` are per-instance state. Production callers construct `new OlFolderClassifierGroup(globals).GetFolderPredictorAsync()` per call (fresh instance, default flag-off config, null holder), so the predictor built at the registration site on a different instance is never returned. Enabling the flag would silently keep using the flat predictor.
   - Fix direction: resolve the config and built predictor from a shared location (e.g., hold on the `Manager` or a shared singleton) or have callers reuse the build-time instance.
   - Blocking scope: blocks the `UseLcppnPredictor=true` rollout only; does not block the default-off merge.

2. **[Minor / AC18 PARTIAL] Strict new-code coverage below 90% for three types.**
   - Files: `FolderHierarchyNode.cs` (60.0% strict / 100.0% inclusive), `FolderHierarchyTree.cs` (86.4% / 91.4%), `LcppnFolderPredictor.cs` (89.1% / 91.4%).
   - Fix direction: add tests exercising `FolderHierarchyTree.GetChildren`/`NodeKeys` accessors and the uncovered `LcppnFolderPredictor` descent branches. `FolderHierarchyNode` strict shortfall is auto-generated record members (every line exercised; inclusive 100%).
   - Evidence: `artifacts/csharp/coverage.xml`; `evidence/qa-gates/2026-06-12T15-26/coverage-comparison.md`.

3. **[Minor / AC20 PARTIAL] Modified file over the 500-line cap.**
   - File: `UtilitiesCS/EmailIntelligence/Bayesian/BayesianClassifierGroup.cs` — 515 lines (baseline 513; +2 from the interface declaration).
   - Fix direction: split the class in a separate refactor to bring it under 500 lines. (Pre-existing over-cap files `SortEmail.cs` 1406 and `FolderScorer.cs` 608 are out of this feature's added scope but also exceed the cap.)

## Items NOT requiring remediation

- Repo-wide coverage 85.40% strict (≥ 80%, no regression) — PASS.
- Option B containment (ManagerAsyncLazy.cs zero diff; out-of-scope classifiers unchanged) — PASS.
- AC13/AC14 backward compatibility at flag-off — PASS.
- Toolchain (all four steps clean, single pass) — PASS.
- Pre-existing flaky test `AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException` (tracked under `ci-flaky-test-isolation-176`) — outside this feature's files; does not affect this feature's gate.
