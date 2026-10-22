using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;

namespace QuickFiler.Test.Controllers
{
    /// <summary>
    /// Issue 1002 trash preselection: a preferred row passed to the router bind is selected
    /// before the document is delivered, so the navigated (or pending) document already marks it
    /// and no follow-up render message is needed.
    /// </summary>
    [TestClass]
    public class BreadcrumbBridgeRouterTrashIssue1002Tests
    {
        private const string Trash = "Trash to Delete";
        private const string TrashRowSelected = "class=\"rowwrap selected\" data-row-id=\"row-0\"";

        private EfcDescendantRouterFixture fixture;

        [TestInitialize]
        public void Setup()
        {
            fixture = new EfcDescendantRouterFixture();
        }

        [TestMethod]
        public async Task BindRows_PreferredTrashRow_SelectsTrashInDeliveredDocument()
        {
            // Act
            await BindWithPreferredAsync(Trash);

            // Assert
            fixture.Router.SelectedFolderPath.Should().Be(Trash);
            fixture.Documents.Should().HaveCount(1, "one document is navigated");
            fixture.Documents[0].Should().Contain(TrashRowSelected);
        }

        [TestMethod]
        public async Task BindRows_PreferredTrashRow_LastSelectionNotificationCarriesTrash()
        {
            // Arrange: an earlier selection makes the bind clear it first (#499).
            await fixture.BindAsync(EfcDescendantRouterFixture.Presented);
            await fixture.InboundAsync("rowSelected", "row-0");
            fixture.SelectionEvents.Clear();

            // Act
            await BindWithPreferredAsync(Trash);

            // Assert
            fixture.SelectionEvents.Count(path => path == Trash).Should().Be(1);
            fixture
                .SelectionEvents.Last()
                .Should()
                .Be(Trash, "the trash selection is the last word");
        }

        [TestMethod]
        public async Task BindRows_PreferredTrashRow_PostsNoFollowUpRender()
        {
            // Arrange
            int before = fixture.Posts.Count;

            // Act
            await BindWithPreferredAsync(Trash);

            // Assert
            fixture.Posts.Count.Should().Be(before, "the selection travels inside the document");
        }

        [TestMethod]
        public async Task BindRows_PreferredRowAbsent_SelectsNothing()
        {
            // Act
            await BindWithPreferredAsync(@"Clients\Missing");

            // Assert
            fixture.Router.SelectedFolderPath.Should().BeNull();
            fixture.SelectionEvents.Should().BeEmpty();
            fixture.Documents.Single().Should().NotContain("rowwrap selected");
        }

        [TestMethod]
        public async Task BindRows_PreferredRowOutsideArchiveRoot_SelectsNothing()
        {
            // Arrange: a rooted target outside the bound archive root is a #614 non-selection.
            const string outside = @"\\other\Mailbox\Outside";

            // Act
            await fixture.Router.BindRowsAsync(
                new[] { outside },
                Array.Empty<FolderScore>(),
                EfcDescendantRouterFixture.Root,
                CancellationToken.None,
                outside
            );

            // Assert
            fixture.Router.SelectedFolderPath.Should().BeNull("the shared row rule refuses it");
            fixture.SelectionEvents.Should().BeEmpty();
            fixture.Documents.Single().Should().NotContain("rowwrap selected");
        }

        [TestMethod]
        public async Task BindRows_NoPreferredRow_KeepsExistingClearBehavior()
        {
            // Arrange
            await fixture.BindAsync(EfcDescendantRouterFixture.Presented);
            await fixture.InboundAsync("rowSelected", "row-0");
            fixture.SelectionEvents.Clear();

            // Act
            await fixture.Router.BindRowsAsync(
                new[] { Trash, EfcDescendantRouterFixture.Presented },
                Array.Empty<FolderScore>(),
                EfcDescendantRouterFixture.Root,
                CancellationToken.None
            );

            // Assert
            fixture.Router.SelectedFolderPath.Should().BeNull("the stale selection is cleared");
            fixture.SelectionEvents.Should().Equal(new string[] { null });
            fixture.Documents.Last().Should().NotContain("rowwrap selected");
        }

        [TestMethod]
        public async Task BindRows_PreferredTrashRowBeforeCoreInit_PendingDocumentCarriesSelection()
        {
            // Arrange
            fixture.CoreInitialized = false;
            await BindWithPreferredAsync(Trash);
            fixture.Documents.Should().BeEmpty("the document waits for core initialization");
            fixture.CoreInitialized = true;

            // Act
            fixture.Router.NotifyCoreInitialized();

            // Assert
            fixture.Documents.Should().ContainSingle().Which.Should().Contain(TrashRowSelected);
            fixture.Router.SelectedFolderPath.Should().Be(Trash);
        }

        private Task BindWithPreferredAsync(string preferredRowText) =>
            fixture.Router.BindRowsAsync(
                new[] { Trash, EfcDescendantRouterFixture.Presented },
                Array.Empty<FolderScore>(),
                EfcDescendantRouterFixture.Root,
                CancellationToken.None,
                preferredRowText
            );
    }
}
