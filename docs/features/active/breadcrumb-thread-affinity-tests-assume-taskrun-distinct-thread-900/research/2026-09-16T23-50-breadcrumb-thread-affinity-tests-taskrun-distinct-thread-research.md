# Research: breadcrumb thread-affinity tests assume `Task.Run` yields a distinct thread (Issue #900)

- Timestamp: 2026-09-16T23-50
- Issue: #900
- Branch: `bug/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900` (base `origin/main` at `91746d2e4776a59ee1db1856c5c490a009c4958b`, as recorded by the orchestrator in `issue.md`; `git` was not available to this session, see Limitations)
- Scope: two tests in `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`; no production change
- Author: task-researcher (research only; no source, configuration, or test files were modified)

Constraint blocks received from the orchestrator and acknowledged for verbatim propagation: `TEST PARALLELISM`, `OBSERVED-FAILING CRITERIA`, `DIFF BASES`, `COMMIT AND SCOPE`, `BASH DISCIPLINE`. No further delegation was issued from this research session.

## Limitations of this session

- The `Bash` tool was disabled for this session, so no `git` command (log, rev-parse, merge-base) could be run. Branch and base SHA are taken from the orchestrator's note in `issue.md` lines 26-34. The plan must re-verify the base with `git fetch` + `git merge-base HEAD origin/main` per the DIFF BASES block.
- `pwsh`, `msbuild`, `dotnet`, and `vstest` were not run. Every finding below is from static reading of the tree plus published documentation and source fetched over the web.
- Two upstream sources could not be quoted verbatim because the fetcher truncated them: `Task.InternalWait` in the .NET Framework reference source, and `ThreadPool.TryPopCustomWorkItem`. Where those are load-bearing, the finding is marked "corroborated, not quoted" and the corroborating sources are named.

## 1. Current state (verified against the tree)

### 1.1 The two tests under repair

| Test | Current line | Issue-body line | Assertion shape |
|---|---|---|---|
| `InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic` | 204 (attribute 203) | 237 | `Task.Run(...).GetAwaiter().GetResult()` at 214-218; `Throw<InvalidOperationException>().Where(Message.Contains("InitializeBreadcrumbPipeline")).Which.Should().NotBeOfType<ObjectDisposedException>()` at 221-227 |
| `ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic` | 237 (attribute 236) | 204 | `Task.Run(...).GetAwaiter().GetResult()` at 246-254; same chain at 257-263 with `"ConfigureBreadcrumbDropDown"` |

The orchestrator's re-derivation is confirmed: the two names appear in the opposite order in the current tree versus the GitHub issue body. The issue body's line numbers are stale; the plan must cite 204 and 237 as above.

### 1.2 The production guard (unchanged by this issue)

- `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:432-447` — `private void ThrowIfOffUiBoundary(string operation)`: reads `Dispatcher owning = UiDispatcher;` (line 434), returns when `owning == null` (435-438), throws `InvalidOperationException` with message `"{operation} must be called on the thread that owns this ItemViewer. The calling thread is not the thread the viewer was constructed on."` when `!owning.CheckAccess()` (440-446).
- `UiDispatcher` is `System.Windows.Threading.Dispatcher` (`QuickFiler/Viewers/ItemViewer.cs:64-68`), captured once in the constructor as `_uiDispatcher = Dispatcher.CurrentDispatcher;` (`ItemViewer.cs:27`). It is not the internal `BreadcrumbUiDispatcher` class.
- Guarded call sites, each with the guard as the first statement: `InitializeBreadcrumbPipeline(provider, operations)` at `ItemViewer.Breadcrumb.cs:51`; `ConfigureBreadcrumbDropDown(CoreWebView2Environment, IWebViewCoreInitializer)` at `:172`; `ConfigureBreadcrumbDropDown(IBreadcrumbDropDownHost, Func<Rectangle>, Func<Rectangle>)` at `:233` (the overload the test exercises); `EnsureBreadcrumbResourceOwnership` at `:393`.
- `ViewerScope` (`ItemViewerBreadcrumbThreadAffinityTests.cs:396-417`) installs a plain `SynchronizationContext` and constructs `new QuickFiler.ItemViewer()` on the calling thread (line 405), so the guard's owner is whatever thread MSTest ran the test method on.

### 1.3 What `Dispatcher.CheckAccess()` compares

WPF `Dispatcher.CheckAccess()` is `return Thread == Thread.CurrentThread;` where `Thread` returns the `_dispatcherThread` field stored at construction; `Dispatcher.FromThread` compares `Thread` object references (not ids) and prunes dead weak references (dotnet/wpf `WindowsBase/System/Windows/Threading/Dispatcher.cs`, fetched from the `main` branch; the Microsoft Learn page for the netframework-4.8.1 moniker states "Determines whether the calling thread is the thread associated with this Dispatcher" and "true if the calling thread is the thread associated with this Dispatcher"). Consequence: the guard proves ownership by `Thread` object identity. A different `Thread` object always fails `CheckAccess()`; the same `Thread` object always passes it. Managed-thread-id reuse is irrelevant to the guard itself.

