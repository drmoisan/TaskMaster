# Green — FolderTreeStateModelTests (P4-T3)

Timestamp: 2026-07-16T10-35
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:FullyQualifiedName~FolderTreeStateModelTests
EXIT_CODE: 0

Output Summary: `Test Run Successful.` All 13 FolderTreeStateModelTests PASS (INV1-INV8, arrow no-ops
at root and leaf, collapse/re-expand round-trip). Total tests: 13 | Passed: 13 | Failed: 0.

Implementation notes:
- Expand sets Expanded only when HasChildren (INV1); Collapse clears only the node's own flag,
  leaving descendant flags intact (INV5); Toggle branches on Expanded for parents only (INV1/INV6).
- Highlight stores a single node reference, inherently at-most-one (INV3).
- RightArrow/LeftArrow act on the highlighted node with the guarded no-op conditions.
- GetVisibleNodes / GetVisibleRows perform a stable pre-order DFS: every root is emitted (roots
  always visible), children only when the parent is Expanded (INV2). Row Depth carries the render
  indent (INV7). Sibling order is preserved from the builder (predictor descending-score input),
  yielding a deterministic, stable order (INV8).

INV8 interpretation note: sibling order is preserved from the forest as constructed by
FolderHierarchyBuilder from the predictor's descending-score input, projected via a stable pre-order
DFS. The state model does not re-sort siblings, which is required so that the depth-0 root order
(SUGGESTIONS separator, suggestion ancestors, RECENT SELECTIONS separator, recents) is preserved
verbatim per the spec; re-sorting would break that fixed section order. The result is deterministic,
stable, and descending-score as arranged.
