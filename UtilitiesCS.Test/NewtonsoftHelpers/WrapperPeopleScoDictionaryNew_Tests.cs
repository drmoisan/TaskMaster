using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
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

        [TestMethod]
        public void ToComposition_WithNullDerivedInstance_ThrowsArgumentNullException()
        {
            // Arrange
            var wrapper = new WrapperPeopleScoDictionaryNew();

            // Act
            Action act = () => wrapper.ToComposition(null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void ToDerived_WhenCoDictionaryIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var wrapper = new WrapperPeopleScoDictionaryNew
            {
                CoDictionary = null,
                RemainingObject = new PeopleScoRemainingObject(),
            };

            // Act
            Action act = () => wrapper.ToDerived();

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void ToDerived_WhenRemainingObjectIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var wrapper = new WrapperPeopleScoDictionaryNew { RemainingObject = null };

            // Act
            Action act = () => wrapper.ToDerived();

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void ToDerived_WhenConfigFieldIsMissing_StillRestoresDictionaryEntries()
        {
            // Arrange
            var wrapper = new WrapperPeopleScoDictionaryNew();
            wrapper.CoDictionary.TryAdd("key", "value");
            wrapper.RemainingObject = new PeopleScoRemainingObject();

            // Act
            var restored = wrapper.ToDerived();

            // Assert
            restored.Should().ContainKey("key");
            restored["key"].Should().Be("value");
        }

        [TestMethod]
        public void CompileType_CreatesTypeWithConfigProperty()
        {
            // Arrange
            var wrapper = new WrapperPeopleScoDictionaryNew();

            // Act
            var createdType = wrapper.CompileType();

            // Assert
            createdType.Should().NotBeNull();
            var configProperty = createdType.GetProperty(
                "Config",
                BindingFlags.Instance | BindingFlags.Public
            );
            configProperty.Should().NotBeNull();
            configProperty.CanRead.Should().BeTrue();
            configProperty.CanWrite.Should().BeFalse();
        }

        [TestMethod]
        public void CopyTo_WithRemainingObjectType_CopiesConfig()
        {
            // Arrange
            var wrapper = new WrapperPeopleScoDictionaryNew();
            var derived = new PeopleScoDictionaryNew { Config = new NewSmartSerializableConfig() };

            // Act
            var copied = wrapper.CopyTo(derived, typeof(PeopleScoRemainingObject));

            // Assert
            copied.Should().BeOfType<PeopleScoRemainingObject>();
            ((PeopleScoRemainingObject)copied).Config.Should().NotBeNull();
        }

        [TestMethod]
        public void CreateConfigProperty_CreatesReadableConfigPropertyAndBackingField()
        {
            // Arrange
            var wrapper = new WrapperPeopleScoDictionaryNew();
            using var typeBuilderScope = new TypeBuilderScope("WrapperPeopleScoCreateConfig");
            var expectedConfig = new NewSmartSerializableConfig();

            // Act
            wrapper.CreateConfigProperty(typeBuilderScope.TypeBuilder);
            var generatedType = typeBuilderScope.TypeBuilder.CreateType();
            var generatedInstance = Activator.CreateInstance(generatedType);
            generatedType
                .GetField("_Config", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(generatedInstance, expectedConfig);

            // Assert
            generatedType
                .GetProperty("Config")
                .GetValue(generatedInstance)
                .Should()
                .BeSameAs(expectedConfig);
        }

        [TestMethod]
        public void ReplicateProperty_WithExplicitBackingField_CreatesRoundTrippableProperty()
        {
            // Arrange
            var wrapper = new WrapperPeopleScoDictionaryNew();
            var property = typeof(PeopleScoRemainingObject).GetProperty(
                nameof(PeopleScoRemainingObject.Name)
            );
            var backingField = typeof(PeopleScoRemainingObject).GetField(
                "<Name>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            using var typeBuilderScope = new TypeBuilderScope("WrapperPeopleScoExplicitField");

            // Act
            wrapper.ReplicateProperty(typeBuilderScope.TypeBuilder, property, backingField);
            var replicatedType = typeBuilderScope.TypeBuilder.CreateType();
            var replicatedInstance = Activator.CreateInstance(replicatedType);
            replicatedType.GetProperty(property.Name).SetValue(replicatedInstance, "replicated");

            // Assert
            replicatedType
                .GetProperty(property.Name)
                .GetValue(replicatedInstance)
                .Should()
                .Be("replicated");
            replicatedType
                .GetField("<Name>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(replicatedInstance)
                .Should()
                .Be("replicated");
        }

        [TestMethod]
        public void ReplicateProperty_WhenSetterIsMissing_SkipsSetterAndCreatesReadableProperty()
        {
            // Arrange
            var wrapper = new WrapperPeopleScoDictionaryNew();
            var property = typeof(GetterOnlyHolder).GetProperty(nameof(GetterOnlyHolder.Value));
            var capturedFields = new Dictionary<string, FieldBuilder>();
            using var typeBuilderScope = new TypeBuilderScope("WrapperPeopleScoGetterOnly");

            // Act
            wrapper.ReplicateProperty(typeBuilderScope.TypeBuilder, property, ref capturedFields);
            var replicatedType = typeBuilderScope.TypeBuilder.CreateType();

            // Assert
            replicatedType.GetProperty(property.Name).CanRead.Should().BeTrue();
            replicatedType.GetProperty(property.Name).CanWrite.Should().BeFalse();
        }

        [TestMethod]
        public void ReplicateProperty_WhenGetterIsMissing_ThrowsInvalidOperationException()
        {
            // Arrange
            var wrapper = new WrapperPeopleScoDictionaryNew();
            var property = typeof(SetterOnlyHolder).GetProperty(nameof(SetterOnlyHolder.Value));
            var capturedFields = new Dictionary<string, FieldBuilder>();
            using var typeBuilderScope = new TypeBuilderScope("WrapperPeopleScoSetterOnly");

            // Act
            Action act = () =>
                wrapper.ReplicateProperty(
                    typeBuilderScope.TypeBuilder,
                    property,
                    ref capturedFields
                );

            // Assert
            act.Should().Throw<InvalidOperationException>().WithMessage("*getter*");
        }

        [TestMethod]
        public void GetBackingField_WhenPropertyHasBackingField_ReturnsUnderlyingField()
        {
            // Arrange
            var wrapper = new WrapperPeopleScoDictionaryNew();
            var property = typeof(PeopleScoRemainingObject).GetProperty(
                nameof(PeopleScoRemainingObject.Name)
            );

            // Act
            var backingField = wrapper.GetBackingField(property);

            // Assert
            backingField.Name.Should().Be("<Name>k__BackingField");
        }

        [TestMethod]
        public void GetBackingField_WhenGetterIsMissing_ThrowsInvalidOperationException()
        {
            // Arrange
            var wrapper = new WrapperPeopleScoDictionaryNew();
            var property = typeof(SetterOnlyHolder).GetProperty(nameof(SetterOnlyHolder.Value));

            // Act
            Action act = () => wrapper.GetBackingField(property);

            // Assert
            act.Should().Throw<InvalidOperationException>().WithMessage("*getter*");
        }

        private sealed class GetterOnlyHolder
        {
            public string Value => "getter";
        }

        private sealed class SetterOnlyHolder
        {
            public string Value
            {
                set => Stored = value;
            }

            public string Stored { get; private set; }
        }

        private sealed class TypeBuilderScope : IDisposable
        {
            public TypeBuilderScope(string typeName)
            {
                var assemblyName = new AssemblyName($"{typeName}_{Guid.NewGuid():N}");
                var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                    assemblyName,
                    AssemblyBuilderAccess.Run
                );
                var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
                TypeBuilder = moduleBuilder.DefineType(
                    typeName,
                    TypeAttributes.Public | TypeAttributes.Class
                );
            }

            public TypeBuilder TypeBuilder { get; }

            public void Dispose() { }
        }
    }
}
