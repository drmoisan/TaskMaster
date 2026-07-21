# Batch F2 Nullable Gate (P3-T3)

Timestamp: 2026-07-19T11-50

## Format / Build / Gate
- `dotnet tool run csharpier format .` — EXIT 0, clean.
- `msbuild TaskMaster.sln /t:Build ... /m` — EXIT 0, 0 errors (test project still compiles after the
  IOutlookFolderHierarchyReader clock-nullability contract change).
- Scoped gate `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false` — EXIT 1; **zero CS86xx** for the 7 Batch F2 files (AC1). Only the pre-existing 15 CS0618/CS0168 warnings-as-errors remain.

## Files remediated (7)
FolderTreeSnapshotNode.cs, FolderTreeSnapshot.cs, FolderTreeSnapshotQueries.cs, FolderTreeSnapshotBuilder.cs,
FolderTreeCompatibilityView.cs, FolderTreeStateModel.cs, FolderHierarchyBuilder.cs.

## Key annotation decisions
- FolderTreeSnapshotNode: `parentKey`/`ParentKey` nullable (root has no parent); `childKeys` nullable (guarded).
- FolderTreeSnapshot: 3-arg ctor `request` nullable (delegating ctor passes null); `TryGetNode(FolderTreeNodeKey? key,
  out FolderTreeSnapshotNode? node)`; `FindByPath` nullable return; `GetNodesForStore(string? storeId)` with a
  justified `storeId!` at TryGetValue (net481 IsNullOrWhiteSpace lacks [NotNullWhen] to narrow); `Covers` request nullable.
  GetChildren guarded with `|| parent is null` (no [NotNullWhen] available on TryGetNode).
- FolderTreeSnapshotQueries: `GetArchiveRoot` nullable return; `EnumerateRelativePaths(string? storeId)`;
  `GetAncestorChain(FolderTreeNodeKey? leafKey)`; CreateSubtreeSnapshot guarded `|| node is null`. GetCompareInputs
  return kept `Tuple<FolderTreeSnapshotNode, FolderTreeSnapshotNode>` (net481 FirstOrDefault returns non-null T here).
- FolderTreeCompatibilityView: `CreateNode` nullable return; Roots filter uses `.Where(!=null).Select(n => n!)`;
  snapshotNode guarded `|| snapshotNode is null`.
- FolderTreeStateModel: `_highlighted` / `Highlighted` nullable. FolderHierarchyBuilder: `currentNode`/`cumulative`
  locals nullable; `Build(IReadOnlyList<FolderRow>? rows)`.
- Cross-batch: IOutlookFolderHierarchyReader.ReadFoldersAsync clocks made `IDeadlineClock?`/`IDispatcherYield?`
  because FolderTreeSnapshotBuilder passes its null-tolerant fields (reader YieldIfNeededAsync guards null). F5's
  OutlookFolderHierarchyReader must match this shape.
- No post-condition attributes; no record/init; TreeNode<T> not edited.
