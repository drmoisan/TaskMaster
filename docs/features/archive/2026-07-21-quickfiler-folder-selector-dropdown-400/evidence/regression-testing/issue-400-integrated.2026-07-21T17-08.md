# Issue #400 Integrated Regression

Timestamp: 2026-07-21T17:08:00Z

Command:

```powershell
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'; $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; if (-not $vstest -or -not (Test-Path -LiteralPath $vstest)) { throw 'vstest.console.exe not found via vswhere.' }; & $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbSelectionSessionTests|FullyQualifiedName~BreadcrumbSelectorMessagesTests|FullyQualifiedName~BreadcrumbStateModelSelectorTests|FullyQualifiedName~BreadcrumbRenderProjectionSelectorTests|FullyQualifiedName~FolderBreadcrumbBridgeRouter|FullyQualifiedName~BreadcrumbBridgeCoordinatorProbabilityTests|FullyQualifiedName~BreadcrumbSelectorCoordinatorTests|FullyQualifiedName~BreadcrumbMessengerHubTests|FullyQualifiedName~BreadcrumbPopupPlacementTests|FullyQualifiedName~BreadcrumbDropDownHostTests|FullyQualifiedName~BreadcrumbDropDownLifecycleTests|FullyQualifiedName~BreadcrumbDropDownIntegrationTests|FullyQualifiedName~ItemViewerBreadcrumbDropDownContractTests|FullyQualifiedName~QfcItemControllerBreadcrumbDropDownTests|FullyQualifiedName~FolderBreadcrumbAssetContractTests"
```

EXIT_CODE: 0

Matched test assemblies: 2

Passed: 115

Failed: 0

Skipped: 0

## Discovery Proof

Every explicitly filtered class family was discovered with named passing output:

- `BreadcrumbSelectionSessionTests`: `ClosedNavigation_CommitsSelectableRows_SkipsLabelsAndStopsAtBoundaries`.
- `BreadcrumbSelectorMessagesTests`: `ViewMessage_RoundTripsModeOpenAndStableIdentities`.
- `BreadcrumbStateModelSelectorTests`: `AddScoredFallbackRow_RetainsIdentityTextAndSuppliedProbability`.
- `BreadcrumbRenderProjectionSelectorTests`: `ProjectCollapsed_ReturnsExactlyCommittedSelectedDataRow`.
- `FolderBreadcrumbBridgeRouter*`: `Route_AffordanceToggleExpand_QueriesProviderAndReturnsRenderPlusResponse` and `SetSuggestionsAsync_WhileUpgradeInFlight_RowCountNeverDropsBelowPreUpgradeCount`.
- `BreadcrumbBridgeCoordinatorProbabilityTests`: `SetSuggestions_ImmediatelyPostsScoredFallbackBeforeProviderCompletes`.
- `BreadcrumbSelectorCoordinatorTests`: `ClosedDown_CommitsNextSelectableAndRaisesOneSelection` and `SelectorView_ContainsRowAlignedStableIdentityAndSelectabilityOptions`.
- `BreadcrumbMessengerHubTests`: `SelectorView_IsSpecializedForClosedAndExpandedSurfaceModes`.
- `BreadcrumbPopupPlacementTests`: `Calculate_BelowInsufficientAndFullHeightFitsAbove_UsesAbove`.
- `BreadcrumbDropDownHostTests`: `OpenAsync_CreatesToolStripControlHostAndUsesCalculatedScreenPlacement`.
- `BreadcrumbDropDownLifecycleTests`: `OpenAndClose_TransferFocusIntoPendingOptionAndBackToAnchor`.
- `BreadcrumbDropDownIntegrationTests`: `ClosedSurfaceToggleMessage_OpensHostExactlyOnce`.
- `ItemViewerBreadcrumbDropDownContractTests`: `ExistingAnchor_RemainsTheDesignerWebViewClosedSurface`.
- `QfcItemControllerBreadcrumbDropDownTests`: `ConfigureBreadcrumbDropDown_PassesExistingEnvironmentAndDarkThemeLazily`.
- `FolderBreadcrumbAssetContractTests`: `Percentage_UsesVisibleHostSuppliedPercentTextWithoutRecomputation` and `ExpandedRows_ExposeListboxOptionsAndOneActiveSelectedOption`.

AC-16 behavior-family proof is present for selection sessions, probability fallbacks, issue #398 concurrency, bridge serialization/routing, popup placement geometry, HTML/accessibility/theme contracts, popup ownership/focus, and lifecycle/reuse.
