using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                .MatchRegex(
                    @"(?s)state\.committedIdentity.*?row\.isSelectable\s*&&\s*row\.identity\s*===\s*state\.committedIdentity"
                );
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
            Match button = Find(@"<button\b[^>]*>");
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
                    @"(?s)var\s+(?<expandedFlag>[A-Za-z_$][\w$]*)\s*=\s*state\.viewMode\s*===\s*\""expanded\""\s*;.*?var\s+active\s*=\s*\k<expandedFlag>\s*&&\s*row\.isSelectable\s*&&\s*row\.identity\s*===\s*state\.pendingIdentity"
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

            Match selectorKeys = Find(@"var\s+selectorKeys\s*=\s*\{(?<keys>.*?)\};");
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

        private static Match Find(string pattern) =>
            Regex.Match(Html, pattern, ContractRegexOptions);

        private const RegexOptions ContractRegexOptions =
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline;
    }
}
