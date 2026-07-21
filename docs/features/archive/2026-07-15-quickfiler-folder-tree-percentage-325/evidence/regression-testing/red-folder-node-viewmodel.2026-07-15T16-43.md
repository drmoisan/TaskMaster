# Red — FolderNodeViewModelTests (P2-T2) [expect-fail]

Timestamp: 2026-07-16T09-50
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:FullyQualifiedName~FolderNodeViewModelTests
EXIT_CODE: 1 (expected failure — red phase)

Output Summary: All 5 FolderNodeViewModelTests FAIL against the unimplemented derived `Glyph` and
`FormattedPercentage` accessors (both stubbed to throw NotImplementedException). Expected red-phase
outcome before P2-T3.
Total tests: 5 | Failed: 5 | Passed: 0.
Failing tests:
- Glyph_CollapsedParent_IsPlus (INV4: HasChildren && !Expanded -> '+')
- Glyph_ExpandedParent_IsMinus (INV4: HasChildren && Expanded -> '-')
- Glyph_Leaf_HasNoGlyph (INV4: leaf -> no glyph / null)
- FormattedPercentage_NullProbability_IsEmpty (null Probability -> "")
- FormattedPercentage_WithProbability_DelegatesToFormatter (0.4267 -> "43%")
