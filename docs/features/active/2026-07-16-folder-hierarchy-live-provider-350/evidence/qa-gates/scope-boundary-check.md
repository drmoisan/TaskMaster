# Wave-0 Mergeability Boundary Check (P4-T6)

Timestamp: 2026-07-18T00-38

## Legacy methods NOT deleted

- `FolderSuggestionTree.BuildFromRows` present:
  `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs:39` —
  `public static FolderSuggestionTree BuildFromRows(IReadOnlyList<string> rows)`
- `FolderHierarchyBuilder.Build` present:
  `UtilitiesCS/OutlookObjects/Folder/FolderHierarchyBuilder.cs:26` —
  `public IReadOnlyList<TreeNode<FolderNodeViewModel>> Build(IReadOnlyList<FolderRow> rows)`

## UI callers NOT rewired

- `EfcFormController.BindFolderRows` still referenced at
  `QuickFiler/Controllers/EfcFormController.cs:581,769`; the file is not in the git diff.
- `ItemViewer.SetFolderSuggestions` still defined at
  `QuickFiler/Viewers/ItemViewer.FolderSearch.cs:24`; the file is not in the git diff.

## Feature-324 probability plumbing untouched

No changes to any scoring/ranking or `FolderScore.Probability` -> `FolderRow.Score` ->
`PercentageFormatter.FormatPercent` file. None appear in the git diff.

## Git diff scope (only Scope-Lock files)

Modified (tracked):
- `UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotQueries.cs` (Scope Lock: MODIFY production — `GetAncestorChain` added)
- `UtilitiesCS/UtilitiesCS.csproj` (Scope Lock: MODIFY production — 3 Compile Include items)
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (Scope Lock: MODIFY test — 3 Compile Include items)
- `docs/features/active/2026-07-16-folder-hierarchy-live-provider-350/plan.2026-07-16T21-52.md` (plan checklist)

Untracked (new, all Scope-Lock ADD):
- `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbSegment.cs`
- `UtilitiesCS/OutlookObjects/Folder/IFolderHierarchyProvider.cs`
- `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs`
- `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbSegmentTests.cs`
- `UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeSnapshotQueriesAncestorChainTests.cs`
- `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs`
- `docs/features/active/2026-07-16-folder-hierarchy-live-provider-350/evidence/` (evidence artifacts)

Result: PASS. Neither legacy method was deleted, neither UI caller was rewired, the feature-324 probability plumbing is untouched, and the git diff of the branch shows only Scope-Lock files plus permitted plan/evidence updates. No MUST-NOT-TOUCH file was modified.
