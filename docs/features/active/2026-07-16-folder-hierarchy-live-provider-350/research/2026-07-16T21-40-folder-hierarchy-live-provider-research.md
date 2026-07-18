# Research — Live Outlook Folder-Hierarchy Provider (Issue #350, epic child 9101)

- Feature: `2026-07-16-folder-hierarchy-live-provider-350`
- Epic: `folder-tree-breadcrumb-redesign` (wave 0, complexity band C3, single shared upstream contract)
- Research timestamp: 2026-07-16T21-40
- Scope: introduce a live Outlook folder-hierarchy provider returning (a) the ordered root-to-leaf
  ancestor chain for a selected leaf folder and (b) on-demand real immediate subfolders of a segment.
- Out of scope (unchanged): scoring/ranking algorithm, feature-324 probability plumbing
  (`FolderScore.Probability` -> `FolderRow.Score` -> `PercentageFormatter.FormatPercent`), the
  WebView2 control replacements (owned by 9102 EfcViewer and 9103 QuickFiler).

All findings below are verified by direct reading of the cited source files in this worktree.

---

## 1. Current state: the two prefix-matching methods

### 1.1 `FolderSuggestionTree.BuildFromRows`
File: `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs` (253 lines).

- Signature: `public static FolderSuggestionTree BuildFromRows(IReadOnlyList<string> rows)`.
- Input: a sectioned `string[]` of already-presented rows (folder paths plus `"===="` banner headers)
  produced by `FolderPredictor`.
- Behavior: partitions rows into sections at banner rows; within a section, path `Y` is a child of
  presented path `X` when `Y.StartsWith(X + "\\")` and `X` is the longest such **presented** prefix
  (`FindLongestPrefixParent`, lines 217-241). Paths whose parent prefix is not among the presented
  rows become section roots. It performs **no ancestor synthesis** and **never queries Outlook**;
  hierarchy is derived purely from the ~5-plus-recents rows already on screen.
- Output: `FolderSuggestionTree` exposing `Roots`, `VisibleRows()`, and expand/collapse state
  transitions (`Expand`/`Collapse`/`Toggle`/`RightArrow`/`LeftArrow`) consumed by the EfcViewer
  `TreeListView`. Nodes are `FolderSuggestionNode` (kind Banner/Folder, `FullPath`, `DisplayName`,
  `Depth`, `IsExpanded`, `Probability`).

### 1.2 `FolderHierarchyBuilder.Build`
File: `UtilitiesCS/OutlookObjects/Folder/FolderHierarchyBuilder.cs` (107 lines).

