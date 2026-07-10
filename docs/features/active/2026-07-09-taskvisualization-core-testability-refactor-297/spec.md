# taskvisualization-core-testability-refactor - Refactor Spec

- **Issue:** #297
- **Parent (optional):** winforms-testability-refactor (#295)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-09
- **Status:** Ready for Planning (revised: maintainer-ratified STA last-resort refinement applied)
- **Version:** 1.1

## Intent & Outcomes

This is a testability refactor of the `TaskVisualization` project core.
`TaskVisualization/TaskController.cs` is 1861 lines — more than 3.7x the
repository 500-line file limit — and is bound directly to the concrete
`TaskViewer` WinForms type (262 lines + a 1422-line designer), mixing
host-neutral business logic with WinForms/COM (Outlook Interop) interaction. Its
logic cannot be unit-tested without instantiating live forms, which violates the
unit-test policy, so the core of the `TaskVisualization` project is effectively
uncovered.

Target outcomes:

- Introduce an `ITaskViewer` interface deriving from
  `UtilitiesCS.Interfaces.IWinForm.IForm`. `ITaskViewer` exposes an intent-named
  primitive facade (for example `TaskNameText`, `ContextText`, `PeopleText`,
  `ProjectText`, `TopicText`, `DurationText`, `PrioritySelectedItem`,
  `KbSelectedItem`, `TodayChecked`, `BullpinChecked`, `FlagAsTaskChecked`,
  `ReminderValue`, `ReminderChecked`, `DueDateValue`, `DueDateChecked`,
  `FocusDuration()`, `SetController(...)`). Facade members are primitives only —
  no `System.Windows.Forms` control types leak across the interface. The ~50
  accelerator / navigation controls are deliberately NOT placed on the interface:
  they are keyed by object identity and require live parents (`TipsController`
  throws without a live `TableLayoutPanel`/`Panel` parent), so they cannot be
  represented as intent-named primitives or mocked without constructing real
  controls.
- Decompose `TaskController.cs` (1861 lines) into multiple files, each <= 500
  lines, using partial classes (to preserve shared private accelerator state)
  plus small extracted pure-logic helper classes.
- Separate host-neutral logic (`TaskDurationParser`, `TaskPriorityMapper`, and
  the facade-reachable controller logic) from WinForms/COM interaction.
- REMOVE the class-level `[ExcludeFromCodeCoverage]` currently on
  `TaskController` (source line 20) so the testable partials count toward the
  coverage metric.
- Bring the refactored core to >= 80% line coverage, contributing to the
  project-wide 80% goal that the follow-up secondary feature (#298) completes.

## Invariants (must not change)

The following behaviors, contracts, and external surfaces must remain identical:

- **Observable UI behavior** — end-user flows, dialog outcomes, keyboard
  accelerator behavior, and control state transitions are preserved exactly. This
  is a structure-only refactor; no behavior or UX change is in scope.
- **Sole constructing caller `FlagTasks.cs` needs zero edits** — the constructor
  retargets its `formInstance` parameter from the concrete `TaskViewer` to
  `ITaskViewer` (satisfied because `TaskViewer` will implement `ITaskViewer`).
  Any new seam parameters (`ITagPromptService`, `Action<string>` notifier,
  `Func<MailItem, Task<MailItemHelper>>` factory) MUST be optional-with-default
  seam parameters carrying production defaults, so `FlagTasks.cs` compiles and
  behaves unchanged without edits.
- **Event routing and method names preserved** — the partial-class split retains
  all public/internal method names that `TaskViewer` event handlers call
  (`OK_Action`, `Cancel_Action`, `KeyboardHandler_KeyDown`,
  `KeyboardHandler_KeyPress`, `Assign*`, `Shortcut_*`, `AutoAssignAllAsync`,
  `Today_Change`, `Bullpin_Change`, `FlagAsTask_Change`, etc.), so event routing
  is unchanged.
- **No new production dependencies** — the refactor introduces no third-party
  package. Only already-referenced libraries are used.
- Performance characteristics to preserve (latency/throughput/memory): no
  measurable change; the refactor is structural and adds only interface
  indirection and thin facade delegation.
- Compatibility guarantees (CLI flags, config schemas, versions): none affected;
  there are no CLI flags or config schemas in this UI project surface.

## Scope (structural changes)

The decomposition follows the research design (Section D). `TaskController` is
split into partial classes to preserve the shared mutable accelerator fields
(`_xlCtrlsActive`, `_altActive`, `_altLevel`, `_activeNavGroup`, `_active`,
`_options`) without widening visibility, plus extracted pure-logic helper
classes. Concrete-control access in the WinForms-bound partials is funneled
through a single `private TaskViewer Form => (TaskViewer)_viewer;` accessor,
which is confined to those partials so the retarget to `ITaskViewer` does not
force ~50 control properties onto the interface.

Ten production files (created or changed), plus dedicated STA test files (see
Test Strategy); the largest is approximately 430 lines, well under the 500-line
limit:

1. `ITaskViewer.cs` — `interface ITaskViewer : IForm`; intent-named primitive
   facade members plus `SetController(...)`. Interface-only (~40 lines).
   - Companion: `ITaskViewerControls.cs` — `interface ITaskViewerControls`; the
     control-identity companion surface added by the maintainer-ratified STA
     last-resort refinement (see Coverage Exemption Constraint). It exposes, as their
     real `System.Windows.Forms` `Label`/`Control` types (deliberately NOT
     primitives), exactly the accelerator/navigation control-identity members the two
     WinForms-bound partials read off `TaskViewer` (`XlSector1..4`, `C{1..4}S{1..4}`,
     `XlProject`/`XlPeople`/`XlTopic`/`XlContext`, the option `Control`s and caption
     `Label`s). Its purpose is to let dedicated STA tests supply real never-shown
     in-memory controls to the control-map/accelerator logic without a `Form`; it
     keeps `ITaskViewer` primitives-only. Interface-only (~25 lines).
2. `TaskController.cs` — `partial class TaskController`: fields, ctors,
   `Options`/`ChangedFlags`/assign properties, `Initialize` split into data
   writes plus delegation to accelerator init, the data part of
   `ActivateOptions`. `_viewer` typed as `ITaskViewer`; the `Form` cast accessor
   is declared here. No class-level exemption (~320 lines).
3. `TaskController.Actions.cs` — `partial class`: `AssignPeople/Context/Project/
   Topic` (using `ITagPromptService`), `Assign_KB`, `Assign_Priority`,
   `Today_Change`, `Bullpin_Change`, `FlagAsTask_Change`, `Shortcut_*`,
   `AnyCategorySelected`, `SetFlag`, `MergeFlag`, `MergeToCollection`,
   `OK_Action`, `Cancel_Action`, `CaptureDuration` (using the duration parser and
   the notifier seam). Testable through `ITaskViewer` + seams (~380 lines).
4. `TaskController.Flags.cs` — `partial class`: `ApplyChanges` (COM iteration /
   orchestration), both `ApplyChange` overloads, `AreCollectionsEqual`
   (~180 lines).
5. `TaskController.Accelerator.cs` — `partial class`: keyboard handlers,
   `SuppressKeystrokes`, `MouseFilter_FormClicked`, the Keyboard UI region
   (`ToggleXl`, `UpdateCaptions`, `ExecuteXlAction`, `ToggleXlGroupNav`,
   `DeactivateActiveXlGroup`, `ActivateXlGroup` x3, `RecurseXl`), accelerator init,
   and the `PostMessage` P/Invoke. (`AutoAssignAllAsync` is relocated to the
   non-exempt `TaskController.Actions.cs` so it stays measured.) Control-identity
   reads route through the `ITaskViewerControls` accessor; only `.Handle`/
   `PostMessage` route through the `Form` accessor. Under the STA last-resort
   refinement this partial is NOT file-level exempt: the accelerator state machine
   is measured via STA tests; only the `PostMessage`/handle/focus residue is exempt
   at method/branch level (~430 lines).
6. `TaskController.ControlMaps.cs` — `partial class`: `GetOptionsLookup`/
   `GetCaptionLookup`/`GetControlLookup` (x2 each), `GetControlRelationships`,
   the `ControlRelationship` struct, `OptionsGroups`, `NavTips`. Control-identity
   reads route through the `ITaskViewerControls` accessor. Under the STA last-resort
   refinement this partial is NOT file-level exempt: the control-identity builders
   (including `TipsController` construction) are measured via STA tests against real
   never-shown in-memory `Label`s parented in real `TableLayoutPanel`/`Panel`
   containers (~400 lines).
7. `TaskDurationParser.cs` — pure parse+validate of the duration string into a
   `(bool ok, int minutes, string error)` result. Fully testable; target
   >= 90%. Reusable by #298 (~40 lines).
8. `TaskPriorityMapper.cs` — `OlImportance` <-> display string ("High"/"Low"/
   "Normal") both directions, used by `Initialize` and `Assign_Priority`. Fully
   testable; target >= 90% (~40 lines).
9. `ITagPromptService.cs` (+ `TagPromptService.cs` adapter) — the prompt seam for
   the four assign dialogs. The adapter is the only place that constructs
   `TagViewer`/`TagController`. Interface testable via mock; adapter is WF/dialog
   (interface ~25 lines; adapter ~90 lines).

Additional structural notes:

- **`ITaskViewer` member summary**: intent-named primitive getters/setters for
  the data-bearing controls (`TaskName`, `CategorySelection`, `PeopleSelection`,
  `ProjectSelection`, `TopicSelection`, `Duration`, `PriorityBox`, `KbSelector`,
  `CbxToday`, `CbxBullpin`, `CbxFlagAsTask`, `DtReminder`, `DtDuedate`),
  `FocusDuration()`, and `SetController(...)`. Form-level members
  (`AcceptButton`, `CancelButton`, `DialogResult`, `Hide`, `Dispose`, `Invoke`,
  `InvokeRequired`, `Focus`, `Controls`) resolve through the `IForm` base and are
  not duplicated on `ITaskViewer`.
- Two accessors are confined to the WinForms-bound partials (files 5 and 6): a
  `private ITaskViewerControls ViewerControls => (ITaskViewerControls)_viewer;`
  accessor carries all measurable control-identity reads (so STA tests can supply
  real in-memory controls), and a `private TaskViewer Form => (TaskViewer)_viewer;`
  accessor carries only the irreducible live-window-handle residue (`.Handle`,
  `PostMessage`). The testable primitive core sees only `ITaskViewer`.
- **`ITaskViewerControls` companion surface** (STA last-resort refinement): the
  ~50 accelerator/navigation controls are exposed on this dedicated interface as
  real `Label`/`Control` types — not on the primitives-only `ITaskViewer` — because
  their real object identity and live parenting ARE the logic under test. A mock of
  these members cannot substitute the logic (`TipsController` throws without a real
  parented container; the lookup dictionaries key on control object-identity), so
  the STA tests populate the surface with real never-shown in-memory controls rather
  than mocks. This is why STA is the last resort (no logic-isolating seam is
  feasible) and why `Form`-derived types are still never constructed in tests.
- **`ITagPromptService` seam** replaces the four in-line `new TagViewer(); ...
  ShowDialog()` calls in `AssignPeople/Context/Project/Topic`. This seam is
  designed for REUSE by sibling #298 (`EditFilterController`/`ManageFilters` open
  the same class of Tags dialogs); it is defined in `TaskVisualization` (or a
  shared location) so #298 reuses it rather than duplicating.
- **`Action<string>` notifier seam** replaces the `MessageBox.Show(...)` calls in
  `CaptureDuration`; the production default is `MessageBox.Show`, and tests inject
  a capturing delegate to assert message content.
- **`Func<MailItem, Task<MailItemHelper>>` factory seam** replaces the direct
  `MailItemHelper.FromMailItemAsync(...)` call in `AutoAssignAllAsync`, enabling
  that method to be exercised without a live Outlook process; the production
  default is `m => MailItemHelper.FromMailItemAsync(m, Globals, default, false)`.
- **`TaskViewer.cs` gains `: ITaskViewer`** with thin facade property
  delegation (for example `public string TaskNameText { get => TaskName.Text; set
  => TaskName.Text = value; }`) and the relocated accept/cancel button wiring in
  `SetController`. It remains Form-derived and exempt.

## Non-Goals

- **Sibling #298 scope, explicitly excluded here**: `EditFilterController`,
  `EditFilterViewer`, `ManageFilters`, `FlagTasks.cs`, `FlagChange*`,
  `AutoCreate*`, and `AutoAssign*` — except where `TaskController` depends on
  them (for example the `IAutoAssign` seam it already consumes, or the
  `FlagChangeGroup` iteration inside `ApplyChanges`). `FlagTasks.cs` is a `Flag*`
  helper owned by #298; #297 only guarantees its call-site compatibility (zero
  edits), and does not otherwise refactor it.
