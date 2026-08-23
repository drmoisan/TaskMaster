Timestamp: 2026-08-04T19-21
Command: C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /Tests:UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderTreeServiceConcurrencyTests.GetSnapshotAsync_WorkerOriginatedColdBuild_UsesCapturedStaDispatcher
EXIT_CODE: 1
Output Summary: The new deterministic worker-originated cold-build test failed before repair. `WpfDispatcherYield.YieldAsync` raised `InvalidOperationException: The thread calling Dispatcher.Yield does not have a current Dispatcher.` The stack included `FolderTreeSnapshotBuilder.YieldIfNeededAsync`, `BuildSnapshotAsync`, and `OutlookFolderTreeService.BuildAndPublishAsync`.

The test uses a fake hierarchy reader that records access thread IDs and a dedicated in-process STA dispatcher as the required execution boundary. No Outlook, network, temporary file, sleep, timer, or retry was used.
