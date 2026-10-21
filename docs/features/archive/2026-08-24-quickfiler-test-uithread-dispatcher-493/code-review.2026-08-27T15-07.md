# Code Review — quickfiler-test-uithread-dispatcher (#493)

- **Branch:** `bug/quickfiler-test-uithread-dispatcher-493` (HEAD `98113b09`) vs base `125c36b0` (`epic/quickfiler-bug-family-integration`)
- **Reviewer timestamp:** 2026-08-27T15-07
- **Scope:** full branch diff. Build-relevant changes: 4 test `.cs` files + `QuickFiler.Test.csproj`. Zero production files changed (reviewer-verified).

## Findings Table

| ID | Severity | Blocking? | Location | Finding |
| --- | --- | --- | --- | --- |
| CR-1 | Minor | Non-blocking | `QfcItemController.InitializationTests.Part2.cs:313-326`; `QfcItemController.UiThreadDispatcherFixture.cs` (`UiThreadDispatcherTransaction.Dispose`) | Restore paths lack `try/finally` hardening: `PumpHarness.Restore()` runs `TokenSource.Dispose()` before `_transaction.Dispose()` after already setting `_restored = true`; `UiThreadDispatcherTransaction.Dispose()` runs `CompareExchange` before `ReleaseTransactionGate()`. A hypothetical throw in the earlier call would permanently leak the gate (and, in `Restore()`, also skip the field restore). Both earlier calls are non-throwing in practice (`CancellationTokenSource.Dispose`, `FieldInfo.SetValue` on a resolved static field), and all downstream consumers are `[Timeout]`-bounded, so risk is theoretical. Recommend `try { … } finally { _transaction.Dispose(); }` as follow-up polish. |
| CR-2 | Minor | Non-blocking | `QfcItemController.UiThreadDispatcherFixtureTests.cs` (R2, R3) | R2/R3 assert on the field's absolute value (null baseline forced via `transaction.Install(null)`), which is airtight against other transaction holders but not against a concurrent unowned `EnsureDispatcher` caller (the two `FocusAndThemeTests` call sites), because `EnsureDispatcher` deliberately does not take `TransactionGate`. The race window is sub-millisecond and the exposure is inherent to the accepted design (keeping Ensure off the gate so un-`[Timeout]`-ed callers cannot hang); recorded so a future rare flake in R2/R3 is diagnosed quickly rather than treated as a fixture defect. |
| CR-3 | Info | Non-blocking | `QfcItemController.UiThreadDispatcherFixture.cs` (`GetParkedDispatcher`) | The `park` `ManualResetEventSlim` is intentionally never set or disposed; the parked STA background thread lives until process exit. This is the pre-existing pattern relocated verbatim from `TestSupport.cs` and is documented in-code. No action needed. |
| CR-4 | Info | Non-blocking | `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs:42-51` (not in owned set) | Residual R-1: raw reflection swap outside both locks, with a verified `finally` restore. Latent ordering hazard vs a concurrent transaction; tracked as issue #648 (OPEN). Out of scope for this feature per spec; do not fix here. |

No Blocking findings.

## Lock-Ordering and Deadlock Analysis (audited hard, as requested)

