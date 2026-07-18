# Acceptance Criteria Check-Off Mapping (P4-T7)

Timestamp: 2026-07-18T00-40

Work Mode: full-feature. AC sources: `spec.md` (## Acceptance Criteria + Seeded Test Conditions) and `user-story.md` (## Acceptance Criteria). Every AC below is checked `[x]` in its source file and maps to concrete evidence.

## spec.md — ## Acceptance Criteria

| AC | Evidence (task ID + artifact / code) |
|---|---|
| Provider returns ordered root-to-leaf ancestor chain | P3-T1/P3-T2 `OutlookFolderHierarchyProvider.GetAncestorChainAsync`; P3-T3 `GetAncestorChainAsync_HappyPath_...`; test run P4-T4 (`final-tests-coverage.md`) |
| Provider returns real immediate subfolders via cached snapshot (not suggestion rows) | P3-T2 `GetImmediateSubfoldersAsync` (calls `snapshot.GetChildren`); P3-T3 `GetImmediateSubfoldersAsync_PopulatedSegment_...`; P4-T4 |
| Live query isolated behind injectable seam; unit-testable without live Outlook; no new COM/exempt code | P3-T1 provider depends only on `IOutlookFolderTreeService`; all tests use Moq (no COM); P4-T6 `scope-boundary-check.md` (no new exempt code) |
| `GetAncestorChain` enforces documented invariants | P2-T1 method; P2-T2/P2-T3 tests (root-only, single/multi-level, ordering, leaf-identity, parent/child linkage, null snapshot -> ArgumentNullException, null/unknown key -> empty, cycle guard, identity by key); P4-T4 |
| New provider supersedes prefix-matching; legacy methods/callers retained for wave-0 mergeability | P4-T6 `scope-boundary-check.md` (BuildFromRows + Build present, callers unchanged) |
| Scoring/ranking + feature-324 probability plumbing unchanged; `FolderBreadcrumbSegment` has no probability field | P1-T1 DTO is probability-free (no `Probability`/`Score` member); P4-T6 (no probability-plumbing files in diff) |
| Full C# toolchain green; coverage thresholds met | P4-T1 `final-csharpier.md`, P4-T2 `final-analyzers.md`, P4-T3 `final-nullable.md`, P4-T4 `final-tests-coverage.md`, P4-T5 `coverage-delta.md` |

## spec.md — Seeded Test Conditions

| Condition | Evidence |
|---|---|
| Ancestor-chain: single-level, multi-level, root-only | P2-T2 `GetAncestorChain_SingleLevel/_MultiLevel/_RootOnlyLeaf_...` |
| Segment-children via mocked seam | P3-T3 `GetImmediateSubfoldersAsync_PopulatedSegment_...` (Moq `IOutlookFolderTreeService`) |
| Negative flows: missing/null folder, unresolved handle, empty subfolder set | P2-T3 null/unknown leaf; P3-T4 unknown segment key, empty set, unknown path |
| Edge cases: leaf == root; duplicate names at different depths | P2-T2 `_RootOnlyLeaf_...`, `_DuplicateDisplayNames..._DistinguishedByKey` |
| Isolation: no live Outlook / COM | All tests use in-memory snapshots + Moq; P4-T4 |

## user-story.md — ## Acceptance Criteria

| AC | Evidence |
|---|---|
| Obtain ancestor chain with last == requested leaf and `HasChildren` set | P3-T3 `GetAncestorChainAsync_HappyPath_...` (asserts order, last==leaf, HasChildren) |
| Obtain real immediate subfolders; empty list (not null) when none | P3-T3/P3-T4 `GetImmediateSubfoldersAsync_...` (populated, empty-never-null, unknown-key) |
| Resolve path to stable key; `null` when absent; route by key (duplicates distinguished) | P3-T3/P3-T4 `ResolveLeafKeyAsync_FoundPath/_UnknownPath/_DuplicatePaths_...` |
| Segment DTO probability-free; scoring/plumbing unchanged | P1-T1 DTO; P4-T6 |
| Provider + helper unit-testable without live Outlook (reused seam) | Moq-based tests; P3-T1 constructor depends only on interface |
| Wave-0 mergeability preserved (no deletion/rewiring) | P4-T6 `scope-boundary-check.md` |
| Full C# toolchain green; coverage thresholds met | P4-T1..P4-T5 artifacts |

All AC items in both `spec.md` and `user-story.md` are checked `[x]`.
