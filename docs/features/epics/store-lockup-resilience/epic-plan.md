---
epic: store-lockup-resilience
integration_branch: epic/store-lockup-resilience-integration
created_at: 2026-07-07T00:00:00Z
epic_issue: 260
features:
  - feature_folder: 2026-07-07-store-disable-service-261
    issue_num: 261
    depends_on: []
  - feature_folder: 2026-07-07-folder-settings-store-model-null-262
    issue_num: 262
    depends_on: []
  - feature_folder: 2026-07-07-store-runtime-reenable-263
    issue_num: 263
    depends_on: [2026-07-07-store-disable-service-261, 2026-07-07-folder-settings-store-model-null-262]
  - feature_folder: 2026-07-07-store-lockup-detect-notify-264
    issue_num: 264
    depends_on: [2026-07-07-store-disable-service-261, 2026-07-07-store-runtime-reenable-263]
  - feature_folder: 2026-07-07-disabled-stores-settings-ui-265
    issue_num: 265
    depends_on: [2026-07-07-store-disable-service-261, 2026-07-07-folder-settings-store-model-null-262, 2026-07-07-store-runtime-reenable-263]
---

# Epic: store-lockup-resilience (Issue #260)

## Goal

Provide a durable, robust capability for the TaskMaster Outlook add-in to survive a single
`Store` that repeatedly locks up the UI thread due to errors, by detecting and attributing the
lockup, isolating (disabling) the offending store, notifying the user with governed options,
and allowing runtime reenable — plus fixing the related defect that prevents Folder Settings
from opening after startup.

## Scope

- Detect an extended, error-driven UI-thread lockup and attribute it to a specific store using
  only cheap, already-cached identity (no new blocking COM reads).
- Auto-disable the offending store (session scope) immediately to restore responsiveness.
- Deliver a modeless message identifying the store and offering "Disable This Session Only",
  "Disable for Future Sessions", and "Reenable".
- Persist future-session disablement through the existing `StoresWrapper` / `SmartSerializable`
  path (no new file or config key).
- Reenable at runtime by re-adding the `Store` to `Stores` and re-registering its AppEvents /
  folder-notification handlers (the original hookup method has already terminated).
- Provide a settings surface listing disabled stores with per-store reenable.
- Fix the "Store settings are not available yet" defect at its root cause in the store
  load/deserialize pipeline.

## Non-Goals

