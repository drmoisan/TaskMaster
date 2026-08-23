Timestamp: 2026-08-04T21:56:00-04:00
Command: & 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' 'UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll' /Tests:UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderTreeServiceInvalidationTests.NotificationRefreshFault_IsObservedWithoutUnexpectedRetry
EXIT_CODE: 0
Output Summary: The P1-T13 notification-refresh fault regression passed. The selected service observer receives the same controlled exception, the initial snapshot remains the sole publication, the reader runs exactly twice with no retry, and service state is `StaleCurrent` after the failed scheduled refresh.
