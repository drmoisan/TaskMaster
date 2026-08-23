# P9-T14 nonnumeric adapter focused failure

Timestamp: 2026-07-27T08-04
Command: Start-Process resolved vstest.console.exe against QuickFiler.Test/bin/Debug/QuickFiler.Test.dll with /InIsolation, detailed console, canonical TRX, and the six-class fully qualified-name filter recorded below.
EXIT_CODE: 1

## Resolved runner and selection

VSTest: C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe.
Assembly: QuickFiler.Test/bin/Debug/QuickFiler.Test.dll.
Runner PID: 276844.
Filter classes:

- QuickFiler.Test.Viewers.BreadcrumbItemViewerLifecycleCoordinatorTests
- QuickFiler.Test.Viewers.BreadcrumbPopupUiOperationsDirectAdapterTests
- QuickFiler.Test.Viewers.ItemViewerBreadcrumbDropDownContractTests
- QuickFiler.Test.Viewers.BreadcrumbPopupBoundaryCoverageTests
- QuickFiler.Test.Viewers.BreadcrumbDropDownIntegrationTests
- QuickFiler.Test.Viewers.BreadcrumbDropDownLifecycleCoverageTests

Canonical TRX: evidence/regression-testing/nonnumeric-adapter-focused.2026-07-27T08-04.trx.

## Result

Total: 60. Passed: 59. Failed: 1. Skipped or other: 0.

The ten required new test identities were each discovered exactly once. Nine passed. HostReplacement_SubscriptionsAndMessengerReplacementPreserveOrder failed. Its assertion expected event operations [add, remove], but the actual sequence was [remove, add, remove].

## Process-tree cleanup verification

After the runner exited, PID 276844 did not exist, direct descendant count was 0, and live issue-400 QuickFiler.Test vstest.console.exe/testhost.exe process count was 0. No process termination was necessary.

Result: FAIL. This is the single P9-T14 focused attempt after the passing build precondition. The failure returns execution to P9-T12 for an in-place plan revision; no retry was run and P9-T15 did not begin.