- No behavior or UX changes to the forms themselves.
- No migration off WinForms/VSTO (that is the separate No-COM architecture
  effort).
- No new production dependencies.

## Dependencies / Touchpoints

- **`UtilitiesCS` `IForm`** — `ITaskViewer` derives from
  `UtilitiesCS.Interfaces.IWinForm.IForm`; the `IForm` inheritance chain
  (`IForm : IContainerControl, IScrollableControl : IControl`, and `IControl`
  extending `IComponent, ISynchronizeInvoke, IWin32Window, IDisposable`) supplies
  the Form-level surface the controller uses, so those members are not duplicated
  on `ITaskViewer`.
- **`TaskVisualization.Test`** — existing MSTest project (net4.8.1, MSTest 4.2.2,
  Moq 4.20.72, FluentAssertions 8.9.0), all already referenced, with the same
  analyzer stack as production. `InternalsVisibleTo("TaskVisualization.Test")` is
  declared in `FlagTasks.cs`, so `internal` controller members are visible to
  tests. The reusable `MoqOlToDo` helper (mock `Categories`/`MailItem`/
  `IApplicationGlobals`) is reused as-is.
- **Sibling #298 depends on this feature** — both modify the same
  `TaskVisualization.csproj` and `TaskVisualization.Test` project. #298 is
  serialized after #297 (executes only after #297 merges) to avoid
  integration-branch merge conflicts. This feature is wave 0; #298 is wave 1.
