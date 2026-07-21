# Prerequisite Gate — F1/F3 Contracts (P0-T4)

Timestamp: 2026-07-08T07-54

Execution base: branch `feature/store-lockup-detect-notify-264` at HEAD
`872eafb4af82d1f766b239aadcc1e96b1ed3c2a7`, branched off
`origin/epic/store-lockup-resilience-integration`. Git log confirms F1 (#261/#275) and
F3 (#263/#276) are merged into the integration base (see git-baseline.md).

## Search Commands and Matched Paths

Command:
`grep -rn "DisableSessionOnly|DisableForFutureSessions|ReenableAsync|IsDisabled|GetDisabledStores" UtilitiesCS/Interfaces/IGlobals/IStoreDisableService.cs`
Matched: `UtilitiesCS/Interfaces/IGlobals/IStoreDisableService.cs`
- `void DisableSessionOnly(StoreIdentity identity);` (line 63)
- `void DisableForFutureSessions(StoreIdentity identity);` (line 74)
- `Task ReenableAsync(StoreIdentity identity);` (line 86)
- `bool IsDisabled(StoreIdentity identity);` (line 94)
- `IReadOnlyCollection<DisabledStoreEntry> GetDisabledStores();` (line 103)

Command:
`grep -rn "public static StoreIdentity Resolve" UtilitiesCS/OutlookObjects/Store/StoreIdentity.cs`
Matched: `UtilitiesCS/OutlookObjects/Store/StoreIdentity.cs`
- `public static StoreIdentity Resolve(string displayName, string filePathFallback = null)` (line 62)
- `public readonly struct StoreIdentity` (line 21)

Command:
`grep -rn "IStoreDisableService StoreDisable" UtilitiesCS/Interfaces/IGlobals/IApplicationGlobals.cs`
Matched: `UtilitiesCS/Interfaces/IGlobals/IApplicationGlobals.cs`
- `IStoreDisableService StoreDisable { get; }` (line 23)

Command:
`grep -rln "interface IStoreRehookService" UtilitiesCS TaskMaster`
Matched: `UtilitiesCS/Interfaces/IGlobals/IStoreRehookService.cs`

## Verdict

PREREQUISITE: PRESENT

All F1 contracts (`IStoreDisableService` with the five required members; `StoreIdentity`
readonly struct with the pure `Resolve(displayName, filePathFallback = null)` factory;
`IApplicationGlobals.StoreDisable`) and F3's `IStoreRehookService` exist on the execution
base. F4 makes no direct F3 call (F1 orchestrates F3 via `ReenableAsync`). Implementation
Phases 1–9 proceed.