## 2. Root cause, refined: the failing branch is wait-inlining, not idle-thread reuse

The issue text frames the defect as "thread reuse can make `CheckAccess()` return true". Static reading of the runtime shows a more specific and more deterministic mechanism for the exact shape `Task.Run(...).GetAwaiter().GetResult()`:

1. `TaskAwaiter.GetResult()` calls `ValidateEnd(m_task)`, which, when the task is not complete, calls `task.InternalWait(Timeout.Infinite, default(CancellationToken))` (microsoft/referencesource `mscorlib/system/runtime/compilerservices/TaskAwaiter.cs`, quoted verbatim by the fetch).
2. `Task.InternalWait` with an infinite timeout and a non-cancellable token attempts inline execution before blocking (`WrappedTryRunInline`). Corroborated, not quoted: Stephen Toub, "Task.Wait and 'Inlining'", pfxteam blog, 2009-10-15: "if it hasn't started executing, Wait may be able to pull the target task out of the scheduler" and "the default scheduler (based on the ThreadPool) is aggressive about inlining, but only if the task can be efficiently removed from the data structures that hold tasks internally in the ThreadPool (such as if the task is living in the local queue associated with the thread attempting to inline it)".
3. `ThreadPoolTaskScheduler.QueueTask` queues a non-`LongRunning` task with `ThreadPool.UnsafeQueueCustomWorkItem(task, forceToGlobalQueue)` where `forceToGlobalQueue` is true only for `PreferFairness`; `TryExecuteTaskInline` returns false only when `taskWasPreviouslyQueued && !ThreadPool.TryPopCustomWorkItem(task)` (microsoft/referencesource `mscorlib/system/threading/Tasks/ThreadPoolTaskScheduler.cs`, quoted verbatim by the fetch).
4. `ThreadPoolWorkQueue.Enqueue` pushes to the calling thread's local work-stealing queue when `ThreadPoolWorkQueueThreadLocals.threadLocals` is non-null, otherwise to the global queue; `threadLocals` is assigned by `EnsureCurrentThreadHasQueue`, which is called from `Dispatch`, i.e. only on thread-pool worker threads (microsoft/referencesource `mscorlib/system/threading/threadpool.cs`, partially quoted by the fetch).

Combined: when the test body itself runs on a thread-pool thread, `Task.Run` places the work item in that thread's own local queue, and `GetAwaiter().GetResult()` pops it back and runs the delegate on the same thread unless another worker steals it first. In that inlined branch the constructing `Thread` object and the calling `Thread` object are identical, `CheckAccess()` is true, the guard does not throw, and the `Throw<InvalidOperationException>()` assertion fails. In the stolen branch a different pool thread runs the delegate and the test passes. The outcome is decided by a microsecond-scale race between the local pop and a remote steal that the test does not control.

Side effect worth recording for the plan's failure-text expectations: in the inlined branch the ambient context is `ViewerScope.Context` (a plain non-null context), so `BreadcrumbUiDispatcher.CaptureCurrent()` (`QuickFiler/Viewers/BreadcrumbUiDispatcher.cs:44-56`) succeeds and the pipeline initializes; the observed failure is therefore "expected an InvalidOperationException, but no exception was thrown", not a different exception.

## 3. Answers to the research questions

### Q1. `ObjectDisposedException` inherits from `InvalidOperationException`

Confirmed. Microsoft Learn, `System.ObjectDisposedException` (moniker range includes netframework-4.8.1): declaration `public class ObjectDisposedException : InvalidOperationException`; inheritance chain for .NET Framework: `Object -> Exception -> SystemException -> InvalidOperationException -> ObjectDisposedException` (mscorlib.dll). Both projects target .NET Framework 4.8.1: `QuickFiler/QuickFiler.csproj:13` and `QuickFiler.Test/QuickFiler.Test.csproj:18` carry `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>`. The existing `Throw<InvalidOperationException>()...NotBeOfType<ObjectDisposedException>()` chain is therefore coherent: `Throw<T>` admits subtypes, so the explicit exclusion is what rules out the disposal exception.

### Q2. FluentAssertions `BeOfType<T>()` semantics (pinned version 8.10.0)

