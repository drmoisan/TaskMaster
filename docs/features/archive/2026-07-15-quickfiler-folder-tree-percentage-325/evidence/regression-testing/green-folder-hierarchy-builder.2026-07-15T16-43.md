# Green — FolderHierarchyBuilderTests (P3-T3)

Timestamp: 2026-07-16T10-12
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:FullyQualifiedName~FolderHierarchyBuilderTests
EXIT_CODE: 0

Output Summary: `Test Run Successful.` All 5 FolderHierarchyBuilderTests PASS. Total tests: 5 | Passed: 5 | Failed: 0.

Implementation: `Build` iterates rows in order. Non-suggestion rows (null Score) become depth-0 leaf
roots preserving Text verbatim. Suggestion rows split `FolderScore.FolderPath` on `\` and walk
find-or-add (matched by cumulative full path among the current level): intermediate segments are
synthesized ancestors (`hasChildren: true`, probability null), the final segment is a leaf with the
probability. `DisplayName` = current segment; `FolderPath` = cumulative full path (node key); `Depth`
= segment index. Reuses the existing `TreeNode<T>` (`AddChild`).
