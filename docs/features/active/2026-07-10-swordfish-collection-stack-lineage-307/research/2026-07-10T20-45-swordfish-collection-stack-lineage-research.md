# F2 swordfish-collection-stack-lineage (#307) — Research

- **Issue:** #307 (epic swordfish-removal, child F2, wave 0)
- **Worktree/branch:** `agent-a6b32d5b42318fa8a` / `feature/swordfish-collection-stack-lineage` (off `epic/swordfish-removal-integration`)
- **Timestamp:** 2026-07-10T20-45
- **Scope:** research only; no production edits.

---

## Executive Summary

1. **`RecentsList<T>` is dead code.** The generic type `RecentsList<T> : ScoCollection<T>` is constructed only in test code (`UtilitiesCS.Test\EmailIntelligence\RecentsList_Tests.cs`) and in commented-out blocks of `AppAutoFileObjects.cs`. Every production consumer of the *property* `AF.RecentsList` binds to `AppAutoFileObjects.RecentsList`, which is already `SloLinkedList<string>` (`AppAutoFileObjects.cs:207`; interface `IAppAutoFileObjects.cs:21`). **Verdict: delete the type and its test; do NOT migrate it onto the clean collection.**

2. **On-disk JSON compatibility is preserved for all five persisted collections with no converter/migration.** Both worlds serialize as a **bare JSON array** with identical `TypeNameHandling.Auto`. `ScoCollection<T>`/`ScoStack<T>` serialize via `IList<T>` (Newtonsoft `JsonArrayContract`); the clean replacements (`SloLinkedList<T>` via `LinkedList<T>`; an `ObservableCollection<T>`-based clean collection) also serialize via array contracts. Element-level `$type` embedding (for polymorphic element types `IMovedMailInfo`, `IPrefix`) is identical because both sides use `TypeNameHandling.Auto`, and no element concrete type name is being renamed. The already-completed RecentsList migration (`ScoCollection<string>` → `SloLinkedList<string>` reading the same "RecentFolders" payload) is a **live production precedent** that the array shapes are interchangeable.

3. **Both undo loops have identical positional semantics:** forward index `i`, indexer read `stack[i]`, and positional `Pop(i)` that removes-and-returns the element at `i` (the loop does **not** advance `i` after a pop, so the element that shifts into slot `i` is reprocessed). The required stack surface for undo is: `Count`, `this[int]` get, `Pop(int)`, and `Serialize()`.

4. **Recommended stack implementation: a dedicated subclass `SloStack<T> : SloLinkedList<T>`** that adds the positional/stack surface (`Push`, `Pop()`, `Pop(int)`, `Peek()`, `Peek(int)`, `this[int]`, `TryPeek`/`TryPop` front+indexed, `SerializeAsync()`) plus its own `ISmartSerializable<SloStack<T>>` + `Static.Deserialize`. This isolates the new O(n) positional members from the general `SloLinkedList<T>` already used in production by `RecentsList`, containing the new coverage denominator and avoiding regressions in the Recents path.

5. **BLOCKER-CLASS FINDING for the collection lineage:** the "clean `ConcurrentObservableCollection` in `UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.*`" named by the spec **does not exist**. The only `ConcurrentObservableCollection<T>` class in the tree is the Swordfish one (`UtilitiesSwordfish\Collections\ConcurrentObservableCollection.cs:31`); the clean `Concurrent.Observable` namespace contains only `Bag` and `Dictionary`. F2 must **create** the clean collection. A low-risk base already exists in-repo: `ObservableCollectionBatchUpdate<T> : ObservableCollection<T>` (`UtilitiesCS\ReusableTypeClasses\Observable\ObservableCollectionBatchUpdate.cs:11`), which is a Swordfish-free `IList<T>` + `INotifyCollectionChanged`. Building the clean collection on `ObservableCollection<T>` is far smaller than porting the Swordfish `ConcurrentObservableBase` tree.

---

## Q1 — RecentsList lifecycle (definitive)

### Type vs. property distinction
- **Type:** `RecentsList<T> : ScoCollection<T>` — `UtilitiesCS\EmailIntelligence\Recents\RecentsList.cs:11`.
- **Property:** `AF.RecentsList` — declared as `SloLinkedList<string>` in `AppAutoFileObjects.cs:207` and `IAppAutoFileObjects.cs:21`.

