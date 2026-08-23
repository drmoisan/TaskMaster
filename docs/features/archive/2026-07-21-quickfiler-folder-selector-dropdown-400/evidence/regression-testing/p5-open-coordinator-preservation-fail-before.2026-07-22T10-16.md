# P5 Open Coordinator Preservation Fail-Before

Timestamp: 2026-07-22T10:16:00Z

Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation "/TestCaseFilter:FullyQualifiedName~ItemViewerBreadcrumbDropDownContractTests|FullyQualifiedName~BreadcrumbDropDownOpenCoordinatorTests|FullyQualifiedName~BreadcrumbSelectorOpenRetryTests|FullyQualifiedName~BreadcrumbSelectorToggleUiBoundaryTests|FullyQualifiedName~BreadcrumbDropDownIntegrationTests" '/Logger:console;Verbosity=detailed'`

EXIT_CODE: 1

Output Summary: VALID EXPECTED FAILURE. VSTest discovered exactly 37 cases: 5 `ItemViewerBreadcrumbDropDownContractTests` passed, 10 `BreadcrumbDropDownOpenCoordinatorTests` passed, 8 `BreadcrumbSelectorOpenRetryTests` passed, 4 `BreadcrumbSelectorToggleUiBoundaryTests` passed, and 10 `BreadcrumbDropDownIntegrationTests` produced 8 passed and the two intended failures. Overall result: 35 passed, 2 failed, 0 skipped. All 15 J1 contract/coordinator cases passed.

Exact intended diagnostics:

- `BreadcrumbDropDownIntegrationTests.Constructor_NullLegacySurfaceFactory_ThrowsForSurfaceFactory`: expected `ArgumentNullException.ParamName` `surfaceFactory`; actual value was `factory`, differing at index 0.
- `BreadcrumbDropDownIntegrationTests.NativeAutomaticClose_RestoresOriginalCommittedIdentityWithoutPendingPublicationAndReturnsFocusOnce`: expected committed identity `A`; actual value was `plain:0:A`, differing at index 0.

No compilation failure, zero-test result, skipped test, or unrelated failure occurred.
