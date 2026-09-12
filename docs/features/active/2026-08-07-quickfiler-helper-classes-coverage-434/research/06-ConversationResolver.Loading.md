# F4 per-file research — `QuickFiler/Helper Classes/ConversationResolver.Loading.cs`

Timestamp: 2026-08-07T22-40

Cluster: CONVERSATION-RESOLUTION (artifacts 05–08). Cross-cutting facts are established in
`research/00-cluster-overview.md`. The sibling partial is analysed in `05-ConversationResolver.md`;
seams S1–S2 defined there are referenced here rather than restated.

Upstream contract: child F1 owns the per-file line-coverage harness and the ratified exemption ledger
at `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`. Neither exists on disk yet.
Numeric per-file coverage is captured at execution time via F1's harness; figures below are
read-derived estimates. `coverage.config` is a shared file this child must not modify.

---

## 1. File facts

| Fact | Value | Evidence |
| --- | --- | --- |
| Path | `QuickFiler/Helper Classes/ConversationResolver.Loading.cs` | — |
| Line count | 329 (last line `}` at `:329`) | `ConversationResolver.Loading.cs:329` |
| Compiled | yes | `QuickFiler/QuickFiler.csproj:344` — `<Compile Include="Helper Classes\ConversationResolver.Loading.cs" />` |
| `[ExcludeFromCodeCoverage]` | **absent** | grep across `QuickFiler/Helper Classes/` returns no match |
| Type declared | second partial of `public partial class ConversationResolver` (`:15`) — no base list, no new interfaces | `:15` |
| Sibling partial | `ConversationResolver.cs:30` (artifact 05) declares the base list `INotifyPropertyChanged, IConversationResolver` | `:30` |
| 500-line limit | 329 / 500 — 171 lines of headroom | — |

This file carries the **entire** state machine of the type: all four lazy `Pair<>` properties, all six
loaders, the `INotifyPropertyChanged` implementation, and the `async void` event handler. The sibling
partial carries construction, static factories, and plain properties.

---

## 2. Member inventory (coverage denominator for THIS file)

| # | Member | Signature | Lines | Decision points |
| --- | --- | --- | --- | --- |
| 1 | field | `private Pair<List<MailItemHelper>> _convInfoFields;` | `:19` | 0 |
| 2 | property | `public Pair<List<MailItemHelper>> ConversationInfo { get; set; }` | `:20-35` | getter 2 (`Initializer.GetOrLoad(ref, loader, callback, strict:false, _mailItem)` → `UtilitiesCS/HelperClasses/Initializer.cs:142-158`: dependency-check arm + already-initialized arm); setter 0 (always notifies, `:33`) |
| 3 | method | `internal Pair<List<MailItemHelper>> LoadConversationInfo()` | `:37-73` | 2 (`Count.Expanded <= 0` guard `:39`) + 1 projection loop `:59-63` + 1 filter predicate `:65-67` |
| 4 | method | `public async Task<Pair<List<MailItemHelper>>> LoadConversationInfoAsync(CancellationToken token, bool backgroundLoad)` | `:75-154` | 12: cancellation guard `:80`; unused-local ternary `:83-85`; per-row `if (entryId == MailHelper.EntryId)`/`else` `:92-105`; `if (convInfoExpanded?.Count > 0)`/`else` `:112-123`; nested `if (idx > -1)` `:115`; `if (UpdateUI is not null)` `:140`; second cancellation guard `:142`. `await` continuations: `:108` `Task.WhenAll`, `:150` dispatcher |
| 5 | field | `private Pair<IList<MailItem>> _conversationItems;` | `:160` | 0 |
| 6 | property | `public Pair<IList<MailItem>> ConversationItems { get; set; }` | `:161-176` | getter 2 (same `GetOrLoad` overload); setter 0 (always notifies `:174`) |
| 7 | method | `internal Pair<IList<MailItem>> LoadConversationItems()` | `:178-183` | 0 + 2 projection loops `:180-181` |
| 8 | method | `public async Task LoadConversationItemsAsync(CancellationToken token, bool backgroundLoad)` | `:185-195` | 2: cancellation guard `:187`; unused-local ternary `:190-192`. 1 `await` continuation `:194` (`Task.Run`) |
| 9 | field | `private Pair<DataFrame> _df;` | `:201` | 0 |
| 10 | property | `public Pair<DataFrame> Df { get; set; }` | `:202-206` | getter 2 (`GetOrLoad(ref, LoadDf, DfNotifyIfNotNull, false, _mailItem)`); setter 0 (`Initializer.SetAndSave` → always notifies) |
| 11 | method | `internal Pair<DataFrame> LoadDf()` | `:208-221` | 0 explicit; two `(Folder)_mailItem.Parent` casts `:213`, `:216` (each an implicit `InvalidCastException`/`NullReferenceException` path) |
| 12 | method | `internal void DfNotifyIfNotNull(Pair<DataFrame> df)` | `:223-229` | 4 (`df.SameFolder is not null && df.Expanded is not null`) |
| 13 | method | `public async Task LoadDfAsync(CancellationToken token, bool backgroundLoad)` | `:231-258` | 8: cancellation guard `:233`; `_mailItem.ThrowIfNull()` `:234`; `_mailItem.Parent as Folder` `:243`; `parent?.Name ?? string.Empty` `:244` (2); `folderName.IsNullOrEmpty() ? … : …` `:248-250` (2). 1 `await` continuation `:242`. **`backgroundLoad` is never read** |
| 14 | field initializer | `private Pair<int> _count = new Pair<int>(-1, -1);` | `:263` | 0 (documented sentinel, `:260-262`) |
| 15 | property | `public Pair<int> Count { get; internal set; }` | `:265-271` | getter 2 (`GetOrLoad(ref _count, static v => v.Expanded >= 0, LoadCount)` → `Initializer.cs:178-189`) |
| 16 | method | `internal Pair<int> LoadCount()` | `:273-286` | 4 (`df.SameFolder is not null` `:277`; `df.Expanded is not null` `:281`) |
| 17 | method | `protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")` | `:292-300` | 2 (`PropertyChanged is not null` `:296`) |
| 18 | event | `public event PropertyChangedEventHandler PropertyChanged;` | `:302` | compiler-generated `add`/`remove` accessors (2 sequence-point pairs) |
| 19 | method | `public async void Handler_PropertyChanged(object sender, PropertyChangedEventArgs e)` | `:304-325` | 6: `if (e.PropertyName == nameof(Df))` `:306`; `else if (e.PropertyName == nameof(UpdateUI))` `:316`; `if (FullyLoaded)` `:318`; `catch (OperationCanceledException)` `:314`. 2 `await` continuations `:311`, `:320` |

Estimated executable-sequence-point denominator: **≈ 115–125**. F1's harness is authoritative.

**Dead code observed (report only, optional cleanup).** `TaskCreationOptions options` is assigned and
never read at `:83-85` and at `:190-192`; the `backgroundLoad` parameter of `LoadDfAsync` (`:231`) is
never read. Removing the two locals would be behaviour-neutral and would shrink the denominator by
~4 sequence points. The `backgroundLoad` **parameter** must NOT be removed — it is on
`IConversationResolver:31` and is supplied by sibling-owned call sites (§8).

