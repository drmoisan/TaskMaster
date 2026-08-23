# Duplicate identity and probability current-tree pass

Timestamp: 2026-07-22T02:37:22.3298801Z

Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' 'UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll' 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation '/TestCaseFilter:FullyQualifiedName~BreadcrumbDuplicateIdentityTests|FullyQualifiedName~BreadcrumbDuplicateIdentityIntegrationTests|FullyQualifiedName~FolderBreadcrumbAssetContractTests|FullyQualifiedName~FolderBreadcrumbRouterSelectionConcurrencyTests' '/Logger:console;Verbosity=normal'`

EXIT_CODE: 0

Output Summary: The exact current-tree P2 identity-surface filter passed 30 of 30 tests with zero failures and zero skips in 2.3413 seconds. The 13 UtilitiesCS duplicate/concurrency tests and 17 QuickFiler integration/asset tests all passed after the two QuickFiler harnesses injected the explicit owner-thread-only test dispatcher. Production coordinator construction still requires an owning synchronization context. This artifact supersedes the pre-P3 30/30 evidence that no longer represented the live dispatcher contract.

Current named proof includes:

- `CollapsedReadback_SecondDuplicateSuggestionRetainsItsProbability`
- `OpenCommit_CollapsedReadbackUsesSecondDuplicateSuggestionProbability`
- `Percentage_UsesVisibleHostSuppliedPercentTextWithoutRecomputation`
- `ExpandedDuplicatePathState_YieldsExactlyOneActiveAriaSelectedOption`
- `ClosedDown_DuplicateSuggestionAndRecentCommitsRecentOccurrence`
- `OpenDownThenEnter_DuplicateSuggestionAndRecentCommitsPendingOccurrence`
- `ActivateSelector_SecondPublishedIdentityCommitsExactDuplicateOccurrence`
