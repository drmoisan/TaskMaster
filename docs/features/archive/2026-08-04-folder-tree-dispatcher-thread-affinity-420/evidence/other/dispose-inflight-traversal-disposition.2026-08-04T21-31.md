# In-flight traversal cancellation disposition

Timestamp: 2026-08-04T21:31:00-04:00

Command: N/A — source-backed disposition of cancellation boundaries; no command was recorded for this artifact.
EXIT_CODE: N/A — no command was run.
Output Summary: This historical disposition records that synchronous Outlook COM calls were non-cancellable at the documented boundary and therefore M3 was NON-PASS at that time; it does not claim a test result.

| Traversal segment | Disposition |
| --- | --- |
| `OutlookFolderTreeService.BuildAndPublishAsync` | Creates a linked token from the caller token and the service-owned disposal token, then passes it to the snapshot builder. |
| `FolderTreeSnapshotBuilder.BuildSnapshotAsync` | Passes the token to `ReadFoldersAsync`, checks it in the tree walk, and gives it to `IDispatcherYield.YieldAsync`. |
| `OutlookFolderHierarchyReader.ReadRecordsAsync` | Checks the token before every store and yield boundary, and passes it to `ReadStoreAsync`. |
| `OutlookFolderHierarchyReader.ReadStoreAsync` | Checks the token before each stack item and child iteration, and passes it to the yield boundary. |
| `IDispatcherYield.YieldAsync` | Receives the linked service-owned token. The controlled regression verifies cancellation at this boundary. |
| Outlook COM property/enumeration calls | `StoreID`, `GetRootFolder`, folder metadata, and `MAPIFolder.Folders` are synchronous COM calls and do not expose cancellation. They are bounded by token checks before and after calls; after disposal, the service terminal state rejects publication, scheduling, events, and notification reattachment. |

M3 status: **NON-PASS**. The live COM calls remain non-cancellable by their interop contract. This record does not treat that boundary as fixed and does not authorize final QA or an acceptance-criteria pass. The implemented terminal isolation prevents any completed non-cancellable segment from publishing or scheduling after disposal.
