using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.HelperClasses;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class ReflectionHelper_Tests
    {
        [TestMethod]
        public void GetAllClassesInSolution_ReturnsKnownProjectTypesAndFiltersNestedPrivateTypes()
        {
            // Act
            var result = ReflectionHelper.GetAllClassesInSolution();

            // Assert
            result.Should().Contain(typeof(ReflectionHelper));
            result.Should().Contain(typeof(ReflectionHelper_Tests));
            result.Should().OnlyContain(type => !type.IsNestedPrivate);
            result
                .Should()
                .OnlyContain(type =>
                    !Attribute.IsDefined(
                        type,
                        typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute),
                        false
                    )
                );
        }

        [TestMethod]
        public void GetAllContainedTypes_ReturnsEmptyListForNullInput()
        {
            // Act
            var result = ReflectionHelper.GetAllContainedTypes(null);

            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void GetAllContainedTypes_CollectsRootNestedGenericAndInterfaceBackedTypesWithoutRepeatingCycles()
        {
            // Arrange
            var shared = new DerivedContract();
            var root = new RootContainer
            {
                Contract = shared,
                GenericWrapper = new GenericWrapper<IContract> { Value = shared },
                Child = new ChildContainer { Parent = null, Contract = shared },
            };
            root.Child.Parent = root;

            // Act
            var result = ReflectionHelper.GetAllContainedTypes(root);

            // Assert
            result.Should().Contain(typeof(RootContainer));
            result.Should().Contain(typeof(ChildContainer));
            result.Should().Contain(typeof(DerivedContract));
            result.Should().Contain(typeof(GenericWrapper<IContract>));
            result.Should().OnlyHaveUniqueItems();
        }

        [TestMethod]
        public void GetAllFields_ReturnsDeclaredInstanceFieldsFromDerivedAndBaseTypes()
        {
            // Act
            var fields = typeof(DerivedFieldContainer).GetAllFields();

            // Assert
            fields
                .Select(field => field.Name)
                .Should()
                .Contain(
                    new[]
                    {
                        "DerivedPublicField",
                        "derivedPrivateField",
                        "BasePublicField",
                        "basePrivateField",
                    }
                );
            fields.Select(field => field.Name).Should().NotContain("StaticField");
        }

        [TestMethod]
        public void GetAllDerivedFields_ReturnsOnlyFieldsBelowSpecifiedBaseType()
        {
            // Act
            var fields = typeof(MostDerivedFieldContainer).GetAllDerivedFields(
                typeof(BaseFieldContainer)
            );

            // Assert
            fields
                .Select(field => field.Name)
                .Should()
                .Contain(new[] { "MostDerivedField", "DerivedPublicField", "derivedPrivateField" });
            fields
                .Select(field => field.Name)
                .Should()
                .NotContain(new[] { "BasePublicField", "basePrivateField" });
        }

        private interface IContract { }

        private sealed class DerivedContract : IContract { }

        private sealed class RootContainer
        {
            public IContract Contract { get; set; }

            public GenericWrapper<IContract> GenericWrapper { get; set; }

            public ChildContainer Child;
        }

        private sealed class GenericWrapper<T>
        {
            public T Value { get; set; }
        }

        private sealed class ChildContainer
        {
            public RootContainer Parent { get; set; }

            public IContract Contract { get; set; }
        }

        // These backing fields are intentionally present only so reflection-based tests can
        // verify field discovery across inheritance boundaries.
#pragma warning disable CS0169, CS0649
        private class BaseFieldContainer
        {
            public static readonly string StaticField = nameof(StaticField);

            public int BasePublicField;

            private readonly int basePrivateField;
        }

        private class DerivedFieldContainer : BaseFieldContainer
        {
            public int DerivedPublicField;

            private readonly int derivedPrivateField;
        }

        private sealed class MostDerivedFieldContainer : DerivedFieldContainer
        {
            public int MostDerivedField;
        }
#pragma warning restore CS0169, CS0649
    }
}
