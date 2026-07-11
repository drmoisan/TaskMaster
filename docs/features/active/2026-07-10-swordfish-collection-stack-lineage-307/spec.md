# swordfish-collection-stack-lineage — Spec

- **Issue:** #307
- **Parent (optional):** Epic swordfish-removal (child F2, wave 0, complexity C3)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-10T21-15
- **Status:** Draft
- **Version:** 0.2
- **Work Mode:** full-feature

## Overview

The vendored `Swordfish.NET.General` project is unmaintained third-party code that the
No-COM/testability direction seeks to remove. Two vendored-based collection types anchor a tree
of first-party dependents:

- `ScoCollection<T>` (`UtilitiesCS\...\SCO\ScoCollection.cs`:
  `ScoCollection<T> : ConcurrentObservableCollection<T>, IList<T>, IList` [Swordfish]).
- `ScoStack<T>` (`UtilitiesCS\...\SCO\ScoStack.cs`: `ScoStack<T> : ScoCollection<T>`).

Until every first-party type that derives from or consumes these two types is re-based onto
Swordfish-free equivalents, the epic's teardown child (F5) cannot remove the `UtilitiesSwordfish`
project reference. F2 performs the collection + stack lineage migration: it re-bases every
first-party collection/stack dependent, migrates the affected tests, and preserves on-disk JSON
compatibility for the five persisted collections and the QuickFiler/SortEmail undo behavior.

## Problem Statement

F5 (teardown) is blocked until no first-party source references a `Swordfish.NET.*` type. F2
removes the collection and stack half of that dependency surface. The migration must satisfy two
hard constraints that the research verified:

1. **On-disk JSON compatibility.** Five persisted collections (MovedMails, Filters, PrefixList,
   CtfMap, SubjectMapSco) are read back from existing user files. The migration must not change
   the on-disk shape those files depend on.
2. **Undo-behavior preservation.** The MovedMails undo history is consumed by two undo loops
   (`SortEmail.UndoAsync` and `QfcFormController.UndoDialog`) whose positional semantics
   (forward index read `stack[i]`, positional `Pop(i)`) must be preserved exactly.

## Scope Correction (verified by research — authoritative)

The epic manifest and issue draft name a pre-existing "clean `ConcurrentObservableCollection` in
`UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.*`". Research verified that this type **does
not currently exist as first-party code**. The only production `ConcurrentObservableCollection<T>`
in the tree is the Swordfish one (`UtilitiesSwordfish\Collections\ConcurrentObservableCollection.cs`);
the clean `Concurrent.Observable` namespace contains only `Bag` and `Dictionary`.

Consequently, F2 must **create** the Swordfish-free clean collection base as enabling work inside
its own boundary. Research recommends building it on the existing Swordfish-free
`ObservableCollection<T>` foundation (in-repo precedent: `ObservableCollectionBatchUpdate<T> :
ObservableCollection<T>`), which supplies `IList<T>` + `INotifyCollectionChanged` natively and
avoids porting the Swordfish `ConcurrentObservableBase`/`ImmutableCollection`/`DoubleLinkList` tree.
This clean-collection creation is the single largest work item in F2 and gates the four
subclass/consumer re-bases. It does NOT cross into F1 (dictionaries), F3 (`ScoSortedDictionary`),
or F5 (interfaces / project teardown).

## In-Scope Work Items

### 1. Create the Swordfish-free clean collection base

Create a clean `ConcurrentObservableCollection<T>` under
`UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Collection` (final name chosen to avoid
clashing with the Swordfish namespace during coexistence), built on
`System.Collections.ObjectModel.ObservableCollection<T>`, carrying at minimum:

- `IList<T>` and non-generic `IList` surface (`this[int]` get/set, `Add`, `Insert`, `RemoveAt`,
  `Remove`, `Contains`, `IndexOf`, `Count`, `CopyTo`, `IsReadOnly`, enumeration).
- List-style helpers the Sco subclasses rely on: `FindIndex` (overloads), `FindIndices`,
  `Find(Predicate<T>)`, `Exists`.
- Observable surface: `event NotifyCollectionChangedEventHandler CollectionChanged` and
  `IDisposable Subscribe(IObserver<NotifyCollectionChangedEventArgs>)`.