- Signature: `public IReadOnlyList<TreeNode<FolderNodeViewModel>> Build(IReadOnlyList<FolderRow> rows)`.
- Input: the ordered `FolderRow[]` from `FolderPredictor.FolderRowArray`.
- Behavior: for each `FolderRowKind.Suggestion` row (non-null `Score`), splits `FolderScore.FolderPath`
  on `\` and walks/inserts with find-or-add ancestor synthesis (`AddSuggestion`, lines 62-105),
  attaching `FolderScore.Probability` only at the full-folder leaf; synthesized ancestors carry no
  probability but are expandable. Every non-suggestion row (Separator/SearchResult/Recent) becomes a
  depth-0 leaf preserving `Text` verbatim. It splits **only the <=5 suggestion paths**; it does
  **not** query real Outlook subfolders.
- Output: forest of `TreeNode<FolderNodeViewModel>`; `FolderNodeViewModel` carries `FolderPath`,
  `DisplayName`, `Probability`, `Depth`, `HasChildren`, mutable `Expanded`, derived `Glyph` and
  `FormattedPercentage`.

### 1.3 Every current caller (whole-repo grep)

Production callers (exactly two):

| Caller | Site | Method | Consumer control |
|---|---|---|---|
| `QuickFiler/Controllers/EfcFormController.cs` | `BindFolderRows`, line 885 | `FolderSuggestionTree.BuildFromRows(_folderRows)` | EfcViewer `BrightIdeasSoftware.TreeListView` (`FolderListBox`) |
| `QuickFiler/Viewers/ItemViewer.FolderSearch.cs` | `SetFolderSuggestions`, line 26 | `new FolderHierarchyBuilder().Build(rows)` | QuickFiler `CboFolders` owner-draw `ComboBox` |

In `EfcFormController.BindFolderRows`, the tree from `BuildFromRows` is passed through
`new FolderProbabilityAdapter(source).Apply(tree)` (line 890) to join 324 probabilities, then
`tlv.SetObjects(tree.Roots)`. In `ItemViewer.FolderSearch`, the forest feeds a `FolderTreeStateModel`
that projects into `CboFolders.Items` (owner-draw). Both callers are the exact UI surfaces that
9102/9103 replace with WebView2.

Test callers (part of the spec, must stay green or be migrated with their consumer):
`UtilitiesCS.Test/OutlookObjects/Folder/FolderSuggestionTreeHierarchyTests.cs`,
`FolderSuggestionTreeStateTests.cs`, `FolderProbabilityAdapterTests.cs`,
`FolderHierarchyBuilderTests.cs`.

### 1.4 What breaks if the prefix-matching behavior is replaced
Deleting or rewiring `BuildFromRows`/`Build` in this feature would break the two live UI callers,
whose replacement controls (TreeListView -> WebView2, ComboBox -> WebView2) are owned by 9102/9103.
See the scope decision in section 7.

---

## 2. Existing-seam reconciliation (primary deliverable)

The repository **already contains a live Outlook folder-hierarchy read path** that isolates COM
behind an interface, is cached and notification-refreshed, and is wired into production globals. The
new provider should **reuse and thinly extend** this infrastructure rather than introduce a second
live-COM seam.

### 2.1 The existing live-query stack (all in `UtilitiesCS/OutlookObjects/Folder/`)

- `IOutlookFolderHierarchyReader` — `Task<IReadOnlyList<FolderTreeSnapshotNode>> ReadFoldersAsync(...)`.
  The live-COM boundary. Its production impl `OutlookFolderHierarchyReader` recursively enumerates
  **`MAPIFolder.Folders`** (the exact interop call the issue names): see the internal
  `OutlookFolderAdapter.Children` => `_folder.Folders.Cast<Outlook.MAPIFolder>()...` (lines 270-274)
  and the stack-based descent in `ReadStoreAsync` (lines 118-163), rooted at `Store.GetRootFolder()`.
  Every COM-touching member and adapter is `[ExcludeFromCodeCoverage]`; COM is wrapped behind the
  internal `IOutlookStoreAdapter`/`IOutlookFolderAdapter` interfaces (lines 216-229), and unit tests
  inject `FakeOutlookFolderHierarchyReader`.
- `FolderTreeSnapshotBuilder.BuildSnapshotAsync` — turns reader output into an immutable
  `FolderTreeSnapshot`, computing roots (`ParentKey == null`) and pre-order ordering.
- `IOutlookFolderTreeService` / `OutlookFolderTreeService` — session-scoped **cache** of the snapshot
  with a state machine (Empty/Building/Current/StaleCurrent/Refreshing/Disposed), notification-driven
  invalidation via `IOutlookFolderNotificationSink` (folder add/remove/change, store add/remove), and
  `Task<FolderTreeSnapshot> GetSnapshotAsync(FolderTreeRequest, CancellationToken)`.
- `FolderTreeSnapshot` — immutable, already exposes the exact two queries this feature needs:
  - **real immediate subfolders**: `GetChildren(FolderTreeNodeKey parentKey)` (lines 119-132) returns
    the child snapshot nodes; empty (never null) for unknown keys.
  - parent edges for the ancestor walk: each `FolderTreeSnapshotNode` carries `ParentKey` and
    `ChildKeys`; plus `TryGetNode`, `FindByPath(storeId, folderPath)`, `GetNodesForStore`, `NodesByKey`.
- `FolderTreeSnapshotNode` — identity `Key` (`FolderTreeNodeKey` = StoreId + EntryId + FolderPath),
  `DisplayName`, `FolderPath`, `RelativePath`, `ParentKey`, `ChildKeys`, `IsStale`.
- `IFolderHandleResolver` / `OutlookFolderHandleResolver` — resolves a live `MAPIFolder` handle from a
  snapshot node via `NameSpace.GetFolderFromID(entryId, storeId)` (COM boundary, exempt). Needed only
  if a consumer must act on a live folder object; not needed for rendering the breadcrumb.
- `FolderTreeSnapshotQueries` — existing static, host-neutral query helpers over a snapshot
  (`GetSelectedNodes`, `GetArchiveRoot`, `EnumerateRelativePaths`, `CreateSubtreeSnapshot`, ...).

### 2.2 Production wiring already exists
`IOlObjects.FolderTreeService` (interface `UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs:21`) exposes
`IOutlookFolderTreeService`. It is constructed/consumed in `TaskMaster/AppGlobals/AppOlObjects.cs`,
`TaskMaster/AppGlobals/StoreRehookCoordinator.cs`, `TaskMaster/Ribbon/RibbonController.FolderTree.cs`.
The live snapshot is therefore already available to both UI consumers via globals.

### 2.3 What is already provided vs the one genuine gap

| Requirement | Already provided? | Where |
|---|---|---|
| Live enumeration of `MAPIFolder.Folders` behind an injectable seam | Yes | `IOutlookFolderHierarchyReader` (COM-exempt impl) |
| Cached/refreshed live hierarchy | Yes | `IOutlookFolderTreeService` |
| Real immediate subfolders of a segment | Yes | `FolderTreeSnapshot.GetChildren(key)` |
| Parent edges (raw material for ancestor chain) | Yes | `FolderTreeSnapshotNode.ParentKey` + `TryGetNode` |
| Resolve a UI-selected folder path -> node | Yes | `FolderTreeSnapshot.FindByPath(storeId, folderPath)` |
| **Ordered root-to-leaf ancestor chain helper** | **No** | not present in `FolderTreeSnapshotQueries` |
| **A single shared provider contract the two UI features depend on** | **No** | must be introduced |

### 2.4 Reconciliation verdict: REUSE, do not add a new live-COM seam
Introduce **one new host-neutral contract** (`IFolderHierarchyProvider`) implemented as a thin facade
over `IOutlookFolderTreeService` plus a new **pure** ancestor-chain helper on
`FolderTreeSnapshotQueries`. Reuse `GetChildren` for subfolders verbatim.

Decisive reasons:
1. The live `MAPIFolder.Folders` query is **already** isolated behind `IOutlookFolderHierarchyReader`
   and already returns real immediate children through the snapshot. A second COM seam would duplicate
   the interop boundary, the notification/refresh state machine, and the test-fake surface — violating
   "simplicity first" and "reusability" (CLAUDE.md General Code Change Policy §1).
2. The snapshot already carries `ParentKey`/`ChildKeys` and stable `FolderTreeNodeKey` identity, so the
   ancestor walk and subfolder projection are **pure, synchronous, fully unit-testable** functions over
   an in-memory snapshot — no new COM-bound code, hence **no new coverage exemption** is requested.
3. The snapshot is already wired to production via `IOlObjects.FolderTreeService`, so both UI features
   consume the provider through existing globals with no new plumbing.

Do NOT extend `IOutlookFolderHierarchyReader` with a per-expand `ReadImmediateChildrenAsync`: the
snapshot is refreshed by the notification sink, so `GetChildren` already reflects the current live
hierarchy without an extra COM round-trip per expand. A lazy per-expand COM read is a possible future
optimization only and would reintroduce interop into the hot path.

---

## 3. Outlook interop reality (verified)

- **Real immediate subfolders** are obtained by enumerating `MAPIFolder.Folders`:
  `OutlookFolderHierarchyReader.OutlookFolderAdapter.Children` (`_folder.Folders.Cast<Outlook.MAPIFolder>()`,
  lines 270-274), descended recursively in `ReadStoreAsync` (stack-based, lines 118-163), rooted at
  `IOutlookStoreAdapter.GetRootFolder()` -> `Store.GetRootFolder()`.
- **Ancestor/parent chains**: the reader records `ParentEntryId` per node
  (`OutlookFolderHierarchyRecord`), and the snapshot materializes `ParentKey`/`ChildKeys`. Direct COM
  parent-walking exists in legacy path code — `FolderMinimalWrapper.RestoreFromRelativePath` walks
  `.Parent` (lines 108-121) and `FolderNavigator.GetOutlookFolder` walks `.Folders[name]` down from the
  store root — but neither backs the snapshot and neither should be used by the new provider.
- **Handles / identity**: `FolderTreeNodeKey` = (StoreId, EntryID, FolderPath), case-insensitive on
  store/path and ordinal on entryId. A live `MAPIFolder` is re-resolved from a node via
  `OutlookFolderHandleResolver` -> `NameSpace.GetFolderFromID(entryId, storeId)`.
- **COM wrapping for testability**: COM objects are wrapped by internal adapter interfaces inside the
  reader/resolver; every COM-touching member is `[ExcludeFromCodeCoverage]`; tests use
  `FakeOutlookFolderHierarchyReader` and `FakeFolderHandleResolver`
  (`UtilitiesCS.Test/OutlookObjects/Folder/Fakes/`).

---

## 4. Proposed public contract

Two layers, keeping pure logic separate from the (interface-only) live-Outlook seam.

### 4.1 New pure helper on `FolderTreeSnapshotQueries` (host-neutral, no COM)
```csharp
// Ordered root-to-leaf ancestor chain for a leaf node identified by key.
public static IReadOnlyList<FolderTreeSnapshotNode> GetAncestorChain(
    FolderTreeSnapshot snapshot, FolderTreeNodeKey leafKey);
