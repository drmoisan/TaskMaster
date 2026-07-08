# Remediation Inputs — Cycle 7 (Issue #181)

Entry timestamp: 2026-06-09T18-00
Feature folder: docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181
Branch: feature/csharp-analyzer-stack-181
Base: main

## Trigger

User directed (after cycle 6) to fully convert the two residual prohibited-timing items that
cycle 6 documented rather than converted, so that the "convert ALL Thread.Sleep / signal.Wait"
directive is satisfied:
- J1: `OlTableExtensions_Tests.GetTableInViewAsync_SlowSynchronousGetTable_ReturnsTableWithoutSyntheticRetry`
  retains a residual `Thread.Sleep(20)` (cycle 6 reduced it from 2100 ms but could not remove it,
  because full determinism requires an injectable-timeout seam on the shared `TimeOutTask` utility
  that was out of cycle-6 budget). Evidence: `evidence/regression-testing/scope-change-J1.2026-06-09T11-31.md`.
- B1-B3: `TimerWrapper_Tests` retain `signal.Wait(<timeout>)` real-timer integration waits.
  Evidence: `evidence/regression-testing/retained-waits-justification.2026-06-09T11-31.md`.

L1 (`ThreadSafeSingleShotGuard_Tests`, no-timeout `start.Wait()`) is already deterministic and is
explicitly OUT OF SCOPE for this cycle — do NOT change it.

## State update at cycle entry (branch HEAD a5fcb3fb)

Since cycle 6 the user pushed two commits on top of the cycle-6 fix (346093ec):
- `642c2851 (fix(stackgeek))` — the user COMMITTED their StackGeek WIP. `StackGeek.cs` and
  `StackGeek_Tests.cs` are now CLEAN/committed, NOT uncommitted WIP. The cycle-7 guardrail is
  therefore simply: do NOT modify, revert, or stage those two files (this cycle does not touch them).
  Baseline records their actual git state (expected: clean); final git-state confirms this cycle
  introduced no change to them. This supersedes any "StackGeek modified-but-unstaged" wording below.
- `a5fcb3fb (docs): organized remediation cycles into folders` — the user ADOPTED a per-cycle folder
  layout for cycle artifacts: each cycle's inputs+plan live in `<entry-ts>-remediation/` and each
  cycle's three reaudit artifacts live in `<exit-ts>-audit/` directly under the feature folder. The
  `evidence/` tree remains as-is. Cycle 7 MUST follow this convention: this cycle's inputs and plan
  are in `2026-06-09T18-00-remediation/`; the three reaudit artifacts go in `<exit-ts>-audit/`. Do
  NOT restore a flat layout or relocate the user's already-foldered cycle 1-6 artifacts. This
  supersedes any "keep flat artifact layout" wording below.

## Authorized production budget (cycle 7)

- `UtilitiesCS/Threading/TimeOutTask.cs` (Seam S7, J1)
- `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` (thread the S7 seam through `GetTableInViewAsync`, if required)
- `UtilitiesCS/ReusableTypeClasses/TimedActions/TimerWrapper.cs` (Seam S8, B1-B3)
- `UtilitiesCS/Interfaces/IGenericTimer.cs` — only if an additive, behavior-preserving extension
  is required for the S8 inner-timer seam (see S8). Additive only; do not remove/rename members.

Test files: `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` (J1) and
`UtilitiesCS.Test/ReusableTypeClasses/TimerWrapper_Tests.cs` (B1-B3), plus any new small test helper
(e.g. a manual-fire inner-timer fake) under `UtilitiesCS.Test/TestHelpers/`.

If a fix requires a production file outside this budget, HALT (Scope-Change Rule) — do not widen.

## Seam designs (verify against source before implementing; preflight must confirm soundness)

### S7 — Injectable timeout source on TimeOutTask.RunWithTimeout (J1)

`GetTableInViewAsync` calls the `Func<TResult>` overload
`TimeOutTask.RunWithTimeout(view.GetTable, token, timeoutMs, 1, false)` (TimeOutTask.cs lines 20-82).
Each private overload creates the timeout via `new CancellationTokenSource(milliseconds)` then
`await Task.Run(() => function(), combinedToken.Token)`.

Add a minimal, behavior-preserving seam ONLY on the `Func<TResult>` overload path (and its private
impl): an optional timeout-source factory, e.g.
`internal Func<int, CancellationTokenSource> timeoutSourceFactory = null`, defaulting to
`ms => new CancellationTokenSource(ms)`. Thread it through `GetTableInViewAsync` (optional internal
parameter) so the test can inject it.

Deterministic test rewrite (no Thread.Sleep): the test injects a factory returning a
CancellationTokenSource it holds; the mock `GetTable`, on its first call, CANCELS that token source
synchronously and THEN returns `mockTable.Object` (no sleep). Because the synchronous, non-cancelable
delegate has already started running on the `Task.Run` thread and runs to completion, the produced
result is returned and `await Task.Run(...)` completes RanToCompletion (cancelling the linked token
mid-flight does not retroactively cancel an already-completed synchronous delegate), so there is NO
synthetic retry: `result == mockTable.Object`, `callCount == 1`. This reproduces the exact scenario
deterministically. (Do NOT inject an already-cancelled source — that would prevent the delegate from
running at all and change the asserted behavior.)
- Production default (no factory injected) must be byte-for-byte equivalent in behavior to today.
- All existing TimeOutTask tests (TimeOutTask_Tests, _AdditionalTests, _OverloadCoverageTests,
  _InternalCoverageTests) must remain green.
