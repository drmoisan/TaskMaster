# F4 per-file research — `QuickFiler/Helper Classes/ConversationResolver.cs`

Timestamp: 2026-08-07T22-40

Cluster: CONVERSATION-RESOLUTION (artifacts 05–08). Cross-cutting facts (test-project wiring, Interop
mocking precedent, `TimeProvider` clock seam, STA situation) are established in
`research/00-cluster-overview.md` and are cited rather than restated.

Upstream contract: child F1 owns the per-file line-coverage harness (derived from the Cobertura
output of `scripts/vscode/Invoke-MSTestWithCoverage.ps1`) and the ratified exemption ledger at
`docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`. Neither exists on disk yet.
Every numeric per-file coverage figure below is an estimate derived by reading production members
and existing tests; the authoritative number is captured at execution time via F1's harness.
`coverage.config` at repository root is a shared file this child must not modify.

---

## 1. File facts

| Fact | Value | Evidence |
| --- | --- | --- |
| Path | `QuickFiler/Helper Classes/ConversationResolver.cs` | — |
| Line count | 358 (last line `}` at `:358`) | `ConversationResolver.cs:358` |
| Compiled | yes | `QuickFiler/QuickFiler.csproj:343` — `<Compile Include="Helper Classes\ConversationResolver.cs" />` |
| `[ExcludeFromCodeCoverage]` | **absent** | grep for `ExcludeFromCodeCoverage` across `QuickFiler/Helper Classes/` returns no match |
| Types declared | `public struct Pair<T>` (`:18`), `public partial class ConversationResolver : INotifyPropertyChanged, IConversationResolver` (`:30`) | `:18`, `:30` |
| Sibling partial | `ConversationResolver.Loading.cs:15` (artifact 06) | `:15` |
| 500-line limit | 358 / 500 — 142 lines of headroom | — |

Namespace: `QuickFiler.Helper_Classes` (`:16`). `InternalsVisibleTo("QuickFiler.Test")` is declared at
`QuickFiler/Properties/AssemblyInfo.cs:5`, so `internal` members of this type are directly reachable
from the test assembly (already exploited at
`QuickFiler.Test/Helper Classes/ConversationResolverTests.cs:78` which sets the `internal`
`Count` setter).

---

## 2. Member inventory (coverage denominator for THIS file)

Decision-point counts are static counts of `if`/`else`, ternary, `??`/`?.`, `switch` arms, loops,
`catch`, and `await` continuations.

### 2.1 `public struct Pair<T>` (`:18-28`)

| # | Member | Signature | Lines | Decision points |
| --- | --- | --- | --- | --- |
| 1 | ctor | `public Pair(T sameFolder, T expanded)` | `:20-24` | 0 |
| 2 | property | `public T SameFolder { get; set; }` | `:26` | 0 (auto) |
| 3 | property | `public T Expanded { get; set; }` | `:27` | 0 (auto) |

`Pair<T>` is a **mutable value type with no `Equals`/`GetHashCode` override**. This matters
functionally: `Initializer.GetOrLoad` compares the backing field to `default(T)` via
`EqualityComparer<T>.Default` (`UtilitiesCS/HelperClasses/Initializer.cs:105, 114`), which for
`Pair<T>` resolves to the reflection-based `ValueType.Equals`. That is precisely the mechanism the
`Count` sentinel comment at `ConversationResolver.Loading.cs:260-262` documents.

### 2.2 `ConversationResolver` members physically declared in THIS file

| # | Member | Signature | Lines | Decision points |
| --- | --- | --- | --- | --- |
| 4 | static field init | `private static readonly log4net.ILog logger` | `:32-34` | 0 (static ctor) |
| 5 | static method | `private static string DescribeSynchronizationContext(SynchronizationContext syncContext)` | `:36-39` | 2 (`?.` + `??`) |
| 6 | static method | `private static string BuildConversationResolverTimingContext()` | `:41-44` | 0 |
| 7 | static method | `private static void LogConversationResolverTiming(string phase, string details = null)` | `:46-58` | 4 (2 ternaries) |
| 8 | ctor | `private ConversationResolver()` | `:62` | 0 |
| 9 | ctor | `public ConversationResolver(IApplicationGlobals appGlobals, MailItem mailItem)` | `:64-68` | 0 |
| 10 | ctor | `public ConversationResolver(IApplicationGlobals, MailItem, CancellationTokenSource, CancellationToken, System.Action<List<MailItemHelper>> updateUI = null)` | `:70-84` | 0; constructs `new MailItemHelper(mailItem, _globals)` at `:82` |
| 11 | static factory | `public static async Task<ConversationResolver> LoadAsync(IApplicationGlobals, MailItem, CancellationTokenSource, CancellationToken, bool loadAll, System.Action<List<MailItemHelper>> updateUI = null)` | `:86-124` | 4 (`updateUI is not null`; `if (loadAll)`/`else`) + 4 `await` continuations (`:102`, `:111`, `:112`, `:113`) |
| 12 | static factory | `public static async Task<ConversationResolver> LoadAsync(IApplicationGlobals, MailItemHelper, CancellationTokenSource, CancellationToken, bool loadAll, System.Action<List<MailItemHelper>> updateUI = null)` | `:126-160` | 4 + 3 `await` continuations (`:147`, `:148`, `:149`) |
| 13 | static factory | `public static async Task<ConversationResolver> LoadAsync(IApplicationGlobals, IEnumerable<MailItem>, CancellationTokenSource, CancellationToken, System.Action<List<MailItemHelper>> updateUI = null)` | `:164-215` | 2 (`updateUI is not null`) + 4 `await` continuations (`:186-202` outer, `:190` inner `Task.Run`, `:192` inner `FromMailItemAsync`, `:211`); implicit throw path in `helpers.First()` at `:205` |
| 14 | instance method | `public async Task BackgroundInitInfoItemsAsync(CancellationToken token)` | `:217-233` | 1 implicit throw (`token.ThrowIfCancellationRequested()` `:219`) + 2 `await` continuations (`:226`, `:227`) |
| 15 | field + property | `private CancellationToken _token;` / `internal CancellationToken Token { get; set; }` | `:239-244` | 0 |
| 16 | field + property | `private CancellationTokenSource _tokenSource;` / `internal CancellationTokenSource TokenSource { get; set; }` | `:246-251` | 0 |
| 17 | field | `protected IApplicationGlobals _globals;` | `:253` | 0 |
| 18 | field + property | `protected MailItem _mailItem;` / `public MailItem Mail { get; protected set; }` | `:255-260` | 0 |
| 19 | field + property | `private bool _fullyLoaded = false;` / `public bool FullyLoaded { get; protected set; }` | `:262-267` | 0 |
| 20 | field + property | `protected System.Action<List<MailItemHelper>> _updateUI;` / `public System.Action<List<MailItemHelper>> UpdateUI { get; set; }` | `:269-279` | setter routes through `Initializer.SetAndSave(ref, value, notify)` (`:274-278`) — notification always fires |
| 21 | field + property | `protected MailItemHelper _mailInfo;` / `public MailItemHelper MailHelper { get; set; }` | `:281-286` | 0 |
| 22 | field + property | `protected object _parent;` / `public object Parent { get; protected internal set; }` | `:288-293` | 0 |
| 23 | field | `private int _uiPublishCount;` | `:295` | 0 (incremented in the sibling partial at `ConversationResolver.Loading.cs:143`) |
| 24 | obsolete method | `[Obsolete("Use LoadConversationInfoAsync instead", true)] internal async Task GetConversationInfoAsync()` | `:301-331` | 1 `await` (`:330`); **uninvocable** |
| 25 | obsolete method | `[Obsolete("Use LoadConversationInfoAsync instead", true)] internal async Task GetConversationInfoAsync(DataFrame df, CancellationToken token)` | `:333-354` | 1 `await` (`:353`); **uninvocable** |

