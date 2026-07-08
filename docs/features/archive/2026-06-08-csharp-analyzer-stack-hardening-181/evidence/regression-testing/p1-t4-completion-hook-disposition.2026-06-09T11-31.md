# P1-T4 — Deferred-Write Completion Hook Disposition

Timestamp: 2026-06-09T11-31

Disposition: NO PRODUCTION COMPLETION HOOK ADDED (option (a) of the task).

Rationale:
- `SmartSerializableBase.SerializeThreadSafe` (lines 443-470) and the analogous method in
  `SmartSerializable<T>` run fully synchronously on the thread that raises the timer's
  `Elapsed` event. They acquire a write lock, call `CreateStreamWriter(filePath)`, serialize,
  and release the lock — no background Task is spawned.
- The test injects its own observable signal inside the test-supplied `CreateStreamWriter`
  callback (`sut.SetCreateStreamWriter(_ => { signal.Set(); ... })`).
- Because `ManualFireTimerWrapper.FireElapsed()` invokes the `Elapsed` handler synchronously on
  the test thread, `CreateStreamWriter` (and therefore `signal.Set()`) completes before
  `FireElapsed()` returns. The assertion `signal.IsSet.Should().BeTrue()` is then deterministic
  with no `signal.Wait(<timeout>)`.
- Adding a production `OnDeferredWriteCompleted` hook would be redundant and would expand the
  production surface unnecessarily, contrary to the minimal-seam guidance. The existing
  test-controlled `CreateStreamWriter` signal is sufficient.

Conclusion: The S1 `TimerFactory` seam alone makes the named test deterministic; no additional
production hook is required.
