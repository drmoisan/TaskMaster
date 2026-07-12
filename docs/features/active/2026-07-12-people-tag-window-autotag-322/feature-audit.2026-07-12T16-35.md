# Feature Audit — people-tag-window-autotag (Issue #322)

- Timestamp: 2026-07-12T16-35
- Work Mode: `minor-audit`
- AC Source: `docs/features/active/2026-07-12-people-tag-window-autotag-322/issue.md`, `## Acceptance Criteria` section only (per work-mode contract; `spec.md`/`user-story.md` are not present and are not required for `minor-audit`)

## Scope and Baseline

- Resolved base branch: `main`, resolved to `origin/main @ 3faa0727211bc75741e433f5ef23ba9c9850ea22`.
- Head: `bug/people-tag-window-autotag-322 @ ee49fb15c12b77448ab69ea8307c1426ab6b4dd4`.
- Merge base: `3faa0727211bc75741e433f5ef23ba9c9850ea22`.
- Diff scope (verified via `git diff --name-status 3faa0727..ee49fb15`, 33 files changed, +18527/-1): 5 `.cs` files (2 production, 3 test) and 28 Markdown/XML/runsettings feature-folder artifacts (plan, issue, evidence). No other language or project is touched.
- `artifacts/pr_context.summary.txt`'s "Changed files overview" undercounts the `.cs` files (reports "Core logic changes: 0 files"); this audit uses `git diff --name-status` and `artifacts/pr_context.appendix.txt` directly as the authoritative scope source (see `policy-audit.2026-07-12T16-35.md` `## PR-Context Tooling Defect`).

## Acceptance Criteria Inventory

Source: `issue.md`, `## Acceptance Criteria` (lines 61-68), 6 items, all originally unchecked in the pre-fix baseline and all checked `[x]` in the current branch head.

