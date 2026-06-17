# Remediation Inputs (Cycle 3): hierarchical-lcppn-folder-prediction (#177)

**Cycle:** 3
**Entry timestamp:** 2026-06-16T01-04 (UTC)
**Authored by:** orchestrator
**Base:** `main`
**Head:** `TaskMaster-wt-2026-06-08-12-06` (`eebcc910`)

## Trigger

Not an audit failure. The cycle-1/cycle-2 reaudit chain closed clean (`blocking_count = 0`,
2026-06-12T17-14). After that, a migration-posture review (research artifact
`artifacts/research/2026-06-15T00-00-issue-177-lcppn-integration-findings.md`) established that the
merged feature is an additive, flag-gated seam that is **not reachable in production** (callers use
the default-off config) and **not persisted across restart** (no load path). The user reviewed the
gap list and selected **option B: expand scope before the PR**. Under the orchestration protocol this
is treated as a new remediation cycle because acceptance criteria are now unmet (new AC21–AC24).

User decision recorded: enablement is a **toggleable config setting with default value ON**, with
**fallback to the flat `BayesianClassifierGroup`** whenever the LCPPN predictor is null or unbuilt.

## In-scope findings for cycle 3

### F4 [AC21, AC22 — production enablement, default ON] LCPPN unreachable from production callers

- Current state: `LcppnFolderPredictorConfig.UseLcppnPredictor` defaults to `false`
  (`LcppnFolderPredictorConfig.cs:20`; default set at `OlFolderClassifierGroup.cs:40-41`). The three
  production callers construct `new OlFolderClassifierGroup(globals)` with that default and never set
  the flag (`EmailFiler.cs:371`, `SortEmail.cs:251,584`, `FolderScorer.cs:162,169`). LCPPN therefore
  never activates at runtime.
- Required outcome:
  1. Source `UseLcppnPredictor` from the application's persistent settings/config mechanism, defaulting
     to **ON (`true`)**, so it is honored by all production callers **without** hand-editing each call
     site. Prefer centralizing the default at `OlFolderClassifierGroup` construction / config
     resolution over editing the three call sites individually; investigate the existing settings
     mechanism and follow the established pattern. If a per-call-site edit is genuinely unavoidable,
     keep it minimal and within the small change budget, and record the rationale.
  2. The setting must remain toggleable to OFF; with it OFF, behavior is byte-for-byte the flat path
     (AC13 must continue to pass).
  3. Fallback (AC22): when the setting is ON but `Globals.AF.FolderPredictor` is null/unbuilt,
     `GetFolderPredictorAsync` (`OlFolderClassifierGroup.cs:80-90`) returns the flat group without
     throwing. The accessor already has this branch (`cs:82-87`); add/confirm a regression test that
     exercises it under the default-ON configuration.
- Verification: a test demonstrates that under default configuration (no explicit flag set) a
  production-style construction selects LCPPN once a predictor is present, and falls back to flat when
  it is absent; AC13 (flag-off flat parity) still passes.

### F5 [AC23 — persistence / load-on-startup] LCPPN predictor is lost on restart

- Current state: `BuildClassifiersAsync` builds LCPPN into `Globals.AF.FolderPredictor`
  (`OlFolderClassifierGroup.cs:279-281`), but `AppAutoFileObjects.LoadAsync` has no step that
  rehydrates `Globals.AF.FolderPredictor`, and there is no registered serialize file path
  (`LcppnFolderPredictor.Serialize()` exists but `SmartSerializable.Config` is not configured in the
  production build path — research gap items 1 and 3). After restart with the setting ON, the holder
  is null and the accessor silently falls back to flat until a manual rebuild.
- Required outcome:
  1. `LcppnFolderPredictor` is serialized to its **own file**, distinct from `Folder.json`, on the
     build/serialize path. Configure the `SmartSerializable` file name/path consistent with how
     `Manager["Folder"]`/`Folder.json` is configured; investigate `Manager.Configuration` and
     `EmailFiler.SerializeFolderManagerAsync` (`EmailFiler.cs:374-378`).
  2. At application startup, `AppAutoFileObjects.LoadAsync` (or the `Manager.Configuration` load
     registration) rehydrates the persisted predictor into `Globals.AF.FolderPredictor`, so it
     survives restart without a manual `BuildClassifiersAsync` rerun.
  3. If the persisted file is absent or unreadable, loading must not throw: the holder stays null and
     the accessor falls back to flat (AC22). Fail-soft on load, fail-fast only on genuine corruption
     surfaced through the project logging pattern.
- Verification: serialization round-trip test for the dedicated file; a load-path test that, given a
  persisted predictor file, populates `Globals.AF.FolderPredictor`; a negative test that a missing
  file leaves the holder null and triggers flat fallback. No temporary files (use the project's
  in-memory/seam serialization test pattern).

## Out-of-scope for cycle 3 (recorded, not remediated here)

- **Retiring the always-on flat rebuild.** `BuildClassifiersAsync` will continue to build and
  serialize `Manager["Folder"]` (the flat group); it is the fallback path the user requires. Research
  gap item 4 is intentionally NOT actioned.
- **Extending LCPPN to non-folder classifiers** (spam, triage, category/multiclass, actionable):
  excluded (research Q1; non-goal).
- **Incremental reparenting** (research gap item 5): documented design limitation, unchanged.
- **Pre-existing over-cap production files** `BayesianClassifierGroup.cs` (515), `FolderScorer.cs`
  (608), `SortEmail.cs` (1406): pre-existing overages, separate refactors, not remediated here. Note:
  if production enablement requires edits inside `FolderScorer.cs` or `SortEmail.cs`, the edit must not
  increase those files' line counts beyond their current values; prefer centralizing at
  `OlFolderClassifierGroup` to avoid touching them.
- Pre-existing flaky `IdleAsyncQueue_Tests.AddEntry_UseUiThreadTrue` (`ci-flaky-test-isolation-176`):
  out of scope; passes in isolation.

## Containment constraints (must hold)

- Zero diff in spam/triage/category/actionable subsystems: `SpamBayes.cs`, `Triage.cs`,
  `CategoryClassifierGroup.cs`, `MulticlassEngine.cs`, and `Manager["Actionable"]` usage.
- `ManagerAsyncLazy` dictionary value typing unchanged (the cycle-1 Option-B containment decision).
- AC1–AC20 remain satisfied; AC13 (flag-off flat parity) explicitly re-verified.

## Exit condition for cycle 3

End-of-cycle reaudit (three reaudit artifacts: `code-review`, `feature-audit`, `policy-audit`) must
show `blocking_count == 0`: AC21–AC24 satisfied (LCPPN default-ON and reachable from production
callers, safe fallback, persisted and reloaded across restart, containment held), AC1–AC20 still
satisfied, coverage policy met (new/changed code >= 90% strict, repo >= 80%), and the full C#
toolchain green in a single final pass.
