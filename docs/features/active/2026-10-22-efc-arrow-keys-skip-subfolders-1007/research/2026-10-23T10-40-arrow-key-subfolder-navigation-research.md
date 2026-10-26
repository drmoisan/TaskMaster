# Research: arrow-key navigation over expanded subfolders in EFC and QFC (Issue #1007)

- Issue: #1007
- Branch: `bug/efc-arrow-keys-skip-subfolders-1007`
- Requirements: `issue.md` AC1 to AC13 and `grill-transcript.md` (Shared Understanding rules 1 to 11), both in this feature folder. They are treated as settled; the sections below record only what the code makes possible, impossible, or ambiguous.
- Evidence convention: every claim cites a file and a symbol. Line numbers are not used. Facts that need `git` or `gh` are marked "orchestrator to confirm".

## 1. Summary of findings

1. EFC and QFC share the descendant node type, `BreadcrumbDescendantNode` (`UtilitiesCS/OutlookObjects/Folder/BreadcrumbDescendantNode.cs`). EFC holds the first-level nodes in `BreadcrumbRow._descendants` (`BreadcrumbRow.Descendants.cs`). QFC holds them as `BreadcrumbStateRow.DescendantRoot.Children` (`BreadcrumbStateModel.Row.cs`). So one pure pre-order "visible rows" helper can serve both viewers.
2. `BreadcrumbDescendantNode` and `BreadcrumbRow.DescendantNodes` are `internal`. `UtilitiesCS/Properties/AssemblyInfo.cs` grants `InternalsVisibleTo` only to `DynamicProxyGenAssembly2`, `UtilitiesCS.Test` and `ToDoModel.Test`, not to `QuickFiler`. The EFC router (`QuickFiler.Controllers.BreadcrumbBridgeRouter`) therefore cannot walk the nodes itself. The visible-order computation must live in `UtilitiesCS` and be exposed through public members.
3. EFC already has a commit path for a descendant: `ActivateDescendant` calls `SelectHierarchyPath(row, path, nodeId)`, which feeds `ConfirmRow`. Up and Down can reuse it directly (AC3).
4. QFC tracks pending state by row identity only (`BreadcrumbSelectionSession.PendingIdentity`). Its Left and Right handling (`FolderBreadcrumbBridgeRouter.ArrowAsync`, which calls `_model.RightArrow()`/`_model.LeftArrow()`) acts on the committed row (`BreadcrumbStateModel.SelectedRow`), not on the pending highlight. Both must change for AC4, AC5 and AC7.
5. QFC's `BreadcrumbSelectionMap.GetSelectedFolder` returns the leaf `FolderPath` and ignores `ActiveSegmentIndex`. Today a QFC "parent selection" moves the expansion anchor but never changes the filing value. AC8 and AC9 ("selects that folder") need a new active-segment-aware value for the selected row only.
6. In the open QFC list, a single click on any segment bubbles to the row's `click` listener (`FolderBreadcrumb.js` `makeRow`), which posts `selectorActivate`. That commits the row and closes the list before a `dblclick` can arrive. The node test "clicking a selectable row uses its host identity" (`tests/quickfiler-breadcrumb/issue-997-common.test.mjs`) pins this. AC9 in the open list is unreachable until segment clicks stop bubbling (Open Question Q-E).
7. The node suites under `tests/quickfiler-breadcrumb/` produce no CI signal. The repository has no root `package.json`, and searching `.github/workflows/*.yml` for `node --test` and `.mjs` found nothing. They must be paired with C# string-contract tests.
8. A latent re-entrancy defect sits on the AC5 Right-expand path. `BreadcrumbBridgeRouter.ExpandLeafAsync` ends with `row.ToggleLeafExpanded()`. If a second Right (for example key auto-repeat) arrives while the first provider fetch is still awaited, both completions toggle and the row ends collapsed. The descendant path has a generation guard (`TryBeginDescendantExpand`); the row-leaf path has none. The fix belongs in this item, because AC5 is the trigger.

## 2. Current state, by viewer

### 2.1 EFC

- **Page script** (`BreadcrumbDocumentAssets.BridgeJs`).
  - The `keydown` handler maps `ArrowLeft`/`ArrowRight`/`ArrowUp`/`ArrowDown` to `{ type: 'arrowKey', rowId, key }`. `rowId` comes from `.rowwrap.selected`. `ctrlKey` is read only in the Enter branch, which ignores modified Enter. Home, End, PageUp and PageDown are not mapped.
  - Enter posts `rowConfirmed`.
  - The `click` handler covers, in order: `segmentActivate` (on `.seg[data-segment-activate="true"]`, which only non-leaf segments carry; see `BreadcrumbHtmlRenderer.AppendSegment`), `descendantToggle`, `descendantActivate`, `renderedChildActivate`, `leafExpandToggle` (any other `.affordance`, including the collapsed-terminal `data-role="reexpand"`) and `rowSelected`. Each branch calls `stopPropagation`.
  - `dblclick` posts `segmentDoubleClick`.
  - After every inbound message the script scrolls `.rowwrap.selected` into view.
- **Codec** (`BreadcrumbMessageCodec.DeserializeInbound`).
  - `arrowKey` requires only a non-empty string `key`. The key set is not closed in the codec.
  - An unknown key reaches `HandleArrowKeyAsync`'s `default` branch and is logged.
  - There is no integer field for a page size today (`BreadcrumbInboundMessage` carries `SegmentIndex`, `ChildIndex`, `Key` and `NodeId`).