- Required coordination (other teams, CI/CD, release tooling): the legacy
  non-SDK csproj uses `packages.config`, so newly created files must be added to
  the `<Compile>` groups of the production and test csproj files manually. The
  maintainer-ratified `[ExcludeFromCodeCoverage]` inventory is a review-time
  approval step (see Technical Specifications).

## Risks & Mitigations

- **Largest decomposition in the epic (1861 lines): highest regression risk.**
  Mitigation: behavior preservation via partial classes that retain the shared
  accelerator state (`_xlCtrlsActive`, `_altActive`, `_altLevel`,
  `_activeNavGroup`, `_active`, `_options`) and preserve every event-routed
  method name, so the split is name- and behavior-preserving rather than a
  rewrite.
- **No existing regression safety net.** The single existing test in
  `FlagTasks_Test.cs` is fully commented out (namespace `Z.Disabled...`), so there
  is no behavioral regression net today. Mitigation: the refactor relies on
  compile-time preservation (identical method names, identical facade-mapped
  behavior) plus the new MSTest coverage added in this feature.
- **Public contract change**: the controller constructor parameter type changes
  from `TaskViewer` to `ITaskViewer`. Mitigation: `TaskViewer` implements
  `ITaskViewer`; any new seam parameters are optional-with-default, keeping the
  sole caller edit-free.
