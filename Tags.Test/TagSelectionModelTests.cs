using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;

namespace Tags.Test
{
    /// <summary>
    /// Unit tests for the host-neutral <see cref="TagSelectionModel"/>. Every method is exercised with
    /// pure inputs and Moq'd interfaces; no WinForms object is constructed.
    /// </summary>
    [TestClass]
    public class TagSelectionModelTests
    {
        [TestMethod]
        public void ParseSearchStrings_WhenEmptyWhitespaceOrWildcard_ReturnsExpectedTokens()
        {
            var model = NewModel();

            model.ParseSearchStrings("   ").Should().BeEmpty();
            model.ParseSearchStrings("").Should().BeEmpty();
            model.ParseSearchStrings("Alpha*Gamma").Should().Equal("Alpha", "Gamma");
            model.ParseSearchStrings("  Alpha  ").Should().Equal("Alpha");
            model.ParseSearchStrings("a**b").Should().Equal("a", "b");
        }

        [TestMethod]
        public void Search_WhenNoStringsOrMatches_ReturnsSourceOrEmpty()
        {
            var model = NewModel();
            var source = new SortedDictionary<string, bool>
            {
                ["TagProgram Alpha"] = true,
                ["TagProgram Beta"] = false,
                ["Topic Gamma"] = true,
            };

            model.Search(source, new List<string>()).Should().BeSameAs(source);
            model
                .Search(source, new List<string> { "tagprogram" })
                .Keys.Should()
                .Equal("TagProgram Alpha", "TagProgram Beta");
            model.Search(source, new List<string> { "missing" }).Should().BeEmpty();
        }

        [TestMethod]
        public void FilterArchive_WhenExclusionsPresent_RemovesMatchesCaseInsensitively()
        {
            var source = new SortedDictionary<string, bool>
            {
                ["Archive Choice"] = true,
                ["Current Choice"] = true,
            };
            var autoAssigner = new Mock<IAutoAssign>(MockBehavior.Loose);
            autoAssigner.SetupGet(x => x.FilterList).Returns(new List<string> { "archive choice" });
            var model = new TagSelectionModel(source, autoAssigner.Object, null);

            model.FilterArchive(source).Keys.Should().Equal("Current Choice");
        }

        [TestMethod]
        public void FilterArchive_WhenAutoAssignerNull_ReturnsSourceUnchanged()
        {
            var source = new SortedDictionary<string, bool> { ["Only Choice"] = true };
            var model = new TagSelectionModel(source, null, null);

            model.FilterArchive(source).Should().BeSameAs(source);
        }

        [TestMethod]
        public void IsPrefixMissing_CoversPresentAbsentNullAndShortSamples()
        {
            var model = NewModel();
            var prefix = NewPrefix("Program", "TagProgram ");

            model.IsPrefixMissing(prefix, "TagProgram Alpha").Should().BeFalse();
            model.IsPrefixMissing(prefix, "Alpha").Should().BeTrue();
            model.IsPrefixMissing(prefix, null).Should().BeTrue();
            model.IsPrefixMissing(NewPrefix("None", ""), "anything").Should().BeFalse();
        }

        [TestMethod]
        public void SelectionAccessors_ReturnOnlySelectedKeys()
        {
            var model = NewModel(
                new SortedDictionary<string, bool>
                {
                    ["Alpha"] = true,
                    ["Beta"] = false,
                    ["Gamma"] = true,
                }
            );

            model.SelectionAsList().Should().Equal("Alpha", "Gamma");
            model.SelectionAsString().Should().Be("Alpha, Gamma");
            model.GetSelections().Should().Equal("Alpha", "Gamma");
        }

        [TestMethod]
        public void ToggleMethods_MutateSelectionState()
        {
            var model = NewModel(
                new SortedDictionary<string, bool> { ["Alpha"] = false, ["Beta"] = true }
            );

            model.ToggleOn("Alpha");
            model.ToggleOff("Beta");
            model.GetSelections().Should().Equal("Alpha");

            model.ToggleChoice("Alpha");
            model.GetSelections().Should().BeEmpty();

            model.ContainsOption("Alpha").Should().BeTrue();
            model.ContainsOption("Missing").Should().BeFalse();
        }

