# swordfish-dictionary-lineage — Spec

- **Issue:** #306
- **Parent (optional):** Epic swordfish-removal (child F1, wave 0)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-10T20-14
- **Status:** Draft
- **Version:** 0.2

## Overview

The vendored `ScoDictionary<TKey,TValue>` (`UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\SCO\SCODictionary.cs`) derives from the Swordfish `ConcurrentObservableDictionary`. It is one of the first-party types that keep the unmaintained `Swordfish.NET.General` project in the build. The repository already contains a Swordfish-free equivalent, `UtilitiesCS.ReusableTypeClasses.ScoDictionaryNew<TKey,TValue>` (derives from the clean `ConcurrentObservableDictionary`, `SmartSerializable`-based). Re-pointing every production consumer to `ScoDictionaryNew` removes a Swordfish dependency and unblocks the epic teardown (F5).

## Behavior

Replace `ScoDictionary<TKey,TValue>` with `ScoDictionaryNew<TKey,TValue>` at every production consumer, migrate affected tests, and preserve on-disk JSON serialization compatibility for every persisted dictionary.

Production consumers to re-point:

- `TaskMaster\AppGlobals\AppToDoObjects.cs`: `_dictRemap` (`DictRemap`, `ScoDictionary<string,string>`), `_filteredFolderScraping` (`FilteredFolderScraping`, `ScoDictionary<string,int>`), `_folderRemap` (`FolderRemap`, `ScoDictionary<string,string>`).
- `UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapEncoder.cs`: `_encoder` (`ScoDictionary<string,int>`), `_decoder` (`ScoDictionary<int,string>`); update `ISubjectMapEncoder.Encoder` return type.
- `UtilitiesCS\OutlookObjects\Folder\FolderScorer.cs`: `_folderNameScores` (`ScoDictionary<string,long>`).
- `UtilitiesCS\Interfaces\IGlobals\IToDoObjects.cs`: `FilteredFolderScraping`, `FolderRemap`, and `DictRemap` interface members — a cross-module contract change.
- Ripple consumers (dictionary lineage, in scope): `UtilitiesCS\OutlookObjects\MailItem\EmailDetails.cs` and `UtilitiesCS\OutlookObjects\MailItem\EmailDetailsWrapper.cs`, which take `IScoDictionary<string,string> dictRemap` parameters and must move to `IScoDictionaryNew<string,string>`.
- Confirm `ToDoModel\Data Model\People\PeopleScoDictionary.cs` reference is inert (entirely block-commented). No F1 change is required for that file.

Reconcile the constructor/deserialize shape difference: legacy `ScoDictionary` uses self-loading `(filename, folderpath)` constructors that mutate `this` in place; `ScoDictionaryNew` uses the factory-style `Static.Deserialize(fileName, folderPath)` path, which returns a new instance and performs deferred writes.

## Inputs / Outputs

- Inputs: existing on-disk JSON dictionary files under PythonStaging (for `DictRemap`, `FilteredFolderScraping`, `FolderRemap`) and the SubjectMap `Encoder` file (constructor-supplied filename/folderpath). No new CLI flags, env vars, or config keys are introduced.
- Outputs: re-serialized dictionary files in the identical flat `{"key": value}` JSON shape. No new artifacts, logs, or telemetry.
- Config keys and defaults: unchanged. Filenames continue to come from `_defaults` (`FileName_DictRemap`, `FileName_FilteredFolderScraping`, `FileName_FolderRemap`) and from the SubjectMap encoder constructor arguments.
- Versioning / backward-compatibility constraints: the on-disk JSON format is a compatibility contract. Existing persisted files must remain readable after the type swap without any data migration. See Data & State and Acceptance Criteria.

## API / CLI Surface

This feature has no CLI surface. The affected programmatic surface is:

