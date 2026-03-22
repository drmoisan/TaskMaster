using System.Reflection.Emit;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDILReader;

namespace UtilitiesCS.Test.NewtonsoftHelpers.SDILReader
{
    [TestClass]
    public class ILGlobals_Tests
    {
        [TestMethod]
        public void LoadOpCodes_Initializes_SingleByteOpCodes()
        {
            // Arrange & Act
            ILGlobals.LoadOpCodes();

            // Assert
            ILGlobals.singleByteOpCodes.Should().NotBeNull();
            ILGlobals.singleByteOpCodes.Length.Should().Be(0x100);
        }

        [TestMethod]
        public void LoadOpCodes_Initializes_MultiByteOpCodes()
        {
            // Arrange & Act
            ILGlobals.LoadOpCodes();

            // Assert
            ILGlobals.multiByteOpCodes.Should().NotBeNull();
            ILGlobals.multiByteOpCodes.Length.Should().Be(0x100);
        }

        [TestMethod]
        public void LoadOpCodes_PopulatesKnownSingleByteOpCodes()
        {
            // Arrange & Act
            ILGlobals.LoadOpCodes();

            // Assert - Nop is 0x00, a well-known single-byte opcode
            ILGlobals.singleByteOpCodes[0x00].Should().Be(OpCodes.Nop);
        }

        [TestMethod]
        public void LoadOpCodes_PopulatesKnownOpCode_Ret()
        {
            // Arrange & Act
            ILGlobals.LoadOpCodes();

            // Assert - Ret opcode is 0x2A
            ILGlobals.singleByteOpCodes[0x2A].Should().Be(OpCodes.Ret);
        }

        [TestMethod]
        public void ProcessSpecialTypes_SystemString_ReturnsString()
        {
            // Arrange & Act
            var result = ILGlobals.ProcessSpecialTypes("System.String");

            // Assert
            result.Should().Be("string");
        }

        [TestMethod]
        public void ProcessSpecialTypes_SystemDotstring_ReturnsString()
        {
            // Arrange & Act
            var result = ILGlobals.ProcessSpecialTypes("System.string");

            // Assert
            result.Should().Be("string");
        }

        [TestMethod]
        public void ProcessSpecialTypes_StringAlone_ReturnsString()
        {
            // Arrange & Act
            var result = ILGlobals.ProcessSpecialTypes("String");

            // Assert
            result.Should().Be("string");
        }

        [TestMethod]
        public void ProcessSpecialTypes_SystemInt32_ReturnsInt()
        {
            // Arrange & Act
            var result = ILGlobals.ProcessSpecialTypes("System.Int32");

            // Assert
            result.Should().Be("int");
        }

        [TestMethod]
        public void ProcessSpecialTypes_Int32_ReturnsInt()
        {
            // Arrange & Act
            var result = ILGlobals.ProcessSpecialTypes("Int32");

            // Assert
            result.Should().Be("int");
        }

        [TestMethod]
        public void ProcessSpecialTypes_Int_ReturnsInt()
        {
            // Arrange & Act
            var result = ILGlobals.ProcessSpecialTypes("Int");

            // Assert
            result.Should().Be("int");
        }

        [TestMethod]
        public void ProcessSpecialTypes_UnknownType_ReturnsSameString()
        {
            // Arrange
            var typeName = "System.Collections.Generic.List`1";

            // Act
            var result = ILGlobals.ProcessSpecialTypes(typeName);

            // Assert
            result.Should().Be(typeName);
        }

        [TestMethod]
        public void Cache_IsInitialized()
        {
            // Assert
            ILGlobals.Cache.Should().NotBeNull();
        }
    }
}
