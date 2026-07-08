# Code Review: outlook-startup-intelconfig-continuation-stall — Phase 1 attribution probe (#211)

**Review Date:** 2026-06-22
**Reviewer:** feature-reviewer agent
**Feature Folder:** `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211`
**Feature Folder Selection Rule:** Active feature folder for issue #211, matching the branch under review.
**Base Branch:** `origin/main` (commit `7a8ee65b`)
**Head Branch:** `bug/outlook-startup-intelconfig-continuation-stall-211` (HEAD; substantive commit `96c15b7f`)
**Review Type:** Initial review

---

## Executive Summary

This change adds a behavior-preserving continuation-latency attribution probe to the TaskMaster Outlook add-in startup sequence. The single production-code change is in `TaskMaster/AppGlobals/ApplicationGlobals.cs`: the existing `YieldBetweenStartupPhasesAsync()` (body `await Task.Yield();`) is replaced by `protected internal virtual async Task YieldWithContinuationProbeAsync(string priorPhaseName)`, which wraps the same single `Task.Yield()` in a `Stopwatch` and emits one `[continuation-resume]` log line via the existing log4net logger. The five inter-phase call sites in `LoadSequentialAsync` are updated to pass the preceding phase name. The remaining changes are test-side: a new deterministic `ContinuationProbeSequenceTests.cs`, rename propagation in two existing test files, and the corresponding csproj `<Compile Include>`.

**What changed:**
- `ApplicationGlobals.cs` (263 lines, was 247): method rename + Stopwatch/log instrumentation; five call-site updates in `LoadSequentialAsync`. The single `Task.Yield()` and the phase order/count are preserved exactly.
- `ContinuationProbeSequenceTests.cs` (NEW, 107 lines): two MSTest methods verifying probe invocation order and count via an overriding subclass seam.
- `ApplicationGlobalsStartupTimingTests.cs` (301 lines) and `ApplicationGlobalsTests.cs` (485 lines): override signature rename and source-structure regex updated to the new method name.
- `TaskMaster.Test.csproj`: one `<Compile Include>` line for the new test file.

**Top 3 risks:**
1. The production log-emitting branch of the new probe (the `logger.Debug(...)` line reading static `ApplicationIdleTimer` members) is not exercised by the unit tests; the recording subclass overrides the probe without calling base. This is an intentional determinism trade-off but leaves the field-formatting logic verified only by manual/runtime capture (AC5).
2. Repo-wide C# line coverage is not numerically evidenced at the >= 80% gate; the local single-assembly `/EnableCodeCoverage` baseline (~11%) is denominator-inflated by construction and is not a valid repo-wide measure. Repo-wide coverage must be confirmed by the multi-assembly PR CI run.
3. The full-suite gated run shows one failing test, which is assessed as a pre-existing unrelated flake (verified not in the branch diff, last modified 2026-03-19), but the final toolchain pass was therefore not fully green locally.

**PR readiness recommendation:** **Conditional Go** — The Phase 1 change is correct, behavior-preserving, and policy-compliant. Merge is appropriate once repo-wide C# coverage is confirmed >= 80% by the PR CI coverage run; the single failing test should be tracked as a separate flaky-test follow-up.

---

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Info | `TaskMaster/AppGlobals/ApplicationGlobals.cs` | `177-191` | The new probe's `logger.Debug(...)` body (including the three static `ApplicationIdleTimer` reads and field formatting) is not covered by the deterministic unit tests, which override the probe without calling base. | Accept for Phase 1; the body is validated by the AC5 runtime capture. Optionally add a focused test that calls base with the static reads behind an injectable seam in a future phase. | The field-formatting/log line is the diagnostic deliverable; it is currently verified by inspection and the pending runtime capture rather than by an automated assertion. | `ContinuationProbeSequenceTests.cs:100-104` overrides and records without base call; diff of `ApplicationGlobals.cs`. |
| Info | `TaskMaster/AppGlobals/ApplicationGlobals.cs` | `177` | Probe parameter `priorPhaseName` has no null/empty guard. | Accept; all five call sites pass compile-time string literals, so an invalid value cannot occur. | A guard would be defensive dead code given the closed set of internal callers; the simplicity-first principle favors omitting it. | Call sites `ApplicationGlobals.cs:140,143,146,149,152` pass literals. |
| Info | `TaskMaster.Test/AppGlobals/ContinuationProbeSequenceTests.cs` | `62-104` | Subclass is named `RecordingApplicationGlobals` rather than the spec's illustrative `TestApplicationGlobals`. | No action. | The seam pattern (override `protected internal virtual` probe + phase wrappers) matches the spec's intent; the name is more descriptive. | `ContinuationProbeSequenceTests.cs`. |
| Info | full suite | `UtilitiesCS.Test...TimedAsyncTask_Tests.RequestTask_WithProvidedTask_InvokesTaskAfterInterval` | One test fails under the full-suite coverage run. | Track as a separate flaky-test follow-up (real-interval timer in a unit test). | Verified pre-existing: file last modified 2026-03-19, not in the branch diff; passes 2/2 in isolation. Not a regression from this change. | `git log -1 -- ...TimedAsyncTask_Tests.cs`; `git diff 7a8ee65b...HEAD --name-only` (absent); `final-qc-2026-06-22T18-05.md`. |

No Blocker or Major findings.

---

## Implementation Audit

### C# implementation audit

#### What changed well

