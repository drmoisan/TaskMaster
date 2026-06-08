Timestamp: 2026-05-07T09:48:37.0903487-04:00

Files Inspected:
- TaskMaster/AppGlobals/ApplicationGlobals.cs
- TaskMaster/AppGlobals/AppOlObjects.cs
- TaskMaster/AppGlobals/AppToDoObjects.cs
- UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs

Coverage Source: docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/evidence/qa-gates/csharp-coverage-summary.2026-05-06T22-59-53-04-00.md

---

## Invariant 1 — Cooperative Yield Points Between Heavy Startup Phases

Yield Points Found: true

Every heavy startup phase boundary in `ApplicationGlobals.LoadSequentialAsync()` is separated by an explicit `await YieldBetweenStartupPhasesAsync()` call, which in turn executes `await Task.Yield()`. Additional per-store yields exist inside `StoresWrapper.RewireOlObjectsAsync()`.

Yield point locations:

1. `ApplicationGlobals.cs` — `LoadSequentialAsync()`:
   - After `LoadIntelConfigPhaseAsync()` → `await YieldBetweenStartupPhasesAsync()`
   - After `LoadOlObjectsPhaseAsync()` → `await YieldBetweenStartupPhasesAsync()`
   - After `LoadToDoPhaseAsync()` → `await YieldBetweenStartupPhasesAsync()`
   - After `LoadAutoFilePhaseAsync()` → `await YieldBetweenStartupPhasesAsync()`
   - After `InitializeEnginesPhaseAsync()` → `await YieldBetweenStartupPhasesAsync()`
   - After `LoadEventsPhaseAsync()` — sequential return (method ends)

2. `ApplicationGlobals.cs` — `YieldBetweenStartupPhasesAsync()`:
   - Body: `await Task.Yield()` — explicit cooperative-yield implementation

3. `StoresWrapper.cs` — `RewireOlObjectsAsync()`:
   - Inside `foreach (var store in stores)`: `if (processedStoreCount > 0) { await Task.Yield(); }` — yields before every store iteration after the first, preserving UI-thread responsiveness between per-store rewire operations.

---

## Invariant 2 — Awaitable Store-Rewire Completion Contract (No `async void` Rewire)

Awaitable Rewire Contract: true

The store-rewire call chain uses properly awaitable `Task`-returning methods throughout the caller path. No `async void` rewire method exists that would obscure completion to callers.

Call chain:

1. `AppOlObjects.cs` — `LoadStoresAsync()`: after deserialization, calls `await AwaitStoreRewireAsync(StoresWrapper)` — explicit await of the rewire result.
2. `AppOlObjects.cs` — `AwaitStoreRewireAsync(StoresWrapper storesWrapper)`: returns `storesWrapper.RewireAfterDeserializeAsync()` — `protected internal virtual Task` return type.
3. `StoresWrapper.cs` — `RewireAfterDeserializeAsync()`: `public virtual Task` — awaitable; calls and returns `RewireOlObjectsAsync(default)`.
4. `StoresWrapper.cs` — `RewireOlObjectsAsync(StreamingContext context)`: `internal async Task` — performs all COM-bound rewire work on the calling (UI) thread; awaited by the chain above.
5. `StoresWrapper.cs` — `RewireOlObjects(StreamingContext context)` (`[OnDeserialized]` hook): `public void` (NOT `async void`) — fires `_ = RewireAfterDeserializeWithLoggingAsync()` for the deserialization-framework callback. This is not the load-path completion contract; it is a separate framework entry point. The load-path completion is exclusively controlled by the explicit `await AwaitStoreRewireAsync(StoresWrapper)` in `LoadStoresAsync()`.
6. `StoresWrapper.cs` — `RewireAfterDeserializeWithLoggingAsync()`: `private async Task` — awaitable, error-logged; not `async void`.

---

## Invariant 3 — No New `Task.Run` Delegate Directly Referencing Outlook COM Objects

Background COM Access Risk: none

All `Task.Run` lambda bodies in the four inspected files access only filesystem paths, pure configuration objects, deserialization helpers, or in-memory data. Outlook COM objects (`Application`, `NamespaceMAPI`, `Store`, `Folder`, `Items`, and COM-backed wrappers) are not referenced inside any `Task.Run` lambda body. Where a COM reference is captured in an outer variable before a `Task.Run` call, it is used only after the await returns (on the caller/UI thread), not inside the lambda body.

Per-call-site analysis:

