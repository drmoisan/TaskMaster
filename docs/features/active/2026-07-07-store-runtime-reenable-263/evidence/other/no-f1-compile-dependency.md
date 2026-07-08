# AC9 — No Compile-Time Dependency on IStoreDisableService

Timestamp: 2026-07-08T01-27

Command:
grep -n "IStoreDisableService" TaskMaster/AppGlobals/StoreRehookCoordinator.cs UtilitiesCS/OutlookObjects/Store/StoreRehookResult.cs TaskMaster/AppGlobals/AppEvents.StoreRehook.cs TaskMaster/AppGlobals/AppOlObjects.StoreRehook.cs

EXIT_CODE: 1 (no matches)

Result: none — zero references to `IStoreDisableService` in any F3 production file.

Notes:
- The `StoreIdentity` dependency is expected and permitted (reconciled AC9): `StoreRehookCoordinator` implements F1's `IStoreRehookService.RehookAsync(StoreIdentity)`, so it has an unavoidable, intended compile-time dependency on `StoreIdentity`. That is NOT part of the AC9 prohibition, which concerns only `IStoreDisableService`.
- The call direction is F1 -> F3 only: F1's `StoreDisableService.ReenableAsync` awaits the injected `IStoreRehookService`. F3 never references `IStoreDisableService`.
- `UtilitiesCS/OutlookObjects/Store/StoreDisableService.cs`, `UtilitiesCS/Interfaces/IGlobals/IApplicationGlobals.cs`, and `UtilitiesCS/Interfaces/IGlobals/IStoreRehookService.cs` are byte-for-byte unchanged (git diff --stat HEAD reports no changes). The single F1-territory edit is the DI construction site in `TaskMaster/AppGlobals/ApplicationGlobals.cs` (line 118), which now passes the real `StoreRehookCoordinator` as the `rehook` argument.
- No `StoreRehook` accessor was added to `IApplicationGlobals` (F1 obtains the collaborator via constructor injection at the DI site, not via the aggregate).

Output Summary: AC9 satisfied — zero `IStoreDisableService` references in F3 production files; F1 files unchanged.
