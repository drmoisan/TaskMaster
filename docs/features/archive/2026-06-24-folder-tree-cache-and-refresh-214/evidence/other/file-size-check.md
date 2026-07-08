# File Size Check

Timestamp: 2026-06-24T19:14:33-04:00
Command: git status --short; count changed and untracked *.cs, *.ps1, *.psm1, and *.psd1 files with Get-Content; require each <= 500 lines.
EXIT_CODE: 0
Output Summary: PASS. 69 touched production/test/reusable script files are <= 500 lines.

| File | Lines | Status |
| --- | ---: | --- |
| TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs | 62 | PASS |
| TaskMaster.Test/Ribbon/RibbonControllerTests.cs | 451 | PASS |
| TaskMaster/AppGlobals/AppOlObjects.cs | 449 | PASS |
| TaskMaster/Ribbon/RibbonController.cs | 265 | PASS |
| TaskMaster/Ribbon/RibbonController.FolderTree.cs | 269 | PASS |
| TaskMaster/Ribbon/RibbonController.Intelligence.cs | 405 | PASS |
| UtilitiesCS.Test/EmailIntelligence/EmailDataMiner_Additional_Tests.cs | 487 | PASS |
| UtilitiesCS.Test/EmailIntelligence/EmailDataMiner_FolderExtractionCoverage_Tests.cs | 155 | PASS |
| UtilitiesCS.Test/EmailIntelligence/EmailDataMiner_TestSupport.cs | 483 | PASS |
| UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersController_Tests.cs | 481 | PASS |
| UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersViewer_Tests.cs | 272 | PASS |
| UtilitiesCS.Test/EmailIntelligence/SubjectMapSco_Orchestration_Tests.cs | 497 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/DeadlineClockTests.cs | 46 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderHandleResolverFakeTests.cs | 57 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeCompatibilityViewDisposalTests.cs | 73 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeCompatibilityViewTests.cs | 116 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeNodeKeyTests.cs | 123 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeNotificationFakeTests.cs | 71 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeRequestTests.cs | 109 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeSelectionOverlayTests.cs | 75 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeSnapshotBuilderCancellationTests.cs | 65 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeSnapshotBuilderTests.cs | 95 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeSnapshotBuilderYieldTests.cs | 71 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeSnapshotNodeTests.cs | 121 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeSnapshotQueriesTests.cs | 246 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeSnapshotTests.cs | 186 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeYieldSeamTests.cs | 54 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHandleResolverTests.cs | 64 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyReaderTests.cs | 114 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderNotificationSinkTests.cs | 90 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceConcurrencyTests.cs | 65 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceDisposalTests.cs | 57 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceInvalidationTests.cs | 80 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceStateTests.cs | 364 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs | 28 | PASS |
| UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperControllerTests.cs | 209 | PASS |
| UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.cs | 143 | PASS |
| UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.FolderExtraction.cs | 475 | PASS |
| UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.Serialization.cs | 404 | PASS |
| UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.Transform.cs | 410 | PASS |
| UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersController.cs | 343 | PASS |
| UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersViewer.cs | 127 | PASS |
| UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FolderInfoViewer.cs | 66 | PASS |
| UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/IFilterOlFoldersViewer.cs | 43 | PASS |
| UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs | 273 | PASS |
| UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs | 39 | PASS |
| UtilitiesCS/OutlookObjects/Folder/DeadlineClock.cs | 35 | PASS |
| UtilitiesCS/OutlookObjects/Folder/FolderTreeCompatibilityView.cs | 81 | PASS |
| UtilitiesCS/OutlookObjects/Folder/FolderTreeNodeKey.cs | 77 | PASS |
| UtilitiesCS/OutlookObjects/Folder/FolderTreeRefreshReason.cs | 17 | PASS |
| UtilitiesCS/OutlookObjects/Folder/FolderTreeRequest.cs | 51 | PASS |
| UtilitiesCS/OutlookObjects/Folder/FolderTreeSelectionOverlay.cs | 54 | PASS |
| UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshot.cs | 101 | PASS |
| UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotBuilder.cs | 78 | PASS |
| UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotChangedEventArgs.cs | 36 | PASS |
| UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotNode.cs | 86 | PASS |
| UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotQueries.cs | 130 | PASS |
| UtilitiesCS/OutlookObjects/Folder/IDeadlineClock.cs | 13 | PASS |
| UtilitiesCS/OutlookObjects/Folder/IDispatcherYield.cs | 14 | PASS |
| UtilitiesCS/OutlookObjects/Folder/IFolderHandleResolver.cs | 13 | PASS |
| UtilitiesCS/OutlookObjects/Folder/IOutlookFolderHierarchyReader.cs | 17 | PASS |
| UtilitiesCS/OutlookObjects/Folder/IOutlookFolderNotificationSink.cs | 25 | PASS |
| UtilitiesCS/OutlookObjects/Folder/IOutlookFolderTreeService.cs | 22 | PASS |
| UtilitiesCS/OutlookObjects/Folder/OutlookFolderHandleResolver.cs | 72 | PASS |
| UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyReader.cs | 225 | PASS |
| UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyRecord.cs | 45 | PASS |
| UtilitiesCS/OutlookObjects/Folder/OutlookFolderNotificationSink.cs | 137 | PASS |
| UtilitiesCS/OutlookObjects/Folder/OutlookFolderTreeService.cs | 199 | PASS |
| UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs | 21 | PASS |
