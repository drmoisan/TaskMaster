# Acceptance Criteria Reconciliation (Issue #1007, P19-T22)

Timestamp: 2026-10-25T23-14
Command: git diff --numstat HEAD and git diff -U0 HEAD on `<FEATURE>/spec.md` and `<FEATURE>/issue.md`; count of checked and unchecked boxes under `## Acceptance Criteria`
EXIT_CODE: 0
Output Summary: `spec.md` `## Acceptance Criteria`: 14 checked, 0 unchecked. `git diff --numstat HEAD` reports 14 lines added and 14 removed in `spec.md`, and the same in `issue.md`. All 14 added lines are `- [x] AC` lines, so only the checkbox markers changed; once the marker is normalized, every criterion text is identical to `spec.md` at HEAD. Each criterion was checked individually (P19-T8 to P19-T21, one helper call per criterion), and each check-off was mirrored into `issue.md` `## Acceptance Criteria`.

### Acceptance Criteria Status
- Source: `docs/features/active/2026-10-22-efc-arrow-keys-skip-subfolders-1007/spec.md` (work mode full-bug: spec.md only), mirrored in `issue.md`
- Total AC items: 14
- Checked off (delivered): 14
- Remaining (unchecked): 0
- Items remaining: none

| AC | Task | Evidence cited (tests Passed in final-toolchain-tests.2026-10-25T23-09.md) |
|---|---|---|
| AC1 | P19-T8 | Step_DownFromExpandedRow_VisitsDescendantsInPreOrder, ArrowUp_FromRowBelowExpandedBlock_SelectsLastVisibleDescendant, SelectorKeyDown_Open_MovesPendingThroughVisibleDescendants (visible-order-pass-after, efc-vertical-pass-after, qfc-open-pass-after) |
| AC2 | P19-T9 | ArrowDown_OnLastVisibleEntry_PostsNothing, ArrowUp_OnFirstEntry_RaisesFocusSearchRequested, SelectorKeyDown_Open_OnLastEntry_KeepsPending |
| AC3 | P19-T10 | ArrowDown_OntoDescendant_CommitsArchiveRelativePathAndRaisesChanged, ArrowDown_OntoDescendant_ThenRowConfirmed_RaisesConfirmationForDescendant |
| AC4 | P19-T11 | OpenMove_DoesNotChangeCommittedValue, CommitPending_WithPendingNode_SelectsDescendant, Cancel_RestoresOriginalAndClearsPendingNode, OpenLeft_OnNonExpandedEntry_CommitsAndCloses |
| AC5 | P19-T12 | ArrowRight_OnExpandedRow_SelectsFirstDescendantWithNodeId, ArrowRight_OnShortenedRow_ListsSegmentChildrenWithSingleQuery, ArrowRight_ReentrantDuringFetch_EndsExpandedWithOneQuery, OpenRight_OnExpandedPendingEntry_PendsFirstChild |
| AC6 | P19-T13 | ArrowRight_OnLeaf_PostsNothing, OpenRight_OnLeaf_CancelsThenRaisesUnhandledArrowRight |
| AC7 | P19-T14 | ArrowLeft_OnExpandedNode_CollapsesOnlyThatNode, ArrowLeft_OnNonExpandedRow_IsNoOp, LeftArrow_NeverHidesTrailingSegment, CollapseHidingSelection_RecommitsNearestVisibleAncestor, CollapseHidingPending_MovesPendingToNearestVisibleAncestor |
| AC8 | P19-T15 | ParentLeft_OnRow_ActivatesParentSegmentCommitsAndShortens, ParentLeft_AtFirstSegment_IsNoOp, ParentLeft_OnNode_SelectsParentKeepingExpansion, SelectorKeyParent_Open_ThenEscape_RestoresRowShapeAndValue, SelectorKeyParent_Closed_CommitsImmediately, BridgeJs_CtrlArrowRight_PostsRight |
| AC9 | P19-T16 | SegmentDoubleClick_ActivatesCommitsAndShortensAtomically, LeafExpandToggle_OnShortenedRow_ListsSegmentChildren, SelectorSegmentDoubleClick_CommitsSegmentFolderAndCloses, GetSelectedFolder_ShortenedRow_ReturnsActiveSegmentValue, NonLeafSegmentClick_OpenList_StopsPropagationAndPostsSelectorHighlight |
| AC10 | P19-T17 | the seven G6 tests (qfc-closed-pass-after) |
| AC11 | P19-T18 | Home_SelectsFirstEntryWithoutFocusSearch, PageDown_MovesByPageSizeAndClamps, NoSelection_DownAndHomeSelectFirst_UpAndEndSelectLast, SelectorKeyPageDown_Open_MovesPendingByPageSize, node "fullyVisibleCount counts only fully visible rows with a minimum of one" (node-suites-run.2026-10-25T22-50.md) |
| AC14 | P19-T19 | the G1 tests (ac14-pass-after) and MakeSubfolders_LoadedChildWithoutChildren_RendersNoToggle (page-scripts-pass-after) |
| AC12 | P19-T20 | the eight fail-before artifacts and the eight pass-after artifacts under `<FEATURE>/evidence/regression-testing/`, with fail-before-exception.2026-10-25T22-23.md for the one P12-T2 updated test that cannot fail; the Disposition Register edits verified in test-policy-audit.2026-10-25T23-12.md |
| AC13 | P19-T21 | final-toolchain-format.2026-10-25T23-07.md, final-toolchain-analyzers.2026-10-25T23-07.md, final-toolchain-typecheck.2026-10-25T23-07.md and final-toolchain-tests.2026-10-25T23-09.md (one clean pass); coverage-comparison.2026-10-25T23-09.md (PASS, no shortfall); node-suites-run.2026-10-25T22-50.md (5 orders, 0 failures); file-sizes.2026-10-25T23-12.md (every file at most 500) |
