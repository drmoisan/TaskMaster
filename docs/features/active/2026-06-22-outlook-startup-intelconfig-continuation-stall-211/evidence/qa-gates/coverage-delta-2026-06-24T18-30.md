# Coverage-Delta Verification (Issue #211 PostLoad/LoadInboxes attribution probe)

Timestamp: 2026-06-24T18-30

Sources:
- Baseline: `evidence/baseline/tests-coverage-baseline-2026-06-24T18-30.md` (+ `baseline-coverage-2026-06-24T18-30.cobertura.xml`)
- Post-change: `evidence/qa-gates/final-tests-coverage-2026-06-24T18-30.md` (+ `final-coverage-2026-06-24T18-30.cobertura.xml`)

Both runs use the identical command and conversion (same two assemblies, same filter, same merge to Cobertura), so the rates are directly comparable.

## Repository-wide / first-party package coverage (no-regression check)

| Scope | Baseline | Post-change | Delta | Result |
|---|---|---|---|---|
| Whole-process line-rate | 61.89% | 61.94% | +0.05 pp | No regression |
| First-party `TaskMaster` package | 53.09% | 53.55% | +0.46 pp | No regression (improved) |
| First-party `UtilitiesCS` package | 87.46% | 87.46% | 0.00 pp | No regression (untouched) |

No package coverage regressed. The first-party `TaskMaster` package improved because the new coverable
`StartupInboxAttributionProbe` and the `EmitPerStoreInboxAttribution` method are fully exercised.

## New/changed-code coverage (>= 90% target)

| New code | Coverage | Target | Result |
|---|---|---|---|
| `TaskMaster.StartupInboxAttributionProbe` (class) | 100% (line-rate 1.0) | >= 90% | PASS |
| `AppOlObjects.EmitPerStoreInboxAttribution` (method) | 100% (line-rate 1.0) | >= 90% | PASS |

The per-store attribution method and the entire pure formatter/aggregator helper are at 100% line
coverage via the deterministic MSTest in `StartupInboxAttributionProbeTests.cs`.

## Conclusion

- Repository-wide coverage did NOT regress (whole-process and both first-party packages flat or improved).
- New-code coverage meets the >= 90% target (both at 100%).
- Outcome: PASS.
