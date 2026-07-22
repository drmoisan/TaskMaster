# Preserved Selector Contracts

Timestamp: 2026-07-21T20-15Z
Command: `$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'; $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; if (-not $vstest -or -not (Test-Path -LiteralPath $vstest)) { throw 'vstest.console.exe not found via vswhere.' }; & $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Tests:BreadcrumbLeftAndRightTransitions_DoNotMutateSelectorSession,ExistingLeftAndRightMessages_StillForwardOnce,OpenDown_ChangesPendingOnlyThenEnterCommitsAndCloses,EscapeAndUncommittedClose_RestoreOpeningSelectionWithoutNotification,MouseActivation_CommitsStableIdentityExactlyOnce,SetSuggestions_ImmediatelyPostsScoredFallbackBeforeProviderCompletes,SetSuggestions_SuccessfulUpgradeRetainsScoreAndLatestSelection,SetSuggestions_UnresolvedEmptyAndFailureRetainFallbackProbability,ReplaceRows_PreservesSelectionWhenIndexStillValid,ReplaceRows_ClearsSelectionWhenIndexBeyondNewCount,SetSuggestionsAsync_WhileUpgradeInFlight_RowCountNeverDropsBelowPreUpgradeCount,SetSuggestionsAsync_WhileUpgradeInFlight_ReadbackStaysConsistentAndSelectionSurvives,SelectRow_WhileSuggestionsUpgradeInFlight_DoesNotThrowAndAppliesSelection,InvalidSelectorMessage_IsNoOpAndDoesNotRaiseTransitions,Parse_MalformedUnknownAndBlankOptionalPayloads_RejectsExplicitly,MalformedInboundMessage_PostsRouterErrorResponse,Route_MalformedInboundJson_ReturnsErrorResponse,NativeClosedEvent_CancelsOnceAndIgnoresLaterCloseNotifications,ClosedNavigation_CommitsSelectableRows_SkipsLabelsAndStopsAtBoundaries,OpenNavigation_ChangesPendingWithoutChangingCommittedOrModelSelection,ClosedDown_CommitsNextSelectableAndRaisesOneSelection,BoundaryAndInvalidOperations_AreDeterministicNoOps,InboundValidSelectorMessages_RouteToggleKeyAndActivationBranches`
EXIT_CODE: 0
Output Summary: All 23 exact preserved-contract tests passed across `UtilitiesCS.Test.dll` and `QuickFiler.Test.dll`. The run directly exercised selector state, routing, coordinator behavior, native close, probability updates, and issue #398 atomicity; no HTML source-token assertion is used as readiness or DOM-focus proof.

## Results

- Resolved vstest: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`
- Matched assemblies: 2
- Total tests: 23
- Passed: 23
- Failed: 0
- Skipped: 0
- Test time: 1.6520 seconds

## Contract Mapping

- Left and Right compatibility:
  - `BreadcrumbLeftAndRightTransitions_DoNotMutateSelectorSession`
  - `ExistingLeftAndRightMessages_StillForwardOnce`
- Enter and Escape:
  - `OpenDown_ChangesPendingOnlyThenEnterCommitsAndCloses`
  - `EscapeAndUncommittedClose_RestoreOpeningSelectionWithoutNotification`
- Mouse activation:
  - `MouseActivation_CommitsStableIdentityExactlyOnce`
- Probability updates:
  - `SetSuggestions_ImmediatelyPostsScoredFallbackBeforeProviderCompletes`
  - `SetSuggestions_SuccessfulUpgradeRetainsScoreAndLatestSelection`
  - `SetSuggestions_UnresolvedEmptyAndFailureRetainFallbackProbability`
- Issue #398 atomicity:
  - `ReplaceRows_PreservesSelectionWhenIndexStillValid`
  - `ReplaceRows_ClearsSelectionWhenIndexBeyondNewCount`
  - `SetSuggestionsAsync_WhileUpgradeInFlight_RowCountNeverDropsBelowPreUpgradeCount`
  - `SetSuggestionsAsync_WhileUpgradeInFlight_ReadbackStaysConsistentAndSelectionSurvives`
  - `SelectRow_WhileSuggestionsUpgradeInFlight_DoesNotThrowAndAppliesSelection`
- Invalid-message handling:
  - `InvalidSelectorMessage_IsNoOpAndDoesNotRaiseTransitions`
  - `Parse_MalformedUnknownAndBlankOptionalPayloads_RejectsExplicitly`
  - `MalformedInboundMessage_PostsRouterErrorResponse`
  - `Route_MalformedInboundJson_ReturnsErrorResponse`
- Native close:
  - `NativeClosedEvent_CancelsOnceAndIgnoresLaterCloseNotifications`
- Selector navigation and boundaries:
  - `ClosedNavigation_CommitsSelectableRows_SkipsLabelsAndStopsAtBoundaries`
  - `OpenNavigation_ChangesPendingWithoutChangingCommittedOrModelSelection`
  - `ClosedDown_CommitsNextSelectableAndRaisesOneSelection`
  - `BoundaryAndInvalidOperations_AreDeterministicNoOps`
  - `InboundValidSelectorMessages_RouteToggleKeyAndActivationBranches`

No `FolderBreadcrumbAssetContractTests` or other HTML source-token test was invoked or cited for runtime readiness, focus, or navigation-completion behavior.
