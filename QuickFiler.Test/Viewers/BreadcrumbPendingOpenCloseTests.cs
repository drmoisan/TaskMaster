using System;
using System.Collections.Generic;
using System.Drawing;
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
    /// <summary>Failure-first close semantics while popup factory/readiness work is pending.</summary>
    [TestClass]
    public sealed class BreadcrumbPendingOpenCloseTests
    {
        [TestMethod]
        public async Task CloseWhileFactoryPending_InvalidatesOpenAndRepeatedCloseIsIdempotent()
        {
            // Arrange
            using (var harness = new PendingHostHarness())
            {
                Task<bool> opening = harness.OpenAsync();
                Task<bool> sharedOpening = harness.OpenAsync();
                PendingAttempt attempt = harness.Attempts[0];

                // Act
                bool firstClose = harness.Host.Close(BreadcrumbDropDownCloseReason.Uncommitted);
                bool repeatedClose = harness.Host.Close(BreadcrumbDropDownCloseReason.Uncommitted);
                opening.IsCompleted.Should().BeTrue("close must not wait for the factory");
                (await opening.ConfigureAwait(false)).Should().BeFalse();
                attempt.CompleteFactory();
                attempt.CompleteReadiness();
                await attempt.Disposed.ConfigureAwait(false);

                // Assert
                sharedOpening.Should().BeSameAs(opening);
                harness
                    .ShowCount.Should()
                    .Be(0, "a factory completion cannot show after pending open was closed");
                harness.FocusPendingCount.Should().Be(0);
                firstClose.Should().BeTrue("pending open work is a closeable selector state");
                repeatedClose.Should().BeFalse();
                harness.CancelCount.Should().Be(1);
                harness.FocusAnchorCount.Should().Be(1);
                harness.Host.IsOpen.Should().BeFalse();
            }
        }

        [TestMethod]
        public async Task CloseWhileReadinessPending_RejectsLateReadyAttachShowAndFocus()
        {
            // Arrange
            using (var harness = new PendingHostHarness())
            {
                Task<bool> opening = harness.OpenAsync();
                PendingAttempt attempt = harness.Attempts[0];
                attempt.CompleteFactory();
                opening.IsCompleted.Should().BeFalse("document readiness remains controlled");

                // Act
                bool closed = harness.Host.Close(BreadcrumbDropDownCloseReason.Uncommitted);
                opening.IsCompleted.Should().BeTrue("close must not wait for readiness");
                (await opening.ConfigureAwait(false)).Should().BeFalse();
                attempt.CompleteReadiness();
                await attempt.Disposed.ConfigureAwait(false);

                // Assert
                harness
                    .ShowCount.Should()
                    .Be(0, "a readiness completion cannot show after pending open was closed");
                harness.FocusPendingCount.Should().Be(0);
                harness.ReadyEventCount.Should().Be(0);
                closed.Should().BeTrue();
                harness.CancelCount.Should().Be(1);
                harness.FocusAnchorCount.Should().Be(1);
                harness.Host.PopupMessenger.Should().BeNull();
            }
        }

        [TestMethod]
        public async Task CloseCanceledFactory_AllowsOneFreshReopenWithoutLateMutation()
        {
            // Arrange
            using (var harness = new PendingHostHarness())
            {
                Task<bool> staleOpening = harness.OpenAsync();
                PendingAttempt stale = harness.Attempts[0];
                bool closed = harness.Host.Close(BreadcrumbDropDownCloseReason.Uncommitted);
                (await staleOpening.ConfigureAwait(false)).Should().BeFalse();
                closed.Should().BeTrue();

                // Act
                Task<bool> freshOpening = harness.OpenAsync();
                PendingAttempt current = harness.Attempts[1];
                current.CompleteFactory();
                current.CompleteReadiness();
                bool opened = await freshOpening.ConfigureAwait(false);
                stale.CompleteFactory();
                stale.CompleteReadiness();
                await stale.Disposed.ConfigureAwait(false);

                // Assert
                opened.Should().BeTrue();
                harness.Attempts.Should().HaveCount(2);
                harness.ReadyEventCount.Should().Be(1);
                harness.ShowCount.Should().Be(1);
                harness.FocusPendingCount.Should().Be(1);
                harness.CancelCount.Should().Be(1);
                harness.FocusAnchorCount.Should().Be(1);
                harness.Host.PopupMessenger.Should().BeSameAs(current.Messenger);
                stale.Surface.DisposeCount.Should().Be(1);
                stale.Messenger.DisposeCount.Should().Be(1);
                current.Surface.DisposeCount.Should().Be(0);
                current.Messenger.DisposeCount.Should().Be(0);
            }
        }

        [TestMethod]
        public void ToggleAndEscapeWhileOpenIsPending_EachClosesHostExactlyOnce()
        {
            // Arrange and act
            int toggleCloseCount = ExercisePendingViewerClose(
                viewer => viewer.SetBreadcrumbDropDownState(false),
                BreadcrumbDropDownCloseReason.Uncommitted
            );
            int escapeCloseCount = ExercisePendingViewerClose(
                viewer =>
                    viewer.BreadcrumbCoordinator.HandleSelectorKey(BreadcrumbSelectorKey.Escape),
                BreadcrumbDropDownCloseReason.ExplicitCommit
            );

            // Assert
            toggleCloseCount.Should().Be(1);
            escapeCloseCount.Should().Be(1);
        }

        [TestMethod]
        public void AutomaticSelectorCloseWhileOpenIsPending_ClosesHostExactlyOnce()
        {
            // Arrange and act
            int closeCount = ExercisePendingViewerClose(
                viewer => viewer.BreadcrumbCoordinator.CancelSelector(),
                BreadcrumbDropDownCloseReason.ExplicitCommit
            );

            // Assert
            closeCount.Should().Be(1);
        }

        private static int ExercisePendingViewerClose(
            Action<QuickFiler.ItemViewer> close,
            BreadcrumbDropDownCloseReason expectedReason
        )
        {
            using (var scope = new ViewerScope())
            {
                var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
                scope.Viewer.InitializeBreadcrumbPipeline(provider.Object);
                scope.Viewer.BreadcrumbCoordinator.AddItems(new[] { "A", "B" });
                var opening = new TaskCompletionSource<bool>();
                int closeCount = 0;
                var host = new Mock<IBreadcrumbDropDownHost>();
                host.SetupGet(value => value.IsOpen).Returns(false);
                host.Setup(value =>
                        value.OpenAsync(
                            It.IsAny<Rectangle>(),
                            It.IsAny<Rectangle>(),
                            It.IsAny<Size>()
                        )
                    )
                    .Returns(opening.Task);
                host.Setup(value => value.Close(expectedReason))
                    .Returns(() =>
                    {
                        closeCount++;
                        opening.TrySetResult(false);
                        return true;
                    });
                scope.Viewer.ConfigureBreadcrumbDropDown(
                    host.Object,
                    () => new Rectangle(0, 0, 300, 25),
                    () => new Rectangle(0, 0, 1920, 1040)
                );
                scope.Viewer.SetBreadcrumbDropDownState(true);

                close(scope.Viewer);
                opening.TrySetResult(false);
                return closeCount;
            }
        }

        private sealed class PendingHostHarness : IDisposable
        {
            private readonly SynchronizationContext _previousContext;
            private readonly Panel _anchor;

            internal PendingHostHarness()
            {
                _previousContext = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(
                    new InlineSynchronizationContext()
                );
                try
                {
                    _anchor = new Panel();
                    var environment = (CoreWebView2Environment)
                        FormatterServices.GetUninitializedObject(typeof(CoreWebView2Environment));
                    Host = new BreadcrumbDropDownHost(
                        _anchor,
                        environment,
                        CreateSurfaceAsync,
                        () => FocusPendingCount++,
                        () => FocusAnchorCount++,
                        () => CancelCount++,
                        (dropDown, owner, point) => ShowCount++
                    );
                    Host.PopupMessengerReady += OnPopupMessengerReady;
                }
                catch
                {
                    SynchronizationContext.SetSynchronizationContext(_previousContext);
                    throw;
                }
            }

            internal BreadcrumbDropDownHost Host { get; }
            internal List<PendingAttempt> Attempts { get; } = new List<PendingAttempt>();
            internal int ReadyEventCount { get; private set; }
            internal int ShowCount { get; private set; }
            internal int FocusPendingCount { get; private set; }
            internal int FocusAnchorCount { get; private set; }
            internal int CancelCount { get; private set; }

            internal Task<bool> OpenAsync() =>
                Host.OpenAsync(
                    new Rectangle(120, 240, 390, 25),
                    new Rectangle(0, 0, 1920, 1040),
                    new Size(390, 180)
                );

            public void Dispose()
            {
                try
                {
                    Host.PopupMessengerReady -= OnPopupMessengerReady;
                    Host.Dispose();
                    foreach (PendingAttempt attempt in Attempts)
                    {
                        attempt.DisposeUnclaimedResources();
                    }
                    _anchor.Dispose();
                }
                finally
                {
                    SynchronizationContext.SetSynchronizationContext(_previousContext);
                }
            }

            private Task<Tuple<Control, IWebViewMessenger, Task>> CreateSurfaceAsync(
                CoreWebView2Environment environment
            )
            {
                var attempt = new PendingAttempt();
                Attempts.Add(attempt);
                return attempt.Factory.Task;
            }

            private void OnPopupMessengerReady(object sender, EventArgs args) => ReadyEventCount++;
        }

        private sealed class PendingAttempt
        {
            internal TaskCompletionSource<
                Tuple<Control, IWebViewMessenger, Task>
            > Factory { get; } =
                new TaskCompletionSource<Tuple<Control, IWebViewMessenger, Task>>();
            internal TaskCompletionSource<bool> Readiness { get; } =
                new TaskCompletionSource<bool>();
            internal TrackingControl Surface { get; } = new TrackingControl();
            internal TrackingMessenger Messenger { get; } = new TrackingMessenger();
            internal Task Disposed => Task.WhenAll(Surface.DisposedTask, Messenger.DisposedTask);

            internal void CompleteFactory() =>
                Factory.SetResult(
                    Tuple.Create<Control, IWebViewMessenger, Task>(
                        Surface,
                        Messenger,
                        Readiness.Task
                    )
                );

            internal void CompleteReadiness() => Readiness.SetResult(true);

            internal void DisposeUnclaimedResources()
            {
                if (!Surface.IsDisposed)
                    Surface.Dispose();
                if (Messenger.DisposeCount == 0)
                    Messenger.Dispose();
            }
        }

        private sealed class TrackingControl : Panel
        {
            private readonly TaskCompletionSource<bool> _disposed =
                new TaskCompletionSource<bool>();

            internal int DisposeCount { get; private set; }
            internal Task DisposedTask => _disposed.Task;

            protected override void Dispose(bool disposing)
            {
                if (disposing && !IsDisposed)
                {
                    DisposeCount++;
                    _disposed.TrySetResult(true);
                }
                base.Dispose(disposing);
            }
        }

        private sealed class TrackingMessenger : IWebViewMessenger, IDisposable
        {
            private EventHandler<string> _messageReceived;
            private readonly TaskCompletionSource<bool> _disposed =
                new TaskCompletionSource<bool>();

            internal int DisposeCount { get; private set; }
            internal Task DisposedTask => _disposed.Task;

            public event EventHandler<string> MessageReceived
            {
                add => _messageReceived += value;
                remove => _messageReceived -= value;
            }

            public void PostJson(string json) { }

            public void Dispose()
            {
                if (DisposeCount > 0)
                    return;
                DisposeCount++;
                _disposed.TrySetResult(true);
            }
        }

        private sealed class ViewerScope : IDisposable
        {
            private readonly SynchronizationContext _previous;

            internal ViewerScope()
            {
                _previous = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(
                    new InlineSynchronizationContext()
                );
                Viewer = new QuickFiler.ItemViewer();
            }

            internal QuickFiler.ItemViewer Viewer { get; }

            public void Dispose()
            {
                Viewer.Dispose();
                SynchronizationContext.SetSynchronizationContext(_previous);
            }
        }

        private sealed class InlineSynchronizationContext : SynchronizationContext
        {
            public override void Post(SendOrPostCallback callback, object state) => callback(state);
        }
    }
}
