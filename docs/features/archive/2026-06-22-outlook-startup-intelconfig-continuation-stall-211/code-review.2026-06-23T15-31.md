# Code Review: Issue #211 startup-latency attribution instrumentation (Phase 1 + Phase 3)

**Review Date:** 2026-06-23
**Reviewer:** feature-reviewer agent
**Feature Folder:** `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211`
**Feature Folder Selection Rule:** Folder suffix `-211` matches the issue number in the branch name and is the only active folder with material scoping-doc changes (`spec.md`) in the branch diff.
**Base Branch:** `main` (merge-base `9385bf607aca6c5722f2da7961a895c685710942`)
**Head Branch:** `bug/outlook-startup-intelconfig-continuation-stall-211` (`e3a84b5dc4544aaf8b498dfed4e7b45708c9c12a`)
**Review Type:** Initial review (full feature-vs-base)

---

## Executive Summary

This branch adds behavior-preserving diagnostic instrumentation for issue #211 in two increments. Phase 1 replaces the inter-phase `Task.Yield()` in `ApplicationGlobals.LoadSequentialAsync` with `YieldWithContinuationProbeAsync(string priorPhaseName)`, which measures the continuation-resume latency on the STA and emits one `[continuation-resume]` line per phase boundary. Phase 3 instruments `AppItemEngines.InitAsync` via a new testable seam, `EngineInitTimingProbe`, that times the upfront `Configuration` await and each per-engine factory invocation and emits `[engine-init-config]` and `[engine-init]` lines. The scope is small and surgical: one new 97-line production type, two thin call-site edits in COM-bound classes, two new deterministic test files, and rename-tracking edits in two existing test files.

The implementation quality is good. Timing logic is extracted out of the `[ExcludeFromCodeCoverage]` COM-bound `AppItemEngines` into a small `public sealed` seam with an injected `Action<string>` sink, making the timing/emission logic 100% unit-testable without a live appender, COM, or timer. Guard clauses fail fast; the throwing-factory path is explicitly tested to confirm fail-fast propagation with no partial emission. The reviewer independently ran the full C# toolchain against the head (CSharpier check, analyzer build, nullable/TWAE build, and the targeted AppGlobals test set with coverage) and all four steps passed cleanly; the new seam reaches 100% line coverage.

**What changed:**
- `TaskMaster/AppGlobals/EngineInitTimingProbe.cs` (NEW): timing/emission seam with constructor sink guard, `TimeEngineAsync`, `EmitConfigTiming`.
- `TaskMaster/AppGlobals/AppItemEngines.cs` (MODIFIED): `InitAsync` wraps the `Configuration` await and the per-engine `tup.EngineFunc` await with the probe; assignment/filter/select semantics unchanged.
- `TaskMaster/AppGlobals/ApplicationGlobals.cs` (MODIFIED): 5 call sites updated to `YieldWithContinuationProbeAsync(priorPhase)`; probe method renamed and expanded to emit STA-occupancy signals.
- Two new test files plus rename-tracking edits in `ApplicationGlobalsTests.cs` and `ApplicationGlobalsStartupTimingTests.cs`; csproj `<Compile Include>` wiring.

**Top 3 risks:**
1. Issue #211's primary objective (eliminate the multi-minute startup latency) is unmet; this branch delivers diagnostics only. AC9 (maintainer re-capture) and AC10 (Phase 4 fix) are not delivered.
2. Repo-wide coverage aggregate (64.05%) is below the 80% raw floor; this is pre-existing with no regression, but the authoritative post-exemption determination is the PR CI run, unavailable locally.
3. The production `YieldWithContinuationProbeAsync` and the `AppItemEngines` instrumentation lines are not directly unit-exercised (they touch static `ApplicationIdleTimer`/COM); correctness rests on the behavior-preserving argument and the seam extraction rather than direct assertion.

**PR readiness recommendation:** **Conditional Go** — the instrumentation is clean, tested, and toolchain-green; merge it as a diagnostic increment, but do not close #211 (objective unmet; AC9/AC10 pending) and confirm the repo-wide coverage gate via PR CI.

