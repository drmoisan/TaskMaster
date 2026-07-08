# Feature Audit: Issue #211 outlook-startup-latency diagnostics + AC10 junk-navigation fix (#211)

**Audit Date:** 2026-06-24
**Feature Folder:** `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211`
**Base Branch:** `main`
**Head Branch:** `bug/outlook-startup-latency-211` (`6d6209f0`)
**Work Mode:** `full-bug`
**Audit Type:** Initial acceptance review (full branch diff vs base)

---

## Scope and Baseline

- **Base branch:** `main` (commit `168eba0ba1f79290be9eda29edc4332ac1ce2061`)
- **Head branch/commit:** `bug/outlook-startup-latency-211` (commit `6d6209f0bcbe331fcf103bb9007e8aac88c29a20`)
- **Merge base:** `9385bf607aca6c5722f2da7961a895c685710942`
- **Evidence sources:**
  - Primary: `artifacts/pr_context.summary.txt`
  - Secondary baseline diff: `artifacts/pr_context.appendix.txt`
  - Feature evidence: `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/**`
  - Additional evidence: direct `git diff` inspection; live C# toolchain runs (csharpier/analyzers/nullable/vstest); parsed baseline + post-change Cobertura coverage.
- **Feature folder used:** `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211`
- **Requirements source:** `spec.md` (work mode `full-bug`)
- **Work mode resolution note:** `issue.md` contains the explicit marker `- Work Mode: full-bug`; per the work-mode contract, `spec.md` is the sole authoritative AC source.
- **Scope note:** The audit scope is the full branch diff `main..HEAD`, not any plan/phase subset. The PR-context summary misclassified the C# production changes as docs ("0 core logic files"); scope was verified from `git diff` (17 production `.cs` files). `spec.md` defines AC1–AC10, AC16, AC17, AC18; there are no AC11–AC15 in the spec. Several runtime ACs are maintainer-gated (live Outlook host required, not CI-automatable).

---

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**
- `spec.md` — only source (work mode `full-bug`)

### Acceptance criteria (transcribed faithfully; abbreviated where long)

1. **AC1:** `LoadSequentialAsync` emits one `[continuation-resume]` line per inter-phase boundary with `priorPhase`, `waitMs`, `resumeThreadId`, `resumeSyncContext`, `staIsIdle`, `staCpuUsage`, `staGuiActivity`.
2. **AC2:** Behavior-preserving — probe replaces existing `Task.Yield()` inter-phase yields without changing phase order/count/outcomes; Stopwatch only; no banned API; net48.
3. **AC3:** Deterministic MSTest (Moq + FluentAssertions) using a `TestApplicationGlobals` subclass overriding the `protected internal virtual` probe verifies once-per-boundary invocation in correct order; no live COM/timer/network/filesystem/temp files.
4. **AC4:** Full C# toolchain passes in order; new seam meets coverage policy; no repo-wide regression; all touched files ≤500 lines.
5. **AC5 (runtime, maintainer):** Non-debugger cold-start capture produces `[continuation-resume]` fields; gating evidence for Phase 2; recorded under `evidence/`. (Not CI-automatable.)
6. **AC6 (Phase 2, evidence-gated):** If non-debugger capture shows IntelConfig continuation `waitMs` > 5000 ms, apply off-STA continuation; else Phase 2 not implemented and finding documented. Resolved via the second branch (waitMs=0.6 < 5000 ms; stall debugger-only).
7. **AC7:** `AppItemEngines.InitAsync` emits per-engine attribution instrumentation (one structured line per engine init) with engine name, wall-clock, thread/apartment, cost classification; behavior-preserving; Stopwatch only; no banned API; net48; files ≤500.
8. **AC8:** Deterministic MSTest covering extracted attribution/aggregation logic + per-engine emission seam; no live COM/timer/network/filesystem/temp files; new seam meets coverage policy; no repo-wide regression.
9. **AC9 (runtime, maintainer):** Non-debugger cold-start capture produces per-engine attribution lines and identifies dominant engine/resource. — SUPERSEDED/REOPENED (cost is cross-cutting intermittent STA stall, not phase-specific).
10. **AC10 (Phase 4, evidence-gated):** Apply minimal TaskMaster-side fix indicated by AC9, with a unit test asserting the behavior/ordering invariant and a re-capture confirming the latency reduction. — Automated portion verified (JunkFolderPathNavigator direct navigation); runtime re-capture maintainer-gated.
16. **AC16:** Per-store filter attribution probe (`[store-filter]`); behavior-preserving; pure decision + formatter in coverable `StoreFilterAttribution`; deterministic MSTest. — Automated portion verified. (Plus AC16 runtime maintainer-gated.)
17. **AC17:** Per-sub-step + per-folder SpamBayes-init attribution probe (`[spam-init]`); behavior-preserving; coverable `SpamInitTimingProbe`; SpamBayes.cs reduced ≤500 via partial split; deterministic MSTest. — Automated portion verified. (Plus AC17 runtime maintainer-gated.)
18. **AC18:** StoreWrapper-init shared-cost attribution probe (`[store-wrapper-init]`, `[phase-net]`); thread-safe `StoreWrapperInitClock`; per-phase net sampling via `SampleStoreWrapperInitTotalMs` seam; coverable helpers; deterministic MSTest incl. concurrency + clamp. — Automated portion verified. (Plus AC18 runtime maintainer-gated.)

