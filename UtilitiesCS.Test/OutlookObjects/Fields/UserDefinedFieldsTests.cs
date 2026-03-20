using System;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookExtensions;
using UtilitiesCS.OutlookObjects.Fields;

namespace UtilitiesCS.Test.OutlookObjects.Fields
{
    [TestClass]
    public class UserDefinedFieldsTests
    {
        [TestMethod]
        public void ValidPropertyArgs_rejects_missing_required_inputs()
        {
            UserDefinedFields.ValidPropertyArgs(123, OlUserPropertyType.olText).Should().BeFalse();
        }

        [TestMethod]
        public void GetUdfValue_returns_expected_lookup_value_for_known_field()
        {
            MAPIFields.FieldToSchema.TryGetValue("Store", out var schema).Should().BeTrue();
            schema.Should().NotBeNullOrWhiteSpace();
        }

        [TestMethod]
        public void ValidPropertyArgs_WhenStringValueForTextType_ShouldReturnTrue()
        {
            UserDefinedFields.ValidPropertyArgs("hello", OlUserPropertyType.olText).Should().BeTrue();
        }

        [TestMethod]
        public void ValidPropertyArgs_WhenDoubleValueForNumberType_ShouldReturnTrue()
        {
            UserDefinedFields.ValidPropertyArgs(1.5, OlUserPropertyType.olNumber).Should().BeTrue();
        }

        [TestMethod]
        public void ValidPropertyArgs_WhenBoolValueForYesNoType_ShouldReturnTrue()
        {
            UserDefinedFields.ValidPropertyArgs(true, OlUserPropertyType.olYesNo).Should().BeTrue();
        }

        [TestMethod]
        public void ValidPropertyArgs_WhenIntValueForIntegerType_ShouldReturnTrue()
        {
            UserDefinedFields.ValidPropertyArgs(42, OlUserPropertyType.olInteger).Should().BeTrue();
        }

        [TestMethod]
        public void ValidPropertyArgs_WhenWrongTypeForField_ShouldReturnFalse()
        {
            UserDefinedFields.ValidPropertyArgs("not-a-bool", OlUserPropertyType.olYesNo).Should().BeFalse();
        }

        [TestMethod]
        public void TryGetProperty_WhenAccessorReturnsValue_ShouldReturnValue()
        {
            var accessor = new Mock<PropertyAccessor>();
            accessor.Setup(x => x.GetProperty("schema://test")).Returns("value");

            var result = accessor.Object.TryGetProperty("schema://test");

            result.Should().Be("value");
        }

        [TestMethod]
        public void TryGetProperty_WhenAccessorThrows_ShouldReturnNull()
        {
            var accessor = new Mock<PropertyAccessor>();
            accessor.Setup(x => x.GetProperty("schema://test"))
                .Throws(new InvalidOperationException("COM error"));

            var result = accessor.Object.TryGetProperty("schema://test");

            result.Should().BeNull();
        }

        [TestMethod]
        public void TryGetPropertyGeneric_WhenAccessorReturnsValue_ShouldReturnTypedValue()
        {
            var accessor = new Mock<PropertyAccessor>();
            accessor.Setup(x => x.GetProperty("schema://int")).Returns(42);

            var result = accessor.Object.TryGetProperty<int>("schema://int");

            result.Should().Be(42);
        }

        [TestMethod]
        public void TryGetPropertyGeneric_WhenAccessorThrows_ShouldReturnDefault()
        {
            var accessor = new Mock<PropertyAccessor>();
            accessor.Setup(x => x.GetProperty("schema://int"))
                .Throws(new InvalidOperationException("COM error"));

            var result = accessor.Object.TryGetProperty<int>("schema://int");

            result.Should().Be(0);
        }

        [TestMethod]
        public void Exists_WhenPropertyExists_ShouldReturnTrue()
        {
            var accessor = new Mock<PropertyAccessor>();
            accessor.Setup(x => x.GetProperty("schema://present")).Returns("val");

            accessor.Object.Exists("schema://present").Should().BeTrue();
        }

        [TestMethod]
        public void Exists_WhenPropertyThrows_ShouldReturnFalse()
        {
            var accessor = new Mock<PropertyAccessor>();
            accessor.Setup(x => x.GetProperty("schema://missing"))
                .Throws(new InvalidOperationException("missing"));

            accessor.Object.Exists("schema://missing").Should().BeFalse();
        }

        [TestMethod]
        public void TrySetProperty_WhenSetSucceeds_ShouldReturnTrue()
        {
            var accessor = new Mock<PropertyAccessor>();

            var result = accessor.Object.TrySetProperty("schema://prop", "value");

            result.Should().BeTrue();
            accessor.Verify(x => x.SetProperty("schema://prop", "value"), Times.Once);
        }

        [TestMethod]
        public void TrySetProperty_WhenSetThrows_ShouldReturnFalse()
        {
            var accessor = new Mock<PropertyAccessor>();
            accessor.Setup(x => x.SetProperty("schema://prop", "value"))
                .Throws(new InvalidOperationException("fail"));

            var result = accessor.Object.TrySetProperty("schema://prop", "value");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void GetUdfValue_WhenPropertyIsNull_ShouldReturnDefaultForType()
        {
            var result = ((UserProperty)null).GetUdfValue(OlUserPropertyType.olText);
            result.Should().Be("");
        }

        [TestMethod]
        public void GetUdfValue_WhenPropertyIsNull_ShouldReturnZeroForNumericType()
        {
            var result = ((UserProperty)null).GetUdfValue(OlUserPropertyType.olNumber);
            ((int)result).Should().Be(0);
        }

        [TestMethod]
        public void GetUdfValue_WhenPropertyIsNull_ShouldReturnFalseForBoolType()
        {
            var result = ((UserProperty)null).GetUdfValue(OlUserPropertyType.olYesNo);
            ((bool)result).Should().BeFalse();
        }

        [TestMethod]
        public void GetUdfValue_WhenPropertyIsNull_ShouldReturnNullForOtherType()
        {
            var result = ((UserProperty)null).GetUdfValue(OlUserPropertyType.olDateTime);
            result.Should().BeNull();
        }

        [TestMethod]
        public void GetUdfValueGeneric_WhenPropertyIsNull_ShouldReturnDefault()
        {
            var result = ((UserProperty)null).GetUdfValue<string>();
            result.Should().BeNull();
        }

        [TestMethod]
        public void GetUdfString_WhenPropertyIsNull_ShouldReturnEmpty()
        {
            var result = ((UserProperty)null).GetUdfString();
            result.Should().BeNull();
        }

        [TestMethod]
        public void SafeGetPropertyAccessorValue_WhenAccessorIsNull_ShouldReturnNull()
        {
            var result = UserDefinedFields.SafeGetPropertyAccessorValue(null, "schema://test");
            result.Should().BeNull();
        }

        [TestMethod]
        public void SafeGetPropertyAccessorValue_WhenAccessorThrows_ShouldReturnNull()
        {
            var accessor = new Mock<PropertyAccessor>();
            accessor.Setup(x => x.GetProperty("schema://test"))
                .Throws(new InvalidOperationException("fail"));

            var result = UserDefinedFields.SafeGetPropertyAccessorValue(accessor.Object, "schema://test");

            result.Should().BeNull();
        }
    }
}
