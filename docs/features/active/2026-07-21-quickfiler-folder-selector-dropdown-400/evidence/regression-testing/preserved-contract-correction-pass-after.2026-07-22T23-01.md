# Preserved Contract Correction Pass-After

Timestamp: 2026-07-22T23-01

Command: `$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'; $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; & $vstest 'UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll' 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation /TestCaseFilter:"FullyQualifiedName~FolderBreadcrumbBridgeRouterTests|FullyQualifiedName~BreadcrumbBridgeCoordinatorTests" /Logger:'console;Verbosity=normal'`

EXIT_CODE: 0

Output Summary: VSTest 18.8.0 discovered exactly 38 tests across the 2 assemblies. All 38 passed with 0 failures and 0 skips. The first post-change run found one stale raw-path identity assertion in the authorized in-flight test file; that assertion was corrected to the existing source-qualified identity contract, and the full P7-T19 through P7-T22 gate sequence was restarted before this passing run.

## Source-partial reconciliation

The 10 unchanged companion-partial cases below map to `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs`:

- `Route_AffordanceToggleExpand_QueriesProviderAndReturnsRenderPlusResponse`
- `Route_SegmentDoubleClick_ProducesCollapsedRenderPayload`
- `Route_RightArrow_ExpandsWhenExpandable`
- `Route_SelectionChange_UpdatesModelAndAcksSelection`
- `Route_ProviderException_SurfacesExplicitErrorResponseAndRevertsExpansion`
- `Route_MalformedInboundJson_ReturnsErrorResponse`
- `Route_OutOfRangeRowIndex_ReturnsErrorResponse`
- `Route_RightArrow_NothingToExpand_ReportsUnhandledRight`
- `Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft`
- `Route_ThemeChange_EchoesThemeAndReRenders`

The 12 router in-flight cases below map to the authorized `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterInFlightTests.cs` partial:

- `Sequence_ExpandCollapseViaMessages_TransitionsDeterministically`
- `SetSuggestions_UnresolvablePath_FallsBackToPlainRowPreservingThePath`
- `SetItemsAndAddItems_NullInput_ThrowExplicitly`
- `Constructor_NullProvider_Throws`
- `SetItems_PlainRows_RenderVerbatimIncludingTrashToDelete`
- `SetSuggestionsAsync_NonScoredRow_BecomesPlainVerbatimRow`
- `SetSuggestionsAsync_WhileUpgradeInFlight_RowCountNeverDropsBelowPreUpgradeCount`
- `SetSuggestionsAsync_WhileUpgradeInFlight_ReadbackStaysConsistentAndSelectionSurvives`
- `SetSuggestionFallbacks_IdentityMigration_RebasesOriginalAndPreservesDistinctPending`
- `SetSuggestionFallbacks_OutOfRangeRetainedIndex_DoesNotFallback`
- `SetSuggestionFallbacks_NonselectableRetainedIndex_DoesNotFallback`
- `SetSuggestionsAsync_OlderCompletionCannotOverwriteNewerGeneration`

The 16 coordinator cases below map to the authorized `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs`:

- `InboundDoubleClick_PostsCollapsedRenderJson`
- `InboundExpand_PostsRenderAndSubfolderResponse`
- `InboundSelectionMessage_RaisesSelectionChangedWithMappedPath`
- `MalformedInboundMessage_PostsRouterErrorResponse`
- `ProviderFailure_SurfacesExplicitErrorResponse`
- `UnhandledRightArrow_RaisesUnhandledArrowRight`
- `UnhandledLeftArrow_RaisesUnhandledArrowLeft`
- `ArrowMessages_RaiseSyntheticFolderKeyDown`
- `Clear_EmptiesSelectionStateAndPostsEmptyRender`
- `AddItems_AppendsPlainRowsAndContainsFindsThem`
- `SetSuggestions_SyncFacade_PopulatesImmediatelyThenUpgradesPreservingSelection`
- `SetSuggestions_NullRows_Throws`
- `SelectItem_KnownItemSelects_UnknownItemIsNoOp`
- `SetTheme_PostsThemeChangeMessage`
- `Constructor_NullArguments_Throw`
- `SelectRow_WhileSuggestionsUpgradeInFlight_DoesNotThrowAndAppliesSelection`

## Regression and scope proof

- The three P7-T15 regressions passed: `SetSuggestionsAsync_WhileUpgradeInFlight_ReadbackStaysConsistentAndSelectionSurvives`, `MalformedInboundMessage_PostsRouterErrorResponse`, and `SelectRow_WhileSuggestionsUpgradeInFlight_DoesNotThrowAndAppliesSelection`.
- The new bounded identity-migration cases passed, including open-session original rebasing with distinct pending preservation and rejection of out-of-range and nonselectable retained-index fallbacks.
- `MalformedInboundMessage_PostsRouterErrorResponse` and `SelectRow_WhileSuggestionsUpgradeInFlight_DoesNotThrowAndAppliesSelection` each capture `SynchronizationContext.Current`, construct through the public coordinator constructor inside the inline context scope, and assert the exact prior context reference was restored. Both tests passed.
- The unchanged companion partial pre/post SHA-256 is `BD5A23947343EE40E9CDC7C66E6E635922EEC779FFC29DC96A0BAA77C30C2BE9`.
- P7 batch-B pre/post SHA-256 values are unchanged:
  - `QuickFiler/Resources/FolderBreadcrumb.html`: `11C7C8B0F1D349FA66BA37E1A50D3C5BD98FDF56E2F4AAF093D0B0B56862B2DD`
  - `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`: `5077FCD19CF8471DAC48D25C579305FC06994B5F909BEB0DC9A973DAD1337A36`
  - `QuickFiler.Test/Viewers/BreadcrumbSubfolderActivationTests.cs`: `BE49A0264312490EDC96386969B174F052251A8363EA990D656143B9901EA687`
  - `QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs`: `3B8BD34198FD77A137586BBBC59EC397306AEE34131BA09D59E9A142C8C3E57E`
  - `QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs`: `A65B2CC6099B3F88F1890A327B9F42B461CA469D6ED351E8D260A0EBF072C825`
- Final scoped `git diff --check` reported no whitespace errors.