- `IToDoObjects` (`UtilitiesCS\Interfaces\IGlobals\IToDoObjects.cs`): members `FilteredFolderScraping`, `FolderRemap`, and `DictRemap` change type from the legacy lineage to `ScoDictionaryNew<string,int>`, `ScoDictionaryNew<string,string>`, and `IScoDictionaryNew<string,string>` respectively. This is a cross-module contract change; all implementers (`AppToDoObjects`) and all callers must compile.
- `ISubjectMapEncoder.Encoder`: return type changes to `IScoDictionaryNew<string,int>`.
- `EmailDetails` / `EmailDetailsWrapper`: `dictRemap` parameter type changes to `IScoDictionaryNew<string,string>`.
- Construction / persistence contract: persisted instances are loaded via `ScoDictionaryNew<...>.Static.Deserialize(fileName, folderPath)` and written via plain `Serialize()` / `SerializeToString()`. The legacy no-arg `Deserialize()` and no-arg `ToDictionary()` call sites in `SubjectMapEncoder` (which have no `ScoDictionaryNew` equivalent) must be rewritten to the factory-load path and to direct dictionary iteration / `new Dictionary<>(...)`.

## Data & State

- Data flow: each persisted dictionary is loaded from disk at initialization (lazy `Initialized`/`Initializer.GetOrLoad` pattern in `AppToDoObjects`; constructor/getter in `SubjectMapEncoder`) and re-serialized on mutation.
- Data transformations and invariants: entry contents (keys and values) are preserved exactly across the type swap. The on-disk JSON shape MUST remain the flat `{"key": value}` object.
- Persistence details: on the production (non-globals) path, both lineages serialize with `TypeNameHandling.None`/`Auto` where the declared type equals the runtime type, producing a flat dictionary object with no `$type`, `$id`, or wrapper. `ScoDictionaryNew`'s default `Static.Deserialize` path round-trips the same flat shape, so a bare type swap preserves on-disk compatibility WITHOUT a serialization binder or converter.
- Compatibility constraint (design invariant): the migrated consumers MUST NOT use the globals-based `GetSettingsJson<T>(globals)` path. That path registers `ScoDictionaryConverter` and `PreserveReferencesHandling.All` and emits an incompatible `{ "$id", "CoDictionary", "RemainingObject" }` wrapper object, which would break every existing persisted file. The four persisted dictionaries must use the default `Static.Deserialize` / plain `Serialize()` path only.
- Migration / backfill: none required. Because legacy files carry no `$type` token (write-path `TypeNameHandling` is disabled), there is no type token to remap and no binder is needed.

Persisted vs in-memory classification (determines which need an on-disk compatibility test):

| Consumer field | Type | Persisted? | On-disk compat test |
|---|---|---|---|
| `AppToDoObjects.DictRemap` (`_dictRemap`) | `ScoDictionary<string,string>` | YES (PythonStaging) | YES |
| `AppToDoObjects.FilteredFolderScraping` | `ScoDictionary<string,int>` | YES (PythonStaging) | YES |
| `AppToDoObjects.FolderRemap` | `ScoDictionary<string,string>` | YES (PythonStaging) | YES |
| `SubjectMapEncoder.Encoder` (`_encoder`) | `ScoDictionary<string,int>` | YES (ctor filename/folderpath) | YES |
| `SubjectMapEncoder.Decoder` (`_decoder`) | `ScoDictionary<int,string>` | NO (derived in-memory) | no |
| `FolderScorer._folderNameScores` | `ScoDictionary<string,long>` | NO (`new()`, never serialized) | no |

Discrepancies vs the epic text (called out per research):

- The epic lists "FolderScorer scores" among persisted dictionaries. The current code does not persist `_folderNameScores` (no filename, no serialize/deserialize; it is a transient per-mail scoring buffer that is `Clear()`-ed on each load). Its migration is a pure in-memory type swap; no on-disk compatibility test is warranted.
- The epic omits `DictRemap`, but `DictRemap` IS persisted (PythonStaging) and DOES require an on-disk compatibility test.

Net: four persisted dictionaries require on-disk round-trip compatibility tests — `DictRemap`, `FilteredFolderScraping`, `FolderRemap`, and the SubjectMap `Encoder`.

## Constraints & Risks

