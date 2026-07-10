# taskvisualization-secondary-testability - Refactor Spec

- **Issue:** #298
- **Parent (optional):** winforms-testability-refactor (#295)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-09
- **Status:** Ready for Planning
- **Version:** 1.0

## Intent & Outcomes

This is a testability refactor of the `TaskVisualization` project's secondary
viewers and helper classes. Beyond `TaskController` (sibling #297's scope), the
project contains secondary viewers and helpers with little or no unit-test
coverage: `EditFilterController.cs` (231 lines) bound to the concrete
`EditFilterViewer` form, `ManageFilters.cs` (57 lines + designer) which is itself
a `Form`-derived class, and helper/business classes (`FlagTasks.cs` 242,
`AutoCreateProject.cs` 211, `FlagChangeGroup.cs` 157, `AutoAssignContext.cs` 96,
`AutoAssignPeople.cs` 95, `FlagChangeTrainingQueue.cs` 78, `FlagChangeItem.cs`
23) that mix business logic with WinForms/Outlook-Interop interaction.

Intended outcomes:

- Create `IEditFilterViewer` and `IManageFiltersViewer`, both deriving from
  `UtilitiesCS.Interfaces.IWinForm.IForm`, and both exposing **behavioral
  members** (string properties for text surfaces plus `event EventHandler`
  click events) rather than raw WinForms controls, so a Moq mock satisfies the
  interface without instantiating any `Control`. The concrete forms implement
  the interfaces; the controllers depend only on the interfaces.
- Extract the `ManageFilters` orchestration logic into a new host-neutral
  `ManageFiltersController` that depends on `IManageFiltersViewer` and
  `IApplicationGlobals`, leaving only thin view wiring on the form.
- Separate the helper classes' host-neutral logic from COM interaction, placing
  seams at the Outlook-Interop and dialog boundaries (interface seam >
  injectable delegate > adapter, per `.claude/rules/csharp.md`) so tests never
  construct live forms or show popups.
- Keep all touched production files <= 500 lines.
- Bring the `TaskVisualization` project as a whole to >= 80% line coverage,
  primarily through pure-unit tests of the extracted host-neutral logic plus
  mocked-interface controller tests, with no live form and no live Outlook
  process.

This spec is bound by the epic's Shared Design Pattern
(`docs/features/epics/winforms-testability-refactor/epic.md`): viewer interfaces
derive from `IForm`; production files stay under 500 lines; COM/logic are
separated; seams are preferred over UI-thread execution; MSTest + Moq +
FluentAssertions bring the project to >= 80% line coverage.

## Invariants (must not change)

External surfaces, contracts, and behaviors that must remain identical:

- **QuickFiler `FlagTasks` factory constructor shape MUST NOT change.** The
  public constructor `FlagTasks(IApplicationGlobals, IList, bool, IntPtr,
  string)` and the `FlagTasks` concrete type name are consumed by the QuickFiler
  factory seam `Func<IApplicationGlobals, List<MailItem>, bool, IntPtr,
  FlagTasks>` (`QuickFiler/Controllers/QfcItemController.Initialization.cs:42,390`
  and matching `QuickFiler.Test` seam tests). Any change breaks QuickFiler, which
  is out of scope for this feature. The refactor achieves its coverage goal by
  extracting the pure statics into a host-neutral file, not by seaming the
  constructor.
- **`EfcFormController`'s three-call `ManageFilters` surface is preserved.**
  `QuickFiler/Controllers/EfcFormController.cs:562-564` calls `new
  ManageFilters(); .LoadFilters(_globals); .Show()`. The public `ManageFilters`
  type and the `LoadFilters`/`Show` members remain; `LoadFilters` internally
  delegates to `ManageFiltersController`. `EfcFormController` is not edited.
- **Observable UI behavior is preserved.** No user-facing behavior or UX changes.
  The refactor preserves the observable behavior of every touched form and
  helper (including `BtnDelete_Click`'s current no-side-effect behavior).
- **No new production dependencies.** The refactor introduces no third-party
  runtime dependency; it uses only libraries already approved in the project.
- Performance characteristics to preserve (latency/throughput/memory): no change
  intended; the refactor is structural and adds seams without altering runtime
  work.
- Compatibility guarantees (CLI flags, config schemas, versions): no CLI, config,
  or public-package surface changes. The `IFlagChange*` interfaces in
  `UtilitiesCS` are not modified; `FlagChangeGroup(IApplicationGlobals,
  MailItem)`, `FlagChangeTrainingQueue.Init()`, and the `FlagChangeItem` POCO
  keep their current shapes.

## Scope (structural changes)

Derived from the research artifact (sections B, C, D, G).

### New files (four; two interface-only, two host-neutral logic)

| New file | Responsibility | Category | Approx lines |
|---|---|---|---|
| `IEditFilterViewer.cs` | Viewer interface deriving from `IForm`; five text properties, `Text`/`Show()`/`Hide()`/`Dispose()`, seven `...Click` events, `ResetTips()` | interface-only (host-neutral) | ~40 |
| `IManageFiltersViewer.cs` | Viewer interface deriving from `IForm`; `SelectedFilter`, `SetFilters`, `RebuildList`, `Show()`, three `...Click` events | interface-only (host-neutral) | ~25 |
| `ManageFiltersController.cs` | Extracted filter-management logic (`LoadFilters`, `EditSelected`, `AddFilter`, `EditFilterCallback`, `DeleteSelected`) depending on `IManageFiltersViewer` + `IApplicationGlobals` + a `Func<IApplicationGlobals, FilterEntry, EditFilterController>` seam | host-neutral (+ seam delegate) | ~90 |
| `FlagCalculations.cs` (or `FlagTasksFlagSelection.cs`) | Extracted pure statics `GetFlagsToSet`, `ConvertFlagStringsToEnum`, `GetSymbolsDictionary` | host-neutral | ~70 |

Interface-only files must reference only `FilterEntry`,
`IEnumerable<FilterEntry>`, `EventHandler`, and `string` (plus deriving `IForm`);
they must not reference WinForms control types. They are legitimately 0%
executable coverage and are excluded from measurement per the interface-only
clarification in `.claude/rules/general-unit-test.md`.

### Modified existing files (retarget + seam; no size violation introduced)

- `EditFilterController.cs` — retarget field to `IEditFilterViewer`; replace the
  seven `_viewer.X.Click += Handler` wirings with `_viewer.XClick += Handler`;
  replace control `.Text` reads/writes with the new string properties; add a
  viewer factory seam (`Func<IEditFilterViewer>` defaulting to `() => new
  EditFilterViewer()`) and the `_tagSelector` delegate seam for the Tag dialog;
  remove the class-level `[ExcludeFromCodeCoverage]`, keeping method-level
  exemptions only on genuinely UI-bound members.
- `EditFilterViewer.cs` — implement the `IEditFilterViewer` pass-throughs and
  `ResetTips()`.
- `ManageFilters.cs` — implement `IManageFiltersViewer`; delegate the filter
  orchestration to `ManageFiltersController`; retain `InitializeComponent` and
  thin view wiring.
- `FlagTasks.cs` — call the extracted statics; add the `_flagSelector` dialog
  seam; narrow exemptions to the genuinely Outlook-bound members; preserve the
  constructor and `Run()`.
- `AutoCreateProject.cs` — add optional seam constructor parameters with safe
  defaults (`_chooseProgram`, `_createCategory`, `_getTaskItems`) keeping the
  single-arg `AutoCreateProject(IApplicationGlobals)` form valid; narrow
  exemptions to live-Interop members.
- `AutoAssignContext.cs` / `AutoAssignPeople.cs` — add the `_toHelper` seam;
  narrow exemptions to classifier-engine invocation lines.
- `FlagChangeTrainingQueue.cs` / `FlagChangeItem.cs` — no structural change; add
  tests. `FlagChangeGroup.TryEnqueue` is already a measured host-neutral seam
  (its four Outlook-bound members keep #197's method-level exemptions).

### Seam catalog (Interop/COM + dialog touchpoints)

Ordered per `.claude/rules/csharp.md` (interface seam > injectable delegate >
adapter):

- **`EditFilterController.SelectItems` — Tag dialog seam.** The `new
  TagViewer()`/`new TagController(...)`/`ShowDialog()` path becomes an injectable
  delegate `Func<SortedDictionary<string,bool>, (bool cancelled, string
  selection)> _tagSelector`. **Reuses #297's `ITagPromptService`** dialog seam if
  #297 introduces one; otherwise a narrow local delegate.
- **`AutoCreateProject` program chooser** — `ChooseOrCreateProgramName`'s
  `TagLauncher`/`ShowDialog()` becomes `Func<IEnumerable<string>, string>
  _chooseProgram`.
- **`AutoCreateProject` category creation** — the MAPI
  `CreateCategoryModule.CreateCategory(NamespaceMAPI, ...)` call becomes adapter
  seam `Func<IPrefix, string, Category> _createCategory`.
- **`AutoCreateProject` task-item folder** — `GetTaskItems` ->
  `Ol.App.Session.GetDefaultFolder(...)` becomes `Func<Items> _getTaskItems`.
- **`AutoAssignContext` / `AutoAssignPeople` MailItemHelper seam** — the private
  `ToHelper`/`AutoFind` `MailItemHelper` construction becomes `Func<object,
  Task<MailItemHelper>> _toHelper`.
- **`FlagTasks` flag-selection dialog** — `GetUserInputFlagsToAdjust`'s
  `TagViewer`/`TagController`/`ShowDialog()` becomes `Func<SortedDictionary<
  string,bool>, List<string>> _flagSelector`, making `GetFlagsToSet(count>1)`
  measurable with a stub.
- **`FlagChangeTrainingQueue` training-queue timer determinism seam** — the
  `TimedAsyncTask(500ms, ...)` created by `Init()` is never awaited in tests; the
  `Immediate` path is driven synchronously and the `Timed` branch is asserted via
  enqueue state. No wall-clock waits.

### Designer file constraint

`EditFilterViewer.designer.cs` (503 lines) technically exceeds the 500-line limit
but is Designer-generated. It is covered by the form partial class's class-level
`[ExcludeFromCodeCoverage]` and stays under the form partial's class-level
exemption. It MUST NOT be hand-split; per the General Code Change Policy,
generated designer code is not hand-edited. If a mechanical reduction is ever
required, it is noted as a generated-code carve-out in the plan. All other
in-scope files are under 500 lines.

## Non-Goals

- **`TaskController` / `TaskViewer` are out of scope** — they belong to sibling
  #297 (`taskvisualization-core-testability-refactor`). #298 must not edit
  `TaskController.cs`.
- **No behavior or UX changes** to any form or helper.
- **No WinForms/VSTO migration** (that is the separate No-COM architecture
  effort).
- No new production dependencies.
- The `UtilitiesCS` `IFlagChange*` interfaces are not modified.

## Dependencies / Touchpoints

- **HARD DEPENDENCY on #297.** This feature (`depends_on: [297]`, wave 1)
  executes only after #297 merges to the epic integration branch
  `epic/winforms-testability-refactor-integration`. Both features modify the
  shared `TaskVisualization.csproj` and `TaskVisualization.Test`; serializing
  them avoids integration-branch merge conflicts.
- **Preserved caller surfaces** (the only production callers that must keep an
  exact surface):
  - `EfcFormController` — `ManageFilters` three-call surface (`new ManageFilters`,
    `LoadFilters`, `Show`).
  - QuickFiler `FlagTasks` factory — the constructor shape and `FlagTasks` type
    name.
- **Other in-repo callers** (`RibbonController`, `TryFunctionalityInConstruction`,
  `AppToDoObjects`, `QfcController`) require no signature edits provided the
  public surfaces above are preserved. All other structural change is internal to
  the `TaskVisualization` project.
- **Shared test scaffolding** — reuse `TaskVisualization.Test/MoqOlToDo.cs`
  (mocks `IApplicationGlobals`, `IOlObjects`, `Categories`, `MailItem`,
  `UserProperties`) and any globals-builder #297 adds, rather than duplicating
  mock scaffolding. The inert `FlagTasks_Test.cs` under the `Z.Disabled.*`
  namespace may be replaced/enabled.
- Required coordination (other teams, CI/CD, release tooling): none beyond the
  epic wave scheduling; execution is driven by `epic-orchestrator` on maintainer
  signal.

### Execution-time assumptions about the post-#297 codebase (Phase 0 must re-verify each)

#297's `spec.md` and atomic plan were template stubs at research time, so #298
cannot bind to specific #297 artifacts by name. The following six assumptions
about the post-#297 integration head MUST be re-verified by the atomic plan's
Phase 0 before implementation:

1. **`ITaskViewer` shape.** `ITaskViewer` exists and `TaskController`'s
   `formInstance` parameter is `ITaskViewer` (or a compatible base). If not,
   `FlagTasks`'s viewer construction seam must be self-contained.
2. **Dialog seam.** A reusable dialog/`MessageBox` seam type (for example
   `ITagPromptService`/`IDialogService`) exists from #297. If absent, #298
   introduces narrow per-call delegate seams (Tag dialog / flag-selection dialog)
   rather than blocking.
3. **Interop adapter.** A reusable Outlook-Interop adapter (folder / items /
   `MailItemHelper`) exists from #297. If absent, #298 uses local injectable
   delegates at each boundary.
4. **`FlagChangeGroup` stability.** `FlagChangeGroup(IApplicationGlobals,
   MailItem)`, `FlagChangeTrainingQueue.Init()`, and the `IFlagChange*`
   interfaces are unchanged by #297.
5. **csproj cleanliness.** `TaskVisualization.csproj` and
   `TaskVisualization.Test.csproj` compile clean at the post-#297 integration
   head before #298 adds its `<Compile Include>` entries (both are non-SDK
   packages.config projects; new files are registered manually).
6. **Exemption state.** #197's `[ExcludeFromCodeCoverage]` annotations are still
   present on the in-scope files at the post-#297 head; #298 removes/narrows them
   as it introduces seams.

**#293 watch item (not a hard dependency).** `EditFilterController.SelectItems`
and `FlagTasks.GetUserInputFlagsToAdjust` construct `TagViewer` + `TagController`
from the `Tags` project, which sibling #293 retargets to `ITagViewer`. #298
depends only on #297, but #293 also lands on the integration branch. Phase 0
verifies whether #293's `TagController` constructor overloads still accept the
arguments these call sites pass; if #293 changed them to `ITagViewer`, the call
sites compile as long as `TagViewer : ITagViewer`.

## Risks & Mitigations

- **Planning against a not-yet-merged upstream (#297).** #298's design assumes
  seams and abstractions #297 is chartered to create, but #297's detailed
  artifacts were stubs at research time. *Mitigation:* the six execution-time
  assumptions above are re-verified at the atomic plan's Phase 0 gate before any
  implementation; each assumption has a documented fallback (self-contained seam
  or local delegate) so a missing #297 abstraction narrows the seam rather than
  blocking the feature.
- **Timer-based `FlagChangeTrainingQueue` determinism.** `Init()` creates a
  `TimedAsyncTask(500ms, ConsumeAsync)`; awaiting it would make tests nondeterministic
  and would violate the banned-API rule. *Mitigation:* the timer seam — tests
  never await the 500ms timer; the `Immediate` path is driven synchronously with
  a mocked `IFlagChangeGroup` returning `Task.CompletedTask`, and the `Timed`
  branch is asserted via enqueue state. No `Thread.Sleep`/`Task.Delay` and no
  wall-clock waits.
- **Sequencing/merge risk on shared csproj + test project.** *Mitigation:* the
  epic serializes #298 after #297 into wave 1; both projects are re-verified
  clean at Phase 0 before `<Compile Include>` entries are added.
- **Over-broad coverage exemptions.** *Mitigation:* exemptions are narrowed to
  irreducible UI/COM wiring, individually justified, and maintainer-ratified;
  testable seams are never exempt.

## Technical Specifications

- **Files created:** `IEditFilterViewer.cs`, `IManageFiltersViewer.cs`,
  `ManageFiltersController.cs`, `FlagCalculations.cs` (or
  `FlagTasksFlagSelection.cs`). Each is manually registered in the non-SDK
  `TaskVisualization.csproj` `<Compile Include>` list.
- **Files changed:** `EditFilterController.cs`, `EditFilterViewer.cs`,
  `ManageFilters.cs`, `FlagTasks.cs`, `AutoCreateProject.cs`,
  `AutoAssignContext.cs`, `AutoAssignPeople.cs`, `FlagChangeTrainingQueue.cs`,
  `FlagChangeItem.cs`. New tests are added in `TaskVisualization.Test`.
- **Public interfaces/contracts affected (behavior unchanged):**
  - New `IEditFilterViewer` — five text properties (`ContextSelectionText`,
    `PeopleSelectionText`, `ProjectSelectionText`, `TopicSelectionText`,
    `FilterNameText`), `Text`, `Show()`, `Hide()`, `Dispose()`, seven `...Click`
    events, `ResetTips()`. Additive over `IForm` (which already supplies
    `ShowDialog()`, `Close()`, `DialogResult`, and every `Form` property/event).
  - New `IManageFiltersViewer` — `SelectedFilter`, `SetFilters(IEnumerable<
    FilterEntry>)`, `RebuildList()`, `Show()`, three `...Click` events.
  - `EditFilterController` field type changes from `EditFilterViewer` to
    `IEditFilterViewer` (internal type; no external caller signature impact).
  - `FlagTasks` constructor and `Run()`: unchanged (invariant).
  - `ManageFilters` public `LoadFilters`/`Show`: unchanged (invariant); internal
    delegation only.
- **Data flow or validation adjustments:** none to observable behavior; the
  extracted `FlagCalculations` statics and `ManageFiltersController` reproduce the
  existing logic paths against seams/mocks instead of live COM/forms.
- **Logging/telemetry updates:** none.
- **Migration or backfill needs:** none.
- **Coverage exemption policy.** Exemptions are narrowed to irreducible UI/COM
  wiring, applied at the method level (or via seam delegation) where possible,
  each individually justified under the ratified CLAUDE.md WinForms/COM policy,
  and maintainer-ratified. The concrete `EditFilterViewer`/`ManageFilters` form
  partial classes carry class-level `[ExcludeFromCodeCoverage]` (also covering
  their `.designer.cs` partials) per the established repo pattern. Testable seams
  are never exempt: `FlagChangeGroup.TryEnqueue`, the extracted
  `FlagCalculations`, `FlagChangeItem`, `FlagChangeTrainingQueue` logic,
  `AutoCreateProject` host-neutral members, and both controllers stay measured.

## Test Strategy

Framework: MSTest + Moq + FluentAssertions. Reuse
`TaskVisualization.Test/MoqOlToDo.cs` for `IApplicationGlobals` mocks. No real
`Form`/`Control` instantiation, no popups, no `Thread.Sleep`/`Task.Delay`, no
temp files; deterministic.

- **Pure-unit tests for host-neutral logic (largest coverage lever, no seam
  needed):**
  - `FlagCalculations`/`FlagTasks` statics: `GetSymbolsDictionary` (excludes
    `All`/`None`, sorted keys); `ConvertFlagStringsToEnum` (empty -> `All`; valid
    strings -> bit-or; invalid ignored); `GetFlagsToSet(1)` -> `All`;
    `GetFlagsToSet(>1)` via injected `_flagSelector` stub -> parsed enum.
  - `AutoCreateProject` host-neutral members: `GetNextProjectID` (mocked
    `ProjInfo`/`IDList` -> seed selection + next-id); `TryAutoExtractProgram`
    (substring match true/false, longest-first ordering); `StripPrefix` (prefix
    present/absent/empty); `FilterList`; `AddChoicesToDict`/`AutoFind` ->
    `NotImplementedException`.
  - `FlagChangeGroup.TryEnqueue`: no difference -> returns false, nothing
    enqueued; additions/removals -> returns true, one `FlagChangeItem` with
    correct `TrainFlags`/`UntrainFlags` from the pure `CompareTo`. Uses a subclass
    overriding the `virtual` `Item`/`Globals`, so no live MailItem.
  - `FlagChangeTrainingQueue`: `Init()` returns self and sets `ConsumerTimer`;
    `Enqueue` with `Options=Immediate` + mocked `IFlagChangeGroup` ->
    `ProcessGroupAsync` invoked, queue drained, guard reset; `Enqueue` with
    `Options=Timed` -> item present in `Queue` (via `InternalsVisibleTo`) and
    timer requested. Never wait on the 500ms timer.
  - `FlagChangeItem`: POCO round-trip; `UntrainFlags`/`TrainFlags` default
    non-null empty.
  - `AutoAssignContext`/`AutoAssignPeople` host-neutral members: `FilterList`;
    the `NotImplementedException` throwers; `AutoFind(null)`/`AutoFind(unknownType)`
    -> `[]`; `AutoFindAsync` with a stubbed `_toHelper` returning null -> `[]`.
- **Mocked-interface controller tests (reusing `MoqOlToDo`, no live form):**
  - `EditFilterController` with `Mock<IEditFilterViewer>` via the injected viewer
    factory: `RegisterEventHandlers` wiring; `BtnOk_Click` (sets
    `_filterEntry.Name` from `FilterNameText`, invokes callback when set, calls
    `Hide()`/`Dispose()`); `BtnCancel_Click` (null callback -> `Close()` +
    `RevertToCopy`; callback set -> no close); `Initialize`/`InitializeFactory`
    (selection texts set from `_filterEntry.Flags.*`, `ResetTips()` called);
    `SelectItems` with injected `_tagSelector` (`(false,"X;Y")` -> target text
    written; `(true,_)` -> no write).
  - `ManageFiltersController` with `Mock<IManageFiltersViewer>` + mocked globals:
    `LoadFilters` -> `SetFilters(globals.AF.Filters)`; `AddFilter` -> seam invoked
    with the callback + `RebuildList`; `EditFilterCallback` ->
    `AF.Filters.Add`/`Serialize` + `RebuildList`; `EditSelected`/`DeleteSelected`
    -> read `SelectedFilter` (behavior preserved).
- **Scenario completeness:** positive, negative, and edge cases per unit (empty
  inputs, cancelled dialogs, null/unknown types, no-difference enqueue).
- **Error handling and logging verification:** assert the `NotImplementedException`
  throwers; assert guarded branches (null callback, cancelled selection). No
  logging changes to verify.
- **Invariant validation:** tests confirm `ManageFilters`'s preserved three-call
  surface and `FlagTasks`'s constructor/`Run` surface remain intact (compile-time
  and behavior via the controllers/seams).
- **Determinism confirmation:** no `Form`/`Control` instantiated; no
  `ShowDialog`/`Show`/`MessageBox` executes (all behind factory/delegate seams
  returning canned results); no timer awaited; no temp files, network, or
  external process; async tests await mocked `Task.CompletedTask`/canned results.
- **Coverage impact and targets:** the `TaskVisualization` project reaches
  >= 80% line coverage overall; any new class targets >= 90% line coverage.
  Exempt files (the two Designer files and the two form partials) do not dilute
  the denominator. The exact post-#297 denominator is recomputed against the
  integration head (assumption 5) before final targets are locked; a baseline
  coverage artifact is captured at plan Phase 0 under
  `<FEATURE>/evidence/baseline/`.
- **Toolchain commands (run in order, restart on any change/failure):**
  1. `dotnet tool run csharpier .` (or `csharpier .`)
  2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe <TaskVisualization.Test assembly> /EnableCodeCoverage`
- **Manual validation steps:** none required; the refactor is code-only and fully
  automatable (no live Outlook process, no popup during tests).

## Definition of Done

- [x] Structure matches this spec; legacy paths retired or redirected
- [x] Invariants validated with tests or comparisons
- [x] Imports/tooling/entry points updated
- [x] Edge cases and error handling verified
- [x] Tests, linting, and type checks clean
- [x] Docs updated (initiative/README/tasks as needed)
- [x] Toolchain pass completed (format → lint → type-check → test)

Alignment with `issue.md` acceptance criteria — the Definition of Done is
satisfied only when all of the following (from `issue.md` `## Acceptance
Criteria`) hold:

- [x] `IEditFilterViewer` and `IManageFiltersViewer` exist, derive from `IForm`, and their concrete forms implement them.
- [x] `EditFilterController` depends on `IEditFilterViewer`; `ManageFilters` logic is testable against `IManageFiltersViewer`.
- [x] Helper classes' host-neutral logic separated from COM interaction with seams at Interop boundaries.
- [x] No touched production file exceeds 500 lines.
- [x] No unit test constructs a live form/window or triggers a popup.
- [x] `TaskVisualization` project reaches >= 80% line coverage overall.
- [x] Full C# toolchain (csharpier → analyzers → nullable → MSTest) passes with no regression.

## User Story Applicability

`user-story.md` is NOT applicable for this refactor child feature and is
intentionally absent. Per the epic's Design-Phase Deliverables
(`docs/features/epics/winforms-testability-refactor/epic.md`), user-story.md is
not applicable to these testability-refactor children; there is no new end-user
capability or UX change — the work preserves observable behavior while making the
secondary viewers and helpers unit-testable. Acceptance criteria for this feature
are tracked in `issue.md` (`## Acceptance Criteria`) and mirrored in the
Definition of Done above.

## Seeded Test Conditions (from potential)
- [x] EditFilter dialog logic covered via mocked `IEditFilterViewer`.
- [x] ManageFilters list-management logic covered via mocked `IManageFiltersViewer`.
- [x] Flag change grouping/queueing logic covered with pure inputs.
- [x] AutoCreateProject / AutoAssign* logic covered with mocked Interop seams.
