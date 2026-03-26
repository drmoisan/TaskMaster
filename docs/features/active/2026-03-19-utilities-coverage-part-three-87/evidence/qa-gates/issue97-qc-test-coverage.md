# Issue #97 QC: Test Coverage

- **Timestamp:** 2026-03-26T18:22 EDT
- **Command:** `pwsh -NoProfile -ExecutionPolicy Bypass -Command "Set-Location 'c:\Users\DanMoisan\repos\TaskMaster-issue97-clean'; pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug"`
- **EXIT_CODE:** 1 (due to 14 pre-existing failures on `origin/development`, not caused by issue #97)
- **Output Summary:**
  - Test Run: Total 2869, Passed 2853, Failed 14, Skipped 2
  - **Pre-existing failures** (all outside issue #97 scope — zero files in UtilitiesSwordfish, UtilitiesCS, or ToDoModel were changed by issue #97):
    - 11 in UtilitiesSwordfish.Test: ToBase10/ToBase36 conversion tests
    - 3 in ToDoModel.Test: Constructor/Property tests
  - **Issue #97 regression status:** Zero regressions. All QuickFiler.Test tests passed.
  - Coverage artifact: `c:\Users\DanMoisan\repos\TaskMaster-issue97-clean\coverage\coverage.cobertura.xml`
  - **QuickFiler package coverage (post-change):**
    - Line rate: 20.99%
    - Branch rate: 7.91%
  - **QuickFiler changed-file coverage (issue #97 touched production files):**
    - `QfcHomeController.cs`: line-rate=78.71%, branch-rate=55.26%
    - `QfcCollectionController.cs`: line-rate=4.27%, branch-rate=3.13%
  - Remaining below-threshold file count for QuickFiler: majority of files at 0% (UI designer files, viewers, helper classes).
  - **Note:** The 14 pre-existing failures exist on bare `origin/development` before any issue #97 commits were applied. The `git diff --name-only origin/development HEAD` shows zero changes in UtilitiesSwordfish, UtilitiesCS, or ToDoModel directories, confirming issue #97 did not introduce these failures.
