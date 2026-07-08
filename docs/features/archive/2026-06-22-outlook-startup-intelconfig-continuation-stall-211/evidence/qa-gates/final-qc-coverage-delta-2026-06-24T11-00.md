# Final QC — Coverage Delta and Thresholds (issue #211, Phase 3.3)

Timestamp: 2026-06-24T11-00

## Sources
- Baseline: `evidence/baseline/baseline-tests-coverage-2026-06-24T11-00.md` (P0-T7)
- Post-change: `evidence/qa-gates/final-qc-tests-coverage-2026-06-24T11-00.md` (P5-T4)

## New / changed code coverage (the threshold-governing metric)

| Type | Baseline | Post-change | Lines |
|---|---|---|---|
| `TaskMaster.StartupDiagnosticsProbe` | 100.0% | 100.0% | 112/112 |
| `TaskMaster.StartupLifetimeStopDecider` (new) | n/a | 100.0% | 54/54 |
| `TaskMaster.StartupStageLabels` (new) | n/a | 100.0% | 16/16 |

New/changed code coverage = 100%, satisfying the >= 90% new-code threshold. The
`EmitLifetimeHeartbeat` method added to `StartupDiagnosticsProbe` and the two new types are fully
exercised by the deterministic MSTests in `StartupDiagnosticsProbeTests.cs`.

## Repository-wide line coverage (no-regression)

The plan's P0-T7 / P5-T4 commands run coverage over the single `TaskMaster.Test` assembly. The raw
cobertura `coverage/@line-rate` for such a single-assembly run is dominated by vendored modules
(Deedle, Apache.Arrow, etc.) that the assembly references, so the raw figure (~11.6% baseline,
~11.8% post-change) is an instrumentation artifact, not the repository-wide first-party floor.

No-regression assessment: this change is purely additive. It adds one method and two types to the
coverable `StartupDiagnosticsProbe.cs` (all at 100% coverage) and adds the `DispatcherTimer` seam to
the `[ExcludeFromCodeCoverage]` `ThisAddIn` (excluded from the denominator by policy). No existing
production line was deleted or made unreachable, and no existing test was removed or weakened (134
prior tests still pass). Therefore the first-party repository-wide line coverage cannot regress
versus baseline; the changed lines are themselves fully covered.

## Outcome

- New/changed-code coverage: 100% (>= 90% PASS).
- Repository-wide first-party coverage: no regression (additive change, all new lines covered,
  no existing coverage removed).
- Threshold result: PASS.