- The ScoCollection serialization surface: file constructors including the AltListLoader/backup
  constructor, `Serialize()`/`Serialize(path)`, `SerializeAsync()`/`SerializeAsync(path)`,
  `Deserialize` overloads, `FilePath`/`FolderPath`/`FileName`, `ToList()`/`FromList(IList<T>)`,
  and injectable filesystem/prompt seams for testability.
- **Serialization guardrail:** the type must remain an array-serializing `IEnumerable`/`IList<T>`
  and must NOT carry `[JsonObject]` or otherwise force object serialization, so Newtonsoft keeps
  the `JsonArrayContract` and the on-disk shape stays a bare JSON array.

### 2. Create the Swordfish-free stack: `SloStack<T> : SloLinkedList<T>`

Create a dedicated stack subclass rather than adding stack members to the shared `SloLinkedList<T>`
(which production Recents already depends on). Top-of-stack == index 0 == `First`. Members to add:

| Member | Implementation | Complexity |
|---|---|---|
| `Push(T)` | `AddFirst(item)` | O(1) |
| `Pop()` | `TakeFirst()`; throw `InvalidOperationException` when empty | O(1) |
| `Peek()` | `First.Value`; throw when empty | O(1) |
| `this[int]` get | node walk from `First` to index `i` | O(n) |
| `Peek(int)` | node walk; throw `IndexOutOfRangeException` on out-of-range | O(n) |
| `Pop(int)` | node walk to `i`, remove node, return value; higher indices shift down | O(n) |
| `TryPeek(out T)` / `TryPeek(out T,int)` | guarded index reads | O(1)/O(n) |
| `TryPop(out T)` / `TryPop(out T,int)` | guarded read+remove | O(1)/O(n) |
| `SerializeAsync()` | async serialize via `Task.Run`/awaited `ism` | trivial |

The subclass re-exposes a typed `ISmartSerializable<SloStack<T>>` plus a `Static` nested class with
file-based `Static.Deserialize(filename, folderpath, askUserOnError:false)`. The base `ism` is
constructed with `this`, so inherited `Serialize`/`SerializeThreadSafe` already serialize the
concrete `SloStack` correctly; only the typed `Deserialize`/`Static.Deserialize` returns need
re-typing. No completion of the four stubbed `SloLinkedList` `ISmartSerializable` members
(`Deserialize<U>(loader)`, `Deserialize<U>(loader, askUserOnError, altLoader)`,
`DeserializeAsync<U>(...)`, `DeserializeObject(json, settings)`) is required, because the MovedMails
path exercises only the already-implemented file-based deserialize path and the JSON round-trip
tests call `JsonConvert.DeserializeObject<...>` directly.

### 3. Re-base ScoCollection subclasses onto the clean collection

- `CtfMap : ScoCollection<CtfMapEntry>` — relies on `FindIndex`, `this[idx]` get, `Add(T)`,
  enumeration, and the AltListLoader constructor.
- `SubjectMapSco : ScoCollection<SubjectMapEntry>` (incl. `AltListLoader`) — relies on `FindIndex`,
  `this[idx]` get, `Add(T)`, `ToList()`, `Serialize()`, `CollectionChanged +=`, and file/AltListLoader
  constructors.

### 4. Re-point direct ScoCollection<T> consumers onto the clean collection

- `TaskMaster\AppGlobals\AppAutoFileObjects.cs`: `Filters` (`ScoCollection<FilterEntry>`) — file
  constructor, `Subscribe(observer)`, `Serialize()`.
- `TaskMaster\AppGlobals\AppToDoObjects.cs`: `PrefixList` (`ScoCollection<IPrefix>`) — file
  constructor, `Count`, `Add(T)`, enumeration.
- `UtilitiesCS\...\OlFolder\OlFolderClassifierGroup.cs`: `_mailInfoCollection`/`LoadStaging`
  (`ScoCollection<MinedMailInfo>`) — file constructor, enumeration.
- Interface return types (in F2 scope; only `IScoCollection`/`IScoCollection2` are reserved for
  F5): `IAppAutoFileObjects.Filters` and `IToDoObjects.PrefixList`/`LoadPrefixList`.

### 5. Migrate ScoStack<IMovedMailInfo> consumers onto SloStack<IMovedMailInfo>