1. AC1 — The root cause of the auto-tag function not being invoked from the People tag-assignment window is identified and documented in the fix commit/plan evidence.
2. AC2 — A failing regression test is authored first that reproduces the defect deterministically (MSTest + Moq + FluentAssertions, no live Outlook process, no temporary files), and it passes after the fix.
3. AC3 — After the fix, invoking the auto-tag function on the People tag-assignment window executes the people auto-assign path (`IAutoAssign.AutoFindAsync` reaching the people classifier seam) for the active item instead of silently returning without invoking it.
4. AC4 — Matching auto-found people tags are toggled on in the dialog options when the mapping contains entries for the item, verified via unit test through the `TagController` auto-assign action seam.
5. AC5 — Existing behavior for the Context and Project assignment flows is unchanged (no regression in their tests).
6. AC6 — The full C# toolchain passes in order (CSharpier format, analyzers build, nullable build, MSTest with coverage) with no regression on changed lines, and changed/new code meets the >= 90% coverage target for testable seams.

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence | Notes |
|---|---|---|---|
| AC1 | PASS | `evidence/other/root-cause-322.2026-07-12T15-57.md`; independently confirmed by reading `TaskVisualization/TaskController.Actions.cs:46`, `Tags/TagController.cs:101-116`, and `TaskVisualization/AutoAssignPeople.cs:59-87` directly | Root cause is documented with exact file:line citations for both the primary argument-type defect and the secondary `ResolveMailItem` gate; verified by direct code inspection, not merely evidence-file assertion. |
| AC2 | PASS | `evidence/regression-testing/fail-before-322.2026-07-12T15-57.md` (EXIT_CODE 1, `AssignPeople_PassesOutlookItemWrapper_NotInnerObject` failed pre-fix), `evidence/regression-testing/pass-after-322.2026-07-12T15-57.md` (EXIT_CODE 0, same test passes post-fix); test itself uses MSTest + Moq + FluentAssertions with no live Outlook process or temp files (`TaskVisualization.Test/TaskControllerActionsTests.cs`) | Fail-first/pass-after sequence is independently evidenced with numeric exit codes, not just narrated. |
| AC3 | PASS | `Tags/TagController.cs:299` (`_autoAssigner.AutoFindAsync(_objItem)` where `_objItem` is now the wrapper reaching `AutoFind`'s `IOutlookItem` branch); `AutoFind_OutlookItemMailBranch_RoutesThroughToHelperSeam` in `TaskVisualization.Test/AutoAssignPeopleTests.cs` proves the wrapper reaches the `_toHelper` seam (i.e., past the branch dispatch and into the classifier construction step) rather than the silent `else` fallthrough | Verified the fix closes the exact code path identified in the root-cause doc: `AssignPeople()` → `_active.OlItem` (wrapper) → `TagController._objItem` → `ButtonAutoAssign_Action` → `AutoFindAsync(_objItem)` → `AutoFind`'s `IOutlookItem`-wrapped-mail branch. |
| AC4 | PASS | Pre-existing `AutoAssignAction_WhenExistingAndNewAssignmentsReturned_UpdatesSelections` (`Tags.Test/TagControllerCoverageExpansionTests.cs`, unmodified) verifies tag-toggle behavior through `ButtonAutoAssign_Action`; new `ResolveMailItem_OutlookItemWrappedMail_ReturnsInnerMailItem` verifies the People-flow-specific wrapper-recognition fix that makes the auto-assign button reachable in the first place | See code-review Low-severity finding: the new test verifies `ResolveMailItem`'s isolated return value rather than the full constructor-driven `_isMail`/button-visibility path for an `IOutlookItem`-wrapped argument. This is a test-design gap, not an unmet acceptance criterion — the toggle behavior itself (the criterion's literal text) is covered by the pre-existing test, and the enabling condition is covered by Cobertura branch coverage plus the isolated method test. Graded PASS; the gap is tracked as a non-blocking recommendation. |
| AC5 | PASS | `evidence/regression-testing/targeted-no-regression-322.2026-07-12T15-57.md` — 54/54 passed for a filter covering `AssignContext\|AssignProject\|AssignTopic\|AutoAssignPeople\|TagController` | No production line in `AssignContext`/`AssignProject`/`AssignTopic` was touched by this diff (confirmed via `git diff`); their tests pass unmodified. |
| AC6 | PASS | `evidence/qa-gates/csharpier-final-322.2026-07-12T15-57.md` (EXIT_CODE 0), `evidence/qa-gates/analyzer-final-322.2026-07-12T15-57.md` (EXIT_CODE 0, 0 errors), `evidence/qa-gates/nullable-final-322.2026-07-12T15-57.md` (EXIT_CODE 0, 0 warnings/errors), `evidence/qa-gates/vstest-coverage-final-322.2026-07-12T15-57.md` (EXIT_CODE 0, 228/228 passed), `evidence/qa-gates/coverage-delta-322.2026-07-12T15-57.md` (100% on both changed production regions, no regression) | Toolchain run in the required order; all four stages green in the final pass. Changed/new-code coverage (100%) exceeds the 90% target. See `policy-audit.2026-07-12T16-35.md` `## 1.2.1` for the full numeric table and the local-scope caveat (evidence limited to the two touched packages, not the full solution, due to a documented pre-existing local-execution constraint). |

## Summary

All 6 acceptance criteria in `issue.md`'s `## Acceptance Criteria` section evaluate to PASS against direct code inspection and the evidence trail in `docs/features/active/2026-07-12-people-tag-window-autotag-322/evidence/`. The fix is minimal and symmetrical (mirrors the existing `AssignContext`/`AssignProject`/`AssignTopic` argument pattern and the existing `AutoAssignPeople.AutoFind` branch pattern), is fully covered by the touched-package Cobertura report, and introduces no regression in adjacent flows. One non-blocking test-design recommendation is recorded in `code-review.2026-07-12T16-35.md` (AC4) but does not change the PASS verdict.

**Recommendation: PR-ready (go).**

## Acceptance Criteria Check-off

All 6 items in `docs/features/active/2026-07-12-people-tag-window-autotag-322/issue.md`'s `## Acceptance Criteria` section were already checked (`[x]`) at review time, matching the PASS verdicts above. No check-off changes were made by this review (nothing to change).

### Acceptance Criteria Status

- Source: `docs/features/active/2026-07-12-people-tag-window-autotag-322/issue.md`
- Total AC items: 6
- Checked off (delivered): 6
- Remaining (unchecked): 0
- Items remaining: none