- No change to the classifier/triage engines or ToDo model.
- No new IoC container; new services attach to the existing `IApplicationGlobals` aggregate.
- No replacement of the existing startup readiness/retry infrastructure (#207/#211/#242/#243);
  this epic extends it.
- Automatic epic decomposition is out of scope; this manifest is human-authored.

## Root-Cause Note (Folder Settings defect, F2 / #262)

The reported symptom ("the settings table is never notified that startup completed") does not
match the code. `StoreWrapperController.EvaluateLaunchReadiness()` reads live model state on
every click, and Issue #240 deliberately chose direct state inspection over an event/flag. The
dialog appears because `Globals.Ol.StoresWrapper` is genuinely null for the whole session, caused
upstream in `AppOlObjects.LoadStoresAsync` (config-missing branch or null deserialize) and
possibly `IntelligenceConfig.ReadConfigurationAsync` dropping the key. F2 therefore fixes the
load pipeline (fresh-build fallback + surfaced failure), robust to all three null paths, rather
than wiring a startup notification. Confirmed decision (2026-07-07).

## Feature Decomposition and Dependency DAG

| Feature | Issue | Folder | Wave | Depends on |
|---|---|---|---|---|
| F1 Disabled-store model + disable/enable service + filter + persistence | #261 | `2026-07-07-store-disable-service-261` | 0 | — |
| F2 Bugfix: `StoresWrapper` null → Folder Settings unavailable (root cause) | #262 | `2026-07-07-folder-settings-store-model-null-262` | 0 | — |
| F3 Runtime reenable: re-add `Store`, rehook AppEvents + notification sink | #263 | `2026-07-07-store-runtime-reenable-263` | 1 | F1, F2 |
| F4 UI-lockup detection + attribution + auto-disable + modeless notification | #264 | `2026-07-07-store-lockup-detect-notify-264` | 2 | F1, F3 |
| F5 Settings UI: list disabled stores with reenable | #265 | `2026-07-07-disabled-stores-settings-ui-265` | 2 | F1, F2, F3 |

Wave assignment by longest-path layering (`wave(f) = 1 + max(wave(d))`, 0 if no deps):
- Wave 0: F1 (#261), F2 (#262)
- Wave 1: F3 (#263)
- Wave 2: F4 (#264), F5 (#265)

## Cross-Cutting Constraints

- COM/STA safety: no new expensive/blocking store-member reads on the UI thread; detection and
  identification use cached identity only.
- Reuse existing primitives: `StoresWrapper`/`SmartSerializable` (persistence + filter),
  `StoreFilterAttribution` (pure decision), `AppEvents.PerformReadinessHookup` +
  `OutlookFolderNotificationSink` (event wiring), `ThreadMonitor` + `TimeOutTask` +
  `OutlookReadinessGate`/`HookReadinessCoordinator` (detection + safe retry), `MyBox` +
  `UiThread`/`IUiDispatcher` (modeless notification), `IApplicationGlobals` (DI).
- Determinism/testability: MSTest + Moq + FluentAssertions; injected clock/timeouts; no live
  Outlook; no temporary files; no `Thread.Sleep`/`Task.Delay`/real timers in tests.
- Backward compatibility: existing startup hardening and standalone behavior must not regress.
- Quality gates: full C# toolchain (csharpier → analyzers → nullable/TreatWarningsAsErrors →
  MSTest with coverage) per feature; coverage targets per repo policy.

## Shared Design Alignment (must stay consistent across features)

- Store identity: a single stable identity convention is defined by F1 as a pure
  `StoreIdentity.Resolve(displayName, filePathFallback = null)` (DisplayName primary, documented
  fallback; no blocking COM read) and reused by F3/F4/F5. A separate COM-touching overload is
  reserved for filter-time call sites only.
- Disable service contract (`IStoreDisableService`) is defined by F1 and exposed on
  `IApplicationGlobals` as the member `StoreDisable`. Fixed method shapes (resolves the F5-flagged
  open items):
  - `void DisableSessionOnly(StoreIdentity identity)`
  - `void DisableForFutureSessions(StoreIdentity identity)` (persists via `Model.Serialize()`)
  - `Task ReenableAsync(StoreIdentity identity)` (async, because rehook involves readiness retry)
  - `bool IsDisabled(StoreIdentity identity)`
  - `IReadOnlyCollection<DisabledStoreEntry> GetDisabledStores()` (entry = identity + scope)
  F4 and F5 call this service only; they do not call F3 directly.
- Staged `ReenableAsync` seam (resolves the F1↔F3 ordering): F1 (wave 0) implements
  `ReenableAsync` to clear both disabled scopes and persist, invoking an injected rehook
  collaborator that defaults to a no-op in wave 0. F3 (wave 1) provides the real
  `IStoreRehookService` and wires it into F1's implementation (a small, in-scope edit to F1's
  service is part of F3's deliverable). This keeps F1 shippable at wave 0 and avoids a forward
  dependency on F3.
- A shared per-store hookup primitive (extracted in F3 from `AppEvents.PerformReadinessHookup`,
  `AppOlObjects.LoadInboxes`, and `OutlookFolderNotificationSink`) keeps startup hookup and
  runtime rehook aligned; F4/F5 do not re-implement rehooking. Idempotency is tracked by
  `StoreID`.

## Post-Research Reconciliation (2026-07-07)

Applied after per-feature research completed; documents plan changes forced by findings:

- **DAG change — F3 now `depends_on: [F1, F2]`** (was `[F1]`). F2, F3, and F4 all modify
  `TaskMaster/AppGlobals/AppOlObjects.cs`, which is already 525 lines (over the 500-line cap) and
  must be split into partials (F2 adds `AppOlObjects.StoreLoading.cs`). Because the wave barrier
  gates per-dependency rather than per-wave, F3 could otherwise launch concurrently with F2 and
  conflict on that file. Adding the F2 edge serializes F3 after F2; F3 remains wave 1. F5 already
  depends on F2; F4 (wave 2) is transitively after F2 via F3, so no explicit F2 edge is added to F4.
- **Attribution mechanism (F4)**: research rejected `AsyncLocal` (cannot flow to the watchdog's
  background thread) in favor of a single-writer/single-reader `static volatile` current-store
  context set/cleared on the STA at the per-store COM entry points (`StoreWrapper.Init`,
  `StoresWrapper.RewireOlObjectsAsync`, `AppOlObjects` per-store attribution). Recorded so F1/F3
  keep those set/clear points intact.
- **Filter triple-implementation (F1)**: the include/exclude decision exists in three places
  (`ShouldIncludeStore`, `StoreIsIncluded`, `ShouldIncludeStoreInstrumented`→
  `StoreFilterAttribution.Decide`); the new `Disabled` reason must be added to all three
  identically (checked last, just before `Included`) so `Stores` and folder-tree gating cannot
  diverge for a disabled store.
- **Modeless notification (F4)**: `MyBox` has no modeless path today (its convenience overloads
  dispose the viewer in a `using` block incompatible with non-blocking `Show()`); F4 adds a new
  modeless composition dispatched via `IUiDispatcher.BeginInvoke`, mirroring
  `EfcHomeController.ViewerShowAction`.

## Integration & Delivery (deferred until user approval)

- Integration branch: `epic/store-lockup-resilience-integration` (created at wave-0 launch).
- Per-feature worktrees, PRs into the integration branch, wave barriers, and the final
  integration-to-`main` PR are the implementation phase and are NOT started until the user
  reads and approves this planning package.

## Status

See `epic-status.md` for the live status projection. Current phase: planning (documentation
only). Implementation is on hold pending user approval.
