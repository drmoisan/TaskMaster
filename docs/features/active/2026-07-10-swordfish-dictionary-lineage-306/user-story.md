# `swordfish-dictionary-lineage` — User Story

- Issue: #306
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-07-10T20-14

## Story Statement

- As a maintainer of TaskMaster, I want every production consumer of the Swordfish-based `ScoDictionary<TKey,TValue>` re-pointed to the Swordfish-free `ScoDictionaryNew<TKey,TValue>`, so that a Swordfish dependency is removed and the epic teardown (F5) is unblocked.
- As an operator who relies on existing persisted dictionary files, I want the on-disk JSON format to remain unchanged after the migration, so that no data migration is required and existing files continue to load.
- As a developer maintaining `IToDoObjects` implementers and callers, I want the interface contract change to compile cleanly across all modules (including the `EmailDetails` ripple consumers), so that the type swap does not break the build.

## Problem / Why

The vendored `ScoDictionary<TKey,TValue>` (`UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\SCO\SCODictionary.cs`) derives from the Swordfish `ConcurrentObservableDictionary`. It is one of the first-party types that keep the unmaintained `Swordfish.NET.General` project in the build. The repository already contains a Swordfish-free equivalent, `UtilitiesCS.ReusableTypeClasses.ScoDictionaryNew<TKey,TValue>` (derives from the clean `ConcurrentObservableDictionary`, `SmartSerializable`-based). Re-pointing every production consumer to `ScoDictionaryNew` removes a Swordfish dependency and unblocks the epic teardown (F5).

## Personas & Scenarios

- Persona: TaskMaster maintainer executing the swordfish-removal epic.
  - Who: an engineer responsible for removing the unmaintained Swordfish dependency from the build.
  - What they care about: eliminating the Swordfish base type from first-party code without regressing behavior or breaking persisted data.
  - Constraints: on-disk JSON files produced by the legacy type must remain readable; the change must not extend into F2/F3/F5 scope; the full C# toolchain must stay green.
  - Goals and frustrations: wants a mechanical, low-risk type/lineage swap; frustrated by the constructor/persistence-shape difference between the self-loading legacy constructor and the factory-style `Static.Deserialize` path, and by the risk that a wrong serialization path silently changes the on-disk format.
  - Context and motivations: F1 is wave 0 of the epic; `ScoDictionary` is one of the last first-party consumers of the Swordfish base, so re-pointing it moves the epic toward final Swordfish removal.

- Scenario: migrating a persisted dictionary while preserving existing files.
  - Who is acting: the maintainer editing `AppToDoObjects`, `SubjectMapEncoder`, `FolderScorer`, `IToDoObjects`, and the `EmailDetails` ripple consumers.
  - What triggered the action: the epic requires removing the Swordfish-based dictionary from production code.
  - Steps: re-point each field/property/interface member to `ScoDictionaryNew`/`IScoDictionaryNew`; replace the self-loading `(filename, folderpath)` constructor with `ScoDictionaryNew<...>.Static.Deserialize(filename, folderpath)`; rewrite the `SubjectMapEncoder` no-arg `Deserialize()`/`ToDictionary()` call sites; keep plain `Serialize()`; add an on-disk round-trip compatibility test for each of the four persisted dictionaries.
  - Obstacles/decisions: must avoid the globals-based `GetSettingsJson`/`ScoDictionaryConverter`/`PreserveReferencesHandling` path, which would emit an incompatible wrapper shape; must classify each dictionary as persisted vs in-memory-only correctly, including the two discrepancies vs the epic text (FolderScorer scores are in-memory; DictRemap is persisted but was not listed by the epic); must drive deferred writes deterministically via the injected timer seam rather than wall-clock elapse.
  - Expected outcome: existing flat `{"key": value}` files load unchanged into the new type, all modules compile, and the toolchain is green with coverage held on changed/new code.

## Acceptance Criteria

The authoritative, technically detailed acceptance criteria are maintained in `spec.md` (`## Acceptance Criteria`). The story-level criteria below track the same delivery from the user perspective and must stay consistent with `spec.md`.

- [x] Every production consumer of `ScoDictionary<TKey,TValue>` (`AppToDoObjects` DictRemap/FilteredFolderScraping/FolderRemap, `SubjectMapEncoder` encoder/decoder, `FolderScorer` scores) is re-pointed to `ScoDictionaryNew<TKey,TValue>`.
- [x] The `IToDoObjects` interface members `FilteredFolderScraping`, `FolderRemap`, and `DictRemap` use the new lineage; all implementers and callers compile, including the `EmailDetails.cs` / `EmailDetailsWrapper.cs` ripple consumers and the `ISubjectMapEncoder.Encoder` return type.
- [x] On-disk JSON serialization compatibility is preserved for each of the four persisted dictionaries — `DictRemap`, `FilteredFolderScraping`, `FolderRemap`, and SubjectMap `Encoder` — with a per-dictionary round-trip compatibility test that loads a representative existing flat-shape payload and verifies successful deserialization and entry fidelity.
- [x] The two in-memory-only fields (SubjectMap `Decoder`, `FolderScorer` scores) are migrated as a pure type swap with no on-disk compatibility test, consistent with the documented discrepancies vs the epic text.
- [x] The globals-converter-path compatibility constraint is respected: no persisted dictionary registers `GetSettingsJson<T>(globals)`, `ScoDictionaryConverter`, or `PreserveReferencesHandling.All`.
- [x] Affected tests migrated; full C# toolchain (csharpier -> analyzers -> nullable -> MSTest) green; coverage thresholds hold for changed/new code.
- [ ] Optional: if re-pointing leaves the legacy `ScoDictionary` class unreferenced, `SCODictionary.cs` and its direct tests may be deleted within this feature only if all remaining test references (including the SmartSerializable negative-sample substitutions) are migrated in the same change.

## Non-Goals

- Deleting `UtilitiesSwordfish`, removing any `ProjectReference`, or touching `TaskMaster.sln` (child F5).
- Migrating `IScoCollection` / `IScoCollection2` (child F5).
- Touching collection/stack types (child F2) or `ScoSortedDictionary` (child F3).
- Switching any persisted dictionary to the globals-based converter/`PreserveReferencesHandling` path.
- `PeopleScoDictionary.cs` changes — the file is entirely block-commented (inert) and requires no F1 change.
