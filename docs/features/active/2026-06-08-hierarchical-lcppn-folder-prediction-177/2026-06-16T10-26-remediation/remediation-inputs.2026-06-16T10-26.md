# Remediation Inputs (Cycle 4): hierarchical-lcppn-folder-prediction (#177)

**Cycle:** 4
**Entry timestamp:** 2026-06-16T10-26 (UTC)
**Authored by:** orchestrator
**Base:** `main`
**Head:** `TaskMaster-wt-2026-06-08-12-06` (`ac3d6b53`)

## Trigger

Cycle 3 surfaced and recorded a latent serialization defect in `FilePathHelper` (a shared
serialization utility) and worked around it for #177 by excluding the runtime-only `Config` from the
serialized document. The cycle-3 reaudit classified the underlying defect as pre-existing and out of
scope. The user has directed that the root-cause defect be fixed before the branch is pushed. Per the
scope-change rule this is a new finding and opens a dedicated cycle (cycle 4) rather than reopening the
closed cycle 3. Root cause, fix shape, and blast radius are documented in
`artifacts/research/2026-06-16-filepathhelper-deserialization-nre-research.md`.

## Single in-scope finding for cycle 4

### F6 [AC25 — FilePathHelper deserialize-safe] NRE on Json.NET deserialization of a populated FilePathHelper

- Root cause (from research): Json.NET uses the default constructor and sets properties in document
  order. The `FileStemSeed` setter calls `NotifyPropertyChanged()`, whose handler
  (`FilePathHelper_PropertyChanged`) calls the instance `AdjustForMaxPath()`
  (`UtilitiesCS/.../FilePathHelper.cs`, ~line 298). `AdjustForMaxPath()` can pass the
  `StemInitialized()` guard and then dereference `FileExtension.Length` while `_fileExtension` is still
  `null` (not yet populated by Json.NET) -> `NullReferenceException`. The constructor-time field
  ordering that normally protects the handler is bypassed by the default-ctor + property-set
  deserialization path.
- Required outcome:
  1. Apply the minimal, contract-preserving root-cause fix recommended by research: guard
     `AdjustForMaxPath()` so it returns safely (e.g. `return false`) when any of `_fileExtension`,
     `_fileStemSuffix`, or `_fileStemSeed` is `null`, placed immediately after the `StemInitialized()`
     check. The exact guard form is the implementer's call provided it is minimal, preserves existing
     non-deserialization behavior, and resolves the NRE.
  2. No change to the serialized-document shape. No public-API change to `FilePathHelper`.
  3. Add deterministic MSTest regression tests at
     `UtilitiesCS.Test/HelperClasses/FilePathHelper_Tests.cs`: one that reproduces the prior
     throw-on-deserialize using a `FileStemSeed`-first JSON document (fails before the fix), and one
     full serialize/deserialize round-trip via `JsonConvert`. No temporary files, no real filesystem,
     no external dependencies; MSTest + FluentAssertions (Moq only if needed).
- Verification: the new tests pass after the fix; pre-existing `FilePathHelper` behavior is unchanged
  (all existing `FilePathHelper` tests still pass); changed lines meet coverage policy; full C#
  toolchain green in a single final pass.

## Explicit retention (do NOT revert)

- Retain the cycle-3 `DoNotSerializeContractResolver("Config")` exclusion in
  `LcppnFolderPredictorStore.BuildSettings()`. It is orthogonal to this fix and correct by the
  `SmartSerializable` contract (`Config` is loader-supplied runtime state; serializing it would embed a
  machine-specific path). AC23 must remain satisfied with the exclusion in place.

## Out-of-scope for cycle 4 (recorded, not remediated here)

- Broader redesign of `FilePathHelper` property-change/recompute behavior beyond the null-guard.
- Any change to other `SmartSerializable<T>` consumers (`ScoDictionaryNew`,
  `WrapperPeopleScoDictionaryNew`); the null-guard fixes them transitively without per-type edits, and
  no per-type behavior change is intended.
- Re-opening any AC1–AC24 work; those remain satisfied and must not regress.

## Containment constraints (must hold)

- Change confined to `FilePathHelper.cs` (production) + `FilePathHelper_Tests.cs` (test). No edits to
  `LcppnFolderPredictor*`, the spam/triage/category/actionable subsystems, `ManagerAsyncLazy`, or the
  cycle-3 enablement/persistence files.
- AC1–AC24 remain satisfied; AC23 explicitly re-verified with the `Config` exclusion retained.
- File-size cap respected (`FilePathHelper.cs` must not exceed 500 lines after the +3-line guard; if
  it is already over cap, this is a pre-existing overage — confirm at Phase 0 and do not grow it beyond
  the minimal guard).

## Change-budget estimate

Small: 1 production file (`FilePathHelper.cs`, ~+3 lines) + 1 test file (`FilePathHelper_Tests.cs`,
~+30 lines). Defect/bugfix discipline applies (failing regression test first, minimal targeted fix).

## Exit condition for cycle 4

End-of-cycle reaudit (three reaudit artifacts) must show `blocking_count == 0`: AC25 satisfied
(deserialize round-trips without NRE, regression test in place), AC1–AC24 still satisfied (AC23 with
the `Config` exclusion retained), changed lines meet coverage policy, containment held, and the full
C# toolchain green in a single final pass.
