# taskvisualization-core-testability-refactor (Issue #297)

- Date captured: 2026-07-09
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/taskvisualization-core-testability-refactor/ (Issue #297)
- Parent epic: winforms-testability-refactor (#295)

- Issue: #297
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/297
- Last Updated: 2026-07-09
- Work Mode: full-feature

## Problem / Why

`TaskVisualization/TaskController.cs` is 1861 lines — more than 3.7x the
repository 500-line file limit — and is bound directly to the concrete
`TaskViewer` WinForms type (262 lines + 1422-line designer), mixing host-neutral
business logic with WinForms/COM (Outlook Interop) interaction. Its logic cannot
be unit-tested without instantiating live forms, which violates the unit-test
policy, so the core of the `TaskVisualization` project is effectively uncovered.

## Proposed Behavior

- Create an `ITaskViewer` interface deriving from
  `UtilitiesCS.Interfaces.IWinForm.IForm` that abstracts the WinForms surface
  `TaskController` consumes; make `TaskViewer` implement it; retarget
  `TaskController` to depend on `ITaskViewer` rather than the concrete form.
- Decompose `TaskController.cs` (1861 lines) along logical divisions into
  multiple files/classes, each <= 500 lines.
- Extract host-neutral business logic (filtering, sorting, state transitions,
  data shaping) into separate host-neutral files, minimizing methods that mix COM
  calls with pure logic.
- Introduce seams (interface > injectable delegate > adapter) for dialogs and
  UI-thread-bound COM calls so tests never construct live forms or show popups.
  COM elements may run on the UI thread in production only when no seam
  alternative exists — never in unit tests.
- Add MSTest + Moq + FluentAssertions unit tests in `TaskVisualization.Test`
  covering the refactored controller and extracted logic, targeting >= 80% line
  coverage for the refactored core (contributing to the project-wide 80% goal
  completed by the follow-up secondary feature).

## Acceptance Criteria (early draft)

- [ ] `ITaskViewer` exists, derives from `IForm`, and `TaskViewer` implements it.
- [ ] `TaskController` depends on `ITaskViewer`, not the concrete form.
- [ ] `TaskController.cs` decomposed; no production file in scope exceeds 500 lines.
- [ ] Host-neutral logic separated from COM/WinForms interaction.
- [ ] No unit test constructs a live form/window or triggers a popup.
- [ ] Refactored core (controller + extracted logic files) reaches >= 80% line coverage.
- [ ] Full C# toolchain (csharpier → analyzers → nullable → MSTest) passes with no regression.

## Constraints & Risks

- Largest decomposition in the epic (1861 lines): highest regression risk; the
  refactor must preserve behavior and existing tests as spec.
- Public contract change: controller constructor signature changes; in-repo
  callers must be updated.
- Legacy non-SDK csproj (packages.config); new files must be added to the csproj
  manually.
- A follow-up feature (taskvisualization-secondary-testability) modifies the same
  csproj/test project and must be serialized after this one via epic dependency.
- `[ExcludeFromCodeCoverage]` exemptions require maintainer ratification and must
  be minimized; testable seams are not exempt from the coverage floor.

## Test Conditions to Consider

- [ ] Business-logic units (filtering, sorting, state shaping) covered with pure inputs.
- [ ] Dialog-driven paths covered via seams intercepting `MessageBox`/input dialogs.
- [ ] Event handler logic covered via a mocked `ITaskViewer`.
- [ ] Outlook Interop boundaries mocked behind seams.

## Next Step

- [ ] Promote to GitHub issue (refactor template)
- [ ] Create active feature folder from the template
