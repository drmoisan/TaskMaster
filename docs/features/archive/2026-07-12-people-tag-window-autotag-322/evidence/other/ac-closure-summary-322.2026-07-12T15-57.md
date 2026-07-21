Timestamp: 2026-07-12T15-57

# AC closure summary — issue #322

| AC | Text (abridged) | Status | Backing evidence |
|---|---|---|---|
| AC1 | Root cause identified and documented | [x] | `evidence/other/root-cause-322.2026-07-12T15-57.md` |
| AC2 | Failing regression test authored first; passes after fix | [x] | `evidence/regression-testing/fail-before-322.2026-07-12T15-57.md` (fail-before), `evidence/regression-testing/pass-after-322.2026-07-12T15-57.md` (pass-after) |
| AC3 | Auto-tag function executes the people auto-assign path for the active item | [x] | `evidence/regression-testing/pass-after-322.2026-07-12T15-57.md`; P1-T3 coverage-confirmation test `AutoFind_OutlookItemMailBranch_RoutesThroughToHelperSeam` in `TaskVisualization.Test/AutoAssignPeopleTests.cs` |
| AC4 | Matching auto-found people tags toggled on, verified via `TagController` auto-assign action seam | [x] | `evidence/regression-testing/targeted-no-regression-322.2026-07-12T15-57.md` (existing `ButtonAutoAssign_Action`/`SetAutoAssignState` coverage unchanged and passing); `evidence/other/secondary-fix-decision-322.2026-07-12T15-57.md` (ResolveMailItem wrapper-recognition fix); `ResolveMailItem_OutlookItemWrappedMail_ReturnsInnerMailItem` test in `Tags.Test/TagControllerSeamTests.cs` |
| AC5 | Context/Project flows unchanged (no regression in their tests) | [x] | `evidence/regression-testing/targeted-no-regression-322.2026-07-12T15-57.md` (54/54 passed) |
| AC6 | Full C# toolchain passes; no regression on changed lines; >=90% new/changed-code coverage | [x] | `evidence/qa-gates/csharpier-final-322.2026-07-12T15-57.md`, `evidence/qa-gates/analyzer-final-322.2026-07-12T15-57.md`, `evidence/qa-gates/nullable-final-322.2026-07-12T15-57.md`, `evidence/qa-gates/vstest-coverage-final-322.2026-07-12T15-57.md`, `evidence/qa-gates/coverage-delta-322.2026-07-12T15-57.md`, `evidence/qa-gates/regression-check-322.2026-07-12T15-57.md` |

All six acceptance criteria (AC1-AC6) are checked off (`[x]`) in
`docs/features/active/2026-07-12-people-tag-window-autotag-322/issue.md`'s `## Acceptance
Criteria` section.

## Production change summary

- `TaskVisualization/TaskController.Actions.cs:46` — one-line change: `AssignPeople()`'s
  `TagPromptRequest.objItemObject` argument changed from `_active.OlItem.InnerObject` to
  `_active.OlItem`, matching `AssignContext`/`AssignProject`/`AssignTopic`.
- `Tags/TagController.cs` — added `using UtilitiesCS.OutlookExtensions;` and one new `else if`
  branch in `ResolveMailItem` recognizing an `IOutlookItem`-wrapped mail item (mirroring
  `AutoAssignPeople.AutoFind`'s own branch pattern), returning its `InnerObject` cast to
  `MailItem`.

## Test change summary

- `TaskVisualization.Test/TaskControllerActionsTests.cs` — new test
  `AssignPeople_PassesOutlookItemWrapper_NotInnerObject` ([expect-fail] regression test, fail
  before fix / pass after fix).
- `TaskVisualization.Test/AutoAssignPeopleTests.cs` — new test
  `AutoFind_OutlookItemMailBranch_RoutesThroughToHelperSeam` (destination-branch coverage
  confirmation, passes without any production change since the branch pre-existed).
- `Tags.Test/TagControllerSeamTests.cs` — new test
  `ResolveMailItem_OutlookItemWrappedMail_ReturnsInnerMailItem` (added while closing a coverage
  gap discovered during P2-T5 on the new `ResolveMailItem` branch).

## Final numeric results

- Full-suite tests: 228/228 passed (baseline was 225/225).
- Coverage: `TaskVisualization.dll` 89.84% (baseline 89.72%), `Tags.dll` 92.69% (baseline 92.63%),
  combined 90.77% (baseline 90.66%) — no regression, both increased.
- Changed-line coverage: 100% on both changed production regions
  (`TaskController.Actions.cs:46`, `TagController.cs:107-113`).
- Toolchain: CSharpier format (0 files changed beyond the 5 intentional edits), analyzer build
  (0 errors), nullable build (0 errors; pre-existing `SVGControl` nullable debt confirmed
  unrelated and out of scope), MSTest with coverage (228/228 passed) — all green.
