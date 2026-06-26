# Feature Audit: outlook-startup-intelconfig-continuation-stall — Phase 1 attribution probe (#211)

**Audit Date:** 2026-06-22
**Feature Folder:** `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211`
**Base Branch:** `origin/main` (base commit `7a8ee65b`)
**Head Branch:** `bug/outlook-startup-intelconfig-continuation-stall-211` (HEAD; substantive commit `96c15b7f`)
**Work Mode:** `full-bug`
**Audit Type:** Initial acceptance review

---

## Scope and Baseline

- **Base branch:** `origin/main` (commit `7a8ee65b`)
- **Head branch/commit:** `bug/outlook-startup-intelconfig-continuation-stall-211` (HEAD `96c15b7f` substantive)
- **Merge base:** `7a8ee65b`
- **Evidence sources:**
  - Primary: `git diff 7a8ee65b...HEAD` and `git log 7a8ee65b..HEAD` (inspected directly)
  - Secondary baseline diff: `git show 7a8ee65b:TaskMaster/AppGlobals/ApplicationGlobals.cs`
  - Feature evidence: `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/**`
  - Additional evidence: `evidence/qa-gates/final-qc-2026-06-22T18-05.md`
- **Feature folder used:** `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211`
- **Requirements source:** `spec.md` (`## Acceptance Criteria`, AC1–AC6)
- **Work mode resolution note:** `issue.md` line 12 declares `- Work Mode: full-bug`. Per the acceptance-criteria-tracking rules, `full-bug` resolves the authoritative AC source to `spec.md` only. The AC list is prose-style checkbox items under `## Acceptance Criteria` in `spec.md`.
- **Scope note:** The audit covers the full branch diff against `7a8ee65b`. Four source files change (`ApplicationGlobals.cs`, two renamed test files, one new test file) plus the csproj include and documentation/evidence. This deliverable is scoped by the spec to Phase 1 (AC1–AC4). AC5 is a maintainer non-debugger runtime capture (not CI-automatable). AC6 is Phase 2 work that is evidence-gated on AC5 and intentionally absent from this change.

---

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**
- `spec.md` — only source (work mode `full-bug`)

### Acceptance criteria

1. **AC1:** `LoadSequentialAsync` emits one `[continuation-resume]` log line per inter-phase boundary via the existing `log4net` logger, each with `priorPhase`, `waitMs` (Stopwatch, F1), `resumeThreadId`, `resumeSyncContext`, `staIsIdle`, `staCpuUsage`, `staGuiActivity`.
2. **AC2:** behavior-preserving — the probe replaces the existing `Task.Yield()` inter-phase yields without changing phase order, count, or outcomes; `Stopwatch` only; no banned API introduced; net48 (no positional `record struct`).
3. **AC3:** a deterministic MSTest (Moq + FluentAssertions) using a `TestApplicationGlobals` subclass overriding the `protected internal virtual` probe verifies it is invoked once per phase boundary in the correct order with the correct phase names; no live COM, no live timer, no network/filesystem, no temporary files.
4. **AC4:** full C# toolchain passes in order (CSharpier -> analyzers -> nullable/TWAE -> MSTest with coverage, gated `/TestCaseFilter:"TestCategory!=LiveOutlook"`); the new testable seam meets the coverage policy; no repository-wide regression; all touched files <= 500 lines.
5. **AC5 (runtime, maintainer):** a non-debugger cold-start capture (DebugView / OutputDebugString) produces the `[continuation-resume]` fields; this is the gating evidence for Phase 2 and is recorded under `evidence/`. (Not CI-automatable.)
6. **AC6 (Phase 2, evidence-gated):** IF the non-debugger capture shows the IntelConfig continuation `waitMs` > 5000 ms with the STA externally occupied, apply the off-STA IntelConfig continuation (`ConfigureAwait(false)` + `await UiThread.UiSyncContext` before `OlObjects`), with a unit test asserting phase ordering is preserved and the `OlObjects` phase resumes on the STA, and a re-capture confirming the reduction. IF the capture shows the stall is debugger-only / not reproduced outside the debugger, Phase 2 is not implemented and the issue closes documenting that finding.

