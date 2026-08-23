# P5-T16 refresh ArchiveRoot close red evidence

Timestamp: 2026-08-04T23:50:00-04:00
Command: `Get-Process vstest`; `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:UtilitiesCS.Test.EmailIntelligence.FilterOlFoldersControllerRefreshDisposalTests.RefreshArchiveRootClose_BeforeCompatibilityView_DoesNotCommit`
EXIT_CODE: 1
Output Summary: No active `vstest` process was present. The one targeted test failed at `FolderTreeView.BeSameAs(initialView)`: a new compatibility view was committed after the reentrant close during the refresh `ArchiveRoot` read.

The deterministic delayed refresh completed before the reentrant `ArchiveRoot` callback closed the viewer. The current refresh path did not recheck terminal state between compatibility-view construction and commit, so it replaced the existing view after disposal. The test additionally requires zero refresh notification and zero retained service handler.
