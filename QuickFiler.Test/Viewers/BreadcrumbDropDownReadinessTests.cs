using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
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
    /// <summary>Failure-first popup document-readiness contracts for issue #400.</summary>
    [TestClass]
    public sealed class BreadcrumbDropDownReadinessTests
    {
        [TestMethod]
        public async Task OpenAsync_ReadinessPendingDefersAttachmentReplayShowAndFocusUntilSuccess()
        {
            // Arrange
            ConstructorInfo constructor = RequireReadinessAwareConstructor();
            using (var harness = new ReadinessHarness(constructor))
            {
                harness.CacheSelectorStateAndOpenSession();

                // Act
                Task<bool> opening = harness.OpenAsync();

                // Assert pending readiness
                opening.IsCompleted.Should().BeFalse();
                harness.ReadyEventCount.Should().Be(0);
                harness.AttachmentCount.Should().Be(0);
                harness.PopupMessenger.SubscriberCount.Should().Be(0);
                harness.PopupMessenger.Posted.Should().BeEmpty();
                harness.ShowCount.Should().Be(0);
                harness.FocusPendingCount.Should().Be(0);
                harness.Host.PopupMessenger.Should().BeNull();

                // Act
                harness.Readiness.SetResult(true);
                bool opened = await opening;

                // Assert successful readiness
                opened.Should().BeTrue();
                harness.FactoryCount.Should().Be(1);
                harness.ReadyEventCount.Should().Be(1);
                harness.AttachmentCount.Should().Be(1);
                harness.PopupMessenger.SubscriberCount.Should().Be(1);
                CountType(harness.PopupMessenger.Posted, "render").Should().Be(1);
                CountType(harness.PopupMessenger.Posted, "themeChange").Should().Be(1);
                CountType(harness.PopupMessenger.Posted, "selectorView").Should().Be(1);
                harness.PopupMessenger.Posted.Should().HaveCount(3);
                harness
                    .PopupMessenger.Posted.Single(message =>
                        message.Contains("\"type\":\"selectorView\"")
                    )
                    .Should()
                    .Contain("\"mode\":\"expanded\"");
                harness.ShowCount.Should().Be(1);
                harness.FocusPendingCount.Should().Be(1);
                harness.Host.PopupMessenger.Should().BeSameAs(harness.PopupMessenger);
                harness.Host.IsOpen.Should().BeTrue();
            }
        }

        [TestMethod]
        public async Task OpenAsync_ReadinessFailureRollsBackDisposesPartialSurfaceAndReturnsFocusOnce()
        {
            // Arrange
            ConstructorInfo constructor = RequireReadinessAwareConstructor();
            using (var harness = new ReadinessHarness(constructor))
            {
                harness.CacheSelectorStateAndOpenSession();
                int selectionPublications = 0;
                harness.Coordinator.SelectionChanged += (sender, args) => selectionPublications++;
                var failure = new InvalidOperationException("document readiness failed");

                // Act
                Task<bool> opening = harness.OpenAsync();
                harness.Readiness.SetException(failure);
                bool opened = await opening;

                // Assert
                opened.Should().BeFalse();
                harness.Coordinator.GetSelectedFolder().Should().Be("A");
                harness.Coordinator.CommittedIdentity.Should().Be("plain:0:A");
                harness.Coordinator.PendingIdentity.Should().BeNull();
                harness.Coordinator.IsSelectorOpen.Should().BeFalse();
                harness.CancelCount.Should().Be(1);
                selectionPublications.Should().Be(0);
                harness.Surface.DisposeCount.Should().Be(1);
                harness.PopupMessenger.DisposeCount.Should().Be(1);
                harness.ReadyEventCount.Should().Be(0);
                harness.AttachmentCount.Should().Be(0);
                harness.PopupMessenger.SubscriberCount.Should().Be(0);
                harness.PopupMessenger.Posted.Should().BeEmpty();
                harness.ShowCount.Should().Be(0);
                harness.FocusPendingCount.Should().Be(0);
                harness.FocusAnchorCount.Should().Be(1);
                harness.Host.PopupMessenger.Should().BeNull();
                harness.Host.IsOpen.Should().BeFalse();
                harness.Host.DropDown.Items.Count.Should().Be(0);
                harness.Host.LastInitializationException.Should().BeSameAs(failure);

                harness
                    .Readiness.TrySetException(new Exception("duplicate completion"))
                    .Should()
                    .BeFalse();
                harness.CancelCount.Should().Be(1);
                harness.FocusAnchorCount.Should().Be(1);
                selectionPublications.Should().Be(0);
            }
        }

        private static ConstructorInfo RequireReadinessAwareConstructor()
        {
            Type factoryType = typeof(Func<
                CoreWebView2Environment,
                Task<Tuple<Control, IWebViewMessenger, Task>>
            >);
            ConstructorInfo constructor = typeof(BreadcrumbDropDownHost).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[]
                {
                    typeof(Control),
                    typeof(CoreWebView2Environment),
                    factoryType,
                    typeof(Action),
                    typeof(Action),
                    typeof(Action),
                    typeof(Action<ToolStripDropDown, Control, Point>),
                },
                null
            );
            constructor
                .Should()
                .NotBeNull(
                    "the popup host requires a readiness-aware surface contract before it can "
                        + "defer messenger exposure, cached replay, show, and focus"
                );
            return constructor;
        }

        private static int CountType(IEnumerable<string> messages, string type) =>
            messages.Count(message => message.Contains("\"type\":\"" + type + "\""));

        private sealed class ReadinessHarness : IDisposable
        {
            private readonly BreadcrumbMessengerHub _hub = new BreadcrumbMessengerHub();
            private readonly Panel _anchor = new Panel();

            internal ReadinessHarness(ConstructorInfo constructor)
            {
                var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
                Coordinator = new BreadcrumbBridgeCoordinator(
                    _hub,
                    provider.Object,
                    BreadcrumbUiDispatcher.CreateForCurrentThreadTests()
                );
                var environment = (CoreWebView2Environment)
                    FormatterServices.GetUninitializedObject(typeof(CoreWebView2Environment));
                Func<
                    CoreWebView2Environment,
                    Task<Tuple<Control, IWebViewMessenger, Task>>
                > factory = CreateSurfaceAsync;
                Action<ToolStripDropDown, Control, Point> show = (dropDown, owner, point) =>
                    ShowCount++;
                Host = (BreadcrumbDropDownHost)
                    constructor.Invoke(
                        new object[]
                        {
                            _anchor,
                            environment,
                            factory,
                            new Action(() => FocusPendingCount++),
                            new Action(() => FocusAnchorCount++),
                            new Action(() =>
                            {
                                CancelCount++;
                                Coordinator.CancelSelector();
                            }),
                            show,
                        }
                    );
                Host.PopupMessengerReady += OnPopupMessengerReady;
            }

            internal BreadcrumbDropDownHost Host { get; }
            internal BreadcrumbBridgeCoordinator Coordinator { get; }
            internal TaskCompletionSource<bool> Readiness { get; } =
                new TaskCompletionSource<bool>();
            internal TrackingControl Surface { get; } = new TrackingControl();
            internal TrackingMessenger PopupMessenger { get; } = new TrackingMessenger();
            internal int FactoryCount { get; private set; }
            internal int ReadyEventCount { get; private set; }
            internal int AttachmentCount { get; private set; }
            internal int ShowCount { get; private set; }
            internal int FocusPendingCount { get; private set; }
            internal int FocusAnchorCount { get; private set; }
            internal int CancelCount { get; private set; }

            internal void CacheSelectorStateAndOpenSession()
            {
                Coordinator.AddItems(new[] { "A", "B" });
                Coordinator.SelectRow(0);
                Coordinator.SetTheme("dark");
                Coordinator.OpenSelector().Should().BeTrue();
                Coordinator.HandleSelectorKey(BreadcrumbSelectorKey.Down).Should().BeTrue();
                Coordinator.GetSelectedFolder().Should().Be("A");
                Coordinator.PendingIdentity.Should().Be("plain:1:B");
            }

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
                _anchor.Dispose();
            }

            private Task<Tuple<Control, IWebViewMessenger, Task>> CreateSurfaceAsync(
                CoreWebView2Environment environment
            )
            {
                FactoryCount++;
                return Task.FromResult(
                    Tuple.Create<Control, IWebViewMessenger, Task>(
                        Surface,
                        PopupMessenger,
                        Readiness.Task
                    )
                );
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
