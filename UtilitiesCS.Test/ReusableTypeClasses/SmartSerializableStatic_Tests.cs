using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Collection;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class SmartSerializableStatic_Tests
    {
        [TestMethod]
        public void IsSmartSerializable_SmartSerializableType_ReturnsTrue()
        {
            // Arrange — SmartSerializable<T> directly implements ISmartSerializable<T>
            var type = typeof(SmartSerializable<>);

            // Act
            var result = type.IsSmartSerializable();

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void IsSmartSerializable_ScoDictionary_ReturnsFalse()
        {
            // Arrange — ScoDictionary does not implement ISmartSerializable<>
            var type = typeof(ScoDictionary<string, int>);

            // Act
            var result = type.IsSmartSerializable();

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void IsSmartSerializable_ConcurrentObservableCollection_ReturnsFalse()
        {
            // Arrange — the clean ConcurrentObservableCollection does not implement ISmartSerializable<>
            var type = typeof(ConcurrentObservableCollection<int>);

            // Act
            var result = type.IsSmartSerializable();

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void IsSmartSerializable_PlainString_ReturnsFalse()
        {
            // Arrange
            var type = typeof(string);

            // Act
            var result = type.IsSmartSerializable();

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void IsSmartSerializable_Int_ReturnsFalse()
        {
            // Arrange
            var type = typeof(int);

            // Act
            var result = type.IsSmartSerializable();

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void IsSmartSerializable_ScoBag_ReturnsFalse()
        {
            // Arrange — ScBag does not implement ISmartSerializable<>
            var type = typeof(ScBag<int>);

            // Act
            var result = type.IsSmartSerializable();

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void IsSmartSerializable_ObjectType_ReturnsFalse()
        {
            // Arrange
            var type = typeof(object);

            // Act
            var result = type.IsSmartSerializable();

            // Assert
            result.Should().BeFalse();
        }
    }
}