```
- Walk `TryGetNode(leafKey)` -> follow `ParentKey` to the store root, collecting nodes, then reverse to
  root-first order.
- `snapshot` null -> `ArgumentNullException` (matches existing methods in this class).
- `leafKey` null, or not present in the snapshot -> empty list (stale/removed folder), never null.
- Invariants: result is ordered root-first / leaf-last; last element equals the requested leaf;
  every adjacent `(parent, child)` satisfies `child.ParentKey.Equals(parent.Key)`; when the leaf is a
  store root the chain has exactly one element. A defensive visited-set guards against a malformed
  cyclic `ParentKey` (returns the partial chain rather than looping).

Immediate subfolders reuse `FolderTreeSnapshot.GetChildren(key)` verbatim — no new helper.

### 4.2 New shared provider contract (the epic's single upstream contract)
```csharp
public interface IFolderHierarchyProvider
{
    // (a) ordered root-to-leaf ancestor chain for the selected leaf folder.
    Task<IReadOnlyList<FolderBreadcrumbSegment>> GetAncestorChainAsync(
        FolderTreeNodeKey leafKey, CancellationToken cancellationToken);

    // (b) on-demand real immediate subfolders of a given segment (live via the cached snapshot).
    Task<IReadOnlyList<FolderBreadcrumbSegment>> GetImmediateSubfoldersAsync(
        FolderTreeNodeKey segmentKey, CancellationToken cancellationToken);

