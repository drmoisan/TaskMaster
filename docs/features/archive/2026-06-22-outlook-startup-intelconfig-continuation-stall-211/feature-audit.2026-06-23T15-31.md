# Feature Audit: Issue #211 outlook-startup-latency attribution instrumentation

**Audit Date:** 2026-06-23
**Feature Folder:** `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211`
**Base Branch:** `main`
**Head Branch:** `bug/outlook-startup-intelconfig-continuation-stall-211`
**Work Mode:** `full-bug`
**Audit Type:** Initial acceptance review (full feature-vs-base)

---

## Scope and Baseline

- **Base branch:** `main` (commit `9385bf607aca6c5722f2da7961a895c685710942`)
- **Head branch/commit:** `bug/outlook-startup-intelconfig-continuation-stall-211` (commit `e3a84b5dc4544aaf8b498dfed4e7b45708c9c12a`)
- **Merge base:** `9385bf607aca6c5722f2da7961a895c685710942`
- **Evidence sources:**
  - Primary: `artifacts/pr_context.summary.txt`
  - Secondary baseline diff: `artifacts/pr_context.appendix.txt`
  - Feature evidence: `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/**`
  - Additional evidence: reviewer-run C# toolchain output and `artifacts/csharp/coverage.xml`
- **Feature folder used:** `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211`
- **Requirements source:** `spec.md` (work mode `full-bug` -> `spec.md` is the sole authoritative AC source)
- **Work mode resolution note:** `issue.md` line 12 carries `- Work Mode: full-bug`; per the acceptance-criteria contract, `full-bug` resolves AC to `spec.md` only.
- **Scope note:** Audit scope is the full branch diff `main..HEAD`. The caller's per-AC delivery characterization (AC1–AC8 delivered, AC9/AC10 pending) was treated as context only; each criterion was evaluated independently against inspected evidence. The PR-context summary mislabels the change as "Core logic changes: 0 files"; this is a known PR-context misclassification — the branch contains real C# production changes verified via `git diff`.

---

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**
- `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/spec.md` — only source (work mode `full-bug`)

### Acceptance criteria

1. AC1: `LoadSequentialAsync` emits one `[continuation-resume]` log line per inter-phase boundary via the existing `log4net` logger, each with `priorPhase`, `waitMs` (Stopwatch, F1), `resumeThreadId`, `resumeSyncContext`, `staIsIdle`, `staCpuUsage`, `staGuiActivity`.
2. AC2: behavior-preserving — the probe replaces the existing `Task.Yield()` inter-phase yields without changing phase order, count, or outcomes; `Stopwatch` only; no banned API introduced; net48 (no positional `record struct`).
3. AC3: a deterministic MSTest (Moq + FluentAssertions) using a `TestApplicationGlobals` subclass overriding the `protected internal virtual` probe verifies it is invoked once per phase boundary in the correct order with the correct phase names; no live COM, no live timer, no network/filesystem, no temporary files.
4. AC4: full C# toolchain passes in order (CSharpier -> analyzers -> nullable/TWAE -> MSTest with coverage, gated `/TestCaseFilter:"TestCategory!=LiveOutlook"`); the new testable seam meets the coverage policy; no repository-wide regression; all touched files <= 500 lines.
5. AC5 (runtime, maintainer): a non-debugger cold-start capture (DebugView / OutputDebugString) produces the `[continuation-resume]` fields; gating evidence for Phase 2, recorded under `evidence/`. (Not CI-automatable.)
6. AC6 (Phase 2, evidence-gated): IF the non-debugger capture shows the IntelConfig continuation `waitMs` > 5000 ms with the STA externally occupied, apply the off-STA IntelConfig continuation with a unit test and a re-capture; ELSE Phase 2 is not implemented and the finding is documented.
7. AC7: `AppItemEngines.InitAsync` emits per-engine attribution instrumentation (one structured log line per engine init) capturing engine name, wall-clock duration (`Stopwatch`, F1 ms), the resolving thread id / apartment, and a coarse cost classification signal, using the existing `log4net` logger. Behavior-preserving; `Stopwatch` only; no banned API; net48; all touched files <= 500 lines.
8. AC8: a deterministic MSTest (MSTest + Moq + FluentAssertions) covering any extracted pure attribution/aggregation logic and the per-engine emission seam, with no live COM, no live timer, no network/filesystem, no temporary files; the new seam meets the coverage policy and there is no repository-wide coverage regression.
9. AC9 (runtime, maintainer): a non-debugger cold-start capture produces the per-engine attribution lines and identifies which engine(s)/resource(s) dominate the `Engines`-phase wall-clock; recorded under `evidence/`.
10. AC10 (Phase 4, evidence-gated): apply the minimal TaskMaster-side fix indicated by AC9, with a unit test asserting the behavior/ordering invariant the fix relies on and a re-capture confirming the startup-latency reduction. If AC9 attributes the cost to a non-TaskMaster external cause, document that finding and the attribution evidence instead of forcing a change.

