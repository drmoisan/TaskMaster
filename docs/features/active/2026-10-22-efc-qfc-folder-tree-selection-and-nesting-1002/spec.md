# 2026-10-22-efc-qfc-folder-tree-selection-and-nesting (Spec)

- **Issue:** #1002
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-10-22
- **Status:** Ready for planning
- **Version:** 0.2
- **Work Mode:** full-bug. This file is the sole authoritative acceptance-criteria source (`acceptance-criteria-tracking`: full-bug resolves to spec.md only). No user-story.md is produced for this item.

> Authoring conventions (do not "fix" these):
> - Every file this fix creates or modifies is listed in `## Write Set` as a backticked repository-relative path and may be backticked elsewhere. Files that are cited for comparison but must NOT change are written as plain prose without backticks, so the footprint harvester does not read them as write targets.
> - Acceptance-criterion lines contain no digits, no percent signs and no angle brackets. Counts and figures are written as words; evidence artifacts are named on AC lines by evidence kind and file stem only, with the full feature-relative path given in `## Test Strategy`. The research record supplies no `## Numeric Derivation Evidence`, so no AC asserts a derived count.
> - Line numbers cited below describe the checkout at research time and are evidence only; the plan must cite symbols, not lines.

## Context

Three defects in the QuickFiler folder-selection surfaces:

1. **EFC has no recursive subfolder selection.** The EFC viewer cannot expand and select a subfolder below the first revealed level, which the QFC viewer gained in issue #997 (PR #998).
2. **EFC trash button does not select the trash entry.** Pressing the trash button prepends the "Trash to Delete" row, but no row is selected afterwards.
3. **QFC nested rows are laid out wrongly.** In the QFC selector, a descendant row that has its own subfolders shows its +/− before the folder name instead of after it, and every nested level shares the same indent.

Environment: Windows 11, Outlook desktop (VSTO add-in), WebView2 breadcrumb surfaces; C# on .NET Framework 4.8 plus the QFC JavaScript asset. Reproduction requires a mailbox whose folder tree has at least three nested levels.

Impact / Severity: Medium. Logs / Screenshots: none attached; reported by the maintainer on 2026-10-22 with no log excerpt.

Research: research/2026-10-22T09-17-efc-qfc-folder-tree-selection-and-nesting-research.md (this folder). Its "Orchestrator Decisions on Open Questions" section is binding and is applied throughout this spec:

- D1 (trash wording): after the trash button is pressed, the trash entry is the selected row and `SelectedFolder` resolves to it. The prior selection is not asserted beyond that.
- D2 (QFC affordance placement): the +/− immediately follows the folder name, as on top-level rows. It is not flush right.
- D3 (EFC keyboard scope): parity with #997 only. Descendant toggles are focusable and respond to Enter and Space. No new arrow-key navigation into deeper levels.
- D4 (first-level stale render): in scope. The same stale-completion guard is applied to the existing first-level `ExpandLeafAsync`.

Design precedent: docs/features/archive/2026-10-17-quickfiler-recursive-folder-selection-997/spec.md.

## Repro & Evidence

Steps to Reproduce:
1. Open the EFC viewer, open the folder suggestions, and try to drill into and select a subfolder of a suggested folder.
2. In the EFC viewer, press the trash button.
3. Open the QFC viewer, expand a suggested folder, then look at a child subfolder that itself has subfolders and expand it.

Expected:
1. EFC offers the same recursive subfolder expansion and selection that QFC gained in #997, and the selected subfolder becomes the filing destination.
2. After the trash button adds the trash entry, that entry is the selected entry.
3. In QFC, the expand affordance of every expandable subfolder row follows the folder name, matching the top-level rows, and each nested level is indented one step further than its visible parent.

Actual:
1. EFC does not offer recursive subfolder selection.
2. The trash entry is created but is not selected.
3. The expand affordance of a nested subfolder renders before the name, and nested levels share one indent.

