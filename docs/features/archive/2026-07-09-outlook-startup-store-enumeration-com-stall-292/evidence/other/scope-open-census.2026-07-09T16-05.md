# CurrentStoreContext Scope-Open Census (issue #292, remediation cycle 1)

- Timestamp: 2026-07-09T16-05
- Tasks: [P1-T2] (production sites), [P1-T3] (test-class census)

## [P1-T2] Production `CurrentStoreContext.Begin` scope-open sites

Grep `CurrentStoreContext\.Begin` under `UtilitiesCS/` (production):

| # | Site | File:Line | Identity written |
|---|------|-----------|------------------|
| 1 | `StoresWrapper.AddOrRestoreStore` | `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs:146` | `storeDisplayName` |
| 2 | `StoresWrapper.MaterializeFilteredStores` | `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs:181` | `CurrentStoreContext.StoresEnumerationPhaseIdentity` = `"<Stores-enumeration>"` |
| 3 | `StoreWrapper.Init` | `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs:47` | `DisplayName` |

`MaterializeFilteredStores` (site 2) is called by `StoresWrapper.Init()` (line 44) and `StoresWrapper.RewireOlObjectsAsync()` (line 89). The pollution observed in the 10 CI failures is exactly `"<Stores-enumeration>"`, i.e. site 2, though ANY of the three writers in the parallel bucket can pollute a null-baseline reader. No other production `Begin` call site exists. These sites are NOT modified by this remediation.

## [P1-T3] `UtilitiesCS.Test` writer-class census

Universe of potential scope-openers = every test class that references `CurrentStoreContext` (direct `Begin`), `StoresWrapper` (construct / `Init` / `RewireOlObjectsAsync` / `AddOrRestoreStore`), or `StoreWrapper` (construct / `Init` / `Restore`). Assembly-wide `grep -lw` for these three types yields the complete candidate set below; no class outside this set can reach `Begin`.

### (a) Scope-opening test classes and disposition

| Class | File | Scope-open evidence | Disposition |
|-------|------|---------------------|-------------|
| `CurrentStoreContextTests` | `Threading/CurrentStoreContextTests.cs` | Direct `CurrentStoreContext.Begin(...)` x9 (lines 26,36,49,53,69,76,92,104,106); also the null-baseline reader | ALREADY `[DoNotParallelize]` (line 16) |
| `ThreadMonitorTests` | `Threading/ThreadMonitorTests.cs` | Direct `CurrentStoreContext.Begin("Mailbox X")` (line 88); also null-baseline reader | ALREADY `[DoNotParallelize]` (line 18) |
| `StoresWrapperTests` | `OutlookObjects/Store/StoresWrapperTests.cs` | `wrapper.Init()` (135), `RewireOlObjectsAsync` (162,188), `new StoreWrapper(...).Init()` (174,201) | MARK — P2-T1 (confirmed direct writer) |
| `StoresWrapperRehookTests` | `OutlookObjects/Store/StoresWrapperRehookTests.cs` | `wrapper.AddOrRestoreStore(...)` (60,83), `new StoreWrapper(...).Init()` (75) | MARK — P2-T2 (confirmed direct writer) |
| `StoresWrapperDisableTests` | `OutlookObjects/Store/StoresWrapperDisableTests.cs` | `wrapper.Init()` (221) | MARK — P2-T3 (confirmed direct writer) |
| `StoreWrapperTests` | `OutlookObjects/Store/StoreWrapperTests.cs` | `wrapper.Init()` (35,61) | MARK — P2-T4 (confirmed direct writer) |
| `StoreWrapperViewerTests` | `OutlookObjects/Store/StoreWrapperViewerTests.cs` | Constructs `new StoreWrapper(null)` (129) and `new StoresWrapper {...}` (64); drives `StoreWrapperController` render paths | MARK — P2-T5 (census member; controller render path may transitively reach `StoreWrapper.Init`; defensive) |
| `StoreWrapperInitProbeTests` | `OutlookObjects/Store/StoreWrapperInitProbeTests.cs` | Constructs `new StoreWrapperInitProbe(...)` (pure COM-free formatter); does NOT call `Begin`/`Init`/enumeration | MARK — P2-T7 (plan-enumerated defensive mark; not a confirmed writer, harmless serialization) |
| `StoreWrapperController_Tests` (partial) | `OutlookObjects/Store/StoreWrapperController_Tests.cs` (+ `.Launch.cs`, `.ButtonAndPopulate.cs`) | Constructs `StoreWrapperController` and `new StoreWrapper(null)`; calls `controller.Launch()`, `controller.PopulateWithCurrent()`; `SetupGet(o => o.StoresWrapper)` | MARK once on the `[TestClass]`-bearing part — P2-T8 (census member; `Launch`/populate may transitively open a scope) |
| `StoreWrapperControllerTests` | `OutlookObjects/Store/StoreWrapperControllerTests.cs` | Constructs `StoreWrapperController`, `new StoreWrapper(null)`, `new StoresWrapper()`; calls `PopulateWithCurrent()` | MARK — P2-T9 (census member; populate may transitively open a scope) |

