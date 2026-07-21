# Research: store-runtime-reenable (Issue #263)

- Date: 2026-07-07
- Epic: #260 (store-lockup-resilience)
- Depends on: F1 (#261, `IStoreDisableService`, store-identity convention) — F1's `spec.md` and
  `plan.2026-07-07T17-41.md` are drafts with unfilled template sections at the time of this
  research; the `Reenable(identity)` contract and identity type are treated as given per the
  epic manifest's "Shared Design Alignment" section, not independently re-derived here.

## 1. Current State Analysis

### 1.1 The three startup hookup subsystems

Startup hookup is not one operation; it is three independent subsystems, each with its own
per-store logic, error-handling policy, and lifecycle, all triggered once from
`AppEvents.Hook()`:

1. **`AppEvents` item-level hookup** (`TaskMaster/AppGlobals/AppEvents.cs`,
   `PerformReadinessHookup()` :215-262). Runs exactly once, invoked by
   `HookReadinessCoordinator` (`TaskMaster/AppGlobals/HookReadinessCoordinator.cs`) once
   `OutlookReadinessGate.IsReady()` (app-wide, `Session.DefaultStore` only) reports ready. Inside:
   `OlToDoItems = ...` (idempotent via property setter, unsubscribes old handler first, :104-124),
   `OlReminders = ...`, and `Globals.Ol.Inboxes.ForEach(x => OlInboxes.AddLast(x.Items, items =>
   items.ItemAdd += OlInboxItems_ItemAdd))` (:244-246). `OlInboxes` is a
   `LockingLinkedList<Items>` (:150) that stores raw `Items` COM references with **no key and no
   "already contains" guard** — every call to this loop body unconditionally appends. `Unhook()`
   (`AppEvents.ReadinessHookup.cs` :18-23) clears the whole list and unsubscribes every entry, but
   there is no partial/per-store unhook.

2. **`AppOlObjects.LoadInboxes()`** (`TaskMaster/AppGlobals/AppOlObjects.cs` :124-181), the source
   of `Globals.Ol.Inboxes`. It enumerates `NamespaceMAPI.Stores`, filters via
   `StoresWrapper.ShouldIncludeStore`, and calls `store.GetDefaultFolder(olFolderInbox)` per
   included store, with a per-store `try/catch (COMException)` that **rethrows only the two
   transient HRESULTs** (`OutlookReadinessGate.TransientStoreNotReadyHResult` /
   `TransientOperationFailedHResult`) and otherwise logs-and-skips. The per-store body is already
   isolated into a static, delegate-injected helper,
   `EmitPerStoreInboxAttribution(shouldInclude, getDefaultFolder, readDisplayName, probe)`
   (:204-240) — this is the repo's existing precedent for extracting a per-store COM operation
   behind injectable delegates for testability, and it is the pattern this feature should follow.
   The result is cached in `Lazy<IEnumerable<Folder>> _inboxes` (:97-98), reset only via
   `ResetLazyInboxes()` (:242), called from the constructor only.

3. **`OutlookFolderNotificationSink`** (`UtilitiesCS/OutlookObjects/Folder/
   OutlookFolderNotificationSink.cs`). `CreateProductionSubscriptions` (:138-157) builds a
   **frozen** `IReadOnlyList<IOutlookFolderNotificationSubscription>` once at construction: one
   `StoresNotificationSubscription` for `Stores.StoreAdd`/`BeforeStoreRemove` (:221-260), plus one
   `FoldersNotificationSubscription` per folder node reached by a depth-first walk from each
   store's root folder (`AddFolderSubscriptions(Store, ...)` :183-218), tagged by
   `store.StoreID ?? string.Empty` (:211). `Start()`/`Dispose()` (:43-56, :58-73) subscribe/
   unsubscribe the whole frozen list exactly once each; **there is no API to add or remove a
   single store's subscriptions after construction**. `AddFolderSubscriptions(Store, ...)` is
   already scoped to one store, but it is `private static` and mutates a caller-supplied
   `ICollection`, not the sink's own field.

4. **`StoresWrapper`/`StoreWrapper`** (`UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`,
   `StoreWrapper.cs`). `Init()` (:35-49) rebuilds `Stores` from scratch (used at first load).
   `RewireOlObjectsAsync(context)` (:83-127) is the one path that already treats stores
   per-store and idempotently for a **bulk re-hydrate after deserialize**: for each filtered live
   `Store`, it does `Stores.Find(x => x.DisplayName == storeDisplayName)`; if absent, `new
   StoreWrapper(store).Init()` and add; if present, `storeWrapper.Restore(store)`. This loop body
   (:100-119) is the closest existing analog to "add or restore one store" and has **no
   per-store try/catch** — a `COMException` from `StoreWrapper.Init()`/`Restore()` for one store
   aborts the whole async method for all remaining stores. `StoreWrapper.Init()`
   (`StoreWrapper.cs` :27-75) performs several sequential, uncatchable COM reads directly on
   `InnerStore` (`DisplayName`, `GetRootFolder()`, `ExchangeStoreType`,
   `GetDefaultFolder(olFolderInbox)`, then the `RootFolder.Session.CurrentUser.AddressEntry
   .GetExchangeUser().PrimarySmtpAddress` chain in `GetSmtpAddressFromStore()` :146-184, which
   *is* wrapped in its own `try/catch (COMException)` returning `null`).

### 1.2 Gaps relative to the F3 requirement

- No subsystem exposes a callable "hook up exactly one store, after startup has already
  completed" entry point. `PerformReadinessHookup`, `LoadInboxes`, and
  `CreateProductionSubscriptions` are each written as "iterate all stores once," not as
  "hook up store X."
- **No idempotency tracking exists today.** `OlInboxes.AddLast` and
  `OutlookFolderNotificationSink`'s frozen subscription list both assume they run exactly once.
  Re-running either without a new StoreID-keyed guard will double-subscribe.
- `OutlookReadinessGate.IsReady()` (`UtilitiesCS/OutlookObjects/OutlookReadinessGate.cs` :61-72)
  probes `_app.Session?.DefaultStore` only — it answers "is Outlook ready at all," not "is store X
  ready." A slow secondary store being re-enabled needs a store-scoped probe; the existing gate
  cannot express that today.
- `HookReadinessCoordinator` (`TaskMaster/AppGlobals/HookReadinessCoordinator.cs`) is a
  **run-exactly-once** state machine (`_completed` latches permanently true, :51,73,89-91) driven
  by a single parameterless `Action`. It is not reusable as a shared singleton for repeated,
  per-store, on-demand rehook calls, but its *shape* (pure gate-driven state machine, tested via
  `Mock<IOutlookReadinessGate>` with no timer/no COM, per
  `TaskMaster.Test/AppGlobals/HookReadinessCoordinatorTests.cs`) is the right pattern to
  replicate for a store-scoped, per-call instance.
- `AppOlObjects.cs` is already at **525 lines**, over the repository's 500-line file-size ceiling
  (General Code Change Policy §4 / `.claude/rules/general-code-change.md`). Any production
  addition to this file must instead go into a new partial-class file, following the precedent
  already set by `AppEvents.ReadinessHookup.cs` (extracted from `AppEvents.cs` for exactly this
  reason, per its own header comment).
- The retry-on-transient-failure behavior of `HookReadinessCoordinator.Tick()` re-invokes the
  **entire** `PerformReadinessHookup()` body, including the `Globals.Ol.Inboxes.ForEach` loop,
  on every transient retry. Because that loop has no per-store dedup guard today, a startup
  retry after a partial transient failure is a latent (pre-existing, out-of-scope-to-fix)
  duplicate-subscription risk that F3 must not reproduce in its own new code path; the
  idempotency guard proposed below (§3) is required specifically because this pattern does not
  self-protect.

## 2. Candidate Approaches: Shared Per-Store Hookup Helper

### Approach A — Force one unified per-store loop shared verbatim by all three subsystems

Restructure all three call sites (`PerformReadinessHookup`, `LoadInboxes`,
`CreateProductionSubscriptions`) to iterate the same store collection and call one method that
internally does all three hookups (inbox item subscribe, folder/store subscribe, StoreWrapper
add/restore) with one shared error-handling policy.

- **Rejected.** The three existing loops have materially different, deliberate error-handling
  policies already tuned to their call site: `LoadInboxes` rethrows only the two known transient
  HRESULTs and otherwise logs-and-skips per store; `AddFolderSubscriptions` catches
  `COMException`/`InvalidCastException` at the *whole-collection* level and silently stops
  processing remaining stores (:172-179); `RewireOlObjectsAsync`'s loop has no per-store catch at
  all. Collapsing these into one policy would change startup behavior for defects unrelated to
  this feature (a purely COM/VSTO-exempt regression risk with no test able to catch it, since
  these call sites are COM/VSTO-exempt by the repository's coverage policy). This also fights the
  bugfix-workflow principle of a minimal, targeted change.

### Approach B — Extract one per-store primitive per subsystem, add a thin orchestrator that calls all three (Recommended)

Each subsystem gets (or already has) a small, per-store method:

- `OutlookFolderNotificationSink` gains instance methods `AddStore(Outlook.Store store)` /
  `RemoveStore(string storeId)` that mutate an internal, mutable, StoreID-keyed collection instead
  of the current frozen `IReadOnlyList` built once in `CreateProductionSubscriptions`. This is a
  natural extension of the already-per-store `AddFolderSubscriptions(Store, ...)` method — it
  becomes the body of `AddStore`.
- `AppOlObjects` gains a per-store inbox-resolution primitive (mirroring
  `EmitPerStoreInboxAttribution`'s existing delegate-injection style) that `LoadInboxes()`'s loop
  calls, and that the new coordinator also calls for one store.
- `AppEvents` gains a per-store inbox-item-subscribe primitive that
  `PerformReadinessHookup()`'s loop calls, and that the new coordinator also calls for one store,
  guarded by the StoreID-keyed idempotency tracker (§3).
- `StoresWrapper`'s existing `RewireOlObjectsAsync` loop body (`Find` → `Init()`-or-`Restore()`)
  is extracted as a single-store method (e.g. `AddOrRestoreStore(Outlook.Store store)`) reused by
  both the bulk rewire and the new coordinator.
- A new orchestrator (`StoreRehookCoordinator` in `TaskMaster/AppGlobals`, implementing a new
  `IStoreRehookService`) calls the four per-store primitives in sequence for exactly one store,
  wraps them in the store-scoped readiness/retry loop (§4), and returns a structured result
  (§6). Startup's own loops keep their existing bulk iteration and per-loop error policy, but the
  *body* of each loop becomes a call to the same extracted per-store method the coordinator uses —
  so there is exactly one implementation of "how to hook up one store" per subsystem, reused by
  both startup and runtime rehook, while each subsystem keeps its own already-tuned
  per-store error policy.

This satisfies the epic's "shared per-store hookup helper... F4/F5 do not re-implement
rehooking" requirement (the call surface for F1/F4/F5 is the single `IStoreRehookService`) while
keeping the change to each existing loop as small as a body-extraction (lower regression risk,
consistent with "reuse existing hookup code paths rather than duplicating them").

**Rejected alternative:** a single God-object that owns all COM access directly (bypassing
`AppEvents`/`AppOlObjects`/`StoresWrapper`/`OutlookFolderNotificationSink` and re-implementing
their per-store logic from scratch) was considered and rejected — it would duplicate logic the
epic explicitly says to reuse, and would double the surface area needing COM/VSTO coverage
exemption review.

## 3. Idempotency Strategy

Track hooked stores by **StoreID** (Outlook's stable per-store identifier, already used as the
tagging key in `FoldersNotificationSubscription` :211 and in `FolderTreeSnapshot`/
`FolderTreeRequest`), not by the F1 display-name identity — StoreID is the correct key at the COM
subscription layer because it is stable across a store being temporarily unreachable, whereas
`DisplayName` is F1's user-facing identity for the *disabled-store list*, a different concern.
The coordinator should resolve F1's identity to a live `Outlook.Store` (which naturally yields its
`StoreID`) as the first step, then key all internal idempotency state by `StoreID`.

- `OutlookFolderNotificationSink`: replace the frozen `_subscriptions` list with a
  `StoreID -> IOutlookFolderNotificationSubscription[]` map (or a set of already-subscribed
  StoreIDs) built at construction and mutated by `AddStore`/`RemoveStore`. `AddStore` checks "is
  this StoreID already present" before subscribing; a call for an already-present StoreID is a
  documented no-op success, not an error, matching the AC's "re-enabling an already-hooked store
  does not create duplicate subscriptions."
- `AppEvents`/`OlInboxes`: introduce a `StoreID -> Items` tracking structure (the raw
  `LockingLinkedList<Items>` alone cannot answer "is store X already hooked" without a key).
  Options are (a) augment `OlInboxes` with a parallel `Dictionary<string, Items>` guarded by the
  same lock, or (b) replace `OlInboxes` with a small keyed wrapper type that still exposes the
  existing enumeration surface `ProcessNewInboxItemsAsync` depends on. Recommendation: (a), the
  smaller diff — `PerformReadinessHookup`'s loop and the new coordinator both consult the
  dictionary before calling `AddLast`.
- `StoresWrapper.Stores`: idempotency is already implicit in `RewireOlObjectsAsync`'s existing
  `Stores.Find(x => x.DisplayName == storeDisplayName)` check — reuse that exact lookup in the
  extracted `AddOrRestoreStore` method; a found `StoreWrapper` is `Restore`d (refreshes COM
  references) rather than duplicated, so this subsystem needs no *new* idempotency guard.
- The coordinator's own top-level idempotency check (`IsAlreadyFullyHooked(storeId)`) should be a
  pure, testable predicate over the three tracking structures above (all thin interfaces/dicts),
  not a live COM re-probe, so it stays fast and mockable.

## 4. STA Safety

- **Never do a synchronous, expensive COM read on the UI thread inside the rehook call itself.**
  The AC requires this explicitly. `StoreWrapper.Init()`'s COM reads (`GetRootFolder`,
  `GetDefaultFolder`, the SMTP chain) are the expensive operations already implicated in the
  epic's lockup scenario; they must run behind the same store-scoped readiness gate used to defer
  the whole rehook, not be attempted eagerly before the gate reports ready.
- **Reuse the poll/backoff shape of `Hook()`/`HookReadinessCoordinator`, not the singleton
  itself.** `HookReadinessCoordinator` is `internal sealed` (accessible within `TaskMaster` only,
  which is fine — the new coordinator lives there too) but is a run-once state machine bound to a
  single `Action`. Construct a **new instance per rehook call**
  (`new HookReadinessCoordinator(storeScopedGate, () => PerformOneStoreHookup(store))`) driven by
  a `DispatcherTimer` with the same initial/backoff cadence constants used in `Hook()`
  (1s initial, 5s after 10 ticks). This reuses the exact tested state-machine shape without
  requiring `HookReadinessCoordinator` itself to become reentrant/multi-instance-unsafe (it
  already is safe to construct multiple instances; nothing today prevents that).
- **Extend readiness probing to be store-scoped.** `IOutlookReadinessGate.IsReady()` has no
  parameter and is hardwired to `Session.DefaultStore`. Add an overload
  `bool IsReady(Outlook.Store store)` to `IOutlookReadinessGate`/`OutlookReadinessGate` that
  performs the same non-throwing `try { store.GetDefaultFolder(olFolderInbox) != null } catch
  (COMException) { false }` pattern already used by `IsReady()`, reusing the existing
  `IsTransientError(COMException)` classification unchanged (it is already store-agnostic — it
  only inspects the HRESULT). This is an additive interface change (existing callers of the
  parameterless `IsReady()` are unaffected).
- **Bounded retry, not infinite polling, for the user-initiated case.** `Hook()`'s "never give
  up" polling is appropriate for a background one-time startup action with no user waiting on
  visible feedback. A `Reenable` click is user-initiated and UI-observable; polling forever with
  no terminal outcome would look like the button did nothing. Recommend a bounded retry window
  (same cadence, capped total elapsed time, e.g. on the order of the existing backoff-after-10-
  ticks threshold extended to a hard stop a short time after) that surfaces a
  `TransientTimeout` failure (§6) rather than "never give up." This is a product-facing decision
  that should be confirmed during planning, not silently inherited from `Hook()`'s infinite-retry
  policy — flagged here because the two call sites (passive startup vs. user-initiated action)
  have different UX requirements even though they share the same mechanical retry shape.
- No `Thread.Sleep`/`Task.Delay`/real blocking wait is introduced; if a delay between poll ticks
  is needed outside the `DispatcherTimer` path (e.g., for a caller without a running WPF
  dispatcher), reuse `NonBlockingDelay.WaitAsync` (`TaskMaster/AppGlobals/NonBlockingDelay.cs`),
  which is already the repo's approved pump-independent, non-blocking wait primitive.

## 5. Adding the Store Back and Clearing the Disabled Scope

Per the epic's "Shared Design Alignment" section, the call direction is: **F1's
`IStoreDisableService.Reenable(identity)` invokes F3's rehook mechanism**, not the reverse. F3
must not take a compile-time dependency on `IStoreDisableService` for its own rehook contract;
F3 exposes a self-contained mechanical operation, and F1 (or F4/F5 acting through F1) is
responsible for clearing the disabled-scope bookkeeping based on F3's reported result. This
keeps F3's dependency on F1 limited to the store-identity type/convention (as stated in the
delegation prompt), avoiding a circular assembly dependency between the two features.

