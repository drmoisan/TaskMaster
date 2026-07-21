# Feature Audit — quickfiler-folder-tree-percentage (#325)

- Timestamp: 2026-07-16T01-53
- Reviewer: feature-review
- Branch: `feature/quickfiler-folder-tree-percentage-325` @ `ae104f84` vs `epic/folder-tree-percentage-ui-integration` @ `34ed0422`
- Work mode: `full-feature` -> AC sources: `spec.md` (§ Acceptance Criteria, 9 items) and `user-story.md` (§ Acceptance Criteria, 6 items)
- Overall verdict: PASS
- blocking_count (this artifact): 0

## Scope and Baseline

Verified against the full branch diff relative to the merge-base `34ed0422`. The delivered contract shape is `FolderRow`/`FolderScore` (owned upstream by #324/9001), not the spec's placeholder `FolderSuggestion` struct. This is an authorized adaptation: spec.md § Consumed Upstream Contract explicitly states "The exact member name and return type are 9001's decision; #325 plans against 'folder identity plus its probability' and adapts to the concrete member at epic execution time." `FolderScore.FolderPath` supplies identity and `FolderScore.Probability` (a `[0,1]` relative-confidence value) supplies the percentage input, satisfying the planned-against shape.

## Acceptance Criteria Inventory

Spec.md (9):
1. Plus/minus expand affordance on folders with subfolders; leaves no glyph.
2. Clicking plus expands; clicking minus collapses.
3. Right arrow expands / Left arrow collapses highlighted node, with leaf/already-state no-ops.
4. Right-aligned whole-number percentage; no-probability rows render empty field.
5. Percentage consumed verbatim from 9001 contract; no scoring recompute.
6. Shared logic in host-neutral seams meeting coverage thresholds with INV1-INV8 tested exhaustively.
7. Change confined to runtime-live ItemViewer + IItemViewer; nine dead variants untouched.
8. Shares no files with 9004; body-render/WebView2 path not modified.
9. Full C# toolchain passes green in a single final pass; existing SetFolderItems expectations remain green.

User-story.md (6):
U1. Plus/minus affordance on runtime-live ItemViewer; leaves no glyph.
U2. Clicking plus expands; clicking minus collapses.
U3. Right expands / Left collapses highlighted node, with leaf/already-state no-ops.
U4. Right-aligned whole-number percentage; no-probability rows empty.
U5. Percentage consumes 9001 contract; no recompute.
U6. Shared logic in host-neutral seams meeting coverage thresholds.

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence |
|---|---|---|
| Spec 1 / U1 | PASS | `FolderNodeViewModel.Glyph` returns '+'/'-' only when `HasChildren` (INV4), null for leaves; `CboFolders_DrawItem` (ItemViewer.FolderSearch.cs:186) paints the glyph at the indented left of the name. Tested: `FolderNodeViewModelTests` glyph cases; `INV4_GlyphBijectionTracksExpansion`. |
| Spec 2 / U2 | PASS | `CboFolders_MouseDown` (ItemViewer.FolderSearch.cs:254) hit-tests the glyph rect for a parent row and calls `FolderTreeStateModel.Toggle`; `Toggle` expands/collapses (INV6). Tested: `INV6_ToggleIsInvolutionOnParent`, `INV1_ExpandOrToggleLeaf_IsNoOp`. |
| Spec 3 / U3 | PASS | `KeyboardHandler.cs` Right/Left cases route to `ItemViewer.FolderTreeRightArrow/LeftArrow`, which invoke `FolderTreeStateModel.RightArrow/LeftArrow` and fall through on no-op. Tested: `RightArrow_*`, `LeftArrow_*`, `RightArrow_OnLeafOrAlreadyExpanded_IsNoOp`, `LeftArrow_OnLeafOrAlreadyCollapsed_IsNoOp`, `Arrows_WithNoHighlight_AreNoOp`. |
| Spec 4 / U4 | PASS | `PercentageFormatter.Format` produces whole-number percent; `FolderNodeViewModel.FormattedPercentage` returns empty string when `Probability` is null; `DrawItem` paints it with `TextFormatFlags.Right` into a fixed right column. Builder assigns null probability to ancestors/separators/recents. Tested: `PercentageFormatterTests` (43%/100%/0%/midpoint/clamp), `FormattedPercentage_NullProbability_IsEmpty`, `Build_NonSuggestionRows_AreDepthZeroLeavesWithNoProbability`. |
| Spec 5 / U5 | PASS | `FolderHierarchyBuilder.AddSuggestion` reads `score.Probability` verbatim; `QfcItemController.FolderHandling.cs` passes `_folderHandler.FolderRowArray` without recompute; no change to `FolderScorer`/`FolderPredictor` scoring math in the diff. Tested: `Build_*` probability attach; `AssignFolderComboBox_HandsPredictorRowArrayToSetFolderSuggestions` asserts Probability 0.9 preserved. |
| Spec 6 / U6 | PASS (with one non-blocking test-gap note) | Four host-neutral seams present and in the coverage denominator; per-seam coverage exceeds thresholds (100/100, 100/100, 96.55/94.44, 100/91.18 line/branch — all line >= 90%, branch >= 75%). INV1-INV8 exhaustively tested (13 state-model tests + view-model glyph tests). Non-blocking: INV8's equal-score ordinal tie-break sub-clause is realized by preserving upstream input order, not an in-seam sort, and is not independently tested (see code-review CR-3). |
| Spec 7 | PASS | Only `ItemViewer` + its Designer partial and `IItemViewer` changed. `git diff --name-status` shows none of the nine dead variants (Form1, ItemViewerExpanded, QfcItemViewer, QFCItemViewerDarkNew, QfcItemViewerExpanded, QfcItemViewerExpandedLight, QFCItemViewerLightNew, QfcItemViewerLightSelected, QfcItemViewerV1) modified. `evidence/qa-gates/non-interference-9004`. |
| Spec 8 | PASS | Diff touches none of MailItemHelper.Html.cs, ItemViewer.WebViewThread.cs, WebView2CoreInitializer.cs, IWebViewCoreInitializer.cs. IItemViewer NavigateToString/WebViewInitializationCompleted members unchanged (2 present in base and head). Designer edit confined to the CboFolders block. Verified independently via `git diff --name-status` and grep. |
| Spec 9 | PASS (with documented nullable posture) | csharpier EXIT 0; analyzers EXIT 0 (0 errors); nullable/TWAE `/t:Build` EXIT 0 (0/0); 4760/4760 tests pass uninstrumented. Existing `SetFolderItems(string[])` expectations retained and re-verified by `AssignFolderComboBox_RetainsSetFolderItemsAndIndexOneSelection` and `MarkItemForDeletion_StillAppendsTrashToDeleteViaSetFolderItems`. The `/t:Rebuild` nullable observation is non-blocking (policy-audit Nullable-Posture Ruling). |

## Acceptance Criteria Check-off

All 15 AC checkboxes across `spec.md` and `user-story.md` are marked `[x]` by the executor. Independent verification above confirms each item is delivered and evidenced; every item evaluates to PASS. The checked state is confirmed correct and left as-is. No item required reverting to unchecked. No phantom criteria were added.

## Acceptance Criteria Status

- Source: `docs/features/active/2026-07-15-quickfiler-folder-tree-percentage-325/spec.md`; `docs/features/active/2026-07-15-quickfiler-folder-tree-percentage-325/user-story.md`
- Total AC items: 15 (9 spec + 6 user-story)
- Checked off (delivered): 15
- Remaining (unchecked): 0
- Items remaining: none

## Summary

Every acceptance criterion in both AC sources is delivered and independently verified against the committed code and the feature evidence. The `FolderRow`/`FolderScore` contract used at integration time is the authorized adaptation of the spec's `FolderSuggestion` placeholder. One non-blocking test-coverage nuance (INV8 equal-score tie-break) and the documented nullable posture do not affect any AC verdict. Feature audit verdict: PASS. blocking_count: 0.