- **Legacy non-SDK csproj (`packages.config`)**: new files must be added to the
  csproj manually. Mitigation: the atomic plan enumerates each `<Compile>`
  addition.
- **Serialization with #298**: the follow-up feature modifies the same csproj /
  test project. Mitigation: epic dependency serializes #298 after #297.
- **Coverage exemptions require maintainer ratification and must be minimized.**
  Mitigation: exemptions are scoped narrowly and individually justified (see
  Technical Specifications); testable seams are never exempt from the coverage
  floor.

## Technical Specifications

- **Files/modules expected to change**: the ten files enumerated in Scope
  (`ITaskViewer.cs`, `ITaskViewerControls.cs`, `TaskController.cs`,
  `TaskController.Actions.cs`, `TaskController.Flags.cs`,
  `TaskController.Accelerator.cs`, `TaskController.ControlMaps.cs`,
  `TaskDurationParser.cs`, `TaskPriorityMapper.cs`, `ITagPromptService.cs` +
  `TagPromptService.cs`), plus `TaskViewer.cs` (add `: ITaskViewer,
  ITaskViewerControls`, facade delegation, relocated accept/cancel wiring), the
  dedicated STA test files (`TaskControllerControlMaps.StaTests.cs`,
  `TaskControllerAccelerator.StaTests.cs`, `StaControlHarness.cs`), and the
  production and test `.csproj` `<Compile>` groups. No changes to `FlagTasks.cs`
  (constructor compatibility preserved via optional-with-default seam parameters).
