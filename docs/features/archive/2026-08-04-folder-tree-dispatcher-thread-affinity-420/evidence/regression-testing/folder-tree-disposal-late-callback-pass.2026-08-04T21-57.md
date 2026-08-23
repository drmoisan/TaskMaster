# Folder-tree disposal late-callback regression

Timestamp: 2026-08-04T21:57:00-04:00 (derived from the artifact filename)
Command: Multiple recorded commands — CSharpier format, the analyzer build, and `vstest.console.exe UtilitiesCS.Test\\bin\\Debug\\UtilitiesCS.Test.dll /Tests:UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderTreeServiceDisposalTests.Dispose_UnsubscribesNotificationsAndSuppressesLaterEvents`.
EXIT_CODE: 0
Output Summary: The recorded targeted test passed 1/1 and established late-callback suppression, exact-once notification-sink disposal, and no live Outlook or UI dependency.

Task: [P5-T5]

The deterministic test `UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderTreeServiceDisposalTests.Dispose_UnsubscribesNotificationsAndSuppressesLaterEvents` retained the `FolderChanged` callback before disposal, called `Dispose` twice, and then invoked the retained callback directly. It verified no additional reader enumeration, snapshot publication, or `SnapshotChanged` event, no remaining folder-change subscription, and exactly one notification-sink disposal.

Verification commands:

```powershell
dotnet tool run csharpier format 'UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceDisposalTests.cs'
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' 'UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll' /Tests:UtilitiesCS.Test.OutlookObjects.Folder.OutlookFolderTreeServiceDisposalTests.Dispose_UnsubscribesNotificationsAndSuppressesLaterEvents
```

Results: CSharpier formatted the changed file; the solution analyzer build succeeded with seven existing warnings and no errors; the targeted MSTest run passed 1 of 1 tests. The test uses fake reader and notification-sink seams only; it creates no live Outlook or UI objects and uses no timing or polling.
