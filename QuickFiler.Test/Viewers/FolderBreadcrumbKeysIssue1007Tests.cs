using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Viewers;

namespace QuickFiler.Test.Viewers
{
    /// <summary>
    /// Issue #1007 string contracts for the QFC page script embedded in
    /// <see cref="FolderBreadcrumbDocument.Html"/>. CI runs no node suite, so each test twins a
    /// test in <c>tests/quickfiler-breadcrumb/issue-1007-keys.test.mjs</c>; these verify the script
    /// text is present and correctly shaped, not that it executes in a browser.
    /// </summary>
    [TestClass]
    public sealed class FolderBreadcrumbKeysIssue1007Tests
    {
        private const RegexOptions Options =
            RegexOptions.CultureInvariant | RegexOptions.Singleline;

        private static readonly string Html = FolderBreadcrumbDocument.Html;

        /// <summary>AC11: Home, End, PageUp and PageDown are selector keys with lowercase wire values.</summary>
        [TestMethod]
        public void SelectorKeys_MapHomeEndPageUpPageDown()
        {
            // Act
            string keys = Regex
                .Match(Html, @"var\s+selectorKeys\s*=\s*\{(?<keys>.*?)\};", Options)
                .Groups["keys"]
                .Value;

            // Assert
            keys.Should().Contain("Home: \"home\"");
            keys.Should().Contain("End: \"end\"");
            keys.Should().Contain("PageUp: \"pageup\"");
            keys.Should().Contain("PageDown: \"pagedown\"");
            keys.Should().NotContain("ArrowLeft").And.NotContain("ArrowRight");
        }

        /// <summary>AC8 (S4): Ctrl+ArrowLeft posts the parent selector key before arrow routing.</summary>
        [TestMethod]
        public void CtrlArrowLeft_PostsParentSelectorKey()
        {
            // Assert
            Html.Should()
                .MatchRegex(
                    @"(?s)if\s*\(event\.key\s*===\s*""ArrowLeft""\s*&&\s*event\.ctrlKey\)\s*\{\s*event\.preventDefault\(\);\s*post\(\{\s*type:\s*""selectorKey"",\s*key:\s*""parent""\s*\}\);\s*return;"
                );
        }

        /// <summary>AC11 (S3, D-Q17): page keys carry the fully visible row count, at least one.</summary>
        [TestMethod]
        public void PageKeys_CarryPageSizeFromFullyVisibleCount()
        {
            // Act
            string counter = FunctionBody("fullyVisibleCount", "visiblePageSize");

            // Assert
            Html.Should().Contain("function fullyVisibleCount(rects, top, bottom)");
            counter.Should().Contain("rect.top >= top && rect.bottom <= bottom");
            counter.Should().Contain("Math.max(1, count)");
            Html.Should()
                .MatchRegex(
                    @"(?s)if\s*\(event\.key\s*===\s*""PageUp""\s*\|\|\s*event\.key\s*===\s*""PageDown""\)\s*\{\s*message\.pageSize\s*=\s*visiblePageSize\(\);"
                );
        }

        /// <summary>AC4 (S6): Left and Right always post arrowKey; the page no longer pre-filters.</summary>
        [TestMethod]
        public void OnArrow_AlwaysPostsArrowKey()
        {
            // Act
            string onArrow = FunctionBody("onArrow", "hasSelectedDescendant");

            // Assert
            onArrow.Should().Contain("post({ type: \"arrowKey\", direction: direction });");
            Html.Should().NotContain("unhandledArrow");
            Html.Should().NotContain("rowHasOpenAffordance");
        }

        /// <summary>AC1, AC4: the pending descendant named by the selector view owns the active option.</summary>
        [TestMethod]
        public void MakeSubfolders_MarksPendingNodeActive()
        {
            // Act
            string makeSubfolders = FunctionBody("makeSubfolders", "render");

            // Assert
            Html.Should()
                .Contain(
                    "state.pendingNodeIdentity = typeof msg.pendingNodeIdentity === \"string\" ? msg.pendingNodeIdentity : null;"
                );
            makeSubfolders.Should().Contain("row.identity === state.pendingIdentity");
            makeSubfolders.Should().Contain("child.identity === state.pendingNodeIdentity");
            makeSubfolders.Should().Contain("\"sub active\"");
        }

        /// <summary>AC9 (D-Q27): an open-list click on a non-leaf segment posts selectorHighlight and stops.</summary>
        [TestMethod]
        public void NonLeafSegmentClick_OpenList_StopsPropagationAndPostsSelectorHighlight()
        {
            // Assert
            Html.Should()
                .MatchRegex(
                    @"(?s)state\.isOpen\s*&&\s*state\.viewMode\s*===\s*""expanded"".*?cell\.segmentIndex\s*!==\s*lastSegmentIndex\(row\).*?event\.stopPropagation\(\);\s*post\(\{\s*type:\s*""selectorHighlight"",\s*identity:\s*row\.identity\s*\}\);"
                );
        }

        /// <summary>AC9 (S5): a segment double-click posts selectorSegmentDoubleClick with the row identity.</summary>
        [TestMethod]
        public void SegmentDoubleClick_PostsSelectorSegmentDoubleClick()
        {
            // Assert
            Html.Should()
                .Contain(
                    "post({ type: \"selectorSegmentDoubleClick\", rowIdentity: row.identity, segmentIndex: cell.segmentIndex });"
                );
            Html.Should().NotContain("type: \"segmentDoubleClick\"");
        }

        /// <summary>AC14 (D-Q28): a loaded child with zero children renders no toggle.</summary>
        [TestMethod]
        public void MakeSubfolders_LoadedChildWithoutChildren_RendersNoToggle()
        {
            // Act
            string makeSubfolders = FunctionBody("makeSubfolders", "render");

            // Assert
            makeSubfolders
                .Should()
                .Contain(
                    "child.childrenLoaded === true && Array.isArray(child.children) && child.children.length === 0"
                );
            makeSubfolders
                .Should()
                .MatchRegex(
                    @"(?s)if\s*\(!loadedEmpty\)\s*\{\s*subfolderElement\.appendChild\(toggle\);\s*\}"
                );
        }

        private static string FunctionBody(string functionName, string nextFunctionName)
        {
            Match match = Regex.Match(
                Html,
                @"function\s+"
                    + Regex.Escape(functionName)
                    + @"\s*\([^)]*\)\s*\{(?<body>.*?)(?=\n\s*function\s+"
                    + Regex.Escape(nextFunctionName)
                    + @"\s*\()",
                Options
            );
            match
                .Success.Should()
                .BeTrue($"the {functionName} function must precede {nextFunctionName}");
            return match.Groups["body"].Value;
        }
    }
}