- **Public interfaces/contracts affected**: `TaskController` constructor
  parameter retargeted from `TaskViewer` to `ITaskViewer`; new optional
  seam parameters (`ITagPromptService`, `Action<string>` notifier,
  `Func<MailItem, Task<MailItemHelper>>` factory) with production defaults; new
  public `ITaskViewer`, `ITaskViewerControls`, and `ITagPromptService` interfaces.
  `ITaskViewerControls` is the control-identity companion surface (real
  `Label`/`Control` types) required by the STA last-resort refinement.
- **Data flow or validation adjustments**: the duration parse/validate logic
  moves into `TaskDurationParser`; the priority mapping moves into
  `TaskPriorityMapper`. Dialog and popup calls are routed through the
  `ITagPromptService` and `Action<string>` notifier seams. No validation rule is
  changed; only its host is separated.
- **Logging/telemetry updates**: none.
- **Migration or backfill needs**: none.

### Coverage Exemption Constraint (binding)

**Maintainer-ratified STA last-resort refinement (2026-07-09).** The maintainer
ratified (epic Shared Design Pattern item 4, `docs/features/epics/winforms-testability-refactor/epic.md`,
the authority for this constraint) a refinement to the original exemption proposal:
in-memory, never-shown WinForms **controls** (`TableLayoutPanel`, `Label`, `Panel`,
`CheckBox`, `Button`) MAY be constructed in unit tests on an STA thread, strictly as
a LAST RESORT where no seam can isolate the logic. This spec adopts the refinement:
the two WinForms-bound partials are NO LONGER file-level exempt. Their control-identity
logic is measured by dedicated STA tests, and only the residue that genuinely requires
a live window handle or the message pump remains exempt at method/branch level. The
refinement conditions bind this feature:

- **(a) Seam first; document why no seam isolates the covered logic.** A seam remains
  the required first approach (the primitive `ITaskViewer` facade, `ITagPromptService`,
  the notifier and factory seams). For the control-map/accelerator regions no seam can
  isolate the logic from real controls (`TipsController` throws without a real parented
  `TableLayoutPanel`/`Panel`; dictionaries key on control object-identity;
  `.BackColor`/`Button.PerformClick` require real `Control`s). The control-identity
  surface is isolated on the `ITaskViewerControls` companion interface (real control
  types) so tests supply real in-memory controls; each STA test records this rationale.
