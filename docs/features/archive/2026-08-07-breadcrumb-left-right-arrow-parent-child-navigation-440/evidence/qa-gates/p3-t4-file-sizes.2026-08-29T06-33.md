# Phase 3 — File-Size Gate Over the Three Owned Files (issue #440, plan task P3-T4)

Timestamp: 2026-08-29T06-33

Command (each file measured as `(Get-Content -LiteralPath <path>).Count`, not with
`Measure-Object -Line`):

```
(Get-Content -LiteralPath UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs).Count
(Get-Content -LiteralPath UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs).Count
(Get-Content -LiteralPath UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs).Count
```

EXIT_CODE: 0

## Output Summary

| File | Observed | 500-line limit (AC-11) | Additional ceiling | Verdict |
| --- | --- | --- | --- | --- |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` | 248 | at or under 500 | at or under its 248-line P0-T9 baseline | PASS |
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs` | 292 | at or under 500 | none | PASS |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` | 491 | at or under 500 | at or under its 495-line P0-T9 baseline | PASS |

## Arithmetic behind the two additional ceilings

- **Production file, ceiling 248.** P2-T1 removed one line and P2-T2's comment budget
  is 5 lines against the 4 it replaced, so the worst permitted case is
  248 - 1 - 4 + 5 = 248. The observed value is exactly 248, which is the worst
  permitted case realised: the comment rewrite used its full 5-line budget.
- **Router test file, ceiling 495.** Decision D3 replaced two inline four-line
  `RouteAsync` calls with two single-line `ArrowAsync(router, "left")` calls and one
  single-line Act call, which is net line-negative and more than offsets the extra
  Arrange press. Observed 491, which is 4 lines below the baseline and 9 lines below
  the repository limit.

All three files satisfy AC-11's 500-line requirement and both additional ceilings.
