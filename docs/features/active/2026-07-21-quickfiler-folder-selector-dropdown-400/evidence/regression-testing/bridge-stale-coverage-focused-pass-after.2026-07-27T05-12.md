# Bridge stale-lease focused pass

- Timestamp (UTC): 2026-07-27T05:12Z
- Task: P8-T72
- Command: `vstest.console.exe QuickFiler.Test\\bin\\Debug\\QuickFiler.Test.dll /InIsolation /Tests:QuickFiler.Test.Viewers.BreadcrumbCoordinatorLifecycleTests.PostRenderAndSelectorAsync_StaleLeaseReturnsCompletedWithoutPublishing /Logger:console;verbosity=detailed`
- Result: `EXIT_CODE=0`; 1 discovered, 1 passed, 0 failed, 0 skipped.
- Behavior proof: the sole existing test directly obtains and invalidates the coordinator upgrade lease, invokes the private stale-lease method by reflection, verifies the returned task is completed, and verifies `TrackingMessenger` has no publication. It introduces no new test method, filter, timing mechanism, retry, delay, or sleep.
