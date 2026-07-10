# Policy Audit — taskvisualization-secondary-testability (#298)

- Timestamp: 2026-07-10T07-40
- Reviewer: feature-reviewer (authoritative feature-review)
- Branch under review: `feature/taskvisualization-secondary-testability-298` @ `f2d2d476b507ef4fb713d54d7c39575989f7f433`
- Diff base: `epic/winforms-testability-refactor-integration` @ `949dddd2df0df4511fcc0ff44c4d77c38821c54c` (merge-base = integration head; clean linear descendant)
- Diff command: `git -C C:/Users/DanMoisan/repos/TaskMaster-wt/winforms-298 diff 949dddd2df0df4511fcc0ff44c4d77c38821c54c...HEAD`
- Work mode: `full-feature` (AC sources = `spec.md` Definition of Done/alignment + `issue.md` `## Acceptance Criteria`; `user-story.md` intentionally absent per `spec.md` "User Story Applicability")

## Executive Summary

Overall policy verdict: **NOT READY TO MERGE**.

The feature meets the file-size limit, the banned-API rule, the framework
requirement (MSTest + Moq + FluentAssertions), the full C# toolchain
(csharpier -> analyzers -> nullable -> MSTest, all EXIT 0), and the >= 80%
testable-denominator coverage floor (measured 89.45%). Two Blocking
coverage-exclusion / seams-first violations were confirmed against the code:
`[ExcludeFromCodeCoverage]` is applied to `AutoAssignPeople.AddChoicesToDict`
(a testable interface-seam method) and to the whole of
`AutoAssignPeople.AddColorCategory` (whose identical MAPI call is seamed and
measured in the sibling `AutoCreateProject` within the same feature). Both
violate the ratified CLAUDE.md rule that "testable seams are never exempt" and
the seams-first ordering in the C# policy. Two related Major findings concern
dead code retained/added in `EditFilterController`.

## Rejected Scope Narrowing

None. The caller directed the C# coverage row to be recorded as PASS on the
basis of the measured 89.45% project coverage and the ratified 80%
testable-denominator policy; this is consistent with the full-branch-diff scope
and is not a narrowing (the language genuinely passes). No caller instruction
attempted to narrow scope, skip a toolchain check, or mark a changed language
out of scope.

## Evidence Location Compliance

