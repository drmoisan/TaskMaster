using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Viewers;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Test.Controllers
{
    /// <summary>
    /// Regression tests for issue #792 on <see cref="BreadcrumbOutboundQueue.DiscardPending"/>
    /// (AC-U7): when CoreWebView2 initialization fails, buffered payloads are discarded rather
    /// than drained, because the host cannot post to a core that does not exist. No timers,
    /// sleeps or temp files are used.
    /// </summary>
    [TestClass]
    public sealed class BreadcrumbOutboundQueueIssue792Tests
    {
        private const string RenderPayload = "{\"type\":\"render\"}";
        private const string FocusPayload = "{\"type\":\"focusSearch\"}";
        private const string ThemePayload = "{\"type\":\"theme\"}";

        private static Mock<IBreadcrumbWebHost> CreateUninitializedHost()
        {
            var host = new Mock<IBreadcrumbWebHost>();
            host.SetupGet(h => h.IsCoreInitialized).Returns(false);
            return host;
        }

        /// <summary>
        /// Three buffered payloads are discarded: the method reports three, nothing remains
        /// pending and nothing was posted. Before the fix the stub returns zero and keeps them.
        /// </summary>
        [TestMethod]
        public void DiscardPending_ReturnsTheDiscardedCountAndLeavesZeroPending()
        {
            // Arrange
            var host = CreateUninitializedHost();
            var queue = new BreadcrumbOutboundQueue(host.Object);
            queue.PostOrQueue(RenderPayload);
            queue.PostOrQueue(FocusPayload);
            queue.PostOrQueue(ThemePayload);
            queue.PendingCount.Should().Be(3, "the uninitialized host must buffer every payload");

            // Act
            int discarded = queue.DiscardPending();

            // Assert
            discarded.Should().Be(3, "the discarded count must equal the number buffered");
            queue.PendingCount.Should().Be(0, "a discard must leave nothing pending");
            host.Verify(
                h => h.PostMessageJson(It.IsAny<string>()),
                Times.Never,
                "a discard must not post to a core that does not exist"
            );
        }

        /// <summary>Control: discarding an empty buffer reports zero and posts nothing.</summary>
        [TestMethod]
        public void DiscardPending_OnAnEmptyQueue_ReturnsZero()
        {
            // Arrange
            var host = CreateUninitializedHost();
            var queue = new BreadcrumbOutboundQueue(host.Object);

            // Act
            int discarded = queue.DiscardPending();

            // Assert
            discarded.Should().Be(0, "an empty buffer has nothing to discard");
            queue.PendingCount.Should().Be(0, "an empty buffer stays empty");
            host.Verify(h => h.PostMessageJson(It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// AC-U7 through the router: the failure entry point discards the router's own outbound
        /// queue without posting. Before the fix the declaration-only body leaves both payloads
        /// pending.
        /// </summary>
        [TestMethod]
        public void NotifyInitializationFailed_DiscardsTheOutboundQueueWithoutPosting()
        {
            // Arrange
            var host = CreateUninitializedHost();
            var queue = new BreadcrumbOutboundQueue(host.Object);
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Loose);
            var router = new BreadcrumbBridgeRouter(
                provider.Object,
                host.Object,
                new BreadcrumbMessageCodec(),
                new BreadcrumbHtmlRenderer(),
                queue
            );
            queue.PostOrQueue(RenderPayload);
            queue.PostOrQueue(FocusPayload);
            queue.PendingCount.Should().Be(2, "the uninitialized host must buffer both payloads");

            // Act
            router.NotifyInitializationFailed(new InvalidOperationException("boom"));

            // Assert
            queue
                .PendingCount.Should()
                .Be(0, "a failed initialization must discard the buffered payloads");
            host.Verify(
                h => h.PostMessageJson(It.IsAny<string>()),
                Times.Never,
                "discarded payloads must never be posted"
            );
        }
    }
}
