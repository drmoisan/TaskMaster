# Phase 0 — Baseline Outcome of the Three Named Tests

Timestamp: 2026-08-26T08-36
Task: [P0-T13]

Source: the `[P0-T12]` TRX
`docs/features/active/qfc-item-controller-defects-484/evidence/baseline/trx-baseline/baseline-quickfiler-test.trx`.

Command: read of the `UnitTestResult` / `UnitTest` elements of that TRX for the three named tests.
EXIT_CODE: 0

## Result rows

| Test | Fully-qualified name | Outcome | Duration |
|---|---|---|---|
| `QfcItemController_FocusAndThemeTests.ToggleNavigation_Synchronous_TogglesPositionTips` | `QuickFiler.Controllers.Tests.QfcItemController_FocusAndThemeTests.ToggleNavigation_Synchronous_TogglesPositionTips` | **Passed** | 00:00:00.0724390 |
| `QfcItemController_ViewerSetupTests.Cleanup_NullsTrackedPrivateFields` | `QuickFiler.Controllers.Tests.QfcItemController_ViewerSetupTests.Cleanup_NullsTrackedPrivateFields` | **Passed** | 00:00:00.0005317 |
| `QfcItemControllerBreadcrumbDropDownTests.Cleanup_ResetsInjectedHostForPooledViewerReuse` | `QuickFiler.Controllers.Tests.QfcItemControllerBreadcrumbDropDownTests.Cleanup_ResetsInjectedHostForPooledViewerReuse` | **Passed** | 00:00:00.0913387 |

All three rows record outcome `Passed`.

## Why these three

- `ToggleNavigation_Synchronous_TogglesPositionTips` is the test whose assertion `[P1-T1]` tightens from
  `Times.AtLeastOnce()` to `Times.Once()`. Its baseline `Passed` outcome is what makes the `[P1-T2]`
  fail-before observation attributable to the tightening rather than to a pre-existing failure.
- `Cleanup_NullsTrackedPrivateFields` and `Cleanup_ResetsInjectedHostForPooledViewerReuse` both call
  `Cleanup()` with a null `_kbdHandler` and an `_itemViewer` that is not a concrete `ItemViewer`. Their
  baseline `Passed` outcome is what makes the `[P5-T8]` unguarded-state failure attributable to the
  missing guards, and their required `Passed` outcome at `[P5-T11]` is what proves the guards restored
  the contract.

Output Summary: All three named tests pass at the baseline.
