# Code Review — Issue #181 Analyzer-Stack Hardening, Cycle 7

**Review timestamp:** 2026-06-09T19-01
**Branch:** feature/csharp-analyzer-stack-181
**Base:** main (merge-base 2a522ed831865c2918ab02df153ef2929b0617dc)
**Branch HEAD:** a5fcb3fb (cycle-7 changes are uncommitted in the WORKING TREE)
**Scope reviewed:** cycle-7 working-tree changes (3 production C#, 2 test C#, 1 new test helper, 1 csproj wiring)

## Executive Summary

Cycle 7 converts the two cycle-6 residual prohibited-timing items (J1 `Thread.Sleep(20)` and B1-B3 `signal.Wait(<timeout>)`) into deterministic equivalents through two minimal, behavior-preserving production seams:

- **S7** adds an optional `Func<int, CancellationTokenSource> timeoutSourceFactory = null` to the `TimeOutTask.RunWithTimeout` `Func<TResult>` overload and its private implementation, defaulting to `ms => new CancellationTokenSource(ms)`, threaded through `OlTableExtensions.GetTableInViewAsync`.
- **S8** adds an internal `IInnerTimer` abstraction, a sealed `SystemTimersTimerAdapter` 1:1 production passthrough, an internal injecting constructor, and an internal `StartNew` overload to `TimerWrapper`, leaving the public `TimerWrapper(TimeSpan)` constructor and public `StartNew` behavior-preserving.

The code is clean, well-documented, and follows the repository's DI-seam preference ordering (delegate seam for the single timeout call path; interface + adapter seam for the static-ish `System.Timers.Timer` boundary). Toolchain is clean (csharpier 0, analyzer 0/0, nullable 0/0) and the full first-party suite is 4065/4065 green. No assertion was weakened, no `[Ignore]` re-added, no banned symbol introduced, and no sleep/wait/timing-slack substitution. The executor's documented deviation from the plan (non-`?` `Func<>` form, because the production files are nullable-disabled) is correct and analyzer-clean.

The review identifies no blocking findings. Two non-blocking observations are recorded (the pre-existing `TimeOutTask.cs` 500-line overage, and the `OlTableExtensions.TableAccess.cs` 75% changed-line coverage attributable to pre-existing untested exception branches). Detailed per-criterion verdicts are below.

## Per-Criterion Verdicts (caller's explicit requests)

1. **S7 correctness / behavior-preservation — PASS (non-blocking):** The factory defaults to `ms => new CancellationTokenSource(ms)`, exactly the prior `new CancellationTokenSource(milliseconds)` construction, so the no-injection production path is behavior-equivalent. Only the `Func<TResult>` overload chain changed; the other overloads and production callers are unchanged. J1 is deterministic — it injects a held source cancelled synchronously inside the first `GetTable` call (the source is NOT pre-cancelled, so the synchronous delegate still runs to completion and `Task.Run` reaches RanToCompletion); assertions `result == mockTable.Object` and `callCount == 1` are preserved. No runtime regression.

2. **S8 correctness / behavior-preservation — PASS (non-blocking):** The public `TimerWrapper(TimeSpan)` ctor delegates to the internal ctor with a `SystemTimersTimerAdapter` wrapping a real `System.Timers.Timer`, and the adapter forwards `AutoReset`/`Enabled`/`Interval`/`Elapsed`/`Start`/`Stop`/`Dispose` 1:1, so production runtime behavior is preserved. The public `StartNew` is unchanged; the new `StartNew(IInnerTimer, ...)` overload is internal and test-only. B1-B3 are deterministic via the manual-fire fake; forwarding (B1), stop-suppression (B2), and AutoReset+callback (B3) intent are preserved. `IGenericTimer.cs` is untouched (verified: no porcelain entry). The cycle-6 `ManualFireTimerWrapper` (outer `ITimerWrapper`) is unaffected (the new fake implements the inner `IInnerTimer`, a distinct abstraction).

3. **CRITICAL completeness — PASS (non-blocking):** Independent reviewer grep `Thread\.Sleep|\.Wait\([0-9]` over both in-scope test files returns ZERO matches (EXIT 1); `signal`/`ManualResetEventSlim` references are zero in both files. L1 (`ThreadSafeSingleShotGuard_Tests`, no-timeout `start.Wait()`) is intentionally untouched and remains deterministic. The user's "convert ALL" directive is satisfied for J1 and B1-B3.

4. **Executor's documented deviation (`Func<int, CancellationTokenSource>` vs plan's `...?`) — PASS / ACCEPTABLE (non-blocking):** The production files are nullable-disabled (no `#nullable enable`); the `?` annotation would emit CS8632, which `/p:TreatWarningsAsErrors=true` promotes to a build error. The non-`?` form is correct, behavior-preserving (parameter still defaults to `null`, default factory substituted), and analyzer/nullable clean (0/0). Not a blocking finding.

5. **Coverage — PASS with one non-blocking documented exception:** Full suite 4065/4065 (`/InIsolation`); UtilitiesCS.dll 85.43% vs 85.46% baseline (-0.03pp variance; >= 80% floor). Changed-line: `TimeOutTask.cs` 100%, `TimerWrapper.cs` 91.9% (both >= 90%). `OlTableExtensions.TableAccess.cs` 75% changed-line is attributable to pre-existing untested exception-retry branches the factory was threaded through (baseline confirms those branches were already untested); no previously-covered line lost coverage. Acceptable documented exception, NON-BLOCKING.

