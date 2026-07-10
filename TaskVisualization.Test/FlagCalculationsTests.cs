using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskVisualization;
using UtilitiesCS;

namespace TaskVisualization.Test
{
    /// <summary>
    /// Unit tests for the host-neutral <see cref="FlagCalculations"/> statics. No
    /// form, popup, sleep, timer, or temp file is used.
    /// </summary>
    [TestClass]
    public class FlagCalculationsTests
    {
        [TestMethod]
        public void GetSymbolsDictionary_ExcludesAllAndNone_ReturnsSortedKeys()
        {
            var dict = FlagCalculations.GetSymbolsDictionary();

            dict.Keys.Should().NotContain("All");
            dict.Keys.Should().NotContain("None");
            dict.Keys.Should().Contain("Context");
            dict.Keys.Should().Contain("Reminder");
            dict.Values.Should().OnlyContain(v => v == false);
            dict.Keys.Should().BeInAscendingOrder();
            // 15 enum members minus All and None = 13 selectable flags.
            dict.Should().HaveCount(13);
        }

        [TestMethod]
        public void ConvertFlagStringsToEnum_EmptyList_ReturnsAll()
        {
            FlagCalculations
                .ConvertFlagStringsToEnum(new List<string>())
                .Should()
                .Be(Enums.FlagsToSet.All);
        }

        [TestMethod]
        public void ConvertFlagStringsToEnum_ValidStrings_ReturnsBitwiseOr()
        {
            FlagCalculations
                .ConvertFlagStringsToEnum(new List<string> { "Context", "People" })
                .Should()
                .Be(Enums.FlagsToSet.Context | Enums.FlagsToSet.People);
        }

        [TestMethod]
        public void ConvertFlagStringsToEnum_InvalidStringsIgnored()
        {
            FlagCalculations
                .ConvertFlagStringsToEnum(new List<string> { "Context", "NotARealFlag" })
                .Should()
                .Be(Enums.FlagsToSet.Context);
        }

        [TestMethod]
        public void GetFlagsToSet_SingleSelection_ReturnsAll_WithoutInvokingSelector()
        {
            var selectorInvoked = false;
            List<string> Selector(SortedDictionary<string, bool> options)
            {
                selectorInvoked = true;
                return new List<string>();
            }

            FlagCalculations.GetFlagsToSet(1, Selector).Should().Be(Enums.FlagsToSet.All);
            selectorInvoked.Should().BeFalse();
        }

        [TestMethod]
        public void GetFlagsToSet_MultipleSelection_UsesSelectorResult()
        {
            List<string> Selector(SortedDictionary<string, bool> options) =>
                new List<string> { "Context", "Topics" };

            FlagCalculations
                .GetFlagsToSet(2, Selector)
                .Should()
                .Be(Enums.FlagsToSet.Context | Enums.FlagsToSet.Topics);
        }

        [TestMethod]
        public void GetFlagsToSet_MultipleSelection_PassesSymbolsDictionaryToSelector()
        {
            SortedDictionary<string, bool> captured = null;
            List<string> Selector(SortedDictionary<string, bool> options)
            {
                captured = options;
                return new List<string>();
            }

            // Empty selection converts to All per ConvertFlagStringsToEnum.
            FlagCalculations.GetFlagsToSet(3, Selector).Should().Be(Enums.FlagsToSet.All);
            captured.Should().NotBeNull();
            captured.Keys.Should().Contain("Context");
            captured.Keys.Should().NotContain("All");
        }
    }
}
