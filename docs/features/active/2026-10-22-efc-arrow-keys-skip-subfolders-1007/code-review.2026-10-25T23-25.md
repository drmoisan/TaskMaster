# Code Review: EFC and QFC arrow keys skip expanded subfolders (Issue #1007)

- Branch: `bug/efc-arrow-keys-skip-subfolders-1007`
- Head: `8bdd970ee`; merge base `3b9a1ebdd` (`main`, current `origin/main`)
- Review timestamp: 2026-10-25T23-25
- Reviewer: feature-review agent
- Scope: the full branch diff `3b9a1ebdd..HEAD` (116 files: 25 production C# files, 1 page script, 25 test C# files, 3 node test files, 4 project files, feature documents and evidence, 1 promotion record)

## Executive Summary

The change is well structured for its size. One pure helper (`BreadcrumbVisibleOrder`) gives both viewers the same pre-order traversal, and new behaviour lives in new partials, so no file breaches the 500-line ceiling. The EFC router keeps its position as the existing pair of selected row and selected descendant, and every keyboard commit goes through `SelectHierarchyPath` or `SelectRowEntry`, so the #614 archive-root contract is preserved. The QFC session adds a pending descendant node whose setter coupling (`PendingIdentity` clears `PendingNode`) prevents a stale node after a row-level move. The re-entrant Right defect is closed with an explicit in-flight flag and an idempotent `ExpandLeaf`, and AC14 is fixed where the flag is derived (`MapNode` now counts `snapshot.GetChildren`). The removed #440 behaviour is removed consistently in both viewers, and each rewritten test records the reason.

One blocking finding: the QFC page renders a pending, non-committed descendant as `class="sub active"`, but the stylesheet has no `.sub.active` rule, and the row's own `active` class is suppressed while a pending node owns the highlight. Arrowing into a subfolder in the open QFC list therefore leaves no visible highlight. The remaining findings are minor or informational.

Verdict: **REMEDIATION_REQUIRED (1 blocking, 2 minor, 5 informational)**.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Blocking | `QuickFiler/Resources/FolderBreadcrumb.html`; `QuickFiler/Resources/FolderBreadcrumb.js` | stylesheet lines 109 to 112 and 158; `makeSubfolders` line 169; `makeRow` lines 122 to 127; `render` lines 243 to 246 | B-1: a pending descendant that is not the committed subfolder gets `class="sub active"`, which no CSS rule styles (only `.row.selected`, `.row.active` and `.sub.selected` exist). `render` passes `pendingNodeOwnsActive` into `makeRow`, which then omits the row's `active` class. The open QFC list shows no highlight after Down or Right moves onto a subfolder, which is the second reproduction in `spec.md`. | Add a `.sub.active` rule (for example extend `.sub.selected` to `.sub.selected, .sub.active`, or a distinct pending style with the same contrast in both themes) and a C# string-contract assertion over the QFC document HTML that the rule exists; optionally a node assertion that the active descendant's computed class is styled. | AC4 requires that arrows move the pending highlight; a highlight with no styling is not visible to the user. The #1002 review raised the same defect class (selection state without a matching CSS rule) as blocking. | Grep of `FolderBreadcrumb.html` for `.sub.active`, `.active`, `aria-selected`: only `.row.active`; `issue-1007-keys.test.mjs` "pending node carries sub active" and `FolderBreadcrumbKeysIssue1007Tests` assert the class string only. |
| Minor | `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs`; `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Descendants.cs`; `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`; `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs`; `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` | `HandleLeafToggleAsync` lines 23 to 31; `ToggleAsync` non-anchored branch; closed `ArrowAsync`; `PublishRouterOutputs` and `RaiseSyntheticArrowKey` unhandled arms; inbound `UnhandledArrowMessage`; `Expand` non-anchored branch | N-1: 27 unchanged instrumented lines lost their only test execution, mainly the non-anchored `ReExpand` paths (reachable when a shortened segment has no provider key) and the legacy `unhandledArrow` route. Every file stays above 80% line and 75% branch and no changed line regressed, but these paths are now untested. | Add direct tests: an EFC `leafExpandToggle` on a row collapsed away from its active segment; a QFC `affordanceToggle` on a non-anchored collapsed row; a coordinator test that a router `unhandledArrow` output raises `UnhandledArrow`. Delete any branch confirmed unreachable instead of testing it. | Coverage is a supporting gate; defensive paths that keep behaviour should keep a test. | Reviewer mapping of baseline hits through `git diff -U0` hunks (policy audit section 5). |
| Minor | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs`; `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs` | `HandleArrowKey_LeftOnMultiSegmentRow_ActivatesParentSegment`, `HandleArrowKey_RepeatedLeft_WalksToRootThenFallsThroughToExistingBehavior`, `HandleArrowKey_RightAfterExpansion_DescendsByChildActivation`, `RightArrow_OnCollapsedRow_ReExpandsBeforeLeafExpansion`, `LeftArrow_RepeatedOnThreeSegmentChain_WalksToRootThenReportsUnhandled`, `LeftArrow_WalkFromAnOpenLeafExpansion_ClearsTheExpansionAndStillReachesTheRoot` | N-2: the rewritten tests assert the #1007 behaviour but keep the #440 names, which now state the opposite (for example a test named `..._ActivatesParentSegment` asserts that Left does not activate the parent). | Rename each test to its asserted behaviour (for example `HandleArrowKey_LeftOnNonExpandedMultiSegmentRow_IsNoOp`) and keep the #1007 summary; record the rename in the disposition evidence. | UT3 requires names that communicate intent; a contradicting name misleads a reader triaging a failure. | Diff of both files. |
| Informational | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs`; `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs` | `AppendSuggestionRow` (`!(anchoredOpen && row.HasNoLoadedChildren)`); `ProjectRow` (`zeroLoaded` requires `LeafExpanded`) | N-5: after a fetch that returned zero children, a Left collapse re-shows `+` on an EFC row shortened at its active segment and on a QFC row, because both defences apply only while expanded (the EFC leaf affordance for a non-shortened row is suppressed in both states). | Consider suppressing on `HasNoLoadedChildren` regardless of expansion in the EFC shortened-terminal branch, and on a loaded-empty `DescendantRoot` in the QFC projection; or accept, because the `MapNode` source fix makes the state reachable only if the snapshot changes between the chain and the child fetch. | AC14 defence in depth; the primary fix holds. | Source read. |
| Informational | `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Navigation.cs` | whole file | N-3: branch coverage 78.33% (47 of 60); line 93.44%. Uncovered: `MoveSelectorToParent` no-op return, `PendingArrowAsync` null-entry and refused-expansion returns. | Add tests for the null-entry and refused-expansion returns if the remediation touches this file. | CLAUDE.md asks new modules to target 90%; the line figure meets it. | Reviewer Cobertura recomputation. |
| Informational | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs` | `PendingIdentity` property, `CommitPending` | N-4: the file grew from 446 to 492 lines although the spec allowed call-throughs only; the added logic (the `PendingIdentity` setter coupling and the three-way commit branch) is small and coherent. | Accept; keep further session logic in the Tree partial. | 500-line ceiling still met. | `awk` line count. |
| Informational | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.Tree.cs` | `MoveToParent` open-list row branch | O-4: an open-list Ctrl+Left on a row entry calls `ActivateSegment` and `CollapseAfter` on the model row before Enter. Escape restores the shape (D-Q24), but if the session ends through another commit (for example Down to a different row, then Enter), the shortened shape of the first row persists, and selecting it later files to the displayed parent folder. | Accept; the displayed and filed folders agree. Document in the PR body as part of the AC8 behaviour. | Consistent with S9 (`SelectedRowValue` files to the active segment). | Source read. |
| Informational | `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs` | `ExpandLeafAsync` | O-5: the new in-flight flag closes the re-entrant Right defect; the pre-existing race in which a segment activation during an in-flight fetch receives the previous segment's children is unchanged (the row-instance guard covers rebinds only). | Out of scope; consider a generation check keyed on `ActiveSegmentIndex` in a later item. | Pre-existing and not reachable by keyboard alone within one fetch. | Source read. |

## Design and Correctness Review

### Shared visible order

- `BreadcrumbVisibleOrder` is pure and fully covered (68 of 68 lines, 30 of 30 branches). `Step` clamps in `long` arithmetic, so a large page size cannot overflow, and returns -1 for "no move", which callers use for the no-wrap rule (D-Q5) and for the EFC Up-to-search-box hand-off.
- `NodeHasChildren` applies the AC14 zero-children rule once for both viewers; `AppendVisiblePositions` (EFC) and `VisiblePositions` (QFC) apply the same rule at row level.

### EFC router

- `PositionOf` falls back to the row entry when the selected descendant is not visible, so a key press after a collapse still has a defined position.
- Right and Left follow the spec decision table exactly; an expanded node whose fetch is in flight has no child in the order, so Right does nothing (verified by `ArrowRight_OnExpandedNodeWithFetchInFlight_DoesNothing`).
- `CollapseRowLeaf` and `CollapseNode` implement S1: the re-commit happens only when the collapse hides the selection (`IsSelectedDescendantWithin` excludes the node itself), so collapsing the selected node keeps the highlight on it (D-Q11).
- `ShortenAt` is atomic (activate, commit, shorten, one render) and keeps the pre-#1007 collapse-only behaviour with a debug log when the segment has no key. The second-click rejection is downgraded to debug only when the segment is already active (S10).
- `ProcessInboundAsync` routes an `arrowKey` with an empty row id to `HandleNoSelectionKey` before the unknown-row rejection (D-Q26).
- No `ConfigureAwait(false)` and no lock were added to the EFC router files (I6).

### QFC session, router and coordinator

- The `PendingIdentity` setter clears `PendingNode`, and `Pend` sets the node after the identity, so every row-level move drops a stale node.
- `BeginOpenTree` places the pending node on the committed subfolder only when it is visible (D-Q29); `ReconcilePendingNode` walks the path from the top and stops at the first collapsed ancestor, which is the deepest visible ancestor (AC7).
- `CommitPending` has three branches: commit the pending node; replace a visible committed descendant with the row entry; or select a different row. A hidden committed descendant on the same row is kept, which matches "Enter on the row entry re-confirms the hidden choice".
- `ClassifyPendingArrow` and `PendingArrowAsync` read the same visible order under `_sync`; the coordinator applies `CommitAndClose` and `CancelAndPopOut` through `_dispatcher.Dispatch`, and the pop-out raises `UnhandledArrow` only after `CancelSelector()` completes (D-Q21).
- The bridge-channel `segmentDoubleClick` is now a logged no-op (S5); the selector channel carries `selectorSegmentDoubleClick` with the row identity.
- `ParseKey` derives every wire value from the lower-cased enum name, so `Serialize` and `ParseKey` cannot drift.

### AC14

- `MapNode` derives `HasChildren` from `snapshot.GetChildren(node.Key).Count > 0` for both `GetAncestorChainAsync` and `GetImmediateSubfoldersAsync`; it is the only `FolderBreadcrumbSegment` construction site in `UtilitiesCS` (reviewer Grep). The cost is O(child keys) per mapped node over an already acquired snapshot.
- `OutlookFolderHierarchyProviderIssue1007Tests` builds a snapshot with a reader-reported child key that has no node and asserts `HasChildren == false` through both entry points, with a resolvable-child guard.

### Page scripts

- `BridgeJs`: Ctrl+ArrowLeft maps to `ParentLeft`, Ctrl+ArrowRight stays `Right`, page keys carry `fullyVisibleCount()` (minimum 1), and the render handler scrolls the selected `.child` into view before falling back to the row. The EFC page has no text input, so intercepting Home and End is safe.
- `FolderBreadcrumb.js`: the `onArrow` pre-filter is removed (S6); Ctrl+ArrowLeft posts `parent`; page keys carry `visiblePageSize()`; the non-leaf segment click stops propagation only in the open, expanded list; the segment double-click posts `selectorSegmentDoubleClick`. The handler is attached to the list and the drop-down button only. The visual gap is B-1.

### Removed #440 behaviour and test dispositions

- `BreadcrumbStateModel.LeftArrow` now only collapses and clears the subfolder selection; `RightArrow` delegates to `Expand`, which restores only a collapse anchored away from the active segment. `TryRightTreeTransition` and the EFC `TryRightTreeTransitionAsync` are deleted.
- `BreadcrumbRow.LeftArrow` no longer hides trailing segments; `ToggleLeafExpanded` and `CanExpandActiveSegment` allow expansion on an anchored collapse.
- The rewritten tests in `BreadcrumbBridgeRouterTests.cs`, `BreadcrumbBridgeRouterTests.Selection.cs`, `BreadcrumbRowStateTests.cs`, `BreadcrumbStateModelTests.cs`, `BreadcrumbStateModelSequenceTests.cs`, `FolderBreadcrumbBridgeRouterTests.cs`, `FolderBreadcrumbBridgeRouterInFlightTests.cs`, `BreadcrumbBridgeCoordinatorTests.cs`, `BreadcrumbSelectorCoordinatorTests.cs` and `FolderBreadcrumbAssetContractTests.cs` match the spec disposition table; each summary cites #1007 and the governing AC. `LeftArrow_WithSubfolderSelected_ResetsSubfolderSelectionAndCollapses` keeps its assertions as the table requires. Names are N-2.

## Best-Practice Checklist

| Area | Verdict | Note |
|---|---|---|
| Naming and documentation | PASS with note | XML documentation on every new public member; N-2 test names. |
| Error handling | PASS | Logged no-ops for invalid input; constructor guards on new messages; no broad catch added. |
| Async correctness | PASS | `TaskCompletionSource` in tests; `try`/`finally` releases the in-flight flag in `ExpandLeafAsync`. |
| Thread safety | PASS | EFC on the UI context (I6); QFC state under `_sync`; coordinator marshals through the dispatcher. |
| File size | PASS | Largest changed code file 500 lines. |
| Language level | PASS | net48: plain sealed class, no `record` or `init`. |
| UI state has visible styling | FAIL | B-1. |
| Test coverage of changed behaviour | PASS with note | N-1, N-3. |

## Recommendation

Address B-1 in a short remediation cycle: one stylesheet rule in `QuickFiler/Resources/FolderBreadcrumb.html`, one C# string-contract assertion (in `FolderBreadcrumbKeysIssue1007Tests.cs`, 158 lines, or `FolderBreadcrumbAssetContractTests.cs`, 429 lines), a full C# toolchain pass in CLAUDE.md order and a node run. In the same cycle, consider the N-1 tests and the N-2 renames, which touch only test files.
