# Feature Audit: EFC and QFC arrow keys skip expanded subfolders (Issue #1007)

- Review timestamp: 2026-10-26T00-14 (re-audit after remediation cycle 1)
- Previous review: `feature-audit.2026-10-25T23-25.md` (13 PASS, AC4 PARTIAL)
- Reviewer: feature-review agent

## Scope and Baseline

- Branch: `bug/efc-arrow-keys-skip-subfolders-1007`, head `e804f5d6b` (remediation cycle 1 commit).
- Resolved base branch: `main`, merge base `3b9a1ebdd` (equal to the current `origin/main`, recomputed with `git merge-base HEAD origin/main`). The audit diff is `git diff 3b9a1ebdd..HEAD` (149 files), the full branch, not the cycle commit alone.
- Work mode: `full-bug` (`- Work Mode: full-bug` in `issue.md`); AC source: `spec.md` `## Acceptance Criteria` only (AC1 to AC14, listed in the order AC1 to AC11, AC14, AC12, AC13). `issue.md` carries a mirror. Decision record: `grill-transcript.md` (D-Q1 to D-Q29) and the spec's S1 to S10.
- Cycle inputs: `remediation-inputs.2026-10-25T23-25.md` and `remediation-plan.2026-10-25T23-50.md` (every task checked); cycle self-review `evidence/qa-gates/self-review.2026-10-26T00-09.md` (36 PASS, 0 FAIL, 4 N/A).
- PR context: `artifacts/pr_context.summary.txt` and `.appendix.txt` are absent from this worktree; scope was derived from `git diff`.
- Evidence basis: executor projections under `evidence/`, the canonical coverage artifact `artifacts/csharp/coverage.xml` (package counters summed by the reviewer), the post-processed Cobertura document of the same run (per-file figures recomputed by the reviewer), a reviewer re-run of the four node suites (90 of 90) and of `csharpier check` over the changed directories (exit 0), and reviewer reads of the cycle diff and the stylesheet.
- Manual Outlook verification (spec Test Strategy) is a human step recorded by the orchestrator; it is not evaluated here.

## Acceptance Criteria Inventory

Source: `docs/features/active/2026-10-22-efc-arrow-keys-skip-subfolders-1007/spec.md`, `## Acceptance Criteria`. Fourteen checkbox items, all checked `[x]` when this review began (AC4 re-checked by the cycle executor in `e804f5d6b`, criterion text unchanged).

