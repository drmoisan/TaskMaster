using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Test.Controllers
{
    /// <summary>
    /// Issue 1002 stale-completion guards: a provider task held open on a
    /// <see cref="TaskCompletionSource{TResult}"/> barrier completes after a collapse, a rebind, or a
    /// later toggle, and the router must attach and post nothing for the superseded request. The
    /// same guard covers the existing first-level leaf expansion.
    /// </summary>
    [TestClass]
    public class BreadcrumbBridgeRouterStaleCompletionIssue1002Tests
    {
        private EfcDescendantRouterFixture fixture;

        [TestInitialize]
        public void Setup()
        {
            fixture = new EfcDescendantRouterFixture();
        }

        [TestMethod]
        public async Task DescendantFetch_CompletesAfterCollapse_AttachesNothing()
        {
            // Arrange
            string q3 = await fixture.BindAndExpandAsync(EfcDescendantRouterFixture.Q3);
            var barrier = HoldFetch(EfcDescendantRouterFixture.Q3);
            Task pending = fixture.ToggleAsync("row-0", q3);
            await fixture.ToggleAsync("row-0", q3);
            int before = fixture.Posts.Count;

            // Act
            barrier.SetResult(SecondLevel());
            await pending;

            // Assert
            fixture.Posts.Count.Should().Be(before, "the collapsed node rejects the late result");
            fixture.LatestRender().Should().NotContain("Invoices");
        }

        [TestMethod]
        public async Task DescendantFetch_CompletesAfterRebind_PostsNothingForReplacedRow()
        {
            // Arrange
            string q3 = await fixture.BindAndExpandAsync(EfcDescendantRouterFixture.Q3);
            var barrier = HoldFetch(EfcDescendantRouterFixture.Q3);
            Task pending = fixture.ToggleAsync("row-0", q3);
            await fixture.BindAsync(EfcDescendantRouterFixture.Presented);
            int before = fixture.Posts.Count;

            // Act
            barrier.SetResult(SecondLevel());
            await pending;

            // Assert
            fixture.Posts.Count.Should().Be(before, "row-0 now names a different row instance");
        }

        [TestMethod]
        public async Task DescendantFetch_ReverseCompletion_OnlyLatestGenerationApplies()
        {
            // Arrange
            string q3 = await fixture.BindAndExpandAsync(EfcDescendantRouterFixture.Q3);
            var first = new TaskCompletionSource<IReadOnlyList<FolderBreadcrumbSegment>>();
            var second = new TaskCompletionSource<IReadOnlyList<FolderBreadcrumbSegment>>();
            fixture
                .Provider.SetupSequence(p =>
                    p.GetImmediateSubfoldersAsync(
                        EfcDescendantRouterFixture.Key(EfcDescendantRouterFixture.Q3),
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(first.Task)
                .Returns(second.Task);
            Task firstToggle = fixture.ToggleAsync("row-0", q3);
            await fixture.ToggleAsync("row-0", q3);
            Task secondToggle = fixture.ToggleAsync("row-0", q3);

            // Act
            second.SetResult(SecondLevel());
            await secondToggle;
            int afterLatest = fixture.Posts.Count;
            first.SetResult(
                new[]
                {
                    EfcDescendantRouterFixture.Segment(
                        EfcDescendantRouterFixture.Q3 + @"\Stale",
                        "Stale",
                        false
                    ),
                }
            );
            await firstToggle;

            // Assert
            fixture.Posts.Count.Should().Be(afterLatest, "the superseded result posts nothing");
            string html = fixture.LatestRender();
            html.Should().Contain(">Invoices</span>");
            html.Should().NotContain("Stale");
        }

        [TestMethod]
        public async Task DescendantFetch_ProviderFault_LeavesStateUnchangedAndAllowsRetry()
        {
            // Arrange
            string q3 = await fixture.BindAndExpandAsync(EfcDescendantRouterFixture.Q3);
            fixture
                .Provider.SetupSequence(p =>
                    p.GetImmediateSubfoldersAsync(
                        EfcDescendantRouterFixture.Key(EfcDescendantRouterFixture.Q3),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ThrowsAsync(new InvalidOperationException("provider fault"))
                .ReturnsAsync(SecondLevel());
            int before = fixture.Posts.Count;

            // Act
            Func<Task> faulted = () => fixture.ToggleAsync("row-0", q3);

            // Assert
            await faulted.Should().NotThrowAsync("the provider fault is logged at the boundary");
            fixture.Posts.Count.Should().Be(before, "a faulted fetch posts nothing");
            await fixture.ToggleAsync("row-0", q3);
            fixture.Posts.Count.Should().Be(before + 1, "the retry renders the children");
            fixture.LatestRender().Should().Contain(">Invoices</span>");
        }

        [TestMethod]
        public async Task DescendantFetch_Canceled_LeavesStateUnchanged()
        {
            // Arrange
            string q3 = await fixture.BindAndExpandAsync(EfcDescendantRouterFixture.Q3);
            fixture
                .Provider.SetupSequence(p =>
                    p.GetImmediateSubfoldersAsync(
                        EfcDescendantRouterFixture.Key(EfcDescendantRouterFixture.Q3),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ThrowsAsync(new OperationCanceledException())
                .ReturnsAsync(SecondLevel());
            string renderBefore = fixture.LatestRender();
            int before = fixture.Posts.Count;

            // Act
            await fixture.ToggleAsync("row-0", q3);

            // Assert
            fixture.Posts.Count.Should().Be(before, "a canceled fetch posts nothing");
            fixture.Router.SelectedFolderPath.Should().BeNull();
            await fixture.ToggleAsync("row-0", q3);
            fixture.Provider.Verify(
                p =>
                    p.GetImmediateSubfoldersAsync(
                        EfcDescendantRouterFixture.Key(EfcDescendantRouterFixture.Q3),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Exactly(2),
                "the node was left collapsed, so the next toggle fetches again"
            );
            renderBefore.Should().Contain("aria-expanded=\"false\"");
        }

        [TestMethod]
        public async Task ExpandLeaf_CompletesAfterRebind_PostsNothingForReplacedRow()
        {
            // Arrange
            await fixture.BindAsync(EfcDescendantRouterFixture.Presented);
            var barrier = HoldFetch(EfcDescendantRouterFixture.North);
            Task pending = fixture.InboundAsync("leafExpandToggle", "row-0");
            await fixture.BindAsync(EfcDescendantRouterFixture.Presented);
            int before = fixture.Posts.Count;

            // Act
            barrier.SetResult(
                new[]
                {
                    EfcDescendantRouterFixture.Segment(EfcDescendantRouterFixture.Q3, "Q3", true),
                }
            );
            await pending;

            // Assert
            fixture.Posts.Count.Should().Be(before, "the replaced row receives no post");
            fixture
                .Posts.Skip(before)
                .Should()
                .NotContain(post => post.Contains("subfolderResult"));
        }

        [TestMethod]
        public async Task ExpandLeaf_CurrentRow_StillPostsSubfolderResultThenRender()
        {
            // Arrange
            await fixture.BindAsync(EfcDescendantRouterFixture.Presented);
            var barrier = HoldFetch(EfcDescendantRouterFixture.North);
            Task pending = fixture.InboundAsync("leafExpandToggle", "row-0");
            int before = fixture.Posts.Count;

            // Act
            barrier.SetResult(
                new[]
                {
                    EfcDescendantRouterFixture.Segment(EfcDescendantRouterFixture.Q3, "Q3", true),
                }
            );
            await pending;

            // Assert
            List<string> posted = fixture.Posts.Skip(before).ToList();
            posted
                .Should()
                .HaveCount(2, "the current row keeps the subfolderResult-then-render pair");
            posted[0]
                .Should()
                .Contain("\"type\":\"subfolderResult\"")
                .And.Contain("\"rowId\":\"row-0\"");
            posted[1].Should().Contain("\"type\":\"render\"").And.Contain("\"rowId\":\"row-0\"");
        }

        private TaskCompletionSource<IReadOnlyList<FolderBreadcrumbSegment>> HoldFetch(string path)
        {
            var barrier = new TaskCompletionSource<IReadOnlyList<FolderBreadcrumbSegment>>();
            fixture
                .Provider.Setup(p =>
                    p.GetImmediateSubfoldersAsync(
                        EfcDescendantRouterFixture.Key(path),
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(barrier.Task);
            return barrier;
        }

        private static FolderBreadcrumbSegment[] SecondLevel() =>
            new[]
            {
                EfcDescendantRouterFixture.Segment(
                    EfcDescendantRouterFixture.Invoices,
                    "Invoices",
                    false
                ),
                EfcDescendantRouterFixture.Segment(
                    EfcDescendantRouterFixture.Receipts,
                    "Receipts",
                    true
                ),
            };
    }
}
