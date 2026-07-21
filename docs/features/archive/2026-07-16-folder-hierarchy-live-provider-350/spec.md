# 2026-07-16-folder-hierarchy-live-provider — Spec

- **Issue:** #350
- **Parent (optional):** epic `folder-tree-breadcrumb-redesign` (manifest issue 9101, wave 0, complexity band C3)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-16T21-52
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** full-feature

## Overview

The `folder-tree-breadcrumb-redesign` epic replaces the EfcViewer indented tree and the QuickFiler
folder dropdown with a single-line breadcrumb control in both surfaces. Both surfaces need to render
a `Folder -> ... -> Leaf` ancestor chain for a selected folder and, on demand, list the real
immediate Outlook subfolders of a given segment.

Today the hierarchy is synthesized only from the already-presented top-ranked suggestion rows:

- `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs` (`BuildFromRows`) derives parent/child
  edges by prefix-matching among the top-5-plus-recents suggestion rows.
- `UtilitiesCS/OutlookObjects/Folder/FolderHierarchyBuilder.cs` (`Build`) splits the same <=5
  suggestion paths on `\`.

Neither queries Outlook's real subfolder structure, so a segment's real immediate subfolders that do
not appear among the top-ranked suggestions are never shown. This feature introduces the epic's single
shared upstream contract; two UI consumers (issues 9102 EfcViewer and 9103 QuickFiler) depend on it.

This feature is wave 0 of the epic and has no upstream dependencies (`depends_on: []`). It must remain
independently mergeable: it ADDS the provider and does not delete the legacy prefix-matching methods or
rewire their UI callers. See "In Scope vs Out of Scope" for the wave-0 mergeability boundary.

## Behavior

Introduce a live Outlook folder-hierarchy provider with a clear public contract. Given a selected leaf
folder it returns:

1. The ordered ancestor chain `Folder -> ... -> Leaf` (root-to-leaf segments) for breadcrumb rendering.
2. On demand, the real immediate subfolders of a given segment, queried live against the real Outlook
   hierarchy through the existing cached snapshot (backed by `MAPIFolder.Folders` enumeration in the
   existing interop reader).

Per repository I/O-boundary policy, the live Outlook query is isolated behind an injectable seam so the
pure ancestor-chain and segment-children logic is unit-testable without a live Outlook process
(MSTest/Moq/FluentAssertions).

### Reuse-existing-seam design (and why)

The repository already contains a live Outlook folder-hierarchy read path that isolates COM behind an
interface, is cached and notification-refreshed, and is wired into production globals. This feature
reuses and thinly extends that infrastructure rather than introducing a second live-COM seam:

- `IOutlookFolderHierarchyReader` is the existing live-COM boundary. Its production implementation
  recursively enumerates `MAPIFolder.Folders`; every COM-touching member is `[ExcludeFromCodeCoverage]`
  and COM is wrapped behind internal adapter interfaces.
- `IOutlookFolderTreeService` / `OutlookFolderTreeService` provide a session-scoped cache of an
  immutable `FolderTreeSnapshot` with a notification-driven invalidation state machine.
- `FolderTreeSnapshot` already exposes the real immediate subfolders of a node
  (`GetChildren(FolderTreeNodeKey)`), and each `FolderTreeSnapshotNode` carries `ParentKey` and
  `ChildKeys`, which are the raw material for the ancestor walk.
- `IOlObjects.FolderTreeService` already exposes `IOutlookFolderTreeService` in production globals, so
  both UI consumers reach the snapshot through existing wiring.

Decisive reasons to reuse rather than add a new seam:

1. The live `MAPIFolder.Folders` query is already isolated behind `IOutlookFolderHierarchyReader` and
   already returns real immediate children through the snapshot. A second COM seam would duplicate the
   interop boundary, the notification/refresh state machine, and the test-fake surface, violating the
   simplicity-first and reusability priorities of the General Code Change Policy.
2. The snapshot already carries `ParentKey`/`ChildKeys` and a stable `FolderTreeNodeKey` identity, so
   the ancestor walk and subfolder projection are pure, synchronous, fully unit-testable functions over
   an in-memory snapshot. No new COM-bound code is added, hence no new coverage exemption is requested.
3. The snapshot is already wired to production via `IOlObjects.FolderTreeService`, so both UI features
   consume the provider through existing globals with no new plumbing.

A per-expand lazy COM read (extending `IOutlookFolderHierarchyReader` with `ReadImmediateChildrenAsync`)
is explicitly rejected: the snapshot is refreshed by the notification sink, so `GetChildren` already
reflects the current live hierarchy without an extra COM round-trip per expand. It is retained only as a
possible future optimization.

## Public Contract

The contract has four elements. Types live under `UtilitiesCS/OutlookObjects/Folder/` alongside the
existing snapshot infrastructure.

### 1. New pure helper: `FolderTreeSnapshotQueries.GetAncestorChain`

Host-neutral, synchronous, no COM. Added to the existing static `FolderTreeSnapshotQueries` class.

```csharp
public static IReadOnlyList<FolderTreeSnapshotNode> GetAncestorChain(
    FolderTreeSnapshot snapshot, FolderTreeNodeKey leafKey);