- On-disk compatibility is the primary risk. Research (`research/research-dictionary-lineage.2026-07-10T20-16.md`, sections 2 and 4) resolved the open question: production payloads are NOT type-name-embedded, so the bare type swap preserves compatibility CONDITIONAL on not using the globals-converter path. The globals path is the only way this migration breaks the on-disk format and must be avoided for these dictionaries.
- `ScoDictionaryNew` writes are deferred (`SmartSerializable.RequestSerialization` schedules the write via a 3-second timer). Compatibility tests must drive `SerializeToString()` / `SerializeThreadSafe(path)` directly or inject the deterministic `TimerFactory` seam; they must not rely on wall-clock elapse. Confirm the deferred-write timer does not change observable startup ordering for the `LoadParallelAsync` tasks.
- The highest-risk edit is the `SubjectMapEncoder` rewrite (reconciling the legacy no-arg `Deserialize()` and no-arg `ToDictionary()` call sites and the duplicate-key rebuild path). It needs focused positive, negative, and edge tests.
- Scope boundary: do NOT delete `UtilitiesSwordfish`, remove any `ProjectReference`, touch `TaskMaster.sln`, or migrate `IScoCollection`/`IScoCollection2` — those are child F5. Do not touch collection/stack types (F2) or `ScoSortedDictionary` (F3). See Non-Goals.

## Implementation Strategy

- Implementation scope: re-point each production consumer field/property/interface member from `ScoDictionary<>`/`IScoDictionary<>` to `ScoDictionaryNew<>`/`IScoDictionaryNew<>`. Construct persisted instances via `ScoDictionaryNew<...>.Static.Deserialize(fileName, folderPath)`; persist via plain `Serialize()` / `SerializeToString()`. In-memory-only fields (`Decoder`, `_folderNameScores`) are a pure type swap.
- New/updated types: no new production types are required. `AppToDoObjects`, `SubjectMapEncoder`, `FolderScorer`, `IToDoObjects`, `ISubjectMapEncoder`, `EmailDetails`, and `EmailDetailsWrapper` are updated in place. Preserve the existing `Initialized` / `Initializer.GetOrLoad` lazy pattern in `AppToDoObjects`.
- Dependency changes: none in F1. No package or `ProjectReference` is added or removed.
- Logging/telemetry: no additions.
- Rollout: single change set; no feature flag. Backward compatibility is guaranteed by the preserved on-disk format rather than by a staged fallback.
- Optional deletion: after re-pointing, the concrete `ScoDictionary<>` class becomes production-unreferenced. `SCODictionary.cs` and its two direct test files (`SCODictionary_Tests.cs`, `SCODictionary_Additional_Tests.cs`) MAY be deleted within F1 ONLY if every remaining test reference is migrated in the same change, including the SmartSerializable negative-sample tests (`SmartSerializableStatic_Tests.cs`, `SmartSerializableNonTyped_Tests.cs`, `SmartSerializableBase_Tests.cs`) which use `ScoDictionary` as a convenient non-`ISmartSerializable` sample type and require a substitute type. Deletion is optional, not required. The legacy interface `IScoDictionary<TKey,TValue>` is a separate type and need not be deleted.

## Acceptance Criteria

