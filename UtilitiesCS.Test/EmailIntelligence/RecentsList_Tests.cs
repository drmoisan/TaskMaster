using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class RecentsList_Tests
    {
        [TestMethod]
        public void DefaultConstructor_ShouldCreateEmptyListWithDefaultMax()
        {
            var list = new RecentsList<string>();

            list.Count.Should().Be(0);
            list.Max.Should().Be(5);
        }

        [TestMethod]
        public void Constructor_WithList_ShouldInitializeWithItems()
        {
            var items = new List<string> { "a", "b", "c" };
            var list = new RecentsList<string>(items, 10);

            list.Count.Should().Be(3);
            list.Max.Should().Be(10);
        }

        [TestMethod]
        public void Constructor_WithEnumerable_ShouldInitializeWithItems()
        {
            IEnumerable<string> items = new[] { "x", "y" };
            var list = new RecentsList<string>(items, 3);

            list.Count.Should().Be(2);
            list.Max.Should().Be(3);
        }

        [TestMethod]
        public void Max_SetAndGet_ShouldWork()
        {
            var list = new RecentsList<string>();

            list.Max = 20;

            list.Max.Should().Be(20);
        }

        [TestMethod]
        public void Constructor_WithEnumerableAndMax_ShouldSetMax()
        {
            // The filename/folderpath constructor attempts file-system deserialization,
            // so use the IEnumerable constructor instead for deterministic testing.
            var list = new RecentsList<string>(new[] { "a", "b" }, 7);

            list.Max.Should().Be(7);
            list.Count.Should().Be(2);
        }
    }
}
