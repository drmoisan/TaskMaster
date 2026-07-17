# efcviewer-breadcrumb-webview2 (Potential — Promoted)

- Date captured: 2026-07-16
- Author: Dan Moisan
- Status: Promoted -> GitHub issue #349 (https://github.com/drmoisan/TaskMaster/issues/349); active folder docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/
- Epic: `folder-tree-breadcrumb-redesign` (integration branch `epic/folder-tree-breadcrumb-redesign-integration`)
- Manifest child: wave 1, complexity band C4, `depends_on: [9101]` (manifest placeholder 9102 -> real issue #349)
- Source objective: `docs/features/potential/2026-07-16-folder-tree-breadcrumb-redesign-epic-request.md` (decomposition item 2)

> Audit-trail note: the MCP promotion (`potential_to_issue`) created issue #349 and populated the
> active `issue.md`, but the on-disk potential markdown did not persist through promotion. This file
> is recreated for audit-trail completeness; the authoritative requirements live in the active
> feature folder's `issue.md`, `spec.md`, and `user-story.md`.

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

Replace the EfcViewer matching-folders `TreeListView` with a WebView2-hosted HTML/CSS/JS breadcrumb
control that: (1) renders each suggestion as a single-line breadcrumb `Folder -> SubFolder -> Leaf`
anchored at the selected/predicted leaf; (2) shows an expand affordance (plus collapsed / minus
expanded) only on the leaf, and only when the leaf has subfolders; (3) collapses a row on double-click
of a non-leaf segment, hiding everything after that segment and showing a plus to re-expand;
(4) on expand of a segment, lists every real immediate Outlook subfolder via the shared live
folder-hierarchy provider from feature 9101 (not prefix-matching over suggestion rows); (5) keeps the
prediction percentage always fully visible via a runtime reproduction followed by a CSS-based fix.
A JS<->.NET event bridge carries double-click and left/right-arrow keyboard interaction and routes the
live subfolder query across the WebView2 boundary. The feature-324 percentage plumbing is reused
as-is; the scoring/ranking algorithm is not changed.

Note (research correction): the drafted "both EfcViewer and EfcViewer3" scope was refined during
preparation. EfcViewer3 is dead code (sole runtime instantiation is `new EfcViewer()` at
`QuickFiler/Helper Classes/EfcViewerQueue.cs:83`); the behavioral WebView2 conversion targets
`EfcViewer` + `EfcFormController` only, and EfcViewer3 receives at most a mechanical Designer-only
control swap or removal.

## Constraints & Risks

- Depends on issue 9101 (live Outlook folder-hierarchy provider), merged before this feature during
  epic execution. Consumes 9101's contract (ancestor chain + on-demand real immediate subfolders
  behind an injectable seam) rather than re-deriving hierarchy from suggestion rows.
- Ruled out: `BrightIdeasSoftware.TreeListView` (or any third-party WinForms tree/list control) and
  WPF/`ElementHost`.
- The live Outlook subfolder query is I/O-bound and must be isolated from pure breadcrumb logic so
  the core is unit-testable without a live Outlook process.
- WinForms/WebView2 host wiring and Designer-generated code are coverage-exempt per policy; testable
  pure logic (breadcrumb model, bridge message shaping) is not exempt and must meet coverage floors.

## Next Step

- [x] Promote to GitHub issue (feature request template) — issue #349
- [x] Create `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/` folder from the template
