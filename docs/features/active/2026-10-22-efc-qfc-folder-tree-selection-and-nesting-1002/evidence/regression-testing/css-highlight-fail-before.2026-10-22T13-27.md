# CSS highlight fail-before (P3-T4, expect-fail, #1002 cycle 1)

Timestamp: 2026-10-22T13-27
Command: vstest.console.exe UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:FullyQualifiedName~BreadcrumbHtmlRendererIssue1002Tests (before the BreadcrumbDocumentAssets.cs change)
EXIT_CODE: 1
Output Summary:
- total 13, executed 13, passed 10, failed 3 (expected failure).
- Failing: LightThemeCss_SelectedChild_UsesSelectionBackground, DarkThemeCss_SelectedChild_UsesSelectionBackground (the theme blocks hold no `.child[aria-selected]` rule), BaseCss_ChildName_CarriesEllipsisRule (no `.child .name {` rule in BaseCss).
- Passing: the 10 pre-existing tests of the class, including BaseCss_Child_IsFlexRow.
