# tagcontroller-testability-refactor (Issue #293)

- Date captured: 2026-07-09
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/tagcontroller-testability-refactor/ (Issue #293)

- Issue: #293
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/293
- Last Updated: 2026-07-09
- Work Mode: full-feature
- Parent Epic: winforms-testability-refactor (#295) — `docs/features/epics/winforms-testability-refactor/epic.md`
- AC Source Exception: user-story.md waived per epic #295; AC = spec.md + issue.md

## Problem / Why

`Tags/TagController.cs` is 878 lines, exceeding the repository 500-line file-size
limit, and mixes host-neutral business logic (dictionary filtering, search parsing,
prefix handling, selection state) with direct Windows Forms / Outlook Interop COM
interaction. Many internal and public methods have no unit-test coverage because
the class is bound directly to the concrete `TagViewer` WinForms type, which cannot
be instantiated or exercised in a unit test without a live UI/COM environment.

## Proposed Behavior

- Introduce an `ITagViewer` interface (deriving from
  `UtilitiesCS.Interfaces.IWinForm.IForm`) that abstracts the WinForms surface
  `TagController` consumes (buttons, panels, search text, checkboxes, caption, close).
- Make `TagViewer` implement `ITagViewer`.
- Change `TagController` to depend on `ITagViewer` rather than the concrete
  `TagViewer`.
- Extract host-neutral business logic into one or more separate files, minimizing
  the mixing of COM calls with pure logic.
- Split `TagController.cs` along logical boundaries so no resulting production file
  exceeds 500 lines.
- Add MSTest + Moq + FluentAssertions unit tests covering the previously untested
  methods (`TryGetAutoAssignment`, `AddColorCategory`, `GetUserInputCategory`,
  `OptionsPanel_PreviewKeyDown`, `OptionsPanel_KeyDown`, and related methods),
  without instantiating real Windows Forms objects, using seams where necessary.

## Acceptance Criteria (early draft)

- [ ] `ITagViewer` interface exists, derives from `IForm`, and exposes the members
      `TagController` requires; `TagViewer` implements it.
- [ ] `TagController` depends on `ITagViewer`, not the concrete `TagViewer`.
- [ ] Host-neutral business logic is separated from COM/WinForms interaction.
- [ ] No resulting production file exceeds 500 lines.
- [ ] Unit tests cover the named methods and related logic without constructing real
      WinForms objects; seams are introduced where required.
- [ ] `TagController` (and extracted logic) reaches >= 80% line coverage.
- [ ] The `Tags` project as a whole reaches >= 80% line coverage (epic #295 goal;
      includes `TagLauncher` and `CheckBoxController` coverage as needed).
- [ ] No unit test constructs a live form/window or triggers a popup requiring
      human interaction.
- [ ] Full C# toolchain (csharpier → analyzers → nullable → MSTest) passes with no
      regression.

## Constraints & Risks

- WinForms/Outlook Interop COM boundary: dialogs (`MessageBox`, `InputBox`) and
  static UI calls must be seamed to avoid launching UI in tests (per repository
  determinism rules and the "tests must not trigger UX or a live worker" guidance).
- Public contract change: `TagController` constructor signature changes; callers
  (`TagLauncher`, `TagViewer.SetController`) must be updated in-repo.
- `[ExcludeFromCodeCoverage]` for irreducible WinForms wiring must be justified and
  minimized; testable seams are not exempt from the coverage floor.

## Test Conditions to Consider

- [ ] Business-logic units (search parse, filter archive, prefix missing detection,
      selection-as-list/string, toggle) covered with pure inputs.
- [ ] Dialog-driven methods (`TryGetAutoAssignment`, `AddColorCategory`,
      `GetUserInputCategory`) covered via seams that intercept `MessageBox`/`InputBox`.
- [ ] Keyboard event handlers (`OptionsPanel_PreviewKeyDown`, `OptionsPanel_KeyDown`,
      `TagViewer_KeyDown`, `SearchText_KeyDown/KeyUp`) covered with mocked `ITagViewer`.
- [ ] Auto-assign flow covered with a mocked `IAutoAssign`.

## Next Step

- [ ] Promote to GitHub issue (refactor template)
- [ ] Create active feature folder from the template
