using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using UtilitiesCS.NewtonsoftHelpers;
using FluentAssertions;

namespace UtilitiesCS.Test.NewtonsoftHelpers
{
    [TestClass]
    public class WrapperScDictionaryTest
    {
        private class TestDerived : ConcurrentDictionary<string, int>
        {
            public string AdditionalField1 { get; set; }
            private int AdditionalField2;
            private string _additionalField3;
            public string AdditionalField3 { get => _additionalField3; set => _additionalField3 = value; }

            public TestDerived()
            {
                AdditionalField1 = "Test";
                AdditionalField2 = 42;
                AdditionalField3 = "Test3";
            }

            public int GetAdditionalField2() => AdditionalField2;
        }
                
        private class RemainingObjectClass
        {
            public string AdditionalField1 { get; set; }
            private int AdditionalField2;
            private string _additionalField3;
            public string AdditionalField3 { get => _additionalField3; set => _additionalField3 = value; }

            public RemainingObjectClass()
            {
                AdditionalField1 = "Test";
                AdditionalField2 = 42;
                AdditionalField3 = "Test3";
            }
        }

        [TestMethod]
        public void ToComposition_ShouldExtractDictionaryAndAdditionalFields()
        {
            // Arrange
            var derivedInstance = new TestDerived();
            derivedInstance.TryAdd("key1", 1);
            derivedInstance.TryAdd("key2", 2);

            var wrapper = new WrapperScDictionary<TestDerived, string, int>();

            // Act
            wrapper.ToComposition(derivedInstance);

            // Assert
            Assert.AreEqual(2, wrapper.ConcurrentDictionary.Count);
            Assert.AreEqual(1, wrapper.ConcurrentDictionary["key1"]);
            Assert.AreEqual(2, wrapper.ConcurrentDictionary["key2"]);

            var remainingObjectType = wrapper.RemainingObject.GetType();
            var additionalProperty1 = remainingObjectType.GetProperty("AdditionalField1", BindingFlags.Instance | BindingFlags.Public);
            var additionalField2 = remainingObjectType.GetField("AdditionalField2", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(additionalProperty1);
            Assert.IsNotNull(additionalField2);
            Assert.AreEqual("Test", additionalProperty1.GetValue(wrapper.RemainingObject));
            Assert.AreEqual(42, additionalField2.GetValue(wrapper.RemainingObject));
        }

        [TestMethod]
        public void ToDerived_ShouldRecreateDerivedInstance()
        {
            // Arrange
            var composedInstance = new WrapperScDictionary<TestDerived, string, int>();
            composedInstance.ConcurrentDictionary.TryAdd("key1", 1);
            composedInstance.ConcurrentDictionary.TryAdd("key2", 2);
            composedInstance.RemainingObject = new RemainingObjectClass();
            var expected = new TestDerived();
            expected.TryAdd("key1", 1);
            expected.TryAdd("key2", 2);

            // Act            
            var recreatedInstance = composedInstance.ToDerived(composedInstance);

            // Assert
            expected.Should().BeEquivalentTo(recreatedInstance);
        }

        [TestMethod]
        public void EndToEnd_ShouldRecreateDerivedInstance()
        {
            // Arrange
            var derivedInstance = new TestDerived();
            derivedInstance.TryAdd("key1", 1);
            derivedInstance.TryAdd("key2", 2);


            // Act
            var wrapper = new WrapperScDictionary<TestDerived, string, int>().ToComposition(derivedInstance);
            var recreatedInstance = wrapper.ToDerived();

            // Assert
            derivedInstance.Should().BeEquivalentTo(recreatedInstance);
        }

        [TestMethod]
        public void CompileType_ShouldCreateTypeWithAdditionalFields()
        {
            // Arrange
            var wrapper = new WrapperScDictionary<TestDerived, string, int>();

            // Act
            var newType = wrapper.CompileType();

            // Assert
            Assert.IsNotNull(newType);
            var additionalProperty1 = newType.GetProperty("AdditionalField1", BindingFlags.Instance | BindingFlags.Public);
            var additionalField2 = newType.GetField("AdditionalField2", BindingFlags.Instance | BindingFlags.NonPublic);
            var additionalProperty3 = newType.GetProperty("AdditionalField3", BindingFlags.Instance | BindingFlags.Public);
            var additionalField3 = newType.GetField("_additionalField3", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(additionalProperty1);
            Assert.IsNotNull(additionalField2);
            Assert.IsNotNull(additionalProperty3);
            Assert.IsNotNull(additionalField3);
        }

        [TestMethod]
        public void ConvertToNewClassInstance_ShouldCreateInstanceWithFields()
        {
            // Arrange
            var derivedInstance = new TestDerived();
            derivedInstance.TryAdd("key1", 1);
            derivedInstance.TryAdd("key2", 2);
            var expected = new WrapperScDictionary<TestDerived, string, int>();
            expected.ConcurrentDictionary.TryAdd("key1", 1);
            expected.ConcurrentDictionary.TryAdd("key2", 2);
            expected.RemainingObject = new RemainingObjectClass();

            // Act
            var wrapper = new WrapperScDictionary<TestDerived, string, int>().ToComposition(derivedInstance);

            var newClassInstance = wrapper.RemainingObject;
            var newClassType = newClassInstance.GetType();

            // Assert
            Assert.IsNotNull(newClassInstance);
            var property = newClassType.GetProperty("AdditionalField1", BindingFlags.Instance | BindingFlags.Public);
            var value = property.GetValue(newClassInstance);
            Assert.AreEqual("Test", newClassType.GetProperty("AdditionalField1", BindingFlags.Instance | BindingFlags.Public).GetValue(newClassInstance));
            Assert.AreEqual(42, newClassType.GetField("AdditionalField2", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(newClassInstance));
            wrapper.Should().BeEquivalentTo(expected);
        }
    }
}
