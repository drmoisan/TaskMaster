# Phase 0 — F1/F2/F3 Contract Verification (P0-T12)

Timestamp: 2026-07-08T03-51

Command: `git grep -n "StoreDisable\|ReenableAsync\|GetDisabledStores\|DisabledStoreEntry\|StoreIdentity" -- UtilitiesCS TaskMaster` (filtered to declaration lines)

EXIT_CODE: 0

Output Summary — every listed F1 contract symbol is present:
- `IApplicationGlobals.StoreDisable { get; }` — UtilitiesCS/Interfaces/IGlobals/IApplicationGlobals.cs:23
  - implemented: TaskMaster/AppGlobals/ApplicationGlobals.cs:431 (`public IStoreDisableService StoreDisable => _storeDisableService;`)
- `interface IStoreDisableService` — UtilitiesCS/Interfaces/IGlobals/IStoreDisableService.cs:54
- `Task ReenableAsync(StoreIdentity identity)` — IStoreDisableService.cs:86 (impl StoreDisableService.cs:108)
- `IReadOnlyCollection<DisabledStoreEntry> GetDisabledStores()` — IStoreDisableService.cs:103 (impl StoreDisableService.cs:145)
- `public readonly struct DisabledStoreEntry` — IStoreDisableService.cs:29; members `StoreIdentity Identity` (line 41), `DisableScope Scope` (line 44)
- `public enum DisableScope { SessionOnly, FutureSessions }` — IStoreDisableService.cs:8 (SessionOnly line 11, FutureSessions line 14)
- `public readonly struct StoreIdentity` — UtilitiesCS/OutlookObjects/Store/StoreIdentity.cs:21; `public static StoreIdentity Resolve(string displayName, string filePathFallback = null)` — StoreIdentity.cs:62

Namespace note (as flagged by the orchestrator, confirmed by reading the files):
- `DisabledStoreEntry`, `DisableScope`, `IStoreDisableService` live in namespace `UtilitiesCS`.
- `StoreIdentity` lives in namespace `UtilitiesCS.OutlookObjects.Store`.
- F5 files under `UtilitiesCS/OutlookObjects/Store/` therefore need `using UtilitiesCS;` to
  reference the entry/scope/service types.

Scope indicator: `DisableScope` enum with members `SessionOnly` and `FutureSessions`.
IsFutureSession projection = `(entry.Scope == DisableScope.FutureSessions)`.

NOTE: `DisabledStoreEntry` has no `DisplayName` member; the display text is
`entry.Identity.Value` (StoreIdentity exposes `Value`, not `DisplayName`). The row's
`DisplayName` is therefore projected from `entry.Identity.Value`.

Verdict: All required symbols present. Plan is NOT blocked; proceeding to Phase 1.
