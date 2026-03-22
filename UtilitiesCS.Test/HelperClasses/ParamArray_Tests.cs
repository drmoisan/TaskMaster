using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.HelperClasses;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class ParamArray_Tests
    {
        [TestMethod]
        public void ParamArray_AnyNull_StaticAndInstanceHandleEmptySingleMultipleAndNullArguments()
        {
            // Arrange
            var instanceWithoutNulls = new ParamArray("alpha", 1, true);
            var instanceWithNull = new ParamArray("alpha", null, 3);

            // Act / Assert
            ParamArray.AnyNull().Should().BeFalse();
            ParamArray.AnyNull("alpha").Should().BeFalse();
            ParamArray.AnyNull("alpha", null, 3).Should().BeTrue();
            instanceWithoutNulls.AnyNull().Should().BeFalse();
            instanceWithNull.AnyNull().Should().BeTrue();
        }

        [TestMethod]
        public void ParamArrayOfT_AnyNull_StaticAndInstanceHandleReferenceTypeArguments()
        {
            // Arrange
            var instanceWithoutNulls = new ParamArray<string>("alpha", "beta");
            var instanceWithNull = new ParamArray<string>("alpha", null);

            // Act / Assert
            ParamArray<string>.AnyNull().Should().BeFalse();
            ParamArray<string>.AnyNull("alpha").Should().BeFalse();
            ParamArray<string>.AnyNull("alpha", null).Should().BeTrue();
            instanceWithoutNulls.AnyNull().Should().BeFalse();
            instanceWithNull.AnyNull().Should().BeTrue();
        }

        [TestMethod]
        public void ParamArrayOfT_AnyNullOrEmpty_StaticAndInstanceHandleEmptyAndValueTypeArguments()
        {
            // Arrange
            var emptyStrings = new ParamArray<string>();
            var populatedStrings = new ParamArray<string>("alpha");
            var integers = new ParamArray<int>(1, 2, 3);

            // Act / Assert
            ParamArray<string>.AnyNullOrEmpty().Should().BeTrue();
            ParamArray<string>.AnyNullOrEmpty("alpha").Should().BeFalse();
            emptyStrings.AnyNullOrEmpty().Should().BeTrue();
            populatedStrings.AnyNullOrEmpty().Should().BeFalse();
            ParamArray<int>.AnyNullOrEmpty(1, 2).Should().BeFalse();
            integers.AnyNullOrEmpty().Should().BeFalse();
        }
    }
}