- QuickFiler: `QfcCollectionController.cs`, `QfcDatamodel.cs`, `QfcFormController.cs`,
  `Interfaces\IQfcCollectionController.cs`, `Interfaces\IQfcDatamodel.cs`.
- `TaskMaster\AppGlobals\AppAutoFileObjects.cs` (`MovedMails`, `LoadMovedMails`),
  `UtilitiesCS\...\SortEmail.cs` (`UndoAsync`, push sites), `EmailFiler.cs` (push site),
  `Interfaces\IAppAutoFileObjects.cs`.
- **Construction reconciliation:** replace `new ScoStack<IMovedMailInfo>(filename, folderpath,
  askUserOnError:false)` in `LoadMovedMails()` with the file-based
  `SloStack<IMovedMailInfo>.Static.Deserialize(_defaults.FileName_MovedEmails, pythonStaging,
  askUserOnError:false)`, mirroring the existing Recents migration precedent. Persist via the
  `Serialize()`/`SerializeAsync()` calls already present at the undo/sort sites.

### 6. Delete RecentsList<T> dead code

`RecentsList<T> : ScoCollection<T>` is constructed only in test code and in commented-out blocks
of `AppAutoFileObjects.cs`; the production property `AppAutoFileObjects.RecentsList` is already
`SloLinkedList<string>`. Delete `UtilitiesCS\EmailIntelligence\Recents\RecentsList.cs` and its test
`UtilitiesCS.Test\EmailIntelligence\RecentsList_Tests.cs` rather than migrating them.

### 7. Migrate and add tests

- Migrate `ScoStack_Tests.cs` positional-semantics assertions to new `SloStack<T>` tests.
- Re-point `ManageFiltersControllerTests.cs`, `ConcurrentObservableCollectionSenderTests.cs`,
  `ConcurrentObservableCollectionLockRecursionTests.cs`, `EmailFiler_Tests.cs` /
  `EmailFiler_TestSupport.cs`, and `AppAutoFileObjectsCoverageExpansionTests.cs` to the clean types.
- Add a per-collection JSON round-trip compatibility test for MovedMails, Filters, PrefixList,
  CtfMap, and SubjectMapSco using in-memory fixture strings (no temp files).
- Add unit coverage for every new `SloStack<T>` positional member and `SerializeAsync`, and for
  the clean-collection members.

### 8. Delete legacy types when unreferenced

After all re-points, if a repo-wide grep confirms `ScoCollection<`/`ScoStack<` is no longer
referenced outside F5-reserved interface files, delete `ScoCollection.cs`, `ScoStack.cs`, and their
direct tests (`ScoCollection_Tests.cs`, `ScoStack_Tests.cs`), and update/delete the moot assertion
in `SmartSerializableStatic_Tests.cs`. Deletion is permitted only if re-pointing leaves them
unreferenced.

## Out of Scope / Non-Goals

- Do NOT delete the `UtilitiesSwordfish` project, remove any `ProjectReference`, or touch
  `TaskMaster.sln` (F5).
- Do NOT migrate `IScoCollection`/`IScoCollection2` (F5).
- Do NOT touch `ScoDictionary`/`ScoDictionaryNew` (F1) or `ScoSortedDictionary` (F3).
- No JSON converter or on-disk migration work — research verified none is required.
- No new production dependencies.
- No behavior or UX changes beyond the migration.
- No stub completion of the four `SloLinkedList` `ISmartSerializable` members (the MovedMails path
  does not exercise them).

## Data & State

- **On-disk shape:** all five persisted collections serialize as a bare JSON array with
  `TypeNameHandling.Auto`. Container-only properties (`FilePath`, `FolderPath`, `FileName`,
  `Config`, `Name`) are not persisted. Concrete-element collections (Filters, CtfMap, SubjectMapSco,
  MinedMailInfo) emit no element `$type`; polymorphic-element collections (MovedMails on
  `IMovedMailInfo`, PrefixList on `IPrefix`) emit a per-element `"$type"`.
