# Baseline File Inventory (P0-T2)

Timestamp: 2026-07-19T10-53

Command: `find UtilitiesCS/OutlookObjects/Folder UtilitiesCS/OutlookObjects/Store -name "*.cs"` with per-file `wc -l` and `grep -q "#nullable enable"`.

## Summary

- Total `.cs` files under `UtilitiesCS/OutlookObjects/Folder/` (incl. `MsgToMime/`) and `UtilitiesCS/OutlookObjects/Store/`: **83**
- Already `#nullable enable` (verify-only): **18** (17 Folder + `StoreRehookResult.cs`)
- Designer-generated, recommended non-opted-in: **2** (`DisabledStoresViewer.Designer.cs`, `StoreWrapperViewer.Designer.cs`)
- Remediation targets (opt-in this feature): **63**

## Pre-existing >500-line files (annotation-only; do NOT split)

- `FolderPredictor.cs` — 974 lines
- `FolderScorer.cs` — 663 lines
- `FolderWrapper .cs` — 531 lines (note literal trailing space before `.cs`)

## Near-limit file (watch 500-line ceiling)

- `OutlookFolderNotificationSink.cs` — 498 lines (flag rather than split if annotation pushes over 500)

## Full Inventory (path | line count | status)

| File | Lines | Status |
| --- | --- | --- |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.cs | 443 | already-enabled |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs | 118 | already-enabled |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs | 225 | already-enabled |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs | 184 | already-enabled |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs | 152 | already-enabled |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs | 230 | already-enabled |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs | 265 | already-enabled |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs | 236 | already-enabled |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbSegment.cs | 45 | already-enabled |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionMap.cs | 120 | already-enabled |
| UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs | 313 | already-enabled |
| UtilitiesCS/OutlookObjects/Folder/DeadlineClock.cs | 35 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs | 348 | already-enabled |
| UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbSegment.cs | 53 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs | 243 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/FolderHierarchyBuilder.cs | 107 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs | 183 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/FolderNavigator.cs | 72 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/FolderNodeViewModel.cs | 91 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs | 974 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/FolderPredictor.IFolderSearchHandler.cs | 10 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/FolderProbabilityAdapter.cs | 67 | already-enabled |
| UtilitiesCS/OutlookObjects/Folder/FolderRow.cs | 61 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/FolderScore.cs | 53 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs | 663 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/FolderSuggestionNode.cs | 82 | already-enabled |
| UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs | 253 | already-enabled |
| UtilitiesCS/OutlookObjects/Folder/FolderTree.cs | 424 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/FolderTreeCompatibilityView.cs | 81 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/FolderTreeNodeKey.cs | 77 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/FolderTreeRefreshReason.cs | 17 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/FolderTreeRequest.cs | 51 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/FolderTreeSelectionOverlay.cs | 54 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshot.cs | 134 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotBuilder.cs | 80 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotChangedEventArgs.cs | 36 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotNode.cs | 86 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotQueries.cs | 183 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/FolderTreeStateModel.cs | 158 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/FolderWrapper .cs | 531 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameAndParentNameComparer.cs | 68 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameComparer.cs | 27 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameCountSizeComparer.cs | 34 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/FolderWrapperNodeComparer.cs | 70 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/FolderWrapperNodeContentsComparer.cs | 31 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/IDeadlineClock.cs | 13 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/IDispatcherYield.cs | 14 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/IFolderHandleResolver.cs | 13 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/IFolderHierarchyProvider.cs | 64 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/IFolderProbabilitySource.cs | 24 | already-enabled |
| UtilitiesCS/OutlookObjects/Folder/IFolderSearchHandler.cs | 39 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/IOutlookFolderHierarchyReader.cs | 20 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/IOutlookFolderNotificationSink.cs | 43 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/IOutlookFolderTreeService.cs | 22 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/MsgToMime/MAPIMethods.cs | 125 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/OutlookFolderHandleResolver.cs | 72 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs | 97 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyReader.cs | 277 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyRecord.cs | 45 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/OutlookFolderNotificationSink.cs | 498 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/OutlookFolderTreeService.cs | 302 | remediation-target |
| UtilitiesCS/OutlookObjects/Folder/PercentageFormatter.cs | 35 | already-enabled |
| UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs | 21 | remediation-target |
| UtilitiesCS/OutlookObjects/Store/DisabledStoreRow.cs | 26 | remediation-target |
| UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs | 179 | remediation-target |
| UtilitiesCS/OutlookObjects/Store/DisabledStoresViewer.cs | 51 | remediation-target |
| UtilitiesCS/OutlookObjects/Store/DisabledStoresViewer.Designer.cs | 135 | designer-non-opt-in |
| UtilitiesCS/OutlookObjects/Store/IDisabledStoresViewer.cs | 30 | remediation-target |
| UtilitiesCS/OutlookObjects/Store/IStoreWrapperViewer.cs | 29 | remediation-target |
| UtilitiesCS/OutlookObjects/Store/StoreDisableService.cs | 197 | remediation-target |
| UtilitiesCS/OutlookObjects/Store/StoreFilterAttribution.cs | 175 | remediation-target |
| UtilitiesCS/OutlookObjects/Store/StoreIdentity.cs | 107 | remediation-target |
| UtilitiesCS/OutlookObjects/Store/StoreLaunchReadinessEvaluator.cs | 40 | remediation-target |
| UtilitiesCS/OutlookObjects/Store/StoreLockupAttribution.cs | 37 | remediation-target |
| UtilitiesCS/OutlookObjects/Store/StoreRehookResult.cs | 100 | already-enabled |
| UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs | 443 | remediation-target |
| UtilitiesCS/OutlookObjects/Store/StoresWrapper.Filtering.cs | 108 | remediation-target |
| UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs | 232 | remediation-target |
| UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs | 477 | remediation-target |
| UtilitiesCS/OutlookObjects/Store/StoreWrapperInitClock.cs | 68 | remediation-target |
| UtilitiesCS/OutlookObjects/Store/StoreWrapperInitProbe.cs | 65 | remediation-target |
| UtilitiesCS/OutlookObjects/Store/StoreWrapperViewer.cs | 133 | remediation-target |
| UtilitiesCS/OutlookObjects/Store/StoreWrapperViewer.Designer.cs | 322 | designer-non-opt-in |
