# Coverage Gap Triage

Timestamp: 2026-05-05T19:02:18-04:00
Coverage Summary Source: `docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/evidence/qa-gates/csharp-coverage-summary.2026-05-05T15-05-19.md`
Coverage XML Source: `coverage/outlook-startup-ui-thread-deblock-141-final.cobertura.xml`

Changed Production Files:
- `TaskMaster/AppGlobals/ApplicationGlobals.cs`
- `TaskMaster/AppGlobals/AppOlObjects.cs`
- `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`
- `TaskMaster/AppGlobals/AppToDoObjects.cs`

Zero-Hit Methods:
- `TaskMaster.ApplicationGlobals..ctor(Application)`
- `TaskMaster.ApplicationGlobals.<LoadSequentialAsync>b__9_0`
- `TaskMaster.ApplicationGlobals.LoadWhenIdle()`
- `TaskMaster.ApplicationGlobals.<LoadWhenIdle>b__10_0`
- `UtilitiesCS.OutlookObjects.Store.StoresWrapper.CreateAsync(...)` success return
- `UtilitiesCS.OutlookObjects.Store.StoresWrapper.RewireAfterDeserializeAsync()`
- `TaskMaster.AppOlObjects.LoadAsync()` with the config-missing `LoadStoresAsync()` branch
- `TaskMaster.AppToDoObjects.LoadProgramInfo()`
- `TaskMaster.AppToDoObjects.People_CollectionChanged(...)`
- `TaskMaster.AppToDoObjects.LoadIDList()`
- `TaskMaster.AppToDoObjects.LoadProjInfo()` sync null-path

Test Homes:
- `TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs`
- `TaskMaster.Test/AppGlobals/AppOlObjectsCoverageTests.cs`
- `TaskMaster.Test/AppGlobals/AppToDoObjectsCoverageTests.cs`
- `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs`

Evidence Notes:
- The current coverage summary still records `Coverage Conclusion: FAIL` with changed/new-code coverage at `61.9469% (70/113 executable changed lines)`.
- The current Cobertura XML still includes the four active production files with the same per-file line rates called out by the failing summary: `ApplicationGlobals.cs` `36.4780%`, `AppOlObjects.cs` `27.8195%`, `StoresWrapper.cs` `92.7273%`, and `AppToDoObjects.cs` `24.7126%`.
- The current Cobertura XML confirms genuinely uncovered current entries for `TaskMaster.ApplicationGlobals..ctor(Application)`, `TaskMaster.ApplicationGlobals.LoadWhenIdle()`, `UtilitiesCS.OutlookObjects.Store.StoresWrapper.RewireAfterDeserializeAsync()`, `TaskMaster.AppToDoObjects.LoadProgramInfo()`, `TaskMaster.AppToDoObjects.People_CollectionChanged(...)`, and the sync `TaskMaster.AppToDoObjects.LoadProjInfo()` null-path, with partial coverage still remaining on `UtilitiesCS.OutlookObjects.Store.StoresWrapper.CreateAsync(...)` and `TaskMaster.AppToDoObjects.LoadIDList()`.

Feasibility Basis:
- `ApplicationGlobals` remains test-reachable without production edits because the single-argument constructor can be instantiated directly, the lazy basic-load state is reflectable from the existing test project, and `IdleAsyncQueue` already has reflection-backed tests proving the queue can be inspected and drained deterministically.
- `AppOlObjects` already has the necessary in-memory configuration and rewire seams in `AppOlObjectsTests`, so adding a dedicated coverage file for the public `LoadAsync()` path and the missing-config `LoadStoresAsync()` branch is test-only work.
- `StoresWrapper` already has real store-source tests in `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs`, so the remaining success-return and public wrapper-entry coverage is reachable with additional MSTest cases only.
- `AppToDoObjects` already has private-method invocation helpers, special-folder doubles, and no-temp-file fixture patterns, so the remaining sync null-path and event-handler coverage is reachable in a dedicated coverage file without widening production scope.

TestOnlyFeasible: true
Deferred Tasks: none
