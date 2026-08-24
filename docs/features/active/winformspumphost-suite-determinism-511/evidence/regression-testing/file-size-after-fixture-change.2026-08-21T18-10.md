# P2-T4 — File-Size Budget After the Phase 2 Fixture Change

Timestamp: 2026-08-22T10-19

Command:
```
(Get-Content -LiteralPath QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs).Count
(Get-Content -LiteralPath QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs).Count
(Get-Content -LiteralPath QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs).Count
```

EXIT_CODE: 0

Output Summary:

| File | Pre-change (P0-T7 baseline) | Post-change | Delta | Under 500 |
| --- | --- | --- | --- | --- |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | 409 | **416** | +7 (P2-T1 comment + statement) | yes |
| `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` | 467 | **470** | +3 (P2-T3 comment + statement) | yes |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` | 290 | **339** | +49 (P1-T1 probe test, added in Phase 1) | yes |

Acceptance:

- All three post-change counts are less than 500.
- `QfcItemController.ViewerSetupTests.cs` is recorded at 470 lines, which is at or below the
  475-line ceiling this task states.

Note on `Part3.cs`: the plan's pre-change budget table records 290 lines. Phase 1 (P1-T1) already
added the `BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread` probe, so the count
observed at the start of Phase 2 was 339. Phase 2 made no edit to this file; P3-T1 adds the second
regression test and P3-T2 re-measures it.
