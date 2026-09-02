# breadcrumb-router-test-cr1-defect-neutral (Issue #693)

- Date captured: 2026-08-29
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/breadcrumb-router-test-cr1-defect-neutral/ (Issue #693)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #693
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/693
- Last Updated: 2026-08-29
## Summary

The regression test `Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft` in `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` is defect-neutral for the #440 fix: its Arrange phase presses the router and discards the results, so the test passes identically before and after the #440 fix regardless of whether the underlying defect exists.

## Environment

- OS/version: Windows, git repository `TaskMaster`
- Command/flags used: n/a - discovered during item-440 implementation in `/parallel-run bugs-635-440`
- Data source or fixture: `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs`

## Steps to Reproduce

1. Open `Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft` in `FolderBreadcrumbBridgeRouterTests.cs`.
2. Note the Arrange phase invokes the router's arrow-press handling one or more times and discards the returned result(s) rather than asserting on them, before the Act/Assert section that actually checks the "nothing to collapse" case.
3. Revert the #440 fix locally (or check out `main` before commit `ecdb1c84`) and re-run this test - it still passes, because its assertions never depended on the discarded prior presses.

## Expected Behavior

A regression test named for a specific router scenario should fail against the pre-fix behavior and pass only after the fix, so it actually pins the behavior it names.

## Actual Behavior

The test passes both before and after the #440 fix because its Arrange phase discards intermediate press results instead of asserting on them, making it defect-neutral with respect to the scenario it is named for.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: parallel-run bugs-635-440 final report, item CR-1: "router test `Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft` is defect-neutral (its Arrange presses discard results), so it passes before and after the fix."

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

## Suspected Cause / Notes

Likely a copy/paste or scaffolding artifact where setup presses meant to establish router state were written as bare statements instead of being captured and asserted, or asserted only on the final press rather than each intermediate one relevant to the scenario name.

## Proposed Fix / Validation Ideas

- [ ] Capture and assert on the discarded Arrange-phase press results, or restructure the test so its Act/Assert actually exercises the "nothing to collapse" transition it is named for
- [ ] Verify the rewritten test fails against the pre-#440 behavior and passes after, confirming it pins the fix
- [ ] Audit sibling tests in the same file for the same discard-in-Arrange pattern

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
