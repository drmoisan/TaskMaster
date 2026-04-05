# P0-T8: Baseline Test Coverage

Timestamp: 2026-03-26T16:12

Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug

EXIT_CODE: 0

Output Summary:
- Test Run Successful: 3409 total, 3407 passed, 2 skipped, 0 failed.
- Coverage artifact: `coverage/coverage.cobertura.xml`

**Overall line-rate:** 0.70528 (70.53%)

**Package-level coverage for touched residual scope:**
- QuickFiler: 0.2154 (21.54%)
- Swordfish.NET.General (UtilitiesSwordfish): 0.4653 (46.53%)
- TaskMaster: 0.0842 (8.42%)

**Per-file coverage for touched residual production files:**
- `QuickFiler/Controllers/EfcHomeController.cs`: 0.0584 (5.84%)
- `QuickFiler/Controllers/QfcCollectionController.cs`: 0.0333 (3.33%)
- `QuickFiler/Controllers/QfcHomeController.cs`: 0.6060 (60.60%)
- `QuickFiler/Controllers/QfcItemController.cs`: 0.0822 (8.22%)
- `UtilitiesSwordfish/Collections/ConcurrentObservableBase.cs`: 0.6667 (66.67%)
- `TaskMaster/AppGlobals/AppAutoFileObjects.cs`: 0.0300 (3.00%)
