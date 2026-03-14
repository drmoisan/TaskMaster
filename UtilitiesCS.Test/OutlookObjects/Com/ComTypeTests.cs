using System;
using System.Runtime.InteropServices;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.OutlookObjects.Com
{
    [TestClass]
    public class ComTypeTests
    {
        [TestMethod]
        public void GetTypeName_WithNonDispatchObject_ReturnsNull()
        {
            // Arrange
            var plainObject = new object();

            // Act
            var result = UtilitiesCS.ComType.TypeInformation.GetTypeName(plainObject);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void GetTypeName_WithDispatchObject_ReturnsComTypeName()
        {
            // Arrange
            var scriptingDictionaryType = Type.GetTypeFromProgID("Scripting.Dictionary", throwOnError: false);
            scriptingDictionaryType.Should().NotBeNull("the Windows scripting runtime should be available in this Windows test environment");

            var dispatchObject = Activator.CreateInstance(scriptingDictionaryType!);

            try
            {
                // Act
                var result = UtilitiesCS.ComType.TypeInformation.GetTypeName(dispatchObject);

                // Assert
                result.Should().BeOneOf("IDictionary", "Dictionary");
            }
            finally
            {
                if (dispatchObject is not null && Marshal.IsComObject(dispatchObject))
                {
                    Marshal.FinalReleaseComObject(dispatchObject);
                }
            }
        }
    }
}