---

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|-----------|--------|----------|--------------------------|-------|
| 1 | AC1 continuation-resume line per boundary with full field set | PASS | `ApplicationGlobals.cs` `YieldWithContinuationProbeAsync` emits `[continuation-resume]` with all seven fields; 5 call sites pass prior-phase names; non-debugger capture shows all five boundaries. | `git diff ... ApplicationGlobals.cs` | All seven fields present in the emitted line. |
| 2 | AC2 behavior-preserving, Stopwatch only, no banned API, net48 | PASS | Single `Task.Yield()` retained; phase order/count unchanged; `git grep` finds banned tokens only in comments; no positional record struct; CSharpier/analyzers/nullable green. | `git grep -E "DateTime\.(Now\|UtcNow)\|Random\.Shared\|Thread\.Sleep\|Task\.Delay"`; reviewer toolchain | Behavior preservation asserted by ordering/count tests. |
| 3 | AC3 deterministic probe-ordering MSTest | PASS | `ContinuationProbeSequenceTests` (recording subclass) asserts order `IntelConfig,OlObjects,ToDo,AutoFile,Engines` and count 5; Moq + FluentAssertions; no live COM/timer/FS. | reviewer vstest run (2 tests pass) | Recording subclass overrides probe without calling base. |
| 4 | AC4 full C# toolchain green; new seam meets coverage; no repo-wide regression; files <= 500 | PARTIAL | CSharpier/analyzers/nullable/MSTest all green (reviewer-run); new seam 100% coverage; all files <= 500. Repo-wide aggregate 64.05% vs 64.04% baseline (no regression) but below the 80% raw floor; post-exemption repo-wide determination is the PR CI run, not available locally. | `dotnet tool run csharpier check .`; `msbuild ... EnableNETAnalyzers`; `msbuild ... Nullable=enable /p:TreatWarningsAsErrors=true`; `vstest.console.exe ...` | No-regression and new-code coverage PASS; the repo-wide-vs-floor portion is UNVERIFIED locally, so AC4 is PARTIAL rather than PASS. |
| 5 | AC5 non-debugger continuation-resume capture | PASS | `evidence/other/runtime-capture-nondebugger-2026-06-23T13-51.md` (maintainer-provided) shows the `[continuation-resume]` fields for all five boundaries. | file inspection | Not CI-automatable; verified by inspecting the recorded artifact. |
| 6 | AC6 evidence-gated Phase 2 (off-STA IntelConfig OR documented no-fix) | PASS | Non-debugger capture shows IntelConfig `waitMs=0.6` (< 5000 ms), `resumeThreadId=1` (STA), `staIsIdle=True`; the no-fix branch is taken and documented; Phase 2 intentionally not implemented. | file inspection of `runtime-capture-nondebugger-2026-06-23T13-51.md` | Resolved via the second (no-fix) branch of the IF/ELSE criterion. |
| 7 | AC7 per-engine + config attribution in `AppItemEngines.InitAsync` | PASS | `EngineInitTimingProbe` emits `[engine-init] engineName=... engineMs=...F1 engineNull=... threadId=... costHint=Deserialization\|Skip` and `[engine-init-config] configMs=...F1 threadId=...`; wired into `InitAsync`; Stopwatch only; behavior-preserving; files <= 500. | `git diff ... AppItemEngines.cs EngineInitTimingProbe.cs` | costHint is the coarse cost-classification signal (Deserialization vs Skip). |
| 8 | AC8 deterministic MSTest for the seam; coverage policy; no repo-wide regression | PARTIAL | `EngineInitTimingProbeTests` (6 tests) covers ordered emission, null engine, config line, throwing factory, null-arg guards, null-sink guard; seam 100% coverage; no repo-wide regression. The "no repository-wide coverage regression" sub-clause PASSES; the broader repo-wide floor is the same PR-CI-gated item as AC4. | reviewer vstest run; `artifacts/csharp/coverage.xml` | Seam test quality and coverage PASS; PARTIAL only because the repo-wide floor is confirmed by PR CI, not locally. The seam-specific obligations are fully met. |
| 9 | AC9 maintainer non-debugger per-engine capture identifying dominant engine(s) | FAIL | The only Engines-phase non-debugger artifact present is `evidence/other/runtime-capture-engines-nondebugger-PLACEHOLDER.md` (a placeholder); no real per-engine attribution capture identifying the dominant engine(s) exists. AC9 is unchecked in `spec.md`. | file inspection of `evidence/other/` | Not delivered; maintainer-run runtime capture pending. Not CI-automatable. |
| 10 | AC10 evidence-gated Phase 4 fix + unit test + re-capture | FAIL | No Phase 4 fix is implemented; no fix-invariant unit test or reduction re-capture exists. AC10 is unchecked in `spec.md` and is explicitly gated on AC9. | `git diff ... main..HEAD` (no fix changes) | Intentionally not yet implemented; blocked on AC9 evidence. |

