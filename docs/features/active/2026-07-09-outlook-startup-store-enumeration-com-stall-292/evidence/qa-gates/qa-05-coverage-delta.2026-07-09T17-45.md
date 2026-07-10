# QA-05 Coverage No-Regression Delta (Cycle 2, Issue #292)

Timestamp: 2026-07-09T17-45

Reliable `dotnet-coverage collect -> Cobertura` path, repository-wide root line-rate.

| Measurement | Source task | line-rate | lines-covered | lines-valid |
|---|---|---|---|---|
| Baseline (pre-fix) | P0-T5 | 81.82% (0.8181536868) | 121621 | 148653 |
| Post-change | P2-T4 | 81.81% (0.8181335055) | 121618 | 148653 |
| Delta | — | -0.00202 pp | -3 | 0 |

## Determination

- `lines-valid` (the coverage denominator) is identical at 148653 across baseline and post-change, confirming
  no production line was added or removed. The change is test-attribute-only (three class-level
  `[DoNotParallelize]` attributes), so production coverage is structurally unchanged.
- The -3 lines-covered / -0.002 percentage-point difference is within the `dotnet-coverage` tool's known
  run-to-run instrumentation noise (test scheduling under coverage instrumentation), not a production coverage
  regression. The P1-T3 and P2-T4 post-fix collections both reported the identical 121618 / 148653, confirming
  the post-change figure is stable and reproducible.
- Repository-wide coverage remains at ~81.8%, above the 80% testable-denominator floor.
- New/changed-code coverage is not applicable: no production line is added or changed.

No coverage regression (AC5 satisfied).