Recommended sequence inside `IStoreRehookService.RehookStoreAsync(identity)`:

1. Resolve `identity` to a live `Outlook.Store` by re-enumerating `NamespaceMAPI.Stores` and
   matching on the same identity convention F1 defines (DisplayName primary, documented
   fallback). If no live store matches, return `StoreNotFound` immediately — no COM read beyond
   the enumeration is attempted.
2. Check the idempotency trackers (§3) for the resolved StoreID. If already fully hooked in all
   three subsystems, return `AlreadyHooked` (a success variant) without re-touching COM.
3. Enter the store-scoped readiness/retry loop (§4). On success, perform, in order:
   a. `StoresWrapper.AddOrRestoreStore(store)` (rebuilds/refreshes the `StoreWrapper`).
   b. The extracted `AppEvents` per-store inbox-item-subscribe primitive (guarded by the
      idempotency dictionary).
   c. `OutlookFolderNotificationSink.AddStore(store)` (guarded by its own StoreID-keyed guard).
   d. `FolderTreeService.MarkStale(store.StoreID, FolderTreeRefreshReason.StoreAdded)` — the
      cached folder-tree snapshot (`OutlookFolderTreeService`) must be invalidated for this store
      so a settings-UI or navigation view built on `IOutlookFolderTreeService` does not keep
      serving a stale (store-absent) snapshot after reenable. `MarkStale` is already part of
      `IOutlookFolderTreeService` (`UtilitiesCS/OutlookObjects/Folder/
      IOutlookFolderTreeService.cs` :20) and is the correct existing seam — no new cache-
      invalidation mechanism is needed.
