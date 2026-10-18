using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Viewers;

namespace QuickFiler.Test.Viewers
{
    [TestClass]
    public sealed class FolderBreadcrumbRecursiveAssetIssue997Tests
    {
        [TestMethod]
        public void RenderDescendantWithChildren_OffersIndependentExpansionControl()
        {
            string html = FolderBreadcrumbDocument.Html;
            html.Should()
                .MatchRegex(@"(?s)if\s*\(\s*child\.hasChildren\s*\).*?selectorDescendantToggle");
            html.Should().Contain("event.stopPropagation()");
            html.Should().MatchRegex(@"makeSubfolders\(row,\s*child\.children");
        }

        [TestMethod]
        public void ComposedDocument_ContainsExactlyOneInlineScript()
        {
            string html = FolderBreadcrumbDocument.Html;
            Regex.Matches(html, @"<script\b", RegexOptions.IgnoreCase).Count.Should().Be(1);
            html.Should().NotContain("<script src=");
            Regex.Matches(html, @"</script>", RegexOptions.IgnoreCase).Count.Should().Be(1);
        }

        [TestMethod]
        public void ComposedDocument_InsertsCompleteCompiledScriptWithoutMarker()
        {
            string html = FolderBreadcrumbDocument.Html;
            string script = Regex.Match(html, @"(?s)<script>(.*?)</script>").Groups[1].Value;
            script
                .Trim()
                .Should()
                .Be(QuickFiler.Properties.Resources.FolderBreadcrumbScript.Trim());
            html.Should().NotContain(FolderBreadcrumbDocument.ScriptMarker);
        }
    }
}
