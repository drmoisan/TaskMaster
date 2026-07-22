using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
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
    /// <summary>Failure-first two-surface ItemViewer integration contracts for issue #400.</summary>
    [TestClass]
    public sealed class BreadcrumbDropDownIntegrationTests
    {
        [TestMethod]
        public void Constructor_NullLegacySurfaceFactory_ThrowsForSurfaceFactory()
        {
            Action act = () =>
                new BreadcrumbDropDownHost(
                    null,
                    null,
                    (Func<CoreWebView2Environment, Task<Tuple<Control, IWebViewMessenger>>>)null,
                    () => { },
                    () => { },
                    () => { },
                    (popup, control, location) => { }
                );

            act.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("surfaceFactory");
        }

        [TestMethod]
        public void SetFolderDroppedDownTrue_OpensOnceWithScreenBoundsAndWorkingArea()
        {
            // Arrange
            using (var harness = new ItemViewerDropDownHarness())
            {
                // Act
                harness.Viewer.SetFolderDroppedDown(true);

                // Assert
                harness.Host.Verify(
                    host =>
                        host.OpenAsync(
                            harness.AnchorScreenBounds,
                            harness.WorkingArea,
                            It.Is<Size>(size => size.Width >= harness.AnchorScreenBounds.Width)
                        ),
                    Times.Once()
                );
                harness.Viewer.BreadcrumbCoordinator.IsSelectorOpen.Should().BeTrue();
            }
        }

        [TestMethod]
        public void ClosedSurfaceToggleMessage_OpensHostExactlyOnce()
        {
            // Arrange
            using (var harness = new ItemViewerDropDownHarness())
            {
                harness.AttachClosedSurface();

                // Act
                harness.ClosedMessenger.Receive("{\"type\":\"selectorToggle\"}");

                // Assert
                harness.Host.Verify(
                    host =>
                        host.OpenAsync(
                            harness.AnchorScreenBounds,
                            harness.WorkingArea,
                            It.IsAny<Size>()
                        ),
                    Times.Once()
                );
            }
        }

        [TestMethod]
        public void SetFolderDroppedDownFalse_RequestsOneUncommittedCloseAndRollback()
        {
            // Arrange
            using (var harness = new ItemViewerDropDownHarness())
            {
                harness.Viewer.SetFolderDroppedDown(true);

                // Act
                harness.Viewer.SetFolderDroppedDown(false);

                // Assert
                harness.Host.Verify(
                    host => host.Close(BreadcrumbDropDownCloseReason.Uncommitted),
                    Times.Once()
                );
                harness.Viewer.BreadcrumbCoordinator.IsSelectorOpen.Should().BeFalse();
                harness.Viewer.GetSelectedFolder().Should().Be("A");
            }
        }

        [TestMethod]
        public void NativeAutomaticClose_RestoresOriginalCommittedIdentityWithoutPendingPublicationAndReturnsFocusOnce()
        {
            // Arrange
            using (var harness = new ItemViewerDropDownHarness())
            {
                int selections = 0;
                harness.Viewer.BreadcrumbCoordinator.SelectionChanged += (sender, args) =>
                    selections++;
                harness.Viewer.SetFolderDroppedDown(true);
                harness
                    .Viewer.BreadcrumbCoordinator.HandleSelectorKey(BreadcrumbSelectorKey.Down)
                    .Should()
                    .BeTrue();
                harness.Viewer.BreadcrumbCoordinator.CommittedIdentity.Should().Be("plain:0:A");
                harness.Viewer.BreadcrumbCoordinator.PendingIdentity.Should().Be("plain:1:B");

                // Act
                harness.RaiseNativeClose();

                // Assert
                harness.Viewer.BreadcrumbCoordinator.IsSelectorOpen.Should().BeFalse();
                harness.Viewer.BreadcrumbCoordinator.CommittedIdentity.Should().Be("plain:0:A");
                harness.Viewer.BreadcrumbCoordinator.PendingIdentity.Should().BeNull();
                harness.Viewer.GetSelectedFolder().Should().Be("A");
                selections.Should().Be(0);
                harness.FocusReturnCount.Should().Be(1);
            }
        }

        [TestMethod]
        public void ClosedAndPopupAttachmentAndTheme_AreExactlyOncePerSurface()
        {
            // Arrange
            using (var harness = new ItemViewerDropDownHarness())
            {
                harness.AttachClosedSurface();
                harness.AttachClosedSurface();
                harness.RaisePopupReady();
                harness.RaisePopupReady();

                // Act
                harness.SetTheme("dark");
                harness.SetTheme("light");

                // Assert
                harness.ClosedMessenger.SubscriberCount.Should().Be(1);
                harness.PopupMessenger.SubscriberCount.Should().Be(1);
                CountType(harness.ClosedMessenger.Posted, "themeChange").Should().Be(2);
                CountType(harness.PopupMessenger.Posted, "themeChange").Should().Be(2);
                harness.Host.Verify(host => host.SetTheme("dark"), Times.Once());
                harness.Host.Verify(host => host.SetTheme("light"), Times.Once());
            }
        }

        [TestMethod]
        public async Task ClosedSurfaceReadyBoundary_DefersPopupReplayAndReopenDoesNotDuplicateSubscriptions()
        {
            // Arrange
            using (var harness = new ItemViewerDropDownHarness())
            {
                harness.Viewer.SetFolderItems(new[] { "A", "B", "C" });
                harness.Viewer.SetFolderSelectedIndex(1);
                harness.SetTheme("dark");
                harness.AttachClosedSurface();

                // Assert pending readiness
                harness.ClosedMessenger.SubscriberCount.Should().Be(1);
                harness.PopupMessenger.SubscriberCount.Should().Be(0);
                harness.PopupMessenger.Posted.Should().BeEmpty();

                // Act
                harness.RaisePopupReady();
                IWebViewMessenger readyMessenger = harness.Host.Object.PopupMessenger;

                // Assert readiness replay
                readyMessenger.Should().BeSameAs(harness.PopupMessenger);
                harness.PopupMessenger.SubscriberCount.Should().Be(1);
                CountType(harness.PopupMessenger.Posted, "render").Should().Be(1);
                CountType(harness.PopupMessenger.Posted, "themeChange").Should().Be(1);
                CountType(harness.PopupMessenger.Posted, "selectorView").Should().Be(1);
                harness.PopupMessenger.Posted.Should().HaveCount(3);

                // Act
                bool firstOpened = await harness.OpenDropDownAsync();
                bool closed = harness.Host.Object.Close(
                    BreadcrumbDropDownCloseReason.ExplicitCommit
                );
                bool reopened = await harness.OpenDropDownAsync();

                // Assert close and reopen reuse
                firstOpened.Should().BeTrue();
                closed.Should().BeTrue();
                reopened.Should().BeTrue();
                harness.Host.Object.PopupMessenger.Should().BeSameAs(readyMessenger);
                harness.PopupMessenger.SubscriberCount.Should().Be(1);
                CountType(harness.PopupMessenger.Posted, "render").Should().Be(1);
                CountType(harness.PopupMessenger.Posted, "themeChange").Should().Be(1);
                CountType(harness.PopupMessenger.Posted, "selectorView").Should().Be(1);
                harness.PopupMessenger.Posted.Should().HaveCount(3);
                harness.Host.Verify(
                    host =>
                        host.OpenAsync(
                            harness.AnchorScreenBounds,
                            harness.WorkingArea,
                            It.IsAny<Size>()
                        ),
                    Times.Exactly(2)
                );

                // Act and assert repeated readiness is idempotent
                harness.RaisePopupReady();
                harness.PopupMessenger.SubscriberCount.Should().Be(1);
                harness.PopupMessenger.Posted.Should().HaveCount(3);
            }
        }

        [TestMethod]
        public void ResetAndPooledReuse_DetachPopupAndDoNotDuplicateCallbacks()
        {
            // Arrange
            using (var harness = new ItemViewerDropDownHarness())
            {
                harness.AttachClosedSurface();
                harness.RaisePopupReady();
                harness.Viewer.SetFolderDroppedDown(true);

                // Act
                harness.Viewer.ResetBreadcrumb();

                // Assert reset
                harness.Host.Verify(host => host.Reset(), Times.Once());
                harness.PopupMessenger.SubscriberCount.Should().Be(0);
                harness.Viewer.GetFolderItems().Should().BeEmpty();

                // Act and assert pooled reuse
                harness.ConfigureAgain();
                harness.RaisePopupReady();
                harness.PopupMessenger.SubscriberCount.Should().Be(1);
                harness.Viewer.SetFolderItems(new[] { "A", "B" });
                harness.Viewer.SetFolderSelectedIndex(0);
                harness.Viewer.SetFolderDroppedDown(true);
                harness.Host.Verify(
                    host =>
                        host.OpenAsync(
                            It.IsAny<Rectangle>(),
                            It.IsAny<Rectangle>(),
                            It.IsAny<Size>()
                        ),
                    Times.Exactly(2)
                );
            }
        }

        [TestMethod]
        public void InitializationFailure_CancelsSessionWithoutDuplicateClose()
        {
            // Arrange
            using (var harness = new ItemViewerDropDownHarness())
            {
                harness
                    .Host.Setup(host =>
                        host.OpenAsync(
                            It.IsAny<Rectangle>(),
                            It.IsAny<Rectangle>(),
                            It.IsAny<Size>()
                        )
                    )
                    .Callback(() =>
                    {
                        harness.SetHostOpen(false);
                        harness.Viewer.BreadcrumbCoordinator.CancelSelector();
                    })
                    .ReturnsAsync(false);

                // Act
                harness.Viewer.SetFolderDroppedDown(true);

                // Assert
                harness.Viewer.BreadcrumbCoordinator.IsSelectorOpen.Should().BeFalse();
                harness.Host.Verify(
                    host => host.Close(It.IsAny<BreadcrumbDropDownCloseReason>()),
                    Times.Never()
                );
            }
        }

        [TestMethod]
        public void ItemViewerDisposal_OwnsHostAndDetachesBothSurfaces()
        {
            // Arrange
            var harness = new ItemViewerDropDownHarness();
            harness.AttachClosedSurface();
            harness.RaisePopupReady();

            // Act
            harness.Viewer.Dispose();

            // Assert
            harness.Host.Verify(host => host.Dispose(), Times.Once());
            harness.ClosedMessenger.SubscriberCount.Should().Be(0);
            harness.PopupMessenger.SubscriberCount.Should().Be(0);
            harness.DisposeContextOnly();
        }

        private static int CountType(IEnumerable<string> messages, string type)
        {
            int count = 0;
            foreach (string message in messages)
            {
                if (message.Contains("\"type\":\"" + type + "\""))
                {
                    count++;
                }
            }
            return count;
        }
    }

    internal sealed class ItemViewerDropDownHarness : IDisposable
    {
        private readonly SynchronizationContext _previousContext;
        private bool _hostOpen;
        private bool _popupReady;

        internal ItemViewerDropDownHarness()
        {
            _previousContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            Viewer = new QuickFiler.ItemViewer();
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            Viewer.InitializeBreadcrumbPipeline(provider.Object);
            Viewer.SetFolderItems(new[] { "A", "B" });
            Viewer.SetFolderSelectedIndex(0);

            Host = new Mock<IBreadcrumbDropDownHost>();
            Host.SetupGet(host => host.PopupMessenger)
                .Returns(() => _popupReady ? PopupMessenger : null);
            Host.SetupGet(host => host.IsOpen).Returns(() => _hostOpen);
            Host.Setup(host =>
                    host.OpenAsync(It.IsAny<Rectangle>(), It.IsAny<Rectangle>(), It.IsAny<Size>())
                )
                .Callback<Rectangle, Rectangle, Size>((anchor, work, desired) => _hostOpen = true)
                .ReturnsAsync(true);
            Host.Setup(host => host.Close(It.IsAny<BreadcrumbDropDownCloseReason>()))
                .Returns<BreadcrumbDropDownCloseReason>(reason =>
                {
                    if (!_hostOpen)
                    {
                        return false;
                    }
                    _hostOpen = false;
                    if (reason == BreadcrumbDropDownCloseReason.Uncommitted)
                    {
                        Viewer.BreadcrumbCoordinator.CancelSelector();
                    }
                    return true;
                });
            Host.Setup(host => host.Reset())
                .Callback(() =>
                {
                    _hostOpen = false;
                    _popupReady = false;
                });
            ConfigureAgain();
        }

        internal QuickFiler.ItemViewer Viewer { get; }
        internal Mock<IBreadcrumbDropDownHost> Host { get; }
        internal TrackingMessenger ClosedMessenger { get; } = new TrackingMessenger();
        internal TrackingMessenger PopupMessenger { get; } = new TrackingMessenger();
        internal Rectangle AnchorScreenBounds { get; } = new Rectangle(120, 240, 390, 25);
        internal Rectangle WorkingArea { get; } = new Rectangle(0, 0, 1920, 1040);
        internal int FocusReturnCount { get; private set; }

        internal void ConfigureAgain()
        {
            MethodInfo method = typeof(QuickFiler.ItemViewer).GetMethod(
                "ConfigureBreadcrumbDropDown",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[]
                {
                    typeof(IBreadcrumbDropDownHost),
                    typeof(Func<Rectangle>),
                    typeof(Func<Rectangle>),
                },
                null
            );
            method
                .Should()
                .NotBeNull("issue #400 requires an injectable ItemViewer popup integration seam");
            method.Invoke(
                Viewer,
                new object[]
                {
                    Host.Object,
                    new Func<Rectangle>(() => AnchorScreenBounds),
                    new Func<Rectangle>(() => WorkingArea),
                }
            );
        }

        internal void AttachClosedSurface()
        {
            MethodInfo method = typeof(QuickFiler.ItemViewer).GetMethod(
                "AttachBreadcrumbMessenger",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            method
                .Should()
                .NotBeNull("issue #400 requires idempotent closed-surface hub attachment");
            method.Invoke(Viewer, new object[] { ClosedMessenger });
        }

        internal void RaisePopupReady()
        {
            _popupReady = true;
            Host.Raise(host => host.PopupMessengerReady += null, Host.Object, EventArgs.Empty);
        }

        internal void RaiseNativeClose()
        {
            if (!_hostOpen)
            {
                return;
            }
            _hostOpen = false;
            Viewer.BreadcrumbCoordinator.CancelSelector();
            FocusReturnCount++;
        }

        internal Task<bool> OpenDropDownAsync() =>
            Host.Object.OpenAsync(
                AnchorScreenBounds,
                WorkingArea,
                new Size(AnchorScreenBounds.Width, 120)
            );

        internal void SetTheme(string theme)
        {
            MethodInfo method = typeof(QuickFiler.ItemViewer).GetMethod(
                "SetBreadcrumbTheme",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            method.Should().NotBeNull("issue #400 requires shared two-surface theme routing");
            method.Invoke(Viewer, new object[] { theme });
        }

        internal void SetHostOpen(bool value) => _hostOpen = value;

        public void Dispose()
        {
            if (!Viewer.IsDisposed)
            {
                Viewer.Dispose();
            }
            DisposeContextOnly();
        }

        internal void DisposeContextOnly()
        {
            SynchronizationContext.SetSynchronizationContext(_previousContext);
        }
    }

    internal sealed class TrackingMessenger : IWebViewMessenger
    {
        private EventHandler<string> _messageReceived;

        internal int SubscriberCount { get; private set; }
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

        internal void Receive(string json) => _messageReceived?.Invoke(this, json);
    }
}