---

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Info | `TaskMaster/AppGlobals/ApplicationGlobals.cs` | `YieldWithContinuationProbeAsync` (~lines 172-190) | The expanded probe reads four static members (`ApplicationIdleTimer.IsIdle`, `CurrentCPUUsage`, `CurrentGUIActivity`, and `SynchronizationContext.Current`) inside the production path; these are not unit-exercised because the test subclass overrides without calling base. | Acceptable for a COM-host-bound diagnostic; keep the behavior-preserving guarantee (single `Task.Yield`). No action required. | The unexercised lines are COM/host-bound by nature; the seam intentionally isolates them. Behavior preservation is the load-bearing property and is asserted by ordering/count tests. | `ContinuationProbeSequenceTests.cs:98-104`; reviewer 20/20 test run |
| Info | `TaskMaster/AppGlobals/AppItemEngines.cs` | `InitAsync` (lines 42-74) | Instrumentation lines live inside an `[ExcludeFromCodeCoverage]` COM-bound class, so they are covered only indirectly; the coverable logic was moved to `EngineInitTimingProbe`. | None; this matches the documented seam-extraction design and the COM/VSTO exemption. | Keeps the 90% new-code floor satisfiable while respecting the COM exemption. | `evidence/qa-gates/final-qc-coverage-delta-2026-06-23T14-30.md`; `git grep ExcludeFromCodeCoverage` |
| Nit | `TaskMaster/AppGlobals/AppItemEngines.cs` | lines 47, 51 | The two instrumentation sites use fully-qualified `System.Diagnostics.Stopwatch` / `System.Threading.Thread` rather than file-scoped `using` directives. | Optional: add `using` directives for consistency with the rest of the file. | Cosmetic only; does not affect correctness, analyzers, or formatting (CSharpier accepts it). | Diff inspection of `AppItemEngines.cs` |
| Info | `docs/features/.../spec.md` | AC9, AC10 | AC9 and AC10 remain unchecked; the issue objective is explicitly unmet on this branch. | Track AC9 (maintainer non-debugger re-capture) and AC10 (Phase 4 fix) as follow-up; do not close #211 on merge. | Prevents misrepresenting an unsolved startup regression as resolved. | `spec.md:181-182` |

No Blocker or Major findings.

---

## Implementation Audit

### C# implementation audit

#### What changed well

- Timing/emission logic is extracted into a small `public sealed EngineInitTimingProbe` with a single injected `Action<string>` sink, cleanly separating the coverable diagnostic logic from the COM-bound `AppItemEngines`. This is the right seam for the COM/VSTO exemption and achieves 100% coverage of the new logic.
- Behavior preservation is concrete and verifiable: the per-engine path still awaits `tup.EngineFunc(Globals)` exactly once (now via `probe.TimeEngineAsync`), and the inter-phase path still performs exactly one `Task.Yield()`. Downstream `.Where(...).ToConcurrentDictionaryAsync(...)` semantics are untouched.
- The throwing-factory test (`TimeEngineAsync_FactoryThrows_PropagatesAndEmitsNoLine`) verifies the instrumentation preserves fail-fast propagation and does not emit a misleading success line on failure — a correct and non-obvious edge to cover.
- `Stopwatch` is used for all timing (hardware-counter based); no banned timing APIs are introduced in source.

#### Type safety and API notes

- Nullable annotations are correct: `Task<IConditionalEngine<MailItemHelper>?>` return, nullable `SynchronizationContext.Current?` with `?? "null"`, and `?? throw new ArgumentNullException` on the sink. The nullable/TreatWarningsAsErrors build passed.
- Public surface is minimal and intentional: one new public type plus the existing `protected internal virtual` override seam used by tests.
- Analyzer build (Meziantou/Sonar/Roslynator/AsyncFixer/BannedApi) passed with no errors/warnings at minimal verbosity.

#### Error handling and logging

- Guard clauses validate `engineName`, `factory`, and the sink at construction/entry. Logging uses the existing `log4net` `logger.Debug` sink in production (`s => logger.Debug(s)`), consistent with the repo logging pattern; no ad-hoc console output.
- The probe emits structured single-line records with stable field names, which suits DebugView/OutputDebugString capture used by the maintainer evidence path.