4. Return the structured result (§6) to the caller. **The caller (F1's `Reenable`), not F3,
   decides whether to clear the disabled-scope entry**, and should do so only when the result is
   `Success` or `AlreadyHooked` — a `StoreNotFound`/`PermanentError`/`TransientTimeout` result
   must leave the store's disabled flag set so the UI does not present a store as "reenabled"
   while it remains unhooked. This ordering directly serves the AC "a store that cannot be
   rehooked reports a clear failure result" and prevents a false-positive reenable state.

This means **F3's implementation work includes a small, targeted edit to F1's disable-service
implementation** (once F1 lands) to inject `IStoreRehookService` and call it from `Reenable`
before the existing disabled-scope-clearing logic — this is the one cross-feature production
file F3 must touch outside its own new files, and it should be called out explicitly in F3's
atomic plan as a dependency on F1's file layout being final.

## 6. Failure Result Contract

Recommend a small result type (not a bare `bool`) returned from `RehookStoreAsync`, e.g.:

```
enum StoreRehookOutcome
{
    Success,          // store newly hooked in all three subsystems
    AlreadyHooked,     // idempotent no-op; all three subsystems already had this store
    StoreNotFound,     // identity does not resolve to any live Store in NamespaceMAPI.Stores
    TransientTimeout,  // readiness gate never reported ready within the bounded retry window
    PermanentError,    // a non-transient COMException (or other exception) was raised during hookup
}
```

paired with a result record carrying the outcome, the resolved `StoreID`/identity (for logging),
and the causing exception when applicable. `PermanentError` and `TransientTimeout` must both be
logged via log4net (per AC "Failures are logged and surfaced without crashing") with enough
context (identity, subsystem that failed, HRESULT if COM-derived) to diagnose without needing to
reproduce live. No outcome should let an exception escape `RehookStoreAsync` uncaught — every
COM boundary crossed by the coordinator's own code (as opposed to the pre-existing, unrelated
per-subsystem loops it delegates to) must be wrapped, consistent with the "fail fast and
explicitly" / "do not silently ignore errors" general policy.