Note: AC11–AC15 are not defined in `spec.md` (intermediate Phase 3.1/3.2/3.3 increments were delivered but their ACs were not retained as numbered items in the spec). They are therefore not evaluable AC items; the underlying probes (`[ui-heartbeat]`, `[gc-delta]`, `[startup-lifetime-heartbeat]`) are reviewed in the code review and policy audit as part of the branch diff.

---

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|-----------|--------|----------|--------------------------|-------|
| AC1 | `[continuation-resume]` per-boundary emission | PASS | `ApplicationGlobals.cs` probe wiring; non-debugger capture `evidence/other/runtime-capture-nondebugger-2026-06-23T13-51.md` shows all five boundaries. | diff inspection; capture file | Implementation commit 72520363. |
| AC2 | Behavior-preserving probe (Stopwatch, no banned API, net48) | PASS | Banned-API diff scan empty; analyzers/nullable builds clean; phase order unchanged. | `git diff ... grep banned`; msbuild gates | |
| AC3 | Deterministic MSTest for probe sequence | PASS | `ContinuationProbeSequenceTests.cs`; 4109/4109 pass. | `vstest.console.exe ... TestCategory!=LiveOutlook` | COM-free, list-capturing. |
| AC4 | Full toolchain passes; coverage; ≤500 lines | PASS | csharpier 0; analyzers 0/0; nullable 0/0; 4109/4109; all changed .cs ≤500 (max 464). New seam ≥90%; no repo-wide regression (61.84%→61.90%). | Appendix B of policy-audit | Note: repo-wide 61.90% < 80% gate is pre-existing (flagged in policy audit), but AC4's literal asks are met (no regression, seam coverage, ≤500). |
| AC5 | Runtime non-debugger `[continuation-resume]` capture (maintainer) | PASS | `evidence/other/runtime-capture-nondebugger-2026-06-23T13-51.md` (maintainer-provided). | n/a (runtime) | Recorded capture exists. |
| AC6 | Evidence-gated off-STA continuation OR documented no-fix | PASS | waitMs=0.6 < 5000 ms; resumeThreadId=1 (STA); stall debugger-only. No-fix branch correctly taken and documented. | capture file | |
| AC7 | Per-engine `[engine-init]` attribution | PASS | `AppItemEngines.InitAsync` uses `EngineInitTimingProbe`; emits `[engine-init]`/`[engine-init-config]`. Behavior-preserving (additive Stopwatch + log). | diff inspection | |
| AC8 | Deterministic MSTest for engine attribution seam | PASS | `EngineInitTimingProbe.cs` 100% (30/30); `EngineInitTimingProbeTests.cs`. | coverage parse; vstest | |
| AC9 | Runtime per-engine capture identifying dominant engine (maintainer) | PARTIAL | Captures exist (`runtime-capture-engines-coldstart-2026-06-23T17-42.md` etc.) but AC9 is marked SUPERSEDED/REOPENED in spec: attribution overturned; cost is cross-cutting intermittent STA stall, not phase-specific. Marked `[~]` in spec. | capture files | Diagnostic data captured but the dominant-engine conclusion did not hold; attribution remains open. |
| AC10 | Minimal fix + invariant unit test + latency re-capture | PARTIAL | Automated portion PASS: `JunkFolderPathNavigator` direct navigation; red-before-green regression (`evidence/regression-testing/{red,green}-run-enumeration-bound-2026-06-24T17-30.md`); 95% new-code coverage; behavior equivalence documented. Runtime re-capture is a maintainer-gated PLACEHOLDER (`evidence/other/runtime-capture-ac10-junk-navigation-PLACEHOLDER.md`). | red/green evidence; coverage; vstest | The "re-capture confirming the startup-latency reduction" clause is not yet satisfied; automated fix + invariant test are. |
| AC16 | `[store-filter]` probe (automated) | PASS | `StoreFilterAttribution.cs` 100% (48/48); `StoreFilterAttributionTests.cs`; behavior-preserving filter (Decide mirrors baseline order). | coverage parse; diff | AC16 runtime portion maintainer-gated (placeholder). |
| AC17 | `[spam-init]` probe + SpamBayes split ≤500 (automated) | PASS | `SpamInitTimingProbe.cs` 100% (18/18); `SpamBayes.cs` 705→446; partials added; tests pass. | line counts; coverage; vstest | AC17 runtime portion maintainer-gated (placeholder). |
| AC18 | `[store-wrapper-init]`/`[phase-net]` probe + thread-safe clock (automated) | PASS | `StoreWrapperInitClock.cs`/`StoreWrapperInitProbe.cs` 100%; `ComputeNetMs` clamp + concurrency tests; `SampleStoreWrapperInitTotalMs` seam overridden in all 3 test subclasses. | coverage parse; vstest | AC18 runtime portion maintainer-gated (placeholder). |

