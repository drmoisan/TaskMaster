using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Viewers;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Test.Viewers
{
    /// <summary>
    /// Issue #781: the breadcrumb affinity guard must prove UI ownership by owner-thread identity
    /// rather than by <see cref="SynchronizationContext"/> reference equality, so a viewer built
    /// inside a WPF dispatcher operation is still usable from its own thread.
    /// </summary>
    /// <remarks>
    /// Declares its own viewer-scope, drainable-context, and drop-down host helpers, because the
    /// equivalents in <see cref="ItemViewerBreadcrumbLifecycleRegressionTests"/> are <c>private</c>;
    /// no existing file's accessibility is widened. Every ambient-context substitution is confined
    /// to the test's own thread and restored in a <c>finally</c>. Only the same-thread
    /// <c>Dispatcher.Invoke(Action)</c> fast path is used, which runs its callback inline at
    /// <see cref="DispatcherPriority.Send"/> and needs no message pump.
    /// </remarks>
    [TestClass]
    public sealed class ItemViewerBreadcrumbThreadAffinityTests
    {
        /// <summary>
        /// The production shape of issue #781: the viewer is constructed inside a dispatcher
        /// operation, so it captures a <c>DispatcherSynchronizationContext</c> that is never the
        /// thread's ambient context again. Initializing the pipeline afterwards on that same thread
        /// must succeed.
        /// </summary>
        [TestMethod]
        public void InitializeBreadcrumbPipeline_ConstructedInsideDispatcherOperation_SucceedsUnderDifferentAmbientContext()
        {
            // Arrange
            SynchronizationContext previous = SynchronizationContext.Current;
            QuickFiler.ItemViewer viewer = null;
            try
            {
                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
                Dispatcher.CurrentDispatcher.Invoke(
                    (Action)(() => viewer = new QuickFiler.ItemViewer())
                );
                BreadcrumbPopupUiOperations operations = InertOperations();
                var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
                ReferenceEquals(viewer.UiSyncContext, SynchronizationContext.Current)
                    .Should()
                    .BeFalse(
                        "the dispatcher operation must have captured a context that is not the "
                            + "thread's ambient context, or this test would pass vacuously"
                    );

                // Act
                Action act = () => viewer.InitializeBreadcrumbPipeline(provider.Object, operations);

                // Assert
                act.Should()
                    .NotThrow(
                        "the calling thread is the thread that constructed the viewer, whatever "
                            + "context happens to be ambient"
                    );
                viewer.BreadcrumbCoordinator.Should().NotBeNull();
            }
            finally
            {
                viewer?.Dispose();
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }

        /// <summary>
        /// A repeat call on the owning thread with a null ambient context must not throw and must
        /// keep the existing coordinator.
        /// </summary>
        /// <remarks>
        /// The repeat-with-same-provider shape is required rather than stylistic. A first-time
        /// initialization reaches <c>BreadcrumbUiDispatcher.CaptureCurrent()</c>, which throws under
        /// a null ambient context whatever the guard does, so only a call that returns through the
        /// already-initialized early return can witness this case. It still discriminates: the
        /// pre-fix guard rejects the second call before that early return is reached.
        /// </remarks>
        [TestMethod]
        public void InitializeBreadcrumbPipeline_OwningThreadNullAmbientContext_DoesNotThrow()
        {
            // Arrange
            using (var scope = new ViewerScope())
            {
                BreadcrumbPopupUiOperations operations = InertOperations();
                var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
                scope.Viewer.InitializeBreadcrumbPipeline(provider.Object, operations);
                object before = scope.Viewer.BreadcrumbCoordinator;
                before.Should().NotBeNull("the first call must build a breadcrumb coordinator");

                // Act
                Action act = () =>
                {
                    SynchronizationContext seeded = SynchronizationContext.Current;
                    try
                    {
                        SynchronizationContext.SetSynchronizationContext(null);
                        scope.Viewer.InitializeBreadcrumbPipeline(provider.Object, operations);
                    }
                    finally
                    {
                        SynchronizationContext.SetSynchronizationContext(seeded);
                    }
                };

                // Assert
                act.Should()
                    .NotThrow(
                        "a null ambient context does not move the call off the owning thread"
                    );
                scope.Viewer.BreadcrumbCoordinator.Should().BeSameAs(before);
            }
        }

        /// <summary>
        /// A different plain <see cref="SynchronizationContext"/> instance installed on the owning
        /// thread must not be treated as a boundary violation.
        /// </summary>
        [TestMethod]
        public void InitializeBreadcrumbPipeline_OwningThreadDifferentPlainContext_DoesNotThrow()
        {
            // Arrange
            using (var scope = new ViewerScope())
            {
                BreadcrumbPopupUiOperations operations = InertOperations();
                var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
                var foreign = new SynchronizationContext();

                // Act
                Action act = () =>
                {
                    SynchronizationContext seeded = SynchronizationContext.Current;
                    try
                    {
                        SynchronizationContext.SetSynchronizationContext(foreign);
                        scope.Viewer.InitializeBreadcrumbPipeline(provider.Object, operations);
                    }
                    finally
                    {
                        SynchronizationContext.SetSynchronizationContext(seeded);
                    }
                };

                // Assert
                act.Should()
                    .NotThrow(
                        "context identity is not what the guard proves; owner-thread identity is"
                    );
                scope.Viewer.BreadcrumbCoordinator.Should().NotBeNull();
            }
        }

        /// <summary>
        /// The third ambient shape issue #781 names, exercised against a second guarded member: the
        /// call site runs inside a dispatcher operation, so a <c>DispatcherSynchronizationContext</c>
        /// is ambient, and the call must still be admitted on the owning thread.
        /// </summary>
        [TestMethod]
        public void ConfigureBreadcrumbDropDown_OwningThreadInsideDispatcherOperation_DoesNotThrow()
        {
            // Arrange
            using (var scope = new ViewerScope())
            {
                BreadcrumbPopupUiOperations operations = InertOperations();
                var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
                scope.Viewer.InitializeBreadcrumbPipeline(provider.Object, operations);
                var host = new InertDropDownHost();

                // Act
                Action act = () =>
                    Dispatcher.CurrentDispatcher.Invoke(
                        (Action)(
                            () =>
                                scope.Viewer.ConfigureBreadcrumbDropDown(
                                    host,
                                    () => new Rectangle(0, 0, 10, 10),
                                    () => new Rectangle(0, 0, 1920, 1040)
                                )
                        )
                    );

                // Assert
                act.Should()
                    .NotThrow(
                        "a dispatcher operation on the owning thread is still the owning thread"
                    );
            }
        }

        /// <summary>
        /// A genuine cross-thread call must still fail fast with a diagnostic naming the operation,
        /// and must not be an <see cref="ObjectDisposedException"/>.
        /// </summary>
        /// <remarks>
        /// Issue #900: the worker is a dedicated thread created by <c>RunOnDedicatedWorkerThread</c>,
        /// never a <c>Task.Run</c> work item. A work item queued from a thread-pool thread lands on
        /// that thread's local queue, and a blocking wait on it can run the delegate inline on the
        /// constructing thread, in which case <c>Dispatcher.CheckAccess()</c> is true and the guard
        /// never throws. A thread object this test constructs is never the object that constructed
        /// the viewer, so the precondition asserted inside the delegate holds by construction under
        /// any scheduler, including the <c>Workers=0</c> class-level parallel run. The helper's
        /// untimed <c>Thread.Join()</c> is a completion wait for one synchronous call on a dedicated
        /// non-pool thread; unlike the previous blocking <c>GetResult()</c> shape it never parks a
        /// thread-pool slot waiting on another thread-pool slot, so it adds no starvation risk under
        /// parallel execution. <c>BeOfType</c> is an exact-type check, so the derived
        /// <see cref="ObjectDisposedException"/> is excluded by it as well as by the explicit
        /// <c>NotBeOfType</c> that documents the intent.
        /// </remarks>
        [TestMethod]
        public void InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic()
        {
            // Arrange
            using (var scope = new ViewerScope())
            {
                BreadcrumbPopupUiOperations operations = InertOperations();
                var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);

                // Act
                Exception captured = RunOnDedicatedWorkerThread(() =>
                {
                    bool isOwnerThread = scope.Viewer.UiDispatcher.CheckAccess();
                    isOwnerThread
                        .Should()
                        .BeFalse(
                            "the dedicated worker thread must not be the thread that constructed "
                                + "the viewer, or the boundary assertion would pass vacuously"
                        );
                    scope.Viewer.InitializeBreadcrumbPipeline(provider.Object, operations);
                });

                // Assert
                captured
                    .Should()
                    .NotBeNull(
                        "a worker thread is not the thread that constructed the viewer, so the "
                            + "guard must throw rather than admit the call"
                    );
                captured.Should().BeOfType<InvalidOperationException>();
                captured.Message.Should().Contain("InitializeBreadcrumbPipeline");
                captured.Should().NotBeOfType<ObjectDisposedException>();
            }
        }

        /// <summary>
        /// The same cross-thread contract on the three-argument <c>ConfigureBreadcrumbDropDown</c>
        /// overload, whose guard is its first statement and therefore throws before any argument
        /// check or control access.
        /// </summary>
        /// <remarks>
        /// Issue #900: the worker is a dedicated thread created by <c>RunOnDedicatedWorkerThread</c>
        /// rather than a <c>Task.Run</c> work item, for the reason given on
        /// <c>InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic</c>: a pool work
        /// item can be inlined onto the constructing thread, and a thread this test creates cannot.
        /// The precondition inside the delegate proves the call is off the owning thread before the
        /// guarded member runs. The helper's untimed <c>Thread.Join()</c> waits for one synchronous
        /// call on a non-pool thread and parks no thread-pool slot, so it is safe under the
        /// <c>Workers=0</c> class-level parallel run.
        /// </remarks>
        [TestMethod]
        public void ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic()
        {
            // Arrange
            using (var scope = new ViewerScope())
            {
                var host = new InertDropDownHost();

                // Act
                Exception captured = RunOnDedicatedWorkerThread(() =>
                {
                    bool isOwnerThread = scope.Viewer.UiDispatcher.CheckAccess();
                    isOwnerThread
                        .Should()
                        .BeFalse(
                            "the dedicated worker thread must not be the thread that constructed "
                                + "the viewer, or the boundary assertion would pass vacuously"
                        );
                    scope.Viewer.ConfigureBreadcrumbDropDown(
                        host,
                        () => new Rectangle(0, 0, 10, 10),
                        () => new Rectangle(0, 0, 1920, 1040)
                    );
                });

                // Assert
                captured
                    .Should()
                    .NotBeNull(
                        "a worker thread is not the thread that constructed the viewer, so the "
                            + "guard must throw rather than admit the call"
                    );
                captured.Should().BeOfType<InvalidOperationException>();
                captured.Message.Should().Contain("ConfigureBreadcrumbDropDown");
                captured.Should().NotBeOfType<ObjectDisposedException>();
            }
        }

        /// <summary>
        /// A viewer with no owning dispatcher stays inert, which is what keeps
        /// <c>FormatterServices.GetUninitializedObject</c>-built viewers in other test files from
        /// throwing. This is the only test covering the null-owner escape.
        /// </summary>
        /// <remarks>
        /// Seeding first and repeating the same provider are both required: a worker thread's
        /// ambient context is null, so a first-time initialization would throw at
        /// <c>BreadcrumbUiDispatcher.CaptureCurrent()</c> regardless of the guard, and only the
        /// already-initialized early return can witness the escape. It still discriminates, because
        /// the pre-fix guard reads the non-null captured context and rejects the worker-thread call.
        /// </remarks>
        [TestMethod]
        public void InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow()
        {
            // Arrange
            using (var scope = new ViewerScope())
            {
                BreadcrumbPopupUiOperations operations = InertOperations();
                var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
                scope.Viewer.InitializeBreadcrumbPipeline(provider.Object, operations);
                object before = scope.Viewer.BreadcrumbCoordinator;
                ClearViewerDispatcher(scope.Viewer);

                // Act
                Action act = () =>
                    Task.Run(() =>
                            scope.Viewer.InitializeBreadcrumbPipeline(provider.Object, operations)
                        )
                        .GetAwaiter()
                        .GetResult();

                // Assert
                act.Should()
                    .NotThrow(
                        "a viewer with no owning dispatcher has no boundary to enforce and must "
                            + "stay inert"
                    );
                scope.Viewer.BreadcrumbCoordinator.Should().BeSameAs(before);
            }
        }

        /// <summary>
        /// Builds injected breadcrumb operations over a queue that is never installed as the ambient
        /// context, so posted work stays queued on this thread and nothing escapes the test.
        /// </summary>
        private static BreadcrumbPopupUiOperations InertOperations() =>
            new BreadcrumbPopupUiOperations(
                new BreadcrumbUiDispatcher(new DrainableSynchronizationContext(), _ => { })
            );

        /// <summary>
        /// Assigns <see langword="null"/> to the viewer's private owning-dispatcher field, asserting
        /// the field still exists so a rename fails the test loudly rather than silently.
        /// </summary>
        private static void ClearViewerDispatcher(QuickFiler.ItemViewer viewer)
        {
            FieldInfo field = typeof(QuickFiler.ItemViewer).GetField(
                "_uiDispatcher",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            field
                .Should()
                .NotBeNull("ItemViewer must still declare the private _uiDispatcher field");
            field.SetValue(viewer, null);
        }

        /// <summary>
        /// Runs <paramref name="action"/> on a dedicated background thread, joins it, and returns
        /// the exception it threw, or <see langword="null"/> when it completed normally.
        /// </summary>
        /// <remarks>
        /// Issue #900: a <c>Task.Run</c> work item is not guaranteed to run on a thread other than
        /// the caller's, so it cannot stand in for a different thread in a thread-identity test. A
        /// thread this method constructs is distinct from every live thread by construction. The
        /// untimed <c>Join()</c> is a completion wait on one bounded synchronous call, not a sleep or
        /// a wall-clock wait, and the waiting thread and the waited-for thread are never both
        /// thread-pool workers, so the wait cannot starve the pool under parallel execution.
        /// </remarks>
        private static Exception RunOnDedicatedWorkerThread(Action action)
        {
            Exception captured = null;
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception error)
                {
                    captured = error;
                }
            });
            thread.IsBackground = true;
            thread.Start();
            thread.Join();
            return captured;
        }

        /// <summary>A drop-down host that records nothing and does nothing.</summary>
        private sealed class InertDropDownHost : IBreadcrumbDropDownHost
        {
            public bool IsOpen => false;
            public IWebViewMessenger PopupMessenger => null;

            public event EventHandler PopupMessengerReady
            {
                add { }
                remove { }
            }

            public Task<bool> OpenAsync(Rectangle anchor, Rectangle area, Size desired) =>
                Task.FromResult(false);

            public Task<bool> OpenAsync(
                Rectangle anchor,
                Rectangle area,
                Size desired,
                bool takeFocus
            ) => Task.FromResult(false);

            public bool Close(BreadcrumbDropDownCloseReason reason) => true;

            public void SetTheme(string theme) { }

            public void Reset() { }

            public void Dispose() { }
        }

        /// <summary>
        /// Queues posted work and runs it only on an explicit <see cref="Drain"/> call, on the
        /// creating thread. Never installed as the ambient context: it is handed to a
        /// <c>BreadcrumbUiDispatcher</c> directly, so the viewer's captured context stays the plain
        /// one <see cref="ViewerScope"/> installs.
        /// </summary>
        private sealed class DrainableSynchronizationContext : SynchronizationContext
        {
            private readonly Queue<Tuple<SendOrPostCallback, object>> _callbacks =
                new Queue<Tuple<SendOrPostCallback, object>>();
            private readonly int _creatorThreadId = Environment.CurrentManagedThreadId;

            public override void Post(SendOrPostCallback callback, object state) =>
                _callbacks.Enqueue(Tuple.Create(callback, state));

            /// <summary>Runs every queued callback, including work queued while draining.</summary>
            internal void Drain()
            {
                Environment.CurrentManagedThreadId.Should().Be(_creatorThreadId);
                while (_callbacks.Count > 0)
                {
                    Tuple<SendOrPostCallback, object> callback = _callbacks.Dequeue();
                    callback.Item1(callback.Item2);
                }
            }
        }

        /// <summary>
        /// Installs a plain ambient context, constructs a real <see cref="QuickFiler.ItemViewer"/>
        /// under it on the current thread, and restores the previous context on disposal.
        /// </summary>
        private sealed class ViewerScope : IDisposable
        {
            private readonly SynchronizationContext _previous;

            internal ViewerScope()
            {
                _previous = SynchronizationContext.Current;
                Context = new SynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(Context);
                Viewer = new QuickFiler.ItemViewer();
            }

            internal SynchronizationContext Context { get; }

            internal QuickFiler.ItemViewer Viewer { get; }

            public void Dispose()
            {
                Viewer.Dispose();
                SynchronizationContext.SetSynchronizationContext(_previous);
            }
        }
    }
}
