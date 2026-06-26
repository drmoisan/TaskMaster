# P5 — Acceptance Criteria Check-Off (AC17) (issue #211)

Timestamp: 2026-06-24T15-10

AC source (work mode `full-bug`): `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/spec.md`

## AC17 — automated portion: VERIFIED (`[x]`)

| AC17 requirement | Evidence | Status |
|---|---|---|
| `SpamBayes.CreateAsync` emits one `[spam-init]` line per sub-step (`ValidatePathsSet`, `ValidateSpamClassifier`, `InitAsync(modelLoad)`), `Stopwatch` F1 ms | P3-T1: instrumentation in `SpamBayes.cs` `CreateAsync` (probe `EmitStep` per sub-step) | PASS |
| `ValidatePathsSet()` emits one `[spam-init]` line per COM folder (`JunkCertain`, `JunkPotential`, `Inbox`) | P3-T2: per-folder `Stopwatch` + `EmitStep` in `SpamBayes.cs` `ValidatePathsSet` | PASS |
| Behavior-preserving (identical validation result, exceptions, engine; deserialize + `PreserveReferencesHandling.All` untouched; existing `[Startup timing]`/`[engine-init]` untouched) | P3-T3 diff review (`final-qc-tests-coverage` shows 0 SpamBayes test regressions; `PreserveReferencesHandling.All` absent from `SpamBayes.cs`, lives in `SmartSerializableLoader.cs` et al., untouched) | PASS |
| `Stopwatch` only; no banned APIs; net48 | Only `System.Diagnostics.Stopwatch` used; grep confirms no `DateTime.Now`/`UtcNow`/`Random.Shared`/`Thread.Sleep`/`Task.Delay` introduced; net48 build clean | PASS |
| Pure formatter in coverable `SpamInitTimingProbe` (NOT `[ExcludeFromCodeCoverage]`), deterministic MSTest, no live COM/timer/filesystem/network, no temp files | P2-T1/T3: `SpamInitTimingProbe.cs` (no exempt attribute) + `SpamInitTimingProbeTests.cs` (13 tests, list-capturing sink) | PASS |
| New code >= 90% coverage; no repository-wide regression | `final-qc-coverage-delta-2026-06-24T15-10.md`: `SpamInitTimingProbe` 100% new-code; repo 60.43% -> 60.47% (no regression) | PASS |
| All touched/new files <= 500 lines (CONSTRAINT-A: `SpamBayes.cs` <= 500 via three partial extractions) | `final-qc-filesize-2026-06-24T15-10.md`: SpamBayes.cs 446; Conditions 100; Actions 117; Classify 121; Probe 81; Tests 214 | PASS |
| Full C# toolchain passes in order (CSharpier -> analyzers -> nullable/TWAE -> MSTest with coverage, `TestCategory!=LiveOutlook`) | `final-qc-csharpier`, `final-qc-analyzers`, `final-qc-nullable`, `final-qc-tests-coverage` (all 2026-06-24T15-10): clean pass in one loop | PASS |

Automated portion of AC17 marked `[x]` in `spec.md`.

## AC17 — runtime portion: MAINTAINER-GATED (`[~]`)

The runtime cold-start capture of the six `[spam-init]` lines requires a live, non-debugger slow
Outlook startup and cannot be automated in CI. It is marked `[~]` in `spec.md` and gated on:
- Capture instructions: `evidence/other/coldstart-spam-init-capture-instructions-2026-06-24T15-10.md` (P4-T1).
- Evidence placeholder: `evidence/other/runtime-capture-spam-init-PLACEHOLDER.md` (P4-T2).

## Notes

- This work is diagnosis-only and behavior-preserving; it applies NO fix (consistent with AC10 still
  gated and the plan scope).
- AC17 was authored by this plan (Acceptance Criterion AC17 section) and added to `spec.md` per P5-T7.
