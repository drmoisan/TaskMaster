# Phase 3 — Efc No-Regression Run (issue #440, plan task P3-T3)

Timestamp: 2026-08-29T06-33

Command:

```
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~BreadcrumbBridgeRouterTests|FullyQualifiedName~BreadcrumbBridgeRouterQueueTests|FullyQualifiedName~BreadcrumbBridgeRouterIssue439Tests|FullyQualifiedName~BreadcrumbBridgeRouterIssue614Tests|FullyQualifiedName~FolderBreadcrumbAssetContractTests|FullyQualifiedName~BreadcrumbBridgeCoordinatorTests|FullyQualifiedName~BreadcrumbSelectorCoordinatorTests|FullyQualifiedName~QfcItemControllerBreadcrumbDropDownTests" "/Logger:trx;LogFileName=p3-t3.trx" "/ResultsDirectory:coverage\trx\p3-t3"
```

EXIT_CODE: 0

## Output Summary

```
Test Run Successful.
Total tests: 119
     Passed: 119
```

- `Total tests:` equals `Passed:` (119 = 119).
- 119 is greater than the gate floor of 40.

## The four router classes are each represented in the TRX

Counted by matching each `UnitTest` definition's `TestMethod/@className` against the
class name, so a filter that silently reached only one class would be visible here.

| Router class | Results present |
| --- | --- |
| `BreadcrumbBridgeRouterTests` | 24 |
| `BreadcrumbBridgeRouterQueueTests` | 26 |
| `BreadcrumbBridgeRouterIssue439Tests` | 10 |
| `BreadcrumbBridgeRouterIssue614Tests` | 8 |

All four are non-zero. The three extra alternates in the filter were required because
`FullyQualifiedName~BreadcrumbBridgeRouterTests` is a substring match that does not
reach the other three class names.

## The four named results, read from the TRX

| Test name | Outcome |
| --- | --- |
| `HandleArrowKey_RepeatedLeft_WalksToRootThenFallsThroughToExistingBehavior` | Passed |
| `Boundary_QfcUnhandledArrow_StillReachesBreadcrumbArrowFallThrough` | Passed |
| `LeftAndRightBreadcrumbMessages_RemainSupported` | Passed |
| `ExistingLeftAndRightMessages_StillForwardOnce` | Passed |

- The first confirms the Efc surface still walks to the root and then falls through,
  which is the contract this change deliberately leaves untouched.
- The second is the AC-4 fall-through boundary observed at the interface seam.
- The third is the Qfc HTML asset contract AC-9 requires to stay green.

The whole Efc breadcrumb router suite passes unmodified, which is the test half of
AC-9.