- [ ] Every production consumer of `ScoDictionary<TKey,TValue>` is re-pointed to `ScoDictionaryNew<TKey,TValue>`: `AppToDoObjects` (`_dictRemap`/`DictRemap`, `_filteredFolderScraping`/`FilteredFolderScraping`, `_folderRemap`/`FolderRemap`), `SubjectMapEncoder` (`_encoder`, `_decoder`), and `FolderScorer` (`_folderNameScores`).
- [ ] The `IToDoObjects` contract change compiles across all modules: `FilteredFolderScraping` -> `ScoDictionaryNew<string,int>`, `FolderRemap` -> `ScoDictionaryNew<string,string>`, `DictRemap` -> `IScoDictionaryNew<string,string>`; all implementers (`AppToDoObjects`) and all callers compile, including the `EmailDetails.cs` / `EmailDetailsWrapper.cs` ripple consumers whose `dictRemap` parameter becomes `IScoDictionaryNew<string,string>`, and the `ISubjectMapEncoder.Encoder` return type becomes `IScoDictionaryNew<string,int>`.
- [ ] A per-persisted-dictionary on-disk JSON round-trip compatibility test exists for each of `DictRemap`, `FilteredFolderScraping`, `FolderRemap`, and SubjectMap `Encoder`, that loads a representative existing flat-shape `{"key": value}` payload through the new `Static.Deserialize` path (via injected read seam, no temporary files), verifies successful deserialization and entry fidelity, and re-serializes to a flat object asserting the absence of `$type`/`$id`/`CoDictionary`/`RemainingObject` tokens.
- [ ] The in-memory-only fields (`SubjectMapEncoder.Decoder`, `FolderScorer._folderNameScores`) are migrated as a pure type swap with no on-disk compatibility test, consistent with the persisted-vs-in-memory classification (including the two documented discrepancies vs the epic text).
- [ ] The globals-converter-path compatibility constraint is respected: none of the four persisted dictionaries registers `GetSettingsJson<T>(globals)`, `ScoDictionaryConverter`, or `PreserveReferencesHandling.All`; they use only the default `Static.Deserialize` / plain `Serialize()` path.
- [ ] The `SubjectMapEncoder` construction/persistence reconciliation is complete: the legacy self-loading `(filename, folderpath)` constructor is replaced with `Static.Deserialize`, and the no-arg `Deserialize()` and no-arg `ToDictionary()` call sites are rewritten to new-lineage equivalents; existing negative-path behavior (missing-file create-empty-and-write, duplicate-key rebuild) is preserved.
- [ ] Affected tests are migrated: consumer-coupled fixtures that construct legacy `ScoDictionary`/`IScoDictionary` are moved to `ScoDictionaryNew`/`IScoDictionaryNew` (including `EmailDetailsTests.cs` and `EmailDetailsWrapperTests.cs`).
- [ ] `PeopleScoDictionary.cs` is confirmed inert (entirely block-commented) with no F1 change.
- [ ] Optional: if `SCODictionary.cs` and its direct tests are deleted, all remaining test references — including the three SmartSerializable negative-sample tests — are migrated to a substitute non-`ISmartSerializable` type in the same change. If deletion is not pursued, this criterion is not applicable.
- [ ] Full C# toolchain is green in a single final pass (csharpier -> .NET analyzers -> nullable/TreatWarningsAsErrors -> MSTest via vstest), and coverage thresholds hold for changed/new code.

## Definition of Done

- [ ] Acceptance criteria documented and mapped to tests or demos
- [ ] Behavior matches acceptance criteria in all documented environments
- [ ] Tests updated/added (unit as applicable)
- [ ] Edge cases and error handling covered by tests (missing-file create-empty-and-write; SubjectMap duplicate-key rebuild)
- [ ] Docs updated (README, docs/features/active/... links)
- [ ] Telemetry/logging added or updated (not applicable — none introduced)
- [ ] Toolchain pass completed (format -> lint -> type-check -> test)

## Non-Goals

- Do NOT delete `UtilitiesSwordfish`, remove any `ProjectReference`, or touch `TaskMaster.sln` (child F5).
- Do NOT migrate `IScoCollection` / `IScoCollection2` (child F5).
- Do NOT touch collection/stack types (child F2) or `ScoSortedDictionary` (child F3).
- Do NOT switch any persisted dictionary to the globals-based `GetSettingsJson`/`ScoDictionaryConverter`/`PreserveReferencesHandling` path.
- `SCODictionary.cs` deletion is optional, not a goal of F1.

## Seeded Test Conditions (from potential)

- [ ] Round-trip JSON compatibility test per persisted dictionary (DictRemap, FilteredFolderScraping, FolderRemap, SubjectMap Encoder).
- [ ] Interface contract change compiles across all modules.
- [ ] Existing dictionary behavior (add/remove/lookup/observe) preserved.
