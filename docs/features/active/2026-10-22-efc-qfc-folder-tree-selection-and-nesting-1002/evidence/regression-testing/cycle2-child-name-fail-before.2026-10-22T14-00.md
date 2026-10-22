# Cycle 2 P4-T2: child-name fail-before (expect-fail)

Timestamp: 2026-10-22T14-00
Command: vstest.console.exe UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:FullyQualifiedName~BreadcrumbHtmlRendererIssue1002Tests (before the BreadcrumbDocumentAssets.cs change)
EXIT_CODE: 1
Output Summary:
- total 14, executed 14, passed 13, failed 1 (expected failure; assertion failure).
- Failing: BridgeJs_SubfolderResultHandler_WrapsChildNameInNameSpan (the handler text does not contain the name span statements).
- The other 13 tests of the class pass, including RenderRowFragment_KeylessChildren_RenderUnchanged.
