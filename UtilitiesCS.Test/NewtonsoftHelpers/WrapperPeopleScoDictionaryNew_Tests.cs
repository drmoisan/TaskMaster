using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using ToDoModel.Data_Model.People;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Dictionary;

namespace UtilitiesCS.Test.NewtonsoftHelpers
{
    [TestClass]
    public class WrapperPeopleScoDictionaryNew_Tests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            Console.SetOut(new DebugTextWriter());
        }

        [TestMethod]
        public void Constructor_Default_InitializesCoDictionary()
        {
            // Arrange & Act
            var wrapper = new WrapperPeopleScoDictionaryNew();

            // Assert
            wrapper.CoDictionary.Should().NotBeNull();
        }

        [TestMethod]
        public void ToComposition_FromPeopleScoDictionary_ExtractsEntries()
        {
            // Arrange
            var dict = new PeopleScoDictionaryNew();
            dict.TryAdd("person1", "info1");
            dict.TryAdd("person2", "info2");
            var wrapper = new WrapperPeopleScoDictionaryNew();

            // Act
            var result = wrapper.ToComposition(dict);

            // Assert
            result.CoDictionary.Should().NotBeNull();
            result.CoDictionary.Should().ContainKey("person1");
            result.CoDictionary.Should().ContainKey("person2");
        }

        [TestMethod]
        public void ToComposition_SetsRemainingObject()
        {
            // Arrange
            var dict = new PeopleScoDictionaryNew();
            dict.TryAdd("k", "v");
            var wrapper = new WrapperPeopleScoDictionaryNew();

            // Act
            var result = wrapper.ToComposition(dict);

            // Assert
            result.RemainingObject.Should().NotBeNull();
        }

        [TestMethod]
        public void ToDerived_FromWrapper_RecreatesDictionary()
        {
            // Arrange
            var original = new PeopleScoDictionaryNew();
            original.TryAdd("key1", "val1");
            original.TryAdd("key2", "val2");
            var wrapper = new WrapperPeopleScoDictionaryNew();
            wrapper.ToComposition(original);

            // Act
            var restored = wrapper.ToDerived();

            // Assert
            restored.Should().NotBeNull();
            restored.Should().ContainKey("key1");
            restored.Should().ContainKey("key2");
        }

        [TestMethod]
        public void ToDerived_WithWrapperParam_RecreatesDictionary()
        {
            // Arrange
            var original = new PeopleScoDictionaryNew();
            original.TryAdd("a", "b");
            var wrapper = new WrapperPeopleScoDictionaryNew();
            wrapper.ToComposition(original);
            var wrapper2 = new WrapperPeopleScoDictionaryNew();

            // Act
            var restored = wrapper2.ToDerived(wrapper);

            // Assert
            restored.Should().NotBeNull();
            restored.Should().ContainKey("a");
        }

        [TestMethod]
        public void RoundTrip_ToCompositionAndToDerived_PreservesEntries()
        {
            // Arrange
            var original = new PeopleScoDictionaryNew();
            original.TryAdd("alpha", "one");
            original.TryAdd("beta", "two");

            var wrapper = new WrapperPeopleScoDictionaryNew();

            // Act
            wrapper.ToComposition(original);
            var restored = wrapper.ToDerived();

            // Assert
            restored.Should().ContainKey("alpha").WhoseValue.Should().Be("one");
            restored.Should().ContainKey("beta").WhoseValue.Should().Be("two");
        }

        [TestMethod]
        public void ToComposition_EmptyDictionary_HandledCorrectly()
        {
            // Arrange
            var dict = new PeopleScoDictionaryNew();
            var wrapper = new WrapperPeopleScoDictionaryNew();

            // Act
            var result = wrapper.ToComposition(dict);

            // Assert
            result.CoDictionary.Should().NotBeNull();
            result.CoDictionary.Should().BeEmpty();
        }
    }
}
