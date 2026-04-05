# Baseline Test Coverage

- **Timestamp:** 2026-03-26T17:55 EDT
- **Command:** `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
- **EXIT_CODE:** 0
- **Output Summary:**
  - Test Run Successful. Total tests: 3409, Passed: 3407, Skipped: 2, Failed: 0.
  - Coverage artifact: `coverage/coverage.cobertura.xml`
  - **QuickFiler package coverage:**
    - Line rate: 0.2154 (21.54%)
    - Branch rate: 0.0808 (8.08%)
  - **QuickFiler changed-file coverage (issue #97 touched files):**
    - `QfcHomeController.cs`: line-rate=0.605965, branch-rate=0.451923
    - `QfcCollectionController.cs`: line-rate=0.033292, branch-rate=0.025316
  - Remaining below-threshold file count for QuickFiler: majority of files at 0% (UI designer files, viewers, helper classes); 2 key controller files above 0%.