| File | Method | Task.Run lambda body | COM reference in lambda body? |
|---|---|---|---|
| `ApplicationGlobals.cs` | `InitializeEnginesPhaseAsync()` | `() => Engines.InitAsync()` — calls external method; no `App`, `NamespaceMAPI`, `Store`, `Folder`, or `Items` reference in the lambda itself | No |
| `AppOlObjects.cs` | `LoadEmailMoveWriter()` | `async (items) => await FileIO2.WriteTextFileAsync(...)` — file I/O only | No |
| `AppToDoObjects.cs` | `LoadProjInfoAsync()` | `() => { … ProjectData(filename, folderpath); proj.Sort(); … }` — `Parent.FS.SpecialFolders` (filesystem), `ProjectData` ctor with strings; `outlookApplication` is captured but the lambda body does NOT reference it | No |
| `AppToDoObjects.cs` | `LoadIdListAsync()` | `() => (IIDList)LoadIdListFromDisk(appData)` — `appData` is a `string`; disk file read; no COM | No |
| `AppToDoObjects.cs` | `LoadPeopleAsync()` | `async () => { Parent.IntelRes.Config.TryGetValue(…); SmartSerializable.DeserializeAsync(…); … }` — `Parent.IntelRes.Config` is a pure config dictionary; `Parent.AF.CancelToken` is a `CancellationToken`; no COM | No |
| `AppToDoObjects.cs` | `LoadProgramInfoAsync()` | `Task.Run(LoadProgramInfo)` — `LoadProgramInfo()` accesses `Parent.FS.SpecialFolders` and deserializes from disk; no COM | No |
| `AppToDoObjects.cs` | `LoadDictRemapAsync()` | `Task.Run(LoadDictRemap, default)` — `LoadDictRemap()` accesses `Parent.FS.SpecialFolders` and constructs in-memory dictionary from disk; no COM | No |
| `AppToDoObjects.cs` | `LoadCategoryFiltersAsync()` | `Task.Run(() => { … new SerializableList<string>(…) … })` — filesystem path lookup and object construction; no COM | No |
| `AppToDoObjects.cs` | `LoadPrefixListAsync()` | `Task.Run(LoadPrefixList)` — `LoadPrefixList()` accesses `Parent.FS.SpecialFolders` and deserializes; no COM | No |
| `AppToDoObjects.cs` | `LoadFilteredFolderScrapingAsync()` | `Task.Run(() => LoadFilteredFolderScraping(), default)` — filesystem-only; no COM | No |
| `StoresWrapper.cs` | All methods | No `Task.Run` call sites present in this file | N/A |

`RewireOlObjectsAsync()` in `StoresWrapper.cs` accesses `Globals.Ol.NamespaceMAPI.Stores` (COM) and `store.DisplayName`, `store.FilePath`, `store.ExchangeStoreType` (COM properties), but this runs directly on the calling thread, not inside a `Task.Run` delegate.

---

## Invariant 4 — Changed/New-Code Coverage ≥ 90.0

Coverage Meets Threshold: true (94.8276)

Source: `csharp-coverage-summary.2026-05-06T22-59-53-04-00.md`
- Final Repo Coverage: 76.1473
- Changed/New-Code Coverage: 94.8276
- Coverage Conclusion: PASS

---

## COM Safety Source Citations

Yield invariant:
- `ApplicationGlobals.cs` — `LoadSequentialAsync()`: lines containing `await YieldBetweenStartupPhasesAsync()` between each of the six phase calls
- `ApplicationGlobals.cs` — `YieldBetweenStartupPhasesAsync()`: body `await Task.Yield()`
- `StoresWrapper.cs` — `RewireOlObjectsAsync()`: `if (processedStoreCount > 0) { await Task.Yield(); }` inside foreach loop

Awaitable rewire contract:
- `AppOlObjects.cs` — `LoadStoresAsync()`: `await AwaitStoreRewireAsync(StoresWrapper)`
- `AppOlObjects.cs` — `AwaitStoreRewireAsync(StoresWrapper storesWrapper)`: `protected internal virtual Task` returning `storesWrapper.RewireAfterDeserializeAsync()`
- `StoresWrapper.cs` — `RewireAfterDeserializeAsync()`: `public virtual Task` body
- `StoresWrapper.cs` — `RewireOlObjectsAsync(StreamingContext context)`: `internal async Task` signature
- `StoresWrapper.cs` — `RewireOlObjects(StreamingContext context)`: `public void` signature confirming not `async void`

Background COM safety:
- `ApplicationGlobals.cs` — `InitializeEnginesPhaseAsync()`: lambda `() => Engines.InitAsync()` — no COM symbol
- `AppToDoObjects.cs` — `LoadProjInfoAsync()`: lambda body `{ var proj = new ProjectData(filename:…, folderpath:…); proj.Sort(); return proj; }` — `outlookApplication` identifier absent from lambda body
- `AppToDoObjects.cs` — `LoadIdListAsync()`: lambda `() => (IIDList)LoadIdListFromDisk(appData)` — `appData` is `string`
- `AppToDoObjects.cs` — `LoadPeopleAsync()`: lambda body `{ Parent.IntelRes.Config.TryGetValue(…); SmartSerializable.DeserializeAsync(…); }` — no `App`, `NamespaceMAPI`, `Store`, `Folder`, `Items` symbol
- `StoresWrapper.cs` — `RewireOlObjectsAsync()`: COM access (`Globals.Ol.NamespaceMAPI.Stores`, `store.DisplayName`, `store.ExchangeStoreType`) is direct method body execution on calling thread; no `Task.Run` present in this file

---

Static Analysis Conclusion: PASS
