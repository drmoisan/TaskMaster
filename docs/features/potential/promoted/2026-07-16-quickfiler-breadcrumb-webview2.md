# quickfiler-breadcrumb-webview2 (Promoted)

- Date captured: 2026-07-16
- Author: Dan Moisan
- Status: Promoted -> Issue #351 (https://github.com/drmoisan/TaskMaster/issues/351)
- Active feature folder: docs/features/active/2026-07-16-quickfiler-breadcrumb-webview2-351/
- Epic: folder-tree-breadcrumb-redesign (integration branch `epic/folder-tree-breadcrumb-redesign-integration`)
- Decomposition item: 3 (manifest placeholder issue 9103, wave 1, complexity band C4)
- Depends on: issue 9101 (live Outlook folder-hierarchy provider), merged before this feature during epic execution
- Independent of: issue 9102 (EfcViewer breadcrumb), executes in parallel

Note: this promoted record is recreated for audit-trail completeness. The MCP promotion in this
repository populates the GitHub issue and the active-folder `issue.md` but does not persist the
potential/promoted markdown on disk. The authoritative record is the active feature folder
(`issue.md`, `spec.md`, `user-story.md`, `plan.2026-07-16T21-53.md`, and `research/`).

## Problem / Why

The `folder-tree-percentage-ui` epic (issues 324, 325, 326, 327) delivered a QuickFiler folder
dropdown whose behavior does not match the intended design. The dropdown is still a stock
`System.Windows.Forms.ComboBox` (`CboFolders`, `DrawMode=OwnerDrawFixed`) in the single live
`ItemViewer` variant. Its hierarchy is synthesized by `FolderHierarchyBuilder.Build`, which splits
at most five suggestion paths on `\`; it does not query the real Outlook subfolder structure. The
prediction percentage is not reliably fully visible.

This feature replaces that control with a WebView2-hosted HTML/CSS/JS breadcrumb control, following
the pattern QuickFiler already uses for its WebView2 message-body pane (feature 326 inline-image cid
fix), and consumes the shared live folder-hierarchy provider (9101).

## Proposed Behavior

1. Single-line breadcrumb `Folder -> SubFolder -> Leaf` anchored at the selected/predicted leaf.
2. A leaf-only expand affordance (plus/minus), shown only when the leaf has subfolders.
3. Double-clicking a non-leaf segment collapses the row after it, with a plus to re-expand.
4. Expanding a segment lists every real immediate Outlook subfolder via the shared 9101 provider
   (live query behind an injectable seam), replacing `FolderHierarchyBuilder.Build`.
5. The prediction percentage is always fully visible (runtime reproduction captured first, then a
   CSS-based fix).
6. A JS<->.NET event bridge handles double-click and keyboard (left/right arrow) interaction and
   routes the live subfolder query.

## Constraints

- No third-party WinForms tree/list control and no WPF/ElementHost. Control technology is WebView2.
- No change to the scoring/ranking algorithm or model output (feature 324 plumbing reused as-is).
- Live Outlook I/O isolated behind an injectable seam; breadcrumb core logic unit-testable without a
  live Outlook process.
- Scope decision (research-confirmed): the single live viewer variant is `ItemViewer`; the nine
  other declared variants are Designer-only dead types and are out of scope.
