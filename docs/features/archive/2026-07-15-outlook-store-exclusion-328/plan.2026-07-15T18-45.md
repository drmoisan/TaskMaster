# outlook-store-exclusion — Plan (Issue #328)

- **Issue:** #328
- **Issue URL:** https://github.com/drmoisan/TaskMaster/issues/328
- **Owner:** drmoisan
- **Work Mode:** full-feature (AC sources: `spec.md` + `user-story.md`)
- **Last Updated:** 2026-07-15T21-05
- **Status:** COMPLETE. All Phase 0–4 tasks (P0-T1..P4-T8) are checked off. The P4-T4 test-double fix
  (handled `get_StoresWrapper` fail-open case in `OlObjectsProxy`) resolved the prior scope conflict;
  the full toolchain passes (csharpier / analyzers 0 errors / nullable+TWAE 0 errors / vstest
  functionally green at 4611/4611 without instrumentation). All 12 spec ACs and all 4 user-story ACs
  are checked off. No outstanding blockers.
- **Version:** 1.2
- **Language/tooling:** C# / MSTest / Moq / FluentAssertions

## Authoritative Inputs (do not re-derive)

- `docs/features/active/2026-07-15-outlook-store-exclusion-328/issue.md`
- `docs/features/active/2026-07-15-outlook-store-exclusion-328/spec.md`
- `docs/features/active/2026-07-15-outlook-store-exclusion-328/user-story.md`
- `docs/features/active/2026-07-15-outlook-store-exclusion-328/research/2026-07-15T18-42-00-store-exclusion-research.md`

All policy authority order is per `.claude/skills/policy-compliance-order`; do not duplicate policy
content here. Evidence-path authority is `.claude/skills/evidence-and-timestamp-conventions`.

## Evidence Location Statement

- All evidence artifacts resolve to `docs/features/active/2026-07-15-outlook-store-exclusion-328/evidence/<kind>/`
  (`baseline/`, `qa-gates/`, `regression-testing/`, `issue-updates/`, `other/`).
- The delegation prompt already specified the canonical scheme; no non-canonical path was supplied,
  so no `EVIDENCE_LOCATION_OVERRIDE_REJECTED` line is required.

## Design Decisions Locked by This Plan (from spec/research options)

- **Gap 3 StoreID readability:** adopt research option (b) — add a persisted `StoreId` to
  `StoreWrapper` captured in `Init()`. Rationale: removes the live-COM dependency from the controller
  unit path and gives a stable value after deserialize-before-rewire; it is additive to
  `StoreWrapper` serialization and preserves backward compatibility the same way `ExcludedStoreIds`
  does. The UI reads `StoreID` guarded and disables the checkbox when unreadable (fail-safe).
- **Gap 2 threading seam:** thread the concrete `StoresWrapper` (or narrow predicate) into
  `TreeOfToDoItems`/`ProjectData` enumeration methods where the seam allows; thread
  `IApplicationGlobals` into `ToDoEvents` (consistent with `RefreshToDoIdSplitsAsync`/`RibbonController`).
- **Dead-code deletion is IN scope** (user-approved scope expansion beyond spec §2.2, 2026-07-15):
  the two apparently-dead `ToDoEvents` methods (`GetListOfToDoItemsInView`, `GetToDoItemsInView`)
  are deleted entirely rather than threaded, after a repo-wide zero-caller verification. See Open Questions.

## Scope-Lock — files this plan authorizes changing

Production (modify):
- `UtilitiesCS/OutlookObjects/Store/StoreFilterAttribution.cs`
- `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`
- `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs`
- `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs`
- `UtilitiesCS/OutlookObjects/Store/IStoreWrapperViewer.cs`
- `UtilitiesCS/OutlookObjects/Store/StoreWrapperViewer.cs`
- `UtilitiesCS/OutlookObjects/Store/StoreWrapperViewer.Designer.cs`
- `ToDoModel/Data Model/Tree/TreeOfToDoItems.cs`
- `ToDoModel/Data Model/ToDo/ToDoEvents.cs` (thread the live path; DELETE the two dead methods `GetListOfToDoItemsInView`/`GetToDoItemsInView` and their XML docs / QUESTION+CLEANUP annotations — do NOT create replacement stubs)
- `ToDoModel/Data Model/Project/ProjectData.cs`
- `TaskMaster/Ribbon/RibbonController.cs`
- `TaskMaster/AppGlobals/AppToDoObjects.cs`
- `TaskMaster/Ribbon/TryFunctionalityInConstruction.cs`

