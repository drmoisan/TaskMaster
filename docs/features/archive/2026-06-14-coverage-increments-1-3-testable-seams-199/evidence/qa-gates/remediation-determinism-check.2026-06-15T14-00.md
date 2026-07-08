# Determinism / Order-Independence Check (Remediation Cycle 2026-06-15T14-00)

Timestamp: 2026-06-15T14-00

Result:
- Within the full UtilitiesCS.Test assembly run captured in P2-T4 (vstest.console.exe over the whole assembly, the run that exercises the same execution ordering that surfaced the original failure), the test
  `UtilitiesCS.Test.Threading.IdleAsyncQueue_Tests.AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException`
  PASSED (3815/3815 passed, 0 failed).
- A targeted re-run (`/Tests:AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException`) also reported the test as Passed.

Why the order dependence is removed:
- The root cause was process-global, set-once static state `UiThread.Dispatcher` (backing field `_dispatcher`). When an earlier test in the assembly triggered `UiThread.Initialize()`, `Dispatcher` became non-null for the remainder of the run, so the Dispatcher-routing branch actually dispatched the action and `callCount` became 1, failing the assertion `callCount == 0`.
- The fix deterministically establishes the documented "Dispatcher unavailable" precondition: the test's Arrange step calls `ForceDispatcherNull()` (reflection set of `_dispatcher` to null), capturing the prior value, and a `finally` block calls `RestoreDispatcher(priorValue)` so the prior value is restored whether the test passes or fails. The precondition no longer depends on which tests ran before it, eliminating both sequential order-dependence and parallelism sensitivity.
- This is a determinism fix only. The three assertions and their reason strings are unchanged; no assertion was weakened, no `[DoNotParallelize]`-only substitute was used, and no sleeps/retries/polling/timing tolerances were introduced.
