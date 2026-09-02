Timestamp: 2026-08-31T10:33:02-04:00
Construction 1: rg -n "SelectedFolderPath\\.Should\\(\\)\\.Be\\(" --glob "*.cs" QuickFiler.Test/
Construction 2: rg -n "rowSelected|SelectFirstRow" --glob "*.cs" QuickFiler.Test/
EXIT_CODE: 0 for both searches
Output Summary: Both constructions identify one assertion that must change: BreadcrumbBridgeRouterIssue439Tests.cs:165, `router.SelectedFolderPath.Should().Be(fullTarget);`, inside the method declared at line 119. It is the sole existing selected-row assertion where a presented filing target is a full Outlook path at or under a non-empty bound root.

Construction 1 classifications: all other assertions are relative stems, no-root pass-through values, pseudo-row outcomes, unchanged state, or rows outside the bound-root/full-Outlook-path contract. Construction 2 classifications: the only matching non-empty-bound-root full-target row selection is the Issue439 test at :156→:165; the other rowSelected/SelectFirstRow hits are selection behavior, queue behavior, no-root behavior, documentation, invalid-row behavior, or already-relative targets. No test binds a presented row whose filing target equals the bound archive root; zero tests depend on archive-root-exact selection.
