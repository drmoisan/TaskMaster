# Popup UI-boundary composition regression gate

Timestamp: `2026-07-22T07:59Z`

VSTest path: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`

Assembly: `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`

Command: `& $vstestPath $assembly /InIsolation "/TestCaseFilter:$filter" '/Logger:console;Verbosity=detailed'`

Exact filter: `FullyQualifiedName~BreadcrumbUiThreadDispatchTests|FullyQualifiedName~BreadcrumbSelectorToggleUiBoundaryTests|FullyQualifiedName~BreadcrumbPopupControlDispatchTests|FullyQualifiedName~BreadcrumbSelectorOpenRetryTests|FullyQualifiedName~BreadcrumbDropDownReadinessTests|FullyQualifiedName~BreadcrumbCollapsedSurfaceReadinessTests|FullyQualifiedName~BreadcrumbDropDownCoverageThresholdTests|FullyQualifiedName~BreadcrumbDuplicateIdentityIntegrationTests|FullyQualifiedName~BreadcrumbBridgeCoordinatorProbabilityTests`

Exit code: `0`

Result: VSTest 18.8.0 discovered and passed all 70 selected cases in 2.7356 seconds with 70 passed, 0 failed, and 0 skipped.

| Selected class | Cases |
| --- | ---: |
| `BreadcrumbUiThreadDispatchTests` | 9 |
| `BreadcrumbSelectorToggleUiBoundaryTests` | 4 |
| `BreadcrumbPopupControlDispatchTests` | 13 |
| `BreadcrumbSelectorOpenRetryTests` | 8 |
| `BreadcrumbDropDownReadinessTests` | 12 |
| `BreadcrumbCollapsedSurfaceReadinessTests` | 10 |
| `BreadcrumbDropDownCoverageThresholdTests` | 7 |
| `BreadcrumbDuplicateIdentityIntegrationTests` | 4 |
| `BreadcrumbBridgeCoordinatorProbabilityTests` | 3 |

The passing set includes current deterministic proofs for creator-thread dispatch, popup control scheduling and cleanup, mouse and keyboard retry equivalence, stale-generation invalidation, readiness gating and reuse, duplicate-row occurrence identity, probability preservation, one-pass primary-preserving rollback, exact native-close/focus recovery counts, reset and Dispose races, no wrapper/direct double disposal, and fresh retry after failure. The separate `popup-ui-boundary-composition-test-hang-diagnostic.2026-07-22T07-57.md` remains nonpassing historical evidence and is not substituted for this gate.
