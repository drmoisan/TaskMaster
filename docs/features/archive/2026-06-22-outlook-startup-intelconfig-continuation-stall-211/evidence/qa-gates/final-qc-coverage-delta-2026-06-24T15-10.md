# Final QC — Coverage Delta Verification (issue #211)

Timestamp: 2026-06-24T15-10

Sources:
- Baseline: `evidence/baseline/baseline-tests-coverage-2026-06-24T15-10.md` (and archived Cobertura
  `evidence/baseline/coverage/baseline-cov-2026-06-24T15-10.cobertura.xml`).
- Post-change: `evidence/qa-gates/final-qc-tests-coverage-2026-06-24T15-10.md` (and archived Cobertura
  `evidence/qa-gates/postchange-cov-2026-06-24T15-10.cobertura.xml`).

## Numeric values

| Metric | Baseline | Post-change | Delta |
|---|---|---|---|
| Repository-wide (whole-process) line coverage | 60.43% (0.604334) | 60.47% (0.604671) | +0.04 pp |
| First-party `UtilitiesCS` package line coverage | 87.16% (0.871566) | 87.17% (0.871731) | +0.02 pp |
| New code: `UtilitiesCS.EmailIntelligence.SpamInitTimingProbe` | n/a (did not exist) | 100% (1.0) | new |

## Threshold assertions

- No repository-wide regression: PASS. Post-change whole-process line coverage (60.47%) is not lower
  than baseline (60.43%); it is marginally higher. The first-party `UtilitiesCS` package (87.17%)
  also did not regress versus baseline (87.16%) and remains above the >= 80% floor.
- New code >= 90%: PASS. The new production class `SpamInitTimingProbe` has 100% line coverage,
  exceeding the 90% requirement for new modules/classes/methods.
- Changed-line coverage (SpamBayes instrumentation): the instrumented `CreateAsync` state machine is
  at 93.75% line-rate; the `ValidatePathsSet` body is exercised by existing SpamBayes_Tests. No
  reduction in coverage for the changed file relative to the 95.58% baseline source-file rate.

## Verdict

PASS against both the no-regression and the >= 90% new-code thresholds.

Note: the whole-process denominator includes vendored/third-party modules (Deedle, FSharp.Core,
log4net, System.Linq.Async, SVGControl, Swordfish, FluentAssertions). The policy >= 80% floor applies
to the testable first-party denominator; `UtilitiesCS` at 87.17% satisfies it.
