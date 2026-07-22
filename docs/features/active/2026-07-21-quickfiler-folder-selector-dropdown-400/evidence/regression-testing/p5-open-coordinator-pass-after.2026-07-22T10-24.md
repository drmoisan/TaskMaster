# P5 Open Coordinator Preservation Pass-After

Timestamp: 2026-07-22T10:24:16Z

Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation "/TestCaseFilter:FullyQualifiedName~ItemViewerBreadcrumbDropDownContractTests|FullyQualifiedName~BreadcrumbDropDownOpenCoordinatorTests|FullyQualifiedName~BreadcrumbSelectorOpenRetryTests|FullyQualifiedName~BreadcrumbSelectorToggleUiBoundaryTests|FullyQualifiedName~BreadcrumbDropDownIntegrationTests" '/Logger:console;Verbosity=detailed'`

EXIT_CODE: 0

Output Summary: PASS. VSTest discovered exactly 37 cases, with 37 passed, 0 failed, and 0 skipped. Per-class results were 5/5 ItemViewerBreadcrumbDropDownContractTests, 10/10 BreadcrumbDropDownOpenCoordinatorTests, 8/8 BreadcrumbSelectorOpenRetryTests, 4/4 BreadcrumbSelectorToggleUiBoundaryTests, and 10/10 BreadcrumbDropDownIntegrationTests. All 15 J1 contract/coordinator cases passed, and the existing mouse, keyboard, retry, and UI-boundary behaviors remained passing.

Corrected observations verified by the two formerly failing integration cases:

- Constructor_NullLegacySurfaceFactory_ThrowsForSurfaceFactory passed with ArgumentNullException.ParamName equal to surfaceFactory.
- NativeAutomaticClose_RestoresOriginalCommittedIdentityWithoutPendingPublicationAndReturnsFocusOnce passed with committed plain:0:A, pending plain:1:B, restored committed plain:0:A, null pending after close, output path A, zero selection publication, and one focus return.