- **Claimed invariant holds.** The only `TransactionGate` acquisition is `BeginTransactionAsync`, which holds no other lock at that point. `FieldLock` is taken only inside `Current`, `Exchange`, `CompareExchange`, and `EnsureDispatcher`, each a straight-line region with no wait, no await, and no acquisition of any other lock inside it (the parked-dispatcher creation, which does wait on a `ManualResetEventSlim`, is deliberately performed *before* `FieldLock` is taken — `UiThreadDispatcherFixture.cs:100-104`). Order is therefore always `TransactionGate` → `FieldLock`; no path takes `FieldLock` then `TransactionGate`; no cycle exists.
- **`EnsureDispatcher` never acquires `TransactionGate`** — verified by inspection; the method body touches only `ParkedDispatcherLock` (released before `FieldLock`) and `FieldLock`.
- **No lock held across an `await`.** `BeginTransactionAsync` awaits the semaphore while holding nothing; `BuildPumpHarnessCoreAsync` awaits extensively while holding the `SemaphoreSlim` permit, which is an async-safe hold by design (that hold window *is* the #230 fix) — no monitor lock spans an await anywhere in the diff.
- **No `async void`** anywhere in the four owned files (reviewer grep).
- **Third lock (`ParkedDispatcherLock`)** cannot deadlock: it is acquired only in `GetParkedDispatcher`, which acquires nothing else while held except the internal `ready.Wait()` on a thread it just started — that thread takes no locks before `ready.Set()`.

## Restore Idempotence and Exception Safety

- `EnsureScope.Dispose`: `_disposed` guard makes the second call a pure no-op (neither rewrite nor throw — R3 proves it); a scope that installed nothing carries `null` and never writes. Restore is conditional via `CompareExchange(_installed, null)`, so a newer owner's value is never clobbered.
- `UiThreadDispatcherTransaction.Dispose`: `_disposed` guard prevents double-release (`SemaphoreFullException` — R5 proves it); restore is conditional via `CompareExchange(_installedValue, _previous)` and strictly precedes `ReleaseTransactionGate()` (R4 proves a waiter never observes the pre-restore value). `Install` is one-shot and fails fast (R6). Residual theoretical gap is CR-1.

## Blast Radius on Shared Test Infrastructure (highest-risk aspect)

The pump gate moved from `QfcItemController_InitializationTests`'s private `SemaphoreSlim(1,1) UiThreadDispatcherGate` to `UiThreadDispatcherFixture.TransactionGate`, also `SemaphoreSlim(1,1)`.

- **Permit count:** unchanged (1 → 1). **Hold window:** unchanged — acquired at `BuildPumpHarnessAsync` entry (previously `WaitAsync`, now `BeginTransactionAsync`), released in `PumpHarness.Restore` (previously `Release()`, now `_transaction.Dispose()`), with the same catch-path release on build failure. The catch path is strictly improved: if the build throws *after* `transaction.Install(...)`, `transaction.Dispose()` now also restores the static, which the old `Release()`-only path did not.
- **Consumers enumerated (reviewer grep):** 7 tests in `QfcItemController.InitializationTests.Part3.cs` (lines 47/90/138/183/252/308/363, `Restore()` at 67/111/159/226/283/332/391) and 2 tests in `QfcItemController.SeamFactoryTests.cs` (`BuildPumpHarnessAsync` at :313 and :384, `Restore()` in `finally` at :358 and :429). None of these files changed; the harness's public surface (`BuildPumpHarnessAsync(host, darkMode)`, `harness.Restore()`) is byte-compatible. New additional acquirers: R1–R6, each holding the gate for milliseconds under a 60 s timeout (spec residual R-5 accepts this serialization).
- **`PumpHarness.Restore` idempotence:** preserved — the `_restored` guard still short-circuits the second call, and the transaction's own `_disposed` guard backs it up.
- **Deliberate reorder judged safe:** old order was `Swap(previous)` → `TokenSource.Dispose()` → `Release()`; new order is `TokenSource.Dispose()` → `_transaction.Dispose()` (restore, then release). The load-bearing invariant — restore strictly before gate release — is preserved (it is now enforced inside `Dispose` rather than by statement order in `Restore`, which is more robust against future edits). Moving `TokenSource.Dispose()` ahead of the restore is behaviorally neutral: the token source is harness-local, disposing it publishes nothing through the shared static, and nothing between the two calls can observe the not-yet-restored field because the gate is still held. The only cost is the theoretical CR-1 window. Sibling suites (the three concurrent epic features) see an unchanged gate protocol, unchanged permit count, and an unchanged steady-state field value; the integrated tree's 1072/1072 pass (which includes all nine pump-consuming tests) is the empirical confirmation.

## Test Quality

- MSTest + FluentAssertions throughout; Moq untouched where pre-existing. Arrange–Act–Assert sections explicitly commented in all six new tests. Every test documents intent via XML doc comment, including an honest statement that R4 is only probabilistically failing under a broken implementation and why a deterministic version would require a forbidden timed wait.
- Determinism: coordination exclusively via `ManualResetEventSlim` and awaited `Task`s; `[Timeout(GateTimeoutMs)]` is a fail-instead-of-hang bound, not a synchronization mechanism (no code path depends on timeout expiry for progress).
- No temporary files, no external dependencies, no mutable-global reliance beyond the static under test itself, which every test accesses only through the gated fixture.

## Verdict

Approve. 0 Blocking, 4 Non-blocking (CR-1 minor hardening, CR-2 documented flake-diagnosis note, CR-3/CR-4 informational).
