# swordfish-dictionary-lineage (Issue #306)

- Date captured: 2026-07-10
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/swordfish-dictionary-lineage/ (Issue #306)
- Epic: swordfish-removal (child F1, wave 0)
- Integration branch: epic/swordfish-removal-integration

- Issue: #306
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/306
- Last Updated: 2026-07-11
- Work Mode: full-feature

## Problem / Why

The vendored `ScoDictionary<TKey,TValue>` (`UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\SCO\SCODictionary.cs`) derives from the Swordfish `ConcurrentObservableDictionary`. It is one of the first-party types that keep the unmaintained `Swordfish.NET.General` project in the build. The repository already contains a Swordfish-free equivalent, `UtilitiesCS.ReusableTypeClasses.ScoDictionaryNew<TKey,TValue>` (derives from the clean `ConcurrentObservableDictionary`, `SmartSerializable`-based). Re-pointing every production consumer to `ScoDictionaryNew` removes a Swordfish dependency and unblocks the epic teardown (F5).

## Proposed Behavior

Replace `ScoDictionary<TKey,TValue>` with `ScoDictionaryNew<TKey,TValue>` at every production consumer, migrate affected tests, and preserve on-disk JSON serialization compatibility for every persisted dictionary.

Production consumers to re-point:
- `TaskMaster\AppGlobals\AppToDoObjects.cs`: `_dictRemap`, `FilteredFolderScraping`, `FolderRemap` (`ScoDictionary<string,string>`, `<string,int>`).
- `UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapEncoder.cs`: `_encoder`, `_decoder` (`<string,int>`, `<int,string>`).
- `UtilitiesCS\OutlookObjects\Folder\FolderScorer.cs`: `_folderNameScores` (`<string,long>`).
- `UtilitiesCS\Interfaces\IGlobals\IToDoObjects.cs`: `FilteredFolderScraping`, `FolderRemap` (interface members — a cross-module contract change).
- Confirm `ToDoModel\Data Model\People\PeopleScoDictionary.cs` reference is inert (commented-out).

Reconcile the constructor/deserialize shape difference: legacy `ScoDictionary` takes `(filename, folderpath)`; `ScoDictionaryNew` uses the `Static.Deserialize` / converter-based path.

## Acceptance Criteria (early draft)

- [ ] Every production consumer of `ScoDictionary<TKey,TValue>` is re-pointed to `ScoDictionaryNew<TKey,TValue>`.
- [ ] The `IToDoObjects` interface members `FilteredFolderScraping` and `FolderRemap` use the new type; all implementers and callers compile.
- [ ] On-disk JSON serialization compatibility is preserved for each persisted dictionary (FolderRemap, FilteredFolderScraping, SubjectMap encoder/decoder, FolderScorer scores); a round-trip compatibility test exists per persisted dictionary.
- [ ] Affected tests migrated; full C# toolchain (csharpier -> analyzers -> nullable -> MSTest) green; coverage thresholds hold for changed/new code.
- [ ] If re-pointing leaves the legacy `ScoDictionary` class unreferenced, `SCODictionary.cs` and its direct tests may be deleted within this feature.

## Constraints & Risks

- Open question (research): are on-disk JSON payloads type-name-embedded (Newtonsoft `TypeNameHandling.Auto`) such that renaming the concrete dictionary type breaks deserialization of existing persisted files? If so, explicit migration/converter work is required rather than a bare type swap.
- Scope boundary: do NOT delete `UtilitiesSwordfish`, remove any `ProjectReference`, touch `TaskMaster.sln`, or migrate `IScoCollection`/`IScoCollection2` — those are child F5. Do not touch collection/stack types (F2) or `ScoSortedDictionary` (F3).

## Test Conditions to Consider

- [ ] Round-trip JSON compatibility test per persisted dictionary.
- [ ] Interface contract change compiles across all modules.
- [ ] Existing dictionary behavior (add/remove/lookup/observe) preserved.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/swordfish-dictionary-lineage/` folder from the template