## 7. Test Strategy Behind Mockable Seams

The repository already establishes, in this exact area, that Moq can proxy Outlook Interop COM
interfaces directly (`Mock<Outlook.NameSpace>`, `Mock<Outlook.Stores>`, `Mock<Items>`,
`Mock<MailItem>` all appear with `MockBehavior.Strict` in
`UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderNotificationSinkTests.cs` and
`TaskMaster.Test/AppGlobals/AppEventsTests.cs`). Two test tiers follow that existing pattern:

1. **Orchestrator tests (`StoreRehookCoordinator`/`IStoreRehookService`), fully seam-mocked, no
   COM at all.** The coordinator should depend on injected, narrow interfaces for each
   subsystem step — a store-lookup seam, an `IStoreWrapperGateway`-shaped seam wrapping
   `AddOrRestoreStore`, an inbox-hookup seam, `IOutlookFolderNotificationSink` (already an
   interface), and the extended `IOutlookReadinessGate`. Mocking all of these with Moq
   (mirroring `HookReadinessCoordinatorTests`'s scripted-sequence style, e.g.
   `SetupSequence(...).Returns(false).Returns(false).Returns(true)`) lets tests drive: not-found,
   transient-then-ready, already-hooked idempotency (verify zero additional `+=`/`AddStore`
   calls on the second invocation), and permanent-error propagation — all deterministically, with
   no live Outlook, no temp files, and no real timers (the `DispatcherTimer` glue itself, like
   `Hook()`, is COM/VSTO-exempt by inspection; only the pure `Tick()`-equivalent decision logic
   carries the coverage obligation).
