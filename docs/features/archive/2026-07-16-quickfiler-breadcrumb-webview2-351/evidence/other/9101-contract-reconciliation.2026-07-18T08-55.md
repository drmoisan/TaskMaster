# 9101/#350 Contract Reconciliation (P2-T1)

Timestamp: 2026-07-18T08-55

Input: P0-T8 verdict `9101-CONTRACT: PRESENT` (`evidence/baseline/baseline-9101-contract-state.2026-07-18T08-41.md`).
Merged surface inspected in `UtilitiesCS/OutlookObjects/Folder/` (namespace `UtilitiesCS.OutlookObjects.Folder`),
merged into base commit 8e242692 by wave-0 feature #350 (PR #353).

## Actual member signatures (quoted from source)

`IFolderHierarchyProvider.cs`:
```csharp
public interface IFolderHierarchyProvider
{
    Task<IReadOnlyList<FolderBreadcrumbSegment>> GetAncestorChainAsync(
        FolderTreeNodeKey leafKey,
        CancellationToken cancellationToken
    );

    Task<IReadOnlyList<FolderBreadcrumbSegment>> GetImmediateSubfoldersAsync(
        FolderTreeNodeKey segmentKey,
        CancellationToken cancellationToken
    );

    Task<FolderTreeNodeKey> ResolveLeafKeyAsync(
        string folderPath,
        CancellationToken cancellationToken
    );
}
```

`FolderBreadcrumbSegment.cs` (sealed, immutable, net48-safe; explicit ctor, get-only properties):
```csharp
public FolderBreadcrumbSegment(
    FolderTreeNodeKey key,
    string displayName,
    string folderPath,
    bool hasChildren
)
public FolderTreeNodeKey Key { get; }
public string DisplayName { get; }
public string FolderPath { get; }
public bool HasChildren { get; }
```

`OutlookFolderHierarchyProvider.cs`: sealed production facade implementing `IFolderHierarchyProvider`
over the injectable `IOutlookFolderTreeService` snapshot seam. Ancestor chain via
`FolderTreeSnapshotQueries.GetAncestorChain` (root-first/leaf-last); `HasChildren` derived from
`FolderTreeSnapshotNode.ChildKeys.Count > 0`; `ResolveLeafKeyAsync` matches `FolderPath`
case-insensitively against the snapshot and returns null for unknown/blank paths. No COM usage in
the facade; unit tests already merged (`UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs`,
`FolderBreadcrumbSegmentTests.cs`).

## Decision

RECONCILIATION: DIRECT-CONSUME

The merged surface is not a raw reader extension over `IOutlookFolderHierarchyReader`/`FolderTreeSnapshotNode`;
it is already the narrow QuickFiler-facing provider the plan's seam tasks would otherwise create:
`IFolderHierarchyProvider` with `GetAncestorChainAsync`/`GetImmediateSubfoldersAsync` returning
JSON-serializable-member segments (`DisplayName`, `FolderPath`, has-children flag). The breadcrumb
consumes it directly. No adapter and no new seam files are created.

Deltas from the plan's assumed contract, absorbed by the consumer (Phase 3 router):
1. Query keying: the merged methods take `FolderTreeNodeKey`, not `string leafFolderPath`. The
   merged interface itself supplies the bridge: `ResolveLeafKeyAsync(string folderPath, ...)`.
   The Phase 3 router composes `ResolveLeafKeyAsync` + `GetAncestorChainAsync` for path-based
   entry (Path A suggestion rows) and uses segment `Key`s for expand routing.
2. DTO member name: `HasChildren` (merged) instead of `HasSubfolders` (assumed). Same semantics
   (drives the leaf-only affordance); consumer code uses `HasChildren`.
3. DTO carries an additional `Key` member (stable node identity), which the bridge serializes as
   an opaque row/segment identity token where needed.

## Task-consequence record (authorized skip branches)

- P2-T2: both named files (`IFolderHierarchyProvider.cs`, `FolderBreadcrumbSegment.cs`) already
  exist as the merged 9101 types and are used instead; zero new files created — satisfied-by-decision.
- P2-T3: no `FolderHierarchyProviderAdapter.cs` is created (adapter branch not taken) — satisfied-by-decision
  per the task's explicitly authorized skip branch.
- P2-T4: with zero new mapping code, the tests pin the merged 9101 contract shape the breadcrumb
  consumes (contract-shape pinning tests to be added in P2-T4).

The `ASSUMED-PENDING-9101-MERGE` marker is resolved: the assumed contract in spec.md §Upstream
Dependency and research §4.4 is superseded by the actual merged surface quoted above (AC-13).
