using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UtilitiesCS.NewtonsoftHelpers;
using FluentAssertions;

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
            public string AdditionalField3 { get => _additionalField3; set => _additionalField3 = value; }

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
            public string TestElement { get => _testElement; set => _testElement = value; }
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

        //[TestMethod]
        //public void MyTypeBuilderTest2()
        //{
        //    var expected = new SimpleProperty() { TestElement = "test1" };
        //    var actual = TypeBuilderNamespace.MyTypeBuilder.CreateReplica(expected);
        //    actual.Should().BeEquivalentTo(expected);
        //}
        
        //[TestMethod]
        //public void MyTypeBuilderTest3()
        //{
        //    var expected = new TestDerived();
        //    expected.TryAdd("key1", 1);
        //    expected.TryAdd("key2", 2);
        //    var actual = TypeBuilderNamespace.MyTypeBuilder.CreateReplica(expected);
        //    actual.Should().BeEquivalentTo(expected);

        //}


    }
}