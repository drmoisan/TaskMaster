# Final QC — Coverage Delta (#211 Phase 3.1)

Timestamp: 2026-06-23T18-40
Source baseline: `evidence/baseline/baseline-tests-coverage-2026-06-23T18-40.md`
Source post-change: `evidence/qa-gates/final-qc-tests-coverage-2026-06-23T18-40.md`
Per-file coverage computed from the post-change cobertura report (per-`<line>` hits aggregated across packages).

## Repo-wide line coverage

- Baseline coverage: 62.73% (lines-covered=102052, lines-valid=162684).
- Post-change coverage: 62.77% (lines-covered=102248, lines-valid=162884).
- Delta: +0.04 percentage points. NO repository-wide regression (post-change >= baseline).
- Note: both runs include the identical 17 pre-existing Deedle/DataFrame cross-assembly flake failures; the comparison is apples-to-apples.

## New / changed-code coverage

- `TaskMaster/AppGlobals/StartupDiagnosticsProbe.cs` (NEW coverable helper): 100.00% (24/24 instrumented lines). >= 90% new-code floor: PASS.
- `TaskMaster/AppGlobals/EngineInitTimingProbe.cs` (MODIFIED: added `threadPriority=`/`isThreadPoolThread=` to the `[engine-init]` line): 100.00% (30/30 instrumented lines). The changed `[engine-init]` emission lines are covered by `EngineInitTimingProbeTests` (existing order test plus the new `TimeEngineAsync_Always_EmitsWorkerThreadContextFieldsAlongsidePriorFields`). PASS.
- `TaskMaster/AppGlobals/ApplicationGlobals.cs` (MODIFIED: `LoadSequentialAsync` heartbeat/GC scaffolding + four new host-bound `protected internal virtual` seam methods): 62.63% (119/190). The new seam method BODIES (`StartEnginesUiHeartbeat`, `StopEnginesUiHeartbeat`, `BeginEnginesGcCapture`, `EmitEnginesGcDelta`) are the host-bound thin call site (DispatcherTimer on `UiThread.Dispatcher`, live `GC.*`/`GCSettings.*` reads) and are intentionally not unit-covered — they are no-op-overridden in the focused MSTest seam exactly as the pre-existing COM-bound phase-wrappers in this same coordinator are. The plan's design places the COVERABLE logic in `StartupDiagnosticsProbe` (100%), with the host-bound scheduling/reads in the thin call site. `LoadSequentialAsync`'s coordination body (phase awaits, RecordPhase, the seam call sites) IS exercised by the AppGlobals tests.

## Determination

PASS.
- New coverable helper `StartupDiagnosticsProbe` reaches 100% (>= 90% floor).
- Modified `EngineInitTimingProbe` changed lines reach 100%.
- No repository-wide coverage regression (62.77% post-change vs 62.73% baseline).
- The uncovered `ApplicationGlobals` host-bound seam bodies are the COM/UI-host-bound thin call site (DispatcherTimer/Dispatcher/GC), consistent with the plan's testable-seam design and the CLAUDE.md COM/VSTO/WinForms coverage exemption for Outlook-Interop-bound coordinator code without an injectable seam at the framework-call boundary.
