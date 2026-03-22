using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using UtilitiesCS.NewtonsoftHelpers;

namespace UtilitiesCS.Test.NewtonsoftHelpers
{
    [TestClass]
    public class DerivedCompositionConverter_ConcurrentDictionaryTests
    {
        private class TestDerived : ConcurrentDictionary<string, int>
        {
            public string AdditionalField1 { get; set; }
            private int AdditionalField2;
            private string _additionalField3;

            [JsonProperty]
            public string AdditionalField3
            {
                get => _additionalField3;
                set => _additionalField3 = value;
            }

            public TestDerived()
            {
                AdditionalField1 = "Test";
                AdditionalField2 = 42;
                AdditionalField3 = "Test3";
            }

            public int GetAdditionalField2() => AdditionalField2;
        }

        private class SimpleProperty()
        {
            private string _testElement;
            public string TestElement
            {
                get => _testElement;
                set => _testElement = value;
            }
        }

        //[TestMethod]
        //public void ToComposition_ShouldExtractDictionaryAndAdditionalFields()
        //{
        //    // Arrange
        //    var derivedInstance = new TestDerived();
        //    derivedInstance.TryAdd("key1", 1);
        //    derivedInstance.TryAdd("key2", 2);

        //    var converter = new DerivedCompositionConverter_ConcurrentDictionary<TestDerived, string, int>();

        //    // Act
        //    converter.ToCompositionOld(derivedInstance);

        //    // Assert
        //    Assert.AreEqual(2, converter.ConcurrentDictionary.Count);
        //    Assert.AreEqual(1, converter.ConcurrentDictionary["key1"]);
        //    Assert.AreEqual(2, converter.ConcurrentDictionary["key2"]);
        //    Assert.AreEqual(3, converter.AdditionalFields.Count);
        //    Assert.AreEqual("Test", converter.AdditionalFields["<AdditionalField1>k__BackingField"]);
        //    Assert.AreEqual(42, converter.AdditionalFields["AdditionalField2"]);
        //    Assert.AreEqual("Test3", converter.AdditionalFields["AdditionalField3"]);
        //}

        //[TestMethod]
        //public void ToDerived_ShouldRecreateDerivedInstance()
        //{
        //    // Arrange
        //    var derivedInstance = new TestDerived();
        //    derivedInstance.TryAdd("key1", 1);
        //    derivedInstance.TryAdd("key2", 2);

        //    var converter = new DerivedCompositionConverter_ConcurrentDictionary<TestDerived, string, int>(derivedInstance);

        //    // Act
        //    var recreatedInstance = converter.ToDerivedOld();

        //    // Assert
        //    Assert.AreEqual(2, recreatedInstance.Count);
        //    Assert.AreEqual(1, recreatedInstance["key1"]);
        //    Assert.AreEqual(2, recreatedInstance["key2"]);
        //    Assert.AreEqual("Test", recreatedInstance.AdditionalField1);
        //    Assert.AreEqual(42, recreatedInstance.GetAdditionalField2());
        //    Assert.AreEqual("Test3", recreatedInstance.AdditionalField3);
        //}

        //[TestMethod]
        //public void EmitNewClass_ShouldCreateTypeWithoutBase()
        //{
        //    // Arrange
        //    var converter = new DerivedCompositionConverter_ConcurrentDictionary<TestDerived, string, int>();

        //    // Act
        //    var newClassType = converter.EmitNewClass();

        //    // Assert
        //    Assert.IsNotNull(newClassType);
        //    Assert.IsTrue(newClassType.GetField("AdditionalField1") != null);
        //    Assert.IsTrue(newClassType.GetField("AdditionalField2") != null);
        //}

        //[TestMethod]
        //public void ConvertToNewClassInstance_ShouldCreateInstanceWithFields()
        //{
        //    // Arrange
        //    var derivedInstance = new TestDerived();
        //    derivedInstance.TryAdd("key1", 1);
        //    derivedInstance.TryAdd("key2", 2);

        //    var converter = new DerivedCompositionConverter_ConcurrentDictionary<TestDerived, string, int>();

        //    // Act
        //    var newClassInstance = converter.ConvertToNewClassInstance(derivedInstance);
        //    var newClassType = newClassInstance.GetType();

        //    // Assert
        //    Assert.IsNotNull(newClassInstance);
        //    Assert.AreEqual("Test", newClassType.GetField("AdditionalField1").GetValue(newClassInstance));
        //    Assert.AreEqual(42, newClassType.GetField("AdditionalField2").GetValue(newClassInstance));
        //}

        [TestMethod]
        public void MyTypeBuilderTest()
        {
            TypeBuilderNamespace.MyTypeBuilder.CreateNewObject();
        }

        [TestMethod]
        public void DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var converter =
                new DerivedCompositionConverter_ConcurrentDictionary<TestDerived, string, int>();

            // Assert
            converter.Should().NotBeNull();
        }

