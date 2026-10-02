---
name: prime-marker-register-before-start-944
description: #944 EngineToggleStateCoordinator hazard B — ContinueWith(None, Default) always queues, never inlines; fix = register TCS marker before ApplyPrimeAsync; only program-order test is deterministic RED
metadata:
  type: project
---

Issue #944 (hazard B, NB-2 of #735): `_primeTasks[engine] = StartObservedPrime(...)` stores the continuation AFTER it is queued, so a pool thread can run CompletePrime's TryRemove first and leave a stale marker.

- Verified from reference source: StandardTaskContinuation.Run inlines only with ExecuteSynchronously; otherwise ScheduleAndStart -> ThreadPool.UnsafeQueueCustomWorkItem. So the continuation never runs on the registering thread today.
- Recommended fix: TaskCompletionSource<bool>(RunContinuationsAsynchronously) marker registered under `_primeGate` before ApplyPrimeAsync; continuation `try { CompletePrime } finally { marker.SetResult(true) }`. Compatible with #942 report-then-clear.
- Lock-in-CompletePrime (i) is correct only because of TaskContinuationOptions.None and admits NO deterministic RED test.
- Deterministic RED: probe `GetPrimeTask(...).IsCompleted` from inside the strict-mock EngineActiveAsync callback (runs synchronously on the caller). Behavioral re-prime tests are GREEN guards only (pre-fix fail is timing-dependent).
- AsyncLazy caches a faulted config load, so EngineActiveAsync returns already-faulted tasks — the realistic trigger.

**Why:** planners kept proposing the lock fix; it cannot satisfy the bugfix workflow's fails-first test.
**How to apply:** for any "marker registered after async start" race, prefer register-before-start and a program-order probe from inside the synchronous mock callback. Research: docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/research/.
