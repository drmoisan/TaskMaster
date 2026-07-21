# Red — FolderTreeStateModelTests (P4-T2) [expect-fail]

Timestamp: 2026-07-16T10-25
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:FullyQualifiedName~FolderTreeStateModelTests
EXIT_CODE: 1 (expected failure — red phase)

Output Summary: All 13 FolderTreeStateModelTests FAIL against the unimplemented transitions and
projection (all methods stubbed to throw NotImplementedException). Expected red-phase outcome before
P4-T3.
Total tests: 13 | Failed: 13 | Passed: 0.
Coverage of invariants (INV1-INV8) plus arrow no-ops and collapse/re-expand round-trip:
- INV1_ExpandOrToggleLeaf_IsNoOp
- INV2_ChildVisibleOnlyWhenAncestorsExpanded_RootsAlwaysVisible
- INV3_HighlightIsSingle
- INV4_GlyphBijectionTracksExpansion
- INV5_CollapsePreservesDescendantExpansion_ReExpandRestores (collapse/re-expand round-trip)
- INV6_ToggleIsInvolutionOnParent
- INV7_VisibleRowDepthEqualsStructuralDepth
- INV8_VisibleOrderIsStablePreOrderDfs_AndDeterministic
- RightArrow_ExpandsHighlightedCollapsedParent
- LeftArrow_CollapsesHighlightedExpandedParent
- RightArrow_OnLeafOrAlreadyExpanded_IsNoOp
- LeftArrow_OnLeafOrAlreadyCollapsed_IsNoOp
- Arrows_WithNoHighlight_AreNoOp
