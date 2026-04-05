# Issue #97 Coverage Delta

- **Timestamp:** 2026-03-26T18:24 EDT
- **Touched Scope:** QuickFiler

## Baseline Coverage (mixed branch, P0-T7/P0-T8)

- **Baseline QuickFiler Coverage:** 21.54% line / 8.08% branch
- **Baseline Changed-File Coverage:**
  - `QfcHomeController.cs`: 60.60% line / 45.19% branch
  - `QfcCollectionController.cs`: 3.33% line / 2.53% branch

## Final Coverage (clean issue #97 branch, P2-T4)

- **Final QuickFiler Coverage:** 20.99% line / 7.91% branch
- **Final Changed-File Coverage:**
  - `QfcHomeController.cs`: 78.71% line / 55.26% branch
  - `QfcCollectionController.cs`: 4.27% line / 3.13% branch

## Changed Production Files

| File | Baseline Line | Final Line | Delta | Baseline Branch | Final Branch | Delta |
|---|---|---|---|---|---|---|
| `QfcHomeController.cs` | 60.60% | 78.71% | **+18.11%** | 45.19% | 55.26% | **+10.07%** |
| `QfcCollectionController.cs` | 3.33% | 4.27% | **+0.94%** | 2.53% | 3.13% | **+0.60%** |

## Changed-Code Coverage

- `QfcHomeController.cs`: The null-guard changes (issue #97 fix) are covered by the new `QfcHomeControllerTests.cs` regression tests. Coverage improved from 60.60% to 78.71% line rate.
- `QfcCollectionController.cs`: The null-guard changes are covered by the new `QfcCollectionControllerTests.cs` regression tests. Coverage improved from 3.33% to 4.27% line rate.

## Output Summary

The clean issue #97 branch **improved** coverage for both touched production files. `QfcHomeController.cs` gained +18.11% line rate and `QfcCollectionController.cs` gained +0.94% line rate. No coverage regression occurred in the touched scope. The slight decrease in overall QuickFiler package line rate (21.54% → 20.99%) is due to the difference in codebase state between the mixed branch baseline and the clean origin/development-based branch (different set of untouched files contributing to the package total), not a regression from issue #97 changes.

**Note:** Baseline coverage was captured on the mixed branch (which includes changes from issues #96, #87, and other work). The clean issue #97 branch is based on bare `origin/development`. The per-file deltas for the two touched production files are the meaningful comparison, and both show improvement.
