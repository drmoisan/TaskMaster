using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
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
        public void Attach_WithDifferentMode_PreservesOriginalModeWithoutReplayOrSecondSubscription()
        {
            // Arrange
            var hub = new BreadcrumbMessengerHub();
            var surface = new TrackingMessenger();
            const string state =
                "{\"type\":\"selectorView\",\"mode\":\"collapsed\",\"isOpen\":true}";
            const string later =
                "{\"type\":\"selectorView\",\"mode\":\"expanded\",\"isOpen\":false}";
            hub.Attach(surface, BreadcrumbSelectorViewMode.Collapsed).Should().BeTrue();
            hub.PostJson(state);

            // Act
            bool attached = hub.Attach(surface, BreadcrumbSelectorViewMode.Expanded);
            hub.PostJson(later);

            // Assert
            attached.Should().BeFalse();
            surface.SubscriberCount.Should().Be(1);
            surface
                .Posted.Should()
                .Equal(state, later.Replace("\"mode\":\"expanded\"", "\"mode\":\"collapsed\""));
        }

        [TestMethod]
        public void Attach_AfterPendingUpdates_ReplaysOnlyCurrentStateOncePerSurface()
        {
            // Arrange
            var hub = new BreadcrumbMessengerHub();
            var collapsed = new TrackingMessenger();
            var popup = new TrackingMessenger();
            const string staleRender = "{\"type\":\"render\",\"rows\":[{\"percentText\":\"21%\"}]}";
            const string render = "{\"type\":\"render\",\"rows\":[{\"percentText\":\"73%\"}]}";
            const string selector =
                "{\"type\":\"selectorView\",\"mode\":\"collapsed\",\"isOpen\":true}";
            const string theme = "{\"type\":\"themeChange\",\"theme\":\"dark\"}";
            hub.PostJson(staleRender);
            hub.PostJson(render);
            hub.PostJson(selector);
            hub.PostJson(theme);

            // Act
            hub.Attach(collapsed, BreadcrumbSelectorViewMode.Collapsed).Should().BeTrue();
            hub.Attach(popup, BreadcrumbSelectorViewMode.Expanded).Should().BeTrue();
            hub.Attach(collapsed, BreadcrumbSelectorViewMode.Collapsed).Should().BeFalse();
            hub.Attach(popup, BreadcrumbSelectorViewMode.Expanded).Should().BeFalse();

            // Assert
            collapsed.Posted.Should().Equal(render, selector, theme);
            popup
                .Posted.Should()
                .Equal(
                    render,
                    selector.Replace("\"mode\":\"collapsed\"", "\"mode\":\"expanded\""),
                    theme
                );
            collapsed.SubscriberCount.Should().Be(1);
            popup.SubscriberCount.Should().Be(1);
        }

        [TestMethod]
        public void Attach_ReplayFailureRollsBackSubscriptionAndAllowsRetry()
        {
            // Arrange
            var hub = new BreadcrumbMessengerHub();
            var surface = new TrackingMessenger { ThrowOnPost = true };
            const string render = "{\"type\":\"render\",\"rows\":[]}";
            hub.PostJson(render);

            // Act
            Action attach = () => hub.Attach(surface, BreadcrumbSelectorViewMode.Collapsed);

            // Assert rollback and retry
            attach.Should().Throw<InvalidOperationException>();
            surface.SubscriberCount.Should().Be(0);
            surface.ThrowOnPost = false;
            hub.Attach(surface, BreadcrumbSelectorViewMode.Collapsed).Should().BeTrue();
            surface.SubscriberCount.Should().Be(1);
            surface.Posted.Should().Equal(render);
        }

        [TestMethod]
        public async Task CollapsedAttachment_ReplayFailureAndDisposeDetachBeforeMessengerCleanup()
        {
            // Arrange a replay failure after exact collapsed readiness
            var hub = new BreadcrumbMessengerHub();
            hub.PostJson("{\"type\":\"render\",\"rows\":[]}");
            var controller = new BreadcrumbCollapsedSurfaceController();
            var attachment = new BreadcrumbCollapsedAttachment(hub, controller);
            var failedSurface = new TrackingMessenger { ThrowOnPost = true };
            Task<bool> failed = attachment.AttachAsync(() =>
                Tuple.Create<IWebViewMessenger, BreadcrumbNavigationReadiness>(
                    failedSurface,
                    CompletedReadiness(501)
                )
            );

            // Act and assert transactional failure cleanup
            Func<Task> awaitFailure = async () => await failed.ConfigureAwait(false);
            await awaitFailure.Should().ThrowAsync<InvalidOperationException>();
            controller.ReadyMessenger.Should().BeNull();
            failedSurface.SubscriberCount.Should().Be(0);
            failedSurface.DisposeCount.Should().Be(1);
            failedSurface.Lifecycle.Should().ContainInOrder("unsubscribe", "dispose");

            // Act a successful retry, then dispose the ready attachment
            var readySurface = new TrackingMessenger();
            (
                await attachment
                    .AttachAsync(() =>
                        Tuple.Create<IWebViewMessenger, BreadcrumbNavigationReadiness>(
                            readySurface,
                            CompletedReadiness(502)
                        )
                    )
                    .ConfigureAwait(false)
            )
                .Should()
                .BeTrue();
            attachment.Dispose();
            hub.Dispose();

            // Assert hub detachment precedes controller-owned messenger disposal
            readySurface.SubscriberCount.Should().Be(0);
            readySurface.DisposeCount.Should().Be(1);
            readySurface.Lifecycle.Should().ContainInOrder("unsubscribe", "dispose");
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
            hub.Detach(surface);
            surface.ReceiveLastRemoved("{\"type\":\"selectorToggle\"}");
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

        /// <summary>
        /// Issue #501, consolidated: one broadcast in which a surface throws must not starve the other
        /// surfaces (I-501.1, AC-08), must still deliver to the recording surface (I-501.2, AC-09),
        /// must not propagate the surface throw to the caller (SR-3, AC-11 containment half), and must
        /// leave a replay cache that a later attach can trust (I-501.3, AC-10).
        /// <para>
        /// The starvation assertion is ORDER-INDEPENDENT by construction. <c>Dictionary.Values</c>
        /// enumeration order is not contractual, so a test attaching "throwing first, recording second"
        /// would pass vacuously whenever the runtime happened to enumerate the recording surface first.
        /// Two surfaces that BOTH increment the attempt counter BEFORE throwing make the expected total
        /// 2 in every enumeration order, while the pre-fix behaviour yields 1 in every order.
        /// </para>
        /// Deterministic: one thread, no timer, no wait, no temp file.
        /// </summary>
        [TestMethod]
        public void PostJson_SurfaceFailureDoesNotStarveOtherSurfacesOrFalsifyReplayCache()
        {
            // Arrange
            var hub = new BreadcrumbMessengerHub();
            int attempts = 0;
            var first = new CountingThrowingMessenger(() => attempts++);
            var second = new CountingThrowingMessenger(() => attempts++);
            var recording = new TrackingMessenger();
            hub.Attach(first, BreadcrumbSelectorViewMode.Collapsed).Should().BeTrue();
            hub.Attach(second, BreadcrumbSelectorViewMode.Expanded).Should().BeTrue();
            hub.Attach(recording, BreadcrumbSelectorViewMode.Collapsed).Should().BeTrue();
            const string render = "{\"type\":\"render\",\"rows\":[]}";

            // Act
            Action post = () => hub.PostJson(render);

            // Assert containment (SR-3), then no starvation, delivery, and cache truthfulness.
            post.Should().NotThrow("PostJson must not propagate a surface throw to its caller");
            attempts
                .Should()
                .Be(2, "every live attachment must receive exactly one delivery attempt (I-501.1)");
            recording
                .Posted.Should()
                .Contain(render, "a throwing sibling must not prevent delivery (I-501.2)");

            var late = new TrackingMessenger();
            hub.Attach(late, BreadcrumbSelectorViewMode.Collapsed).Should().BeTrue();
            late.Posted.Should()
                .Contain(
                    render,
                    "the replay cache must hold a state a live surface received (I-501.3)"
                );
        }

        /// <summary>
        /// Records one delivery ATTEMPT before failing, which is what I-501.1 counts. The existing
        /// <c>TrackingMessenger</c> cannot serve: its <c>ThrowOnPost</c> path throws without recording,
        /// so it counts successes only.
        /// </summary>
        private sealed class CountingThrowingMessenger : IWebViewMessenger, IDisposable
        {
            private readonly Action _onAttempt;

            internal CountingThrowingMessenger(Action onAttempt)
            {
                _onAttempt = onAttempt;
            }

            public event EventHandler<string> MessageReceived
            {
                add { }
                remove { }
            }

            public void PostJson(string json)
            {
                _onAttempt();
                throw new InvalidOperationException("Surface delivery rejected");
            }

            public void Dispose() { }
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

        private static BreadcrumbNavigationReadiness CompletedReadiness(ulong navigationId)
        {
            var readiness = new BreadcrumbNavigationReadiness("Collapsed", () => { });
            readiness.BeginNavigation(() =>
            {
                readiness.NavigationStarted(navigationId);
                readiness.NavigationCompleted(navigationId, true, null);
            });
            return readiness;
        }

        private sealed class TrackingMessenger : IWebViewMessenger, IDisposable
        {
            private EventHandler<string> _messageReceived;
            private EventHandler<string> _lastRemovedMessageReceived;

            public int SubscriberCount { get; private set; }
            public int DisposeCount { get; private set; }
            public List<string> Posted { get; } = new List<string>();
            public List<string> Lifecycle { get; } = new List<string>();
            public bool ThrowOnPost { get; set; }

            public event EventHandler<string> MessageReceived
            {
                add
                {
                    _messageReceived += value;
                    SubscriberCount++;
                    Lifecycle.Add("subscribe");
                }
                remove
                {
                    _messageReceived -= value;
                    _lastRemovedMessageReceived = value;
                    SubscriberCount--;
                    Lifecycle.Add("unsubscribe");
                }
            }

            public void PostJson(string json)
            {
                if (ThrowOnPost)
                    throw new InvalidOperationException("Replay rejected");
                Posted.Add(json);
            }

            public void Receive(string json) => _messageReceived?.Invoke(this, json);

            public void ReceiveLastRemoved(string json) =>
                _lastRemovedMessageReceived?.Invoke(this, json);

            public void Dispose()
            {
                if (DisposeCount == 0)
                {
                    DisposeCount++;
                    Lifecycle.Add("dispose");
                }
            }
        }
    }
}
