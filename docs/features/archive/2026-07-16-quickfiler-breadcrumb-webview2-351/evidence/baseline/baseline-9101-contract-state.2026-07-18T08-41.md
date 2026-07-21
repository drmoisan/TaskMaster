# Phase 0 — 9101/#350 Provider Contract State (P0-T8)

Timestamp: 2026-07-18T08-50

Search commands/patterns used:
- `ls UtilitiesCS/OutlookObjects/Folder/`
- `grep -rn "IFolderHierarchyProvider|FolderBreadcrumbSegment|GetAncestorChain|GetImmediateSubfolders" UtilitiesCS/OutlookObjects/Folder/ --include=*.cs -l`

SearchScope: `UtilitiesCS/OutlookObjects/Folder/` (execution worktree, branch `feature/quickfiler-breadcrumb-webview2-351`, base commit 8e242692 which includes wave-0 feature #350 merged via PR #353)
SearchResult:
- `UtilitiesCS/OutlookObjects/Folder/IFolderHierarchyProvider.cs` — public interface `IFolderHierarchyProvider` with `Task<IReadOnlyList<FolderBreadcrumbSegment>> GetAncestorChainAsync(FolderTreeNodeKey leafKey, CancellationToken)`, `Task<IReadOnlyList<FolderBreadcrumbSegment>> GetImmediateSubfoldersAsync(FolderTreeNodeKey segmentKey, CancellationToken)`, `Task<FolderTreeNodeKey> ResolveLeafKeyAsync(string folderPath, CancellationToken)`.
- `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbSegment.cs` — immutable net48-safe sealed class with ctor `(FolderTreeNodeKey key, string displayName, string folderPath, bool hasChildren)` and get-only members `Key`, `DisplayName`, `FolderPath`, `HasChildren`.
- `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs` — sealed production facade implementing `IFolderHierarchyProvider` over the injectable `IOutlookFolderTreeService` snapshot seam (no COM in the facade; `HasChildren` derived from `FolderTreeSnapshotNode.ChildKeys.Count > 0`).
- `UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotQueries.cs` — includes `GetAncestorChain` used by the facade.
- Legacy `FolderHierarchyBuilder.cs` (`Build`) remains present, as expected until Phase 5 rewires the QuickFiler consumer.

Verdict:
9101-CONTRACT: PRESENT IFolderHierarchyProvider, FolderBreadcrumbSegment, OutlookFolderHierarchyProvider, FolderTreeSnapshotQueries.GetAncestorChain (namespace `UtilitiesCS.OutlookObjects.Folder`)

Deviations from the plan's assumed contract (to reconcile in P2-T1):
- Chain/subfolder queries are keyed by `FolderTreeNodeKey`, not by `string leafFolderPath`; a separate `ResolveLeafKeyAsync(string folderPath, ...)` bridges path -> key.
- The DTO member is `HasChildren`, not `HasSubfolders`, and the DTO additionally carries `Key`.
