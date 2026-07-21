# Code Review — taskvisualization-secondary-testability (#298)

- Timestamp: 2026-07-10T08-09
- Reviewer: feature-reviewer (remediation-cycle-1 REAUDIT)
- Branch: `feature/taskvisualization-secondary-testability-298` @ `b49dbebca7aece65c3a9dd75636835f4edc049a7`
- Diff base: `epic/winforms-testability-refactor-integration` @ `949dddd2df0df4511fcc0ff44c4d77c38821c54c`
- Scope: C# testability refactor of the `TaskVisualization` project's secondary viewers and helper classes.

## Executive Summary

The refactor is well-structured and follows the repository's seams-first design
intent. Pure flag math is cleanly extracted into `FlagCalculations` (100%
covered); `ManageFiltersController` and the retargeted `EditFilterController`
depend on the new `IManageFiltersViewer` / `IEditFilterViewer` interfaces so
controller logic is exercised against Moq mocks with no live form. The
`AutoCreateProject` seam design (`_chooseProgram`, `_createCategory`,
`_getTaskItems` with safe defaults) isolates COM at one-line defaults while
keeping orchestration measured.

Remediation cycle 1 resolved all four prior findings, confirmed against the code
at HEAD `b49dbebc`:

- **B1 resolved.** `AutoAssignPeople.AddChoicesToDict` (`AutoAssignPeople.cs:114-122`)
  no longer carries `[ExcludeFromCodeCoverage]` and is covered by a Moq
  `IPeopleScoDictionaryNew` pass-through test.
- **B2 resolved.** `AutoAssignPeople` now applies the same
  `Func<IPrefix,string,Category> _createCategory` seam as `AutoCreateProject`;
  `AddColorCategory` (`AutoAssignPeople.cs:124-127`) delegates to it and is
  measured, with only the one-line `DefaultCreateCategory` default exempt.
- **M1 resolved.** The dead static `DeleteFilterDialog`, its orphaned private
  constructor, and the unused `using System.Windows.Forms` are deleted.
- **M2 resolved.** The empty `SetUpDeleteDialog()` and its sole test caller are
  deleted.

The DI-seam pattern is now applied consistently across `AutoAssignPeople` and
`AutoCreateProject`. Naming, nullable handling, and file cohesion are consistent
with repo style. No Blocking or Major findings remain.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Resolved (was Blocking B1) | TaskVisualization/AutoAssignPeople.cs | 114-122 (`AddChoicesToDict`) | Exemption removed; the method (`return _globals.TD.People.AddMissingEntries(olMail);`) is now measured via a `Mock<IPeopleScoDictionaryNew>` pass-through test asserting the returned list and `AddMissingEntries(mail)` called once. | No action — verified resolved. | CLAUDE.md "testable seams are never exempt" now satisfied; per-method line-rate 1.0. | `AutoAssignPeople.cs:114-122`; test `AutoAssignPeopleTests.cs:104-121` |
| Resolved (was Blocking B2) | TaskVisualization/AutoAssignPeople.cs | 124-127 (`AddColorCategory`), 29/31-40/131-139 (seam) | `Func<IPrefix,string,Category> _createCategory` seam added with optional ctor param defaulting to the exempt one-line `DefaultCreateCategory`; `AddColorCategory` delegates to the seam and is measured. Mirrors `AutoCreateProject`. | No action — verified resolved. | Seams-first ordering (`.claude/rules/csharp.md`) and in-feature consistency now satisfied; per-method line-rate 1.0; only the live MAPI default is exempt. | `AutoAssignPeople.cs:29,31-40,124-127,131-139`; test `AutoAssignPeopleTests.cs:123-151` |
| Resolved (was Major M1) | TaskVisualization/EditFilterController.cs | (formerly 114-132, `DeleteFilterDialog`) | Dead static method, its orphaned private single-arg constructor, and the unused `using System.Windows.Forms` deleted. No `DeleteFilterDialog` reference remains in any `.cs` file. Analyzer build reports no IDE0005/IDE0051/CS0246. | No action — verified resolved. | Simplicity-first / no dead code now satisfied. | grep for `DeleteFilterDialog` across the worktree returns only docs/evidence/archive, no `.cs` |
| Resolved (was Major M2) | TaskVisualization/EditFilterController.cs + TaskVisualization.Test/EditFilterControllerTests.cs | (formerly `SetUpDeleteDialog`) | Empty no-op method and its sole coverage-only test caller deleted. No `SetUpDeleteDialog` reference remains in any `.cs` file. | No action — verified resolved. | Dead code + UT3 (tests must assert real behavior) now satisfied. | grep for `SetUpDeleteDialog` returns only docs/evidence/archive, no `.cs`; `EditFilterControllerTests.cs` has no such line |
| Info | TaskVisualization/ManageFiltersController.cs | `DefaultEditFilterFactory` | `[ExcludeFromCodeCoverage]` on the irreducible live-form default of the injected `_editFilterFactory` seam; the null-vs-non-null branch is asserted through the injected seam in `AddFilter`/`EditSelected` tests. | No action — ratified per caller adjudication. | Permitted irreducible live-form default seam; not a finding. | `ManageFiltersController.cs`; `ManageFiltersControllerTests.cs`; `exemption-inventory.md` |

## Design-Principle Observations (non-blocking)

- **Seam consistency (now positive).** `AutoAssignPeople` now matches
  `AutoCreateProject`'s optional-parameter seam pattern for the MAPI
  `CreateCategory` call, so the host-neutral forwarding is measured and only the
  one-line live default is exempt. The prior in-feature inconsistency is gone.
- **Interface extraction (positive).** `IEditFilterViewer` / `IManageFiltersViewer`
  expose behavioral members (string text surfaces + `...Click` events) rather than
  raw `Control` types, so a Moq mock satisfies the interface with no `Control`
  instantiation. Consistent with the epic's Shared Design Pattern.
- **Invariant preservation (verified).** `ManageFilters`'s three-call surface
  (`new ManageFilters` / `LoadFilters` / `Show`) and the `FlagTasks` constructor
  shape are preserved; `EfcFormController` is not edited. Consistent with
  `spec.md` Invariants.
- **Error handling (minor, non-blocking).** `AutoAssignPeople.AutoFindAsync`
  (`AutoAssignPeople.cs:47-57`) uses a `try { ... } catch (Exception) { throw; }`
  that adds no context; it is behavior-preserving (rethrow) but the empty catch is
  redundant. Not a blocker and out of the remediation scope; noted for a future
  cleanup pass.

## Verdict

Code quality is acceptable. All four prior findings (B1, B2, M1, M2) are
confirmed resolved. Zero Blocking and zero Major findings remain. Recommend merge.