---

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|-----------|--------|----------|--------------------------|-------|
| 1 | AC1 — probe emits one `[continuation-resume]` line per boundary with the seven fields | PASS | `ApplicationGlobals.cs:177-191` emits a single `logger.Debug` line containing `priorPhase`, `waitMs={...:F1}`, `resumeThreadId`, `resumeSyncContext`, `staIsIdle`, `staCpuUsage={...:F3}`, `staGuiActivity={...:F1}`. `logger` is the existing log4net `ILog` field (`ApplicationGlobals.cs:18`). `ApplicationIdleTimer.IsIdle/CurrentCPUUsage/CurrentGUIActivity` exist (`UtilitiesCS/Threading/ApplicationIdleTimer.cs:371,382,392`). The five call sites at lines 140/143/146/149/152 each invoke the probe once per boundary. | `git diff 7a8ee65b...HEAD -- TaskMaster/AppGlobals/ApplicationGlobals.cs` | `waitMs` is the Stopwatch attribution number, formatted `F1` as required. The spec parenthetical "`resumeThreadId` (vs `UiThread.UiThreadId`)" is satisfied by emitting the resuming managed thread id; the comparison against `UiThread.UiThreadId` is left to the human reading the log, which matches the diagnostic intent. |
| 2 | AC2 — behavior-preserving; Stopwatch only; no banned API; net48 | PASS | Baseline body was `await Task.Yield();` with exactly 5 `YieldBetweenStartupPhasesAsync()` call sites (`git show 7a8ee65b:.../ApplicationGlobals.cs`). New body still performs exactly one `await Task.Yield()`; phase order and count (IntelConfig, OlObjects, ToDo, AutoFile, Engines, Events with 5 inter-phase boundaries) are unchanged. No `DateTime.Now/UtcNow`, `Random.Shared`, `Thread.Sleep`, or `Task.Delay` in the file (the only textual match is a comment). No `record struct`. | `grep -nE "DateTime\.(Now\|UtcNow)\|Random\.Shared\|Thread\.Sleep\|Task\.Delay\|record struct" TaskMaster/AppGlobals/ApplicationGlobals.cs` | The probe adds timing measurement and one log line; phase bodies are untouched. |
| 3 | AC3 — deterministic MSTest via overriding subclass; no live COM/timer/fs/temp files | PASS | `ContinuationProbeSequenceTests.cs` defines `RecordingApplicationGlobals : ApplicationGlobals` overriding all six phase wrappers to `Task.CompletedTask` and overriding `YieldWithContinuationProbeAsync` to record the prior-phase name WITHOUT calling base, so the static `ApplicationIdleTimer` reads never execute. Two `[TestMethod] [DoNotParallelize]` tests assert order (`Equal("IntelConfig","OlObjects","ToDo","AutoFile","Engines")`) and count (`HaveCount(5)`). Uses MSTest + Moq (`new Mock<OutlookApplication>().Object`) + FluentAssertions. No filesystem, network, or temp files. | `git diff 7a8ee65b...HEAD -- TaskMaster.Test/AppGlobals/ContinuationProbeSequenceTests.cs` | Final-qc records both tests PASS. The subclass is named `RecordingApplicationGlobals` rather than `TestApplicationGlobals`; the spec's "`TestApplicationGlobals`" is illustrative of the seam pattern, and the override-based seam satisfies the criterion's substance. |
| 4 | AC4 — full toolchain green in order; coverage policy; no repo-wide regression; files <= 500 | PARTIAL (non-blocking) | `evidence/qa-gates/final-qc-2026-06-22T18-05.md`: CSharpier check EXIT 0; analyzers Build succeeded 0 errors / 7 pre-existing CS8632 warnings in untouched test files (corroborated by `evidence/baseline/baseline-analyzers.md` showing CS8632 + CS0067 present at baseline); nullable/TWAE Build succeeded 0 warnings 0 errors; MSTest gated run 4312 total, 4311 passed, 1 failed. File sizes verified: `ApplicationGlobals.cs` 263, `ContinuationProbeSequenceTests.cs` 107, `ApplicationGlobalsStartupTimingTests.cs` 301, `ApplicationGlobalsTests.cs` 485 — all <= 500. | `awk 'END{print NR}' <file>`; `git log -1 -- UtilitiesCS.Test/.../TimedAsyncTask_Tests.cs` | The single failure `UtilitiesCS...TimedAsyncTask_Tests.RequestTask_WithProvidedTask_InvokesTaskAfterInterval` is in a file last modified 2026-03-19 (predates #211) and is NOT in the branch diff; it is a real-interval timer flake that passes in isolation. Assessed as pre-existing and unrelated, not a regression. PARTIAL only because the toolchain final pass was not fully green (one failing test) and repo-wide C# coverage is not numerically evidenced (single-assembly `/EnableCodeCoverage` baseline ~11% is denominator-inflated by construction); neither is attributable to this change. See policy-audit for coverage disposition. |
| 5 | AC5 — maintainer non-debugger runtime capture under `evidence/` | UNVERIFIED (expected) | No `[continuation-resume]` non-debugger capture artifact exists under `evidence/`. The spec explicitly marks AC5 as a maintainer runtime task and "Not CI-automatable." | n/a | Intentionally pending. Not a defect of this deliverable; it is the gating evidence the probe was built to produce. |
| 6 | AC6 — Phase 2 off-STA continuation, evidence-gated | UNVERIFIED (expected) | No `ConfigureAwait(false)` change in `LoadIntelConfigAsync` and no `await UiThread.UiSyncContext` insertion before `OlObjects` in the diff. | `git diff 7a8ee65b...HEAD -- TaskMaster/AppGlobals/ApplicationGlobals.cs` | Correctly absent. AC6 is gated on AC5 per the spec; implementing it now would be premature and out of Phase 1 scope. |

---

## Summary

**Overall Feature Readiness:** PASS (Phase 1 scope)

**Criteria summary:**
- **PASS:** 3 criteria (AC1, AC2, AC3)
- **PARTIAL:** 1 criterion (AC4 — non-blocking; lone failure and coverage-denominator caveat are pre-existing/structural and not attributable to this change)
- **UNVERIFIED:** 2 criteria (AC5, AC6 — both intentionally deferred per the spec's Phase 1 / evidence-gated design)
- **FAIL:** 0 criteria

**Top gaps preventing PASS:**

1. None blocking. AC4 is PARTIAL due to one pre-existing unrelated test flake and a structurally-inflated single-assembly coverage denominator; both are documented and neither is introduced by this change.
2. AC5 and AC6 are deferred by design (maintainer runtime capture gates Phase 2). They are not in this deliverable's scope and are correctly absent.

**Recommended follow-up verification steps:**

1. Maintainer performs the AC5 non-debugger cold-start capture (DebugView/OutputDebugString) and records the `[continuation-resume]` fields under `evidence/`.
2. Confirm repo-wide C# line coverage >= 80% via the PR CI multi-assembly coverage run (the local single-assembly figure is not a valid repo-wide measure).

---

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules:
- Criteria evaluated as **PASS** may be checked off in the authoritative source file(s) if represented as markdown checkboxes and not already checked.
- Criteria evaluated as **PARTIAL**, **FAIL**, or **UNVERIFIED** must remain unchecked.

AC1, AC2, AC3 evaluate PASS and are represented as `- [ ]` checkboxes in `spec.md`. This reviewer does not modify policy/source files beyond AC check-off; per the tracking protocol the reviewer may check off PASS items. However, because `spec.md` AC items remain `- [ ]` and the orchestrator/maintainer manages spec check-off in this repo's workflow, this audit records the PASS status here and leaves the source checkboxes unchanged to avoid contending with maintainer-owned spec edits. No source-file checkbox change was made by this audit.

### AC Status Summary

- Source: `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/spec.md`
- Total AC items: 6
- Checked off (delivered): 0 (left to maintainer/orchestrator; AC1–AC3 verified PASS in this audit)
- Remaining (unchecked): 6
- Items remaining: AC1, AC2, AC3 (verified PASS, eligible for check-off), AC4 (PARTIAL — non-blocking), AC5 (UNVERIFIED — deferred), AC6 (UNVERIFIED — deferred)

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|-------------|----------|----------------|-----------|-------|
| `spec.md` | 6 | 0 | 6 | Checkbox-backed; AC1–AC3 verified PASS and eligible for maintainer check-off; AC4 PARTIAL non-blocking; AC5/AC6 deferred by design |

Blocking findings: 0
