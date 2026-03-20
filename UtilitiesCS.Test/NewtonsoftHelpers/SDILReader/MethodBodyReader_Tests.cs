using System;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDILReader;

namespace UtilitiesCS.Test.NewtonsoftHelpers.SDILReader
{
    [TestClass]
    public class MethodBodyReader_Tests
    {
        #region Constructor and Parsing

        [TestMethod]
        public void Constructor_WithSimpleMethod_ParsesInstructions()
        {
            // Use a simple known method for IL parsing
            var methodInfo = typeof(MethodBodyReader_Tests)
                .GetMethod(nameof(SimpleTestMethod), BindingFlags.NonPublic | BindingFlags.Static);

            var reader = new MethodBodyReader(methodInfo);

            reader.instructions.Should().NotBeNull();
            reader.instructions.Should().NotBeEmpty();
        }

        [TestMethod]
        public void Constructor_WithVoidMethod_ParsesSuccessfully()
        {
            var methodInfo = typeof(MethodBodyReader_Tests)
                .GetMethod(nameof(VoidTestMethod), BindingFlags.NonPublic | BindingFlags.Static);

            var reader = new MethodBodyReader(methodInfo);
            reader.instructions.Should().NotBeNull();
        }

        [TestMethod]
        public void GetBodyCode_ReturnsConcatenatedInstructions()
        {
            var methodInfo = typeof(MethodBodyReader_Tests)
                .GetMethod(nameof(SimpleTestMethod), BindingFlags.NonPublic | BindingFlags.Static);

            var reader = new MethodBodyReader(methodInfo);
            string bodyCode = reader.GetBodyCode();

            bodyCode.Should().NotBeNullOrEmpty();
        }

        #endregion

        #region Helper Methods

        private static int SimpleTestMethod()
        {
            int x = 1;
            int y = 2;
            return x + y;
        }

        private static void VoidTestMethod()
        {
            var s = "hello";
            _ = s.Length;
        }

        #endregion
    }
}
