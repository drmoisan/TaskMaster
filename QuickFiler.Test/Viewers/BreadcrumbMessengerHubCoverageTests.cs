using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Viewers;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Test.Viewers
{
    /// <summary>Deterministic numeric-coverage contracts for hub and collapsed attachment lifetimes.</summary>
    [TestClass]
    public sealed class BreadcrumbMessengerHubCoverageTests
    {
        [TestMethod]
        public void Hub_NullDuplicateAndDisposedOperations_FollowExactContracts()
        {
            var hub = new BreadcrumbMessengerHub();
            var surface = new TrackingMessenger();

            AssertParameter(
                () => hub.Attach(null, BreadcrumbSelectorViewMode.Collapsed),
                "messenger"
            );
            AssertParameter(() => hub.Detach(null), "messenger");
            AssertParameter(() => hub.PostJson(null), "json");
            hub.Attach(surface, BreadcrumbSelectorViewMode.Collapsed).Should().BeTrue();
            hub.Attach(surface, BreadcrumbSelectorViewMode.Expanded).Should().BeFalse();
            surface.SubscribeAttempts.Should().Be(1);

            hub.Dispose();
            hub.Dispose();

            surface.UnsubscribeAttempts.Should().Be(1);
            hub.Detach(surface).Should().BeFalse();
            AssertThrows<ObjectDisposedException>(() =>
                hub.Attach(surface, BreadcrumbSelectorViewMode.Collapsed)
            );
            AssertThrows<ObjectDisposedException>(() => hub.PostJson("{}"));
        }

        [TestMethod]
        public void Hub_SubscribeAndUnsubscribeFailures_RollBackWithoutStaleInbound()
        {
            var hub = new BreadcrumbMessengerHub();
            var surface = new TrackingMessenger { ThrowOnSubscribe = true };
            var received = new List<string>();
            hub.MessageReceived += (sender, json) => received.Add(json);

            AssertThrows<InvalidOperationException>(() =>
                hub.Attach(surface, BreadcrumbSelectorViewMode.Collapsed)
            );
            surface.SubscribeAttempts.Should().Be(1);
            surface.UnsubscribeAttempts.Should().Be(1);
            surface.SubscriberCount.Should().Be(0);

            surface.ThrowOnSubscribe = false;
            hub.Attach(surface, BreadcrumbSelectorViewMode.Collapsed).Should().BeTrue();
            surface.ThrowOnUnsubscribe = true;
            hub.Detach(surface).Should().BeTrue();
            hub.Detach(surface).Should().BeFalse();
            surface.UnsubscribeAttempts.Should().Be(2);
            surface.SubscriberCount.Should().Be(0);
            surface.ReceiveLastRemoved("stale");
            received.Should().BeEmpty();
            hub.Dispose();
        }

        [TestMethod]
        public void Hub_CachedAndNoncachedPosts_ReplayOnlyLatestStateInSequence()
        {
            var hub = new BreadcrumbMessengerHub();
            const string unknown = "{\"event\":\"ignored\"}";
            const string theme = "{\"type\":\"themeChange\",\"theme\":\"dark\"}";
            const string render = "{\"type\":\"render\",\"version\":2}";
            const string selector =
                "{\"type\":\"selectorView\",\"mode\":\"collapsed\",\"isOpen\":true}";
            hub.PostJson(unknown);
            hub.PostJson("{\"type\":\"render\",\"version\":1}");
            hub.PostJson(theme);
            hub.PostJson(render);
            hub.PostJson(selector);

            var collapsed = new TrackingMessenger();
            hub.Attach(collapsed, BreadcrumbSelectorViewMode.Collapsed).Should().BeTrue();
            hub.PostJson(unknown);
            var expanded = new TrackingMessenger();
            hub.Attach(expanded, BreadcrumbSelectorViewMode.Expanded).Should().BeTrue();

            collapsed.Posted.Should().Equal(theme, render, selector, unknown);
            string expandedSelector = selector.Replace("\"collapsed\"", "\"expanded\"");
            expanded.Posted.Should().Equal(theme, render, expandedSelector);
            hub.Dispose();
        }

        [TestMethod]
        public void Hub_MalformedMissingAndSameModeMessages_AreParsedWithoutMutation()
        {
            var hub = new BreadcrumbMessengerHub();
            var collapsed = new TrackingMessenger();
            var expanded = new TrackingMessenger();
            hub.Attach(collapsed, BreadcrumbSelectorViewMode.Collapsed);
            hub.Attach(expanded, BreadcrumbSelectorViewMode.Expanded);
            string[] messages =
            {
                "{\"payload\":1}",
                "{\"type\"}",
                "{\"type\":7}",
                "{\"type\":\"render}",
                "{\"type\":\"selectorView\"}",
                "{\"type\":\"selectorView\",\"mode\":\"collapsed\",\"isOpen\":false}",
                "{\"type\":\"selectorView\",\"\\u006dode\":\"collapsed\",\"isOpen\":true}",
            };

            foreach (string message in messages)
                hub.PostJson(message);

            collapsed.Posted.Should().Equal(messages);
            expanded.Posted.Take(5).Should().Equal(messages.Take(5));
            expanded.Posted[5].Should().Contain("\"mode\":\"expanded\"");
            expanded.Posted[6].Should().Be(messages[6]);
            hub.Dispose();
        }

        [TestMethod]
        public void Hub_InvalidUnknownAndStaleInboundSenders_AreIgnoredExactly()
        {
            var hub = new BreadcrumbMessengerHub();
            var surface = new TrackingMessenger();
            var received = new List<string>();
            hub.MessageReceived += (sender, json) => received.Add(json);
            hub.Attach(surface, BreadcrumbSelectorViewMode.Collapsed);

            surface.ReceiveFrom(null, "null");
            surface.ReceiveFrom(new object(), "invalid");
            surface.ReceiveFrom(new TrackingMessenger(), "unknown");
            surface.ReceiveFrom(surface, "current");
            hub.Detach(surface);
            surface.ReceiveLastRemoved("detached");
            hub.Attach(surface, BreadcrumbSelectorViewMode.Collapsed);
            hub.Dispose();
            surface.ReceiveLastRemoved("disposed");

            received.Should().Equal("current");
        }

        [TestMethod]
        public async Task Attachment_ConstructorFactoryAndCandidateGuards_AllowRetry()
        {
            var hub = new BreadcrumbMessengerHub();
            var controller = new BreadcrumbCollapsedSurfaceController();
            AssertParameter(() => new BreadcrumbCollapsedAttachment(null, controller), "hub");
            AssertParameter(() => new BreadcrumbCollapsedAttachment(hub, null), "controller");
            var disposalFailureSurface = new TrackingMessenger { ThrowOnDispose = true };
            AssertParameter(() => controller.AttachAsync(null, Task.CompletedTask), "messenger");
            AssertParameter(
                () => controller.AttachAsync(disposalFailureSurface, (Task)null),
                "readiness"
            );
            AssertParameter(
                () =>
                    controller.AttachAsync(
                        disposalFailureSurface,
                        (BreadcrumbNavigationReadiness)null
                    ),
                "readiness"
            );
            (await controller.AttachAsync(disposalFailureSurface, Task.CompletedTask))
                .Should()
                .BeTrue();
            controller.Reset();
            disposalFailureSurface.DisposeCount.Should().Be(1);
            var attachment = new BreadcrumbCollapsedAttachment(hub, controller);
            AssertParameter(() => attachment.AttachAsync(null), "candidateFactory");

            var expected = new InvalidOperationException("factory failed");
            Task<bool> factoryFailure = attachment.AttachAsync(() => throw expected);
            (await AssertFaultAsync<InvalidOperationException>(factoryFailure))
                .Should()
                .BeSameAs(expected);
            await AssertFaultAsync<InvalidOperationException>(attachment.AttachAsync(() => null));
            var invalidSurface = new TrackingMessenger();
            await AssertInvalidCandidateAsync(attachment, invalidSurface, null);
            invalidSurface.DisposeCount.Should().Be(1);
            int orphanDetachCount = 0;
            BreadcrumbNavigationReadiness orphanReadiness = Readiness(
                602,
                detach: () => orphanDetachCount++
            );
            await AssertInvalidCandidateAsync(attachment, null, orphanReadiness);
            orphanDetachCount.Should().Be(1);

            var surface = new TrackingMessenger();
            (await Attach(attachment, surface, Readiness(601, true))).Should().BeTrue();
            attachment.Dispose();
            surface.UnsubscribeAttempts.Should().Be(1);
            surface.DisposeCount.Should().Be(1);
            hub.Dispose();
        }

        [TestMethod]
        public async Task Attachment_SharedPendingAndReadyBypass_ReuseOneCandidate()
        {
            var hub = new BreadcrumbMessengerHub();
            var controller = new BreadcrumbCollapsedSurfaceController();
            var controllerSurface = new TrackingMessenger();
            var otherSurface = new TrackingMessenger();
            BreadcrumbNavigationReadiness controllerReadiness = Readiness(700);
            BreadcrumbNavigationReadiness otherReadiness = Readiness(702);
            Task<bool> controllerPending = controller.AttachAsync(
                controllerSurface,
                controllerReadiness
            );
            controller
                .AttachAsync(controllerSurface, controllerReadiness)
                .Should()
                .BeSameAs(controllerPending);
            InvalidOperationException messengerConflict = AssertThrows<InvalidOperationException>(
                () =>
                    controller.AttachAsync(controllerSurface, otherReadiness)
            );
            messengerConflict
                .Message.Should()
                .Be("The collapsed messenger already has a pending navigation.");
            InvalidOperationException readinessConflict = AssertThrows<InvalidOperationException>(
                () =>
                    controller.AttachAsync(otherSurface, controllerReadiness)
            );
            readinessConflict
                .Message.Should()
                .Be("The pending navigation already belongs to another collapsed messenger.");
            controllerReadiness.NavigationCompleted(700, true, null);
            (await controllerPending).Should().BeTrue();
            (await controller.AttachAsync(controllerSurface, Task.CompletedTask)).Should().BeTrue();
            controller.Reset();
            controllerSurface.DisposeCount.Should().Be(1);
            otherSurface.DisposeCount.Should().Be(0);
            otherReadiness.Dispose();
            var attachment = new BreadcrumbCollapsedAttachment(hub, controller);
            var surface = new TrackingMessenger();
            BreadcrumbNavigationReadiness readiness = Readiness(701);
            int factoryCalls = 0;
            Task<bool> first = attachment.AttachAsync(() =>
            {
                factoryCalls++;
                return Candidate(surface, readiness);
            });
            Task<bool> shared = attachment.AttachAsync(() =>
            {
                factoryCalls++;
                return null;
            });

            shared.Should().BeSameAs(first);
            readiness.NavigationCompleted(701, true, null);
            (await first).Should().BeTrue();
            (await shared).Should().BeTrue();
            (await attachment.AttachAsync(() => throw new InvalidOperationException()))
                .Should()
                .BeTrue();
            factoryCalls.Should().Be(1);
            surface.SubscriberCount.Should().Be(1);
            attachment.Dispose();
            hub.Dispose();
        }

        [TestMethod]
        public async Task Attachment_StaleFactoryCandidateAndReadyReset_CleanExactlyOnce()
        {
            var hub = new BreadcrumbMessengerHub();
            var attachment = new BreadcrumbCollapsedAttachment(
                hub,
                new BreadcrumbCollapsedSurfaceController()
            );
            int staleDetachCount = 0;
            var staleSurface = new TrackingMessenger();
            BreadcrumbNavigationReadiness staleReadiness = Readiness(
                801,
                detach: () => staleDetachCount++
            );

            bool stale = await attachment.AttachAsync(() =>
            {
                attachment.Reset();
                return Candidate(staleSurface, staleReadiness);
            });
            attachment.Reset();

            stale.Should().BeFalse();
            staleDetachCount.Should().Be(1);
            staleSurface.DisposeCount.Should().Be(1);
            var readySurface = new TrackingMessenger();
            (await Attach(attachment, readySurface, Readiness(802, true))).Should().BeTrue();
            attachment.Reset();
            readySurface.UnsubscribeAttempts.Should().Be(1);
            readySurface.DisposeCount.Should().Be(1);
            attachment.Dispose();
            hub.Dispose();
        }

        [TestMethod]
        public async Task Attachment_ControllerAndHubFailures_ResetAndPermitRetry()
        {
            var hub = new BreadcrumbMessengerHub();
            var attachment = new BreadcrumbCollapsedAttachment(
                hub,
                new BreadcrumbCollapsedSurfaceController()
            );
            var controllerFailureSurface = new TrackingMessenger();

            (await Attach(attachment, controllerFailureSurface, Readiness(901, false)))
                .Should()
                .BeFalse();
            controllerFailureSurface.DisposeCount.Should().Be(1);
            hub.PostJson("{\"type\":\"render\",\"rows\":[]}");
            var replayFailureSurface = new TrackingMessenger { ThrowOnPost = true };
            await AssertFaultAsync<InvalidOperationException>(
                Attach(attachment, replayFailureSurface, Readiness(902, true))
            );
            replayFailureSurface.UnsubscribeAttempts.Should().Be(1);
            replayFailureSurface.DisposeCount.Should().Be(1);

            var retrySurface = new TrackingMessenger();
            (await Attach(attachment, retrySurface, Readiness(903, true))).Should().BeTrue();
            retrySurface.Posted.Should().ContainSingle();
            attachment.Dispose();
            retrySurface.UnsubscribeAttempts.Should().Be(1);
            retrySurface.DisposeCount.Should().Be(1);
            hub.Dispose();
        }

        [TestMethod]
        public async Task Attachment_PendingDisposeIsIdempotentAndBlocksLaterAttach()
        {
            var hub = new BreadcrumbMessengerHub();
            var controller = new BreadcrumbCollapsedSurfaceController();
            var attachment = new BreadcrumbCollapsedAttachment(hub, controller);
            var surface = new TrackingMessenger();
            int detachCount = 0;
            BreadcrumbNavigationReadiness readiness = Readiness(1001, detach: () => detachCount++);
            Task<bool> pending = Attach(attachment, surface, readiness);

            attachment.Dispose();
            attachment.Dispose();
            attachment.Reset();

            (await pending).Should().BeFalse();
            detachCount.Should().Be(1);
            surface.DisposeCount.Should().Be(1);
            int factoryCalls = 0;
            AssertThrows<ObjectDisposedException>(() =>
                attachment.AttachAsync(() =>
                {
                    factoryCalls++;
                    return Candidate(surface, Readiness(1002, true));
                })
            );
            factoryCalls.Should().Be(0);
            AssertThrows<ObjectDisposedException>(() =>
                controller.AttachAsync(surface, Task.CompletedTask)
            );
            hub.Dispose();
        }

        private static Tuple<IWebViewMessenger, BreadcrumbNavigationReadiness> Candidate(
            IWebViewMessenger messenger,
            BreadcrumbNavigationReadiness readiness
        ) => Tuple.Create(messenger, readiness);

        private static Task<bool> Attach(
            BreadcrumbCollapsedAttachment attachment,
            IWebViewMessenger messenger,
            BreadcrumbNavigationReadiness readiness
        ) => attachment.AttachAsync(() => Candidate(messenger, readiness));

        private static async Task AssertInvalidCandidateAsync(
            BreadcrumbCollapsedAttachment attachment,
            IWebViewMessenger messenger,
            BreadcrumbNavigationReadiness readiness
        ) =>
            (
                await AssertFaultAsync<InvalidOperationException>(
                    Attach(attachment, messenger, readiness)
                )
            )
                .Message.Should()
                .Be("Collapsed attachment did not provide a messenger and readiness lease.");

        private static BreadcrumbNavigationReadiness Readiness(
            ulong navigationId,
            bool? success = null,
            Action detach = null
        )
        {
            var readiness = new BreadcrumbNavigationReadiness("Collapsed", detach ?? (() => { }));
            readiness.BeginNavigation(() => { });
            readiness.NavigationStarted(navigationId);
            if (success.HasValue)
                readiness.NavigationCompleted(
                    navigationId,
                    success.Value,
                    success.Value ? null : "Failed"
                );
            return readiness;
        }

        private static T AssertThrows<T>(Action action)
            where T : Exception => action.Should().Throw<T>().Which;

        private static void AssertParameter(Action action, string parameter) =>
            AssertThrows<ArgumentNullException>(action).ParamName.Should().Be(parameter);

        private static async Task<T> AssertFaultAsync<T>(Task task)
            where T : Exception
        {
            Func<Task> awaitTask = async () => await task.ConfigureAwait(false);
            return (await awaitTask.Should().ThrowAsync<T>()).Which;
        }

        private sealed class TrackingMessenger : IWebViewMessenger, IDisposable
        {
            private EventHandler<string> _messageReceived;
            private EventHandler<string> _lastRemoved;

            internal int DisposeCount { get; private set; }
            internal int SubscriberCount { get; private set; }
            internal int SubscribeAttempts { get; private set; }
            internal int UnsubscribeAttempts { get; private set; }
            internal bool ThrowOnPost { get; set; }
            internal bool ThrowOnDispose { get; set; }
            internal bool ThrowOnSubscribe { get; set; }
            internal bool ThrowOnUnsubscribe { get; set; }
            internal List<string> Posted { get; } = new List<string>();

            public event EventHandler<string> MessageReceived
            {
                add
                {
                    SubscribeAttempts++;
                    if (ThrowOnSubscribe)
                        throw new InvalidOperationException("Subscribe failed");
                    _messageReceived += value;
                    SubscriberCount++;
                }
                remove
                {
                    UnsubscribeAttempts++;
                    _messageReceived -= value;
                    _lastRemoved = value;
                    if (SubscriberCount > 0)
                        SubscriberCount--;
                    if (ThrowOnUnsubscribe)
                        throw new InvalidOperationException("Unsubscribe failed");
                }
            }

            public void PostJson(string json)
            {
                if (ThrowOnPost)
                    throw new InvalidOperationException("Post failed");
                Posted.Add(json);
            }

            internal void ReceiveFrom(object sender, string json) =>
                _messageReceived?.Invoke(sender, json);

            internal void ReceiveLastRemoved(string json) => _lastRemoved?.Invoke(this, json);

            public void Dispose()
            {
                DisposeCount++;
                if (ThrowOnDispose)
                    throw new InvalidOperationException("Dispose failed");
            }
        }
    }
}