        [TestMethod]
        public void Constructor_WithDerivedInstance_ExtractsComposition()
        {
            // Arrange
            var derived = new TestDerived();
            derived.TryAdd("key1", 1);
            derived.TryAdd("key2", 2);

            // Act
            var converter = new DerivedCompositionConverter_ConcurrentDictionary<
                TestDerived,
                string,
                int
            >(derived);

            // Assert
            converter.ConcurrentDictionary.Should().NotBeNull();
            converter.ConcurrentDictionary.Should().HaveCount(2);
            converter.AdditionalFields.Should().NotBeNull();
            converter.AdditionalProperties.Should().NotBeNull();
        }

        [TestMethod]
        public void ToCompositionOld_ExtractsDictionaryEntries()
        {
            // Arrange
            var derived = new TestDerived();
            derived.TryAdd("a", 10);
            derived.TryAdd("b", 20);
            var converter =
                new DerivedCompositionConverter_ConcurrentDictionary<TestDerived, string, int>();

            // Act
            converter.ToCompositionOld(derived);

            // Assert
            converter.ConcurrentDictionary.Should().ContainKey("a").WhoseValue.Should().Be(10);
            converter.ConcurrentDictionary.Should().ContainKey("b").WhoseValue.Should().Be(20);
        }

        [TestMethod]
        public void ToCompositionOld_ExtractsAdditionalFields()
        {
            // Arrange
            var derived = new TestDerived();
            var converter =
                new DerivedCompositionConverter_ConcurrentDictionary<TestDerived, string, int>();

            // Act
            converter.ToCompositionOld(derived);

            // Assert
            converter.AdditionalFields.Should().NotBeEmpty();
        }

        [TestMethod]
        public void ToDerivedOld_RecreatesDictionaryEntries()
        {
            // Arrange
            var original = new TestDerived();
            original.TryAdd("key1", 1);
            original.TryAdd("key2", 2);
            var converter = new DerivedCompositionConverter_ConcurrentDictionary<
                TestDerived,
                string,
                int
            >(original);

            // Act
            var recreated = converter.ToDerivedOld();

            // Assert
            recreated.Should().ContainKey("key1").WhoseValue.Should().Be(1);
            recreated.Should().ContainKey("key2").WhoseValue.Should().Be(2);
        }

        [TestMethod]
        public void ToDerivedOld_RestoresAdditionalFields()
        {
            // Arrange
            var original = new TestDerived();
            original.TryAdd("x", 99);
            var converter = new DerivedCompositionConverter_ConcurrentDictionary<
                TestDerived,
                string,
                int
            >(original);

            // Act
            var recreated = converter.ToDerivedOld();

            // Assert
            recreated.AdditionalField1.Should().Be("Test");
            recreated.GetAdditionalField2().Should().Be(42);
        }

        [TestMethod]
        public void EmitNewClass_CreatesType()
        {
            // Arrange
            var converter =
                new DerivedCompositionConverter_ConcurrentDictionary<TestDerived, string, int>();

            // Act
            var newType = converter.EmitNewClass();

            // Assert
            newType.Should().NotBeNull();
        }

        [TestMethod]
        public void ConvertToNewClassInstance_CopiesAdditionalStateToProjectedType()
        {
            // Arrange
            var derived = new TestDerived();
            derived.TryAdd("key", 7);
            var converter =
                new DerivedCompositionConverter_ConcurrentDictionary<TestDerived, string, int>();

            // Act
            var projectedInstance = converter.ConvertToNewClassInstance(derived);
            var projectedType = projectedInstance.GetType();
            var privateField = projectedType.GetField(
                "AdditionalField2",
                BindingFlags.Instance | BindingFlags.Public
            );
            var publicProperty = projectedType.GetProperty(
                nameof(TestDerived.AdditionalField3),
                BindingFlags.Instance | BindingFlags.Public
            );

            // Assert
            projectedInstance.Should().NotBeNull();
            privateField.Should().NotBeNull();
            privateField!.GetValue(projectedInstance).Should().Be(42);
            publicProperty.Should().NotBeNull();
            publicProperty!.GetValue(projectedInstance).Should().Be("Test3");
        }

        [TestMethod]
        public void ToComposition_CapturesRemainingObjectProjection()
        {
            // Arrange
            var derived = new TestDerived();
            derived.TryAdd("key", 11);
            var converter =
                new DerivedCompositionConverter_ConcurrentDictionary<TestDerived, string, int>();

            // Act
            converter.ToComposition(derived);

            // Assert
            converter.ConcurrentDictionary.Should().BeSameAs(derived);
            converter.RemainingObject.Should().NotBeNull();
            converter.RemainingObject!.GetType().Name.Should().Be("TestDerived_WithoutBase");
        }

        [TestMethod]
        public void RoundTrip_ToCompositionAndToDerived_PreservesEntries()
        {
            // Arrange
            var original = new TestDerived();
            original.TryAdd("r1", 100);
            original.TryAdd("r2", 200);

            var converter =
                new DerivedCompositionConverter_ConcurrentDictionary<TestDerived, string, int>();

            // Act
            converter.ToCompositionOld(original);
            var restored = converter.ToDerivedOld();

            // Assert
            restored.Should().HaveCount(2);
            restored["r1"].Should().Be(100);
            restored["r2"].Should().Be(200);
        }
    }
}