**Irreducible finding (members 24 and 25).** Both carry `[Obsolete(message, error: true)]`. Under
C# semantics any call site is compile error CS0619, so no production code and no compiled test can
invoke them. Their bodies are one statement each (`await Task.CompletedTask;`) plus the method
prologue/epilogue — approximately 4–6 sequence points combined, all permanently unreachable except
via reflection. See §12 for the disposition.

Estimated executable-sequence-point denominator for this file: **≈ 100–110**. F1's harness supplies
the authoritative figure.

---

## 3. Existing test inventory

### 3.1 `QuickFiler.Test/Helper Classes/ConversationResolverTests.cs` (578 lines, 10 `[TestMethod]`)

`[TestClass] ConversationResolverTests`, namespace `QuickFiler.Test.HelperClasses` (`:32-33`).
`[TestInitialize] TestInitialize` (`:48-54`) builds `MockRepository(MockBehavior.Loose)`,
`Mock<IApplicationGlobals>`, `Mock<MailItem>`.

| Test method | Lines | Members of THIS file exercised |
| --- | --- | --- |
| `LoadConversationInfo_WhenCountExpandedIsZero_ReturnsSingleItemFallbackContainingMailHelper` | `:71-88` | #9 ctor, #21 `MailHelper` getter (null), `Count` setter (sibling partial) |
| `ConversationInfoGetter_WhenCountExpandedIsZero_ReturnsSingleItemFallback` | `:96-110` | #9 ctor, #21 getter |
| `Count_WhenZeroCountIsSetViaInternalSetter_SubsequentGetDoesNotInvokeLoadCount` | `:128-148` | #9 ctor |
| `Count_WhenNotYetInitialized_AttemptsToLoadCount` | `:159-181` | #9 ctor |
| `ConversationInfo_WhenNotSetAndCountIsZero_ReturnsFallbackWithoutThrowing` | `:212-227` | #9 ctor |
| `ConversationInfo_WhenSetBeforeAccessWithCountAtZero_ReturnsCachedValueWithoutThrowing` | `:238-266` | #9 ctor |
| `LoadDfAsync_ConsumesConversationSnapshotsWithoutRepeatedUiPublishes` | `:273-312` | #10 ctor (5-arg with `updateUI` defaulted), #4 logger, #5, #6, #7 (via `LogConversationResolverTiming` calls at `ConversationResolver.Loading.cs:237, 254`), #23 `_uiPublishCount` read |
| `LoadAsync_WithPreloadedHelperAndLoadAllTrue_ReusesHelperForSingleItemConversation` | `:314-349` | #12 `LoadAsync(helper, …)` **loadAll=true** branch (`:145-151`), #8 private ctor, #21, #18 `Mail` protected setter, #15/#16 `Token`/`TokenSource` setters |
| `LoadAsync_WithMailItemAndLoadAllTrue_LoadsConversationInfoAndItems` | `:351-377` | #11 `LoadAsync(mailItem, …)` **loadAll=true** branch (`:109-115`), #9 ctor, #15/#16 |
| `LoadAsync_WithPreloadedHelperAndLoadAllFalse_ReturnsResolverWithStagedDataFrame` | `:379-412` | #12 **loadAll=false** branch (`:152-157`), #8 private ctor |

Private helpers (not tests): `GetPropertyValue` `:414-421`, `GetDataFrameRowCount` `:423-440`,
`CreateConversationTable` `:442-482`, `CreateResolverGlobals` `:484-504`, `CreateMailItem`
`:506-562`, `GetPrivateField<T>` `:564-576`.

### 3.2 Sibling-owned test files that already cover members of THIS file

Per-file coverage counts every line executed by any test in the run, so these contribute:

| File (owner) | Lines | Members of THIS file exercised |
| --- | --- | --- |
| `QuickFiler.Test/Controllers/EfcDataModelTests.cs:24-49` (F5) | `CreateAsync_StagesSnapshotLoadBeforeBackgroundInitialization` | **#13 `LoadAsync(IEnumerable<MailItem>, …)`** — the only coverage of that overload anywhere; routed via `QuickFiler/Controllers/EfcDataModel.cs:115-121` |
| `EfcDataModelTests.cs:56-81, 120-149` (F5) | `CreateAsync_WithSingleSelectedMail_*` | #11 `LoadAsync(mailItem, …)` loadAll=false branch via `EfcDataModel.cs:125-132`; #22 `Parent` protected-internal setter via `EfcDataModel.cs:121, 132`; #19 `FullyLoaded` getter |
| `EfcDataModelTests.cs:84-118` (F5) | `LoadConversationInfoAsync_WhenGlobalsDoNotExposeOutlookApp_DoesNotRequireApp` | #9 ctor, #21 `MailHelper` setter |
| `EfcDataModelTests.cs:152-218` (F5) | `Constructor_WhenMailProvided_*` | #10 ctor (4-arg form) via `EfcDataModel.cs:66` |
| `QuickFiler.Test/Controllers/QfcItemController.*Tests.cs` (F10) | multiple | #9 ctor only (resolvers are constructed as fixtures, not exercised) |

**`MailItemInfoTests.cs` contributes nothing to this file** — see artifact 08 §3.

---

## 4. Per-member coverage gap

