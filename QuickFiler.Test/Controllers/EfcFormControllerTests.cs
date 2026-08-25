using System;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Viewers;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Controllers.Tests
{
    [TestClass]
    public class EfcFormControllerTests
    {
        /// <summary>
        /// Creates an EfcFormController via the private no-arg constructor, which allocates
        /// the object without initializing any sub-components, leaving all fields null.
        /// Used to exercise method-level guards without a live Outlook COM context.
        /// </summary>
        private static EfcFormController CreateMinimalController()
        {
            var ctor = typeof(EfcFormController).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                Type.EmptyTypes,
                null
            );
            ctor.Should().NotBeNull("private no-arg constructor must exist on EfcFormController");
            return (EfcFormController)ctor.Invoke(Array.Empty<object>());
        }

        // Regression test for issue #145. A minimally constructed controller now returns
        // before touching `_dataModel` when `_formViewer` has already been cleared, which is
        // the safety contract that prevents the post-await null race from surfacing as a UI
        // thread crash.
        [TestMethod]
        public async Task PopulateFolderCombobox_WhenFormViewerIsNull_ReturnsWithoutTouchingDataModel()
        {
            // Arrange
            // Both `_dataModel` and `_formViewer` are null in a minimally constructed
            // controller. The current contract is that `PopulateFolderCombobox` should exit
            // immediately when the viewer has already been cleaned up, which also avoids
            // dereferencing `_dataModel` in this COM-free test path.
            var controller = CreateMinimalController();

            // Act
            Func<Task> act = () => controller.PopulateFolderCombobox();

            // Assert
            await act.Should()
                .NotThrowAsync(
                    "PopulateFolderCombobox should return immediately when Cleanup has already"
                        + " cleared the form viewer, instead of dereferencing downstream state"
                );
        }

        [TestMethod]
        public async Task Issue439BindBreadcrumbRowsAsync_SubmitsArchiveRootToRealRouter()
        {
            // Arrange: a private no-argument construction and strict interface seams keep this
            // binding-boundary test independent of WinForms, WebView2, Outlook COM, and a UI pump.
            const string archiveRoot = @"\Archive";
            const string presentedTarget = @"Clients\North";
            const string hierarchyTarget = @"\Archive\Clients\North";
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            var host = new Mock<IBreadcrumbWebHost>(MockBehavior.Strict);
            var ol = new Mock<IOlObjects>(MockBehavior.Strict);
            var globals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            var key = new FolderTreeNodeKey("archive", hierarchyTarget, hierarchyTarget);

            host.SetupGet(value => value.IsCoreInitialized).Returns(true);
            host.SetupAdd(value => value.MessageReceived += It.IsAny<EventHandler<string>>());
            host.Setup(value => value.NavigateToString(It.IsAny<string>()));
            host.Setup(value => value.PostMessageJson(It.IsAny<string>()));
            ol.SetupGet(value => value.ArchiveRootPath).Returns(archiveRoot);
            globals.SetupGet(value => value.Ol).Returns(ol.Object);
            provider
                .Setup(value =>
                    value.ResolveLeafKeyAsync(
                        hierarchyTarget,
                        It.IsAny<System.Threading.CancellationToken>()
                    )
                )
                .ReturnsAsync(key);
            provider
                .Setup(value =>
                    value.GetAncestorChainAsync(key, It.IsAny<System.Threading.CancellationToken>())
                )
                .ReturnsAsync(
                    new[]
                    {
                        new FolderBreadcrumbSegment(
                            new FolderTreeNodeKey("archive", archiveRoot, archiveRoot),
                            "Archive",
                            archiveRoot,
                            true
                        ),
                        new FolderBreadcrumbSegment(
                            new FolderTreeNodeKey(
                                "archive",
                                @"\Archive\Clients",
                                @"\Archive\Clients"
                            ),
                            "Clients",
                            @"\Archive\Clients",
                            true
                        ),
                        new FolderBreadcrumbSegment(key, "North", hierarchyTarget, false),
                    }
                );
            var router = new BreadcrumbBridgeRouter(
                provider.Object,
                host.Object,
                new BreadcrumbMessageCodec(),
                new BreadcrumbHtmlRenderer(),
                new BreadcrumbOutboundQueue(host.Object)
            );
            var controller = CreateMinimalController();
            SetPrivateField(controller, "_globals", globals.Object);
            SetPrivateField(controller, "_router", router);

            // Act
            await controller.BindBreadcrumbRowsAsync(new[] { presentedTarget });

            // Assert
            provider.Verify(
                value =>
                    value.ResolveLeafKeyAsync(
                        hierarchyTarget,
                        It.IsAny<System.Threading.CancellationToken>()
                    ),
                Times.Once
            );
            provider.Verify(
                value =>
                    value.GetAncestorChainAsync(
                        key,
                        It.IsAny<System.Threading.CancellationToken>()
                    ),
                Times.Once
            );
            host.Verify(
                value => value.NavigateToString(It.Is<string>(html => html.Contains("North"))),
                Times.Once
            );
            host.VerifyAdd(
                value => value.MessageReceived += It.IsAny<EventHandler<string>>(),
                Times.Once
            );
            host.VerifyGet(value => value.IsCoreInitialized, Times.Once);
            provider.VerifyNoOtherCalls();
            host.VerifyNoOtherCalls();
            ol.VerifyGet(value => value.ArchiveRootPath, Times.Once);
            globals.VerifyGet(value => value.Ol, Times.Once);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target
                .GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field.Should().NotBeNull($"{fieldName} must remain available for this headless seam");
            field.SetValue(target, value);
        }
    }
}