- Pinned version: `QuickFiler.Test/packages.config:8` (`FluentAssertions` 8.10.0) and `QuickFiler.Test.csproj:255-256` (HintPath `..\packages\FluentAssertions.8.10.0\lib\net47\FluentAssertions.dll`).
- Source at tag 8.10.0, `Src/FluentAssertions/Primitives/ReferenceTypeAssertions.cs`: `BeOfType<T>()` delegates to `BeOfType(typeof(T))`, which asserts `subjectType.Should().Be(expectedType, ...)` (exact runtime-type equality; generic types compare the generic type definition). `NotBeOfType<T>()` delegates to `NotBeOfType(Type)` with the negated equality. `BeAssignableTo<T>()` uses `Subject is T` (accepts subtypes).
- `ExceptionAssertions<TException>.Which` "Gets the exception object of the exception thrown" and returns `TException`; `Where(Expression<Func<TException,bool>>)` "Asserts that the exception matches a particular condition" (`Src/FluentAssertions/Specialized/ExceptionAssertions.cs` at 8.10.0). The exceptions documentation states `Throw<T>` is satisfied by a more specific exception and that `ThrowExactly<T>` is the strict form.
- Consequence: `captured.Should().BeOfType<InvalidOperationException>()` alone already excludes `ObjectDisposedException` (different runtime type). If the redesign keeps the `Action`-plus-`Throw<T>` chain (by rethrowing on the test thread), the explicit `NotBeOfType<ObjectDisposedException>()` remains necessary; if it asserts on a captured exception with `BeOfType<InvalidOperationException>()`, the `NotBeOfType` becomes redundant and may be kept only as documentation of intent.

### Q3. `ManagedThreadId` uniqueness

Microsoft Learn, `Thread.ManagedThreadId` (netframework-4.8.1 in range): "A thread's ManagedThreadId property value serves to uniquely identify that thread within its process" and "does not vary over time". The documentation page does not state a reuse-after-termination rule; the repository's own production remark (`ItemViewer.Breadcrumb.cs:425-427`) and the #781 potential document (`docs/features/potential/promoted/2026-09-05-breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers.md:114`) both state that ids are unique among live threads. What matters for this fix: a `new Thread(...)` started and joined while the constructing thread is alive and blocked in `Thread.Join()` is simultaneously live with the owner, so the two ids cannot collide under the documented uniqueness, and, independently of ids, the two `Thread` objects are distinct, which is what `CheckAccess()` compares (section 1.3). An id-based vacuity guard in the test is therefore sound, and a `Thread`-object-based one (`ReferenceEquals(Thread.CurrentThread, owner)` or `scope.Viewer.UiDispatcher.CheckAccess()` being false on the worker) is sounder still because it checks the guard's exact predicate.

### Q4. The other tests in the file and shared helpers

Complete test population (Numeric Derivation Evidence in section 7): seven `[TestMethod]`s.

| Line | Test | Uses `Task.Run`? | Affected by redesigning the two WorkerThread tests? |
|---|---|---|---|
| 39 | `InitializeBreadcrumbPipeline_ConstructedInsideDispatcherOperation_SucceedsUnderDifferentAmbientContext` | No | No: builds its own viewer via `Dispatcher.CurrentDispatcher.Invoke`, uses only `InertOperations()` |
| 89 | `InitializeBreadcrumbPipeline_OwningThreadNullAmbientContext_DoesNotThrow` | No | No: `ViewerScope` + `InertOperations()`, same-thread |
| 129 | `InitializeBreadcrumbPipeline_OwningThreadDifferentPlainContext_DoesNotThrow` | No | No |
| 168 | `ConfigureBreadcrumbDropDown_OwningThreadInsideDispatcherOperation_DoesNotThrow` | No | No: `ViewerScope`, `InertOperations()`, `InertDropDownHost`, same-thread `Dispatcher.Invoke` |
| 204 | `InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic` | Yes (214) | Target |
| 237 | `ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic` | Yes (246) | Target |
| 280 | `InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow` | Yes (293) | See below |

Shared private helpers: `InertOperations()` (313-316), `ClearViewerDispatcher` (322-332), `InertDropDownHost` (335-363), `DrainableSynchronizationContext` (371-390), `ViewerScope` (396-417). None hold static state; each test constructs its own instances. Adding a private static "run on a dedicated thread" helper and rewriting the two Act blocks changes no helper contract, so no sibling assumption is invalidated.

`InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow` (280-307): after `ClearViewerDispatcher` nulls `_uiDispatcher`, the guard returns at `ItemViewer.Breadcrumb.cs:435-438` regardless of thread, and the same-provider early return at `:60-72` runs before any `CaptureCurrent()`. Under the inlined branch the ambient context is the plain scope context; under the stolen branch it is null; neither path reaches a throw. A `NotThrow()` assertion is therefore not defeated by inlining and this test is not flaky. It is out of scope for #900. One residual observation, not a defect in the assertion: the remark at 273-277 claims the test would discriminate against the pre-#781 context-reference guard "because the pre-fix guard reads the non-null captured context and rejects the worker-thread call"; under the inlined branch the ambient context equals the captured one, so that discrimination claim holds only in the stolen branch. This does not affect pass/fail on the current tree and should be recorded as a potential follow-up rather than folded into #900.