| # | Member | Status |
| --- | --- | --- |
| 1 | `Pair<T>` ctor | covered (`ConversationResolverTests.cs:78, 249-252`) |
| 2 | `Pair<T>.SameFolder` get/set | get covered; **set uncovered** (no test assigns the property after construction; `LoadCount` at `ConversationResolver.Loading.cs:279` does, and is reached only through the throwing path in `Count_WhenNotYetInitialized_AttemptsToLoadCount`) |
| 3 | `Pair<T>.Expanded` get/set | get covered; **set uncovered** (same reasoning; `Loading.cs:283`) |
| 4 | `logger` static init | covered |
| 5 | `DescribeSynchronizationContext` | partially covered (branches missed: the **non-null** `syncContext` arm — MSTest worker threads normally have `SynchronizationContext.Current == null`, so only the `?? "null"` arm is deterministic today) |
| 6 | `BuildConversationResolverTimingContext` | covered |
| 7 | `LogConversationResolverTiming` | partially covered (branches missed: `details` non-empty arm is covered by `Loading.cs:255-257`; `phase.StartsWith("[Conversation resolver timing]")` **true** arm is covered by `Loading.cs:238`; the `details` **null/whitespace** arm is covered by `:222`. Coverage here is incidental and unasserted — no test names this member) |
| 8 | private ctor `:62` | covered |
| 9 | ctor `(globals, mailItem)` | covered |
| 10 | ctor 5-arg | covered (4-arg call form only; the `updateUI` non-default argument is never supplied) |
| 11 | `LoadAsync(mailItem, …)` | partially covered (branches missed: `updateUI is not null` **true** arm `:100`; loadAll=false arm `:116-121` is covered only indirectly through `EfcDataModel`, not by any test that names this method) |
| 12 | `LoadAsync(helper, …)` | partially covered (branches missed: `updateUI is not null` **true** arm `:143`) |
| 13 | `LoadAsync(IEnumerable<MailItem>, …)` | partially covered indirectly by F5's `EfcDataModelTests.cs:24-49` (branches missed: `updateUI is not null` **true** arm `:178`; the empty-`mailItems` error path at `:205` `helpers.First()`) |
| 14 | `BackgroundInitInfoItemsAsync` | **uncovered deterministically.** It is reached only from `Handler_PropertyChanged` (`ConversationResolver.Loading.cs:311`), which is `async void` and is never triggered by any existing test (see artifact 06 §4). No test calls it directly. Branch missed: the cancelled-token throw at `:219` |
| 15 | `Token` get/set | covered |
| 16 | `TokenSource` get/set | covered |
| 17 | `_globals` | covered |
| 18 | `Mail` get/protected set | covered |
| 19 | `FullyLoaded` get | covered (F5); **protected setter uncovered** (only written from `Handler_PropertyChanged`, `Loading.cs:308, 312`) |
| 20 | `UpdateUI` get/set | **setter uncovered** — no test assigns `UpdateUI`; the `NotifyPropertyChanged(nameof(UpdateUI))` callback at `:277` is therefore never executed |
| 21 | `MailHelper` get/set | covered |
| 22 | `Parent` get/protected-internal set | covered (F5) |
| 23 | `_uiPublishCount` | read-covered; incremented only in the sibling partial |
| 24 | `GetConversationInfoAsync()` | **uncoverable** (`[Obsolete(…, true)]`) |
| 25 | `GetConversationInfoAsync(DataFrame, CancellationToken)` | **uncoverable** (`[Obsolete(…, true)]`) |

---

## 5. Testability classification per member

| # | Member | Classification | Interop / host touch |
| --- | --- | --- | --- |
| 1–3 | `Pair<T>` | `pure-testable-now` | none |
| 4 | `logger` | `pure-testable-now` | log4net static; already exercised |
| 5 | `DescribeSynchronizationContext` | `pure-testable-now` (private static; reachable by making the caller run, or by asserting the composed log string; a test may install a `SynchronizationContext` on the current thread with `SynchronizationContext.SetSynchronizationContext(new SynchronizationContext())` to hit the non-null arm) | none |
| 6 | `BuildConversationResolverTimingContext` | `pure-testable-now` | none |
| 7 | `LogConversationResolverTiming` | `pure-testable-now` | none |
| 8–9 | ctors | `pure-testable-now` — proven at `ConversationResolverTests.cs:75` | stores a `MailItem` reference without dereferencing it |
| 10 | 5-arg ctor | `pure-testable-now` — proven at `ConversationResolverTests.cs:285-290` | `new MailItemHelper(mailItem, _globals)` at `:82` calls `MailItemHelper.InitLazyFields` (`UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs:92-112`) which only *creates* `Lazy<>` closures; **no COM member is touched at construction time** |
| 11–12 | `LoadAsync` (mailItem / helper) | `pure-testable-now` — proven at `ConversationResolverTests.cs:337, 367`. Interop touched: `MailItem.GetConversation()`, `Conversation.GetTable()`, `MailItem.Parent` → `Folder.Name`/`Folder.FolderPath`, plus the property graph in `CreateMailItem` (`:506-562`). All are COM **interfaces** and are mocked directly with Moq per `00-cluster-overview.md` §3 | `MailItem`, `Conversation`, `Table`, `Columns`, `Row`, `Column`, `Folder` |
| 13 | `LoadAsync(IEnumerable<MailItem>)` | `pure-testable-now` — the same mock graph plus `MailItemHelper.FromMailItemAsync` per item. Precedent for the multi-item shape: `EfcDataModelTests.cs:24-49` | same |
| 14 | `BackgroundInitInfoItemsAsync` | `needs-seam` — the method itself is directly awaitable and therefore deterministic, but reaching its post-`LoadConversationInfoAsync` continuation requires a UI-dispatch seam when `UpdateUI` is non-null (`ConversationResolver.Loading.cs:150` calls the static `UiThread.Dispatcher`, which is `null` in a unit-test process — see §7 S1) | none directly; transitively `Folder.Name` |
| 15–23 | properties / fields | `pure-testable-now` | none |
| 24–25 | obsolete methods | `host-bound-irreducible` — not host-bound but **compiler-bound**: `[Obsolete(…, true)]` makes every call site CS0619 | none |

No member of this file requires a live WinForms control, a message pump, or the UI thread. See §11.

---

## 6. Ordering and async invariants (load-bearing)

The invariants below are the observable contract this file must preserve. Each is stated with its
evidence and with the deterministic test technique. **No test may use `Thread.Sleep`, `Task.Delay`,
`SpinWait.SpinUntil` with a wall-clock budget, or any real wall-clock wait** (`.claude/rules/csharp.md:79`,
`.claude/rules/general-unit-test.md` § Determinism Infrastructure, `BannedSymbols.txt:4-7`).

### INV-1 — `LoadAsync` subscribes `Handler_PropertyChanged` LAST, after `LoadDfAsync`