- **(b) Dedicated STA files.** All STA-bound tests live in `*.StaTests.cs` files using
  `[STATestClass]`/`[STATestMethod]` (MSTest 4.2.2), keeping the STA surface minimal.
- **(c) No pump / no popups / dispose per test.** No `Show()`/`ShowDialog()`; no
  `PostMessage` round-trip assertions, no `DoEvents`, no timers; all controls disposed
  per test; STA assertions target reliable state (`.BackColor`, `.Text`, `.Checked`,
  returned tuples/dictionaries), not the parent-dependent `.Visible` getter.
- **(d) No Form-derived types.** No test constructs `TaskViewer` or any `Form`-derived
  type, even unshown.

The original research proposal (file-level `[ExcludeFromCodeCoverage]` on the two
WinForms-bound partials — `TaskController.Accelerator.cs` ~430 lines and
`TaskController.ControlMaps.cs` ~400 lines, approximately 900 lines of accelerator /
control-map code) is superseded by the refinement above: `ControlMaps.cs` becomes
measured via STA with no file-level exemption, and `Accelerator.cs` retains only
method/branch-level exemptions for its `PostMessage`/handle/focus residue. This spec
constrains the residual exemptions as follows:

1. **Narrowest justifiable scope.** Exemptions are applied at the narrowest
   scope that is justifiable, not as a convenience. Prefer method- or
   branch-level exemption over file-level where the file mixes testable and
   irreducible code; file-level exemption is acceptable only when the entire file
   is irreducibly host-bound.
2. **Per-region individual justification.** Each exemption is individually
   justified under the ratified COM/VSTO/WinForms exemption policy in `CLAUDE.md`
   (form-derived / Designer-generated / live-control-bound code without an
   injectable seam). The justification must name the specific irreducible
   dependency (for example `Label`/`Control` identity, `.Visible`/`.BackColor`/
   `.Handle`, `PerformClick`, `PostMessage`, `TipsController` requiring a live
   parent).
3. **Reducibility explicitly assessed.** Any logic that CAN be seamed out of an
   exempt region MUST be extracted into a testable host-neutral unit and covered.
   Testable seams are never exempt. This assessment must be recorded per exempt
   region so a reviewer can confirm no coverable logic was hidden behind an
   exemption.
4. **Final exemption inventory listed for maintainer ratification.** The atomic
   plan must list the final `[ExcludeFromCodeCoverage]` inventory (file-level and
   any method/branch-level, plus `TaskViewer.cs`, `TaskViewer.Designer.cs`, and
   the `TagPromptService` adapter) so the maintainer can ratify it at review.
5. **Blanket partial-level exemption is not acceptable** without the per-region
   justification and reducibility assessment above.

Explicitly NOT exempt (must count toward coverage): `ITaskViewer`-driven
controller logic, `TaskDurationParser`, `TaskPriorityMapper`, `SetFlag`,
`MergeFlag`, `MergeToCollection`, `AreCollectionsEqual`, `ApplyChange` (both
overloads), the assign/shortcut model updates, the `OK_Action`/`Cancel_Action`
decisions, the `TaskController.ControlMaps.cs` control-identity builders
(`GetControlRelationships`, `GetOptionsLookup`/`GetCaptionLookup`/`GetControlLookup`,
`OptionsGroups`, `NavTips`) now measured via STA, and the measured accelerator state
machine in `TaskController.Accelerator.cs` (`ToggleXl`, `UpdateCaptions`, the
non-DateTimePicker branches of `ExecuteXlAction`, `ToggleXlGroupNav`,
`DeactivateActiveXlGroup`, `ActivateXlGroup` x3, `RecurseXl`,
`KeyboardHandler_KeyDown`/`KeyPress`, `InitializeAccelerators`). Removal of the
current class-level `[ExcludeFromCodeCoverage]` (source line 20) is required so these
lines are measured. Only the `Accelerator.cs` `PostMessage`/handle/focus residue
(`DispatchDateTimePickerClick`, the `PostMessage` `extern`, and any reconciled
focus/handle/pump-bound handler) remains exempt, at method/branch level.