### Q5. Other tests in the repo using `Task.Run` as a "different thread" for a thread-affinity assertion

Search: `Task.Run(` across `*.Test/**/*.cs` (54 occurrences, 32 files), then each QuickFiler.Test hit was read and classified. Only the two target tests assert that a guard throws because the `Task.Run` delegate is on a different thread. The other `Task.Run` sites fall into three classes:

- Completion-on-worker sources feeding a captured `SynchronizationContext`, asserted through post/drain counts and thread ids recorded by the context, not through a throw (`BreadcrumbSelectorToggleUiBoundaryTests.cs:61,75,137,148,154,161`; `BreadcrumbSelectorOpenRetryTests.cs:37,210,218,219`; `BreadcrumbCoordinatorLifecycleTests.cs:350`; `BreadcrumbPopupControlDispatchTests.cs:29,111,300`; `BreadcrumbPopupBoundaryCoverageTests.Part2.cs:192`; `BreadcrumbUiThreadDispatchTests.cs:51,90`). These assert that posted work runs on the creator thread when drained; if a delegate were inlined, the post count assertions would still be exercised by the dispatcher's boundary test (`BreadcrumbUiDispatcher.IsCurrentBoundary`), so they were not classified as the same defect. They are not audited further here.
- Two sites that do assert a cross-thread failure from a `Task.Run` delegate and share the same assumption: `BreadcrumbPopupBoundaryCoverageTests.cs:58-61` (`Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction`: an owner-only `BreadcrumbUiDispatcher` created on the test thread must report "cannot marshal" when `Dispatch` is called from `Task.Run`, and `executions.Should().Be(0)`), and `BreadcrumbUiThreadDispatchTests.cs:298-307` (`CreateForCurrentThreadTests()` then `await Task.Run(() => testDispatcher.DispatchValue(...))` expected to throw "cannot marshal cross-thread UI work"). `BreadcrumbUiDispatcher` compares `Environment.CurrentManagedThreadId` against `_ownerThreadId` (`BreadcrumbUiDispatcher.cs:40,54,64`), so an inlined delegate would pass the owner test and defeat both assertions in the same way. The second uses `await Task.Run(...)` rather than a blocking `GetResult()`; `await` does not attempt inlining, so it is exposed only to genuine idle-thread reuse, which cannot occur while the awaiting continuation has not yet been scheduled on that thread; its exposure is lower but not zero under the async-void/continuation reuse patterns. Both are candidates for a separate issue; they must not be changed under #900.
- `EmailMoveMonitorTests.cs:298-304` (`UnhookItem_InvokedFromThreadPoolThread_RunsComAccessOnMarshalTargetThread`) asserts `recordedBodyThreadId != callingThreadId`, where the body thread is a `new Thread` and the caller is the `Task.Run` delegate. Since the dedicated thread is always distinct from any pool thread, this is sound regardless of inlining. The class is `[DoNotParallelize]` (line 24), which is unrelated.

No other QuickFiler.Test file contains `WorkerThread`, `CheckAccess`, "different thread", or "distinct thread" (grep over `QuickFiler.Test/**/*.cs`).

### Q6. On which thread MSTest 4.4.0 runs a test body under `Workers=0`, `Scope=ClassLevel`

