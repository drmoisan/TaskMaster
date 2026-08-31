using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Viewers;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Test.Controllers
{
    public partial class BreadcrumbBridgeRouterIssue439Tests
    {
        [TestMethod]
        public void Issue609_DirectRowSelection_UsesFullLookupAndRelativeFilingTarget()
        {
            const string archiveRootPath = @"\\mailbox@example.com\Archive";
            const string presentedTarget = @"Clients\North";
            const string hierarchyPath = @"\\mailbox@example.com\Archive\Clients\North";
            FolderTreeNodeKey key = Key(hierarchyPath);
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            var host = new Mock<IBreadcrumbWebHost>(MockBehavior.Strict);
            host.SetupGet(value => value.IsCoreInitialized).Returns(true);
            host.Setup(value => value.NavigateToString(It.IsAny<string>()));
            host.Setup(value => value.PostMessageJson(It.IsAny<string>()));
            provider
                .Setup(value =>
                    value.ResolveLeafKeyAsync(hierarchyPath, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(key);
            provider
                .Setup(value => value.GetAncestorChainAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Chain(hierarchyPath, "Clients", "North"));
            var router = new BreadcrumbBridgeRouter(
                provider.Object,
                host.Object,
                new BreadcrumbMessageCodec(),
                new BreadcrumbHtmlRenderer(),
                new BreadcrumbOutboundQueue(host.Object)
            );

            router
                .BindRowsAsync(
                    new[] { presentedTarget },
                    new[] { new FolderScore(presentedTarget, 730, 0.73) },
                    archiveRootPath,
                    CancellationToken.None
                )
                .GetAwaiter()
                .GetResult();
            router
                .ProcessInboundAsync("{\"type\":\"rowSelected\",\"rowId\":\"row-0\"}")
                .GetAwaiter()
                .GetResult();

            provider.Verify(
                value => value.ResolveLeafKeyAsync(hierarchyPath, It.IsAny<CancellationToken>()),
                Times.Once
            );
            router.SelectedFolderPath.Should().Be(presentedTarget).And.NotBe(hierarchyPath);
        }

        [TestMethod]
        public void Issue609_AncestorActivation_EmitsArchiveRelativeFilingTarget()
        {
            const string archiveRootPath = @"\\mailbox@example.com\Archive";
            const string presentedTarget = @"Clients\North";
            const string hierarchyPath = @"\\mailbox@example.com\Archive\Clients\North";
            FolderTreeNodeKey leafKey = Key(hierarchyPath);
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            var host = new Mock<IBreadcrumbWebHost>(MockBehavior.Strict);
            host.SetupGet(value => value.IsCoreInitialized).Returns(true);
            host.Setup(value => value.NavigateToString(It.IsAny<string>()));
            host.Setup(value => value.PostMessageJson(It.IsAny<string>()));
            provider
                .Setup(value =>
                    value.ResolveLeafKeyAsync(hierarchyPath, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(leafKey);
            provider
                .Setup(value => value.GetAncestorChainAsync(leafKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Chain(hierarchyPath, "Clients", "North"));
            var router = new BreadcrumbBridgeRouter(
                provider.Object,
                host.Object,
                new BreadcrumbMessageCodec(),
                new BreadcrumbHtmlRenderer(),
                new BreadcrumbOutboundQueue(host.Object)
            );

            router
                .BindRowsAsync(
                    new[] { presentedTarget },
                    new[] { new FolderScore(presentedTarget, 730, 0.73) },
                    archiveRootPath,
                    CancellationToken.None
                )
                .GetAwaiter()
                .GetResult();
            router
                .ProcessInboundAsync(
                    "{\"type\":\"segmentActivate\",\"rowId\":\"row-0\",\"segmentIndex\":1}"
                )
                .GetAwaiter()
                .GetResult();

            router.SelectedFolderPath.Should().Be(@"Clients").And.NotBe(hierarchyPath);
        }

        [TestMethod]
        public void Issue609_ImmediateChildActivation_EmitsArchiveRelativeFilingTarget()
        {
            const string archiveRootPath = @"\\mailbox@example.com\Archive";
            const string presentedTarget = @"Clients\North";
            const string hierarchyPath = @"\\mailbox@example.com\Archive\Clients\North";
            FolderTreeNodeKey leafKey = Key(hierarchyPath);
            FolderTreeNodeKey clientsKey = Key(@"\\mailbox@example.com\Archive\Clients");
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            var host = new Mock<IBreadcrumbWebHost>(MockBehavior.Strict);
            host.SetupGet(value => value.IsCoreInitialized).Returns(true);
            host.Setup(value => value.NavigateToString(It.IsAny<string>()));
            host.Setup(value => value.PostMessageJson(It.IsAny<string>()));
            provider
                .Setup(value =>
                    value.ResolveLeafKeyAsync(hierarchyPath, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(leafKey);
            provider
                .Setup(value => value.GetAncestorChainAsync(leafKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Chain(hierarchyPath, "Clients", "North"));
            provider
                .Setup(value =>
                    value.GetImmediateSubfoldersAsync(clientsKey, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(new[] { Segment(hierarchyPath, "North", false) });
            var router = new BreadcrumbBridgeRouter(
                provider.Object,
                host.Object,
                new BreadcrumbMessageCodec(),
                new BreadcrumbHtmlRenderer(),
                new BreadcrumbOutboundQueue(host.Object)
            );

            router
                .BindRowsAsync(
                    new[] { presentedTarget },
                    new[] { new FolderScore(presentedTarget, 730, 0.73) },
                    archiveRootPath,
                    CancellationToken.None
                )
                .GetAwaiter()
                .GetResult();
            router
                .ProcessInboundAsync(
                    "{\"type\":\"segmentActivate\",\"rowId\":\"row-0\",\"segmentIndex\":1}"
                )
                .GetAwaiter()
                .GetResult();
            router
                .ProcessInboundAsync("{\"type\":\"leafExpandToggle\",\"rowId\":\"row-0\"}")
                .GetAwaiter()
                .GetResult();
            router
                .ProcessInboundAsync(
                    "{\"type\":\"renderedChildActivate\",\"rowId\":\"row-0\",\"childIndex\":0}"
                )
                .GetAwaiter()
                .GetResult();

            provider.Verify(
                value => value.ResolveLeafKeyAsync(hierarchyPath, It.IsAny<CancellationToken>()),
                Times.Once
            );
            router.SelectedFolderPath.Should().Be(presentedTarget).And.NotBe(hierarchyPath);
        }

        [TestMethod]
        public void Issue439AncestorActivationQueriesAncestorKeyAndSelectsArchiveRelativeChild()
        {
            // Arrange: pure router seams only; no controls, WebView2, Outlook COM, or message loop.
            const string archiveRoot = @"\Archive";
            const string target = @"Clients\North";
            const string fullTarget = @"\Archive\Clients\North";
            FolderTreeNodeKey leafKey = Key(fullTarget);
            FolderTreeNodeKey clientsKey = Key(@"\Archive\Clients");
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            var host = new Mock<IBreadcrumbWebHost>(MockBehavior.Strict);
            host.SetupGet(h => h.IsCoreInitialized).Returns(true);
            host.Setup(h => h.NavigateToString(It.IsAny<string>()));
            host.Setup(h => h.PostMessageJson(It.IsAny<string>()));
            provider
                .Setup(p => p.ResolveLeafKeyAsync(fullTarget, It.IsAny<CancellationToken>()))
                .ReturnsAsync(leafKey);
            provider
                .Setup(p => p.GetAncestorChainAsync(leafKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Chain(fullTarget, "Clients", "North"));
            provider
                .Setup(p =>
                    p.GetImmediateSubfoldersAsync(clientsKey, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(
                    new[]
                    {
                        Segment(@"\Archive\Clients\North", "North", false),
                        Segment(@"\Archive\Clients\South", "South", false),
                    }
                );
            var router = new BreadcrumbBridgeRouter(
                provider.Object,
                host.Object,
                new BreadcrumbMessageCodec(),
                new BreadcrumbHtmlRenderer(),
                new BreadcrumbOutboundQueue(host.Object)
            );

            router
                .BindRowsAsync(
                    new[] { target },
                    new[] { new FolderScore(target, 730, 0.73) },
                    archiveRoot,
                    CancellationToken.None
                )
                .GetAwaiter()
                .GetResult();

            // Act: make the ancestor active, expand its real immediate children, and select a sibling.
            router
                .ProcessInboundAsync(
                    "{\"type\":\"segmentActivate\",\"rowId\":\"row-0\",\"segmentIndex\":1}"
                )
                .GetAwaiter()
                .GetResult();
            router.SelectedFolderPath.Should().Be(@"Clients");
            router
                .ProcessInboundAsync("{\"type\":\"leafExpandToggle\",\"rowId\":\"row-0\"}")
                .GetAwaiter()
                .GetResult();
            router
                .ProcessInboundAsync(
                    "{\"type\":\"renderedChildActivate\",\"rowId\":\"row-0\",\"childIndex\":1}"
                )
                .GetAwaiter()
                .GetResult();

            // Assert
            provider.Verify(
                p => p.GetImmediateSubfoldersAsync(clientsKey, It.IsAny<CancellationToken>()),
                Times.Once
            );
            router.SelectedFolderPath.Should().Be(@"Clients\South");
        }
    }
}