2. **Per-subsystem primitive tests**, extending the existing suites rather than creating parallel
   ones: `OutlookFolderNotificationSinkTests` gains `AddStore`/`RemoveStore` cases using the same
   `FakeSubscription` fixture already defined there; `AppEventsTests` gains a case for the
   extracted per-store inbox-hookup primitive using the existing `Mock<Items>`
   /`BuildInboxSubscriptions` helpers; a `StoresWrapper`-focused test (new or extended) drives
   `AddOrRestoreStore` with a `Mock<Outlook.Store>` (`DisplayName`, `GetRootFolder`,
   `ExchangeStoreType`, `GetDefaultFolder`, the `RootFolder.Session.CurrentUser` chain) to confirm
   the found/absent branches without a live Outlook session. No coverage-exemption applies to
   these primitive methods themselves (they contain new decision logic, e.g. the StoreID-keyed
   idempotency guards) — only the outermost COM-construction call sites
   (`OutlookFolderNotificationSink`'s constructor overload, `Hook()`'s `DispatcherTimer` wiring)
   remain exempt, consistent with the existing `[ExcludeFromCodeCoverage]` markings already
   present on those exact methods.
3. **Regression coverage for startup.** Because the shared per-store primitives replace inline
   loop bodies rather than the loops themselves, existing startup-path tests
   (`AppEventsTests`, any `AppOlObjects`/`StoresWrapper` startup tests) should continue to pass
   unmodified except where they assert on now-extracted method names; this is the "regression:
   normal startup hookup unaffected by the extracted shared helper" test condition called out in
   the issue.

