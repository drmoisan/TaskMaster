# winforms-testability-refactor (Issue #295)

- Date captured: 2026-07-09
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/winforms-testability-refactor/ (Issue #295)

- Issue: #295
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/295
- Last Updated: 2026-07-09
## Problem / Why

Three WinForms/Outlook-Interop UI projects — `Tags`, `TaskTree`, and
`TaskVisualization` — contain controller classes that are bound directly to
concrete `Form`-derived viewer types, mix host-neutral business logic with
COM/WinForms interaction, and in several cases exceed the repository 500-line
file limit (`Tags/TagController.cs` 877 lines, `TaskTree/TaskTreeController.cs`
546 lines, `TaskVisualization/TaskController.cs` 1861 lines). As a result these
projects have little or no unit-test coverage: their logic cannot be exercised
without instantiating live forms, which violates the unit-test policy
(no live UI, no popups requiring human interaction, deterministic tests only).
`TaskTree` has no test project at all.

## Proposed Behavior

Epic covering per-project testability refactors, all following one shared pattern:

1. For each viewer/form a controller consumes, create a viewer interface derived
   from `UtilitiesCS.Interfaces.IWinForm.IForm` (pattern: `ITagViewer` for
   `TagViewer`); make the concrete form implement it; retarget the controller to
   the interface.
2. Refactor each production file to <= 500 lines, splitting along logical
   divisions.
3. Separate host-neutral business logic from COM interaction into distinct files;
   minimize methods that mix COM calls with pure logic.
4. Introduce seams (interface > injectable delegate > adapter, per repo DI-seam
   preference) so tests never construct live forms/windows and never show popups.
   COM elements may run on the UI thread only when no seam alternative exists,
   and never in unit tests.
5. Add MSTest + Moq + FluentAssertions unit tests bringing each project to
   >= 80% line coverage (creating `TaskTree.Test` where missing).

## Child Features (planned decomposition)

- Tags project testability refactor (`TagController`/`TagViewer` -> `ITagViewer`) — issue #293, already promoted.
- TaskTree project testability refactor (`TaskTreeController`/`TaskTreeForm` -> `ITaskTreeForm`, new `TaskTree.Test`).
- TaskVisualization core testability refactor (`TaskController`/`TaskViewer` -> `ITaskViewer`, 1861-line decomposition).
- TaskVisualization secondary viewers and helpers (`EditFilterController`/`EditFilterViewer` -> `IEditFilterViewer`, `ManageFilters` -> `IManageFiltersViewer`, Flag*/AutoCreate/AutoAssign helper coverage) — depends on the core refactor (shared csproj/test project).

## Acceptance Criteria (early draft)

- [ ] Every controller in scope depends on a viewer interface derived from `IForm`, not a concrete form.
- [ ] No production file in the three projects exceeds 500 lines.
- [ ] Host-neutral logic separated from COM/WinForms interaction.
- [ ] No unit test constructs a live form/window or triggers a popup.
- [ ] `Tags`, `TaskTree`, and `TaskVisualization` each reach >= 80% line coverage.
- [ ] Full C# toolchain passes for every child feature with no regression.

## Constraints & Risks

- Legacy non-SDK csproj (packages.config) projects; test-project creation for TaskTree must follow the existing MSTest project pattern.
- Public contract changes (controller constructors) require in-repo caller updates.
- `[ExcludeFromCodeCoverage]` exemptions require maintainer ratification and must be minimized; testable seams are not exempt.
- Two children touch `TaskVisualization` — they must be serialized via epic dependency to avoid parallel csproj merge conflicts.

## Test Conditions to Consider

- [ ] Dialog-driven paths seamed (`MessageBox`, `InputBox`) and covered without UI.
- [ ] Keyboard/event handler logic covered via mocked viewer interfaces.
- [ ] Business-logic units covered with pure inputs.
- [ ] Coverage measured per project via vstest /EnableCodeCoverage.

## Next Step

- [ ] Promote to GitHub issue (epic template)
- [ ] Create epic home `docs/features/epics/winforms-testability-refactor/epic.md`
- [ ] Promote child features and run research/spec/plan/preflight per child
