using System;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Viewers;

namespace QuickFiler.Test.Viewers
{
    /// <summary>
    /// Issue 1002 CI-visible contract over the composed QFC selector document: inside
    /// <c>makeSubfolders</c> the name is appended before the toggle, and each nested level
    /// carries its depth and a per-depth left padding. CI runs no node suite, so these text
    /// contracts are the CI signal for the JavaScript change.
    /// </summary>
    [TestClass]
    public sealed class FolderBreadcrumbNestingIssue1002Tests
    {
        [TestMethod]
        public void MakeSubfolders_AppendsNameBeforeToggle()
        {
            // Arrange
            string body = MakeSubfoldersBody();

            // Act
            int name = body.IndexOf("subfolderElement.appendChild(name)", StringComparison.Ordinal);
            int toggle = body.IndexOf(
                "subfolderElement.appendChild(toggle)",
                StringComparison.Ordinal
            );

            // Assert
            name.Should().BeGreaterThan(-1, "the name is appended to the row");
            toggle.Should().BeGreaterThan(-1, "the toggle is appended to the row");
            name.Should().BeLessThan(toggle, "the +/- follows the folder name");
        }

        [TestMethod]
        public void MakeSubfolders_RecursiveCallPassesDepthPlusOne()
        {
            // Arrange
            string body = MakeSubfoldersBody();

            // Act
            bool passesDepth = Regex.IsMatch(
                body,
                @"makeSubfolders\(\s*row\s*,\s*child\.children\s*,\s*selectedChildOwnsActive\s*,\s*depth\s*\+\s*1\s*\)"
            );

            // Assert
            passesDepth.Should().BeTrue("each nested level is one step deeper than its parent");
        }

        [TestMethod]
        public void MakeSubfolders_SetsDataDepthAndPerDepthPadding()
        {
            // Arrange
            string body = MakeSubfoldersBody();

            // Act / Assert
            body.Should().Contain("\"data-depth\"", "each row carries its depth");
            body.Should().Contain("padding-left: ", "the indent is padding on the row itself");
            body.Should().Contain("24 + 16 * depth", "one 16px step per level from a 24px base");
        }

        [TestMethod]
        public void MakeSubfolders_PinnedRecursiveCallPrefixPreserved()
        {
            // Arrange
            string body = MakeSubfoldersBody();

            // Act
            string[] declared = Regex
                .Matches(body, @"function\s+([A-Za-z_]\w*)\s*\(")
                .Cast<Match>()
                .Select(match => match.Groups[1].Value)
                .ToArray();

            // Assert
            body.Should().Contain("makeSubfolders(row, child.children", "the #997 pin survives");
            declared
                .Should()
                .BeEquivalentTo(
                    new[] { "toggleDescendant", "activateSubfolder" },
                    "no function is inserted between makeSubfolders and render"
                );
        }

        // The makeSubfolders body runs from its declaration to the next declaration, render.
        private static string MakeSubfoldersBody()
        {
            string html = FolderBreadcrumbDocument.Html;
            const string header = "function makeSubfolders(";
            int start = html.IndexOf(header, StringComparison.Ordinal);
            int end = html.IndexOf("function render", StringComparison.Ordinal);
            start.Should().BeGreaterThan(-1, "makeSubfolders is declared");
            end.Should().BeGreaterThan(start, "render follows makeSubfolders");
            return html.Substring(start + header.Length, end - start - header.Length);
        }
    }
}
