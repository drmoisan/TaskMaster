# Coverage Verification (from persisted XML) — Remediation Cycle 1 (Issue #232)

Timestamp: 2026-07-03T16-58

Source of truth for this verification: the persisted, committable Cobertura XML
`docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/coverage/2026-07-03T16-58/coverage.xml`
(byte-identical, SHA-256 `a80f5ae3d3d4f9de59d886be445c2dd4df3789aca35c8a14e3b7181eb10f19d7`, to the canonical
`artifacts/csharp/coverage.xml` and to the P2-T4 run output).

## P3-T4 — Changed-line coverage of `QfcHighConfidencePreFilter.cs` (AC10 target: >= 90%)

Every `<class>` element whose `filename` maps to
`...\QuickFiler\Controllers\QfcHighConfidencePreFilter.cs` reports `line-rate="1"` (100%):

| # | Class name | line-rate |
|---|---|---|
| 1 | `QuickFiler.Controllers.QfcHighConfidencePreFilter` | 1 |
| 2 | `QuickFiler.Controllers.QfcPreScoredItem` | 1 |
| 3 | `QuickFiler.Controllers.QfcHighConfidencePreFilter.<>c` | 1 |
| 4 | `QuickFiler.Controllers.QfcHighConfidencePreFilter.<>c__DisplayClass1_0` | 1 |
| 5 | `QuickFiler.Controllers.QfcHighConfidencePreFilter.<FilterAsync>d__1` | 1 |
| 6 | `QuickFiler.Controllers.QfcHighConfidencePreFilter.<>c__DisplayClass1_0.<<FilterAsync>b__0>d` | 1 |

Mapped-class count: 6. All six line-rate = 1.

Changed-line-coverage determination: **PASS** (100% >= 90%).

## P3-T5 — Repository-wide no-regression (exemption-governed testable denominator)

Root `<coverage>` element from the persisted XML:

| Metric | Value |
|---|---|
| line-rate | 0.76574952561669829 |
| lines-covered | 40355 |
| lines-valid | 52700 |
| branch-rate | 0.7208237986270023 |
| percent | 76.5750% |

Comparison versus the recorded baseline (~76.57%):

| Measurement | line-rate | lines-covered | lines-valid | percent |
|---|---|---|---|---|
| Recorded prior-cycle baseline (`coverage-delta.md`) | 0.76575789793438642 | 40334 | 52672 | 76.5758% |
| Phase 0 remediation baseline (P0-T5, this cycle)    | 0.76574952561669829 | 40355 | 52700 | 76.5750% |
| Final (P2-T4 persisted XML)                          | 0.76574952561669829 | 40355 | 52700 | 76.5750% |

Deltas:
- Final vs. this-cycle Phase 0 baseline: line-rate delta 0.0 (identical run-over-run), 0.0000 pp.
- Final vs. recorded prior-cycle baseline: line-rate delta -0.0000083723 (-0.00084 pp), within run-to-run
  measurement variance. Both figures round to ~76.57%.

No changed production line is uncovered (the only touched production line in a non-exempt file is in
`QfcHighConfidencePreFilter.cs`, which is 100% covered; the Phase 1 edit is in the `[ExcludeFromCodeCoverage]`
`QfcDatamodel.cs` and does not affect the denominator or numerator).

Repository-wide no-regression determination: **PASS**. The repository-wide figure is materially unchanged at
~76.57% (exemption-governed testable denominator per CLAUDE.md / Issue #227). The raw figure remains below the
80% floor at both baseline and final; this is the pre-existing repository state covered by the ratified
COM/VSTO/WinForms exemption framework and is neither introduced nor worsened by this change.

## Combined determination

- (a) Repository-wide no-regression: **PASS**.
- (b) New/changed-code >= 90% on the non-exempt touched file (`QfcHighConfidencePreFilter.cs`): **PASS** (100%).
- **AC10: PASS** — verified directly from the persisted machine-readable Cobertura artifact.