## Test Strategy

- **Regression tests to add or update**: new MSTest files under
  `TaskVisualization.Test/` mirroring source names (`TaskControllerTests.cs`,
  `TaskControllerActionsTests.cs`, `TaskControllerFlagsTests.cs`,
  `TaskDurationParserTests.cs`, `TaskPriorityMapperTests.cs`), plus the dedicated
  STA-bound files required by the STA last-resort refinement
  (`TaskControllerControlMaps.StaTests.cs`, `TaskControllerAccelerator.StaTests.cs`)
  and the STA support fixture `StaControlHarness.cs`, all added to the test
  `.csproj` `<Compile>` group. The `Z.Disabled` namespace is not reused. Test
  doubles for the primitive controller tests: `Mock<ITaskViewer>` via Moq with
  `SetupAllProperties()` and explicit `InvokeRequired => false` (no real forms),
  injected `Action<string>` notifier, `Mock<ITagPromptService>`,
  `Func<MailItem, Task<MailItemHelper>>` factory stub, and `Mock<IAutoAssign>`.
  Reuse the `MoqOlToDo` builders for `Categories`/`MailItem`/`ToDoItem`.
- **STA test mechanics (control-identity regions)**: the STA files use
  `[STATestClass]`/`[STATestMethod]` (MSTest 4.2.2 — verified present and already used
  in-repo, e.g. `UtilitiesCS.Test/HelperClasses/WindowsForms/WinFormsLayoutTests.cs`;
  global STA stays disabled so only opt-in classes run STA). They construct real,
  never-shown, in-memory controls (`Label`, `Panel`, `TableLayoutPanel`, `Button`,
  `CheckBox`) via `StaControlHarness.cs`, supplied to `TaskController` through the
  `ITaskViewerControls` surface (a `Mock<ITaskViewerControls>` returning the real
  controls, or a non-`Form` fake). No `Form`-derived type is constructed; no window
  is shown; no message pump is used. Fallback if the attributes were unavailable
  (not required here): an assembly-scoped `.runsettings`
  `<ExecutionThreadApartmentState>STA</ExecutionThreadApartmentState>`, at the cost of
  forcing the whole assembly to STA and losing default parallelism — which is why the
  attribute-scoped approach is preferred.
- **Invariant validation tests**: assert facade writes via
  `mock.VerifySet(...)`/`mock.Verify(...)` (for example
  `mock.Verify(v => v.Hide())`, `mock.Verify(v => v.FocusDuration())`) so observed
  behavior through `ITaskViewer` is unchanged. `OK_Action`/`Cancel_Action`
  lifecycle verified on the mock rather than by executing a real form.
- **Edge cases and negative scenarios**: for each unit, positive, negative, and
  edge/boundary cases — for example `TaskDurationParser` positive ("15"), zero
  ("0"), negative ("-3"), non-integer ("abc"), empty/whitespace;
  `TaskPriorityMapper` each mapping both directions plus unknown-fallback to
  Normal; `AreCollectionsEqual` equal-any-order / disjoint / null handling /
  duplicate collapse; `Assign*` cancel-vs-select paths;
  `AutoAssignAllAsync` non-mail early return vs mail path.
- **Error handling and logging verification**: the `CaptureDuration` invalid path
  invokes the injected notifier `Action<string>` (assert captured message) and
  leaves `_active.TotalWork` unchanged; the `MessageBox`/dialog paths are exercised
  only through seams.
