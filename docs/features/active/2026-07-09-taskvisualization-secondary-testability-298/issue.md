# taskvisualization-secondary-testability (Issue #298)

- Date captured: 2026-07-09
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/taskvisualization-secondary-testability/ (Issue #298)
- Parent epic: winforms-testability-refactor (#295)
- Depends on: taskvisualization-core-testability-refactor (same csproj/test project)

- Issue: #298
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/298
- Last Updated: 2026-07-09
- Work Mode: full-feature

## Problem / Why

Beyond `TaskController`, the `TaskVisualization` project contains secondary
viewers and helper classes with little or no unit-test coverage:
`EditFilterController.cs` (231 lines) bound to the concrete `EditFilterViewer`
form, `ManageFilters.cs` (57 lines + 164-line designer) which is itself a
`Form`-derived class, and helper/business classes (`FlagTasks.cs` 242,
`AutoCreateProject.cs` 211, `FlagChangeGroup.cs` 157, `AutoAssignContext.cs` 96,
`AutoAssignPeople.cs` 95, `FlagChangeTrainingQueue.cs` 78, `FlagChangeItem.cs`
23) that mix business logic with WinForms/Outlook-Interop interaction. Without
covering these, the `TaskVisualization` project cannot reach the epic's >= 80%
project-wide coverage goal.

## Proposed Behavior

- Create an `IEditFilterViewer` interface deriving from
  `UtilitiesCS.Interfaces.IWinForm.IForm`; make `EditFilterViewer` implement it;
  retarget `EditFilterController` to depend on `IEditFilterViewer`.
- Create an `IManageFiltersViewer` interface deriving from `IForm`; make
  `ManageFilters` implement it; extract its logic from the form class so the
  logic is controller/helper-hosted and testable against the interface.
- Extract host-neutral business logic in the Flag*/AutoCreate/AutoAssign helper
  classes away from COM interaction; introduce seams (interface > injectable
  delegate > adapter) for Outlook Interop and dialog calls so tests never
  construct live forms or show popups.
- Keep all touched production files <= 500 lines.
- Add MSTest + Moq + FluentAssertions unit tests in `TaskVisualization.Test`
  covering the secondary viewers' controllers and the helper classes, bringing
  the `TaskVisualization` project as a whole to >= 80% line coverage.

## Acceptance Criteria (early draft)

- [ ] `IEditFilterViewer` and `IManageFiltersViewer` exist, derive from `IForm`, and their concrete forms implement them.
- [ ] `EditFilterController` depends on `IEditFilterViewer`; `ManageFilters` logic is testable against `IManageFiltersViewer`.
- [ ] Helper classes' host-neutral logic separated from COM interaction with seams at Interop boundaries.
- [ ] No touched production file exceeds 500 lines.
- [ ] No unit test constructs a live form/window or triggers a popup.
- [ ] `TaskVisualization` project reaches >= 80% line coverage overall.
- [ ] Full C# toolchain (csharpier → analyzers → nullable → MSTest) passes with no regression.

## Constraints & Risks

- Must run AFTER taskvisualization-core-testability-refactor merges (same
  csproj and `TaskVisualization.Test` project; parallel edits would conflict).
- Helper classes interact with Outlook Interop types (`MailItem`, folders);
  seams must isolate these per the DI-seam preference order.
- `[ExcludeFromCodeCoverage]` exemptions require maintainer ratification and must
  be minimized; Designer-generated code follows existing repo exemption policy.

## Test Conditions to Consider

- [ ] EditFilter dialog logic covered via mocked `IEditFilterViewer`.
- [ ] ManageFilters list-management logic covered via mocked `IManageFiltersViewer`.
- [ ] Flag change grouping/queueing logic covered with pure inputs.
- [ ] AutoCreateProject / AutoAssign* logic covered with mocked Interop seams.

## Next Step

- [ ] Promote to GitHub issue (refactor template)
- [ ] Create active feature folder from the template
