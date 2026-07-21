# Batch F5 Nullable Gate (P9-T3)

Timestamp: 2026-07-19T15-30

- Phase 8 (Batch S2) completed before this task (StoreWrapper/StoresWrapper committed earlier), satisfying the
  hard ordering: OutlookFolderHierarchyReader consumes StoresWrapper.
- csharpier format EXIT 0; full /t:Build EXIT 0.
- Scoped gate: **zero CS86xx** for the 5 Batch F5 files (AC1).

## Files remediated (5)
OutlookFolderHandleResolver.cs, OutlookFolderHierarchyReader.cs, OutlookFolderNotificationSink.cs,
MsgToMime/MAPIMethods.cs, WpfDispatcherYield.cs.

## Key annotation decisions
- OutlookFolderHandleResolver.TryResolve(FolderTreeSnapshotNode? node, out object? folder) matches the F0
  interface; Resolve returns `folder!` (TryResolve==true guarantees non-null). [ExcludeFromCodeCoverage] class.
- OutlookFolderHierarchyReader: `GetRootFolder` returns `IOutlookFolderAdapter?`; all request/clock params
  (`FolderTreeRequest? request`, `IDeadlineClock? deadlineClock`, `IDispatcherYield? dispatcherYield`) made
  nullable across ReadFoldersAsync/ReadRecords/ReadRecordsAsync/ReadStoreAsync/YieldIfNeededAsync, matching the
  F0 interface (the clocks are null-tolerant; YieldIfNeededAsync guards `deadlineClock == null`).
- OutlookFolderNotificationSink (**499 lines, under the 500-line limit — not split, no flag required**):
  the six public events and the two inner-class `_handler` fields made nullable; the inner
  `FolderTreeNotification` ctor takes `string? storeId` (`?? string.Empty`) so the existing `store?.StoreID`
  null-conditional patterns need no new guards (AC4).
- MsgToMime/MAPIMethods.cs (COM interop declarations) and WpfDispatcherYield.cs ([ExcludeFromCodeCoverage])
  needed only the pragma. No new runtime guards on the COM-exempt classes (AC4). No post-condition attributes;
  no record/init.
