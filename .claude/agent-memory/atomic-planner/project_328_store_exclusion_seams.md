---
name: project-328-store-exclusion-seams
description: Issue #328 outlook-store-exclusion planning constraints — file-size pressure, new-test-file csproj wiring, and the four-surface StoreID lockstep
metadata:
  type: project
---

Issue #328 adds an exact `ExcludedStoreIds` StoreID exclusion (authoritative first check) to the mature
`StoresWrapper` filter subsystem, routes four `Session.Stores` bypass sites through the shared
`ShouldIncludeStore` predicate, and adds a `StoreWrapperController` checkbox. Plan:
`docs/features/active/2026-07-15-outlook-store-exclusion-328/plan.2026-07-15T18-45.md`.

**Load-bearing planning facts (verified 2026-07-15):**
- `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs` = 469 lines; `ToDoModel/Data Model/Tree/TreeOfToDoItems.cs` = 481. Both risk crossing the hard 500-line limit after additive changes — plan carries explicit <=500 checks. Contingency for StoresWrapper: relocate the static `StoreIsIncluded` overload to a csproj-wired partial `StoresWrapper.Filtering.cs`.
- `ToDoModel/Data Model/ToDo/ToDoEvents.cs` = 594 lines, ALREADY over the 500 limit before any #328 change. RESOLVED IN SCOPE via user-approved scope expansion (2026-07-15, revision 2): the two dead methods `GetListOfToDoItemsInView`/`GetToDoItemsInView` are DELETED (verified zero repo callers — only self-declarations/comments in ToDoEvents.cs), dropping the file below 500. P4-T6 now treats `ToDoEvents.cs <= 500` as a real pass condition, not a pre-existing exception. Spec §2.2 (which deferred deletion) is superseded; spec.md was NOT edited by the planner (orchestrator reconciles separately). If re-planning, do not revert to the "thread-a-param-for-consistency" approach.
- Four inclusion surfaces must stay in lockstep with the StoreID first-branch: instance `ShouldIncludeStore`, static `StoreIsIncluded`, pure `StoreFilterAttribution.Decide`, and `ShouldIncludeStoreInstrumented` (the `GetFilteredStores` path). `StoreFilterRule.StoreId` is the new FIRST enum member.
- Adopted research option (b): persisted `StoreWrapper.StoreId` captured in `Init()` (removes live-COM from the controller unit path). Both `ExcludedStoreIds` and `StoreWrapper.StoreId` are additive JSON, backward-compatible like #261's `DisabledStoreIdentities`.

**Test-file placement (respects [[legacy-csproj-explicit-compile-include]]):**
- `StoresWrapperTests` and `StoreFilterAttributionTests` are non-`partial` `public class`; `StoreWrapperController_Tests` IS `partial` (siblings `.ButtonAndPopulate.cs`, `.Launch.cs`, each with its own `<Compile Include>`).
- Plan converts `StoresWrapperTests` to `partial` + extends its private `CreateStore` helper with an optional `storeId` setup, then adds Gap-1 tests in a NEW partial `StoresWrapperTests.StoreIdExclusion.cs` (avoids pushing the 416-line file past 500).
- New test .cs files each need explicit `<Compile Include>` wiring folded into the creation task's binary outcome: `StoresWrapperTests.StoreIdExclusion.cs` + `StoreWrapperController_Tests.ExcludeStore.cs` → `UtilitiesCS.Test.csproj`; `ToDoModel.Test/Data Model/StoreFilterRoutingTests.cs` → `ToDoModel.Test.csproj` (ToDoModel.Test is also legacy explicit-include, ToolsVersion 15.0).
- Tree/ToDoEvents are `[ExcludeFromCodeCoverage]`; meaningful new-code coverage lands on the non-exempt `StoresWrapper`/`StoreFilterAttribution`/`StoreWrapperController`. ProjectData.Rebuild is the most unit-invokable Gap-2 routing site (extend existing `ProjectDataCoverageExpansionTests.cs`).
