# Code Review: Issue #181 — Deterministic Test Timer Conversion (Cycle 6)

- Branch: `feature/csharp-analyzer-stack-181`
- HEAD: `6ede1964` (cycle-6 changes are in the WORKING TREE, uncommitted)
- Base: `main`; merge-base: `2a522ed831865c2918ab02df153ef2929b0617dc`
- Reviewed: committed `2a522ed8..HEAD` diff AND the uncommitted working-tree diff
- Work Mode: full-feature (per `issue.md` marker)
- Timestamp: 2026-06-09T13-02

## Executive Summary

Cycle 6 converts prohibited non-deterministic timing primitives in `UtilitiesCS.Test` to deterministic seams. Seven production files gained behavior-preserving seams (S1–S6); fourteen test files plus a new `ManualFireTimerWrapper` helper were converted. I reviewed every production seam diff and every converted test diff independently against the disposition evidence.

Outcome: the code is of acceptable quality and the change is sound. All six production seams default to current runtime behavior and only become deterministic when a test injects the factory/hook; no banned symbol (`Thread.Sleep`, `Task.Delay`, `DateTime.Now/UtcNow`, `Random.Shared`) was added to production. The named test is deterministic. No assertion was weakened, no `[Ignore]` re-added, and no sleeps/retries were added as fixes. The D1 STA fix uses `Thread.Yield()` (a scheduler yield), not a timing hack. The out-of-scope StackGeek files remain modified-but-unstaged.

Two non-blocking findings are recorded: a documented, authorized residual `Thread.Sleep(20)` in the J1 test (PARTIAL conversion with a scope-change deferral), and a pre-existing 500-line file-size overrun on the two `SmartSerializable*` production files (already over the limit at merge-base). Neither blocks cycle exit.

This review renders explicit verdicts on each of the seven requested points below (Detailed Verdicts), each with a blocking/non-blocking determination.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|----------|------|----------|---------|----------------|-----------|----------|
| Info | SmartSerializableBase.cs / SmartSerializable.cs | `RequestSerialization`, `TimerFactory` prop | Seam S1 retypes `_timer` to `ITimerWrapper` and adds a `protected Func<TimeSpan, ITimerWrapper> TimerFactory` defaulting to `interval => new TimerWrapper(interval)`. Production default identical to prior behavior. | Accept. | Behavior-preserving; deterministic only on test injection. | working-tree diff |
| Info | TimedQueueOfActions.cs | `StartTimer`, `TimerFactory` | Seam S2 `internal` factory; invoked on each `StartTimer`, preserving stop/dispose-then-recreate lifecycle. | Accept. | Behavior-preserving. | working-tree diff |
| Info | AsyncMultiTasker.cs | 4 chunker overloads | Seam S3 optional `Func<TimeSpan, ITimerWrapper>? timerFactory = null` coalesced to real factory; `timer` local retyped to `ITimerWrapper`. | Accept. | Optional param; existing callers unchanged. | working-tree diff |
| Info | IEnumerableExtensions.cs | `ToList` | Seam S4 optional `Action<int> onItemCompleted = null` invoked alongside existing progress timer. | Accept. | Null in production (no-op). | working-tree diff |
| Info | FolderRemapTree.cs | new internal ctor | Seam S6 internal ctor takes `batchNotifierTimerFactory`; public ctors unchanged. | Accept. | Behavior-preserving; test-only constructor. | working-tree diff |
| Info | OlTableExtensions.TableAccess.cs | `GetTableInViewAsync` | Seam S5 optional `int timeoutMs = 2000` threaded into `TimeOutTask.RunWithTimeout` and the recursive retry. | Accept. | Default preserves 2000 ms production behavior. | working-tree diff |
| Low (non-blocking) | OlTableExtensions_Tests.cs | line 1287 | J1: residual `Thread.Sleep(20)` with `timeoutMs: 5` — documented PARTIAL conversion; genuinely-required synchronization to exceed the timeout. | Defer full determinism to a future TimeOutTask injectable-timeout cycle. | Authorized by plan P5-T4 + Risk R5; not flakiness masking. | scope-change-J1.2026-06-09T11-31.md |
| Low (non-blocking) | SmartSerializableBase.cs (534), SmartSerializable.cs (596) | whole file | Both exceed the 500-line limit, but were already over at merge-base (524, 586). Cycle added 10 lines each. | Track a split refactor for a future cycle. | Pre-existing condition, not introduced this cycle. | `git show 2a522ed8:<file>` line counts |
| Info | ConfigController_Tests.cs | line ~288 (D1) | STA pump retains `Application.DoEvents()` and replaces `Thread.Sleep(10)` with `Thread.Yield()`; then `GetAwaiter().GetResult()` surfaces exceptions. | Accept. | `Thread.Yield()` is a scheduler yield, not banned and not a wall-clock wait; bare `GetResult()` deadlocks on the WinForms STA context. | residual-timing-scan.2026-06-09T11-31.md |
| Info | ManualFireTimerWrapper.cs | new helper (118 lines) | Test double for `ITimerWrapper`; synchronous `FireElapsed()`; supports repeated start/stop cycles (Risk R2); `IDisposable`; nullable-annotated `Elapsed`. | Accept. | Clean, well-documented test infrastructure. | working-tree diff |

