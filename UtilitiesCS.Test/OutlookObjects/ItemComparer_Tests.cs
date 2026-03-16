using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class ItemComparer_Tests
    {
        [TestMethod]
        public void ItemComparer_ShouldNotBeLoadable_BecauseProductionFileContainsNoLiveType()
        {
            // Arrange
            Type itemComparerType = typeof(OutlookItem).Assembly.GetType("UtilitiesCS.ItemComparer");

            // Act / Assert
            itemComparerType.Should().BeNull();
        }
    }
}