- Note: TimeOutTask.cs is already >500 lines (pre-existing); keep the addition minimal. If the seam
  can be confined to the single `Func<TResult>` overload, do not touch the other overloads.

### S8 — Injectable inner timer on TimerWrapper (B1-B3)

`TimerWrapper` (TimerWrapper.cs) news a `System.Timers.Timer` in its constructor and forwards its
`Elapsed` via `WhenTimerElapsed`. B1-B3 verify the wrapper raises/suppresses `Elapsed` and that
`StartNew` configures `AutoReset` + invokes the callback.

Introduce a minimal inner-timer seam so the test can fire the underlying timer deterministically:
- Define (or reuse) an inner-timer abstraction with Start/Stop/Elapsed/AutoReset/Enabled/Interval.
  `IGenericTimer` (UtilitiesCS/Interfaces/IGenericTimer.cs) is close but lacks `AutoReset` and
  `ResetTimer`. Either (a) extend `IGenericTimer` ADDITIVELY with `AutoReset` (and `ResetTimer` if
  needed) — additive only, all current implementers updated — or (b) introduce a small dedicated
  internal inner-timer interface. Choose the smallest behavior-preserving option; preflight to
  confirm no existing IGenericTimer implementer/consumer regresses.
- Add a production adapter wrapping `System.Timers.Timer` implementing the inner-timer abstraction,
  and an `internal TimerWrapper(<innerTimer>)` constructor (or internal factory) used by tests. The
  PUBLIC `TimerWrapper(TimeSpan)` constructor and `StartNew` must keep current runtime behavior
  (default to the real System.Timers.Timer adapter).
- Deterministic test rewrite (no signal.Wait): inject a manual-fire inner-timer fake.
  - B1 `StartTimer_RaisesElapsedEvent`: StartTimer() then fake.FireElapsed() -> assert
    `TimerWrapper.Elapsed` raised with expected args. No wait.
  - B2 `StopTimer_PreventsPendingElapsedEvent`: StopTimer() then assert a subsequent fire is
    suppressed per the wrapper's contract (drive via the fake; assert deterministically). No wait.
  - B3 `StartNew_ConfiguresAutoResetAndInvokesCallback`: assert AutoReset configured on the inner
    fake and that firing elapsed invokes the callback. No wait.
- Assertion INTENT must be preserved (these still verify TimerWrapper's forwarding/AutoReset/stop
  semantics — now via a deterministic inner seam rather than the OS timer). Keep `[DoNotParallelize]`
  if still appropriate, but the wait-based timing must be gone.
- All other TimerWrapper consumers (production and the cycle-6 ManualFireTimerWrapper helper, which
  implements the OUTER ITimerWrapper) must remain unaffected.

## Constraints (hard)

- Allowed delegates this cycle: `atomic-planner`, `atomic-executor`, `feature-review` only.
- Remove the J1 `Thread.Sleep(20)` and the B1-B3 `signal.Wait(<timeout>)` entirely; do NOT replace
  with any other sleep/wait/timing slack. Do NOT weaken/delete assertions; do NOT re-add `[Ignore]`.
- Production seams MUST be behavior-preserving (default to current runtime behavior; deterministic
  only on test injection). No banned symbol introduced; nullable-clean; analyzer-clean.
- No new NuGet packages. No `.editorconfig`/`.globalconfig`/vendored/`BannedSymbols.txt`/
  analyzer-wiring/`.claude/rules/` changes.
- OUT OF SCOPE — never modify/revert/stage: `UtilitiesCS/ReusableTypeClasses/Other/StackGeek.cs` and
  `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs` (user WIP). They must remain
  modified-but-unstaged; the final git-state task must confirm this.
- Do NOT touch L1 (ThreadSafeSingleShotGuard_Tests) — already deterministic.
- Mandatory C# toolchain in exact order, one passing pass: `dotnet tool run csharpier .` ->
  `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` ->
  `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` ->
  `vstest.console.exe <first-party Test.dll paths> /EnableCodeCoverage /InIsolation`. Restart from csharpier on any change/failure.
- Zero regression across the full first-party suite (currently 4065/4065 green), especially the
  existing TimeOutTask and TimerWrapper test suites. Coverage no-regression on changed lines.
  Required CI check green against branch head after push.
- Keep flat artifact layout; do not reorganize committed artifacts.

## Acceptance for Cycle Exit

- J1 test passes deterministically with NO `Thread.Sleep` (assertions `result == mockTable.Object`,
  `callCount == 1` preserved).
- B1-B3 pass deterministically with NO `signal.Wait(<timeout>)` (forwarding/AutoReset/stop intent preserved).
- A residual-grep scan confirms zero `Thread.Sleep` / `\.Wait\(\d` remain in the in-scope test files
  (J1's file and TimerWrapper_Tests). The only intentionally-retained wait in the repo's cataloged set
  is L1's no-timeout `start.Wait()` (deterministic, out of scope).
- Full toolchain one clean pass; zero regression; coverage gate met.
- Three reaudit artifacts (code-review, feature-audit, policy-audit) by `feature-review` with
  `blocking_count == 0`.
- Required CI check green against branch head after push.
