# Code Review: debug-startup-timing-instrumentation (Issue #202)

**Review Date:** 2026-06-15
**Reviewer:** feature-reviewer agent
**Feature Folder:** `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202`
**Feature Folder Selection Rule:** Folder suffix `-202` matches the issue number in the branch name `feature/debug-startup-timing-instrumentation-202`; it holds the material scoping-doc changes.
**Base Branch:** `main` (`a21d09e18dfebb9e3450c1b3322e7715c09d91e6`)
**Head Branch:** `feature/debug-startup-timing-instrumentation-202` (`253270ac6dbc94bd5b97de1d98a79938f9575040`)
**Review Type:** Cycle-exit re-review (after remediation cycle that split the over-limit test file)

---

## Executive Summary

This branch adds enable-on-demand startup-timing instrumentation to the TaskMaster Outlook VSTO add-in. A new `Settings.Default.StartupTimingEnabled` user setting (default `False`) gates the feature. When enabled, `ApplicationGlobals.LoadAsync(parallel: false)` measures wall-clock time for the seven established startup phase seams (LoadBasic, IntelConfig, OlObjects, ToDo, AutoFile, Engines, Events) and emits a single `[Startup timing]` table via the existing log4net logger. Recording and formatting are isolated behind a new COM-free `IStartupTimingRecorder` abstraction with a production `StartupTimingRecorder` and a `NullStartupTimingRecorder` no-op selected on the flag-off path.

**What changed since the prior review:**
This is the re-review after a remediation cycle. The prior cycle's single Major finding — `ApplicationGlobalsTests.cs` at 687 lines, over the 500-line limit — has been remediated by extracting the four startup-timing wiring tests and their helpers into a new file, `ApplicationGlobalsStartupTimingTests.cs`. At HEAD, `ApplicationGlobalsTests.cs` is 483 lines and `ApplicationGlobalsStartupTimingTests.cs` is 299 lines, both under the limit. The prior cycle's Minor process finding — the absent canonical `artifacts/csharp/coverage.xml` — is also resolved; the artifact is present and was parsed for this review. The split was a pure move (no test removed or weakened); the toolchain re-ran clean (CSharpier, analyzer build, nullable build, MSTest+coverage over 7 assemblies) with 4194/4194 tests passing.

**Scope:** Seven C# source files (plus two `.csproj` and one `.settings`) relative to `main`: two new recorder files, the `ApplicationGlobals` coordinator wiring, the new user setting (settings + generated designer), and three test files (one new recorder test file, the reduced wiring test file, and the new split-out startup-timing wiring test file). The implementation reuses `UtilitiesCS.PrettyPrinters.ToFormattedText` for column alignment and uses `Stopwatch` (avoiding the banned `DateTime.Now`/`UtcNow`).

**Top risks (residual):**
1. The wiring tests mutate the process-global `Settings.Default` singleton and attach an appender to the process-global log4net logger; correctness depends on the `[DoNotParallelize]` markers and save/restore being respected across the whole test process. This is mitigated (markers and save/restore are preserved in the split file) but remains a shared-global-state pattern to watch (Minor; informational).
2. On the `parallel: true` path only LoadBasic is recorded; this is an explicit user-story Non-Goal, not an oversight (Info).

No Blocker findings. No Major findings. No remaining policy violations.

**PR readiness recommendation:** **Ready for merge** — Both prior findings are resolved; the implementation is correct, well-tested, toolchain-clean, and policy-compliant.

