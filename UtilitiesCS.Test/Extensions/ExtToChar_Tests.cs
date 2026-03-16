using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Reflection;
using UtilitiesCS;

namespace UtilitiesCS.Test.Extensions
{
    [TestClass]
    public class ExtToChar_Tests
    {
        [TestMethod]
        public void ExtToChar_CurrentlyExposesNoPublicMethods()
        {
            // Arrange
            var publicStaticMethods = typeof(ExtToChar)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(method => !method.IsSpecialName)
                .ToArray();

            // Act / Assert
            publicStaticMethods.Should().BeEmpty();
        }
    }
}
