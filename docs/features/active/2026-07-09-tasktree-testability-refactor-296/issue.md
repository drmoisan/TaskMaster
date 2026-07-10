# tasktree-testability-refactor (Issue #296)

- Date captured: 2026-07-09
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/tasktree-testability-refactor/ (Issue #296)
- Parent epic: winforms-testability-refactor (#295)

- Issue: #296
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/296
- Last Updated: 2026-07-09
- Work Mode: full-feature

## Problem / Why

`TaskTree/TaskTreeController.cs` is 546 lines, exceeding the repository 500-line
file limit, and is bound directly to the concrete `TaskTreeForm` WinForms type,
mixing host-neutral tree/business logic with WinForms/COM interaction. The
`TaskTree` project has **no test project at all** (`TaskTree.Test` does not
exist), so the project has 0% unit-test coverage.

## Proposed Behavior

- Create an `ITaskTreeForm` interface deriving from
  `UtilitiesCS.Interfaces.IWinForm.IForm` that abstracts the WinForms surface
  `TaskTreeController` consumes; make `TaskTreeForm` implement it; retarget
  `TaskTreeController` to depend on `ITaskTreeForm` rather than the concrete form.
- Extract host-neutral business logic from COM/WinForms interaction into separate
  file(s); minimize methods that mix COM calls with pure logic.
- Split `TaskTreeController.cs` along logical divisions so no resulting production
  file exceeds 500 lines.
- Introduce seams (interface > injectable delegate > adapter) for any dialog or
  UI-thread-bound calls so tests never construct live forms or show popups.
- Create a new `TaskTree.Test` MSTest project (MSTest + Moq + FluentAssertions)
  following the existing test-project pattern (e.g., `Tags.Test`), wire it into
  `TaskMaster.sln`, and add unit tests bringing the `TaskTree` project to >= 80%
  line coverage without instantiating real Windows Forms objects.

## Acceptance Criteria (early draft)

- [x] `ITaskTreeForm` exists, derives from `IForm`, and `TaskTreeForm` implements it.
- [x] `TaskTreeController` depends on `ITaskTreeForm`, not the concrete form.
- [x] Host-neutral logic separated from COM/WinForms interaction.
- [x] No production file in `TaskTree` exceeds 500 lines.
- [x] `TaskTree.Test` project exists, follows the repo MSTest pattern, and is in the solution.
- [x] No unit test constructs a live form/window or triggers a popup.
- [x] `TaskTree` project reaches >= 80% line coverage.
- [x] Full C# toolchain (csharpier → analyzers → nullable → MSTest) passes with no regression.

## Constraints & Risks

- Legacy non-SDK csproj (packages.config); the new test project must mirror an
  existing test project's format (references, packages, coverage wiring).
- Public contract change: controller constructor signature changes; in-repo
  callers must be updated.
- `[ExcludeFromCodeCoverage]` exemptions require maintainer ratification and must
  be minimized; testable seams are not exempt from the coverage floor.

## Test Conditions to Consider

- [x] Tree/business-logic units covered with pure inputs.
- [x] Dialog-driven or UI-bound paths covered via seams (no popups).
- [x] Event handler logic covered via a mocked `ITaskTreeForm`.

## Next Step

- [ ] Promote to GitHub issue (refactor template)
- [ ] Create active feature folder from the template