```

- **Input:** an immutable `FolderTreeSnapshot` and the `FolderTreeNodeKey` of the leaf.
- **Behavior:** resolves `leafKey` via `TryGetNode`, follows `ParentKey` to the store root collecting
  nodes, then reverses to root-first order.
- **Output:** ordered root-to-leaf list of `FolderTreeSnapshotNode`.
- **Invariants:**
  - Result is ordered root-first / leaf-last.
  - The last element equals the requested leaf node.
  - Every adjacent `(parent, child)` pair satisfies `child.ParentKey.Equals(parent.Key)`.
  - When the leaf is a store root the chain has exactly one element.
  - A defensive visited-set guards against a malformed cyclic `ParentKey`; on a detected cycle it
    returns the partial chain rather than looping.
- **Error / edge behavior:**
  - `snapshot` null -> `ArgumentNullException` (matching the existing methods in this class).
  - `leafKey` null, or a key not present in the snapshot (stale/removed folder) -> empty list, never
    null.

### 2. New shared provider contract: `IFolderHierarchyProvider`

The epic's single upstream contract consumed across module boundaries by 9102 and 9103. Host-neutral;
its only dependency is `IOutlookFolderTreeService` (an interface, not COM).

```csharp
public interface IFolderHierarchyProvider
{
    Task<IReadOnlyList<FolderBreadcrumbSegment>> GetAncestorChainAsync(
        FolderTreeNodeKey leafKey, CancellationToken cancellationToken);

    Task<IReadOnlyList<FolderBreadcrumbSegment>> GetImmediateSubfoldersAsync(
        FolderTreeNodeKey segmentKey, CancellationToken cancellationToken);