- The change is minimal and behavior-preserving: the single `await Task.Yield()` is retained verbatim, the five inter-phase boundaries and phase order are unchanged, and the only added runtime behavior is one debug-level log line. This matches the spec's "behavior-preserving attribution probe" intent precisely.
- The probe reuses the existing log4net `logger` field and existing `ApplicationIdleTimer` static signals rather than introducing new dependencies, satisfying the "use approved libraries only" and "isolate I/O" principles.
- `Stopwatch` is used for timing (hardware-counter based), avoiding all banned timing APIs; an in-code comment explains the rationale.
- The method retains the `protected internal virtual` modifier, preserving the existing test seam pattern used by the other AppGlobals timing tests.
- The XML-free block comment above the method explains *why* (issue #211, the attribution number, behavior preservation) rather than restating *what*, consistent with the comment policy.

#### Type safety and API notes

- Nullable handling is correct: `SynchronizationContext.Current?.GetType().FullName ?? "null"` safely renders a null sync context. The nullable/TWAE build reports 0 warnings (`final-qc`), confirming no nullable-flow regressions in the touched path.
- The public/protected surface is unchanged in shape (one `protected internal virtual` method renamed with one added parameter); no public API break. The rename is propagated to both overriding test subclasses, so no caller is left dangling — confirmed by the green nullable build and the source-structure regex tests.

#### Error handling and logging

- Logging uses the project's log4net pattern at `Debug` level, appropriate for diagnostic instrumentation that must not affect normal operation. No exceptions are introduced; the probe cannot throw under normal conditions (string interpolation over value-typed static reads).
- The probe does not swallow or broad-catch anything; it adds no new error paths.

---

## Test Quality Audit

The new tests are deterministic unit tests that drive the real `LoadSequentialAsync` sequence through an overriding subclass, isolating the orchestration wiring from live COM, timers, and the static `ApplicationIdleTimer`. Coverage of the probe's *logging body* is intentionally deferred to the AC5 runtime capture; the tests verify the *wiring contract* (one probe call per boundary, correct order, correct phase names, exactly five boundaries).

### Reviewed test and QA artifacts

- `TaskMaster.Test/AppGlobals/ContinuationProbeSequenceTests.cs` — verifies probe invocation order (`Equal("IntelConfig","OlObjects","ToDo","AutoFile","Engines")`) and count (`HaveCount(5)`). Both methods are `[DoNotParallelize]` to serialize against the process-global ApplicationGlobals seam. No live COM/timer/filesystem/temp files. Both pass per final-qc.
- `TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs` — source-structure regex assertions updated to `YieldWithContinuationProbeAsync\([^\)]*\)`; these guard phase ordering and the "nothing but RecordPhase between phase and yield" invariant, so the rename did not weaken the structural guards.
- `TaskMaster.Test/AppGlobals/ApplicationGlobalsStartupTimingTests.cs` — `YieldCount` override propagated to the new signature and still calls base, preserving the existing yield-count assertion.
- `evidence/qa-gates/final-qc-2026-06-22T18-05.md` — records the four-step toolchain results and the single pre-existing flake.
- `evidence/baseline/baseline-analyzers.md` — corroborates that CS8632/CS0067 warnings pre-date this change.

### Quality assessment prompts

- **Determinism:** Tests avoid live timers and static `ApplicationIdleTimer` reads by overriding the probe without calling base; outcomes depend only on the orchestration order. Deterministic.
- **Isolation:** Each test targets one observable contract (order; count). Phase bodies are no-ops, so failures localize to the wiring.
- **Speed:** Pure in-memory `Task.CompletedTask` sequence; fast. Baseline TaskMaster.Test run completed ~4.86 s for 117 tests (`baseline-mstest-coverage.md`).
- **Diagnostics:** FluentAssertions `Should().Equal(...)` and `Should().HaveCount(5)` produce clear sequence-mismatch and count-mismatch messages naming the actual recorded phases.

---

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets in code | ✅ PASS | Diff contains only a log format string of diagnostic field names; no credentials or tokens. |
| No unsafe subprocess or command construction | ✅ PASS | No process invocation introduced; the change is in-process instrumentation. |
| Input validation at boundaries | ✅ PASS | `priorPhaseName` is supplied only by five internal compile-time literals; null sync context handled with `?? "null"`. |
| Error handling remains explicit | ✅ PASS | No new catch blocks; no error suppression; behavior preserved. |
| Configuration / path handling is safe | N/A | No configuration or filesystem path handling in this change. |

---

## Research Log

No external research was required. All findings are grounded in direct diff inspection, baseline comparison via `git show`, file-size measurement, member-existence verification in `UtilitiesCS`, and the feature folder's baseline and QA-gate evidence artifacts.

---

## Verdict

The Phase 1 attribution probe is a correct, minimal, behavior-preserving change that satisfies its acceptance criteria for the deliverable's scope. The production change preserves the single `Task.Yield()` and the five-boundary phase order, uses only approved APIs and the existing logger, introduces no banned timing calls, keeps all touched files under 500 lines, and propagates the method rename to all overriding test subclasses without weakening the existing structural guards. The new tests are deterministic and isolated.

The change is ready for normal PR flow after one follow-up: repo-wide C# line coverage must be confirmed >= 80% via the multi-assembly PR CI coverage run (the local single-assembly figure is not a valid repo-wide measure). The single failing test (`TimedAsyncTask_Tests.RequestTask_WithProvidedTask_InvokesTaskAfterInterval`) is verified pre-existing and unrelated and should be tracked separately; it does not block this change. This recommendation is consistent with the Findings Table (no Blocker/Major findings) and the Conditional Go readiness recommendation above.

Blocking findings: 0