### (b) Already carrying `[DoNotParallelize]` (no edit needed)

- `CurrentStoreContextTests` (reader + direct writer) — line 16.
- `ThreadMonitorTests` (reader + direct writer) — line 18.
- `StoreWrapperInitClockTests` (`OutlookObjects/Store/StoreWrapperInitClockTests.cs` line 16) — the plan's reference already-marked writer-adjacent class (ResourceTiming accumulator).

### (c) Type-referencing classes that do NOT open a scope (correctly NOT marked)

- `StoreDisableServiceTests` (`OutlookObjects/Store/StoreDisableServiceTests.cs`) — subclasses `StoresWrapper` as `TestableStoresWrapper` only to observe serialization; exercises `DisableSessionOnly`/`DisableForFutureSessions`/`IsDisabled`/`GetDisabledStores`/`ReenableAsync`. `ReenableAsync` uses a mocked/no-op `IStoreRehookService`; no `Init`/`Rewire`/`AddOrRestoreStore`/`Begin` executes, no `CurrentStoreContext` reference. => P2-T10 resolves to **N/A (census shows no scope-open path)**.
- `OutlookFolderHierarchyReaderTests` (`OutlookObjects/Folder/OutlookFolderHierarchyReaderTests.cs`) — constructs `new StoresWrapper { ExcludedStoreNameContains = ... }` as a plain data/predicate object passed to `ShouldInclude`; never calls `Init`/`Rewire`/`AddOrRestore`/`Begin`. Not a writer.
- `StoreLockupResponderTests` (`Threading/StoreLockupResponderTests.cs`) — reads the constant string `CurrentStoreContext.StoresEnumerationPhaseIdentity` (line 227) as a `LockupAttribution` input only; no `Begin(`, no `.Current` assertion. Neither writer nor null-baseline reader.
- Plan-unlisted Store files `DisabledStoresControllerTests`, `StoreFilterAttributionTests`, `StoreIdentityTests`, `StoreLockupAttributionTests` — do not reference `StoresWrapper`/`StoreWrapper`/`CurrentStoreContext` types and execute no scope-opening call; not writers.

### (c-cross) Cross-assembly note (vstest `/InIsolation`)

The CI invocation uses `/InIsolation`, so each vstest run hosts test execution in a separate process; the `CurrentStoreContext` process-global static is not shared across independent host processes for the pass/fail proof. Within a single assembly's run, the class-level parallel bucket is the shared-static hazard. Marking every `UtilitiesCS.Test` writer with `[DoNotParallelize]` (joining the already-marked readers in the serial bucket) removes the overlap for `UtilitiesCS.Test`, which is the assembly that turned CI red (all 10 CI failures were in `UtilitiesCS.Test`).

> CORRECTION (added at P3-T4): the initial cross-assembly claim here — that "no `CurrentStoreContext.Current` null-baseline reader class exists in any other `*.Test` assembly" — was WRONG. A repo-wide grep (not just `UtilitiesCS.Test`) shows `TaskMaster.Test` ALSO contains scope-opening writers and null-baseline readers (`StoresWrapperEnumerationScopeTests`, `AppOlObjectsAttributionContextTests`) that are unmarked. This is a pre-existing, intra-`TaskMaster.Test` instance of the same #292 race, latent under the CI `/EnableCodeCoverage` path but reproducible under `dotnet-coverage collect` (and with `TaskMaster.Test` run alone). It is OUTSIDE the plan's `UtilitiesCS.Test`-only scope and was reported, not fixed, per the execution directive. See `evidence/other/out-of-scope-finding-taskmaster-test-race.2026-07-09T16-05.md`.

### Completeness conclusion

Every CONFIRMED direct writer (`StoresWrapperTests`, `StoresWrapperRehookTests`, `StoresWrapperDisableTests`, `StoreWrapperTests`) plus the two direct-writer readers (already marked) is covered. The plan's Phase 2 set additionally marks the controller/viewer/probe classes defensively (census members that construct the store types and/or drive controller render paths). No genuine scope-opening class lies outside the plan's Phase 2 targets.
