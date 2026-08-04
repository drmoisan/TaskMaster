# P5 collapsed-readiness nine-class pass-after

Timestamp: `2026-07-22T08:25:00Z`

Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation "/TestCaseFilter:FullyQualifiedName~BreadcrumbUiThreadDispatchTests|FullyQualifiedName~BreadcrumbSelectorToggleUiBoundaryTests|FullyQualifiedName~BreadcrumbPopupControlDispatchTests|FullyQualifiedName~BreadcrumbSelectorOpenRetryTests|FullyQualifiedName~BreadcrumbDropDownReadinessTests|FullyQualifiedName~BreadcrumbCollapsedSurfaceReadinessTests|FullyQualifiedName~BreadcrumbDropDownCoverageThresholdTests|FullyQualifiedName~BreadcrumbDuplicateIdentityIntegrationTests|FullyQualifiedName~BreadcrumbBridgeCoordinatorProbabilityTests" '/Logger:console;Verbosity=detailed'`

EXIT_CODE: `0`

Output Summary: `PASS. VSTest discovered all nine selected classes and exactly 70 cases. All 70 passed; zero failed and zero skipped. Both Viewer-integration tests synchronously drained the captured queue on the creator thread, observed no captured callback exception, and restored the prior synchronization context on that thread.`

## VSTest result

- VSTest version: `18.8.0 (x64)`
- Test assembly count: `1`
- Selected classes: `9`
- Discovered cases: `70`
- Passed: `70`
- Failed: `0`
- Skipped: `0`
- Total time: `2.7039 seconds`

| Selected class | Discovered | Passed | Failed | Skipped |
| --- | ---: | ---: | ---: | ---: |
| `BreadcrumbUiThreadDispatchTests` | 8 | 8 | 0 | 0 |
| `BreadcrumbSelectorToggleUiBoundaryTests` | 4 | 4 | 0 | 0 |
| `BreadcrumbPopupControlDispatchTests` | 13 | 13 | 0 | 0 |
| `BreadcrumbSelectorOpenRetryTests` | 8 | 8 | 0 | 0 |
| `BreadcrumbDropDownReadinessTests` | 12 | 12 | 0 | 0 |
| `BreadcrumbCollapsedSurfaceReadinessTests` | 10 | 10 | 0 | 0 |
| `BreadcrumbDropDownCoverageThresholdTests` | 7 | 7 | 0 | 0 |
| `BreadcrumbDuplicateIdentityIntegrationTests` | 4 | 4 | 0 | 0 |
| `BreadcrumbBridgeCoordinatorProbabilityTests` | 4 | 4 | 0 | 0 |

## Corrected Viewer-integration observations

The two corrected tests passed:

- `ViewerAttachment_PendingCachesAndReplaysCurrentStateExactlyOnce`
- `ViewerAttachment_FailureResetReuseAndDisposalLeaveNoStaleAttachment`

Each test performs every attachment completion through `CapturingSynchronizationContext.DrainUntil` before reading the task result. The shared assertion requires a nonempty executed-thread snapshot, requires every callback thread ID to equal `CreatorThreadId`, and requires `ExceptionSnapshot` to remain empty. `ViewerIntegrationHarness.Dispose` rejects non-creator cleanup and restores the prior context in `finally`; both tests additionally assert the prior context after the harness is disposed. Their passing results therefore verify creator-thread draining, empty captured exceptions, and same-thread restoration rather than inferring those properties from timing.

All ten existing `BreadcrumbCollapsedSurfaceReadinessTests` names remain present. The authorized file remains CSharpier-stable at 489 physical lines with SHA-256 `B53E9E091C461F835D900A8FB5DE0DB6B02080645EDFE18ACD0541E6E95E68F6`.

The initial P5-T76 launcher-resolution attempt started no test process. The planned P5-T73 through P5-T75 sequence was rerun successfully before this authoritative P5-T76 test execution.