---

## 3. Existing test inventory

### 3.1 `QuickFiler.Test/Helper Classes/ConversationResolverTests.cs` (F4-owned)

| Test method | Lines | Members of THIS file exercised |
| --- | --- | --- |
| `LoadConversationInfo_WhenCountExpandedIsZero_ReturnsSingleItemFallbackContainingMailHelper` | `:71-88` | #15 setter, #3 **fallback arm only** (`:39-56`) |
| `ConversationInfoGetter_WhenCountExpandedIsZero_ReturnsSingleItemFallback` | `:96-110` | #2 getter (load arm) → #3 fallback arm → callback `:26` → #17 (`PropertyChanged is null` arm) |
| `Count_WhenZeroCountIsSetViaInternalSetter_SubsequentGetDoesNotInvokeLoadCount` | `:128-148` | #15 getter **already-initialized arm**, #15 internal setter |
| `Count_WhenNotYetInitialized_AttemptsToLoadCount` | `:159-181` | #15 getter **load arm** → #16 first line `var df = Df;` → #10 getter load arm → #11 `LoadDf` up to the throwing cast at `:213` |
| `ConversationInfo_WhenNotSetAndCountIsZero_ReturnsFallbackWithoutThrowing` | `:212-227` | #2 getter, #3 fallback arm |
| `ConversationInfo_WhenSetBeforeAccessWithCountAtZero_ReturnsCachedValueWithoutThrowing` | `:238-266` | #2 setter (`:31-34`) + #17 null-subscriber arm; #2 getter cached arm |
| `LoadDfAsync_ConsumesConversationSnapshotsWithoutRepeatedUiPublishes` | `:273-312` | #13 full positive path (`:233-257`) with a non-empty folder name (ternary **false** arm `:250`); #10 setter; #17; asserts `_uiPublishCount == 0` |
| `LoadAsync_WithPreloadedHelperAndLoadAllTrue_ReusesHelperForSingleItemConversation` | `:314-349` | #4 (`entryId == MailHelper.EntryId` **true** arm `:92-95`; `convInfoExpanded?.Count > 0` **true** arm; `idx > -1` **true** arm; `UpdateUI is not null` **false** arm); #2 setter; #8; #7; #6 getter; #18 `add` |
| `LoadAsync_WithMailItemAndLoadAllTrue_LoadsConversationInfoAndItems` | `:351-377` | same members as the row above |
| `LoadAsync_WithPreloadedHelperAndLoadAllFalse_ReturnsResolverWithStagedDataFrame` | `:379-412` | #13, #10 setter, #18 `add` |

### 3.2 Sibling-owned tests that already cover members of THIS file

| File:line (owner) | Members exercised |
| --- | --- |
| `QuickFiler.Test/Controllers/EfcDataModelTests.cs:152-218` (F5) | **#11 `LoadDf()` full positive path** (`:210-220`) via `QuickFiler/Controllers/EfcDataModel.cs:67`; #10 setter |
| `EfcDataModelTests.cs:84-118` (F5) | #13 and #4 with a real `DataFrame` built from a mocked `Table` |
| `EfcDataModelTests.cs:24-49` (F5) | #8, #7, #6, #2 via `ConversationResolver.LoadAsync(IEnumerable<MailItem>, …)` |
| `EfcDataModelTests.cs:120-149, 152-178` (F5) | `FullyLoaded` read only; **asserts the handler did NOT run**, so it adds no coverage to #19 |

---

## 4. Per-member coverage gap

