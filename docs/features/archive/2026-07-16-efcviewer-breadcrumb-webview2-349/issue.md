# efcviewer-breadcrumb-webview2 (Issue #349)

- Date captured: 2026-07-16
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/efcviewer-breadcrumb-webview2/ (Issue #349)
- Epic: `folder-tree-breadcrumb-redesign` (integration branch `epic/folder-tree-breadcrumb-redesign-integration`)
- Manifest child: wave 1, complexity band C4, `depends_on: [9101]`
- Source objective: `docs/features/potential/2026-07-16-folder-tree-breadcrumb-redesign-epic-request.md` (decomposition item 2)

- Issue: #349
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/349
- Last Updated: 2026-07-17
- Work Mode: full-feature

## Problem / Why

The EfcViewer matching-folders control currently renders folder suggestions as a conventional
indented multi-row tree using `BrightIdeasSoftware.TreeListView` (`QuickFiler/Viewers/EfcViewer.cs`,
`QuickFiler/Viewers/EfcViewer3.cs`, plus their Designer files). The intended design is a single-line
breadcrumb per suggestion anchored at the selected leaf. The current hierarchy is synthesized by
`FolderSuggestionTree.BuildFromRows` via prefix-matching over the top-ranked suggestion rows, so
expanding a folder does not reveal its real Outlook subfolders. The prediction percentage is also
reported as obscured at runtime even though static column/rect math shows no overlap.

`TreeListView` (as currently used) does not naturally support single-line breadcrumb rendering with
per-segment double-click collapse, and it is a VSTO/WinForms-hosting-specific investment that would
not carry forward to the planned VSTO migration. The redesign targets WebView2 (HTML/CSS/JS), which
is largely reusable across a post-VSTO UI stack and reuses a dependency already proven in this
codebase (QuickFiler's WebView2 message-body pane, including the `cid:` fix from feature 326).

## Proposed Behavior

Replace the EfcViewer matching-folders `TreeListView` in both `EfcViewer` and `EfcViewer3` with a
WebView2-hosted HTML/CSS/JS breadcrumb control that:

1. Renders each suggestion as a single-line breadcrumb `Folder -> SubFolder -> Leaf` anchored at the
   selected/predicted leaf.
2. Shows an expand affordance (plus when collapsed, minus when expanded) only on the leaf, and only
   when the leaf has subfolders.
3. Collapses a row on double-click of a non-leaf segment, hiding everything after that segment and
   showing a plus to re-expand back to the full breadcrumb.
4. On expand of a segment, lists every real immediate Outlook subfolder of that folder via the
   shared live folder-hierarchy provider from feature 9101 (not prefix-matching over suggestion rows).
5. Keeps the prediction percentage always fully visible: first capture a runtime reproduction of the
   current obscuring defect, then apply a CSS-based fix.

A JS<->.NET event bridge carries double-click and keyboard (left/right arrow) interaction and routes
the live subfolder query across the WebView2 boundary. The percentage plumbing (feature 324) is reused
as-is; the scoring/ranking algorithm is not changed.

## Acceptance Criteria (early draft)

- [ ] Each EfcViewer suggestion renders as a single-line breadcrumb anchored at the selected leaf in a WebView2-hosted control (both `EfcViewer` and `EfcViewer3`).
- [ ] The leaf shows a plus/minus expand affordance only when it has subfolders.
- [ ] Double-clicking a non-leaf segment collapses the row after that segment with a plus to re-expand.
- [ ] Expanding a segment lists every real immediate Outlook subfolder via the 9101 provider.
- [ ] The prediction percentage is always fully visible; a runtime reproduction of the current defect precedes the CSS-based fix.
- [ ] A JS<->.NET bridge carries double-click and left/right-arrow keyboard events and routes the live subfolder query.
- [ ] No third-party WinForms tree/list control and no WPF/ElementHost are introduced; the control is WebView2 (HTML/CSS/JS).
- [ ] No change to the scoring/ranking algorithm; the C# toolchain (csharpier, analyzers, nullable, MSTest) is green and coverage thresholds are met.

## Constraints & Risks

- Depends on issue 9101 (live Outlook folder-hierarchy provider), which is merged before this feature
  during epic execution. This feature consumes 9101's contract (ancestor chain + on-demand real
  immediate subfolders behind an injectable seam) rather than re-deriving hierarchy from suggestion rows.
- Ruled out: `BrightIdeasSoftware.TreeListView` (or any third-party WinForms tree/list control) and
  WPF/`ElementHost`.
- The live Outlook subfolder query is I/O-bound and must be isolated from pure breadcrumb logic per
  the repository I/O-boundary policy so the core is unit-testable without a live Outlook process.
- Two parallel non-shared EfcViewer implementations (`EfcViewer`, `EfcViewer3`) must both be converted.
- WinForms/WebView2 host wiring and Designer-generated code are coverage-exempt per policy; testable
  pure logic (breadcrumb model, bridge message shaping) is not exempt and must meet coverage floors.

## Test Conditions to Consider

- [ ] Unit coverage: breadcrumb model construction from an ancestor chain; collapse/expand state transitions; bridge message serialization/deserialization; segment-children request shaping.
- [ ] Integration scenarios: WebView2 host initialization; JS<->.NET bridge round-trip for double-click, arrow keys, and subfolder query.
- [ ] Runtime reproduction of the percentage-obscuring defect and verification of the CSS fix.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2/` folder from the template
