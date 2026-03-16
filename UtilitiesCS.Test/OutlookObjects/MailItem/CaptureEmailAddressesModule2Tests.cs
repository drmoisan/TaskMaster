using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.OutlookObjects.MailItemCoverage
{
    [TestClass]
    public class CaptureEmailAddressesModule2Tests
    {
        [TestMethod]
        public void Module_ShouldRemainStaticPlaceholderWithoutPublicMethods()
        {
            // Arrange
            Type moduleType = typeof(CaptureEmailAddressesModule2);

            // Act
            MethodInfo[] declaredPublicMethods = moduleType
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

            // Assert
            moduleType.IsAbstract.Should().BeTrue();
            moduleType.IsSealed.Should().BeTrue();
            declaredPublicMethods.Should().BeEmpty();
        }

        [TestMethod]
        public void Module_ShouldNotExposeLegacyGetEmailAddressesMethod()
        {
            // Arrange
            Type moduleType = typeof(CaptureEmailAddressesModule2);

            // Act
            bool hasLegacyMethod = moduleType
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Any(method => string.Equals(method.Name, "GetEmailAddresses", StringComparison.Ordinal));

            // Assert
            hasLegacyMethod.Should().BeFalse();
        }
    }
}