Timestamp: 2026-06-23T17-55
Issue: #211
EXIT_CODE: 0

# Acceptance Criteria Check-off — Issue #211

Canonical AC source: `spec.md` (AC1–AC6). This artifact records the final
check-off for the IntelConfig continuation-stall diagnosis.

## Result

All six acceptance criteria are satisfied.

| AC | Status | Evidence |
| --- | --- | --- |
| AC1 — `[continuation-resume]` probe lines per inter-phase boundary | PASS | impl commit 72520363; `evidence/other/runtime-capture-nondebugger-2026-06-23T13-51.md` |
| AC2 — behavior-preserving, Stopwatch only, no banned API, net48 | PASS | `evidence/qa-gates/final-qc-2026-06-22T18-05.md`; `code-review.2026-06-22T22-45.md` |
| AC3 — deterministic MSTest via `TestApplicationGlobals` seam | PASS | feature-review PASS (`feature-audit.2026-06-22T22-45.md`) |
| AC4 — full C# toolchain passes; coverage; <=500 lines | PASS | `evidence/qa-gates/final-qc-2026-06-22T18-05.md` |
| AC5 — non-debugger cold-start capture with probe fields | PASS | `evidence/other/runtime-capture-nondebugger-2026-06-23T13-51.md` |
| AC6 — Phase 2 evidence gate | PASS (no-fix branch) | `waitMs=0.6` < 5000 ms; stall not reproduced outside debugger |

## Phase 2 Gate Decision

The non-debugger cold-start capture records the IntelConfig `Task.Run`
continuation resuming on the STA (`resumeThreadId=1`, `staIsIdle=True`) with
`waitMs=0.6`. This is far below AC6's `> 5000 ms` trigger. The originally-reported
60–115 s IntelConfig stall was attributable to Visual Studio debugger overhead in
the earlier debugger-attached captures, not a TaskMaster-caused STA block.

Per AC6's second branch and the maintainer rule "in scope iff this add-in causes
it," the off-STA IntelConfig continuation change (`ConfigureAwait(false)` +
`await UiThread.UiSyncContext`) is intentionally NOT implemented.

## Integration / Migration Posture

- The Phase 1 attribution probe (`YieldWithContinuationProbeAsync`) is merged to
  `origin/main` (commit 72520363) and is reachable on the production startup path
  (`ApplicationGlobals.LoadSequentialAsync`). It emits via the existing log4net
  logger; no separate persistence or migration is required.
- No legacy code path was retired (the probe replaced the prior bare `Task.Yield()`
  inter-phase yields in place, behavior-preserving).

## Known Pre-existing Condition

`UtilitiesCS TimedAsyncTask_Tests.RequestTask_WithProvidedTask_InvokesTaskAfterInterval`
is a recorded pre-existing real-interval timer flake under full-suite/coverage
load. It is not a regression introduced by this change.

## Follow-up Candidate (out of #211 scope)

The same capture shows the `Engines` phase at `1:52.59` (TOTAL `1:58.79`). That
cost is separate from the IntelConfig continuation-stall hypothesis and should be
tracked as a new issue if still reproducible and user-impacting.
