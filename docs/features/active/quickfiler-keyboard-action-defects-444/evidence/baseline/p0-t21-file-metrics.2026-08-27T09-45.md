# [P0-T21] Baseline file metrics

Timestamp: 2026-08-27T09-45
Command: `(Get-Content <path>).Count` for each path below, plus `Select-String -SimpleMatch -Pattern '[TestMethod]'` occurrence counts
EXIT_CODE: 0

`(Get-Content <path>).Count` is used, not `Measure-Object -Line`, which reports a different figure.

## Line counts

| Path | Baseline line count |
| --- | --- |
| `QuickFiler/Controllers/KbdActions.cs` | 146 |
| `QuickFiler/Controllers/QfcCollectionController.cs` | 2437 |
| `QuickFiler/Controllers/QfcItemController.Navigation.cs` | 228 |
| `QuickFiler.Test/Controllers/KbdActionsTests.cs` | 88 |
| `QuickFiler.Test/Controllers/KbdActionsRemainingBranchesTests.cs` | 181 |
| `QuickFiler.Test/Controllers/QfcItemController.NavigationTests.cs` | 391 |
| `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` | 500 |

## `[TestMethod]` occurrence counts

```
FrozenTestMethodCount             = 13    (QfcCollectionControllerTests.cs)
BaselineKbdActionsTestMethodCount = 13    (KbdActionsTests.cs 3 + KbdActionsRemainingBranchesTests.cs 10)
```

## Observations relevant to later gates

- `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` is **exactly 500 lines** with zero
  spare, and its `[TestMethod]` count is 13. `[P2-T11]` and `[P4-T15]` gate both figures against these
  values. No test is added to this file by this feature.
- `QuickFiler/Controllers/QfcCollectionController.cs` is **2437 lines**, far above the 500-line cap.
  Per decision D-P6 this excess is a pre-existing condition this feature neither creates nor is
  permitted to remediate. `[P2-T14]` and `[P4-T3]` therefore require its post-change count to be **not
  greater than 2437**, not to be at or below 500.
- `QuickFiler.Test/Controllers/QfcItemController.NavigationTests.cs` is **391 lines**, matching the
  figure the plan's Phase 3 line-budget section states. The Phase 3 block therefore has 109 lines of
  headroom before the 500-line cap; `[P3-T17]` gates it.
- `QuickFiler.Test/Controllers/KbdActionsRemainingBranchesTests.cs` is 181 lines and
  `KbdActionsTests.cs` is 88, both with ample headroom for the Phase 1 additions.
- `[P1-T11]` expects a passed count of `BaselineKbdActionsTestMethodCount + 5` = **18**.

Output Summary: seven numeric line counts recorded; `FrozenTestMethodCount = 13`;
`BaselineKbdActionsTestMethodCount = 13`; `QfcCollectionControllerTests.cs` confirmed at exactly 500
lines and `QfcCollectionController.cs` at 2437.
