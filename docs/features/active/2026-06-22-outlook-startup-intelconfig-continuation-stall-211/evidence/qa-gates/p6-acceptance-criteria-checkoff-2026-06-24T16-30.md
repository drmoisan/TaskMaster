# AC18 Check-Off Mapping (issue #211, Phase 3.6)

Timestamp: 2026-06-24T16-30
AC source: `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/spec.md` (Work Mode: full-bug)

## AC18 — StoreWrapper-init shared-cost attribution probe (diagnosis-only)

Automated portion: marked `[x]` in spec.md. Runtime portion: separate `[~] AC18 (runtime, maintainer)` entry, maintainer-gated.

### Automated-portion sub-claim -> evidence mapping

| AC18 sub-claim | Implementation | Verifying evidence |
|---|---|---|
| (1) `StoreWrapper.Init()` emits one `[store-wrapper-init]` line + unchanged `[Startup timing]`; `Restore` transitive, no double-count | `StoreWrapper.cs` Init wrap/add/emit (P3-T1); `Restore` unchanged (P3-T2/T3) | git diff (only Init changed); `final-qc-tests-coverage` (4099 pass) |
| (2) Process-global thread-safe `StoreWrapperInitClock` (Interlocked Add/TotalMs/Reset), no lost updates | `StoreWrapperInitClock.cs` (P1-T1) | `StoreWrapperInitClockTests` incl. `Add_ConcurrentCalls_AccumulatesWithoutLostUpdates`; 100% new-code coverage |
| (3) `LoadSequentialAsync` per-phase `[phase-net]` via `SampleStoreWrapperInitTotalMs()` seam; `netMs` clamp at 0.0; gross table unchanged | `ApplicationGlobals.cs` seam + per-phase emit (P4-T2/T3); `StartupDiagnosticsProbe.EmitPhaseNet`/`ComputeNetMs` (P4-T1) | `PhaseNetProbeTests` incl. clamp + boundary tests; `ApplicationGlobalsTests`/`...StartupTimingTests`/`ContinuationProbeSequenceTests` pass (behavior-preserving) |
| Pure formatting in coverable helpers; deterministic MSTest; no live COM/timer/fs/network; no temp files | `StoreWrapperInitProbe.cs` (P1-T2), `StartupDiagnosticsProbe` additions | `StoreWrapperInitProbeTests`, `PhaseNetProbeTests`; all list-capturing sinks |
| New code >= 90% coverage, no repo-wide regression | — | `final-qc-coverage-delta`: 100% new-code; UtilitiesCS 85.46->85.48, TaskMaster 49.41->50.21 |
| All three subclasses override the seam | `TestableApplicationGlobals.cs`, `ApplicationGlobalsStartupTimingTests.cs`, `ContinuationProbeSequenceTests.cs` (P4-T5) | grep confirms `SampleStoreWrapperInitTotalMs` override in all three; tests pass |
| All touched/new files <= 500 lines | — | `final-qc-filesize`: max 464 (`ApplicationGlobals.cs`); `ApplicationGlobalsTests.cs` 500->429 |
| Stopwatch only; no banned APIs; net48 | — | `final-qc-analyzers` (no RS0030 on new code); grep of `StoreWrapper.cs` for banned APIs = none |
| Full toolchain in order | — | `final-qc-csharpier`, `final-qc-analyzers`, `final-qc-nullable`, `final-qc-tests-coverage` (all PASS) |

### Runtime portion (maintainer-gated, NOT automated)

`[~] AC18 (runtime, maintainer)` remains pending. Capture instructions:
`evidence/other/coldstart-store-wrapper-init-capture-instructions-2026-06-24T16-30.md` (P5-T1).
Placeholder: `evidence/other/runtime-capture-store-wrapper-init-PLACEHOLDER.md` (P5-T2).

## Conclusion

- spec.md contains AC18 (automated `[x]` + runtime `[~]`).
- Automated portion mapped to P1/P2/P3/P4/P6 evidence and verified.
- Runtime portion is maintainer-gated (P5 artifacts present).