        [TestMethod]
        public void AddOption_AddsNewUpdatesExistingAndSyncsFilteredSet()
        {
            var options = new SortedDictionary<string, bool> { ["Alpha"] = false };
            var model = NewModel(options);
            model.FilteredOptions = new SortedDictionary<string, bool> { ["Alpha"] = false };

            model.AddOption("Beta", blClickTrue: true);
            model.DictOptions.Keys.Should().Contain("Beta");
            model.FilteredOptions.Keys.Should().Contain("Beta");

            model.AddOption("Beta", blClickTrue: false);
            model.DictOptions["Beta"].Should().BeFalse();
            model.FilteredOptions["Beta"].Should().BeFalse();
        }

        [TestMethod]
        public void UpdateSelections_SnapshotsSelectedKeysFromBothDictionaries()
        {
            var model = NewModel(
                new SortedDictionary<string, bool> { ["Alpha"] = true, ["Beta"] = false }
            );
            model.FilteredOptions = new SortedDictionary<string, bool>
            {
                ["Alpha"] = true,
                ["Beta"] = false,
            };

            model.UpdateSelections();

            model.Selections.Should().Equal("Alpha");
            model.FilteredSelections.Should().Equal("Alpha");
        }

        [TestMethod]
        public void FilterToSelectedSet_KeepsOnlySelectedOptions()
        {
            var model = NewModel(
                new SortedDictionary<string, bool>
                {
                    ["Alpha"] = true,
                    ["Beta"] = false,
                    ["Gamma"] = true,
                }
            );

            var result = model.FilterToSelectedSet();

            result.Keys.Should().Equal("Alpha", "Gamma");
            model.FilteredOptions.Should().BeSameAs(result);
        }

        [TestMethod]
        public void ResolvePrefix_ResolvesDefaultsAndThrowsOnUnknownKey()
        {
            var model = NewModel();
            var prefix = NewPrefix("Program", "TagProgram ");

            model.ResolvePrefix(new List<IPrefix> { prefix }, "Program");
            model.Prefix.Should().BeSameAs(prefix);

            model.ResolvePrefix(null, "Program");
            model.Prefix.Value.Should().BeEmpty();

            model.ResolvePrefix(new List<IPrefix> { prefix }, "");
            model.Prefix.Value.Should().BeEmpty();

            System.Action act = () => model.ResolvePrefix(new List<IPrefix> { prefix }, "Unknown");
            act.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void GetDefaultPrefix_ReturnsEmptyOtherPrefix()
        {
            var model = NewModel();

            var prefix = model.GetDefaultPrefix();

            prefix.Key.Should().BeEmpty();
            prefix.Value.Should().BeEmpty();
            prefix.Color.Should().Be(OlCategoryColor.olCategoryColorNone);
        }

        [TestMethod]
        public void SetDictOptions_ReplacesActiveOptionsWithoutTouchingOriginal()
        {
            var original = new SortedDictionary<string, bool> { ["Alpha"] = true };
            var model = new TagSelectionModel(original, null, null);
            var replacement = new SortedDictionary<string, bool> { ["Beta"] = true };

            model.SetDictOptions(replacement);

            model.DictOptions.Should().BeSameAs(replacement);
            model.DictOriginal.Should().BeSameAs(original);
        }

        private static TagSelectionModel NewModel(SortedDictionary<string, bool> options = null) =>
            new TagSelectionModel(options ?? new SortedDictionary<string, bool>(), null, null);

        private static IPrefix NewPrefix(string key, string value)
        {
            var prefix = new Mock<IPrefix>(MockBehavior.Loose);
            prefix.SetupGet(p => p.Key).Returns(key);
            prefix.SetupGet(p => p.Value).Returns(value);
            return prefix.Object;
        }
    }
}
