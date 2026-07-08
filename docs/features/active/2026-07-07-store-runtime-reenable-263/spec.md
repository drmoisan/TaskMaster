# store-runtime-reenable (Spec)

- **Issue:** #263
- **Parent (epic):** #260 (store-lockup-resilience)
- **Feature ID:** F3
- **Wave:** 1 (depends on F1 #261 and F2 #262)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-07
- **Status:** Draft
- **Version:** 0.1
- **Work Mode:** full-feature

## Overview

F3 delivers the runtime rehook mechanism for the store-lockup-resilience epic. When a store
has been disabled (auto-disabled by F4, or disabled by the user through the F5 settings UI) and
the user chooses to reenable it, the add-in must re-establish that store at runtime without
restarting Outlook. The original hookup runs exactly once during startup and its methods have
already terminated by the time a reenable is requested.

Startup hookup is not a single operation. It is three independent subsystems, each iterating all
stores once and each with its own error-handling policy:

1. `AppEvents.PerformReadinessHookup` — per-inbox `ItemAdd` subscriptions.
2. `AppOlObjects.LoadInboxes` — store to default-inbox resolution feeding `Globals.Ol.Inboxes`.
3. `OutlookFolderNotificationSink` — per-store `StoreAdd`/`BeforeStoreRemove` and per-folder
   subscriptions, built once as a frozen list at construction.

None of these subsystems exposes a callable "hook up exactly one store after startup" entry
point, and none tracks which stores are already hooked. F3 introduces that seam by extracting one
per-store primitive from each subsystem so that startup and runtime rehook share a single code
path, then adds a thin orchestrator (`IStoreRehookService`) that drives the four per-store
primitives in sequence for exactly one store, behind a store-scoped readiness/retry loop, and
returns a structured result.

This feature delivers the rehook mechanics only. The trigger surfaces (lockup detection and
notification in F4, settings UI in F5) and the disabled-store model/service (F1) are separate
features. Reenable is routed through F1's `IStoreDisableService.ReenableAsync`, which invokes
F3's rehook service; F4 and F5 never call F3 directly.

## Scope and Non-Scope

### In scope

- Extract one per-store primitive from each of the three startup hookup subsystems, so that each
  subsystem's existing bulk loop and the new runtime rehook path share a single implementation of
  "how to hook up one store" for that subsystem.
- Extract `StoresWrapper.RewireOlObjectsAsync`'s loop body into a reusable single-store
  `AddOrRestoreStore(Outlook.Store)` method, reused by both the bulk rewire and the new
  coordinator.
- A new `IStoreRehookService` interface and its implementation (`StoreRehookCoordinator`) that
  resolves a store identity to a live `Outlook.Store`, checks idempotency, runs the store-scoped
  readiness/retry loop, drives the four per-store primitives in order, invalidates the cached
  folder-tree snapshot, and returns a structured result.
- A store-scoped readiness overload `bool IsReady(Outlook.Store store)` on
  `IOutlookReadinessGate`/`OutlookReadinessGate`.
- StoreID-keyed idempotency tracking across the three hookup subsystems.
- A structured failure-result contract (`StoreRehookOutcome` enum + result record) with distinct
  outcomes for store-missing, transient-not-ready-after-retry, and permanent error, logged via
  log4net.
- The in-scope edit to F1's `ReenableAsync` implementation to inject the real
  `IStoreRehookService` and call it, replacing the wave-0 no-op collaborator F1 shipped.
- Deterministic MSTest coverage across a COM-free orchestrator tier and a COM-mocked primitive
  tier.

### Non-scope

- The lockup detection, attribution, auto-disable, and modeless notification (F4).
- The disabled-stores settings UI (F5).
- The disabled-store model, disable/enable service, store filter changes, and persistence (F1).
- The Folder Settings null-store load-pipeline fix (F2).
- Any redesign of the existing startup readiness/retry infrastructure (#207/#211/#242/#243); F3
  reuses and extends it.
- Changing the deliberate, differing per-store error-handling policies of the three startup loops
  (rejected Approach A). Each subsystem keeps its own tuned per-store error policy; only the loop
  *body* becomes a call to the shared per-store primitive.
- A compile-time dependency from F3 on `IStoreDisableService`. The call direction is F1 to F3
  only.

## Design: Extracted Per-Store Primitives (Shared Startup + Rehook Path)

The design follows Approach B from the research: extract one per-store primitive per subsystem
and add a thin orchestrator that calls all three (plus the `StoresWrapper` primitive). Each
existing startup loop keeps its own iteration and error-handling policy; the loop body becomes a
call to the extracted primitive so there is exactly one implementation of per-store hookup per
subsystem, reused by both startup and runtime rehook. This mirrors the repository's existing
precedent, `AppOlObjects.EmitPerStoreInboxAttribution`, which already isolates a per-store COM
operation behind injectable delegates for testability.

### P1 — StoresWrapper: `AddOrRestoreStore(Outlook.Store store)`

Extract the body of `RewireOlObjectsAsync`'s per-store loop into a single-store method. It reuses
the existing `Stores.Find(x => x.DisplayName == storeDisplayName)` lookup: if absent, construct
`new StoreWrapper(store).Init()` and add to `StoresWrapper.Stores`; if present, call
`storeWrapper.Restore(store)` to refresh the COM references. Idempotency is already implicit in
this lookup, so this subsystem needs no new idempotency guard. Both the bulk rewire loop and the
coordinator call `AddOrRestoreStore`. The expensive COM reads inside `StoreWrapper.Init()`
(`GetRootFolder`, `GetDefaultFolder`, the SMTP chain) run only after the store-scoped readiness
gate reports ready (see STA-Safety Design), never eagerly.

### P2 — AppEvents: per-store inbox-item-subscribe primitive

Extract the body of `PerformReadinessHookup`'s
`Globals.Ol.Inboxes.ForEach(x => OlInboxes.AddLast(...))` loop into a per-store primitive that
subscribes one store's inbox `Items.ItemAdd` handler. This primitive is guarded by a StoreID-keyed
idempotency dictionary (see Idempotency Design). It lives in a new `AppEvents.StoreRehook.cs`
partial, mirroring the existing `AppEvents.ReadinessHookup.cs` split, to keep `AppEvents.cs` within
the file-size ceiling. `PerformReadinessHookup`'s loop and the coordinator both call this
primitive; the loop consults the idempotency dictionary before appending, closing the pre-existing
latent double-subscribe risk within F3's own new code path.

### P3 — AppOlObjects: per-store inbox-resolution primitive

Extract `LoadInboxes`'s per-store body (store to `GetDefaultFolder(olFolderInbox)` resolution,
retaining the existing transient-HRESULT rethrow / log-and-skip policy) into a per-store primitive
that follows the delegate-injection style of `EmitPerStoreInboxAttribution`. Because
`AppOlObjects.cs` is already 525 lines (over the 500-line ceiling), the extracted primitive lives
in a new `AppOlObjects.StoreRehook.cs` partial. This partial builds on F2's
`AppOlObjects.StoreLoading.cs` extraction, which is the F2 dependency: F2 and F3 both split
`AppOlObjects.cs` into partials, so F3 must land after F2 to avoid a file conflict. `LoadInboxes`
and the coordinator both call this primitive.

### P4 — OutlookFolderNotificationSink: `AddStore` / `RemoveStore`

Convert the frozen `IReadOnlyList<IOutlookFolderNotificationSubscription>` built once in
`CreateProductionSubscriptions` into a mutable, StoreID-keyed structure. Add instance methods:

- `void AddStore(Outlook.Store store)` — the body of the already-per-store
  `AddFolderSubscriptions(Store, ...)` method becomes `AddStore`'s implementation; guarded by a
  StoreID-keyed check so an already-present StoreID is a documented no-op success.
- `void RemoveStore(string storeId)` — unsubscribes and removes that StoreID's subscriptions.

`Start()`/`Dispose()` continue to subscribe/unsubscribe the whole collection once each. The single
`StoresNotificationSubscription` for `Stores.StoreAdd`/`BeforeStoreRemove` remains an app-level
subscription. Both `CreateProductionSubscriptions` and the coordinator populate the sink via
`AddStore`.

### Orchestrator sequencing

Inside `IStoreRehookService.RehookStoreAsync(identity)`:

1. Resolve `identity` to a live `Outlook.Store` by re-enumerating `NamespaceMAPI.Stores` and
   matching on F1's identity convention (DisplayName primary, documented fallback). If no live
   store matches, return `StoreNotFound` immediately (no COM read beyond the enumeration).
2. Check the idempotency trackers for the resolved `StoreID`. If already fully hooked in all
   three subsystems, return `AlreadyHooked` (a success variant) without re-touching COM.
3. Enter the store-scoped readiness/retry loop (see STA-Safety Design). On ready, perform in order:
   a. `StoresWrapper.AddOrRestoreStore(store)` (P1).
   b. The `AppEvents` per-store inbox-item-subscribe primitive (P2), idempotency-guarded.
   c. `OutlookFolderNotificationSink.AddStore(store)` (P4), idempotency-guarded.
   d. `IOutlookFolderTreeService.MarkStale(store.StoreID, FolderTreeRefreshReason.StoreAdded)` to
      invalidate the cached folder-tree snapshot so downstream views do not serve a stale,
      store-absent snapshot after reenable. `MarkStale` is an existing seam; no new
      cache-invalidation mechanism is added.

   (P3, the `AppOlObjects` inbox-resolution primitive, is the shared body reused by
   `LoadInboxes`; the coordinator uses the resolved inbox as required for step b.)
4. Return the structured result to the caller. The caller (F1's `ReenableAsync`), not F3, decides
   whether to clear the disabled-scope entry, and does so only on `Success` or `AlreadyHooked`.

## `IStoreRehookService` Contract

```csharp
namespace TaskMaster
{
    /// <summary>
    /// Re-establishes a single Outlook store at runtime after startup hookup has completed:
    /// re-adds the store to StoresWrapper.Stores and re-registers its AppEvents item-level and
    /// OutlookFolderNotificationSink folder/store-level handlers. Idempotent and STA-safe.
    /// </summary>
    public interface IStoreRehookService
    {
        /// <summary>
        /// Rehooks the store identified by <paramref name="storeIdentity"/>. Never throws for a
        /// hookup failure; all outcomes are reported via the returned result and logged.
        /// </summary>
        /// <param name="storeIdentity">F1's stable store identity (DisplayName primary).</param>
        /// <returns>A structured result describing the outcome.</returns>
        Task<StoreRehookResult> RehookStoreAsync(string storeIdentity);
    }
}
```

- The interface lives in `UtilitiesCS/Interfaces/IGlobals/IStoreRehookService.cs` under the
  `TaskMaster` namespace, matching the convention of `IAppEvents`/`IApplicationGlobals`/`IOlObjects`
  (interface in `UtilitiesCS`, COM-bound implementation in the `TaskMaster` assembly).
- The implementation, `StoreRehookCoordinator`, lives in `TaskMaster/AppGlobals/`.
- `IStoreRehookService` is exposed on the `IApplicationGlobals` aggregate (exact accessor placement
  is confirmed during atomic planning against F1's final composition) so F1 can obtain it via DI
  without constructing it directly.
- F3 takes no compile-time dependency on `IStoreDisableService`; the only coupling to F1 is the
  store-identity convention.

## Idempotency Design

Hooked stores are tracked by `StoreID` (Outlook's stable per-store identifier, already the tagging
key in `FoldersNotificationSubscription` and the folder-tree snapshot types), not by F1's
display-name identity. `StoreID` is the correct key at the COM subscription layer because it is
stable across a store being temporarily unreachable; `DisplayName` is F1's user-facing identity for
the disabled-store list, a separate concern. The coordinator resolves F1's identity to a live
`Outlook.Store` first (which yields its `StoreID`), then keys all internal idempotency state by
`StoreID`.

- `OutlookFolderNotificationSink`: a StoreID-keyed structure replaces the frozen list. `AddStore`
  checks whether the StoreID is already present before subscribing; a call for an already-present
  StoreID is a no-op success, not an error.
- `AppEvents`/`OlInboxes`: a `StoreID -> Items` tracking dictionary (guarded by the same lock as
  `OlInboxes`) answers "is store X already hooked." Both `PerformReadinessHookup`'s loop and the
  coordinator consult the dictionary before calling `OlInboxes.AddLast`.
- `StoresWrapper.Stores`: idempotency is already implicit in the existing
  `Stores.Find(x => x.DisplayName == storeDisplayName)` check reused by `AddOrRestoreStore`; a found
  wrapper is `Restore`d, not duplicated. No new guard is added here.
- The coordinator's top-level `IsAlreadyFullyHooked(storeId)` check is a pure, testable predicate
  over the three tracking structures, not a live COM re-probe, so it stays fast and mockable.

Re-enabling an already-hooked store must not create duplicate `+=` subscriptions in any subsystem.

## STA-Safety Design

- **No synchronous expensive COM read on the UI thread during rehook.** `StoreWrapper.Init()`'s
  COM reads are the operations implicated in the epic's lockup scenario. They run only behind the
  store-scoped readiness gate, never eagerly before the gate reports ready.
- **Reuse the readiness-gate/transient-retry shape, not the singleton.**
  `HookReadinessCoordinator` is a run-exactly-once state machine bound to a single parameterless
  `Action` (`_completed` latches permanently). It is not made reentrant. Instead, the coordinator
  constructs a NEW `HookReadinessCoordinator` instance per rehook call
  (`new HookReadinessCoordinator(storeScopedGate, () => PerformOneStoreHookup(store))`), driven with
  the same cadence constants used by `Hook()` (1 s initial, 5 s after 10 ticks). Constructing
  multiple instances is already safe; nothing today prevents it.
- **Store-scoped readiness probe.** Add `bool IsReady(Outlook.Store store)` to
  `IOutlookReadinessGate`/`OutlookReadinessGate`. It performs the same non-throwing
  `try { store.GetDefaultFolder(olFolderInbox) != null } catch (COMException) { false }` pattern as
  the existing parameterless `IsReady()`, reusing `IsTransientError(COMException)` unchanged (it is
  already store-agnostic, inspecting only the HRESULT). This is additive; existing callers of the
  parameterless `IsReady()` are unaffected. The existing `IsReady()` checks only
  `Session.DefaultStore` and cannot express "is store X ready."
- **Bounded retry, not infinite polling.** `Hook()`'s never-give-up polling is appropriate for a
  passive one-time startup action. A reenable is user-initiated and UI-observable, so the
  coordinator uses a bounded retry window (same cadence, capped total elapsed time) that surfaces a
  `TransientTimeout` result rather than polling forever. The exact cap is confirmed during atomic
  planning.
- **No banned waits.** No `Thread.Sleep`/`Task.Delay`/real blocking wait is introduced. If a delay
  between poll ticks is needed outside the `DispatcherTimer` path, reuse the existing
  `NonBlockingDelay.WaitAsync` primitive.

## F1 Wiring Edit

Per the epic's Staged `ReenableAsync` seam, F1 (wave 0) implements `ReenableAsync` to clear both
disabled scopes and persist, invoking an injected rehook collaborator that defaults to a no-op in
wave 0. F3's deliverable includes a small, targeted edit to F1's disable-service implementation:

- Inject `IStoreRehookService` into F1's disable service (replacing the wave-0 no-op collaborator).
- `ReenableAsync(identity)` calls `RehookStoreAsync(identity)` before its existing
  disabled-scope-clearing logic.
- F1 clears the disabled-scope entry only when the result is `Success` or `AlreadyHooked`. A
  `StoreNotFound`, `PermanentError`, or `TransientTimeout` result must leave the disabled flag set
  so the UI does not present a store as reenabled while it remains unhooked.

This is the one cross-feature production file F3 touches outside its own new files. Its exact path
depends on F1's final file layout and is called out as a dependency in F3's atomic plan.

## Failure Contract

`RehookStoreAsync` returns a structured result (not a bare `bool`):

```csharp
public enum StoreRehookOutcome
{
    Success,          // store newly hooked in all three subsystems
    AlreadyHooked,    // idempotent no-op; all three subsystems already had this store
    StoreNotFound,    // identity does not resolve to any live Store in NamespaceMAPI.Stores
    TransientTimeout, // readiness gate never reported ready within the bounded retry window
    PermanentError,   // a non-transient COMException (or other exception) was raised during hookup
}
```

The paired `StoreRehookResult` record carries the outcome, the resolved `StoreID`/identity (for
logging), and the causing exception when applicable. Contract rules:

- `Success` and `AlreadyHooked` are success variants; the caller may clear the disabled scope.
- `StoreNotFound`, `TransientTimeout`, and `PermanentError` are failure variants; the caller must
  leave the disabled scope set.
- `PermanentError` and `TransientTimeout` are logged via log4net with enough context (identity, the
  subsystem that failed, HRESULT if COM-derived) to diagnose without reproducing live.
- No outcome lets an exception escape `RehookStoreAsync` uncaught. Every COM boundary the
  coordinator's own code crosses is wrapped, consistent with the "fail fast and explicitly / do not
  silently ignore errors" policy. The pre-existing per-subsystem loops retain their own error
  policies and are not re-wrapped by the coordinator.

## Determinism and Testability

The repository already establishes in this exact area that Moq can proxy Outlook Interop COM
interfaces directly (`Mock<Outlook.NameSpace>`, `Mock<Outlook.Stores>`, `Mock<Items>`,
`Mock<MailItem>` with `MockBehavior.Strict` in `OutlookFolderNotificationSinkTests.cs` and
`AppEventsTests.cs`). Tests follow that pattern in two tiers plus regression coverage. No live
Outlook, no temporary files, no real timers, no `Thread.Sleep`/`Task.Delay`, no wall-clock reads.
Tests use MSTest, Moq, and FluentAssertions per repo policy, and live in the `<Project>.Test`
sibling-project layout used by every existing C# test.

### Tier 1 — COM-free orchestrator tests

`StoreRehookCoordinator`/`IStoreRehookService` depends on injected narrow interfaces for each step
(a store-lookup seam, a `StoresWrapper`-gateway seam wrapping `AddOrRestoreStore`, an inbox-hookup
seam, `IOutlookFolderNotificationSink`, `IOutlookFolderTreeService`, and the extended
`IOutlookReadinessGate`). Mocking all of these with Moq (mirroring
`HookReadinessCoordinatorTests`'s scripted-sequence style, e.g.
`SetupSequence(...).Returns(false).Returns(false).Returns(true)`) drives every branch
deterministically: `StoreNotFound`, transient-then-ready, `AlreadyHooked` idempotency (verify zero
additional `AddStore`/subscribe calls on a second invocation), `TransientTimeout` (readiness never
ready within the bounded window), and `PermanentError` propagation. The pure decision logic (the
`Tick()`-equivalent and `IsAlreadyFullyHooked`) carries the coverage obligation; only the
`DispatcherTimer` glue itself is COM/VSTO-exempt by inspection, consistent with `Hook()`.

### Tier 2 — COM-mocked primitive tests

Extend the existing suites rather than creating parallel ones:

- `OutlookFolderNotificationSinkTests` gains `AddStore`/`RemoveStore` cases (including
  already-present-StoreID no-op idempotency) using the existing `FakeSubscription` fixture.
- `AppEventsTests` (or a new `AppEvents.StoreRehookTests.cs`) gains a case for the extracted
  per-store inbox-item-subscribe primitive and its StoreID-keyed idempotency guard, using the
  existing `Mock<Items>`/`BuildInboxSubscriptions` helpers.
- A `StoresWrapper`-focused test drives `AddOrRestoreStore` found/absent branches with a
  `Mock<Outlook.Store>` (`DisplayName`, `GetRootFolder`, `ExchangeStoreType`, `GetDefaultFolder`,
  the `RootFolder.Session.CurrentUser` chain), confirming both branches without a live session.
- A new or extended `OutlookReadinessGateTests` covers the new `IsReady(Outlook.Store)` overload
  (ready, transient not-ready, permanent COMException classification).

The new decision logic in these primitives (the StoreID-keyed idempotency guards) is not
coverage-exempt; only the outermost COM-construction call sites already marked
`[ExcludeFromCodeCoverage]` remain exempt.

### Regression coverage for startup

Because the shared per-store primitives replace inline loop bodies rather than the loops
themselves, existing startup-path tests should continue to pass unmodified except where they assert
on now-extracted method names. This is the "normal startup hookup unaffected by the extracted shared
helper" condition. Coverage targets and the full C# toolchain (CSharpier -> analyzers ->
nullable/TWAE -> MSTest with coverage, gated `TestCategory!=LiveOutlook`) apply per repo policy;
all touched and new files remain <= 500 lines.

## Acceptance Criteria

These criteria refine the early-draft ACs in `issue.md`. Each is testable via the two test tiers
above unless marked as a code-review/inspection check.

- [x] AC1: A new per-store primitive is extracted from each of the three startup hookup subsystems
      (`AppEvents.PerformReadinessHookup` inbox-item subscribe, `AppOlObjects.LoadInboxes` inbox
      resolution, `OutlookFolderNotificationSink` folder/store subscriptions) plus a
      `StoresWrapper.AddOrRestoreStore` primitive, such that each existing startup loop body and the
      runtime rehook path call the same per-store implementation for that subsystem.
- [x] AC2: `IStoreRehookService.RehookStoreAsync(storeIdentity)` re-adds the resolved live store to
      `StoresWrapper.Stores` and re-registers both item-level (`AppEvents`) and folder/store-level
      (`OutlookFolderNotificationSink`) handlers for that store, and invalidates the cached
      folder-tree snapshot via `IOutlookFolderTreeService.MarkStale`.
- [x] AC3: The operation is idempotent, keyed by `StoreID`: a second `RehookStoreAsync` for a store
      already hooked in all three subsystems returns `AlreadyHooked` and makes zero additional
      subscribe/`AddStore`/`AddLast` calls (verified by Moq `Verify(..., Times.Never())` on the
      second invocation).
- [x] AC4: The operation reuses the readiness-gate/transient-retry shape by constructing a new
      `HookReadinessCoordinator` instance per call (the run-once singleton is not made reentrant)
      and uses the store-scoped `IOutlookReadinessGate.IsReady(Outlook.Store)` overload; it
      introduces no synchronous expensive COM read on the UI thread before the gate reports ready.
- [x] AC5: The store-scoped `bool IsReady(Outlook.Store store)` overload is added to
      `IOutlookReadinessGate`/`OutlookReadinessGate`, reusing `IsTransientError(COMException)`
      unchanged, and does not alter the behavior of the existing parameterless `IsReady()`.
- [x] AC6: A transient-not-ready store is retried within a bounded window and, if never ready,
      returns `TransientTimeout` without blocking the STA; a store whose identity does not resolve
      to a live store returns `StoreNotFound`; a non-transient exception during hookup returns
      `PermanentError`. `TransientTimeout` and `PermanentError` are logged via log4net with
      identity, failing subsystem, and HRESULT (when COM-derived).
- [x] AC7: `RehookStoreAsync` never lets an exception escape uncaught; all outcomes are reported
      through the `StoreRehookResult`/`StoreRehookOutcome` contract.
- [x] AC8 (reconciled to F1's merged contract): The real `StoreRehookCoordinator` is injected as
      F1's `IStoreRehookService` collaborator at the DI construction site
      (`ApplicationGlobals.cs`, `new StoreDisableService(this, <coordinator>)`), replacing the wave-0
      no-op default. On reenable, F1's shipped `ReenableAsync` clears the disabled scope (session then
      persisted) and then awaits the real rehook collaborator **unconditionally**; the coordinator's
      outcome is logged (log4net), not used to gate scope-clearing. F1's `StoreDisableService` body is
      not modified. Reconciliation note: the original "clear the disabled scope only on
      `Success`/`AlreadyHooked`" wording was based on a pre-merge assumption about F1's seam; F1
      shipped a void-returning `Task` collaborator and clear-first-then-rehook-unconditionally
      ordering, which the epic directive forbids redesigning. (Code-review / integration check.)
- [x] AC9 (reconciled to F1's merged contract): F3 takes no compile-time dependency on
      `IStoreDisableService`. F3 does depend on `StoreIdentity` via F1's collaborator seam
      (`StoreRehookCoordinator` implements `RehookAsync(StoreIdentity)`); this dependency is
      unavoidable and intended. Reconciliation note: the pre-merge draft prohibited a `StoreIdentity`
      dependency, but F1's shipped `IStoreRehookService.RehookAsync(StoreIdentity)` makes it a
      required, intended coupling. (Code-review check.)
- [x] AC10: Deterministic MSTest coverage exists in two tiers — a COM-free orchestrator tier
      (`StoreRehookCoordinatorTests`) exercising all five `StoreRehookOutcome` branches and
      idempotency via Moq, and a COM-mocked primitive tier extending
      `OutlookFolderNotificationSinkTests`, `AppEventsTests`, a `StoresWrapper` test, and a
      readiness-gate test — with no live Outlook, no temporary files, no real timers.
- [x] AC11: Existing startup-path tests continue to pass (normal startup hookup unaffected by the
      extraction), and the full C# toolchain passes in order (CSharpier -> analyzers ->
      nullable/TWAE -> MSTest with coverage, `TestCategory!=LiveOutlook`) with no repository-wide
      coverage regression; all touched and new files remain <= 500 lines.

### Acceptance Criteria — Delivery Evidence (F3 #263, verified 2026-07-08)

All AC1–AC11 are checked above. Each is satisfied by the following implementation tasks and/or evidence artifacts:

- AC1 — P3-T1 (`StoresWrapper.AddOrRestoreStore`), P3-T3 (`AppEvents.SubscribeInboxForStore`), P3-T5 (`OutlookFolderNotificationSink.AddStore`/`AddStoreSubscriptions`), P3-T7 (`AppOlObjects.ResolveInboxForStore`); regression `evidence/regression-testing/startup-regression.md`.
- AC2 — P4-T2 (`RehookStoreCoreAsync` drives AddOrRestoreStore → SubscribeInboxForStore → sink.AddStore → MarkStale; public `RehookAsync(StoreIdentity)` adapter), P5-T1/P5-T2 (DI); tested P4-T3 (`StoreRehookCoordinatorTests`).
- AC3 — P3-T3/P3-T5/P4-T2 (StoreID-keyed idempotency); tested P3-T4, P3-T6, P4-T3 (second call zero additional subscribes via Times/no-op assertions).
- AC4 — P4-T2 (new `HookReadinessCoordinator` per call via `StoreScopedReadinessGate`, bounded 20-attempt window, `IsReady(Store)`, no eager COM read); tested P4-T3 (`...WhenGateNeverReady...NoEagerComRead`).
- AC5 — P1-T3/P2-T1 (`IsReady(Outlook.Store)` overload reusing `IsTransientError`, parameterless `IsReady()` unchanged); tested P2-T2 (`OutlookReadinessGateTests`).
- AC6 — P1-T1 (`StoreRehookOutcome`/`StoreRehookResult`), P4-T2 (`TransientTimeout`/`StoreNotFound`/`PermanentError` + log4net `LogOutcome`/`DescribeHResult`); tested P4-T3.
- AC7 — P4-T2 (broad catch at the AC7 boundary → `PermanentError`, no exception escapes); tested P4-T3 (`...PermanentError...WithoutThrowing`, adapter `NotThrowAsync`).
- AC8 — P5-T1/P5-T2 (real `StoreRehookCoordinator` injected at `ApplicationGlobals.cs` line 118; F1's `StoreDisableService.cs` byte-for-byte unchanged, clear-first-then-rehook-unconditionally preserved).
- AC9 — P5-T3 inspection; `evidence/other/no-f1-compile-dependency.md` (zero `IStoreDisableService` references in F3 production files; F1 files unchanged).
- AC10 — P2-T2, P3-T2, P3-T4, P3-T6, P4-T3 (two-tier deterministic MSTest: COM-free orchestrator tier + COM-mocked primitive tier; no live Outlook/temp files/real timers).
- AC11 — P5-T4 (`evidence/regression-testing/startup-regression.md`), P6-T1..T4 (`evidence/qa-gates/qa-01..04`), P6-T5 (`qa-05-coverage-delta.md`, no regression, new-code 99.6% >= 90%, testable denominator 83.23% >= 80%), P6-T6 (`evidence/other/file-size-check.md`, all files <= 500).

## Assumptions, Constraints, Dependencies

- **Assumptions:** F1's `StoreIdentity` convention and `IStoreDisableService.ReenableAsync` seam
  (with the injected no-op rehook collaborator) are final before F3's F1-wiring edit lands. F2's
  `AppOlObjects.cs` partial split has landed before F3 adds `AppOlObjects.StoreRehook.cs`.
- **Constraints:** COM/STA safety (no new blocking store-member reads on the UI thread); reuse
  existing primitives rather than duplicating them; net48; all touched/new files <= 500 lines;
  `AppOlObjects.cs` is at 525 lines, so additions go in a new partial.
- **Dependencies:** F1 (#261) for the identity convention and `ReenableAsync` seam; F2 (#262) for
  the `AppOlObjects.cs` partial split. Must remain compatible with #207/#211/#242/#243 readiness
  work.

## Risks and Mitigations

- **Highest-risk feature: live COM event rewiring.** A leaked or duplicated subscription is the
  primary risk. Mitigated by StoreID-keyed idempotency guards in every subsystem, verified by Moq
  `Times.Never()` assertions on repeat invocation.
- **STA re-freeze during rehook.** Mitigated by the store-scoped readiness gate, per-call bounded
  retry, and the prohibition on eager expensive COM reads.
- **Cross-feature file conflict on `AppOlObjects.cs`.** Mitigated by the F2 dependency edge
  (serializing F3 after F2) and by placing F3's additions in a separate partial.
- **False-positive reenable state.** Mitigated by the caller-clears-only-on-success contract; F3
  reports outcomes but F1 owns the disabled-scope bookkeeping.
