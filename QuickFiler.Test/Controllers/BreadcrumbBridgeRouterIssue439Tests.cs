using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Headless Issue #439 regression tests for the pure breadcrumb router seams.
    /// </summary>
    [TestClass]
    public partial class BreadcrumbBridgeRouterIssue439Tests
    {
        [TestMethod]
        public void Issue439ArchiveRelativeRowsRenderLineagePreserveFilingTargetAndProbability()
        {
            // Arrange: all collaborators are Moq seams; this test creates no UI, COM object, or
            // message pump. The provider recognizes only archive-rooted hierarchy identities.
            const string archiveRoot = @"\Archive";
            const string suggestionTarget = @"Clients\North";
            const string searchTarget = @"Search\Follow Up";
            const string suggestionHierarchyPath = @"\Archive\Clients\North";
            const string searchHierarchyPath = @"\Archive\Search\Follow Up";
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            var host = new Mock<IBreadcrumbWebHost>(MockBehavior.Strict);
            var navigated = new List<string>();
            host.SetupGet(h => h.IsCoreInitialized).Returns(true);
            host.Setup(h => h.NavigateToString(It.IsAny<string>()))
                .Callback<string>(html => navigated.Add(html));
            host.Setup(h => h.PostMessageJson(It.IsAny<string>()));

            FolderTreeNodeKey suggestionKey = Key(suggestionHierarchyPath);
            FolderTreeNodeKey searchKey = Key(searchHierarchyPath);
            provider
                .Setup(p =>
                    p.ResolveLeafKeyAsync(suggestionHierarchyPath, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(suggestionKey);
            provider
                .Setup(p =>
                    p.ResolveLeafKeyAsync(searchHierarchyPath, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(searchKey);
            provider
                .Setup(p => p.GetAncestorChainAsync(suggestionKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Chain(suggestionHierarchyPath, "Clients", "North"));
            provider
                .Setup(p => p.GetAncestorChainAsync(searchKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Chain(searchHierarchyPath, "Search", "Follow Up"));

            var router = new BreadcrumbBridgeRouter(
                provider.Object,
                host.Object,
                new BreadcrumbMessageCodec(),
                new BreadcrumbHtmlRenderer(),
                new BreadcrumbOutboundQueue(host.Object)
            );

            // Act: a banner, suggestion, search result, and pseudo-row share the Efc bind.
            router
                .BindRowsAsync(
                    new[]
                    {
                        "==== SUGGESTIONS ====",
                        suggestionTarget,
                        searchTarget,
                        "Trash to Delete",
                    },
                    new[]
                    {
                        new FolderScore(suggestionTarget, 730, 0.73),
                        new FolderScore(searchTarget, 610, 0.61),
                    },
                    archiveRoot,
                    CancellationToken.None
                )
                .GetAwaiter()
                .GetResult();
            router
                .ProcessInboundAsync("{\"type\":\"rowSelected\",\"rowId\":\"row-1\"}")
                .GetAwaiter()
                .GetResult();

            // Assert: ordinary Efc targets must resolve through the archive root, rendered
            // hierarchy remains root-first, and the original filing/score identity is retained.
            provider.Verify(
                p => p.ResolveLeafKeyAsync(suggestionHierarchyPath, It.IsAny<CancellationToken>()),
                Times.Once
            );
            provider.Verify(
                p => p.ResolveLeafKeyAsync(searchHierarchyPath, It.IsAny<CancellationToken>()),
                Times.Once
            );
            provider.Verify(
                p => p.ResolveLeafKeyAsync("==== SUGGESTIONS ====", It.IsAny<CancellationToken>()),
                Times.Never
            );
            provider.Verify(
                p => p.ResolveLeafKeyAsync("Trash to Delete", It.IsAny<CancellationToken>()),
                Times.Never
            );
            navigated.Should().ContainSingle();
            navigated[0].IndexOf(archiveRoot, StringComparison.Ordinal).Should().BePositive();
            navigated[0]
                .IndexOf("Clients", StringComparison.Ordinal)
                .Should()
                .BeGreaterThan(navigated[0].IndexOf(archiveRoot, StringComparison.Ordinal));
            navigated[0].Should().Contain("73%");
            router.SelectedFolderPath.Should().Be(suggestionTarget);
        }

        [TestMethod]
        public void Issue439RootedTargetUsesOriginalPathForProviderLookupCaseInsensitively()
        {
            // Arrange: the presented target is rooted with casing different from the configured
            // root, so the provider must receive the original full path unchanged (#439).
            const string archiveRoot = @"\Archive";
            const string fullTarget = @"\aRcHiVe\Clients\North";
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            var host = new Mock<IBreadcrumbWebHost>(MockBehavior.Strict);
            host.SetupGet(h => h.IsCoreInitialized).Returns(true);
            host.Setup(h => h.NavigateToString(It.IsAny<string>()));
            host.Setup(h => h.PostMessageJson(It.IsAny<string>()));
            FolderTreeNodeKey key = Key(fullTarget);
            provider
                .Setup(p => p.ResolveLeafKeyAsync(fullTarget, It.IsAny<CancellationToken>()))
                .ReturnsAsync(key);
            provider
                .Setup(p => p.GetAncestorChainAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Chain(fullTarget, "Clients", "North"));
            var router = new BreadcrumbBridgeRouter(
                provider.Object,
                host.Object,
                new BreadcrumbMessageCodec(),
                new BreadcrumbHtmlRenderer(),
                new BreadcrumbOutboundQueue(host.Object)
            );

            // Act
            router
                .BindRowsAsync(
                    new[] { fullTarget },
                    new[] { new FolderScore(fullTarget, 730, 0.73) },
                    archiveRoot,
                    CancellationToken.None
                )
                .GetAwaiter()
                .GetResult();
            router
                .ProcessInboundAsync("{\"type\":\"rowSelected\",\"rowId\":\"row-0\"}")
                .GetAwaiter()
                .GetResult();

            // Assert
            provider.Verify(
                p => p.ResolveLeafKeyAsync(fullTarget, It.IsAny<CancellationToken>()),
                Times.Once
            );
            router.SelectedFolderPath.Should().Be(@"Clients\North");
        }

        [TestMethod]
        public void Issue439UnresolvedChainsUseSelectableFallbackForEveryDiagnosableProviderOutcome()
        {
            // Arrange: no key, empty chain, provider exception, and cancellation are all
            // diagnosable hierarchy outcomes that retain one selectable presented segment.
            const string archiveRoot = @"\Archive";
            const string noKey = @"Clients\NoKey";
            const string emptyChain = @"Clients\Empty";
            const string providerFailure = @"Clients\Failure";
            const string canceled = @"Clients\Canceled";
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            var host = new Mock<IBreadcrumbWebHost>(MockBehavior.Strict);
            var documents = new List<string>();
            host.SetupGet(h => h.IsCoreInitialized).Returns(true);
            host.Setup(h => h.NavigateToString(It.IsAny<string>())).Callback<string>(documents.Add);
            host.Setup(h => h.PostMessageJson(It.IsAny<string>()));
            FolderTreeNodeKey emptyKey = Key(@"\Archive\Clients\Empty");
            provider
                .Setup(p =>
                    p.ResolveLeafKeyAsync(@"\Archive\Clients\NoKey", It.IsAny<CancellationToken>())
                )
                .ReturnsAsync((FolderTreeNodeKey)null);
            provider
                .Setup(p =>
                    p.ResolveLeafKeyAsync(@"\Archive\Clients\Empty", It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(emptyKey);
            provider
                .Setup(p => p.GetAncestorChainAsync(emptyKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<FolderBreadcrumbSegment>());
            provider
                .Setup(p =>
                    p.ResolveLeafKeyAsync(
                        @"\Archive\Clients\Failure",
                        It.IsAny<CancellationToken>()
                    )
                )
                .ThrowsAsync(new InvalidOperationException("expected provider failure"));
            provider
                .Setup(p =>
                    p.ResolveLeafKeyAsync(
                        @"\Archive\Clients\Canceled",
                        It.IsAny<CancellationToken>()
                    )
                )
                .ThrowsAsync(new OperationCanceledException("expected provider cancellation"));
            var router = new BreadcrumbBridgeRouter(
                provider.Object,
                host.Object,
                new BreadcrumbMessageCodec(),
                new BreadcrumbHtmlRenderer(),
                new BreadcrumbOutboundQueue(host.Object)
            );

            // Act
            router
                .BindRowsAsync(
                    new[] { noKey, emptyChain, providerFailure, canceled },
                    new FolderScore[0],
                    archiveRoot,
                    CancellationToken.None
                )
                .GetAwaiter()
                .GetResult();
            router
                .ProcessInboundAsync("{\"type\":\"rowSelected\",\"rowId\":\"row-3\"}")
                .GetAwaiter()
                .GetResult();

            // Assert: no key cannot query ancestors; all fallback labels remain rendered and
            // the selected fallback keeps its original filing target.
            provider.Verify(
                p =>
                    p.GetAncestorChainAsync(
                        It.IsAny<FolderTreeNodeKey>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
            documents.Should().ContainSingle();
            documents[0]
                .Should()
                .Contain("NoKey")
                .And.Contain("Empty")
                .And.Contain("Failure")
                .And.Contain("Canceled");
            router.SelectedFolderPath.Should().Be(canceled);
        }

        [TestMethod]
        public void Issue439InvalidTypedNavigationDoesNotSelectBannerOrPseudoRows()
        {
            // Arrange: these rows have no hierarchy-provider contract and remain headless.
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            var host = new Mock<IBreadcrumbWebHost>(MockBehavior.Strict);
            host.SetupGet(h => h.IsCoreInitialized).Returns(true);
            host.Setup(h => h.NavigateToString(It.IsAny<string>()));
            host.Setup(h => h.PostMessageJson(It.IsAny<string>()));
            var router = new BreadcrumbBridgeRouter(
                provider.Object,
                host.Object,
                new BreadcrumbMessageCodec(),
                new BreadcrumbHtmlRenderer(),
                new BreadcrumbOutboundQueue(host.Object)
            );
            router
                .BindRowsAsync(
                    new[] { "==== BANNER ====", "Trash to Delete" },
                    new FolderScore[0],
                    @"\Archive",
                    CancellationToken.None
                )
                .GetAwaiter()
                .GetResult();

            // Act: the payloads are syntactically valid but target non-navigation row kinds.
            router
                .ProcessInboundAsync(
                    "{\"type\":\"segmentActivate\",\"rowId\":\"row-0\",\"segmentIndex\":0}"
                )
                .GetAwaiter()
                .GetResult();
            router
                .ProcessInboundAsync(
                    "{\"type\":\"renderedChildActivate\",\"rowId\":\"row-1\",\"childIndex\":0}"
                )
                .GetAwaiter()
                .GetResult();

            // Assert
            router.SelectedFolderPath.Should().BeNull();
            provider.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void Issue439ArchiveRootBoundarySelectionAndHostEventRemainDeterministic()
        {
            // Arrange: all boundaries are strict mocks; no test path creates a form, WebView2,
            // Outlook COM object, message pump, file, or network resource.
            const string archiveRoot = @"\Archive";
            const string target = @"Clients\North";
            const string hierarchyTarget = @"\Archive\Clients\North";
            FolderTreeNodeKey leafKey = Key(hierarchyTarget);
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            var host = new Mock<IBreadcrumbWebHost>(MockBehavior.Strict);
            var selected = new List<string>();
            host.SetupGet(value => value.IsCoreInitialized).Returns(true);
            host.Setup(value => value.NavigateToString(It.IsAny<string>()));
            host.Setup(value => value.PostMessageJson(It.IsAny<string>()));
            provider
                .Setup(value =>
                    value.ResolveLeafKeyAsync(hierarchyTarget, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(leafKey);
            provider
                .Setup(value => value.GetAncestorChainAsync(leafKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new[]
                    {
                        Segment(archiveRoot, "Archive", true),
                        Segment(@"\External\Clients", "External Clients", true),
                        Segment(hierarchyTarget, "North", false),
                    }
                );
            var router = new BreadcrumbBridgeRouter(
                provider.Object,
                host.Object,
                new BreadcrumbMessageCodec(),
                new BreadcrumbHtmlRenderer(),
                new BreadcrumbOutboundQueue(host.Object)
            );
            router.SelectedFolderPathChanged += (sender, path) => selected.Add(path);

            router
                .BindRowsAsync(
                    new[] { target },
                    new[] { new FolderScore(target, 730, 0.73) },
                    archiveRoot,
                    CancellationToken.None
                )
                .GetAwaiter()
                .GetResult();

            // Act: the host event targets the exact archive root and the direct typed
            // activation targets a path outside it; both are now deterministic non-selections.
            host.Raise(
                value => value.MessageReceived += null,
                host.Object,
                "{\"type\":\"segmentActivate\",\"rowId\":\"row-0\",\"segmentIndex\":0}"
            );
            router
                .ProcessInboundAsync(
                    "{\"type\":\"segmentActivate\",\"rowId\":\"row-0\",\"segmentIndex\":1}"
                )
                .GetAwaiter()
                .GetResult();

            // Assert
            selected.Should().BeEmpty();
            router.SelectedFolderPath.Should().BeNull();
            host.Verify(value => value.PostMessageJson(It.IsAny<string>()), Times.Never);
            provider.Verify(
                value => value.ResolveLeafKeyAsync(hierarchyTarget, It.IsAny<CancellationToken>()),
                Times.Once
            );
            provider.Verify(
                value => value.GetAncestorChainAsync(leafKey, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [TestMethod]
        public void Issue439SlashOnlyArchiveRootPreservesFullHierarchySelection()
        {
            // Arrange: a slash-only root trims to empty for both hierarchy conversion directions.
            const string target = @"\Archive\Clients\North";
            FolderTreeNodeKey key = Key(target);
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            var host = new Mock<IBreadcrumbWebHost>(MockBehavior.Strict);
            host.SetupGet(value => value.IsCoreInitialized).Returns(true);
            host.Setup(value => value.NavigateToString(It.IsAny<string>()));
            host.Setup(value => value.PostMessageJson(It.IsAny<string>()));
            provider
                .Setup(value => value.ResolveLeafKeyAsync(target, It.IsAny<CancellationToken>()))
                .ReturnsAsync(key);
            provider
                .Setup(value => value.GetAncestorChainAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Chain(target, "Clients", "North"));
            var router = new BreadcrumbBridgeRouter(
                provider.Object,
                host.Object,
                new BreadcrumbMessageCodec(),
                new BreadcrumbHtmlRenderer(),
                new BreadcrumbOutboundQueue(host.Object)
            );

            // Act
            router
                .BindRowsAsync(new[] { target }, new FolderScore[0], @"\", CancellationToken.None)
                .GetAwaiter()
                .GetResult();
            router
                .ProcessInboundAsync(
                    "{\"type\":\"segmentActivate\",\"rowId\":\"row-0\",\"segmentIndex\":0}"
                )
                .GetAwaiter()
                .GetResult();

            // Assert
            provider.Verify(
                value => value.ResolveLeafKeyAsync(target, It.IsAny<CancellationToken>()),
                Times.Once
            );
            provider.Verify(
                value => value.GetAncestorChainAsync(key, It.IsAny<CancellationToken>()),
                Times.Once
            );
            host.Verify(value => value.NavigateToString(It.IsAny<string>()), Times.Once);
            router.SelectedFolderPath.Should().Be(@"\Archive");
        }

        private static FolderTreeNodeKey Key(string path)
        {
            return new FolderTreeNodeKey("archive-store", path, path);
        }

        private static IReadOnlyList<FolderBreadcrumbSegment> Chain(
            string leafPath,
            string middleName,
            string leafName
        )
        {
            int separator = leafPath.LastIndexOf('\\');
            string middlePath = leafPath.Substring(0, separator);
            return new[]
            {
                Segment(@"\Archive", "Archive", true),
                Segment(middlePath, middleName, true),
                Segment(leafPath, leafName, false),
            };
        }

        private static FolderBreadcrumbSegment Segment(string path, string name, bool hasChildren)
        {
            return new FolderBreadcrumbSegment(Key(path), name, path, hasChildren);
        }
    }
}
