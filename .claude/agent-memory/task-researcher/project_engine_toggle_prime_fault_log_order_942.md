---
name: engine-toggle-prime-fault-log-order-942
description: "#942: EngineToggleStateCoordinator.CompletePrime clears the _primeTasks marker BEFORE _logError, so a post-trigger GetPrimeTask can return Task.CompletedTask while the log is pending; H1 (unawaited observer) is false; NB-2 registration race from #735 was never promoted; in-sink probe beats gated delegates for a deterministic regression"
metadata:
  type: project
---

Issue #942 (2026-09-29): the flaky `GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse` is caused by statement order in `CompletePrime` (`TryRemove` at :348 precedes `_logError` at :354), not by an unawaited continuation. `StartObservedPrime` stores the `ContinueWith` continuation itself, so awaiting the handle DOES cover the log — the hole is only when the handle is fetched after the trigger and the pool thread has already removed it.

**Why:** the TCS `SetException` inlines the async resumption on the test thread, but the `ContinueWith(None, TaskScheduler.Default)` continuation is queued to the pool (local queue, since MSTest 4.4 workers are pool threads). First-call JIT of `BuildPrimeFailedMessage` + culture init widens the remove-to-log window enough for CI to hit it once.

**How to apply:**
- For "await handle then assert on a sink" flakes, first check whether the SUT removes the handle from its registry before the sink call; that inverts the contract even when the continuation is awaited.
- Deterministic regression without blocking: probe `GetPrimeTask` from INSIDE the injected sink (the fixture's `OnInvalidate` precedent) and assert `BeSameAs(prime)`; pre-fix it returns `Task.CompletedTask` by program order, so it fails every time. Prefer this to a gated delegate (blocks a pool thread under Workers=0).
- Hazard B (assignment at :276 racing `TryRemove` when the prime completes synchronously — reachable in production because `AsyncLazy` caches a faulted configuration load) is #735 code-review NB-2 and was NEVER promoted to an issue; promote it separately, do not fold into #942.
- `EngineToggleStateCoordinatorTests.cs` is at 459/500 lines; new tests go in a new partial and need a `<Compile Include>` in `TaskMaster.Test.csproj`.
- Related: [[taskrun-getresult-inlines-on-pool-thread-900]], [[ribbon-engine-toggle-defects-735]].
