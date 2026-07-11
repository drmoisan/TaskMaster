# swordfish-collection-stack-lineage (Issue #307)

- Date captured: 2026-07-10
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/swordfish-collection-stack-lineage/ (Issue #307)
- Epic: swordfish-removal (child F2, wave 0, complexity C3)
- Integration branch: epic/swordfish-removal-integration

- Issue: #307
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/307
- Last Updated: 2026-07-11
- Work Mode: full-feature

## Problem / Why

The vendored `Swordfish.NET.General` project is unmaintained third-party code that the
No-COM/testability direction seeks to remove. Two vendored-based collection types anchor a
tree of first-party dependents:

- `ScoCollection<T>` (`UtilitiesCS\...\SCO\ScoCollection.cs`:
  `ScoCollection<T> : ConcurrentObservableCollection<T>, IList<T>, IList` [Swordfish]).
- `ScoStack<T>` (`UtilitiesCS\...\SCO\ScoStack.cs`: `ScoStack<T> : ScoCollection<T>`).

Until every first-party type that derives from or consumes these two types is re-based onto the
repository's Swordfish-free equivalents, the epic's teardown child (F5) cannot remove the
`UtilitiesSwordfish` project reference. F2 performs the collection + stack lineage migration.

## Proposed Behavior

Re-base every first-party consumer of the Swordfish-based collection/stack onto the repository's
Swordfish-free equivalents while preserving on-disk JSON compatibility and undo behavior:

- Collection lineage: the clean `ConcurrentObservableCollection`
  (`UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.*`).
- Ordered/indexable/observable stack: `SloLinkedList<T>`
  (`: LockingObservableLinkedList<T>`, `SmartSerializable`-based).

Subclasses to re-base onto the clean collection:
- `UtilitiesCS\EmailIntelligence\Ctf\CtfMap.cs` (`CtfMap : ScoCollection<CtfMapEntry>`).
- `UtilitiesCS\EmailIntelligence\Recents\RecentsList.cs` (`RecentsList<T> : ScoCollection<T>`).
- `UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapSco.cs`
  (`SubjectMapSco : ScoCollection<SubjectMapEntry>`, incl. `AltListLoader`).

Direct `ScoCollection<T>` consumers:
- `TaskMaster\AppGlobals\AppAutoFileObjects.cs`: `Filters` (`ScoCollection<FilterEntry>`).
- `TaskMaster\AppGlobals\AppToDoObjects.cs`: `PrefixList` (`ScoCollection<IPrefix>`).
- `UtilitiesCS\...\OlFolder\OlFolderClassifierGroup.cs`: `ScoCollection<MinedMailInfo>`.
- `UtilitiesCS\Interfaces\IGlobals\IAppAutoFileObjects.cs`: `Filters`.
- `UtilitiesCS\Interfaces\IGlobals\IToDoObjects.cs`: `PrefixList`, `LoadPrefixList`.

`ScoStack<IMovedMailInfo>` replacement (build on `SloLinkedList`, NOT `ScBag`). `ScBag`
(`: ConcurrentBag`) is unordered, non-indexable, non-observable and cannot satisfy the consumers.
Both undo loops (`SortEmail.UndoAsync` and `QfcFormController.Actions.cs`) use index reads
`stack[i]` and positional `Pop(i)`, which require an ordered, indexable, observable base;
`SloLinkedList` satisfies ordered+observable+serializable.

`ScoStack<IMovedMailInfo>` consumers:
- `QuickFiler\Controllers\QfcCollectionController.cs`, `QfcDatamodel.cs`, `QfcFormController.cs`,
  `Interfaces\IQfcCollectionController.cs`, `IQfcDatamodel.cs`.
- `TaskMaster\AppGlobals\AppAutoFileObjects.cs` (`MovedMails`, `LoadMovedMails`),
  `UtilitiesCS\...\SortEmail.cs` (`UndoAsync`), `Interfaces\IAppAutoFileObjects.cs`.

## Acceptance Criteria (early draft)

- [ ] Every listed `ScoCollection`/`ScoStack` subclass and consumer is re-based onto the clean
      collection or the `SloLinkedList`-based stack; no first-party source outside F5 scope
      references `ScoCollection<T>` or `ScoStack<T>`.
- [ ] The `SloLinkedList`-based stack exposes the positional surface the undo loops require:
      `this[int]` indexer, `Peek(int)`, `Pop(int)`, `TryPeek`/`TryPop` (front and indexed);
      `Push`->AddFirst, `Pop()`/`Peek()`->TakeFirst/First.
- [ ] `SloLinkedList` exposes `SerializeAsync()` and completes the stubbed `ISmartSerializable`
      deserialize members the stack exercises.
- [ ] Construction is reconciled (file-based constructor or migration to `Static.Deserialize`).
- [ ] On-disk JSON compatibility is preserved for MovedMails, Filters, PrefixList, CtfMap, and
      SubjectMapSco; a round-trip compatibility test exists per persisted collection.
- [ ] QuickFiler and SortEmail undo behavior is preserved with no regression.
- [ ] Full C# toolchain passes (csharpier -> analyzers -> nullable -> MSTest); new SloLinkedList
      stack members meet the new-code coverage bar.

## Constraints & Risks

- On-disk JSON compatibility is the primary risk: if payloads embed concrete type names
  (`TypeNameHandling.Auto`), renaming collection/stack types breaks deserialization of existing
  persisted files and requires explicit migration/converter work.
- Scope boundary: do NOT delete the `UtilitiesSwordfish` project, remove any `ProjectReference`,
  touch `TaskMaster.sln`, or migrate `IScoCollection`/`IScoCollection2` (child F5). Do not touch
  dictionary types (F1) or `ScoSortedDictionary` (F3). Legacy `ScoCollection.cs`/`ScoStack.cs`
  and their direct tests MAY be deleted within F2 only if re-pointing leaves them unreferenced.
- No new production dependencies; no behavior/UX changes beyond the migration.

## Test Conditions to Consider

- [ ] Unit coverage for the new `SloLinkedList` positional stack members (indexer, Peek(int),
      Pop(int), TryPeek/TryPop front and indexed, SerializeAsync, deserialize paths).
- [ ] Round-trip JSON compatibility test per persisted collection (MovedMails, Filters,
      PrefixList, CtfMap, SubjectMapSco) against a representative existing on-disk payload shape.
- [ ] Regression tests for QuickFiler undo and SortEmail undo semantics (index read + positional
      Pop) with no behavior change.

## Open Questions (resolved in research)

- [ ] Is `RecentsList<T> : ScoCollection<T>` still consumed, or dead code superseded by
      `AppAutoFileObjects.RecentsList` (already `SloLinkedList<string>`)? Scope migration/removal
      accordingly.
- [ ] Are on-disk JSON payloads type-name-embedded such that renaming concrete types breaks
      deserialization of existing persisted files? Plan converter/migration work if so.
- [ ] Exact undo-loop semantics (index read + positional Pop) so the new positional surface
      preserves QuickFiler and SortEmail undo behavior.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create active feature folder from the template
