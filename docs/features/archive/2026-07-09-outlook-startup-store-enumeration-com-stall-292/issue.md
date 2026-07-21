# outlook-startup-store-enumeration-com-stall (Issue #292)

- Date captured: 2026-07-09
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/outlook-startup-store-enumeration-com-stall/ (Issue #292)

- Issue: #292
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/292
- Last Updated: 2026-07-09
- Work Mode: full-bug

## Problem / Why

On Outlook cold start, the STA/UI thread (`VSTA_Main`) blocks for ~108 seconds inside the raw COM enumeration of `Namespace.Stores`, tripping the `ContextSwitchDeadlock` MDA (CLR unable to transition COM contexts for 60 seconds).

Ground-truth evidence (`artifacts/log.txt`, captured 2026-07-09 14:14:49 → 14:16:37+):

```
at System.Runtime.InteropServices.CustomMarshalers.EnumeratorViewOfEnumVariant.MoveNext()
at System.Linq.Enumerable.<CastIterator>d__97`1.MoveNext()
at System.Linq.Enumerable.WhereEnumerableIterator`1.MoveNext()
at System.Collections.Generic.List`1..ctor(IEnumerable`1 collection)
at System.Linq.Enumerable.ToList[TSource](IEnumerable`1 source)
at UtilitiesCS.OutlookObjects.Store.StoresWrapper.Init() ... StoresWrapper.cs:line 44
at TaskMaster.AppOlObjects.BuildFreshStoresWrapper() ... AppOlObjects.StoreLoading.cs:line 33
at TaskMaster.AppOlObjects.LoadStoresAsync() ... AppOlObjects.StoreLoading.cs:line 64
at TaskMaster.ApplicationGlobals.LoadOlObjectsPhaseAsync()
at TaskMaster.ApplicationGlobals.LoadSequentialAsync()
```

The blocking call is `GetFilteredStores().ToList()` at `StoresWrapper.cs:44`, on the synchronous fresh-build path (`BuildFreshStoresWrapper` → `Init()`). The enumeration is stuck on the first `IEnumVARIANT::Next()` — before any store is yielded.

This is a new, previously-unhardened instance of the recurring COM-blocking startup stall documented in issues #207, #211, and epic #260. Prior incidents hit the same class of problem at different call sites (`JunkCertain` folder enumeration, `App.Reminders`, `LoadInboxes.GetDefaultFolder`) caused by a misbehaving mail store stalling during MAPI provider logon (`WrappedMSProvider::Logon`). Each site was hardened individually; the raw `Namespace.Stores` enumeration itself was not.

## Proposed Behavior

The Outlook add-in startup must not block the STA message pump on the `Namespace.Stores` enumeration. When a store is mid-logon and the enumeration would block, the startup path should keep pumping and the existing #260 watchdog/attribution/auto-disable pipeline should be able to attribute the stall to a store and recover, rather than reporting a blank attribution (`CurrentStoreContext.Current == null`) because the block occurs before any per-store scope is opened.

The specific achievable fix (readiness-gating the enumeration, attribution parity via `CurrentStoreContext`, or another approach) is uncertain and must be determined by research, because no managed cancellation/timeout exists for a blocking `IEnumVARIANT::Next()` and prior deferrals only migrated the block to the next COM call site.

## Acceptance Criteria (revised after research; see `research/2026-07-09-outlook-startup-store-enumeration-com-stall-research.md` §5)

Research established that no managed mechanism can prevent, shorten, cancel, or bound the block once `IEnumVARIANT::Next()` is entered (no cancellation for a blocked COM call; STA-apartment-bound; the stall duration is set by the store's MAPI provider logon, not add-in code). The achievable, in-scope defect is that the add-in's own #260 resilience pipeline is silent at this site (blank attribution) and is one code path from a watchdog-thread crash. The ACs are therefore the meaningful, crash-safe watchdog action, not a preserved pump.

- [x] **AC1 — Attributed watchdog action at both enumeration sites.** A stall inside the `Namespace.Stores` materialization at `StoresWrapper.Init()` (line 44) and `RewireOlObjectsAsync` (line 89) produces a watchdog action within the attribution threshold: a `[store-lockup]` WARN line attributed to the enumeration phase (and optionally an informational modeless notification), instead of today's total silence (blank attribution). _Delivered: both materializations wrapped in the `CurrentStoreContext` enumeration-phase scope (`MaterializeFilteredStores`); verified by T1/T2 (RED->GREEN)._
- [x] **AC2 — Non-null phase identity, handled safely (no disable write, no crash).** The stall attribution is a non-null phase identity (`"<Stores-enumeration>"`, distinct from `"<unavailable>"`). `StoreLockupResponder` gains a phase-identity branch that emits WARN + optional notify with `autoDisabled: false` and returns WITHOUT calling `IsDisabled`/`DisableSessionOnly`/the action-button wiring. This closes the verified `InvalidOperationException` watchdog-thread crash path (null model during the fresh-build window) and the #265 disabled-store UI pollution path. _Delivered: phase-identity terminal branch precedes every disable-service call; verified by T3 (Strict mock, zero disable calls)._
- [x] **AC3 — Behavior-preserving for healthy stores.** The included-store set and enumeration order are unchanged; `CurrentStoreContext.Current` is null after materialization completes (scope disposed); nested per-store scopes continue to work. _Delivered: observational-only scope; verified by T4/T5 (GREEN before and after)._
- [x] **AC4 — Deterministic RED-before-GREEN regression coverage.** Coverage via the existing `ReflectionRealProxy`/`Mock<Stores>().As<IEnumerable>()` seams (no live Outlook, no temp files) covering: attribution observable from inside `MoveNext()` at both sites (RED on HEAD), the responder phase branch with a `MockBehavior.Strict` `IStoreDisableService` asserting zero disable calls (RED on HEAD), behavior preservation, and scope-restore-on-failure. New code meets the >= 90% new-code coverage obligation. _Delivered: RED (EXIT 1, T1/T2/T3 fail) -> GREEN (4519/4519 pass); new executable-code coverage 14/14 = 100%._

## Out of Scope / Residual (non-gating)

- The code fix cannot prevent, shorten, or cancel the block itself. Environmental relief (repair/re-sync/remove the misbehaving store account in Outlook) is a human action and the only relief for the stall duration.
- End-to-end reproduction of the real ~108s stall and in-situ confirmation of the WARN/notification require a live affected Outlook profile; recorded as post-merge manual validation notes, not acceptance gates.
- Optional secondary hardening (default-store readiness pre-gate; routing the fresh-build loop through `AddOrRestoreStore` for per-store parity) is deferred unless the planner accepts the same causal scope.

## Constraints & Risks

- No managed cancellation or timeout exists for a blocking `IEnumVARIANT::Next()`; the enumeration cannot be aborted once entered. A fix must avoid entering the blocking call on the pumping STA, or bound it, rather than cancel it.
- Outlook Interop objects are STA-apartment-bound; COM calls cannot be offloaded to a worker thread (they marshal back to the STA).
- Architectural directive: minimize STA reliance, never block the STA message pump, gate readiness-dependent COM hookups on a real store-readiness signal, and use pumping-wait primitives (DispatcherTimer / Application.Idle) rather than synchronous blocks or `Task.Delay`/`Thread.Sleep`.
- The change is cross-cutting into the #260 resilience system (`ThreadMonitor`, `CurrentStoreContext`, `HookReadinessCoordinator`, `StoreRehookCoordinator`). Scope must be attributed by causation.
- Environmental relief (repairing/removing the misbehaving store account in Outlook) is a human action outside the code fix and must be recorded under automation feasibility if surfaced.

## Test Conditions to Consider

- [ ] Injectable-seam unit test that simulates a blocking/slow store enumeration and asserts the STA pump is preserved and/or attribution is non-blank.
- [ ] Behavior-preserving test: healthy stores yield the identical included set and order.
- [ ] Watchdog attribution test: a stall at the enumeration site resolves a store identity for the auto-disable pipeline.

## Next Step

- [x] Promote to GitHub issue (bug template)
- [ ] Create `docs/features/active/` folder from the template
- [ ] Research: determine the achievable pump-preserving fix and automation-feasibility of any environmental dependency
