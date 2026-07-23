using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using FluentAssertions.Execution;
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
        public void OpenAsync_RollbackCallbackFailsOnce_OuterPipelineCompletesRecovery()
        {
            // Arrange
            using (new AssertionScope())
            {
                using (var harness = new ThresholdHarness())
                {
                    var initializationFailure = new InvalidOperationException(
                        "initialization failed"
                    );
                    var rollbackFailure = new InvalidOperationException("rollback failed");
                    var focusFailure = new InvalidOperationException("focus failed");
                    var expectedFailures = new[] { rollbackFailure, focusFailure };
                    harness.FactoryFailure = initializationFailure;
                    harness.CancelAction = () => throw rollbackFailure;
                    harness.FocusAnchorAction = () => throw focusFailure;

                    // Act
                    bool opened = harness.Open();

                    // Assert
                    opened.Should().BeFalse();
                    harness.FactoryCount.Should().Be(1);
                    harness.CancelCount.Should().Be(1);
                    harness.FocusAnchorCount.Should().Be(1);
                    harness.ShowCount.Should().Be(0);
                    harness.FocusPendingCount.Should().Be(0);
                    harness.NativeCloseCount.Should().Be(0);
                    harness
                        .Host.LastInitializationException.Should()
                        .BeSameAs(initializationFailure);
                    harness.ErrorSnapshot.Should().BeEquivalentTo(expectedFailures);
                    AssertClosedWithoutSurface(harness);

                    harness.FactoryFailure = null;
                    harness.CancelAction = () => { };
                    harness.FocusAnchorAction = () => { };
                    harness.Open().Should().BeTrue();
                    harness.Host.IsOpen.Should().BeTrue();
                    harness.ErrorSnapshot.Should().BeEquivalentTo(expectedFailures);
                }

                using (var placementHarness = new ThresholdHarness())
                {
                    var rollbackFailure = new InvalidOperationException(
                        "placement rollback failed"
                    );
                    placementHarness.CancelAction = () => throw rollbackFailure;

                    bool opened = placementHarness.Open(Rectangle.Empty);

                    opened.Should().BeFalse();
                    placementHarness.CancelCount.Should().Be(1);
                    placementHarness.FocusAnchorCount.Should().Be(1);
                    InvalidOperationException placementFailure = placementHarness
                        .Host.LastInitializationException.Should()
                        .BeOfType<InvalidOperationException>()
                        .Which;
                    placementFailure
                        .Message.Should()
                        .Be("The active working area has no space for the folder selector popup.");
                    placementHarness
                        .ErrorSnapshot.Should()
                        .HaveCount(2)
                        .And.Contain(placementFailure)
                        .And.Contain(rollbackFailure);
                    placementHarness.Host.IsOpen.Should().BeFalse();
                }
            }
        }

        [TestMethod]
        public void OpenAsync_ReadyHandlerResetsLifecycle_RejectsInstalledSurface()
        {
            // Arrange
            using (var harness = new ThresholdHarness())
            {
                harness.ReadyAction = harness.Host.Reset;

                // Act
                bool opened = harness.Open();

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
        public void OpenAsync_ShowCallbackResetsLifecycle_StopsBeforeFocus()
        {
            // Arrange
            using (var harness = new ThresholdHarness())
            {
                harness.ShowAction = harness.Host.Reset;

                // Act
                bool opened = harness.Open();

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
        public void OpenAsync_FocusCallbackFailsAfterShow_ClosesThenPermitsRetry()
        {
            // Arrange
            using (var harness = new ThresholdHarness())
            {
                var focusFailure = new InvalidOperationException("focus failed");
                harness.FocusPendingAction = () =>
                {
                    if (harness.FocusPendingCount == 1)
                    {
                        throw focusFailure;
                    }
                };

                // Act
                bool opened = harness.Open();
                bool closedAfterFailure = !harness.Host.IsOpen;
                Exception observedFailure = harness.Host.LastInitializationException;
                Exception[] errorSnapshot = harness.ErrorSnapshot;
                int nativeCloseCount = harness.NativeCloseCount;
                int focusAnchorCount = harness.FocusAnchorCount;
                bool retried = harness.Open();

                // Assert
                using (new AssertionScope())
                {
                    opened.Should().BeFalse();
                    closedAfterFailure.Should().BeTrue();
                    observedFailure.Should().BeSameAs(focusFailure);
                    errorSnapshot.Should().ContainSingle().Which.Should().BeSameAs(focusFailure);
                    nativeCloseCount.Should().Be(1);
                    focusAnchorCount.Should().Be(1);
                    harness.CancelCount.Should().Be(1);
                    retried.Should().BeTrue();
                    harness.Host.IsOpen.Should().BeTrue();
                    harness.ShowCount.Should().Be(2);
                    harness.FocusPendingCount.Should().Be(2);
                    harness.NativeCloseCount.Should().Be(1);
                    harness.FocusAnchorCount.Should().Be(1);
                }
            }
        }

        [TestMethod]
        public void OpenAsync_ShowCallbackResetsThenThrows_DoesNotOverwriteCurrentLifecycle()
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
                bool opened = harness.Open();

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
        public void OpenAsync_ResetWhileReadinessPending_CancellationRejectsSurface()
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
                bool opened = harness.Drain(opening);

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

                harness.PrepareFreshSurface();
                harness.Open().Should().BeTrue();
                harness.Host.IsOpen.Should().BeTrue();
            }
        }

        [TestMethod]
        public void OpenAsync_LegacyFactoryReturnsNull_ReportsNoSurfaceAndRollsBack()
        {
            // Arrange
            using (var harness = new ThresholdHarness(useLegacyFactory: true))
            {
                // Act
                bool opened = harness.Open();

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
            private readonly BreadcrumbSelectorToggleUiBoundaryTests.CapturingSynchronizationContext _context;
            private readonly ConcurrentQueue<Exception> _errors = new ConcurrentQueue<Exception>();

            internal ThresholdHarness(bool useLegacyFactory = false)
            {
                _context =
                    new BreadcrumbSelectorToggleUiBoundaryTests.CapturingSynchronizationContext();
                _anchor = new Panel();
                var environment = (CoreWebView2Environment)
                    FormatterServices.GetUninitializedObject(typeof(CoreWebView2Environment));
                Func<
                    CoreWebView2Environment,
                    Task<Tuple<Control, IWebViewMessenger, Task>>
                > factory = CreateReadySurfaceAsync;
                if (useLegacyFactory)
                {
                    Func<
                        CoreWebView2Environment,
                        Task<Tuple<Control, IWebViewMessenger>>
                    > legacyFactory = CreateLegacySurfaceAsync;
                    factory = BreadcrumbPopupUiOperations.NormalizeFactory(legacyFactory);
                }
                var operations = new BreadcrumbPopupUiOperations(
                    new BreadcrumbUiDispatcher(_context, _errors.Enqueue)
                );
                Host = new BreadcrumbDropDownHost(
                    _anchor,
                    environment,
                    factory,
                    FocusPending,
                    FocusAnchor,
                    CancelSelection,
                    ShowPopup,
                    operations,
                    ClosePopup
                );
                Host.PopupMessengerReady += OnPopupMessengerReady;
            }

            internal BreadcrumbDropDownHost Host { get; }
            internal TrackingControl Surface { get; private set; } = new TrackingControl();
            internal TrackingMessenger Messenger { get; private set; } = new TrackingMessenger();
            internal Task ReadinessTask { get; set; } = Task.CompletedTask;
            internal Exception FactoryFailure { get; set; }
            internal Action ReadyAction { get; set; } = () => { };
            internal Action ShowAction { get; set; } = () => { };
            internal Action FocusPendingAction { get; set; } = () => { };
            internal Action FocusAnchorAction { get; set; } = () => { };
            internal Action CancelAction { get; set; } = () => { };
            internal int FactoryCount { get; private set; }
            internal int ReadyEventCount { get; private set; }
            internal int ShowCount { get; private set; }
            internal int FocusPendingCount { get; private set; }
            internal int FocusAnchorCount { get; private set; }
            internal int CancelCount { get; private set; }
            internal int NativeCloseCount { get; private set; }
            internal Exception[] ErrorSnapshot => _errors.ToArray();

            internal bool Open(Rectangle? workingArea = null) => Drain(OpenAsync(workingArea));

            internal Task<bool> OpenAsync(Rectangle? workingArea = null) =>
                Host.OpenAsync(
                    new Rectangle(120, 240, 390, 25),
                    workingArea ?? new Rectangle(0, 0, 1920, 1040),
                    new Size(390, 180)
                );

            internal bool Drain(Task<bool> opening)
            {
                _context.DrainUntil(opening);
                return opening.GetAwaiter().GetResult();
            }

            internal void PrepareFreshSurface()
            {
                Surface = new TrackingControl();
                Messenger = new TrackingMessenger();
                ReadinessTask = Task.CompletedTask;
            }

            public void Dispose()
            {
                Host.PopupMessengerReady -= OnPopupMessengerReady;
                Host.Dispose();
                _context.DrainAll();
                if (!Surface.IsDisposed)
                {
                    Surface.Dispose();
                }
                if (Messenger.DisposeCount == 0)
                    Messenger.Dispose();
                _anchor.Dispose();
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

            private void ClosePopup(
                ToolStripDropDown dropDown,
                ToolStripDropDownCloseReason reason
            ) => NativeCloseCount++;

            private void FocusPending()
            {
                FocusPendingCount++;
                FocusPendingAction();
            }

            private void FocusAnchor()
            {
                FocusAnchorCount++;
                FocusAnchorAction();
            }

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
                if (disposing)
                    DisposeCount++;
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

            public void Dispose() => DisposeCount++;
        }
    }
}