---

## Summary

**Overall Feature Readiness:** NEEDS REVISION

The automated engineering portion of every AC is delivered and verified to a high standard (all four C# toolchain steps clean, 4109/4109 tests, new-code coverage 95–100%, behavior preservation argued, AC10 bugfix workflow honored with red-before-green evidence). However, this is a `full-bug` whose stated goal is to eliminate the multi-minute startup latency, and that outcome is not yet proven:

- **AC9** is explicitly SUPERSEDED/REOPENED in the spec — the per-engine attribution did not converge on a stable dominant cause; the latency is a cross-cutting, intermittent STA stall.
- **AC10** has its automated fix and invariant test complete, but its "re-capture confirming the startup-latency reduction" is a maintainer-gated placeholder. The fix is well-justified by the ~50s JunkCertain cold capture, but its end-to-end effect is unverified at runtime.

The diagnostics + AC10 automated work are mergeable on quality grounds (see code-review Conditional Go). The feature/bug as a whole is NEEDS REVISION because the latency-resolution outcome (AC9 attribution closure + AC10 runtime confirmation) remains open and maintainer-gated.

**Criteria summary:**
- **PASS:** 11 criteria (AC1–AC8, AC16, AC17, AC18)
- **PARTIAL:** 2 criteria (AC9, AC10)
- **UNVERIFIED:** 0 criteria
- **FAIL:** 0 criteria

**Top gaps preventing PASS:**
1. AC9: per-engine attribution superseded; dominant-cause attribution of the latency remains open (cross-cutting intermittent STA stall).
2. AC10: maintainer-gated runtime re-capture confirming the startup-latency reduction is still a placeholder.
3. (Policy, not an AC) Repo-wide C# coverage 61.90% < 80% gate — pre-existing, non-regressing; see policy audit.

**Recommended follow-up verification steps:**
1. Maintainer performs the non-debugger cold-start re-capture per `evidence/other/ac10-coldstart-junk-navigation-recapture-instructions-2026-06-24T17-30.md` to confirm the JunkCertain navigation latency reduction; record the real capture in place of the placeholder.
2. Maintainer captures the remaining `[store-filter]`/`[spam-init]`/`[store-wrapper-init]`/`[phase-net]` runtime lines to settle the reopened AC9 attribution, then update the spec's AC9/AC10 status.

---

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules:
- Criteria evaluated as PASS may be checked off in the authoritative source file(s) if represented as markdown checkboxes and not already checked.
- Criteria evaluated as PARTIAL, FAIL, or UNVERIFIED must remain unchecked.

All PASS criteria (AC1–AC8, AC16, AC17, AC18) are already marked `- [x]` in `spec.md`; no checkbox state change was needed. AC9 and AC10 are already marked `- [~]` (in-progress) in `spec.md`, consistent with their PARTIAL evaluations here; the reviewer did not alter them. The reviewer therefore made no edits to `spec.md` checkboxes — the source file already reflects the verified state.

### AC Status Summary

- Source: `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/spec.md`
- Total AC items: 13 (AC1–AC10, AC16, AC17, AC18; AC11–AC15 not defined)
- Checked off (delivered / PASS): 11 (AC1–AC8, AC16, AC17, AC18)
- Remaining (PARTIAL, unchecked-as-`[x]`): 2 (AC9, AC10 — both marked `[~]`)
- Items remaining: AC9 (attribution reopened/superseded); AC10 (runtime latency-reduction re-capture maintainer-gated)

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|-------------|----------|----------------|-----------|-------|
| `spec.md` | 13 | 11 | 2 | Checkbox-backed; AC9/AC10 marked `[~]` (in-progress), consistent with PARTIAL. No reviewer edits required. |
