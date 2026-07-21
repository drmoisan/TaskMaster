using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Viewers;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Test.Viewers
{
    /// <summary>Failure-first two-surface routing and reuse contracts for issue #400.</summary>
    [TestClass]
    public sealed class BreadcrumbMessengerHubTests
    {
        [TestMethod]
        public void PostJson_BroadcastsOneLogicalRenderAndThemeOncePerSurface()
        {
            // Arrange
            object hub = CreateHub();
            var closed = new TrackingMessenger();
            var popup = new TrackingMessenger();
            Attach(hub, closed, BreadcrumbSelectorViewMode.Collapsed);
            Attach(hub, popup, BreadcrumbSelectorViewMode.Expanded);
            string render = "{\"type\":\"render\",\"rows\":[]}";
            string theme = "{\"type\":\"themeChange\",\"theme\":\"dark\"}";

            // Act
            ((IWebViewMessenger)hub).PostJson(render);
            ((IWebViewMessenger)hub).PostJson(theme);

            // Assert
            closed.Posted.Should().Equal(render, theme);
            popup.Posted.Should().Equal(render, theme);
        }

        [TestMethod]
        public void SelectorView_IsSpecializedForClosedAndExpandedSurfaceModes()
        {
            // Arrange
            object hub = CreateHub();
            var closed = new TrackingMessenger();
            var popup = new TrackingMessenger();
            Attach(hub, closed, BreadcrumbSelectorViewMode.Collapsed);
            Attach(hub, popup, BreadcrumbSelectorViewMode.Expanded);
            const string state =
                "{\"type\":\"selectorView\",\"mode\":\"collapsed\",\"isOpen\":true,"
                + "\"committedIdentity\":\"A\",\"pendingIdentity\":\"B\","
                + "\"options\":[{\"identity\":\"A\",\"isSelectable\":true}],"
                + "\"future\":{\"token\":7}}";

            // Act
            ((IWebViewMessenger)hub).PostJson(state);

            // Assert
            closed.Posted[0].Should().Be(state);
            popup
                .Posted[0]
                .Should()
                .Be(state.Replace("\"mode\":\"collapsed\"", "\"mode\":\"expanded\""));
        }

        [TestMethod]
        public void InboundMessage_FromEitherSurface_IsRoutedOnce()
        {
            // Arrange
            object hub = CreateHub();
            var closed = new TrackingMessenger();
            var popup = new TrackingMessenger();
            Attach(hub, closed, BreadcrumbSelectorViewMode.Collapsed);
            Attach(hub, popup, BreadcrumbSelectorViewMode.Expanded);
            var received = new List<string>();
            ((IWebViewMessenger)hub).MessageReceived += (sender, json) => received.Add(json);

            // Act
            popup.Receive("{\"type\":\"selectorToggle\"}");

            // Assert
            received.Should().Equal("{\"type\":\"selectorToggle\"}");
        }

        [TestMethod]
        public void AttachDetachReattach_IsIdempotentAndDoesNotDuplicateSubscriptions()
        {
            // Arrange
            object hub = CreateHub();
            var surface = new TrackingMessenger();

            // Act and assert attach twice
            Attach(hub, surface, BreadcrumbSelectorViewMode.Collapsed);
            Attach(hub, surface, BreadcrumbSelectorViewMode.Collapsed);
            surface.SubscriberCount.Should().Be(1);
            ((IWebViewMessenger)hub).PostJson("{\"type\":\"render\",\"rows\":[]}");
            surface.Posted.Should().ContainSingle();

            // Act and assert detach/re-attach
            Detach(hub, surface).Should().BeTrue();
            surface.SubscriberCount.Should().Be(0);
            Attach(hub, surface, BreadcrumbSelectorViewMode.Collapsed);
            surface.SubscriberCount.Should().Be(1);
            surface.Receive("{\"type\":\"selectorToggle\"}");
        }

        [TestMethod]
        public void PublicOperations_RejectNullArgumentsAndUseAfterDispose()
        {
            // Arrange
            var hub = new BreadcrumbMessengerHub();
            var surface = new TrackingMessenger();

            // Act
            Action attachNull = () => hub.Attach(null, BreadcrumbSelectorViewMode.Collapsed);
            Action detachNull = () => hub.Detach(null);
            Action postNull = () => hub.PostJson(null);

            // Assert
            attachNull
                .Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("messenger");
            detachNull
                .Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("messenger");
            postNull.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("json");

            hub.Dispose();
            Action attachAfterDispose = () =>
                hub.Attach(surface, BreadcrumbSelectorViewMode.Collapsed);
            Action postAfterDispose = () => hub.PostJson("{\"type\":\"render\"}");
            attachAfterDispose.Should().Throw<ObjectDisposedException>();
            postAfterDispose.Should().Throw<ObjectDisposedException>();
        }

        [TestMethod]
        public void Attach_WithDifferentMode_ReplaysCachedStateWithoutSecondSubscription()
        {
            // Arrange
            var hub = new BreadcrumbMessengerHub();
            var surface = new TrackingMessenger();
            const string state =
                "{\"type\":\"selectorView\",\"mode\":\"collapsed\",\"isOpen\":true}";
            hub.Attach(surface, BreadcrumbSelectorViewMode.Collapsed).Should().BeTrue();
            hub.PostJson(state);

            // Act
            bool attached = hub.Attach(surface, BreadcrumbSelectorViewMode.Expanded);

            // Assert
            attached.Should().BeFalse();
            surface.SubscriberCount.Should().Be(1);
            surface
                .Posted.Should()
                .Equal(state, state.Replace("\"mode\":\"collapsed\"", "\"mode\":\"expanded\""));
        }

        [TestMethod]
        public void DetachAndDispose_HandleUnknownSurfacesAndStaleCallbacksSafely()
        {
            // Arrange
            var hub = new BreadcrumbMessengerHub();
            var surface = new TrackingMessenger();
            var received = new List<string>();
            hub.MessageReceived += (sender, json) => received.Add(json);

            // Act
            bool detachedUnknown = hub.Detach(surface);
            hub.Attach(surface, BreadcrumbSelectorViewMode.Collapsed);
            hub.Dispose();
            hub.Dispose();
            surface.ReceiveLastRemoved("{\"type\":\"selectorToggle\"}");

            // Assert
            detachedUnknown.Should().BeFalse();
            surface.SubscriberCount.Should().Be(0);
            received.Should().BeEmpty();
        }

        [TestMethod]
        public void PostJson_PreservesUnknownAndMalformedSelectorMessagesVerbatim()
        {
            // Arrange
            var hub = new BreadcrumbMessengerHub();
            var surface = new TrackingMessenger();
            hub.Attach(surface, BreadcrumbSelectorViewMode.Collapsed);
            const string unknown = "{\"payload\":7}";
            const string malformedSelector = "{\"type\":\"selectorView\"}";

            // Act
            hub.PostJson(unknown);
            hub.PostJson(malformedSelector);

            // Assert
            surface.Posted.Should().Equal(unknown, malformedSelector);
        }

        [TestMethod]
        public void SelectorView_WithEscapedModeProperty_IsParsedAndPreservedVerbatim()
        {
            // Arrange
            var hub = new BreadcrumbMessengerHub();
            var surface = new TrackingMessenger();
            hub.Attach(surface, BreadcrumbSelectorViewMode.Expanded);
            const string escapedMode =
                "{\"type\":\"selectorView\",\"\\u006dode\":\"collapsed\",\"isOpen\":true}";
            BreadcrumbSelectorMessage parsed = BreadcrumbSelectorMessageSerializer.Parse(
                escapedMode
            );

            // Act
            hub.PostJson(escapedMode);

            // Assert
            parsed
                .Should()
                .BeOfType<BreadcrumbSelectorViewMessage>()
                .Which.Mode.Should()
                .Be(BreadcrumbSelectorViewMode.Collapsed);
            surface.Posted.Should().Equal(escapedMode);
        }

        private static object CreateHub()
        {
            Type type = typeof(BreadcrumbBridgeCoordinator).Assembly.GetType(
                "QuickFiler.Viewers.BreadcrumbMessengerHub",
                false
            );
            type.Should().NotBeNull("issue #400 requires a two-surface messenger hub");
            return Activator.CreateInstance(type);
        }

        private static void Attach(
            object hub,
            IWebViewMessenger messenger,
            BreadcrumbSelectorViewMode mode
        ) => hub.GetType().GetMethod("Attach").Invoke(hub, new object[] { messenger, mode });

        private static bool Detach(object hub, IWebViewMessenger messenger) =>
            (bool)hub.GetType().GetMethod("Detach").Invoke(hub, new object[] { messenger });

        private sealed class TrackingMessenger : IWebViewMessenger
        {
            private EventHandler<string> _messageReceived;
            private EventHandler<string> _lastRemovedMessageReceived;

            public int SubscriberCount { get; private set; }
            public List<string> Posted { get; } = new List<string>();

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
                    _lastRemovedMessageReceived = value;
                    SubscriberCount--;
                }
            }

            public void PostJson(string json) => Posted.Add(json);

            public void Receive(string json) => _messageReceived?.Invoke(this, json);

            public void ReceiveLastRemoved(string json) =>
                _lastRemovedMessageReceived?.Invoke(this, json);
        }
    }
}
