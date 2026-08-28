using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Web.WebView2.Core;
using Moq;
using QuickFiler.Viewers;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Test.Viewers
{
    /// <summary>
    /// Deterministic regression contracts for the <see cref="QuickFiler.ItemViewer"/> breadcrumb
    /// pipeline defects filed as issue #488 (D1, D3, D4, D5) and issue #475.
    /// </summary>
    /// <remarks>
    /// This file defines its own viewer scope and its own reflection accessors rather than reusing
    /// the equivalents in <c>QfcItemControllerBreadcrumbDropDownTests</c> or
    /// <c>BreadcrumbItemViewerLifecycleCoordinatorTests</c>: those are <c>private</c> nested types
    /// and <c>private static</c> members and are not reachable from another file. No existing test
    /// file's member accessibility is widened by this feature. The one genuinely reusable harness is
    /// <see cref="BreadcrumbSelectorToggleUiBoundaryTests.InvokeAmbientNull{T}"/>, which is
    /// <c>internal static</c> on a <c>public sealed</c> class.
    /// </remarks>
    [TestClass]
    public sealed class ItemViewerBreadcrumbLifecycleRegressionTests
    {
        /// <summary>
        /// Issue #488 defect D1: a WebView2 environment change must dispose the outgoing
        /// <see cref="BreadcrumbDropDownHost"/> between the same-environment early return and the
        /// construction of the replacement, so that the ordering is guaranteed by statement order
        /// rather than by dispatcher behaviour.
        /// </summary>
        /// <remarks>
        /// Three observations are asserted, in the order written. The first is the discriminating
        /// one: <c>SetTheme</c> on the captured host reaches the host's disposal guard and throws
        /// <see cref="ObjectDisposedException"/> once that host is disposed, and succeeds silently
        /// while it is not. The second and third hold in the pre-fix state as well and are therefore
        /// corroborating rather than discriminating. <c>Close</c> returns <see langword="false"/>
        /// before the fix too, because with no pending open the lifetime helper's cancel attempt
        /// also returns <see langword="false"/>. <c>DropDown.IsDisposed</c> becomes
        /// <see langword="true"/> after the queue is drained in both states, because the pre-fix
        /// release path disposes the outgoing host too and only the ordering differs, which that
        /// flag cannot observe.
        /// The queued context is never installed as the ambient context, so posted work is drainable
        /// rather than inline; under an inline context the outgoing host would be disposed anyway
        /// and the test would pass before the fix. No second thread, no sleep, no timer delay, and
        /// no wall-clock wait is used.
        /// </remarks>
        [TestMethod]
        public void ConfigureBreadcrumbDropDown_EnvironmentChange_DisposesOutgoingHostBeforeReplacement()
        {
            // Arrange
            using (var scope = new ViewerScope())
            {
                var queue = new DrainableSynchronizationContext();
                var operations = new BreadcrumbPopupUiOperations(
                    new BreadcrumbUiDispatcher(queue, _ => { })
                );
                var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
                var initializer = new Mock<IWebViewCoreInitializer>(MockBehavior.Strict);
                CoreWebView2Environment firstEnvironment = UninitializedEnvironment();
                CoreWebView2Environment secondEnvironment = UninitializedEnvironment();
                scope.Viewer.InitializeBreadcrumbPipeline(provider.Object, operations);
                scope.Viewer.ConfigureBreadcrumbDropDown(firstEnvironment, initializer.Object);
                queue.Drain();
                BreadcrumbDropDownHost outgoing = Host(scope.Viewer);
                outgoing
                    .Should()
                    .NotBeNull("the first configure call must adopt a concrete drop-down host");

                // Act
                scope.Viewer.ConfigureBreadcrumbDropDown(secondEnvironment, initializer.Object);

                // Assert
                Action theme = () => outgoing.SetTheme("dark");
                theme.Should()
                    .Throw<ObjectDisposedException>(
                        "the outgoing host must already be disposed when the replacement is constructed"
                    );
                outgoing.Close(BreadcrumbDropDownCloseReason.Uncommitted).Should().BeFalse();
                queue.Drain();
                outgoing.DropDown.IsDisposed.Should().BeTrue();
            }
        }

        /// <summary>
        /// Issue #488 defect D3: a second, <em>different</em> <see cref="IFolderHierarchyProvider"/>
        /// must not be silently discarded. Once the pipeline is initialized,
        /// <c>InitializeBreadcrumbPipeline</c> fails fast on a provider that is not reference-equal to
        /// the retained one, matching the reference comparison
        /// <c>BreadcrumbItemViewerLifecycleCoordinator.SetBridgeCoordinator</c> already performs.
        /// </summary>
        /// <remarks>
        /// The thrown instance is additionally asserted <em>not</em> to be an
        /// <see cref="ObjectDisposedException"/>. That type derives from
        /// <see cref="InvalidOperationException"/>, so without the exclusion a D5 disposal throw would
        /// satisfy this D3 assertion and the test would stop discriminating.
        /// </remarks>
        [TestMethod]
        public void InitializeBreadcrumbPipeline_SecondDifferentProvider_ThrowsInvalidOperationException()
        {
            // Arrange
            using (var scope = new ViewerScope())
            {
                var operations = new BreadcrumbPopupUiOperations(
                    new BreadcrumbUiDispatcher(new DrainableSynchronizationContext(), _ => { })
                );
                var first = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
                var second = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
                scope.Viewer.InitializeBreadcrumbPipeline(first.Object, operations);

                // Act
                Action act = () =>
                    scope.Viewer.InitializeBreadcrumbPipeline(second.Object, operations);

                // Assert
                act.Should()
                    .Throw<InvalidOperationException>(
                        "a second, different provider must be refused rather than silently discarded"
                    )
                    .Which.Should()
                    .NotBeOfType<ObjectDisposedException>();
            }
        }

        /// <summary>
        /// Issue #488 defect D3, positive case: repeating the call with the <em>same</em> provider
        /// reference must return without effect, leaving the existing breadcrumb coordinator in place.
        /// This is what keeps the fail-fast guard from breaking an idempotent re-initialization.
        /// </summary>
        [TestMethod]
        public void InitializeBreadcrumbPipeline_RepeatSameProvider_DoesNotThrowAndKeepsCoordinator()
        {
            // Arrange
            using (var scope = new ViewerScope())
            {
                var operations = new BreadcrumbPopupUiOperations(
                    new BreadcrumbUiDispatcher(new DrainableSynchronizationContext(), _ => { })
                );
                var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
                scope.Viewer.InitializeBreadcrumbPipeline(provider.Object, operations);
                object before = scope.Viewer.BreadcrumbCoordinator;
                before.Should().NotBeNull("the first call must build a breadcrumb coordinator");

                // Act
                Action act = () =>
                    scope.Viewer.InitializeBreadcrumbPipeline(provider.Object, operations);

                // Assert
                act.Should().NotThrow();
                scope.Viewer.BreadcrumbCoordinator.Should().BeSameAs(before);
            }
        }

        /// <summary>
        /// Issue #488 defect D4: <c>InitializeBreadcrumbPipeline</c> declares and enforces UI-thread
        /// affinity, rejecting a call made where <see cref="SynchronizationContext.Current"/> is not
        /// reference-equal to the context the viewer captured in its constructor. This case nulls the
        /// ambient context.
        /// </summary>
        /// <remarks>
        /// This proxy <strong>proves the guard fires and does not prove the race is absent.</strong> A
        /// true two-thread data race cannot be reproduced deterministically under the repository ban
        /// on sleeps and wall-clock waits: two threads with no barrier give no way to force the
        /// interleaving. What is asserted is the declared contract, on a single thread, with no timing
        /// construct.
        /// The call uses the <em>two-argument</em> overload with injected operations. The one-argument
        /// overload evaluates <c>BreadcrumbPopupUiOperations.CaptureCurrent()</c> as an eager argument,
        /// and that method already throws <see cref="InvalidOperationException"/> under a null ambient
        /// context, so a test written against it would pass before the guard existed, for the wrong
        /// reason. The assertion additionally requires the message to name the operation, which the
        /// dispatcher's own message does not, and excludes
        /// <see cref="ObjectDisposedException"/> so a D5 throw cannot satisfy it.
        /// </remarks>
        [TestMethod]
        public void InitializeBreadcrumbPipeline_AmbientContextNull_ThrowsBoundaryDiagnostic()
        {
            // Arrange
            using (var scope = new ViewerScope())
            {
                var operations = new BreadcrumbPopupUiOperations(
                    new BreadcrumbUiDispatcher(new DrainableSynchronizationContext(), _ => { })
                );
                var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);

                // Act
                Action act = () =>
                    BreadcrumbSelectorToggleUiBoundaryTests.InvokeAmbientNull(() =>
                    {
                        scope.Viewer.InitializeBreadcrumbPipeline(provider.Object, operations);
                        return true;
                    });

                // Assert
                act.Should()
                    .Throw<InvalidOperationException>(
                        "the guard must reject a call made off the viewer's owning boundary"
                    )
                    .Where(
                        error => error.Message.Contains("InitializeBreadcrumbPipeline"),
                        "the diagnostic must name the operation that was refused"
                    )
                    .Which.Should()
                    .NotBeOfType<ObjectDisposedException>();
            }
        }

        /// <summary>
        /// Issue #488 defect D4, second case: a <em>different non-null</em> ambient context is also
        /// rejected, which proves the comparison is reference equality against the viewer's captured
        /// context rather than a bare null check.
        /// </summary>
        /// <remarks>
        /// This proxy <strong>proves the guard fires and does not prove the race is absent</strong>, for
        /// the same reason recorded on the ambient-null case: a true two-thread data race cannot be
        /// reproduced deterministically under the repository ban on sleeps and wall-clock waits. The
        /// substituted context is installed and restored in a <c>try</c>/<c>finally</c> on the same
        /// thread; no second thread and no timing construct is used.
        /// </remarks>
        [TestMethod]
        public void InitializeBreadcrumbPipeline_DifferentNonNullContext_ThrowsBoundaryDiagnostic()
        {
            // Arrange
            using (var scope = new ViewerScope())
            {
                var operations = new BreadcrumbPopupUiOperations(
                    new BreadcrumbUiDispatcher(new DrainableSynchronizationContext(), _ => { })
                );
                var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
                var foreign = new SynchronizationContext();

                // Act
                Action act = () =>
                {
                    SynchronizationContext previous = SynchronizationContext.Current;
                    try
                    {
                        SynchronizationContext.SetSynchronizationContext(foreign);
                        scope.Viewer.InitializeBreadcrumbPipeline(provider.Object, operations);
                    }
                    finally
                    {
                        SynchronizationContext.SetSynchronizationContext(previous);
                    }
                };

                // Assert
                act.Should()
                    .Throw<InvalidOperationException>(
                        "a different non-null context is off the viewer's owning boundary too"
                    )
                    .Where(
                        error => error.Message.Contains("InitializeBreadcrumbPipeline"),
                        "the diagnostic must name the operation that was refused"
                    )
                    .Which.Should()
                    .NotBeOfType<ObjectDisposedException>();
            }
        }

        /// <summary>
        /// Issue #488 defect D5: once teardown has begun, <c>EnsureBreadcrumbResourceOwnership</c>
        /// must refuse to create a <c>Container</c> or add a breadcrumb resource owner, so no
        /// pipeline is built against a dead viewer. The disposed viewer is called on the same ambient
        /// context it was constructed under, so the D4 affinity guard passes and the
        /// <see cref="ObjectDisposedException"/> is attributable to the disposal guard alone.
        /// </summary>
        [TestMethod]
        public void InitializeBreadcrumbPipeline_AfterViewerDisposed_ThrowsObjectDisposedException()
        {
            // Arrange
            using (var scope = new ViewerScope())
            {
                var operations = new BreadcrumbPopupUiOperations(
                    new BreadcrumbUiDispatcher(new DrainableSynchronizationContext(), _ => { })
                );
                var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
                scope.Viewer.Dispose();

                // Act
                Action act = () =>
                    scope.Viewer.InitializeBreadcrumbPipeline(provider.Object, operations);

                // Assert
                act.Should()
                    .Throw<ObjectDisposedException>(
                        "no breadcrumb resource may be created after teardown has begun"
                    );
                scope.Viewer.BreadcrumbCoordinator.Should().BeNull();
            }
        }

        /// <summary>
        /// Produces a <see cref="CoreWebView2Environment"/> identity token without any WebView2 SDK
        /// call. The environment is only ever compared by reference, so an uninitialized instance is
        /// sufficient and keeps the test free of an external dependency.
        /// </summary>
        private static CoreWebView2Environment UninitializedEnvironment() =>
            (CoreWebView2Environment)
                FormatterServices.GetUninitializedObject(typeof(CoreWebView2Environment));

        /// <summary>Reads the viewer's non-public breadcrumb drop-down host as its concrete type.</summary>
        private static BreadcrumbDropDownHost Host(QuickFiler.ItemViewer viewer) =>
            viewer.BreadcrumbDropDownHost as BreadcrumbDropDownHost;

        /// <summary>
        /// Assigns the viewer's private owning-context field. Used only by the #475 seam-preservation
        /// test, to obtain a viewer that has run its constructor yet reports a null
        /// <c>UiSyncContext</c>, so the UI-affinity guard added for D4 is inert for that viewer.
        /// </summary>
        private static void SetViewerSyncContext(
            QuickFiler.ItemViewer viewer,
            SynchronizationContext context
        )
        {
            FieldInfo field = typeof(QuickFiler.ItemViewer).GetField(
                "_context",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            if (field == null)
            {
                throw new InvalidOperationException(
                    "QuickFiler.ItemViewer no longer declares the private _context field."
                );
            }

            field.SetValue(viewer, context);
        }

        /// <summary>
        /// Installs a plain ambient synchronization context, constructs a real
        /// <see cref="QuickFiler.ItemViewer"/> under it, and restores the previous ambient context on
        /// disposal. The context is installed <em>before</em> the viewer is constructed, because the
        /// viewer captures <see cref="SynchronizationContext.Current"/> in its constructor and the
        /// D4 affinity guard compares against that captured value.
        /// </summary>
        /// <summary>
        /// A synchronization context that only queues posted work and runs it on an explicit
        /// <see cref="Drain"/> call, on the same thread that created the context. It makes
        /// "posted but not yet run" a first-class deterministic state, with no second thread and no
        /// timing construct. It is deliberately never installed as the ambient context: it is handed
        /// to a <see cref="BreadcrumbUiDispatcher"/> directly, so the viewer's own captured context
        /// stays the plain one installed by <see cref="ViewerScope"/>.
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
                Environment
                    .CurrentManagedThreadId.Should()
                    .Be(_creatorThreadId, "the drain must stay on the creating thread");
                while (_callbacks.Count > 0)
                {
                    Tuple<SendOrPostCallback, object> callback = _callbacks.Dequeue();
                    callback.Item1(callback.Item2);
                }
            }
        }

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
