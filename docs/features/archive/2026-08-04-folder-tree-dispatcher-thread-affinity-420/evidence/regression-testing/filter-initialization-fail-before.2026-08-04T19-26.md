Timestamp: 2026-08-04T19:26:00-04:00
Command: & 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /Tests:UtilitiesCS.Test.EmailIntelligence.FilterOlFoldersControllerInitializationTests.CreateAsync_WiresViewerOnlyAfterSnapshotCompletes
EXIT_CODE: 1
Output Summary: Expected pre-fix failure. Reflection did not find the non-public static CreateAsync(IApplicationGlobals, IFilterOlFoldersViewer) asynchronous factory. The existing constructor synchronously waits on GetFolderTreeSnapshotAsync with GetAwaiter().GetResult(), so it cannot provide the required incomplete-task initialization boundary.
