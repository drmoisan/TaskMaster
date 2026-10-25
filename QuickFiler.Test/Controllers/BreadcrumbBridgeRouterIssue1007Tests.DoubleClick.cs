using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookObjects.Folder;
using F = QuickFiler.Test.Controllers.EfcDescendantRouterFixture;

namespace QuickFiler.Test.Controllers
{
    /// <summary>
    /// Issue #1007 AC9 and AC5 (S10, D-Q13, D-Q18, D-Q23): a segment double-click on the
    /// three-segment row <c>Clients &gt; Acme &gt; Beta</c> (below the bound archive root)
    /// activates, commits and shortens the row at Acme; the [+] or Right then lists every child of
    /// Acme instead of restoring Beta, and the next Right moves to the first child.
    /// </summary>
    public sealed partial class BreadcrumbBridgeRouterIssue1007Tests
    {
        private const string Acme = F.Root + @"\Clients\Acme";
        private const string Beta = Acme + @"\Beta";

        /// <summary>AC9: the double-click selects the segment's folder and shortens the row there.</summary>
        [TestMethod]
        public async Task SegmentDoubleClick_ActivatesCommitsAndShortensAtomically()
        {
            // Arrange
            var fixture = await ThreeSegmentAsync();
            await SelectAsync(fixture, "row-0");

            // Act
            await DoubleClickAsync(fixture, 1);

            // Assert
            fixture.Router.SelectedFolderPath.Should().Be(@"Clients\Acme");
            string row = fixture.LatestRender();
            row.Should().Contain("data-role=\"reexpand\"");
            row.Should().NotContain("data-segment-index=\"2\"", "Beta is hidden");
        }

        /// <summary>AC9: the [+] on the shortened row lists Acme's children; Beta stays hidden.</summary>
        [TestMethod]
        public async Task LeafExpandToggle_OnShortenedRow_ListsSegmentChildren()
        {
            // Arrange
            var fixture = await ThreeSegmentAsync();
            await DoubleClickAsync(fixture, 1);

            // Act
            await ExpandAsync(fixture, "row-0");

            // Assert
            VerifyFetched(fixture, Acme, Times.Once());
            string row = fixture.LatestRender();
            row.Should().Contain(">Alpha</span>");
            row.Should().NotContain("data-segment-index=\"2\"");
        }

        /// <summary>AC5: Right on the shortened row lists the segment's children with one query.</summary>
        [TestMethod]
        public async Task ArrowRight_OnShortenedRow_ListsSegmentChildrenWithSingleQuery()
        {
            // Arrange
            var fixture = await ThreeSegmentAsync();
            await DoubleClickAsync(fixture, 1);

            // Act
            await KeyAsync(fixture, "row-0", "Right");

            // Assert
            VerifyFetched(fixture, Acme, Times.Once());
            fixture.LatestRender().Should().Contain(">Alpha</span>");
            fixture.LatestRender().Should().NotContain("data-segment-index=\"2\"");
        }

        /// <summary>AC9 (D-Q18): the highlight stays on Acme, and the next Right selects its first child.</summary>
        [TestMethod]
        public async Task ShortenedRowExpanded_NextRight_SelectsFirstChild()
        {
            // Arrange
            var fixture = await ThreeSegmentAsync();
            await DoubleClickAsync(fixture, 1);
            await KeyAsync(fixture, "row-0", "Right");
            fixture.Router.SelectedFolderPath.Should().Be(@"Clients\Acme");

            // Act
            await KeyAsync(fixture, "row-0", "Right");

            // Assert
            fixture.Router.SelectedFolderPath.Should().Be(@"Clients\Acme\Alpha");
        }

        /// <summary>AC9: while the shortened row is expanded, its terminal carries the collapse affordance.</summary>
        [TestMethod]
        public async Task ShortenedRowExpanded_RendersCollapseAffordanceAtTerminal()
        {
            // Arrange
            var fixture = await ThreeSegmentAsync();
            await DoubleClickAsync(fixture, 1);

            // Act
            await KeyAsync(fixture, "row-0", "Right");

            // Assert
            fixture.LatestRender().Should().Contain("data-role=\"reexpand\">&#8722;</span>");
        }

        /// <summary>AC1, AC9: Up from Acme's first child onto the shortened row entry selects Acme again.</summary>
        [TestMethod]
        public async Task ArrowUp_OntoShortenedRowEntry_SelectsActiveSegmentFolder()
        {
            // Arrange: the row is shortened at Acme and its first child Alpha is selected.
            var fixture = await ThreeSegmentAsync();
            await DoubleClickAsync(fixture, 1);
            await KeyAsync(fixture, "row-0", "Right");
            await KeyAsync(fixture, "row-0", "Right");
            fixture.Router.SelectedFolderPath.Should().Be(@"Clients\Acme\Alpha");

            // Act
            await KeyAsync(fixture, "row-0", "Up");

            // Assert
            fixture.Router.SelectedFolderPath.Should().Be(@"Clients\Acme");
        }

        /// <summary>
        /// R3: a click on Clients after the double-click at Acme leaves the row collapsed away from
        /// its active segment; Right then restores the full chain instead of listing children.
        /// </summary>
        [TestMethod]
        public async Task ArrowRight_OnRowCollapsedAwayFromActiveSegment_RestoresChain()
        {
            // Arrange
            var fixture = await ThreeSegmentAsync();
            await DoubleClickAsync(fixture, 1);
            await fixture.Router.ProcessInboundAsync(
                "{\"type\":\"segmentActivate\",\"rowId\":\"row-0\",\"segmentIndex\":0}"
            );

            // Act
            await KeyAsync(fixture, "row-0", "Right");

            // Assert
            fixture
                .LatestRender()
                .Should()
                .Contain("data-segment-index=\"2\"", "Beta is shown again");
            VerifyFetched(fixture, Acme, Times.Never());
        }

        // Binds Clients\Acme\Beta with the chain Clients > Acme > Beta and Acme's children.
        private static async Task<EfcDescendantRouterFixture> ThreeSegmentAsync()
        {
            var fixture = new EfcDescendantRouterFixture();
            fixture.Children[Acme] = new[]
            {
                F.Segment(Acme + @"\Alpha", "Alpha", false),
                F.Segment(Beta, "Beta", false),
            };
            IReadOnlyList<FolderBreadcrumbSegment> chain = new[]
            {
                F.Segment(F.Root + @"\Clients", "Clients", true),
                F.Segment(Acme, "Acme", true),
                F.Segment(Beta, "Beta", false),
            };
            fixture
                .Provider.Setup(p =>
                    p.GetAncestorChainAsync(
                        It.Is<FolderTreeNodeKey>(k => k.FolderPath == Beta),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(chain);
            await fixture.BindAsync(@"Clients\Acme\Beta");
            return fixture;
        }

        private static Task DoubleClickAsync(EfcDescendantRouterFixture fixture, int index) =>
            fixture.Router.ProcessInboundAsync(
                "{\"type\":\"segmentDoubleClick\",\"rowId\":\"row-0\",\"segmentIndex\":"
                    + index
                    + "}"
            );
    }
}
