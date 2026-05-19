# Evidence: QA Test + Coverage

- **Timestamp:** 2026-03-27T08:18 UTC
- **Command:** `pwsh -NoProfile -ExecutionPolicy Bypass -Command "Set-Location 'c:\Users\DanMoisan\repos\TaskMaster-residual-clean'; pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug"`
- **EXIT_CODE:** 0
- **Output Summary:**

## Test Results

- **Total tests:** 2861
- **Passed:** 2859
- **Failed:** 0
- **Skipped:** 2

All tests pass. The lower test count (2861 vs 3409 on the mixed branch) is expected because the clean residual branch does not include the #87 UtilitiesCS coverage tests or other excluded scope.

## Per-Package Coverage

| Package | Coverage |
|---|---|
| QuickFiler | 20.15% |
| Swordfish.NET.General (UtilitiesSwordfish) | 46.46% |
| TaskMaster | 8.42% |
| UtilitiesCS | 52.41% |

## Per-File Coverage (Touched Residual Production Files)

| File | Coverage |
|---|---|
| QuickFiler/Controllers/EfcHomeController.cs | 5.84% |
| QuickFiler/Controllers/QfcHomeController.cs | 48.66% |
| QuickFiler/Controllers/QfcItemController.cs | 1.96% |
| UtilitiesSwordfish/Collections/ConcurrentObservableBase.cs | 66.24% |
| TaskMaster/AppGlobals/AppAutoFileObjects.cs | 3.00% |

## Overall Coverage

61.11%
