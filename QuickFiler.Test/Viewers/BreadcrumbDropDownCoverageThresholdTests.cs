using System;
using System.Drawing;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Web.WebView2.Core;
using QuickFiler.Viewers;

namespace QuickFiler.Test.Viewers
{
    /// <summary>Deterministic coverage-threshold contracts for popup lifecycle guards.</summary>
    [TestClass]
    public sealed class BreadcrumbDropDownCoverageThresholdTests
    {
        [TestMethod]
        public async Task OpenAsync_RollbackCallbackFailsOnce_OuterPipelineCompletesRecovery()
        {
            // Arrange
            using (var harness = new ThresholdHarness())
            {
                var initializationFailure = new InvalidOperationException("initialization failed");
                var rollbackFailure = new InvalidOperationException("rollback failed");
                harness.FactoryFailure = initializationFailure;
                harness.CancelAction = () =>
                {
                    if (harness.CancelCount == 1)
                    {
                        throw rollbackFailure;
                    }
                };

                // Act
                bool opened = await harness.OpenAsync();

                // Assert
                opened.Should().BeFalse();
                harness.FactoryCount.Should().Be(1);
                harness.CancelCount.Should().Be(2);
                harness.FocusAnchorCount.Should().Be(1);
                harness.ShowCount.Should().Be(0);
                harness.FocusPendingCount.Should().Be(0);
                harness.Host.LastInitializationException.Should().BeSameAs(rollbackFailure);
                AssertClosedWithoutSurface(harness);
            }
        }

        [TestMethod]
        public async Task OpenAsync_ReadyHandlerResetsLifecycle_RejectsInstalledSurface()
        {
            // Arrange
            using (var harness = new ThresholdHarness())
            {
                harness.ReadyAction = harness.Host.Reset;

                // Act
                bool opened = await harness.OpenAsync();

                // Assert
                opened.Should().BeFalse();
                harness.FactoryCount.Should().Be(1);
                harness.ReadyEventCount.Should().Be(1);
                harness.ShowCount.Should().Be(0);
                harness.FocusPendingCount.Should().Be(0);
                harness.CancelCount.Should().Be(0);
                harness.FocusAnchorCount.Should().Be(0);
                AssertDisposedSurface(harness);
                AssertClosedWithoutSurface(harness);
                harness.Host.LastInitializationException.Should().BeNull();
            }
        }

        [TestMethod]
        public async Task OpenAsync_ShowCallbackResetsLifecycle_StopsBeforeFocus()
        {
            // Arrange
            using (var harness = new ThresholdHarness())
            {
                harness.ShowAction = harness.Host.Reset;

                // Act
                bool opened = await harness.OpenAsync();

                // Assert
                opened.Should().BeFalse();
                harness.ReadyEventCount.Should().Be(1);
                harness.ShowCount.Should().Be(1);
                harness.FocusPendingCount.Should().Be(0);
                harness.CancelCount.Should().Be(1);
                harness.FocusAnchorCount.Should().Be(1);
                AssertDisposedSurface(harness);
                AssertClosedWithoutSurface(harness);
                harness.Host.LastInitializationException.Should().BeNull();
            }
        }

        [TestMethod]
        public async Task OpenAsync_FocusCallbackResetsLifecycle_StopsBeforeSuccess()
        {
            // Arrange
            using (var harness = new ThresholdHarness())
            {
                harness.FocusPendingAction = harness.Host.Reset;

                // Act
                bool opened = await harness.OpenAsync();

                // Assert
                opened.Should().BeFalse();
                harness.ReadyEventCount.Should().Be(1);
                harness.ShowCount.Should().Be(1);
                harness.FocusPendingCount.Should().Be(1);
                harness.CancelCount.Should().Be(1);
                harness.FocusAnchorCount.Should().Be(1);
                AssertDisposedSurface(harness);
                AssertClosedWithoutSurface(harness);
                harness.Host.LastInitializationException.Should().BeNull();
            }
        }

