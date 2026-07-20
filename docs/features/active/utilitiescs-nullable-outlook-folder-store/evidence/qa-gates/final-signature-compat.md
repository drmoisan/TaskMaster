# Final AC5 — Public Signature Compatibility (P12-T10)

Timestamp: 2026-07-19T16-40
Command: `git diff dffadd5a..HEAD -- UtilitiesCS/OutlookObjects/Folder/*.cs UtilitiesCS/OutlookObjects/Store/*.cs`
(63 files changed, 378 insertions / 297 deletions).

## Finding
Every public/internal signature change is limited to **additive nullability annotations** (`?`) plus justified
null-forgiving operators (`!`); no signature became behavior-incompatible. An existing caller that compiled
before still compiles and behaves identically — a nullable annotation is additive contract metadata, not a
source- or binary-breaking change.

Representative signature changes (all annotation-only, reflecting actual null behavior — AC5):
- `FolderNavigator.GetOutlookFolder` -> `Folder?`; `FolderConverter.ToFsFolderpath(...,IApplicationGlobals)` ->
  `string?`; `FolderPredictor` GetFolder overloads -> `Folder?`, CreateFolder -> `MAPIFolder?`,
  CreateFolderAsync -> `Task<object?>`.
- `FolderTreeSnapshot.TryGetNode(FolderTreeNodeKey? key, out FolderTreeSnapshotNode? node)`; `FindByPath` ->
  `FolderTreeSnapshotNode?`; `IFolderHierarchyProvider.ResolveLeafKeyAsync` -> `Task<FolderTreeNodeKey?>`.
- `StoreDisableService(..., IStoreRehookService? rehook = null)`; `StoreIdentity.Resolve(string? displayName,
  string? filePathFallback = null)`; StoreWrapper Init/Restore-populated properties -> nullable.
- Interface contracts widened consistently with their implementations (IFolderHandleResolver.TryResolve,
  IOutlookFolderHierarchyReader.ReadFoldersAsync clocks, IOutlookStoreAdapter.GetRootFolder).

## Behavior-identical non-signature refinements (not behavior changes)
- FolderWrapperNodeComparer: explicit `x is null || x.Value is null || ...` guard and `xChildren.Count` (the
  non-null local equals `x.Children.Count`) — same results as the prior `x?.Value is null` form.
- FolderTreeCompatibilityView.Roots: added `.Select(node => node!)` after the existing `.Where(node => node !=
  null)` — filters the same already-non-null nodes.
- Nullable-string Lazy fields use `new Lazy<string?>(() => value)` where #363's `ToLazy<T>() where T : class`
  rejects `string?` — behavior-equivalent lazy-returning-value.
- `null!` partial-init on navigation-only ctors / lifecycle-set fields documents pre-existing "set-before-use"
  contracts without changing runtime behavior.

No public API was removed, renamed, or had a parameter added/removed. AC5 satisfied.
