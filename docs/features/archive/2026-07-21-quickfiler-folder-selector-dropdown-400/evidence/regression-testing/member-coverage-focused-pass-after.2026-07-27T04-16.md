# Focused member-coverage pass after remediation

- Timestamp (UTC): 2026-07-27T04:16Z
- Task: P8-T65
- Command: `vstest.console.exe QuickFiler.Test\\bin\\Debug\\QuickFiler.Test.dll /InIsolation /Tests:QuickFiler.Test.Viewers.BreadcrumbCoordinatorUpgradeLifetimeTests.ArgumentGuards_NullInputsThrowArgumentNullException,QuickFiler.Test.Viewers.BreadcrumbCoordinatorUpgradeLifetimeTests.RunSynchronous_FailureAbandonsLinkedLeaseAndReportsCancellationFailure,QuickFiler.Test.Viewers.BreadcrumbCoordinatorUpgradeLifetimeTests.RunAsync_SupersededCancellationIsSwallowedAndSettled,QuickFiler.Test.Viewers.BreadcrumbCoordinatorUpgradeLifetimeTests.Disposal_RepeatedLifetimeDisposeIsSafeAndLeaseDisposeFailureIsReported,QuickFiler.Test.Viewers.BreadcrumbDropDownOpenCoordinatorTests.Reset_HostAlreadyClosedWithOpenSelector_CancelsExactlyOnce,QuickFiler.Test.Viewers.BreadcrumbDropDownOpenCoordinatorTests.SetDroppedDown_CloseThrows_ReportsOnceAndAllowsRetry,QuickFiler.Test.Viewers.BreadcrumbCoordinatorLifecycleTests.PostRenderAndSelectorAsync_StaleLeaseReturnsCompletedWithoutPublishing /Logger:console;verbosity=detailed`
- Result: `EXIT_CODE=0`; 7 discovered, 7 passed, 0 failed, 0 skipped.

The P8-T59 lifecycle coverage adjustment introduces no additional test method. Its existing lifecycle test is verified by P8-T66's all-eight test-assembly pass.
