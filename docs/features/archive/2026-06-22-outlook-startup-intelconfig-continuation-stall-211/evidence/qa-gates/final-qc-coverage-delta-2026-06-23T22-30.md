# Final QC — Coverage Delta and Thresholds (#211 Phase 3.2)

Timestamp: 2026-06-23T22-30

## Repo-wide line coverage (no-regression check)

| Measure | Baseline (P0-T7) | Post-change (P5-T4) | Delta |
|---|---|---|---|
| line-rate | 0.627741 (62.77%) | 0.627992 (62.80%) | +0.000250 (+0.03 pts) |
| lines-covered | 102249 | 102362 | +113 |
| lines-valid | 162684 | 162999 | +315 |

Repo-wide coverage did NOT regress (post-change >= baseline). The raw aggregate denominator
(62.80%) is below the 80% floor because it includes vendored Swordfish/SVGControl and the broad
COM/VSTO/WinForms surface that is formally exempted in CLAUDE.md (testable-denominator rule); this
matches the established baseline measurement method for this feature and is not changed by this
diagnosis-only increment.

## New / changed-code coverage (>= 90% threshold)

| Unit | Lines covered | Lines valid | Rate |
|---|---|---|---|
| `TaskMaster.StartupDiagnosticsProbe` (coverable, includes both new phase-annotated overloads) | 46 | 46 | 100.00% |

- The new code introduced by this increment that is required to meet the >= 90% threshold is the
  pure formatting logic added to `StartupDiagnosticsProbe` (the phase-annotated `EmitHeartbeat`
  and `EmitGcDelta` overloads, P1-T1/P1-T2). It is at 100% via the deterministic MSTests P3-T1..P3-T3.
- The host-bound seams added/renamed in `ApplicationGlobals` (`StartStartupUiHeartbeat`,
  `StopStartupUiHeartbeat`, `BeginPhaseGcCapture`, `EmitPhaseGcDelta`) construct a live
  `DispatcherTimer` and perform live `GC.*`/`GCSettings.*` reads with no injectable seam beyond the
  override-to-no-op test pattern. They fall under the CLAUDE.md COM/host-bound coverage exemption
  (same structure as the Phase 3.1 Engines-only probe). The `LoadSequentialAsync` body and the
  `BeginPhase` private helper ARE exercised by `ContinuationProbeSequenceTests` and
  `LoadSequentialAsync_ExecutesRealCoordinatorSequenceThroughPhaseWrappers` (real coordinator driven
  through no-op seam overrides).

## Outcome

PASS: repo-wide line coverage >= baseline (no regression); new coverable code (StartupDiagnosticsProbe
additions) at 100% (>= 90%). No threshold unmet.
