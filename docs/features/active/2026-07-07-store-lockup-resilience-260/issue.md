# store-lockup-resilience (Issue #260)

- Date captured: 2026-07-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/store-lockup-resilience/ (Issue #260)
- Promotion type: epic

- Issue: #260
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/260
- Last Updated: 2026-07-07
- Work Mode: full-feature

## Problem / Why

A specific Outlook `Store` can lock up the TaskMaster add-in's UI thread (STA) for an
extended period when interaction with that store errors repeatedly (for example transient
store-not-ready HRESULTs, a failing Exchange logon, or a slow/expensive per-store COM read such
as `FilePath`, `ExchangeStoreType`, or the SMTP-address chain). This is a re-emerging class of
problem at the boundary between this add-in and another add-in / store provider. Existing work
(Issues #207, #211, #240, #242, #243) hardened startup against a single slow store but did not
give the add-in a durable, user-visible way to (a) detect that one store is repeatedly locking
the UI, (b) isolate that store, and (c) let the user govern and reverse the isolation.

A related, separate defect compounds the pain: opening "TaskMaster -> Settings -> Folder
Settings" shows "Store settings are not available yet. Please try again after startup
completes." even though startup completed long ago. Root-cause analysis (see the epic manifest)
shows there is no missing startup-complete notification — the readiness guard at
`StoreWrapperController.EvaluateLaunchReadiness()` reads live model state correctly, but
`Globals.Ol.StoresWrapper` is genuinely null for the whole session because
`AppOlObjects.LoadStoresAsync` hit a config-missing or null-deserialize path and left the model
unpopulated with only a silent `logger.Error`.

## Proposed Behavior

Provide a durable, robust store-lockup resilience capability:

1. Detect when a particular `Store` locks up the UI for an extended period due to errors, and
   attribute the lockup to a specific store without itself causing further lockups or delays
   (using only cheap, already-cached store identity).
2. On attribution, add the offending store to a disabled list immediately (session scope) to
   restore UI responsiveness.
3. Deliver a modeless (non-blocking) message telling the user the store was disabled,
   identifying the store as specifically as possible, and offering three options:
   "Disable This Session Only", "Disable for Future Sessions", and "Reenable".
4. "Reenable" re-hooks the store at runtime: because the original hookup method has already
   terminated, this adds the `Store` back to `Stores` and re-registers its AppEvents / folder
   notification handlers.
5. "Disable for Future Sessions" persists the disablement so the store is excluded on subsequent
   startups.
6. Add a setting under "TaskMaster -> Settings -> Folder Settings" (or an appropriate
   alternative surface) listing disabled stores with the ability to reenable each.
7. Fix the "Store settings are not available yet" defect at its root cause so Folder Settings
   opens with a populated store model after startup.

## Acceptance Criteria (early draft)

- [ ] An extended, error-driven UI lockup attributable to a single store is detected and the
      store is auto-disabled (session scope) without introducing new blocking calls.
- [ ] A modeless message identifies the disabled store and offers Disable This Session Only /
      Disable for Future Sessions / Reenable, each wired to the correct behavior.
- [ ] "Reenable" re-adds the store to `Stores` and re-registers its event handlers at runtime.
- [ ] "Disable for Future Sessions" persists across restarts; the store is excluded at startup.
- [ ] Folder Settings shows a list of disabled stores with per-store reenable.
- [ ] Folder Settings no longer shows "not available yet" after startup completes; the store
      model is populated (root-cause load-pipeline fix).
- [ ] Full C# toolchain passes (csharpier -> analyzers -> nullable/TreatWarningsAsErrors ->
      MSTest with coverage) for every feature; new/changed code meets coverage targets.

## Constraints & Risks

- COM/STA safety: detection and identification must not call expensive/blocking store members
  (`FilePath`, `ExchangeStoreType`, SMTP chain). Prefer cached identity (`DisplayName`,
  `StoreID`) captured before the lockup.
- Runtime rehook of a live COM `Store` (Reenable) is the highest-risk element: it must
  re-establish `AppEvents` item subscriptions and `OutlookFolderNotificationSink` store/folder
  subscriptions safely, idempotently, and without double-hooking.
- Persistence must reuse the existing `StoresWrapper` (`SmartSerializable`) filter/serialize
  path; do not introduce a new settings file or config key.
- Modeless UI must marshal to the UI thread via the existing `UiThread`/`IUiDispatcher` seam and
  reuse `MyBox` action-button primitives.
- Determinism/testability: all new logic must be unit-testable behind interfaces with Moq; no
  live Outlook, no temporary files, injected clock/timeouts.
- Backward compatibility: standalone (non-epic) behavior and existing startup hardening
  (#207/#211/#240) must not regress.

## Test Conditions to Consider

- [ ] Unit coverage areas: lockup attribution decision, disable/enable service state and
      persistence, filter honoring the disabled list, reenable rehook orchestration, modeless
      notification wiring, load-pipeline fallback when config is missing/null.
- [ ] Integration scenarios: open Folder Settings before/after load; disable then reenable a
      store; persisted disablement across a simulated restart.
- [ ] Edge/negative: attribution on a store with no cached identity; reenable of an already-hooked
      store (idempotency); config-missing and null-deserialize load paths.

## Next Step

- [ ] Promote to GitHub issue (epic) via MCP tooling
- [ ] Create `docs/features/epics/store-lockup-resilience/epic-plan.md` manifest
- [ ] Promote and link 5 child issues (F1-F5)
