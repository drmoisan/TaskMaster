# Census Determination (Cycle 2, Issue #292)

Timestamp: 2026-07-09T17-45

Per-class verdict for every candidate `TaskMaster.Test` class that could open a `CurrentStoreContext`
scope (directly or via `StoresWrapper.Init`/`RewireOlObjectsAsync`/`RewireAfterDeserializeAsync`/
`MaterializeFilteredStores`/`AddOrRestoreStore` or `StoreWrapper.Init`/`Restore`) and/or read the
`CurrentStoreContext` null baseline.

| Class | Verdict | Source evidence |
|---|---|---|
| `StoresWrapperEnumerationScopeTests` | **mark** | Scope-opener + null-baseline reader: `wrapper.Init()` (StoresWrapperEnumerationScopeTests.cs L43), `RewireAfterDeserializeAsync()` (L74); reads `CurrentStoreContext.Current ... BeNull()` (L115, L136). |
| `StoresWrapperTests` | **mark** | Real public-entry `RewireAfterDeserializeAsync()` invocations (StoresWrapperTests.cs L246, L287) drive `MaterializeFilteredStores` and `AddOrRestoreStore`, opening a real `CurrentStoreContext` scope. |
| `AppOlObjectsCoverageTests` | **mark** | `InvokeBaseBuildFreshStoresWrapper()` -> `base.BuildFreshStoresWrapper()` (AppOlObjectsCoverageTests.cs L203, L243-244) executes the real `new StoresWrapper(_globals).Init()` -> `MaterializeFilteredStores` path. |
| `AppOlObjectsAttributionContextTests` | **already-marked** | Class-level `[DoNotParallelize]` present (AppOlObjectsAttributionContextTests.cs L21). |
| `AppOlObjectsTests` | **not-a-writer** | Rewire is mocked via the `DelayedRewireStoresWrapper : StoresWrapper` override of `RewireAfterDeserializeAsync` (AppOlObjectsTests.cs L360-371); `new StoresWrapper()` instances (e.g., L188) are never `.Init()`-ed into a real scope. No real `CurrentStoreContext` scope open; no null-baseline read. |
| `ContinuationProbeSequenceTests` | **not-a-writer** | Broad-search match is the `StoreWrapper` substring inside `SampleStoreWrapperInitTotalMs`/`StoreWrapperInitClock`; the live clock read is no-op'd (L124). No `CurrentStoreContext` scope open. (Already carries method-level `[DoNotParallelize]` for an unrelated process-global recording reason.) |
| `ApplicationGlobalsStartupTimingTests` | **not-a-writer** | Same `StoreWrapper*` substring match; mutates `Settings.Default.StartupTimingEnabled`, already method-level `[DoNotParallelize]`; no `CurrentStoreContext` scope open. |
| `TestableApplicationGlobals` (helper, not a `[TestClass]`) | **not-a-writer** | Not a test class; `SampleStoreWrapperInitTotalMs` no-op override only. |

## Determination

Three unmarked classes must be marked `[DoNotParallelize]`: `StoresWrapperEnumerationScopeTests`,
`StoresWrapperTests`, `AppOlObjectsCoverageTests`. `AppOlObjectsAttributionContextTests` is already marked.
All other candidates are not-a-writer. Confirmed empirically by the P0-T6 pre-fix ClassLevel run, whose only
2 failures were both in `StoresWrapperEnumerationScopeTests`.
