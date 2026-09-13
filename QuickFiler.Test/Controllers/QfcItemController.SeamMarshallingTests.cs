using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler;
using QuickFiler.Controllers;
using UtilitiesCS;
using UtilitiesCS.Threading;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Issue #743 seam-marshalling tests. Drives <c>ResolveControlGroupsAsync</c> through the
    /// <see cref="IItemViewer"/> interface and <c>AssignControlsAsync</c> through the injected
    /// <see cref="IUiDispatcher"/> seam, with no concrete <see cref="QuickFiler.ItemViewer"/> and
    /// no message pump. Every <c>await</c> on the viewer's <c>UiSyncContext</c> completes inline
    /// because the same context instance the viewer mock returns is installed as the ambient
    /// <see cref="SynchronizationContext"/> for the duration of each test (the awaiter reports
    /// <c>IsCompleted</c> on reference equality), so no wait, poll, sleep or wall-clock read is
    /// needed anywhere in this class.
    /// </summary>
    [TestClass]
    public class QfcItemController_SeamMarshallingTests
    {
        /// <summary>
        /// Converts a genuine deadlock in production code into a test failure instead of a CI
        /// hang. It is the only time-valued construct in this class; no assertion measures time.
        /// </summary>
        private const int SeamTimeoutMs = 60000;

        private static Label BuildHostedLabel(Panel host)
        {
            Label label = new Label();
            host.Controls.Add(label);
            return label;
        }

        /// <summary>
        /// Builds a viewer mock whose tip labels are all parented on <paramref name="host"/> (the
        /// tooltip factory's parent resolution accepts exactly a <see cref="TableLayoutPanel"/> or a
        /// <see cref="Panel"/>), whose <c>UiSyncContext</c> is <paramref name="context"/>, and whose
        /// descendant enumeration contains one <see cref="TableLayoutPanel"/> and one
        /// <see cref="Button"/> so the control-group assertions are non-vacuous.
        /// </summary>
        private static Mock<IItemViewer> BuildViewer(
            Panel host,
            SynchronizationContext context,
            IList<Label> tipsLabels,
            IList<Label> expandedTipsLabels
        )
        {
            Mock<IItemViewer> viewer = new Mock<IItemViewer>();
            viewer.SetupGet(v => v.InvokeRequired).Returns(false);
            viewer.SetupGet(v => v.UiSyncContext).Returns(context);
            viewer.SetupGet(v => v.TipsLabels).Returns(tipsLabels);
            viewer.SetupGet(v => v.ExpandedTipsLabels).Returns(expandedTipsLabels);
            viewer.SetupGet(v => v.ItemNumberLabel).Returns(tipsLabels[0]);
            viewer
                .Setup(v => v.DescendantControls())
                .Returns(new Control[] { host, new TableLayoutPanel(), new Button() });
            return viewer;
        }

        private static HarnessController BuildController(
            Mock<IItemViewer> viewer,
            Mock<IUiDispatcher> dispatcher
        )
        {
            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
            QfcItemControllerTestSupport.SetField(controller, "_uiDispatcher", dispatcher.Object);
            controller.Token = CancellationToken.None;
            return controller;
        }

        /// <summary>
        /// Mirrors the private <c>BuildGlobals</c> helper of the ViewerSetup test file: the
        /// control-assignment member reads <c>_globals.QfSettings</c> after the marshal.
        /// </summary>
        private static Mock<IApplicationGlobals> BuildGlobals()
        {
            Mock<IAppQuickFilerSettings> settings = new Mock<IAppQuickFilerSettings>();
            settings.SetupGet(s => s.MoveEntireConversation).Returns(false);
            settings.SetupGet(s => s.SaveEmailCopy).Returns(false);
            settings.SetupGet(s => s.SaveAttachments).Returns(false);
            settings.SetupGet(s => s.SavePictures).Returns(false);
            Mock<IApplicationGlobals> globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(g => g.QfSettings).Returns(settings.Object);
            return globals;
        }

        /// <summary>
        /// AC3A structural zero-construction assertion: the control-group resolution member runs to
        /// completion when driven with a viewer mock and the synchronous dispatcher double, and the
        /// object it was driven with is not a concrete <see cref="QuickFiler.ItemViewer"/>.
        /// </summary>
        [TestMethod]
        [Timeout(SeamTimeoutMs)]
        public async Task ResolveControlGroupsAsync_WithMockViewerAndSyncDispatcher_CompletesWithoutAConcreteViewer()
        {
            // Arrange
            SynchronizationContext previous = SynchronizationContext.Current;
            SynchronizationContext context = new SynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(context);
            try
            {
                using (Panel host = new Panel())
                {
                    IList<Label> tips = new List<Label>
                    {
                        BuildHostedLabel(host),
                        BuildHostedLabel(host),
                    };
                    IList<Label> expanded = new List<Label> { BuildHostedLabel(host) };
                    Mock<IItemViewer> viewer = BuildViewer(host, context, tips, expanded);
                    HarnessController controller = BuildController(
                        viewer,
                        QfcItemControllerTestSupport.BuildSyncDispatcher()
                    );

                    // Act
                    await controller.ResolveControlGroupsAsync(viewer.Object);

                    // Assert — no concrete viewer was constructed or required.
                    viewer
                        .Object.Should()
                        .NotBeAssignableTo<QuickFiler.ItemViewer>(
                            because: "the member must be drivable through the interface alone"
                        );
                    QfcItemControllerTestSupport
                        .GetField(controller, "_itemPositionTips")
                        .Should()
                        .NotBeNull(because: "the item-number tip is built from ItemNumberLabel");
                }
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }

        /// <summary>
        /// AC2 regression test: with a viewer mock supplying panel-parented tip labels and a
        /// descendant enumeration, the member populates both tip-detail collections and both
        /// concrete control groups.
        /// </summary>
        [TestMethod]
        [Timeout(SeamTimeoutMs)]
        public async Task ResolveControlGroupsAsync_WithMockViewer_PopulatesTipsAndControlGroups()
        {
            // Arrange
            SynchronizationContext previous = SynchronizationContext.Current;
            SynchronizationContext context = new SynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(context);
            try
            {
                using (Panel host = new Panel())
                {
                    IList<Label> tips = new List<Label>
                    {
                        BuildHostedLabel(host),
                        BuildHostedLabel(host),
                        BuildHostedLabel(host),
                    };
                    IList<Label> expanded = new List<Label>
                    {
                        BuildHostedLabel(host),
                        BuildHostedLabel(host),
                    };
                    Mock<IItemViewer> viewer = BuildViewer(host, context, tips, expanded);
                    HarnessController controller = BuildController(
                        viewer,
                        QfcItemControllerTestSupport.BuildSyncDispatcher()
                    );

                    // Act
                    await controller.ResolveControlGroupsAsync(viewer.Object);

                    // Assert — one tip per label, and the control groups classified by type.
                    ICollection tipsDetails = (ICollection)
                        QfcItemControllerTestSupport.GetField(controller, "_listTipsDetails");
                    tipsDetails.Should().NotBeNull();
                    tipsDetails.Count.Should().Be(tips.Count);
                    ICollection tipsExpanded = (ICollection)
                        QfcItemControllerTestSupport.GetField(controller, "_listTipsExpanded");
                    tipsExpanded.Should().NotBeNull();
                    tipsExpanded.Count.Should().Be(expanded.Count);
                    controller.TableLayoutPanels.Should().NotBeNullOrEmpty();
                    controller.Buttons.Should().NotBeNullOrEmpty();
                    viewer.Verify(v => v.DescendantControls(), Times.Once());
                }
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }

        /// <summary>
        /// Structural contract: the first parameter of the asynchronous control-group resolution
        /// member is the viewer interface, not the concrete viewer type.
        /// </summary>
        [TestMethod]
        [Timeout(SeamTimeoutMs)]
        public void ResolveControlGroupsAsync_FirstParameterType_IsTheViewerInterface()
        {
            // Arrange
            MethodInfo method = typeof(QfcItemController).GetMethod(
                "ResolveControlGroupsAsync",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            // Act
            ParameterInfo[] parameters = method.GetParameters();

            // Assert
            method.Should().NotBeNull();
            parameters.Should().HaveCount(1);
            parameters[0]
                .ParameterType.Should()
                .Be(typeof(IItemViewer), because: "the member is driven through the interface");
        }

        /// <summary>
        /// Negative flow: a cancelled controller token is observed before any work is done.
        /// </summary>
        [TestMethod]
        [Timeout(SeamTimeoutMs)]
        public async Task ResolveControlGroupsAsync_WithCancelledToken_ThrowsOperationCanceled()
        {
            // Arrange
            Mock<IItemViewer> viewer = new Mock<IItemViewer>();
            HarnessController controller = BuildController(
                viewer,
                QfcItemControllerTestSupport.BuildSyncDispatcher()
            );
            controller.Token = new CancellationToken(canceled: true);

            // Act
            Func<Task> act = () => controller.ResolveControlGroupsAsync(viewer.Object);

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        /// <summary>
        /// The asynchronous control-assignment member marshals through the injected
        /// <see cref="IUiDispatcher"/> seam rather than the viewer-owned WPF dispatcher. The Moq
        /// verification distinguishes the seam path from the null-tolerance branch.
        /// </summary>
        [TestMethod]
        [Timeout(SeamTimeoutMs)]
        public async Task AssignControlsAsync_WithSyncDispatcherDouble_AssignsThroughTheInjectedSeam()
        {
            // Arrange
            SynchronizationContext previous = SynchronizationContext.Current;
            SynchronizationContext context = new SynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(context);
            try
            {
                using (Panel host = new Panel())
                {
                    IList<Label> tips = new List<Label> { BuildHostedLabel(host) };
                    IList<Label> expanded = new List<Label> { BuildHostedLabel(host) };
                    Mock<IItemViewer> viewer = BuildViewer(host, context, tips, expanded);
                    Mock<IUiDispatcher> dispatcher =
                        QfcItemControllerTestSupport.BuildSyncDispatcher();
                    HarnessController controller = BuildController(viewer, dispatcher);
                    QfcItemControllerTestSupport.SetField(
                        controller,
                        "_globals",
                        BuildGlobals().Object
                    );

                    // Act
                    await controller.AssignControlsAsync(new MailItemHelper(), 2);

                    // Assert — exactly one marshal through the injected seam, and the assignment ran.
                    dispatcher.Verify(d => d.InvokeAsync(It.IsAny<Action>()), Times.Once());
                    viewer.VerifySet(v => v.ItemNumberText = "2", Times.Once());
                }
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }
    }
}
