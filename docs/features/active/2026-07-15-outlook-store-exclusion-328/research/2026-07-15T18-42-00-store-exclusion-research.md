# Research — Outlook Store Exclusion (Issue #328)

- Date: 2026-07-15
- Issue: #328
- Author: task-researcher
- Scope: Implementation-approach research for the three gaps in the existing `StoresWrapper` store-exclusion mechanism. Research and analysis only; no code changed.

## Summary of Findings

All three gaps are additive changes over an already-mature filter subsystem. The store-inclusion decision is already centralized in `StoresWrapper.ShouldIncludeStore(Outlook.Store)` and its pure sibling `StoreFilterAttribution.Decide(...)`. A prior epic (#260, issues #261/#263/#264/#292) already added a persisted-plus-session disabled-identity mechanism (`DisabledStoreIdentities` / `SessionDisabledStoreIdentities`, `StoreIdentity`, `IStoreDisableService`), so the JSON schema and the predicate already have precedent for additive fields. The recommended path for each gap:

1. Add a new `ExcludedStoreIds` list to `StoresWrapper`, checked first (authoritative), threaded through the four inclusion surfaces.
2. Thread the existing injected `IApplicationGlobals` (already reachable at every bypass call site's caller) into the four enumeration sites and replace raw `Session.Stores` filtering with `StoresWrapper.ShouldIncludeStore`.
3. Add a checkbox toggle to `StoreWrapperViewer`/`IStoreWrapperViewer`, handled in `StoreWrapperController`, that adds/removes the current store's `StoreID` from `Model.ExcludedStoreIds` and persists via the existing `Model.Serialize()`.

## Current State (verified)

### The centralized predicate

`UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs` (`SmartSerializable<StoresWrapper>`) holds the config fields and three inclusion surfaces that must stay in lockstep:

- `ShouldIncludeStore(Outlook.Store store)` — instance predicate (lines 329-391). Short-circuit order: public-folder → `ExcludedStoreNameContains` (DisplayName substring) → `ExcludeGwsoStores`/`GwsoFilePathContains` (FilePath substring) → `ExcludedStoreFilePathContains` (FilePath substring) → `IsEffectivelyDisabled(...)` (issue #261). Returns `true` if none match.
- `StoreIsIncluded(...)` — a `static` overload (lines 257-327) taking the config lists as explicit parameters; mirrors the same order.
- `StoreFilterAttribution.Decide(...)` — the pure, COM-free decision over already-read primitives (`UtilitiesCS/OutlookObjects/Store/StoreFilterAttribution.cs`, lines 59-118), returning `(bool Included, StoreFilterRule Rule)`. `StoreFilterRule` (lines 14-33) enumerates the rules in short-circuit order with `Included` last.
- `ShouldIncludeStoreInstrumented(...)` — the diagnostics wrapper actually used by `GetFilteredStores()` (lines 203-255); it reads primitives once and delegates the decision to `StoreFilterAttribution.Decide`.

Config fields, all `[JsonProperty]` with field-initializer defaults (lines 401-424):
- `bool ExcludePublicFolderStores = true`
- `bool ExcludeGwsoStores = true`
- `List<string> GwsoFilePathContains` (two default tokens)
- `List<string> ExcludedStoreNameContains = []`
- `List<string> ExcludedStoreFilePathContains = []`
- `List<string> DisabledStoreIdentities = []` (persisted, issue #261)
- `HashSet<string> SessionDisabledStoreIdentities` (`[JsonIgnore]`, session-only)

`GetFilteredStores()` (lines 187-192) enumerates `Globals.Ol.NamespaceMAPI.Stores` and applies `ShouldIncludeStoreInstrumented`. `MaterializeFilteredStores()` wraps it in a `CurrentStoreContext` enumeration-phase scope (issue #292 STA-lockup attribution).

### Wiring and persistence

- Inbox loading: `TaskMaster/AppGlobals/AppOlObjects.cs` `LoadInboxes()` (lines 119-143) uses `StoresWrapper.ShouldIncludeStore` per store (via `ResolveInboxForStore`). This is the one site already routed through the filter.
- `StoresWrapper` is exposed on `IOlObjects.StoresWrapper` (`UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs:24`) as `Globals.Ol.StoresWrapper`, and persisted as JSON under the `"StoresWrapper"` intelligence-resources config key. It round-trips through `SmartSerializable`/Newtonsoft.
- The disable service is exposed on `IApplicationGlobals.StoreDisable` (`IApplicationGlobals.cs:23`, `IStoreDisableService`). `StoreDisableService` reads `Globals.Ol.StoresWrapper` lazily per call and never caches it.

### The store-identity mechanism (issue #261, do not conflate with #328)

`StoreIdentity` (`UtilitiesCS/OutlookObjects/Store/StoreIdentity.cs`) is a `readonly struct` whose `Resolve(displayName, filePathFallback)` prefers **DisplayName**, falls back to FilePath, else returns `UnresolvedSentinel`. It deliberately performs no COM access on its pure overload because F3/F4/F5 (the lockup responders) must not trigger a blocking FilePath-class COM read during detection. This identity is **DisplayName-based, not StoreID-based**, and drives the `DisabledStoreIdentities` disable/reenable feature. Issue #328's StoreID exclusion is a separate, user-driven, persistent exclusion; see the Rejected-alternatives note under Gap 1 for why these should not be merged.

## Gap 1 — Exact StoreID exclusion

### `Store.StoreID` semantics (COM interop)

`Microsoft.Office.Interop.Outlook.Store.StoreID` returns a `string`: the MAPI entry-ID of the store, rendered as an uppercase hexadecimal string (typically 100+ hex characters). Properties of the value relevant to this design:

- It is unique per store within an Outlook profile and is the same value returned by `Folder.StoreID` for folders in that store.
- It is stable across Outlook **sessions** for a given profile/account (it is the entry-ID persisted in the profile), so it satisfies the "persists across sessions" acceptance criterion.
- It is **not** guaranteed stable across profile recreation, account removal/re-add, or a different machine/profile. This is the same profile-scoping limitation the DisplayName/FilePath substrings already have, and it is acceptable because the exclusion is a per-profile user preference persisted in the per-profile config.
- Reading `StoreID` is a COM property read on the STA. It is generally cheaper than `FilePath`/`ExchangeStoreType`, but it must still be treated as a COM call: guard it with a try/catch mirroring the existing `FilePath` guard (lines 351-356), and treat a read failure as "no StoreID match" (do not exclude on an unread ID).

### Recommended approach

Add one additive config field and thread it through the four inclusion surfaces as the **first** (authoritative) check.

1. New field on `StoresWrapper` (place beside the other exclusion lists, ~line 415):
   ```csharp
   [JsonProperty]
   public List<string> ExcludedStoreIds { get; set; } = [];
   ```
   Default empty list. Membership comparison should be `StringComparison.OrdinalIgnoreCase` for robustness (MAPI entry-IDs are conventionally uppercase, but case-insensitive matching is safe and matches the tone of the other list checks). Each candidate value guarded with `!string.IsNullOrWhiteSpace(x)` exactly like the substring lists.

2. New enum member on `StoreFilterRule` (first member so attribution reflects the authoritative order):
   ```csharp
   /// <summary>Excluded because the store's StoreID matched a configured excluded StoreID.</summary>
   StoreId,
   ```

3. Extend `StoreFilterAttribution.Decide(...)` with two new leading parameters `string storeId` and `IList<string> excludedStoreIds`, and add the check as the first branch:
   ```csharp
   if (excludedStoreIds is not null
       && !string.IsNullOrWhiteSpace(storeId)
       && excludedStoreIds.Any(x =>
           !string.IsNullOrWhiteSpace(x)
           && string.Equals(x, storeId, StringComparison.OrdinalIgnoreCase)))
   {
       return (false, StoreFilterRule.StoreId);
   }
   ```
   Exact-match (`string.Equals`), not substring — this is the precision the feature is buying.

4. Add the same first-check to the instance `ShouldIncludeStore` and the `static StoreIsIncluded` overload (new `storeId` + `excludedStoreIds` parameters), guarding the `store.StoreID` read with try/catch. In `ShouldIncludeStoreInstrumented`, read `store.StoreID` (guarded) alongside the existing primitive reads and pass it into `Decide`.

Because the check is exact-match and placed first, it never interferes with the existing substring/public-folder/GWSO/disabled rules; a store not matched by StoreID falls through to the unchanged logic.

### Backward compatibility

- Newtonsoft invokes the parameterless constructor and only assigns properties present in the JSON. Old config JSON has no `ExcludedStoreIds` key, so the field-initializer default (`[]`) is retained — identical behavior to today. This mirrors exactly how `DisabledStoreIdentities` was added in #261 (see `StoresWrapperDisableTests` round-trip assertions).
- The new field round-trips through the existing `"StoresWrapper"` config key; no new file or config key is introduced.
- The `static StoreIsIncluded` and `Decide` signatures gain parameters. `StoreIsIncluded` has no in-repo callers other than tests (verify during implementation); `Decide` is called from `ShouldIncludeStoreInstrumented` and from `StoreFilterAttributionTests`. Adding required parameters is a compile-time-checked change confined to this assembly and its test project. Prefer adding the parameters (keeping one code path) over an overload, to avoid divergent decision logic.

### Rejected alternative (Gap 1)

Reusing the #261 `StoreIdentity`/`DisabledStoreIdentities` path by making `Resolve` prefer `StoreID`. Rejected: (a) it would change the identity semantics of the lockup disable/reenable feature, which intentionally avoids COM reads and keys on DisplayName; (b) it conflates a user's durable "never process this mailbox" choice with the runtime "temporarily disabled because it locked up" state, which have different lifecycles and different UIs; (c) the pure `StoreIdentity.Resolve(string,string)` overload used by F3/F4/F5 cannot obtain a StoreID without a prohibited blocking COM read. Keep the two mechanisms separate.

## Gap 2 — Route the four bypass sites through the filter

The common shape at every site is `olApp.Session.Stores.Cast<Store>()` (or `Application.Session.Stores`) with no filter (or a public-folder-only filter). `Session` and `NamespaceMAPI` are the same MAPI namespace, so `ShouldIncludeStore` applies identically. The predicate is an instance method on `StoresWrapper`, reachable as `globals.Ol.StoresWrapper`. Every bypass site's **caller** already holds an injected `IApplicationGlobals`; the minimal change threads it (or the resolved `StoresWrapper`) to the enumeration method.

Recommended predicate expression at each site (null-safe so a not-yet-loaded model does not over-exclude):
```csharp
.Where(store => storesWrapper is null || storesWrapper.ShouldIncludeStore(store))
```

### Site A — `ToDoModel/Data Model/Tree/TreeOfToDoItems.cs`

- `GetToDoList(LoadOptions, Application)` (line 161) enumerates `Application.Session.Stores.Cast<Store>()` with a public-folder-only `.Where` (lines 170-174). `GetToDoListAsync(LoadOptions, Application)` (line 209) has the same bypass with no filter at all.
- Reachability: the only caller, `LoadTree(LoadOptions, IApplicationGlobals appGlobals)` (line 58), already holds `appGlobals` and calls `GetToDoList(LoadType, appGlobals.Ol.App)` (line 61).
- Minimal change: pass the filter into both enumeration methods. Cleanest is to add an `IApplicationGlobals appGlobals` (or `StoresWrapper storesWrapper`) parameter to `GetToDoList`/`GetToDoListAsync`, resolve `appGlobals.Ol.StoresWrapper`, and add the `.Where(...)` predicate above (the public-folder-only `.Where` becomes redundant because `ShouldIncludeStore` already excludes public folders when configured — keep or drop it, but the centralized predicate is authoritative). `LoadTree` passes `appGlobals`. `TreeOfToDoItems` is `[ExcludeFromCodeCoverage]`, so the coverage burden is minimal; still, prefer threading `StoresWrapper` explicitly so a unit test can pass a real `StoresWrapper` over a mocked `Stores` collection (the `StoresWrapperTests.CreateGlobalsWithStores` helper is the established pattern).

### Site B — `ToDoModel/Data Model/ToDo/ToDoEvents.cs`

- `ToDoEvents` is a `static` `[ExcludeFromCodeCoverage]` class. Three static methods enumerate `Session.Stores`:
  - `GetListOfToDoItemsInView(Outlook.Application)` (line 101, raw enumerate at line 114) — issue-cited ":112".
  - `GetToDoItemsInView(Outlook.Application)` (line 145, raw enumerate at line 158) — issue-cited ":156".
  - `GetAsyncEnumerableOfToDoItemsInView(Outlook.Application)` (line 127, raw enumerate at lines 133-134).
- Caller analysis (grep across repo): `GetListOfToDoItemsInView` and `GetToDoItemsInView` have **no callers** in the codebase and are annotated "QUESTION: When is this called? Is it needed?" / "Depricated?". The **live** bypass is `GetAsyncEnumerableOfToDoItemsInView`, called by `RefreshToDoIdSplitsAsync(Outlook.Application)` (line 212), which is called from `TaskMaster/Ribbon/RibbonController.cs:85` as `await ToDoEvents.RefreshToDoIdSplitsAsync(Globals.Ol.App)`.
- Reachability: `RibbonController` holds `Globals`. Minimal change for the live path: change `RefreshToDoIdSplitsAsync` and `GetAsyncEnumerableOfToDoItemsInView` to accept `IApplicationGlobals globals`, use `globals.Ol.App` for the view/session and `globals.Ol.StoresWrapper.ShouldIncludeStore` in the `.Where`; `RibbonController` passes `Globals`.
- For the two dead methods the issue explicitly names: add the same `IApplicationGlobals` parameter and filter predicate for consistency (the atomic plan should note they appear dead and consider whether to delete them instead; deletion is out of scope for this feature and should be a separate issue if pursued). Since they are static with no callers, threading a parameter is safe.

### Site C — `ToDoModel/Data Model/Project/ProjectData.cs`

- `Rebuild(Outlook.Application olApp)` (line 245) enumerates `olApp.Session.Stores` with no filter (line 248) and calls `GetDfToDo(store)` per store.
- `ProjectData` (a `[Serializable]` `SerializableList<IProjectEntry>`) has no globals field; its constructors take filenames/lists only.
- Callers all hold globals: `TaskMaster/AppGlobals/AppToDoObjects.cs:121` (`_projInfo.Rebuild(outlookApplication)`) and `:135` (`projectInfo.Rebuild(Parent.Ol.App)`), and `TaskMaster/Ribbon/TryFunctionalityInConstruction.cs:83` (`AppGlobals.TD.ProjInfo.Rebuild(AppGlobals.Ol.App)`).
- Minimal change: add an overload `Rebuild(IApplicationGlobals globals)` (or add a second parameter `StoresWrapper storesWrapper`) that resolves the filter and applies `.Where(store => storesWrapper is null || storesWrapper.ShouldIncludeStore(store))` before `GetDfToDo`. Update the three callers to pass globals / the resolved `StoresWrapper`. Passing `StoresWrapper` directly keeps `ProjectData` free of an `IApplicationGlobals` dependency and is the more testable seam (a unit test constructs a `ProjectData`, calls `Rebuild` with a real `StoresWrapper` over mocked stores, and asserts the excluded store's `GetDfToDo` path is not taken).

### Recommended threading choice

Prefer passing the concrete `StoresWrapper` (or a narrow `Func<Outlook.Store,bool>` predicate) into the enumeration methods rather than the whole `IApplicationGlobals`, for these reasons: (a) it is the smallest seam; (b) it keeps `ProjectData`/`TreeOfToDoItems` from acquiring a broad aggregate dependency; (c) it matches the established test helper `CreateGlobalsWithStores` which already builds a `StoresWrapper` over a mocked `Stores` collection. Where a caller finds it simpler, passing `IApplicationGlobals` and resolving `.Ol.StoresWrapper` inside is acceptable and consistent with `LoadTree`'s existing signature. This satisfies the constraint "centralize all store-access decisions in `StoresWrapper.ShouldIncludeStore`; do not add parallel filtering logic in the ToDoModel sites."

### Rejected alternative (Gap 2)

An ambient/static `StoresWrapper` singleton reachable without threading a parameter. Rejected: no such ambient singleton exists in this codebase — the store model is always reached through the injected `IApplicationGlobals` aggregate (`Globals.Ol.StoresWrapper`). Introducing a static global would violate the DI conventions used throughout (`StoreDisableService`, `StoreWrapperController`, `AppOlObjects` all read the model off the injected aggregate) and would be untestable without global mutable state (banned by the unit-test policy).

## Gap 3 — UI toggle in `StoreWrapperController`

### Current UI shape

`StoreWrapperController` (`UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs`) drives `IStoreWrapperViewer` (a WinForms `Form`, `StoreWrapperViewer`). Flow:
- `Launch()` (line 116, `[ExcludeFromCodeCoverage]`) gates on `EvaluateLaunchReadiness()`, then binds `Viewer.DisplayName.DataSource` to the store display names and shows the dialog.
- `DisplayName_SelectedValueChanged` (line 152) sets `Current = Model.Stores.Find(s => s.DisplayName == displayName)` then `PopulateWithCurrent()` (line 249) which fills the labels for archive/junk folders.
- Per-field click handlers (`ArchiveFS_Click`, `ArchiveOutlook_Click`, `JunkEmail_Click`, `JunkPotential_Click`) mutate controller state; `AnyChanges()` (line 228) compares controller state to `Current`; `SaveChanges()` (line 285) writes back to `Current` and calls `Model.Serialize()` (line 292).

`StoreWrapper` (the `Current` type) carries `DisplayName` and the live `InnerStore` (`Outlook.Store`), but does **not** currently persist a `StoreID`. `Current.InnerStore.StoreID` is reachable at UI time because `Launch()` only proceeds when readiness is `Ready` (model populated and rewired, `InnerStore` set).

### Recommended approach

Add a boolean "Exclude this store" affordance that toggles the current store's `StoreID` in `Model.ExcludedStoreIds` (the Gap 1 authoritative list) and persists through the existing `Model.Serialize()` path.

1. View surface: add a `CheckBox ExcludeStore` to `StoreWrapperViewer.Designer.cs`, expose it on `IStoreWrapperViewer` (mirroring the existing control properties), and forward its `CheckedChanged` event to the controller in `StoreWrapperViewer` (mirroring `DisplayName_SelectedValueChanged` forwarding). The Designer and `Form`-derived class are WinForms-exempt from coverage; keep the code-behind a thin forwarder.
2. Controller: 
   - In `PopulateWithCurrent()`, set the checkbox state from `Model.ExcludedStoreIds.Contains(currentStoreId, OrdinalIgnoreCase)` where `currentStoreId` is read from `Current.InnerStore.StoreID` (guarded try/catch; if the ID is unreadable, disable the checkbox rather than risk a wrong toggle).
   - Add a handler (e.g. `ExcludeStore_CheckedChanged` or fold into `SaveChanges`/`AnyChanges`) that, on OK/save, adds the StoreID to `Model.ExcludedStoreIds` when checked and removes it when unchecked, then `Model.Serialize()`. Extending `AnyChanges()`/`SaveChanges()` keeps the persistence path single and consistent with the existing archive/junk save; the existing "Save changes?" prompt on selection change then also covers the exclusion toggle.
3. Persistence: reuse `Model.Serialize()` (already the persistence primitive in `SaveChanges`). No new persistence path.

The controller is unit-testable today via a mocked `IStoreWrapperViewer` (`StoreWrapperController_Tests.*`), so the add/remove-and-persist logic can be covered with Moq + a real `StoresWrapper` model and a `Mock<IStoreWrapperViewer>` asserting the checkbox binding and `Model.ExcludedStoreIds` mutation. To make the StoreID readable in a unit test without live COM, either (a) read `Current.InnerStore.StoreID` (mockable — `Mock<Outlook.Store>().SetupGet(s => s.StoreID)`, as done in `DfDeedle_COM_Tests`), or (b) add a persisted `string StoreId` to `StoreWrapper` captured in `Init()` (additive to JSON, mirrors `DisplayName` capture at `StoreWrapper.cs:37`). Option (b) is slightly more work but removes the live-COM dependency from the controller path and gives a stable value even after deserialize-before-rewire; recommend (b) if the atomic plan can absorb the `StoreWrapper` field, otherwise (a).

### Rejected alternative (Gap 3)

Routing the UI toggle through `IStoreDisableService` (`Globals.StoreDisable`). Rejected: that service keys on the DisplayName-based `StoreIdentity` and models session-vs-future scopes for the lockup feature; using it for the durable StoreID exclusion would create two divergent exclusion sources and reintroduce the DisplayName brittleness this feature is meant to remove. The UI should toggle `ExcludedStoreIds` directly.

## State model and precedence (proposed)

Effective inclusion for a store, evaluated in `ShouldIncludeStore` / `Decide` in this order:
1. `ExcludedStoreIds` exact StoreID match → excluded (NEW, authoritative).
2. Public-folder store and `ExcludePublicFolderStores` → excluded.
3. `ExcludedStoreNameContains` DisplayName substring → excluded.
4. `ExcludeGwsoStores` + `GwsoFilePathContains` FilePath substring → excluded.
5. `ExcludedStoreFilePathContains` FilePath substring → excluded.
6. `IsEffectivelyDisabled` (session or persisted disabled identity) → excluded.
7. Otherwise → included.

Enumeration surfaces that must all reflect this order: `ShouldIncludeStore`, `StoreIsIncluded` (static), `StoreFilterAttribution.Decide`, and `ShouldIncludeStoreInstrumented` (which reads the primitives and calls `Decide`). Adding the StoreID branch to all four in the same PR is required to keep them consistent.

## Required file changes (map)

- `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs` — add `ExcludedStoreIds`; add StoreID branch to `ShouldIncludeStore`, `StoreIsIncluded`, and read `StoreID` in `ShouldIncludeStoreInstrumented`.
- `UtilitiesCS/OutlookObjects/Store/StoreFilterAttribution.cs` — add `StoreFilterRule.StoreId`; add `storeId`/`excludedStoreIds` params and first-branch logic to `Decide`.
- `ToDoModel/Data Model/Tree/TreeOfToDoItems.cs` — thread filter into `GetToDoList`/`GetToDoListAsync`; update `LoadTree` call.
- `ToDoModel/Data Model/ToDo/ToDoEvents.cs` — thread filter into `GetAsyncEnumerableOfToDoItemsInView` + `RefreshToDoIdSplitsAsync` (live) and the two dead methods (issue-named).
- `TaskMaster/Ribbon/RibbonController.cs` — pass `Globals` to `RefreshToDoIdSplitsAsync`.
- `ToDoModel/Data Model/Project/ProjectData.cs` — add filtered `Rebuild` overload/param.
- `TaskMaster/AppGlobals/AppToDoObjects.cs` (x2 call sites) and `TaskMaster/Ribbon/TryFunctionalityInConstruction.cs` — pass filter to `Rebuild`.
- `UtilitiesCS/OutlookObjects/Store/IStoreWrapperViewer.cs`, `StoreWrapperViewer.cs`, `StoreWrapperViewer.Designer.cs` — add `ExcludeStore` checkbox + event forwarding.
- `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` — bind checkbox in `PopulateWithCurrent`, mutate `Model.ExcludedStoreIds` in save path.
- Optional: `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs` — add persisted `StoreId` captured in `Init()` (recommended for testability of Gap 3).

## Behavior semantics / edge cases

- StoreID read failure (COM throw): treat as no match; never exclude a store whose StoreID could not be read (fail-open, consistent with the `FilePath` try/catch pattern). In the UI, disable the checkbox when `StoreID` is unreadable.
- Empty/whitespace entries in `ExcludedStoreIds`: ignored via `!string.IsNullOrWhiteSpace` guard (matches the substring lists).
- Not-yet-loaded model at a bypass site (`Globals.Ol.StoresWrapper` null before async load completes): the null-safe `.Where` predicate includes all stores (fail-open) rather than throwing or over-excluding — matches `AppOlObjects.LoadInboxes` which falls back to `new StoresWrapper()` (line 121).
- Precedence: a StoreID-excluded store is excluded regardless of the substring/public-folder/GWSO/disabled rules; a store not StoreID-matched behaves exactly as today.
- Case-insensitivity: MAPI entry-IDs are conventionally uppercase; use `OrdinalIgnoreCase` to be robust to any casing differences in hand-edited config.
- Duplicate toggling in UI: adding an already-present StoreID is a no-op (guard with `Contains`), removing an absent one is a no-op; only `Serialize()` when the list actually changed (mirrors `StoreDisableService.DisableForFutureSessions`/`ReenableAsync` idempotency).
- Cross-session persistence: `ExcludedStoreIds` round-trips through the `"StoresWrapper"` config key; StoreID is stable within the profile, so exclusion survives restart.

## Testing implications (MSTest + Moq + FluentAssertions)

Extend `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs` and siblings. The existing `CreateStore(displayName, filePath, smtp, exchangeStoreType, throwOnFilePathAccess)` helper should gain a `storeId` setup (`store.SetupGet(x => x.StoreID).Returns(storeId)`), matching the `DfDeedle_COM_Tests` `SetupGet(f => f.StoreID)` pattern.

Gap 1:
- StoreID exact match excludes; a near-but-not-equal StoreID does not (proves exact-match, not substring).
- StoreID exclusion takes precedence / is independent of name/path/public-folder/GWSO/disabled rules (a store excluded only by StoreID is excluded; a store matched by StoreID plus other rules is still excluded and attributed `StoreFilterRule.StoreId`).
- `GetFilteredStores()`/`Init()` omits a StoreID-excluded store (extend the existing `Init_WhenStoresMatchFilters_ProjectsOnlyIncludedStores` shape).
- StoreID read throws → store included (fail-open); empty/whitespace list entries ignored.
- JSON round-trip: `ExcludedStoreIds` serialized and restored; absent key in legacy JSON → empty default (mirror `StoresWrapperDisableTests` round-trip test).
- `StoreFilterAttribution.Decide` unit tests for the new branch and `StoreId` rule attribution (extend `StoreFilterAttributionTests`).

Gap 2 (unit, no live COM — use `CreateGlobalsWithStores`/real `StoresWrapper` over mocked `Stores`):
- `TreeOfToDoItems.GetToDoList`/`GetToDoListAsync`: an excluded StoreID store's ToDo folder is not enumerated.
- `ProjectData.Rebuild`: an excluded store's `GetDfToDo` path is not taken (assert via a spy/derived `ProjectData` or by observing the resulting frame excludes the store's rows).
- `ToDoEvents.GetAsyncEnumerableOfToDoItemsInView` (live path): excluded store's items absent. Note `ToDoEvents`/`TreeOfToDoItems` are `[ExcludeFromCodeCoverage]`; prefer testing the filter effect through the `StoresWrapper` seam so the coverage lands on `StoresWrapper` (not exempt).

Gap 3 (controller unit tests with `Mock<IStoreWrapperViewer>`, extend `StoreWrapperController_Tests.*`):
- Selecting a store binds the checkbox to current `ExcludedStoreIds` membership.
- Toggling on + save adds the StoreID and calls `Model.Serialize()`; toggling off + save removes it and serializes.
- Idempotency: no duplicate add, no serialize when unchanged.
- Unreadable StoreID → checkbox disabled / no mutation.

All tests must be deterministic, independent, and avoid temp files (policy). Coverage: new/changed lines in the non-exempt `StoresWrapper`, `StoreFilterAttribution`, and `StoreWrapperController` paths must meet the repo floor; the WinForms Designer/form additions are covered by the WinForms exemption.

## Automation Feasibility

This research and the resulting implementation are pure in-repo C#/WinForms work. No third-party UI (Azure portal, Entra, Outlook desktop/mobile UI, M365 admin center) is touched, and no human-interaction requirements are introduced by this research or by the recommended approach.
