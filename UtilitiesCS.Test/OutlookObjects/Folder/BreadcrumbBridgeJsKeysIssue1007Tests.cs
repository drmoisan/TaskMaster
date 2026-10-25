#nullable enable
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Issue #1007 string contracts for the EFC page script
    /// <see cref="BreadcrumbDocumentAssets.BridgeJs"/>. The repository has no JS engine in its test
    /// suite, so these verify the script text is present and correctly shaped (the
    /// <c>Issue995BridgeJsPostsConfirmationForPlainEnterAndScrollsSelectedRowIntoView</c> precedent),
    /// not that it executes in WebView2.
    /// </summary>
    [TestClass]
    public sealed class BreadcrumbBridgeJsKeysIssue1007Tests
    {
        private static readonly string BridgeJs = BreadcrumbDocumentAssets.BridgeJs;

        /// <summary>AC8 (S4): Ctrl+ArrowLeft posts the ParentLeft arrow key.</summary>
        [TestMethod]
        public void BridgeJs_CtrlArrowLeft_PostsParentLeft()
        {
            // Assert
            BridgeJs
                .Should()
                .Contain("if (e.ctrlKey && e.key === 'ArrowLeft') { key = 'ParentLeft'; }");
            BridgeJs
                .Should()
                .Contain("var message = { type: 'arrowKey', rowId: id || '', key: key };");
            BridgeJs.Should().Contain("post(message);");
        }

        /// <summary>AC8 (D-Q20): Ctrl modifies only ArrowLeft, so Ctrl+ArrowRight posts Right.</summary>
        [TestMethod]
        public void BridgeJs_CtrlArrowRight_PostsRight()
        {
            // Assert
            BridgeJs.Should().Contain("ArrowRight: 'Right'");
            BridgeJs.Should().Contain("if (e.ctrlKey && e.key === 'ArrowLeft')");
            BridgeJs.Should().NotContain("e.key === 'ArrowRight'");
        }

        /// <summary>AC11 (D-Q8): Home, End, PageUp and PageDown map to their EFC key values.</summary>
        [TestMethod]
        public void BridgeJs_HomeEndPageKeys_MapToKeyValues()
        {
            // Assert
            BridgeJs
                .Should()
                .Contain("Home: 'Home', End: 'End', PageUp: 'PageUp', PageDown: 'PageDown'");
        }

        /// <summary>AC11 (S3, D-Q17): page keys carry the measured fully visible row count, at least one.</summary>
        [TestMethod]
        public void BridgeJs_PageKeys_CarryMeasuredPageSize()
        {
            // Assert
            BridgeJs
                .Should()
                .Contain(
                    "if (key === 'PageUp' || key === 'PageDown') { message.pageSize = fullyVisibleCount(); }"
                );
            BridgeJs.Should().Contain("function fullyVisibleCount()");
            BridgeJs.Should().Contain("if (r.top >= 0 && r.bottom <= bottom) { count++; }");
            BridgeJs.Should().Contain("return Math.max(1, count);");
        }

        /// <summary>AC1: the post-message scroll keeps a selected descendant in view before its row.</summary>
        [TestMethod]
        public void BridgeJs_Scroll_TargetsSelectedChildBeforeSelectedRow()
        {
            // Assert
            BridgeJs
                .Should()
                .Contain(
                    "document.querySelector('.child[aria-selected=\"true\"]') || document.querySelector('.rowwrap.selected')"
                );
            BridgeJs.Should().Contain("scrollIntoView({ block: 'nearest' })");
        }

        /// <summary>Guard (D-Q26): with nothing selected the arrow key posts an empty row id.</summary>
        [TestMethod]
        public void BridgeJs_NoSelection_PostsEmptyRowId()
        {
            // Assert
            BridgeJs
                .Should()
                .Contain("var id = selected ? selected.getAttribute('data-row-id') : '';");
            BridgeJs.Should().Contain("rowId: id || ''");
        }
    }
}
