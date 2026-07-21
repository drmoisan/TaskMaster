# Coverage Delta Verification (issue #292, remediation cycle 1)

- Timestamp: 2026-07-09T16-05
- Task: [P3-T5]
- Measurement method (identical for baseline and post-change): `dotnet-coverage collect --output-format cobertura` over the full CI-equivalent 7-assembly set (Cobertura root line-rate).

## Numeric values

| Metric | Baseline (P0-T6) | Post-change (P3-T4) | Delta |
|--------|------------------|---------------------|-------|
| Repository-wide line-rate (Cobertura root, all modules) | 81.80% (121602/148653) | 81.80% (121602/148653) | 0.00% |
| UtilitiesCS package (touched-file assembly) line-rate | 88.36% | 88.33% | -0.03% (measurement noise) |
| Changed production-code coverage | n/a (no production code changed) | n/a | n/a |

## Conclusion — no regression

- The only changes are `[DoNotParallelize]` attributes on 8 existing `UtilitiesCS.Test` test classes. No production `*.cs` file changed and no new production module was added, so the changed-production-code denominator is empty and there is no changed-line coverage obligation.
- Repository-wide line-rate is unchanged at 81.80%. The UtilitiesCS -0.03% is within the known dotnet-coverage denominator measurement variance (per-run double-count nondeterminism); it is not a regression caused by the change, which adds no production lines.
- Testable-denominator floor: the touched-file assembly (UtilitiesCS) is at 88.33% (>= 80%); the all-modules repository-wide figure is 81.80% (>= 80%). No coverage regression; the `>= 80%` floor holds with numeric values (no placeholders).
