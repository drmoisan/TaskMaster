# Coverage Delta and Threshold Verification (Cycle 2)

Timestamp: 2026-06-12T17:12Z

Source: artifacts/csharp/coverage.xml (post-change full UtilitiesCS.Test run, merged via
dotnet-coverage). Aggregated over all <function> elements with type_name="LcppnFolderPredictor".

## LcppnFolderPredictor strict coverage

| Metric | Baseline (P0-T6) | Post-change (P2-T1) | Delta |
|---|---|---|---|
| Strict line coverage | 97.71% | 97.71% | 0.00 |
| Strict block coverage | 97.58% | 97.58% | 0.00 |

- Post-change strict coverage = 97.71% (line), 97.58% (block) >= 90% threshold: PASS.
- No regression on changed lines: this cycle is a test-only file partition; every
  LcppnFolderPredictor test moved intact and no production line changed, so the per-line
  coverage of LcppnFolderPredictor is byte-for-byte unchanged from baseline (covered=171,
  partial=4, not=0 lines; covered=242, not=6 blocks in both runs). No regression: PASS.

## Repository / module floor

- UtilitiesCS.dll module line coverage (post-change) = 85.46% >= 80% floor: PASS.
  (Baseline cycle-1 recorded 85.45% strict; within noise, no regression.)

## Conclusion
Coverage no-regression and threshold criteria satisfied: LcppnFolderPredictor strict
coverage unchanged at 97.71% line / 97.58% block (>= 90%); repository module floor held at
85.46% (>= 80%).
