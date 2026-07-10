# Remediation Inputs — taskvisualization-secondary-testability (#298)

- Timestamp: 2026-07-10T07-40
- Branch: `feature/taskvisualization-secondary-testability-298` @ `f2d2d476b507ef4fb713d54d7c39575989f7f433`
- Source artifacts:
  - `policy-audit.2026-07-10T07-40.md`
  - `code-review.2026-07-10T07-40.md`
  - `feature-audit.2026-07-10T07-40.md`
- Overall verdict: NOT READY TO MERGE

## Blocking Findings (must fix before merge)

### B1 — Testable interface-seam method exempted from coverage

- File:line: `TaskVisualization/AutoAssignPeople.cs:108-117` (`AddChoicesToDict`)
- Violated rule: CLAUDE.md ratified COM/VSTO/WinForms exemption ("testable seams within otherwise-COM-bound assemblies ... are explicitly NOT exempt"); `.claude/rules/general-unit-test.md` coverage-exclusion intent; the feature's own `spec.md` (lines 261, 297) commitment.
- Why it is a violation: the entire body is `return _globals.TD.People.AddMissingEntries(olMail);`. `_globals.TD.People` is the interface `IPeopleScoDictionaryNew` (`UtilitiesCS/Interfaces/IGlobals/IToDoObjects.cs:13`), `AddMissingEntries(MailItem)` is an interface member (`UtilitiesCS/Interfaces/IToDo/IPeopleScoDictionaryNew.cs:18`), and `olMail` is passed through untouched. The method is unit-testable with a `Mock<IPeopleScoDictionaryNew>` (already mocked in `TaskMaster.Test/AppGlobals/AppToDoObjectsCoverageTests.cs:41`) and a null/mock `MailItem`.
- Required fix: remove `[ExcludeFromCodeCoverage]` from `AddChoicesToDict`; add an MSTest+Moq test that stubs `IPeopleScoDictionaryNew.AddMissingEntries` to return a canned list and asserts `AddChoicesToDict` returns it (pass-through). Re-run the C# toolchain.

### B2 — Whole COM-call method exempted where an in-feature seam already exists

- File:line: `TaskVisualization/AutoAssignPeople.cs:121-129` (`AddColorCategory`)
- Violated rule: seams-first ordering (interface > injectable delegate > adapter) in `.claude/rules/csharp.md`; in-feature consistency; the "testable seams are never exempt" commitment.
- Why it is a violation: the method is entirely `[ExcludeFromCodeCoverage]` and calls the static `CreateCategoryModule.CreateCategory(...)` inline. The identical MAPI call is seamed via `Func<IPrefix,string,Category> _createCategory` and measured in `AutoCreateProject` (`AutoCreateProject.cs:57-94`) within the same feature, with only the one-line `DefaultCreateCategory` default exempt.
- Required fix: add a `Func<IPrefix,string,Category> _createCategory` seam to `AutoAssignPeople` (optional constructor parameter defaulting to a `DefaultCreateCategory` that wraps the static call); call the seam from a measured `AddColorCategory`; exempt only `DefaultCreateCategory`. Add a test injecting a stub `_createCategory` and asserting `AddColorCategory` returns it. Re-run the C# toolchain.

## Major Findings (fix before merge; not independently merge-blocking but required for a clean simplicity-first refactor)

### M1 — Dead static method retained and newly exempted

- File:line: `TaskVisualization/EditFilterController.cs:114-132` (`DeleteFilterDialog`, static)
- Violated rule: simplicity-first / no dead code (`.claude/rules/general-code-change.md` design principles).
- Why it is a violation: the public static method has zero production callers (grep-confirmed across the worktree). It is pre-existing dead code carried through a testability/simplicity refactor and now carries a beyond-plan `[ExcludeFromCodeCoverage]` added to protect the coverage percentage.
- Required fix: remove `DeleteFilterDialog` and its exemption. If a delete-confirmation dialog is genuinely required, wire it to a real caller behind a viewer-factory seam; otherwise delete.

### M2 — Empty no-op method plus coverage-only test

- File:line: `TaskVisualization/EditFilterController.cs:207` (`SetUpDeleteDialog() { }`) and `TaskVisualization.Test/EditFilterControllerTests.cs:95`
- Violated rule: simplicity-first / no dead code; `.claude/rules/general-unit-test.md` UT3 (a test must exercise and assert real behavior).
- Why it is a violation: `SetUpDeleteDialog` is an empty method whose only caller is a test invoking it with the comment "no-op hook; kept covered" and asserting nothing. It exists solely to register a covered line.
- Required fix: remove `SetUpDeleteDialog` and the test line at `EditFilterControllerTests.cs:95`.

## Adjudicated / Not a Finding

- `ManageFiltersController.DefaultEditFilterFactory` (`ManageFiltersController.cs:56-65`) — ratified irreducible live-form default seam; both branches asserted through the injected seam in `AddFilter`/`EditSelected` tests. No action.

## Exit Criteria for This Remediation Cycle

- B1, B2 exemptions removed; the affected `AutoAssignPeople` members measured with new MSTest+Moq tests.
- M1, M2 dead code removed.
- Full C# toolchain re-run green (csharpier -> analyzers -> nullable -> vstest), project coverage re-measured and still >= 80% testable-denominator.
- Updated `exemption-inventory.md` reflecting the removed exemptions.