## Detailed Verdicts (requested points)

1. **Behavior-preservation of the 6 production seams — NON-BLOCKING (PASS).** Each seam (S1 timer-factory props on SmartSerializableBase/SmartSerializable; S2 internal factory on TimedQueueOfActions; S3 optional `timerFactory` params on AsyncMultiTasker; S4 optional `onItemCompleted` hook on IEnumerableExtensions.ToList; S5 `timeoutMs=2000` on GetTableInViewAsync; S6 internal timer-factory ctor on FolderRemapTree) defaults to the prior runtime behavior (`new TimerWrapper(...)` / null no-op / 2000 ms) and becomes deterministic only when a test injects the factory/hook. No banned symbol added to production (verified by diff scan). Analyzer 0/0 and nullable 0/0. No production runtime regression: full suite 4065/4065. Confirmed.

2. **Named test `Serialize_WithConfiguredPath_TriggersDeferredThreadSafeWrite` deterministic — NON-BLOCKING (PASS).** Both the `Thread.Sleep(50)`/`signal.Wait(5000)` (Base, A1/A2) and `signal.Wait(1000)` (Generic, A3) are removed. The test injects `ManualFireTimerWrapper` via `SetTimerFactory`, asserts `timerStub.Started.Should().BeTrue()`, calls `FireElapsed()`, and asserts `signal.IsSet.Should().BeTrue()`. Assertion intent preserved: the deferred write still invokes `CreateStreamWriter`, which sets the signal. Confirmed deterministic.

