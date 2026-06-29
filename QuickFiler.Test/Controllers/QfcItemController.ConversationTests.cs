using System;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Helper_Classes;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Conversation-cluster tests (research §5.2). Covers the resolver null-guard and the
    /// RenderConversationCount routing through the narrowed IItemViewer intent members
    /// (ConversationCountText / ConversationCountBackColor), including the InvokeRequired
    /// dispatch-routing branch, via Mock&lt;IItemViewer&gt;.
    /// </summary>
    [TestClass]
    public class QfcItemController_ConversationTests
    {
        private sealed class SeamController : QfcItemController
        {
            private readonly Func<Task<ConversationResolver>> _loadCore;

            internal SeamController(Func<Task<ConversationResolver>> loadCore)
                : base()
            {
                _loadCore = loadCore;
            }

            protected override Task<ConversationResolver> DoLoadConversationResolverCoreAsync(
                CancellationTokenSource tokenSource,
                CancellationToken token,
                bool loadAll
            ) => _loadCore();
        }

        private sealed class ViewerController : QfcItemController
        {
            internal ViewerController(IItemViewer viewer)
                : base()
            {
                typeof(QfcItemController)
                    .GetField("_itemViewer", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(this, viewer);
            }
        }

        [TestMethod]
        public async Task PopulateConversationAsync_WhenSeamReturnsNullResolver_ReturnsWithoutCrash()
        {
            // Arrange — the load seam yields a null resolver; the null guard in
            // PopulateConversationAsync must return cleanly without dereferencing it.
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            var controller = new SeamController(() => Task.FromResult<ConversationResolver>(null));

            // Act
            Func<Task> act = () => controller.PopulateConversationAsync(cts, token, false);

            // Assert
            await act.Should()
                .NotThrowAsync(
                    because: "a null ConversationResolver must be guarded, not dereferenced"
                );

            cts.Dispose();
        }

        [TestMethod]
        public async Task LoadConversationResolverAsync_WhenSeamCancels_PropagatesCancellation()
        {
            // Arrange — the load seam throws OperationCanceledException; the catch must rethrow it
            // so callers can observe cancellation rather than swallowing it.
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            var controller = new SeamController(() =>
                Task.FromException<ConversationResolver>(new OperationCanceledException())
            );

            // Act
            Func<Task> act = () => controller.LoadConversationResolverAsync(cts, token, false);

            // Assert
            await act.Should()
                .ThrowAsync<OperationCanceledException>(
                    because: "cancellation is an expected flow that must propagate"
                );

            cts.Dispose();
        }

        [TestMethod]
        public async Task LoadConversationResolverAsync_WhenSeamFaults_SwallowsAndLeavesResolverNull()
        {
            // Arrange — a non-cancellation fault must be logged and swallowed, leaving the resolver
            // unset rather than surfacing the exception to the caller.
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            var controller = new SeamController(() =>
                Task.FromException<ConversationResolver>(
                    new InvalidOperationException("boom from resolver load")
                )
            );

            // Act
            Func<Task> act = () => controller.LoadConversationResolverAsync(cts, token, false);

            // Assert
            await act.Should()
                .NotThrowAsync(
                    because: "non-cancellation faults are caught and logged, not rethrown"
                );
            controller.ConversationResolver.Should().BeNull();

            cts.Dispose();
        }

        [TestMethod]
        public void RenderConversationCount_NonZero_SetsConversationCountTextOnly()
        {
            // Arrange — non-zero count: text is set, the red zero-count back color is not.
            var mock = new Mock<IItemViewer>();
            mock.SetupGet(v => v.InvokeRequired).Returns(false);
            var controller = new ViewerController(mock.Object);

            // Act
            controller.RenderConversationCount(5);

            // Assert
            mock.VerifySet(v => v.ConversationCountText = "5", Times.Once());
            mock.VerifySet(v => v.ConversationCountBackColor = It.IsAny<Color>(), Times.Never());
        }

        [TestMethod]
        public void RenderConversationCount_Zero_SetsRedBackColor()
        {
            // Arrange — zero count: text "0" and the red back color marker are both set.
            var mock = new Mock<IItemViewer>();
            mock.SetupGet(v => v.InvokeRequired).Returns(false);
            var controller = new ViewerController(mock.Object);

            // Act
            controller.RenderConversationCount(0);

            // Assert
            mock.VerifySet(v => v.ConversationCountText = "0", Times.Once());
            mock.VerifySet(v => v.ConversationCountBackColor = Color.Red, Times.Once());
        }

        [TestMethod]
        public void RenderConversationCount_WhenInvokeRequired_MarshalsViaInvoke()
        {
            // Arrange — when the view requires marshaling, the count write must be routed through
            // Invoke rather than set directly on the calling thread.
            var mock = new Mock<IItemViewer>();
            mock.SetupGet(v => v.InvokeRequired).Returns(true);
            var controller = new ViewerController(mock.Object);

            // Act
            controller.RenderConversationCount(3);

            // Assert
            mock.Verify(v => v.Invoke(It.IsAny<Delegate>()), Times.Once());
            mock.VerifySet(v => v.ConversationCountText = It.IsAny<string>(), Times.Never());
        }
    }
}