### Every reference to the *type* `RecentsList<...>`
- `UtilitiesCS.Test\EmailIntelligence\RecentsList_Tests.cs` — constructs `new RecentsList<string>(...)` at lines 15, 25, 35, 44, 56, 81, 112, 147, and reflects on `RecentsList<T>` at 170-176. **Test-only.**
- `TaskMaster\AppGlobals\AppAutoFileObjects.cs:241-278` — all references to `RecentsList<string>` are inside a commented-out `//` block (fields `_recentsList`, property, `LoadRecentsListAsync`). **Dead/commented.**
- No other source constructs or declares `RecentsList<T>`.

### Every production consumer of the *property* `AF.RecentsList` (binds to `SloLinkedList<string>`)
- `UtilitiesCS\...\SortEmail.cs:188-189, 423-424, 528-535` — `AddOrMoveFirst(...)`, `Serialize()`.
- `UtilitiesCS\...\EmailFiler.cs:421` — `AddOrMoveFirst(...)`.
- `UtilitiesCS\OutlookObjects\Folder\FolderPredictor.cs:219, 681-684` — `.Count`, enumeration (`AddRange`).
- `ToDoModel\...\SortItemsToExistingFolder.cs:160` — `AddRecent(...)`.
- Test mocks setup `SloLinkedList<string>`: `EmailFiler_TestSupport.cs:112`, `FolderPredictorTests.cs:913`, `AppAutoFileObjectsCoverageExpansionTests.cs:182-192`.

`AddOrMoveFirst` is defined on `LockingObservableLinkedList<T>` (`LockingObservableLinkedList.cs:110, 129`), confirming these consumers ride the `SloLinkedList` lineage, not `RecentsList<T>`.

### Verdict
**Delete `RecentsList<T>` (`RecentsList.cs`) and its test (`RecentsList_Tests.cs`) as dead code.** It is fully superseded by `AppAutoFileObjects.RecentsList : SloLinkedList<string>`. No re-base onto the clean collection is required for this type. (Deletion is explicitly permitted by the spec when re-pointing leaves the legacy type unreferenced; here it is already unreferenced in production.)

---

## Q2 — On-disk JSON type-name embedding (per collection)

### Serializer settings in play
- `ScoCollection<T>` uses `TypeNameHandling.Auto` on every path: deserialize `ScoCollection.cs:120, 133`; serialize `ScoCollection.cs:274`. Serialization call is `serializer.Serialize(sw, this)` (`ScoCollection.cs:278`) — no root type argument, so with `Auto` no root `$type` wrapper is emitted; the object serializes as a bare JSON array because the Swordfish base implements `IList<T>` (`ConcurrentObservableCollection.cs:31-37`).
- `SmartSerializable` (the `SloLinkedList` path) also uses `TypeNameHandling.Auto` (`SmartSerializableBase.cs:382-389`, `GetDefaultSettings`). Its `SerializeToStream` passes `instance.GetType()` (`SmartSerializableBase.cs:498-501`); passing the actual type still yields no root `$type` wrapper. `SloLinkedList<T>` serializes as a bare array because it derives from `LinkedList<T>` (via `LockingLinkedList<T> : LinkedList<T>`, `LockingLinkedList.cs:13`), and Newtonsoft's contract resolver selects a `JsonArrayContract` for `IEnumerable` types before it ever considers `ISerializable`.

### Key structural fact
Both the Sco* container and the clean container serialize as a **bare JSON array**; container-only properties (`FilePath`, `FolderPath`, `FileName`, `Config`, `Name`) are **not** persisted (array contracts serialize only elements). Element `$type` appears only where the declared element type is polymorphic and `Auto` fires.

### Per-collection conclusion

| Persisted collection | Declared T | Element `$type` on disk? | Re-base changes on-disk shape? | Migration needed? |
|---|---|---|---|---|
| **MovedMails** — `ScoStack<IMovedMailInfo>` (`AppAutoFileObjects.cs:177`) | interface `IMovedMailInfo` | **Yes** (concrete `MovedMailInfo` differs from `IMovedMailInfo` → `$type` per element) | No — `SloStack`/`SloLinkedList<IMovedMailInfo>` also emits array + per-element `$type` under `Auto` | **None** (element type name unchanged) |
| **Filters** — `ScoCollection<FilterEntry>` (`AppAutoFileObjects.cs:462`) | concrete `FilterEntry` | No (T == concrete) | No — array of `FilterEntry` either way | None |
| **PrefixList** — `ScoCollection<IPrefix>` (`AppToDoObjects.cs:388-390`) | interface `IPrefix` | **Yes** (`$type` per element) | No | None |
| **CtfMap** — `ScoCollection<CtfMapEntry>` (`CtfMap.cs:10`) | concrete `CtfMapEntry` | No | No | None |
| **SubjectMapSco** — `ScoCollection<SubjectMapEntry>` (`SubjectMapSco.cs:24`) | concrete `SubjectMapEntry` | No | No | None |

