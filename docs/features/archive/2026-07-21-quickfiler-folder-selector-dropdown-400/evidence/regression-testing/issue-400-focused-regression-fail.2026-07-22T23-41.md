# Issue #400 focused regression failure

Timestamp: 2026-07-23T03:41:14.3876448Z

Command: `$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'; $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; if (-not $vstest) { throw 'VSTest was not resolved.' }; $filter = 'FullyQualifiedName~QfcItemControllerBreadcrumbDropDownTests|FullyQualifiedName~BreadcrumbBridgeCoordinatorProbabilityTests|FullyQualifiedName~BreadcrumbDropDownCoverageThresholdTests|FullyQualifiedName~BreadcrumbDropDownHostTests|FullyQualifiedName~BreadcrumbDropDownIntegrationTests|FullyQualifiedName~BreadcrumbDropDownLifecycleConcurrencyTests|FullyQualifiedName~BreadcrumbDropDownLifecycleTests|FullyQualifiedName~BreadcrumbDropDownReadinessTests|FullyQualifiedName~BreadcrumbMessengerHubTests|FullyQualifiedName~BreadcrumbPopupPlacementTests|FullyQualifiedName~BreadcrumbSelectorCoordinatorTests|FullyQualifiedName~FolderBreadcrumbAssetContractTests|FullyQualifiedName~ItemViewerBreadcrumbDropDownContractTests|FullyQualifiedName~BreadcrumbRenderProjectionSelectorTests|FullyQualifiedName~BreadcrumbSelectionSessionTests|FullyQualifiedName~BreadcrumbSelectorMessagesTests|FullyQualifiedName~BreadcrumbStateModelSelectorTests|FullyQualifiedName~FolderBreadcrumbBridgeRouterEdgeTests|FullyQualifiedName~FolderBreadcrumbBridgeRouterTests|FullyQualifiedName~BreadcrumbDuplicateIdentityTests|FullyQualifiedName~BreadcrumbDuplicateIdentityIntegrationTests|FullyQualifiedName~BreadcrumbUiThreadDispatchTests|FullyQualifiedName~BreadcrumbCollapsedSurfaceReadinessTests|FullyQualifiedName~FolderBreadcrumbRouterSelectionConcurrencyTests|FullyQualifiedName~BreadcrumbCoordinatorLifecycleTests|FullyQualifiedName~BreadcrumbPendingOpenCloseTests|FullyQualifiedName~BreadcrumbSubfolderSelectorSessionTests|FullyQualifiedName~BreadcrumbSubfolderActivationTests|FullyQualifiedName~BreadcrumbSelectorToggleUiBoundaryTests|FullyQualifiedName~BreadcrumbPopupControlDispatchTests|FullyQualifiedName~BreadcrumbSelectorOpenRetryTests|FullyQualifiedName~BreadcrumbDropDownOpenCoordinatorTests|FullyQualifiedName~BreadcrumbPopupBoundaryCoverageTests|FullyQualifiedName~BreadcrumbDropDownLifecycleCoverageTests|FullyQualifiedName~BreadcrumbMessengerHubCoverageTests'; & $vstest 'UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll' 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation "/TestCaseFilter:$filter" /Logger:'console;Verbosity=normal'; exit $LASTEXITCODE`

EXIT_CODE: 1

Output Summary: VSTest 18.8.0 matched both assemblies and dynamically discovered 358 cases in the exact 19 pre-remediation plus 16 new-class filter. The run completed naturally with 353 passed, 5 failed, and zero skipped. The `FolderBreadcrumbBridgeRouterInFlightTests.cs` source was correctly addressed through its compiled partial-class alias `FolderBreadcrumbBridgeRouterTests`. P8-T2 remains unchecked; this artifact is failure diagnostics and is not the required passing `issue-400-focused-regression.<timestamp>.md`.

## Failures

1. `QfcItemControllerBreadcrumbDropDownTests.ConfigureBreadcrumbDropDown_PassesExistingEnvironmentAndDarkThemeLazily`
   - `TargetInvocationException` wrapping `ArgumentNullException`.
   - Parameter name: `operations`.
   - Production path: `BreadcrumbWebViewSurfaceFactory.Create(...)` line 185 -> `BreadcrumbDropDownHost` constructor line 67 -> `ItemViewer.ConfigureBreadcrumbDropDown(...)` line 169 -> `QfcItemController.ConfigureBreadcrumbDropDown(...)` line 166.

2. `QfcItemControllerBreadcrumbDropDownTests.ConfigureBreadcrumbDropDown_LightThemeUsesSameControllerSetupSeam`
   - Same `TargetInvocationException` / `ArgumentNullException("operations")` production path.

3. `QfcItemControllerBreadcrumbDropDownTests.ConfigureBreadcrumbDropDown_RepeatedSameEnvironmentReusesPopupHost`
   - Same `TargetInvocationException` / `ArgumentNullException("operations")` production path.

4. `BreadcrumbDropDownIntegrationTests.InitializationFailure_CancelsSessionWithoutDuplicateClose`
   - Moq expected `host.Close(It.IsAny<BreadcrumbDropDownCloseReason>())` never to be called.
   - The run observed one `host.Close(BreadcrumbDropDownCloseReason.ExplicitCommit)` invocation.

5. `BreadcrumbDropDownCoverageThresholdTests.OpenAsync_RollbackCallbackFailsOnce_OuterPipelineCompletesRecovery`
   - Actual placement-failure message: `The popup working area has no available space.`
   - Expected message: `The active working area has no space for the folder selector popup.`

## Disposition

No production, test, project, configuration, filter, or threshold source was changed. P8-T2 is not complete, and P8-T3 through P8-T6 were not executed by this batch after the non-whitespace regression failures appeared.
