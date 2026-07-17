# quickfiler-breadcrumb-webview2 (Issue #351)

- Date captured: 2026-07-16
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-breadcrumb-webview2/ (Issue #351)
- Epic: folder-tree-breadcrumb-redesign (integration branch `epic/folder-tree-breadcrumb-redesign-integration`)
- Decomposition item: 3 (manifest placeholder issue 9103, wave 1, complexity band C4)
- Depends on: issue 9101 (live Outlook folder-hierarchy provider), prepared in parallel and merged before this feature during epic execution
- Independent of: issue 9102 (EfcViewer breadcrumb), executes in parallel

- Issue: #351
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/351
- Last Updated: 2026-07-17
- Work Mode: full-feature

## Problem / Why

The `folder-tree-percentage-ui` epic (issues 324, 325, 326, 327) delivered a QuickFiler folder
dropdown whose behavior does not match the intended design. The dropdown is still a stock
`System.Windows.Forms.ComboBox` (`CboFolders`, `DrawMode=OwnerDrawFixed`) in the single live
`ItemViewer` variant. Its hierarchy is synthesized by `FolderHierarchyBuilder.Build`, which
splits the same set of at most five suggestion paths on `\`; it does not query the real Outlook
subfolder structure. The prediction percentage is not reliably fully visible.

This feature replaces that control with a WebView2-hosted HTML/CSS/JS breadcrumb control,
following the pattern QuickFiler already uses for its WebView2 message-body pane (feature 326
inline-image cid fix). It is the QuickFiler-surface counterpart to the EfcViewer breadcrumb
(9102) and consumes the shared live folder-hierarchy provider (9101).

## Proposed Behavior

Replace the QuickFiler `CboFolders` `ComboBox` with a WebView2-hosted breadcrumb control whose
required behavior mirrors the EfcViewer surface:

1. Single-line breadcrumb `Folder -> SubFolder -> Leaf`, anchored at the selected/predicted leaf.
2. A leaf-only expand affordance (plus when collapsed, minus when expanded), shown only when the
   leaf has subfolders.
3. Double-clicking a non-leaf segment collapses the row after that segment, leaving a plus to the
   left of the now-terminal segment that re-expands the full breadcrumb.
4. Expanding a segment lists every real immediate Outlook subfolder of that folder via the shared
   provider from feature 9101 (live Outlook query behind an injectable seam), replacing
   `FolderHierarchyBuilder.Build`.
5. The prediction percentage is always fully visible and unobstructed. Capture a runtime
   reproduction of the obscuring defect first, then apply a CSS-based fix.

Build the JS<->.NET event bridge for double-click and keyboard (left/right arrow) interaction and
route the live subfolder query across that bridge.

## Upstream Contract (issue 9101)

Given a selected leaf folder, the provider returns the ordered ancestor chain
`Folder -> ... -> Leaf` and, on demand, the real immediate subfolders of a given segment (live
Outlook query behind an injectable seam). It replaces the prefix-matching logic in
`FolderSuggestionTree.BuildFromRows` and `FolderHierarchyBuilder.Build`. This feature's spec and
atomic plan cite this provider as the source of both the ancestor chain and the on-demand
subfolder listing. Issue 9101 is executed and merged before this feature.

## Acceptance Criteria (early draft)

- [ ] The live QuickFiler viewer variant's `CboFolders` `ComboBox` is replaced by a WebView2-hosted
  HTML/CSS/JS breadcrumb control, following the QuickFiler WebView2 message-body pane pattern.
- [ ] Each suggestion renders as a single-line breadcrumb `Folder -> SubFolder -> Leaf` anchored at
  the leaf.
- [ ] The leaf carries a plus/minus expand affordance only when it has subfolders.
- [ ] Double-clicking a non-leaf segment collapses the row after it and shows a plus to re-expand.
- [ ] Expanding a segment lists every real immediate Outlook subfolder via the 9101 provider, not
  only subfolders present among the top-ranked suggestions.
- [ ] The prediction percentage is fully visible and unobstructed (runtime reproduction captured,
  CSS-based fix applied).
- [ ] A JS<->.NET event bridge handles double-click and keyboard (left/right arrow) interaction and
  routes the live subfolder query.
- [ ] No third-party WinForms tree/list control and no WPF/ElementHost are introduced.
- [ ] The scoring/ranking algorithm and model output are unchanged.
- [ ] Full C# toolchain (csharpier, .NET analyzers, nullable, MSTest) is green; changed and new
  code meets repository coverage thresholds; breadcrumb core logic is unit-testable without a live
  Outlook process.

## Constraints & Risks

- Do NOT introduce a third-party WinForms tree/list control or WPF/ElementHost. The control
  technology is WebView2 (HTML/CSS/JS).
- Do not change the scoring/ranking algorithm or model output; the surfaced percentage is the
  score already computed for internal ranking (feature 324 plumbing reused as-is).
- Re-verify which QuickFiler viewer variant(s) are actually live before fixing scope. The prior
  epic scoped to the single live `ItemViewer` and left nine other declared variants
  (`QfcItemViewerV1`, `QfcItemViewerExpanded`, etc.) untouched. Re-confirm this liveness
  assessment during research rather than assuming it still holds; record the scope decision in the
  spec.
- Live Outlook I/O must be isolated behind an injectable seam so the breadcrumb core logic is
  unit-testable without a live Outlook process.

## Test Conditions to Consider

- [ ] Unit coverage: pure breadcrumb-model logic (ancestor-chain rendering, collapse/expand state
  transitions) without a live Outlook process.
- [ ] Integration scenarios: JS<->.NET bridge message routing for double-click, keyboard, and live
  subfolder query.
- [ ] Runtime reproduction of the percentage-obscuring defect, then verification the CSS fix
  resolves it.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/2026-07-16-quickfiler-breadcrumb-webview2/` folder from the template
