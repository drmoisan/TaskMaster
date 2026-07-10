# Code Review — taskvisualization-secondary-testability (#298)

- Timestamp: 2026-07-10T07-40
- Branch: `feature/taskvisualization-secondary-testability-298` @ `f2d2d476b507ef4fb713d54d7c39575989f7f433`
- Diff base: `epic/winforms-testability-refactor-integration` @ `949dddd2df0df4511fcc0ff44c4d77c38821c54c`
- Scope: C# testability refactor of the `TaskVisualization` project's secondary viewers and helper classes.

## Executive Summary

The refactor is well-structured and largely follows the repository's seams-first
design intent. Pure flag math is cleanly extracted into `FlagCalculations`
(100% covered); `ManageFiltersController` and the retargeted `EditFilterController`
depend on the new `IManageFiltersViewer` / `IEditFilterViewer` interfaces so
controller logic is exercised against Moq mocks with no live form. The
`AutoCreateProject` seam design (`_chooseProgram`, `_createCategory`,
`_getTaskItems` with safe defaults) is a good example of interface/delegate seams
isolating COM at one-line defaults while keeping orchestration measured.

Two Blocking issues arise from inconsistent application of that same seam
discipline in `AutoAssignPeople`, and two Major issues concern dead code in
`EditFilterController`. Details below. The DI-seam pattern, naming, nullable
handling, and file cohesion are otherwise consistent with repo style.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Blocking | TaskVisualization/AutoAssignPeople.cs | 108-117 (`AddChoicesToDict`) | `[ExcludeFromCodeCoverage]` on a method whose entire body is `return _globals.TD.People.AddMissingEntries(olMail);`. `_globals.TD.People` is the interface `IPeopleScoDictionaryNew` (`IToDoObjects.People { get; }`), `AddMissingEntries(MailItem)` is an interface member, and the `olMail` parameter is passed through untouched (never dereferenced). The method is testable with a `Mock<IPeopleScoDictionaryNew>` and a null/mock `MailItem`. | Remove the exemption; add a Moq test stubbing `AddMissingEntries` and asserting the pass-through return. | CLAUDE.md ratified exemption: "Testable seams within otherwise-COM-bound assemblies ... are explicitly NOT exempt." The spec (line 261/297) and `exemption-inventory.md` restate "testable seams are never exempt". The exemption-inventory misattributes the COM dependency: recipient reading happens inside `PeopleScoDictionaryNew.AddMissingEntries` (a separate, separately-exempted class), not in this method. | `AutoAssignPeople.cs:108-117`; interface at `UtilitiesCS/Interfaces/IToDo/IPeopleScoDictionaryNew.cs:18`; `IToDoObjects.cs:13`; existing `Mock<IPeopleScoDictionaryNew>` at `TaskMaster.Test/AppGlobals/AppToDoObjectsCoverageTests.cs:41` |
| Blocking | TaskVisualization/AutoAssignPeople.cs | 121-129 (`AddColorCategory`) | The whole method is `[ExcludeFromCodeCoverage]` and calls the static `CreateCategoryModule.CreateCategory(...)` inline with no seam. The identical MAPI call is seamed via `Func<IPrefix,string,Category> _createCategory` and measured in `AutoCreateProject` within the same feature (only the `DefaultCreateCategory` default is exempt). | Introduce a `Func<IPrefix,string,Category> _createCategory` seam mirroring `AutoCreateProject`; call it from a measured `AddColorCategory`; exempt only a one-line `DefaultCreateCategory` default. | Seams-first ordering (interface > injectable delegate > adapter) in `.claude/rules/csharp.md`; in-feature consistency. The feature proves the seam is feasible for this exact static call, so leaving the sibling call inline-exempt is an unjustified exclusion of otherwise-measurable logic. | `AutoAssignPeople.cs:121-129` vs `AutoCreateProject.cs:57-94` (`AddColorCategory` measured, `_createCategory` seam, `DefaultCreateCategory` exempt); spec seam catalog `spec.md:140-142` |
| Major | TaskVisualization/EditFilterController.cs | 114-132 (`DeleteFilterDialog`, static) | Public static method with **zero production callers** (grep-confirmed across the worktree; only self-definition, research/evidence docs, and historical coverage XMLs reference it). Pre-existing dead code retained through a testability/simplicity-first refactor, now carrying a beyond-plan `[ExcludeFromCodeCoverage]` added to preserve the coverage percentage. | Remove `DeleteFilterDialog` and its exemption. If a delete-confirmation dialog is intended, wire it to a real caller behind a seam; otherwise delete. | Simplicity-first / no dead code (general-code-change design principles). Exempting unreachable code to protect a metric is not a valid use of the exemption policy. | `EditFilterController.cs:114-132`; grep for `DeleteFilterDialog` shows no production `.cs` caller |
| Major | TaskVisualization/EditFilterController.cs + TaskVisualization.Test/EditFilterControllerTests.cs | `EditFilterController.cs:207` (`SetUpDeleteDialog() { }`) and `EditFilterControllerTests.cs:95` | Empty no-op method whose only caller is a test that invokes it with the comment "no-op hook; kept covered" and asserts nothing about behavior. The method exists solely so the invocation registers a covered line. | Remove `SetUpDeleteDialog` and the test line that calls it. | Dead code (simplicity-first) plus UT3 (a test must exercise and assert real behavior; invoking an empty method to register coverage is not a meaningful test and games the coverage metric). | `EditFilterController.cs:207`; `EditFilterControllerTests.cs:95` |
| Info | TaskVisualization/ManageFiltersController.cs | 56-65 (`DefaultEditFilterFactory`) | `[ExcludeFromCodeCoverage]` on the irreducible live-form default of the injected `_editFilterFactory` seam; the null-vs-non-null branch is asserted through the injected seam in `AddFilter`/`EditSelected` tests. | No action — reviewed and accepted. | Adjudicated as a permitted irreducible live-form default seam; not a finding. | `ManageFiltersController.cs:56-86`; `ManageFiltersControllerTests.cs` |

## Design-Principle Observations (non-blocking)

- **Seam design (positive).** `AutoCreateProject`'s optional-parameter seams with
  live-host defaults keep the single-arg constructor valid for existing callers
  while making host-neutral branches measurable. This is the pattern the two
  Blocking findings ask `AutoAssignPeople` to match.
- **Interface extraction (positive).** `IEditFilterViewer` / `IManageFiltersViewer`
  expose behavioral members (string text surfaces + `...Click` events) rather than
  raw `Control` types, so a Moq mock satisfies the interface with no `Control`
  instantiation. Consistent with the epic's Shared Design Pattern.
- **Invariant preservation (verified).** `ManageFilters`'s three-call surface
  (`new ManageFilters` / `LoadFilters` / `Show`) and the `FlagTasks` constructor
  shape are preserved; `EfcFormController` is not edited. Consistent with
  `spec.md` Invariants.
- **Error handling.** `AutoAssignPeople.AutoFindAsync` uses a `try { ... } catch
  (Exception) { throw; }` that adds no context; it is behavior-preserving (rethrow)
  but the empty catch is redundant. Minor; not a blocker.

## Verdict

Code quality is acceptable apart from the two Blocking seam/exemption
inconsistencies and the two Major dead-code items. Recommend remediation of B1,
B2, M1, M2 before merge.
