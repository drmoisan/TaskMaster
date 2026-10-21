Timestamp: 2026-08-31T10:31:34-04:00
Search 1: rg -n "Issue439SlashOnlyArchiveRootPreservesFullHierarchySelection|SegmentActivate_WithNoBoundArchiveRoot_PreservesThePassThroughMode" --glob "*.cs" .
Search 2: rg -n "BindRowsAsync\\(" --glob "*.cs" QuickFiler.Test/Controllers/
EXIT_CODE: 0 for both searches
Output Summary: Search 1 returned exactly 2 declarations. Search 2 found 21 BindRowsAsync sites; classification identified the same two pass-through cases and 19 calls that bind a non-empty archive root, are unrelated queue/selection coverage, or use a non-pass-through input.

Pass-through cases:
- BreadcrumbBridgeRouterIssue439Tests.cs:619 Issue439SlashOnlyArchiveRootPreservesFullHierarchySelection; binds @"\\" at :645 and asserts Be(@"\\Archive") at :665.
- BreadcrumbBridgeRouterIssue614Tests.cs:188 SegmentActivate_WithNoBoundArchiveRoot_PreservesThePassThroughMode; uses the three-argument overload at :213 and asserts Be(@"\\Archive") at :221.

All other BindRowsAsync hits were classified as non-pass-through because they bind a non-empty root, bind row data for a separate test contract, or exercise the null-row overload; none asserts the no-bound-root full-hierarchy pass-through result.
