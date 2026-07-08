# J1 — Decision Gate Disposition: Documented PARTIAL Improvement (not a HALT)

Timestamp: 2026-06-09T11-31
Row: J1 — UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs :
  GetTableInViewAsync_SlowSynchronousGetTable_ReturnsTableWithoutSyntheticRetry
Seam: S5 (OlTableExtensions.GetTableInViewAsync `int timeoutMs = 2000`)

## Disposition

DECISION GATE outcome: option (b) — PARTIAL improvement recorded with residual minimal sleep.
NOT halted: the plan (P5-T4) and research (Risk R5) explicitly authorize this disposition; the
residual sleep is genuinely-required synchronization, not flakiness masking.

## What changed

- Production seam S5: `GetTableInViewAsync` gained `int timeoutMs = 2000`, passed through to
  `TimeOutTask.RunWithTimeout(view.GetTable, token, timeoutMs, 1, false)` and propagated into the
  recursive retry call. Production callers receive the unchanged 2000 ms default.
- Test J1: reflection signature extended to `{ Explorer, CancellationToken, int, int }`; the test
  passes `timeoutMs: 5`. The mock `GetTable` first-call block was reduced from `Thread.Sleep(2100)`
  to `Thread.Sleep(20)` (a 99% reduction; test runtime ~2 s -> ~0.2 s).

## Why a residual sleep is genuinely required (full determinism is out of scope)

- The test's purpose (`SlowSynchronousGetTable_ReturnsTableWithoutSyntheticRetry`) is to verify that
  a slow SYNCHRONOUS `GetTable` that outlasts the timeout still returns its result with
  `callCount == 1`. This works because `TimeOutTask.RunWithTimeout`'s `Func<TResult>` overload runs
  `await Task.Run(() => function(), combinedToken.Token)` with a `CancellationTokenSource(timeoutMs)`:
  a cancellation token cannot abort an already-running non-cancelable synchronous delegate, so the
  delegate completes and the result is returned without a synthetic retry.
- Reproducing this behavior REQUIRES the mock delegate to run longer than `timeoutMs`. With
  `timeoutMs: 5`, a ~20 ms block reliably exceeds the timeout window. This is real wall-clock
  duration by design of the scenario under test, not a mask for nondeterminism.
- A fully sleep-free conversion would require making `TimeOutTask.RunWithTimeout`'s timeout mechanism
  injectable (replacing the real `CancellationTokenSource(milliseconds)` + `Task.Run` race with a
  controllable timeout seam). `TimeOutTask.cs` is a shared threading utility explicitly marked
  do-not-modify for this cycle (plan P5-T2; research Seam S5 note). That refactor is OUT OF SCOPE.

## Recommendation for a future cycle (scope-change finding)

To make J1 fully deterministic, a future cycle should introduce an injectable timeout/clock seam on
`TimeOutTask.RunWithTimeout` (e.g., an `IClock`/`TimeProvider`-based timeout or a delegate seam for
the timeout source), then drive the timeout deterministically in the test. This touches the shared
`TimeOutTask` threading utility and its other call sites, so it is correctly deferred rather than
widened into this cycle.

## Verification

- vstest: `GetTableInViewAsync_SlowSynchronousGetTable_ReturnsTableWithoutSyntheticRetry` PASSED
  (EXIT_CODE 0, ~203 ms). Assertions unchanged in intent (`result` same instance; `callCount == 1`).
