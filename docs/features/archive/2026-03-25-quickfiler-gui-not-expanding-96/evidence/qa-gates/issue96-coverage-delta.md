# Coverage Delta: Issue #96 Clean Branch vs Baseline

- **Timestamp:** 2026-03-26T16:53 UTC
- **Touched Scope:** QuickFiler

## Baseline QuickFiler Coverage (from `baseline-test-coverage.md`)

| Scope | Baseline Line Rate |
|---|---|
| QuickFiler (package) | 21.54% |
| QfcItemController.cs | 8.22% |
| KbdActions.cs | 26.42% |
| KeyboardHandler.cs | 0.00% |

## Final QuickFiler Coverage (from `issue96-qc-test-coverage.md`)

| Scope | Final Line Rate |
|---|---|
| QuickFiler (package) | 21.91% |
| QfcItemController.cs | 8.22% |
| KbdActions.cs | 26.42% |
| KeyboardHandler.cs | 0.00% |

## Changed Production Files

| File | Baseline | Final | Delta |
|---|---|---|---|
| QfcItemController.cs | 8.22% | 8.22% | 0.00% (preserved) |
| KbdActions.cs | 26.42% | 26.42% | 0.00% (preserved) |
| KeyboardHandler.cs | 0.00% | 0.00% | 0.00% (preserved) |
| QuickFiler (package) | 21.54% | 21.91% | +0.37% (improved) |

## Changed-Code Coverage

All changed production files preserved their baseline coverage. No regression detected.

## Output Summary

The clean issue #96 branch (`bug/quickfiler-gui-not-expanding-96-clean`) **preserved or improved** coverage for all touched QuickFiler scope files compared to the baseline captured from the main workspace. The package-level line-rate improved slightly from 21.54% to 21.91% due to the different code base composition on `origin/development` vs the mixed feature branch. No individual changed file regressed. Coverage delta is acceptable.
