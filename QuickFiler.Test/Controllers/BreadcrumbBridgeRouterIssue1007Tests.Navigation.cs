using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Test.Controllers
{
    /// <summary>
    /// Issue #1007 AC1, AC2, AC3, AC11: Up, Down, Home, End, PageUp and PageDown walk the visible
    /// order (rows plus shown subfolders) and select each entry as a click does.
    /// </summary>
    public sealed partial class BreadcrumbBridgeRouterIssue1007Tests
    {
        private const string North = @"Clients\North";
        private const string South = @"Clients\South";

        /// <summary>AC1: Down from an expanded row selects its first visible subfolder.</summary>
        [TestMethod]
        public async Task ArrowDown_FromExpandedRow_SelectsFirstVisibleDescendant()
        {
            // Arrange
            var fixture = await BoundAsync(North, South);
            await SelectAsync(fixture, "row-0");
            await ExpandAsync(fixture, "row-0");

            // Act
            await KeyAsync(fixture, "row-0", "Down");

            // Assert
            fixture.Router.SelectedFolderPath.Should().Be(@"Clients\North\Q3");
            SelectedNodeTitle(fixture).Should().Be(EfcDescendantRouterFixture.Q3);
        }

        /// <summary>AC1: Down from a selected subfolder selects the next visible entry.</summary>
        [TestMethod]
        public async Task ArrowDown_FromSelectedDescendant_SelectsNextVisibleEntry()
        {
            // Arrange
            var fixture = await BoundAsync(North, South);
            await SelectAsync(fixture, "row-0");
            await ExpandAsync(fixture, "row-0");
            await fixture.ActivateAsync("row-0", fixture.NodeId(EfcDescendantRouterFixture.Q3));

            // Act
            await KeyAsync(fixture, "row-0", "Down");

            // Assert
            fixture.Router.SelectedFolderPath.Should().Be(@"Clients\North\Q4");
        }

        /// <summary>AC1: the children of a collapsed subfolder are skipped.</summary>
        [TestMethod]
        public async Task ArrowDown_SkipsChildrenOfCollapsedNode()
        {
            // Arrange: Q3 is expanded, then collapsed again, then selected.
            var fixture = await BoundAsync(North, South);
            await SelectAsync(fixture, "row-0");
            await ExpandAsync(fixture, "row-0");
            string q3 = fixture.NodeId(EfcDescendantRouterFixture.Q3);
            await fixture.ToggleAsync("row-0", q3);
            await fixture.ToggleAsync("row-0", q3);
            await fixture.ActivateAsync("row-0", q3);

            // Act
            await KeyAsync(fixture, "row-0", "Down");

            // Assert
            fixture.Router.SelectedFolderPath.Should().Be(@"Clients\North\Q4");
        }

        /// <summary>AC1: Up from the row below an expanded block lands on its last visible subfolder.</summary>
        [TestMethod]
        public async Task ArrowUp_FromRowBelowExpandedBlock_SelectsLastVisibleDescendant()
        {
            // Arrange: row-0 shows Q3 (expanded: Invoices, Receipts) and Q4.
            var fixture = await BoundAsync(North, South);
            await ExpandAsync(fixture, "row-0");
            await fixture.ToggleAsync("row-0", fixture.NodeId(EfcDescendantRouterFixture.Q3));
            await SelectAsync(fixture, "row-1");

            // Act
            await KeyAsync(fixture, "row-1", "Up");

            // Assert
            fixture.Router.SelectedFolderPath.Should().Be(@"Clients\North\Q4");
        }

        /// <summary>AC3: an arrow onto a subfolder commits its archive-relative path and notifies.</summary>
        [TestMethod]
        public async Task ArrowDown_OntoDescendant_CommitsArchiveRelativePathAndRaisesChanged()
        {
            // Arrange
            var fixture = await BoundAsync(North, South);
            await SelectAsync(fixture, "row-0");
            await ExpandAsync(fixture, "row-0");
            int before = fixture.SelectionEvents.Count;

            // Act
            await KeyAsync(fixture, "row-0", "Down");

            // Assert
            fixture.SelectionEvents.Skip(before).Should().Equal(@"Clients\North\Q3");
        }

        /// <summary>AC3: Enter after moving onto a subfolder files to that subfolder.</summary>
        [TestMethod]
        public async Task ArrowDown_OntoDescendant_ThenRowConfirmed_RaisesConfirmationForDescendant()
        {
            // Arrange
            var fixture = await BoundAsync(North, South);
            await SelectAsync(fixture, "row-0");
            await ExpandAsync(fixture, "row-0");
            await KeyAsync(fixture, "row-0", "Down");

            // Act
            await fixture.InboundAsync("rowConfirmed", "row-0");

            // Assert
            fixture.Confirmations.Should().Be(1);
            fixture.Router.SelectedFolderPath.Should().Be(@"Clients\North\Q3");
        }

        /// <summary>AC11: Home selects the first entry and never moves focus to the search box.</summary>
        [TestMethod]
        public async Task Home_SelectsFirstEntryWithoutFocusSearch()
        {
            // Arrange
            var fixture = await BoundAsync(North, South);
            int focusRequests = 0;
            fixture.Router.FocusSearchRequested += (sender, args) => focusRequests++;
            await SelectAsync(fixture, "row-1");

            // Act
            await KeyAsync(fixture, "row-1", "Home");

            // Assert
            fixture.Router.SelectedFolderPath.Should().Be(North);
            focusRequests.Should().Be(0);
            fixture.Posts.Should().NotContain(post => post.Contains("focusSearch"));
        }

        /// <summary>AC11: End selects the last visible entry, a subfolder of the last row.</summary>
        [TestMethod]
        public async Task End_SelectsLastVisibleDescendant()
        {
            // Arrange
            var fixture = await BoundAsync(North, South);
            await ExpandAsync(fixture, "row-1");
            await SelectAsync(fixture, "row-0");

            // Act
            await KeyAsync(fixture, "row-0", "End");

            // Assert
            fixture.Router.SelectedFolderPath.Should().Be(@"Clients\South\S1");
        }

        /// <summary>AC11: PageDown moves by the page size and stops at the last entry.</summary>
        [TestMethod]
        public async Task PageDown_MovesByPageSizeAndClamps()
        {
            // Arrange: visible order row-0, Q3, Q4, row-1.
            var fixture = await BoundAsync(North, South);
            await SelectAsync(fixture, "row-0");
            await ExpandAsync(fixture, "row-0");

            // Act / Assert
            await KeyAsync(fixture, "row-0", "PageDown", "2");
            fixture.Router.SelectedFolderPath.Should().Be(@"Clients\North\Q4");
            await KeyAsync(fixture, "row-0", "PageDown", "2");
            fixture.Router.SelectedFolderPath.Should().Be(South);
        }

        /// <summary>AC11: PageUp moves by the page size and stops at the first entry.</summary>
        [TestMethod]
        public async Task PageUp_MovesByPageSizeAndClamps()
        {
            // Arrange: visible order row-0, Q3, Q4, row-1.
            var fixture = await BoundAsync(North, South);
            await ExpandAsync(fixture, "row-0");
            await SelectAsync(fixture, "row-1");

            // Act / Assert
            await KeyAsync(fixture, "row-1", "PageUp", "2");
            fixture.Router.SelectedFolderPath.Should().Be(@"Clients\North\Q3");
            await KeyAsync(fixture, "row-0", "PageUp", "2");
            fixture.Router.SelectedFolderPath.Should().Be(North);
        }

        /// <summary>Error: a non-integer page size is rejected by the codec before routing.</summary>
        [TestMethod]
        public async Task PageDown_NonIntegerPageSize_IsRejectedByCodec()
        {
            // Arrange
            var fixture = await BoundAsync(North, South);
            await SelectAsync(fixture, "row-0");

            // Act
            Func<Task> act = () => KeyAsync(fixture, "row-0", "PageDown", "\"two\"");

            // Assert
            await act.Should().ThrowAsync<BreadcrumbMessageException>();
            fixture.Router.SelectedFolderPath.Should().Be(North);
        }

        /// <summary>AC11 (D-Q26): with nothing selected, Down and Home select the first row; Up and End the last.</summary>
        [TestMethod]
        public async Task NoSelection_DownAndHomeSelectFirst_UpAndEndSelectLast()
        {
            // Arrange
            var expected = new[]
            {
                ("Down", North),
                ("Home", North),
                ("Up", South),
                ("End", South),
            };

            foreach ((string key, string path) in expected)
            {
                var fixture = await BoundAsync(North, South);

                // Act
                await KeyAsync(fixture, string.Empty, key);

                // Assert
                fixture.Router.SelectedFolderPath.Should().Be(path, $"{key} with no selection");
            }
        }

        /// <summary>Guard (AC2): Down on the last visible entry posts nothing.</summary>
        [TestMethod]
        public async Task ArrowDown_OnLastVisibleEntry_PostsNothing()
        {
            // Arrange
            var fixture = await BoundAsync(North, South);
            await SelectAsync(fixture, "row-1");
            int before = fixture.Posts.Count;

            // Act
            await KeyAsync(fixture, "row-1", "Down");

            // Assert
            fixture.Posts.Count.Should().Be(before);
            fixture.Router.SelectedFolderPath.Should().Be(South);
        }

        /// <summary>Guard (AC2): Up on the first entry moves focus to the search box.</summary>
        [TestMethod]
        public async Task ArrowUp_OnFirstEntry_RaisesFocusSearchRequested()
        {
            // Arrange
            var fixture = await BoundAsync(North, South);
            int focusRequests = 0;
            fixture.Router.FocusSearchRequested += (sender, args) => focusRequests++;
            await SelectAsync(fixture, "row-0");

            // Act
            await KeyAsync(fixture, "row-0", "Up");

            // Assert
            focusRequests.Should().Be(1);
            fixture.Posts.Last().Should().Contain("focusSearch");
        }

        /// <summary>Guard (S3): a missing, zero or negative page size is a logged no-op.</summary>
        [TestMethod]
        public async Task PageKey_InvalidPageSize_IsLoggedNoOp()
        {
            // Arrange
            var fixture = await BoundAsync(North, South);
            await SelectAsync(fixture, "row-0");
            int before = fixture.Posts.Count;

            // Act
            await KeyAsync(fixture, "row-0", "PageDown");
            await KeyAsync(fixture, "row-0", "PageDown", "0");
            await KeyAsync(fixture, "row-0", "PageUp", "-1");

            // Assert
            fixture.Posts.Count.Should().Be(before);
            fixture.Router.SelectedFolderPath.Should().Be(North);
        }

        private static async Task<EfcDescendantRouterFixture> BoundAsync(params string[] rows)
        {
            var fixture = new EfcDescendantRouterFixture();
            await fixture.BindAsync(rows);
            return fixture;
        }

        private static Task SelectAsync(EfcDescendantRouterFixture fixture, string rowId) =>
            fixture.InboundAsync("rowSelected", rowId);

        private static Task ExpandAsync(EfcDescendantRouterFixture fixture, string rowId) =>
            fixture.InboundAsync("leafExpandToggle", rowId);

        private static Task KeyAsync(
            EfcDescendantRouterFixture fixture,
            string rowId,
            string key,
            string pageSizeJson = null
        ) =>
            fixture.Router.ProcessInboundAsync(
                "{\"type\":\"arrowKey\",\"rowId\":\""
                    + rowId
                    + "\",\"key\":\""
                    + key
                    + "\""
                    + (pageSizeJson == null ? string.Empty : ",\"pageSize\":" + pageSizeJson)
                    + "}"
            );

        // The title of the node marked aria-selected in the latest render, or null.
        private static string SelectedNodeTitle(EfcDescendantRouterFixture fixture)
        {
            var match = System.Text.RegularExpressions.Regex.Match(
                fixture.LatestRender(),
                "title=\"([^\"]+)\" aria-selected=\"true\""
            );
            return match.Success ? System.Net.WebUtility.HtmlDecode(match.Groups[1].Value) : null;
        }
    }
}