- Pinned adapter/framework: `MSTest.TestAdapter` 4.4.0 and `MSTest.TestFramework` 4.4.0 (`QuickFiler.Test/packages.config:123-124`; csproj HintPaths at 368-372). Parallelization comes only from `scripts/vscode/TaskMaster.cli.runsettings` (Workers 0, Scope ClassLevel); `QuickFiler.Test` has no `[assembly: Parallelize]` (only `UtilitiesCS.Test/Properties/AssemblyInfo.cs:18-21` carries one). No runsettings in the repo sets `ExecutionApartmentState` (grep over `*.runsettings`), so the MTA default applies.
- microsoft/testfx at tag `v4.4.0`, `src/Adapter/MSTestAdapter.PlatformServices/Execution/TestExecutionManager.cs`, `DefaultFactoryAsync`: `if (MSTestSettings.RunConfigurationSettings.ExecutionApartmentState == ApartmentState.STA && RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return StaThreadHelper.RunOnStaThreadAsync(taskGetter); if (!RuntimeContext.IsMultiThreaded) return taskGetter(); return Task.Run(taskGetter);` with the comment "If you replace this with `return taskGetter()`, you will break parallel tests".
- Same tag, `TestExecutionManager.ParallelExecution.cs`: `for (int i = 0; i < parallelWorkers; i++) tasks.Add(_taskFactory(async () => { while (!queue.IsEmpty) { if (queue.TryDequeue(out testSet)) { ... await ExecuteTestsWithTestRunnerAsync(...) } } }));` then `Task.WhenAll(tasks)`. Each worker is therefore a `Task.Run` work item, i.e. a thread-pool thread, and it drains class-level chunks serially.
- Same tag, `TestMethodInfo.Execution.cs`: without a `[Timeout]`, the method is invoked directly on the worker's current thread (`MethodInfo.GetInvokeResultWithParametersAsync(...)`, then `await ... ConfigureAwait(true)`); with a `[Timeout]`, `PlatformServiceProvider.Instance.ThreadOperations.Execute(ExecuteAsyncAction, ...)` is used, and the action blocks with `.GetAwaiter().GetResult()`. Neither of the two target tests carries `[Timeout]` (the only `[Timeout]` uses in QuickFiler.Test are in other files).
- Conclusion: on this pinned version and configuration the claim "the test's own calling thread is itself a ThreadPool thread" is accurate and should not be softened to "frequently"; it is the normal case for every test in QuickFiler.Test that has no `[Timeout]`. The only branches that would move a body off the pool are STA apartment configuration (absent here) and the `[Timeout]` path (absent on these tests). `Parallelize` is not what makes the thread a pool thread; `Task.Run(taskGetter)` is, so the same mechanism applies when the suite runs serially (CI also passes the runsettings since #869, see `.github/workflows/README.md:46-63` and `_mstest-coverage.yml:94`). Under a serial run the pool has more idle workers to steal the item, which is consistent with the tests appearing stable serially and flaky when the pool is saturated by `Workers=0`.

### Q7. Deterministic reproduction of the CURRENT tests' flakiness

Assessment of the proposed `ThreadPool.SetMinThreads(1, 1)` followed by two sequential `Task.Run(...).Wait()` calls (construct in one, invoke in the other, from the test thread):

- That shape differs from the shipped tests (which construct on the test thread and only invoke from `Task.Run`), so even a reliable same-id outcome would not reproduce the shipped assertion's failure; it would reproduce a related but different scenario.
- Reuse determinism is low. `SetMinThreads` neither retires existing idle workers nor caps the pool; the docs say only that the pool "provides new worker threads ... on demand until it reaches a specified minimum" and "creates and destroys worker threads in order to optimize throughput", and `SetMaxThreads` "cannot set the maximum number of worker threads ... smaller than the number of processors" nor below the minimum. Which idle worker picks the second item is not documented. Confidence that the two calls land on the identical `ManagedThreadId`: low-to-moderate, and unmeasurable without running it.
- `SetMinThreads`/`SetMaxThreads` are process-global. A committed test that changes them would perturb every class running in parallel and violates the environment-stability rule; it is admissible only in a throwaway diagnostic that is never committed.

A closer-to-deterministic diagnostic for the actual shape exists but rests on an undocumented pool policy: run the original test body from within a `Task.Run` work item while `Environment.ProcessorCount` other pool threads are blocked on a `ManualResetEventSlim`; the inner `Task.Run` then needs a thread beyond the minimum, which the .NET Framework pool injects only after its starvation/hill-climbing delay, so the local pop in `GetResult()` wins and the delegate is inlined (assert `Environment.CurrentManagedThreadId` equality and observe the `Throw` assertion fail). The delay is not documented on the Learn pages consulted, so this cannot be presented as a guaranteed repro. Confidence: moderate.

Recommendation: do not gate the plan on a deterministic failing run of the ORIGINAL two tests. Author `<FEATURE>/evidence/regression-testing/fail-before-exception.<timestamp>.md` with `WhyFailingRunImpossible` stating that the failure is a microsecond-scale local-pop versus remote-steal race inside the runtime's wait-inlining path that a test cannot control without mutating process-global thread-pool state, and provide as the alternative proof: (a) the mechanism chain in section 2 with its sources; (b) if run, the diagnostic's output showing same-thread inlining; and (c) the guard-disabled failing run of the NEW tests described in section 5, which is deterministic and satisfies the OBSERVED-FAILING block. Precedent for this dossier shape: `docs/features/archive/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/regression-testing/fail-before-exception.2026-07-07T14-05.md`.

### Q8. Existing repo guidance or helper for "genuinely distinct thread" tests

- No rule or doc prescribes a pattern: grep of `.claude/rules/*.md` for `Task.Run`, "dedicated thread", "distinct thread", "thread pool" found nothing; `docs/**/*.md` hits are feature folders, not guidance. `.claude/rules/general-unit-test.md` bans `Thread.Sleep`, `Task.Delay`, and wall-clock waits; an untimed `Thread.Join()` is a completion wait on a deterministic event and has three in-repo precedents.
- Reusable helper in QuickFiler.Test: none that fits. `BayesianPerformanceControllerTestSupport.RunWithViewer` (`QuickFiler.Test/Controllers/BayesianPerformanceController.TestSupport.cs:16-53`) runs an action on a `new Thread` (STA), captures the exception, joins, and rethrows with `ExceptionDispatchInfo.Capture(captured).Throw()`, but it is bound to `BayesianPerformanceViewer`. `QfcItemControllerTestSupport.StartRunningDispatcher` (`QfcItemController.TestSupport.cs:251-271`) starts an STA thread running a dispatcher loop; heavier than needed. `WinFormsPumpHost` (`QuickFiler.Test/TestSupport/WinFormsPumpHost.cs:53-58`) is a pump host. `EmailMoveMonitorTests.cs:281-288` inlines a `new Thread` + `Join` in the test body.
- `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs:78-104` has `ApartmentThreadRunner.RunOnThread(ApartmentState, Action)` returning the captured exception, `IsBackground = true`, `Join()`. It is `internal static` in a different test assembly and is not reachable from QuickFiler.Test; it is the right shape to copy as a private helper in the target file. The test file's own remarks (lines 21-25) already establish the convention of declaring private helpers locally rather than widening another file's accessibility.

### Q9. `using` directives

Lines 1-12 already import `System`, `System.Collections.Generic`, `System.Drawing`, `System.Reflection`, `System.Threading`, `System.Threading.Tasks`, `System.Windows.Threading`, `FluentAssertions`, `Microsoft.VisualStudio.TestTools.UnitTesting`, `Moq`, `QuickFiler.Viewers`, `UtilitiesCS.OutlookObjects.Folder`. A fix built on `System.Threading.Thread` and `Environment.CurrentManagedThreadId` needs no new directive. If the helper rethrows the captured exception on the test thread with `ExceptionDispatchInfo`, add `using System.Runtime.ExceptionServices;` (precedent: `BayesianPerformanceController.TestSupport.cs:3`). If the helper instead returns the captured `Exception` and the test asserts `BeOfType<InvalidOperationException>()`, no new directive is needed. `System.Threading.Tasks` stays in use by `InertDropDownHost` (`Task<bool>`, `Task.FromResult`) and the untouched test at line 293, so removing it is not required.

### Q10. Prior #781 material and stability of the guard's design intent

- `docs/features/potential/promoted/2026-09-05-breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers.md` (issue #781): root cause was the #488 D4 guard comparing `SynchronizationContext.Current` by reference; the fix proposal (line 118) names `_uiDispatcher.CheckAccess()` on the constructor-captured `Dispatcher` and says the worker case "needs no message pump because the guard throws before any control is touched" (line 119). Line 119 is also where the `Task.Run` worker shape was prescribed; #900 corrects that prescription's thread assumption, not the guard.
- `docs/features/active/2026-09-05-breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers-781/evidence/issue-updates/issue-781.2026-09-05T10-49.md` records all eight #781 ACs checked, including AC3: "A guarded member called from a different thread (for example a `Task.Run` worker) still throws `InvalidOperationException` whose message names the operation, and the exception is not an `ObjectDisposedException`." The contract the two tests assert is therefore ratified; only the means of obtaining "a different thread" is defective.
- No potential/promoted document contains "781" or "thread-affinity" beyond those two and two unrelated UiThread documents (`2026-09-05-uithread-synccontext-awaiter-always-posts-for-dispatcher-built-viewers.md`, `2026-09-07-uithread-init-contract-residuals-784-787-788.md`), neither of which questions the guard. The `#900` source potential file named in `issue.md:55` is absent from the tree, as the orchestrator noted.
- The guard's design intent (owner-thread identity via `Dispatcher.CheckAccess()`, null-owner escape) is stable and not under question by #900; the production remark at `ItemViewer.Breadcrumb.cs:416-431` is accurate and does not need editing.

## 4. Candidate approaches

### A. Dedicated `new Thread` per act (recommended)

Replace each `Task.Run(...).GetAwaiter().GetResult()` with a private static helper that starts a `new Thread(...)` with `IsBackground = true`, runs the guarded call inside `try/catch (Exception)`, records the exception, and `Join()`s. Assert the captured exception. Add a vacuity guard inside the thread body: `scope.Viewer.UiDispatcher.CheckAccess().Should().BeFalse(...)` (the guard's exact predicate) or `Environment.CurrentManagedThreadId.Should().NotBe(ownerId)`.

- Advantages: a `Thread` object created by the test is never the `Thread` object that constructed the viewer, so `CheckAccess()` is false by construction on every run and under any scheduler; no process-global state; no pump; no sleeps or timeouts; three in-repo precedents for the shape; default apartment (MTA) matches what a pool worker would have had, so the scenario's meaning is unchanged; the ambient `SynchronizationContext` on the new thread is null, which does not matter because the guard throws first (`:51`, `:233`).
- Limitations: about 25 lines of helper code in a 420-line file (stays under 500); the helper must marshal the exception explicitly.
- Convention fit: matches `ApartmentThreadRunner.RunOnThread` and `EmailMoveMonitorTests.cs:281-288`; keeps helpers private to the file as its remarks require.

### B. `Task.Factory.StartNew(..., TaskCreationOptions.LongRunning)`

`ThreadPoolTaskScheduler.QueueTask` creates a dedicated `new Thread` for `LongRunning` tasks (quoted in section 2), so the delegate never lands in a local queue and cannot be inlined or reused.

- Advantages: smallest textual diff; keeps `Action` + `Throw<T>` chain unchanged.
- Limitations: the distinct-thread property is an implementation detail of the default scheduler rather than a documented contract of `LongRunning` ("a hint"), so a reader has to know the scheduler internals to see why the test is sound; `.GetAwaiter().GetResult()` still blocks a pool worker; harder to add the vacuity guard cleanly. Rejected for relying on a hint whose documented semantics do not promise a new thread.

### C. Assert on `Environment.CurrentManagedThreadId` and retry until distinct

Rejected outright: retries and scheduling tolerance are prohibited by the TEST PARALLELISM block and the determinism policy.

Rejected alternatives summary: B (undocumented reliance on a scheduler hint), C (policy violation), any use of `ThreadPool.SetMinThreads/SetMaxThreads` in committed tests (process-global state).

## 5. Behavior semantics and requirements mapping

### 5.1 Intended behavior of the two tests after the fix

- Arrange: `ViewerScope` constructs the viewer on the test thread (unchanged), `InertOperations()`/`InertDropDownHost` as today.
- Act: run the guarded member on a thread that is provably not the owner; the thread body first proves the precondition (`UiDispatcher.CheckAccess()` is false there), then calls the member and captures any exception.
- Assert: the captured exception is exactly `InvalidOperationException` (not `ObjectDisposedException`), and its message contains the operation name (`"InitializeBreadcrumbPipeline"` / `"ConfigureBreadcrumbDropDown"`), matching the message template at `ItemViewer.Breadcrumb.cs:442-445`.
- Ordering/edge rules: the thread must be joined before `ViewerScope.Dispose()` runs (the `using` block guarantees this if the helper joins synchronously); the helper must not swallow a `null` exception silently (assert non-null with a reason); no `[Timeout]` and no `Join(timeout)`.

### 5.2 Observed-failing demonstration for the NEW tests (deterministic)

Two options, either satisfies the OBSERVED-FAILING block; the first needs no production edit:

1. Test-only: in a throwaway variant, call the existing `ClearViewerDispatcher(scope.Viewer)` before the act. The guard then takes its null-owner escape (`:435-438`); on the dedicated thread the ambient context is null, so `InitializeBreadcrumbPipeline` reaches `BreadcrumbUiDispatcher.CaptureCurrent()` via `EnsureBreadcrumbLifecycle` (`:74-76`, `:364`) and throws `InvalidOperationException("Breadcrumb UI components must be constructed on an owning UI synchronization context.")`, and `ConfigureBreadcrumbDropDown` does the same via `EnsureBreadcrumbLifecycle(BreadcrumbPopupUiOperations.CaptureCurrent)` (`:241-243`, `BreadcrumbPopupUiOperations.cs:80-81`). The expected failure is then the `.Where(Message.Contains(<operation>))` clause, not the `Throw<T>` clause. The plan must record this exact expected failure text so a pass is not mistaken for coverage of the wrong predicate.
2. Production throwaway: neutralize `ThrowIfOffUiBoundary` (return unconditionally), run the two tests, record the failure, revert. Same expected failure as above.

Either run proves the assertion is exercised and fails when the guard is absent. Note that with the ORIGINAL `Task.Run` shape the guard-disabled failure text differs by branch ("no exception was thrown" when inlined, the `CaptureCurrent` message otherwise), which is a second, independent symptom of the scheduling dependence and worth one line in the dossier.

### 5.3 Files to change

- `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` only: two Act blocks (214-218, 246-254), possibly the two assertion chains (221-227, 257-263), a new private static helper, updated XML remarks on the two tests and on the class remark at lines 21-28 (which currently promises "Every ambient-context substitution is confined to the test's own thread"; a dedicated worker thread does not substitute any context, so the sentence remains true, but the plan should re-read it). The file is already listed at `QuickFiler.Test.csproj:96`, so no csproj edit is needed.
- No production file. No runsettings change (TEST PARALLELISM block).

## 6. Testing implications

- The fix is itself test code; the regression evidence is: (a) fail-before dossier per Q7; (b) guard-disabled failing run per 5.2 (`ExpectedExitCode` non-zero in its own artifact file, per the evidence skill); (c) pass-after run of the whole `ItemViewerBreadcrumbThreadAffinityTests` class under `scripts/vscode/Invoke-MSTest.ps1`/`Invoke-MSTestWithCoverage.ps1` with the CLI runsettings (parallel), and the four-step toolchain in order.
- Coverage: no production lines change; `ItemViewer` is `[ExcludeFromCodeCoverage]` (`ItemViewer.cs:20`), so changed-line coverage is not measurable for the guard, as #781 already recorded; the test project is excluded from the denominator.
- Determinism audit for the new helper: no `Thread.Sleep`, `Task.Delay`, `Join(timeout)`, or `[Timeout]`; `Join()` waits on thread completion only; thread is background so a helper bug cannot keep the host alive.
- Separate-issue candidates (do not fix here): `BreadcrumbPopupBoundaryCoverageTests.cs:58-61` and `BreadcrumbUiThreadDispatchTests.cs:298-307` (same assumption against `BreadcrumbUiDispatcher`'s owner-id check), and the discrimination remark on `InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow` (Q4).

## 7. Numeric Derivation Evidence

### 7.1 Claim: the target file contains exactly 7 `[TestMethod]` tests

- Complete Family: all MSTest test methods declared in `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`.
- Exhaustive Search Scope: the whole file (420 lines), including nested classes.
- Inclusion Rules: a method carrying `[TestMethod]` (or a `public void` parameterless method immediately following such an attribute).
- Exclusion Rules: methods of nested helper classes (`InertDropDownHost`, `ViewerScope`) and non-attributed methods.
- Primary Search Strategy or Query Expression: `Grep` pattern `\[TestMethod\]` on the file.
- Primary Member Set: attribute lines 38, 88, 128, 167, 203, 236, 279, each immediately preceding the declarations at 39, 89, 129, 168, 204, 237, 280.
- Primary Count: 7.
- Cross-check Search Strategy or Query Expression: `Grep` pattern `public void [A-Za-z_]+\(\)` on the file (method-declaration shape, independent of attributes).
- Cross-check Member Set: lines 39, 89, 129, 168, 204, 237, 280 (test methods) plus 360 (`Reset`), 362 (`Dispose`), 412 (`Dispose`), the latter three excluded as nested-helper members with no `[TestMethod]`.
- Cross-check Count: 7 after exclusions (10 raw).
- Member-set Comparison: the normalized sets are identical (`{39, 89, 129, 168, 204, 237, 280}`); the claim is admitted.

### 7.2 Claim: the target file contains exactly 3 `Task.Run(...).GetAwaiter().GetResult()` sites, of which 2 are in scope

- Complete Family: blocking `Task.Run` invocations in the file.
- Exhaustive Search Scope: the whole file.
- Inclusion Rules: any `Task.Run(` call.
- Exclusion Rules: none.
- Primary Search Strategy or Query Expression: `Grep` pattern `Task\.Run\(`.
- Primary Member Set: lines 214, 246, 293.
- Primary Count: 3.
- Cross-check Search Strategy or Query Expression: `Grep` pattern `\.GetAwaiter\(\)` (the blocking continuation, a different token).
- Cross-check Member Set: lines 217, 253, 296, each three lines below a primary member and inside the same statement.
- Cross-check Count: 3.
- Member-set Comparison: one-to-one correspondence (214/217, 246/253, 293/296); the claim is admitted. The in-scope subset is the two inside `Throw<T>` assertions (214, 246); the third (293) is inside a `NotThrow()` assertion and is out of scope per Q4.

## 8. Sources consulted

- Repository files cited inline by path and line (paths relative to `<repo-root>`).
- Microsoft Learn: `System.ObjectDisposedException`; `System.Threading.Thread.ManagedThreadId`; `System.Threading.ThreadPool.SetMinThreads`; `System.Threading.ThreadPool.SetMaxThreads`; "The managed thread pool"; `System.Windows.Threading.Dispatcher.CheckAccess` (netframework-4.8.1 moniker); "Configure MSTest" (Parallelize entry).
- microsoft/referencesource (GitHub mirror, `master`): `mscorlib/system/threading/Tasks/ThreadPoolTaskScheduler.cs` (quoted), `mscorlib/system/runtime/compilerservices/TaskAwaiter.cs` (quoted), `mscorlib/system/threading/threadpool.cs` (partially quoted), `mscorlib/system/threading/Tasks/Task.cs` (truncated; not quoted).
- dotnet/wpf (`main`): `src/Microsoft.DotNet.Wpf/src/WindowsBase/System/Windows/Threading/Dispatcher.cs` (`CheckAccess`, `Thread`, `FromThread`).
- microsoft/testfx tag `v4.4.0`: `src/Adapter/MSTestAdapter.PlatformServices/Execution/TestExecutionManager.cs`, `TestExecutionManager.ParallelExecution.cs`, `TestMethodInfo.Execution.cs`.
- fluentassertions/fluentassertions tag `8.10.0`: `Src/FluentAssertions/Primitives/ReferenceTypeAssertions.cs`, `Src/FluentAssertions/Specialized/ExceptionAssertions.cs`; fluentassertions.com "Exceptions" page.
- Stephen Toub, "Task.Wait and 'Inlining'", Microsoft DevBlogs (pfxteam), 2009-10-15.
