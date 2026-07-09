# tagcontroller-testability-refactor - Refactor Spec

- **Issue:** #293
- **Parent (optional):** Epic winforms-testability-refactor (#295), wave 0
- **Owner:** drmoisan
- **Last Updated:** 2026-07-09
- **Status:** Ready for Planning
- **Version:** 1.0

> **user-story.md is intentionally absent.** This is a testability refactor child of
> epic #295. Per the epic manifest (`## Design-Phase Deliverables`), `user-story.md`
> is not applicable to refactor children: the work introduces no new end-user behavior
> or UX. `spec.md` is the sole authoritative acceptance-criteria source for this
> feature, alongside the acceptance criteria in `issue.md`.

## Intent & Outcomes

`Tags/TagController.cs` is 877 lines, exceeding the repository 500-line file-size
limit, and mixes host-neutral business logic (dictionary filtering, search parsing,
prefix handling, selection state) with direct Windows Forms / Outlook Interop COM
interaction. Many internal and public methods have no unit-test coverage because the
class is bound directly to the concrete `TagViewer` WinForms type, which cannot be
instantiated or exercised in a unit test without a live UI/COM environment.

This feature refactors the `Tags` project for testability while preserving observable
behavior. Intended outcomes:

- **`ITagViewer` seam.** Introduce an `ITagViewer` interface deriving from
  `UtilitiesCS.Interfaces.IWinForm.IForm`; `TagViewer` implements it; `TagController`
  depends only on `ITagViewer`, never the concrete `TagViewer`.
- **COM/logic separation.** Extract host-neutral business logic into dedicated files
  so pure decision logic no longer mixes with COM/WinForms calls. Dialog and
  focus-rectangle COM dependencies are isolated behind seams.
- **File-size compliance.** Split the 877-line controller along logical boundaries so
  every resulting production file is `<= 500` lines.
- **Coverage.** The `Tags` project reaches `>= 80%` line coverage (epic #295 goal);
  new modules reach `>= 90%`. Unit tests run deterministically with no live forms, no
  popups, no UI-thread dependence.

## Invariants (must not change)

The following behaviors and external surfaces must remain identical after the refactor:

- **Observable UI behavior of TagViewer flows.** The end-user experience of the tag
  selection dialog — search filtering, archive hiding, option toggling, keyboard
  navigation (Up/Down/PageUp/PageDown/Enter/Escape), auto-assign, add-color-category,
  new-category prompts, OK/Cancel exit — must be functionally unchanged. The viewer
  maps the new intent members 1:1 onto the existing designer controls.
- **`TagLauncher` public API.** `TagLauncher`'s public surface (including its
  `Viewer` property typed as the concrete `TagViewer`, `LaunchAndSelect`,
  `LaunchAndFindMatch`, and the `GetAutoAssign` factory entry points) must remain
  callable as it is today by existing production callers.
- **`TagViewer.SetController(TagController)`.** The `SetController` method keeps its
  `TagController` parameter type; `CheckBoxController._parent` stays typed as the
  concrete `TagController`; the public callback methods `CheckBoxController` invokes
  stay public on `TagController`.
- **Existing passing tests are part of the spec.** Current passing tests in
  `Tags.Test` encode required behavior. Where a test must be migrated to a
  deterministic pattern (mocked `ITagViewer`, injected `IUserPrompt`, `Task`-awaited
  auto-assign), the assertions on behavior/state are preserved or strengthened, never
  weakened.
- **No new production dependencies.** No third-party package is added to the `Tags`
  project. `Tags.Test` already references Moq 4.20, FluentAssertions 8.9, and MSTest
  4.2; no additional test dependencies are introduced.
- Performance characteristics to preserve: none beyond current behavior; this is a
  structural refactor with no performance-sensitive path.
- Compatibility guarantees: `.NET Framework v4.8.1`, non-SDK csproj, `packages.config`
  build model unchanged.

## Scope (structural changes)

Concrete decomposition and seam design, drawn from the research artifact
(`research/research-findings.2026-07-09T21-30-00Z.md`):

### File decomposition (all `<= 500` lines)

| File | New/Changed | Responsibility | Approx. lines |
|---|---|---|---|
| `Tags/ITagViewer.cs` | new | Viewer interface `: IForm`. Interface-only, no executable lines. | ~65 |
| `Tags/IUserPrompt.cs` | new | Dialog seam interface. | ~20 |
| `Tags/WinFormsUserPrompt.cs` | new | Production adapter over `MessageBox`/`InputBox`. Thin, host-bound. | ~35 |
| `Tags/TagSelectionModel.cs` | new | Host-neutral selection/search/filter/prefix logic; owns the dictionaries and selection state. Zero WinForms references. | ~210 |
| `Tags/TagController.cs` | changed | Constructors, fields, `WireEvents`, button/keyboard handlers, public properties, `ResolveMailItem`, `SetAutoAssignState`, `TryGetAutoAssignment`, `AddColorCategory`, `GetUserInputCategory`. Delegates state to `TagSelectionModel`, UI to `ITagViewer`/`IUserPrompt`. | ~240 |
| `Tags/TagController.Rendering.cs` | new (partial of same class) | `LoadControls`, `RemoveControls`, `FilterToSelected`, `Select_*` navigation, `FocusCheckbox`, the `DrawFocus` seam. | ~180 |
| `Tags/TagViewer.cs` | changed | `: Form, ITagViewer`; implement intent members mapping to designer controls; keep `SetController`. | ~120 |
| `Tags/LauncherAutoAssign.cs` | new (extracted) | Move `LauncherAutoAssign` out of `TagLauncher.cs` WITHOUT `[ExcludeFromCodeCoverage]` so its pass-through logic is testable. | ~90 |
| `Tags/TagLauncher.cs` | changed | Remaining live-form launcher + globals wiring; stays `[ExcludeFromCodeCoverage]`. | ~175 |

`TagController` remains a single public partial class split across `TagController.cs`
and `TagController.Rendering.cs`, so `TagLauncher` and `CheckBoxController` are
unaffected by the split.

### `ITagViewer` member summary (`: IForm`)

- **Reused from `IForm` (not redeclared):** `Close()`, `KeyPreview`, `ShowDialog()`.
- **Command intent events** (replace raw Button/TextBox/CheckBox event subscriptions):
  `OkClicked`, `CancelClicked`, `NewClicked`, `AutoAssignClicked`, `SearchTextChanged`,
  `SearchKeyDown` (`KeyEventHandler`), `SearchKeyUp` (`KeyEventHandler`),
  `HideArchiveChanged`, `ViewKeyDown` (`KeyEventHandler`; maps to the form `KeyDown`
  that `IForm` does not expose), `OptionsPreviewKeyDown` (`PreviewKeyDownEventHandler`),
  `OptionsKeyDown` (`KeyEventHandler`).
- **State intent properties:** `HideArchiveChecked { get; }`, `AutoAssignVisible
  { get; set; }`, `AutoAssignEnabled { get; set; }`, `ButtonNewVisible { get; set; }`,
  `SearchTextValue { get; set; }`, `SearchSelectionStart { get; }`, `Caption
  { get; set; }`.
- **Intent methods / option-panel abstraction:** `ControlPosition
  CaptureAndRemoveTemplate()`, `void FocusSearch()`, `void AddOptionControl(CheckBox)`,
  `void RemoveOptionControl(CheckBox)`, `IReadOnlyList<CheckBox> OptionControls { get; }`,
  `int OptionsPanelHeight { get; }`, `int OptionsScrollMaximum { get; }`, `Point
  OptionsAutoScrollPosition { get; set; }`.
- **Retained:** `void SetController(TagController controller);` stays on the interface
  so `TagLauncher` and `CheckBoxController._parent` are unchanged.

`CheckBox` remains the option element type: it is constructible headless, and its
layout/property members do not realize an HWND. Only `.Handle`/focus-rectangle drawing
forces an HWND, which is isolated by the `DrawFocus` delegate seam.

### `IUserPrompt` seam signatures (new, in `Tags`)

- `DialogResult ShowYesNo(string message, string title);` — covers `LoadSelections`
  and `TryGetAutoAssignment` Yes/No prompts.
- `void ShowMessage(string message);` — covers the three `LoadControls` error popups.
- `string GetCategoryInput(string prompt, string title, string defaultResponse);` —
  covers `GetUserInputCategory`; wraps `InputBox.ShowDialog` so tests never construct
  `InputBoxViewer`.

Production default adapter `WinFormsUserPrompt : IUserPrompt` calls `MessageBox.Show`
and `InputBox.ShowDialog`. It is injected via a new optional constructor parameter with
a safe default (`prompt ??= new WinFormsUserPrompt()`) so existing production
construction paths keep current behavior. `GetUserInputCategory` changes from
`internal static` to an instance method routing through the injected seam; its only
caller is `AddColorCategory`.

### `DrawFocus` delegate seam

Extract the two-line `ControlPaint.DrawFocusRectangle(Graphics.FromHwnd(cbx.Handle),
cbx.ClientRectangle)` into a private `DrawFocus(CheckBox)` routed through an injectable
`Action<CheckBox> _drawFocus` defaulting to the real `ControlPaint` call. Tests inject
a no-op. This makes `Select_Ctrl_By_Offset`/`Select_Ctrl_By_Position` (index
arithmetic + focus routing) testable without an HWND. `.Focus()` itself is a safe
no-op when the handle is not created and needs no seam.

### `TagSelectionModel` extraction

A new host-neutral class owning `_dictOriginal`, `_dictOptions`, `_filteredOptions`,
`_selections`, `_filteredSelections`, `_prefix`, and the pure logic: `Search`,
`ParseSearchStrings`, `FilterArchive`, `IsPrefixMissing`, `SelectionAsList`,
`SelectionAsString`, `GetSelections`, `ToggleChoice`/`On`/`Off`, the dictionary part
of `AddOption`, `UpdateSelections`, `ResolvePrefix`, `GetDefaultPrefix`, the dictionary
toggle of `LoadSelections`, and the filtered-set computation for
`FilterToSelected`/`SearchAndReload`. Zero WinForms references.

### `LauncherAutoAssign` extraction

Move the inner `LauncherAutoAssign` class and the pure `GetAutoAssign(filterList,
delegates...)` factory out of `TagLauncher.cs` into `Tags/LauncherAutoAssign.cs`
WITHOUT `[ExcludeFromCodeCoverage]`. These are pure delegate wiring
(`AddChoicesToDict`/`AddColorCategory`/`AutoFind`/`AutoFindAsync`/`FilterList`
pass-through) and are unit-testable. This adds testable lines and removes an
over-broad exemption. The remaining live-form launcher and globals/COM wiring stay in
`TagLauncher.cs` under a maintainer-ratified `[ExcludeFromCodeCoverage]`.

### Caller updates

- `Tags/TagLauncher.cs` constructs `new TagController(...)` passing a concrete
  `TagViewer`. After the constructor parameter changes `TagViewer` -> `ITagViewer`,
  these call sites compile unchanged via implicit upcast (`TagViewer` implements
  `ITagViewer`). If an `IUserPrompt` constructor parameter is added, an overload
  defaults it so `TagLauncher` need not pass it.
- `Tags/TagViewer.cs` `SetController(TagController)` — unchanged.
- `Tags/Helper Classes/CheckBoxController.cs` — unchanged; `_parent` stays
  `TagController`; the callback methods it uses stay public on `TagController`.
- `Tags.Test/TagControllerTests.cs` and
  `Tags.Test/TagControllerCoverageExpansionTests.cs` switch from a live `TagViewer` to
  a mocked/faked `ITagViewer`, inject a Moq `IUserPrompt` and a no-op `DrawFocus`.

## Non-Goals

- **No behavior or UX changes.** The tag dialog's observable behavior and appearance
  are preserved. This is a structural refactor only.
- **No WinForms migration.** No migration off WinForms/VSTO/Outlook Interop (the
  separate No-COM architecture effort is out of scope).
- **No new production dependencies** (per epic #295 NFR).
- **Pre-existing defects are report-only.** Defects surfaced by research —
  `RemoveControls` `_colColorbox.Remove(i)` index/element confusion (latent; the
  collection is empty in current flows), the orphaned uncompiled files
  `Tags/CheckBoxController.cs` and `Tags/AutoAssignInterface.cs`, and the
  `PrefixItem.PrefixType`/`OlUserFieldName` `NotImplementedException` members — are
  documented but NOT fixed as part of #293. Exception: if a file this refactor
  actually touches contains a banned API (for example `Task.Delay` in test code that
  is migrated), that banned API must be remediated per repository policy as part of
  the touch.

## Dependencies / Touchpoints

- **`UtilitiesCS.Interfaces.IWinForm.IForm`** — `ITagViewer` derives from it. `IForm`
  provides `Close()`, `KeyPreview`, `ShowDialog()`, `ShowDialog(IWin32Window)` but not
  `Text`, a `KeyDown` event, or `Controls`; the members the controller needs beyond
  `IForm` are declared explicitly on `ITagViewer`.
- **The compiled `Tags.IAutoAssign`** (`UtilitiesCS/Interfaces/IToDo/IAutoAssign.cs`,
  declared in `namespace Tags`) and `IPrefix` are already interfaces and are
  Moq-friendly; no change to them.
- **`Tags.Test`** — the test project consuming the new seams; migrated to mocked
  `ITagViewer` + injected `IUserPrompt` + no-op `DrawFocus`.
- **Epic siblings** — #296 (TaskTree), #297 (TaskVisualization core), #298
  (TaskVisualization secondary) follow the same Shared Design Pattern but touch
  disjoint projects.
- **Epic dependency: none.** This feature is **wave 0** with `depends_on: []` in the
  epic manifest. It touches only the `Tags` project and `Tags.Test`, so it runs in
  parallel with #296 and #297 without integration-branch conflicts.
- Required coordination: execution (worktrees, integration branch, PRs) begins only on
  maintainer signal via `epic-orchestrator`, per the epic manifest.

## Risks & Mitigations

- **WinForms/Outlook Interop COM boundary.** Dialogs (`MessageBox`, `InputBox`) and
  the focus-rectangle draw force UI/HWND creation. *Mitigation:* route dialogs through
  the `IUserPrompt` interface seam (tests never construct `InputBoxViewer` or show a
  popup) and route the focus draw through the injectable `DrawFocus` delegate (tests
  inject a no-op). No live `Form` is constructed in any test.
- **Public contract change on `TagController` constructor.** The parameter type
  changes `TagViewer` -> `ITagViewer` and an optional `IUserPrompt` parameter is added.
  *Mitigation:* `TagViewer` implements `ITagViewer` (implicit upcast keeps
  `TagLauncher` call sites compiling); the `IUserPrompt` parameter defaults via
  `??= new WinFormsUserPrompt()` and an overload so no production caller must change.
- **Existing tests use anti-deterministic patterns.** Current tests construct a live
  `TagViewer` (`new TagViewer()`, `[STAThread]`), walk a live control tree, and wait on
  `async void ButtonAutoAssign_Click` via `Task.Delay(50)` — a banned API and a
  nondeterministic wait. *Mitigation:* migrate these tests to a mocked/faked
  `ITagViewer` asserting model state or the fake's recorded option rows; extract
  `internal async Task ButtonAutoAssign_Action()` and await the `Task`-returning method
  directly (no delay). `[STAThread]` becomes unnecessary once no live control realizes
  an HWND. Behavioral assertions are preserved or strengthened, never weakened.
- **`PrefixItem` throws on two `IPrefix` members.** `PrefixType`/`OlUserFieldName`
  throw `NotImplementedException`. *Mitigation:* tests use the complete `IPrefix` fake
  (`TestPrefix`); production code paths (`ResolvePrefix`/`LoadSelections`) use only
  `.Key`/`.Value` and avoid the throwing members. The defect itself is report-only.
- **Coverage regression risk from over-broad exemptions.** *Mitigation:* extract
  testable logic (`LauncherAutoAssign`, `TagSelectionModel`, rendering arithmetic) out
  from under `[ExcludeFromCodeCoverage]`; keep exemptions narrowed to irreducible
  wiring and maintainer-ratified.

## Technical Specifications

- **Files expected to change:** `Tags/TagController.cs`, `Tags/TagViewer.cs`,
  `Tags/TagLauncher.cs`, `Tags/Tags.csproj` (add new `<Compile>` entries),
  `Tags.Test/TagControllerTests.cs`, `Tags.Test/TagControllerCoverageExpansionTests.cs`
  (and `Tags.Test/Tags.Test.csproj` if new test files are added).
- **Files expected to be created:** `Tags/ITagViewer.cs`, `Tags/IUserPrompt.cs`,
  `Tags/WinFormsUserPrompt.cs`, `Tags/TagSelectionModel.cs`,
  `Tags/TagController.Rendering.cs`, `Tags/LauncherAutoAssign.cs`, and new
  `Tags.Test` files for `TagSelectionModel` and `LauncherAutoAssign` coverage.
- **Public contracts affected (behavior unchanged):** `TagController` constructor
  signature (`ITagViewer` + optional `IUserPrompt`); `GetUserInputCategory` moves from
  `internal static` to an instance method; `ButtonAutoAssign_Action` is extracted as
  `internal async Task`; `ITagViewer` and `IUserPrompt` are new public/internal
  interfaces; `TagViewer` gains the `ITagViewer` implementation.
- **Data flow adjustments:** selection/search/filter/prefix state moves from the
  controller into `TagSelectionModel`; the controller orchestrates between the model,
  `ITagViewer`, and `IUserPrompt`. No change to persisted data formats.
- **Logging/telemetry:** none added or changed.
- **Migration/backfill:** none.
- **Coverage exemption policy.** `[ExcludeFromCodeCoverage]` is applied only to
  irreducible WinForms/COM wiring: the `DrawFocus` seam default body
  (`Graphics.FromHwnd`/`ControlPaint`), `WinFormsUserPrompt` adapter bodies, `TagViewer`
  intent-member bodies, the remaining `TagLauncher` live-form/globals members, and the
  `CheckBoxController` event-wiring members. Each exemption is individually justified
  and maintainer-ratified. Testable seams — `TagSelectionModel`, controller
  orchestration, rendering arithmetic against the panel abstraction, and
  `LauncherAutoAssign` — are NEVER exempt and must meet the coverage floor. If
  extracting the `CheckBoxController` decision logic is judged out of scope for #293,
  its exemption must be re-ratified explicitly rather than inherited silently.

## Test Strategy

- **Framework and seams.** MSTest + Moq + FluentAssertions. Mock `ITagViewer` via Moq
  (or a small in-memory fake with a backing `List<CheckBox>`); inject a Moq
  `IUserPrompt` and a no-op `DrawFocus` delegate; mock `IAutoAssign` and `MailItem`
  via Moq. No live `Form`/`Control` is constructed; no popup is shown; no `Thread.Sleep`
  or `Task.Delay`; no temporary files; no wall-clock or RNG.
- **Per-method test mapping** (from research section H): `ParseSearchStrings`,
  `Search`, `FilterArchive`, `IsPrefixMissing`, `SelectionAsList`/`AsString`/
  `GetSelections`, `ToggleChoice`/`AddOption`/`UpdateSelections`, `ResolvePrefix`/
  `GetDefaultPrefix`, `ResolveMailItem`, `LoadSelections`, `TryGetAutoAssignment`,
  `AddColorCategory`, `GetUserInputCategory`, `OptionsPanel_PreviewKeyDown`,
  `OptionsPanel_KeyDown`, `TagViewer_KeyDown`, `SearchText_KeyDown`/`KeyUp`,
  `Select_Ctrl_By_Offset`/`_By_Position`/`_First`/`_Last`/`_PageDown`/`_PageUp`,
  `LoadControls`/`RemoveControls`, `FilterToSelected`/`SearchAndReload`,
  `SetAutoAssignState`/`ButtonAutoAssignActive`/`ButtonNewActive`/`SetCaption`/
  `SetSearchText`, the extracted `ButtonAutoAssign_Action`, and `LauncherAutoAssign`.
- **Scenario completeness.** Each unit covers positive flows (valid input), negative
  flows (null/empty/missing input, user declines a prompt), and edge cases (boundary
  index math, out-of-range navigation raising `ArgumentOutOfRangeException`, empty
  search collapsing splits, unknown prefix key raising `ArgumentException`, empty
  auto-assign result skipping `FilterToSelected`). Dialog-driven methods are exercised
  through the mocked `IUserPrompt`; the async auto-assign path is awaited on the
  `Task`-returning method.
- **Migration of existing tests.** Reuse the reflection-based `PrefixItem`
  construction, the `InvokeOnClick` helper, `TestPrefix`, `NewAutoAssigner`,
  `NewMailItem`, private-field reflection helpers, and dictionary fixtures. Replace live
  `TagViewer` construction and control-tree walking with the fake `ITagViewer` and its
  `OptionControls`. Remove `[STAThread]` and the `Task.Delay` wait.
- **Toolchain commands (run in order, restart on any change/failure):**
  1. `csharpier .`
  2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe <Tags.Test assembly path> /EnableCodeCoverage`
- **Coverage targets.** `Tags` project `>= 80%` line coverage; new modules
  (`TagSelectionModel`, and the extracted `LauncherAutoAssign`) `>= 90%`. Changed lines
  must not regress coverage. Testable seams are not exempt.
- **Manual validation:** none required; the seams eliminate every live-form and dialog
  dependency from the test path (research `## Automation Feasibility`).

## Definition of Done

- [ ] Structure matches this spec; legacy paths retired or redirected
- [ ] Invariants validated with tests or comparisons
- [ ] Imports/tooling/entry points updated
- [ ] Edge cases and error handling verified
- [ ] Tests, linting, and type checks clean
- [ ] Docs updated (initiative/README/tasks as needed)
- [ ] Toolchain pass completed (format → lint → type-check → test)

### Acceptance Criteria (aligned with issue.md #293)

- [ ] `ITagViewer` interface exists, derives from `IForm`, and exposes the members
      `TagController` requires; `TagViewer` implements it.
- [ ] `TagController` depends on `ITagViewer`, not the concrete `TagViewer`.
- [ ] Host-neutral business logic is separated from COM/WinForms interaction.
- [ ] No resulting production file exceeds 500 lines.
- [ ] Unit tests cover the named methods and related logic without constructing real
      WinForms objects; seams are introduced where required.
- [ ] `TagController` (and extracted logic) reaches `>= 80%` line coverage.
- [ ] The `Tags` project as a whole reaches `>= 80%` line coverage (epic #295 goal;
      includes `TagLauncher` and `CheckBoxController` coverage as needed).
- [ ] No unit test constructs a live form/window or triggers a popup requiring human
      interaction.
- [ ] Full C# toolchain (csharpier → analyzers → nullable → MSTest) passes with no
      regression.

## Seeded Test Conditions (from potential)
- [ ] Business-logic units (search parse, filter archive, prefix missing detection,
- [ ] selection-as-list/string, toggle) covered with pure inputs.
- [ ] Dialog-driven methods (`TryGetAutoAssignment`, `AddColorCategory`,
- [ ] `GetUserInputCategory`) covered via seams that intercept `MessageBox`/`InputBox`.
- [ ] Keyboard event handlers (`OptionsPanel_PreviewKeyDown`, `OptionsPanel_KeyDown`,
- [ ] `TagViewer_KeyDown`, `SearchText_KeyDown/KeyUp`) covered with mocked `ITagViewer`.
- [ ] Auto-assign flow covered with a mocked `IAutoAssign`.