Evidence: `:114`, `:120`, `:150`, `:156`, `:213`, and the explanatory comments at `:118` and `:154`
("Subscribe after LoadDfAsync so initial dataframe assignment does not trigger background
initialization"). The `Df` setter unconditionally raises `PropertyChanged("Df")`
(`ConversationResolver.Loading.cs:205` → `Initializer.SetAndSave` → `UtilitiesCS/HelperClasses/Initializer.cs:95-101`),
and `Handler_PropertyChanged` reacts to exactly that property name (`Loading.cs:306`). Subscribing
before the assignment would start a background load during construction.

Deterministic test: construct via `LoadAsync`, attach a recording handler **before** calling, assert
the recording handler observed `"Df"` while `FullyLoaded` remained `false` and
`ConversationInfo`/`ConversationItems` were not populated by a second, resolver-internal pass.
Equivalent and simpler: assert that immediately after `LoadAsync(..., loadAll: false)` returns,
`FullyLoaded == false`. No timing primitive is required because `LoadAsync` is awaited.

### INV-2 — `loadAll == true` performs Df → ConversationInfo → ConversationItems strictly in that order

Evidence: `:111-113` and `:147-149`. `LoadConversationInfoAsync` reads `Count.Expanded` and
`Df.Expanded` (`Loading.cs:88, 91`), which is only meaningful after `LoadDfAsync` has assigned `Df`;
`LoadConversationItemsAsync` reads `ConversationInfo` (`Loading.cs:180-181`), which is only
meaningful after `LoadConversationInfoAsync` assigned it (`Loading.cs:138`).

Deterministic test: attach a `PropertyChanged` recorder before invoking, then assert the recorded
property-name sequence is `["Df", "ConversationInfo", "ConversationItems"]` (the `Df` notification is
raised inside `LoadDfAsync`; the recorder must be attached to the resolver, which for the static
factory means recording after construction — hence the seam in §7 S4, or the equivalent test that
constructs the resolver with a public constructor and drives the three loaders in order).

### INV-3 — `loadAll == false` performs Df ONLY; conversation info and items stay lazy

Evidence: `:119` and `:155`. Proven at `ConversationResolverTests.cs:379-412` for the helper
overload; **not proven for the `MailItem` overload** (`:116-121`) by any test that names it.

Deterministic test: after `await LoadAsync(..., loadAll: false)`, assert `Df.Expanded` is non-null
and the private backing fields `_convInfoFields` / `_conversationItems` are still `default` (reflection
helper `GetPrivateField<T>` already exists at `ConversationResolverTests.cs:564-576`).

### INV-4 — `LoadAsync(IEnumerable<MailItem>)` sets `ConversationInfo` and `Count` from the SAME materialized list

Evidence: `:207-212`. `ConversationInfo` is assigned `Pair(sameFolder: helpers, expanded: helpers)` —
the *same* list instance in both slots — then `LoadConversationItemsAsync` runs, then
`Count = Pair(helpers.Count, helpers.Count)`. Note that `Count` is assigned **after**
`LoadConversationItemsAsync`, so any consumer reading `Count` during that await observes the
uninitialized `(-1,-1)` sentinel.

Deterministic test: pass two mocked `MailItem`s, assert
`ReferenceEquals(resolver.ConversationInfo.SameFolder, resolver.ConversationInfo.Expanded)` and
`resolver.Count.Expanded == 2`.

### INV-5 — helper ordering inside `LoadAsync(IEnumerable<MailItem>)` preserves input order

Evidence: `:186-202` — `ToAsyncEnumerable().SelectAwaitWithCancellation(...).ToListAsync()`. The
`System.Linq.Async` `SelectAwait*` operator is order-preserving even though each element's projection
runs on a `Task.Run` (`:190`). `resolver.MailHelper = helpers.First()` (`:205`) therefore binds to the
**first input mail item**, which is exactly what `EfcDataModel.CreateAsync` relies on
(`QuickFiler/Controllers/EfcDataModel.cs:106` takes `mailSelectionSnapshot[0]` as `Mail`, and
`EfcDataModelTests.cs:45` asserts `dataModel.Mail.Should().BeSameAs(firstMail.Object)`).

Deterministic test: supply three mocks with distinct `EntryID`s and assert
`resolver.ConversationInfo.Expanded.Select(x => x.EntryId)` equals the input order. This is a pure
ordering assertion with no timing dependency; the awaited `ToListAsync()` is the synchronization point.

### INV-6 — the `#pragma warning disable CS0618` suppression at `:185-203` is intentional and narrow

Evidence: the comment block at `:180-184` records that migrating off the obsolete
`SelectAwaitWithCancellation` overload would be a call-shape change to production code and is
deliberately not done ("preserves the exact pre-existing behavior (no behavior change per AC7)").
F4 must not widen or remove this suppression.

### INV-7 — `BackgroundInitInfoItemsAsync` is cancellation-first

Evidence: `:219` `token.ThrowIfCancellationRequested()` executes before the stopwatch and before
either loader. Deterministic test: pass an already-cancelled `CancellationToken` and assert
`OperationCanceledException`; assert no `PropertyChanged` notification was raised.

### INV-8 — `UpdateUI` assignment raises `PropertyChanged("UpdateUI")` unconditionally

Evidence: `:273-278` → `Initializer.SetAndSave(ref _updateUI, value, x => NotifyPropertyChanged(nameof(UpdateUI)))`
→ `Initializer.cs:52-56` → `:95-101`, whose only guard is `DependenciesNotNull(false, objectSetter)`
on the **callback**, which is always non-null here. Assigning `null` to `UpdateUI` therefore still
notifies. Deterministic test: assign `null` and assert the notification fired with property name
`"UpdateUI"`.

This invariant is load-bearing because `Handler_PropertyChanged` reacts to `"UpdateUI"`
(`Loading.cs:316-324`) and, when `FullyLoaded`, immediately republishes to the UI. That is the
production path `QuickFiler/Controllers/EfcItemController.cs:314` takes.

### INV-9 — banned-API audit of this file: CLEAN

A grep of `QuickFiler/Helper Classes/` for `DateTime.Now`, `DateTime.UtcNow`, `Thread.Sleep`,
`Task.Delay`, `Random.Shared` returns **no match**. This file therefore introduces **no banned-API
finding** and needs **no `TimeProvider` injection**.

Two `System.Diagnostics.Stopwatch` uses exist — `:221` `Stopwatch.StartNew()` and `:231`
`ElapsedMilliseconds`. `Stopwatch` is **not** on `BannedSymbols.txt` (`:1-7`), and both uses are
confined to a log-message interpolation; **no control-flow decision reads elapsed time**, so they
introduce no test nondeterminism and require no clock seam. Recorded as an observation, not a finding.

One latent wall-clock dependency exists **outside** F4's file set and is reported for awareness only:
`LoadDfAsync` (`Loading.cs:242`) calls `MailItem.GetConversationDfAsync(token)`, which internally uses
`TimeOutTask.RunWithTimeout(..., 1000, 3, false)` at
`UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.cs:252-258`. That 1000 ms budget is
UtilitiesCS-owned, is not reached when the mock returns synchronously, and must not be edited by F4.

---

## 7. Seam proposal (ranked per epic.md Shared Design §2: interface > delegate > adapter)

### S1 (interface seam, tier 1) — UI dispatch: REUSE `UtilitiesCS.Threading.IUiDispatcher`

- Problem it solves: `ConversationResolver.Loading.cs:150` and `:320` call the static
  `UiThread.Dispatcher.InvokeAsync(...)`. `UiThread.Dispatcher` is a static property backed by
  `private static Dispatcher _dispatcher = null!` (`UtilitiesCS/Threading/UiThread.cs:135-140`) that is
  assigned **only** by `UiThread.Initialize()`, which constructs and `Show()`s a hidden WinForms
  `SyncContextForm` (`UiThread.cs:48-79`). In a unit-test process the getter returns `null` and the
  call throws `NullReferenceException`. Initializing it would create a live form, which epic.md
  Shared Design §2 forbids.
- Do **not** invent a new abstraction. `UtilitiesCS/Threading/IUiDispatcher.cs:15` already declares
  exactly the needed member `Task InvokeAsync(Action action)` (`:21`), with the production adapter
  `UtilitiesCS/Threading/WpfUiDispatcher.cs:17`.
- **Production default is safe to construct in a test process**: `WpfUiDispatcher()` is
  `: this(() => UiThread.Dispatcher)` (`WpfUiDispatcher.cs:24-25`) — a *lambda*, evaluated lazily, so
  constructing the adapter touches nothing. Already proven by
  `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs:26`.
- Proposed injection point (declared in **this** file, `ConversationResolver.cs`, in the Properties
  region near `:295`):

  ```
  internal UtilitiesCS.Threading.IUiDispatcher UiDispatcher { get; set; }
      = new UtilitiesCS.Threading.WpfUiDispatcher();
  ```

  Consumed by replacing `UiThread.Dispatcher.InvokeAsync(...)` at `Loading.cs:150` and `:320` with
  `UiDispatcher.InvokeAsync(...)`.
- Precedent inside F4: `QuickFiler/Helper Classes/EmailMoveMonitor.cs:38-40` uses the same
  default-to-real-implementation shape for its marshal delegate. Precedent for the property form:
  `QuickFiler/Controllers/QfcHomeController.Metrics.cs:13-17`.
- **Requires no sibling-owned file change.** No constructor signature changes; no interface member is
  added to `IConversationResolver`; every existing call site compiles unchanged.

### S2 (injectable delegate, tier 2) — `MailItemHelper` projection factories

- Problem: `LoadConversationInfo` (`Loading.cs:61`) and `LoadConversationInfoAsync`
  (`Loading.cs:98-104`) call the static `MailItemHelper.FromDf` / `FromDfAsync`
  (`UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Loading.cs:18, 88`), which dereference
  `appGlobals.Ol.NamespaceMAPI` and resolve a live item by EntryID. The strict `IOlObjects` mock in
  `ConversationResolverTests.CreateResolverGlobals` (`:490-500`) does not stub `NamespaceMAPI`, so the
  multi-row path is currently unreachable.
- Proposal (declared in `ConversationResolver.cs`, defaults preserve exact current behaviour):

  ```
  internal Func<DataFrame, long, IApplicationGlobals, CancellationToken, MailItemHelper> HelperFromDf
      { get; set; } = MailItemHelper.FromDf;
  internal Func<DataFrame, long, IApplicationGlobals, CancellationToken, bool, Task<MailItemHelper>> HelperFromDfAsync
      { get; set; } = MailItemHelper.FromDfAsync;
  ```

- **Requires no sibling-owned file change.**
- Alternative if the plan prefers zero new members: stub `IOlObjects.NamespaceMAPI` with a
  `Mock<NameSpace>` whose `GetItemFromID` returns a `Mock<MailItem>`. This is achievable with the
  established Moq-on-Interop pattern (`00-cluster-overview.md` §3.3) but produces a much larger
  arrange block per test and couples F4's tests to `MailItemHelper` internals. The delegate seam is
  preferred.

### S3 (no seam required) — `Task.Run` inside `LoadConversationItemsAsync`

`Loading.cs:194` uses `await Task.Run(...)`. `Task.Run` is not banned (only `Task.Delay` is,
`BannedSymbols.txt:6-7`) and awaiting the returned `Task` is a hard synchronization point. No seam.

### S4 (see artifact 06) — async-void observability for `Handler_PropertyChanged`

Fully specified in artifact 06 §7; it is the seam that makes INV-2 and member #14
(`BackgroundInitInfoItemsAsync`) deterministically observable through the event path.

### Rejected alternatives (brief)

- **New `IConversationResolverClock` / `TimeProvider` injection**: rejected. §6 INV-9 shows no
  time-dependent control flow exists in this file, so a clock seam would add surface with no coverage
  benefit.
- **Making `Handler_PropertyChanged` return `Task`**: rejected. It is an
  `INotifyPropertyChanged` event-handler method named in `IConversationResolver:25` and subscribed by
  method group at `QuickFiler/Controllers/EfcDataModel.cs:69` and
  `QuickFiler/Controllers/EfcItemController.cs:667` (both sibling-owned). Changing the return type
  breaks the delegate conversion and forces sibling edits.
- **Initializing `UiThread` in `[AssemblyInitialize]`**: rejected. `UiThread.Initialize` shows a
  WinForms form (`UiThread.cs:51-54`), violating epic.md Shared Design §2 and UT4.

---

## 8. Cross-child conflict analysis

F4's production file set is the 13 files under `QuickFiler/Helper Classes/` plus
`QuickFiler/Interfaces/IEmailMoveMonitor.cs` (epic.md `:276-283`). **Every** other file below is
owned by a sibling child running in parallel.

### 8.1 Production call sites of `ConversationResolver` outside F4

| File:line | Call | Owner |
| --- | --- | --- |
| `QuickFiler/Controllers/EfcDataModel.cs:66` | `new ConversationResolver(Globals, Mail, TokenSource, Token)` — 4 positional args into member #10 | F5 |
| `QuickFiler/Controllers/EfcDataModel.cs:67` | `_conversationResolver.Df = _conversationResolver.LoadDf();` — consumes the **`internal`** `LoadDf()` (`Loading.cs:208`) | F5 |
| `QuickFiler/Controllers/EfcDataModel.cs:68-69` | `PropertyChanged += Handler_PropertyChanged` (method group) | F5 |
| `QuickFiler/Controllers/EfcDataModel.cs:115-121` | `await ConversationResolver.LoadAsync(globals, mailSelectionSnapshot, tokenSource, token)` — member #13; then `Parent = dataModel` (member #22) | F5 |
| `QuickFiler/Controllers/EfcDataModel.cs:125-132` | `await ConversationResolver.LoadAsync(globals, mailSelectionSnapshot[0], tokenSource, token, loadAll)` — member #11; then `Parent = dataModel` | F5 |
| `QuickFiler/Controllers/EfcDataModel.cs:214-215, 232, 273` | field/property of type `ConversationResolver`; `MailInfo => ConversationResolver?.MailHelper`; `ConversationInfo.SameFolder` | F5 |
| `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:69` | `DataModel.ConversationResolver.ConversationInfo.SameFolder` | F8 |
| `QuickFiler/Controllers/EfcItemController.cs:310, 314, 315` | null guard; `UpdateUI = SetTopicThread` (member #20 setter); `Count.SameFolder` | F9 |
| `QuickFiler/Controllers/EfcItemController.cs:666-668` | `PropertyChanged += new PropertyChangedEventHandler(ConversationResolverPropertyChanged)` | F9 |
| `QuickFiler/Controllers/EfcItemController.cs:741-751, 1103` | consumes `ConversationInfo.Expanded` | F9 |
| `QuickFiler/Controllers/QfcCollectionController.cs:535, 1809, 1877` | `ConversationResolver` parameter type | F11 |
| `QuickFiler/Controllers/QfcItemController.cs:69, 109, 110` | `Func<MailItem, ConversationResolver>` field; `ConversationResolver` property | F10 |
| `QuickFiler/Controllers/QfcItemController.Conversation.cs:34, 40-43, 46-57, 80-85, 100-121, 126-135, 175` | factory call, `PopulateConversation(ConversationResolver)`, `LoadAsync` seam override, `Count.SameFolder`, `ConversationInfo.Expanded` | F10 |
| `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:294, 296` | `Count.Expanded`, `ConversationInfo.Expanded` | F10 |
| `QuickFiler/Controllers/QfcItemController.Initialization.cs:41, 382-388` | `Func<MailItem, ConversationResolver>` ctor parameter; default factory calls member #10 with **5 positional args** | F10 |
| `QuickFiler/Controllers/QfcItemController.MailActions.cs:42, 44, 163` | passes the resolver; `Count.SameFolder`; `ConversationInfo.SameFolder` | F10 |
| `QuickFiler/Interfaces/IQfcItemController.cs:69` | `void PopulateConversation(ConversationResolver resolver);` | F10 |
| `QuickFiler/Interfaces/IQfcCollectionController.cs:82` | `ConversationResolver resolver` parameter | F11 |

### 8.2 Test call sites outside `QuickFiler.Test/Helper Classes/`

`QuickFiler.Test/Controllers/EfcDataModelTests.cs` (F5, `:47, 56, 79-80, 104, 145, 148, 174, 177, 208-209`);
`EfcHomeControllerExecuteMovesTests.cs:212` (F8);
`QfcItemControllerTests.cs:33-46, 58-163` (F10);
`QfcItemController.ConversationTests.cs:29-37, 62-120, 179-206, 299-303` (F10);
`QfcItemController.SeamFactoryTests.cs:42-64` (F10);
`QfcItemController.SeamDispatcherTests.cs:165` (F10);
`QfcItemController.PropertiesTests.cs:113` (F10);
`QfcItemController.MailActionsTests.cs:97-101` (F10).
Inside F4 but owned by the theme-cluster researcher:
`QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:371, 418`.

### 8.3 Conflict verdict for every proposed seam

| Seam | Verdict |
| --- | --- |
| S1 `internal IUiDispatcher UiDispatcher { get; set; }` new property + two call-site rewrites inside `ConversationResolver.Loading.cs` | **Requires no sibling-owned file change.** Additive member; no signature altered. |
| S2 `HelperFromDf` / `HelperFromDfAsync` delegate properties | **Requires no sibling-owned file change.** Additive internal members with static-method-group defaults. |
| S4 extract `HandlePropertyChangedCoreAsync` (artifact 06) | **Requires no sibling-owned file change.** `Handler_PropertyChanged`'s signature is preserved for the method-group subscriptions at `EfcDataModel.cs:69` and `EfcItemController.cs:667`. |
| Any change to the **positional shape** of ctor #10 | **Would require editing `QuickFiler/Controllers/EfcDataModel.cs:66` (F5) and `QuickFiler/Controllers/QfcItemController.Initialization.cs:382-388` (F10).** PROHIBITED. New parameters must be appended with defaults only. |
| Any change to `internal Pair<DataFrame> LoadDf()` | **Would require editing `QuickFiler/Controllers/EfcDataModel.cs:67` (F5).** PROHIBITED. Keep the signature. |
| Any new member added to `IConversationResolver` | safe today (single implementer, zero consumers — artifact 07 §3) but **still not recommended**; keep seams on the concrete class. |

**Additional coordination note.** `EfcDataModelTests.cs:144-147` and `:173-176` use
`SpinWait.SpinUntil(..., 250)` — a real wall-clock budget in a sibling-owned test file. F4 must not
copy that pattern and must not edit that file. It is an existing determinism defect in F5's
territory; if the plan wants it fixed, promote it as a separate issue rather than editing across the
child boundary.

---

## 9. 500-line compliance

| File | Now | After proposed seams | Limit |
| --- | --- | --- | --- |
| `QuickFiler/Helper Classes/ConversationResolver.cs` | 358 | ≈ 375 (S1 property ≈ 6 lines with XML doc; S2 two delegate properties ≈ 12 lines) | 500 — **compliant, 125 lines of headroom** |

Confirmed: this file has headroom and requires no partial split. If the plan additionally deletes the
two `[Obsolete(…, true)]` methods (§12), the file shrinks by ~56 lines to ≈ 319.

**No new production file is proposed for this cluster.** Should one become necessary, it would require
a `<Compile Include=...>` line in `QuickFiler/QuickFiler.csproj` inside the `Helper Classes\` block
(`:342-354`) — a shared-file conflict risk with all thirteen siblings.

Test-side: each new test file requires a `<Compile Include=...>` entry in
`QuickFiler.Test/QuickFiler.Test.csproj` inside the existing contiguous `Helper Classes\` block
(`:158-165`), inserted in alphabetical order per `00-cluster-overview.md` §1.3. Same shared-file
conflict class.

---

## 10. Recommended test cases (one per line; each becomes its own atomic plan task)

Framework: MSTest + Moq + FluentAssertions. All destinations are new files under
`QuickFiler.Test/Helper Classes/`. **None of the tests below duplicates an existing test**; the
exclusions are listed at the end of this section.

Destination A = `QuickFiler.Test/Helper Classes/PairTests.cs` (namespace `QuickFiler.Test.HelperClasses`).
Destination B = `QuickFiler.Test/Helper Classes/ConversationResolverLifecycleTests.cs` (namespace `QuickFiler.Test.HelperClasses`).

| # | `[TestMethod]` name | Arrange / Act / Assert | Category | Destination |
| --- | --- | --- | --- | --- |
| T01 | `Pair_Constructor_AssignsSameFolderAndExpandedInOrder` | Arrange two distinct sentinel strings; Act `new Pair<string>("a","b")`; Assert `SameFolder=="a"` and `Expanded=="b"` (guards against a constructor-argument swap) | positive | A |
| T02 | `Pair_SameFolderSetter_ReplacesValue` | Arrange a `Pair<int>(1,2)`; Act assign `SameFolder = 9`; Assert `SameFolder==9` and `Expanded==2` | positive | A |
| T03 | `Pair_ExpandedSetter_ReplacesValue` | Arrange a `Pair<int>(1,2)`; Act assign `Expanded = 9`; Assert `Expanded==9` and `SameFolder==1` | positive | A |
| T04 | `Pair_DefaultInstance_HasDefaultMembersForReferenceType` | Arrange nothing; Act `default(Pair<List<MailItemHelper>>)`; Assert both members are `null` (documents the sentinel semantics `Initializer.GetOrLoad` depends on) | boundary | A |
| T05 | `Pair_IsValueType_CopiesOnAssignment` | Arrange `var a = new Pair<int>(1,2); var b = a;` Act mutate `b.SameFolder = 7`; Assert `a.SameFolder == 1` (value-type copy semantics are load-bearing for the `Count` sentinel) | boundary | A |
| T06 | `Constructor_WithGlobalsAndMailItem_DoesNotTouchAnyComMember` | Arrange `Mock<MailItem>(MockBehavior.Strict)` with **no** setups and a strict `Mock<IApplicationGlobals>`; Act construct member #9; Assert `Mail` is the mock instance and `mockMailItem.VerifyNoOtherCalls()` | positive | B |
| T07 | `Constructor_FiveArg_BuildsMailHelperWithoutTouchingComMembers` | Arrange strict `Mock<MailItem>` with no setups; Act construct member #10; Assert `MailHelper` non-null, `Token`/`TokenSource` round-trip, `mockMailItem.VerifyNoOtherCalls()` (locks the `Lazy<>`-only construction contract at `MailItemHelper.cs:92-112`) | positive | B |
| T08 | `Constructor_FiveArg_WithUpdateUiSupplied_StoresDelegateWithoutInvokingIt` | Arrange a counting `Action<List<MailItemHelper>>`; Act construct member #10 with it; Assert `UpdateUI` is the same delegate and the counter is 0 | positive | B |
| T09 | `UpdateUISetter_WhenAssigned_RaisesPropertyChangedForUpdateUi` | Arrange resolver + `PropertyChanged` recorder; Act assign a delegate to `UpdateUI`; Assert exactly one notification with `PropertyName == "UpdateUI"` (INV-8; member #20, currently uncovered) | positive | B |
| T10 | `UpdateUISetter_WhenAssignedNull_StillRaisesPropertyChanged` | Arrange resolver + recorder; Act assign `null`; Assert a notification with `PropertyName == "UpdateUI"` fired (documents that `Initializer.SetAndSave` guards only the callback, `Initializer.cs:95-101`) | boundary | B |
| T11 | `MailHelperSetter_DoesNotRaisePropertyChanged` | Arrange resolver + recorder; Act assign a `new MailItemHelper()`; Assert zero notifications (member #21 has a plain setter — locks the asymmetry with `UpdateUI`) | boundary | B |
| T12 | `LoadAsync_WithMailItemAndLoadAllFalse_LoadsDataFrameOnlyAndLeavesConversationInfoLazy` | Arrange the `CreateMailItem`/`CreateConversationTable`/`CreateResolverGlobals` graph; Act `await LoadAsync(globals, mailItem, cts, token, loadAll: false)`; Assert `Df.Expanded` non-null and private `_convInfoFields` is `default` via reflection (INV-3 for member #11's uncovered `else` arm) | positive | B |
| T13 | `LoadAsync_WithMailItemAndUpdateUiSupplied_AssignsUpdateUiBeforeLoading` | Arrange as T12 plus a recording `updateUI`; Act `await LoadAsync(..., loadAll: false, updateUI: recorder)`; Assert `resolver.UpdateUI` is the recorder (covers the `updateUI is not null` **true** arm at `:99-100`, currently uncovered) | positive | B |
| T14 | `LoadAsync_WithPreloadedHelperAndUpdateUiSupplied_AssignsUpdateUi` | as T13 but through member #12 (`:142-143`) | positive | B |
| T15 | `LoadAsync_WithMailItemCollectionAndUpdateUiSupplied_AssignsUpdateUi` | as T13 but through member #13 (`:177-178`) | positive | B |
| T16 | `LoadAsync_WithMailItemCollection_PreservesInputOrderAndBindsFirstItemAsMailHelper` | Arrange three mocked `MailItem`s with EntryIDs `e1,e2,e3`; Act `await LoadAsync(globals, items, cts, token)`; Assert `ConversationInfo.Expanded` EntryIds equal `[e1,e2,e3]` and `MailHelper.EntryId == "e1"` (INV-5) | positive | B |
| T17 | `LoadAsync_WithMailItemCollection_SharesOneListInstanceBetweenSameFolderAndExpanded` | Arrange two mocks; Act as T16; Assert `ReferenceEquals(ConversationInfo.SameFolder, ConversationInfo.Expanded)` (INV-4) | boundary | B |
| T18 | `LoadAsync_WithMailItemCollection_SetsCountFromMaterializedHelperList` | Arrange two mocks; Act as T16; Assert `Count.SameFolder == 2 && Count.Expanded == 2` (INV-4, `:212`) | positive | B |
| T19 | `LoadAsync_WithEmptyMailItemCollection_ThrowsInvalidOperationException` | Arrange an empty `MailItem[]`; Act/Assert `await` throws `InvalidOperationException` from `helpers.First()` at `:205` | invalid-input | B |
| T20 | `LoadAsync_WithNullMailItemCollection_ThrowsArgumentNullException` | Arrange `null` for `mailItems`; Act/Assert `await` throws (`ToAsyncEnumerable` on null, `:186`) | invalid-input | B |
| T21 | `LoadAsync_WithCancelledToken_PropagatesOperationCanceledException` | Arrange a pre-cancelled `CancellationTokenSource`; Act `await LoadAsync(globals, mailItem, cts, cts.Token, loadAll: true)`; Assert `OperationCanceledException` (`LoadDfAsync` guards at `Loading.cs:233`) | error-handling | B |
| T22 | `LoadAsync_SubscribesPropertyChangedHandlerOnlyAfterDataFrameAssignment` | Arrange as T12 with an external recorder attached to `PropertyChanged` immediately after `await`; Act assert `FullyLoaded == false` and no `ConversationInfo` notification was observed during the load; Assert the resolver's own handler is registered (reflection on the `PropertyChanged` delegate invocation list, technique precedented at `ConversationResolverTests.cs:564-576`) | positive | B |
| T23 | `BackgroundInitInfoItemsAsync_WhenTokenAlreadyCancelled_ThrowsBeforeAnyLoading` | Arrange resolver with a stubbed `Count`; Act `await BackgroundInitInfoItemsAsync(cancelledToken)`; Assert `OperationCanceledException` and zero `PropertyChanged` notifications (INV-7, member #14 currently uncovered) | error-handling | B |
| T24 | `BackgroundInitInfoItemsAsync_WithZeroRowDataFrame_PopulatesConversationInfoThenItems` | Arrange resolver with `Count = (0,0)`, `MailHelper = new MailItemHelper{EntryId="e1",FolderName="Inbox"}`, mocked `Folder.Name=="Inbox"`, S1 dispatcher mock; Act `await BackgroundInitInfoItemsAsync(CancellationToken.None)`; Assert the recorded notification order is `["ConversationInfo","ConversationItems"]` (INV-2 tail; member #14 positive path) | positive | B |
| T25 | `BackgroundInitInfoItemsAsync_WhenUpdateUiSupplied_DispatchesExactlyOnePublish` | Arrange as T24 plus `UpdateUI` and a `Mock<IUiDispatcher>` executing the action inline; Act await; Assert `dispatcher.Verify(d => d.InvokeAsync(It.IsAny<Action>()), Times.Once)` and the private `_uiPublishCount == 1` | positive | B |
| T26 | `LogConversationResolverTiming_WhenPhaseAlreadyPrefixed_DoesNotDoublePrefix` | Arrange a log4net memory appender (or invoke the private static via reflection and capture the composed string); Act call with a phase beginning `"[Conversation resolver timing]"`; Assert the prefix appears exactly once (member #7, `:49-54` true arm) | boundary | B |
| T27 | `LogConversationResolverTiming_WhenDetailsWhitespace_OmitsDetailSegment` | as T26 with `details = "   "`; Assert the composed message contains no `" | "` detail segment (member #7, `:48` true arm) | boundary | B |
| T28 | `DescribeSynchronizationContext_WhenContextPresent_ReturnsFullTypeName` | Arrange `SynchronizationContext.SetSynchronizationContext(new SynchronizationContext())` for the duration of the test, restoring it in a `finally`; Act invoke the composed timing context; Assert the string contains `System.Threading.SynchronizationContext` (member #5 non-null arm, currently uncovered) | boundary | B |
| T29 | `DescribeSynchronizationContext_WhenNoContext_ReturnsLiteralNull` | Arrange explicit `SetSynchronizationContext(null)`; Act as T28; Assert the string contains `syncContext=null` (member #5 null arm) | boundary | B |

**Count for this file: 29 enumerated test cases** (5 for `Pair<T>`, 24 for `ConversationResolver`).
Category spread: positive 13, invalid-input 2, boundary 10, error-handling 4 — all four categories
covered, satisfying the issue's AC5.

**Excluded as duplicates of existing coverage** (do not re-author):
`LoadAsync` with a preloaded helper and `loadAll: true` (already `ConversationResolverTests.cs:314-349`);
`LoadAsync` with a `MailItem` and `loadAll: true` (already `:351-377`);
`LoadAsync` with a preloaded helper and `loadAll: false` (already `:379-412`);
`Count` sentinel behaviour for `(0,0)` and `(-1,-1)` (already `:128-148` and `:159-181`);
`ConversationInfo` fallback when `Count.Expanded == 0` (already `:71-88`, `:96-110`, `:212-227`);
`ConversationInfo` cached-read-after-assignment (already `:238-266`);
`LoadDfAsync` snapshot/no-repeat-publish contract (already `:273-312`).

---

## 11. STA determination

**No member of this file requires an STA test.** The determination:

- The file constructs no WinForms control, derives from no `Form`, and touches no `Control` member.
- The only host coupling is the static `UiThread.Dispatcher` reached from the sibling partial
  (`Loading.cs:150, 320`), and §7 S1 eliminates it with the pre-existing `IUiDispatcher` interface
  seam. Epic.md Shared Design §3 permits STA only where a seam is proven infeasible; here a seam is
  not merely feasible, it already exists in `UtilitiesCS/Threading/IUiDispatcher.cs:15`.
- `QuickFiler.Test` has no `[STATestClass]`, `[STATestMethod]`, or `*.StaTests.cs` file today
  (`00-cluster-overview.md` §5). This artifact does not propose introducing the first one.

---

## 12. Projected coverage

Estimated denominator ≈ 100–110 sequence points. Attribution of the proposed 29 tests plus the
existing 10 tests plus sibling-owned contributions:

| Region | Approx. sequence points | Covered after the proposed set? |
| --- | --- | --- |
| `Pair<T>` (members 1–3) | ~8 | Yes — T01–T05 |
| Logging helpers (members 4–7) | ~10 | Yes — T26–T29 plus incidental coverage from `LoadDfAsync` |
| Constructors (members 8–10) | ~14 | Yes — T06–T08 plus existing tests |
| `LoadAsync` × 3 (members 11–13) | ~48 | Yes, both arms of every branch — T12–T21 plus existing `:314-412` and F5's `EfcDataModelTests.cs:24-49` |
| `BackgroundInitInfoItemsAsync` (member 14) | ~7 | Yes — T23–T25 |
| Properties / fields (members 15–23) | ~18 | Yes — T09–T11 plus existing and sibling tests |
| `[Obsolete(…, true)]` methods (members 24–25) | ~5 | **No — permanently uncoverable** |

Projected line coverage: **≈ 95%**, comfortably clearing the 80% acceptance threshold. Even with a
pessimistic assumption that half of the `LoadAsync` await-continuation state-machine lines stay
unhit, the file clears 85%.

The only irreducible fraction is members 24–25 at **≈ 5% of the file**. Two dispositions, in
preference order:

1. **Preferred — delete both `[Obsolete("…", true)]` methods** (`:299-356`, the whole `#region
   Obsolete`). This is behaviour-neutral by construction: `error: true` guarantees no compiled call
   site exists anywhere, and a repository grep for `GetConversationInfoAsync` confirms the only
   occurrences are the declarations themselves and the obsolete-message strings. Deleting them
   removes ~56 lines, raises this file's projected coverage to ≈ 100%, and touches no sibling-owned
   file. The methods' commented-out bodies preserve no live behaviour.
2. **Fallback — leave them and request an F1-ledger line-level note.** If the plan declines the
   deletion, record members 24–25 in `coverage-ledger.md` as *uncoverable-by-compiler-contract*
   (distinct from `ratified-exempt`, which presumes reachable-but-untestable lines). Do **not** add
   `[ExcludeFromCodeCoverage]` — epic.md Shared Design §1 makes that a Blocking finding on a testable
   file, and the file as a whole is testable.

Either disposition leaves this file above 80%, so **no exemption request against F1's ledger is
required for `ConversationResolver.cs`.**
