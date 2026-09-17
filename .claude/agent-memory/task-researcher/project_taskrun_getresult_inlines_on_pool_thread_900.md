---
name: taskrun-getresult-inlines-on-pool-thread-900
description: "#900: Task.Run(...).GetAwaiter().GetResult() from a pool thread INLINES the delegate on the caller (local-queue pop) unless stolen; MSTest 4.4 runs every no-[Timeout] test body inside Task.Run(taskGetter); Dispatcher.CheckAccess compares Thread objects"
metadata:
  type: project
---

Issue #900 (2026-09-16): the "Task.Run gives a different thread" assumption fails through wait-inlining, not idle reuse.

**Why:** `TaskAwaiter.GetResult` -> `InternalWait(Infinite, default)` tries `TryRunInline`; `ThreadPoolTaskScheduler.QueueTask` local-pushes non-LongRunning tasks when the caller is a pool thread, and `TryExecuteTaskInline` pops it back. MSTest 4.4.0 `DefaultFactoryAsync` wraps every parallel worker in `Task.Run(taskGetter)` and invokes no-`[Timeout]` test bodies directly on that worker, so QuickFiler.Test bodies are pool threads by default (serial or parallel). WPF `Dispatcher.CheckAccess()` is `Thread == Thread.CurrentThread` (object identity), so ManagedThreadId reuse is irrelevant to that guard.

**How to apply:** when a test needs a provably distinct thread, recommend `new Thread` + `Join()` (precedents: `EmailMoveMonitorTests.cs`, `UiThreadInitContract_Tests.ApartmentThreadRunner`, `BayesianPerformanceController.TestSupport.cs`); never `SetMinThreads` in committed tests. Same latent assumption exists in `BreadcrumbPopupBoundaryCoverageTests.cs:58` and `BreadcrumbUiThreadDispatchTests.cs:300` (separate issue). `referencesource.microsoft.com` now redirects to GitHub and its raw URLs 404; use `github.com/microsoft/referencesource/blob/master/...` (truncates large files) or the pfxteam "Task.Wait and Inlining" post. FA 8.10 `BeOfType` is exact-type equality; `Throw<T>` admits subtypes. Bash was disabled in this session (no git).
