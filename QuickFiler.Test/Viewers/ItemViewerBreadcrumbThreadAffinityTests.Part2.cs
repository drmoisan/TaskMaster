using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Test.TestSupport;
using QuickFiler.Viewers;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Test.Viewers
{
    /// <summary>
    /// Continuation partial of <see cref="ItemViewerBreadcrumbThreadAffinityTests"/> holding the
    /// three cross-thread cases, each of which runs its guarded call on a dedicated thread
    /// created by <see cref="DedicatedWorkerThread"/>. The owner-thread admission cases, the
    /// shared <c>InertOperations</c> factory and the nested helper types live in the primary
    /// partial so that each file stays under the 500-line limit (issue #931).
    /// </summary>
    public sealed partial class ItemViewerBreadcrumbThreadAffinityTests
    {
        /// <summary>
        /// A genuine cross-thread call must still fail fast with a diagnostic naming the operation,
        /// and must not be an <see cref="ObjectDisposedException"/>.
        /// </summary>
        /// <remarks>
        /// Issue #900: the worker is a dedicated thread created by <c>DedicatedWorkerThread.Run</c>,
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
                Exception captured = DedicatedWorkerThread.Run(() =>
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
        /// Issue #900: the worker is a dedicated thread created by <c>DedicatedWorkerThread.Run</c>
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
                Exception captured = DedicatedWorkerThread.Run(() =>
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
        /// Issue #931: the guarded call is made from a dedicated thread created by
        /// <c>DedicatedWorkerThread.Run</c>, and the delegate asserts through the owner captured
        /// before the dispatcher is cleared that it is not on the owner thread. The call is
        /// therefore off the owner thread unconditionally, so the test discriminates against the
        /// pre-#781 context-reference guard: that guard would read the non-null captured context,
        /// find the worker's null ambient context different from it, and reject the call, whereas
        /// the null-owner escape admits it. Seeding first and repeating the same provider are
        /// still required: a first-time initialization under a null ambient context would throw
        /// at <c>BreadcrumbUiDispatcher.CaptureCurrent()</c> regardless of the guard, and only the
        /// already-initialized early return can witness the escape.
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
                Dispatcher owner = scope.Viewer.UiDispatcher;
                owner.Should().NotBeNull("the viewer must own a dispatcher before it is cleared");
                ClearViewerDispatcher(scope.Viewer);

                // Act
                Exception captured = DedicatedWorkerThread.Run(() =>
                {
                    bool isOwnerThread = owner.CheckAccess();
                    isOwnerThread
                        .Should()
                        .BeFalse(
                            "the dedicated worker thread must not be the owner thread, or the "
                                + "null-owner escape would be witnessed on the owner thread and "
                                + "the test would pass vacuously"
                        );
                    scope.Viewer.InitializeBreadcrumbPipeline(provider.Object, operations);
                });

                // Assert
                captured
                    .Should()
                    .BeNull(
                        "a viewer with no owning dispatcher has no boundary to enforce and must "
                            + "stay inert"
                    );
                scope.Viewer.BreadcrumbCoordinator.Should().BeSameAs(before);
            }
        }

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
    }
}