(Also `OlFolderClassifierGroup._mailInfoCollection : ScoCollection<MinedMailInfo>`, `OlFolderClassifierGroup.cs:120-140` — concrete element, array, no `$type`, no migration.)

### Verdict
**No converter or on-disk migration is required for any of the five collections**, provided (a) element concrete type names are not renamed (they are not — F2 renames containers, not element DTOs), and (b) the clean collection replacements serialize via array contracts (confirmed for `SloLinkedList<T>` and for any `ObservableCollection<T>`/`IList<T>`-based clean collection). The one guardrail for the planner: the new clean collection type must **not** carry `[JsonObject]` or otherwise force object serialization, and must remain an `IEnumerable`/`IList<T>` so Newtonsoft keeps the array contract.

### Representative on-disk payload shapes for round-trip tests
- **Concrete-element collections** (Filters, CtfMap, SubjectMapSco, MinedMailInfo): a JSON array of element objects, no `$type`, e.g. `[ { "EmailFolder": "...", "ConversationID": "...", "EmailCount": 3 }, ... ]` (indented).
- **Polymorphic-element collections** (MovedMails, PrefixList): a JSON array where each element carries `"$type": "<Namespace>.<Concrete>, <Assembly>"` alongside its members, e.g. `[ { "$type": "UtilitiesCS.MovedMailInfo, UtilitiesCS", ... }, ... ]`.

A round-trip test per collection should: (1) write a fixture array string of the above shape (in-memory string, **no temp file** per repo policy), (2) `JsonConvert.DeserializeObject<CleanType>(fixture, settingsWithAuto)`, (3) assert element order and values, (4) re-serialize and assert the array shape/`$type` presence is stable. Existing tests already prove array round-trip for the legacy types: `ScoStack_Tests.cs:159` and `ScoCollection_Tests.cs:226` deserialize bare arrays via `JsonConvert`.

---

## Q3 — Undo-loop semantics (exact)

### `SortEmail.UndoAsync` — `SortEmail.cs:552-606`
```
i = 0
while (i < movedStack.Count && repeatResponse == Yes):
    message = movedStack[i].UndoMoveMessage(globals.Ol.App)   // indexer GET at i
    if message is null: i++                                    // skip, keep item, advance
    else:
        if user says Yes:
            helper = await MailItemHelper.FromMailItemAsync(movedStack[i].MailItem, ...)  // indexer GET at i
            ... UnTrain ...
            movedStack[i].UndoMove()                            // indexer GET at i
            movedStack.Pop(i)                                   // positional remove-and-return at i; i NOT advanced
        else: i++
        repeatResponse = MessageBox(...)
movedStack.Serialize()                                          // final persist
```
- Direction: **forward** (`i` starts 0, increments only on skip/decline).
- Removal: **positional `Pop(i)`** (`SortEmail.cs:587`), not `Pop()`. After a pop, `i` is unchanged, so the element previously at `i+1` (now at `i`) is processed next iteration.
- Calls: `Count` (561), `this[i]` get (563, 578, 586), `Pop(int)` (587), `Serialize()` (605). Method is `[ExcludeFromCodeCoverage]` (`SortEmail.cs:552`).
- Call site: `RibbonController.cs:230` → `SortEmail.UndoAsync(Globals.AF.MovedMails, Globals)`.

### `QfcFormController.UndoDialog` — `QfcFormController.Actions.cs:204-250`
```
i = 0
while (i < _movedItems.Count && repeatResponse == Yes):
    message = _movedItems[i].UndoMoveMessage(olApp)             // indexer GET at i
    if message is null: i++
    else:
        if user says Yes: _undoQueue.Add(_movedItems.Pop(i))    // positional pop at i; result enqueued; i NOT advanced
        else: i++
        repeatResponse = MessageBox(...)
_movedItems.Serialize()                                         // final persist (line 250)
```
- Same forward-`i` + positional-`Pop(i)` (`QfcFormController.Actions.cs:232`) semantics.
- Calls: `Count` (216), `this[i]` get (218), `Pop(int)` (232), `Serialize()` (250).
- `_movedItems` bound at `QfcFormController.cs:48` (`_movedItems = _globals.AF.MovedMails`), declared `QfcFormController.cs:85`, nulled in disposal `QfcFormController.SetupDisposal.cs:223`.

