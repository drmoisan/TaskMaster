using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
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
    /// Defines its own viewer scope and reflection accessors, because the equivalents in other test
    /// files are <c>private</c>; no existing file's accessibility is widened. The one reusable harness
    /// is <see cref="BreadcrumbSelectorToggleUiBoundaryTests.InvokeAmbientNull{T}"/>.
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
        /// Three observations are asserted in the order written. The first is the discriminating one:
        /// <c>SetTheme</c> on the captured host reaches its disposal guard and throws once that host is
        /// disposed. The second and third hold in the pre-fix state too and are therefore corroborating
        /// rather than discriminating: <c>Close</c> also returns <see langword="false"/> before the fix,
        /// because with no pending open the cancel attempt returns <see langword="false"/>; and
        /// <c>DropDown.IsDisposed</c> becomes true after the drain either way, since the pre-fix release
        /// path disposes the outgoing host too and only the ordering differs. The queued context is
        /// never installed as ambient, so posted work is drainable rather than inline; under an inline
        /// context the test would pass before the fix. No second thread and no timing construct.
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
                theme
                    .Should()
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
        /// must not be silently discarded. <c>InitializeBreadcrumbPipeline</c> fails fast on a
        /// provider that is not reference-equal to the retained one, matching the comparison
        /// <c>BreadcrumbItemViewerLifecycleCoordinator.SetBridgeCoordinator</c> already performs.
        /// </summary>
        /// <remarks>
        /// The thrown instance is asserted <em>not</em> to be an <see cref="ObjectDisposedException"/>,
        /// which derives from <see cref="InvalidOperationException"/>; without the exclusion a D5
        /// disposal throw would satisfy this D3 assertion.
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
        /// reference returns without effect and keeps the existing breadcrumb coordinator, so the
        /// fail-fast guard does not break an idempotent re-initialization.
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
        /// affinity, rejecting a call where <see cref="SynchronizationContext.Current"/> is not
        /// reference-equal to the context the viewer captured. This case nulls the ambient context.
        /// </summary>
        /// <remarks>
        /// This proxy <strong>proves the guard fires and does not prove the race is absent.</strong> A
        /// true two-thread data race cannot be reproduced deterministically under the repository ban
        /// on sleeps and wall-clock waits: two threads with no barrier give no way to force the
        /// interleaving.
        /// The <em>two-argument</em> overload is used with injected operations: the one-argument
        /// overload evaluates <c>CaptureCurrent()</c> eagerly and that already throws under a null
        /// ambient context, so a test against it would pass before the guard existed. The message must
        /// name the operation, which the dispatcher's own message does not.
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
                    .Where(error => error.Message.Contains("InitializeBreadcrumbPipeline"))
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
        /// This proxy <strong>proves the guard fires and does not prove the race is absent</strong>: a
        /// true two-thread data race cannot be reproduced deterministically under the repository ban
        /// on sleeps and wall-clock waits. The substituted context is installed and restored in a
        /// <c>try</c>/<c>finally</c> on the same thread; no second thread and no timing construct.
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
                    .Where(error => error.Message.Contains("InitializeBreadcrumbPipeline"))
                    .Which.Should()
                    .NotBeOfType<ObjectDisposedException>();
            }
        }

        /// <summary>
        /// Issue #488 defect D5: once teardown has begun, <c>EnsureBreadcrumbResourceOwnership</c>
        /// refuses to create a <c>Container</c> or add a resource owner, so no pipeline is built
        /// against a dead viewer. The call is made on the ambient context the viewer was constructed
        /// under, so the D4 guard passes and the throw is attributable to the disposal guard alone.
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
        /// Issue #475: the `public` seven-parameter <see cref="BreadcrumbDropDownHost"/> constructor
        /// fails fast with no ambient context rather than silently substituting a test dispatcher. A
        /// <strong>non-null</strong> surface factory is supplied so the argument-null guard is not
        /// reached and the operations argument is what throws. The host is disposed inside the
        /// delegate so the pre-fix run leaks nothing.
        /// </summary>
        [TestMethod]
        public void LegacySurfaceFactoryConstructor_AmbientContextNull_ThrowsInvalidOperationException()
        {
            // Arrange
            Func<CoreWebView2Environment, Task<Tuple<Control, IWebViewMessenger>>> surfaceFactory =
                _ => Task.FromResult<Tuple<Control, IWebViewMessenger>>(null);

            // Act
            Action act = () =>
                BreadcrumbSelectorToggleUiBoundaryTests.InvokeAmbientNull(() =>
                {
                    var host = new BreadcrumbDropDownHost(
                        null,
                        null,
                        surfaceFactory,
                        () => { },
                        () => { },
                        () => { },
                        (popup, control, location) => { }
                    );
                    host.Dispose();
                    return true;
                });

            // Assert
            act.Should()
                .Throw<InvalidOperationException>(
                    "the public constructor must refuse to run without an owning boundary"
                );
        }

        /// <summary>
        /// Issue #475 part 3: a viewer whose lifecycle was already seeded with injected operations
        /// must not throw when the 3-arg <c>ConfigureBreadcrumbDropDown</c> runs with no ambient
        /// context. That call discards its operations argument, so evaluating it eagerly would make a
        /// pure no-op throw and would remove the injected seam every such test relies on.
        /// </summary>
        /// <remarks>
        /// The private <c>_context</c> field is nulled reflectively, the only way to obtain a viewer
        /// with a null owning context <strong>that has run its constructor</strong>. The qualifier is
        /// required: unqualified the claim is false, since <c>QfcThemeHelperTests.CreateItemViewer()</c>
        /// also yields a null <c>_context</c> via <c>CreateUninitialized</c> — unusable here, because
        /// this test needs a seeded pipeline that an uninitialized object cannot supply.
        /// </remarks>
        [TestMethod]
        public void ConfigureBreadcrumbDropDown_SeededLifecycleNullUiContext_DoesNotThrow()
        {
            // Arrange
            using (var scope = new ViewerScope())
            {
                var operations = new BreadcrumbPopupUiOperations(
                    new BreadcrumbUiDispatcher(new DrainableSynchronizationContext(), _ => { })
                );
                var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
                scope.Viewer.InitializeBreadcrumbPipeline(provider.Object, operations);
                SetViewerSyncContext(scope.Viewer, null);
                var host = new SeamProbeDropDownHost();

                // Act
                Action act = () =>
                    BreadcrumbSelectorToggleUiBoundaryTests.InvokeAmbientNull(() =>
                    {
                        scope.Viewer.ConfigureBreadcrumbDropDown(
                            host,
                            () => new Rectangle(0, 0, 10, 10),
                            () => new Rectangle(0, 0, 1920, 1040)
                        );
                        return true;
                    });

                // Assert
                act.Should()
                    .NotThrow(
                        "the already-seeded lifecycle discards the operations argument, so it must "
                            + "never be evaluated on a thread without an ambient context"
                    );
            }
        }

        /// <summary>A hand-written drop-down host that records nothing and does nothing.</summary>
        private sealed class SeamProbeDropDownHost : IBreadcrumbDropDownHost
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
        /// A <see cref="CoreWebView2Environment"/> identity token with no WebView2 SDK call. The
        /// environment is only ever compared by reference, so an uninitialized instance suffices.
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
        /// <c>UiSyncContext</c>, so the D4 affinity guard is inert for that viewer.
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
            field.Should().NotBeNull("ItemViewer must still declare the private _context field");
            field.SetValue(viewer, context);
        }

        /// <summary>
        /// Queues posted work and runs it only on an explicit <see cref="Drain"/> call, on the
        /// creating thread, making "posted but not yet run" a deterministic state with no second
        /// thread and no timing construct. Never installed as the ambient context: it is handed to a
        /// <see cref="BreadcrumbUiDispatcher"/> directly, so the viewer's captured context stays the
        /// plain one <see cref="ViewerScope"/> installs.
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
        /// Installs a plain ambient context, constructs a real <see cref="QuickFiler.ItemViewer"/> under
        /// it, and restores the previous context on disposal. The install precedes the construction
        /// because the viewer captures the ambient context and the D4 guard compares against it.
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
