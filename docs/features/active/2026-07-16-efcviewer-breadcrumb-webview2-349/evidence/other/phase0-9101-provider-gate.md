# Phase 0 — 9101 Provider Dependency Gate (P0-T6)

Timestamp: 2026-07-18T08-55
Gate result: PASS — both provider types are present in `UtilitiesCS`; execution proceeds past Phase 0.

Search commands used:
- `grep -r "IFolderHierarchyProvider" UtilitiesCS/` (ripgrep files-with-matches)
- `grep -r "FolderBreadcrumbSegment|FolderSegmentInfo" UtilitiesCS/` (ripgrep files-with-matches)
- `grep -rn "class FolderTreeNodeKey|struct FolderTreeNodeKey" UtilitiesCS/`

Resolved actual merged surface (feature #350 / PR #353), namespace `UtilitiesCS.OutlookObjects.Folder`:

1. `UtilitiesCS/OutlookObjects/Folder/IFolderHierarchyProvider.cs` — `public interface IFolderHierarchyProvider`
   - `Task<IReadOnlyList<FolderBreadcrumbSegment>> GetAncestorChainAsync(FolderTreeNodeKey leafKey, CancellationToken cancellationToken)` — root-first/leaf-last chain; empty list (never null) when leafKey null/absent.
   - `Task<IReadOnlyList<FolderBreadcrumbSegment>> GetImmediateSubfoldersAsync(FolderTreeNodeKey segmentKey, CancellationToken cancellationToken)` — real immediate children from the live cached snapshot; empty list (never null) on unknown/childless.
   - `Task<FolderTreeNodeKey> ResolveLeafKeyAsync(string folderPath, CancellationToken cancellationToken)` — resolves a UI-selected full folder path to a stable node key; null when no match.
   - Concrete implementation: `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs`.

2. `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbSegment.cs` — `public sealed class FolderBreadcrumbSegment` (plain sealed class, net48-safe, get-only properties, explicit ctor)
   - ctor `(FolderTreeNodeKey key, string displayName, string folderPath, bool hasChildren)`; throws `ArgumentNullException` on null key; null displayName/folderPath coerced to `string.Empty`.
   - Members: `FolderTreeNodeKey Key`, `string DisplayName`, `string FolderPath`, `bool HasChildren`.

3. Supporting identity type: `UtilitiesCS/OutlookObjects/Folder/FolderTreeNodeKey.cs` — `public sealed class FolderTreeNodeKey : IEquatable<FolderTreeNodeKey>`
   - ctor `(string storeId, string entryId, string folderPath)`; `StoreId`/`FolderPath` require non-whitespace text (ArgumentException), `EntryId` null-coerced to empty; value equality (StoreId + EntryId + FolderPath, case-insensitive on store/path).

Deviations from the research §C.3 assumed shape (Phases 2 and 5 MUST code against the actual surface below; re-alignment point is the row-builder/router input as pre-authorized by the plan):
- DTO name is `FolderBreadcrumbSegment`, NOT `FolderSegmentInfo`.
- Full-path member is `FolderPath`, NOT `FullPath`.
- Children flag is `HasChildren`, NOT `HasSubfolders`.
- `GetAncestorChainAsync` / `GetImmediateSubfoldersAsync` take a `FolderTreeNodeKey`, NOT a `string`; the string-path bridge is `ResolveLeafKeyAsync(string, CancellationToken)`.
- Segments carry a `Key` used to route expand-this-segment calls.
