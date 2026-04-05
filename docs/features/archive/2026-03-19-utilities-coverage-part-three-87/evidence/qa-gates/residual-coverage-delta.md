# Evidence: Coverage Delta

- **Timestamp:** 2026-03-27T08:20 UTC
- **Touched Scope:** QuickFiler + UtilitiesSwordfish + TaskMaster/AppGlobals
- **Baseline Scope Coverage:** From `residual-baseline-test-coverage.md` (mixed branch `feature/utilities-coverage-part-three-87`)
- **Final Scope Coverage:** From `residual-qc-test-coverage.md` (clean residual branch `chore/mixed-branch-excluded-work-clean`)

## Per-Package Coverage Delta

| Package | Baseline | Final | Delta |
|---|---|---|---|
| QuickFiler | 21.54% | 20.15% | -1.39% |
| Swordfish.NET.General (UtilitiesSwordfish) | 46.53% | 46.46% | -0.07% |
| TaskMaster | 8.42% | 8.42% | 0.00% |

## Changed Production Files Coverage Delta

| File | Baseline | Final | Delta |
|---|---|---|---|
| QuickFiler/Controllers/EfcHomeController.cs | 5.84% | 5.84% | 0.00% |
| QuickFiler/Controllers/QfcHomeController.cs | 60.60% | 48.66% | -11.94% |
| QuickFiler/Controllers/QfcItemController.cs | 8.22% | 1.96% | -6.26% |
| QuickFiler/Controllers/QfcCollectionController.cs | 3.33% | 0.00% | -3.33% |
| UtilitiesSwordfish/Collections/ConcurrentObservableBase.cs | 66.67% | 66.24% | -0.43% |
| TaskMaster/AppGlobals/AppAutoFileObjects.cs | 3.00% | 3.00% | 0.00% |

## Changed-Code Coverage

The per-file deltas above reflect coverage measured on different code bases — the baseline was captured from the mixed branch which includes #87 UtilitiesCS coverage scope tests and additional test files not present on the clean residual branch. The clean residual branch is based from `origin/development` and only adds the residual non-#87 commits. Coverage reductions in QuickFiler files (QfcHomeController -11.94%, QfcItemController -6.26%, QfcCollectionController -3.33%) are attributable to the mixed branch having additional #87-scope test files that exercised these controllers incidentally but are intentionally excluded from the residual scope.

## Overall Coverage

| Metric | Baseline | Final | Delta |
|---|---|---|---|
| Overall | 70.53% | 61.11% | -9.42% |

The overall delta is expected: the mixed branch contained ~548 additional tests from in-scope #87 UtilitiesCS coverage work that significantly raised the repository-wide line rate. The clean residual branch contains only the residual excluded work from `origin/development`.

## Output Summary

The clean residual branch did not introduce any new code regressions. All coverage reductions are attributable to the intentional exclusion of #87 coverage scope from this branch. Zero test failures. All QA gates passed in the final clean pass:
- `csharpier format .`: EXIT_CODE 0, no files changed
- `run-actionlint.ps1`: EXIT_CODE 0, no findings
- `Invoke-VSBuild.ps1 -EnableNETAnalyzers -EnforceCodeStyleInBuild`: EXIT_CODE 0, 39 warnings / 0 errors
- `Invoke-VSBuild.ps1 -EnableNullable -TreatWarningsAsErrors`: EXIT_CODE 0, 0 warnings / 0 errors
- `Invoke-MSTestWithCoverage.ps1`: EXIT_CODE 0, 2861 tests (2859 passed, 2 skipped)