- **Coverage impact and targets**: the refactored core (files 2–6, 7–8, and the
  `ITagPromptService` mock paths) reaches >= 80% line coverage; the new helper
  classes (`TaskDurationParser`, `TaskPriorityMapper`) target >= 90% per policy.
  Under the STA last-resort refinement, `TaskController.ControlMaps.cs` (file 6) and
  the measured portion of `TaskController.Accelerator.cs` (file 5) are now IN the
  denominator and covered via the STA tests; only the `Accelerator.cs`
  `PostMessage`/handle/focus residue is removed from the denominator, via the
  ratified per-region-justified method/branch-level exemption inventory.
- **Determinism constraints**: no `Form`-derived instantiation anywhere (even
  unshown); the non-STA controller tests construct no live controls at all
  (`Mock<ITaskViewer>` only). The STA-bound tests (`*.StaTests.cs`) construct only
  never-shown, in-memory leaf/container controls on an STA thread, disposed per test,
  with no shown window and no message pump (no `PostMessage` round-trips, no
  `DoEvents`, no timers); STA assertions target reliable state (`.BackColor`,
  `.Text`, `.Checked`, returned tuples/dictionaries), not the parent-dependent
  `.Visible` getter. No popups (`MessageBox.Show` and `TagViewer.ShowDialog` replaced
  by seams); no `Thread.Sleep`/`Task.Delay` (already banned by BannedApiAnalyzers);
  `Task.Run(ApplyChanges)` is awaited to completion over in-memory mocks; no temp
  files; no network; fixed `DateTime` inputs (no `DateTime.Now`/`UtcNow` in the
  refactored core; new tests must not assert on time-derived values from
  `MoqOlToDo.MailItemMock`).
- **Toolchain commands to run (format → lint → type-check → test)**:
  1. `dotnet tool run csharpier .`
  2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe <TaskVisualization.Test.dll> /EnableCodeCoverage`
- **Manual validation steps**: none required at implementation time; verification
  is fully automatable. The only non-automatable step is the maintainer's
  review-time ratification of the `[ExcludeFromCodeCoverage]` inventory.

## Definition of Done

- [ ] Structure matches this spec; `TaskController.cs` decomposed so no in-scope
      production file exceeds 500 lines
- [ ] `ITaskViewer` exists, derives from `IForm`, and `TaskViewer` implements it;
      `TaskController` depends on `ITaskViewer`, not the concrete form
- [ ] Host-neutral logic separated from COM/WinForms interaction; class-level
      `[ExcludeFromCodeCoverage]` on `TaskController` removed
- [ ] No unit test constructs a live form/window or triggers a popup; seams
      (`ITagPromptService`, notifier, factory) injected
- [ ] Refactored core reaches >= 80% line coverage; new helper classes >= 90%;
      exemption inventory listed for maintainer ratification
- [ ] Control-identity regions measured via STA last-resort per the ratified
      refinement: `TaskController.ControlMaps.cs` and the measured portion of
      `TaskController.Accelerator.cs` carry no file-level exemption and are covered by
      dedicated `*.StaTests.cs` (`[STATestClass]`/`[STATestMethod]`) against real
      never-shown in-memory controls; no `Form`-derived type is constructed; only the
      `PostMessage`/handle/focus residue remains exempt at method/branch level
- [ ] Edge cases and error handling verified (positive/negative/edge per unit)
- [ ] Tests, linting, and type checks clean
- [ ] Docs updated (spec/plan/epic manifest as needed)
- [ ] Full C# toolchain pass completed (format → lint → type-check → test) with no
      regression

## Seeded Test Conditions (from potential)
- [ ] Business-logic units (filtering, sorting, state shaping) covered with pure inputs.
- [ ] Dialog-driven paths covered via seams intercepting `MessageBox`/input dialogs.
- [ ] Event handler logic covered via a mocked `ITaskViewer`.
- [ ] Outlook Interop boundaries mocked behind seams.

## User Story Applicability

`user-story.md` is NOT applicable for this refactor child and is intentionally
absent. This feature is a testability/structure refactor with no new end-user
behavior; per the epic's Design-Phase Deliverables, user-story.md is not produced
for these refactor children. The acceptance criteria for #297 are carried by
`issue.md` and this `spec.md`.