        [TestMethod]
        public async Task OpenAsync_ShowCallbackResetsThenThrows_DoesNotOverwriteCurrentLifecycle()
        {
            // Arrange
            using (var harness = new ThresholdHarness())
            {
                var staleFailure = new InvalidOperationException("stale show failure");
                harness.ShowAction = () =>
                {
                    harness.Host.Reset();
                    throw staleFailure;
                };

                // Act
                bool opened = await harness.OpenAsync();

                // Assert
                opened.Should().BeFalse();
                harness.ReadyEventCount.Should().Be(1);
                harness.ShowCount.Should().Be(1);
                harness.FocusPendingCount.Should().Be(0);
                harness.CancelCount.Should().Be(1);
                harness.FocusAnchorCount.Should().Be(1);
                AssertDisposedSurface(harness);
                AssertClosedWithoutSurface(harness);
                harness.Host.LastInitializationException.Should().BeNull();
            }
        }

        [TestMethod]
        public async Task OpenAsync_ResetWhileReadinessPending_CancellationRejectsSurface()
        {
            // Arrange
            using (var harness = new ThresholdHarness())
            {
                var readiness = new TaskCompletionSource<bool>(
                    TaskCreationOptions.RunContinuationsAsynchronously
                );
                harness.ReadinessTask = readiness.Task;
                Task<bool> opening = harness.OpenAsync();
                opening.IsCompleted.Should().BeFalse();

                // Act
                harness.Host.Reset();
                bool opened = await opening;

                // Assert
                opened.Should().BeFalse();
                readiness.Task.IsCompleted.Should().BeFalse();
                harness.FactoryCount.Should().Be(1);
                harness.ReadyEventCount.Should().Be(0);
                harness.ShowCount.Should().Be(0);
                harness.FocusPendingCount.Should().Be(0);
                harness.CancelCount.Should().Be(0);
                harness.FocusAnchorCount.Should().Be(0);
                AssertDisposedSurface(harness);
                AssertClosedWithoutSurface(harness);
                harness.Host.LastInitializationException.Should().BeNull();
            }
        }

        [TestMethod]
        public async Task OpenAsync_LegacyFactoryReturnsNull_ReportsNoSurfaceAndRollsBack()
        {
            // Arrange
            using (var harness = new ThresholdHarness(useLegacyFactory: true))
            {
                // Act
                bool opened = await harness.OpenAsync();

                // Assert
                opened.Should().BeFalse();
                harness.FactoryCount.Should().Be(1);
                harness.ReadyEventCount.Should().Be(0);
                harness.ShowCount.Should().Be(0);
                harness.FocusPendingCount.Should().Be(0);
                harness.CancelCount.Should().Be(1);
                harness.FocusAnchorCount.Should().Be(1);
                AssertClosedWithoutSurface(harness);
                harness
                    .Host.LastInitializationException.Should()
                    .BeOfType<InvalidOperationException>()
                    .Which.Message.Should()
                    .Be("Popup initialization returned no surface.");
            }
        }

        private static void AssertDisposedSurface(ThresholdHarness harness)
        {
            harness.Surface.DisposeCount.Should().Be(1);
            harness.Messenger.DisposeCount.Should().Be(1);
        }

        private static void AssertClosedWithoutSurface(ThresholdHarness harness)
        {
            harness.Host.IsOpen.Should().BeFalse();
            harness.Host.PopupMessenger.Should().BeNull();
            harness.Host.ControlHost.Should().BeNull();
            harness.Host.DropDown.Items.Count.Should().Be(0);
        }

        private sealed class ThresholdHarness : IDisposable
        {
            private readonly Panel _anchor;
            private readonly SynchronizationContext _previousContext;

