# Pre-Fix Census Baseline (Cycle 2, Issue #292)

Timestamp: 2026-07-09T17-45

## Search commands

- `rg -l "CurrentStoreContext|\.Init\(\)|RewireOlObjectsAsync|RewireAfterDeserializeAsync|MaterializeFilteredStores|AddOrRestoreStore|StoreWrapper|\.Restore\(" TaskMaster.Test --glob *.cs`
- Per-file follow-up greps for `CurrentStoreContext`, `RewireAfterDeserializeAsync`, `new StoresWrapper`, `.Init()`, `DoNotParallelize`, and null-baseline `BeNull()` assertions.

## Candidate classes and pre-fix `[DoNotParallelize]` state

| Class | File | Scope-open evidence | Null-baseline reader | Pre-fix marking |
|---|---|---|---|---|
| `StoresWrapperEnumerationScopeTests` | `TaskMaster.Test/OutlookObjects/Store/StoresWrapperEnumerationScopeTests.cs` | `wrapper.Init()` (L43), `RewireAfterDeserializeAsync()` (L74) drive `MaterializeFilteredStores`/`AddOrRestoreStore` | Yes — `CurrentStoreContext.Current ... BeNull()` (L115, L136) | UNMARKED |
| `StoresWrapperTests` | `TaskMaster.Test/OutlookObjects/Store/StoresWrapperTests.cs` | real public `RewireAfterDeserializeAsync()` (L246, L287) | indirect | UNMARKED |
| `AppOlObjectsCoverageTests` | `TaskMaster.Test/AppGlobals/AppOlObjectsCoverageTests.cs` | `InvokeBaseBuildFreshStoresWrapper()` → `base.BuildFreshStoresWrapper()` → `new StoresWrapper(_globals).Init()` → `MaterializeFilteredStores` (L203, L243-244) | indirect | UNMARKED |
| `AppOlObjectsTests` | `TaskMaster.Test/AppGlobals/AppOlObjectsTests.cs` | rewire mocked via `DelayedRewireStoresWrapper` override (L360-371); `new StoresWrapper()` created but never `.Init()`/rewired into a real scope | No | UNMARKED (not-a-writer candidate) |
| `AppOlObjectsAttributionContextTests` | `TaskMaster.Test/AppGlobals/AppOlObjectsAttributionContextTests.cs` | reads process-global attribution context | Yes | ALREADY MARKED (`[DoNotParallelize]` L21) |

## Additional broad-search matches (substring false positives — not CurrentStoreContext scope-openers)

| Class/File | Why matched | Verdict |
|---|---|---|
| `ContinuationProbeSequenceTests` | `SampleStoreWrapperInitTotalMs`/`StoreWrapperInitClock` substring; no `CurrentStoreContext` scope open (already carries method-level `[DoNotParallelize]` for a different process-global recording reason) | not-a-writer |
| `ApplicationGlobalsStartupTimingTests` | same `StoreWrapper*` substring; mutates `Settings.Default.StartupTimingEnabled`, already method-level `[DoNotParallelize]`; no `CurrentStoreContext` scope open | not-a-writer |
| `TestableApplicationGlobals` (test helper, not a `[TestClass]`) | `SampleStoreWrapperInitTotalMs` no-op override; not a test class | not-a-writer |

## Baseline determination

Three unmarked classes open a real `CurrentStoreContext` scope and require marking:
`StoresWrapperEnumerationScopeTests`, `StoresWrapperTests`, `AppOlObjectsCoverageTests`.
`AppOlObjectsAttributionContextTests` is already marked. `AppOlObjectsTests` is not-a-writer
(rewire mocked). The three `StoreWrapper*`-substring matches are not scope-openers.
