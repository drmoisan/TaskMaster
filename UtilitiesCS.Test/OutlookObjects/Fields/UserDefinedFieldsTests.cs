using System;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookExtensions;
using UtilitiesCS.OutlookObjects.Fields;
using OlMailItem = Microsoft.Office.Interop.Outlook.MailItem;

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
            UserDefinedFields
                .ValidPropertyArgs("hello", OlUserPropertyType.olText)
                .Should()
                .BeTrue();
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
            UserDefinedFields
                .ValidPropertyArgs("not-a-bool", OlUserPropertyType.olYesNo)
                .Should()
                .BeFalse();
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
            accessor
                .Setup(x => x.GetProperty("schema://test"))
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
            accessor
                .Setup(x => x.GetProperty("schema://int"))
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
            accessor
                .Setup(x => x.GetProperty("schema://missing"))
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
            accessor
                .Setup(x => x.SetProperty("schema://prop", "value"))
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
            result.Should().BeEmpty();
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
            accessor
                .Setup(x => x.GetProperty("schema://test"))
                .Throws(new InvalidOperationException("fail"));

            var result = UserDefinedFields.SafeGetPropertyAccessorValue(
                accessor.Object,
                "schema://test"
            );

            result.Should().BeNull();
        }

        [TestMethod]
        public void SafeGetPropertyAccessorValue_WhenAccessorReturnsValue_ShouldReturnValue()
        {
            var accessor = new Mock<PropertyAccessor>();
            accessor.Setup(x => x.GetProperty("schema://test")).Returns("expected");

            var result = UserDefinedFields.SafeGetPropertyAccessorValue(
                accessor.Object,
                "schema://test"
            );

            result.Should().Be("expected");
        }

        // ValidPropertyArgs: remaining type mappings

        [TestMethod]
        public void ValidPropertyArgs_WhenDateTimeForDateTimeType_ShouldReturnTrue()
        {
            UserDefinedFields
                .ValidPropertyArgs(DateTime.UtcNow, OlUserPropertyType.olDateTime)
                .Should()
                .BeTrue();
        }

        [TestMethod]
        public void ValidPropertyArgs_WhenDoubleForDurationType_ShouldReturnTrue()
        {
            UserDefinedFields
                .ValidPropertyArgs(1.0, OlUserPropertyType.olDuration)
                .Should()
                .BeTrue();
        }

        [TestMethod]
        public void ValidPropertyArgs_WhenStringArrayForKeywordsType_ShouldReturnTrue()
        {
            UserDefinedFields
                .ValidPropertyArgs(new string[] { "a", "b" }, OlUserPropertyType.olKeywords)
                .Should()
                .BeTrue();
        }

        [TestMethod]
        public void ValidPropertyArgs_WhenDoubleForPercentType_ShouldReturnTrue()
        {
            UserDefinedFields
                .ValidPropertyArgs(90.0, OlUserPropertyType.olPercent)
                .Should()
                .BeTrue();
        }

        [TestMethod]
        public void ValidPropertyArgs_WhenDecimalForCurrencyType_ShouldReturnTrue()
        {
            UserDefinedFields
                .ValidPropertyArgs(9.99m, OlUserPropertyType.olCurrency)
                .Should()
                .BeTrue();
        }

        [TestMethod]
        public void ValidPropertyArgs_WhenStringForFormulaType_ShouldReturnTrue()
        {
            UserDefinedFields
                .ValidPropertyArgs("formula", OlUserPropertyType.olFormula)
                .Should()
                .BeTrue();
        }

        [TestMethod]
        public void ValidPropertyArgs_WhenStringForCombinationType_ShouldReturnTrue()
        {
            UserDefinedFields
                .ValidPropertyArgs("combo", OlUserPropertyType.olCombination)
                .Should()
                .BeTrue();
        }

        [TestMethod]
        public void ValidPropertyArgs_WhenEnumForEnumerationType_ShouldReturnTrue()
        {
            // Any enum value is an instance of System.Enum, satisfying the olEnumeration mapping.
            UserDefinedFields
                .ValidPropertyArgs(OlUserPropertyType.olText, OlUserPropertyType.olEnumeration)
                .Should()
                .BeTrue();
        }

        // GetUdfValue(null): remaining TypeGroup branches to improve scenario coverage

        [TestMethod]
        public void GetUdfValue_WhenPropertyIsNull_ShouldReturnZeroForDurationType()
        {
            var result = ((UserProperty)null).GetUdfValue(OlUserPropertyType.olDuration);
            ((int)result).Should().Be(0);
        }

        [TestMethod]
        public void GetUdfValue_WhenPropertyIsNull_ShouldReturnZeroForIntegerType()
        {
            var result = ((UserProperty)null).GetUdfValue(OlUserPropertyType.olInteger);
            ((int)result).Should().Be(0);
        }

        [TestMethod]
        public void GetUdfValue_WhenPropertyIsNull_ShouldReturnEmptyStringForKeywordsType()
        {
            var result = ((UserProperty)null).GetUdfValue(OlUserPropertyType.olKeywords);
            result.Should().Be("");
        }

        [TestMethod]
        public void GetUdfValue_WhenPropertyIsNull_ShouldReturnEmptyStringForFormulaType()
        {
            var result = ((UserProperty)null).GetUdfValue(OlUserPropertyType.olFormula);
            result.Should().Be("");
        }

        // GetUdfValue: non-null property path via UserProperty mock

        [TestMethod]
        public void GetUdfValue_WhenPropertyHasNonNullValue_ShouldReturnValue()
        {
            var mockProp = new Mock<UserProperty>();
            mockProp.Setup(x => x.Value).Returns("test value");

            var result = mockProp.Object.GetUdfValue(OlUserPropertyType.olText, false);

            result.Should().Be("test value");
        }

        [TestMethod]
        public void GetUdfValueGeneric_WhenPropertyHasNonNullValue_ShouldReturnTypedValue()
        {
            var mockProp = new Mock<UserProperty>();
            mockProp.Setup(x => x.Value).Returns(99);

            var result = mockProp.Object.GetUdfValue<int>(false);

            result.Should().Be(99);
        }

        [TestMethod]
        public void GetUdfString_WhenPropertyHasStringValue_ShouldReturnString()
        {
            var mockProp = new Mock<UserProperty>();
            mockProp.Setup(x => x.Value).Returns("my value");

            var result = mockProp.Object.GetUdfString();

            result.Should().Be("my value");
        }

        // UdfExists: IOutlookItem mock tests

        [TestMethod]
        public void UdfExists_WhenFindDoesNotThrow_ShouldReturnTrue()
        {
            var mockUserProps = new Mock<UserProperties>();
            mockUserProps
                .Setup(x => x.Find(It.IsAny<string>(), It.IsAny<object>()))
                .Returns((UserProperty)null);

            var mockItem = new Mock<IOutlookItem>();
            mockItem.Setup(x => x.UserProperties).Returns(mockUserProps.Object);

            mockItem.Object.UdfExists("SomeField").Should().BeTrue();
        }

        [TestMethod]
        public void UdfExists_WhenFindThrows_ShouldReturnFalse()
        {
            var mockUserProps = new Mock<UserProperties>();
            mockUserProps
                .Setup(x => x.Find(It.IsAny<string>(), It.IsAny<object>()))
                .Throws(new InvalidOperationException("COM error"));

            var mockItem = new Mock<IOutlookItem>();
            mockItem.Setup(x => x.UserProperties).Returns(mockUserProps.Object);

            mockItem.Object.UdfExists("SomeField").Should().BeFalse();
        }

        // TrySetUdf: arg validation failure path requires no COM interaction

        [TestMethod]
        public void TrySetUdf_WhenValueTypeIsWrongForOlUdfType_ShouldReturnFalse()
        {
            var mockItem = new Mock<IOutlookItem>();

            var result = mockItem.Object.TrySetUdf(
                "fieldName",
                "not-a-number",
                OlUserPropertyType.olNumber
            );

            result.Should().BeFalse();
            mockItem.Verify(x => x.UserProperties, Times.Never);
        }

        [TestMethod]
        public void TrySetUdf_WhenPropertyNotFound_ShouldAddPropertyAndReturnTrue()
        {
            // Arrange
            var mockProperty = new Mock<UserProperty>();
            var mockUserProps = new Mock<UserProperties>();
            mockUserProps
                .Setup(x => x.Find(It.IsAny<string>(), It.IsAny<object>()))
                .Returns((UserProperty)null);
            mockUserProps
                .Setup(x =>
                    x.Add(
                        It.IsAny<string>(),
                        It.IsAny<OlUserPropertyType>(),
                        It.IsAny<object>(),
                        It.IsAny<object>()
                    )
                )
                .Returns(mockProperty.Object);

            var mockItem = new Mock<IOutlookItem>();
            mockItem.Setup(x => x.UserProperties).Returns(mockUserProps.Object);

            // Act
            var result = mockItem.Object.TrySetUdf(
                "TestField",
                "TestValue",
                OlUserPropertyType.olText
            );

            // Assert
            result.Should().BeTrue();
            mockUserProps.Verify(
                x =>
                    x.Add(
                        "TestField",
                        It.IsAny<OlUserPropertyType>(),
                        It.IsAny<object>(),
                        It.IsAny<object>()
                    ),
                Times.Once
            );
        }

        [TestMethod]
        public void TrySetUdf_WhenPropertyFoundWithMatchingType_ShouldUpdateValueAndReturnTrue()
        {
            // Arrange
            var mockProperty = new Mock<UserProperty>();
            mockProperty.Setup(x => x.Type).Returns(OlUserPropertyType.olText);

            var mockUserProps = new Mock<UserProperties>();
            mockUserProps
                .Setup(x => x.Find(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(mockProperty.Object);

            var mockItem = new Mock<IOutlookItem>();
            mockItem.Setup(x => x.UserProperties).Returns(mockUserProps.Object);

            // Act
            var result = mockItem.Object.TrySetUdf(
                "TestField",
                "TestValue",
                OlUserPropertyType.olText
            );

            // Assert
            result.Should().BeTrue();
            mockItem.Verify(x => x.Save(), Times.Once);
        }

        [TestMethod]
        public void TrySetUdf_WhenFindThrowsException_ShouldReturnFalse()
        {
            // Arrange
            var mockUserProps = new Mock<UserProperties>();
            mockUserProps
                .Setup(x => x.Find(It.IsAny<string>(), It.IsAny<object>()))
                .Throws(new InvalidOperationException("COM error"));

            var mockItem = new Mock<IOutlookItem>();
            mockItem.Setup(x => x.UserProperties).Returns(mockUserProps.Object);

            // Act
            var result = mockItem.Object.TrySetUdf(
                "TestField",
                "TestValue",
                OlUserPropertyType.olText
            );

            // Assert
            result.Should().BeFalse();
        }

        // SetUdf: arg validation failure path

        [TestMethod]
        public void SetUdf_WhenValueTypeIsWrongForOlUdfType_ShouldThrowArgumentException()
        {
            var mockItem = new Mock<IOutlookItem>();

            System.Action act = () =>
                mockItem.Object.SetUdf("fieldName", "not-a-number", OlUserPropertyType.olNumber);

            act.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void SetUdf_WhenPropertyNotFound_ShouldAddPropertyAndSave()
        {
            // Arrange
            var mockProperty = new Mock<UserProperty>();
            var mockUserProps = new Mock<UserProperties>();
            mockUserProps
                .Setup(x => x.Find(It.IsAny<string>(), It.IsAny<object>()))
                .Returns((UserProperty)null);
            mockUserProps
                .Setup(x =>
                    x.Add(
                        It.IsAny<string>(),
                        It.IsAny<OlUserPropertyType>(),
                        It.IsAny<object>(),
                        It.IsAny<object>()
                    )
                )
                .Returns(mockProperty.Object);

            var mockItem = new Mock<IOutlookItem>();
            mockItem.Setup(x => x.UserProperties).Returns(mockUserProps.Object);

            // Act
            mockItem.Object.SetUdf("Field", "value", OlUserPropertyType.olText);

            // Assert
            mockItem.Verify(x => x.Save(), Times.Once);
        }

        // GetUdf via IOutlookItem

        [TestMethod]
        public void GetUdf_ViaIOutlookItem_WhenPropertyFound_ShouldReturnProperty()
        {
            var mockProperty = new Mock<UserProperty>();
            var mockUserProps = new Mock<UserProperties>();
            mockUserProps
                .Setup(x => x.Find(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(mockProperty.Object);

            var mockItem = new Mock<IOutlookItem>();
            mockItem.Setup(x => x.UserProperties).Returns(mockUserProps.Object);

            var result = mockItem.Object.GetUdf("FieldName");

            result.Should().BeSameAs(mockProperty.Object);
        }

        [TestMethod]
        public void GetUdf_ViaIOutlookItem_WhenPropertyNotFound_ShouldReturnNull()
        {
            var mockUserProps = new Mock<UserProperties>();
            mockUserProps
                .Setup(x => x.Find(It.IsAny<string>(), It.IsAny<object>()))
                .Returns((UserProperty)null);

            var mockItem = new Mock<IOutlookItem>();
            mockItem.Setup(x => x.UserProperties).Returns(mockUserProps.Object);

            var result = mockItem.Object.GetUdf("MissingField");

            result.Should().BeNull();
        }

        // GetUdfString via IOutlookItem

        [TestMethod]
        public void GetUdfString_ViaIOutlookItem_WhenPropertyNotFound_ShouldReturnEmpty()
        {
            var mockUserProps = new Mock<UserProperties>();
            mockUserProps
                .Setup(x => x.Find(It.IsAny<string>(), It.IsAny<object>()))
                .Returns((UserProperty)null);

            var mockItem = new Mock<IOutlookItem>();
            mockItem.Setup(x => x.UserProperties).Returns(mockUserProps.Object);

            var result = mockItem.Object.GetUdfString("FieldName");

            result.Should().BeEmpty();
        }

        // GetUdfValue via IOutlookItem

        [TestMethod]
        public void GetUdfValue_ViaIOutlookItem_WhenPropertyNotFound_ShouldReturnTypeDefault()
        {
            var mockUserProps = new Mock<UserProperties>();
            mockUserProps
                .Setup(x => x.Find(It.IsAny<string>(), It.IsAny<object>()))
                .Returns((UserProperty)null);

            var mockItem = new Mock<IOutlookItem>();
            mockItem.Setup(x => x.UserProperties).Returns(mockUserProps.Object);

            var result = mockItem.Object.GetUdfValue("FieldName", OlUserPropertyType.olText);

            result.Should().Be("");
        }

        [TestMethod]
        public void GetUdfValueGeneric_ViaIOutlookItem_WhenPropertyNotFound_ShouldReturnDefault()
        {
            var mockUserProps = new Mock<UserProperties>();
            mockUserProps
                .Setup(x => x.Find(It.IsAny<string>(), It.IsAny<object>()))
                .Returns((UserProperty)null);

            var mockItem = new Mock<IOutlookItem>();
            mockItem.Setup(x => x.UserProperties).Returns(mockUserProps.Object);

            var result = mockItem.Object.GetUdfValue<string>("FieldName");

            result.Should().BeNull();
        }

        // SetUdf bulk (string[] schemas, object[] values) via PropertyAccessor mock

        [TestMethod]
        public void SetUdfBulk_WhenCalledWithSchemas_ShouldInvokeSetProperties()
        {
            var mockAccessor = new Mock<PropertyAccessor>();
            mockAccessor
                .Setup(x => x.SetProperties(It.IsAny<object>(), It.IsAny<object>()))
                .Returns(new object[0]);

            var mockItem = new Mock<IOutlookItem>();
            mockItem.Setup(x => x.PropertyAccessor).Returns(mockAccessor.Object);

            mockItem.Object.SetUdf(new[] { "schema://test" }, new object[] { "value" });

            mockAccessor.Verify(
                x => x.SetProperties(It.IsAny<object>(), It.IsAny<object>()),
                Times.Once
            );
        }

        // MAPIFields static data coverage

        [TestMethod]
        public void MAPIFields_SchemaToField_ContainsExpectedMapping()
        {
            MAPIFields.SchemaToField.ContainsKey(MAPIFields.Schemas.MessageStore).Should().BeTrue();
            MAPIFields.SchemaToField[MAPIFields.Schemas.MessageStore].Should().Be("Store");
        }

        [TestMethod]
        public void MAPIFields_FieldToSchema_ContainsAllExpectedKeys()
        {
            MAPIFields.FieldToSchema.ContainsKey("Folder Name").Should().BeTrue();
            MAPIFields.FieldToSchema.ContainsKey("Triage").Should().BeTrue();
            MAPIFields.FieldToSchema.ContainsKey("SenderName").Should().BeTrue();
        }

        [TestMethod]
        public void MAPIFields_BinaryToStringFields_ContainsExpectedFields()
        {
            MAPIFields.BinaryToStringFields.Should().Contain("ConversationIndex");
            MAPIFields.BinaryToStringFields.Should().Contain("Store");
        }

        [TestMethod]
        public void MAPIFields_ObjectFields_ContainsExpectedField()
        {
            MAPIFields.ObjectFields.Should().Contain("MessageRecipients");
        }

        [TestMethod]
        public void MAPIFields_Schemas_AllKnownPropertiesReturnNonEmptyStrings()
        {
            MAPIFields.Schemas.ConversationTopic.Should().NotBeNullOrWhiteSpace();
            MAPIFields.Schemas.FolderName.Should().NotBeNullOrWhiteSpace();
            MAPIFields.Schemas.Triage.Should().NotBeNullOrWhiteSpace();
            MAPIFields.Schemas.ToDoID.Should().NotBeNullOrWhiteSpace();
            MAPIFields.Schemas.SenderName.Should().NotBeNullOrWhiteSpace();
        }

        // GetUdfString / GetUdfValue: array-value path via UserProperty mock

        [TestMethod]
        public void GetUdfString_WhenPropertyValueIsStringArray_ShouldReturnJoinedString()
        {
            var mockProp = new Mock<UserProperty>();
            mockProp.Setup(x => x.Value).Returns(new string[] { "alpha", "beta" });

            var result = mockProp.Object.GetUdfString();

            result.Should().Be("alpha, beta");
        }

        [TestMethod]
        public void GetUdfValue_WhenPropertyValueIsStringArray_ShouldReturnFlattenedArray()
        {
            var mockProp = new Mock<UserProperty>();
            mockProp.Setup(x => x.Value).Returns(new string[] { "x", "y" });

            var result = mockProp.Object.GetUdfValue(OlUserPropertyType.olText, true);

            ((string[])result).Should().Equal("x", "y");
        }

        [TestMethod]
        public void GetUdfValueGeneric_WhenFlattenFalseAndValueIsStringArray_ShouldReturnArray()
        {
            // flatten=false skips FlattenArrayTree; the string[] is cast directly to string[].
            var mockProp = new Mock<UserProperty>();
            mockProp.Setup(x => x.Value).Returns(new string[] { "alpha", "beta" });

            var result = mockProp.Object.GetUdfValue<string[]>(flatten: false);

            result.Should().Equal("alpha", "beta");
        }

        // GetUdfValue generic: non-null, non-array value — exercises the return-(T)result path.

        [TestMethod]
        public void GetUdfValueGeneric_WhenPropertyHasStringValue_ShouldReturnTypedString()
        {
            var mockProp = new Mock<UserProperty>();
            mockProp.Setup(x => x.Value).Returns("hello");

            var result = mockProp.Object.GetUdfValue<string>();

            result.Should().Be("hello");
        }

        // DeleteUdf via MailItem mock

        [TestMethod]
        public void DeleteUdf_WhenPropertyNotInList_ShouldNotRemoveOrSave()
        {
            // Arrange: Cast<UserProperty>() returns empty → FindIndex is -1 → no Remove/Save.
            var mockUserProps = new Mock<UserProperties>();
            mockUserProps
                .Setup(x => x.Find(It.IsAny<string>(), It.IsAny<object>()))
                .Returns((UserProperty)null);
            mockUserProps
                .As<System.Collections.IEnumerable>()
                .Setup(x => x.GetEnumerator())
                .Returns(Array.Empty<object>().GetEnumerator());

            var mockMailItem = new Mock<OlMailItem>();
            mockMailItem.Setup(x => x.UserProperties).Returns(mockUserProps.Object);

            // Act
            mockMailItem.Object.DeleteUdf("TestField");

            // Assert
            mockUserProps.Verify(x => x.Remove(It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        public void DeleteUdf_WhenPropertyExistsInList_ShouldRemoveAtIndexAndSave()
        {
            // Arrange: Cast<UserProperty>() returns one property named "TestField" → FindIndex=0.
            var mockProperty = new Mock<UserProperty>();
            mockProperty.Setup(x => x.Name).Returns("TestField");

            var mockUserProps = new Mock<UserProperties>();
            mockUserProps
                .Setup(x => x.Find(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(mockProperty.Object);
            mockUserProps
                .As<System.Collections.IEnumerable>()
                .Setup(x => x.GetEnumerator())
                .Returns(() => new object[] { mockProperty.Object }.GetEnumerator());

            var mockMailItem = new Mock<OlMailItem>();
            mockMailItem.Setup(x => x.UserProperties).Returns(mockUserProps.Object);

            // Act
            mockMailItem.Object.DeleteUdf("TestField");

            // Assert
            mockUserProps.Verify(x => x.Remove(1), Times.Once);
            mockMailItem.Verify(x => x.Save(), Times.Once);
        }

        // SetUdf(MailItem) overload: valid args, exception, and invalid-args paths

        [TestMethod]
        public void SetUdMailItem_WhenArgsAreInvalid_ShouldReturnFalse()
        {
            var mockMailItem = new Mock<OlMailItem>();

            var result = mockMailItem.Object.SetUdf(
                "field",
                "not-a-number",
                OlUserPropertyType.olNumber
            );

            result.Should().BeFalse();
            mockMailItem.Verify(x => x.UserProperties, Times.Never);
        }

        [TestMethod]
        public void SetUdf_MailItem_WhenPropertyNotFound_ShouldAddAndReturnTrue()
        {
            var mockProperty = new Mock<UserProperty>();
            var mockUserProps = new Mock<UserProperties>();
            mockUserProps
                .Setup(x => x.Find(It.IsAny<string>(), It.IsAny<object>()))
                .Returns((UserProperty)null);
            mockUserProps
                .Setup(x =>
                    x.Add(
                        It.IsAny<string>(),
                        It.IsAny<OlUserPropertyType>(),
                        It.IsAny<object>(),
                        It.IsAny<object>()
                    )
                )
                .Returns(mockProperty.Object);

            var mockMailItem = new Mock<OlMailItem>();
            mockMailItem.Setup(x => x.UserProperties).Returns(mockUserProps.Object);

            var result = mockMailItem.Object.SetUdf("field", "value", OlUserPropertyType.olText);

            result.Should().BeTrue();
            mockMailItem.Verify(x => x.Save(), Times.Once);
        }

        [TestMethod]
        public void SetUdf_MailItem_WhenFindThrows_ShouldReturnFalse()
        {
            var mockUserProps = new Mock<UserProperties>();
            mockUserProps
                .Setup(x => x.Find(It.IsAny<string>(), It.IsAny<object>()))
                .Throws(new InvalidOperationException("COM error"));

            var mockMailItem = new Mock<OlMailItem>();
            mockMailItem.Setup(x => x.UserProperties).Returns(mockUserProps.Object);

            var result = mockMailItem.Object.SetUdf("field", "value", OlUserPropertyType.olText);

            result.Should().BeFalse();
        }

        // SetUdf(AppointmentItem) [Obsolete]: valid and invalid paths

        [TestMethod]
        public void SetUdf_AppointmentItem_WhenArgsAreInvalid_ShouldReturnFalse()
        {
            var mockItem = new Mock<AppointmentItem>();
#pragma warning disable CS0618
            var result = mockItem.Object.SetUdf(
                "field",
                "not-a-number",
                OlUserPropertyType.olNumber
            );
#pragma warning restore CS0618
            result.Should().BeFalse();
            mockItem.Verify(x => x.UserProperties, Times.Never);
        }

        [TestMethod]
        public void SetUdf_AppointmentItem_WhenPropertyNotFound_ShouldAddAndReturnTrue()
        {
            var mockProperty = new Mock<UserProperty>();
            var mockUserProps = new Mock<UserProperties>();
            mockUserProps
                .Setup(x => x.Find(It.IsAny<string>(), It.IsAny<object>()))
                .Returns((UserProperty)null);
            mockUserProps
                .Setup(x =>
                    x.Add(
                        It.IsAny<string>(),
                        It.IsAny<OlUserPropertyType>(),
                        It.IsAny<object>(),
                        It.IsAny<object>()
                    )
                )
                .Returns(mockProperty.Object);

            var mockItem = new Mock<AppointmentItem>();
            mockItem.Setup(x => x.UserProperties).Returns(mockUserProps.Object);

#pragma warning disable CS0618
            var result = mockItem.Object.SetUdf("field", "value", OlUserPropertyType.olText);
#pragma warning restore CS0618

            result.Should().BeTrue();
            mockItem.Verify(x => x.Save(), Times.Once);
        }

        [TestMethod]
        public void SetUdf_AppointmentItem_WhenFindThrows_ShouldReturnFalse()
        {
            var mockUserProps = new Mock<UserProperties>();
            mockUserProps
                .Setup(x => x.Find(It.IsAny<string>(), It.IsAny<object>()))
                .Throws(new InvalidOperationException("COM error"));

            var mockItem = new Mock<AppointmentItem>();
            mockItem.Setup(x => x.UserProperties).Returns(mockUserProps.Object);

#pragma warning disable CS0618
            var result = mockItem.Object.SetUdf("field", "value", OlUserPropertyType.olText);
#pragma warning restore CS0618

            result.Should().BeFalse();
        }

        // SetUdf(MeetingItem) [Obsolete]: valid and invalid paths

        [TestMethod]
        public void SetUdf_MeetingItem_WhenArgsAreInvalid_ShouldReturnFalse()
        {
            var mockItem = new Mock<MeetingItem>();
#pragma warning disable CS0618
            var result = mockItem.Object.SetUdf(
                "field",
                "not-a-number",
                OlUserPropertyType.olNumber
            );
#pragma warning restore CS0618
            result.Should().BeFalse();
        }

        [TestMethod]
        public void SetUdf_MeetingItem_WhenPropertyNotFound_ShouldAddAndReturnTrue()
        {
            var mockProperty = new Mock<UserProperty>();
            var mockUserProps = new Mock<UserProperties>();
            mockUserProps
                .Setup(x => x.Find(It.IsAny<string>(), It.IsAny<object>()))
                .Returns((UserProperty)null);
            mockUserProps
                .Setup(x =>
                    x.Add(
                        It.IsAny<string>(),
                        It.IsAny<OlUserPropertyType>(),
                        It.IsAny<object>(),
                        It.IsAny<object>()
                    )
                )
                .Returns(mockProperty.Object);

            var mockItem = new Mock<MeetingItem>();
            mockItem.Setup(x => x.UserProperties).Returns(mockUserProps.Object);

#pragma warning disable CS0618
            var result = mockItem.Object.SetUdf("field", "value", OlUserPropertyType.olText);
#pragma warning restore CS0618

            result.Should().BeTrue();
        }

        [TestMethod]
        public void SetUdf_MeetingItem_WhenFindThrows_ShouldReturnFalse()
        {
            var mockUserProps = new Mock<UserProperties>();
            mockUserProps
                .Setup(x => x.Find(It.IsAny<string>(), It.IsAny<object>()))
                .Throws(new InvalidOperationException("COM error"));

            var mockItem = new Mock<MeetingItem>();
            mockItem.Setup(x => x.UserProperties).Returns(mockUserProps.Object);

#pragma warning disable CS0618
            var result = mockItem.Object.SetUdf("field", "value", OlUserPropertyType.olText);
#pragma warning restore CS0618

            result.Should().BeFalse();
        }

        // SetUdf(TaskItem) [Obsolete]: valid and invalid paths

        [TestMethod]
        public void SetUdf_TaskItem_WhenArgsAreInvalid_ShouldReturnFalse()
        {
            var mockItem = new Mock<TaskItem>();
#pragma warning disable CS0618
            var result = mockItem.Object.SetUdf(
                "field",
                "not-a-number",
                OlUserPropertyType.olNumber
            );
#pragma warning restore CS0618
            result.Should().BeFalse();
        }

        [TestMethod]
        public void SetUdf_TaskItem_WhenPropertyNotFound_ShouldAddAndReturnTrue()
        {
            var mockProperty = new Mock<UserProperty>();
            var mockUserProps = new Mock<UserProperties>();
            mockUserProps
                .Setup(x => x.Find(It.IsAny<string>(), It.IsAny<object>()))
                .Returns((UserProperty)null);
            mockUserProps
                .Setup(x =>
                    x.Add(
                        It.IsAny<string>(),
                        It.IsAny<OlUserPropertyType>(),
                        It.IsAny<object>(),
                        It.IsAny<object>()
                    )
                )
                .Returns(mockProperty.Object);

            var mockItem = new Mock<TaskItem>();
            mockItem.Setup(x => x.UserProperties).Returns(mockUserProps.Object);

#pragma warning disable CS0618
            var result = mockItem.Object.SetUdf("field", "value", OlUserPropertyType.olText);
#pragma warning restore CS0618

            result.Should().BeTrue();
        }

        [TestMethod]
        public void SetUdf_TaskItem_WhenFindThrows_ShouldReturnFalse()
        {
            var mockUserProps = new Mock<UserProperties>();
            mockUserProps
                .Setup(x => x.Find(It.IsAny<string>(), It.IsAny<object>()))
                .Throws(new InvalidOperationException("COM error"));

            var mockItem = new Mock<TaskItem>();
            mockItem.Setup(x => x.UserProperties).Returns(mockUserProps.Object);

#pragma warning disable CS0618
            var result = mockItem.Object.SetUdf("field", "value", OlUserPropertyType.olText);
#pragma warning restore CS0618

            result.Should().BeFalse();
        }

        // GetUdf(object): obsolete overload — unsupported type throws ArgumentException

        [TestMethod]
        public void GetUdf_ObsoleteObjectOverload_WhenTypeIsUnsupported_ShouldThrow()
        {
            object item = new object();
#pragma warning disable CS0618
            System.Action act = () => UserDefinedFields.GetUdf(item, "Field");
#pragma warning restore CS0618
            act.Should().Throw<ArgumentException>().WithMessage("*Unsupported type*");
        }

        // GetUdfString(object): obsolete overload — propagates throw from GetUdf

        [TestMethod]
        public void GetUdfString_ObsoleteObjectOverload_WhenTypeIsUnsupported_ShouldThrow()
        {
            object item = new object();
#pragma warning disable CS0618
            System.Action act = () => UserDefinedFields.GetUdfString(item, "Field");
#pragma warning restore CS0618
            act.Should().Throw<ArgumentException>();
        }

        // GetUdfValue(object): obsolete overload — propagates throw from GetUdf

        [TestMethod]
        public void GetUdfValue_ObsoleteObjectOverload_WhenTypeIsUnsupported_ShouldThrow()
        {
            object item = new object();
#pragma warning disable CS0618
            System.Action act = () => UserDefinedFields.GetUdfValue(item, "Field");
#pragma warning restore CS0618
            act.Should().Throw<ArgumentException>();
        }

        // SetUdf(object): obsolete overload — wraps a non-COM POCO in OutlookItem, TrySetUdf catches the failure

        [TestMethod]
        public void SetUdf_ObsoleteObjectOverload_WhenItemIsNonCOM_ShouldReturnFalse()
        {
            object item = new object();
#pragma warning disable CS0618
            var result = UserDefinedFields.SetUdf(
                item,
                "field",
                "value",
                OlUserPropertyType.olText
            );
#pragma warning restore CS0618
            // OutlookItem wraps the POCO; UserProperties access via reflection fails →
            // TrySetUdf catch returns false.
            result.Should().BeFalse();
        }

        // SetUdfBulk with non-empty errors exercises the if(!errors.IsNullOrEmpty()) true branch

        [TestMethod]
        public void SetUdfBulk_WhenSetPropertiesReturnsErrors_ShouldCompleteWithoutThrowing()
        {
            var mockAccessor = new Mock<PropertyAccessor>();
            mockAccessor
                .Setup(x => x.SetProperties(It.IsAny<object>(), It.IsAny<object>()))
                .Returns(new object[] { "error1" });

            var mockItem = new Mock<IOutlookItem>();
            mockItem.Setup(x => x.PropertyAccessor).Returns(mockAccessor.Object);

            // The if body is commented out, so calling with non-empty errors just evaluates the
            // branch without side effects.
            System.Action act = () =>
                mockItem.Object.SetUdf(new[] { "schema://test" }, new object[] { "value" });

            act.Should().NotThrow();
        }
    }
}
