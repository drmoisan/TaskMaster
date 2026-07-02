using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
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

        // ---------------------------------------------------------------------------
        // Cycle-2 Phase 5 (AC8) de-exemption coverage: PopulateConversation(ConversationResolver),
        // RenderConversationCount() (parameterless), SetTopicThread.
        // ---------------------------------------------------------------------------

        private static ConversationResolver BuildResolverWithCount(int sameFolder)
        {
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockMail = new Mock<MailItem>();
            var resolver = new ConversationResolver(mockGlobals.Object, mockMail.Object);
            resolver.Count = new Pair<int>(sameFolder: sameFolder, expanded: sameFolder);
            return resolver;
        }

        [TestMethod]
        public void PopulateConversation_WithResolver_StoresResolver()
        {
            // Arrange — the resolver-taking overload stores the resolver and delegates the count
            // render to the int overload, which (cycle-2 Phase 6, P6-T3) now routes its fire-and-forget
            // dispatch through the injectable IUiDispatcher seam. The sync-dispatcher mock executes the
            // BeginInvoke delegate against a mocked viewer.
            var dispatcher = QfcItemControllerTestSupport.BuildSyncDispatcher();
            var viewer = new Mock<IItemViewer>();
            var controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_uiDispatcher", dispatcher.Object);
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
            var resolver = BuildResolverWithCount(2);

            // Act
            controller.PopulateConversation(resolver);

            // Assert
            controller.ConversationResolver.Should().BeSameAs(resolver);
            dispatcher.Verify(d => d.BeginInvoke(It.IsAny<System.Action>()), Times.Once());
            viewer.VerifySet(v => v.ConversationCountText = "2", Times.Once());
        }

        [TestMethod]
        public void RenderConversationCountParameterless_WhenResolverNull_RendersZeroWithRedBackColor()
        {
            // Arrange — no resolver set: the null-coalescing default yields count 0.
            var mock = new Mock<IItemViewer>();
            mock.SetupGet(v => v.InvokeRequired).Returns(false);
            var controller = new ViewerController(mock.Object);

            // Act
            controller.RenderConversationCount();

            // Assert
            mock.VerifySet(v => v.ConversationCountText = "0", Times.Once());
            mock.VerifySet(v => v.ConversationCountBackColor = Color.Red, Times.Once());
        }

        [TestMethod]
        public void RenderConversationCountParameterless_WhenResolverSet_RendersSameFolderCount()
        {
            // Arrange — resolver with SameFolder == 5 injected via the backing field.
            var mock = new Mock<IItemViewer>();
            mock.SetupGet(v => v.InvokeRequired).Returns(false);
            var controller = new ViewerController(mock.Object);
            QfcItemControllerTestSupport.SetField(
                controller,
                "_conversationResolver",
                BuildResolverWithCount(5)
            );

            // Act
            controller.RenderConversationCount();

            // Assert
            mock.VerifySet(v => v.ConversationCountText = "5", Times.Once());
            mock.VerifySet(v => v.ConversationCountBackColor = It.IsAny<Color>(), Times.Never());
        }

        [TestMethod]
        public void SetTopicThread_WhenNotInvokeRequired_SetsItemsAndSortsDescending()
        {
            // Arrange
            var mock = new Mock<IItemViewer>();
            mock.SetupGet(v => v.InvokeRequired).Returns(false);
            var controller = new ViewerController(mock.Object);
            var conversation = new List<MailItemHelper> { new MailItemHelper() };

            // Act
            controller.SetTopicThread(conversation);

            // Assert
            mock.Verify(v => v.SetConversationItems(conversation), Times.Once());
            mock.Verify(v => v.SortConversationByDate(SortOrder.Descending), Times.Once());
        }

        [TestMethod]
        public void SetTopicThread_WhenInvokeRequired_MarshalsViaInvoke()
        {
            // Arrange
            var mock = new Mock<IItemViewer>();
            mock.SetupGet(v => v.InvokeRequired).Returns(true);
            var controller = new ViewerController(mock.Object);

            // Act
            controller.SetTopicThread(new List<MailItemHelper>());

            // Assert
            mock.Verify(v => v.Invoke(It.IsAny<Delegate>()), Times.Once());
            mock.Verify(
                v => v.SetConversationItems(It.IsAny<System.Collections.IList>()),
                Times.Never()
            );
        }
    }
}
