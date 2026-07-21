# Code Review — folder-hierarchy-live-provider (Issue #350)

- Timestamp: 2026-07-18T08-23
- Feature branch: feature/folder-hierarchy-live-provider-350 (HEAD 158ccb84)
- Base branch: origin/epic/folder-tree-breadcrumb-redesign-integration

## Executive Summary

Overall verdict: PASS. Zero Blocking findings.

The change is small, cohesive, and idiomatic for the UtilitiesCS module. Production code is host-neutral
and testable without COM; the pure ancestor-walk is separated from async snapshot acquisition; error
paths fail fast with `ArgumentNullException`; XML documentation covers the public contract and its
invariants. Tests are deterministic, use the mandated MSTest/Moq/FluentAssertions stack, and cover
positive, negative, edge, and cancellation scenarios. No temporary files, no live Outlook, no COM.
Two Advisory (non-blocking) observations are recorded for future consideration only.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Advisory | UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs | ResolveLeafKeyAsync (lines 65-67) | Linear `FirstOrDefault` scan over `NodesByKey.Values` for path resolution is O(n) per call. | Acceptable for current snapshot sizes; revisit only if profiling shows a hot path. | Real Outlook hierarchies are modest; the spec documents first-match semantics. Not a defect. | Source read; spec section "New implementation". |
| Advisory | UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbSegment.cs | Constructor (lines 36-37) | Null `displayName`/`folderPath` are coalesced to `string.Empty` rather than rejected. | Retain; matches the null-tolerant convention of `FolderTreeSnapshotNode`. | Consistent with surrounding module behavior; tested by `Constructor_WithNullStrings_...`. | Source read; `FolderTreeSnapshotNode.cs` ctor. |

No Blocking or Major findings.

## Detailed Observations

### Design and structure

- The provider is a thin facade over `IOutlookFolderTreeService`; the only stateful field is the
  injected service, guarded for null in the constructor. Cohesive single responsibility.
- `GetAncestorChain` is a pure static method added to the existing `FolderTreeSnapshotQueries` static
  class, consistent with the sibling query helpers (`GetSelectedNodes`, `GetArchiveRoot`, etc.). The
  cycle guard uses a `HashSet<FolderTreeNodeKey>` and terminates on a repeat key with a clear
  explanatory comment about the `visited.Add` return contract.
- `MapNode`/`MapNodes` are private static helpers, avoiding duplication between the ancestor-chain and
  immediate-subfolder projections. `HasChildren` is derived from `node.ChildKeys.Count > 0`, matching
  the spec.
- Async members correctly use `ConfigureAwait(false)` on the snapshot acquisition await.

### Contracts and error handling

- Null `treeService` and null `key` raise `ArgumentNullException` with the correct `nameof` parameter
  names, matching the class-wide convention and asserted in tests via `WithParameterName`.
- `ResolveLeafKeyAsync` returns `null` on whitespace/empty input before acquiring a snapshot, and
  `null` when no node matches — matching the documented contract.
- `GetAncestorChain` returns `Array.Empty<...>()` (never null) for null/unknown keys.

### Documentation

- All public members carry XML documentation describing inputs, outputs, invariants, and exceptions.
  `<inheritdoc />` is used on the implementation, keeping the contract single-sourced on the interface.
- Comments explain "why" (cycle-guard rationale, first-match duplicate-path rationale) rather than
  restating code.

### Tests

- Node/key fixtures are built via small local `Key`/`Node` factory helpers, keeping tests readable.
- Cancellation is exercised deterministically with a pre-cancelled token and a Moq setup that observes
  the token, avoiding timing dependence.
- `GetAncestorChainAsync_RequestsAllStoresAllowingStaleSnapshot` verifies the `AllStores(allowStale)`
  request shape, protecting the "do not block breadcrumb rendering on refresh" design decision.
- Test files are located under `UtilitiesCS.Test/OutlookObjects/Folder/`, mirroring the production
  tree; no colocation with production source.

## Toolchain Confirmation

- Format (csharpier): green (exit 0, no diffs).
- Lint (.NET analyzers): green (0 errors, no new warnings on touched files).
- Type-check (nullable, warnings-as-errors): green (0/0).
- Tests (MSTest): green (4344/4344; +23 new).

Evidence: `evidence/qa-gates/final-csharpier.md`, `final-analyzers.md`, `final-nullable.md`,
`final-tests-coverage.md`.