### Ordering guarantee the undo depends on
Index 0 is the **top** of the stack. `Pop(i)` must remove exactly the element at ordinal position `i` and shift higher indices down by one. The new stack's `this[int]` and `Pop(int)` must implement ordinal semantics over the linked list (position 0 == `First`). `Push`→front, `Pop()`/`Peek()`→front is consistent with the legacy `ScoStack` (`Push = Insert(0)`, `Pop()/Peek() = this[0]`; `ScoStack.cs:28-64`).

### `MovedMails.SerializeAsync()` / `Serialize()` call sites
- `SortEmail.cs:200`, `SortEmail.cs:435` — `appGlobals.AF.MovedMails.SerializeAsync()` (awaited inside a `Task.WhenAll`-style block).
- `SortEmail.cs:538` — `appGlobals.AF.MovedMails.Serialize()`.
- `SortEmail.cs:605`, `QfcFormController.Actions.cs:250` — `Serialize()` on the passed stack instance.
- Pushes: `SortEmail.cs:1302`, `EmailFiler.cs:183` — `AF.MovedMails.Push(info)`.

### `new ScoStack<...>(filename, folderpath, askUserOnError)` construction sites
- `AppAutoFileObjects.cs:186-190` — `new ScoStack<IMovedMailInfo>(filename: _defaults.FileName_MovedEmails, folderpath: pythonStaging, askUserOnError: false)` inside `LoadMovedMails()`; `_movedMails` memoized via `Initialized(...)` (`AppAutoFileObjects.cs:179`).
- Empty ctor `new ScoStack<IMovedMailInfo>()` used in tests (`EmailFiler_Tests.cs:360, 431, 474`) and in the classifier path.

---

## Q4 — Collection re-base gap (member-by-member)

### Surface `ScoCollection<T>` adds on top of the Swordfish base
Constructors (`ScoCollection.cs:66-107`): `()`, `(IEnumerable<T>)`, `(byte[])`, `(fileName, folderPath)`, `(fileName, folderPath, askUserOnError)`, `(fileName, folderPath, AltListLoader, backupFilepath, askUserOnError)`.
- `public delegate IList<T> AltListLoader(string filePath)` (`ScoCollection.cs:109`).
- Serialization surface: `FilePath`/`FolderPath`/`FileName` (211-227), `Serialize()`/`Serialize(path)` (229-239), `SerializeAsync()`/`SerializeAsync(path)` (241-260), `SerializeThreadSafe` (264-293), `Deserialize()` overloads incl. AltListLoader/backup (309-460), `ToList()`/`FromList(IList<T>)` (186-203).
- Static injectable seams `FileSystem`/`Prompt` (`ScoCollection.cs:60-62`) for testability.

### Surface inherited from the **Swordfish** base that consumers actually rely on
(These must exist on the clean collection or be added.)
- `IList<T>`: `this[int]` get/set, `Add`, `Insert`, `RemoveAt`, `Remove`, `Contains`, `IndexOf`, `Count`, `CopyTo`, `IsReadOnly`, enumeration (`ConcurrentObservableCollection.cs:62-122`).
- `IList` (non-generic) explicit members (`ConcurrentObservableCollection.cs:153-239`).
- **List<T>-style helpers "by Dan Moisan"**: `FindIndex` (several overloads), `FindIndices`, `Find(Predicate<T>)`, `Exists` (`ConcurrentObservableCollection.cs:245-352`). **CtfMap and SubjectMapSco depend on these.**
- Observable surface: `event NotifyCollectionChangedEventHandler CollectionChanged` (`ConcurrentObservableBase.cs:613`) and `IDisposable Subscribe(IObserver<NotifyCollectionChangedEventArgs>)` (`ConcurrentObservableBase.cs:557`).

### Per-consumer reliance