            internal ThresholdHarness(bool useLegacyFactory = false)
            {
                _previousContext = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
                _anchor = new Panel();
                var environment = (CoreWebView2Environment)
                    FormatterServices.GetUninitializedObject(typeof(CoreWebView2Environment));
                if (useLegacyFactory)
                {
                    Func<CoreWebView2Environment, Task<Tuple<Control, IWebViewMessenger>>> factory =
                        CreateLegacySurfaceAsync;
                    Host = new BreadcrumbDropDownHost(
                        _anchor,
                        environment,
                        factory,
                        FocusPending,
                        FocusAnchor,
                        CancelSelection,
                        ShowPopup
                    );
                }
                else
                {
                    Func<
                        CoreWebView2Environment,
                        Task<Tuple<Control, IWebViewMessenger, Task>>
                    > factory = CreateReadySurfaceAsync;
                    Host = new BreadcrumbDropDownHost(
                        _anchor,
                        environment,
                        factory,
                        FocusPending,
                        FocusAnchor,
                        CancelSelection,
                        ShowPopup
                    );
                }
                Host.PopupMessengerReady += OnPopupMessengerReady;
            }

            internal BreadcrumbDropDownHost Host { get; }
            internal TrackingControl Surface { get; } = new TrackingControl();
            internal TrackingMessenger Messenger { get; } = new TrackingMessenger();
            internal Task ReadinessTask { get; set; } = Task.CompletedTask;
            internal Exception FactoryFailure { get; set; }
            internal Action ReadyAction { get; set; } = () => { };
            internal Action ShowAction { get; set; } = () => { };
            internal Action FocusPendingAction { get; set; } = () => { };
            internal Action CancelAction { get; set; } = () => { };
            internal int FactoryCount { get; private set; }
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
                Host.PopupMessengerReady -= OnPopupMessengerReady;
                Host.Dispose();
                if (!Surface.IsDisposed)
                {
                    Surface.Dispose();
                }
                Messenger.Dispose();
                _anchor.Dispose();
                SynchronizationContext.SetSynchronizationContext(_previousContext);
            }

            private Task<Tuple<Control, IWebViewMessenger, Task>> CreateReadySurfaceAsync(
                CoreWebView2Environment environment
            )
            {
                FactoryCount++;
                if (FactoryFailure != null)
                {
                    return Task.FromException<Tuple<Control, IWebViewMessenger, Task>>(
                        FactoryFailure
                    );
                }
                return Task.FromResult(
                    Tuple.Create<Control, IWebViewMessenger, Task>(
                        Surface,
                        Messenger,
                        ReadinessTask
                    )
                );
            }

            private Task<Tuple<Control, IWebViewMessenger>> CreateLegacySurfaceAsync(
                CoreWebView2Environment environment
            )
            {
                FactoryCount++;
                return Task.FromResult<Tuple<Control, IWebViewMessenger>>(null);
            }

            private void OnPopupMessengerReady(object sender, EventArgs args)
            {
                ReadyEventCount++;
                ReadyAction();
            }

            private void ShowPopup(ToolStripDropDown dropDown, Control owner, Point location)
            {
                ShowCount++;
                ShowAction();
            }

            private void FocusPending()
            {
                FocusPendingCount++;
                FocusPendingAction();
            }

            private void FocusAnchor() => FocusAnchorCount++;

            private void CancelSelection()
            {
                CancelCount++;
                CancelAction();
            }
        }

        private sealed class TrackingControl : Panel
        {
            internal int DisposeCount { get; private set; }

            protected override void Dispose(bool disposing)
            {
                if (disposing && !IsDisposed)
                {
                    DisposeCount++;
                }
                base.Dispose(disposing);
            }
        }

        private sealed class TrackingMessenger : IWebViewMessenger, IDisposable
        {
            internal int DisposeCount { get; private set; }

            public event EventHandler<string> MessageReceived
            {
                add { }
                remove { }
            }

            public void PostJson(string json) { }

            public void Dispose()
            {
                if (DisposeCount == 0)
                {
                    DisposeCount++;
                }
            }
        }
    }
}
