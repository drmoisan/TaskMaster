# Issue 995 issue-995-csharp-green

Timestamp: 2026-10-17T23-09
Command: vstest.console.exe UtilitiesCS.Test and QuickFiler.Test /TestCaseFilter:Issue995,renderer,codec,all-router,all-EfcFormController /Settings:scripts/vscode/TaskMaster.cli.runsettings /ResultsDirectory:SCRATCH/issue-995-green-results
EXIT_CODE: 0
Output Summary:   Passed BindRowsAsync_WithNoPriorSelection_RaisesNoSelectedFolderPathChangedEvent [1 ms]
  Passed BindRowsAsync_DoesNotAutoSelectAnyRow [< 1 ms]
  Passed Boundary_QfcUnhandledArrow_StillReachesBreadcrumbArrowFallThrough [803 ms]
  Passed ArrowKeyUp_AtTopSelectableRow_PostsFocusSearchAndRaisesEvent [< 1 ms]
  Passed RowSelected_UpdatesSelectedFolderPathAndRaisesEvent [< 1 ms]
  Passed RowSelected_OnBannerRow_IsIgnored [< 1 ms]
  Passed SelectFirstRow_SelectsTopSelectableRowAndPostsRender [< 1 ms]
  Passed ApplyTheme_Dark_ReDeliversDarkDocument [< 1 ms]
  Passed ArrowKeyDown_SelectsNextSelectableRow [< 1 ms]
  Passed ArrowKeyUp_OnNonTopRow_SelectsPreviousRowWithoutFocusSearch [< 1 ms]
  Passed LeafExpandToggle_OnCollapsedRow_ReExpandsWithoutProviderQuery [1 ms]
  Passed LeafExpandToggle_OnExpandedLeaf_CollapsesWithoutSecondQuery [< 1 ms]
  Passed ArrowKeyRight_WhenCollapsed_ReExpandsWithoutProviderQuery [< 1 ms]
  Passed ArrowKey_UnknownKey_IsLoggedNoOp [< 1 ms]
  Passed RowSelected_OnTrashPseudoRow_SelectsTrashPath [< 1 ms]
  Passed Issue614_SegmentActivate_StoreRootSegment_DoesNotStoreFullOutlookPath [1 ms]
Results File: <host-path>

Test Run Successful.
Total tests: 232
     Passed: 232
 Total time: 3.1374 Seconds