| # | Member | Status |
| --- | --- | --- |
| 1 | `_convInfoFields` | covered |
| 2 | `ConversationInfo` | partially covered (branch missed: the `DependenciesNotNull` **false** arm — `_mailItem == null` returning `default(Pair<…>)`, `Initializer.cs:155-157`) |
| 3 | `LoadConversationInfo` | partially covered (branches missed: the **entire positive path** `:58-72` — the `MailItemHelper.FromDf` projection, the `OrderByDescending(ConversationID)` ordering, and the `FolderName == ((Folder)_mailItem.Parent).Name` filter) |
| 4 | `LoadConversationInfoAsync` | partially covered (branches missed: the `else` arm at `:96-105` calling `MailItemHelper.FromDfAsync`; the `else` arm at `:120-123` assigning `[MailHelper]`; the `idx > -1` **false** arm at `:115`; the **entire** `UpdateUI is not null` block `:140-151` including the `Interlocked.Increment` at `:143` and the dispatcher call at `:150`; the second cancellation guard `:142`; the initial cancellation guard's throwing arm `:80`) |
| 5 | `_conversationItems` | covered |
| 6 | `ConversationItems` | partially covered (branch missed: `_mailItem == null` dependency-check arm) |
| 7 | `LoadConversationItems` | covered (single-element lists only; multi-element ordering unverified) |
| 8 | `LoadConversationItemsAsync` | partially covered (branch missed: cancelled-token throw `:187`) |
| 9 | `_df` | covered |
| 10 | `Df` | covered both getter arms and setter |
| 11 | `LoadDf` | covered (positive path via F5's `EfcDataModelTests.cs:152-218`; the null-`Parent` cast failure is reached but unasserted in `Count_WhenNotYetInitialized_AttemptsToLoadCount`) |
| 12 | `DfNotifyIfNotNull` | **uncovered.** It runs only as the `GetOrLoad` callback when the `Df` getter performs a load (`:204`). The single test that triggers that load (`ConversationResolverTests.cs:159-181`) throws inside `LoadDf` before the callback is reached; every other path assigns `Df` through the **setter**, which uses a different callback (`:205`). Both arms missed |
| 13 | `LoadDfAsync` | partially covered (branches missed: cancelled-token throw `:233`; `_mailItem.ThrowIfNull()` failure `:234`; `parent` **null** arm of `:243-244`; the `folderName.IsNullOrEmpty()` **true** arm at `:249`) |
| 14 | `_count` initializer | covered |
| 15 | `Count` | covered both arms |
| 16 | `LoadCount` | **effectively uncovered** beyond `var df = Df;` (`:275-276`). The only test that reaches it throws inside `Df`. Both `is not null` arms and the return are missed |
| 17 | `NotifyPropertyChanged` | partially covered (branch missed: the **`PropertyChanged is not null` true arm** `:296-299`. In every existing test the notification-raising assignments occur *before* any subscriber is attached — `LoadAsync` subscribes last per INV-1 of artifact 05, and `EfcDataModel.cs:67-69` assigns `Df` before subscribing) |
| 18 | `PropertyChanged` event | `add` covered; **`remove` uncovered** |
| 19 | `Handler_PropertyChanged` | **entirely uncovered.** No existing test raises `PropertyChanged` on a resolver that has the handler attached. `EfcDataModelTests.cs:144-148` and `:173-177` explicitly assert the opposite (`FullyLoaded` stays `false`). All 6 branches and both `await` continuations missed |

Uncovered concentration: members 12, 16, 19 plus the unreached arms of 3, 4, 13, 17 — collectively
the largest coverage gap in the F4 conversation cluster.

---

## 5. Testability classification per member

| # | Member | Classification | Interop / host touch |
| --- | --- | --- | --- |
| 1, 5, 9, 14 | backing fields | `pure-testable-now` | none |
| 2, 6 | `ConversationInfo` / `ConversationItems` | `pure-testable-now` | none directly; the loaders they call touch `MailItem.Parent` |
| 3 | `LoadConversationInfo` | **`needs-seam`** for the positive path. Interop touched: `MailItem.Parent` → `(Folder).Name` (`:66`) — mockable with Moq. The blocking dependency is not Interop but the **static** `MailItemHelper.FromDf` (`:61`), which dereferences `appGlobals.Ol.NamespaceMAPI` and resolves a live item by EntryID (`UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Loading.cs:18-34`). Seam S2 of artifact 05 removes it | `MailItem.Parent`, `Folder.Name` |
| 4 | `LoadConversationInfoAsync` | **`needs-seam`** on two axes: (a) the `UpdateUI` block `:140-151` calls the static `UiThread.Dispatcher` — seam S1 of artifact 05; (b) the `else` arm `:98-104` calls the static `MailItemHelper.FromDfAsync` — seam S2. Interop: `DataFrame` column read `Df.Expanded["EntryID"][indexRow]` (`:91`, not Interop), `MailItem.Parent` → `Folder.Name` (`:126`) | `MailItem.Parent`, `Folder.Name` |
| 7 | `LoadConversationItems` | `pure-testable-now` — reads only `MailItemHelper.Item` (a plain property, `UtilitiesCS/.../MailItemHelper.Properties.cs:92-98`), no COM member access | none |
| 8 | `LoadConversationItemsAsync` | `pure-testable-now` — `await Task.Run(...)` is a hard synchronization point; `Task.Run` is not banned | none |
| 10 | `Df` | `pure-testable-now` | none |
| 11 | `LoadDf` | `pure-testable-now` — proven by F5 at `EfcDataModelTests.cs:186-218`. Interop touched: `MailItem.GetConversation()` (extension `UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.cs:355-359`), `Conversation.GetTable()`, `MailItem.Parent` → `Folder.Name`. All are COM interfaces, mockable with Moq per `00-cluster-overview.md` §3 | `MailItem`, `Conversation`, `Table`, `Folder` |
| 12 | `DfNotifyIfNotNull` | `pure-testable-now` — `internal`, directly callable from `QuickFiler.Test` via `InternalsVisibleTo` (`QuickFiler/Properties/AssemblyInfo.cs:5`) | none |
| 13 | `LoadDfAsync` | `pure-testable-now` — proven at `ConversationResolverTests.cs:273-312`. Interop: `MailItem.GetConversationDfAsync` (which internally calls `MailItem.GetConversation()` / `Conversation.GetTable()`), `MailItem.Parent` | `MailItem`, `Conversation`, `Table`, `Folder` |
| 15 | `Count` | `pure-testable-now` | none |
| 16 | `LoadCount` | `pure-testable-now` — `internal`; a test may assign `Df` through the public setter first, then call `LoadCount()` directly, avoiding the throwing `Df` load entirely | none |
| 17 | `NotifyPropertyChanged` | `pure-testable-now` — `protected`; reachable through any notifying setter with a subscriber attached | none |
| 18 | `PropertyChanged` | `pure-testable-now` | none |
| 19 | `Handler_PropertyChanged` | **`needs-seam`** — see §7 S4. The method is `async void`; without a seam its completion is unobservable, and the only deterministic-looking alternative (a wall-clock spin) is prohibited | transitively `MailItem.Parent`, `Folder.Name`, and the static `UiThread.Dispatcher` at `:320` |

No member requires a live WinForms control or the UI thread once S1 and S4 are applied. See §11.

---

## 6. Ordering and async invariants (load-bearing for this cluster)

`Thread.Sleep`, `Task.Delay`, `SpinWait.SpinUntil` with a wall-clock budget, and every other real
wall-clock wait are **prohibited** in tests (`BannedSymbols.txt:4-7`,
`.claude/rules/general-unit-test.md` § Determinism Infrastructure). Every technique below is
synchronization-point-based, not timing-based.

### INV-10 — the lazy `Pair<>` properties are load-once, notify-on-first-load

`ConversationInfo` (`:22-29`), `ConversationItems` (`:164-170`), and `Df` (`:204`) all route through
`Initializer.GetOrLoad(ref field, loader, callbackOnSet, strict: false, _mailItem)`
(`UtilitiesCS/HelperClasses/Initializer.cs:142-158` → `:112-120`). Consequences:

1. The loader runs **only** when the backing field equals `default(T)`.
2. The `callbackOnSet` fires **only** on that first load, never on cache hits.
3. If `_mailItem` is `null` the dependency check fails and the getter returns `default(Pair<…>)`
   **without** invoking the loader and **without** notifying — a silent-null contract that callers
   must handle (`Initializer.cs:122-123` documents it explicitly).

Deterministic test: read the property twice against a resolver whose loader is instrumented (via S2
for `ConversationInfo`, or by pre-assigning `Df` for `Count`) and assert the loader ran exactly once
and exactly one notification was recorded.

### INV-11 — `Count` uses a `(-1,-1)` sentinel, NOT `default`

`_count = new Pair<int>(-1,-1)` (`:263`) with the predicate overload
`GetOrLoad(ref _count, static v => v.Expanded >= 0, LoadCount)` (`:269` →
`Initializer.cs:178-189`). The comment at `:260-262` records why: a genuine `(0,0)` result is
indistinguishable from `default(Pair<int>)`, which caused `LoadCount` to re-run on every access.
This is already regression-tested (`ConversationResolverTests.cs:128-181`) and must not regress.

Deterministic test (new, non-duplicating): assign `Count = new Pair<int>(-1, 5)` and assert the getter
**re-loads**, proving the predicate keys on `Expanded`, not on `SameFolder`.

### INV-12 — `LoadConversationInfo` returns a single-item fallback, never throws, when `Count.Expanded <= 0`

`:39-56`. The comment at `:41-47` records both the reason (Junk E-mail where `FilterConversation`
removes all rows) and the hard constraint: *"Do NOT access ConversationInfo or Df in this path: they
are lazy properties backed by this same loader and would recurse back into LoadConversationInfo()"*.
The fallback list contains `MailHelper` in **both** slots. Already covered; do not re-author.

### INV-13 — `LoadConversationInfoAsync` orders the expanded list ASCENDING by `ConversationID`, while the synchronous loader orders it DESCENDING

`:109` `.OrderBy(x => x.ConversationID)` versus `:62` `.OrderByDescending(itemInfo => itemInfo.ConversationID)`.
This is an **observable divergence between the two loaders of the same property**. Consumers read
`ConversationInfo.Expanded` positionally (`QuickFiler/Controllers/EfcItemController.cs:1103`,
`QuickFiler/Controllers/QfcItemController.Conversation.cs:121`), so the order is part of the contract.

Deterministic test: build three helpers with `ConversationID` values `c3, c1, c2`; drive the sync
loader and assert descending; drive the async loader and assert ascending. Record the divergence in
the test's XML doc so a future reader does not "fix" one to match the other — F4 is a coverage
feature with an explicit no-behaviour-change acceptance criterion.

### INV-14 — `LoadConversationInfoAsync` reuses the caller's `MailHelper` instance rather than re-projecting it

Two mechanisms, in this order: the pre-projection short-circuit at `:91-95`
(`entryId == MailHelper.EntryId` → `Task.FromResult(this.MailHelper)`), and the post-ordering
identity repair at `:112-119` (`FindIndex(x => x.EntryId == MailHelper.EntryId)` → overwrite with
`MailHelper`). The empty-result fallback at `:120-123` assigns `[MailHelper]`.

The observable guarantee is *reference identity*: the element of `ConversationInfo.Expanded` matching
`MailHelper.EntryId` is the **same object** as `MailHelper`. Already asserted for the single-item
case at `ConversationResolverTests.cs:346`; the multi-item and no-match cases are uncovered.

Deterministic test: with S2 injected, return three distinct helpers where the second matches
`MailHelper.EntryId`; assert `ReferenceEquals(result.Expanded[i], resolver.MailHelper)` for exactly
one `i`. Separately, return helpers where none matches and assert the list is left intact
(`idx > -1` false arm).

### INV-15 — `ConversationInfo` is assigned BEFORE `UpdateUI` is invoked, and `UpdateUI` receives the LOCAL list

`:138` then `:140-151`, with `UpdateUI(pair.Expanded)` at `:150` — the local, not the property. The
comments at `:134-137` and `:148-149` record why: reading the property before assignment re-enters the
synchronous loader. This is the Bug-3 regression documented at `ConversationResolverTests.cs:183-202`.

Deterministic test (with S1's dispatcher mock executing inline): supply an `UpdateUI` that captures
`resolver.ConversationInfo.Expanded` at call time and asserts it is already the new pair, and that the
argument it received is reference-equal to `resolver.ConversationInfo.Expanded`.

### INV-16 — the UI-publish counter increments once per publish and only when `UpdateUI` is non-null

`:143` `Interlocked.Increment(ref _uiPublishCount)` inside the `if (UpdateUI is not null)` block. The
existing `LoadDfAsync` test asserts the counter stays at `0` (`ConversationResolverTests.cs:310`).
The increment path itself is uncovered.

Deterministic test: with `UpdateUI` set and S1 injected, call `LoadConversationInfoAsync` twice and
assert `_uiPublishCount == 2` (reflection helper already exists at `ConversationResolverTests.cs:564-576`).
`Interlocked` is used because publication may occur from a background thread; a single-threaded
assertion is sufficient for coverage and remains deterministic.

### INV-17 — cancellation is checked at method entry AND immediately before the UI publish

`:80` and `:142` in `LoadConversationInfoAsync`; `:187` in `LoadConversationItemsAsync`; `:233` in
`LoadDfAsync`. The second check at `:142` exists so that a cancellation arriving during the
`Task.WhenAll` await suppresses the UI publish.

Deterministic test for `:142`: use a `CancellationTokenSource` cancelled by the S2 helper-factory
delegate itself (the delegate is invoked between `:80` and `:142`), then assert
`OperationCanceledException` and that `UpdateUI` was never invoked. This is a pure callback-ordering
technique with no timing dependency.

### INV-18 — `LoadDfAsync` tolerates a null/unnamed parent folder; `LoadDf` does not

`LoadDfAsync` uses `_mailItem.Parent as Folder` then `parent?.Name ?? string.Empty` (`:243-244`) and
branches on `folderName.IsNullOrEmpty()` (`:248-250`) to skip the same-folder filter, assigning
`dfSameFolder = dfExpanded`. `LoadDf` (`:213, 216`) uses a hard `(Folder)_mailItem.Parent` cast with
no guard and will throw. Both behaviours are current contract.

Deterministic test: `LoadDfAsync` with `MailItem.Parent` returning `null` — assert
`Df.SameFolder` is reference-equal to `Df.Expanded` (the `IsNullOrEmpty` **true** arm, uncovered).

### INV-19 — `Df` assignment triggers background initialization through `Handler_PropertyChanged`; the STATE TRANSITION is `FullyLoaded: true → false → true`

`:306-315`. On a `"Df"` notification the handler sets `FullyLoaded = false` **synchronously**, awaits
`BackgroundInitInfoItemsAsync` (`ConversationResolver.cs:217-233`, which runs
`LoadConversationInfoAsync` then `LoadConversationItemsAsync`), then sets `FullyLoaded = true`. On
`OperationCanceledException` the catch at `:314` swallows and **leaves `FullyLoaded == false`** — the
resolver stays permanently unloaded with no error surfaced. That silent-failure shape is existing
contract and must be locked by a test, not changed.

The complete state machine for the type:

| From | Trigger | To | Evidence |
| --- | --- | --- | --- |
| constructed (`FullyLoaded=false`, all `Pair<>` fields `default`) | `Df` setter | `Df` cached; `PropertyChanged("Df")` raised | `:205`, `Initializer.cs:95-101` |
| `PropertyChanged("Df")` with handler attached | handler `:306` | `FullyLoaded=false`, background init started | `:308-311` |
| background init completes | `:312` | `FullyLoaded=true`, `ConversationInfo` + `ConversationItems` cached | `ConversationResolver.cs:226-227` |
| background init cancelled | `:314` | `FullyLoaded` remains `false`, exception swallowed | `:314` |
| `UpdateUI` setter | `ConversationResolver.cs:274-278` → `PropertyChanged("UpdateUI")` | handler `:316` | `:316` |
| `PropertyChanged("UpdateUI")` while `FullyLoaded==true` | `:318-323` | republishes `ConversationInfo.Expanded` to the UI | `:320-322` |
| `PropertyChanged("UpdateUI")` while `FullyLoaded==false` | `:318` false | no-op | `:318` |
| `PropertyChanged` with any other name | `:306`/`:316` both false | no-op | — |

**Latent asymmetry worth recording (do not "fix" in F4).** The `UpdateUI` republish at `:321` reads
the **lazy property** `ConversationInfo.Expanded`, whereas `LoadConversationInfoAsync` deliberately
passes the local (`:150`, INV-15). If `ConversationInfo` has not been loaded, `:321` re-enters
`LoadConversationInfo()`, which since the Issue-#103 fix returns the safe fallback rather than
throwing (`:39-56`). The republish therefore emits a single-item list instead of the real
conversation. This is a behavioural nuance, not a crash; it is out of F4's no-behaviour-change scope
and should be promoted as its own issue if the maintainer wants it changed.

### INV-20 — subscribe/unsubscribe symmetry

`ConversationResolver` never unsubscribes its own handler; `remove` on the `PropertyChanged` event
(`:302`) has no production call site. Sibling-owned code subscribes at
`QuickFiler/Controllers/EfcDataModel.cs:68-69` and `QuickFiler/Controllers/EfcItemController.cs:667`
and likewise never unsubscribes. Recorded as an observation; F4 must not change it.

### INV-21 — banned-API audit of this file: CLEAN

Grep of `QuickFiler/Helper Classes/` for `DateTime.Now`, `DateTime.UtcNow`, `Thread.Sleep`,
`Task.Delay`, `Random.Shared` returns **no match**. **No banned-API finding in this file. No
`TimeProvider` seam is required.**

Two `System.Diagnostics.Stopwatch` uses exist at `:236` (`Stopwatch.StartNew()`) and `:256`
(`ElapsedMilliseconds`). `Stopwatch` is not on `BannedSymbols.txt` (`:1-7`) and both uses feed only a
log-message interpolation; **no control flow reads elapsed time**, so no clock seam is warranted.
Recorded as an observation, not a finding.

Awareness-only, outside F4's file set: `:242` calls
`MailItem.GetConversationDfAsync(token)`, whose implementation at
`UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.cs:252-258` applies a 1000 ms
`TimeOutTask.RunWithTimeout` budget. It is never hit when the mocked `Conversation` returns
synchronously. F4 must not edit UtilitiesCS.

---

## 7. Seam proposal (ranked: interface > injectable delegate > adapter)

Seams **S1** (`IUiDispatcher` injection) and **S2** (`HelperFromDf` / `HelperFromDfAsync` delegate
properties) are specified in `05-ConversationResolver.md` §7 and are declared in the sibling partial;
their consumption sites are in **this** file:

- S1 consumption: replace `UiThread.Dispatcher.InvokeAsync(...)` at `:150` and `:320` with
  `UiDispatcher.InvokeAsync(...)`.
- S2 consumption: replace `MailItemHelper.FromDf(...)` at `:61` with `HelperFromDf(...)` and
  `MailItemHelper.FromDfAsync(...)` at `:98-104` with `HelperFromDfAsync(...)`.

### S4 (extract-to-core, tier 1 shape) — make the `async void` handler awaitable

- Problem: `Handler_PropertyChanged` (`:304-325`) is `async void`. Its first `await`
  (`BackgroundInitInfoItemsAsync`, `:311`) reaches `LoadConversationItemsAsync`, which awaits
  `Task.Run` (`:194`) and therefore genuinely yields. The method returns to the event raiser before
  the work completes, so no assertion on `FullyLoaded`, `ConversationInfo`, or notification order can
  be made without a wall-clock wait — which is prohibited.
- Proposal, entirely inside this file:

  ```
  public async void Handler_PropertyChanged(object sender, PropertyChangedEventArgs e) =>
      await HandlePropertyChangedCoreAsync(e).ConfigureAwait(false);

  internal async Task HandlePropertyChangedCoreAsync(PropertyChangedEventArgs e)
  {
      // exact current body, including the OperationCanceledException catch
  }
  ```

- Behaviour preservation: the `try`/`catch (OperationCanceledException)` moves wholesale into the
  core, so cancellation is swallowed at the same point and `FullyLoaded` retains its `false` value on
  cancellation (INV-19). Every other exception continues to escape an `async void` method and reach
  the process-level unhandled handler exactly as today.
- Tests then call `await resolver.HandlePropertyChangedCoreAsync(new PropertyChangedEventArgs("Df"))`
  — one hard synchronization point, no timers.
- **Optional companion (tier 2, only if the plan wants the event path itself covered):**
  `internal Task LastHandlerTask { get; private set; }` assigned inside the `async void` wrapper, so a
  test can raise the real event and then `await resolver.LastHandlerTask`. Recommended only if T-list
  items requiring the genuine event path are kept.
- **Requires no sibling-owned file change.** `Handler_PropertyChanged`'s signature is unchanged, so
  the method-group subscriptions at `QuickFiler/Controllers/EfcDataModel.cs:69` and
  `QuickFiler/Controllers/EfcItemController.cs:667` still bind, and the
  `IConversationResolver:25` member is still satisfied.

### S5 (no seam required) — `LoadCount` and `DfNotifyIfNotNull`

Both are `internal` and side-effect-light. Tests reach them directly through `InternalsVisibleTo`
(`QuickFiler/Properties/AssemblyInfo.cs:5`), pre-assigning `Df` through the public setter to avoid the
COM-backed load. No seam.

### Rejected alternatives (brief)

- **Making `Handler_PropertyChanged` return `Task`**: rejected — breaks the `PropertyChangedEventHandler`
  delegate conversion at two sibling-owned call sites and the `IConversationResolver` member.
- **A `TaskCompletionSource` field signalled at the end of the handler, awaited by tests**: rejected as
  the primary seam — it exposes async plumbing on the public surface and duplicates what the extracted
  core gives for free. Retained only as the optional `LastHandlerTask` companion above.
- **`FakeTimeProvider` for the loaders**: rejected — INV-21 shows no time-dependent control flow in
  this file, so a clock seam adds surface without coverage benefit. (`FakeTimeProvider` remains the
  correct tool for other F4 files per `00-cluster-overview.md` §4.)
- **Mocking `IOlObjects.NamespaceMAPI` + `NameSpace.GetItemFromID` instead of S2**: viable with the
  established Moq-on-Interop pattern, but each test then carries a large arrange block and becomes
  coupled to `MailItemHelper`'s resolution internals. Kept as a documented fallback only.

---

## 8. Cross-child conflict analysis

The full inventory of files outside F4 that call into `ConversationResolver` is in
`05-ConversationResolver.md` §8.1–8.2 and is not repeated. The entries that bind specifically to
members declared in **this** file:

| File:line | Member of this file consumed | Owner |
| --- | --- | --- |
| `QuickFiler/Controllers/EfcDataModel.cs:67` | **`internal Pair<DataFrame> LoadDf()`** (`:208`) and the `Df` setter (`:205`) | F5 |
| `QuickFiler/Controllers/EfcDataModel.cs:68-69` | `PropertyChanged` `add` (`:302`) + `Handler_PropertyChanged` method group (`:304`) | F5 |
| `QuickFiler/Controllers/EfcDataModel.cs:273` | `ConversationInfo.SameFolder` (`:20`) | F5 |
| `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:69` | `ConversationInfo.SameFolder` | F8 |
| `QuickFiler/Controllers/EfcItemController.cs:315` | `Count.SameFolder` (`:265`) | F9 |
| `QuickFiler/Controllers/EfcItemController.cs:667-668` | `PropertyChanged` `add` | F9 |
| `QuickFiler/Controllers/EfcItemController.cs:746-751, 1103` | `ConversationInfo.Expanded` | F9 |
| `QuickFiler/Controllers/EfcItemController.cs:323` (commented) | `LoadConversationItemsAsync(token, backgroundLoad: true)` | F9 |
| `QuickFiler/Controllers/QfcItemController.Conversation.cs:36, 43, 105, 135, 175` | `Count.SameFolder` | F10 |
| `QuickFiler/Controllers/QfcItemController.Conversation.cs:121` | `ConversationInfo.Expanded` | F10 |
| `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:294, 296` | `Count.Expanded`, `ConversationInfo.Expanded` | F10 |
| `QuickFiler/Controllers/QfcItemController.MailActions.cs:44, 163` | `Count.SameFolder`, `ConversationInfo.SameFolder` | F10 |
| `QuickFiler/Helper Classes/IConversationResolver.cs:14-31` | declares `ConversationInfo`, `ConversationItems`, `Count`, `Df`, `Handler_PropertyChanged`, `LoadConversationInfoAsync`, `LoadConversationItemsAsync`, `LoadDfAsync` | **F4 (this child)** |

### Conflict verdict for every proposed seam

| Seam | Verdict |
| --- | --- |
| S1 — replace the two `UiThread.Dispatcher.InvokeAsync` call sites (`:150`, `:320`) with the injected `UiDispatcher` | **Requires no sibling-owned file change.** Both edits are inside this file; the injected property is declared in `ConversationResolver.cs`. |
| S2 — replace the two `MailItemHelper.FromDf*` call sites (`:61`, `:98-104`) with the injected delegates | **Requires no sibling-owned file change.** |
| S4 — extract `HandlePropertyChangedCoreAsync` | **Requires no sibling-owned file change.** Signature of `Handler_PropertyChanged` preserved for `EfcDataModel.cs:69` and `EfcItemController.cs:667`. |
| S5 — none | n/a |
| Changing the signature of `internal Pair<DataFrame> LoadDf()` | **Would require editing `QuickFiler/Controllers/EfcDataModel.cs:67` (F5).** PROHIBITED. |
| Changing the return type or parameter list of `LoadConversationInfoAsync` / `LoadConversationItemsAsync` / `LoadDfAsync` | **Would require editing `QuickFiler/Helper Classes/IConversationResolver.cs` (F4-owned, safe) AND `QuickFiler/Controllers/EfcItemController.cs:323` (F9, commented) — and would break any future sibling caller.** Not proposed. Keep signatures, including the unused `backgroundLoad` parameter. |
| Removing the unused `TaskCreationOptions options` locals (`:83-85`, `:190-192`) | **Requires no sibling-owned file change** (locals only). Optional, behaviour-neutral. |
| Adding a member to `IConversationResolver` | safe today (single implementer, zero consumers — artifact 07 §3) but not recommended; keep seams on the concrete class. |

**Additional coordination note.** F5's `QuickFiler.Test/Controllers/EfcDataModelTests.cs:144-147` and
`:173-176` use `SpinWait.SpinUntil(..., 250)` — a real wall-clock budget — to assert that background
initialization has **not** run. F4 must not copy that pattern and must not edit that file. S4 gives F4
the deterministic alternative for the same assertion.

---

## 9. 500-line compliance

| File | Now | After proposed seams | Limit |
| --- | --- | --- | --- |
| `QuickFiler/Helper Classes/ConversationResolver.Loading.cs` | 329 | ≈ 350 (S4 wrapper + core signature and doc ≈ +10; S1/S2 are in-place call-site substitutions, net ≈ 0; optional dead-local removal ≈ −4) | 500 — **compliant, 150 lines of headroom** |

Confirmed: headroom is ample; no partial split is required. No new production file is proposed for
this file, so no `<Compile Include=...>` edit to `QuickFiler/QuickFiler.csproj` (`Helper Classes\`
block at `:342-354`) is needed.

Test-side: the two new test files below each require a `<Compile Include=...>` entry in
`QuickFiler.Test/QuickFiler.Test.csproj` inside the existing contiguous `Helper Classes\` block
(`:158-165`), in alphabetical order — a shared-file conflict surface with all thirteen siblings
(`00-cluster-overview.md` §1.3).

---

## 10. Recommended test cases (one per line; each becomes its own atomic plan task)

MSTest + Moq + FluentAssertions. Destinations, both new, namespace `QuickFiler.Test.HelperClasses`:

- **C** = `QuickFiler.Test/Helper Classes/ConversationResolverLoadingTests.cs` (loaders and lazy properties)
- **D** = `QuickFiler.Test/Helper Classes/ConversationResolverNotificationTests.cs` (`INotifyPropertyChanged` and the handler state machine)

| # | `[TestMethod]` name | Arrange / Act / Assert | Category | Destination |
| --- | --- | --- | --- | --- |
| T30 | `ConversationInfoGetter_WhenMailItemIsNull_ReturnsDefaultPairWithoutInvokingLoader` | Arrange resolver via the private ctor path with `_mailItem` left null (reflection, or the `LoadAsync(helper,…)` factory before `Mail` assignment); Act read `ConversationInfo`; Assert both members are `null` and no notification fired (INV-10 item 3, `Initializer.cs:155-157`) | boundary | C |
| T31 | `ConversationItemsGetter_WhenMailItemIsNull_ReturnsDefaultPair` | as T30 for member #6 | boundary | C |
| T32 | `ConversationInfoGetter_WhenLoaderRuns_InvokesLoaderExactlyOnceAcrossTwoReads` | Arrange `Count = (0,0)` so the fallback loader runs; Act read `ConversationInfo` twice; Assert exactly one `PropertyChanged("ConversationInfo")` recorded (INV-10 items 1–2) | positive | C |
| T33 | `LoadConversationInfo_WhenCountExpandedPositive_ProjectsEveryRowThroughInjectedHelperFactory` | Arrange S2 `HelperFromDf` returning one helper per index, `Count = (2,2)`, `Df` pre-assigned, mocked `Folder.Name`; Act call `LoadConversationInfo()`; Assert the factory was invoked once per index with indices `0..1` (member #3 positive path, currently uncovered) | positive | C |
| T34 | `LoadConversationInfo_OrdersExpandedListDescendingByConversationId` | Arrange S2 returning helpers with `ConversationID` `c1,c3,c2`; Act call `LoadConversationInfo()`; Assert `Expanded` order is `c3,c2,c1` (INV-13 sync half, `:62`) | positive | C |
| T35 | `LoadConversationInfo_FiltersSameFolderByParentFolderName` | Arrange S2 returning helpers with `FolderName` `Inbox,Archive,Inbox` and mocked `MailItem.Parent` → `Folder.Name == "Inbox"`; Act call; Assert `SameFolder` has 2 items and `Expanded` has 3 (`:65-67`) | positive | C |
| T36 | `LoadConversationInfo_WhenParentIsNotAFolder_ThrowsInvalidCastException` | Arrange `Count = (1,1)`, S2 returning one helper, `MailItem.Parent` returning a `Mock<Store>`; Act/Assert `InvalidCastException` from `(Folder)_mailItem.Parent` at `:66` | error-handling | C |
| T37 | `LoadConversationInfoAsync_OrdersExpandedListAscendingByConversationId` | Arrange S2 async factory returning `c3,c1,c2`, `Df` and `Count` pre-assigned; Act `await LoadConversationInfoAsync(None,false)`; Assert order `c1,c2,c3` (INV-13 async half, `:109`) | positive | C |
| T38 | `LoadConversationInfoAsync_WhenRowEntryIdMatchesMailHelper_SkipsTheAsyncFactoryForThatRow` | Arrange a two-row `DataFrame` where row 0's `EntryID` equals `MailHelper.EntryId`; Act await; Assert the async factory was invoked for row 1 only (`:92-95`, true arm partially covered — this asserts the *skip*, which no existing test does) | positive | C |
| T39 | `LoadConversationInfoAsync_WhenRowEntryIdDiffers_ProjectsThroughTheAsyncFactory` | Arrange a one-row `DataFrame` whose `EntryID` differs from `MailHelper.EntryId`; Act await; Assert the factory ran once (`:96-105` **else** arm, currently uncovered) | positive | C |
| T40 | `LoadConversationInfoAsync_RestoresMailHelperIdentityAtItsOrderedPosition` | Arrange three helpers, the middle one sharing `MailHelper.EntryId`; Act await; Assert exactly one element is `ReferenceEquals` to `MailHelper` (INV-14, `:112-119`) | positive | C |
| T41 | `LoadConversationInfoAsync_WhenNoRowMatchesMailHelper_LeavesProjectedListUnchanged` | Arrange helpers with no matching `EntryId`; Act await; Assert no element is `ReferenceEquals` to `MailHelper` and the count is unchanged (`idx > -1` **false** arm, uncovered) | boundary | C |
| T42 | `LoadConversationInfoAsync_WhenNoRowsProjected_FallsBackToSingleMailHelperList` | Arrange `Count = (0,0)` so `Enumerable.Range(0,0)` yields nothing; Act await; Assert `Expanded` is a single-element list containing `MailHelper` (`:120-123` **else** arm, uncovered) | boundary | C |
| T43 | `LoadConversationInfoAsync_AssignsConversationInfoBeforeInvokingUpdateUi` | Arrange S1 dispatcher mock executing inline and an `UpdateUI` that captures `resolver.ConversationInfo.Expanded` when called; Act await; Assert the captured value is already the new list (INV-15, `:138` before `:150`) | positive | C |
| T44 | `LoadConversationInfoAsync_PassesTheLocalListToUpdateUiNotTheLazyProperty` | as T43; Assert the argument delivered to `UpdateUI` is `ReferenceEquals` to `resolver.ConversationInfo.Expanded` and the lazy loader was never re-entered (S2 factory call count unchanged) (INV-15, `:150`) | positive | C |
| T45 | `LoadConversationInfoAsync_IncrementsUiPublishCountOncePerPublish` | Arrange as T43; Act await twice; Assert private `_uiPublishCount == 2` (INV-16, `:143`) | positive | C |
| T46 | `LoadConversationInfoAsync_WhenUpdateUiIsNull_DoesNotDispatchAndLeavesPublishCountZero` | Arrange with `UpdateUI` unset and a strict `Mock<IUiDispatcher>`; Act await; Assert `dispatcher.VerifyNoOtherCalls()` and `_uiPublishCount == 0` (`:140` false arm — asserted directly rather than incidentally) | boundary | C |
| T47 | `LoadConversationInfoAsync_WhenTokenAlreadyCancelled_ThrowsBeforeProjecting` | Arrange pre-cancelled token and a strict S2 factory; Act/Assert `OperationCanceledException` and the factory was never invoked (`:80`) | error-handling | C |
| T48 | `LoadConversationInfoAsync_WhenCancelledDuringProjection_SuppressesTheUiPublish` | Arrange S2 factory that cancels the CTS on first invocation, `UpdateUI` set; Act/Assert `OperationCanceledException` and `UpdateUI` never invoked (INV-17, `:142`) | error-handling | C |
| T49 | `LoadConversationItems_ProjectsHelperItemsPreservingOrderForBothSlots` | Arrange `ConversationInfo` pre-assigned with three helpers whose `Item` are three distinct `Mock<MailItem>`; Act call `LoadConversationItems()`; Assert both slots equal the helper order (member #7 multi-element case) | positive | C |
| T50 | `LoadConversationItemsAsync_WhenTokenAlreadyCancelled_ThrowsWithoutAssigningConversationItems` | Arrange pre-cancelled token; Act/Assert `OperationCanceledException` and the private `_conversationItems` field is still `default` (`:187`) | error-handling | C |
| T51 | `LoadCount_WhenBothDataFramesPresent_ReturnsRowCountsForEachSlot` | Arrange `Df` assigned via the public setter with two `DataFrame`s of 3 and 5 rows; Act call `LoadCount()`; Assert `(3, 5)` (member #16, `:277-284`, currently uncovered) | positive | C |
| T52 | `LoadCount_WhenSameFolderDataFrameIsNull_LeavesSameFolderAtNegativeOneSentinel` | Arrange `Df = new Pair<DataFrame>(null, nonNull)`; Act call `LoadCount()`; Assert `SameFolder == -1` and `Expanded` is the real count (`:277` false arm) | boundary | C |
| T53 | `LoadCount_WhenExpandedDataFrameIsNull_LeavesExpandedAtNegativeOneSentinel` | mirror of T52 for `:281` | boundary | C |
| T54 | `CountGetter_WhenExpandedIsNegativeButSameFolderIsPositive_StillReloads` | Arrange `Count = new Pair<int>(5, -1)` and `Df` pre-assigned; Act read `Count`; Assert the loaded value replaced the injected one, proving the predicate keys on `Expanded` (INV-11, `:269`) | boundary | C |
| T55 | `LoadDf_WhenParentFolderNamed_ReturnsExpandedAndSameFolderDataFrames` | Arrange the mocked `Conversation`/`Table` graph and `Folder.Name`; Act call `LoadDf()`; Assert both slots non-null with the expected row counts (member #11 — direct, named coverage; today it is only reached indirectly through F5's `EfcDataModel` test) | positive | C |
| T56 | `LoadDf_WhenParentIsNull_ThrowsNullReferenceException` | Arrange `MailItem.Parent` returning `null`; Act/Assert throw from `((Folder)_mailItem.Parent).Name` at `:213` (INV-18 contrast half) | error-handling | C |
| T57 | `LoadDfAsync_WhenMailItemIsNull_ThrowsArgumentNullException` | Arrange resolver with `_mailItem` null; Act/Assert `ArgumentNullException` from `_mailItem.ThrowIfNull()` (`:234`, `UtilitiesCS/Extensions/NullExtensions.cs:16-35`) | invalid-input | C |
| T58 | `LoadDfAsync_WhenTokenAlreadyCancelled_ThrowsBeforeTouchingTheMailItem` | Arrange pre-cancelled token and a strict `Mock<MailItem>` with no setups; Act/Assert `OperationCanceledException` and `VerifyNoOtherCalls()` (`:233`) | error-handling | C |
| T59 | `LoadDfAsync_WhenParentIsNotAFolder_UsesEmptyFolderNameAndSharesTheExpandedDataFrame` | Arrange `MailItem.Parent` returning a `Mock<Store>` so the `as Folder` yields null; Act await; Assert `Df.SameFolder` is `ReferenceEquals` to `Df.Expanded` (INV-18, `:243-244` null arm and `:249` true arm — both uncovered) | boundary | C |
| T60 | `DfNotifyIfNotNull_WhenBothSlotsPresent_RaisesDfNotification` | Arrange resolver with a `PropertyChanged` recorder; Act call the `internal` `DfNotifyIfNotNull(new Pair<DataFrame>(df, df))`; Assert one notification with `PropertyName == "Df"` (member #12, uncovered) | positive | C |
| T61 | `DfNotifyIfNotNull_WhenEitherSlotIsNull_RaisesNoNotification` | Act call with `(null, df)` and with `(df, null)`; Assert zero notifications both times (`:225` false arm) | boundary | C |
| T62 | `NotifyPropertyChanged_WhenSubscriberAttached_DeliversSenderAndPropertyName` | Arrange a subscriber attached BEFORE the mutation; Act assign `ConversationItems`; Assert the handler received `sender == resolver` and `PropertyName == "ConversationItems"` (member #17 **true** arm, currently uncovered) | positive | D |
| T63 | `NotifyPropertyChanged_WhenNoSubscriber_DoesNotThrow` | Arrange no subscriber; Act assign `ConversationInfo`; Assert no throw (`:296` false arm — asserted directly) | boundary | D |
| T64 | `PropertyChangedEvent_AfterUnsubscribe_StopsDeliveringNotifications` | Arrange subscribe then `-=`; Act assign `Df`; Assert zero notifications (member #18 `remove`, uncovered) | boundary | D |
| T65 | `HandlePropertyChangedCore_WhenDfChanged_TransitionsFullyLoadedFalseThenTrue` | Arrange S1 + S2 injected, `Count`/`Df` pre-assigned, a recorder capturing `FullyLoaded` at each notification; Act `await HandlePropertyChangedCoreAsync(new PropertyChangedEventArgs("Df"))`; Assert `FullyLoaded` was `false` during and `true` after (INV-19, `:306-313`) | positive | D |
| T66 | `HandlePropertyChangedCore_WhenDfChanged_LoadsConversationInfoThenConversationItems` | as T65; Assert the recorded notification order is `["ConversationInfo","ConversationItems"]` (INV-19 / artifact 05 INV-2) | positive | D |
| T67 | `HandlePropertyChangedCore_WhenBackgroundInitCancelled_SwallowsAndLeavesFullyLoadedFalse` | Arrange a pre-cancelled `_token` via the `internal Token` setter; Act `await HandlePropertyChangedCoreAsync(new PropertyChangedEventArgs("Df"))`; Assert no exception escapes and `FullyLoaded == false` (`:314`, uncovered silent-failure contract) | error-handling | D |
| T68 | `HandlePropertyChangedCore_WhenUpdateUiChangedAndFullyLoaded_RepublishesExpandedConversation` | Arrange `FullyLoaded == true` (drive T65 first, or set via the `Df` path), `ConversationInfo` pre-assigned, `UpdateUI` recorder, S1 inline dispatcher; Act `await HandlePropertyChangedCoreAsync(new PropertyChangedEventArgs("UpdateUI"))`; Assert `UpdateUI` received `ConversationInfo.Expanded` exactly once (`:318-323`, uncovered) | positive | D |
| T69 | `HandlePropertyChangedCore_WhenUpdateUiChangedAndNotFullyLoaded_DoesNotRepublish` | Arrange `FullyLoaded == false`, strict `Mock<IUiDispatcher>`; Act await with `"UpdateUI"`; Assert `dispatcher.VerifyNoOtherCalls()` (`:318` false arm) | boundary | D |
| T70 | `HandlePropertyChangedCore_WhenUnrelatedPropertyChanged_IsANoOp` | Arrange strict dispatcher and strict S2 factory; Act await with `"MailHelper"`; Assert nothing was invoked and `FullyLoaded` is unchanged (`:306`/`:316` both false) | boundary | D |
| T71 | `HandlerPropertyChanged_WhenSubscribedAndDfAssigned_CompletesTheRecordedHandlerTask` | Arrange the S4 optional `LastHandlerTask` companion, subscribe the real handler; Act assign `Df`, then `await resolver.LastHandlerTask`; Assert `FullyLoaded == true` (covers the `async void` wrapper `:304-305` itself, which the core-only tests do not) | positive | D |

**Count for this file: 42 enumerated test cases** (T30–T71).
Category spread: positive 17, invalid-input 1, boundary 16, error-handling 8 — all four categories
covered, satisfying AC5.

**Excluded as duplicates of existing coverage** (do not re-author):
`LoadConversationInfo` fallback when `Count.Expanded == 0` (already `ConversationResolverTests.cs:71-88`);
`ConversationInfo` getter fallback (already `:96-110`, `:212-227`);
`ConversationInfo` cached read after direct assignment (already `:238-266`);
`Count` `(0,0)` no-reload and `(-1,-1)` reload (already `:128-148`, `:159-181`);
`LoadDfAsync` happy path with a named parent folder and zero UI publishes (already `:273-312`);
`LoadConversationInfoAsync` single-row `EntryId`-match path end-to-end (already `:314-377` and F5's
`EfcDataModelTests.cs:84-118`);
`LoadDf` positive path via the `EfcDataModel` constructor (already F5's `EfcDataModelTests.cs:186-218`
— T55 is retained deliberately because it is the first test that names `LoadDf` and asserts its
contract directly rather than through a sibling-owned controller).

---

## 11. STA determination

**No member of this file requires an STA test.** Justification:

- The file constructs no WinForms or WPF object, derives from no `Form`, and touches no `Control`.
- The only host coupling is the static `UiThread.Dispatcher` at `:150` and `:320`. Epic.md Shared
  Design §3 permits STA only where a seam is proven infeasible; here the seam is not merely feasible,
  the interface already exists (`UtilitiesCS/Threading/IUiDispatcher.cs:15`) with a production adapter
  (`UtilitiesCS/Threading/WpfUiDispatcher.cs:17`) and an in-repo mock precedent
  (`QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:112, 124, 142`).
- Constructing the production default `new WpfUiDispatcher()` inside a unit test is already proven
  safe at `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs:26`, because its parameterless ctor
  passes a **lazy** `() => UiThread.Dispatcher` provider (`WpfUiDispatcher.cs:24-25`).
- `QuickFiler.Test` has no `[STATestClass]`, `[STATestMethod]`, or `*.StaTests.cs` file today
  (`00-cluster-overview.md` §5); this artifact does not propose introducing the first one.

---

## 12. Projected coverage

Estimated denominator ≈ 115–125 sequence points.

| Region | Approx. sequence points | Covered after the proposed set? |
| --- | --- | --- |
| Lazy properties #2, #6, #10, #15 (getters + setters) | ~26 | Yes — T30–T32, T54, existing tests, F5 tests |
| `LoadConversationInfo` #3 | ~14 | Yes, both arms — T33–T36 plus existing fallback tests |
| `LoadConversationInfoAsync` #4 | ~34 | Yes, every branch — T37–T48 |
| `LoadConversationItems` / `…Async` #7, #8 | ~10 | Yes — T49, T50, existing tests |
| `LoadDf` / `DfNotifyIfNotNull` / `LoadDfAsync` #11–#13 | ~24 | Yes, every branch — T55–T59 (loaders), T60–T61 (callback), existing `:273-312`, F5's `:186-218` |
| `LoadCount` #16 | ~9 | Yes, all three arms — T51–T53 |
| `NotifyPropertyChanged` / event #17, #18 | ~7 | Yes, both arms plus `remove` — T62–T64 |
| `Handler_PropertyChanged` #19 (+ S4 core) | ~14 | Yes, all six branches — T65–T71 |

Projected line coverage: **≈ 95%**, clearing the 80% threshold with margin. Under a pessimistic
assumption that a quarter of the compiler-generated async state-machine lines in members #4 and #19
remain unhit, the file still lands near 88%.

**No fraction of this file is irreducible.** Every member is reachable once seams S1, S2, and S4 are
applied, all three of which are internal to F4's own files. **No exemption request against F1's
ledger is required for `ConversationResolver.Loading.cs`**, and no `[ExcludeFromCodeCoverage]`
attribute may be added (epic.md Shared Design §1 makes that a Blocking finding on a testable file).