- **Interchange invariant:** the Sco* and clean replacements are shape-interchangeable because both
  select a `JsonArrayContract`. No element concrete type names are renamed (F2 renames containers,
  not element DTOs), so no `$type` string changes. The live RecentsList migration
  (`ScoCollection<string>` → `SloLinkedList<string>` reading the same payload) is a production
  precedent that the array shapes are interchangeable.
- **Ordering guarantee:** on deserialize, the JSON array replays via `Add` → `AddLast`, so array
  index 0 becomes `First`. With `Push`→`AddFirst` and `Pop()`→`TakeFirst`, top-of-stack == index 0
  == array[0], identical to `ScoStack`. No reversal is required.

## Technical Approach

- **Clean collection base:** `ObservableCollection<T>` supplies `IList<T>` + `CollectionChanged`
  natively; F2 adds the `FindIndex`/`FindIndices`/`Find`/`Exists` helpers, a `Subscribe` adapter
  over `CollectionChanged`, and the serialization surface. This avoids the Swordfish
  `ReaderWriterLockSlim` lock-recursion hazard documented at `AppAutoFileObjects.cs:588-609`. The
  planner should confirm no consumer depends on concurrent multi-writer semantics (current write
  paths run under `Task.Run`).
- **Stack base:** `SloStack<T> : SloLinkedList<T>` isolates the O(n) positional members from the
  Recents-shared `SloLinkedList<T>`, containing the new coverage denominator in one small,
  host-neutral, fully unit-testable type.
- **Undo-loop contract preserved:** both loops use forward index `i`, indexer read `stack[i]`, and
  positional `Pop(i)` that removes-and-returns the element at ordinal `i` without advancing `i` (so
  the element shifting into slot `i` is reprocessed), then `Serialize()`. `SloStack` `this[int]` and
  `Pop(int)` implement ordinal semantics over the linked list (position 0 == `First`).

## Constraints & Risks

- **On-disk JSON compatibility** is the primary risk. Research verified no converter is required,
  but the guardrail (clean collection stays an array-serializing `IList<T>` with no `[JsonObject]`)
  must hold, and each persisted collection requires a round-trip test.
- **Undo regression risk** in QuickFiler and SortEmail if `Pop(int)`/`this[int]` ordinal semantics
  diverge from `ScoStack`. Mitigated by porting the positional-semantics assertions and by
  behavior-preserving contract tests.
- **Coverage risk** on new members. `SloStack` and clean-collection members are new code and must
  meet the new-code coverage bar; the subclass design keeps them concentrated and testable.
- **Unknowns to confirm at implementation time:** the exact assembly-qualified `$type` strings for
  MovedMails (`IMovedMailInfo` concrete) and PrefixList (`IPrefix` concrete) must be taken from the
  actual DTO types when authoring round-trip fixtures; and `AF.RecentsList.AddRecent(...)` (likely
  an extension method) should be confirmed present before relying on the Recents path in tests.

## Implementation Strategy

- Create the clean collection first (gates the four subclass/consumer re-bases), then `SloStack<T>`,
  then re-base subclasses, re-point direct consumers and interfaces, migrate the stack consumers and
  undo loops, delete RecentsList dead code, migrate/add tests, and finally delete the legacy types
  once unreferenced.
- No dependency changes (no packages added or removed).
- No new logging/telemetry; preserve existing logging patterns at the migrated sites.
- No feature flags; the migration is a like-for-like base swap with preserved on-disk shape.

## Acceptance Criteria

- [x] The Swordfish-free clean `ConcurrentObservableCollection<T>` base is created under
      `UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.*`, built on `ObservableCollection<T>`,
      carrying `IList<T>` + `IList`, `FindIndex`/`FindIndices`/`Find`/`Exists`, `CollectionChanged`,
      `Subscribe(IObserver<...>)`, and the ScoCollection serialization surface (file ctors incl.
      AltListLoader/backup, `Serialize`/`SerializeAsync`, `Deserialize` overloads,
      `FilePath`/`FolderPath`/`FileName`, `ToList`/`FromList`, injectable FS/Prompt seams).
- [x] The clean collection serializes as a bare JSON array (no `[JsonObject]`; remains an
      `IEnumerable`/`IList<T>` so Newtonsoft keeps the array contract).
- [x] `CtfMap` and `SubjectMapSco` (incl. `AltListLoader`) are re-based onto the clean collection
      and compile against its member surface (`FindIndex`, indexer, `Add`, `ToList`,
      `CollectionChanged`, AltListLoader ctor).