---

## Test Quality Audit

The new tests are deterministic and isolated. The seam tests use list-capturing sinks and Moq stubs; the ordering test drives the real `LoadSequentialAsync` through a recording subclass that overrides every phase wrapper to a no-op and overrides the probe without calling base, so no static `ApplicationIdleTimer` read, live COM, or live timer executes in CI. Timing assertions check field shape via regex (`engineMs=\d+\.\d`) rather than exact values, avoiding timing flake.

### Reviewed test and QA artifacts

- `TaskMaster.Test/AppGlobals/EngineInitTimingProbeTests.cs` — 6 tests covering ordered emission, null-engine (Skip), config line, throwing factory (no emit), null-argument guards, and null-sink guard. Reviewer-run: all pass.
- `TaskMaster.Test/AppGlobals/ContinuationProbeSequenceTests.cs` — 2 tests asserting exact probe ordering (`IntelConfig, OlObjects, ToDo, AutoFile, Engines`) and count (5). Reviewer-run: all pass.
- `TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs` / `ApplicationGlobalsStartupTimingTests.cs` — updated to the renamed probe; the source-shape regex assertions now match `YieldWithContinuationProbeAsync([^\)]*)`.
- `evidence/qa-gates/final-qc-tests-coverage-2026-06-23T14-30.md` — executor full non-live run, 4318/4318 pass; `EngineInitTimingProbe` line-rate=1.
- `evidence/qa-gates/final-qc-coverage-delta-2026-06-23T14-30.md` — repo-wide 64.04% -> 64.05% (no regression); new seam 100%.
- `artifacts/csharp/coverage.xml` — reviewer-regenerated Cobertura confirming `TaskMaster.EngineInitTimingProbe` and `<TimeEngineAsync>d__2` both line-rate=1.

### Quality assessment prompts

- **Determinism:** Stub factories (`Task.FromResult`/`Task.FromException`); no clock/network/PATH/filesystem dependence; shape-based timing assertions.
- **Isolation:** Each test targets one behavior; `[DoNotParallelize]` documents the only shared-global constraint.
- **Speed:** Reviewer-run 20 tests in 2.96 s; most under 10 ms.
- **Diagnostics:** FluentAssertions produce descriptive failures; the ordering assertion names the full expected sequence.

---

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets in code | ✅ PASS | Diff inspection; only diagnostic log strings and timing fields added. |
| No unsafe subprocess or command construction | ✅ PASS | No process/shell invocation introduced; instrumentation only. |
| Input validation at boundaries | ✅ PASS | `ArgumentNullException` guards on sink, engineName, factory (`EngineInitTimingProbe.cs:37-67`). |
| Error handling remains explicit | ✅ PASS | Throwing factory propagates (fail-fast) with no emission; verified by test. |
| Configuration / path handling is safe | ✅ PASS | No new file or path handling; the `Configuration` await is timed, not altered. |

---

## Research Log

No external research was required. All findings are grounded in diff inspection, the reviewer-run C# toolchain (CSharpier/msbuild/vstest), the regenerated Cobertura coverage artifact, and the feature-folder evidence.

---

## Verdict

The change is a clean, well-tested, behavior-preserving diagnostic increment. The C# toolchain is green (reviewer-run), the new `EngineInitTimingProbe` seam reaches 100% coverage with comprehensive positive/negative/edge/error scenarios, no banned APIs are introduced, and all touched files are under 500 lines. There are no Blocker or Major findings; the recorded findings are Info/Nit and do not impede merge of the instrumentation.

Readiness is Conditional Go: merge the Phase 1 + Phase 3 instrumentation, but two conditions bear on issue closure rather than on the diff quality — the repo-wide coverage floor must be confirmed by the PR CI run against the post-exemption testable denominator, and issue #211 must remain open because its stated objective (eliminate the multi-minute startup latency) is unmet pending AC9 (maintainer non-debugger re-capture) and AC10 (the evidence-gated Phase 4 fix). This conclusion is consistent with the Findings Table and the Conditional Go recommendation above.
