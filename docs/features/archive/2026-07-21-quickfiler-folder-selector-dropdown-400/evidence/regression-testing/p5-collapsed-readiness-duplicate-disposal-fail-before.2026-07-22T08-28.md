# P5 collapsed-readiness duplicate-disposal failure

Timestamp: `2026-07-22T08:28:00Z`

Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation "/TestCaseFilter:FullyQualifiedName~BreadcrumbUiThreadDispatchTests|FullyQualifiedName~BreadcrumbSelectorToggleUiBoundaryTests|FullyQualifiedName~BreadcrumbPopupControlDispatchTests|FullyQualifiedName~BreadcrumbSelectorOpenRetryTests|FullyQualifiedName~BreadcrumbDropDownReadinessTests|FullyQualifiedName~BreadcrumbCollapsedSurfaceReadinessTests|FullyQualifiedName~BreadcrumbDropDownCoverageThresholdTests|FullyQualifiedName~BreadcrumbDuplicateIdentityIntegrationTests|FullyQualifiedName~BreadcrumbBridgeCoordinatorProbabilityTests" '/Logger:console;Verbosity=detailed'`

EXIT_CODE: `1`

Output Summary: `FAIL (production ownership defect exposed). VSTest discovered exactly 70 cases: 69 passed, 1 failed, and 0 skipped. ViewerAttachment_FailureResetReuseAndDisposalLeaveNoStaleAttachment observed resetSurface.DisposeCount == 2 where the retained assertion requires exactly one disposal. The test-only batch cannot correct the overlapping production ownership.`

## Exact failure

- Failed test: `BreadcrumbCollapsedSurfaceReadinessTests.ViewerAttachment_FailureResetReuseAndDisposalLeaveNoStaleAttachment`
- Assertion: `resetSurface.DisposeCount.Should().Be(1)`
- Observed count: `2`
- Source: `QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs`, physical line `258`
- Total cases: `70`
- Passed: `69`
- Failed: `1`
- Skipped: `0`
- Total time: `2.6921 seconds`

The earlier saturating tracker incremented only when `DisposeCount` was zero, so any later disposal remained reported as one. Independent review required it to increment on every call while retaining all existing `Be(1)` expectations. This strengthened observation is valid and must not be weakened.

## Read-only ownership trace

The reset path has two production disposal owners for the same pending messenger:

1. `BreadcrumbCollapsedAttachment.Release` calls `_controller.Reset()` at `BreadcrumbMessengerHub.cs:421`.
2. The same method directly calls `(pending as IDisposable)?.Dispose()` at `BreadcrumbMessengerHub.cs:423`.
3. The controller's invalidated `CompleteAttachmentAsync` reaches `RejectPending` at `BreadcrumbCollapsedSurfaceController.cs:214`.
4. `RejectPending` concludes that the messenger is no longer pending or ready and calls `SafeDispose(messenger as IDisposable)` at `BreadcrumbCollapsedSurfaceController.cs:237`.

The two production paths therefore dispose the same reset candidate twice. The same ownership overlap can affect other invalidated pending completions and was hidden by the previous tracker.

## Scope and decision

The authorized P5-T71 batch permits zero production files. The sole test-file correction is functioning as intended and the existing exact-once assertion must remain. No in-scope test change can make this run pass without masking the production defect.

P5-T76 remains unchecked. P5-T77 and P5-T78 were not run. P5-T67 and P5-T68 remain unchecked. A plan revision must authorize a bounded production ownership correction and then restart the ordered sequence at P5-T73.