- **Router** (`QuickFiler/Controllers/BreadcrumbBridgeRouter*.cs`).
  - `HandleArrowKeyAsync` (`.Arrows.cs`) handles four keys:
    - Right: `TryRightTreeTransitionAsync` (the #440 walk: `ReExpand`, then `ExpandLeafAsync`, then `GetActiveChild(0)` and `SelectHierarchyPath(row, child.FullPath)` with no node id, so the child is committed but not highlighted). If that does not apply: `ReExpand`, else `ExpandLeafAsync`.
    - Left: `ActivateSegment(ActiveSegmentIndex - 1)` (the #440 parent walk). If that does not apply: `BreadcrumbRow.LeftArrow`.
    - Up: `HandleUpArrow`.
    - Down: `MoveSelection`.
  - `FindSelectable`, `IndexOf` and `MoveSelection` (`.Selection.cs`) walk `_rows` only.
  - `SelectRow` calls `row.TrySelectDescendant(null)` and commits `FilingTarget`.
  - `SelectHierarchyPath` enforces the archive-relative contract through `ArchiveStemContract.TryMakeArchiveRelative`. It rejects the root itself (`stem.Length == 0`) and paths outside the root.
  - `ConfirmRow` raises `ConfirmationRequested` when the confirmed row is `_selectedRowId` and `SelectedFolderPath` is valid. The existing test `DescendantActivate_ThenRowConfirmed_RaisesConfirmationOnce` proves that Enter on a selected descendant files to the descendant.
  - The `SegmentDoubleClick` arm validates the index and calls `CollapseAfter`.
  - `HandleLeafToggleAsync`: a collapsed row is restored with `ReExpand` (the `[+]` restore); an expanded leaf is toggled; otherwise `ExpandLeafAsync` runs.
  - The router is wired in `EfcFormController.Breadcrumb.ConfigureBreadcrumbControl`, which is coverage-exempt and connects `FocusSearchRequested` to `SearchText.Select()`.
- **Row model** (`BreadcrumbRow.cs`).
  - `ActivateSegment` refuses the leaf index, unkeyed segments and the already-active index. It clears the descendants and the expansion.
  - `CollapseAfter` also clears the expansion and the descendants.
  - `ToggleLeafExpanded` is a no-op while `IsCollapsed`.
  - `LeftArrow` has two steps: leaf collapse first, otherwise hide the trailing segment.
  - `RightArrow` has no production caller. A search for `.RightArrow()` across `QuickFiler/` and `UtilitiesCS/` finds only `BreadcrumbStateModel.RightArrow` callers.
  - Production stores children only through `SetLeafDescendants`, called in `ExpandLeafAsync`. The key-less `SetLeafChildren` path is reached only from tests. Production EFC children are therefore always keyed nodes.
- **Renderer** (`BreadcrumbHtmlRenderer`).
  - A collapsed row renders `<span class="affordance reexpand" data-role="reexpand">+</span>` to the left of the terminal segment and omits the leaf affordance.
  - `AppendChildren` renders keyed nodes recursively when `IsLeafExpanded`. It marks the selected node with `aria-selected="true"` only on the selected row.

### 2.2 QFC

- **Page script** (`QuickFiler/Resources/FolderBreadcrumb.js`).
  - The `selectorKeys` map covers ArrowUp, ArrowDown, Enter and Escape and posts `selectorKey`.
  - ArrowLeft and ArrowRight go through `onArrow`. It posts `arrowKey` when the committed `selectedRow()` is a suggestion, collapsed, expanded or has a plus affordance; otherwise it posts `unhandledArrow`. `ctrlKey` is not read.
  - The same `handleKeyDown` is bound to `list` (the popup surface) and to `dropDownButton` (the collapsed surface).
  - `makeSubfolders` renders nodes recursively. The active sub is `selectedChildOwnsActive && child.selected`, that is, the committed descendant.
- **Two surfaces, one coordinator.** `BreadcrumbMessengerHub` fans one stream out to a `Collapsed` attachment and an `Expanded` attachment and rewrites the `selectorView` `mode` field for each (`PostToSurface`/`RewriteSelectorMode`). Both surfaces post into one `BreadcrumbBridgeCoordinator`.
- **Coordinator** (`QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`).
  - `selector*` messages go through `HandleSelectorMessage`, which runs synchronously on the dispatcher. `selectorToggle` calls `OpenSelector` or `CancelSelector`. `HandleSelectorKey` maps Up and Down to `MoveSelector`, Enter to `CommitSelector` and Escape to `CancelSelector`.
  - Every other message goes through `DispatchAsync`: `RaiseSyntheticArrowKey` (the `FolderArrowKeyDown` event), then `_router.RouteAsync`, then `PublishRouterOutputs`, which turns `UnhandledArrowMessage` into the `UnhandledArrow` event and posts everything else.
- **Open and close.** `OpenSelector` returns an `OpenStateChanged` transition. `PublishTransition` raises `SelectorOpenStateChanged`. `BreadcrumbItemViewerLifecycleCoordinator.OnSelectorOpenStateChanged` calls `BreadcrumbDropDownOpenCoordinator.HandleSelectorOpenStateChanged`, which posts `RequestOpen` when the session is open and `CloseCore(ExplicitCommit)` otherwise. Opening the list is therefore triggered entirely by opening the selector session.
- **Fall-through.** `QfcItemController.ViewerSetup.OnBreadcrumbUnhandledArrow` calls `KeyboardHandler.BreadcrumbArrowFallThrough`. That method is coverage-exempt (`[ExcludeFromCodeCoverage]` on the class). Right shows the `MyBox` "Pop Out Item or Enumerate Conversation?" dialog. Left calls `SetFolderDroppedDown(false)`, which is `CloseCore(Uncommitted)`, a cancel.
- **Where closed-list Right goes today.** The collapsed surface's `dropDownButton` keydown runs `onArrow("right")` and posts `arrowKey` (or `unhandledArrow`). That reaches `FolderBreadcrumbBridgeRouter.ArrowAsync`, which calls `_model.RightArrow()` on the committed row. The effect is one of three: the leaf expansion is fetched while the list is closed and not shown (`visibleRows` shows only the committed row in collapsed mode, and `makeSubfolders` runs only when `viewMode === "expanded"`); the #440 descent commits a subfolder through `SelectSubfolder(0)`; or, when nothing applies, the pop-out dialog opens. Nothing opens the list.
- **Selection session** (`BreadcrumbSelectionSession*.cs`).
  - `Move` uses `PendingIdentity` when open and `CommittedIdentity` when closed, and walks `_model.Rows` with `NextSelectableIndex`.
  - `CommitPending`, `Cancel` (which restores `OriginalIdentity` plus `_originalDescendant` through `RestoreDescendantSelection`) and `ActivateDescendant` (which commits and closes immediately) all key on rows.
- **Model** (`BreadcrumbStateModel.cs`).
  - `RightArrow`: `TryRightTreeTransition`, else `ReExpand`, else `TryExpandLeaf`.
  - `LeftArrow`: the #440 `ActivateSegment(active - 1)` walk, then `TryCollapseLeaf`. QFC has no Left segment hiding.
  - `BreadcrumbStateRow.ActiveSegmentHasSubfolders` and `LeafHasSubfolders` both require `CollapsedAfterIndex == null`.
- **Double-click.** `FolderBreadcrumbBridgeRouter.RouteAsync`, `SegmentDoubleClickMessage` arm: `RowAt(m.RowIndex).CollapseAfter(m.SegmentIndex)` only. QFC has no single-click segment activation; segment cells register only `dblclick` (`makeCell`).
- **Projection** (`BreadcrumbRenderProjection.ProjectRow`). A collapsed row gets a `Plus` cell to the left of the terminal segment. The leaf `Plus`/`Minus` cell is emitted only when `LeafHasSubfolders`. Subfolders are projected for any row with `LeafExpanded`, not only the selected row.

## 3. Answers to the research questions

### Q1. Visible-order traversal

- **Node types.** Both viewers store `BreadcrumbDescendantNode` (`Identity`, `Segment`, `Expanded`, `Loaded`, `Children`). A node's children are visible exactly when `Expanded` is true. `Children` stays empty until `Attach`, so an in-flight node shows nothing, which matches `BreadcrumbHtmlRenderer.AppendDescendants` and `BreadcrumbRenderProjection.ProjectDescendant`.
  - EFC first level: `BreadcrumbRow.DescendantNodes`, visible when `IsLeafExpanded` (the renderer condition in `AppendChildren`).
  - QFC first level: `BreadcrumbStateRow.DescendantRoot.Children`, visible when `LeafExpanded`. `DescendantRoot` is the active segment itself and is never an entry.
- **Row-level entries.**
  - EFC: every non-banner row in `_rows`, including `TrashPseudoRow`. This is the `FindSelectable` rule.
  - QFC: every `IsSelectable` row in `_model.Rows`. This is the `NextSelectableIndex` rule.
- **Recommendation: one pure helper in `UtilitiesCS`.**
  - New file `UtilitiesCS/OutlookObjects/Folder/BreadcrumbVisibleOrder.cs` holding a public immutable `BreadcrumbVisiblePosition`: `RowIndex`, `NodeId` (null for the row entry), `ParentNodeId`, `Depth`, `HasChildren`, `IsExpanded`. Use a plain class or `readonly struct`, not `record`, because net48 has no `IsExternalInit`.
  - A static `BreadcrumbVisibleOrder` with:
    - an internal pre-order flattener over `IReadOnlyList<BreadcrumbDescendantNode>`;
    - public pure index operations: `IndexOf(order, rowIndex, nodeId)`, `Step(order, index, delta)` clamped without wrap (returning -1 when there is no move), `First`, and `Last`.
  - EFC entry point: a public `BreadcrumbRow.AppendVisiblePositions(int rowIndex, List<BreadcrumbVisiblePosition> target)`, so the router never touches the internal node type.
  - QFC entry point: an internal `BreadcrumbStateModel.VisiblePositions()`, consumed by `BreadcrumbSelectionSession`.
  - The helper is pure and needs no WebView2, so it is unit-testable in `UtilitiesCS.Test` with internal node fixtures.
- **Rejected alternatives.**
  - (a) A separate traversal in each viewer: duplicates the pre-order logic.
  - (b) Computing the order in each page script from DOM order and posting a target identity: the order would live in code with no CI test, and C# would have to trust a page-supplied identity.

### Q2. EFC highlight identity

- **Representation.** Keep the existing pair: `_selectedRowId` plus `BreadcrumbRow.SelectedDescendantId`. No new state is needed.
  - The current position is `(IndexOf(row for _selectedRowId), selected row's SelectedDescendantId)`.
  - The `arrowKey` message keeps sending only the row id, and the router derives the node.
- **Moving onto a descendant.** Call `SelectHierarchyPath(row, row.GetDescendantPath(nodeId), nodeId)`, the same path as `ActivateDescendant`. That satisfies AC3 "exactly as a mouse click does":
  - `SelectedFolderPath` becomes the archive-relative stem;
  - `SelectedFolderPathChanged` is raised;
  - the full-list render marks the node.
- **Enter (`ConfirmRow`).** It needs no change. The `.rowwrap.selected` element is still the owning row, because `RenderRowFragment(row, row.RowId == _selectedRowId)` is unchanged.
- **Moving onto a row entry** needs a new helper, `SelectRowEntry(row)`:
  - when `ActiveSegmentIndex` is the leaf, it keeps today's `SelectRow` (`FilingTarget`);
  - when the active segment is an ancestor, which after this change happens only through AC8 or AC9, it commits `SelectHierarchyPath(row, ActiveSegment.FullPath)`. A shortened row then files to the folder it displays (`Archive > Clients [+]` files to Clients).
- **Nearest visible ancestor (AC7).** Collapsing node N hides only N's subtree, so the nearest visible ancestor of a hidden selection is N itself. Collapsing the row's leaf expansion makes it the row entry.
  - `BreadcrumbRow.TryCollapseDescendant` leaves `SelectedDescendantId` pointing at a hidden node.
  - `ToggleLeafExpanded(false)` and `CollapseAfter` call `ClearDescendants`, which nulls `SelectedDescendantId`. But `SelectedFolderPath` keeps the hidden descendant's stem, so the router reports a path the page no longer shows. This is an existing inconsistency.
  - Recommended: `BreadcrumbRow` reports whether the collapse hid the selected node. The router then re-commits N (`SelectHierarchyPath(row, N path, N id)`) or `SelectRowEntry(row)`.
  - This narrows #1002 invariant I3 ("a toggle never changes the selection") to "unless the toggle hides the selected node". The existing test `DescendantToggle_DoesNotChangeSelection` selects the row and toggles a node below it, so it stays valid unchanged.

### Q3. QFC pending identity

- **Minimal extension.**
  - Add `PendingNode` (an internal `BreadcrumbDescendantNode?`) next to `PendingIdentity`, in a new partial (`BreadcrumbSelectionSession.Tree.cs`), because `BreadcrumbSelectionSession.cs` is 446 lines.
  - Expose its identity through `BreadcrumbSelectorState.PendingNodeIdentity`, and through an optional `pendingNodeIdentity` field on `BreadcrumbSelectorViewMessage`. Add it as a constructor parameter with a default, so existing call sites compile.
  - The coordinator's `PostSelectorStateCore` passes it on. `FolderBreadcrumb.js` `render`/`makeSubfolders` marks `.sub.active` where `child.identity === state.pendingNodeIdentity` on the pending row.
  - `BreadcrumbMessengerHub.RewriteSelectorMode` parses and string-patches only `mode`, so the added field passes through unchanged.
- **Moving.** In open mode, `Move` walks `BreadcrumbVisibleOrder` from `(PendingIdentity, PendingNode)`. In closed mode it keeps the row-only walk unchanged (AC10).
- **Enter.** `CommitPending` calls `_model.SelectDescendant(rowIndex, PendingNode)` when `PendingNode` is set. `BreadcrumbSelectionMap.TryDescendantValue` must succeed; reject otherwise, as `ActivateDescendant` already does.
- **Escape.** `Cancel` already restores `OriginalIdentity` plus `_originalDescendant`. It also clears `PendingNode`.
- **Left commit-and-close (AC4).** Commit the pending entry, then close. Use the existing `CommitSelector` transition, which reports `OpenStateChanged`; the open coordinator then closes with `ExplicitCommit`. This replaces the current Left fall-through, `SetFolderDroppedDown(false)`, which cancels.
- **Right when closed (AC10).** In the coordinator, an `arrowKey` or `unhandledArrow` Right that arrives while `IsSelectorOpen` is false calls `OpenSelector()` and does not route to the router. `Open()` already sets `PendingIdentity = CommittedIdentity`; set `PendingNode` to the committed descendant when it is visible (Open Question Q-G). A closed Left is consumed with no effect.
- **Page-side pre-filter.** `onArrow` decides between `arrowKey` and `unhandledArrow` from the committed `selectedRow()`. With a pending row that differs from the committed row this is wrong. For example, with Trash committed and a suggestion pending, Right posts `unhandledArrow`. Recommendation: `onArrow` always posts `arrowKey`, and C# decides.

### Q4. Right and Left semantics; #440 and Left segment hiding

**Where the removed behaviour lives:**

| Behaviour | Location (file, symbol) |
|---|---|
| EFC #440 Left parent walk | `BreadcrumbBridgeRouter.Arrows.cs`, `HandleArrowKeyAsync`, `case "Left"` (`row.ActivateSegment(ActiveSegmentIndex.Value - 1)`) |
| EFC Left segment hiding | `BreadcrumbRow.cs`, `LeftArrow` (`CollapsedAfterIndex = terminalIndex - 1`), called from the same `case "Left"` |
| EFC #440 Right walk (replaced by the visible-order Right) | `BreadcrumbBridgeRouter.Arrows.cs`, `TryRightTreeTransitionAsync` |
| QFC #440 Left parent walk | `BreadcrumbStateModel.cs`, `LeftArrow` (`row.ActivateSegment(activeIndex.Value - 1)`) |
| QFC #440 Right walk | `BreadcrumbStateModel.cs`, `RightArrow` and `TryRightTreeTransition` |
| QFC Left segment hiding | none. QFC `LeftArrow` never sets `CollapsedAfterIndex`. |
| `[+]` restore, EFC | `BreadcrumbBridgeRouter.Arrows.cs`, `HandleLeafToggleAsync` (`IsCollapsed`, then `ReExpand`); `HandleArrowKeyAsync` Right (`IsCollapsed`, then `ReExpand`) |
| `[+]` restore, QFC | `FolderBreadcrumbBridgeRouter.Descendants.cs`, `ToggleAsync` (`CollapsedAfterIndex != null`, then `ReExpand`); `BreadcrumbStateModel.RightArrow` (`ReExpand`) |

**Existing tests that pin removed or changed behaviour, with the proposed disposition:**

| Test (project, file, method) | Pins | Disposition |
|---|---|---|
| QuickFiler.Test `BreadcrumbBridgeRouterTests.cs` `HandleArrowKey_LeftOnMultiSegmentRow_ActivatesParentSegment` | EFC #440 Left walk | Rewrite: Left on a non-expanded row is a no-op (AC7) |
| same file `HandleArrowKey_RepeatedLeft_WalksToRootThenFallsThroughToExistingBehavior` | Walk plus segment hiding | Rewrite: three Lefts post no render and keep `SelectedFolderPath` |
| same file `HandleArrowKey_RightOnActivatedParent_ExpandsViaSingleImmediateSubfolderCall` | Right on a collapsed, activated parent | Update to the AC9 shape (double-click selects and shortens; Right lists the parent's children). The single-query assertion is kept. |
| same file `HandleArrowKey_RightAfterExpansion_DescendsByChildActivation` | Right descends to child 0 without a highlight | Update: Right selects the first descendant node with its `nodeId` (AC5) |
| same file `Boundary_EfcLeftAtRootAndRightOnChildlessNode_RemainSilentNoOps` | Four Lefts walk to the root | Update: every Left is a silent no-op. Right on a childless node is unchanged (AC6 EFC). |
| same file `ArrowKeyRight_ThenLeft_ExpandsAndCollapses` | Leaf expand, then collapse | Unaffected (AC5, AC7) |
| same file `ArrowKey_SingleSegmentRow_TakesPreExistingCollapsePath` | No render for a one-segment row | Unaffected; re-run |
| QuickFiler.Test `BreadcrumbBridgeRouterTests.Selection.cs` `LeafExpandToggle_OnCollapsedRow_ReExpandsWithoutProviderQuery` | `[+]` restore | Rewrite for AC9: `[+]` lists the active segment's children (one provider query); the trailing segment stays hidden |
| same file `ArrowKeyRight_WhenCollapsed_ReExpandsWithoutProviderQuery` | Right restores | Rewrite for AC9 (see Open Question Q-A) |
| same file `ArrowKeyDown_SelectsNextSelectableRow`, `ArrowKeyUp_OnNonTopRow_SelectsPreviousRowWithoutFocusSearch`, `ArrowKeyUp_AtTopSelectableRow_PostsFocusSearchAndRaisesEvent` | Row navigation without expansion | Unaffected (no expanded rows) |
| QuickFiler.Test `BreadcrumbBridgeRouterQueueTests.Part2.cs` `SegmentDoubleClick_ValidIndex_ViaHostEvent_CollapsesRowAndPostsRender` | Double-click collapses and renders | Re-run. Update only if it asserts the exact post count, because the AC9 arm may add a full-list render. |
| UtilitiesCS.Test `BreadcrumbRowStateTests.cs` `LeftArrow_WhenFullyExpanded_HidesTrailingSegment` | Left segment hiding | Rewrite: returns false, `CollapsedAfterIndex` stays null |
| same file `LeftArrow_AtRootSegment_IsNoOp` | Hiding boundary | Fold into the rewrite above, or delete with a recorded reason |
| same file `LeftArrow_WhenLeafExpanded_CollapsesLeafFirst` | Leaf collapse | Unaffected |
| same file `RightArrow_WhenCollapsed_RestoresFullBreadcrumb`, `ToggleLeafExpanded_WhileCollapsed_IsNoOp` | `[+]` restore on the row model | Update. These change only if `RightArrow` and `ToggleLeafExpanded` are themselves changed for AC9; see the Q6 recommendation. |
| UtilitiesCS.Test `BreadcrumbRowDescendantsIssue1002Tests.cs` (the test that calls `row.LeftArrow()`) | Left clears descendants | Re-run. Unaffected when the leaf-collapse step is kept. |
| UtilitiesCS.Test `BreadcrumbStateModelTests.cs` `LeftArrow_QfcMultiSegmentRow_SelectsParentNode`, `RightArrow_QfcSelectedParentNode_ExpandsIntoChildren`, `ArrowKey_QfcSingleSegmentRow_TakesPreExistingCollapsePath` | QFC #440 walks | First: rewrite as a no-op. Second: rewrite to the AC9 entry point. Third: re-run. |
| UtilitiesCS.Test `BreadcrumbStateModelSequenceTests.cs` `Arrows_RightExpandsThenLeftCollapses_UnhandledWhenNothingChanges`, `RightArrow_OnCollapsedRow_ReExpandsBeforeLeafExpansion`, `LeftArrow_WithSubfolderSelected_ResetsSubfolderSelectionAndCollapses`, `LeftArrow_RepeatedOnThreeSegmentChain_WalksToRootThenReportsUnhandled`, `LeftArrow_WalkFromAnOpenLeafExpansion_ClearsTheExpansionAndStillReachesTheRoot` | #440 walk and restore | Rewrite each to the AC7 and AC9 rules. Record the reason ("#440 Left walk removed by #1007 AC7"). |
| UtilitiesCS.Test `BreadcrumbSelectionSessionTests.cs` `BreadcrumbLeftAndRightTransitions_DoNotMutateSelectorSession` | `model.LeftArrow()` succeeds on the #440 walk | Rewrite. The new arrow decisions are session-aware by design. |
| UtilitiesCS.Test `FolderBreadcrumbBridgeRouterTests.cs` `Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft`, `ArrowAsync_QfcLeftOnMultiSegmentRow_RoutesParentSelectTransition`, `ArrowAsync_QfcRightOnSelectedParentNode_RoutesChildExpansion` | QFC #440 walk through the router | Rewrite. The file is 495 lines: replace bodies in place and add nothing; new cases go in a new test file. |
| QuickFiler.Test `BreadcrumbBridgeCoordinatorTests.cs` `UnhandledRightArrow_RaisesUnhandledArrowRight`, `UnhandledLeftArrow_RaisesUnhandledArrowLeft`, `ArrowMessages_RaiseSyntheticFolderKeyDown` | Closed-mode arrows reach the fall-through | Update (AC10). With the list closed, Right opens it and Left does nothing. Move the pop-out expectation to an open-list precondition. The harness's closed state must be confirmed by reading `CreateHarness`. |
| QuickFiler.Test `BreadcrumbSelectorCoordinatorTests.cs` `ExistingLeftAndRightMessages_StillForwardOnce` | Closed Left produces one `UnhandledArrow` | Update (AC10). The file is 500 lines, so the replacement must not add lines. |
| QuickFiler.Test `BreadcrumbBridgeRouterTests.cs` `Boundary_QfcUnhandledArrow_StillReachesBreadcrumbArrowFallThrough` | Controller forwards to `IQfcKeyboardHandler` | Unaffected (it calls `OnBreadcrumbUnhandledArrow` by reflection) |
| QuickFiler.Test `FolderBreadcrumbAssetContractTests.cs` `LeftAndRightBreadcrumbMessages_RemainSupported` | Literal `{ type: "unhandledArrow", direction: direction }` | Update if the `onArrow` pre-filter is removed (Q3) |
| same file `RenderReceiver_OwnsSelectedChildExpandedAndCollapsedProjection` | Literal `selectedChildOwnsActive && child.selected` | Update for the pending-node highlight |
| same file `SelectorKeys_PreventBrowserScrollingAndPostNativeKeyMessages` | The four mapped keys; no ArrowLeft or ArrowRight in `selectorKeys` | Extend for Home/End/PageUp/PageDown. The ArrowLeft and ArrowRight negative assertions stay valid. |
| node `issue-997-common.test.mjs` "arrow … routes … through host contract" (twelve generated cases) | `onArrow` pre-filter | Update if the pre-filter is removed |
| node `issue-997-common.test.mjs` "clicking a selectable row uses its host identity" | A segment click commits the row | Keep for leaf segments. Add a non-leaf case per Open Question Q-E. |

**Right on a shortened row.** Today a shortened row exists in two ways:

- EFC: `CollapseAfter` from a double-click, or `LeftArrow` hiding.
- QFC: `CollapseAfter` from a double-click.

Right restores it through `ReExpand` (EFC `HandleArrowKeyAsync` and `TryRightTreeTransitionAsync`; QFC `BreadcrumbStateModel.RightArrow`). After AC7 removes Left hiding, the only producer of a shortened row is a segment double-click (AC9) or Ctrl+Left (AC8, defined as the same action). AC9 says the next Right on that row lists the folder's children and does not restore it. That conflicts with AC5's "first restores the full path"; see Open Question Q-A.

**Proposed Right and Left decision table** (P is the current position: the EFC selection, or the QFC pending entry when the list is open):

| P | Right | Left |
|---|---|---|
| Row, has children, not expanded | Expand (EFC `ExpandLeafAsync`; QFC `TryExpandLeaf`/`TryExpandActiveSegment` plus fetch) | Nothing (QFC open: commit and close) |
| Row, expanded | Move to the first visible child (EFC select; QFC pending) | Collapse the row's expansion; P stays |
| Row, no children (leaf, Trash, plain or fallback row) | EFC: nothing. QFC open: cancel, close, pop-out dialog. | Nothing (QFC open: commit and close) |
| Node, has children, not expanded | Expand (EFC `HandleDescendantToggleAsync`; QFC `ToggleDescendantAsync`) | Nothing (QFC open: commit and close) |
| Node, expanded, children loaded | Move to `Children[0]` | `TryCollapseDescendant` / `node.Collapse()`; P stays |
| Node, expanded, fetch in flight or zero children | Nothing | Collapse; P stays |
| Node, no children | As for a leaf row | As for a leaf row |

### Q5. Ctrl+Left message shape

- **EFC.**
  - Add the key values `ParentLeft`, `Home`, `End`, `PageUp` and `PageDown` to the existing free-string `key`. In `BridgeJs`: `e.ctrlKey && e.key === 'ArrowLeft'` maps to `ParentLeft`, and Ctrl+Right maps to `Right`.
  - `BreadcrumbMessageCodec` needs no change for the key values. It needs a new optional integer `pageSize` (`OptionalInt`) on `arrowKey`, carried as a new optional `BreadcrumbInboundMessage.PageSize` constructor parameter with a default.
  - Router: new `case` arms. `default` keeps logging unknown keys.
- **QFC.**
  - Route Ctrl+Left as a selector key, not as an arrow direction. Adding a value to `BreadcrumbArrowDirection` would reach `KeyboardHandler.BreadcrumbArrowFallThrough`, whose `else` branch treats every non-Right direction as Left.
  - Add `BreadcrumbSelectorKey.Parent`, `Home`, `End`, `PageUp` and `PageDown`.
  - `BreadcrumbSelectorMessageSerializer.ParseKey` gains `parent`, `home`, `end`, `pageup` and `pagedown`. These match `Serialize`'s `Key.ToString().ToLowerInvariant()`.
  - Add an optional `pageSize` on `BreadcrumbSelectorKeyMessage` (parsed with `RequireNonNegativeInt`-style validation when present; serialized when set).
  - Extend the page `selectorKeys` map to Home, End, PageUp and PageDown. Ctrl+ArrowLeft posts `{ type: "selectorKey", key: "parent" }`.
- **Tests that pin the closed key sets** (extend, do not break):
  - UtilitiesCS.Test `BreadcrumbSelectorMessagesTests.cs`: its key round-trip and unknown-key `DataRow` cases. Add the new values. The unknown-key negative cases must use a value that stays unknown.
  - QuickFiler.Test `FolderBreadcrumbAssetContractTests.SelectorKeys_PreventBrowserScrollingAndPostNativeKeyMessages`.
  - node `issue-997-common.test.mjs` "selector key … sends exact payload" (add rows) and "unrecognized keys preserve browser behavior" (it uses Tab, which stays unrecognized).
  - EFC `BreadcrumbHtmlRendererTests.Issue995BridgeJsPostsConfirmationForPlainEnterAndScrollsSelectedRowIntoView` asserts `e.ctrlKey` and `scrollIntoView({ block: 'nearest' })`. Both remain present.
- **Stop below the archive root.**
  - `OutlookFolderHierarchyProvider.GetAncestorChainAsync` trims every chain below the archive root (`ArchiveChainProjection.TryTrimBelowArchiveRoot`). Both production constructions pass `() => _globals.Ol.ArchiveRootPath`: `EfcFormController.Breadcrumb.ConfigureBreadcrumbControl` and `QfcItemController.BreadcrumbWiring`.
  - So segment index 0 is already one level below the root, and the stop condition is "the active segment index is 0", or "a depth-0 node whose parent is the row entry" for nodes.
  - Defence in depth: EFC `SelectHierarchyPath` rejects the root (`stem.Length == 0`) and paths outside it, keyed on `_boundRoot`. QFC `BreadcrumbSelectionMap.TryDescendantValue` rejects an empty stem when `ArchiveRoot` is set.
  - In the maintainer's example, `Archive` in `Archive > Clients > Beta` is therefore a first-level folder below the root. Ctrl+Left from `Archive > Clients [+]` yields `Archive [+]`, and a further Ctrl+Left does nothing.

### Q6. Segment double-click (AC9)

- **EFC first click: confirmed.** `BridgeJs` `click` posts `segmentActivate` for `.seg[data-segment-activate="true"]`, and the renderer adds that attribute only to non-leaf segments. Standard DOM ordering is click, click, dblclick. The first click runs `ActivateSegment` and then `SelectHierarchyPath(row, ActiveSegment.FullPath)`. The second click is rejected by `BreadcrumbRow.ActivateSegment` ("already active"), and the router logs an error for it. The `dblclick` then runs `CollapseAfter(index)`.
  - Recommendation: make the `SegmentDoubleClick` arm atomic and independent of the click ordering. Activate the segment if it is not already active, commit it through `SelectHierarchyPath`, then `CollapseAfter`. If activation is impossible (an unkeyed segment), leave the row unchanged. Downgrade the second-click "rejected" log when the index is already active.
- **QFC: confirmed that no single-click segment activation exists.** `makeCell` segment cells register only `dblclick`. The `SegmentDoubleClickMessage` arm must do three things:
  - `ActivateSegment(index)`;
  - `CollapseAfter(index)`;
  - commit the row with the active segment as its value: `SelectRow(rowIndex)` with `SynchronizeCommittedSelection`, or `CommitPending`/`Activate` when the list is open, plus a close.
  - Because `RouteAsync` returns JSON strings rather than a `BreadcrumbSelectionTransition`, the double-click should move to the selector channel. The coordinator can then publish `SelectionChanged` and `OpenStateChanged` through `ApplyTransition`. Alternatively the coordinator can follow the router call with `CommitSelector()`. The selector-channel move is cleaner. It requires a new `selectorSegmentDoubleClick` message (rowIdentity, segmentIndex) and removes the old `segmentDoubleClick` routing for QFC.
- **QFC filing value.** Add `BreadcrumbSelectionMap.SelectedRowValue(model, row)`, returning the archive-relative `Chain[ActiveSegmentIndex].FolderPath` when the active index is below the leaf (the same conversion as `TryDescendantValue`), and otherwise the existing `RowValue`. Use it only in `GetSelectedFolder`. `GetFolderItems`, `IndexOfItem`, `FolderContains` and `TrySelectItem` must keep `RowValue` (the presented targets).
- **Making `[+]` list the segment's children.**
  - EFC: in `HandleLeafToggleAsync` and Right, a collapsed row whose `ActiveSegmentIndex == CollapsedAfterIndex` expands the active segment (`ExpandLeafAsync`) instead of calling `ReExpand`.
    - `BreadcrumbRow.ToggleLeafExpanded`'s `IsCollapsed` guard must allow this case.
    - `BreadcrumbHtmlRenderer.AppendSuggestionRow` must render a collapse affordance (minus) at the terminal segment while expanded.
    - `AppendChildren` already renders whenever `IsLeafExpanded`.
  - QFC: `BreadcrumbStateRow.ActiveSegmentHasSubfolders` (and therefore `TryExpandActiveSegment`) must accept `CollapsedAfterIndex == ActiveSegmentIndex`. `ToggleAsync` and `RightArrow` must expand instead of `ReExpand` in that case. `BreadcrumbRenderProjection.ProjectRow` must emit `Minus` instead of the terminal `Plus` while expanded. `FetchAndAttachSubfoldersAsync` keys on `DescendantRoot`, which follows `ActiveSegment`, so the fetch already targets Clients.
- **Provider calls.** Both viewers call `IFolderHierarchyProvider.GetImmediateSubfoldersAsync(key)`:
  - EFC key: `BreadcrumbRow.ActiveSegmentKey`, from `_segmentKeys`, set by `AttachSegmentKeys` at bind time.
  - QFC key: `DescendantRoot.Segment.Key` (the active chain segment's `Key`).
  - No `ResolveLeafKeyAsync` call is needed.
- **Highlight after the expansion.** It stays on Clients: the selected row entry, with `SelectedDescendantId` null. The next Right takes the "row, expanded" path to the first child (Q18 A).

### Q7. Right on a leaf (AC6)

- **EFC:** do nothing. Remove the fall-through branches, so no render is posted.
- **QFC today:** `UnhandledArrow` is raised in `BreadcrumbBridgeCoordinator.PublishRouterOutputs` when the router returns an `UnhandledArrowMessage`. The router produces it in `ArrowAsync` when `_model.RightArrow()`/`LeftArrow()` return false; the page produces it in `onArrow`.
- **Sequence for an open list.** The session decision returns "cancel and pop out". The coordinator then:
  1. calls `ApplyTransition(_router.CancelSelector())`. This restores the original, raises `OpenStateChanged`, and the open coordinator posts `CloseCore(ExplicitCommit)` to the popup operations queue.
  2. then raises `UnhandledArrow(Right)`. `KeyboardHandler.BreadcrumbArrowFallThrough` shows `MyBox.ShowDialog`.
- **Ordering.** Inside a dispatched action, `BreadcrumbUiDispatcher.Dispatch` runs nested work inline when on the boundary (`IsCurrentBoundary`). So in a C# test the order "transition published before `UnhandledArrow`" is deterministic. Whether the popup is visibly closed before the modal dialog paints depends on the popup operations queue and the modal message pump. That is a manual verification item.

### Q8. Home, End, PageUp and PageDown

- **Page size.** C# cannot know the viewport. Recommendation: each page script measures and posts `pageSize` with PageUp/PageDown, and C# moves by `pageSize` in `BreadcrumbVisibleOrder`, clamped at both ends and without wrap. The movement is then a pure, deterministic C# function of `(order, index, pageSize)`.
  - Measurement: count the navigable elements whose bounding rectangle lies fully inside the scroll viewport:
    - EFC: `window.innerHeight` against `.rowwrap > .row.selectable` and `.child[data-node-id]`;
    - QFC: the `list` element's rectangle against `[role="option"]` rows and `.sub` elements.
  - Use a minimum of 1.
  - Make the counting a small pure function, for example `fullyVisibleCount(rects, top, bottom)`, so it can be tested in node with stub rectangles. The node harness `Element` has no layout API; add `getBoundingClientRect` stubs to `issue-997-browser-harness.mjs`.
  - C# validation: a missing, zero or negative `pageSize` makes the key a logged no-op, or treats it as 1 (to be decided in the plan).
  - Rejected alternative: posting a target identity, for the reasons in Q1 (b).
- **QFC closed Home/End (AC10).** Use the closed branch of a new `MoveToBoundary(first)`: `_model.SelectRow(FirstSelectableIndex()/LastSelectableIndex())` and `CommittedIdentity`, which follows the existing closed `Move` contract. The open branch sets the pending entry to `First`/`Last` of the visible order. Closed PageUp/PageDown are not specified (Open Question Q-C).
- **EFC Home** selects the first visible entry and must not post `BreadcrumbFocusSearchMessage` or raise `FocusSearchRequested`. Only `HandleUpArrow` at the top does that (AC2).

### Q9. File-size budget

Primary method: physical line counts from ripgrep `^` counts (Grep tool, `output_mode: count`). Cross-check: the last numbered line of each file read in full with the Read tool, which matched for every file read in full.

| File | Lines | Project (csproj) | Guidance |
|---|---|---|---|
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 471 | QuickFiler | Do not extend; new arms delegate to a new partial |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs` | 222 | QuickFiler | Rewrite in place; navigation goes to a new `BreadcrumbBridgeRouter.Navigation.cs` |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` | 305 | QuickFiler | Small edits only |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Descendants.cs` | 104 | QuickFiler | Room for the AC7 re-commit |
| `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs` | 219 | QuickFiler | No change expected |
| `QuickFiler/Controllers/KeyboardHandler.cs` | 414 | QuickFiler (coverage-exempt) | No change expected |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | 451 | QuickFiler | Do not extend; new `BreadcrumbBridgeCoordinator.Keyboard.cs` |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Descendants.cs` | 47 | QuickFiler | Room |
| `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` | 490 | QuickFiler | Do not touch |
| `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | 497 | QuickFiler | Do not touch |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` | 395 | QuickFiler | No change expected |
| `QuickFiler/Resources/FolderBreadcrumb.js` | 350 | QuickFiler (`Content` plus resx) | Growth acceptable; keep under 500 |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs` | 185 | UtilitiesCS | Room |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs` | 370 | UtilitiesCS | Small edits; the visible-position member goes in `BreadcrumbRow.Descendants.cs` (200) |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.Descendants.cs` | 200 | UtilitiesCS | Room |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDescendantNode.cs` | 72 | UtilitiesCS | Unchanged |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs` | 298 | UtilitiesCS | Room |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs` | 220 | UtilitiesCS | Room |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs` | 197 | UtilitiesCS | Room |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` | 268 | UtilitiesCS | Room |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs` | 408 | UtilitiesCS | Limited headroom; guard changes only |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs` | 446 | UtilitiesCS | Do not extend; new `BreadcrumbSelectionSession.Tree.cs` |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.Descendants.cs` | 77 | UtilitiesCS | Room |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.Highlight.cs` | 59 | UtilitiesCS | Room |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionMap.cs` | 144 | UtilitiesCS | Room |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectorMessages.cs` | 371 | UtilitiesCS | About 40 lines of headroom; enough for the new keys and `pageSize` |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.cs` | 376 | UtilitiesCS | Change only if a QFC double-click message is added there |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs` | 289 | UtilitiesCS | Room |
| `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` | 404 | UtilitiesCS | Avoid growth; new `FolderBreadcrumbBridgeRouter.Navigation.cs` |
| `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Descendants.cs` | 225 | UtilitiesCS | Room |

Test files at or near the limit, which must not grow: QuickFiler.Test `BreadcrumbSelectorCoordinatorTests.cs` (500), UtilitiesCS.Test `FolderBreadcrumbBridgeRouterTests.cs` (495), QuickFiler.Test `BreadcrumbBridgeCoordinatorTests.cs` (488), `BreadcrumbBridgeRouterIssue1002Tests.cs` (487), UtilitiesCS.Test `BreadcrumbRowDescendantsIssue1002Tests.cs` (466), QuickFiler.Test `BreadcrumbBridgeRouterTests.cs` (462), `BreadcrumbBridgeRouterQueueTests.cs` (462), `BreadcrumbBridgeRouterIssue439Tests.cs` (455), UtilitiesCS.Test `BreadcrumbStateModelTests.cs` (454).

Every new `.cs` file needs a `<Compile Include>` entry in its legacy project file:
- `UtilitiesCS/UtilitiesCS.csproj` (precedent: `OutlookObjects\Folder\BreadcrumbDescendantNode.cs`);
- `QuickFiler/QuickFiler.csproj` (precedent: `Controllers\BreadcrumbBridgeRouter.Descendants.cs`);
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (precedent: `OutlookObjects\Folder\BreadcrumbRowDescendantsIssue1002Tests.cs`);
- `QuickFiler.Test/QuickFiler.Test.csproj` (precedent: `Controllers\BreadcrumbBridgeRouterIssue1002Tests.cs`).

### Q10. Test inventory and seams

- **EFC router.** QuickFiler.Test `Controllers/BreadcrumbBridgeRouterTests.cs` with `.Selection.cs`, `BreadcrumbBridgeRouterIssue1002Tests.cs`, `BreadcrumbBridgeRouterStaleCompletionIssue1002Tests.cs`, `BreadcrumbBridgeRouterQueueTests.cs` with `.Part2.cs`, `BreadcrumbBridgeRouterIssue439Tests.cs` with `.Activation.cs`, `BreadcrumbBridgeRouterIssue614Tests.cs`, `BreadcrumbBridgeRouterIssue995Tests.cs`, `BreadcrumbBridgeRouterTrashIssue1002Tests.cs`, `BreadcrumbBridgeRouterScoreJoinTests.cs`, `BreadcrumbBridgeRouterIssue637Tests.cs` and `BreadcrumbBridgeRouterIssue792Tests.cs`.
  - Reusable fixture: `EfcDescendantRouterFixture` (internal sealed, declared in `BreadcrumbBridgeRouterIssue1002Tests.cs`). It extracts node ids from the rendered `data-node-id` attributes with a regular expression, because `BreadcrumbDescendantNode.Identity` is a fresh GUID that is not visible to QuickFiler.Test.
- **EFC model, renderer, codec and assets.** UtilitiesCS.Test `BreadcrumbRowStateTests.cs`, `BreadcrumbRowDescendantsIssue1002Tests.cs`, `BreadcrumbHtmlRendererTests.cs`, `BreadcrumbHtmlRendererIssue1002Tests.cs`, `BreadcrumbMessageCodecTests.cs`, `BreadcrumbMessageCodecIssue1002Tests.cs` and `BreadcrumbMessagesIssue1002Tests.cs`.
- **QFC model, session, router, messages and projection.** UtilitiesCS.Test `BreadcrumbStateModelTests.cs`, `BreadcrumbStateModelSequenceTests.cs`, `BreadcrumbStateModelSelectorTests.cs`, `BreadcrumbSelectionSessionTests.cs`, `BreadcrumbSelectionSessionHighlightTests.cs`, `BreadcrumbSubfolderSelectorSessionTests.cs`, `BreadcrumbDescendantSelectionIssue997Tests.cs`, `BreadcrumbDescendantValidationIssue997Tests.cs`, `BreadcrumbDescendantMessagesIssue997Tests.cs`, `FolderBreadcrumbBridgeRouterTests.cs` (with `EdgeTests`, `InFlightTests` and `ReplaceItemsTests`), `FolderBreadcrumbDescendantOrderingIssue997Tests.cs`, `FolderBreadcrumbRouterSelectionConcurrencyTests.cs`, `BreadcrumbSelectorMessagesTests.cs`, `BreadcrumbBridgeMessagesTests.cs`, `BreadcrumbSelectionMapTests.cs`, `BreadcrumbRenderProjectionTests.cs` and `BreadcrumbRenderProjectionSelectorTests.cs`.
- **QFC coordinator.** QuickFiler.Test `Viewers/BreadcrumbBridgeCoordinatorTests.cs`, `BreadcrumbSelectorCoordinatorTests.cs`, `BreadcrumbSubfolderActivationTests.cs` (its internal `SubfolderActivationHarness` can be reused), `BreadcrumbDescendantActivationIssue997Tests.cs`, `BreadcrumbDropDownIntegrationTests.cs` (internal `ItemViewerDropDownHarness`), `BreadcrumbPendingOpenCloseTests.cs` and `BreadcrumbDuplicateIdentityIntegrationTests.cs`.
- **Page-script text contracts.** QuickFiler.Test `FolderBreadcrumbAssetContractTests.cs`, `FolderBreadcrumbRecursiveAssetIssue997Tests.cs` and `FolderBreadcrumbNestingIssue1002Tests.cs` (regular-expression assertions over `FolderBreadcrumbDocument.Html`).
- **EFC `BridgeJs`.** It has no JavaScript execution test. The established pattern is `Contains`/regex assertions against the `BreadcrumbDocumentAssets.BridgeJs` constant (UtilitiesCS.Test `BreadcrumbHtmlRendererTests.Issue995BridgeJsPostsConfirmationForPlainEnterAndScrollsSelectedRowIntoView`, whose summary states that the suite has no headless-browser dependency).
- **Node harness** (`tests/quickfiler-breadcrumb/`).
  - `issue-997-source-loader.mjs` reads `QuickFiler/Resources/FolderBreadcrumb.js` once (overridable by `ISSUE_997_SCRIPT_FILE`).
  - `issue-997-browser-harness.mjs` runs the script with `node:vm` `runInNewContext` against a minimal fake DOM (`Element` with listeners, attributes, `querySelectorAll` by class, `focus`, `scrollIntoView`; bubbling `dispatch`) and captures `postMessage` payloads.
  - The suites are `issue-997-common.test.mjs`, `issue-997-recursive.test.mjs` and `issue-1002-nesting.test.mjs`, run with `node --test`.
  - They are not run in CI (finding 7).
- **Hard constraints** (CLAUDE.md UT4 and the General Unit Test Policy).
  - No temporary files.
  - No `Task.Delay`, `Thread.Sleep` or wall-clock waits: provider mocks use `ReturnsAsync`, and in-flight states use a `TaskCompletionSource`.
  - Tests must be safe under parallel execution: `TaskMaster.runsettings` and `UtilitiesCS.Test/Properties/AssemblyInfo.cs` set `Workers = 0` and `ClassLevel` scope, so no static mutable state may be shared.
  - Never assert literal node GUIDs.
  - The re-entrancy regression (finding 8) is tested deterministically by holding the first provider call on a `TaskCompletionSource`, delivering a second Right, then completing both.

## 4. Behaviour semantics consolidated (both viewers)

- **Visible order.** Row entries in display order. Each row's visible descendants follow it in pre-order. Banners are skipped (EFC), and so are non-selectable rows (QFC).
- **Up and Down** (AC1, AC2): ±1 in the visible order, with no wrap.
  - EFC: Up at index 0 focuses the search box; Down at the end does nothing.
  - QFC closed: unchanged row-only direct commit (AC10).
  - QFC open: pending only.
- **Ctrl+Left** (AC8):
  - on a node, select (EFC) or set pending (QFC open) on the parent node, or on the row entry for depth 0, with the expansion kept;
  - on a row entry, do the parent-segment double-click (activate `ActiveSegmentIndex - 1`, commit, `CollapseAfter`); with the active index at 0, do nothing;
  - QFC closed: commit immediately.
  - Ctrl+Right is Right.
- **Collapse that hides the highlight** (AC7): the highlight moves to the collapsed node, or to the row entry. In EFC this is a commit.
- **Failure handling.** Unknown row ids, unknown node ids, rejected archive-relative paths and provider faults keep today's handling: a logged no-op, with state unchanged (`ProcessInboundAsync`, `SelectHierarchyPath`, `HandleDescendantToggleAsync`, `FetchAndAttachSubfoldersAsync`).

## Open Questions for the Maintainer

**Q-A. AC5 "restore the full path" versus AC9 "Right lists the children".** After AC7 removes Left segment hiding, a top-level row can be shortened only by a double-click (or by Ctrl+Left, defined as the same action). AC9 then defines Right as listing that folder's children.

- Before (today): `Archive > Clients [+]` (shortened); Right gives `Archive > Clients > Beta`.
- After, AC9 reading (recommended): `Archive > Clients [+]`; Right gives `Archive > Clients [−]` with `Acme, Beta, Gamma` listed; a second Right selects Acme.
- After, AC5 reading: `Archive > Clients [+]`; Right gives `Archive > Clients > Beta`.

Recommendation: AC9 governs, and AC5's restore clause has no remaining trigger. Please confirm.

**Q-B. QFC Ctrl+Left on a top-level row with the list open, then Escape.** AC8 says "moves only the pending highlight", but the parent-segment action changes the row itself.

- Before Escape: committed `Archive > Clients > Beta`; list open; Ctrl+Left; the row shows `Archive > Clients [+]` with the pending highlight on it.
- After Escape, recommended: the row returns to `Archive > Clients > Beta` and the committed value stays Beta. This needs the open session to snapshot and restore the row's `ActiveSegmentIndex` and `CollapsedAfterIndex`.
- After Escape, alternative: the row stays `Archive > Clients [+]`, but the committed value is still Beta, so the closed surface would display a row that is not the filing value.

**Q-C. QFC PageUp and PageDown with the list closed.** AC10 lists Right, Left, Up, Down, Home and End for the closed list, but not PageUp or PageDown.

- Before: PageDown is not handled; the browser scrolls the collapsed surface.
- After, option 1: nothing.
- After, option 2: commit the top-level row `pageSize` rows down, as closed Up and Down do.

Recommendation: option 1, because the closed surface shows one row and has no page.

**Q-D. EFC keys with no selection.** The page sends `rowId: ''` when no `.rowwrap.selected` exists, and `ProcessInboundAsync` rejects an unknown row.

- Before: nothing selected; Down does nothing.
- After, recommended: Down and Home select the first entry; Up and End select the last entry (Up as the AC2 boundary is then unchanged only once something is selected).
- After, alternative: unchanged.

**Q-E. QFC single click on a non-leaf segment in the open list.** Today the click bubbles to the row and commits the row's leaf, so AC9's double-click can never arrive in the open list.

- Before: open list, click once on `Clients` in `Archive > Clients > Beta`; Beta is committed and the list closes.
- After, recommended: the first click on a non-leaf segment does not bubble and moves only the pending highlight to the row; the double-click then commits Clients and closes the list. A click on the leaf segment or the percentage keeps today's commit.

**Q-F. QFC Right on an expanded node whose fetch returned no children.**

- Before: not reachable by keyboard.
- After, recommended: nothing. This is not treated as a leaf and does not open the pop-out dialog.

**Q-G. QFC Right with the list closed while the committed choice is a subfolder.** AC10 says the pending highlight is placed "on the committed row".

- Before: committed `Projects > Alpha` with subfolder `Specs` committed; Right opens the pop-out dialog or expands the row.
- After, recommended: the list opens with the pending highlight on `Specs` when it is visible, and otherwise on the row.

## Automation Feasibility

- **Fully automatable (C#, MSTest, Moq, FluentAssertions, headless):** visible order, Up and Down, Home and End, PageUp and PageDown given a `pageSize`, Right and Left decisions, Ctrl+Left, the double-click state transitions, the nearest-visible-ancestor re-commit, the QFC pending, commit, cancel and close transitions, the closed-list Right opening the selector, the cancel-then-`UnhandledArrow` ordering, the codec and serializer key sets, and the re-entrant Right guard. Every one of these runs through `BreadcrumbBridgeRouter.ProcessInboundAsync`, `FolderBreadcrumbBridgeRouter`/`BreadcrumbSelectionSession` and `BreadcrumbBridgeCoordinator` with mocked `IFolderHierarchyProvider`, `IBreadcrumbWebHost` and `IWebViewMessenger`.
- **Automatable locally, with no CI signal (node `node:test`):** `FolderBreadcrumb.js` key mapping (Ctrl+Left, Home, End, PageUp, PageDown), the `pageSize` count from stub rectangles, the pending-node `.sub.active` highlight, and the non-bubbling non-leaf segment click. Each one gets a matching C# string-contract assertion.
- **Text-contract only:** EFC `BridgeJs` key mapping, `pageSize` measurement and scroll target. No JS engine is available to the C# suite.
- **Human step (manual Outlook verification):**
  - real viewport page sizes;
  - focus moving to the search box;
  - the popup visibly closing before the "Pop Out Item or Enumerate Conversation?" dialog paints;
  - double-click timing in WebView2;
  - holding an arrow key with auto-repeat.

## Recommended Design

**Production files to change:**
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs`: drop the trailing-segment hiding from `LeftArrow`; allow the expansion while `CollapsedAfterIndex == ActiveSegmentIndex`; add an idempotent expand-to-true operation used by `ExpandLeafAsync`.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.Descendants.cs`: `AppendVisiblePositions`; collapse operations that report whether the selected node was hidden.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs`: collapse affordance at the terminal segment while expanded.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs` (`BridgeJs`): `ParentLeft`, `Home`, `End`, `PageUp` and `PageDown` with `pageSize`; Ctrl+Right as Right; scroll the selected `.child` when present.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs` and `BreadcrumbMessages.cs`: optional `pageSize`.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` and `BreadcrumbStateModel.Row.cs`: remove the #440 walks; collapsed-anchor expansion; `VisiblePositions()`.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs`: `Cancel` and `CommitPending` honour `PendingNode`.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionMap.cs`: `GetSelectedFolder` uses the active-segment value.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectorMessages.cs`: new keys, `pageSize`, `pendingNodeIdentity`, and a selector-channel segment double-click if Q6's recommendation is adopted.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs`: `Minus` at the collapsed terminal while expanded.
- `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` and `.Descendants.cs`: pending-aware arrows; the AC7 pending move on toggle collapse.
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`, `.Arrows.cs`, `.Selection.cs` and `.Descendants.cs`: the new key arms; `SelectRowEntry`; the AC9 double-click and `[+]`; the AC7 re-commit; the re-entrancy guard.
- `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`: routing only, delegating to the new partial.
- `QuickFiler/Resources/FolderBreadcrumb.js`: new keys, `pageSize`, the `onArrow` pre-filter removed, the pending-node highlight, non-leaf segment click handling.

**Production files to create (each with a `<Compile Include>`):**
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbVisibleOrder.cs` (UtilitiesCS.csproj);
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.Tree.cs` (UtilitiesCS.csproj);
- `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Navigation.cs` (UtilitiesCS.csproj);
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.Navigation.cs` (QuickFiler.csproj);
- `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Keyboard.cs` (QuickFiler.csproj).

**Test files to create:**
- UtilitiesCS.Test `OutlookObjects/Folder/BreadcrumbVisibleOrderTests.cs`;
- UtilitiesCS.Test `OutlookObjects/Folder/BreadcrumbSelectionSessionTreeIssue1007Tests.cs`;
- UtilitiesCS.Test `OutlookObjects/Folder/FolderBreadcrumbBridgeRouterIssue1007Tests.cs`;
- QuickFiler.Test `Controllers/BreadcrumbBridgeRouterIssue1007Tests.cs`, split into `.Navigation.cs` and `.DoubleClick.cs` partials as needed to stay under 500 lines;
- QuickFiler.Test `Viewers/BreadcrumbBridgeCoordinatorIssue1007Tests.cs`;
- node `tests/quickfiler-breadcrumb/issue-1007-keys.test.mjs`, with stub rectangles added to `issue-997-browser-harness.mjs`.

Existing tests are updated as listed in Q4.

**Task ordering** (bugfix workflow: failing regression tests first):
1. Record the baseline: the full C# toolchain and the node suites, under `<FEATURE>/evidence/baseline/`.
2. Write the failing tests for the pure helper (`BreadcrumbVisibleOrderTests`). They fail to compile or fail until the helper exists; record this as expect-fail evidence.
3. Write the failing EFC router tests (AC1, AC2, AC3, AC5, AC6, AC7, AC8, AC9, AC11, plus the re-entrant Right) and the failing QFC session, router and coordinator tests (AC4, AC5, AC6, AC7, AC8, AC9, AC10, AC11). Rewrite the pinned #440, Left-hiding and `[+]`-restore tests to their new expectations, with the reason recorded in each test's summary. Capture the failing run under `<FEATURE>/evidence/regression-testing/`.
4. Write the failing text-contract and node tests for `BridgeJs` and `FolderBreadcrumb.js`.
5. Implement `BreadcrumbVisibleOrder` and the row and model members (UtilitiesCS).
6. Implement the EFC router changes (QuickFiler).
7. Implement the QFC session, router, messages, projection and coordinator changes.
8. Implement the page-script changes.
9. Run the full toolchain loop (CSharpier, then analyzers with `/t:Rebuild`, then `/p:TreatWarningsAsErrors=true` with `/t:Rebuild`, then `Invoke-MSTestWithCoverage.ps1`) and the node suites in one clean pass. Check per-file coverage of the changed files against the 80% line and 75% branch floors (90% for new code).
10. Manual Outlook verification (human step).

**Rejected alternatives (brief):**
- Per-viewer traversal: duplicate logic.
- Computing the visible order in the page script and posting a target: untested in CI, and the page would become authoritative.
- A `ctrl` flag on QFC `arrowKey`: it would reach `BreadcrumbArrowDirection` consumers that equate "not Right" with Left.
- A fixed page size of 10: rejected by the maintainer (Q17).

## Numeric Derivation Evidence

No numeric count, enumeration or population is proposed for any `spec.md` acceptance criterion. The line counts in Q9 are inputs to the 500-line budget, not acceptance assertions. They were taken by two independent methods (ripgrep line counts, and the last numbered line from a full Read), which agreed for every file read in full.

## Pre-Mortem

- **Review checklists:** a reviewer could flag that the #440 tests were rewritten without a recorded reason, or that invariant I3 was narrowed silently. Mitigation: every rewritten test carries a summary line naming "#1007 AC7/AC9", and the plan records the I3 narrowing (Q2) as a decision.
- **Coverage:** new partials (`BreadcrumbVisibleOrder`, `.Tree.cs`, `.Navigation.cs`, `.Keyboard.cs`) below 90%, or changed lines of the routers losing coverage, would fail AC13. Mitigation: each new decision branch in the Q4 table gets a test, including the in-flight, zero-children and archive-root-stop branches. Per-file figures are read from the committed coverage projection. `KeyboardHandler` remains coverage-exempt and is not changed.
- **Mirror parity:** not applicable to `.claude/**`, because this item edits no push-down-owned file. The internal parity that does apply is between the C# key enums and parsers and the two page scripts. Mitigation: each new key value has a C# serializer round-trip test, a page-script text-contract assertion and a node test.
- **File size:** extending `BreadcrumbSelectionSession.cs` (446), `BreadcrumbBridgeCoordinator.cs` (451), `BreadcrumbBridgeRouter.cs` (471) or any test file of 487 lines or more would breach 500. Mitigation: new partials per Q9. `BreadcrumbSelectorCoordinatorTests.cs` (500) and `FolderBreadcrumbBridgeRouterTests.cs` (495) are edited in place without growth.
- **Test isolation:** tests that assert GUID node ids, share a static router or fixture, or use real waits would break under `Workers = 0`/`ClassLevel` parallelism. Mitigation: a fixture instance per test (`EfcDescendantRouterFixture` is instance-based), ids extracted from the rendered output, and `TaskCompletionSource` for in-flight provider calls. No temporary files.
- **CI-only behavior:** the node suites never run in CI, so a page-script regression could pass CI unnoticed. Mitigation: pair every node assertion with a C# regular-expression contract test over `FolderBreadcrumbDocument.Html` or `BreadcrumbDocumentAssets.BridgeJs`. Record the local node run as evidence. Orchestrator to confirm that the TaskMaster workflows (`_format-check.yml`, `_build-analyzers.yml`, `_build-nullable.yml`, `_mstest-coverage.yml`, `_pester.yml`) are still the complete required set.
- **Evidence:** missing expect-fail evidence for the rewritten tests, or evidence written outside `<FEATURE>/evidence/<kind>/`, would block the audit. Mitigation: the baseline goes under `evidence/baseline/`, the failing run under `evidence/regression-testing/`, and the final gates under `evidence/qa-gates/`, each with `Timestamp`, `Command`, `EXIT_CODE` and `Output Summary`. Test-run evidence is committed as a summary derived from the TRX, never as the raw document.
