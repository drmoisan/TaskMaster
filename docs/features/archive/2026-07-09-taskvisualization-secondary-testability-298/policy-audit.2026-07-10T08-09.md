# Policy Audit — taskvisualization-secondary-testability (#298)

- Timestamp: 2026-07-10T08-09
- Reviewer: feature-reviewer (authoritative remediation-cycle-1 REAUDIT / exit audit)
- Branch under review: `feature/taskvisualization-secondary-testability-298` @ `b49dbebca7aece65c3a9dd75636835f4edc049a7`
- Diff base: `epic/winforms-testability-refactor-integration` @ `949dddd2df0df4511fcc0ff44c4d77c38821c54c` (merge-base = integration head; clean linear descendant)
- Diff command: `git -C C:/Users/DanMoisan/repos/TaskMaster-wt/winforms-298 diff 949dddd2df0df4511fcc0ff44c4d77c38821c54c...HEAD`
- Work mode: `full-feature` (AC sources = `spec.md` Definition of Done / alignment + `issue.md` `## Acceptance Criteria`; `user-story.md` intentionally absent per `spec.md` "User Story Applicability")
- Prior cycle artifacts: `policy-audit.2026-07-10T07-40.md`, `code-review.2026-07-10T07-40.md`, `feature-audit.2026-07-10T07-40.md`, `remediation-inputs.2026-07-10T07-40.md` (verdict NOT READY TO MERGE, findings B1, B2, M1, M2)

## Executive Summary

Overall policy verdict: **READY TO MERGE**.

This is the authoritative exit audit for remediation cycle 1. All four prior
findings are confirmed resolved against the code at HEAD `b49dbebc`:

- **B1 (resolved):** `AutoAssignPeople.AddChoicesToDict` no longer carries
  `[ExcludeFromCodeCoverage]` (`AutoAssignPeople.cs:114-122`). A Moq
  `IPeopleScoDictionaryNew` pass-through test
  (`AutoAssignPeopleTests.cs:104-121`) exercises it; measured at 100% line rate.
- **B2 (resolved):** `AutoAssignPeople` now has a
  `Func<IPrefix,string,Category> _createCategory` seam (`AutoAssignPeople.cs:29`)
  with an optional constructor parameter (`AutoAssignPeople.cs:31-40`) defaulting
  to an exempt `DefaultCreateCategory` (`AutoAssignPeople.cs:131-139`).
  `AddColorCategory` (`AutoAssignPeople.cs:124-127`) delegates to the seam and is
  no longer exempt; a stub-injection test (`AutoAssignPeopleTests.cs:123-151`)
  covers it; measured at 100% line rate.
- **M1 (resolved):** `EditFilterController.DeleteFilterDialog`, its orphaned
  private single-arg constructor, and the unused `using System.Windows.Forms`
  are deleted. No occurrence of `DeleteFilterDialog` remains in any `.cs` file.
- **M2 (resolved):** the empty `SetUpDeleteDialog()` and its sole test caller are
  deleted. No occurrence of `SetUpDeleteDialog` remains in any `.cs` file.

The independent full policy sweep confirms: file size <= 500 (max touched file
262 lines), zero banned-API / determinism violations in the changed test files,
MSTest + Moq + FluentAssertions framework, all four C# toolchain gates EXIT 0,
and the >= 80% testable-denominator coverage floor met at measured 89.72%.
Zero Blocking findings and zero Major findings remain.

## Rejected Scope Narrowing

None. The caller directed the C# coverage row to be recorded as PASS on the basis
of the measured 89.72% project coverage and the ratified 80% testable-denominator
policy in CLAUDE.md; this is consistent with the full-branch-diff scope and is not
a narrowing (the language genuinely passes). No caller instruction attempted to
narrow scope to a subset of files, skip a toolchain check for a changed language,
or mark a changed language out of scope. The caller's adjudication that
`ManageFiltersController.DefaultEditFilterFactory` is a ratified irreducible
live-form default seam is a legitimate prior-ratified decision, not a scope
narrowing.

## Evidence Location Compliance

