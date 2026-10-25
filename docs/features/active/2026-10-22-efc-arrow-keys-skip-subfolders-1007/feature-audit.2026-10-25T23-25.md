# Feature Audit: EFC and QFC arrow keys skip expanded subfolders (Issue #1007)

- Review timestamp: 2026-10-25T23-25 (first review of this branch)
- Reviewer: feature-review agent

## Scope and Baseline

- Branch: `bug/efc-arrow-keys-skip-subfolders-1007`, head `8bdd970ee` (merge of `origin/main`; the #1007 execution commit is `8082ef3b2`).
- Resolved base branch: `main`, merge base `3b9a1ebdd` (the current `origin/main`). The audit diff is `git diff 3b9a1ebdd..HEAD` (116 files); the merge brought in #1005 only, which the merge base already contains, so no #1005 file is in the diff.
- Work mode: `full-bug` (`- Work Mode: full-bug` in `issue.md`); AC source: `spec.md` `## Acceptance Criteria` only (AC1 to AC14, in the order AC1 to AC11, AC14, AC12, AC13). `issue.md` carries a mirror of the same criteria. Decision record: `grill-transcript.md` (D-Q1 to D-Q29) and the spec's S1 to S10.
- PR context: `artifacts/pr_context.summary.txt` and `.appendix.txt` are absent from this worktree and the PR-context MCP route is not available to this agent; scope was derived from `git diff`.
- Evidence basis: executor projections under `evidence/{baseline,regression-testing,qa-gates,other}/`, the canonical coverage artifact `artifacts/csharp/coverage.xml`, the post-processed Cobertura documents of the baseline and final runs (recomputed by the reviewer), a reviewer re-run of the node suites, and reviewer reads of source and diff.
- Manual Outlook verification (spec Test Strategy) is a human step recorded by the orchestrator; it is not evaluated here.

## Acceptance Criteria Inventory

Source: `docs/features/active/2026-10-22-efc-arrow-keys-skip-subfolders-1007/spec.md`, `## Acceptance Criteria`. Fourteen checkbox items, all checked `[x]` when this review began.

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
| AC1 | PASS | `BreadcrumbVisibleOrder.AppendNodes` adds a node's children only while it is expanded. EFC: `ArrowDown_FromExpandedRow_SelectsFirstVisibleDescendant`, `ArrowDown_SkipsChildrenOfCollapsedNode`, `ArrowUp_FromRowBelowExpandedBlock_SelectsLastVisibleDescendant`; QFC: `SelectorKeyDown_Open_MovesPendingThroughVisibleDescendants`; pure helper: `BreadcrumbVisibleOrderTests`. Fail-before and pass-after in `visible-order-*`, `efc-vertical-*`, `qfc-open-*`. The traversal order holds in both viewers; the QFC rendering of a pending descendant is evaluated under AC4. |
| AC2 | PASS | `BreadcrumbVisibleOrder.Step` clamps and returns -1 at either end; EFC `HandleVerticalKey` posts `BreadcrumbFocusSearchMessage` and raises `FocusSearchRequested` only for Up with no previous entry. Tests `ArrowDown_OnLastVisibleEntry_PostsNothing`, `ArrowUp_OnFirstEntry_RaisesFocusSearchRequested`, `SelectorKeyDown_Open_OnLastEntry_KeepsPending`. |
| AC3 | PASS | `CommitPosition` commits a node through `SelectHierarchyPath(row, path, nodeId)` and a row entry through `SelectRowEntry`, the click paths. `ArrowDown_OntoDescendant_CommitsArchiveRelativePathAndRaisesChanged`, `ArrowDown_OntoDescendant_ThenRowConfirmed_RaisesConfirmationForDescendant`. |
| AC4 | PARTIAL | Behaviour: open-list moves change only `PendingIdentity` and `PendingNode` (`OpenMove_DoesNotChangeCommittedValue`), Enter commits the pending node (`CommitPending_WithPendingNode_SelectsDescendant`), Escape restores (`Cancel_RestoresOriginalAndClearsPendingNode`), Left on a non-expanded entry commits and closes (`OpenLeft_OnNonExpandedEntry_CommitsAndCloses`). Gap (B-1): when the pending entry is a descendant that is not the committed subfolder, `FolderBreadcrumb.js` renders it as `class="sub active"` and removes the row's `active` class, and `FolderBreadcrumb.html` has no `.sub.active` rule, so the pending highlight is not visible. The criterion's "pending highlight" is not observable to the user in that state. |
| AC5 | PASS | `HandleRightAsync` (EFC) and `PendingArrowAsync` (QFC) follow the decision table. `ArrowRight_OnUnexpandedRowWithChildren_Expands`, `ArrowRight_OnExpandedRow_SelectsFirstDescendantWithNodeId`, `ArrowRight_OnShortenedRow_ListsSegmentChildrenWithSingleQuery`, `ArrowRight_ReentrantDuringFetch_EndsExpandedWithOneQuery` (held on a `TaskCompletionSource`; one provider query, row ends expanded), `OpenRight_OnExpandedPendingEntry_PendsFirstChild`. |
| AC6 | PASS | EFC Right on a position without children returns. QFC: `ClassifyPendingArrow` returns `CancelAndPopOut`; the coordinator calls `CancelSelector()` and then raises `UnhandledArrow(Right)`. `ArrowRight_OnLeaf_PostsNothing`, `OpenRight_OnLeaf_CancelsThenRaisesUnhandledArrowRight` (fail-before in `qfc-open-fail-before.2026-10-25T22-08.md`). |
| AC7 | PASS | `BreadcrumbRow.LeftArrow` and `BreadcrumbStateModel.LeftArrow` only collapse; `TryRightTreeTransition` and the EFC walk are deleted. `CollapseNode` and `CollapseRowLeaf` re-commit only when the collapse hides the selection; `ReconcilePendingNode` moves a hidden pending node to its deepest visible ancestor. `ArrowLeft_OnExpandedNode_CollapsesOnlyThatNode`, `ArrowLeft_OnNonExpandedRow_IsNoOp`, `LeftArrow_NeverHidesTrailingSegment`, `CollapseHidingSelection_RecommitsNearestVisibleAncestor`, `CollapseHidingPending_MovesPendingToNearestVisibleAncestor`. |
| AC8 | PASS | EFC `HandleParentLeft` (node: commit parent keeping expansion; row: `ShortenAt(active - 1)`; index 0: no-op). QFC `MoveToParent` (open: pending only; closed: `CommitParent`) and `RestoreOpenShape` on Cancel. `BridgeJs` maps Ctrl+ArrowLeft to `ParentLeft` and Ctrl+ArrowRight to `Right`; `FolderBreadcrumb.js` posts `parent`. Tests named in `ac-reconciliation.2026-10-25T23-14.md` row AC8. |
| AC9 | PASS | EFC `ShortenAt` is atomic; `[+]` on an anchored collapse expands the active segment. QFC `DoubleClickSegment` activates, shortens, commits and closes; `SelectedRowValue` files to the active segment (S9). The non-leaf single click posts `selectorHighlight` and stops propagation; the row-level pending highlight uses the styled `.row.active`. Tests `SegmentDoubleClick_ActivatesCommitsAndShortensAtomically`, `LeafExpandToggle_OnShortenedRow_ListsSegmentChildren`, `ShortenedRowExpanded_NextRight_SelectsFirstChild`, `SelectorSegmentDoubleClick_CommitsSegmentFolderAndCloses`, `GetSelectedFolder_ShortenedRow_ReturnsActiveSegmentValue`, node "non-leaf segment click". |
| AC10 | PASS | `TryDispatchOpenArrowAsync`: closed Right opens the selector, closed Left is consumed; `BeginOpenTree` pends the committed subfolder when visible (D-Q29); closed `Move` is unchanged; `CommitSelectorBoundaryRow` commits the first and last top-level row; `MovePage` returns false when closed. Seven G6 tests (`qfc-closed-fail-before` and `-pass-after`). |
| AC11 | PASS | EFC `HandleVerticalKey` and `HandleNoSelectionKey`; QFC `MoveToBoundary` and `MovePage`; `TryPageDelta` rejects a missing or non-positive page size; both page scripts measure fully visible rows with a minimum of 1. `Home_SelectsFirstEntryWithoutFocusSearch`, `End_SelectsLastVisibleDescendant`, `PageDown_MovesByPageSizeAndClamps`, `PageKey_InvalidPageSize_IsLoggedNoOp`, `NoSelection_DownAndHomeSelectFirst_UpAndEndSelectLast`, node `fullyVisibleCount`. |
| AC14 | PASS | `MapNode` derives `HasChildren` from `snapshot.GetChildren(node.Key).Count > 0` for both provider entry points; defences in `BreadcrumbHtmlRenderer` (leaf affordance and descendant toggle), `BreadcrumbRenderProjection` (leaf and descendant) and `makeSubfolders`. `MapNode_UnresolvableChildKey_HasChildrenFalse`, `GetImmediateSubfolders_ChildWithOnlyUnresolvableKeys_HasChildrenFalse`, `GetAncestorChain_ResolvableChildKey_HasChildrenTrue`, `BreadcrumbEmptyExpanderIssue1007Tests` (5), C# string-contract test `MakeSubfolders_LoadedChildWithoutChildren_RendersNoToggle` and its node twin. Fail-before `ac14-fail-before.2026-10-23T13-51.md`. Observation N-5 (a collapse after a zero-children fetch re-shows `+` in two shortened or QFC states) is defence-in-depth only and does not contradict the criterion while the source fix holds. |
| AC12 | PASS | Eight fail-before and eight pass-after projections. The single exception, `fail-before-exception.2026-10-25T22-23.md` for `UnhandledRightArrow_RaisesUnhandledArrowRight`, is valid: the update adds only an `OpenSelector()` precondition to a behaviour that holds in every code state, and the AC6 behaviour has its own failing evidence. Rewritten #440 and `[+]` restore tests match the spec disposition table and each summary records #1007 and the governing AC (names are code-review N-2). |
| AC13 | PASS | One clean pass in CLAUDE.md order (`final-toolchain-format.2026-10-25T23-07.md`, `-analyzers.2026-10-25T23-07.md`, `-typecheck.2026-10-25T23-07.md`, `-tests.2026-10-25T23-09.md`: 7734 of 7734). Node suites 90 of 90 in five orders, re-run by the reviewer. Repository 86.40% line and 80.73% branch; new files 91.84% to 100% line; every changed line at or above its replaced-line baseline (reviewer recomputation). Largest changed code file 500 lines. |

## Summary

Thirteen of fourteen acceptance criteria evaluate PASS; AC4 evaluates PARTIAL. The review returns **REMEDIATION_REQUIRED** with one blocking finding, B-1 (autonomous): the QFC page has no stylesheet rule for a pending descendant (`.sub.active`), so in the open QFC list a pending highlight on a subfolder other than the committed one is not visible. The keyboard state machine behind it is correct and tested. The remedy is a one-rule stylesheet change in `QuickFiler/Resources/FolderBreadcrumb.html` with a C# string-contract assertion, followed by a full toolchain pass.

The removed #440 behaviour is removed consistently in both viewers, and each test that pinned it is rewritten with the reason recorded. The AC14 fix sits at the single place where the has-children flag is derived. The fail-before exception dossier is valid.

### Acceptance Criteria Status
- Source: `docs/features/active/2026-10-22-efc-arrow-keys-skip-subfolders-1007/spec.md`
- Total AC items: 14
- Checked off (delivered): 13
- Remaining (unchecked): 1
- Items remaining: AC4 (QFC open-list pending model): with the QFC folder list open, arrows move only the pending highlight. Enter commits, Escape restores the original choice, and Left on a highlighted item that is not expanded commits it and closes the list. (Q7, Q15)

## Acceptance Criteria Check-off

All fourteen items were `- [x]` in `spec.md` when this review began. Thirteen evaluate PASS and stay checked; no box was newly checked. AC4 evaluates PARTIAL, so its box was changed from `- [x]` to `- [ ]` in `spec.md` and in the `issue.md` mirror, with the criterion text unchanged, per the acceptance-criteria-tracking rule that PARTIAL items stay unchecked. The remediation executor checks AC4 off again once B-1 is fixed and verified.
