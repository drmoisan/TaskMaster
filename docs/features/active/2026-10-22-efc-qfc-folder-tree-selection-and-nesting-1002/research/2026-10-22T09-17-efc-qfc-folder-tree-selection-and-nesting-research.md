<!-- markdownlint-disable-file -->

# Task Research Notes: EFC recursive subfolder selection, EFC trash selection, QFC descendant nesting (#1002)

- Issue: #1002
- Branch: bug/efc-qfc-folder-tree-selection-and-nesting-1002
- Researcher: task-researcher (read-only; no source, configuration, or test changes; no commands executed)
- Timestamp note: the orchestrator renamed this file to the UTC clock reading `2026-10-22T09-17` (`date -u`) taken when the file was accepted; the researcher's estimate `2026-10-22T09-30` was later than the clock.

## Orchestrator Decisions on Open Questions (2026-10-22)

1. Trash acceptance wording: after the trash button is pressed, the trash entry is the selected row and `SelectedFolder` resolves to the trash entry. The prior selection is not asserted beyond that.
2. QFC nested affordance: the +/− follows the folder name, matching the placement on top-level rows. It is not flush right.
3. EFC keyboard scope: parity with #997 only. Descendant toggles are focusable and respond to Enter and Space, as the QFC toggles do. New arrow-key navigation into deeper levels is out of scope, as #997 excluded new keyboard features.
4. First-level `ExpandLeafAsync` stale-render exposure: in scope. It is in a file this item already changes and has the same root-cause class as the new descendant fetch, so the same stale-completion guard is applied to it.
- Line numbers below describe the session checkout at the time of reading. They are research evidence only; a plan must cite symbols, not lines (plan gate G11).

## Summary of Root Causes

