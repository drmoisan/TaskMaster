# Final QC — Non-Interference with 9004 (P6-T6)

Timestamp: 2026-07-16T11-45
Command: git diff --name-only + git ls-files --others --exclude-standard (tracked edits + new files)
EXIT_CODE: 0

## Changed/new source file set (#325)

Production (.cs):
- QuickFiler/Controllers/KeyboardHandler.cs
- QuickFiler/Controllers/QfcItemController.FolderHandling.cs
- QuickFiler/Viewers/IItemViewer.cs
- QuickFiler/Viewers/ItemViewer.Designer.cs
- QuickFiler/Viewers/ItemViewer.FolderSearch.cs
- UtilitiesCS/OutlookObjects/Folder/FolderHierarchyBuilder.cs (new)
- UtilitiesCS/OutlookObjects/Folder/FolderNodeViewModel.cs (new)
- UtilitiesCS/OutlookObjects/Folder/FolderTreeStateModel.cs (new)
- UtilitiesCS/OutlookObjects/Folder/IFolderSearchHandler.cs
- UtilitiesCS/OutlookObjects/Folder/PercentageFormatter.cs (new)

Tests (.cs):
- QuickFiler.Test/Controllers/QfcItemController.FolderSuggestionsTests.cs (new)
- UtilitiesCS.Test/OutlookObjects/Folder/PercentageFormatterTests.cs (new)
- UtilitiesCS.Test/OutlookObjects/Folder/FolderNodeViewModelTests.cs (new)
- UtilitiesCS.Test/OutlookObjects/Folder/FolderHierarchyBuilderTests.cs (new)
- UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeStateModelTests.cs (new)

Project wiring (.csproj): UtilitiesCS.csproj, UtilitiesCS.Test.csproj, QuickFiler.Test.csproj
(explicit <Compile Include> entries for the new files).

## 9004 non-interference: PASS

Zero overlap with the 9004 body-render files. None of the following appears in the diff:
- UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Html.cs — NOT modified
- QuickFiler/Viewers/ItemViewer.WebViewThread.cs — NOT modified
- QuickFiler/Viewers/WebView2CoreInitializer.cs — NOT modified
- QuickFiler/Viewers/IWebViewCoreInitializer.cs — NOT modified

No WebView2 / NavigateToString member was altered: the IItemViewer.cs edit only adds the
`SetFolderSuggestions(IReadOnlyList<FolderRow>)` intent member (the NavigateToString and
WebViewInitializationCompleted members are untouched); the ItemViewer.Designer.cs edit is confined to
the `CboFolders` block (DrawMode.OwnerDrawFixed + DrawItem/MouseDown event wiring) and touches no
WebView2 initialization member.

## Dead viewer variants: PASS

None of the nine dead design-time [ExcludeFromCodeCoverage] variants is modified (Form1,
ItemViewerExpanded, QfcItemViewer, QFCItemViewerDarkNew, QfcItemViewerExpanded,
QfcItemViewerExpandedLight, QFCItemViewerLightNew, QfcItemViewerLightSelected, QfcItemViewerV1). Only
the runtime-live `ItemViewer` (and its Designer partial) is changed.

Result: the #325 file set is disjoint from the 9004 body-render path and the dead viewer variants.
