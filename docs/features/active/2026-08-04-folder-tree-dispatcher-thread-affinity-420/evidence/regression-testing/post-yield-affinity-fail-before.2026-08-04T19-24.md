Timestamp: 2026-08-04T19:24:00-04:00
Command: & 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /Tests:UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderHierarchyReaderTests.ReadRecordsAsync_AfterForcedYield_KeepsFolderAccessOnDispatcher
EXIT_CODE: 1
Output Summary: Expected pre-fix failure. The forced-yield reader test observed all post-yield fake folder adapter accesses on worker thread 27 instead of the dedicated dispatcher host thread. The assertion reported access IDs {27, 27, 27, 27, 27, 27, 27}; this proves ConfigureAwait(false) moves the live traversal continuation off the captured STA dispatcher.