| Consumer | Members used | Evidence |
|---|---|---|
| **CtfMap** (`: ScoCollection<CtfMapEntry>`) | `FindIndex`, `this[idx]` get, `Add(T)`, enumeration (`Where`), AltListLoader ctor `(filename, folderpath, backupLoader, backupFilepath, askUserOnError)` | `CtfMap.cs:18-30, 38-59, 64-72` |
| **SubjectMapSco** (`: ScoCollection<SubjectMapEntry>`) | `FindIndex`, `this[idx]` get, `Add(T)`, `ToList()`, `Serialize()`, `CollectionChanged +=`, ctors incl. AltListLoader | `SubjectMapSco.cs:71-82, 106-160, 181-195`; `AppAutoFileObjects.cs:503-511` |
| **AppAutoFileObjects.Filters** (`ScoCollection<FilterEntry>`) | `(fileName, folderPath)` ctor, `Subscribe(observer)`, `Serialize()`, `.Count`(tests) | `AppAutoFileObjects.cs:465-495`; observer at `:479` |
| **AppToDoObjects.PrefixList** (`ScoCollection<IPrefix>`) | `(fileName, folderPath)` ctor, `.Count`, `Add(T)`, enumeration | `AppToDoObjects.cs:388-407` |
| **OlFolderClassifierGroup._mailInfoCollection / LoadStaging** (`ScoCollection<MinedMailInfo>`) | `(filename, folderpath)` ctor; returned & enumerated | `OlFolderClassifierGroup.cs:120-140` |
| **IAppAutoFileObjects.Filters** | return-type `ScoCollection<FilterEntry>` | `IAppAutoFileObjects.cs:32` |
| **IToDoObjects.PrefixList / LoadPrefixList** | return-type `ScoCollection<IPrefix>` | `IToDoObjects.cs:25-26` |

### Clean-collection requirement (assembled surface)
The clean collection type (to be created under `UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.*`) must provide, at minimum: `IList<T>` + `IList`, `FindIndex`/`FindIndices`/`Find`/`Exists`, `event NotifyCollectionChangedEventHandler CollectionChanged`, `IDisposable Subscribe(IObserver<NotifyCollectionChangedEventArgs>)`, plus the ScoCollection serialization surface (file ctors incl. the AltListLoader/backup ctor, `Serialize`/`SerializeAsync`, `Deserialize` overloads, `FilePath`/`FolderPath`/`FileName`, `ToList`/`FromList`).

**Base-class recommendation:** build on `System.Collections.ObjectModel.ObservableCollection<T>` (already used in-repo via `ObservableCollectionBatchUpdate<T>`, `ObservableCollectionBatchUpdate.cs:11`). `ObservableCollection<T>` supplies `IList<T>` + `CollectionChanged` natively; F2 adds the List<T> helpers, a `Subscribe` adapter over `CollectionChanged`, and the serialization surface. This avoids porting the Swordfish `ConcurrentObservableBase`/`ImmutableCollection`/`DoubleLinkList*` tree (`UtilitiesSwordfish\Collections\*`). Note the Swordfish base's `ReaderWriterLockSlim` recursion hazard is documented in `AppAutoFileObjects.cs:588-609` and `ConcurrentObservableCollectionLockRecursionTests.cs`; an `ObservableCollection<T>`-based clean type does not reproduce that lock model (planner should confirm thread-safety expectations for the write paths, which today run under `Task.Run` load).

---

## Q5 — Stack re-base gap on SloLinkedList

### `ScoStack<T>` surface (`ScoStack.cs`)
`Peek()` (28), `Peek(int)` (35), `Pop()` (44), `Pop(int)` (53), `Push(T)`→`Insert(0)` (64), `ToArray()` (66, **known infinite-recursion bug**, see `ScoStack_Tests.cs:351`), `ToArray(bool)` (68), `ToList(bool)` (80), `TryPeek(out T)` (92), `TryPeek(out T,int)` (106), `TryPop(out T)` (120), `TryPop(out T,int)` (135). Constructors (11-24): `()`, `(List<T>)`, `(IEnumerable<T>)`, `(filename, folderpath)`, `(filename, folderpath, askUserOnError)`. Inherits `Serialize`/`SerializeAsync`/indexer/`Count`/`RemoveAt` from `ScoCollection`.

**Production usage is narrower than the full surface:** `Push`, `Pop(int)`, `this[int]` get, `Count`, `Serialize()`, `SerializeAsync()`, and the `(filename, folderpath, askUserOnError)` + `()` ctors. `Peek`/`TryPeek`/`TryPop`/`ToArray`/`ToList(bool)` appear only in tests (`EmailFiler_Tests.cs:465`, `ScoStack_Tests.cs`). The AC nonetheless requires the full positional surface.