## 8. File-by-File Change List

**New production files (4):**

| File | Purpose |
|---|---|
| `UtilitiesCS/Interfaces/IGlobals/IStoreRehookService.cs` | Public interface (namespace `TaskMaster`, matching the existing convention of `IAppEvents`/`IApplicationGlobals`/`IOlObjects` living in `UtilitiesCS` under the `TaskMaster` namespace so the COM-bound implementation in the `TaskMaster` assembly can implement it). Declares `Task<StoreRehookResult> RehookStoreAsync(string storeIdentity)`. |
| `UtilitiesCS/OutlookObjects/Store/StoreRehookResult.cs` (or `TaskMaster/AppGlobals/`) | The outcome enum/result record from §6. |
| `TaskMaster/AppGlobals/StoreRehookCoordinator.cs` | Orchestrator implementing `IStoreRehookService`; store-scoped readiness/retry loop; calls the four extracted per-store primitives in sequence; owns the top-level idempotency check. |
| `TaskMaster/AppGlobals/AppEvents.StoreRehook.cs` | New partial of `AppEvents` (mirrors the existing `AppEvents.ReadinessHookup.cs` split) holding the extracted per-store inbox-item-subscribe primitive and its StoreID-keyed idempotency dictionary — kept out of `AppEvents.cs` to preserve file-size headroom. |

