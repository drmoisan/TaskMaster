# Coordinator lifetime regression pass

Timestamp: `2026-07-22T21:12:00-04:00`

Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbCoordinatorLifecycleTests|FullyQualifiedName~BreadcrumbCollapsedSurfaceReadinessTests|FullyQualifiedName~BreadcrumbMessengerHubTests" /Logger:"console;Verbosity=normal"`

Resolved VSTest: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`

Result: PASS, exit code `0`. Exactly 32 tests were discovered and all 32 passed in 2.5811 seconds, with no failures or skips. The passing set includes overlapping populations, current cancellation propagation, clear/reset/reuse/disposal invalidation, exact handler removal, deferred cancellation-source disposal, queued completion after disposal, one current update, and no stale hub publication.