### Current `SloLinkedList<T>` / `LockingObservableLinkedList<T>` surface
- Add/remove/move: `AddFirst`, `AddLast`, `AddOrMoveFirst`, `AddBefore/After`, `Clear`, `Remove(...)`, `RemoveFirst/Last`, `Find` (`LockingObservableLinkedList.cs:104-269`).
- Take: `TakeFirst()`, `TakeFirst(int)`, `TryTakeFirst(int)`, `TakeLast()/…` (`LockingObservableLinkedList.cs:271-365`).
- `First`/`Last` return **nodes** (`LockingObservableLinkedListNode<T>`), not values (`:83-102`). `Count` from `LockingLinkedList.cs:41`.
- Serialize: `Serialize()`, `Serialize(string)`, `SerializeThreadSafe(string)` via `ism` (`SloLinkedList.cs:46-50`). **No `SerializeAsync`.**
- Deserialize: instance `Deserialize(fileName, folderPath[, askUserOnError[, settings]])` and `Static.Deserialize(...)` / `Static.DeserializeAsync(...)` all implemented (`SloLinkedList.cs:52-66, 138-165`).
- **Stubbed `NotImplementedException`** members (`SloLinkedList.cs:78-108`): `ISmartSerializable<>.Deserialize<U>(loader)`, `Deserialize<U>(loader, askUserOnError, altLoader)`, `DeserializeAsync<U>(config, askUserOnError, altLoader)`, `DeserializeObject(json, settings)`.
- **Missing entirely:** `this[int]` indexer, `Peek()/Peek(int)`, `Pop()/Pop(int)`, `Push`, `TryPeek`/`TryPop`.

### Gap list — members to ADD for the stack

| Member | Implementation | Complexity |
|---|---|---|
| `Push(T)` | `AddFirst(item)` (top = `First`) | O(1) |
| `Pop()` | `TakeFirst()`; throw `InvalidOperationException` when empty (match `ScoStack.cs:44-51`) | O(1) |
| `Peek()` | `First.Value`; throw when empty | O(1) |
| `this[int]` get | node walk from `First` to index `i` | O(n) |
| `Peek(int)` | node walk; throw `IndexOutOfRangeException` (match `ScoStack.cs:35-42`) | O(n) |
| `Pop(int)` | node walk to `i`, `Remove(node)`, return value | O(n) |
| `TryPeek(out T)` / `TryPeek(out T,int)` | guarded index reads (match `ScoStack.cs:92-118`) | O(1)/O(n) |
| `TryPop(out T)` / `TryPop(out T,int)` | guarded read+remove | O(1)/O(n) |
| `SerializeAsync()` | `await Task.Run(() => Serialize())` or await ism | trivial |

**Ordering/round-trip correctness:** on deserialize, the JSON array is replayed via `ICollection<T>.Add` → `AddLast`, so array index 0 becomes `First`. Because `Push`→`AddFirst` and `Pop()`→`TakeFirst`, top-of-stack == index 0 == array[0], identical to `ScoStack` (top == index 0). No reversal needed. This is the same mechanism already proven by the live RecentsList (`SloLinkedList<string>`) migration.

### Stubbed `ISmartSerializable` members: which must be completed?
If `LoadMovedMails` migrates to `SloStack<IMovedMailInfo>.Static.Deserialize(filename, folderpath, askUserOnError:false)`, it exercises only the **already-implemented** file-based deserialize path (`SloLinkedList.cs:141-145` → `SmartSerializableBase.Deserialize<T>`); **none of the four stubbed members are hit.** The JSON round-trip tests can call `JsonConvert.DeserializeObject<...>` directly (as existing Sco tests do), also avoiding `DeserializeObject(json, settings)`. **Conclusion: no stub completion is strictly required for the MovedMails path.** If the planner chooses to route through the config/altLoader overloads, then `DeserializeObject(json, settings)` and the `altLoader` overloads would need completing — recommend NOT taking that dependency and keeping the file-based path.

### Construction reconciliation
- Legacy: `new ScoStack<IMovedMailInfo>(filename, folderpath, askUserOnError:false)` (`AppAutoFileObjects.cs:186-190`).
- Migration template (already in production for Recents): `RecentsList = await SloLinkedList<string>.Static.DeserializeAsync(config, true)` (`AppAutoFileObjects.cs:215-218`), with change-persistence wired via `CollectionChanged += SmartSerializable_CollectionChanged` (`:219, 229-236`).
- Recommended for MovedMails: `_movedMails = SloStack<IMovedMailInfo>.Static.Deserialize(_defaults.FileName_MovedEmails, pythonStaging, askUserOnError:false)` (synchronous variant exists at `SloLinkedList.cs:141-145`; async variant analogous). Persist explicitly via the existing `Serialize()`/`SerializeAsync()` calls already present at the undo/sort sites, or wire a `CollectionChanged` handler mirroring Recents.

