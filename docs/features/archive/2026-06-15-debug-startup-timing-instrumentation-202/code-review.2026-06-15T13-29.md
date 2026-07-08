# Code Review: debug-startup-timing-instrumentation (Issue #202)

**Review Date:** 2026-06-15
**Reviewer:** feature-reviewer agent
**Feature Folder:** `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202`
**Feature Folder Selection Rule:** Folder suffix `-202` matches the issue number in the branch name `feature/debug-startup-timing-instrumentation-202`; it holds the material scoping-doc changes.
**Base Branch:** `main` (`a21d09e18dfebb9e3450c1b3322e7715c09d91e6`)
**Head Branch:** `feature/debug-startup-timing-instrumentation-202` (`1d193d90dba55eec0a739ff13f5ecb5e3d218b99`)
**Review Type:** Initial review

---

## Executive Summary

This branch adds enable-on-demand startup-timing instrumentation to the TaskMaster Outlook VSTO add-in. A new `Settings.Default.StartupTimingEnabled` user setting (default `False`) gates the feature. When enabled, `ApplicationGlobals.LoadAsync(parallel: false)` measures wall-clock time for the seven established startup phase seams (LoadBasic, IntelConfig, OlObjects, ToDo, AutoFile, Engines, Events) and emits a single `[Startup timing]` table via the existing log4net logger. Recording and formatting are isolated behind a new COM-free `IStartupTimingRecorder` abstraction with a production `StartupTimingRecorder` and a `NullStartupTimingRecorder` no-op selected on the flag-off path.

**What changed:**
Six C# source files (plus two `.csproj` and one `.settings`) relative to `main`: two new recorder files, the `ApplicationGlobals` coordinator wiring, the new user setting (settings + generated designer), and two test files (one new recorder test file, one extended wiring test file). Net +1685/-2 lines, dominated by tests and documentation. The implementation reuses `UtilitiesCS.PrettyPrinters.ToFormattedText` for column alignment and uses `Stopwatch` (avoiding the banned `DateTime.Now`/`UtcNow`). The toolchain (CSharpier, analyzer build, nullable/TreatWarningsAsErrors build, MSTest+coverage over 7 assemblies) is recorded EXIT_CODE 0 with 4194/4194 tests passing.

**Top 3 risks:**
1. The modified test file `ApplicationGlobalsTests.cs` is 687 lines, exceeding the repository 500-line file-size limit (Major; policy violation).
2. The wiring tests mutate the process-global `Settings.Default` singleton and attach an appender to the process-global log4net logger; correctness depends on the `[DoNotParallelize]` markers and save/restore being respected across the whole test process (mitigated, but a shared-global-state pattern to watch).
3. The canonical C# coverage artifact `artifacts/csharp/coverage.xml` named by the workflow is absent; coverage was verified from `TestResults/final-full.cobertura.xml` instead (Minor/process).

**PR readiness recommendation:** **Needs Revision** — The implementation is correct and well-tested, but the 500-line test-file limit must be resolved before merge.

---

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Major | `TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs` | whole file (687 lines) | File exceeds the repository 500-line limit (baseline 440 -> 687). | Move the four startup-timing wiring tests and their helpers (`AttachMemoryAppender`, `DetachMemoryAppender`, `SetEnginesMock`, the timing-related `TestableApplicationGlobals` members) into a new file, e.g. `ApplicationGlobalsStartupTimingTests.cs`, keeping each file under 500 lines. | General Code Change Policy §4 / `.claude/rules/general-code-change.md` applies the 500-line limit to test code. | `awk 'END{print NR}'` = 687; baseline `git show a21d09e1:...` = 440. |
| Minor | (build/process) | `artifacts/csharp/coverage.xml` | Canonical C# coverage artifact path expected by the feature-review-workflow is absent. | Emit or copy the merged Cobertura output to `artifacts/csharp/coverage.xml` so the workflow's artifact contract is satisfied. | Workflow names this artifact for C# coverage verification; absence forces fallback parsing. | `ls artifacts/csharp/` -> no such directory; data exists at `TestResults/final-full.cobertura.xml`. |
| Info | `TaskMaster/AppGlobals/ApplicationGlobals.cs` | `LoadAsync` parallel branch | On the `parallel: true` path only LoadBasic is recorded (no per-phase spans); `EmitTable` would emit a single-row table when the flag is on. | None required — `LoadAsync(parallel: true)` / `LoadParallelAsync` instrumentation is an explicit user-story Non-Goal; the startup entry point uses `parallel: false`. Optionally document that a parallel-path table is LoadBasic-only. | Confirms scope boundary is intentional, not an oversight. | `user-story.md` Non-Goals; diff of `LoadParallelAsync` (unchanged). |
| Info | `TaskMaster/AppGlobals/ApplicationGlobals.cs` | `protected internal virtual LoadBasicMethod` | Visibility widened from `private` to `protected internal virtual` to provide a test seam. | None — minimal, documented seam; production behavior unchanged. | Confirms the API-surface change is a deliberate, narrow test seam. | Diff + inline comment; `TestableApplicationGlobals` override. |

No Blocker findings.

---

## Implementation Audit

### C# implementation audit

#### What changed well