| # | Defect | Root cause (verified) |
|---|---|---|
| 1 | EFC has no recursive subfolder selection | EFC does not use the #997 surface at all. It uses a separate generated-document surface (`QuickFiler.Controllers.BreadcrumbBridgeRouter` + `UtilitiesCS...BreadcrumbHtmlRenderer` + `BreadcrumbDocumentAssets.BridgeJs`). That surface stores only one flat list of first-level children as key-less `BreadcrumbSegment` objects, renders them without any expand affordance, and has no inbound message that can address a child below the first level. None of the #997 code is reachable from EFC. |
| 2 | EFC trash entry is created but not selected | `ActionDeleteAsync` -> `ApplyDeleteGesture` prepends the trash row and calls the fire-and-forget `BindFolderRows`; `BreadcrumbBridgeRouter.BindRowsAsync` then deliberately clears the selection (#499) and re-navigates the document with no selected row. No code path ever selects the trash row. `EfcDataModel` is not involved. |
| 3 | QFC nested expand affordance on the left; no per-level indent | `FolderBreadcrumb.js` `makeSubfolders` appends the toggle span before the name span, so the toggle is the first flex item. Indentation comes only from the fixed `.sub { padding: 1px 4px 1px 24px; }` rule; nested `.subs` containers have no padding or margin, so every level renders at the same 24px indent. Depth is not carried on `BreadcrumbDescendantNode` or the render DTO, but it does not need to be: `makeSubfolders` is already recursive and can pass a depth argument. |

## Research Executed

### Files read end-to-end (or the relevant region)

- Feature: `issue.md`, `spec.md`, `plan.2026-10-22T05-08.md` (template, unfilled).
- #997 archive: `research/research.2026-10-18T00-45.md`; `evidence/regression-testing/issue-997-javascript-behavior.2026-10-18T01-53.md`.
- EFC: `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs`, `EfcFormController.Actions.cs`, `EfcFormController.EventHandlers.cs` (60-249), `EfcFormController.cs` (200-266), `BreadcrumbBridgeRouter.cs`, `BreadcrumbBridgeRouter.Selection.cs`, `BreadcrumbBridgeRouter.Arrows.cs`, `BreadcrumbOutboundQueue.cs`, `QuickFiler/Viewers/WebView2BreadcrumbHost.cs`.
- UtilitiesCS (EFC model): `BreadcrumbRow.cs`, `BreadcrumbSegment.cs`, `BreadcrumbHtmlRenderer.cs`, `BreadcrumbDocumentAssets.cs`, `BreadcrumbMessageCodec.cs`, grep of `BreadcrumbMessages.cs` and `BreadcrumbRowBuilder.cs`.
- UtilitiesCS (QFC model): `BreadcrumbDescendantNode.cs`, `FolderBreadcrumbBridgeRouter.Descendants.cs`, `BreadcrumbRenderProjection.cs` (60-289).
- QFC page: `QuickFiler/Resources/FolderBreadcrumb.js` (full), `FolderBreadcrumb.html` (85-177).
- Tests: `tests/quickfiler-breadcrumb/issue-997-browser-harness.mjs`, `issue-997-recursive.test.mjs`, `issue-997-source-loader.mjs`; `QuickFiler.Test/Viewers/FolderBreadcrumbRecursiveAssetIssue997Tests.cs`; `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` (1-120, 355-434); `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs` (70-99); greps across `*.Test/**` for pinned markup and JS.

### Searches

- `InitializeBreadcrumbPipeline|EnsureBreadcrumbPipeline` in `QuickFiler`: only `QfcItemController` calls it, so the EFC form never constructs the #997 pipeline (its embedded `ItemViewer` included).
- `Resources\.FolderBreadcrumb|FolderBreadcrumbBridgeRouter|BreadcrumbBridgeCoordinator|new BreadcrumbBridgeRouter` in `QuickFiler`: EFC constructs only `new BreadcrumbBridgeRouter(` (`EfcFormController.Breadcrumb.cs:80`).
- `InternalsVisibleTo` in `UtilitiesCS`: granted to `UtilitiesCS.Test`, `ToDoModel.Test`, `DynamicProxyGenAssembly2` (`UtilitiesCS/Properties/AssemblyInfo.cs:18-20`). Not granted to `QuickFiler`, so `internal BreadcrumbDescendantNode` is not visible to the EFC router.
- `BreadcrumbHtmlRenderer|BreadcrumbMessageCodec|new BreadcrumbRow(|BreadcrumbRowBuilder` in production code: the EFC model types are used only by the EFC router, EFC controller files, and `EfcSelectionGuard`; QFC files reference only the `BreadcrumbRowBuilder.BannerPrefix` constant (`BreadcrumbStateModel.Row.cs:393`, `FolderSuggestionTree.cs:198`).
- `quickfiler-breadcrumb|node --test|\.mjs` in `.github/**`, `package.json`, `.vscode/tasks.json`, `scripts/**`: no matches. The node `.mjs` suites are not run by TaskMaster CI.

## Question 1: How EFC presents and selects folders, and the #997 gap

### Current EFC surface (separate from QFC)

- Wiring: `EfcFormController.ConfigureBreadcrumbControl` (`EfcFormController.Breadcrumb.cs:69-92`) creates `WebView2BreadcrumbHost` over `EfcViewer.BreadcrumbWebView` (`EfcViewer.cs:85`, alias of the Designer `FolderListBox` WebView2), an `OutlookFolderHierarchyProvider`, and `new BreadcrumbBridgeRouter(provider, host, BreadcrumbMessageCodec, BreadcrumbHtmlRenderer, BreadcrumbOutboundQueue)`.
- Document: `BreadcrumbHtmlRenderer.RenderDocument` inlines `BreadcrumbDocumentAssets.BaseCss`, a theme block, row fragments, and `BridgeJs` (`BreadcrumbHtmlRenderer.cs:32-52`). It is delivered by `NavigateToString` (`BreadcrumbBridgeRouter.Selection.cs:183-195`).
- Row model: `BreadcrumbRow` (`UtilitiesCS`) holds `_leafChildren` as `List<BreadcrumbSegment>` (`BreadcrumbRow.cs:37`). `BreadcrumbSegment` carries only `FullPath`, `DisplayName`, `HasSubfolders`; it has no provider key (`BreadcrumbSegment.cs:29-43`).
- Expansion: `leafExpandToggle` -> `HandleLeafToggleAsync` -> `ExpandLeafAsync` (`BreadcrumbBridgeRouter.Arrows.cs:17-40, 165-209`) calls `_provider.GetImmediateSubfoldersAsync(row.ActiveSegmentKey, ...)`, maps the result through `BreadcrumbRowBuilder.MapSegments` (which drops each child's `FolderTreeNodeKey`), stores it with `row.SetLeafChildren(mapped)`, posts `subfolderResult`, then posts a row render.
- Child rendering: `AppendChildren` (`BreadcrumbHtmlRenderer.cs:213-232`) emits a flat `<div class="child" data-child-index="i">name</div>` per first-level child. There is no affordance, no node identity, and no nested container.
- Child selection: `BridgeJs` posts `renderedChildActivate {rowId, childIndex}` for `.child[data-child-index]` (`BreadcrumbDocumentAssets.cs:79-88`); the router's `ActivateChild` (`BreadcrumbBridgeRouter.Selection.cs:36-48`) calls `SelectHierarchyPath`, which converts the rooted hierarchy path to the archive-relative stem and commits it (`Selection.cs:121-151`). `SelectedFolderPath` is the controller's `SelectedFolder` (`EfcFormController.SetupAndProperties.cs:179-184`), which `ActionOkAsync` validates with `EfcSelectionGuard.IsValidFilingSelection` and files through `EfcHomeController.ExecuteMovesAsync` (`EfcFormController.Actions.cs:30-64`, `EfcHomeController.ExecuteMoves.cs:69`).
- Inbound contract: the codec accepts only seven types (`BreadcrumbMessageCodec.cs:132-141`), each addressed by `rowId` plus integer `segmentIndex`/`childIndex`; no string node identity field exists (`BreadcrumbMessageCodec.cs:100-129`).

### #997 pieces present for QFC and absent for EFC

| #997 capability (QFC location) | EFC equivalent | Status |
|---|---|---|
| Lazy keyed descendant tree `BreadcrumbDescendantNode` with `Identity`, `Segment` (keyed `FolderBreadcrumbSegment`), `Expanded`, `Loaded`, `RequestGeneration`, `Children`, `Find`, `Collapse`, `Attach` (`BreadcrumbDescendantNode.cs:9-71`) | `BreadcrumbRow._leafChildren` flat, key-less | Absent |
| Descendant toggle with stale-completion rejection: `FolderBreadcrumbBridgeRouter.ToggleDescendantAsync` + `FetchAndAttachSubfoldersAsync` + `IsCurrentRequest` (`FolderBreadcrumbBridgeRouter.Descendants.cs:19-51, 112-199`) | none | Absent |
| Descendant activation by stable identity: `ActivateSelectorDescendant(rowIdentity, nodeIdentity)` (`Descendants.cs:13-16`) | `renderedChildActivate` by first-level integer index only | Absent below level 1 |
| Selected-descendant tracking and highlight (`BreadcrumbRenderProjection.ProjectDescendant`, `selected` flag, `selectedDescendantIdentity` in the page) | Row-level `_selectedRowId` only | Absent |
| Page: per-child `+/−` toggle posting `selectorDescendantToggle`, recursive `makeSubfolders`, activation posting `selectorDescendantActivate` (`FolderBreadcrumb.js:145-203`) | `BridgeJs` has one row-level `.affordance` handler and a flat `.child` handler | Absent |
| Coordinator dispatch (`BreadcrumbBridgeCoordinator.Descendants.cs`) and wiring (`QfcItemController.BreadcrumbWiring.cs`, `ItemViewer.Breadcrumb.cs`) | EFC router subscribes to `IBreadcrumbWebHost.MessageReceived` directly (`BreadcrumbBridgeRouter.cs:62, 446-457`) | Not applicable to EFC; no coordinator layer exists or is needed |

The QFC filing-value fix of #997 (archive-relative stem for child selections) is already present in EFC through `SelectHierarchyPath`, so EFC needs only the tree, the addressing, and the rendering.

### Candidate approaches

**A. Extend the EFC surface with a keyed descendant tree (recommended).**
Keep the EFC router, renderer, codec, and `BridgeJs`. Store keyed descendants on `BreadcrumbRow` by reusing the existing `internal BreadcrumbDescendantNode` from inside `UtilitiesCS` (same assembly, so no visibility change). Add two inbound types addressed by `rowId` plus a string node identity. Render nested children recursively with the affordance after the name and one indent step per level.

- Advantages: smallest change set; preserves every EFC-specific behavior (banner rows, trash pseudo-row, #440 arrow transitions, #799 absence suppression, #995 Enter confirmation, #614 archive-root validation); filing still flows through `SelectHierarchyPath`; reuses the #997 node type and its stale-request discipline; no QFC file changes.
- Limitations: EFC's inline JS is a C# string constant, so its click handlers can be verified only by text-contract tests (the existing EFC pattern), not by the node harness.

**B. Replace the EFC surface with the QFC `FolderBreadcrumb.html` selector and `FolderBreadcrumbBridgeRouter`.**
- Rejected: the QFC selector is a collapsed/popup listbox with its own state model and keyboard contract; it has no banner rows, trash pseudo-row, segment activation, #440 tree transitions, or #995 `rowConfirmed` path. Porting EFC onto it is a rewrite of the EFC folder area and would put QFC-shared files in the change set, contrary to the minimal-fix rule.

**C. Index-path addressing (for example `"0/2/1"`) on the existing flat model.**
- Rejected: an index path is invalidated by any re-fetch or rebind (row ids are `row-<index>` and are reused across binds), which reintroduces the stale-address defects #997 removed with stable identities.

### Recommended design for defect 1 (EFC)

State model (UtilitiesCS, new partial file of `BreadcrumbRow`; `BreadcrumbRow.cs` is 361 lines and gains only the `partial` modifier and the reset calls):

- `BreadcrumbRow` owns an optional descendant root `BreadcrumbDescendantNode` built from the active segment's key when first-level children are supplied as keyed `FolderBreadcrumbSegment`s. The existing `SetLeafChildren(IEnumerable<BreadcrumbSegment>)` stays unchanged for compatibility (existing tests call it with key-less segments; those rows simply have no expandable descendants).
- New public members (names are proposals): `SetLeafDescendants(IReadOnlyList<FolderBreadcrumbSegment>)` (sets `_leafChildren` to the mapped segments and builds the keyed tree); `TryBeginDescendantExpand(string nodeId, out FolderTreeNodeKey key, out int generation)`; `bool TryCollapseDescendant(string nodeId)`; `bool TryAttachDescendantChildren(string nodeId, int generation, IReadOnlyList<FolderBreadcrumbSegment>)` (rejects when the node is no longer in the tree, no longer expanded, or the generation moved); `string? GetDescendantPath(string nodeId)`; `string? SelectedDescendantId` with a setter method that validates the identity.
- `ActivateSegment`, `CollapseAfter`, `LeftArrow`, and leaf collapse must clear the descendant tree and `SelectedDescendantId` exactly where they already clear `_leafChildren` (`BreadcrumbRow.cs:169-170, 227, 300`).

Router (QuickFiler, new partial `BreadcrumbBridgeRouter.Descendants.cs`):

- `ExpandLeafAsync` passes the provider's keyed `children` to `row.SetLeafDescendants` instead of mapping and discarding keys (`Arrows.cs:185-190`).
- `descendantToggle {rowId, nodeId}`: collapse when expanded and post a row render; otherwise begin expand, `await _provider.GetImmediateSubfoldersAsync(key, CancellationToken.None)` (no `ConfigureAwait(false)`, matching `ExpandLeafAsync`), then attach only when `ReferenceEquals(FindRow(row.RowId), row)` and `TryAttachDescendantChildren` succeeds; post one row render (no `subfolderResult`, mirroring #997 where descendant expansion returns only a render, `FolderBreadcrumbBridgeRouter.Descendants.cs:169-172`). Provider faults and cancellation are logged and leave state unchanged, as in `ExpandLeafAsync`.
- `descendantActivate {rowId, nodeId}`: resolve the path with `GetDescendantPath`; reject unknown identities with a log line; otherwise set `SelectedDescendantId` and call the existing `SelectHierarchyPath(row, path)`, which enforces the archive-relative contract and raises `SelectedFolderPathChanged`. `ConfirmRow` already accepts it because `_selectedRowId` becomes the containing row (`Selection.cs:153-166`).
- `ProcessInboundAsync` (`BreadcrumbBridgeRouter.cs:395-443`) gains two `case` arms that delegate to the new partial; `BreadcrumbBridgeRouter.cs` is 459 lines, so the arms must stay one-line delegations.

Codec and messages (UtilitiesCS): add `DescendantToggle = "descendantToggle"` and `DescendantActivate = "descendantActivate"` to `BreadcrumbMessageTypes`, an optional `NodeId` string on `BreadcrumbInboundMessage` (optional constructor parameter, source-compatible), registration in `IsKnownInboundType`, and a required non-empty `nodeId` check for both types, failing fast like the existing `childIndex` check.

Renderer and assets (UtilitiesCS):

- `AppendChildren` renders recursively from the descendant tree when one exists, otherwise exactly as today. Each child: `<div class="child" data-child-index="i" data-node-id="..." data-depth="d">` (first-level children keep the `class="child" data-child-index="0"` prefix pinned by `BreadcrumbHtmlRendererTests.cs:89`), a name span, then, only when `HasChildren`, `<span class="affordance node" data-role="node">+</span>` or `&#8722;`. An expanded node is followed by a nested `<div class="children">`; `.children { margin-left: 18px; }` (`BreadcrumbDocumentAssets.cs:28`) then compounds per level without a CSS change. Mark the selected descendant with an attribute (for example `aria-selected="true"`) rather than a second class so the pinned prefix survives.
- `BridgeJs` click handler: add a `.affordance[data-role="node"]` branch that posts `descendantToggle` and stops propagation, placed BEFORE the `.child` branch (the affordance sits inside the `.child` element, and the current `.child` branch at `BreadcrumbDocumentAssets.cs:79-88` would otherwise swallow it); add a `.child[data-node-id]` branch posting `descendantActivate`; keep the existing `renderedChildActivate` branch for key-less children (pinned by `BreadcrumbHtmlRendererTests.cs:91`). Add `.child { display: flex; align-items: center; }` so the affordance renders to the right of the name.

## Question 2: EFC trash button

### Current behavior

1. `BtnDelItem.Click` -> `ButtonDelete_Click` -> `ButtonDeleteClickAsync` -> `ActionDeleteAsync` (`EfcFormController.EventHandlers.cs:71, 204-217`); the `T` key routes to the same method (`EventHandlers.cs:278, 350`).
2. `ActionDeleteAsync` awaits `_formViewer.UiSyncContext`, then calls synchronous `ApplyDeleteGesture` (`EfcFormController.Actions.cs:95-105`).
3. `ApplyDeleteGesture` sets `_folderRows = WithTrashRow(_folderRows)` (prepends `"Trash to Delete"` idempotently, `Actions.cs:79-92`) and calls `BindFolderRows`, which discards the task of `BindBreadcrumbRowsAsync` (`EfcFormController.Breadcrumb.cs:163-172`).
4. `BreadcrumbBridgeRouter.BindRowsAsync` rebuilds rows (the trash text is classified `TrashPseudoRow`, `BreadcrumbRowBuilder.cs:164-166`), sets `_selectedRowId = null`, nulls `SelectedFolderPath` and raises `SelectedFolderPathChanged(null)` when a selection existed (#499), then `DeliverDocument` navigates a document with no selected row (`BreadcrumbBridgeRouter.cs:157-179`).
5. Nothing selects the new row. `EfcDataModel` is not mutated by the gesture; its only trash reference is the attachment decision during a move (`EfcDataModel.cs:281`).

Observation: per the code, the router clears the previous selection rather than keeping it, so after the gesture `SelectedFolder` is null and OK reports "Please select a valid folder." The maintainer's description ("the selection stays where it was") could not be reconciled with the code by reading alone; either way the required outcome is the same. ORCHESTRATOR/MAINTAINER TO CONFIRM if the distinction matters for the acceptance wording.

The QFC analogue does select it: `QfcItemController.MailActions.cs:239-243` adds the trash item if missing and calls `SetFolderSelectedItem("Trash to Delete")`.

### Why "select after bind" is unsafe

Selecting with the existing `SelectRow` after the bind would commit state correctly but would deliver the highlight as a posted `render` message (`CommitSelection`, `Selection.cs:143-151`) immediately after `NavigateToString` replaced the document. `WebView2BreadcrumbHost` posts with `PostWebMessageAsJson` to whatever document is current (`WebView2BreadcrumbHost.cs:210-248`) and nothing waits for navigation completion; `BreadcrumbOutboundQueue` gates only on core initialization (`BreadcrumbOutboundQueue.cs:37-52`). The posted render can therefore reach the outgoing document or arrive before the new page has registered its listener, leaving the highlight missing even though `SelectedFolder` is correct. This is an inference from the WebView2 delivery model; it is not reproducible in a unit test.

### Recommended fix for defect 2

Make the selection part of the bound document:

- Router: add an internal `BindRowsAsync` overload with an optional `string? preferredRowText`. After rows are rebuilt and the #499 clear has run, if a row whose presented text equals `preferredRowText` exists (for the trash text: `Kind == TrashPseudoRow`), set `_selectedRowId` and `SelectedFolderPath` through the same normalization as `SelectRow` and raise `SelectedFolderPathChanged` once, before `DeliverDocument`, so `RenderDocument(_rows, _darkMode, _selectedRowId)` emits `rowwrap selected` for that row. The existing four-argument overload delegates with `null`, so every current caller is unchanged. Put the helper in `BreadcrumbBridgeRouter.Selection.cs` (236 lines) to keep `BreadcrumbBridgeRouter.cs` (459 lines) under the limit.
- Controller: replace the synchronous `ApplyDeleteGesture` with an awaited form: set `_folderRows = WithTrashRow(_folderRows)`; return when `_formViewer` or `_router` is null (preserves the guard the existing `ActionDeleteAsync_AwaitedTwice_LeavesExactlyOneTrashRowInFolderRows` test relies on, `EfcFormControllerTests.cs:385-416`); otherwise `await` a bind that passes `TrashRowText` as the preferred row. `ActionDeleteAsync` awaits it.

Rejected alternative: post-bind `SelectRow` (navigation race above).

## Question 3: QFC descendant row layout

### Evidence

- DOM order: in `makeSubfolders` the toggle is appended at `FolderBreadcrumb.js:177` and the name at `:179`, so for a child with `hasChildren` the `.sub` flex row is `[toggle][name]`. No CSS `order`, `float`, or `row-reverse` is involved; `.sub` is `display: flex` (`FolderBreadcrumb.html:151-157`).
- Top-level parity: the top-level leaf affordance is projected as a cell after the terminal segment (`BreadcrumbRenderProjection.cs:230-241`) and rendered in cell order (`FolderBreadcrumb.js:134`), so on top-level rows the `+/−` follows the name. (The collapsed-chain re-expand plus deliberately sits left of its segment, `BreadcrumbRenderProjection.cs:217-223`; it is unrelated to descendant rows.)
- Indent: nested lists are appended as `.subs` inside the parent `.subs` (`FolderBreadcrumb.js:198-200`), but `.subs` carries only a background (`FolderBreadcrumb.html:150`) and every `.sub` has the same fixed `padding-left: 24px` (`:155`). All levels therefore share one indent.
- Depth availability: `BreadcrumbDescendantNode` has no depth (`BreadcrumbDescendantNode.cs:9-71`) and `BreadcrumbSubfolderRender` has none (`BreadcrumbRenderProjection.cs:60-91`). Depth is implicit in the recursive `Children` nesting, which the page already walks recursively; no C# change is required.

### Recommended fix for defect 3 (JS only)

- In `makeSubfolders`, append `name` first, then the toggle, so the affordance is to the right of the name at every level, matching the top-level cell order.
- Add a `depth` parameter (top-level call in `render` passes `0`; the recursive call at `FolderBreadcrumb.js:199` passes `depth + 1`). On each `.sub`, set `data-depth` and an explicit left padding of `24 + 16 * depth` pixels through `setAttribute("style", "padding-left: ...px")` (the harness `Element` has `setAttribute` but no `style` object, so this form needs no harness change). The full-width highlight of `.sub.selected` is preserved because the indent is padding on the row itself.
- Keep the recursive call text `makeSubfolders(row, child.children` so the pins in `FolderBreadcrumbRecursiveAssetIssue997Tests.cs:18` and `FolderBreadcrumbAssetContractTests.cs:357` still match, and add no function between `makeSubfolders` and `render` (`FolderBreadcrumbAssetContractTests.cs:317-318` slices bodies by adjacent function names).

Rejected alternative: CSS-only `.subs .subs { padding-left: 16px; }`. It indents correctly, but it indents the selection highlight too, and a node-harness test cannot observe CSS, so the fix would be verifiable only by a text assertion on the stylesheet.

Open interpretation: "on the right" is taken as "immediately after the name", matching top-level rows, not "flush with the right edge". MAINTAINER TO CONFIRM if right-edge alignment is intended (that would add `margin-left: auto` on the descendant toggle).

## Question 4: Test seams and locations

All proposed tests are in-memory: Moq strict mocks of `IFolderHierarchyProvider` and `IBreadcrumbWebHost`, captured `NavigateToString`/`PostMessageJson` strings, `TaskCompletionSource` barriers for ordering, and the existing node DOM harness. No temporary files.

| Defect | Seam | Existing pattern | Proposed new test file |
|---|---|---|---|
| 1 (row state) | `BreadcrumbRow` public API with `FolderBreadcrumbSegment` fixtures | `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowStateTests.cs` (379 lines) | `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowDescendantsIssue1002Tests.cs` |
| 1 (codec) | `BreadcrumbMessageCodec.DeserializeInbound` | `BreadcrumbMessageCodecTests.cs` (309 lines) | `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecIssue1002Tests.cs` |
| 1 (renderer, assets) | `BreadcrumbHtmlRenderer.RenderRowFragment`, `BreadcrumbDocumentAssets.BridgeJs` text | `BreadcrumbHtmlRendererTests.cs` (363 lines; Issue439/Issue995 contract style) | `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererIssue1002Tests.cs` |
| 1 (router) | Real `BreadcrumbBridgeRouter` over mocked provider/host, inbound JSON via `ProcessInboundAsync` | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue995Tests.cs` (197), `...Issue439Tests.Activation.cs` (253) | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1002Tests.cs` |
| 2 (router) | `BindRowsAsync` overload with preferred row; assert `SelectedFolderPath`, one changed event, and `rowwrap selected` in the navigated document | same router tests | in `BreadcrumbBridgeRouterIssue1002Tests.cs` (or a `.Trash.cs` sibling if the file grows) |
| 2 (controller) | `ActionDeleteAsync` on the private-constructor controller with `_formViewer` (uninitialized `EfcViewer` plus `SynchronizationContext`), `_router` (real router, strict mocks), `_globals` (mock `IOlObjects.ArchiveRootPath`) injected by reflection | `EfcFormControllerTests.Issue439BindBreadcrumbRowsAsync_SubmitsArchiveRootToRealRouter` and `ActionDeleteAsync_AwaitedTwice...` | new partial `QuickFiler.Test/Controllers/EfcFormControllerTests.Issue1002.cs` (`EfcFormControllerTests.cs` is 485 and `.Part2.cs` 490 lines) |
| 3 (JS behavior) | `createBrowser(source)` DOM harness running the real `FolderBreadcrumb.js` | `tests/quickfiler-breadcrumb/issue-997-recursive.test.mjs` | `tests/quickfiler-breadcrumb/issue-1002-nesting.test.mjs` |
| 3 (CI-visible contract) | `FolderBreadcrumbDocument.Html` text | `FolderBreadcrumbRecursiveAssetIssue997Tests.cs` | `QuickFiler.Test/Viewers/FolderBreadcrumbNestingIssue1002Tests.cs` |

Fail-before candidates (each fails on the current tree):

- Router: `descendantToggle` on a second-level node calls `GetImmediateSubfoldersAsync` with that node's key and the next row render contains its children (today the codec throws `Unknown inbound breadcrumb message type`).
- Router: `descendantActivate` on a grandchild sets `SelectedFolderPath` to the archive-relative stem (for example `Clients\North\Q3\Invoices`) and `rowConfirmed` then raises `ConfirmationRequested` once.
- Router: a toggle whose provider task is completed after the node was collapsed (or after a rebind) attaches nothing and posts no render for the replaced row.
- Renderer: a keyed child with `HasChildren` renders `data-role="node"` after its name; a leaf child renders none; nested children sit in a nested `.children` container.
- Trash: after `await ActionDeleteAsync()`, `SelectedFolder` equals `"Trash to Delete"` and the last navigated document marks the trash row `rowwrap selected`.
- JS: for every `.sub` with a toggle, the toggle's child index is greater than the name's; `.sub` elements at levels 0/1/2 carry `data-depth` `0`/`1`/`2` and increasing `padding-left`.
- C# contract: inside the `makeSubfolders` body, `appendChild(name)` precedes `appendChild(toggle)`, and the recursive call passes `depth + 1`.

Existing tests to keep green (no expected change): `BreadcrumbHtmlRendererTests` (pins `class="child" data-child-index="0"`, `type: 'renderedChildActivate'`, `e.stopPropagation();`), `BreadcrumbBridgeRouterTests` and `...QueueTests` (pin `subfolderResult` on first-level expansion), `FolderBreadcrumbAssetContractTests` (pins `makeSubfolders(row, child.children`, `subfolderElement.*` attribute lines), `issue-997-recursive.test.mjs` (order-independent `.aff`/`.seg` queries), `EfcFormControllerTests.ActionDeleteAsync_AwaitedTwice...` (requires the null-router guard to remain).

## Question 5: Proposed file inventory

Physical line counts are from reading or a line-count search on the session checkout.

Production:

| File | Lines now | Change | Size note |
|---|---|---|---|
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs` | 361 | add `partial`; clear descendant state in the three existing reset points | ok |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.Descendants.cs` | new | keyed tree members (reuses `BreadcrumbDescendantNode`) | new |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs` | 234 | recursive children, node affordance after name, selected marker | ok |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs` | 149 | `.child` flex CSS; node-affordance and descendant-activate branches ordered before `.child` | ok |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs` | 173 | two type constants; optional `NodeId` | ok |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs` | 203 | register types; require `nodeId` | ok |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 459 | two delegating `case` arms; optional preferred-row parameter threaded into the existing bind | FLAG: near 500; add no logic here |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Descendants.cs` | new | toggle/activate handlers with stale guard | new |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs` | 211 | `ExpandLeafAsync` keeps keyed children | ok |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` | 236 | preferred-row selection helper | ok |
| `QuickFiler/Controllers/EfcFormController.Actions.cs` | 184 | awaited delete gesture with trash preselection | ok |
| `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs` | 208 | bind entry point accepting the preferred row | ok |
| `QuickFiler/Resources/FolderBreadcrumb.js` | 347 | name-then-toggle order; depth parameter and indent | ok |
| `UtilitiesCS/UtilitiesCS.csproj`, `QuickFiler/QuickFiler.csproj` | n/a | explicit `<Compile Include>` for the two new partial files (legacy non-SDK projects) | ORCHESTRATOR TO CONFIRM item placement |

No change: `FolderBreadcrumb.html` (177), `BreadcrumbDescendantNode.cs` (72), all `FolderBreadcrumbBridgeRouter*`/`BreadcrumbSelectionSession*`/`BreadcrumbBridgeCoordinator*` files, `EfcDataModel.cs` (485, FLAG near limit, untouched).

Tests (new files; `QuickFiler.Test.csproj` and `UtilitiesCS.Test.csproj` need `<Compile Include>` entries if they are non-SDK, ORCHESTRATOR TO CONFIRM): the seven files in the Question 4 table. Near-limit test files that must not be extended: `EfcFormControllerTests.cs` (485), `EfcFormControllerTests.Part2.cs` (490), `BreadcrumbBridgeRouterTests.cs` (462), `BreadcrumbBridgeRouterQueueTests.cs` (462), `BreadcrumbBridgeRouterScoreJoinTests.cs` (463), `BreadcrumbBridgeRouterIssue439Tests.cs` (455), `FolderBreadcrumbAssetContractTests.cs` (415).

## Question 6: Risks

- Shared code with QFC: the EFC model types (`BreadcrumbRow`, `BreadcrumbHtmlRenderer`, `BreadcrumbDocumentAssets`, `BreadcrumbMessageCodec`, `BreadcrumbMessages`) are not used by the QFC path except for the `BannerPrefix` constant, so the EFC change cannot alter QFC behavior. The one shared type reused, `BreadcrumbDescendantNode`, must be consumed unchanged; any change to it reaches `FolderBreadcrumbBridgeRouter` and `BreadcrumbRenderProjection`.
- QFC JS change: `FolderBreadcrumb.js` is the single page for both the collapsed and popup QFC surfaces; the change must not touch `render`, `makeRow`, or the message listener. Existing pinned strings listed in Question 4 constrain the edit.
- Thread affinity (EFC): the EFC router has no lock and relies on running on the UI context: inbound messages arrive on the WebView2 event (UI thread), and `ExpandLeafAsync`/`FetchChainAsync` await without `ConfigureAwait(false)`, so continuations resume on the captured UI context. The new descendant handler must follow the same rule. Copying the QFC router's `ConfigureAwait(false)` plus `lock (_sync)` model would mutate EFC row state on a pool thread concurrently with UI handlers.
- Stale completions (EFC): row ids are positional (`row-<index>`) and reused across binds, so a descendant fetch that completes after a rebind (for example a trash gesture during a pending expand) could post a fragment that replaces the new row with the same id. The new handler must check `ReferenceEquals(FindRow(row.RowId), row)` and the node generation before attaching or rendering. The existing first-level `ExpandLeafAsync` has the same latent exposure; it is out of the stated defect scope and is recorded here so the planner can decide whether to remediate it in place.
- Trash ordering: awaiting the bind makes the delete gesture wait for provider chain lookups of every suggestion row. The current gesture already triggers those lookups (fire-and-forget); awaiting adds no work, but an exception path is now observed by `ButtonDeleteClickAsync`'s boundary instead of being reported by `BindBreadcrumbRowsAsync`'s own boundary. Keeping the bind call inside `BindBreadcrumbRowsAsync` (which catches and reports) preserves the current reporting.
- `subfolderResult` JS: `BridgeJs` renders `subfolderResult` children without `data-child-index` (`BreadcrumbDocumentAssets.cs:131-142`); the subsequent row render replaces them. Descendant expansion must post only the row render so key-less transient children are never shown at deeper levels.

## Automation Feasibility

- Manual Outlook verification (open EFC with a three-level folder tree; expand and select a grandchild; press OK and confirm the filing destination; press the trash button and observe the highlight; open QFC and inspect the affordance side and per-level indent) is a human-interaction step. It cannot be automated in this repository: it requires a live Outlook process, a mailbox, and the WebView2 runtime.
- Verifiable by unit tests instead:
  - Defect 1: codec acceptance and rejection of the new types; row-state transitions (expand, collapse, attach with matching and stale generations, unknown identity); router end to end over mocked provider and host (provider called with the descendant key, render content, archive-relative `SelectedFolderPath`, one change event, Enter confirmation, stale-completion rejection); rendered markup (affordance after name, nesting, encoding); `BridgeJs` text contract (branch order, message shapes).
  - Defect 2: router preselection (state, single event, `rowwrap selected` in the navigated document) and controller `ActionDeleteAsync` end to end with a real router.
  - Defect 3: node harness assertions on DOM order and depth attributes against the real `FolderBreadcrumb.js`, plus a CI-visible C# text contract.
- Not verifiable by unit tests: actual pixel layout, WebView2 message delivery timing across `NavigateToString`, and the EFC `BridgeJs` click behavior in a browser engine (it is a C# string constant with no node harness).

## Pre-Mortem

- Review checklists: a reviewer can flag the 500-line limit on `BreadcrumbBridgeRouter.cs` (459 lines) and the near-limit test files, and a missing fail-before artifact per defect. Mitigation: plan only delegating arms in the main router file, put all new logic in new partial files, add tests only in new files, and record fail-before/pass-after runs under `evidence/regression-testing/` for each defect.
- Coverage: new members must reach at least 90% and changed lines must not lose coverage (CLAUDE.md floors 80% line / 75% branch repository-wide). The EFC router, row, renderer, and codec are measured (not coverage-exempt); the JS file is not measured by the C# coverage tool. Mitigation: router and row tests cover every rejection branch (unknown row, unknown node, leaf node, stale generation, replaced row, provider fault, cancellation); record the node `--experimental-test-coverage` figure for `FolderBreadcrumb.js` as #997 did.
- Mirror parity: not applicable; no `.claude`, `.agents`, `.codex`, or bundled resource file is changed, and the execution-environment brief's facts are unchanged.
- File size: `BreadcrumbBridgeRouter.cs` 459, `EfcDataModel.cs` 485, `EfcFormControllerTests.cs` 485, `EfcFormControllerTests.Part2.cs` 490, `BreadcrumbBridgeRouterTests.cs` 462, `...QueueTests.cs` 462, `...ScoreJoinTests.cs` 463, `...Issue439Tests.cs` 455. Mitigation: none of these receives more than a few delegating lines; new partial files carry the logic; verify physical counts with a whole-file line count after edits.
- Test isolation: tests must create no files and share no static state; the router and controller tests construct fresh instances per test. MSTest runs in parallel at class level (`Workers=0`), so no test may rely on static state in `WebView2BreadcrumbHost._owners` (avoid constructing real hosts). Mitigation: strict Moq host mocks only; for JS, run the new `.mjs` file with the #997 suites in normal, reversed, and shuffled order as #997 recorded.
- CI-only behavior: TaskMaster CI (actionlint, build-analyzers, build-nullable, format-check, hygiene, mstest-coverage, pester) does not run the node `.mjs` suites, so the JS behavioral tests give no CI signal. Mitigation: pair each JS behavior with a C# text-contract test in `QuickFiler.Test` that runs under `mstest-coverage`; keep new C# files under `#nullable enable` consistent with neighbors so `build-nullable` checks them; run CSharpier on the new files (format-check).
- Evidence: baseline, fail-before, pass-after, final QA, and coverage-comparison artifacts are required under `<FEATURE>/evidence/<kind>/` with `Timestamp`, `Command`, `EXIT_CODE`, `Output Summary`, no host paths, and `SCRATCH` for scratch locations. The manual Outlook step must be recorded as a human-interaction step, not claimed as executed. Mitigation: the plan names each artifact path per task.

## Open Questions

1. Trash: the code clears the prior selection on rebind; the maintainer reports it "stays where it was". Confirm whether the acceptance criterion should state only "the trash entry is selected and `SelectedFolder` is `Trash to Delete`".
2. QFC affordance placement: immediately after the name (recommended, matches top-level rows) or flush right.
3. EFC keyboard navigation into deeper descendants (Right/Left/Up/Down across nested children) is not requested and is not in the recommendation. Confirm it is out of scope.
4. Whether the latent stale-render exposure in the existing first-level `ExpandLeafAsync` should be remediated in this item (related root cause) or left as is.
5. Orchestrator to confirm: the exact research filename stamp, and `<Compile Include>` placement for new files in the legacy project files.

No numeric acceptance-criterion count is proposed by this research.
