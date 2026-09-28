---
name: gettableinviewasync-null-contract-838
description: Issue #838 research - RunWithTimeout's maxAttempts is zero-based so 1 means two attempts; ThrowIfCancellationRequested raises OperationCanceledException not TaskCanceledException, making 3 of the 4 null paths production-unreachable; Task.Run's token suppresses scheduling only
metadata:
  type: project
---

Findings from researching issue #838 (`GetTableInViewAsync` returns null on timeout),
`UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` and
`UtilitiesCS/Threading/TimeOutTask.cs`.

**Why:** each of these was counter-intuitive enough that a plan written from the issue text alone
would have been wrong.

**How to apply:** when reasoning about any `TimeOutTask.RunWithTimeout` call site, or about any
`catch (TaskCanceledException)` that is supposed to observe an outer token.

1. `RunWithTimeout`'s `attempt` is zero-based and the guard is `attempt < maxAttempts`, so
   `maxAttempts: 1` produces **two** scheduled attempts, not one. `maxAttempts: 0` would be the
   single-attempt value.
2. `CancellationToken.ThrowIfCancellationRequested()` throws `OperationCanceledException`, which is
   the **base** of `TaskCanceledException`. A `catch (TaskCanceledException)` therefore never sees
   it. In `GetTableInViewAsync` this makes three of its four null-producing assignments
   production-unreachable; only the absorbed-`default` path fires in production. Do not assume a
   `catch (TaskCanceledException)` observes caller cancellation.
3. `Task.Run(() => work(), token)` suppresses **scheduling** only; it cannot interrupt a delegate
   already on a thread-pool thread. So a genuinely slow synchronous COM call does not time out at
   all, and on the path that does return null the delegate body runs **zero** times. Repo states
   this at `DfDeedle.QfcColumns.cs:114-118`.
4. The only test seam that reaches those unreachable catch clauses is the `timeoutSourceFactory`,
   because `RunWithTimeout` invokes it **outside** its `try`. The factory must return a *fresh*
   `CancellationTokenSource` per call: `RunWithTimeout` holds it in a `using` declaration and
   disposes it each attempt. A pre-cancelled fresh source is the deterministic way to drive the
   absorbed-default path with no clock advance and no gate.
5. `Type.GetMethod(name, flags, binder, types, modifiers)` matches on **parameter** types only, so
   changing a method's return-type *nullable annotation* does not break a reflective binding.
   Changing the runtime return type (e.g. to a result struct) breaks the downstream
   `task.GetType().GetProperty("Result")` unboxing and every `BeSameAs` assertion instead.
6. `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` is 1822 lines and says so at
   `:1614`; nothing may be added to it. `TableAccess.cs` is 452/500.
7. Best in-repo precedent for a timeout contract is `DfDeedle.QfcColumns.cs:146-155`
   (`AddQfcColumnsAsync`): quiet return on outer-token cancellation, `TimeoutException` naming the
   folder on exhausted retry. The `InvalidOperationException` at `DfDeedle.cs:186-193` is a
   caller-side compensation for a *different* method's swallowed null, not a producer contract.

Related: [[etl-deadline-followups-825]], [[console-out-and-rs0030-promotion-826]].
