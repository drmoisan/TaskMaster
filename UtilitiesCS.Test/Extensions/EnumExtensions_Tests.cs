using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;

namespace UtilitiesCS.Test.Extensions
{
    [TestClass]
    public class EnumExtensions_Tests
    {
        [Flags]
        private enum SampleFlags
        {
            None = 0,
            First = 1,
            Second = 2,
            Third = 4,
        }

        private enum BaseOneItem
        {
            Default = 0,
            Value = 1,
        }

        [TestMethod]
        public void HasAnyFlags_ReturnsExpectedResultsForDefinedAndUndefinedValues()
        {
            // Arrange
            var combined = SampleFlags.First | SampleFlags.Third;
            var undefined = (SampleFlags)8;

            // Act / Assert
            combined.HasAnyFlags(SampleFlags.First, SampleFlags.Second).Should().BeTrue();
            combined.HasAnyFlags(SampleFlags.Second).Should().BeFalse();
            undefined.HasAnyFlags(SampleFlags.First, SampleFlags.Second).Should().BeFalse();
            SampleFlags.None.HasAnyFlags(SampleFlags.None).Should().BeTrue();
        }

        [TestMethod]
        public void HasAllFlags_ReturnsFalseForNullOrEmptyAndTrueWhenAllFlagsPresent()
        {
            // Arrange
            var combined = SampleFlags.First | SampleFlags.Second | SampleFlags.Third;
            SampleFlags[] noFlags = null;

            // Act / Assert
            combined.HasAllFlags(SampleFlags.First, SampleFlags.Third).Should().BeTrue();
            combined
                .HasAllFlags(SampleFlags.First, SampleFlags.Second, SampleFlags.Third)
                .Should()
                .BeTrue();
            combined
                .HasAllFlags(SampleFlags.First, SampleFlags.Second, (SampleFlags)8)
                .Should()
                .BeFalse();
            combined.HasAllFlags().Should().BeFalse();
            combined.HasAllFlags(noFlags).Should().BeFalse();
        }

        [TestMethod]
        public void AddFlagsAndToCombined_CombineEnumeratedFlagsIntoSingleValue()
        {
            // Arrange
            IEnumerable<SampleFlags> values = new[]
            {
                SampleFlags.First,
                SampleFlags.Second,
                SampleFlags.Third,
            };

            // Act
            var addFlagsResult = values.AddFlags();
            var combinedResult = values.ToCombined();

            // Assert
            addFlagsResult.Should().Be(SampleFlags.First | SampleFlags.Second | SampleFlags.Third);
            combinedResult.Should().Be(SampleFlags.First | SampleFlags.Second | SampleFlags.Third);
        }

        [TestMethod]
        public void ToArray_WhenBase1SimulationIsEnabled_PrependsDefaultValue()
        {
            // Arrange
            IList<BaseOneItem> values = new List<BaseOneItem> { BaseOneItem.Value };

            // Act
            var simulated = values.ToArray(Base1Simulation: true);
            var regular = new List<BaseOneItem> { BaseOneItem.Value }.ToArray(
                Base1Simulation: false
            );

            // Assert
            simulated.Should().Equal(BaseOneItem.Default, BaseOneItem.Value);
            regular.Should().Equal(BaseOneItem.Value);
        }
    }
}
