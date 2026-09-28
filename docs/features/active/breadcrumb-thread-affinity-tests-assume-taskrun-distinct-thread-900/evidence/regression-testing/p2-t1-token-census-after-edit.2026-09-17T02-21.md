# P2-T1 — Post-Edit Token Census and Anchored Hunk Audit

Timestamp: 2026-09-17T02-21

Command: `CMD-CENSUS` over
`QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`; then
`git diff -U0 66b65a4626095ade5a01643aee4a43c90cc58cbf -- QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`.

EXIT_CODE: 0

CHANNEL: COMMAND

## Census

    TOKEN Task.Run( = 1
    TOKEN .GetAwaiter() = 1
    TOKEN Throw<InvalidOperationException>( = 0
    TOKEN error.Message.Contains( = 0
    TOKEN Contains( = 0
    TOKEN NotBeOfType<ObjectDisposedException>() = 2
    TOKEN ClearViewerDispatcher(scope.Viewer); = 1
    TOKEN [TestMethod] = 7
    TOKEN vacuously = 3
    TOKEN RunOnDedicatedWorkerThread( = 3
    TOKEN Exception captured = RunOnDedicatedWorkerThread( = 2
    TOKEN new Thread( = 1
    TOKEN IsBackground = true = 1
    TOKEN thread.Join(); = 1
    TOKEN Join( = 4
    TOKEN Join() = 4
    TOKEN UiDispatcher.CheckAccess() = 2
    TOKEN isOwnerThread = 4
    TOKEN BeOfType<InvalidOperationException>() = 2
    TOKEN captured.Message.Should().Contain( = 2
    TOKEN #900 = 3
    TOKEN action(); = 1
    TOKEN Thread.Sleep = 0
    TOKEN Task.Delay = 0
    TOKEN [Timeout = 0
    TOKEN DoNotParallelize = 0
    LINES = 490
    SHA256 = 8EBC19F829536957BEB0DAEC628929CA8DE3F11E10AA819DEA41362C6FCBB164

### Acceptance against the Token Census Expectations table

Every value equals the "After P2-T1 and P2-T2" column, including each of the twenty the acceptance
condition names explicitly. The transitions measured against the P1-T1 positive controls are:

| Token | Before (P1-T1) | After | Meaning |
| --- | --- | --- | --- |
| `Task.Run(` | 3 | 1 | Both in-scope `Task.Run` act phases removed; the survivor is the out-of-scope `NotThrow` test. |
| `.GetAwaiter()` | 3 | 1 | Same, for the blocking continuation. |
| `Throw<InvalidOperationException>(` | 2 | 0 | The `Action`-plus-`Throw<T>` chains are gone. |
| `error.Message.Contains(` | 2 | 0 | Replaced by an assertion on the captured exception. |
| `Contains(` | 2 | 0 | No `Contains(` remains; the new assertion uses `Contain(`, a different literal. |
| `RunOnDedicatedWorkerThread(` | 0 | 3 | Two call sites plus one declaration. |
| `Exception captured = RunOnDedicatedWorkerThread(` | 0 | 2 | One per rewritten test. |
| `new Thread(` | 0 | 1 | The single dedicated-thread construction, in the helper. |
| `IsBackground = true` | 0 | 1 | The helper's background flag. |
| `thread.Join();` | 0 | 1 | The single untimed completion wait. |
| `UiDispatcher.CheckAccess()` | 0 | 2 | The precondition, once inside each delegate. |
| `isOwnerThread` | 0 | 4 | Declaration and assertion subject, twice each. |
| `BeOfType<InvalidOperationException>()` | 0 | 2 | Exact-type assertion, once per test. |
| `captured.Message.Should().Contain(` | 0 | 2 | Operation-name assertion, once per test. |
| `#900` | 0 | 3 | Issue reference in the two test remarks and the helper remark. |
| `vacuously` | 1 | 3 | Pre-existing prose occurrence plus the two `BeFalse` reason strings. |
| `action();` | 0 | 1 | The helper's single invocation of the delegate. |

Invariants held rather than moved: `NotBeOfType<ObjectDisposedException>()` 2,
`ClearViewerDispatcher(scope.Viewer);` 1, `[TestMethod]` 7.

Banned-API tokens are 0 each, as before: `Thread.Sleep`, `Task.Delay`, `[Timeout`,
`DoNotParallelize`. No test was serialised, pinned, retried, or given a tolerance.

`Join(` equals `Join()`: both 4. The equality is the gate, not the value. It proves no
`Join(timeout)` form exists anywhere in the file: every occurrence of `Join(` is immediately closed,
so each is either the code's `thread.Join();` or one of the three documentation references
`<c>Thread.Join()</c>`, `<c>Thread.Join()</c>` and `<c>Join()</c>`. A `Join(` count exceeding the
`Join()` count would be a timed join.

`LINES = 490`, at least 421 and at most 500. The arithmetic is 419 minus the 67 replaced lines (199
through 265) plus 106 test-block lines (53 plus one separating blank plus 52) plus 32 helper-block
lines including its leading blank.

The remarks avoid the literals `Task.Run(`, `.GetAwaiter()`, `RunOnDedicatedWorkerThread(` and
`UiDispatcher.CheckAccess()`, referring to those members as `Task.Run`, `GetResult()`,
`RunOnDedicatedWorkerThread` and `Dispatcher.CheckAccess()` respectively, so the code counts above
are counts of code and not of prose.

## Anchored hunk audit

The diff below is anchored on the merge base with `origin/main`. The Write Set file is unchanged
between the merge base and the pre-edit `HEAD`, so this diff shows exactly this task's edit.

Hunk headers and their old-side ranges:

| Hunk header | Old-side range | Classification |
| --- | --- | --- |
| `@@ -202,0 +203,15 @@` | pure insertion at 202 | inside 199-265 |
| `@@ -213,6 +228,11 @@` | 213-218 | inside 199-265 |
| `@@ -221,7 +241,9 @@` | 221-227 | inside 199-265 |
| `@@ -235,0 +258,10 @@` | pure insertion at 235 | inside 199-265 |
| `@@ -245,10 +277,15 @@` | 245-254 | inside 199-265 |
| `@@ -257,7 +294,9 @@` | 257-263 | inside 199-265 |
| `@@ -333,0 +373,32 @@` | pure insertion, old count 0, old start 333 | the helper insertion |

Six hunks lie entirely within pre-edit lines 199 through 265. The seventh is a pure insertion whose
old-side start is 333, which is the permitted helper-insertion position. There is no hunk anywhere
else in the file, which is the mechanical proof that no sibling region changed: the using block
(lines 1-12), the class remark (16-28), the five sibling tests, `InertOperations`,
`ClearViewerDispatcher`, `InertDropDownHost`, `DrainableSynchronizationContext` and `ViewerScope`
are byte-identical before and after.

The helper is inserted after old line 333, which was the blank line following
`ClearViewerDispatcher`'s closing brace at 332. It is therefore preceded by exactly one blank line,
and the insertion's own trailing blank line separates it from the `InertDropDownHost`
documentation.

### Verbatim diff

    diff --git a/QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs b/QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs
    index edf4e55e3..fe3094d7f 100644
    --- a/QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs
    +++ b/QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs
    @@ -202,0 +203,15 @@ namespace QuickFiler.Test.Viewers
    +        /// <remarks>
    +        /// Issue #900: the worker is a dedicated thread created by <c>RunOnDedicatedWorkerThread</c>,
    +        /// never a <c>Task.Run</c> work item. A work item queued from a thread-pool thread lands on
    +        /// that thread's local queue, and a blocking wait on it can run the delegate inline on the
    +        /// constructing thread, in which case <c>Dispatcher.CheckAccess()</c> is true and the guard
    +        /// never throws. A thread object this test constructs is never the object that constructed
    +        /// the viewer, so the precondition asserted inside the delegate holds by construction under
    +        /// any scheduler, including the <c>Workers=0</c> class-level parallel run. The helper's
    +        /// untimed <c>Thread.Join()</c> is a completion wait for one synchronous call on a dedicated
    +        /// non-pool thread; unlike the previous blocking <c>GetResult()</c> shape it never parks a
    +        /// thread-pool slot waiting on another thread-pool slot, so it adds no starvation risk under
    +        /// parallel execution. <c>BeOfType</c> is an exact-type check, so the derived
    +        /// <see cref="ObjectDisposedException"/> is excluded by it as well as by the explicit
    +        /// <c>NotBeOfType</c> that documents the intent.
    +        /// </remarks>
    @@ -213,6 +228,11 @@ namespace QuickFiler.Test.Viewers
    -                Action act = () =>
    -                    Task.Run(() =>
    -                            scope.Viewer.InitializeBreadcrumbPipeline(provider.Object, operations)
    -                        )
    -                        .GetAwaiter()
    -                        .GetResult();
    +                Exception captured = RunOnDedicatedWorkerThread(() =>
    +                {
    +                    bool isOwnerThread = scope.Viewer.UiDispatcher.CheckAccess();
    +                    isOwnerThread
    +                        .Should()
    +                        .BeFalse(
    +                            "the dedicated worker thread must not be the thread that constructed "
    +                                + "the viewer, or the boundary assertion would pass vacuously"
    +                        );
    +                    scope.Viewer.InitializeBreadcrumbPipeline(provider.Object, operations);
    +                });
    @@ -221,7 +241,9 @@ namespace QuickFiler.Test.Viewers
    -                act.Should()
    -                    .Throw<InvalidOperationException>(
    -                        "a worker thread is not the thread that constructed the viewer"
    -                    )
    -                    .Where(error => error.Message.Contains("InitializeBreadcrumbPipeline"))
    -                    .Which.Should()
    -                    .NotBeOfType<ObjectDisposedException>();
    +                captured
    +                    .Should()
    +                    .NotBeNull(
    +                        "a worker thread is not the thread that constructed the viewer, so the "
    +                            + "guard must throw rather than admit the call"
    +                    );
    +                captured.Should().BeOfType<InvalidOperationException>();
    +                captured.Message.Should().Contain("InitializeBreadcrumbPipeline");
    +                captured.Should().NotBeOfType<ObjectDisposedException>();
    @@ -235,0 +258,10 @@ namespace QuickFiler.Test.Viewers
    +        /// <remarks>
    +        /// Issue #900: the worker is a dedicated thread created by <c>RunOnDedicatedWorkerThread</c>
    +        /// rather than a <c>Task.Run</c> work item, for the reason given on
    +        /// <c>InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic</c>: a pool work
    +        /// item can be inlined onto the constructing thread, and a thread this test creates cannot.
    +        /// The precondition inside the delegate proves the call is off the owning thread before the
    +        /// guarded member runs. The helper's untimed <c>Thread.Join()</c> waits for one synchronous
    +        /// call on a non-pool thread and parks no thread-pool slot, so it is safe under the
    +        /// <c>Workers=0</c> class-level parallel run.
    +        /// </remarks>
    @@ -245,10 +277,15 @@ namespace QuickFiler.Test.Viewers
    -                Action act = () =>
    -                    Task.Run(() =>
    -                            scope.Viewer.ConfigureBreadcrumbDropDown(
    -                                host,
    -                                () => new Rectangle(0, 0, 10, 10),
    -                                () => new Rectangle(0, 0, 1920, 1040)
    -                            )
    -                        )
    -                        .GetAwaiter()
    -                        .GetResult();
    +                Exception captured = RunOnDedicatedWorkerThread(() =>
    +                {
    +                    bool isOwnerThread = scope.Viewer.UiDispatcher.CheckAccess();
    +                    isOwnerThread
    +                        .Should()
    +                        .BeFalse(
    +                            "the dedicated worker thread must not be the thread that constructed "
    +                                + "the viewer, or the boundary assertion would pass vacuously"
    +                        );
    +                    scope.Viewer.ConfigureBreadcrumbDropDown(
    +                        host,
    +                        () => new Rectangle(0, 0, 10, 10),
    +                        () => new Rectangle(0, 0, 1920, 1040)
    +                    );
    +                });
    @@ -257,7 +294,9 @@ namespace QuickFiler.Test.Viewers
    -                act.Should()
    -                    .Throw<InvalidOperationException>(
    -                        "a worker thread is not the thread that constructed the viewer"
    -                    )
    -                    .Where(error => error.Message.Contains("ConfigureBreadcrumbDropDown"))
    -                    .Which.Should()
    -                    .NotBeOfType<ObjectDisposedException>();
    +                captured
    +                    .Should()
    +                    .NotBeNull(
    +                        "a worker thread is not the thread that constructed the viewer, so the "
    +                            + "guard must throw rather than admit the call"
    +                    );
    +                captured.Should().BeOfType<InvalidOperationException>();
    +                captured.Message.Should().Contain("ConfigureBreadcrumbDropDown");
    +                captured.Should().NotBeOfType<ObjectDisposedException>();
    @@ -333,0 +373,32 @@ namespace QuickFiler.Test.Viewers
    +        /// <summary>
    +        /// Runs <paramref name="action"/> on a dedicated background thread, joins it, and returns
    +        /// the exception it threw, or <see langword="null"/> when it completed normally.
    +        /// </summary>
    +        /// <remarks>
    +        /// Issue #900: a <c>Task.Run</c> work item is not guaranteed to run on a thread other than
    +        /// the caller's, so it cannot stand in for a different thread in a thread-identity test. A
    +        /// thread this method constructs is distinct from every live thread by construction. The
    +        /// untimed <c>Join()</c> is a completion wait on one bounded synchronous call, not a sleep or
    +        /// a wall-clock wait, and the waiting thread and the waited-for thread are never both
    +        /// thread-pool workers, so the wait cannot starve the pool under parallel execution.
    +        /// </remarks>
    +        private static Exception RunOnDedicatedWorkerThread(Action action)
    +        {
    +            Exception captured = null;
    +            var thread = new Thread(() =>
    +            {
    +                try
    +                {
    +                    action();
    +                }
    +                catch (Exception error)
    +                {
    +                    captured = error;
    +                }
    +            });
    +            thread.IsBackground = true;
    +            thread.Start();
    +            thread.Join();
    +            return captured;
    +        }
    +

## Gate note

No PreToolUse hook refused the `.cs` edit. `PRE-IMPLEMENTATION GATE BLOCKED` was not reached. The
edit required no new `using` directive: `System`, `System.Threading` and `System.Threading.Tasks`
are already imported at lines 1-12, and `System.Threading.Tasks` remains in use by
`InertDropDownHost` and by the out-of-scope test that still uses `Task.Run`.