    // convenience: resolve a UI-selected folder path to a key against the current snapshot.
    Task<FolderTreeNodeKey> ResolveLeafKeyAsync(
        string folderPath, CancellationToken cancellationToken);
}
```
Implementation `OutlookFolderHierarchyProvider : IFolderHierarchyProvider` (host-neutral; depends only
on `IOutlookFolderTreeService`, an interface — NOT COM, NOT coverage-exempt):
- Acquires the snapshot via `_treeService.GetSnapshotAsync(FolderTreeRequest.AllStores(allowStaleSnapshot:true), ct)`.
- `GetAncestorChainAsync`: `FolderTreeSnapshotQueries.GetAncestorChain(snapshot, leafKey)` then maps
  each node to a `FolderBreadcrumbSegment`.
- `GetImmediateSubfoldersAsync`: `snapshot.GetChildren(segmentKey)` then maps.
- `ResolveLeafKeyAsync`: `snapshot.FindByPath(...)`; because the UI often knows only a path, match by
  `FolderPath` across `NodesByKey` (case-insensitive) and return the node `Key`, or `null` when absent.
  (Real Outlook full paths embed the store name, so they are unique in practice; the
  duplicate-path-across-stores fixture is a synthetic test case — document first-match and prefer the
  key-based overloads when the caller already holds store identity.)
- Async only because snapshot acquisition is async; the ancestor walk and children projection
  themselves are synchronous and independently unit-tested via the pure helper.

### 4.3 New segment DTO (host-neutral, immutable, net48-safe plain class)
```csharp
public sealed class FolderBreadcrumbSegment
{
    public FolderTreeNodeKey Key { get; }      // stable identity for the expand-this-segment call
    public string DisplayName { get; }         // folder name (leaf path segment)
    public string FolderPath { get; }          // full path; the selection value returned to the host
    public bool HasChildren { get; }           // node.ChildKeys.Count > 0 -> render the expand affordance
}
```
Deliberately probability-free: the percentage is joined by each UI feature from the existing 324
plumbing (`IFolderProbabilitySource` / `FolderScore.Probability` keyed by `FolderPath`), exactly as
`FolderProbabilityAdapter` / `EfcFormController.BuildProbabilitySource` do today. Keeping probability
out of the segment preserves the "no change to scoring/probability plumbing" boundary.

### 4.4 How 9102 and 9103 call it
Both obtain `IFolderHierarchyProvider` from globals (backed by `IOlObjects.FolderTreeService`):
1. On a selected/predicted suggestion, `ResolveLeafKeyAsync(selectedFolderPath)` -> `leafKey`.
2. `GetAncestorChainAsync(leafKey)` -> ordered segments; render `Seg -> Seg -> ... -> Leaf`; the leaf
   shows the expand affordance only when its segment `HasChildren`.
3. On a segment expand (double-click / arrow), route `GetImmediateSubfoldersAsync(segment.Key)` across
   the JS<->.NET bridge and render the real immediate subfolders.
4. Join the percentage per `FolderPath` from the existing probability source (unchanged).

---

## 5. Testability plan

- **Pure, unit-tested (Moq/FluentAssertions/MSTest), no exemption:**
  - `FolderTreeSnapshotQueries.GetAncestorChain` over hand-built `FolderTreeSnapshot` fixtures:
    single-level, multi-level, root-only (leaf == root), null/unknown leaf, defensive cycle guard,
    duplicate segment names at different depths (identity by `FolderTreeNodeKey`, not display name).
  - `FolderBreadcrumbSegment` construction/mapping (including `HasChildren` from `ChildKeys`).
  - `OutlookFolderHierarchyProvider` with a fake/Moq `IOutlookFolderTreeService` returning a
    prebuilt snapshot: ancestor chain, immediate subfolders (empty set, unknown key), path resolution
    (found, not found, duplicate-path first-match), cancellation propagation.
- **Thin COM-bound seam:** none is added by this feature. The only COM code
  (`OutlookFolderHierarchyReader`, `OutlookFolderHandleResolver`) is pre-existing and already
  `[ExcludeFromCodeCoverage]`; this feature does not touch it.
- **Snapshot fixtures** may be built directly via the `FolderTreeSnapshot(rootKeys, nodes)` constructor
  or via `FolderTreeSnapshotBuilder` + `FakeOutlookFolderHierarchyReader` (existing pattern).
- **Coverage denominator:** all three new files are in the testable denominator; the interface file has
  no executable lines (type-only). No test touches a live Outlook process or COM interop. New code
  targets >= 90% and the repository floor (>= 80% testable denominator per CLAUDE.md General/C# Unit
  Test Policy) is not reduced; the feature adds zero exempt lines, so it contributes positively.

  Note: the embedded CLAUDE.md policy (authority #1 per Policy Compliance Order) specifies 80% floor /
  90% new-code. The `.claude/rules/*.md` summaries state 85/75; where they differ, CLAUDE.md governs.
  The plan meets the stricter-applicable interpretation because the new code is fully testable.

---

## 6. File / line budget (500-line cap respected)

New production files (`UtilitiesCS/OutlookObjects/Folder/`):
- `IFolderHierarchyProvider.cs` — ~35 lines (interface + XML docs). Type-only, no executable lines.
- `FolderBreadcrumbSegment.cs` — ~45 lines (immutable DTO + XML docs).
- `OutlookFolderHierarchyProvider.cs` — ~90-130 lines (facade over `IOutlookFolderTreeService`;
  host-neutral; not exempt).

Modified production file:
- `FolderTreeSnapshotQueries.cs` — currently 130 lines; add `GetAncestorChain` (~25-35 lines) ->
  well under 500.

Not modified by this feature (deferred to consumers — see section 7):
- `FolderSuggestionTree.cs`, `FolderHierarchyBuilder.cs`, `QuickFiler/Controllers/EfcFormController.cs`,
  `QuickFiler/Viewers/ItemViewer.FolderSearch.cs`.

New test files (`UtilitiesCS.Test/OutlookObjects/Folder/`, mirrored layout):
- `FolderTreeSnapshotQueriesAncestorChainTests.cs`
- `OutlookFolderHierarchyProviderTests.cs`
- `FolderBreadcrumbSegmentTests.cs`
Register new production files in `UtilitiesCS.csproj` and test files in `UtilitiesCS.Test.csproj`
(both use explicit `<Compile Include>` item lists).

---

## 7. Scope decision — replacing vs deferring the prefix-matching methods

The issue/epic ACs say the provider "replaces the prefix-matching logic in
`FolderSuggestionTree.BuildFromRows` and `FolderHierarchyBuilder.Build`." Those two methods are
consumed by the two live UI callers (section 1.3), whose control replacements (TreeListView -> WebView2,
ComboBox -> WebView2) are owned by **9102/9103**, not 9101. 9101 has `depends_on: []` and must remain
independently mergeable in wave 0.

Recommended interpretation (keeps 9101 independently mergeable and the build green):
- 9101 **adds** the provider contract, implementation, pure ancestor helper, and tests. It **does not
  delete** `BuildFromRows`/`Build` and **does not rewire** `EfcFormController`/`ItemViewer.FolderSearch`.
- Mark the two legacy methods as superseded in XML docs (pointing to `IFolderHierarchyProvider`).
- The prefix-matching methods and their tests are removed by each UI feature (9102 EfcViewer, 9103
  QuickFiler) as it migrates its control to the provider and drops the last caller. "Replaced" is an
  epic-level outcome realized across waves.

Rejected alternative: rewire both UI callers inside 9101. Rejected because it pulls WebView2/UI work
(wave 1) into wave 0, breaks the dependency DAG and the independent-merge property, and would leave the
existing TreeListView/ComboBox controls consuming a provider whose output shape is designed for the
breadcrumb, forcing throwaway adapter code.

The atomic planner should confirm this scope split with the orchestrator, since it narrows the literal
"replaced ... is replaced" AC to "superseded; deletion deferred to the consuming UI features."

---

## 8. Rejected alternatives (brief)

- **New standalone live-COM seam** (e.g., `IFolderSubfolderReader` calling `MAPIFolder.Folders` per
  expand): rejected — duplicates the existing `IOutlookFolderHierarchyReader` COM boundary, the
  notification/refresh state machine, and the fake surface; adds new exempt COM lines; contradicts the
  reuse verdict (section 2.4).
- **Extend `IOutlookFolderHierarchyReader` with a per-key lazy children read**: rejected as
  unnecessary — the snapshot is already notification-refreshed and `GetChildren` already returns the
  live immediate subfolders; a per-expand COM call adds latency and interop to the hot path with no
  correctness benefit. Retained only as a possible future optimization.
- **Path-only string contract** (no `FolderTreeNodeKey`): rejected as the primary shape — loses stable
  identity, mishandles duplicate segment names at different depths, and cannot address the
  duplicate-path-across-stores edge; retained only as the `ResolveLeafKeyAsync` convenience entry.

---

## 9. Recommended test strategy (no test code authored here)

MSTest + Moq + FluentAssertions, mirrored under `UtilitiesCS.Test/OutlookObjects/Folder/`.
Cover, per the seeded conditions: ancestor chain for single-level / multi-level / root-only;
segment-children via mocked `IOutlookFolderTreeService`; negatives (null/unknown leaf, unresolved path,
empty subfolder set); edges (leaf == root, duplicate segment names at different depths); isolation (no
live Outlook, no COM, no temp files, deterministic — no wall-clock/RNG needed). Assert the ordering and
adjacency invariants of section 4.1 explicitly.