**Modified production files (6, all small/targeted):**

| File | Change |
|---|---|
| `UtilitiesCS/OutlookObjects/IOutlookReadinessGate.cs` | Add `bool IsReady(Outlook.Store store)` overload. |
| `UtilitiesCS/OutlookObjects/OutlookReadinessGate.cs` | Implement the new overload, reusing `IsTransientError` unchanged. |
| `UtilitiesCS/OutlookObjects/Folder/IOutlookFolderNotificationSink.cs` | Add `AddStore(Outlook.Store store)` / `RemoveStore(string storeId)`. |
| `UtilitiesCS/OutlookObjects/Folder/OutlookFolderNotificationSink.cs` | Convert the frozen `_subscriptions` list to a mutable, StoreID-keyed structure; implement `AddStore`/`RemoveStore` with idempotency guards; `AddFolderSubscriptions(Store, ...)` becomes the reused per-store body. |
| `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs` | Extract `RewireOlObjectsAsync`'s loop body into a reusable `AddOrRestoreStore(Outlook.Store store)` method called by both the bulk rewire loop and the new coordinator. |
| `TaskMaster/AppGlobals/AppOlObjects.cs` | Replace `LoadInboxes()`'s inline loop body with a call to a newly extracted per-store primitive; **file is already at 525 lines (over the 500-line ceiling), so the extracted primitive itself must live in a new partial file, not grow this one.** |