    Task<FolderTreeNodeKey> ResolveLeafKeyAsync(
        string folderPath, CancellationToken cancellationToken);
}
```

- `GetAncestorChainAsync` returns the ordered root-to-leaf segments for the selected leaf.
- `GetImmediateSubfoldersAsync` returns the real immediate subfolders of a given segment, sourced from
  the live cached snapshot.
- `ResolveLeafKeyAsync` resolves a UI-selected folder path to a `FolderTreeNodeKey` against the current
  snapshot. Returns `null` when no matching node exists.
- All members are `Task`-returning only because snapshot acquisition is async; the ancestor walk and
  children projection themselves are synchronous and are independently unit-tested via the pure helper.

### 3. New DTO: `FolderBreadcrumbSegment`

Immutable, host-neutral, net48-safe plain class (no `init`/`record`; per the repository net48 constraint
these fail `CS0518`).

```csharp
public sealed class FolderBreadcrumbSegment
{
    public FolderTreeNodeKey Key { get; }     // stable identity for the expand-this-segment call
    public string DisplayName { get; }        // folder name (leaf path segment)
    public string FolderPath { get; }         // full path; the selection value returned to the host
    public bool HasChildren { get; }          // node.ChildKeys.Count > 0 -> render the expand affordance
}
```

The DTO is deliberately probability-free. The percentage is joined by each UI feature from the existing
feature-324 plumbing (`IFolderProbabilitySource` / `FolderScore.Probability` keyed by `FolderPath`),
exactly as `FolderProbabilityAdapter` and `EfcFormController.BuildProbabilitySource` do today. Keeping
probability out of the segment preserves the "no change to scoring/probability plumbing" boundary.

### 4. New implementation: `OutlookFolderHierarchyProvider`

`OutlookFolderHierarchyProvider : IFolderHierarchyProvider`. Host-neutral facade; depends only on
`IOutlookFolderTreeService`. Not COM-bound and not coverage-exempt.

- Acquires the snapshot via
  `_treeService.GetSnapshotAsync(FolderTreeRequest.AllStores(allowStaleSnapshot: true), ct)`.
- `GetAncestorChainAsync` calls `FolderTreeSnapshotQueries.GetAncestorChain(snapshot, leafKey)` and maps
  each node to a `FolderBreadcrumbSegment`.
- `GetImmediateSubfoldersAsync` calls `snapshot.GetChildren(segmentKey)` and maps each node.
- `ResolveLeafKeyAsync` matches by `FolderPath` across the snapshot nodes (case-insensitive) and returns
  the node `Key`, or `null` when absent. Real Outlook full paths embed the store name and are unique in
  practice; a duplicate-path-across-stores input is a synthetic test case for which first-match is the
  documented behavior. Callers that already hold store identity should prefer the key-based members.

## Data & State

- No new persisted state is introduced. The provider is a read-only projection over the existing cached
  `FolderTreeSnapshot`.
- Data transformations: node -> `FolderBreadcrumbSegment` mapping and the ancestor walk described above.
  `HasChildren` is derived from `ChildKeys.Count > 0`.
- Caching: the provider does not add caching; it relies on the existing `IOutlookFolderTreeService`
  cache and its notification-driven invalidation. `allowStaleSnapshot: true` is used so breadcrumb
  rendering does not block on a refresh.
- Identity: `FolderTreeNodeKey` (StoreId + EntryId + FolderPath) is the identity used throughout, so
  duplicate segment display names at different depths are distinguished by key rather than by name.
- No migration or backfill is required.

## Constraints & Risks

- Constraint: do not change the scoring/ranking algorithm or the feature-324 probability plumbing.
- Constraint: repository I/O-boundary policy requires the live Outlook interop query to be isolated
  behind an injectable seam. This is satisfied by reusing `IOutlookFolderTreeService` /
  `IOutlookFolderHierarchyReader`.
- Constraint: this is a shared public contract consumed across module boundaries by two UI features
  (9102, 9103); it must have documented inputs, outputs, and invariants and remain stable.
- Constraint (net48): the DTO must be a plain class; `init`/`record`/`record struct` are unavailable on
  net48 and would fail `CS0518`.
- Risk (COM/Outlook interop): live `MAPIFolder.Folders` enumeration must not be exercised in unit tests.
  Mitigation: this feature adds no COM code; it depends only on the interface `IOutlookFolderTreeService`,
  which is mocked in tests.
- Risk: an existing seam already provides part of the live-query surface. Mitigation: the design reuses
  those seams rather than introducing a second live-COM boundary (see the reconciliation above).

## Implementation Strategy

- New production files under `UtilitiesCS/OutlookObjects/Folder/`:
  - `IFolderHierarchyProvider.cs` (interface + XML docs; type-only, no executable lines).
  - `FolderBreadcrumbSegment.cs` (immutable DTO + XML docs).
  - `OutlookFolderHierarchyProvider.cs` (facade over `IOutlookFolderTreeService`; host-neutral; not
    exempt).
- Modified production file:
  - `FolderTreeSnapshotQueries.cs` — add `GetAncestorChain` (well under the 500-line file limit).
- Register new production files in `UtilitiesCS.csproj` and new test files in `UtilitiesCS.Test.csproj`
  (both use explicit `<Compile Include>` item lists).
- The two legacy methods (`FolderSuggestionTree.BuildFromRows`, `FolderHierarchyBuilder.Build`) may be
  annotated in XML docs as superseded, pointing to `IFolderHierarchyProvider`. They are not deleted and
  their UI callers are not rewired in this feature.
- No new packages. Logging follows the existing project pattern; no new telemetry is introduced by this
  read-only projection.

## Testability & Coverage Plan

- Pure logic is unit-tested with MSTest, Moq, and FluentAssertions; no live Outlook process and no COM
  interop are touched.
- `FolderTreeSnapshotQueries.GetAncestorChain` is tested over hand-built `FolderTreeSnapshot` fixtures:
  single-level, multi-level, root-only (leaf == root), null snapshot (`ArgumentNullException`),
  null/unknown leaf (empty list), the defensive cycle guard, and duplicate segment names at different
  depths (identity by `FolderTreeNodeKey`, not display name).
- `FolderBreadcrumbSegment` construction/mapping is tested, including `HasChildren` derivation from
  `ChildKeys`.
- `OutlookFolderHierarchyProvider` is tested with a Moq `IOutlookFolderTreeService` returning a prebuilt
  snapshot: ancestor chain, immediate subfolders (populated set, empty set, unknown key), path
  resolution (found, not found, duplicate-path first-match), and cancellation propagation.
- Snapshot fixtures are built directly via the `FolderTreeSnapshot` constructor or via
  `FolderTreeSnapshotBuilder` + `FakeOutlookFolderHierarchyReader` (existing pattern). No temporary
  files are used.
- Coverage denominator: all three new production files are in the testable denominator; the interface
  file has no executable lines (type-only). New code targets >= 90% and does not reduce the repository
  coverage floor. Because the feature adds zero exempt lines, it contributes positively to the metric.
  Where the embedded CLAUDE.md thresholds (80% floor / 90% new-code) and the `.claude/rules` summaries
  (85% line / 75% branch) differ, CLAUDE.md governs per the Policy Compliance Order; the plan meets the
  stricter-applicable interpretation because the new code is fully testable.

## In Scope vs Out of Scope

### In scope

- Add `IFolderHierarchyProvider`, `FolderBreadcrumbSegment`, `OutlookFolderHierarchyProvider`, and the
  `FolderTreeSnapshotQueries.GetAncestorChain` pure helper.
- Reuse the existing snapshot/live-query seams (`IOutlookFolderTreeService`,
  `IOutlookFolderHierarchyReader`) with no new COM code and no new coverage exemption.
- Unit tests for the pure helper, the DTO, and the provider facade.
- Optional XML-doc annotation on the two legacy methods marking them superseded.

### Out of scope

- No change to the scoring/ranking algorithm.
- No change to the feature-324 probability plumbing (`FolderScore.Probability` -> `FolderRow.Score` ->
  `PercentageFormatter.FormatPercent`); the UI join of the percentage by `FolderPath` happens in the
  consuming UI features.
- No WebView2 controls or JS<->.NET bridges (owned by 9102 and 9103).
- No new live-COM seam.

### Wave-0 mergeability boundary (explicit)

Feature 9101 ADDS the provider. It does NOT delete `FolderSuggestionTree.BuildFromRows` or
`FolderHierarchyBuilder.Build`, and it does NOT rewire their UI callers
(`EfcFormController.BindFolderRows`, `ItemViewer.SetFolderSuggestions`). Those two methods are consumed
by live UI callers whose control replacements (TreeListView -> WebView2, ComboBox -> WebView2) are owned
by 9102/9103. Deleting or rewiring them here would break the build for unmigrated UI code and pull
wave-1 UI work into wave 0. Removal of the legacy methods and rewiring of their callers is executed by
each consuming UI feature as it adopts the provider. "Replaced" is therefore an epic-level outcome
realized across waves; within 9101 the accurate statement is "superseded, deletion deferred to the
consuming UI features."

## Acceptance Criteria

- [x] A public folder-hierarchy provider contract (`IFolderHierarchyProvider`) returns the ordered
      root-to-leaf ancestor chain for a selected leaf folder.
- [x] The provider returns, on demand, the real immediate subfolders of a given segment queried live
      against the real Outlook hierarchy (via the existing cached snapshot, not from suggestion rows).
- [x] The live Outlook query is isolated behind an injectable seam (reused `IOutlookFolderTreeService` /
      `IOutlookFolderHierarchyReader`); the pure ancestor-chain and segment-children logic is
      unit-testable without a live Outlook process, and the feature adds no new COM/coverage-exempt code.
- [x] The pure ancestor-chain helper (`FolderTreeSnapshotQueries.GetAncestorChain`) enforces the
      documented invariants: root-first/leaf-last ordering; last element equals the requested leaf;
      adjacent `(parent, child)` satisfies `child.ParentKey == parent.Key`; leaf == root yields a
      single-element chain; null snapshot raises `ArgumentNullException`; null/unknown key returns an
      empty list (never null); identity is by `FolderTreeNodeKey`; a defensive cycle guard prevents
      looping.
- [x] The new provider supersedes the prefix-matching-over-suggestion-rows approach used by
      `FolderSuggestionTree.BuildFromRows` and `FolderHierarchyBuilder.Build` by delivering the live
      ancestor-chain and real-immediate-subfolder queries those methods lack. Removal of the old methods
      and rewiring of their UI callers (`EfcFormController.BindFolderRows` for 9102,
      `ItemViewer.SetFolderSuggestions` for 9103) is executed by the consuming UI features when they
      adopt the provider, so wave-0 feature 9101 stays independently mergeable and does not break the
      build by deleting methods still referenced by unmigrated UI code.
- [x] The scoring/ranking algorithm and the feature-324 probability plumbing
      (`FolderScore.Probability` -> `FolderRow.Score` -> `PercentageFormatter.FormatPercent`) are
      unchanged, and `FolderBreadcrumbSegment` carries no probability field.
- [x] Full C# toolchain green (csharpier, .NET analyzers, nullable, MSTest); changed and new code meets
      repository coverage thresholds.

## Seeded Test Conditions (from potential)

- [x] Unit coverage: ancestor-chain computation for single-level, multi-level, and root-only folders.
- [x] Unit coverage: segment-children retrieval via a mocked live-hierarchy seam.
- [x] Negative flows: missing/null selected folder, unresolved handle, empty subfolder set.
- [x] Edge cases: leaf equals root; duplicate segment names at different depths.
- [x] Isolation: no test touches a live Outlook process or COM interop.