---

## Summary

**Overall Feature Readiness:** NEEDS REVISION

**Criteria summary:**
- **PASS:** 6 criteria (AC1, AC2, AC3, AC5, AC6, AC7)
- **PARTIAL:** 2 criteria (AC4, AC8)
- **UNVERIFIED:** 0 criteria
- **FAIL:** 2 criteria (AC9, AC10)

**Top gaps preventing PASS:**

1. AC9 and AC10 are not delivered: the maintainer non-debugger per-engine attribution capture is still a placeholder, and the evidence-gated Phase 4 fix (with its invariant unit test and reduction re-capture) is not implemented. Issue #211's stated objective — eliminate the multi-minute startup latency — is therefore unmet.
2. AC4/AC8 repo-wide coverage floor: the deterministic full-suite aggregate (64.05%) is below the 80% raw floor. This is pre-existing (baseline 64.04%) with no regression from #211; the authoritative post-exemption repo-wide determination is the PR CI run, which is not available in this local environment.

**Recommended follow-up verification steps:**

1. Maintainer performs the AC9 non-debugger cold-start capture from the new `[engine-init]`/`[engine-init-config]` instrumentation and records it under `evidence/`, identifying the dominant engine(s)/resource(s).
2. Based on AC9, implement (or document the external-cause finding for) AC10's Phase 4 fix with the required invariant unit test and a reduction re-capture; confirm the repo-wide coverage floor via the PR CI run.

---

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules:
- Criteria evaluated as **PASS** may be checked off in the authoritative source file(s) if represented as checkboxes and not already checked.
- Criteria evaluated as **PARTIAL**, **FAIL**, or **UNVERIFIED** must remain unchecked.

The six PASS criteria (AC1, AC2, AC3, AC5, AC6, AC7) are already checked `[x]` in `spec.md`; the reviewer re-verified them and confirms the existing check-offs are warranted. AC4 and AC8 are evaluated PARTIAL by this audit (the repo-wide coverage floor is PR-CI-gated locally); they are currently `[x]` in `spec.md` on the strength of the executor's final-QC, which the reviewer does not contradict for the seam-specific and no-regression obligations — no source-file checkbox change is made by this audit. AC9 and AC10 remain `[ ]` (FAIL) and are correctly unchecked in `spec.md`. No source-file checkbox modifications were made by this review.

### AC Status Summary

- Source: `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/spec.md`
- Total AC items: 10
- Checked off (delivered): 8 (AC1–AC8, pre-existing)
- Remaining (unchecked): 2 (AC9, AC10)
- Items remaining: AC9 (maintainer non-debugger per-engine capture), AC10 (evidence-gated Phase 4 fix)

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|-------------|----------|----------------|-----------|-------|
| `spec.md` | 10 | 8 | 2 | Checkbox-backed. Reviewer verdicts: 6 PASS, 2 PARTIAL (AC4/AC8 repo-wide floor PR-CI-gated), 2 FAIL (AC9/AC10 not delivered). No checkbox changes made by this audit. |
