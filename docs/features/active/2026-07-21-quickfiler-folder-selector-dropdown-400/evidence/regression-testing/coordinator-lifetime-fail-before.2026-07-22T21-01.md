# Coordinator lifetime failure-first baseline

Timestamp: `2026-07-22T21:01:02.0457498-04:00`

Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation '/TestCaseFilter:FullyQualifiedName~BreadcrumbCoordinatorLifecycleTests|FullyQualifiedName~BreadcrumbCollapsedSurfaceReadinessTests|FullyQualifiedName~BreadcrumbMessengerHubTests' '/Logger:console;Verbosity=detailed'`

EXIT_CODE: `1`

Output Summary: `EXPECTED FAILURE-FIRST RESULT. Exactly 32 cases were discovered: 27 passed, 5 failed, and 0 skipped. The five failures reproduce the missing coordinator-owned lifetime boundaries in the externally continued partial implementation.`

## Intended failures

1. `CurrentProviderCancellation_PropagatesWithoutPublishingAnUpgrade` — the partial implementation swallowed an `OperationCanceledException` from the current provider rather than propagating it.
2. `DisposedCoordinator_RejectsPopulationAndClearRemainsSafe` — `Clear` still published an empty render after coordinator disposal.
3. `AsyncPopulation_SupersededCompletionDoesNotPublishAgain` — the public async population did not receive or cancel a coordinator-owned token.
4. `AddItems_InvalidatesLateUpgradeBeforeDuplicatePost` — `AddItems` did not cancel the earlier suggestion lease.
5. `QueuedCompletion_DisposedBeforeOwnerDrain_DoesNotPublish` — disposal after the outer currency check but before owner-context drain still allowed the queued render to post.

No production source was changed between adding these assertions and this run. The preceding diagnostic build completed successfully with only the repository's recorded package warnings.