### Recommendation: subclass vs. direct
**Recommend a dedicated `SloStack<T> : SloLinkedList<T>`** (lower risk):
- Isolates the O(n) positional/stack surface from the general-purpose `SloLinkedList<T>` already relied on by production Recents, so Recents behavior and its coverage denominator are untouched.
- Keeps stack semantics as a distinct domain concept (per repo design principles §2.1).
- Cost: the subclass must re-expose `ISmartSerializable<SloStack<T>>` + a `Static` nested class, mirroring `SloLinkedList.cs:37-166` re-typed to `SloStack<T>` (the base `ism` is constructed with `this`, so inherited `Serialize`/`SerializeThreadSafe` already serialize the concrete `SloStack` correctly via `instance.GetType()`; only the typed `Deserialize`/`Static.Deserialize` returns need re-typing).
- **Coverage implication:** all new positional members and `SerializeAsync` are new code and must meet the new-code coverage bar (repo CLAUDE.md: ">= 90% for new modules/methods"; `.claude/rules/general-unit-test.md` states >= 85% line / >= 75% branch). A subclass concentrates these in one small, fully unit-testable type (no COM/host dependency), so the bar is readily met.

The alternative (adding stack members directly to `SloLinkedList<T>`) avoids the `ISmartSerializable` re-typing but pollutes the Recents-shared type with `Push`/`Pop`/indexer it does not need and broadens that type's coverage surface — higher regression risk. Not recommended.

---

## Q6 — Existing test inventory

### Direct legacy-type tests (deletable within F2 once the legacy types are unreferenced)
- `UtilitiesCS.Test\ReusableTypeClasses\ScoCollection_Tests.cs` — exhaustive `ScoCollection<T>` unit tests incl. injectable `IScoCollectionFileSystem`/`IScoCollectionPrompt` seams and array round-trip (`:226`). Delete with `ScoCollection.cs` if removed; otherwise migrate representative round-trip coverage to the clean collection.
- `UtilitiesCS.Test\ReusableTypeClasses\ScoStack_Tests.cs` — `ScoStack<T>` unit tests incl. array round-trip (`:159`), file ctor with invalid path (`:206`), and a documented `ToArray()` recursion bug (`:351`). Delete with `ScoStack.cs`; port the positional-semantics assertions to the new `SloStack<T>` tests.
- `UtilitiesCS.Test\EmailIntelligence\RecentsList_Tests.cs` — tests the dead `RecentsList<T>`. **Delete with the type (Q1).**
- `UtilitiesCS.Test\ReusableTypeClasses\SmartSerializableStatic_Tests.cs:38-41` — asserts `ScoCollection<int>` does NOT implement `ISmartSerializable<>`. Becomes moot if `ScoCollection` is removed; update or delete.

### Tests that must be migrated/re-pointed to the clean types
- `TaskVisualization.Test\ManageFiltersControllerTests.cs:22-123` — constructs real `new ScoCollection<FilterEntry>()` as the filter set. Re-point to the clean collection type.
- `UtilitiesCS.Test\...\Concurrent\Observable\Collection\ConcurrentObservableCollectionSenderTests.cs` & `ConcurrentObservableCollectionLockRecursionTests.cs` — currently regression tests against the **Swordfish** `ConcurrentObservableCollection` (`using Swordfish.NET.Collections`). Re-point to the clean collection (verify the sender-identity and any lock behavior on the new base).
- `UtilitiesCS.Test\EmailIntelligence\EmailFiler_Tests.cs` (`:360, 431, 465-474`) and `EmailFiler_TestSupport.cs:104-113` — use `ScoStack<IMovedMailInfo>` (incl. `Peek`). Migrate to `SloStack<IMovedMailInfo>`.
- `TaskMaster.Test\AppGlobals\AppAutoFileObjectsCoverageExpansionTests.cs:100-193` — exercises `MovedMails` and `RecentsList` load paths; update for the new `MovedMails` type/loader.