Production (new .cs — legacy explicit-`<Compile Include>` project, so csproj wiring is part of the task's binary outcome):
- `ToDoModel/Data Model/ToDo/ToDoEvents.Filtering.cs` (new `partial` file holding relocated `ToDoEvents` filtering/enumeration methods, required by P2-T10 to bring `ToDoEvents.cs` to <=500 lines; carries `[ExcludeFromCodeCoverage]`) → wire into `ToDoModel/ToDoModel.csproj`

Tests (modify):
- `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs` (extend `CreateStore`; mark class `partial`)
- `UtilitiesCS.Test/OutlookObjects/Store/StoreFilterAttributionTests.cs`
- `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperTests.cs`
- `ToDoModel.Test/Data Model/Project/ProjectDataCoverageExpansionTests.cs`
- `TaskMaster.Test/AppGlobals/AppToDoObjectsTestDoubles.cs` (only the `OlObjectsProxy.Invoke` method is touched — add one new handled `MethodName` case for `get_StoresWrapper`; no other change to the file)

Tests (new .cs — each is a legacy explicit-`<Compile Include>` project, so csproj wiring is part of the task's binary outcome):
- `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.StoreIdExclusion.cs` → wire into `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
- `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.ExcludeStore.cs` → wire into `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
- `ToDoModel.Test/Data Model/StoreFilterRoutingTests.cs` → wire into `ToDoModel.Test/ToDoModel.Test.csproj`

Build config (modify — required):
- `ToDoModel/ToDoModel.csproj` (new partial file `ToDoEvents.Filtering.cs` `<Compile Include>` wiring — required by P2-T10; `ToDoEvents.cs` remains over 500 lines after the P2-T3 deletion, so this relocation is not contingent)

Build config (modify, only if the ≤500-line contingency in P1-T7 fires):
- `UtilitiesCS/UtilitiesCS.csproj` (new partial file `StoresWrapper.Filtering.cs` `<Compile Include>` wiring)

## Coverage Floor (authority: CLAUDE.md)

- Repository-wide line coverage remains >= 80%.
- New modules/classes/methods target >= 90%.
- No coverage regression on changed lines.
- `TreeOfToDoItems` and `ToDoEvents` are `[ExcludeFromCodeCoverage]`; the meaningful new-code coverage
  lands on the non-exempt `StoresWrapper`, `StoreFilterAttribution`, and `StoreWrapperController`.
  WinForms Designer/form additions are covered by the WinForms exemption.

## Implementation Plan (Atomic Tasks)

### Phase 0 — Policy Reads and Toolchain Baseline Capture

- [x] [P0-T1] Read policy set in required order and record the read in `docs/features/active/2026-07-15-outlook-store-exclusion-328/evidence/baseline/phase0-instructions-read.md`
  - Files to read: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, `.claude/skills/atomic-plan-contract/SKILL.md`, `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`, `.claude/skills/policy-compliance-order/SKILL.md`
  - Acceptance: artifact exists with `Timestamp:`, `Policy Order:`, and the explicit list of files read.
- [x] [P0-T2] Capture CSharpier baseline via `dotnet tool run csharpier --check .` into `docs/features/active/2026-07-15-outlook-store-exclusion-328/evidence/baseline/baseline-csharpier.2026-07-15T18-45.md`
  - Acceptance: artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (format-clean or list of unformatted files).
- [x] [P0-T3] Capture analyzer-build baseline via `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` into `docs/features/active/2026-07-15-outlook-store-exclusion-328/evidence/baseline/baseline-analyzer-build.2026-07-15T18-45.md`
  - Acceptance: artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (warning/error counts).
- [x] [P0-T4] Capture nullable/TreatWarningsAsErrors baseline via `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` into `docs/features/active/2026-07-15-outlook-store-exclusion-328/evidence/baseline/baseline-nullable-build.2026-07-15T18-45.md`
  - Acceptance: artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [x] [P0-T5] Capture MSTest coverage baseline via `vstest.console.exe` with `/EnableCodeCoverage` over `UtilitiesCS.Test`, `TaskMaster.Test`, and `ToDoModel.Test` assemblies, saving the cobertura report to `docs/features/active/2026-07-15-outlook-store-exclusion-328/evidence/baseline/baseline-coverage.2026-07-15T18-45.cobertura.xml` and a summary to `docs/features/active/2026-07-15-outlook-store-exclusion-328/evidence/baseline/baseline-vstest.2026-07-15T18-45.md`
  - Acceptance: summary records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with numeric baseline line% and branch% headline values.

### Phase 1 — StoreID Exclusion Core in StoresWrapper and StoreFilterAttribution

- [x] [P1-T1] Add `StoreId` as the first `StoreFilterRule` enum member (with XML doc) in `UtilitiesCS/OutlookObjects/Store/StoreFilterAttribution.cs`
  - Acceptance: enum order is `StoreId, PublicFolder, NameContains, GwsoFilePath, FilePathContains, Disabled, Included`; project compiles.
- [x] [P1-T2] Extend `Decide(...)` in `UtilitiesCS/OutlookObjects/Store/StoreFilterAttribution.cs` with leading params `string storeId` and `IReadOnlyCollection<string> excludedStoreIds`, and add the exact-match first branch returning `(false, StoreFilterRule.StoreId)` using `!string.IsNullOrWhiteSpace` guards and `StringComparison.OrdinalIgnoreCase`
  - Acceptance: `Decide` remains pure/COM-free; matching StoreID short-circuits first; non-matching preserves existing attribution.
- [x] [P1-T3] Add `[JsonProperty] public List<string> ExcludedStoreIds { get; set; } = [];` (with XML doc) beside the existing exclusion lists in `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`
  - Acceptance: field default is an empty list; `[JsonProperty]` present; project compiles.
- [x] [P1-T4] Add the guarded `store.StoreID` read (try/catch fail-open mirroring the existing `FilePath` guard) plus the StoreID first-branch check to the instance `ShouldIncludeStore(Outlook.Store)` in `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`
  - Acceptance: a StoreID in `ExcludedStoreIds` excludes the store first; a store whose `StoreID` read throws is not excluded on that basis (fail-open).
- [x] [P1-T5] Add `storeId` + `excludedStoreIds` params and the same first branch to the static `StoreIsIncluded(...)` overload in `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`, updating any in-assembly call sites to the single code path
  - Acceptance: one decision code path (no divergent overload); assembly compiles.
- [x] [P1-T6] Read the guarded `store.StoreID` in `ShouldIncludeStoreInstrumented(...)` and pass `storeId` + `ExcludedStoreIds` into `Decide(...)` in `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`
  - Acceptance: `GetFilteredStores()`/`MaterializeFilteredStores()` path applies the StoreID branch; `Decide` receives the read primitive.
- [x] [P1-T7] Verify `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs` remains <= 500 lines after CSharpier; if the additive changes exceed 500, relocate only the static `StoreIsIncluded` overload into a new partial file `UtilitiesCS/OutlookObjects/Store/StoresWrapper.Filtering.cs` and add its `<Compile Include>` item to `UtilitiesCS/UtilitiesCS.csproj`, then re-verify
  - Acceptance: `StoresWrapper.cs` <= 500 lines; any new partial file is csproj-wired and the assembly compiles.
- [x] [P1-T8] Extend the `CreateStore(...)` helper in `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs` with an optional `storeId` parameter (`store.SetupGet(x => x.StoreID).Returns(storeId)`) and mark the class `partial`
  - Acceptance: existing callers still compile; helper can seed a StoreID; class is `partial`.
- [x] [P1-T9] Create `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.StoreIdExclusion.cs` (partial of `StoresWrapperTests`, wired into `UtilitiesCS.Test/UtilitiesCS.Test.csproj`) with Gap-1 behavior tests: exact-match excludes; near-but-not-equal does not exclude; `OrdinalIgnoreCase`; empty/whitespace entries ignored; StoreID-only exclusion independent of other rules; store matched by StoreID plus other rules still excluded and attributed `StoreFilterRule.StoreId`; `GetFilteredStores()`/`Init()` omits a StoreID-excluded store; fail-open when `StoreID` read throws
  - Acceptance: new file exists AND is csproj-wired so it compiles; all tests pass; file <= 500 lines.
- [x] [P1-T10] Add JSON round-trip and legacy backward-compat tests to `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.StoreIdExclusion.cs`: `ExcludedStoreIds` serializes and restores; legacy JSON with no `ExcludedStoreIds` key deserializes to the empty default (mirror the `StoresWrapperDisableTests` round-trip pattern)
  - Acceptance: round-trip and absent-key tests pass.
- [x] [P1-T11] Extend `UtilitiesCS.Test/OutlookObjects/Store/StoreFilterAttributionTests.cs` with `Decide` tests: matching StoreID returns `(false, StoreFilterRule.StoreId)` as the first branch; non-matching StoreID preserves existing attribution for every other rule
  - Acceptance: new tests pass; `StoreFilterAttributionTests.cs` <= 500 lines.

### Phase 2 — Route the Four Bypass Sites Through the Shared Predicate

- [x] [P2-T1] Thread the store filter into `GetToDoList(...)` and `GetToDoListAsync(...)` in `ToDoModel/Data Model/Tree/TreeOfToDoItems.cs` (add a `StoresWrapper` parameter, apply `.Where(store => storesWrapper is null || storesWrapper.ShouldIncludeStore(store))`) and update the `LoadTree(...)` caller to pass `appGlobals.Ol.StoresWrapper`; verify the file remains <= 500 lines
  - Acceptance: both methods filter via the shared predicate; no site-local filtering logic added; `TreeOfToDoItems.cs` <= 500 lines; assembly compiles.
- [x] [P2-T2] Thread `IApplicationGlobals globals` into the live path `GetAsyncEnumerableOfToDoItemsInView(...)` and `RefreshToDoIdSplitsAsync(...)` in `ToDoModel/Data Model/ToDo/ToDoEvents.cs`, using `globals.Ol.App` for the view/session and the null-safe `globals.Ol.StoresWrapper.ShouldIncludeStore` predicate in the `.Where`
  - Acceptance: the live enumeration filters via the shared predicate; no parallel filtering logic added.
- [x] [P2-T3] Verify zero callers of `GetListOfToDoItemsInView` and `GetToDoItemsInView` across the full repository (production and test `.cs`) via grep, then delete both methods entirely from `ToDoModel/Data Model/ToDo/ToDoEvents.cs` — including their XML doc comments and the `QUESTION:`/`CLEANUP:` annotations — and remove any `using` directives in `ToDoModel/Data Model/ToDo/ToDoEvents.cs` that become unused as a result
  - Preconditions: grep over the repo returns no external caller of either method (only the declarations/comments inside `ToDoEvents.cs` itself). If any external caller is found, halt this task, do NOT delete, and re-flag the conflict for the coordinator.
  - Acceptance: both methods are removed (no replacement stubs); `ToDoModel/Data Model/ToDo/ToDoEvents.cs` no longer references them; no unused `using` remains; the solution compiles.
- [x] [P2-T4] Update the `RefreshToDoIdSplitsAsync` call in `TaskMaster/Ribbon/RibbonController.cs` to pass `Globals`
  - Acceptance: caller passes globals; project compiles.
- [x] [P2-T5] Add a filtered `Rebuild(StoresWrapper storesWrapper)` overload (or an added `StoresWrapper` parameter) to `ToDoModel/Data Model/Project/ProjectData.cs` that applies `.Where(store => storesWrapper is null || storesWrapper.ShouldIncludeStore(store))` before `GetDfToDo(store)`
  - Acceptance: `ProjectData` acquires no `IApplicationGlobals` field; the filtered overload skips excluded stores; assembly compiles.
- [x] [P2-T6] Update the three `ProjectData.Rebuild` callers — the two sites in `TaskMaster/AppGlobals/AppToDoObjects.cs` and the site in `TaskMaster/Ribbon/TryFunctionalityInConstruction.cs` — to pass the resolved `StoresWrapper`
  - Acceptance: all three callers pass the filter; project compiles.
- [x] [P2-T7] Add a `ProjectData.Rebuild` routing test to `ToDoModel.Test/Data Model/Project/ProjectDataCoverageExpansionTests.cs` using a real `StoresWrapper` with a populated `ExcludedStoreIds` over a mocked `Stores` collection, asserting the excluded store's `GetDfToDo` path is not taken (no live COM, no temp files)
  - Acceptance: test proves the excluded store is not processed by `Rebuild`; test passes.
- [x] [P2-T8] Create `ToDoModel.Test/Data Model/StoreFilterRoutingTests.cs` (wired into `ToDoModel.Test/ToDoModel.Test.csproj`) covering `TreeOfToDoItems.GetToDoList`/`GetToDoListAsync` and `ToDoEvents.GetAsyncEnumerableOfToDoItemsInView`: with a real `StoresWrapper` (`ExcludedStoreIds` populated) over a mocked `Stores` collection, a StoreID-excluded store is not enumerated/processed (no live COM, no temp files)
  - Acceptance: new file exists AND is csproj-wired so it compiles; tests pass; file <= 500 lines.
- [x] [P2-T9] Verify no parallel filtering logic exists outside `StoresWrapper.ShouldIncludeStore`/`StoreFilterAttribution.Decide` by reviewing all four sites in `ToDoModel/Data Model/Tree/TreeOfToDoItems.cs`, `ToDoModel/Data Model/ToDo/ToDoEvents.cs`, and `ToDoModel/Data Model/Project/ProjectData.cs`, recording the review in `docs/features/active/2026-07-15-outlook-store-exclusion-328/evidence/other/no-parallel-filter-review.2026-07-15T18-45.md`
  - Acceptance: artifact confirms every bypass site calls the shared predicate; no reimplemented include/exclude logic.
- [x] [P2-T10] Relocate the store-filtering/enumeration methods out of `ToDoModel/Data Model/ToDo/ToDoEvents.cs` into a new csproj-wired partial file `ToDoModel/Data Model/ToDo/ToDoEvents.Filtering.cs` (mirroring the P1-T7 `StoresWrapper.Filtering.cs` relocation). This is required because after the P2-T3 deletion (~48 lines removed from the 594-line baseline ≈ 546 lines) and the P2-T2 threading additions (+~3-6 lines ≈ ~549-552 lines), `ToDoEvents.cs` remains over the 500-line limit; reaching <= 500 requires relocating at least ~52 lines net. Declare `ToDoEvents` as `partial` in both files, move the threaded filtering methods `GetAsyncEnumerableOfToDoItemsInView(...)` and `RefreshToDoIdSplitsAsync(...)` (and, only if still needed to reach the line target, adjacent helper methods such as the two `WriteToCSV` overloads) verbatim into the new partial, carry `[ExcludeFromCodeCoverage]` on the new partial declaration, and add the `<Compile Include>` item for the new file to `ToDoModel/ToDoModel.csproj`. `ToDoEvents.cs` is `[ExcludeFromCodeCoverage]`, so this relocation is low-risk and behavior-preserving. This task runs after P2-T2 and P2-T3 (its preconditions) and before the Phase 4 file-size check (P4-T7).
  - Acceptance: `ToDoModel/Data Model/ToDo/ToDoEvents.cs` is <= 500 lines after CSharpier; `ToDoModel/Data Model/ToDo/ToDoEvents.Filtering.cs` exists, is `<Compile Include>`-wired in `ToDoModel/ToDoModel.csproj`, carries `[ExcludeFromCodeCoverage]`, and is <= 500 lines; no method is duplicated across the two partials; the solution compiles.

### Phase 3 — UI Toggle in StoreWrapper Settings

- [x] [P3-T1] Add a persisted `[JsonProperty] string StoreId` to `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs`, captured in `Init()` guarded by try/catch (mirroring the existing `DisplayName` capture), additive to serialization and backward-compatible (absent key → default)
  - Acceptance: field serializes/restores; legacy JSON without the key deserializes to the default; assembly compiles.
- [x] [P3-T2] Add a `CheckBox ExcludeStore` control to `UtilitiesCS/OutlookObjects/Store/StoreWrapperViewer.Designer.cs`
  - Acceptance: control declared, instantiated, and added to the form layout; Designer compiles.
- [x] [P3-T3] Expose `ExcludeStore` on `UtilitiesCS/OutlookObjects/Store/IStoreWrapperViewer.cs` mirroring the existing control property shape
  - Acceptance: interface exposes the checkbox; implementers compile.
- [x] [P3-T4] Forward the checkbox `CheckedChanged` event to the controller in `UtilitiesCS/OutlookObjects/Store/StoreWrapperViewer.cs` as a thin forwarder (mirroring `DisplayName_SelectedValueChanged` forwarding)
  - Acceptance: event is forwarded; no business logic in the code-behind.
- [x] [P3-T5] Bind the checkbox in `PopulateWithCurrent()` in `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` from `Model.ExcludedStoreIds` membership (`OrdinalIgnoreCase`) using the current store's `StoreId`, and disable the checkbox (fail-safe) when the StoreID is unreadable
  - Acceptance: checkbox reflects membership on selection; unreadable StoreID disables the checkbox and blocks mutation.
- [x] [P3-T6] Mutate `Model.ExcludedStoreIds` in the save path (`AnyChanges()`/`SaveChanges()`) in `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs`: add the StoreID when checked, remove it when unchecked, guard with `Contains` for idempotency, and call `Model.Serialize()` only when the list actually changed
  - Acceptance: toggling on/off adds/removes exactly once; no duplicate add; no `Serialize()` when unchanged.
- [x] [P3-T7] Add a `StoreWrapper.StoreId` round-trip and backward-compat test to `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperTests.cs` (serializes/restores; absent key → default)
  - Acceptance: tests pass; file <= 500 lines.
- [x] [P3-T8] Create `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.ExcludeStore.cs` (partial of `StoreWrapperController_Tests`, wired into `UtilitiesCS.Test/UtilitiesCS.Test.csproj`) with tests: selecting a store binds the checkbox to `ExcludedStoreIds` membership; toggle-on + save adds the StoreID and calls `Model.Serialize()`; toggle-off + save removes it and serializes; idempotency (no duplicate add; no serialize when unchanged); unreadable StoreID disables the checkbox and performs no mutation — using `Mock<IStoreWrapperViewer>` + a real `StoresWrapper` model (no live COM, no temp files)
  - Acceptance: new file exists AND is csproj-wired so it compiles; all tests pass; file <= 500 lines.

### Phase 4 — Final QA Loop, Coverage Delta, and Acceptance Criteria

- [x] [P4-T1] Run CSharpier format via `dotnet tool run csharpier .` and record `docs/features/active/2026-07-15-outlook-store-exclusion-328/evidence/qa-gates/final-csharpier.2026-07-15T18-45.md`
  - Acceptance: artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; if files changed, restart the loop from P4-T1.
- [x] [P4-T2] Run the analyzer build via `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record `docs/features/active/2026-07-15-outlook-store-exclusion-328/evidence/qa-gates/final-analyzer-build.2026-07-15T18-45.md`
  - Acceptance: artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; zero analyzer errors on touched code.
- [x] [P4-T3] Run the nullable/TreatWarningsAsErrors build via `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and record `docs/features/active/2026-07-15-outlook-store-exclusion-328/evidence/qa-gates/final-nullable-build.2026-07-15T18-45.md`
  - Acceptance: artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; no nullable warnings on touched paths.
- [x] [P4-T4] Add a second handled case to `OlObjectsProxy.Invoke(IMessage msg)` in `TaskMaster.Test/AppGlobals/AppToDoObjectsTestDoubles.cs`: when `call.MethodName == "get_StoresWrapper"`, return `new ReturnMessage(null, null, 0, call.LogicalCallContext, call)` (fail-open — mirrors the existing `get_App` `ReturnMessage` shape but returns `null` instead of the app instance). This lets `ProjectData.Rebuild`'s `storesWrapper is null || storesWrapper.ShouldIncludeStore(store)` predicate treat the proxy as not-yet-loaded, preserving the test's original intent (it asserts that `Rebuild` reaches `get_Session`, not `StoresWrapper` filtering behavior). The change is required because the in-scope, already-executed P2-T6 threading of `Parent.Ol.StoresWrapper` into the `ProjectData.Rebuild` call sites now evaluates `get_StoresWrapper` on the proxy, whose current `else` branch throws `NotSupportedException("Member 'get_StoresWrapper' is not used by this test proxy.")` during argument evaluation before `Rebuild` reaches `get_Session`. Edit only `OlObjectsProxy.Invoke`; make no other change to the file. This task runs after P4-T1/T2/T3 and before the P4-T5 vstest re-run.
  - Acceptance: `TaskMaster.Test.LoadProjInfoAsync_RebuildsWhenProjectCountIsZeroAndOutlookApplicationIsAvailable` passes; no other test in `AppToDoObjectsTestDoubles.cs`-dependent suites regresses; `TaskMaster.Test/AppGlobals/AppToDoObjectsTestDoubles.cs` remains <= 500 lines; this single handled-case addition is the only change to the file.
- [x] [P4-T5] Run MSTest with coverage via `vstest.console.exe` `/EnableCodeCoverage` over `UtilitiesCS.Test`, `TaskMaster.Test`, and `ToDoModel.Test`, saving `docs/features/active/2026-07-15-outlook-store-exclusion-328/evidence/qa-gates/final-coverage.2026-07-15T18-45.cobertura.xml` and a summary at `docs/features/active/2026-07-15-outlook-store-exclusion-328/evidence/qa-gates/final-vstest.2026-07-15T18-45.md`. Run this after the P4-T4 test-double fix. P4-T1/T2/T3 already passed in the prior execution pass and do not need to be redone unless the P4-T4 edit changes csharpier/analyzer/nullable outcomes; if it does, restart the full P4-T1..P4-T8 loop per the restart-on-any-file-change rule stated in P4-T1 and P4-T8.
  - Acceptance: summary records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with numeric post-change line% and branch%; all tests pass (including `LoadProjInfoAsync_RebuildsWhenProjectCountIsZeroAndOutlookApplicationIsAvailable`, unblocked by P4-T4).
- [x] [P4-T6] Compute the coverage delta (baseline vs post-change vs new/changed-code for `StoresWrapper`, `StoreFilterAttribution`, `StoreWrapper`, and `StoreWrapperController`) and record `docs/features/active/2026-07-15-outlook-store-exclusion-328/evidence/qa-gates/coverage-delta.2026-07-15T18-45.md`
  - Acceptance: artifact reports baseline%, post-change%, and new/changed-code%; new code >= 90%, repo-wide >= 80%, no regression on changed lines; otherwise outcome is remediation-required (not PASS).
- [x] [P4-T7] Verify file-size compliance (<= 500 lines) for every touched production and test file, including `ToDoModel/Data Model/ToDo/ToDoEvents.cs` (must be <= 500 lines after the P2-T3 deletion of the two dead methods AND the P2-T10 relocation into `ToDoEvents.Filtering.cs`) and the new `ToDoModel/Data Model/ToDo/ToDoEvents.Filtering.cs`, and record `docs/features/active/2026-07-15-outlook-store-exclusion-328/evidence/qa-gates/file-size-check.2026-07-15T18-45.md`. One documented pre-existing exception applies: `TaskMaster/AppGlobals/AppToDoObjects.cs` is 503 lines at its pre-#328 baseline (already over the limit before this feature), and P2-T6 changes only two `ProjectData.Rebuild` call-site arguments in it (not a size-driving change). For this file the gate requires that it has NOT GROWN beyond its 503-line baseline rather than requiring <= 500.
  - Acceptance: every touched file is <= 500 lines with the single documented exception of `AppToDoObjects.cs`; `ToDoEvents.cs` and `ToDoEvents.Filtering.cs` are each <= 500 lines (if `ToDoEvents.cs` still exceeds 500 after the P2-T3 deletion and P2-T10 relocation, the outcome is remediation-required, not PASS); `AppToDoObjects.cs` is <= its 503-line pre-#328 baseline (any growth beyond 503 is remediation-required, not PASS); no file other than the documented `AppToDoObjects.cs` exception exceeds 500 lines.
- [x] [P4-T8] Check off acceptance criteria against `docs/features/active/2026-07-15-outlook-store-exclusion-328/spec.md` (AC1–AC12), `docs/features/active/2026-07-15-outlook-store-exclusion-328/user-story.md` (4 ACs), and `docs/features/active/2026-07-15-outlook-store-exclusion-328/issue.md`, recording `docs/features/active/2026-07-15-outlook-store-exclusion-328/evidence/issue-updates/ac-checkoff.2026-07-15T18-45.md`. Perform this only after the P4-T4 test-double fix and the P4-T5 vstest re-run report all tests passing.
  - Acceptance: every AC maps to a passing test or evidence artifact; if any step from P4-T1 changed files or failed, restart the full loop from P4-T1.

## Acceptance Criteria Traceability (spec §12 → tasks)

- AC1 (`ExcludedStoreIds` field, exact-match, OrdinalIgnoreCase, whitespace ignored) → P1-T3, P1-T9
- AC2 (first/authoritative precedence + attribution) → P1-T2, P1-T4, P1-T9, P1-T11
- AC3 (`Decide` branch + `StoreFilterRule.StoreId`) → P1-T1, P1-T2, P1-T11
- AC4 (all four inclusion surfaces in lockstep) → P1-T4, P1-T5, P1-T6, P1-T9
- AC5 (fail-open on unreadable StoreID) → P1-T4, P1-T9
- AC6 (bypass sites route through the filter; no parallel logic) → P2-T1..P2-T9. Note: spec AC6's sub-clause "the two issue-named `ToDoEvents` methods are threaded with the same filter for consistency" is superseded by the user-approved scope expansion — those two dead methods are deleted (P2-T3) rather than threaded, so there is no longer a surface to route. The live path (`GetAsyncEnumerableOfToDoItemsInView`, P2-T2) and the other three sites still route through the shared predicate.
- AC7 (not-yet-loaded model is fail-open) → P2-T1, P2-T5, P2-T7, P2-T8
- AC8 (UI toggle binds to membership) → P3-T5, P3-T8
- AC9 (UI toggle mutates and persists; idempotency) → P3-T6, P3-T8
- AC10 (UI fail-safe on unreadable StoreID) → P3-T5, P3-T8
- AC11 (backward-compatible persistence) → P1-T3, P1-T10, P3-T1, P3-T7
- AC12 (toolchain, coverage, no regression, files <500) → P4-T1..P4-T7
- User-story ACs (exclude by StoreID end-to-end; persistence; UI toggle without JSON; toolchain/coverage) → P1/P2/P3 + P4-T5..P4-T8

## Open Questions / Notes

- **Dead-code deletion is in scope (scope expansion beyond spec §2.2, user-approved 2026-07-15).**
  Spec §2.2 originally deferred deletion of the two apparently-dead `ToDoEvents` methods
  (`GetListOfToDoItemsInView`, `GetToDoItemsInView`) to a separate issue and required threading a
  parameter into them for consistency. The user directly approved expanding #328 scope during
  orchestration to delete both methods instead (this decision was made by the user, not silently by an
  agent). P2-T3 now performs the deletion after a repo-wide zero-caller verification (verified during
  planning: the only occurrences of either name are the declarations and comments inside
  `ToDoModel/Data Model/ToDo/ToDoEvents.cs` itself; no external callers in production or test).
  **File-size arithmetic (corrected).** Deleting the two dead methods does NOT by itself bring
  `ToDoEvents.cs` under 500. The two methods total ~47-49 lines (`GetListOfToDoItemsInView` at lines
  101-125 and `GetToDoItemsInView` at lines 145-166, plus surrounding blank lines), so from the
  594-line baseline the file lands at ~546 lines after deletion — still over the 500-line limit — and
  P2-T2 (threading `IApplicationGlobals globals` into the live path) adds a few more lines, leaving
  `ToDoEvents.cs` at ~549-552 lines. Reaching <= 500 therefore requires relocating at least ~52 lines
  net; P2-T10 performs that relocation into the csproj-wired partial `ToDoEvents.Filtering.cs`
  (mirroring the P1-T7 `StoresWrapper.Filtering.cs` contingency). Because `ToDoEvents.cs` is
  `[ExcludeFromCodeCoverage]`, the relocation is low-risk and behavior-preserving. P4-T7 therefore
  treats `ToDoEvents.cs <= 500` as a real pass condition that depends on BOTH the P2-T3 deletion and
  the P2-T10 relocation, not on deletion alone. `spec.md`/`user-story.md` are not edited by this plan;
  the orchestrator reconciles the spec separately if needed.
- **Pre-existing file-size exception: `TaskMaster/AppGlobals/AppToDoObjects.cs` (503 lines).**
  This file is already 503 lines at its pre-#328 baseline — an over-limit condition that predates and
  is not introduced by this feature. P2-T6 touches it only to update two `ProjectData.Rebuild`
  call-site arguments (argument insertion), which grows the file by at most a couple of lines and is
  not a size-driving change. Bringing this file under 500 would require an out-of-scope reduction that
  #328 does not meaningfully justify. P4-T7 therefore records `AppToDoObjects.cs` as a documented
  pre-existing exception: the gate requires that the file has NOT GROWN beyond its 503-line baseline
  rather than requiring <= 500. Growth beyond 503 is remediation-required.
- **`StoresWrapper.cs`/`TreeOfToDoItems.cs` proximity to 500 lines.** Baselines are 469 and 481 lines.
  P1-T7 and P2-T1 carry explicit <=500 verification with a documented relocation contingency for
  `StoresWrapper.cs` (extract the static `StoreIsIncluded` overload into a csproj-wired partial).

## Preflight

- Preflight signal is reported in the planner's final message per `atomic-plan-contract`
  (`DIRECTIVE: PREFLIGHT VALIDATION ONLY`).
