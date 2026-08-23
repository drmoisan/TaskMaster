# P5 Hub and Attachment Coverage Pass-After

Timestamp: 2026-07-22T11:32:02Z

Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation '/TestCaseFilter:FullyQualifiedName~BreadcrumbMessengerHubCoverageTests|FullyQualifiedName~BreadcrumbMessengerHubTests' '/Logger:console;Verbosity=detailed'`

EXIT_CODE: 0

Output Summary: PASS. VSTest 18.8.0 discovered exactly 22 cases: ten `BreadcrumbMessengerHubCoverageTests` cases and 12 existing `BreadcrumbMessengerHubTests` cases. All 22 passed, with 0 failed and 0 skipped, in 1.1776 seconds.

## Every test result

### BreadcrumbMessengerHubTests — 12 passed

- PASS `PostJson_BroadcastsOneLogicalRenderAndThemeOncePerSurface`
- PASS `SelectorView_IsSpecializedForClosedAndExpandedSurfaceModes`
- PASS `InboundMessage_FromEitherSurface_IsRoutedOnce`
- PASS `AttachDetachReattach_IsIdempotentAndDoesNotDuplicateSubscriptions`
- PASS `PublicOperations_RejectNullArgumentsAndUseAfterDispose`
- PASS `Attach_WithDifferentMode_PreservesOriginalModeWithoutReplayOrSecondSubscription`
- PASS `Attach_AfterPendingUpdates_ReplaysOnlyCurrentStateOncePerSurface`
- PASS `Attach_ReplayFailureRollsBackSubscriptionAndAllowsRetry`
- PASS `CollapsedAttachment_ReplayFailureAndDisposeDetachBeforeMessengerCleanup`
- PASS `DetachAndDispose_HandleUnknownSurfacesAndStaleCallbacksSafely`
- PASS `PostJson_PreservesUnknownAndMalformedSelectorMessagesVerbatim`
- PASS `SelectorView_WithEscapedModeProperty_IsParsedAndPreservedVerbatim`

### BreadcrumbMessengerHubCoverageTests — 10 passed

- PASS `Hub_NullDuplicateAndDisposedOperations_FollowExactContracts`
- PASS `Hub_SubscribeAndUnsubscribeFailures_RollBackWithoutStaleInbound`
- PASS `Hub_CachedAndNoncachedPosts_ReplayOnlyLatestStateInSequence`
- PASS `Hub_MalformedMissingAndSameModeMessages_AreParsedWithoutMutation`
- PASS `Hub_InvalidUnknownAndStaleInboundSenders_AreIgnoredExactly`
- PASS `Attachment_ConstructorFactoryAndCandidateGuards_AllowRetry`
- PASS `Attachment_SharedPendingAndReadyBypass_ReuseOneCandidate`
- PASS `Attachment_StaleFactoryCandidateAndReadyReset_CleanExactlyOnce`
- PASS `Attachment_ControllerAndHubFailures_ResetAndPermitRetry`
- PASS `Attachment_PendingDisposeIsIdempotentAndBlocksLaterAttach`