The branch diff writes all feature evidence under
`docs/features/active/2026-07-09-taskvisualization-secondary-testability-298/evidence/<kind>/`
(`baseline/`, `qa-gates/`, `other/`), which is the canonical
`<FEATURE>/evidence/<kind>/` location. A scan of the diff file list for
`artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or
`artifacts/coverage/` returned no matches. The machine-readable Cobertura input at
`artifacts/csharp/coverage.xml` is a coverage-tooling input artifact (the
canonical per-language coverage-artifact path in this reviewer's coverage table),
not a feature evidence artifact, and is not a violation. No evidence-location
violation found.

## 1. Coverage Policy (per-language)

### 1.1 Changed-language set (from branch diff)

The branch diff changes C# production and test files plus two `.csproj` files.
No TypeScript, Python, or PowerShell production files are changed.

| Language | Changed files in diff? | Coverage verdict |
|---|---|---|
| C# / .NET | Yes (11 production `.cs`, 9 test `.cs`, 2 `.csproj`) | **PASS** |
| TypeScript | No | N/A (zero changed files) |
| Python | No | N/A (zero changed files) |
| PowerShell | No | N/A (zero changed files) |

### 1.2 C# / .NET coverage

- Coverage artifact: `artifacts/csharp/coverage.xml` (Cobertura). Post-remediation figures corroborated by `evidence/qa-gates/remediation-final-vstest-coverage.2026-07-10T07-40.md` and `evidence/qa-gates/remediation-coverage-delta.2026-07-10T07-40.md`.
- Repo-wide / project-wide figure: **TaskVisualization project line coverage 89.72% (1431/1595 lines)** (Cobertura header line-rate=0.89717868, branch-rate=0.8275, branches-covered=331/400).
- Applicable floor: CLAUDE.md ratified COM/VSTO/WinForms **80% testable-denominator** policy (Form-derived and Designer-generated classes carry class-level `[ExcludeFromCodeCoverage]`; testable seams are NOT exempt).
- New-class coverage: `FlagCalculations` 100%, `ManageFiltersController` 100%, `EditFilterController` (retargeted) orchestration measured; `AddChoicesToDict` 100%, `AddColorCategory` 100% (both newly measured post-remediation).

#### 1.2.1 Comparison (baseline vs post-change)

- **C# / TaskVisualization line coverage**
  - Baseline: 89.45% (1424/1592 lines) — pre-remediation cycle-1 snapshot (`evidence/qa-gates/remediation-coverage-delta.2026-07-10T07-40.md`)
  - Post-change: 89.72% (1431/1595 lines)
  - Change: +0.27 percentage points on a denominator grown by +3 lines (B1/B2 exemptions removed and their logic added to measurement; one new exempt line `DefaultCreateCategory`)
  - New/changed-code coverage: 100% (`AddChoicesToDict`, `AddColorCategory`, `FlagCalculations`, `ManageFiltersController`); all >= 90%
  - Disposition: PASS (>= 80% testable-denominator floor met; no regression on changed lines; each new/changed member >= 90%)
  - Evidence: `evidence/qa-gates/remediation-final-vstest-coverage.2026-07-10T07-40.md`, `evidence/qa-gates/remediation-coverage-delta.2026-07-10T07-40.md`, `artifacts/csharp/coverage.xml`

Coverage floor verdict for C#: **PASS**. The measured 89.72% clears the ratified
80% testable-denominator floor; the two prior coverage-exclusion Blocking findings
(B1, B2) are resolved, so both formerly-exempt seam methods are now measured at
100%.

## 2. Coverage-Exclusion Policy (`[ExcludeFromCodeCoverage]` audit)

PASS. The General Unit Test Policy and CLAUDE.md permit `[ExcludeFromCodeCoverage]`
only on irreducible COM/VSTO/WinForms wiring that lacks an injectable seam.
"Testable seams within otherwise-COM-bound assemblies ... are explicitly NOT
exempt." The two prior violations are resolved: `AddChoicesToDict` and
`AddColorCategory` are no longer exempt and are measured at 100%. The remaining
exemptions on `AutoAssignPeople` are `RunPeopleClassifier` (live
`AutoFile.AutoFindPeople`), `DefaultToHelper` (live `MailItemHelper`
construction), and the single new `DefaultCreateCategory` (live MAPI
`CreateCategoryModule.CreateCategory`) — each an irreducible one-line live-host
call behind an injectable seam. The exemption on
`ManageFiltersController.DefaultEditFilterFactory` is a ratified irreducible
live-form default seam (adjudicated as permitted per caller instruction; both
branches asserted through the injected seam in `AddFilter`/`EditSelected` tests)
and is NOT a finding. `evidence/other/exemption-inventory.md` reflects the removed
and added exemptions.

## 3. File-Size Policy (<= 500 lines)

PASS. Maximum touched production file is `EditFilterController.cs` at 262 lines;
`AutoCreateProject.cs` 261; `FlagTasks.cs` 207. All touched production and test
files are <= 500 lines (verified by line-count of every changed `.cs` file in the
diff). `EditFilterViewer.designer.cs` (Designer-generated) is NOT in the change
set and is carried under the form partial's class-level exemption; per the General
Code Change Policy generated designer code is not hand-split. No violation.

## 4. Banned-API / Determinism Policy (tests)

PASS. An independent scan of the nine changed `TaskVisualization.Test` files
(`AutoAssignContextTests.cs`, `AutoAssignPeopleTests.cs`,
`AutoCreateProjectTests.cs`, `EditFilterControllerTests.cs`,
`FlagCalculationsTests.cs`, `FlagChangeGroupTests.cs`, `FlagChangeItemTests.cs`,
`FlagChangeTrainingQueueTests.cs`, `ManageFiltersControllerTests.cs`) for
`ShowDialog`, `.Show()`, `new *Viewer(`, `MessageBox`, `Thread.Sleep`,
`Task.Delay`, `Path.GetTemp*`, and `GetTempFileName` returned no matches. All
controller tests drive `Mock<IEditFilterViewer>` / `Mock<IManageFiltersViewer>`
plus injected factory/tag-selector/edit-filter-factory/`_createCategory` seams; no
live form is constructed and no popup is shown. Async tests await mocked results.
(Pre-existing `new MoqTaskViewer()` occurrences appear only in the sibling #297
`TaskController*Tests.cs` files, which are not in the #298 diff; `MoqTaskViewer`
is a mock, not a live form.) Confirms `issue.md` AC #5.

## 5. Framework Policy (C# unit tests)

PASS. Tests use MSTest (`[TestClass]`/`[TestMethod]`), Moq for mocks, and
FluentAssertions for assertions, per CUT1/CUT2. All nine changed test files
reference `Microsoft.VisualStudio.TestTools.UnitTesting`; the pure-static /
POCO test files (`FlagCalculationsTests.cs`, `FlagChangeItemTests.cs`) legitimately
use MSTest + FluentAssertions without Moq.

## 6. Toolchain Policy (format -> lint -> type-check -> test)

PASS. All four C# gates report EXIT 0 at the branch head:
- csharpier `check` — EXIT 0 (`evidence/qa-gates/remediation-final-csharpier.2026-07-10T07-40.md`)
- msbuild analyzers (`EnableNETAnalyzers`/`EnforceCodeStyleInBuild`) — EXIT 0, zero analyzer errors; no IDE0005/IDE0051/CS0246 from the M1 removals (`evidence/qa-gates/remediation-final-msbuild-analyzers.2026-07-10T07-40.md`)
- msbuild nullable (`Nullable=enable`/`TreatWarningsAsErrors`) — EXIT 0; the new optional `createCategory` parameter introduced no nullable/binding warnings (`evidence/qa-gates/remediation-final-msbuild-nullable.2026-07-10T07-40.md`)
- vstest `/EnableCodeCoverage` (Cobertura runsettings, `/InIsolation`) — 161/161 pass, EXIT 0 (`evidence/qa-gates/remediation-final-vstest-coverage.2026-07-10T07-40.md`)

## 7. Policy Findings Summary

| Row label | Rule | Verdict |
|---|---|---|
| C# coverage floor | CLAUDE.md 80% testable-denominator | PASS (89.72%) |
| C# coverage-exclusion | testable seams never exempt (CLAUDE.md / general-unit-test) | PASS (B1, B2 resolved) |
| File size <= 500 | general-code-change §4 | PASS |
| Banned APIs / determinism | general-unit-test | PASS |
| Test framework | CUT1/CUT2 | PASS |
| Toolchain loop | general-code-change §8 | PASS |
| Dead code / simplicity-first | general-code-change design principles | PASS (M1, M2 resolved) |

## Remediation Triggers

None. All four prior findings (B1, B2 Blocking; M1, M2 Major) are confirmed
resolved at HEAD `b49dbebc`. No new remediation-required findings were identified.
No `remediation-inputs` artifact is produced for this cycle.
