# Fail-Before Exception Dossier — Issue #900

Timestamp: 2026-09-17T02-19

Scope: the two original, unfixed tests
`InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic` and
`ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic` in
`QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`.

WhyFailingRunImpossible: The original tests' failure is a microsecond-scale race inside the
runtime's wait-inlining path, between the local-queue pop performed by `TaskAwaiter.GetResult()` on
a pool thread and a remote worker's steal of the same `Task.Run` work item. A test cannot force the
inlined branch without mutating process-global thread-pool state through `ThreadPool.SetMinThreads`
or `SetMaxThreads`, which is prohibited in committed tests by the repository's environment-stability
rule and would perturb every class running concurrently under `Workers=0`, `Scope=ClassLevel`.
Neither branch of that race is selectable by the test, so no deterministic failing run of the
original pair can be recorded.

## Alternative Proof

### Mechanism chain (research artifact, section 2)

The defect is not "thread reuse" in general. It is a specific, documented path that makes the
delegate run on the calling thread.

1. `TaskAwaiter.GetResult()` calls `ValidateEnd(m_task)`, which, when the task is not complete,
   calls `task.InternalWait(Timeout.Infinite, default(CancellationToken))`.
   Source: microsoft/referencesource,
   `mscorlib/system/runtime/compilerservices/TaskAwaiter.cs`, quoted verbatim in the research
   artifact.
2. `Task.InternalWait` with an infinite timeout and a non-cancellable token attempts inline
   execution before blocking, through `WrappedTryRunInline`. Corroborated rather than quoted,
   because the fetcher truncated `Task.cs`: Stephen Toub, "Task.Wait and 'Inlining'", Microsoft
   DevBlogs (pfxteam), 2009-10-15, states that "if it hasn't started executing, Wait may be able to
   pull the target task out of the scheduler", and that the default ThreadPool-based scheduler is
   "aggressive about inlining, but only if the task can be efficiently removed from the data
   structures that hold tasks internally in the ThreadPool (such as if the task is living in the
   local queue associated with the thread attempting to inline it)".
3. `ThreadPoolTaskScheduler.QueueTask` queues a non-`LongRunning` task with
   `ThreadPool.UnsafeQueueCustomWorkItem(task, forceToGlobalQueue)`, where `forceToGlobalQueue` is
   true only for `PreferFairness`; `TryExecuteTaskInline` returns false only when
   `taskWasPreviouslyQueued && !ThreadPool.TryPopCustomWorkItem(task)`.
   Source: microsoft/referencesource,
   `mscorlib/system/threading/Tasks/ThreadPoolTaskScheduler.cs`, quoted verbatim.
4. `ThreadPoolWorkQueue.Enqueue` pushes to the calling thread's own local work-stealing queue when
   `ThreadPoolWorkQueueThreadLocals.threadLocals` is non-null, and `threadLocals` is assigned by
   `EnsureCurrentThreadHasQueue`, which is called only from `Dispatch`, that is, only on thread-pool
   worker threads.
   Source: microsoft/referencesource, `mscorlib/system/threading/threadpool.cs`, partially quoted.

Combined: when the test body itself runs on a thread-pool thread, `Task.Run` places the work item on
that thread's own local queue, and `GetAwaiter().GetResult()` pops it back off and runs the delegate
inline unless another worker steals it first. The outcome is decided by a race the test does not
control.

### Why the inlined branch defeats the assertion (research artifact, section 1.3)

WPF `Dispatcher.CheckAccess()` is `return Thread == Thread.CurrentThread;`, comparing the `Thread`
object stored at Dispatcher construction against the calling thread's `Thread` object. It is an
object-identity comparison, not a managed-thread-id comparison. `ItemViewer` captures its owner once
in its constructor as `_uiDispatcher = Dispatcher.CurrentDispatcher` at
`QuickFiler/Viewers/ItemViewer.cs:27`. In the inlined branch the constructing `Thread` object and
the calling `Thread` object are the same object, so `CheckAccess()` returns true, the guard
`ThrowIfOffUiBoundary` at `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:432-447` does not throw, and
the `Throw<InvalidOperationException>()` assertion fails with "expected an InvalidOperationException,
but no exception was thrown". In the stolen branch a different pool thread runs the delegate and the
test passes.

### Why the test body is itself on a pool thread (research artifact, Q6)

On the pinned `MSTest.TestAdapter` and `MSTest.TestFramework` 4.4.0, `DefaultFactoryAsync` in
`src/Adapter/MSTestAdapter.PlatformServices/Execution/TestExecutionManager.cs` returns
`Task.Run(taskGetter)` for every worker unless the run is configured for STA, carrying the comment
"If you replace this with `return taskGetter()`, you will break parallel tests". No runsettings in
this repository sets `ExecutionApartmentState`, so the MTA default applies. Without a `[Timeout]`
attribute the test method is invoked directly on that worker's current thread
(`TestMethodInfo.Execution.cs`), and neither target test carries `[Timeout]`. The precondition for
the mechanism above — that the test's own calling thread is a ThreadPool thread — therefore holds as
the normal case rather than occasionally. It is `Task.Run(taskGetter)` that makes the thread a pool
thread, not the `Parallelize` setting, so the same mechanism applies under a serial run; a serial
run simply has more idle workers available to steal the item, which is consistent with the tests
appearing stable serially and intermittent when the pool is saturated under `Workers=0`.

### Direct observation of the unfixed tests on this run

P0-T9 ran the whole `ItemViewerBreadcrumbThreadAffinityTests` class against the unfixed file under
`scripts/vscode/TaskMaster.cli.runsettings` and recorded
`ORIGINAL-FLAKE-OBSERVED: NO`: `total=7 executed=7 passed=7 failed=0`. The repository-wide baseline
run in P0-T10 observed the same two tests `Passed` as well.

That observation is recorded here for completeness and is not evidence against the defect. A green
observation samples the stolen branch once. It carries no information about the reachability of the
inlined branch, which is exactly the property that makes the failure unrecordable on demand and is
the reason this dossier exists rather than a failing run.

### Deterministic substitute, deferred to the replacement tests

`P3-T1` is the task that demonstrates deterministically that the replacement assertions are not
vacuous: it disables the boundary guard for the rewritten tests by inserting a temporary, fully
reverted `ClearViewerDispatcher(scope.Viewer)` call inside each dedicated-thread delegate, after the
precondition assertion and before the guarded call, so the guard takes its null-owner escape at
`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:435-438`, and records the resulting run. No production
file is edited at any point. That task has not run at the time this dossier is written, and no
result of it is stated here.

A second, independent symptom of the same scheduling dependence is worth one line: with the original
`Task.Run` shape, a guard-disabled run's failure text differs by branch — "no exception was thrown"
in the inlined branch and the `CaptureCurrent()` message in the stolen branch — so even the
mutation evidence would have been branch-dependent had the original shape been retained.

## Output Summary

A deterministic failing run of the two original tests is not achievable without mutating
process-global thread-pool state, which committed tests may not do. In its place this dossier
records the four-step runtime mechanism chain with its primary sources, the object-identity
semantics of `Dispatcher.CheckAccess()` that the inlined branch defeats, the pinned MSTest 4.4.0
behaviour that puts the test body on a pool thread in the first place, and the direct observation
from P0-T9 (`ORIGINAL-FLAKE-OBSERVED: NO`) with an explicit statement of what that observation does
and does not establish. The deterministic non-vacuity proof for the replacement tests is produced
separately by the plan's mutation-testing phase.

Precedent for this dossier shape:
`docs/features/archive/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/regression-testing/fail-before-exception.2026-07-07T14-05.md`.
