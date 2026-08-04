# Preserved Breadcrumb Contracts Nonpassing Gate

Timestamp: 2026-07-23T02:32:04.9690119Z

Command: `$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'; $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; & $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Tests:BreadcrumbLeftAndRightTransitions_DoNotMutateSelectorSession,ExistingLeftAndRightMessages_StillForwardOnce,OpenDown_ChangesPendingOnlyThenEnterCommitsAndCloses,EscapeAndUncommittedClose_RestoreOpeningSelectionWithoutNotification,MouseActivation_CommitsStableIdentityExactlyOnce,SetSuggestions_ImmediatelyPostsScoredFallbackBeforeProviderCompletes,SetSuggestions_SuccessfulUpgradeRetainsScoreAndLatestSelection,SetSuggestions_UnresolvedEmptyAndFailureRetainFallbackProbability,ReplaceRows_PreservesSelectionWhenIndexStillValid,ReplaceRows_ClearsSelectionWhenIndexBeyondNewCount,SetSuggestionsAsync_WhileUpgradeInFlight_RowCountNeverDropsBelowPreUpgradeCount,SetSuggestionsAsync_WhileUpgradeInFlight_ReadbackStaysConsistentAndSelectionSurvives,SelectRow_WhileSuggestionsUpgradeInFlight_DoesNotThrowAndAppliesSelection,InvalidSelectorMessage_IsNoOpAndDoesNotRaiseTransitions,Parse_MalformedUnknownAndBlankOptionalPayloads_RejectsExplicitly,MalformedInboundMessage_PostsRouterErrorResponse,Route_MalformedInboundJson_ReturnsErrorResponse,NativeClosedEvent_CancelsOnceAndIgnoresLaterCloseNotifications,ClosedNavigation_CommitsSelectableRows_SkipsLabelsAndStopsAtBoundaries,OpenNavigation_ChangesPendingWithoutChangingCommittedOrModelSelection,ClosedDown_CommitsNextSelectableAndRaisesOneSelection,BoundaryAndInvalidOperations_AreDeterministicNoOps,InboundValidSelectorMessages_RouteToggleKeyAndActivationBranches /Logger:'console;Verbosity=normal'`

EXIT_CODE: 1

Output Summary: The exact 23-test preserved filter discovered all 23 cases; 20 passed and 3 failed. Each failure reproduced in class or method isolation and is outside the authorized P7 batch-B edit tuple.

## Deterministic failures

- `SetSuggestionsAsync_WhileUpgradeInFlight_ReadbackStaysConsistentAndSelectionSurvives` expected selected index 1 but observed -1 after the two-row rebuild. The router's `ReplaceRowsPreservingSession` calls `BreadcrumbSelectionSession.ReconcileRowsReplaced`; that method resolves the prior plain-row committed identity against new suggestion identities, cannot find it, and clears the model selection. This direct router path cannot be corrected in either authorized P7-B production surface.
- `MalformedInboundMessage_PostsRouterErrorResponse` throws because its existing harness constructs the public `BreadcrumbBridgeCoordinator` without an owning UI synchronization context.
- `SelectRow_WhileSuggestionsUpgradeInFlight_DoesNotThrowAndAppliesSelection` throws at the same public-constructor boundary for the same reason.

The two coordinator failures predate the P7-B route and require a deterministic owning-context test harness or a separately authorized production-contract decision. Changing `BreadcrumbBridgeCoordinator.CaptureProductionDispatcher` to permit context-free production construction would weaken the existing UI-affinity boundary and is not authorized. No failing assertion was changed, skipped, or excluded.
