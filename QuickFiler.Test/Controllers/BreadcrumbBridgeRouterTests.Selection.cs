using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Test.Controllers
{
    /// <summary>
    /// Selection, theme, multi-row navigation and collapse/expand-toggle tests for
    /// <see cref="BreadcrumbBridgeRouter"/>, relocated verbatim from
    /// <c>BreadcrumbBridgeRouterTests.cs</c> so that neither part of the test class exceeds the
    /// 500-line file limit. This is a pure mechanical relocation: every member below is
    /// byte-identical to its pre-split form, in its pre-split order.
    /// </summary>
    public partial class BreadcrumbBridgeRouterTests
    {
        [TestMethod]
        public void ArrowKeyUp_AtTopSelectableRow_PostsFocusSearchAndRaisesEvent()
        {
            // Arrange: row-1 is the top selectable row (row-0 is a banner).
            Bind();
            bool raised = false;
            _router.FocusSearchRequested += (s, e) => raised = true;

            // Act
            Inbound("{\"type\":\"arrowKey\",\"rowId\":\"row-1\",\"key\":\"Up\"}");

            // Assert
            _posted.Should().Contain(p => p.Contains("\"type\":\"focusSearch\""));
            raised.Should().BeTrue();
        }

        [TestMethod]
        public void RowSelected_UpdatesSelectedFolderPathAndRaisesEvent()
        {
            // Arrange
            Bind();
            string observed = null;
            _router.SelectedFolderPathChanged += (s, path) => observed = path;

            // Act
            Inbound("{\"type\":\"rowSelected\",\"rowId\":\"row-1\"}");

            // Assert
            _router.SelectedFolderPath.Should().Be(LeafPath);
            observed.Should().Be(LeafPath);
        }

        [TestMethod]
        public void RowSelected_OnBannerRow_IsIgnored()
        {
            // Arrange
            Bind();
            bool raised = false;
            _router.SelectedFolderPathChanged += (s, path) => raised = true;

            // Act: row-0 is the banner row.
            Inbound("{\"type\":\"rowSelected\",\"rowId\":\"row-0\"}");

            // Assert: banner rows are never selectable.
            _router.SelectedFolderPath.Should().BeNull();
            raised.Should().BeFalse();
        }

        [TestMethod]
        public void SelectFirstRow_SelectsTopSelectableRowAndPostsRender()
        {
            // Arrange
            Bind();

            // Act
            _router.SelectFirstRow();

            // Assert: the first selectable (non-banner) row is selected and re-rendered.
            _router.SelectedFolderPath.Should().Be(LeafPath);
            string render = _posted.Single(p => p.Contains("\"type\":\"render\""));
            render.Should().Contain(JsonEscaped("rowwrap selected"));
        }

        [TestMethod]
        public void ApplyTheme_Dark_ReDeliversDarkDocument()
        {
            // Arrange
            Bind();

            // Act
            _router.ApplyTheme(true);

            // Assert: a second navigation carrying the dark theme block.
            _navigated.Should().HaveCount(2);
            _navigated[1].Should().Contain("background: #1e1e1e");
        }

        private void BindThreeRows()
        {
            // Second suggestion resolves to an unknown chain -> single-segment fallback.
            _provider
                .Setup(p =>
                    p.GetAncestorChainAsync(
                        It.IsAny<FolderTreeNodeKey>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(
                    (FolderTreeNodeKey k, CancellationToken ct) =>
                        k.FolderPath == LeafPath
                            ? new[]
                            {
                                ProviderSegment("Inbox", "Inbox", true),
                                ProviderSegment(LeafPath, "Alpha", true),
                            }
                            : (IReadOnlyList<FolderBreadcrumbSegment>)new FolderBreadcrumbSegment[0]
                );
            _router
                .BindRowsAsync(
                    new[] { "==== SUGGESTIONS ====", LeafPath, "Inbox\\Beta" },
                    new FolderScore[0],
                    CancellationToken.None
                )
                .GetAwaiter()
                .GetResult();
        }

        [TestMethod]
        public void ArrowKeyDown_SelectsNextSelectableRow()
        {
            // Arrange
            BindThreeRows();

            // Act
            Inbound("{\"type\":\"arrowKey\",\"rowId\":\"row-1\",\"key\":\"Down\"}");

            // Assert
            _router.SelectedFolderPath.Should().Be("Inbox\\Beta");
        }

        [TestMethod]
        public void ArrowKeyUp_OnNonTopRow_SelectsPreviousRowWithoutFocusSearch()
        {
            // Arrange
            BindThreeRows();

            // Act
            Inbound("{\"type\":\"arrowKey\",\"rowId\":\"row-2\",\"key\":\"Up\"}");

            // Assert: selection moves up; focusSearch is reserved for the top row.
            _router.SelectedFolderPath.Should().Be(LeafPath);
            _posted.Should().NotContain(p => p.Contains("\"type\":\"focusSearch\""));
        }

        [TestMethod]
        public void LeafExpandToggle_OnCollapsedRow_ReExpandsWithoutProviderQuery()
        {
            // Arrange: collapse the row first.
            Bind();
            Inbound("{\"type\":\"segmentDoubleClick\",\"rowId\":\"row-1\",\"segmentIndex\":0}");

            // Act: the affordance now re-expands the full breadcrumb.
            Inbound("{\"type\":\"leafExpandToggle\",\"rowId\":\"row-1\"}");

            // Assert
            _provider.Verify(
                p =>
                    p.GetImmediateSubfoldersAsync(
                        It.IsAny<FolderTreeNodeKey>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never
            );
            _posted.Last(p => p.Contains("\"type\":\"render\"")).Should().Contain(">Alpha<");
        }

        [TestMethod]
        public void LeafExpandToggle_OnExpandedLeaf_CollapsesWithoutSecondQuery()
        {
            // Arrange: expand the leaf once (one provider query).
            Bind();
            Inbound("{\"type\":\"leafExpandToggle\",\"rowId\":\"row-1\"}");

            // Act: toggling again collapses the children.
            Inbound("{\"type\":\"leafExpandToggle\",\"rowId\":\"row-1\"}");

            // Assert: still exactly one query; last render shows the plus affordance again.
            _provider.Verify(
                p =>
                    p.GetImmediateSubfoldersAsync(
                        It.IsAny<FolderTreeNodeKey>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
            _posted
                .Last(p => p.Contains("\"type\":\"render\""))
                .Should()
                .Contain(JsonEscaped("data-role=\"leaf\">+</span>"));
        }

        [TestMethod]
        public void ArrowKeyRight_WhenCollapsed_ReExpandsWithoutProviderQuery()
        {
            // Arrange
            Bind();
            Inbound("{\"type\":\"segmentDoubleClick\",\"rowId\":\"row-1\",\"segmentIndex\":0}");

            // Act
            Inbound("{\"type\":\"arrowKey\",\"rowId\":\"row-1\",\"key\":\"Right\"}");

            // Assert
            _provider.Verify(
                p =>
                    p.GetImmediateSubfoldersAsync(
                        It.IsAny<FolderTreeNodeKey>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never
            );
            _posted.Last(p => p.Contains("\"type\":\"render\"")).Should().Contain(">Alpha<");
        }

        [TestMethod]
        public void ArrowKey_UnknownKey_IsLoggedNoOp()
        {
            // Arrange
            Bind();
            int postedBefore = _posted.Count;

            // Act
            Inbound("{\"type\":\"arrowKey\",\"rowId\":\"row-1\",\"key\":\"Home\"}");

            // Assert: no state change and no outbound payloads.
            _posted.Count.Should().Be(postedBefore);
            _router.SelectedFolderPath.Should().BeNull();
        }

        [TestMethod]
        public void RowSelected_OnTrashPseudoRow_SelectsTrashPath()
        {
            // Arrange
            _router
                .BindRowsAsync(
                    new[] { "Trash to Delete", LeafPath },
                    new FolderScore[0],
                    CancellationToken.None
                )
                .GetAwaiter()
                .GetResult();

            // Act
            Inbound("{\"type\":\"rowSelected\",\"rowId\":\"row-0\"}");

            // Assert
            _router.SelectedFolderPath.Should().Be("Trash to Delete");
        }

        /// <summary>
        /// Issue #614 producer-side regression: activating the mailbox store-root segment, which
        /// sits ABOVE the bound archive root, must not place a full Outlook path into
        /// <c>SelectedFolderPath</c>. The filing boundary downstream accepts only archive-relative
        /// stems, so a verbatim store-root pass-through leaks a non-relative path into filing.
        /// </summary>
        [TestMethod]
        public void Issue614_SegmentActivate_StoreRootSegment_DoesNotStoreFullOutlookPath()
        {
            // Arrange: bind through the internal archive-root overload with a provider chain whose
            // segment 0 is the mailbox store root above the bound archive root.
            const string storeRoot = @"\\mailbox@example.com";
            const string archiveRoot = @"\\mailbox@example.com\Archive";
            const string target = @"Clients\North";
            const string hierarchyTarget = @"\\mailbox@example.com\Archive\Clients\North";
            FolderTreeNodeKey leafKey = Key(hierarchyTarget);
            _provider
                .Setup(p => p.ResolveLeafKeyAsync(hierarchyTarget, It.IsAny<CancellationToken>()))
                .ReturnsAsync(leafKey);
            _provider
                .Setup(p => p.GetAncestorChainAsync(leafKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new[]
                    {
                        ProviderSegment(storeRoot, "Mailbox", true),
                        ProviderSegment(archiveRoot, "Archive", true),
                        ProviderSegment(hierarchyTarget, "North", false),
                    }
                );
            _router
                .BindRowsAsync(
                    new[] { target },
                    new[] { new FolderScore(target, 730, 0.73) },
                    archiveRoot,
                    CancellationToken.None
                )
                .GetAwaiter()
                .GetResult();

            // Act: activate segment 0 (the store root above the bound archive root).
            Inbound("{\"type\":\"segmentActivate\",\"rowId\":\"row-0\",\"segmentIndex\":0}");

            // Assert: the store-root full Outlook path is never stored as the filing selection.
            _router.SelectedFolderPath.Should().NotBe(storeRoot);
        }
    }
}
