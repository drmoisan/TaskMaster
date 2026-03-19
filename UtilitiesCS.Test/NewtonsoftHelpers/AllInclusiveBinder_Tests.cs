using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.NewtonsoftHelpers;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class AllInclusiveBinder_Tests
    {
        [TestMethod]
        public void GetAssemblies_ShouldReturnNull_WhenNoAssembliesAreConfigured()
        {
            // Arrange
            var binder = new AllInclusiveBinder();

            // Act
            Assembly[] assemblies = binder.GetAssemblies();

            // Assert
            assemblies.Should().BeNull();
        }

        [TestMethod]
        public void GetAssemblies_ShouldRemainStableAcrossCalls()
        {
            // Arrange
            var binder = new AllInclusiveBinder();

            // Act
            Assembly[] first = binder.GetAssemblies();
            Assembly[] second = binder.GetAssemblies();

            // Assert
            first.Should().BeNull();
            second.Should().BeNull();
        }
    }
}
