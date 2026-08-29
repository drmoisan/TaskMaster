# quickfiler-breadcrumb-bridge-r2-silently-reverts-440 (Issue #690)

- Date captured: 2026-08-29
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-breadcrumb-bridge-r2-silently-reverts-440/ (Issue #690)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #690
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/690
- Last Updated: 2026-08-29
## Summary

Branch `feature/quickfiler-breadcrumb-bridge-coverage-r2` (issue #495) was cut from `main` before issue #440's breadcrumb Left/Right arrow parent/child navigation fix merged (PR #689, commit `ecdb1c84`, 2026-08-29). It touches the same Qfc breadcrumb navigation surface #440 changed. Git reports no textual conflict against current `main`, so merging #495 as-is would silently revert the #440 fix with no warning at merge time.

## Environment

- OS/version: Windows, git repository `TaskMaster`
- Branches: `feature/quickfiler-breadcrumb-bridge-coverage-r2` (#495, pre-#440 base) vs. `main` at `ecdb1c84`
- Command/flags used: n/a - surfaced during `/parallel-run bugs-635-440` orchestration of item #440
- Data source or fixture: parallel-orchestrator final report for run `bugs-635-440`, section "Needs your decision", item OB-5

## Steps to Reproduce

1. Checkout `feature/quickfiler-breadcrumb-bridge-coverage-r2`, still based on `main` prior to PR #689.
2. Diff it against current `main`; observe it modifies the same Qfc breadcrumb files PR #689 changed (`BreadcrumbBridgeCoordinator` / `FolderBreadcrumbBridgeRouter` / `BreadcrumbStateModel` surface).
3. Open or simulate merging #495 into current `main` - git reports no conflicts.
4. After the merge, exercise the Left/Right arrow parent/child navigation added by #440 - the behavior is no longer present.

## Expected Behavior

Merging #495 should either produce a conflict requiring reconciliation, or preserve the #440 navigation behavior after the merge.

## Actual Behavior

The merge is clean and silently drops the #440 fix; there is no merge-time signal that behavior was lost.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: Reported verbatim by the item-440 orchestrator delegation in run `bugs-635-440` as "OB-5, the one with a deadline: feature/quickfiler-breadcrumb-bridge-coverage-r2 (#495) would silently revert the landed #440 payload with no merge conflict. It needs rebuilding on main before it lands."

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

## Suspected Cause / Notes

#495 was branched before #440 merged and independently touches the same breadcrumb navigation surface. Because #495's stored diff does not textually intersect the lines #440 changed (e.g. it may rewrite a method or region wholesale that #440 also touched), a standard 3-way merge produces no conflict markers while still functionally discarding #440's behavior change. This is a silent-revert pattern distinct from a normal merge conflict and needs to be caught before #495 lands, not after.

## Proposed Fix / Validation Ideas

- [ ] Rebuild/rebase `feature/quickfiler-breadcrumb-bridge-coverage-r2` onto current `main` (past `ecdb1c84`) before it merges
- [ ] Add a regression test asserting the #440 Left/Right arrow parent/child navigation behavior survives #495's merge
- [ ] Consider a general safeguard (e.g., an additions-only or behavior-diff check) for branches that overlap already-merged surfaces without textual conflict - see repository memory `feedback_stale_base_deletes_silently_on_fan_in`

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
