# POSTING BLOCKED — Issue #440 Update Mirror (plan task P5-T17)

Timestamp: 2026-08-29T06-45

PostedAs: not posted — deferred to the orchestrator

## Reason it was not posted

The `gh` CLI is available and authenticated in this environment, so the block is not
a tooling failure. It is a scope constraint. This executor's stop point reserves every
remote-publishing action — push, pull request, merge — to the orchestrator, and a
GitHub issue comment is a remote-publishing action of the same class. The branch
`bug/breadcrumb-left-right-arrow-parent-child-navigation-440` has deliberately not
been pushed, so a comment posted now would cite a commit that is not reachable on the
remote and a reader could not verify any of its claims.

The exact text intended for issue #440 is recorded verbatim below. The orchestrator
should post it after the push, adding the commit SHA and the pull-request link, which
this executor cannot supply.

## Exact text intended for issue #440

> **Residual Qfc defect fixed: repeated Left now walks the ancestor chain to the root.**
>
> Most of #440 already landed on `main` under feature #498. The residual defect was
> Qfc-only and narrow: `BreadcrumbStateModel.LeftArrow()` gated its parent-select on
> the active segment index equalling the last chain position, so Left walked up
> exactly one level and the second press fell through to the legacy handler, which
> closed the QuickFiler folder drop-down. The Efc surface already walked to the root
> and is unchanged.
>
> **The fix** removes exactly one conjunct from one guard in one production method:
> the clause `activeIndex.Value == row.Chain.Count - 1` in `LeftArrow()` in
> `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs`. The
> `_selectedSubfolderIndex < 0` clause is retained, so Left with a subfolder
> highlighted still resets the subfolder selection rather than selecting a parent.
> The root boundary is now enforced by `BreadcrumbStateRow.ActivateSegment`, which
> already refuses a negative index, so Left at the root still returns `false` and the
> legacy fall-through that dismisses the drop-down is preserved unchanged. The
> adjacent `#440` comment was rewritten to describe the walk.
>
> **Tests.** Two new MSTest cases in
> `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs` cover
> the walk and the open-leaf-expansion edge case, asserting the active segment index
> after every press. Both were confirmed red before the fix and green after. Two
> landed tests that encoded the one-step limit were corrected:
> `Arrows_RightExpandsThenLeftCollapses_UnhandledWhenNothingChanges` in the same file
> and `Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft` in
> `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs`; each
> now drives the chain to the root before asserting the unhandled press.
>
> **Verification.** The full suite is green at 6859 of 6859, up from a 6857 baseline
> by exactly the two added tests, with no failures in either run. The full C#
> toolchain passed in a single final pass: CSharpier check exit 0 over 1560 files,
> matching the baseline count; the analyzer and nullable msbuild `/t:Rebuild` gates
> each exit 0 with 0 errors and 5 warnings, unchanged from baseline, and each shown
> non-vacuous with zero `Skipping target "CoreCompile"` occurrences. Repository-wide
> line coverage moved from 85.2935 % to 85.3026 % and branch coverage from 79.2523 %
> to 79.2558 %. For the changed file, uncovered lines held at 2 and uncovered
> branches held at 3, and every line in the post-change `LeftArrow()` span is covered.
>
> **Scope.** The diff is exactly three source files plus this feature folder's
> documentation and evidence. The Efc surface, the HTML bridge asset, the QuickFiler
> keyboard handler and the test project file are all absent from it.
>
> **Known divergences left in place**, both recorded as non-goals in the spec rather
> than defects introduced here: the Right-descent commit asymmetry between the two
> surfaces, and the single-level Right descent limit present on both.

## Posting instructions for the orchestrator

Post as a **comment** on https://github.com/drmoisan/TaskMaster/issues/440, then
record the resulting comment URL and the commit SHA in this artifact, replacing the
`POSTING BLOCKED` header with `PostedAs: comment`.
