# P5-T31 fail-before evidence

Timestamp: 2026-08-05T04:18:00-04:00

Command: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /Tests:Dispose_ReentrantHierarchyReadQueuesCleanupAndReportsOriginalStageFailureOnce`

EXIT_CODE: 1

Output Summary: One test ran and failed as expected before the disposal repair.

- The command failed as expected.
- The reentrant `EntryID` getter reached `Dispose`, but the captured dispatcher received zero queued cleanup actions because the implementation used synchronous `IUiDispatcher.Invoke`.
- The controlled `FolderAdded` unsubscription fault stopped cleanup before `FolderRemoved`, `FolderChanged`, `StoreAdded`, and `StoreRemoved`; the test reported the missing stage follow-through.
- The red test also encodes the required second-dispose idempotence, zero post-cleanup adapter access, zero publication, single sink disposal, detached handlers, and original cleanup-fault observer identity requirements for P5-T33.
