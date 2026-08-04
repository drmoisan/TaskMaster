# P5 OpenCoordinator Line-Limit Split Pass-After

Timestamp: 2026-07-22T12:54:13Z

Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbDropDownOpenCoordinatorTests"`

EXIT_CODE: 0

Output Summary: PASS. Total tests: 10, Passed: 10, Failed: 0, Skipped: 0. The partial-class split preserved every original OpenCoordinator case; the 10 discovered cases match the pre-split file one-for-one.

Discovered case list (10):
1. ConstructorAndProviderUpdates_GuardEveryRequiredDelegate
2. RequestOpen_ConcurrentCallersShareOneUiBoundSnapshot
3. RequestOpen_SnapshotFailureCancelsOnceAndRetrySucceeds
4. RequestOpen_FalseResultCancelsOnceAndPermitsRetry
5. RequestOpen_SynchronousAndAsynchronousFaultsAreObserved
6. RequestOpen_HostSideCancellationBeforeFalseCompletionIsNotDuplicated
7. RequestOpen_SelectorClosesBeforeSuccess_ClosesLatePopupExplicitly
8. SetDroppedDown_MouseAndKeyboardPathsShareRequestAndCloseUncommitted
9. SelectorStateTransitions_RequestOpenThenCloseOnlyWhenRequired
10. ResetReleaseAndCloseResults_PreserveRetryAndBlockReleasedWork

Cases 1-5 reside in the primary partial `BreadcrumbDropDownOpenCoordinatorTests.cs`; cases 6-10 reside in the sibling partial `BreadcrumbDropDownOpenCoordinatorTests.Part2.cs`. Both share the `[TestClass] partial class BreadcrumbDropDownOpenCoordinatorTests` identity.
