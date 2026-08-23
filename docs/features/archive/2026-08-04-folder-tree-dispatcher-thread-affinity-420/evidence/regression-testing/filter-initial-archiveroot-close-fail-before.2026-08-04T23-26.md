# P5-T13 Initial ArchiveRoot Close Failure Evidence

Timestamp: 2026-08-04T23:26:00-04:00
Command: `vstest.console.exe UtilitiesCS.Test\\bin\\Debug\\UtilitiesCS.Test.dll /Tests:InitialArchiveRootClose_BeforeCompatibilityView_DoesNotCommit`
EXIT_CODE: 1
Output Summary: Expected-red result: the reentrant ArchiveRoot close left a compatibility view after the viewer closed before wiring.

- Timestamp: 2026-08-04T23:26 America/New_York
- Test: `UtilitiesCS.Test.EmailIntelligence.FilterOlFoldersControllerRefreshDisposalTests.InitialArchiveRootClose_BeforeCompatibilityView_DoesNotCommit`
- Command: `vstest.console.exe UtilitiesCS.Test\\bin\\Debug\\UtilitiesCS.Test.dll /Tests:InitialArchiveRootClose_BeforeCompatibilityView_DoesNotCommit`
- EXIT_CODE: 1 (expected)
- Result: 1 test failed in 0.344 seconds.
- Failed assertion: `controller.FolderTreeView.Should().BeNull()` found a `FolderTreeCompatibilityView` after the `ArchiveRoot` getter reentrantly closed the viewer before compatibility-view wiring.
- Process hygiene: no `vstest` process was present before the run or remained after it.

This is the expected red result before the lifecycle guard is implemented.
