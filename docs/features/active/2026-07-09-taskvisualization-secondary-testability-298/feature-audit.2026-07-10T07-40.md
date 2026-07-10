# Feature Audit — taskvisualization-secondary-testability (#298)

- Timestamp: 2026-07-10T07-40
- Branch: `feature/taskvisualization-secondary-testability-298` @ `f2d2d476b507ef4fb713d54d7c39575989f7f433`
- Work mode: `full-feature`
- AC sources: `issue.md` `## Acceptance Criteria` (seven items) + `spec.md` `## Definition of Done` / issue-alignment block. `user-story.md` intentionally absent per `spec.md` "User Story Applicability" (not a finding).

## Scope and Baseline

Baseline = `epic/winforms-testability-refactor-integration` @
`949dddd2df0df4511fcc0ff44c4d77c38821c54c` (merge-base = integration head; clean
linear descendant). The audit evaluates the full branch diff against this
baseline: 11 production `.cs` files (2 new: `FlagCalculations.cs`,
`IEditFilterViewer.cs`, `IManageFiltersViewer.cs`, `ManageFiltersController.cs`
new; the rest retargeted), 9 test `.cs` files, and 2 `.csproj` files.

## Acceptance Criteria Inventory

From `issue.md` `## Acceptance Criteria` (authoritative, mirrored in `spec.md`
Definition-of-Done alignment):

1. `IEditFilterViewer` and `IManageFiltersViewer` exist, derive from `IForm`, and their concrete forms implement them.
2. `EditFilterController` depends on `IEditFilterViewer`; `ManageFilters` logic is testable against `IManageFiltersViewer`.
3. Helper classes' host-neutral logic separated from COM interaction with seams at Interop boundaries.
4. No touched production file exceeds 500 lines.
5. No unit test constructs a live form/window or triggers a popup.
6. `TaskVisualization` project reaches >= 80% line coverage overall.
7. Full C# toolchain (csharpier -> analyzers -> nullable -> MSTest) passes with no regression.

## Acceptance Criteria Evaluation

| # | Acceptance Criterion | Verdict | Evidence / Rationale |
|---|---|---|---|
| 1 | Viewer interfaces exist, derive from `IForm`, forms implement them | PASS | `IEditFilterViewer.cs` and `IManageFiltersViewer.cs` derive from `IForm`; `EditFilterViewer : Form, IEditFilterViewer`, `ManageFilters : Form, IManageFiltersViewer`. Compiles clean (analyzer/nullable gates EXIT 0). |
| 2 | `EditFilterController` depends on `IEditFilterViewer`; `ManageFilters` logic testable via `IManageFiltersViewer` | PASS | `EditFilterController.cs:137` field is `IEditFilterViewer _viewer`; `ManageFiltersController` depends only on `IManageFiltersViewer` + `IApplicationGlobals`; both driven by `Mock<...Viewer>` in `EditFilterControllerTests` / `ManageFiltersControllerTests`. |
| 3 | Helper classes' host-neutral logic separated from COM with seams at Interop boundaries | PARTIAL | Largely satisfied: `FlagCalculations` extraction, `AutoCreateProject` `_chooseProgram`/`_createCategory`/`_getTaskItems` seams, `AutoAssign*` `_toHelper` seam. **Gap:** `AutoAssignPeople.AddColorCategory` (121-129) leaves the MAPI `CreateCategoryModule.CreateCategory` call inline-exempt instead of applying the `_createCategory` seam that the same feature uses in `AutoCreateProject`; and `AutoAssignPeople.AddChoicesToDict` (108-117) is exempted although it delegates to the mockable interface member `IPeopleScoDictionaryNew.AddMissingEntries`. Seam discipline is inconsistent at these two Interop boundaries (Blocking findings B1, B2). |
| 4 | No touched production file exceeds 500 lines | PASS | `evidence/other/file-size-check.md`: max touched file 289 lines. `EditFilterViewer.designer.cs` (503) is generated and not in the change set. |
| 5 | No unit test constructs a live form/window or triggers a popup | PASS | Independent scan of `TaskVisualization.Test` for `ShowDialog`/`.Show()`/`new *Viewer`/`MessageBox`/`Thread.Sleep`/`Task.Delay`/temp-file APIs returned no matches; all tests use mocked viewers + injected seams. |
| 6 | `TaskVisualization` project reaches >= 80% line coverage overall | PASS | Measured 89.45% (1424/1592) per `final-vstest-coverage.md` / `coverage-delta.md`; artifact `artifacts/csharp/coverage.xml`. Floor met even if the two disputed exemptions (~2 lines) were counted as uncovered. Coverage-exclusion policy concerns are tracked as findings B1/B2, but the numeric floor passes. |
| 7 | Full C# toolchain passes with no regression | PASS | csharpier EXIT 0; msbuild analyzers EXIT 0 (zero errors); msbuild nullable EXIT 0 (no new touched-code warnings); vstest 159/159 pass, EXIT 0. |

## Acceptance Criteria Check-off

Checked off in `issue.md` / `spec.md` by the executor for all seven items. This
review confirms 6 of 7 as PASS. AC #3 is downgraded to **PARTIAL** here due to the
seam-discipline gaps (Blocking findings B1, B2); its check-off in the source
files is left in place but this audit records the gap. No source-file checkbox is
newly toggled by this review (no previously-unchecked item became PASS).

## Summary

- Total AC items: 7
- PASS: 6 (AC 1, 2, 4, 5, 6, 7)
- PARTIAL: 1 (AC 3 — seam discipline inconsistent at two `AutoAssignPeople` Interop boundaries)
- FAIL: 0

### Acceptance Criteria Status
- Source: `issue.md` (`## Acceptance Criteria`), mirrored in `spec.md` (Definition of Done)
- Total AC items: 7
- Checked off (delivered): 7 (by executor)
- Confirmed PASS by review: 6
- PARTIAL: 1 (AC 3)
- Items remaining: AC 3 pending remediation of Blocking findings B1 and B2 (see `remediation-inputs.2026-07-10T07-40.md`)

## Overall Feature Verdict

**NOT READY TO MERGE.** All seven acceptance criteria are functionally delivered
and the coverage floor and toolchain are green, but two Blocking
coverage-exclusion / seams-first violations in `AutoAssignPeople` (and two related
Major dead-code items in `EditFilterController`) must be remediated first. These
violate the feature's own ratified "testable seams are never exempt" commitment.
