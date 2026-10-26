#nullable enable
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Issue #1007 R3, N-1: the QFC <c>affordanceToggle</c> on a row whose collapse is anchored away
    /// from its active segment. The public <see cref="BreadcrumbStateRow.CollapseAfter"/> contract
    /// accepts any non-leaf index whatever the active segment, so this state is reachable. The
    /// toggle restores the full chain instead of listing children.
    /// </summary>
    public sealed partial class FolderBreadcrumbBridgeRouterIssue1007Tests
    {
        /// <summary>
        /// Issue #1007 R3, N-1 (D3): with the leaf still active, a row collapsed after Projects is
        /// restored by the [+] in one render, and no subfolder query is made.
        /// </summary>
        [TestMethod]
        public async Task AffordanceToggle_OnRowCollapsedAwayFromActiveSegment_RestoresChainWithoutQuery()
        {
            // Arrange: Archive > Projects > Alpha with Alpha active, collapsed after Projects.
            var (router, provider) = await RouterAsync();
            router.Model.Rows[0].CollapseAfter(1);

            // Act
            var outputs = await ExpandAlphaAsync(router);

            // Assert
            outputs.Should().ContainSingle().Which.Should().Contain("\"type\":\"render\"");
            router.Model.Rows[0].CollapsedAfterIndex.Should().BeNull("the full chain is restored");
            router.Model.Rows[0].LeafExpanded.Should().BeFalse("no children are listed");
            provider.Verify(
                p => p.GetImmediateSubfoldersAsync(Key(Alpha), It.IsAny<CancellationToken>()),
                Times.Never()
            );
            provider.Verify(
                p => p.GetImmediateSubfoldersAsync(Key(Projects), It.IsAny<CancellationToken>()),
                Times.Never()
            );
        }
    }
}
