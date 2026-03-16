using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace UtilitiesCS.Test.Extensions
{
    [TestClass]
    public class CompilerServicesExtensions_Tests
    {
        [TestMethod]
        public void CallerArgumentExpressionAttribute_StoresProvidedParameterName()
        {
            // Arrange
            var attribute = new CallerArgumentExpressionAttribute("argument");

            // Act / Assert
            attribute.ParameterName.Should().Be("argument");
        }

        [TestMethod]
        public void CallerArgumentExpressionAttribute_HasExpectedUsageMetadata()
        {
            // Arrange
            var usage = typeof(CallerArgumentExpressionAttribute)
                .GetCustomAttribute<AttributeUsageAttribute>();

            // Act / Assert
            usage.Should().NotBeNull();
            usage.ValidOn.Should().Be(AttributeTargets.Parameter);
            usage.AllowMultiple.Should().BeFalse();
            usage.Inherited.Should().BeFalse();
        }
    }
}
