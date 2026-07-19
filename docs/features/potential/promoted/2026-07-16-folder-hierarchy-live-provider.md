# folder-hierarchy-live-provider (Potential — Promoted)

- Date captured: 2026-07-16
- Author: Dan Moisan
- Status: Promoted -> GitHub issue #350, active folder docs/features/active/2026-07-16-folder-hierarchy-live-provider-350/
- Epic: folder-tree-breadcrumb-redesign (manifest issue placeholder 9101, wave 0, complexity band C3)
- Integration branch: epic/folder-tree-breadcrumb-redesign-integration
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/350

> Recreated for audit trail. The MCP promotion tool consumed the original
> `docs/features/potential/2026-07-16-folder-hierarchy-live-provider.md` and populated the active
> feature folder's `issue.md`, but did not persist the promoted copy to disk. The full requirements
> now live in `docs/features/active/2026-07-16-folder-hierarchy-live-provider-350/issue.md`,
> `spec.md`, and `user-story.md`.

## Problem / Why

The `folder-tree-breadcrumb-redesign` epic replaces the EfcViewer indented tree and the QuickFiler
folder dropdown with a single-line breadcrumb control in both surfaces. Both surfaces need to render
a `Folder -> ... -> Leaf` ancestor chain for a selected folder and, on demand, list the real
immediate Outlook subfolders of a given segment.

Today the hierarchy is synthesized only from the already-presented top-ranked suggestion rows:

- `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs` (`BuildFromRows`) derives parent/child
  edges by prefix-matching among the top-5-plus-recents suggestion rows.
- `UtilitiesCS/OutlookObjects/Folder/FolderHierarchyBuilder.cs` (`Build`) splits the same <=5
  suggestion paths on `\`.

Neither queries Outlook's real subfolder structure. This is the epic's single shared upstream
contract; two UI consumers (issues 9102 EfcViewer and 9103 QuickFiler) depend on it.

## Proposed Behavior

Introduce a live Outlook folder-hierarchy provider with a clear public contract. Given a selected leaf
folder it returns (a) the ordered ancestor chain `Folder -> ... -> Leaf` (root-to-leaf segments) for
breadcrumb rendering, and (b) on demand, the real immediate subfolders of a given segment, queried
live against the real Outlook hierarchy. The live Outlook query is isolated behind an injectable seam
so the pure ancestor-chain and segment-children logic is unit-testable without a live Outlook process
(MSTest/Moq/FluentAssertions). The scoring/ranking algorithm and feature 324 probability plumbing are
reused as-is and are out of scope.

## Next Step

- [x] Promote to GitHub issue (feature request template) — issue #350
- [x] Create `docs/features/active/2026-07-16-folder-hierarchy-live-provider-350/` folder