The branch diff writes all feature evidence under
`docs/features/active/2026-07-09-taskvisualization-secondary-testability-298/evidence/<kind>/`
(`baseline/`, `qa-gates/`, `other/`), which is the canonical
`<FEATURE>/evidence/<kind>/` location. No evidence file is written to
`artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or
`artifacts/coverage/`. The machine-readable Cobertura input at
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

- Coverage artifact: `artifacts/csharp/coverage.xml` (Cobertura, present, 325 KB), refreshed 2026-07-10 by the final vstest gate.
- Repo-wide / project-wide figure: **TaskVisualization project line coverage 89.45% (1424/1592 lines)** per `evidence/qa-gates/final-vstest-coverage.md` and `evidence/qa-gates/coverage-delta.md`.
- Applicable floor: CLAUDE.md ratified COM/VSTO/WinForms **80% testable-denominator** policy (Form-derived and Designer-generated classes carry class-level `[ExcludeFromCodeCoverage]`; testable seams are NOT exempt).
- New-class coverage: `FlagCalculations` 100%, `ManageFiltersController` 100%, `EditFilterController` (retargeted) 95.07% — all >= 90%.

#### 1.2.1 Comparison (baseline vs post-change)

- **C# / TaskVisualization line coverage**
  - Baseline: 85.36% (1032/1209 lines, pre-#298 with #197 class-level exemptions)
  - Post-change: 89.45% (1424/1592 lines)
  - Change: +4.09 percentage points on a denominator grown by +383 lines (class-level exemptions removed and their logic added to measurement)
  - New/changed-code coverage: 100% (`FlagCalculations`, `ManageFiltersController`); 95.07% (`EditFilterController` retargeted) — all >= 90%
  - Disposition: PASS (>= 80% testable-denominator floor met; each new class >= 90%)
  - Evidence: `evidence/qa-gates/final-vstest-coverage.md`, `evidence/qa-gates/coverage-delta.md`, `artifacts/csharp/coverage.xml`

Coverage floor verdict for C#: **PASS**. The measured 89.45% clears the ratified
80% testable-denominator floor even with the disputed exemptions counted as
uncovered (the two disputed sites total roughly two source lines). The Blocking
findings below concern the *exemption policy* ("testable seams are never
exempt"), not the numeric floor.

## 2. Coverage-Exclusion Policy (`[ExcludeFromCodeCoverage]` audit)

The General Unit Test Policy and CLAUDE.md permit `[ExcludeFromCodeCoverage]`
only on irreducible COM/VSTO/WinForms wiring that lacks an injectable seam.
"Testable seams within otherwise-COM-bound assemblies ... are explicitly NOT
exempt." The feature's own `spec.md` (lines 261, 297) and
`evidence/other/exemption-inventory.md` restate this rule. Two exemptions violate
it (see Findings B1, B2). The exemption on
`ManageFiltersController.DefaultEditFilterFactory` is a ratified irreducible
live-form default seam (adjudicated as permitted; both branches asserted through
the injected seam in `AddFilter`/`EditSelected` tests) and is NOT a finding.

## 3. File-Size Policy (<= 500 lines)

PASS. Maximum touched production file is `EditFilterController.cs` at 289 lines;
all touched production and test files are <= 500 lines
(`evidence/other/file-size-check.md`). `EditFilterViewer.designer.cs` (503 lines)
is Designer-generated, is NOT in the change set (confirmed against the diff), and
is carried under the form partial's class-level exemption; per the General Code
Change Policy generated designer code is not hand-split. No violation.

## 4. Banned-API / Determinism Policy (tests)

PASS. A scan of `TaskVisualization.Test` for `Thread.Sleep`, `Task.Delay`,
`Path.GetTemp*`, `GetTempFileName`, `new TagViewer`, `new EditFilterViewer`,
`ShowDialog`, `.Show()`, and `MessageBox` returned no matches. All controller
tests drive `Mock<IEditFilterViewer>` / `Mock<IManageFiltersViewer>` plus
injected factory/tag-selector/edit-filter-factory seams; no live form is
constructed and no popup is shown. Async tests await mocked results. Confirms
`issue.md` AC #5.

## 5. Framework Policy (C# unit tests)

PASS. Tests use MSTest (`[TestClass]`/`[TestMethod]`), Moq for mocks, and
FluentAssertions for assertions, per CUT1/CUT2.

## 6. Toolchain Policy (format -> lint -> type-check -> test)

PASS. All four C# gates report EXIT 0 at the branch head:
- csharpier `check .` — EXIT 0 (`evidence/qa-gates/final-csharpier.md`)
- msbuild analyzers (`EnableNETAnalyzers`/`EnforceCodeStyleInBuild`) — EXIT 0, zero analyzer errors (`evidence/qa-gates/final-msbuild-analyzers.md`)
- msbuild nullable (`Nullable=enable`/`TreatWarningsAsErrors`) — EXIT 0, no new nullable errors in touched code (`evidence/qa-gates/final-msbuild-nullable.md`)
- vstest `/EnableCodeCoverage` (Cobertura runsettings, `/InIsolation`) — 159/159 pass, EXIT 0 (`evidence/qa-gates/final-vstest-coverage.md`)

## 7. Policy Findings Summary

| Row label | Rule | Verdict |
|---|---|---|
| C# coverage floor | CLAUDE.md 80% testable-denominator | PASS (89.45%) |
| C# coverage-exclusion | testable seams never exempt (CLAUDE.md / general-unit-test) | FAIL (Findings B1, B2) |
| File size <= 500 | general-code-change §4 | PASS |
| Banned APIs / determinism | general-unit-test | PASS |
| Test framework | CUT1/CUT2 | PASS |
| Toolchain loop | general-code-change §8 | PASS |
| Dead code / simplicity-first | general-code-change design principles | FAIL (Findings M1, M2) |

## Remediation Triggers

Findings B1 and B2 (Blocking) and M1, M2 (Major) are remediation-required and are
enumerated with file:line, violated rule, and required fix in
`remediation-inputs.2026-07-10T07-40.md`.