| AC | Subject |
|---|---|
| AC1 | Up and Down visit every visible row in tree order |
| AC2 | No wrap; EFC Up on the first row focuses the search box |
| AC3 | EFC arrow selects as a click does; Enter files to it |
| AC4 | QFC open-list pending model (arrows move only the pending highlight; Enter, Escape, Left commit-and-close) |
| AC5 | Right expands or moves to the first child; shortened row lists the segment's children |
| AC6 | Right on a leaf (EFC no-op; QFC open: close, keep, pop-out) |
| AC7 | Left collapses only; #440 walk and Left segment hiding removed; nearest visible ancestor |
| AC8 | Ctrl+Left parent selection, Escape restores shape and value, Ctrl+Right is Right |
| AC9 | Segment double-click shortens and commits; `[+]` lists the segment's children; QFC non-leaf single click |
| AC10 | QFC closed list keys |
| AC11 | Home, End, PageUp, PageDown; EFC no-selection start |
| AC14 | No `[+]` without subfolders |
| AC12 | Regression tests first; rewritten #440 tests record the reason |
| AC13 | Toolchain, coverage and file size |

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence |
|---|---|---|
| AC1 | PASS | Unchanged since the first review (`BreadcrumbVisibleOrder`, EFC and QFC traversal tests; `visible-order-*`, `efc-vertical-*`, `qfc-open-*` fail-before and pass-after). With B-1 fixed, a pending descendant reached by Down in the open QFC list is now visibly highlighted. All tests pass in the final run (7761 of 7761). |
| AC2 | PASS | Unchanged since the first review (`Step` clamps; `ArrowDown_OnLastVisibleEntry_PostsNothing`, `ArrowUp_OnFirstEntry_RaisesFocusSearchRequested`, `SelectorKeyDown_Open_OnLastEntry_KeepsPending`). |
| AC3 | PASS | Unchanged since the first review (`CommitPosition` through the click paths; `ArrowDown_OntoDescendant_CommitsArchiveRelativePathAndRaisesChanged`). |
| AC4 | PASS | Behaviour: `OpenMove_DoesNotChangeCommittedValue`, `CommitPending_WithPendingNode_SelectsDescendant`, `Cancel_RestoresOriginalAndClearsPendingNode`, `OpenLeft_OnNonExpandedEntry_CommitsAndCloses` (Passed in `final-toolchain-tests.2026-10-26T00-03.md`). Visibility (B-1): `FolderBreadcrumb.html` line 158 styles `.sub.active` with `var(--bc-sel-back)` and `var(--bc-sel-fore)`, which are defined for the light, explicit dark and system dark themes; `makeRow` applies no `selected` class in the expanded view and drops `active` while the pending node owns it, so exactly one list element carries the highlight. `Stylesheet_StylesPendingDescendantLikeSelectedSubfolder` failed before the fix (`qfc-pending-style-fail-before.2026-10-25T23-55.md`) and passes after (`qfc-pending-style-pass-after.2026-10-25T23-55.md`, `qfc-pending-style.2026-10-26T00-05.md`); reviewer read of the stylesheet confirms one `.sub.active` match inside `<style>`. |
| AC5 | PASS | Unchanged since the first review (`ArrowRight_OnUnexpandedRowWithChildren_Expands`, `ArrowRight_ReentrantDuringFetch_EndsExpandedWithOneQuery`, `OpenRight_OnExpandedPendingEntry_PendsFirstChild`). Right onto a first child in the open QFC list is now visible (B-1). The N-1 test `RightArrow_OnRowCollapsedAwayFromActiveSegment_RestoresChain` adds direct coverage of the state-model restore path. |
| AC6 | PASS | Unchanged since the first review (`ArrowRight_OnLeaf_PostsNothing`, `OpenRight_OnLeaf_CancelsThenRaisesUnhandledArrowRight`). |
| AC7 | PASS | Unchanged since the first review (`ArrowLeft_OnExpandedNode_CollapsesOnlyThatNode`, `LeftArrow_NeverHidesTrailingSegment`, `CollapseHidingPending_MovesPendingToNearestVisibleAncestor`). The renamed `LeftArrow_RepeatedOnNonExpandedChain_IsUnhandledAndKeepsLeafActive` and `LeftArrow_FromOpenLeafExpansion_CollapsesThenIsUnhandled` now name the removal of the #440 walk correctly. |
| AC8 | PASS | Unchanged since the first review (`HandleParentLeft`, `MoveToParent`, `RestoreOpenShape`; `BridgeJs` and `FolderBreadcrumb.js` key mapping). |
| AC9 | PASS | Unchanged since the first review (`SegmentDoubleClick_ActivatesCommitsAndShortensAtomically`, `LeafExpandToggle_OnShortenedRow_ListsSegmentChildren`, `SelectorSegmentDoubleClick_CommitsSegmentFolderAndCloses`). The N-1 tests `LeafExpandToggle_OnRowCollapsedAwayFromActiveSegment_RestoresChain` and `AffordanceToggle_OnRowCollapsedAwayFromActiveSegment_RestoresChainWithoutQuery` pin the `[+]` behaviour on a collapse anchored away from the active segment in both viewers. |
| AC10 | PASS | Unchanged since the first review (seven G6 tests; `qfc-closed-fail-before` and `-pass-after`). A closed-list Right that places the pending highlight on the committed subfolder renders `sub selected active`, which is styled. |
| AC11 | PASS | Unchanged since the first review (`Home_SelectsFirstEntryWithoutFocusSearch`, `End_SelectsLastVisibleDescendant`, `PageDown_MovesByPageSizeAndClamps`, `NoSelection_DownAndHomeSelectFirst_UpAndEndSelectLast`). |
| AC14 | PASS | Unchanged since the first review (`MapNode` source fix and renderer defences; `OutlookFolderHierarchyProviderIssue1007Tests`, `BreadcrumbEmptyExpanderIssue1007Tests`, `MakeSubfolders_LoadedChildWithoutChildren_RendersNoToggle`). N-5 remains informational. |
| AC12 | PASS | First-review evidence stands (eight fail-before and pass-after pairs; the valid exception dossier). The cycle adds a ninth pair for B-1 (`qfc-pending-style-fail-before` and `-pass-after`); the four N-1 tests are characterization tests of unchanged behaviour and correctly pass on their first run (`n1-direct-tests.2026-10-25T23-58.md`). The N-2 renames resolve the remaining naming defect in the rewritten #440 tests. |
| AC13 | PASS | One clean pass in CLAUDE.md order (`final-toolchain-format.2026-10-26T00-00.md` after one recorded restart, `-analyzers.2026-10-26T00-01.md`, `-typecheck.2026-10-26T00-01.md`, `-tests.2026-10-26T00-03.md`: 7761 of 7761). Repository 86.49% line and 80.85% branch (reviewer sum of `artifacts/csharp/coverage.xml`); new files 91.84% to 100.00% line; the four N-1 files rose; no exclusion added. Largest changed code file 500 lines. Reviewer re-runs: `csharpier check` exit 0, node suites 90 of 90. |

## Remediation Findings Verification

| Finding | Status | Evidence |
|---|---|---|
| B-1 (blocking) | Resolved | Stylesheet rule at `FolderBreadcrumb.html` line 158; string contract with positive control; fail-before and pass-after; AC4 PASS. |
| N-1 | Resolved | Four direct tests; every N-1 body line from 0 to at least 1 hit (`coverage-comparison.2026-10-26T00-04.md`); per-file rises confirmed by reviewer recomputation. |
| N-2 | Resolved | Six renames; old names 0 matches, new names 1 each (`test-renames.2026-10-25T23-59.md`); hunks change signatures only. |
| N-3, N-4, N-5 | Informational (carried) | See policy audit section 8. |

## Summary

All fourteen acceptance criteria evaluate PASS. The review returns **PASS** with no blocking finding. Remediation cycle 1 fixed B-1 with one stylesheet selector that reuses the committed-subfolder colours in every theme, proved it with a C# string contract that failed before the fix, and resolved N-1 and N-2 with test-only changes. No C# production file changed in the cycle, the full suite passes, and repository coverage rose to 86.49% line and 80.85% branch.

Open items are outside this review's blocking scope: the PR-context artifacts should be regenerated before the PR is authored (O-1), CI runs at PR time (O-2), and the manual Outlook verification is a human step (O-3).

### Acceptance Criteria Status
- Source: `docs/features/active/2026-10-22-efc-arrow-keys-skip-subfolders-1007/spec.md`
- Total AC items: 14
- Checked off (delivered): 14
- Remaining (unchecked): 0
- Items remaining: none

## Acceptance Criteria Check-off

All fourteen items were already `- [x]` in `spec.md` and in the `issue.md` mirror when this review began; the cycle executor re-checked AC4 in commit `e804f5d6b` after its pass-after run, changing only the checkbox marker (reviewer read of the hunk). Every item evaluates PASS in this review, so every box stays checked and no box was newly checked or unchecked by the reviewer. No criterion text was edited.
