# Retained Waits — Justification (Intentionally NOT Converted)

Timestamp: 2026-06-09T11-31

The following cataloged occurrences are intentionally retained with research-backed justification.
They are NOT prohibited-timing defects in the sense this cycle remediates: B1-B3 exercise the real
timer implementation (the only legitimate place to do so), and L1 is a no-timeout synchronization
gate that does not depend on wall-clock time.

## B1-B3 — UtilitiesCS.Test/ReusableTypeClasses/TimerWrapper_Tests.cs

- B1 (line 45): `StartTimer_RaisesElapsedEvent` — `signal.Wait(500)` (20 ms timer interval)
- B2 (line 62): `StopTimer_PreventsPendingElapsedEvent` — `signal.Wait(250)` (150 ms timer interval)
- B3 (line 80): `StartNew_ConfiguresAutoResetAndInvokesCallback` — `signal.Wait(500)` (20 ms timer interval)

Justification (research Group B):
- These tests verify `TimerWrapper` itself — i.e., the behavior of the real `System.Timers.Timer`
  (fires/suppresses its `Elapsed` callback). Replacing the real timer with a fake would hollow out
  the test's entire purpose; these are the ONLY tests that legitimately exercise the production timer.
- The class is already annotated `[DoNotParallelize]`, which is the correct mitigation for ThreadPool
  saturation that could otherwise delay the callback past the wait window under parallel load.
- The `signal.Wait(250/500)` windows are `ManualResetEventSlim.Wait(<timeout>)` calls, generous
  relative to the 20-150 ms timer intervals; they are integration-style upper bounds on real timer
  behavior, not a timing hack masking flakiness. RS0030 (BannedApiAnalyzers) does not flag
  `ManualResetEventSlim.Wait`; these are not `Thread.Sleep`.

Disposition: RETAINED with `[DoNotParallelize]` (research recommendation). No change.

## L1 — UtilitiesCS.Test/Threading/ThreadSafeSingleShotGuard_Tests.cs

- L1 (line 39): `CheckAndSetFirstCall_ShouldAllowOnlyOneConcurrentWinner` — `start.Wait()` (NO timeout)

Justification (research Group L):
- `start.Wait()` is a `ManualResetEventSlim.Wait()` with NO timeout, used purely as a start gate to
  release 16 concurrent tasks simultaneously so they truly contend for `CheckAndSetFirstCall`. It is a
  deterministic concurrency-gate pattern that does not depend on wall-clock time and does not violate
  the prohibited-timing policy (the policy targets wall-clock waits / timeouts / sleeps).

Disposition: RETAINED — already deterministic. No change.