- Clean separation of concerns: pure recording/formatting (`StartupTimingRecorder`) is fully decoupled from the COM-bound `ApplicationGlobals` coordinator and has no Outlook/COM/filesystem/network dependency, making it unit-testable without a live Outlook process.
- The recorder deliberately does not wrap `SegmentStopWatch.GetDurations()` and documents why (its TOTAL derives from the watch's own `Elapsed`, which is zero for injected spans), while still reusing the existing `PrettyPrinters.ToFormattedText` alignment primitive rather than reimplementing column layout. This is a good reuse-vs-correctness tradeoff with a clear rationale comment.
- The Null Object pattern (`NullStartupTimingRecorder`) lets the coordinator record and emit unconditionally, keeping the flag-off path branch-free at the call sites and guaranteeing zero output when disabled.
- `Stopwatch` is used for measurement, explicitly avoiding the BannedApiAnalyzers `DateTime.Now`/`UtcNow` rule, and the shared-stopwatch `StopAndRestart` helper cleanly excludes the inter-phase yields from per-phase timing.
- The LoadBasic-at-construction measurement subtlety (the `BasicLoaded` Lazy materializes before `LoadAsync`) is correctly handled by measuring inside `LoadBasicMethod` and recording the stored elapsed as the first phase, with a comment explaining why measuring in `LoadAsync` would record ~0.

#### Type safety and API notes

- Nullable reference types respected; `RecordPhase` and `EmitTable` guard null inputs with `ArgumentNullException` and explicit `nameof`. The nullable/TreatWarningsAsErrors build passes (EXIT_CODE 0).
- New public-ish surface is minimized: recorder types are `internal sealed`, exposed to tests only via the existing `InternalsVisibleTo`. The one widened member (`LoadBasicMethod` to `protected internal virtual`) is a documented test seam.
- XML doc comments on the interface and classes state contracts and the COM-free guarantee.

#### Error handling and logging

- Logging uses the existing `ApplicationGlobals` log4net logger via `logger.Info(...)` with the `[Startup timing]` prefix, consistent with the prior #139/#141 entries and the single-channel decision recorded in evidence. No new logging channel introduced.
- Fail-fast on null arguments; no broad catches added. The emission is a single `Info` call at end of `LoadAsync`, so a flag-on run produces exactly one table.

---

## Test Quality Audit

The change is covered by 11 new MSTest tests (7 recorder, 4 wiring) using MSTest + Moq + FluentAssertions per the C# Unit Test Policy. Recorder tests inject deterministic spans (no clock/sleep). Wiring tests drive `LoadAsync` end-to-end through `TestableApplicationGlobals`, asserting flag-off no-emit, flag-on phase order with LoadBasic first, exactly-one-table emission with all phase names and TOTAL, and ordering/yield-count parity between flag on and off. New-code line coverage is 100%; the modified `ApplicationGlobals` improved from 60.75% to 73.88% aggregate with no regression on changed lines.

### Reviewed test and QA artifacts

- `TaskMaster.Test/AppGlobals/StartupTimingRecorderTests.cs` — recorder ordering, zero-duration, summed TOTAL, null-name/null-logger throws, emit prefix, null-recorder no-op. Deterministic, isolated.
- `TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs` — flag-on/off wiring via a process-local `MemoryAppender`; `[DoNotParallelize]` + settings save/restore. Correct but pushes the file over the 500-line limit.
- `evidence/qa-gates/coverage-delta.2026-06-15T12-15.md` — baseline-vs-post comparison, new-code 100%, no regression.
- `evidence/qa-gates/final-test-coverage.2026-06-15T12-15.md` — 4194/4194 pass; numeric coverage.
- `TestResults/final-full.cobertura.xml` — parsed directly: root line-rate 0.7636; new recorder classes line-rate 1.0; `ApplicationGlobals` class line-rate 0.776.

### Quality assessment prompts

- **Determinism:** No real clock or sleep; all spans injected; appender read in-process.
- **Isolation:** Each test targets one behavior; shared-global tests marked `[DoNotParallelize]` and restore state.
- **Speed:** No timing waits; full suite reported green with EXIT_CODE 0.
- **Diagnostics:** FluentAssertions with `because` rationale strings yield actionable failure messages.

---

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets in code | ✅ PASS | Diff inspection: no credentials, tokens, or connection strings. |
| No unsafe subprocess or command construction | ✅ PASS | No process invocation introduced. |
| Input validation at boundaries | ✅ PASS | `RecordPhase`/`EmitTable` validate null arguments and throw. |
| Error handling remains explicit | ✅ PASS | Fail-fast null guards; no broad catches; single explicit log emission. |
| Configuration / path handling is safe | ✅ PASS | New setting is a typed boolean user setting (default False); no path construction. |

---

## Research Log

No external research was required. The review relied on the branch diff, the feature-folder evidence artifacts, the parsed Cobertura coverage file, and the repository policy documents (CLAUDE.md, `.claude/rules/*`).

---

## Verdict

The implementation is correct, well-structured, well-documented, and well-tested: COM-free recorder abstraction, Null Object for the flag-off path, deliberate formatter reuse, banned-API avoidance, 100% new-code coverage, and no coverage regression, with a clean four-step toolchain pass. One Major policy violation prevents normal PR flow: the modified test file `ApplicationGlobalsTests.cs` (687 lines) exceeds the 500-line file-size limit and must be split. A Minor process gap (absent canonical `artifacts/csharp/coverage.xml`) should be resolved but does not affect coverage verdicts. Recommendation: **Needs Revision** — address the test-file size violation, then this change is ready for normal PR flow.
