---
name: project-store-runtime-reenable-263
description: Issue #263 (epic #260 store-lockup-resilience) research findings on runtime store rehook — no existing per-store post-startup hookup seam anywhere; AppOlObjects.cs already over the 500-line ceiling
metadata:
  type: project
---

Researched 2026-07-07 for F3 store-runtime-reenable (#263), highest-risk feature in epic #260
(store-lockup-resilience). Full research at
`docs/features/active/2026-07-07-store-runtime-reenable-263/research/2026-07-07-store-runtime-reenable-research.md`.

Key non-obvious findings, load-bearing for F3 and for F4/F5 (#264/#265) which depend on F3:

- None of the three startup hookup subsystems (`AppEvents.PerformReadinessHookup`,
  `AppOlObjects.LoadInboxes`, `OutlookFolderNotificationSink.CreateProductionSubscriptions`)
  expose a callable "hook up exactly one store after startup" entry point today. Each is written
  as "iterate all stores once." `OutlookFolderNotificationSink`'s `_subscriptions` list is frozen
  at construction with no `AddStore`/`RemoveStore` API.
- No idempotency tracking exists anywhere in this area. `OlInboxes.AddLast` and the frozen
  notification-sink list both assume single-run; a StoreID-keyed guard must be added as new work,
  not reused.
- `OutlookReadinessGate.IsReady()` is app-wide (`Session.DefaultStore` only) — it cannot express
  "is store X ready." A store-scoped overload is required.
- `HookReadinessCoordinator` is a run-exactly-once state machine bound to a single parameterless
  `Action` (`_completed` latches permanently). Reuse its *shape* (construct a new instance per
  rehook call) — do not try to make the existing singleton reentrant.
- `TaskMaster/AppGlobals/AppOlObjects.cs` is already 525 lines, over the repo's 500-line file-size
  ceiling. Any addition there must go into a new partial file (precedent:
  `AppEvents.ReadinessHookup.cs` was extracted from `AppEvents.cs` for this exact reason).
- Epic's call graph: F1's `IStoreDisableService.Reenable(identity)` invokes F3's rehook mechanism
  (not the reverse) — F3 must not depend on `IStoreDisableService`; F1's disable-service
  implementation is a cross-feature file F3's work will need to edit once F1 lands, to inject
  `IStoreRehookService` and only clear the disabled flag on rehook success.
- Moq already proxies Outlook Interop COM interfaces directly in this repo's tests
  (`Mock<Outlook.Store>`, `Mock<Items>`, `Mock<NameSpace>`, `Mock<Stores>` all appear with
  `MockBehavior.Strict` in `OutlookFolderNotificationSinkTests.cs` and `AppEventsTests.cs`) — use
  this precedent for testing the new per-store primitives without live Outlook.

See also [[qfc-item-controller-227-r2-denial]] for the general pattern of per-member/per-store
barrier analysis before claiming a coverage exemption in COM-adjacent code — likely relevant when
F3's atomic plan reaches the coverage-exemption question for the new coordinator's COM-touching
adapters.