### New tests to add (per repo test-location policy — mirror source tree under the test project)
- **`SloStack<T>` positional surface** — `this[int]`, `Peek(int)`, `Pop(int)`, `TryPeek`/`TryPop` (front + indexed), `Push`/`Pop()`/`Peek()`, `SerializeAsync`, file-based deserialize. Location: `UtilitiesCS.Test\ReusableTypeClasses\SerializableNew\Concurrent\Observable\SloStack_Tests.cs` (mirroring `UtilitiesCS\ReusableTypeClasses\SerializableNew\Concurrent\Observable\`).
- **Per-collection JSON round-trip** (MovedMails, Filters, PrefixList, CtfMap, SubjectMapSco) — in-memory fixture strings (no temp files), assert element order/values and `$type` stability. Co-locate near each type's test folder.
- **Undo regression** — deterministic tests for `SortEmail.UndoAsync` and `QfcFormController.UndoDialog` positional semantics (forward `i`, `stack[i]` read, `Pop(i)` shift-and-reprocess). Note `UndoAsync` is `[ExcludeFromCodeCoverage]`; a focused test can still assert the `SloStack` `Pop(int)`/indexer contract that the loops depend on, which is the behavior-preserving guarantee.

---

## Planning implications (for atomic-planner / prd-feature)

1. **Create the clean collection first (new work, not a rename).** The spec's target type does not exist. Build `ConcurrentObservableCollection<T>` (name TBD to avoid clashing with the Swordfish namespace during coexistence) under `UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Collection`, based on `ObservableCollection<T>`, carrying: `IList<T>`/`IList`, `FindIndex`/`FindIndices`/`Find`/`Exists`, `CollectionChanged`, `Subscribe(IObserver<...>)`, and the ScoCollection serialization surface (file ctors incl. AltListLoader/backup, `Serialize`/`SerializeAsync`, `Deserialize` overloads, `FilePath`/`FolderPath`/`FileName`, `ToList`/`FromList`, injectable FS/Prompt seams). This is the largest single work item and gates the four subclass/consumer re-bases.
2. **Re-base subclasses** CtfMap, SubjectMapSco onto the clean collection (they need `FindIndex` + indexer + AltListLoader ctor). Re-point direct consumers Filters (`AppAutoFileObjects`), PrefixList (`AppToDoObjects`), `_mailInfoCollection`/`LoadStaging` (`OlFolderClassifierGroup`), and update return types on `IAppAutoFileObjects.Filters` and `IToDoObjects.PrefixList`/`LoadPrefixList`. (These interface edits are in F2 scope; only `IScoCollection`/`IScoCollection2` are reserved for F5.)
3. **Delete `RecentsList<T>` + its test** as dead code (Q1).
4. **Create `SloStack<T> : SloLinkedList<T>`** with the positional/stack surface + `SerializeAsync` + typed `ISmartSerializable`/`Static.Deserialize`; migrate `MovedMails` (property/loader in `AppAutoFileObjects`), `QfcDatamodel.MovedItems`, `QfcCollectionController.MoveEmailsAsync`, `QfcFormController._movedItems`, `IQfcDatamodel`/`IQfcCollectionController`/`IAppAutoFileObjects.MovedMails`, and the two undo loops + push sites (`SortEmail`, `EmailFiler`) from `ScoStack<IMovedMailInfo>` to `SloStack<IMovedMailInfo>`.
5. **No JSON converter/migration work** for any of the five collections (Q2) — but add the guardrail that the clean collection stays an array-serializing `IList<T>` with no `[JsonObject]`.
6. **Legacy `ScoCollection.cs`/`ScoStack.cs` deletion** is permitted only after all re-points leave them unreferenced; verify with a final repo-wide grep for `ScoCollection<`/`ScoStack<` outside F5-reserved interface files.
7. **Coverage:** all new `SloStack` and clean-collection members are host-neutral and unit-testable; they must meet the new-code coverage bar. Round-trip and undo-contract tests use in-memory fixtures only (temp files prohibited).

### Unknowns / cannot be answered from code alone
- **Exact fully-qualified `$type` strings** written into existing on-disk MovedMails/PrefixList files (assembly-qualified name of `MovedMailInfo`/`Prefix`) cannot be read without a sample production file; the round-trip test fixtures should be authored from the concrete DTO types' actual assembly-qualified names at implementation time (confirm `IMovedMailInfo` concrete type and `IPrefix` concrete type in the DTO source).
- **Thread-safety expectations** of the clean collection vs. the Swordfish `ReaderWriterLockSlim` model are a design decision; current write paths run under `Task.Run`, and the documented lock-recursion hazard (`AppAutoFileObjects.cs:588-609`) argues for the simpler `ObservableCollection<T>` base, but the planner should confirm no consumer depends on concurrent multi-writer semantics.
- **`AF.RecentsList.AddRecent(...)`** (`SortItemsToExistingFolder.cs:160`) resolves against the `SloLinkedList<string>` property; `AddRecent` was not located as a member of `SloLinkedList`/`LockingObservableLinkedList` in this pass (likely an extension method). It is outside F2's collection/stack re-base but should be confirmed present before relying on the Recents path in tests.
