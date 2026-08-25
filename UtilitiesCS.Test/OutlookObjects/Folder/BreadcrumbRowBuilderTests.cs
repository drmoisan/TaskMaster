using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Unit tests for <see cref="BreadcrumbRowBuilder"/> (#349): chain-to-row construction
    /// anchored at the leaf, probability join, banner/trash classification, empty and
    /// single-segment chains, and preserved presented order.
    /// </summary>
    [TestClass]
    public class BreadcrumbRowBuilderTests
    {
        private static readonly IReadOnlyDictionary<string, double> NoProbabilities =
            new Dictionary<string, double>();

        private static FolderBreadcrumbSegment ProviderSegment(
            string folderPath,
            string displayName,
            bool hasChildren
        )
        {
            var key = new FolderTreeNodeKey("store-1", "entry-" + displayName, folderPath);
            return new FolderBreadcrumbSegment(key, displayName, folderPath, hasChildren);
        }

        private static IReadOnlyList<FolderBreadcrumbSegment> InboxProjectsAlphaChain()
        {
            return new[]
            {
                ProviderSegment(@"Inbox", "Inbox", true),
                ProviderSegment(@"Inbox\Projects", "Projects", true),
                ProviderSegment(@"Inbox\Projects\Alpha", "Alpha", false),
            };
        }

        [TestMethod]
        public void BuildRow_WithAncestorChain_MapsSegmentsRootToLeafAnchoredAtLeaf()
        {
            // Arrange: a three-segment root-to-leaf 9101 chain.
            var builder = new BreadcrumbRowBuilder();

            // Act
            var row = builder.BuildRow(
                "row-0",
                @"Inbox\Projects\Alpha",
                InboxProjectsAlphaChain(),
                NoProbabilities
            );

            // Assert: order preserved, leaf anchored last, members mapped
            // (FolderPath->FullPath, HasChildren->HasSubfolders).
            row.Kind.Should().Be(BreadcrumbRowKind.Suggestion);
            row.Segments.Select(s => s.DisplayName)
                .Should()
                .ContainInOrder("Inbox", "Projects", "Alpha");
            row.LeafSegment!.FullPath.Should().Be(@"Inbox\Projects\Alpha");
            row.LeafSegment.HasSubfolders.Should().BeFalse();
            row.Segments[0].HasSubfolders.Should().BeTrue();
        }

        [TestMethod]
        public void BuildRow_WithMatchingProbability_JoinsByFullPathEquality()
        {
            // Arrange: probability keyed by the exact leaf full path.
            var builder = new BreadcrumbRowBuilder();
            var probabilities = new Dictionary<string, double> { [@"Inbox\Projects\Alpha"] = 0.87 };

            // Act
            var row = builder.BuildRow(
                "row-0",
                @"Inbox\Projects\Alpha",
                InboxProjectsAlphaChain(),
                probabilities
            );

            // Assert
            row.Probability.Should().Be(0.87);
        }

        [TestMethod]
        public void BuildRow_WithUnmatchedProbability_LeavesProbabilityNull()
        {
            // Arrange: probability dictionary keyed by a different path.
            var builder = new BreadcrumbRowBuilder();
            var probabilities = new Dictionary<string, double> { [@"Inbox\Other"] = 0.5 };

            // Act
            var row = builder.BuildRow(
                "row-0",
                @"Inbox\Projects\Alpha",
                InboxProjectsAlphaChain(),
                probabilities
            );

            // Assert
            row.Probability.Should().BeNull();
        }

        [TestMethod]
        public void BuildRows_WithNoScores_LeavesEveryProbabilityNull()
        {
            // Arrange: no scores supplied at all.
            var builder = new BreadcrumbRowBuilder();

            // Act
            var rows = builder.BuildRows(
                new[] { @"Inbox\Projects\Alpha" },
                _ => InboxProjectsAlphaChain(),
                Enumerable.Empty<FolderScore>()
            );

            // Assert
            rows.Single().Probability.Should().BeNull();
        }

        [TestMethod]
        public void BuildRow_WithBannerText_ClassifiesAsNonInteractiveBanner()
        {
            // Arrange
            var builder = new BreadcrumbRowBuilder();

            // Act
            var row = builder.BuildRow(
                "row-0",
                "========= SUGGESTIONS =========",
                null,
                NoProbabilities
            );

            // Assert: banner kind, no probability, banner text carried for rendering.
            row.Kind.Should().Be(BreadcrumbRowKind.Banner);
            row.Probability.Should().BeNull();
            row.Segments.Single().DisplayName.Should().Be("========= SUGGESTIONS =========");
        }

        [TestMethod]
        public void BuildRow_WithTrashText_ClassifiesAsTrashPseudoRowWithoutSegments()
        {
            // Arrange
            var builder = new BreadcrumbRowBuilder();

            // Act
            var row = builder.BuildRow(
                "row-0",
                BreadcrumbRowBuilder.TrashRowText,
                null,
                NoProbabilities
            );

            // Assert: selectable pseudo-row with no segments and no affordance data.
            row.Kind.Should().Be(BreadcrumbRowKind.TrashPseudoRow);
            row.Segments.Should().BeEmpty();
            row.Probability.Should().BeNull();
        }

        [TestMethod]
        public void BuildRow_WithEmptyChain_FallsBackToSingleLeafOnlySegment()
        {
            // Arrange: the provider does not know this path (empty chain).
            var builder = new BreadcrumbRowBuilder();

            // Act
            var row = builder.BuildRow(
                "row-0",
                @"Inbox\Projects\Alpha",
                Array.Empty<FolderBreadcrumbSegment>(),
                NoProbabilities
            );

            // Assert: presented path survives as a single non-expandable leaf segment.
            row.Kind.Should().Be(BreadcrumbRowKind.Suggestion);
            row.Segments.Should().HaveCount(1);
            row.Segments[0].FullPath.Should().Be(@"Inbox\Projects\Alpha");
            row.Segments[0].DisplayName.Should().Be("Alpha");
            row.Segments[0].HasSubfolders.Should().BeFalse();
        }

        [TestMethod]
        public void BuildRow_WithSingleSegmentChain_ProducesSingleSegmentRow()
        {
            // Arrange: a root-level suggestion (chain of one).
            var builder = new BreadcrumbRowBuilder();
            var chain = new[] { ProviderSegment("Inbox", "Inbox", true) };

            // Act
            var row = builder.BuildRow("row-0", "Inbox", chain, NoProbabilities);

            // Assert
            row.Segments.Should().HaveCount(1);
            row.LeafSegment!.DisplayName.Should().Be("Inbox");
            row.LeafSegment.HasSubfolders.Should().BeTrue();
        }

        [TestMethod]
        public void BuildRows_WithMixedPresentedRows_PreservesPresentedOrderAndAssignsRowIds()
        {
            // Arrange: banner + suggestion + trash in a specific presented order.
            var builder = new BreadcrumbRowBuilder();
            var presented = new[]
            {
                BreadcrumbRowBuilder.TrashRowText,
                "========= SUGGESTIONS =========",
                @"Inbox\Projects\Alpha",
            };

            // Act
            var rows = builder.BuildRows(
                presented,
                path =>
                    path == @"Inbox\Projects\Alpha"
                        ? InboxProjectsAlphaChain()
                        : (IReadOnlyList<FolderBreadcrumbSegment>)null,
                new[] { new FolderScore(@"Inbox\Projects\Alpha", 1000, 0.75) }
            );

            // Assert: order and ids preserved; probability joined to the suggestion row.
            rows.Should().HaveCount(3);
            rows[0].Kind.Should().Be(BreadcrumbRowKind.TrashPseudoRow);
            rows[1].Kind.Should().Be(BreadcrumbRowKind.Banner);
            rows[2].Kind.Should().Be(BreadcrumbRowKind.Suggestion);
            rows.Select(r => r.RowId).Should().ContainInOrder("row-0", "row-1", "row-2");
            rows[2].Probability.Should().Be(0.75);
        }

        [TestMethod]
        public void BuildRows_WithNullPresentedRows_ThrowsArgumentNullException()
        {
            // Arrange
            var builder = new BreadcrumbRowBuilder();

            // Act
            Action act = () => builder.BuildRows(null, _ => null, Enumerable.Empty<FolderScore>());

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("presentedRows");
        }

        [TestMethod]
        public void MapSegments_WithNullChain_ReturnsEmptyList()
        {
            // Act
            var mapped = BreadcrumbRowBuilder.MapSegments(null);

            // Assert
            mapped.Should().BeEmpty();
        }

        [TestMethod]
        public void MapSegments_WithNullElement_ThrowsArgumentException()
        {
            // Act: a chain containing a null segment violates the provider contract.
            Action act = () =>
                BreadcrumbRowBuilder.MapSegments(new FolderBreadcrumbSegment[] { null });

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*null segments*");
        }

        [TestMethod]
        public void BuildRow_WithNullArguments_ThrowsArgumentNullException()
        {
            // Arrange
            var builder = new BreadcrumbRowBuilder();

            // Act / Assert: each required argument fails fast with its parameter name.
            ((Action)(() => builder.BuildRow(null, "Inbox", null, NoProbabilities)))
                .Should()
                .Throw<ArgumentNullException>()
                .WithParameterName("rowId");
            ((Action)(() => builder.BuildRow("row-0", null, null, NoProbabilities)))
                .Should()
                .Throw<ArgumentNullException>()
                .WithParameterName("presentedText");
            ((Action)(() => builder.BuildRow("row-0", "Inbox", null, null)))
                .Should()
                .Throw<ArgumentNullException>()
                .WithParameterName("probabilityByPath");
        }

        [TestMethod]
        public void Classify_WithNullText_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => BreadcrumbRowBuilder.Classify(null);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("presentedText");
        }

        [TestMethod]
        public void BuildRows_WithNullLookupOrScores_ThrowsArgumentNullException()
        {
            // Arrange
            var builder = new BreadcrumbRowBuilder();

            // Act / Assert
            (
                (Action)(
                    () =>
                        builder.BuildRows(new[] { "Inbox" }, null, Enumerable.Empty<FolderScore>())
                )
            )
                .Should()
                .Throw<ArgumentNullException>()
                .WithParameterName("ancestorChainLookup");
            ((Action)(() => builder.BuildRows(new[] { "Inbox" }, _ => null, null)))
                .Should()
                .Throw<ArgumentNullException>()
                .WithParameterName("scores");
        }

        [TestMethod]
        public void BuildRows_WithNullPresentedEntry_CoercesToEmptySuggestion()
        {
            // Arrange
            var builder = new BreadcrumbRowBuilder();

            // Act: a null presented entry is coerced to an empty suggestion row.
            var rows = builder.BuildRows(
                new string[] { null },
                _ => null,
                Enumerable.Empty<FolderScore>()
            );

            // Assert
            rows.Single().Kind.Should().Be(BreadcrumbRowKind.Suggestion);
            rows.Single().Segments.Single().FullPath.Should().BeEmpty();
        }

        [TestMethod]
        public void BuildRow_WithTrailingSeparatorPath_DerivesLeafToken()
        {
            // Arrange: fallback single-segment path ends with a separator.
            var builder = new BreadcrumbRowBuilder();

            // Act
            var row = builder.BuildRow("row-0", "Inbox\\Projects\\", null, NoProbabilities);

            // Assert
            row.Segments.Single().DisplayName.Should().Be("Projects");
        }

        [TestMethod]
        public void BuildRows_WithEmptyScorePath_SkipsScoreIndexEntry()
        {
            // Arrange: an empty FolderScore path must not join to any row.
            var builder = new BreadcrumbRowBuilder();

            // Act
            var rows = builder.BuildRows(
                new[] { "Inbox" },
                _ => null,
                new[] { new FolderScore(string.Empty, 10, 0.4) }
            );

            // Assert
            rows.Single().Probability.Should().BeNull();
        }

        [TestMethod]
        public void Issue439ResolvedFullHierarchyRetainsOriginalFilingTargetAndScore()
        {
            // Arrange: hierarchy paths are archive-rooted while the presented filing target and
            // score key remain archive-relative.
            const string filingTarget = @"Clients\North";
            var builder = new BreadcrumbRowBuilder();
            var chain = new[]
            {
                ProviderSegment(@"\Archive", "Archive", true),
                ProviderSegment(@"\Archive\Clients", "Clients", true),
                ProviderSegment(@"\Archive\Clients\North", "North", false),
            };

            // Act
            var row = builder.BuildRow(
                "row-439",
                filingTarget,
                chain,
                new Dictionary<string, double> { [filingTarget] = 0.73 }
            );

            // Assert
            row.FilingTarget.Should().Be(filingTarget);
            row.Probability.Should().Be(0.73);
            row.Segments.Select(segment => segment.DisplayName)
                .Should()
                .ContainInOrder("Archive", "Clients", "North");
            row.LeafSegment.FullPath.Should().Be(@"\Archive\Clients\North");
        }
    }
}