---

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Resolved | `TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs` | whole file | Prior-cycle Major (687 lines, over 500-line limit) is resolved by the remediation split: now 483 lines. | None — resolved. | General Code Change Policy §4 500-line limit now satisfied. | `awk 'END{print NR}'` at HEAD = 483; new split file `ApplicationGlobalsStartupTimingTests.cs` = 299; `evidence/qa-gates/post-split-linecounts.2026-06-15T13-29.md`. |
| Resolved | (build/process) | `artifacts/csharp/coverage.xml` | Prior-cycle Minor (canonical C# coverage artifact absent) is resolved; the artifact is present. | None — resolved. | Workflow names this artifact; it now exists and parses to the verified figures. | `ls -la artifacts/csharp/coverage.xml` -> present (21,499,084 bytes); `evidence/qa-gates/coverage-artifact-copy.2026-06-15T13-29.md`. |
| Minor | `TaskMaster.Test/AppGlobals/ApplicationGlobalsStartupTimingTests.cs` | `[DoNotParallelize]` wiring tests | Tests mutate process-global `Settings.Default.StartupTimingEnabled` and attach an appender to the process-global log4net logger. | None required — `[DoNotParallelize]` markers and `[TestInitialize]`/`[TestCleanup]` save/restore are preserved in the split file, isolating the global state. Keep this pattern intact in future edits. | Shared-global-state pattern; mitigated but worth noting. | Lines 21-41, 43-47 of the split file; `DetachMemoryAppender` in `finally` blocks. |
| Info | `TaskMaster/AppGlobals/ApplicationGlobals.cs` | `LoadAsync` parallel branch | On the `parallel: true` path only LoadBasic is recorded; `EmitTable` would emit a single-row table when the flag is on. | None required — `LoadAsync(parallel: true)` / `LoadParallelAsync` instrumentation is an explicit user-story Non-Goal; the startup entry point uses `parallel: false`. | Confirms scope boundary is intentional. | `user-story.md` Non-Goals; diff of `LoadParallelAsync` (unchanged). |
| Info | `TaskMaster/AppGlobals/ApplicationGlobals.cs` | `protected internal virtual LoadBasicMethod` | Visibility widened from `private` to `protected internal virtual` to provide a test seam. | None — minimal, documented seam; production behavior unchanged. | Confirms the API-surface change is a deliberate, narrow test seam. | Diff + inline comment; `TestableApplicationGlobals` override in the split file. |

No Blocker findings. No Major findings.

---

## Implementation Audit

### C# implementation audit

#### What changed well

- Clean separation of concerns: pure recording/formatting (`StartupTimingRecorder`) is fully decoupled from the COM-bound `ApplicationGlobals` coordinator and has no Outlook/COM/filesystem/network dependency, making it unit-testable without a live Outlook process.
- The recorder deliberately does not wrap `SegmentStopWatch.GetDurations()` and documents why (its TOTAL derives from the watch's own `Elapsed`, which is zero for injected spans), while still reusing the existing `PrettyPrinters.ToFormattedText` alignment primitive rather than reimplementing column layout. This is a sound reuse-vs-correctness tradeoff with a clear rationale comment.
- The Null Object pattern (`NullStartupTimingRecorder`) lets the coordinator record and emit unconditionally, keeping the flag-off path branch-free at the call sites and guaranteeing zero output when disabled.
- `Stopwatch` is used for measurement, explicitly avoiding the BannedApiAnalyzers `DateTime.Now`/`UtcNow` rule, and the shared-stopwatch `StopAndRestart` helper cleanly excludes the inter-phase yields from per-phase timing.
- The LoadBasic-at-construction measurement subtlety (the `BasicLoaded` Lazy materializes before `LoadAsync`) is correctly handled by measuring inside `LoadBasicMethod` and recording the stored elapsed as the first phase, with a comment explaining why measuring in `LoadAsync` would record ~0.

#### Remediation quality (this cycle)

- The test-file split is a pure move: the four `[DoNotParallelize]` wiring tests plus the helpers `SetEnginesMock`, `AttachMemoryAppender`, `DetachMemoryAppender`, and the timing-only members of `TestableApplicationGlobals` (the `TimingRecorder` observation seam and the `LoadBasicMethod` override) were moved into the new file, and the now-unused `using` directives were removed from the original. The original retained tests use `LoadSequentialAsync`/`InitializeEnginesPhaseAsync` rather than `LoadAsync`, so the moved seam members were genuinely unused there. The split file re-establishes the necessary `using` directives, the settings save/restore, and the `[DoNotParallelize]` markers. No test was deleted or weakened; the suite count remains 4194 (`qa-test.2026-06-15T13-29.md`).

#### Type safety and API notes

- Nullable reference types respected; `RecordPhase` and `EmitTable` guard null inputs with `ArgumentNullException` and explicit `nameof`. The nullable/TreatWarningsAsErrors build passes (EXIT_CODE 0).
- New public-ish surface is minimized: recorder types are `internal sealed`, exposed to tests only via the existing `InternalsVisibleTo`. The one widened member (`LoadBasicMethod` to `protected internal virtual`) is a documented test seam.
- XML doc comments on the interface and classes state contracts and the COM-free guarantee.

#### Error handling and logging

- Logging uses the existing `ApplicationGlobals` log4net logger via `logger.Info(...)` with the `[Startup timing]` prefix, consistent with the prior #139/#141 entries. No new logging channel introduced.
- Fail-fast on null arguments; no broad catches added. The emission is a single `Info` call at end of `LoadAsync`, so a flag-on run produces exactly one table.

---

## Test Quality Audit

The change is covered by 11 MSTest tests (7 recorder, 4 wiring) using MSTest + Moq + FluentAssertions per the C# Unit Test Policy. Recorder tests inject deterministic spans (no clock/sleep). Wiring tests drive `LoadAsync` end-to-end through `TestableApplicationGlobals`, asserting flag-off no-emit, flag-on phase order with LoadBasic first, exactly-one-table emission with all phase names and TOTAL, and ordering/yield-count parity between flag on and off. New-code line coverage is 100%; the modified `ApplicationGlobals` class improved from 74.4% (99/133) baseline to 77.9% (120/154) with no regression on changed lines.

### Reviewed test and QA artifacts

- `TaskMaster.Test/AppGlobals/StartupTimingRecorderTests.cs` (184 lines) — recorder ordering, zero-duration, summed TOTAL, null-name/null-logger throws, emit prefix, null-recorder no-op. Deterministic, isolated.
- `TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs` (483 lines) — retained pre-existing tests; reduced below the 500-line limit by the split.
- `TaskMaster.Test/AppGlobals/ApplicationGlobalsStartupTimingTests.cs` (299 lines) — flag-on/off wiring via a process-local `MemoryAppender`; `[DoNotParallelize]` + settings save/restore. Correct and within the size limit.
- `evidence/qa-gates/qa-test.2026-06-15T13-29.md` — 4194/4194 pass; numeric coverage; confirms the four wiring tests pass under the new class.
- `evidence/qa-gates/coverage-delta.2026-06-15T13-29.md` — split did not reduce coverage; figures equal baseline within rounding.
- `artifacts/csharp/coverage.xml` — parsed directly: root line-rate 0.7637; new recorder classes 100% (48/48, 10/10); `ApplicationGlobals` class 120/154.

### Quality assessment prompts

- **Determinism:** No real clock or sleep; all spans injected; appender read in-process.
- **Isolation:** Each test targets one behavior; shared-global tests marked `[DoNotParallelize]` and restore state.
- **Speed:** No timing waits; full suite reported green with EXIT_CODE 0 (~48 s).
- **Diagnostics:** FluentAssertions with `because` rationale strings yield actionable failure messages.

---

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets in code | PASS | Diff inspection: no credentials, tokens, or connection strings. |
| No unsafe subprocess or command construction | PASS | No process invocation introduced. |
| Input validation at boundaries | PASS | `RecordPhase`/`EmitTable` validate null arguments and throw. |
| Error handling remains explicit | PASS | Fail-fast null guards; no broad catches; single explicit log emission. |
| Configuration / path handling is safe | PASS | New setting is a typed boolean user setting (default False); no path construction. |
| Banned-API compliance | PASS | `Stopwatch` used; no `DateTime.Now`/`UtcNow`/`Random.Shared`/`Thread.Sleep`/`Task.Delay` call sites introduced (diff scan matched only a comment). |

---

## Research Log

No external research was required. The review relied on the branch diff, the feature-folder evidence artifacts, the parsed canonical Cobertura artifact `artifacts/csharp/coverage.xml`, the baseline `TestResults/baseline-full.cobertura.xml`, the prior-cycle review artifacts, and the repository policy documents (CLAUDE.md, `.claude/rules/*`).

---

## Verdict

The implementation is correct, well-structured, well-documented, and well-tested: COM-free recorder abstraction, Null Object for the flag-off path, deliberate formatter reuse, banned-API avoidance, 100% new-code coverage, and no coverage regression, with a clean four-step toolchain pass. Both findings from the prior review are resolved: the test-file size violation is fixed by a pure split (483 + 299 lines, both under 500), and the canonical `artifacts/csharp/coverage.xml` is present. No Blocker or Major findings remain; the two residual items are Minor/Info and require no action. Recommendation: **Ready for merge.**
