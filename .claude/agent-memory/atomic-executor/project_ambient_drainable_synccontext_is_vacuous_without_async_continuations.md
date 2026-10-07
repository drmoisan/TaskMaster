---
name: ambient-drainable-synccontext-is-vacuous-without-async-continuations
description: A test-installed "drainable" SynchronizationContext receives no Post when the test releases a plain TaskCompletionSource on the same thread; the await continuation runs inline inside SetResult, so Drain() is dead code unless the TCS uses RunContinuationsAsynchronously
metadata:
  type: project
---

On .NET Framework 4.8, when a test installs a custom SynchronizationContext as ambient, starts an
async void handler synchronously on the same thread (so its `await` captures that context), and later
calls `SetResult` on a default `TaskCompletionSource` from that same thread, the continuation is NOT
posted. `SynchronizationContextAwaitTaskContinuation.Run` inlines when the captured context equals
`SynchronizationContext.Current` and inlining is allowed. The flag the test asserts is already cleared
when `SetResult` returns, and `Drain()` runs zero callbacks.

**Why:** measured during #950 preflight (2026-10-01) with a scratchpad csc probe: plain TCS gave
`posts=0 drained=0` with the flag already false after `SetResult`, for both the completing and the
throwing loader. With `new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously)`
the same scenario gave `posts=1`, flag still true after `SetResult`, and false only after `Drain()`.
A plan whose doc comments say "the continuation is drained" is then describing a mechanism that never
runs, and removing the `Drain()` call would not make the test fail.

**How to apply:** when a plan prescribes a drainable or pumpable context, check which thread calls
`SetResult` and whether the TCS allows synchronous continuations. If the release happens on the
context's own thread, require `RunContinuationsAsynchronously` on the released TCS, or require the
docs to state that the continuation runs inline. Probe it with csc in the scratchpad (see
[[preflight-csc-probe-for-mandated-csharp-shapes]]). Related: [[qfc-backgroundworker-async-void-dowork-race]].

Companion technique from the same pass: a plan's multi-line pwsh payload can be parse-checked through
the Bash channel without executing it by wrapping it as `pwsh -NoProfile -Command '$null = { ... }; "PARSED"'`.
