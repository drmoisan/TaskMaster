# Batch F1 Nullable Gate (P2-T3)

Timestamp: 2026-07-19T11-34

## Format / Build / Gate

- `dotnet tool run csharpier format .` — EXIT 0, clean.
- `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /m` — EXIT 0, 0 errors.
- Scoped gate `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false` — EXIT_CODE 1; **zero CS86xx** for the 10 Batch F1 files (AC1). Only the pre-existing 15 CS0618/CS0168 non-CS86xx warnings-as-errors remain (unchanged from baseline).

## Files remediated (10)

`FolderTreeNodeKey.cs`, `FolderTreeRequest.cs`, `FolderTreeSelectionOverlay.cs`, `FolderNodeViewModel.cs`,
`DeadlineClock.cs`, `FolderWrapperNameComparer.cs`, `FolderWrapperNameCountSizeComparer.cs`,
`FolderWrapperNameAndParentNameComparer.cs`, `FolderWrapperNodeComparer.cs`,
`FolderWrapperNodeContentsComparer.cs`.

## Key annotation decisions

- `FolderTreeNodeKey.Equals(object? obj)` and `Equals(FolderTreeNodeKey? other)` (IEquatable overrides).
- `FolderTreeRequest(IEnumerable<string>? storeIds, ...)` and `FolderTreeSelectionOverlay(IEnumerable<string>? ...)`
  / `IsSelected(FolderTreeSnapshotNode? node)` — default/guarded-null enumerables and node.
- All five comparers implement `IEqualityComparer<T>`, so `Equals(T? x, T? y)` params are nullable to match
  the BCL contract. `FolderWrapperNodeComparer` required three null-safety guard refinements (explicit
  `x is null`/`x.Value is null` guard, removal of redundant leading `x?.`/`y?.` on already-non-null x/y at the
  parent-comparison branch, and using the non-null `xChildren` local's `.Count` instead of re-dereferencing
  `x.Children.Count`) — behavior-identical, no `!` needed. No post-condition attributes; no record/init.
