# AC14 Check-off Summary (#211 Phase 3.2)

Timestamp: 2026-06-23T22-30

## AC14 — All-phase UI-heartbeat + per-phase GC, phase-annotated

Status: SATISFIED (toolchain/coverage portion); runtime validation is maintainer-gated (P4-T1/P4-T2).

| AC14 sub-requirement | Satisfying task(s) | Evidence |
|---|---|---|
| Heartbeat spans the entire sequential startup (starts before IntelConfig, stops after Events in a finally) | P2-T2, P2-T3 | `LoadSequentialAsync` starts `StartStartupUiHeartbeat` before the IntelConfig phase and stops it in a `finally` after the Events phase (ApplicationGlobals.cs) |
| Each `[ui-heartbeat]` line annotated with the currently-active phase via a current-phase marker field updated at each boundary | P1-T1, P2-T1, P2-T2 | `_currentStartupPhase` set by `BeginPhase(...)` before each await; heartbeat tick passes it to `StartupDiagnosticsProbe.EmitHeartbeat(phase, nominal, actual)` |
| One `[gc-delta]` line emitted per phase, phase-annotated, with `isServerGC`/`latencyMode` | P1-T2, P2-T4 | `BeginPhaseGcCapture(phase)` before each await; `EmitPhaseGcDelta(probe, phase)` after each phase emits one phase-annotated `[gc-delta]` (six total) |
| Pure phase-annotation and per-phase GC-delta formatting live in `StartupDiagnosticsProbe` (coverable, not `[ExcludeFromCodeCoverage]`) and covered by deterministic MSTest | P1-T1, P1-T2, P3-T1, P3-T2, P3-T3 | New `EmitHeartbeat(string,...)` / `EmitGcDelta(string,...)` overloads in StartupDiagnosticsProbe.cs (100% covered) + 3 new MSTests |
| Host-bound scheduling and live GC reads stay in `protected internal virtual` seams | P2-T2, P2-T4, P2-T6 | Seams `StartStartupUiHeartbeat`/`StopStartupUiHeartbeat`/`BeginPhaseGcCapture`/`EmitPhaseGcDelta` remain `protected internal virtual`; no-op overridden in all three test subclasses |
| Phase order/set/semantics unchanged; `[continuation-resume]` and `[engine-init]` lines unchanged | P2-T5 | `YieldWithContinuationProbeAsync` and `EngineInitTimingProbe` untouched; `LoadSequentialAsync_ExecutesRealCoordinatorSequenceThroughPhaseWrappers` verifies order intel/ol/todo/auto/engine/events with YieldCount==5; source-inspection regressions pass |
| New code >= 90% coverage; no repo-wide regression; full toolchain passes in order | P5-T1..P5-T6 | StartupDiagnosticsProbe additions 100%; repo-wide 62.77% -> 62.80% (no regression); CSharpier/analyzers/nullable all exit 0; tests pass except the pre-existing flake |

## Runtime portion (maintainer-gated)

- P4-T1: maintainer capture instructions written (`coldstart-allphase-uiheartbeat-gc-capture-instructions-2026-06-23T22-30.md`).
- P4-T2: evidence placeholder created (`runtime-capture-allphase-uiheartbeat-gc-PLACEHOLDER.md`), marked PENDING MAINTAINER CAPTURE.
- The all-phase cold-start capture is a runtime task not executed by the automated toolchain; it confirms, on a live cold start, whether the slow phase freezes the STA (heartbeat gaps tracking the phase duration) or whether an async continuation merely waits while the STA stays responsive.

## AC source note

AC14 is introduced by this plan (the increment's acceptance criterion stated in the plan body).
The feature `issue.md` does not contain an explicit `## Acceptance Criteria` checkbox section, so
AC14 is tracked here per the plan's P5-T7 mapping rather than via an `issue.md` checkbox.