**New production file required by the above (1, to keep `AppOlObjects.cs` from growing):**

| File | Purpose |
|---|---|
| `TaskMaster/AppGlobals/AppOlObjects.StoreRehook.cs` | New partial of `AppOlObjects` holding the extracted per-store inbox-resolution primitive reused by `LoadInboxes()` and the coordinator. |

**Cross-feature file (1, touched by F3, owned by F1):**

| File | Change |
|---|---|
| F1's `IStoreDisableService` implementation (path not yet final; F1's `spec.md`/`plan.md` are draft templates as of this research) | Inject `IStoreRehookService`; `Reenable(identity)` calls it and only clears the disabled-scope entry on `Success`/`AlreadyHooked`. |

**DI exposure (1, exact placement open):**

`IApplicationGlobals` (`UtilitiesCS/Interfaces/IGlobals/IApplicationGlobals.cs`) or `IOlObjects`
needs to expose `IStoreRehookService` so F1/F4/F5 can obtain it without constructing it directly.
The exact accessor location depends on F1's final composition (this research does not
independently redesign F1); flagged as an open integration point for the atomic plan.

**Production file count: 5 new + 6 modified (one of the "modified" entries, `AppOlObjects.cs`,
is edited only to replace a loop body with a one-line call, to respect the file-size ceiling) +
1 cross-feature file in F1's territory.**

**New test files / extended existing test files (mirrors the repo's existing `<Project>.Test`
sibling-project layout, not a generic top-level `tests/` tree — this matches every existing
C# test in the repository, e.g. `TaskMaster.Test/AppGlobals/*`,
`UtilitiesCS.Test/OutlookObjects/**`):**

- `TaskMaster.Test/AppGlobals/StoreRehookCoordinatorTests.cs` (new) — orchestration, idempotency,
  bounded-retry, and all four `StoreRehookOutcome` branches via Moq.
- `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderNotificationSinkTests.cs` (extended) —
  `AddStore`/`RemoveStore` idempotency using the existing `FakeSubscription` fixture.
- A new or extended `OutlookReadinessGateTests.cs` under `UtilitiesCS.Test/OutlookObjects/` (none
  currently exists by that name; confirm during planning) — the new `IsReady(Outlook.Store)`
  overload.
- `TaskMaster.Test/AppGlobals/AppEventsTests.cs` (extended) or a new
  `AppEvents.StoreRehookTests.cs` — the extracted per-store inbox-hookup primitive and its
  idempotency guard, using the existing `Mock<Items>` helpers already in that file.
- A `StoresWrapper`-focused test (new or extended, location under
  `UtilitiesCS.Test/OutlookObjects/Store/`) — `AddOrRestoreStore` found/absent branches with a
  `Mock<Outlook.Store>`.

## Cross-Feature Impact Summary (for F4/F5)

- F4 (lockup detect/notify) and F5 (settings UI reenable) must call `IStoreDisableService
  .Reenable(identity)` (F1), **not** `IStoreRehookService` directly — the epic's stated call
  graph routes reenable through F1 so the disabled-scope bookkeeping stays centralized. F3's
  contract (`IStoreRehookService`) is an implementation detail F1 depends on, not a surface F4/F5
  need to know about, provided F1's `Reenable` is wired per §5 above.
- The `StoreRehookOutcome` enum (§6) is the shape F1's `Reenable` must translate into whatever
  result type F4's notification UI and F5's settings UI surface to the user; F4/F5 should not
  need to interpret COM-level detail (HRESULTs, exception types) themselves — F3's result
  contract exists precisely to hide that detail behind a small, closed enum.
- `FolderTreeService.MarkStale` (§5 step 3d) directly benefits any F5 settings-UI view built on
  `IOutlookFolderTreeService`'s cached snapshot; F5 does not need its own cache-invalidation
  logic for a just-reenabled store as long as F3 performs this step.
