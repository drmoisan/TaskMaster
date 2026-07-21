# Batch F4 Nullable Gate (P5-T3)

Timestamp: 2026-07-19T13-45

- csharpier format EXIT 0; full /t:Build EXIT 0.
- Scoped gate (UtilitiesCS Rebuild, TreatWarningsAsErrors, BuildProjectReferences=false): **zero CS86xx** for the
  2 Batch F4 files (AC1).

## Files remediated (2)
OutlookFolderHierarchyProvider.cs, OutlookFolderTreeService.cs.

## Key annotation decisions
- OutlookFolderHierarchyProvider.ResolveLeafKeyAsync returns `Task<FolderTreeNodeKey?>` (matches the P1
  IFolderHierarchyProvider contract; returns null / match?.Key).
- OutlookFolderTreeService fields that start null are nullable: `_snapshot`, `_inFlightSnapshot`,
  `_scheduledRefresh`, `_pendingRefreshRequest`; `SnapshotChanged` event nullable.
- The request/snapshot pipeline handles null defensively (`request?.`, `== null` guards), so the
  request/currentSnapshot params of `BuildAndPublishAsync`, `CreatePublishedSnapshot`, and
  `MergeRefreshRequests` are nullable; `CreatePublishedSnapshot` returns `refreshedSnapshot!` (provably non-null
  from the awaited build) in its early-out.
- Cross-batch contract refinements (consistent nullable shape): `FolderTreeSnapshotBuilder.BuildSnapshotAsync`
  and `IOutlookFolderHierarchyReader.ReadFoldersAsync` take `FolderTreeRequest? request` (reader guards
  `request != null`); `FolderTreeSnapshotChangedEventArgs` ctor `affectedStoreIds` is nullable (it `?? Empty`s).
- No post-condition attributes; no record/init.
