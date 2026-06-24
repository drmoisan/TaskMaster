# Baseline — File Size (#211 Phase 3.1)

Timestamp: 2026-06-23T18-40
Command: `wc -l TaskMaster/AppGlobals/ApplicationGlobals.cs TaskMaster/AppGlobals/EngineInitTimingProbe.cs`
EXIT_CODE: 0

Output Summary:
- `TaskMaster/AppGlobals/ApplicationGlobals.cs`: 263 lines. Headroom to the 500-line limit: 237 lines. Sufficient headroom for the heartbeat start/stop scaffolding and the GC before/after capture + `[gc-delta]` emission in `LoadSequentialAsync`. No private-helper extraction is required to stay under 500.
- `TaskMaster/AppGlobals/EngineInitTimingProbe.cs`: 97 lines. Headroom to the 500-line limit: 403 lines. Sufficient for adding `threadPriority=`/`isThreadPoolThread=` to the `[engine-init]` line.

Determination: Both files have ample headroom under 500 lines. The optional private-helper extraction described in plan P0-T2 / P2-T1 is NOT required for `ApplicationGlobals.cs`.
