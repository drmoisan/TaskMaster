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
    /// Headless issue #799 regression tests for the breadcrumb router's score join (AC6), the
    /// archive-relative filing invariant under a trimmed chain (AC3), and the zero-candidate row
    /// suppression (AC7). Every collaborator is a Moq seam or a real pure type; this file creates
    /// no WebView2 control, no Outlook COM object, and no message pump.
    /// <para>
    /// Authored for C# 7.3. QuickFiler.Test declares no LangVersion element and targets v4.8.1, so
    /// it compiles at the 7.3 default while every other project in scope is at Latest, preview, or
    /// 12.0. Do not introduce target-typed <c>new</c>, <c>is not null</c>, switch expressions, or
    /// nullable reference annotations here: they surface as CS8370 at build time, not at edit time.
    /// </para>
    /// </summary>
    [TestClass]
    public sealed class BreadcrumbBridgeRouterScoreJoinTests
    {
        private const string ArchiveRoot = @"\Archive";
        private const string RelativeTarget = @"Clients\North";
        private const string RootedTarget = @"\Archive\Clients\North";
        private const string ClientsPath = @"\Archive\Clients";

        /// <summary>
        /// AC6: the Efc surface hands the router the RAW score paths, which are archive-rooted,
        /// while the presented rows are archive-relative stems. The join must still find the score,
        /// or an archive-rooted suggestion silently loses its percentage.
        /// </summary>
        [TestMethod]
        public void BindRowsAsync_RootedScoreAndRelativeRow_RendersThePercentage()
        {
            // Arrange
            var documents = new List<string>();
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            var host = StrictHost(documents);
            FolderTreeNodeKey key = Key(RootedTarget);
            SetupChain(provider, RootedTarget, key, TwoSegmentChain(ClientsPath, "Clients",
                RootedTarget, "North"));
            var router = RouterOver(provider, host);

            // Act: presented row is the stem; the score carries the rooted path.
            router
                .BindRowsAsync(
                    new[] { RelativeTarget },
                    new[] { new FolderScore(RootedTarget, 730, 0.73) },
                    ArchiveRoot,
                    CancellationToken.None
                )
                .GetAwaiter()
                .GetResult();

            // Assert
            documents.Should().ContainSingle();
            documents[0].Should().Contain("73%", "an archive-rooted score must still join");
        }

        /// <summary>
        /// Decision D7: the projected score key is ADDED alongside the raw key, never substituted
        /// for it. A substitution would fix the stem-presented case and silently break this one.
        /// </summary>
        [TestMethod]
        public void BindRowsAsync_RootedScoreAndRootedRow_StillRendersThePercentage()
        {
            // Arrange
            var documents = new List<string>();
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            var host = StrictHost(documents);
            FolderTreeNodeKey key = Key(RootedTarget);
            SetupChain(provider, RootedTarget, key, TwoSegmentChain(ClientsPath, "Clients",
                RootedTarget, "North"));
            var router = RouterOver(provider, host);

            // Act: presented row and score both carry the rooted path.
            router
                .BindRowsAsync(
                    new[] { RootedTarget },
                    new[] { new FolderScore(RootedTarget, 730, 0.73) },
                    ArchiveRoot,
                    CancellationToken.None
                )
                .GetAwaiter()
                .GetResult();

            // Assert
            documents.Should().ContainSingle();
            documents[0].Should().Contain("73%", "the rooted-presented case must not regress");
        }

        /// <summary>
        /// The public three-argument overload forwards an empty archive root, so the projection is
        /// the identity and no existing caller changes behaviour.
        /// </summary>
        [TestMethod]
        public void BindRowsAsync_EmptyBoundRoot_LeavesTheJoinUnchanged()
        {
            // Arrange
            var documents = new List<string>();
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            var host = StrictHost(documents);
            FolderTreeNodeKey key = Key(RelativeTarget);
            SetupChain(provider, RelativeTarget, key, TwoSegmentChain(@"Clients", "Clients",
                RelativeTarget, "North"));
            var router = RouterOver(provider, host);

            // Act: the public overload, which supplies no archive root at all.
            router
                .BindRowsAsync(
                    new[] { RelativeTarget },
                    new[] { new FolderScore(RelativeTarget, 730, 0.73) },
                    CancellationToken.None
                )
                .GetAwaiter()
                .GetResult();

            // Assert
            documents.Should().ContainSingle();
            documents[0].Should().Contain("73%");
        }

        /// <summary>
        /// AC3, pinned as its own criterion rather than as an incidental consequence of the trim:
        /// with an ancestor chain that begins BELOW the archive root, the filing target and the
        /// joined score key are both still the archive-relative stem.
        /// </summary>
        [TestMethod]
        public void BindRowsAsync_TrimmedChain_PreservesFilingTargetAndScoreKey()
        {
            // Arrange: the chain carries no store segment and no Archive segment.
            var documents = new List<string>();
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            var host = StrictHost(documents);
            FolderTreeNodeKey key = Key(RootedTarget);
            SetupChain(provider, RootedTarget, key, TwoSegmentChain(ClientsPath, "Clients",
                RootedTarget, "North"));
            var router = RouterOver(provider, host);

            // Act
            router
                .BindRowsAsync(
                    new[] { RelativeTarget },
                    new[] { new FolderScore(RootedTarget, 730, 0.73) },
                    ArchiveRoot,
                    CancellationToken.None
                )
                .GetAwaiter()
                .GetResult();
            router
                .ProcessInboundAsync("{\"type\":\"rowSelected\",\"rowId\":\"row-0\"}")
                .GetAwaiter()
                .GetResult();

            // Assert
            router
                .SelectedFolderPath.Should()
                .Be(RelativeTarget, "the filing target stays archive-relative (#439)");
            documents[documents.Count - 1]
                .Should()
                .Contain("73%", "the score key stays joined to the archive-relative stem");
        }

        /// <summary>
        /// The spec's integration scenario, driven entirely through the router: a banner row, a
        /// suggestion row, a search-result row, the trash pseudo-row, and one stale label that the
        /// provider cannot resolve. Lineage renders on both folder row kinds only.
        /// </summary>
        [TestMethod]
        public void BindRowsAsync_MixedRowSet_RendersLineageOnFolderRowsOnly()
        {
            // Arrange
            const string searchTarget = @"Search\Follow Up";
            const string searchRooted = @"\Archive\Search\Follow Up";
            const string staleTarget = @"Clients\Stale";
            var documents = new List<string>();
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            var host = StrictHost(documents);
            FolderTreeNodeKey suggestionKey = Key(RootedTarget);
            FolderTreeNodeKey searchKey = Key(searchRooted);
            SetupChain(provider, RootedTarget, suggestionKey, TwoSegmentChain(ClientsPath,
                "Clients", RootedTarget, "North"));
            SetupChain(provider, searchRooted, searchKey, TwoSegmentChain(@"\Archive\Search",
                "Search", searchRooted, "Follow Up"));
            provider
                .Setup(p =>
                    p.ResolveLeafKeyAsync(@"\Archive\Clients\Stale", It.IsAny<CancellationToken>())
                )
                .ReturnsAsync((FolderTreeNodeKey)null);
            var router = RouterOver(provider, host);

            // Act
            router
                .BindRowsAsync(
                    new[]
                    {
                        "==== SUGGESTIONS ====",
                        RelativeTarget,
                        searchTarget,
                        "Trash to Delete",
                        staleTarget,
                    },
                    new[] { new FolderScore(RootedTarget, 730, 0.73) },
                    ArchiveRoot,
                    CancellationToken.None
                )
                .GetAwaiter()
                .GetResult();

            // Assert: exactly two rows render an ancestor separator, and they are the two folder
            // row kinds; the banner, the trash pseudo-row and the stale label render none.
            string document = documents[0];
            Occurrences(document, "class=\"sep\"")
                .Should()
                .Be(2, "lineage renders on the suggestion and search rows only");
            document.Should().Contain("title=\"" + ClientsPath + "\"");
            document.Should().Contain("title=\"\\Archive\\Search\"");
            document.Should().Contain("row banner");
            document.Should().Contain("row selectable trash");
            document.Should().Contain(">Stale<", "the stale label keeps the leaf-only fallback");
        }

        /// <summary>
        /// AC7 row half, and the only test here that takes the TRUE arm of the suppression
        /// predicate. The suppressed row sits in the MIDDLE of the presented sequence, so a
        /// suppression that removed the row from the built list without removing it from the
        /// presented list would misalign every later row's segment keys.
        /// </summary>
        [TestMethod]
        public void BindRowsAsync_ZeroCandidateLabel_SuppressesTheRowAndKeepsSegmentKeysAligned()
        {
            // Arrange
            const string vendorsTarget = @"Vendors\South";
            const string vendorsRooted = @"\Archive\Vendors\South";
            const string vendorsPath = @"\Archive\Vendors";
            const string staleTarget = @"Clients\Stale";
            const string staleRooted = @"\Archive\Clients\Stale";
            var documents = new List<string>();
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);

            // Moq throws when an interface is added to a mock whose object already exists, so the
            // As<> call precedes every use of provider.Object.
            var absence = provider.As<IFolderLabelAbsenceReport>();
            absence.Setup(a => a.IsAbsentLabel(It.IsAny<string>())).Returns(false);
            absence.Setup(a => a.IsAbsentLabel(staleRooted)).Returns(true);
            absence.Setup(a => a.IsAbsentLabel(staleTarget)).Returns(true);

            var host = StrictHost(documents);
            FolderTreeNodeKey northKey = Key(RootedTarget);
            FolderTreeNodeKey southKey = Key(vendorsRooted);
            FolderTreeNodeKey vendorsKey = Key(vendorsPath);
            SetupChain(provider, RootedTarget, northKey, TwoSegmentChain(ClientsPath, "Clients",
                RootedTarget, "North"));
            SetupChain(provider, vendorsRooted, southKey, TwoSegmentChain(vendorsPath, "Vendors",
                vendorsRooted, "South"));
            provider
                .Setup(p => p.ResolveLeafKeyAsync(staleRooted, It.IsAny<CancellationToken>()))
                .ReturnsAsync((FolderTreeNodeKey)null);
            provider
                .Setup(p => p.GetImmediateSubfoldersAsync(vendorsKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { Segment(vendorsRooted, "South", false) });
            var router = RouterOver(provider, host);

            // Act
            router
                .BindRowsAsync(
                    new[] { "==== SUGGESTIONS ====", RelativeTarget, staleTarget, vendorsTarget },
                    new[] { new FolderScore(RootedTarget, 730, 0.73) },
                    ArchiveRoot,
                    CancellationToken.None
                )
                .GetAwaiter()
                .GetResult();
            router
                .ProcessInboundAsync(
                    "{\"type\":\"segmentActivate\",\"rowId\":\"row-2\",\"segmentIndex\":0}"
                )
                .GetAwaiter()
                .GetResult();
            router
                .ProcessInboundAsync("{\"type\":\"leafExpandToggle\",\"rowId\":\"row-2\"}")
                .GetAwaiter()
                .GetResult();

            // Assert
            string document = documents[0];
            document.Should().NotContain("Stale", "the zero-candidate label is suppressed");
            Occurrences(document, "data-row-id=\"row-")
                .Should()
                .Be(3, "one presented row of four was suppressed");
            provider.Verify(
                p => p.GetImmediateSubfoldersAsync(vendorsKey, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        /// <summary>
        /// Decision D-B restricts suppression to the zero-candidate cause. An ambiguous label also
        /// yields a null chain, but it is not absent and must still render with today's fallback.
        /// </summary>
        [TestMethod]
        public void BindRowsAsync_AmbiguousLabel_IsNotSuppressed()
        {
            // Arrange
            const string ambiguousTarget = @"Clients\Stale";
            const string ambiguousRooted = @"\Archive\Clients\Stale";
            var documents = new List<string>();
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            var absence = provider.As<IFolderLabelAbsenceReport>();
            absence.Setup(a => a.IsAbsentLabel(It.IsAny<string>())).Returns(false);

            var host = StrictHost(documents);
            FolderTreeNodeKey northKey = Key(RootedTarget);
            SetupChain(provider, RootedTarget, northKey, TwoSegmentChain(ClientsPath, "Clients",
                RootedTarget, "North"));
            provider
                .Setup(p => p.ResolveLeafKeyAsync(ambiguousRooted, It.IsAny<CancellationToken>()))
                .ReturnsAsync((FolderTreeNodeKey)null);
            var router = RouterOver(provider, host);

            // Act
            router
                .BindRowsAsync(
                    new[] { RelativeTarget, ambiguousTarget },
                    new[] { new FolderScore(RootedTarget, 730, 0.73) },
                    ArchiveRoot,
                    CancellationToken.None
                )
                .GetAwaiter()
                .GetResult();

            // Assert
            string document = documents[0];
            document.Should().Contain(">Stale<", "ambiguity is not absence");
            Occurrences(document, "data-row-id=\"row-").Should().Be(2);
        }

        private static BreadcrumbBridgeRouter RouterOver(
            Mock<IFolderHierarchyProvider> provider,
            Mock<IBreadcrumbWebHost> host
        )
        {
            return new BreadcrumbBridgeRouter(
                provider.Object,
                host.Object,
                new BreadcrumbMessageCodec(),
                new BreadcrumbHtmlRenderer(),
                new BreadcrumbOutboundQueue(host.Object)
            );
        }

        private static Mock<IBreadcrumbWebHost> StrictHost(List<string> documents)
        {
            var host = new Mock<IBreadcrumbWebHost>(MockBehavior.Strict);
            host.SetupGet(h => h.IsCoreInitialized).Returns(true);
            host.Setup(h => h.NavigateToString(It.IsAny<string>()))
                .Callback<string>(html => documents.Add(html));
            host.Setup(h => h.PostMessageJson(It.IsAny<string>()));
            return host;
        }

        private static void SetupChain(
            Mock<IFolderHierarchyProvider> provider,
            string hierarchyPath,
            FolderTreeNodeKey key,
            IReadOnlyList<FolderBreadcrumbSegment> chain
        )
        {
            provider
                .Setup(p => p.ResolveLeafKeyAsync(hierarchyPath, It.IsAny<CancellationToken>()))
                .ReturnsAsync(key);
            provider
                .Setup(p => p.GetAncestorChainAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(chain);
        }

        /// <summary>
        /// An ancestor chain that already begins below the archive root, which is the shape the
        /// provider returns once the #799 trim is in place.
        /// </summary>
        private static IReadOnlyList<FolderBreadcrumbSegment> TwoSegmentChain(
            string parentPath,
            string parentName,
            string leafPath,
            string leafName
        )
        {
            return new[]
            {
                Segment(parentPath, parentName, true),
                Segment(leafPath, leafName, false),
            };
        }

        private static FolderBreadcrumbSegment Segment(string path, string name, bool hasChildren)
        {
            return new FolderBreadcrumbSegment(Key(path), name, path, hasChildren);
        }

        private static FolderTreeNodeKey Key(string path)
        {
            return new FolderTreeNodeKey("archive-store", path, path);
        }

        private static int Occurrences(string haystack, string needle)
        {
            int count = 0;
            int index = haystack.IndexOf(needle, StringComparison.Ordinal);
            while (index >= 0)
            {
                count++;
                index = haystack.IndexOf(needle, index + needle.Length, StringComparison.Ordinal);
            }

            return count;
        }
    }
}