3. **Completeness vs. "convert ALL uses":**
   - **3a. J1 residual `Thread.Sleep(20)` — NON-BLOCKING (acceptable documented deferral).** The directive is satisfied to the extent achievable within the authorized production budget. `TimeOutTask.RunWithTimeout` is explicitly do-not-modify this cycle; the test's scenario (a slow synchronous delegate that outlasts the timeout returns with `callCount == 1`) requires the mock to actually run longer than the injected 5 ms timeout, so a brief real-wall-clock block is inherent to the behavior under test, not a flakiness mask. This is a 99% reduction (2100 ms -> 20 ms) plus a fully-documented scope-change finding (`scope-change-J1`) for a future injectable-timeout refactor. This is a legitimate documented deferral, not a blocking finding. The HALT-vs-defer rule was applied correctly (recorded, not silently omitted or masked).
   - **3b. B1–B3 and L1 retentions — NON-BLOCKING (each legitimately deterministic/necessary).**
     - B1, B2, B3 (`TimerWrapper_Tests`): `signal.Wait(250/500)` is RETAINED legitimately. These are integration tests of the real `System.Timers.Timer` behavior; replacing it with a fake would hollow out the test purpose. They are `[DoNotParallelize]`, and the wait windows are generous upper bounds, not timing hacks. A real-timer integration test inherently needs a wall-clock bound. PASS / not blocking.
     - L1 (`ThreadSafeSingleShotGuard_Tests`): `start.Wait()` is a NO-timeout `ManualResetEventSlim.Wait()` used purely as a start gate to release 16 concurrent tasks. A no-timeout wait that blocks until the code-under-test signals is deterministic and is not a wall-clock-timeout violation. PASS / not blocking.
   - **3c. No other prohibited occurrence silently omitted or masked — NON-BLOCKING (PASS).** Cross-checked the P6 residual grep scan against the 26-row inventory: every cataloged occurrence A1–K1 (excluding the retained/PARTIAL set) is gone. The only residual matches are B1–B3 (retained), L1 (retained, no-timeout), J1 (documented PARTIAL), and the approved Risk-R7 `SpinUntil(... > TimeSpan.Zero, <bound>)` structural guarantees. One out-of-catalog `task.Wait(TimeSpan.FromSeconds(10))` in `QfcTipsDetails_Tests` is pre-existing and outside the 12 cataloged files — not a cycle-6 occurrence. Confirmed complete.

4. **D1 STA fix (`DoEvents()` + `Thread.Yield()`) — NON-BLOCKING (PASS).** `Thread.Yield()` is a scheduler yield, not a wall-clock sleep, and is not in `BannedSymbols.txt`. It is required because `SaveAsync` installs a `WindowsFormsSynchronizationContext` and posts its continuation to the STA message queue; a bare `GetAwaiter().GetResult()` deadlocks. The pump-then-yield loop is deterministic (it terminates exactly when the task completes). Not a prohibited timing hack.

5. **No assertion weakened, no `[Ignore]` re-added, no sleeps/retries added as fixes — NON-BLOCKING (PASS).** Diff review found zero `[Ignore]` additions. The four removed assertion-like lines are relocations/strengthenings (e.g., `.Wait(...).Should().BeTrue()` -> `.IsSet.Should().BeTrue()`; `SpinWait.SpinUntil(...).Should().BeTrue()` -> deterministic `FireElapsed()` loop + `TimerActive.Should().BeFalse()`; an `OrderBy().Should().Equal()` moved out of a `lock` block but preserved). The two deleted `AcceleratePrivateTimer` reflection helpers are dead code made unnecessary by the seam; the test methods and assertions remain. No sleeps/retries were introduced as a fix (D1 uses `Thread.Yield`).

6. **Out-of-scope protection (StackGeek) — NON-BLOCKING (PASS).** `git status` shows `UtilitiesCS/ReusableTypeClasses/Other/StackGeek.cs` and `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs` as ` M` (modified, unstaged). They are NOT part of this cycle's staged changes. Confirmed.

7. **Policy/toolchain — NON-BLOCKING (PASS).** csharpier EXIT 0 clean; analyzer 0/0; nullable 0/0; vstest 4065/4065 with `/InIsolation`; coverage 85.46% vs 85.52% baseline (no changed-line regression, −0.06 pp denominator-driven). MSTest/Moq/FluentAssertions used. File-size: 5 of 7 production files under 500; `SmartSerializableBase.cs` (534) and `SmartSerializable.cs` (596) exceed 500 but were pre-existing overruns at merge-base — flagged as a Low non-blocking finding (see Findings Table), not a cycle-6 regression.

## Conclusion

The cycle-6 changes are well-scoped, behavior-preserving in production, and deterministic in tests. No blocking code-quality defect was found. The two non-blocking findings (J1 authorized PARTIAL deferral; pre-existing file-size overrun) are documented and tracked.

BLOCKING FINDINGS: 0