6. **No assertion weakened / no `[Ignore]` / no banned symbol / no timing-slack substitution — PASS (non-blocking):** Confirmed by diff inspection and grep. Nullable 0/0, analyzer 0/0, csharpier clean.

7. **Out-of-scope protection — PASS (non-blocking):** `StackGeek.cs` / `StackGeek_Tests.cs` show no working-tree porcelain entry (clean/committed at 642c2851); `IGenericTimer.cs` untouched; new artifacts follow the per-cycle folder convention; cycle 1-6 folders not relocated.

8. **Pre-existing flaky IdleAsyncQueue Dispatcher test — PASS (non-blocking):** Unrelated WPF Dispatcher/UI-thread test in UtilitiesCS.Test, documented at both the cycle-6 and cycle-7 baselines, passes 4/4 in isolation. NOT a cycle-7 regression.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|----------|------|----------|---------|----------------|-----------|----------|
| Info | UtilitiesCS/Threading/TimeOutTask.cs | RunWithTimeout `Func<TResult>` overload + private impl (~lines 22-80) | S7 optional `timeoutSourceFactory` defaults to `ms => new CancellationTokenSource(ms)`, equivalent to prior construction; only this overload chain changed. | None — behavior-preserving. | Default path is byte-equivalent; no production caller passes a factory. | git diff TimeOutTask.cs; `evidence/regression-testing/p1-timeouttask-tests.2026-06-09T18-00.md` |
| Info | UtilitiesCS/ReusableTypeClasses/TimedActions/TimerWrapper.cs | internal IInnerTimer + SystemTimersTimerAdapter + internal ctor + internal StartNew (lines 12-118) | S8 inner-timer seam is internal; public ctor/StartNew preserved; adapter forwards 1:1; null inner timer fails fast. | None — minimal, behavior-preserving. | Composition over inheritance; smallest seam per DI-seam ordering. | git diff TimerWrapper.cs; `evidence/regression-testing/p2-timerwrapper-tests.2026-06-09T18-00.md` |
| Info | UtilitiesCS.Test/TestHelpers/ManualFireInnerTimer.cs | whole file | Deterministic internal fake; `FireElapsed` raises inner Elapsed synchronously; `ElapsedEventArgs` via `GetUninitializedObject` (no clock dependency). | None. | Removes wall-clock dependence; deterministic across IDE/CLI. | file read; `.claude/rules/csharp.md` Deterministic Test Rules |
| Info | UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs | J1 (rewritten) + 3 reflection call-sites | `Thread.Sleep(20)` removed; injected timeout-source factory cancels mid-flight; assertions `result == mockTable.Object`, `callCount == 1` preserved. | None. | Deterministic reproduction of the slow-synchronous scenario. | git diff; residual-timing-scan |
| Info | UtilitiesCS.Test/ReusableTypeClasses/TimerWrapper_Tests.cs | B1-B3 (rewritten) | All `signal.Wait(<timeout>)` removed; deterministic assertions on raisedCount/Started/Stopped/AutoReset; `[DoNotParallelize]` retained for real-timer ctor tests. | None. | Forwarding/stop/AutoReset intent preserved without timing. | git diff; residual-timing-scan |
| Non-blocking (style) | UtilitiesCS/Threading/TimeOutTask.cs | whole file (975 lines) | File exceeds the 500-line limit. | Track decomposition as follow-up; do not widen this cycle. | PRE-EXISTING; cycle 7 added only ~22 lines of seam threading and did not create the overage. | `wc -l` = 975; remediation-inputs note |
| Non-blocking (coverage) | UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs | exception-retry recursions (TaskCanceled ~lines 80-85; Timeout ~lines 99-106) | Changed-line coverage 75% (< 90% target) on the threaded-through retry branches. | Accept as documented exception; optionally add retry-branch tests as follow-up. | Branches were already untested pre-cycle (baseline confirms); factory merely plumbed through; no previously-covered line lost coverage. | `evidence/qa-gates/final-coverage-delta.2026-06-09T18-00.md` |
| Info | UtilitiesCS.Test/UtilitiesCS.Test.csproj | one `<Compile Include>` | Required wiring for the new helper in this legacy packages.config project. | None. | Mechanically required; test-project-only, no production behavior. | git diff csproj |

## Toolchain Verification

| Stage | Command | Result | Status |
|-------|---------|--------|--------|
| Format | `dotnet tool run csharpier .` (verify `csharpier check .`) | Checked 1059 files, EXIT 0, no changes | PASS |
| Analyze | `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 Warning(s), 0 Error(s) | PASS |
| Type-check | `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` | Build succeeded, 0/0 | PASS |
| Test | `vstest.console.exe <first-party Test.dll> /EnableCodeCoverage /InIsolation` | 4065/4065, EXIT 0 | PASS |

## Verdict

**PASS — Ready for merge** (pending the cycle-exit green required CI check against branch head after push). The cycle-7 seams are minimal, behavior-preserving, analyzer-clean, and deterministic; all prohibited timing is removed from J1 and B1-B3 with assertion intent intact. No production or test code was modified by this review.

BLOCKING FINDINGS: 0
