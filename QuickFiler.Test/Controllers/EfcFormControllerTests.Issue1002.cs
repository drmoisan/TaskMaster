using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Viewers;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Issue 1002 delete gesture end to end: <c>ActionDeleteAsync</c> on a controller wired to a
    /// real router over strict mocks selects the trash row inside the bound document, and a bind
    /// fault is still reported at the bind boundary.
    /// </summary>
    public partial class EfcFormControllerTests
    {
        private const string Issue1002ArchiveRoot = @"\Archive";
        private const string Issue1002Presented = @"Clients\North";
        private const string Issue1002TrashSelected =
            "class=\"rowwrap selected\" data-row-id=\"row-0\"";

        [TestMethod]
        public async Task ActionDeleteAsync_WithRouter_SelectsTrashEntry()
        {
            // Arrange
            var documents = new List<string>();
            EfcFormController controller = CreateIssue1002Controller(documents, withGlobals: true);

            // Act
            await controller.ActionDeleteAsync();

            // Assert
            controller.SelectedFolder.Should().Be(EfcFormController.TrashRowText);
        }

        [TestMethod]
        public async Task ActionDeleteAsync_WithRouter_DeliveredDocumentMarksTrashSelected()
        {
            // Arrange
            var documents = new List<string>();
            EfcFormController controller = CreateIssue1002Controller(documents, withGlobals: true);

            // Act
            await controller.ActionDeleteAsync();

            // Assert
            documents.Should().NotBeEmpty("the gesture binds and navigates a document");
            documents.Last().Should().Contain(Issue1002TrashSelected);
        }

        [TestMethod]
        public async Task ActionDeleteAsync_AwaitedTwice_KeepsTrashSelectedAndSingleTrashRow()
        {
            // Arrange
            var documents = new List<string>();
            EfcFormController controller = CreateIssue1002Controller(documents, withGlobals: true);

            // Act
            await controller.ActionDeleteAsync();
            await controller.ActionDeleteAsync();

            // Assert
            controller.SelectedFolder.Should().Be(EfcFormController.TrashRowText);
            documents.Last().Should().Contain(Issue1002TrashSelected);
            var folderRows = (string[])GetPrivateField(controller, "_folderRows");
            folderRows.Count(row => row == EfcFormController.TrashRowText).Should().Be(1);
        }

        [TestMethod]
        public async Task ActionDeleteAsync_BindFault_IsReportedAtBindBoundary()
        {
            // Arrange: with _globals unset the bind boundary faults reading the archive root.
            var documents = new List<string>();
            EfcFormController controller = CreateIssue1002Controller(documents, withGlobals: false);
            var reported = new List<Exception>();
            controller.BoundaryErrorSink = (message, exception) => reported.Add(exception);

            // Act
            Func<Task> act = () => controller.ActionDeleteAsync();

            // Assert
            await act.Should().NotThrowAsync("the bind boundary contains its own fault");
            reported.Should().ContainSingle("the fault is reported once at the bind boundary");
            documents.Should().BeEmpty("no document is delivered by a faulted bind");
        }

        private static EfcFormController CreateIssue1002Controller(
            List<string> documents,
            bool withGlobals
        )
        {
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            var host = new Mock<IBreadcrumbWebHost>(MockBehavior.Strict);
            host.SetupGet(value => value.IsCoreInitialized).Returns(true);
            host.Setup(value => value.NavigateToString(It.IsAny<string>()))
                .Callback<string>(documents.Add);
            host.Setup(value => value.PostMessageJson(It.IsAny<string>()));
            provider
                .Setup(value =>
                    value.ResolveLeafKeyAsync(
                        Issue1002ArchiveRoot + @"\" + Issue1002Presented,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync((FolderTreeNodeKey)null);
            var router = new BreadcrumbBridgeRouter(
                provider.Object,
                host.Object,
                new BreadcrumbMessageCodec(),
                new BreadcrumbHtmlRenderer(),
                new BreadcrumbOutboundQueue(host.Object)
            );

            EfcFormController controller = CreateMinimalController();
            var viewer = (EfcViewer)
                System.Runtime.Serialization.FormatterServices.GetUninitializedObject(
                    typeof(EfcViewer)
                );
            SetPrivateField(viewer, "_context", new SynchronizationContext());
            SetPrivateField(controller, "_formViewer", viewer);
            SetPrivateField(controller, "_router", router);
            SetPrivateField(controller, "_folderRows", new[] { Issue1002Presented });
            if (withGlobals)
            {
                var ol = new Mock<IOlObjects>(MockBehavior.Strict);
                var globals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
                ol.SetupGet(value => value.ArchiveRootPath).Returns(Issue1002ArchiveRoot);
                globals.SetupGet(value => value.Ol).Returns(ol.Object);
                SetPrivateField(controller, "_globals", globals.Object);
            }

            return controller;
        }
    }
}