Note on defect 2: the maintainer reported that "the selection stays where it was". Reading the code shows the router clears the prior selection on rebind (#499), so after the gesture `SelectedFolder` is null and OK reports "Please select a valid folder." Per D1 the required outcome is identical either way, and no criterion asserts the prior selection.

Fail-before candidates (each fails on the current tree; see Test Strategy for test names):
- EFC: a `descendantToggle` inbound message is rejected by the codec as an unknown type.
- EFC: after `await ActionDeleteAsync()`, `SelectedFolder` is null and the navigated document contains no `rowwrap selected` row.
- QFC: inside `makeSubfolders`, `subfolderElement.appendChild(toggle)` precedes `subfolderElement.appendChild(name)`, and the recursive call carries no depth.

## Scope & Non-Goals

- In scope:
  - Defect 1: EFC keyed descendant tree on the EFC row model, identity-addressed `descendantToggle` / `descendantActivate` inbound messages, recursive EFC rendering with the affordance after the name, stale-completion guards on the new descendant fetch AND on the existing first-level `ExpandLeafAsync` (D4), and filing of the selected descendant through the existing `SelectHierarchyPath` archive-relative contract.
  - Defect 2: EFC trash preselection delivered inside the same bound document through a preferred-row parameter on the router bind, with the delete gesture awaiting that bind.
  - Defect 3: QFC `makeSubfolders` DOM order (name, then toggle) and a per-depth indent of one step of 16px per level, in the JavaScript asset only.
- Out of scope / non-goals:
  - No new arrow-key navigation (Left/Right/Up/Down) into or across EFC descendants (D3; #997 likewise excluded new keyboard features). Existing EFC arrow transitions (#440) are unchanged.
  - No keyboard activation of EFC descendant rows beyond the toggle parity in D3; Enter outside a focused descendant toggle keeps the existing row-confirmation behavior (#995).
  - No changes to the QFC router, selection session, coordinator, render projection, or QFC wiring. The QFC fix is JavaScript-only.
  - No move of the EFC folder area onto the QFC selector document or router (research approach B, rejected).
  - No index-path addressing of descendants (research approach C, rejected).
  - No flush-right placement of the QFC affordance (D2).
  - No change to how the trash row is filed, moved, or deleted; only its selection after the gesture.
- Explicitly excluded files (deliberately unbackticked; they must not appear in the diff): UtilitiesCS/OutlookObjects/Folder/BreadcrumbDescendantNode.cs (reused unchanged), UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs and every FolderBreadcrumbBridgeRouter partial, every BreadcrumbSelectionSession file, UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs, every QuickFiler/Viewers/BreadcrumbBridgeCoordinator file, QuickFiler/Viewers/ItemViewer.Breadcrumb.cs, QuickFiler/Controllers/QfcItemController.BreadcrumbWiring.cs, QuickFiler/Resources/FolderBreadcrumb.html, QuickFiler/Properties/Resources.resx, QuickFiler/Controllers/EfcDataModel.cs.

## Root Cause Analysis

| # | Defect | Verified root cause |
|---|---|---|
| 1 | EFC has no recursive subfolder selection | EFC does not use the #997 surface. It uses its own generated-document surface: `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` with `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs` and the `BridgeJs` constant in `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs`. `BreadcrumbRow` stores first-level children as a flat key-less `List<BreadcrumbSegment>`; `ExpandLeafAsync` maps the provider's keyed children through `BreadcrumbRowBuilder.MapSegments`, which discards each `FolderTreeNodeKey`. `AppendChildren` renders a flat `div.child` per first-level child with no affordance and no identity. The codec accepts only integer-indexed inbound types, so no message can address a node below the first level. |
| 2 | EFC trash entry created but not selected | `ActionDeleteAsync` calls synchronous `ApplyDeleteGesture`, which sets `_folderRows = WithTrashRow(_folderRows)` and calls `BindFolderRows`, which discards the task of `BindBreadcrumbRowsAsync`. `BreadcrumbBridgeRouter.BindRowsAsync` rebuilds rows, sets `_selectedRowId = null`, clears `SelectedFolderPath` (#499), and `DeliverDocument` navigates a document with no selected row. No code path selects the trash row. The EFC data model is not involved. |
| 3 | QFC nested affordance before the name; flat indent | In `QuickFiler/Resources/FolderBreadcrumb.js` `makeSubfolders`, `subfolderElement.appendChild(toggle)` runs before `subfolderElement.appendChild(name)`, so the toggle is the first flex item. Indent comes only from the fixed `.sub` rule `padding: 1px 4px 1px 24px` in FolderBreadcrumb.html; nested `.subs` containers carry no padding, so every level renders at the same indent. Depth is implicit in the recursive call and need not be added to any C# DTO. |

Why "select after bind" is rejected for defect 2: calling the existing `SelectRow` after the bind commits state correctly but delivers the highlight as a posted `render` message immediately after `NavigateToString` replaced the document. The WebView2 host posts to whatever document is current and nothing waits for navigation completion, so the highlight can reach the outgoing document or arrive before the new listener is registered. This is an inference from the WebView2 delivery model and is not reproducible in a unit test; the fix therefore puts the selection inside the bound document.

## Proposed Fix

### Design summary (what changes where):

- **Defect 1 (EFC recursive selection, research approach A).** Keep the EFC router, renderer, codec, and `BridgeJs`. Add a keyed descendant tree to `BreadcrumbRow` in a new partial file, reusing the existing internal `BreadcrumbDescendantNode` unchanged (same assembly, so no visibility change). Add two inbound types addressed by `rowId` plus a string `nodeId`. Render children recursively with the affordance after the name. Route both new messages through a new router partial with stale-completion guards, and file the activated descendant through the existing `SelectHierarchyPath`.
- **Defect 2 (EFC trash).** Add a preferred-row parameter to the router bind so the trash row is selected before `DeliverDocument`, and make the delete gesture await that bind.
- **Defect 3 (QFC nesting).** In `makeSubfolders`, append the name before the toggle and add a depth parameter that sets `data-depth` and an inline left padding per level.

### Boundaries and invariants to preserve:

Invariants this fix establishes, one sentence each:

- **I1 (EFC descendant identity):** a descendant can be toggled or activated only by a `nodeId` that the router itself issued for a node currently present in the addressed row's tree; any other identity is logged and changes no state, raises no event, and calls no provider.
- **I2 (EFC stale completion):** a provider result is applied to a row only when, after the await, `FindRow(row.RowId)` returns the same row instance and (for descendants) the node is still in the tree, still expanded, and its `RequestGeneration` equals the generation captured at request time; otherwise nothing is attached and nothing is posted for that row id. This applies to the new descendant fetch and to the existing first-level `ExpandLeafAsync`.
- **I3 (EFC toggle versus selection):** a `descendantToggle` changes expansion only; it never changes `SelectedFolderPath` and never raises `SelectedFolderPathChanged`.
- **I4 (EFC filing value):** a `descendantActivate` commits only through `SelectHierarchyPath`, so the committed `SelectedFolderPath` is the archive-relative stem when an archive root is bound, and an out-of-root or archive-root target is a non-selection that leaves the prior selection and the rendered highlight unchanged (#614, #499).
- **I5 (EFC trash):** when the delete gesture completes, `SelectedFolderPath` equals "Trash to Delete" and the document delivered by that bind already marks the trash row `rowwrap selected`; no follow-up render message is used to establish the selection.
- **I6 (EFC thread affinity):** all EFC router state is mutated on the UI context. New awaits in router files do not use `ConfigureAwait(false)`, and no lock is introduced.
- **I7 (QFC layout):** in every `.sub` row rendered by `makeSubfolders`, the name element precedes the toggle element, and a row at depth d carries `data-depth` equal to d and a left padding of 24 + 16 × d pixels, so each nested level is exactly one 16px step further in than its visible parent.

Trace for the EFC descendant path (defect 1), one accepted value from accept to commit:

1. Accept point: `BridgeJs` posts `{ type: 'descendantActivate', rowId, nodeId }`. `BreadcrumbMessageCodec.DeserializeInbound` validates only shape (known type, string `rowId`, non-empty string `nodeId`); it does not validate that the node exists.
2. Row resolution: `ProcessInboundAsync` resolves the row with `FindRow`; an unknown row is logged and returns, as today.
3. Identity validation: the new router partial resolves the path with the row's `GetDescendantPath(nodeId)`; an unknown identity is logged and returns (I1). This is the only check that the identity is host-issued.
4. Commit point: `SelectHierarchyPath(row, path)` applies the archive-root contract and calls `CommitSelection`, which sets `_selectedRowId` to the containing row, sets `SelectedFolderPath`, posts the row render, and raises `SelectedFolderPathChanged` once. `ConfirmRow` then accepts `rowConfirmed` for that row because `_selectedRowId` matches, and `ActionOkAsync` reads the same value through `SelectedFolder` and validates it with `EfcSelectionGuard.IsValidFilingSelection`.

Trace for the toggle path (stale completion): accept (codec shape check) -> identity validation (`TryBeginDescendantExpand` returns the node key and captured generation, or false) -> provider await (`GetImmediateSubfoldersAsync(key, CancellationToken.None)`, no `ConfigureAwait(false)`) -> post-await validation (I2: same row instance and `TryAttachDescendantChildren` accepts the generation) -> one row render. Neither the codec check nor the pre-await identity check suffices alone: the first cannot know the tree, and the second is invalidated by any collapse, rebind, or later toggle that happens during the await.

Trace for the trash path (defect 2): `ActionDeleteAsync` -> delete gesture sets `_folderRows = WithTrashRow(_folderRows)` -> when `_formViewer` or `_router` is null, returns (guard preserved) -> otherwise awaits `BindBreadcrumbRowsAsync` with the preferred row "Trash to Delete" -> `BindRowsAsync` rebuilds rows and runs the #499 clear -> preferred-row selection sets `_selectedRowId` and `SelectedFolderPath` through the same normalization as `SelectRow` and raises `SelectedFolderPathChanged` -> `DeliverDocument` renders with `_selectedRowId` set (I5).

Inverse constraints (must not be widened or weakened):

- `OnHostMessageReceived` keeps catching only `BreadcrumbMessageException`. New handlers validate and log rather than throw, so no new exception type is relied on at the async-void boundary.
- `BindBreadcrumbRowsAsync` keeps its own catch-and-report boundary; the delete gesture awaits it rather than calling the router directly, so bind faults are still reported there instead of surfacing through `ButtonDeleteClickAsync`.
- The existing first-level `renderedChildActivate` path and its integer `childIndex` contract remain for key-less children.

### Dependencies or blocked work:

None. All seams exist: `IFolderHierarchyProvider.GetImmediateSubfoldersAsync`, `IBreadcrumbWebHost`, the in-memory router, and the node DOM harness under tests/quickfiler-breadcrumb used by #997.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

UtilitiesCS (EFC model):
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs`: add `partial`; clear descendant state and `SelectedDescendantId` exactly where `_leafChildren` is already cleared (segment activation, `CollapseAfter`, left-arrow collapse, leaf collapse).
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.Descendants.cs` (new): keyed tree members. Proposed names: `SetLeafDescendants(IReadOnlyList<FolderBreadcrumbSegment>)`, `TryBeginDescendantExpand(string nodeId, out FolderTreeNodeKey key, out int generation)`, `TryCollapseDescendant(string nodeId)`, `TryAttachDescendantChildren(string nodeId, int generation, IReadOnlyList<FolderBreadcrumbSegment>)`, `GetDescendantPath(string nodeId)`, `SelectedDescendantId` with a validating setter method, and a read-only view of the root's children for the renderer.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs`: `DescendantToggle = "descendantToggle"`, `DescendantActivate = "descendantActivate"` on `BreadcrumbMessageTypes`; optional `NodeId` on `BreadcrumbInboundMessage` (optional constructor parameter, source-compatible).
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs`: register both types in `IsKnownInboundType`; require a non-empty string `nodeId` for both, failing fast like the existing `childIndex` check.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs`: `AppendChildren` renders recursively from the descendant tree when one exists, otherwise exactly as today.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs`: `.child { display: flex; align-items: center; }` (merged into the existing `.child` rule); node-affordance click branch, descendant-activate click branch, and a node-affordance Enter/Space keydown branch.
- `UtilitiesCS/UtilitiesCS.csproj`: explicit `Compile Include` for the new partial.

QuickFiler (EFC router and controller):
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` (459 lines, near the limit): two one-line delegating `case` arms in `ProcessInboundAsync`; the preferred-row parameter threaded into the existing internal bind; no other logic.
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.Descendants.cs` (new): toggle and activate handlers with the I1/I2/I3 checks.
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs`: `ExpandLeafAsync` passes the provider's keyed children to `SetLeafDescendants` (still mapping them for the unchanged `subfolderResult` message) and applies the I2 same-instance guard after its await.
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs`: preferred-row selection helper sharing `SelectRow`'s normalization (extract the selection-value computation so it is not duplicated).
- `QuickFiler/Controllers/EfcFormController.Actions.cs`: replace synchronous `ApplyDeleteGesture` with an awaited gesture; `ActionDeleteAsync` awaits it.
- `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs`: `BindBreadcrumbRowsAsync` accepts an optional preferred row and passes it to the router.
- `QuickFiler/QuickFiler.csproj`: explicit `Compile Include` for the new router partial.

QFC page:
- `QuickFiler/Resources/FolderBreadcrumb.js`: in `makeSubfolders`, append `name` then the toggle; add a `depth` parameter (an absent value is treated as zero, so the top-level call in `render` needs no edit); set `data-depth` and `setAttribute("style", "padding-left: " + (24 + 16 * depth) + "px")` on each `.sub`; the recursive call becomes `makeSubfolders(row, child.children, selectedChildOwnsActive, depth + 1)`. The resource is a ResXFileRef to this file, so no resx change is needed.

#### Functions/classes/CLI commands impacted:

`BreadcrumbRow` (new partial members; reset points), `BreadcrumbMessageTypes`, `BreadcrumbInboundMessage`, `BreadcrumbMessageCodec.DeserializeInbound` / `IsKnownInboundType`, `BreadcrumbHtmlRenderer.AppendChildren`, `BreadcrumbDocumentAssets.BaseCss` / `BridgeJs`, `BreadcrumbBridgeRouter.ProcessInboundAsync` / `BindRowsAsync` / `ExpandLeafAsync` / `SelectRow` (refactor of the value computation only), `EfcFormController.ActionDeleteAsync` / `ApplyDeleteGesture` / `BindBreadcrumbRowsAsync`, and `makeSubfolders` in the QFC page. No CLI surface.

#### Data flow and validation changes:

- EFC expand: provider keyed children -> `SetLeafDescendants` (tree plus mapped `_leafChildren`) -> unchanged `subfolderResult` -> row render (now carrying node identities and affordances).
- EFC descendant toggle: `descendantToggle {rowId, nodeId}` -> codec shape check -> `FindRow` -> collapse (row render, no provider call) or begin expand -> provider await -> I2 check -> attach -> one row render. No `subfolderResult` is posted for descendant expansion, mirroring #997, so key-less transient children are never shown below the first level.
- EFC descendant activate: `descendantActivate {rowId, nodeId}` -> codec -> `FindRow` -> `GetDescendantPath` -> set `SelectedDescendantId` -> `SelectHierarchyPath` -> `CommitSelection`. On a rejected path (I4) the previous `SelectedDescendantId` is restored, so the highlight is unchanged.
- EFC trash: described in the trash trace above.
- Rendering of a keyed child: `<div class="child" data-child-index="i" data-node-id="..." data-depth="d" title="...">` (the first-level prefix `class="child" data-child-index=` is preserved), then a name span, then, only when the node has children, `<span class="affordance node" data-role="node" role="button" tabindex="0" aria-expanded="...">+</span>` or `&#8722;`. An expanded node is followed by a nested `<div class="children">` placed as a sibling after the node's `.child` element (not inside it), so `closest('.child')` resolves to the clicked node and the existing `.children { margin-left: 18px; }` rule compounds per level. The selected descendant carries `aria-selected="true"` (not a second class) and only on the row that is the selected row.
- `BridgeJs` ordering: in the click listener, the node-affordance branch (posts `descendantToggle`, stops propagation, returns) precedes the `.child` branches and the leaf `.affordance` branch; the `.child[data-node-id]` branch (posts `descendantActivate`) precedes the existing key-less `.child[data-child-index]` branch, which is kept. In the keydown listener, an Enter or Space on a focused node affordance posts `descendantToggle` and returns before the existing Enter-confirms-row branch, so toggling a descendant by keyboard never confirms the row.

#### Error handling and logging updates:

Use the router's existing `log.Error` pattern for: unknown node identity, toggle of a leaf node, attach rejected as stale, provider fault, and cancellation during a descendant fetch. Faults and cancellation leave state unchanged and permit a later retry. No new telemetry.

#### Rollback/feature-flag considerations (if applicable):

No feature flag, persisted data, or migration. Reverting the diff restores prior behavior.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

- New inbound JSON: `{"type":"descendantToggle","rowId":"row-0","nodeId":"<32-hex>"}` and `{"type":"descendantActivate","rowId":"row-0","nodeId":"<32-hex>"}`. Node identities are `BreadcrumbDescendantNode.Identity` values (`Guid.NewGuid().ToString("N")`), issued only by the host.
- Example filing value: with archive root `\\mailbox\Archive` and presented row `Clients\North`, activating the grandchild `Invoices` under `Q3` yields `SelectedFolderPath` = `Clients\North\Q3\Invoices`.
- Router bind: an overload or optional parameter `string? preferredRowText` on the internal `BindRowsAsync`; the public three-argument overload and the existing four-argument internal call shape remain valid for every current caller.

#### Required configuration keys and defaults:

None.

#### Backward-compatibility expectations:

- `SetLeafChildren(IEnumerable<BreadcrumbSegment>)` is unchanged; rows populated through it have no expandable descendants and render exactly as today.
- The existing seven inbound types, the `subfolderResult` message on first-level expansion, `renderedChildActivate`, and the #440 / #499 / #614 / #799 / #995 behaviors are unchanged.
- Pinned test strings preserved: `class="child" data-child-index="0"`, `type: 'renderedChildActivate'`, `e.stopPropagation();` (BreadcrumbHtmlRendererTests); `makeSubfolders(row, child.children` and the `subfolderElement.*` attribute and listener lines (FolderBreadcrumbAssetContractTests, FolderBreadcrumbRecursiveAssetIssue997Tests); no function is inserted between `makeSubfolders` and `render` (FolderBreadcrumbAssetContractTests slices bodies by adjacent function names).

#### Performance constraints (latency/throughput/memory):

Descendant fetches are lazy and per node; no eager subtree enumeration. Awaiting the trash bind adds no provider work (the same chain lookups already ran fire-and-forget). No numeric latency target is asserted.

## Assumptions, Constraints, Dependencies

- Assumptions: inbound WebView2 messages arrive on the UI thread, and EFC router continuations resume on the captured UI context (current behavior of `ExpandLeafAsync` and `FetchChainAsync`). Provider keys identify nodes uniquely across duplicate names.
- Constraints:
  - Tests use MSTest, Moq, and FluentAssertions only; no temporary files; no network or external processes; strict Moq mocks of `IFolderHierarchyProvider` and `IBreadcrumbWebHost`; no real WebView2BreadcrumbHost (its static owner table is shared state under class-level parallel execution); `TaskCompletionSource` barriers for ordering, never sleeps or delays.
  - New C# tests go in the sibling `.Test` projects mirroring the production folders. CLAUDE.md's unit test policies impose no `tests/` tree for C#, CLAUDE.md outranks `.claude/rules`, and every existing C# test lives in the `.Test` projects. The node suite goes under tests/quickfiler-breadcrumb, beside the #997 suites.
  - All four affected projects are legacy non-SDK projects; each new `.cs` file needs an explicit `Compile Include`.
  - 500-physical-line limit for production, test, and script files. Near-limit files: `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` (459) receives only delegating arms and parameter threading; EfcDataModel.cs (485) is not touched. Near-limit test files that must not be extended: EfcFormControllerTests.cs (485), EfcFormControllerTests.Part2.cs (490), BreadcrumbBridgeRouterTests.cs (462), BreadcrumbBridgeRouterQueueTests.cs (462), BreadcrumbBridgeRouterScoreJoinTests.cs (463), BreadcrumbBridgeRouterIssue439Tests.cs (455), FolderBreadcrumbAssetContractTests.cs (415).
  - EFC router stays UI-thread-affine: no new locks and no `ConfigureAwait(false)` in router files (I6). Copying the QFC router's lock-plus-`ConfigureAwait(false)` model would mutate EFC row state on a pool thread concurrently with UI handlers.
  - New C# files carry `#nullable enable`, consistent with their neighbors.
  - `BreadcrumbDescendantNode` is consumed unchanged; any edit to it reaches the QFC router and render projection.
- External dependencies: none new. No package changes.
- CI visibility: TaskMaster CI (actionlint, build-analyzers, build-nullable, format-check, hygiene, mstest-coverage, pester) does not run the node `.mjs` suites. Every JavaScript behavior in this item (the QFC `makeSubfolders` order and depth, and the EFC `BridgeJs` branches, which are a C# string constant with no node harness) therefore has a C# text-contract test that runs under mstest-coverage. The node suite is supplementary local evidence.

## Data / API / Config Impact

- User-facing: EFC descendants become expandable and selectable as filing destinations; the trash row is highlighted and selected after the trash button; QFC nested rows show +/− after the name with a per-level indent.
- Internal API: two new inbound bridge types and an optional `NodeId`; new `BreadcrumbRow` members; an optional preferred-row bind parameter. All additive.
- Data/migration: none. Session state only.
- Logging: new `log.Error` lines for rejected descendant requests and fetch failures, following the existing router pattern.

## Test Strategy

Bugfix workflow: each defect group gets a failing regression test first, captured as a fail-before projection, then the minimal fix, then a pass-after projection.

New test files and the scenarios each must cover (names are digit-free and binding unless the plan records a rename):

- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecIssue1002Tests.cs`: `DeserializeInbound_DescendantToggle_ReturnsNodeId`, `DeserializeInbound_DescendantActivate_ReturnsNodeId`, `DeserializeInbound_DescendantMessageWithoutNodeId_Throws`, `DeserializeInbound_DescendantMessageWithEmptyNodeId_Throws`, `DeserializeInbound_DescendantMessageWithNonStringNodeId_Throws`.
- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowDescendantsIssue1002Tests.cs`: keyed tree construction; begin expand on known, unknown, and leaf nodes; collapse; attach with matching generation, superseded generation, collapsed node, and removed node; `GetDescendantPath` for known and unknown identities; `SelectedDescendantId` validation; descendant state cleared by segment activation, `CollapseAfter`, left-arrow collapse, and leaf collapse; key-less `SetLeafChildren` yields no descendants.
- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererIssue1002Tests.cs`: `RenderRowFragment_KeyedChildWithChildren_RendersNodeAffordanceAfterName`, `RenderRowFragment_KeyedLeafChild_RendersNoAffordance`, `RenderRowFragment_ExpandedDescendant_NestsChildrenContainerAfterNode`, `RenderRowFragment_SelectedDescendant_MarksAriaSelected`, `RenderRowFragment_DescendantNames_AreHtmlEncoded`, `RenderRowFragment_KeylessChildren_RenderUnchanged`, `BridgeJs_NodeAffordanceBranch_PrecedesChildAndLeafBranches`, `BridgeJs_DescendantActivateBranch_PrecedesKeylessChildBranch`, `BridgeJs_NodeAffordanceEnterOrSpace_PrecedesRowConfirmation`, `BaseCss_Child_IsFlexRow`.
- `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1002Tests.cs` (real router, strict mocks, inbound JSON through `ProcessInboundAsync`): `DescendantToggle_SecondLevelNode_FetchesWithNodeKeyAndRendersChildren` (fail-before: codec rejects the type), `DescendantToggle_ExpandedNode_CollapsesWithoutProviderCall`, `DescendantToggle_DoesNotChangeSelection`, `DescendantToggle_PostsNoSubfolderResult`, `DescendantActivate_Grandchild_CommitsArchiveRelativeStem`, `DescendantActivate_ThenRowConfirmed_RaisesConfirmationOnce`, `DescendantActivate_OutsideArchiveRoot_LeavesSelectionAndHighlightUnchanged`, `DescendantRequest_UnknownNode_IsRejectedWithoutStateChange`, `DescendantToggle_LeafNode_IsRejectedWithoutProviderCall`, `DescendantRequest_NodeFromAnotherRow_IsRejected`, `ExpandLeaf_KeyedChildren_RenderNodeAffordances`.
- `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterStaleCompletionIssue1002Tests.cs`: `DescendantFetch_CompletesAfterCollapse_AttachesNothing`, `DescendantFetch_CompletesAfterRebind_PostsNothingForReplacedRow`, `DescendantFetch_ReverseCompletion_OnlyLatestGenerationApplies`, `DescendantFetch_ProviderFault_LeavesStateUnchangedAndAllowsRetry`, `DescendantFetch_Canceled_LeavesStateUnchanged`, `ExpandLeaf_CompletesAfterRebind_PostsNothingForReplacedRow`, `ExpandLeaf_CurrentRow_StillPostsSubfolderResultThenRender`.
- `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTrashIssue1002Tests.cs`: `BindRows_PreferredTrashRow_SelectsTrashInDeliveredDocument`, `BindRows_PreferredTrashRow_LastSelectionNotificationCarriesTrash`, `BindRows_PreferredTrashRow_PostsNoFollowUpRender`, `BindRows_PreferredRowAbsent_SelectsNothing`, `BindRows_NoPreferredRow_KeepsExistingClearBehavior`, `BindRows_PreferredTrashRowBeforeCoreInit_PendingDocumentCarriesSelection`.
- `QuickFiler.Test/Controllers/EfcFormControllerTests.Issue1002.cs` (new partial of `EfcFormControllerTests`; the two existing files are near the limit): `ActionDeleteAsync_WithRouter_SelectsTrashEntry` (fail-before: `SelectedFolder` is null), `ActionDeleteAsync_WithRouter_DeliveredDocumentMarksTrashSelected`, `ActionDeleteAsync_AwaitedTwice_KeepsTrashSelectedAndSingleTrashRow`, `ActionDeleteAsync_BindFault_IsReportedAtBindBoundary`.
- `QuickFiler.Test/Viewers/FolderBreadcrumbNestingIssue1002Tests.cs` (CI-visible contract over `FolderBreadcrumbDocument.Html`): `MakeSubfolders_AppendsNameBeforeToggle` (fail-before), `MakeSubfolders_RecursiveCallPassesDepthPlusOne` (fail-before), `MakeSubfolders_SetsDataDepthAndPerDepthPadding`, `MakeSubfolders_PinnedRecursiveCallPrefixPreserved`.
- `tests/quickfiler-breadcrumb/issue-1002-nesting.test.mjs` (node DOM harness over the real FolderBreadcrumb.js; not run by CI): for every `.sub` with a toggle, the toggle's child index is greater than the name's; `.sub` elements at levels 0, 1, 2 carry `data-depth` 0, 1, 2 and padding-left 24px, 40px, 56px. Run together with the #997 suites in normal, reversed, and shuffled order, as #997 recorded.

Existing tests to keep green with no edits: BreadcrumbHtmlRendererTests, BreadcrumbMessageCodecTests, BreadcrumbRowStateTests, BreadcrumbBridgeRouterTests (and .Selection), BreadcrumbBridgeRouterQueueTests (and .Part2), BreadcrumbBridgeRouterScoreJoinTests, BreadcrumbBridgeRouterIssue439Tests (and .Activation), BreadcrumbBridgeRouterIssue614Tests, BreadcrumbBridgeRouterIssue637Tests, BreadcrumbBridgeRouterIssue792Tests, BreadcrumbBridgeRouterIssue995Tests, EfcFormControllerTests (including `ActionDeleteAsync_AwaitedTwice_LeavesExactlyOneTrashRowInFolderRows`, which relies on the null-router guard), EfcFormControllerIssue995Tests, EfcHomeControllerExecuteMovesTests, FolderBreadcrumbAssetContractTests, FolderBreadcrumbRecursiveAssetIssue997Tests, and the issue-997 node suites. Test disposition: no test references `ApplyDeleteGesture` except in a comment, and no test reaches router or row members by reflection name that this fix renames; the plan re-runs that grep (`GetField(` / `GetMethod(` / `GetProperty(` plus each renamed member) before marking any file unchanged.

Edge cases and negative scenarios: unknown row; unknown, leaf, and cross-row node identities; malformed `nodeId`; collapse during fetch; rebind during fetch; reversed completions; provider fault; cancellation; out-of-root and archive-root activation; preferred row absent; host core not yet initialized; delete gesture with a null router.

Coverage: new classes, partials, and methods target at least 90% line coverage; changed lines must not lose coverage. Repository floors are 80% line and 75% branch (CLAUDE.md § UT2, testable denominator). No baseline exists yet in this folder; the plan captures it, and if the raw repository figure is already below a floor the blocking condition is change-scoped and the repository figure is recorded and shown not lowered. The JS file is not measured by the C# coverage tool; the node `--experimental-test-coverage` figure for FolderBreadcrumb.js is recorded as #997 did. No coverage exclusions are added.

Toolchain (CLAUDE.md order, one clean pass, restart from step 1 on any failure or file change):
1. `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .`
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. The `test: MSTest with Coverage (Koverage)` VS Code task, or Invoke-MSTestWithCoverage.ps1 under scripts/vscode directly.

Evidence (Markdown projections only, each with Timestamp, Command, EXIT_CODE, and an output summary; no host paths; no raw TRX, Cobertura, or .coverage file is added to the repository):
- `docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/evidence/baseline/baseline-toolchain.md`
- `docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/evidence/baseline/baseline-coverage.md`
- `docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/evidence/regression-testing/efc-descendants-fail-before.md`
- `docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/evidence/regression-testing/efc-descendants-pass-after.md`
- `docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/evidence/regression-testing/efc-trash-fail-before.md`
- `docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/evidence/regression-testing/efc-trash-pass-after.md`
- `docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/evidence/regression-testing/qfc-nesting-fail-before.md`
- `docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/evidence/regression-testing/qfc-nesting-pass-after.md`
- `docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/evidence/regression-testing/node-suites-run.md`
- `docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/evidence/qa-gates/final-toolchain.md`
- `docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/evidence/qa-gates/coverage-comparison.md`
- `docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/evidence/other/manual-outlook-check.md`

Manual validation (non-blocking human check): open EFC on a mailbox with a three-level tree; expand a suggested folder, expand a child, select a grandchild, press OK, and confirm the filing destination; press the trash button and confirm the trash row is highlighted and OK targets it; open QFC, expand a suggested folder and a child with subfolders, and confirm the +/− follows the name and each level is indented one step further. This requires a live Outlook process, a mailbox, and the WebView2 runtime, so it cannot be automated here. It is recorded in manual-outlook-check.md as a pending human-interaction step with these instructions; it must not be claimed as executed by an agent, and it does not block completion.

## Acceptance Criteria

Criteria are labelled ACn and grouped by defect. Each is one checkbox.

### Group A: EFC recursive subfolder expansion and selection (defect one)

- [x] AC1 Codec contract: `BreadcrumbMessageCodec.DeserializeInbound` accepts `descendantToggle` and `descendantActivate` carrying a `rowId` and a non-empty string `nodeId`, exposing the identity as `BreadcrumbInboundMessage.NodeId`; a missing, empty, or non-string `nodeId` on either type throws `BreadcrumbMessageException`; every previously accepted inbound type deserializes exactly as before. Verified by `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecIssue1002Tests.cs` and the unchanged BreadcrumbMessageCodecTests.
- [x] AC2 Row state: `BreadcrumbRow` holds a keyed descendant tree built from keyed first-level children, reusing BreadcrumbDescendantNode with that file absent from the diff; expansion begins only for a known node that has children, attach is rejected when the node was removed, collapsed, or its request generation moved, unknown identities resolve to no path, and the tree and the selected descendant are cleared at every point where the first-level children are already cleared; rows populated through the key-less `SetLeafChildren` have no expandable descendants. Verified by `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowDescendantsIssue1002Tests.cs` and the unchanged BreadcrumbRowStateTests.
- [x] AC3 First-level expansion keeps provider keys: `ExpandLeafAsync` stores the provider's keyed children on the row, so a first-level child that has subfolders renders a node affordance, while the current-row sequence of a `subfolderResult` message followed by a row render is unchanged.
- [x] AC4 Descendant toggle: a `descendantToggle` for a second-level node calls `GetImmediateSubfoldersAsync` exactly once with that node's key and posts one row render whose markup nests the returned children below that node; a second toggle collapses the node with no provider call; no `subfolderResult` is posted for descendant expansion; and a toggle never changes `SelectedFolderPath` nor raises `SelectedFolderPathChanged`.
- [x] AC5 Descendant activation is the filing destination: with an archive root bound, a `descendantActivate` for a grandchild sets `SelectedFolderPath` to its archive-relative stem through `SelectHierarchyPath` (for example the stem Clients\North\Q3\Invoices), raises `SelectedFolderPathChanged` exactly once, makes the containing row the selected row with the descendant marked selected in the posted render, satisfies `EfcSelectionGuard.IsValidFilingSelection`, and a following `rowConfirmed` for that row raises `ConfirmationRequested` exactly once; an out-of-root or archive-root target leaves the prior selection, the event stream, and the rendered highlight unchanged.
- [x] AC6 Identity validation: a descendant message naming an unknown row, an unknown node, a node from another row, or (for a toggle) a leaf node is logged and rejected with no provider call, no state change, no posted message, and no selection event.
- [x] AC7 Descendant stale-completion guard: a descendant fetch whose provider task completes after the node was collapsed, after a rebind replaced the row, or after a later toggle superseded its generation attaches nothing and posts nothing for that row id; a provider fault or cancellation is logged, leaves state unchanged, and a later toggle of the same node succeeds. Verified with `TaskCompletionSource` barriers in `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterStaleCompletionIssue1002Tests.cs`.
- [x] AC8 First-level stale-completion guard: when the provider task of an existing first-level `ExpandLeafAsync` completes after a rebind replaced the row, the replaced row is not mutated and neither a `subfolderResult` nor a render is posted for that row id; a completion for the current row behaves exactly as before.
- [x] AC9 Recursive rendering: `BreadcrumbHtmlRenderer` renders each keyed child with its node identity and depth, a name element, and, only when the node has children, a focusable node affordance with button role and expanded state placed after the name; leaf nodes carry no affordance; an expanded node is followed by a nested children container that is a sibling of the node element rather than inside it; display names and paths are HTML-encoded; the selected descendant is marked with an aria-selected attribute rather than a new class; the first-level child markup prefix pinned by BreadcrumbHtmlRendererTests is unchanged; and key-less children render exactly as before. Verified by `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererIssue1002Tests.cs`.
- [x] AC10 EFC bridge script contract: in `BreadcrumbDocumentAssets.BridgeJs`, the node-affordance click branch posts `descendantToggle` with row and node identity, stops propagation, and precedes both the child branches and the leaf affordance branch; a child branch keyed by node identity posts `descendantActivate` and precedes the retained key-less `renderedChildActivate` branch; Enter or Space on a focused node affordance posts `descendantToggle` and returns before the existing Enter-confirms-row branch; and the child rule in `BreadcrumbDocumentAssets.BaseCss` is a flex row so the affordance follows the name. Because CI runs no browser engine for this C# string constant, these orderings are asserted by C# text-contract tests in the Group A renderer test file.

### Group B: EFC trash button selects the trash entry (defect two)

- [x] AC11 Router preselection in the same bind: `BreadcrumbBridgeRouter.BindRowsAsync` accepts an optional preferred row; when the preferred text is "Trash to Delete" and the trash row is bound, `SelectedFolderPath` equals "Trash to Delete" before the document is delivered, the single navigated document (or the pending document when the host core is not yet initialized) marks the trash row as the selected rowwrap, exactly one `SelectedFolderPathChanged` notification of the bind carries the trash text and it is the last notification of the bind, and no outbound render message is posted to establish that selection. The preferred-row value computation is shared with `SelectRow`, not duplicated. Verified by `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTrashIssue1002Tests.cs`.
- [x] AC12 Router compatibility: a bind without a preferred row, or with a preferred row that is not present, behaves exactly as before, including the existing clear of a stale selection, and every existing caller of the public and internal bind compiles and passes unchanged.
- [x] AC13 Controller end to end: after `await ActionDeleteAsync()` on a controller wired to a real router over strict mocks, `SelectedFolder` equals "Trash to Delete" and the last navigated document marks the trash row selected; a second awaited invocation keeps the trash row selected and `_folderRows` holds exactly one trash row; the bind is awaited inside `BindBreadcrumbRowsAsync` so a bind fault is still reported at that boundary; the null-router and null-viewer guard is preserved and `ActionDeleteAsync_AwaitedTwice_LeavesExactlyOneTrashRowInFolderRows` passes unchanged. Verified by `QuickFiler.Test/Controllers/EfcFormControllerTests.Issue1002.cs`.

### Group C: QFC nested row affordance and indentation (defect three)

- [x] AC14 Affordance after the name at every depth: in `makeSubfolders` in `QuickFiler/Resources/FolderBreadcrumb.js`, the name element is appended before the toggle element, so on every descendant row at every depth the +/− immediately follows the folder name as on top-level rows (not flush right); the toggle keeps its click and Enter/Space handlers and the descendant-toggle message unchanged. Asserted CI-visibly by `MakeSubfolders_AppendsNameBeforeToggle` in `QuickFiler.Test/Viewers/FolderBreadcrumbNestingIssue1002Tests.cs`.
- [x] AC15 One indent step per level: `makeSubfolders` takes a depth that is zero when absent, the recursive call passes depth plus one, and each descendant row carries a data-depth attribute equal to its depth and an inline left padding of twenty-four pixels plus sixteen pixels per depth level, so each nested level sits one sixteen-pixel step further in than its visible parent while the selected-row highlight stays full width. Asserted CI-visibly by the depth tests in the Group C nesting contract file; the `render` function, `makeRow`, the message listener, FolderBreadcrumb.html, and every pinned `makeSubfolders` and `subfolderElement` string in FolderBreadcrumbAssetContractTests and FolderBreadcrumbRecursiveAssetIssue997Tests are unchanged, and no function is inserted between `makeSubfolders` and `render`.
- [x] AC16 Node behavioral suite: the new node nesting suite named in the Write Set asserts, against the real page script, that the toggle follows the name on every descendant row and that rows at depths zero, one, and two carry increasing depth attributes and left padding; it passes together with the predecessor recursive-selection node suites in normal, reversed, and shuffled order, recorded in the regression-testing projection node-suites-run. Because CI does not run node suites, this criterion is satisfied by that local record and does not replace AC14 or AC15.

### Group D: Cross-cutting scope, quality, and evidence

- [x] AC17 Fail-before and pass-after evidence: for each defect group, the named fail-before tests fail on the unfixed tree and pass after the fix, recorded in the regression-testing projections efc-descendants-fail-before, efc-descendants-pass-after, efc-trash-fail-before, efc-trash-pass-after, qfc-nesting-fail-before, and qfc-nesting-pass-after (full paths in Test Strategy).
- [x] AC18 Scope fences: the diff modifies no file in the explicitly excluded list in Scope and Non-Goals (the QFC router, selection session, coordinator, render projection and wiring files, BreadcrumbDescendantNode, FolderBreadcrumb.html, the resource table, and the EFC data model); EFC does not reference the QFC selector document or the QFC router; and no new arrow-key handling is added to the EFC router or bridge script.
- [x] AC19 Thread affinity: no file matching `QuickFiler/Controllers/BreadcrumbBridgeRouter*.cs` contains `ConfigureAwait(false)` or a lock statement after the change, the new descendant handler awaits the provider without `ConfigureAwait`, and the delete gesture path in `QuickFiler/Controllers/EfcFormController.Actions.cs` adds no `ConfigureAwait(false)`.
- [x] AC20 File size: each new or modified C# file in the Write Set and `QuickFiler/Resources/FolderBreadcrumb.js` is at most five hundred physical lines after the change (project files and Markdown are exempt under the repository rule), `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` gains only delegating case arms and preferred-row parameter threading, and none of the near-limit test files listed in Constraints is extended.
- [x] AC21 Test policy and registration: every new C# test uses MSTest, Moq, and FluentAssertions, follows Arrange-Act-Assert, creates no temporary or other files, uses strict mocks and no real WebView2 host, and uses no sleep or delay; each new C# source and test file has an explicit compile entry in its legacy project file, and every new test name appears in the executed test list of the final MSTest run.
- [x] AC22 Existing tests: every existing test listed in Test Strategy as kept green passes with no edit to its assertions or pinned strings.
- [x] AC23 Coverage: each new class, partial, and method in the Write Set reaches at least ninety percent line coverage, changed lines lose no coverage, no coverage exclusion is added, and the repository line and branch figures are recorded against the eighty percent line and seventy-five percent branch floors and shown not lowered by the change, in the qa-gates projection coverage-comparison against the baseline projection baseline-coverage.
- [x] AC24 Toolchain: one complete pass of format, analyzers rebuild, warnings-as-errors rebuild, and MSTest with coverage, in the CLAUDE.md order and with the CLAUDE.md commands, succeeds with no failure or file change, recorded in the qa-gates projection final-toolchain.
- [x] AC25 Evidence form: all evidence is committed as Markdown projections under the feature folder's evidence directories, and the diff adds no raw test-result, Cobertura, or binary coverage file.
- [x] AC26 Manual Outlook check recorded as non-blocking: the other-evidence projection manual-outlook-check records the manual EFC and QFC verification steps from Test Strategy as a pending human-interaction step, not as executed by an agent; this criterion is satisfied by that record, and the human check itself does not block completion.

## Write Set

Production:
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs`
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.Descendants.cs` (new)
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs`
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs`
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs`
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs`
- `UtilitiesCS/UtilitiesCS.csproj`
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.Descendants.cs` (new)
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs`
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs`
- `QuickFiler/Controllers/EfcFormController.Actions.cs`
- `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs`
- `QuickFiler/Resources/FolderBreadcrumb.js`
- `QuickFiler/QuickFiler.csproj`

Tests:
- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecIssue1002Tests.cs` (new)
- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowDescendantsIssue1002Tests.cs` (new)
- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererIssue1002Tests.cs` (new)
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
- `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1002Tests.cs` (new)
- `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterStaleCompletionIssue1002Tests.cs` (new)
- `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTrashIssue1002Tests.cs` (new)
- `QuickFiler.Test/Controllers/EfcFormControllerTests.Issue1002.cs` (new)
- `QuickFiler.Test/Viewers/FolderBreadcrumbNestingIssue1002Tests.cs` (new)
- `QuickFiler.Test/QuickFiler.Test.csproj`
- `tests/quickfiler-breadcrumb/issue-1002-nesting.test.mjs` (new; local evidence only, not run by CI)

Feature documents and evidence:
- `docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/**`

## Risks & Mitigations

- Near-limit router file (459 lines): only delegating arms and parameter threading go in it; logic goes in the new partials. Verified by a whole-file line count after edits (AC20).
- Stale completions over positional row ids (`row-<index>` is reused across binds): the same-instance and generation checks of I2, applied to both fetch paths (AC7, AC8).
- Thread affinity: copying the QFC router's concurrency model would race UI handlers; I6 and AC19 forbid it.
- Click and keydown routing in `BridgeJs`: the node affordance sits inside the `.child` element and the document-level Enter handler confirms the row, so branch order is load-bearing; AC10 pins it by text contract because no browser engine runs in CI.
- Shared type reuse: `BreadcrumbDescendantNode` is used unchanged (AC2, AC18) because QFC depends on it.
- QFC page shared by the collapsed and popup surfaces: the edit is confined to `makeSubfolders`, and the pinned strings constrain it (AC15, AC22).
- JS behavior has no CI signal: paired C# text contracts (AC10, AC14, AC15) plus the local node suite (AC16).
- Delete-gesture exception path: awaiting the bind inside `BindBreadcrumbRowsAsync` keeps faults reported at the existing boundary (AC13).
- Rollback: revert the diff; no persisted data or configuration is affected.

## Rollout & Follow-up

- Release: regression-first implementation, ordered baseline and final QA, acceptance tracking, feature review, and final-head CI through the orchestrator; shipped with the normal TaskMaster release. No migration.
- Follow-up: the maintainer performs the manual Outlook check recorded in manual-outlook-check.md. No other follow-up is planned.
- Links: Issue https://github.com/drmoisan/TaskMaster/issues/1002; issue.md and the research record in this folder; design precedent docs/features/archive/2026-10-17-quickfiler-recursive-folder-selection-997/spec.md (Issue #997, PR #998).
