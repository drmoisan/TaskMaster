using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskVisualization;

namespace TaskVisualization.Test
{
    /// <summary>
    /// Unit tests for the <see cref="FlagChangeItem"/> POCO. No form, popup, sleep,
    /// timer, or temp file is used.
    /// </summary>
    [TestClass]
    public class FlagChangeItemTests
    {
        [TestMethod]
        public void DefaultConstruction_FlagListsAreEmptyAndNonNull()
        {
            var item = new FlagChangeItem();

            item.UntrainFlags.Should().NotBeNull().And.BeEmpty();
            item.TrainFlags.Should().NotBeNull().And.BeEmpty();
            item.ClassifierName.Should().BeNull();
        }

        [TestMethod]
        public void Properties_RoundTrip()
        {
            var item = new FlagChangeItem
            {
                ClassifierName = "People",
                UntrainFlags = new List<string> { "old" },
                TrainFlags = new List<string> { "new1", "new2" },
            };

            item.ClassifierName.Should().Be("People");
            item.UntrainFlags.Should().ContainSingle().Which.Should().Be("old");
            item.TrainFlags.Should().BeEquivalentTo(new[] { "new1", "new2" });
        }
    }
}
