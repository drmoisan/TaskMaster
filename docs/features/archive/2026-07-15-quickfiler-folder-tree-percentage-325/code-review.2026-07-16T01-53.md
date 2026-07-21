# Code Review — quickfiler-folder-tree-percentage (#325)

- Timestamp: 2026-07-16T01-53
- Reviewer: feature-review
- Branch: `feature/quickfiler-folder-tree-percentage-325` @ `ae104f84` vs `epic/folder-tree-percentage-ui-integration` @ `34ed0422`
- Overall verdict: PASS
- blocking_count (this artifact): 0

## Executive Summary

The change delivers four small, pure, host-neutral C# seams (`PercentageFormatter`, `FolderNodeViewModel`, `FolderHierarchyBuilder`, `FolderTreeStateModel`) that own all tree/percentage correctness, with thin WinForms owner-draw glue confined to the exempt `ItemViewer.FolderSearch.cs` partial and Designer. The separation-of-concerns design matches the spec's Host-Neutral Seam Architecture: the ComboBox is a dumb renderer and the state model is fully unit-tested. Naming, XML documentation, error handling, and reuse of the existing `TreeNode<T>` are consistent with repository conventions. Formatter clamp/round logic, hierarchy find-or-add synthesis, and state-model INV enforcement are implemented as described in the spec and are directly readable. Three non-blocking findings are recorded below; none blocks merge.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Low (non-blocking) | UtilitiesCS/OutlookObjects/Folder/FolderTreeStateModel.cs; FolderHierarchyBuilder.cs | `_highlighted` field; `cumulative` local | Under a full `/t:Rebuild /p:Nullable=enable` the new files emit CS8618/CS8600/CS8625; the specified `/t:Build` gate passes 0/0. Not real defects (`_highlighted` intentionally null until `Highlight()`; `cumulative` always assigned before use since `Split` yields >=1 segment). | Leave as-is to stay consistent with the repo's nullable-disabled convention and sibling #324 types; open a separate maintainer-owned repo-wide `#nullable enable` migration issue. Adding `?` here would emit CS8632 in the default build. | Repo convention is nullable-disabled; the mandated gate command is `/t:Build`, which is clean. | `evidence/qa-gates/final-nullable`; CLAUDE.md C#1.3 |
| Low/Medium (non-blocking) | QuickFiler/Controllers/KeyboardHandler.cs | whole file | File is 631 lines at head, exceeding the 500-line limit. It was already 604 lines at the base (pre-existing violation); #325 added 27 lines of arrow-routing glue. | Track a follow-up refactor to split `KeyboardHandler` (extract the ComboBox key-switch). Do not block #325: the file was non-compliant before this feature and the added glue is minimal, host-bound, and `[ExcludeFromCodeCoverage]`. | `.claude/rules/general-code-change.md` File Size Limit; violation is pre-existing, not introduced. | `git show <base>:...KeyboardHandler.cs` = 604 lines; head = 631 lines |
| Low (non-blocking) | UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeStateModelTests.cs; FolderHierarchyBuilderTests.cs | INV8 coverage | INV8's stable pre-order DFS and determinism are tested, and descending-score order preservation is demonstrated (`Build_SiblingSuggestions` asserts 0.9 before 0.8). The equal-score ordinal-key tie-break sub-clause of INV8 is not independently exercised, because the seam preserves the predictor's input order rather than re-sorting. | Add a builder/state-model test that feeds equal-score sibling rows and asserts the deterministic ordinal ordering, or amend the spec to state that sibling ordering is inherited from the upstream (#324/9001) row order. | Ordering originates upstream in `FolderPredictor.FolderRowArray` (out of #325 scope); the seam's job is stable preservation, which is tested. | spec.md INV8; FolderHierarchyBuilder.cs `AddSuggestion` (input-order find-or-add, no sort) |

## Detailed Observations (by area)

### Host-neutral seams (in coverage denominator)

- `PercentageFormatter.Format` (PercentageFormatter.cs:22): explicit `[0,1]` clamp (net48-safe, no `Math.Clamp`), `MidpointRounding.AwayFromZero`, matches spec examples (0.4267 -> 43%, 1.0 -> 100%, 0.0 -> 0%). Tests cover typical, boundaries, midpoint, and out-of-range clamps. Clean.
- `FolderNodeViewModel` (FolderNodeViewModel.cs): plain net48-safe class; `Glyph` and `FormattedPercentage` are derived (INV4 + empty-percentage rule). Correct.
- `FolderHierarchyBuilder.Build/AddSuggestion`: find-or-add ancestor synthesis on `\`, leaf-only probability attach, non-suggestion rows as depth-0 leaves preserving text/order, full path retained as key. Matches spec. One defensive `rows == null` guard line is the single uncovered line (96.55% line / 94.44% branch), acceptable.
- `FolderTreeStateModel`: Expand/Collapse/Toggle/Highlight/RightArrow/LeftArrow and stable pre-order-DFS projection; INV1-INV8 enforced with a single-reference highlight (INV3) and descendant-state-preserving collapse (INV5). Guard sub-branches in the arrow no-op conditions account for 91.18% branch (>= 75% floor). Clean.

### WinForms glue (exempt, kept minimal)

- `ItemViewer.FolderSearch.cs`: `SetFolderSuggestions` builds the forest, rebinds visible rows; `CboFolders_DrawItem` paints indent + glyph (left) + name + right-aligned percentage; `CboFolders_MouseDown` hit-tests the glyph column and delegates the toggle to the state model; `GetSelectedFolder` maps a `FolderNodeViewModel` back to its full path and preserves the legacy string path for `SetFolderItems`. Glue is thin and delegates all correctness to the tested seams, as the spec requires.
- `KeyboardHandler.cs`: Right/Left now route to `FolderTreeRightArrow/LeftArrow` and fall through to legacy Pop-Out/close behavior on a no-op. The transition logic itself is in the tested state model. Design is sound (see CR-2 for the file-size note).
- `ItemViewer.Designer.cs`: `DrawMode.OwnerDrawFixed` + `DrawItem`/`MouseDown` wiring confined to the `_cboFolders` block; no WebView2 member touched.

### Controller injection

- `QfcItemController.FolderHandling.cs`: additionally calls `SetFolderSuggestions(_folderHandler.FolderRowArray)` guarded by a `Suggestions != null` check, while retaining the existing `SetFolderItems(FolderArray)` population and selection logic. Scores are consumed verbatim; no recompute. Controller-injection tests verify both the new call and the retained call sites (index-1 selection, predetermined preselection, "Trash to Delete" append).

## Test Quality

- MSTest + Moq + FluentAssertions throughout, per CUT1/CUT2. AAA structure with reason strings.
- Determinism: no `Thread.Sleep`/wall-clock; INV8 test asserts repeat-projection stability.
- Isolation: `Mock<IItemViewer>`, in-memory `FolderRow[]`, `FolderScorer()` default; private-field injection via reflection avoids COM/live Outlook. No temp files. No live WinForms form or BackgroundWorker instantiated.

## Summary

The implementation is clean, well-documented, and faithful to the spec's seam architecture. All correctness lives in tested host-neutral seams; glue is thin and exempt. Three non-blocking findings (nullable rebuild posture, pre-existing `KeyboardHandler` file size, INV8 tie-break test gap) are recommendations, not defects. Code review verdict: PASS. blocking_count: 0.
