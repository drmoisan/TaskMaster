# breadcrumb-left-right-arrow-parent-child-navigation (Issue #440)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/breadcrumb-left-right-arrow-parent-child-navigation/ (Issue #440)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #440
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/440
- Last Updated: 2026-08-08
## Summary

In both the QuickFiler item folder selector (Qfc) and the EfcViewer folder list (Efc), the Left and Right arrow keys on a highlighted folder row do not perform parent/child tree navigation. Left should select the parent as a node; Right should expand that parent node into its children. Today Left only collapses the displayed breadcrumb text and Right only expands the leaf's children, so the selected node never moves up the tree.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8.1 WinForms VSTO add-in with Microsoft WebView2
- UI paths:
  - Qfc — `ItemViewer` breadcrumb folder selector via `BreadcrumbBridgeCoordinator` and `FolderBreadcrumbBridgeRouter` / `BreadcrumbStateModel`, hosted document `QuickFiler/Resources/FolderBreadcrumb.html`
  - Efc — `EfcViewer.FolderListBox` via `BreadcrumbBridgeRouter` and `BreadcrumbRow`, hosted document from `BreadcrumbDocumentAssets`
- Data source or fixture: any folder row whose ancestor chain resolves to more than one segment

## Steps to Reproduce

1. Open QuickFiler and give the folder selector keyboard focus so a folder row is highlighted.
2. Press Left. Observe that the selected node does not become the parent folder.
3. Press Right. Observe that the expansion applies to the leaf, not to the parent node selected by Left.
4. Repeat steps 1-3 in EfcViewer against a highlighted row in the folder list.

## Expected Behavior

- With a folder row highlighted, Left selects that row's parent as the current node. Repeated Left presses walk up the ancestor chain until the root is reached, where Left is a no-op (or falls through to the existing legacy behavior, to be decided during planning).
- With a node selected, Right expands that node into its children so the children become navigable. Right on a node with no children is a no-op (or falls through, per the same decision).
- Left and Right therefore compose into ordinary tree navigation: Left to move up, Right to open the level below, Up/Down to move within a level.
- Both surfaces, Qfc and Efc, implement the same contract.

## Actual Behavior

- Left collapses the displayed breadcrumb chain (or closes an open leaf expansion). The selected node is unchanged.
- Right re-expands a collapsed chain, or expands the leaf's children. It never expands a parent node, because no parent node is ever selected.
- On the Qfc surface, when the row transition reports nothing to do, the arrow falls through to legacy handling: Right opens the "Pop Out Item or Enumerate Conversation?" dialog and Left closes the folder drop-down.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Code-read evidence (2026-08-07) is recorded under Suspected Cause below; no runtime log capture yet.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Keyboard-only filing cannot reach a folder that is not already in the presented row set. Without parent selection plus child expansion, the user must return to the search textbox and retype to reach a sibling or cousin folder, which is the slow path the arrow-key contract exists to avoid. Severity is Medium because mouse navigation and the search textbox remain available.

## Suspected Cause / Notes

Read of the current sources on 2026-08-07. Both surfaces implement a breadcrumb display-collapse semantic rather than a tree-selection semantic.

Efc surface:

- `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:225-250` — `HandleArrowKeyAsync` maps `Right` to `row.ReExpand()` when collapsed, otherwise `ExpandLeafAsync(row)`; it maps `Left` to `row.LeftArrow()`.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs:195-216` — `LeftArrow()` closes an open leaf expansion, otherwise decrements `CollapsedAfterIndex` by one, returning `false` once only the root segment remains. This changes which segments are rendered; it does not change the selected node.

Qfc surface:

- `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs:385-386` — routes the arrow to `BreadcrumbStateModel.RightArrow()` / `LeftArrow()`.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs:424-455` — `RightArrow()` re-expands a collapsed row or calls `row.TryExpandLeaf()`; `LeftArrow()` clears `_selectedSubfolderIndex` and calls `row.TryCollapseLeaf()`. Neither reassigns the selected row or node to a parent.
- `QuickFiler/Resources/FolderBreadcrumb.html:395-404` — `onArrow` gates on `canRight = row.collapsed || (!row.leafExpanded && rowHasOpenAffordance(row))` and `canLeft = row.leafExpanded`, posting `unhandledArrow` otherwise. With no parent-selection concept, a row that is fully expanded and has no leaf affordance reports both directions as unhandled.
- `QuickFiler/Controllers/KeyboardHandler.cs:288-314` — `BreadcrumbArrowFallThrough` is the legacy target: Right opens the Pop Out / Enumerate Conversation dialog, Left issues `SetFolderDroppedDown(false)`.

The requested contract is a genuine behavior change to the arrow semantics on both surfaces, not a one-line repair. Planning must decide explicitly what happens at the boundaries (Left at the root, Right on a childless node) and whether the Qfc legacy fall-through in `BreadcrumbArrowFallThrough` is retained, re-gated, or removed. Removing it would be a user-visible change to the Pop Out / Enumerate Conversation entry point, which is out of scope for this bug unless the maintainer decides otherwise.

Relationship to issue #400: the #400 acceptance criteria state that "Left and Right retain the existing breadcrumb expansion, collapse, and fall-through behavior." This bug proposes changing exactly that behavior. The two must be reconciled during planning; #400's Up/Down/Enter/Escape selector contract is not affected.

Depends on the lineage defect filed separately for EfcViewer: parent selection is only meaningful once rows carry a resolved multi-segment ancestor chain. Sequence that fix first, or scope this one to rows whose chain already resolves.

## Proposed Fix / Validation Ideas

Design direction (to be confirmed during planning):

- Introduce an explicit selected-node concept, distinct from the selected row and from the row's display-collapse state, in the shared row/state model so both routers can move the selection up the ancestor chain.
- Map Left to "select parent node" and Right to "expand selected node into its children", keeping the existing child-list retrieval seam (`IFolderHierarchyProvider`) for the expansion.
- Share the transition logic between `BreadcrumbStateModel` (Qfc) and `BreadcrumbBridgeRouter` (Efc) rather than implementing it twice; the two surfaces already share `BreadcrumbRow`.
- Decide and document the boundary and fall-through behavior before implementation.

Validation:

- [ ] Unit coverage areas: MSTest coverage over the new transitions in the shared model — Left from a leaf selects its parent, repeated Left walks to the root, Left at the root is a no-op or reports unhandled per the chosen contract, Right on a selected parent requests and shows that parent's children, Right on a childless node is a no-op. Cover both routers so Qfc and Efc are asserted against the same contract. Use Moq for `IFolderHierarchyProvider`; no live Outlook dependency.
- [ ] Integration scenario to retest: keyboard-only navigation from a suggested leaf up two levels and back down into a different child, on both surfaces.
- [ ] Manual verification notes: confirm the Qfc Pop Out / Enumerate Conversation dialog still reachable by whatever gesture planning assigns to it, and confirm Up/Down/Enter/Escape selector behavior from issue #400 is unchanged.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
