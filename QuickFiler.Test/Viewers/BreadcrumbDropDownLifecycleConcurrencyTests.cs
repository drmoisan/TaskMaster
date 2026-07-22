using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Web.WebView2.Core;
using QuickFiler.Viewers;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Test.Viewers
{
    /// <summary>Failure-first asynchronous popup lifecycle contracts for issue #400.</summary>
    [TestClass]
    public sealed class BreadcrumbDropDownLifecycleConcurrencyTests
    {
        [TestMethod]
        public async Task ConcurrentOpenAsync_PendingInitializationIsSharedAndOpensOnePopup()
        {
            // Arrange
            using (var harness = new LifecycleHarness())
            {
                // Act
                Task<bool> firstOpening = harness.OpenAsync();
                Task<bool> secondOpening = harness.OpenAsync();
                SurfaceAttempt[] startedAttempts = harness.Attempts.ToArray();
                foreach (SurfaceAttempt attempt in startedAttempts)
                {
                    attempt.CompleteSuccess();
                }
                bool[] results = await Task.WhenAll(firstOpening, secondOpening);

                // Assert
                results.Should().Equal(true, true);
                harness.FactoryCount.Should().Be(1);
                harness.Attempts.Should().ContainSingle();
                harness.Host.DropDown.Items.OfType<ToolStripControlHost>().Should().ContainSingle();
                harness.ReadyEventCount.Should().Be(1);
                harness.AttachmentCount.Should().Be(1);
                harness.ShowCount.Should().Be(1);
                harness.FocusPendingCount.Should().Be(1);
                harness.FocusAnchorCount.Should().Be(0);
                harness.CancelCount.Should().Be(0);
                harness.Host.IsOpen.Should().BeTrue();
                harness.Host.PopupMessenger.Should().BeSameAs(harness.Attempts.Single().Messenger);
            }
        }

        [TestMethod]
        public async Task Reset_DuringPendingInitializationDisposesLateSuccessAndAllowsFreshOpen()
        {
            // Arrange
            using (var harness = new LifecycleHarness())
            {
                Task<bool> staleOpening = harness.OpenAsync();
                SurfaceAttempt staleAttempt = harness.Attempts.Single();

                // Act
                harness.Host.Reset();
                staleAttempt.CompleteSuccess();
                bool staleOpened = await staleOpening;

                // Assert invalidated lifecycle
                staleOpened.Should().BeFalse();
                staleAttempt.Surface.DisposeCount.Should().Be(1);
                staleAttempt.Messenger.DisposeCount.Should().Be(1);
                staleAttempt.Messenger.Posted.Should().BeEmpty();
                harness.ReadyEventCount.Should().Be(0);
                harness.AttachmentCount.Should().Be(0);
                harness.ShowCount.Should().Be(0);
                harness.FocusPendingCount.Should().Be(0);
                harness.FocusAnchorCount.Should().Be(0);
                harness.CancelCount.Should().Be(0);
                harness.Host.IsOpen.Should().BeFalse();
                harness.Host.PopupMessenger.Should().BeNull();
                harness.Host.LastInitializationException.Should().BeNull();

                // Act
                Task<bool> freshOpening = harness.OpenAsync();
                harness.FactoryCount.Should().Be(2);
                SurfaceAttempt freshAttempt = harness.Attempts[1];
                freshAttempt.CompleteSuccess();
                bool freshOpened = await freshOpening;

                // Assert fresh lifecycle
                freshOpened.Should().BeTrue();
                harness.FactoryCount.Should().Be(2);
                harness.Host.DropDown.Items.OfType<ToolStripControlHost>().Should().ContainSingle();
                harness.ReadyEventCount.Should().Be(1);
                harness.AttachmentCount.Should().Be(1);
                harness.ShowCount.Should().Be(1);
                harness.FocusPendingCount.Should().Be(1);
                harness.Host.IsOpen.Should().BeTrue();
                harness.Host.PopupMessenger.Should().BeSameAs(freshAttempt.Messenger);
            }
        }

        [TestMethod]
        public async Task Dispose_DuringPendingInitializationDisposesLateSuccessWithoutMutation()
        {
            // Arrange
            using (var harness = new LifecycleHarness())
            {
                Task<bool> opening = harness.OpenAsync();
                SurfaceAttempt attempt = harness.Attempts.Single();

                // Act
                harness.Host.Dispose();
                attempt.CompleteSuccess();
                bool opened = await opening;

                // Assert
                opened.Should().BeFalse();
                attempt.Surface.DisposeCount.Should().Be(1);
                attempt.Messenger.DisposeCount.Should().Be(1);
                attempt.Messenger.Posted.Should().BeEmpty();
                harness.ReadyEventCount.Should().Be(0);
                harness.AttachmentCount.Should().Be(0);
                harness.ShowCount.Should().Be(0);
                harness.FocusPendingCount.Should().Be(0);
                harness.FocusAnchorCount.Should().Be(0);
                harness.CancelCount.Should().Be(0);
                harness.Host.IsOpen.Should().BeFalse();
                harness.Host.PopupMessenger.Should().BeNull();
                harness.Host.LastInitializationException.Should().BeNull();
            }
        }

        [TestMethod]
        public async Task Dispose_DuringPendingInitializationIgnoresLateFailureWithoutMutation()
        {
            // Arrange
            using (var harness = new LifecycleHarness())
            {
                Task<bool> opening = harness.OpenAsync();
                SurfaceAttempt attempt = harness.Attempts.Single();
                var failure = new InvalidOperationException("late disposed failure");

                // Act
                harness.Host.Dispose();
                attempt.CompleteFailure(failure);
                bool opened = await opening;

                // Assert
                opened.Should().BeFalse();
                harness.ReadyEventCount.Should().Be(0);
                harness.AttachmentCount.Should().Be(0);
                harness.ShowCount.Should().Be(0);
                harness.FocusPendingCount.Should().Be(0);
                harness.FocusAnchorCount.Should().Be(0);
                harness.CancelCount.Should().Be(0);
                harness.Host.IsOpen.Should().BeFalse();
                harness.Host.PopupMessenger.Should().BeNull();
                harness.Host.LastInitializationException.Should().BeNull();
            }
        }

        [TestMethod]
        public async Task ResetLifecycle_LateFailureCannotOverwriteLaterSuccessfulLifecycle()
        {
            // Arrange
            using (var harness = new LifecycleHarness())
            {
                Task<bool> staleOpening = harness.OpenAsync();
                SurfaceAttempt staleAttempt = harness.Attempts.Single();
                harness.Host.Reset();
                Task<bool> freshOpening = harness.OpenAsync();
                SurfaceAttempt freshAttempt = harness.Attempts[1];
                freshAttempt.CompleteSuccess();
                (await freshOpening).Should().BeTrue();
                var staleFailure = new InvalidOperationException("stale reset failure");

                // Act
                staleAttempt.CompleteFailure(staleFailure);
                bool staleOpened = await staleOpening;

                // Assert
                staleOpened.Should().BeFalse();
                harness.Host.LastInitializationException.Should().BeNull();
                harness.Host.IsOpen.Should().BeTrue();
                harness.Host.PopupMessenger.Should().BeSameAs(freshAttempt.Messenger);
                freshAttempt.Surface.DisposeCount.Should().Be(0);
                freshAttempt.Messenger.DisposeCount.Should().Be(0);
                harness.ReadyEventCount.Should().Be(1);
                harness.AttachmentCount.Should().Be(1);
                harness.ShowCount.Should().Be(1);
                harness.FocusPendingCount.Should().Be(1);
                harness.FocusAnchorCount.Should().Be(0);
                harness.CancelCount.Should().Be(0);
            }
        }

        [TestMethod]
        public async Task CurrentLifecycle_FactoryFailureRemainsObservableAndRestoresOnce()
        {
            // Arrange
            using (var harness = new LifecycleHarness())
            {
                Task<bool> opening = harness.OpenAsync();
                SurfaceAttempt attempt = harness.Attempts.Single();
                var failure = new InvalidOperationException("current lifecycle failure");

                // Act
                attempt.CompleteFailure(failure);
                bool opened = await opening;

                // Assert
                opened.Should().BeFalse();
                harness.Host.LastInitializationException.Should().BeSameAs(failure);
                harness.Host.IsOpen.Should().BeFalse();
                harness.Host.PopupMessenger.Should().BeNull();
                harness.ReadyEventCount.Should().Be(0);
                harness.AttachmentCount.Should().Be(0);
                harness.ShowCount.Should().Be(0);
                harness.FocusPendingCount.Should().Be(0);
                harness.FocusAnchorCount.Should().Be(1);
                harness.CancelCount.Should().Be(1);
            }
        }

        private sealed class LifecycleHarness : IDisposable
        {
            private readonly BreadcrumbMessengerHub _hub = new BreadcrumbMessengerHub();
            private readonly Panel _anchor = new Panel();

            internal LifecycleHarness()
            {
                _hub.PostJson("{\"type\":\"render\"}");
                _hub.PostJson("{\"type\":\"themeChange\",\"theme\":\"dark\"}");
                _hub.PostJson(
                    BreadcrumbSelectorMessageSerializer.Serialize(
                        new BreadcrumbSelectorViewMessage(
                            BreadcrumbSelectorViewMode.Collapsed,
                            true,
                            "A",
                            "B"
                        )
                    )
                );
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

            internal BreadcrumbDropDownHost Host { get; }
            internal List<SurfaceAttempt> Attempts { get; } = new List<SurfaceAttempt>();
            internal int FactoryCount => Attempts.Count;
            internal int ReadyEventCount { get; private set; }
            internal int AttachmentCount { get; private set; }
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
                _hub.Dispose();
                foreach (SurfaceAttempt attempt in Attempts)
                {
                    attempt.DisposeUnclaimedResources();
                }
                _anchor.Dispose();
            }

            private Task<Tuple<Control, IWebViewMessenger>> CreateSurfaceAsync(
                CoreWebView2Environment environment
            )
            {
                var attempt = new SurfaceAttempt();
                Attempts.Add(attempt);
                return attempt.Completion.Task;
            }

            private void OnPopupMessengerReady(object sender, EventArgs args)
            {
                ReadyEventCount++;
                IWebViewMessenger messenger = Host.PopupMessenger;
                if (
                    messenger != null
                    && _hub.Attach(messenger, BreadcrumbSelectorViewMode.Expanded)
                )
                {
                    AttachmentCount++;
                }
            }
        }

        private sealed class SurfaceAttempt
        {
            internal TaskCompletionSource<Tuple<Control, IWebViewMessenger>> Completion { get; } =
                new TaskCompletionSource<Tuple<Control, IWebViewMessenger>>();
            internal TrackingControl Surface { get; } = new TrackingControl();
            internal TrackingMessenger Messenger { get; } = new TrackingMessenger();

            internal void CompleteSuccess() =>
                Completion.SetResult(Tuple.Create<Control, IWebViewMessenger>(Surface, Messenger));

            internal void CompleteFailure(Exception exception) =>
                Completion.SetException(exception);

            internal void DisposeUnclaimedResources()
            {
                if (!Surface.IsDisposed)
                {
                    Surface.Dispose();
                }
                Messenger.Dispose();
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
            private EventHandler<string> _messageReceived;

            internal int SubscriberCount { get; private set; }
            internal int DisposeCount { get; private set; }
            internal List<string> Posted { get; } = new List<string>();

            public event EventHandler<string> MessageReceived
            {
                add
                {
                    _messageReceived += value;
                    SubscriberCount++;
                }
                remove
                {
                    _messageReceived -= value;
                    SubscriberCount--;
                }
            }

            public void PostJson(string json) => Posted.Add(json);

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
