---
name: folder-hierarchy-provider-350
description: Issue #350 / epic 9101 — live folder-hierarchy provider should reuse existing snapshot infra, not add a new COM seam
metadata:
  type: project
---

Epic `folder-tree-breadcrumb-redesign` child 9101 (issue #350, wave 0, C3): live Outlook
folder-hierarchy provider (ancestor chain + on-demand immediate subfolders) for the WebView2
breadcrumb consumers 9102 (EfcViewer) and 9103 (QuickFiler).

Key reconciliation finding (non-obvious, verified 2026-07-16): the live `MAPIFolder.Folders` query
is ALREADY isolated in `UtilitiesCS/OutlookObjects/Folder/`:
- `IOutlookFolderHierarchyReader` (COM-exempt impl recursively reads `MAPIFolder.Folders`)
- `FolderTreeSnapshotBuilder` -> immutable `FolderTreeSnapshot`
- `IOutlookFolderTreeService` (cached + notification-refreshed), exposed on `IOlObjects.FolderTreeService`
- `FolderTreeSnapshot.GetChildren(key)` already returns real immediate subfolders; nodes carry
  `ParentKey`/`ChildKeys` and stable `FolderTreeNodeKey` identity.
Only genuine gap: no ordered root-to-leaf ancestor-chain helper. Recommendation: add pure
`FolderTreeSnapshotQueries.GetAncestorChain`, a shared `IFolderHierarchyProvider` facade over
`IOutlookFolderTreeService`, and a `FolderBreadcrumbSegment` DTO. Adds ZERO new COM/exempt code.

**Why:** avoids duplicating the COM boundary + refresh state machine + fakes; keeps provider fully
unit-testable; keeps 9101 independently mergeable in wave 0.
**How to apply:** for 9102/9103, consume `IFolderHierarchyProvider` from globals; join the percentage
per FolderPath from the existing 324 plumbing (provider DTO is deliberately probability-free).
Scope note: 9101 does NOT delete `FolderSuggestionTree.BuildFromRows` /
`FolderHierarchyBuilder.Build` — their only live callers are `EfcFormController.BindFolderRows`
(#885) and `ItemViewer.FolderSearch.SetFolderSuggestions` (#26), which 9102/9103 replace; deletion is
deferred to those UI features.

Research artifact: docs/features/active/2026-07-16-folder-hierarchy-live-provider-350/research/2026-07-16T21-40-folder-hierarchy-live-provider-research.md
