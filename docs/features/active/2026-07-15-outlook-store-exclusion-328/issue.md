# outlook-store-exclusion (Issue #328)

- Date captured: 2026-07-15
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/outlook-store-exclusion/ (Issue #328)

- Issue: #328
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/328
- Last Updated: 2026-07-15
- Work Mode: full-feature

## Problem / Why

`StoresWrapper` (UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs:150) already implements a store deny-list (`ExcludedStoreNameContains`, `ExcludedStoreFilePathContains`, public-folder and GWSO store exclusion) with a `ShouldIncludeStore` predicate and a `GetFilteredStores()` enumeration. It is wired into inbox loading only (TaskMaster/AppGlobals/AppOlObjects.cs:101), and persisted under the `StoresWrapper` config key (AppOlObjects.cs:127-145).

Four other enumeration sites iterate `Session.Stores` directly and bypass the filter, so an excluded store is still processed by the to-do tree, to-do events, and project data scanning:
- ToDoModel/Data Model/Tree/TreeOfToDoItems.cs:168
- ToDoModel/Data Model/ToDo/ToDoEvents.cs:112 and :156
- ToDoModel/Data Model/Project/ProjectData.cs:248

Matching is name/path-substring based only, which is imprecise and brittle across Outlook profiles. There is also no UI to toggle a store's exclusion; `StoreWrapperController` only edits per-store archive/junk folder assignments, so users must hand-edit the JSON config to add or remove an exclusion.

## Proposed Behavior

1. Add exact `StoreID`-based exclusion to `StoresWrapper` as the authoritative identifier, keeping existing name/path substring options as fallbacks.
2. Route the four bypass sites through the existing `ShouldIncludeStore` predicate instead of iterating `Session.Stores` directly, so store-access decisions stay centralized in `StoresWrapper`.
3. Add a UI affordance in `StoreWrapperController` (or its view) letting a user select a loaded store and toggle exclude/re-include, persisting to the existing `StoresWrapper` JSON config.

## Acceptance Criteria (early draft)

- [ ] A specific store/mailbox can be excluded by StoreID, and once excluded it is not enumerated or processed by inbox loading, the to-do tree, to-do events, or project data scanning.
- [ ] Exclusion persists across sessions via the StoresWrapper config.
- [ ] A user can toggle a store on/off through the UI without hand-editing JSON.
- [ ] New/changed code meets the repo's coverage thresholds; full toolchain (csharpier → analyzer build → nullable build → vstest) passes.

## Constraints & Risks

- Centralize all store-access decisions in `StoresWrapper.ShouldIncludeStore`; do not add parallel filtering logic in the ToDoModel sites.
- Preserve the existing serialized config schema and defaults for backward compatibility; new fields must be additive.
- MSTest + Moq + FluentAssertions; extend the existing tests at `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs`.
- Cross-cutting change touching 5+ production files (StoresWrapper, 4 ToDoModel call sites, StoreWrapperController) — routed as a large-path feature.

## Test Conditions to Consider

- [ ] Unit coverage areas: StoreID exact-match exclusion, precedence/interaction with existing name/path/public-folder/GWSO rules, `GetFilteredStores()` behavior with a StoreID exclusion present.
- [ ] Integration scenarios: TreeOfToDoItems, ToDoEvents (both call sites), and ProjectData each honor an excluded StoreID end-to-end.
- [ ] UI scenario: toggling a store's exclusion through StoreWrapperController persists to config and is reflected on reload.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/outlook-store-exclusion/` folder from the template