- [x] The direct `ScoCollection<T>` consumers are re-pointed to the clean collection:
      `AppAutoFileObjects.Filters`, `AppToDoObjects.PrefixList`, and
      `OlFolderClassifierGroup._mailInfoCollection`/`LoadStaging`.
- [x] The interface members `IAppAutoFileObjects.Filters` and `IToDoObjects.PrefixList`/`LoadPrefixList`
      are updated to the clean collection return type (F2 scope; `IScoCollection`/`IScoCollection2`
      untouched).
- [x] `SloStack<T> : SloLinkedList<T>` is created exposing the full positional surface: `this[int]`
      get, `Peek(int)`, `Pop(int)`, `TryPeek`/`TryPop` (front and indexed), `Push`→AddFirst,
      `Pop()`/`Peek()`→TakeFirst/First, with top-of-stack == index 0.
- [x] `SloStack<T>` exposes `SerializeAsync()` and a typed `ISmartSerializable<SloStack<T>>` with
      file-based `Static.Deserialize(filename, folderpath, askUserOnError:false)`.
- [x] All `ScoStack<IMovedMailInfo>` consumers are migrated to `SloStack<IMovedMailInfo>`:
      QuickFiler (`QfcCollectionController`, `QfcDatamodel`, `QfcFormController`,
      `IQfcCollectionController`, `IQfcDatamodel`), `AppAutoFileObjects.MovedMails`/`LoadMovedMails`,
      `SortEmail` (undo + push sites), `EmailFiler` (push site), and `IAppAutoFileObjects.MovedMails`.
- [x] MovedMails construction is reconciled to the file-based `SloStack<IMovedMailInfo>.Static.Deserialize`
      pattern (no reliance on the four stubbed `SloLinkedList` `ISmartSerializable` members).
- [x] A JSON round-trip compatibility test exists and passes for each persisted collection —
      MovedMails, Filters, PrefixList, CtfMap, SubjectMapSco — using in-memory fixtures (no temp
      files), asserting element order/values and `$type` stability.
- [x] `SortEmail.UndoAsync` and `QfcFormController.UndoDialog` undo behavior is preserved with no
      regression (forward index read `stack[i]`, positional `Pop(i)` shift-and-reprocess,
      `Serialize()`); the `SloStack` `Pop(int)`/indexer contract the loops depend on is covered by
      tests.
- [x] `RecentsList<T>` dead code is removed: `RecentsList.cs` and `RecentsList_Tests.cs` are deleted
      (not migrated).
- [x] Legacy `ScoCollection.cs`/`ScoStack.cs` and their direct tests are deleted only after a
      repo-wide grep confirms no first-party reference to `ScoCollection<`/`ScoStack<` remains
      outside F5-reserved interface files.
- [x] Migrated tests compile and pass against the clean types (`ManageFiltersControllerTests`,
      `ConcurrentObservableCollectionSenderTests`, `ConcurrentObservableCollectionLockRecursionTests`,
      `EmailFiler_Tests`/`EmailFiler_TestSupport`, `AppAutoFileObjectsCoverageExpansionTests`).
- [ ] New `SloStack<T>` positional members and `SerializeAsync`, and the new clean-collection
      members, meet the new-code coverage bar (>= 90% for new modules/methods per repo CLAUDE.md;
      line >= 85% / branch >= 75% per repo rules).
- [ ] The full C# toolchain passes in order (csharpier → analyzers → nullable → MSTest) with no
      errors in the final pass.
- [ ] No `UtilitiesSwordfish` project deletion, `ProjectReference` removal, `TaskMaster.sln` edit,
      or F1/F3/F5-reserved type changes are introduced (scope boundary held).

## Definition of Done

- [ ] Acceptance criteria documented and mapped to tests or demos
- [ ] Behavior matches acceptance criteria in all documented environments
- [ ] Tests updated/added (unit/integration as applicable)
- [ ] Edge cases and error handling covered by tests
- [ ] Docs updated (README, docs/features/active/... links)
- [ ] Telemetry/logging added or updated (if applicable)
- [ ] Toolchain pass completed (format → lint → type-check → test)
