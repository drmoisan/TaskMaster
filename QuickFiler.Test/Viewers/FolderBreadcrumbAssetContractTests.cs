using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Viewers;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Test.Viewers
{
    /// <summary>Compiled-resource accessibility and selector behavior contracts for issue #400.</summary>
    [TestClass]
    public sealed class FolderBreadcrumbAssetContractTests
    {
        private static readonly string Html = QuickFiler.Properties.Resources.FolderBreadcrumb;

        [TestMethod]
        public void CompiledResource_RemainsSelfContainedAndThemeAware()
        {
            // Assert
            Html.Should().NotBeNullOrWhiteSpace();
            Html.Should().NotContain("<script src=");
            Html.Should().NotContain("<link rel=");
            Html.Should().Contain(":root[data-theme=\"dark\"]");
            Html.Should().Contain("msg.type === \"themeChange\"");
            Html.Should()
                .MatchRegex(@"msg\.theme\s*===\s*\""dark\""\s*\?\s*\""dark\""\s*:\s*\""light\""");
        }

        [TestMethod]
        public void CollapsedMode_RendersOnlyTheCommittedSelectedDataRow()
        {
            // Assert
            Html.Should().MatchRegex(@"<html\b[^>]*\bdata-mode=\""collapsed\""");
            Html.Should().Contain("viewMode: \"collapsed\"");
            Html.Should()
                .MatchRegex(
                    @"(?s)function\s+visibleRows\s*\(\s*\).*?state\.viewMode\s*!==\s*\""collapsed\"".*?return\s+state\.rows"
                );
            Html.Should()
                .MatchRegex(
                    @"state\.rows\.filter\(function\s*\(row\)\s*\{\s*return\s+row\.selected;\s*\}\)\.slice\(0,\s*1\)"
                );
            Html.Should()
                .Contain("var committedRow = selectableRowForIdentity(state.committedIdentity);");
            Html.Should().Contain("return committedRow === null ? selectedRows : [committedRow];");
        }

        [TestMethod]
        public void Percentage_UsesVisibleHostSuppliedPercentTextWithoutRecomputation()
        {
            // Assert
            Html.Should().Contain("pct.textContent = row.percentText;");
            Html.Should().Contain("className = \"pct\"");
            Html.Should().NotContain("row.probability");
            Html.Should().NotContain("row.score");
            Html.Should().NotContain("PercentageFormatter");
        }

        [TestMethod]
        public void CollapsedDocumentAndList_HideVerticalOverflowWithoutScrollControls()
        {
            // Assert
            Html.Should()
                .MatchRegex(
                    @"(?s)(?:html|:root)\[data-mode=\""collapsed\""\]\s*,\s*(?:html|:root)\[data-mode=\""collapsed\""\]\s+body\s*,\s*(?:html|:root)\[data-mode=\""collapsed\""\]\s+#list\s*\{[^}]*overflow-y:\s*hidden"
                );
            Html.Should().NotMatchRegex(@"<input\b[^>]*\btype\s*=\s*[\""']number[\""']");
            Html.Should()
                .NotMatchRegex(
                    @"<(?:button|div|span)\b[^>]*(?:scroll[-_ ]?(?:up|down)|spin[-_ ]?(?:up|down)|spinner)"
                );
            Html.Should().NotContain("::-webkit-scrollbar-button");
        }

        [TestMethod]
        public void Markup_ContainsExactlyOneAccessibleDropDownButton()
        {
            // Assert
            System.Text.RegularExpressions.Match button = Find(@"<button\b[^>]*>");
            Count(@"<button\b").Should().Be(1);
            Count(@"document\.createElement\(\s*[\""']button[\""']\s*\)").Should().Be(0);
            button.Success.Should().BeTrue("the compiled page must contain the drop-down button");
            button.Value.Should().MatchRegex(@"\btype\s*=\s*[\""']button[\""']");
            button.Value.Should().MatchRegex(@"\baria-label\s*=\s*[\""'][^\""']+[\""']");
            button.Value.Should().MatchRegex(@"\baria-haspopup\s*=\s*[\""']listbox[\""']");
            button.Value.Should().MatchRegex(@"\baria-expanded\s*=\s*[\""']false[\""']");
            button.Value.Should().MatchRegex(@"\baria-controls\s*=\s*[\""']list[\""']");
        }

        [TestMethod]
        public void SelectorView_UpdatesModeAndAccurateAriaExpandedState()
        {
            // Assert
            Html.Should().Contain("msg.type === \"selectorView\"");
            Html.Should()
                .Contain(
                    "state.viewMode = msg.mode === \"expanded\" ? \"expanded\" : \"collapsed\";"
                );
            Html.Should().Contain("state.isOpen = msg.isOpen === true;");
            Html.Should().Contain("state.committedIdentity = msg.committedIdentity || null;");
            Html.Should().Contain("state.pendingIdentity = msg.pendingIdentity || null;");
            Html.Should().Contain("state.options = Array.isArray(msg.options) ? msg.options : [];");
            Html.Should().MatchRegex(@"state\.options\s*\[\s*(?:row\.rowIndex|index)\s*\]");
            Html.Should()
                .MatchRegex(
                    @"(?s)row\.identity\s*=\s*(?<identityOption>[A-Za-z_$][\w$]*).*?\k<identityOption>\.identity"
                );
            Html.Should()
                .MatchRegex(
                    @"(?s)row\.isSelectable\s*=\s*(?<selectableOption>[A-Za-z_$][\w$]*).*?\k<selectableOption>\.isSelectable\s*===\s*true"
                );
            Html.Should()
                .Contain("document.documentElement.setAttribute(\"data-mode\", state.viewMode);");
            Html.Should()
                .Contain("dropDownButton.setAttribute(\"aria-expanded\", String(state.isOpen));");
        }

        [TestMethod]
        public void ExpandedRows_ExposeListboxOptionsAndOneActiveSelectedOption()
        {
            // Assert
            Html.Should()
                .MatchRegex(
                    @"(?s)state\.viewMode\s*===\s*\""expanded\"".*?list\.setAttribute\(\""role\"",\s*\""listbox\""\)"
                );
            Html.Should().Contain("row.isSelectable");
            Html.Should()
                .MatchRegex(
                    @"(?s)function\s+selectableRowForIdentity\s*\(\s*identity\s*\).*?state\.rows\.find\(function\s*\(row\).*?row\.isSelectable\s*&&\s*row\.identity\s*===\s*identity"
                );
            Html.Should()
                .Contain("var pendingRow = selectableRowForIdentity(state.pendingIdentity);");
            Html.Should()
                .MatchRegex(
                    @"var\s+active\s*=\s*expanded\s*&&\s*row\.isSelectable\s*&&\s*row\.rowIndex\s*===\s*activeRowIndex"
                );
            Html.Should()
                .MatchRegex(
                    @"(?s)var\s+(?<rowElement>[A-Za-z_$][\w$]*)\s*=\s*document\.createElement\(\s*[\""']div[\""']\s*\);\s*\k<rowElement>\.className\s*=\s*active\s*\?\s*[\""']row\s+active[\""'].*?\k<rowElement>\.setAttribute\(\s*[\""']role[\""']\s*,\s*[\""']option[\""']\s*\).*?\k<rowElement>\.setAttribute\(\s*[\""']aria-selected[\""']\s*,\s*String\(\s*active\s*\)\s*\)"
                );
            Html.Should()
                .MatchRegex(
                    @"list\.setAttribute\(\s*[\""']aria-activedescendant[\""']\s*,\s*[A-Za-z_$][\w$]*\s*\)"
                );
        }

        [TestMethod]
        public void ExpandedDuplicatePathState_YieldsExactlyOneActiveAriaSelectedOption()
        {
            // Arrange
            const string duplicatePath = "\\Inbox\\Shared";
            var key = new FolderTreeNodeKey("store", "shared", duplicatePath);
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            provider
                .Setup(candidate =>
                    candidate.ResolveLeafKeyAsync(duplicatePath, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(key);
            provider
                .Setup(candidate =>
                    candidate.GetAncestorChainAsync(key, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(
                    new[] { new FolderBreadcrumbSegment(key, "Shared", duplicatePath, false) }
                );
            var posted = new List<string>();
            var surface = new Mock<IWebViewMessenger>();
            surface
                .Setup(messenger => messenger.PostJson(It.IsAny<string>()))
                .Callback<string>(posted.Add);

            // Act
            using (var hub = new BreadcrumbMessengerHub())
            {
                hub.Attach(surface.Object, BreadcrumbSelectorViewMode.Expanded);
                var coordinator = new BreadcrumbBridgeCoordinator(
                    hub,
                    provider.Object,
                    BreadcrumbUiDispatcher.CreateForCurrentThreadTests()
                );
                coordinator.SetSuggestions(
                    new[]
                    {
                        new FolderRow(
                            duplicatePath,
                            FolderRowKind.Suggestion,
                            new FolderScore(duplicatePath, 100, 0.73)
                        ),
                        new FolderRow(duplicatePath, FolderRowKind.Recent, null),
                    }
                );
                coordinator.SuggestionsUpgrade.GetAwaiter().GetResult();
                coordinator.SelectRow(0);
                coordinator.OpenSelector().Should().BeTrue();

                string selectorView = posted.Last(json =>
                    json.Contains("\"type\":\"selectorView\"")
                );
                string[] selectableIdentities = Regex
                    .Matches(
                        selectorView,
                        @"""identity"":""(?<identity>(?:\\.|[^""])*)"",""isSelectable"":true",
                        RegexOptions.CultureInvariant
                    )
                    .Cast<System.Text.RegularExpressions.Match>()
                    .Select(match => match.Groups["identity"].Value)
                    .ToArray();
                string pendingIdentity = Regex
                    .Match(
                        selectorView,
                        @"""pendingIdentity"":""(?<identity>(?:\\.|[^""])*)""",
                        RegexOptions.CultureInvariant
                    )
                    .Groups["identity"]
                    .Value;

                // Assert: the compiled asset resolves one pending logical row and applies active
                // and aria-selected through that row's unique render index.
                selectableIdentities.Should().HaveCount(2);
                selectableIdentities.Count(identity => identity == pendingIdentity).Should().Be(1);
                Html.Should()
                    .Contain("var pendingRow = selectableRowForIdentity(state.pendingIdentity);");
                Html.Should().Contain("row.rowIndex === activeRowIndex");
                Html.Should().Contain("setAttribute(\"aria-selected\", String(active))");
            }
        }

        [TestMethod]
        public void ActiveRow_ScrollsIntoViewOnlyInExpandedMode()
        {
            // Assert
            Count(@"scrollIntoView\(").Should().Be(1);
            Html.Should()
                .MatchRegex(
                    @"(?s)if\s*\(state\.viewMode\s*===\s*\""expanded\""\s*&&\s*activeRow\s*!==\s*null\)\s*\{.*?activeRow\.scrollIntoView\(\{\s*block:\s*\""nearest\""\s*\}\)"
                );
        }

        [TestMethod]
        public void SelectorKeys_PreventBrowserScrollingAndPostNativeKeyMessages()
        {
            // Assert
            Html.Should().Contain("ArrowUp: \"up\"");
            Html.Should().Contain("ArrowDown: \"down\"");
            Html.Should().Contain("Enter: \"enter\"");
            Html.Should().Contain("Escape: \"escape\"");
            Html.Should()
                .MatchRegex(
                    @"(?s)if\s*\(Object\.prototype\.hasOwnProperty\.call\(selectorKeys,\s*event\.key\)\)\s*\{.*?event\.preventDefault\(\);.*?post\(\{\s*type:\s*\""selectorKey\"",\s*key:\s*selectorKeys\[event\.key\]\s*\}\);.*?return;\s*\}"
                );

            System.Text.RegularExpressions.Match selectorKeys = Find(
                @"var\s+selectorKeys\s*=\s*\{(?<keys>.*?)\};"
            );
            selectorKeys.Success.Should().BeTrue();
            selectorKeys.Groups["keys"].Value.Should().NotContain("ArrowLeft");
            selectorKeys.Groups["keys"].Value.Should().NotContain("ArrowRight");
        }

        [TestMethod]
        public void ButtonAndRows_PostToggleAndStableIdentityActivationMessages()
        {
            // Assert
            Html.Should()
                .MatchRegex(
                    @"(?s)dropDownButton\.addEventListener\(\""click\"".*?post\(\{\s*type:\s*\""selectorToggle\""\s*\}\)"
                );
            Html.Should()
                .MatchRegex(
                    @"(?s)if\s*\([A-Za-z_$][\w$]*\s*&&\s*row\.isSelectable\)\s*\{.*?[A-Za-z_$][\w$]*\.addEventListener\(\s*[\""']click[\""'].*?post\(\{\s*type:\s*[\""']selectorActivate[\""']\s*,\s*identity:\s*row\.identity\s*\}\)"
                );
            Html.Should().NotContain("identity: row.rowIndex");
        }

        [TestMethod]
        public void LeftAndRightBreadcrumbMessages_RemainSupported()
        {
            // Assert
            Html.Should().MatchRegex(@"(?:event|ev)\.key\s*===\s*\""ArrowRight\""");
            Html.Should().MatchRegex(@"(?:event|ev)\.key\s*===\s*\""ArrowLeft\""");
            Html.Should().Contain("{ type: \"arrowKey\", direction: direction }");
            Html.Should().Contain("{ type: \"unhandledArrow\", direction: direction }");
        }

        [TestMethod]
        public void ModeAndThemeHooks_RemainIndependentAndFocusTheActiveListTarget()
        {
            // Assert
            Html.Should().MatchRegex(@"setAttribute\(\s*\""data-theme\""");
            Html.Should().MatchRegex(@"setAttribute\(\s*\""data-mode\""");
            Html.Should().Contain("window.addEventListener(\"focus\", focusForMode);");
            Html.Should()
                .MatchRegex(
                    @"(?s)function\s+focusForMode\(\)\s*\{.*?state\.viewMode\s*===\s*\""expanded\"".*?list\.focus\(\);.*?else.*?dropDownButton\.focus\(\);"
                );
            Html.Should().Contain("list.setAttribute(\"aria-activedescendant\", activeId);");
        }

        private static int Count(string pattern) =>
            Regex.Matches(Html, pattern, ContractRegexOptions).Count;

        private static System.Text.RegularExpressions.Match Find(string pattern) =>
            Regex.Match(Html, pattern, ContractRegexOptions);

        private const RegexOptions ContractRegexOptions =
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline;
    }
}
