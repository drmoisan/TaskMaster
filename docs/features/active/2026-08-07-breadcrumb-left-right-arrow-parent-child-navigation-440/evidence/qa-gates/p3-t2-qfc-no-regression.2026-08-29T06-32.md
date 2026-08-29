# Phase 3 — Qfc No-Regression Run (issue #440, plan task P3-T2)

Timestamp: 2026-08-29T06-32

Command:

```
& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~BreadcrumbStateModelTests|FullyQualifiedName~FolderBreadcrumbBridgeRouterTests|FullyQualifiedName~FolderBreadcrumbBridgeRouterEdgeTests|FullyQualifiedName~BreadcrumbStateModelSelectorTests|FullyQualifiedName~BreadcrumbSelectionSessionTests|FullyQualifiedName~BreadcrumbRowStateTests" "/Logger:trx;LogFileName=p3-t2.trx" "/ResultsDirectory:coverage\trx\p3-t2"
```

EXIT_CODE: 0

## Output Summary

```
Test Run Successful.
Total tests: 117
     Passed: 117
```

- `Total tests:` equals `Passed:` (117 = 117).
- 117 is greater than the gate floor of 60.
- The filter runs after the fix, so it legitimately includes the two tests added by
  P1-T1 and P1-T2.

## The eight named results, read from the TRX

Read from `coverage\trx\p3-t2\p3-t2.trx` by matching each `UnitTestResult` element's
`testName` attribute exactly and reading its `outcome` attribute. The TRX carries 117
`UnitTestResult` elements, consistent with the console total.

| Test name | Outcome |
| --- | --- |
| `LeftArrow_WithSubfolderSelected_ResetsSubfolderSelectionAndCollapses` | Passed |
| `Arrows_WithNoSelection_AreUnhandled` | Passed |
| `LeftArrow_QfcMultiSegmentRow_SelectsParentNode` | Passed |
| `ArrowKey_QfcSingleSegmentRow_TakesPreExistingCollapsePath` | Passed |
| `RightArrow_QfcSelectedParentNode_ExpandsIntoChildren` | Passed |
| `Route_RightArrow_NothingToExpand_ReportsUnhandledRight` | Passed |
| `Arrows_RightExpandsThenLeftCollapses_UnhandledWhenNothingChanges` | Passed |
| `Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft` | Passed |

No name was reported MISSING, so the filter reached every class involved and no rerun
with a corrected filter was required.

## Interpretation against the acceptance criteria

- The first entry is the test AC-3 requires to pass **without modification**. It is
  unmodified by this change and it passed.
- The sixth entry is the test AC-5 requires to pass **unmodified**. It is unmodified
  by this change and it passed.
- The seventh and eighth entries are the two tests P2-T3 and P2-T4 corrected. Their
  `Passed` outcomes are the evidence that those corrections encode the walk contract
  rather than merely deleting the old comment: each now drives the chain to the root
  before asserting the unhandled press, and each would fail if the production guard
  still imposed the one-step limit.
