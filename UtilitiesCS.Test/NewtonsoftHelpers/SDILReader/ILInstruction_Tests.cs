using System.Reflection.Emit;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDILReader;

namespace UtilitiesCS.Test.NewtonsoftHelpers.SDILReader
{
    [TestClass]
    public class ILInstruction_Tests
    {
        [TestMethod]
        public void DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var instruction = new ILInstruction();

            // Assert
            instruction.Should().NotBeNull();
        }

        [TestMethod]
        public void Code_SetAndGet_ReturnsExpected()
        {
            // Arrange
            var instruction = new ILInstruction();

            // Act
            instruction.Code = OpCodes.Nop;

            // Assert
            instruction.Code.Should().Be(OpCodes.Nop);
        }

        [TestMethod]
        public void Operand_SetAndGet_ReturnsExpected()
        {
            // Arrange
            var instruction = new ILInstruction();

            // Act
            instruction.Operand = "test operand";

            // Assert
            instruction.Operand.Should().Be("test operand");
        }

        [TestMethod]
        public void OperandData_SetAndGet_ReturnsExpected()
        {
            // Arrange
            var instruction = new ILInstruction();
            var data = new byte[] { 0x01, 0x02, 0x03 };

            // Act
            instruction.OperandData = data;

            // Assert
            instruction.OperandData.Should().BeEquivalentTo(data);
        }

        [TestMethod]
        public void Offset_SetAndGet_ReturnsExpected()
        {
            // Arrange
            var instruction = new ILInstruction();

            // Act
            instruction.Offset = 42;

            // Assert
            instruction.Offset.Should().Be(42);
        }

        [TestMethod]
        public void GetCode_NopNoOperand_ReturnsFormattedString()
        {
            // Arrange
            var instruction = new ILInstruction { Code = OpCodes.Nop, Offset = 0 };

            // Act
            var result = instruction.GetCode();

            // Assert
            result.Should().Contain("0000");
            result.Should().Contain("nop");
        }

        [TestMethod]
        public void GetCode_WithStringOperand_ContainsString()
        {
            // Arrange
            var instruction = new ILInstruction
            {
                Code = OpCodes.Ldstr,
                Offset = 5,
                Operand = "hello",
            };

            // Act
            var result = instruction.GetCode();

            // Assert
            result.Should().Contain("hello");
        }

        [TestMethod]
        public void GetCode_WithNewlineStringOperand_EscapesNewline()
        {
            // Arrange
            var instruction = new ILInstruction
            {
                Code = OpCodes.Ldstr,
                Offset = 0,
                Operand = "\r\n",
            };

            // Act
            var result = instruction.GetCode();

            // Assert
            result.Should().Contain("\\r\\n");
        }

        [TestMethod]
        public void GetCode_WithTypeOperand_ShowsTypeName()
        {
            // Arrange
            var instruction = new ILInstruction
            {
                Code = OpCodes.Box,
                Offset = 10,
                Operand = typeof(int),
            };

            // Act
            var result = instruction.GetCode();

            // Assert
            result.Should().Contain("int");
        }

        [TestMethod]
        public void GetCode_WithIntOperand_ShowsValue()
        {
            // Arrange
            var instruction = new ILInstruction
            {
                Code = OpCodes.Ldc_I4,
                Offset = 0,
                Operand = 42,
            };

            // Act
            var result = instruction.GetCode();

            // Assert
            result.Should().Contain("42");
        }

        [TestMethod]
        public void GetCode_WithBrTargetOperand_ShowsExpandedOffset()
        {
            // Arrange
            var instruction = new ILInstruction
            {
                Code = OpCodes.Br_S,
                Offset = 0,
                Operand = 15,
            };

            // Act
            var result = instruction.GetCode();

            // Assert
            result.Should().Contain("0015");
        }

        [TestMethod]
        public void GetCode_WithTokenTypeOperand_ShowsFullName()
        {
            // Arrange
            var instruction = new ILInstruction
            {
                Code = OpCodes.Ldtoken,
                Offset = 0,
                Operand = typeof(string),
            };

            // Act
            var result = instruction.GetCode();

            // Assert
            result.Should().Contain("System.String");
        }

        [TestMethod]
        public void GetCode_WithNullOperand_ReturnsCodeOnly()
        {
            // Arrange
            var instruction = new ILInstruction
            {
                Code = OpCodes.Nop,
                Offset = 0,
                Operand = null,
            };

            // Act
            var result = instruction.GetCode();

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().Contain("nop");
        }

        [TestMethod]
        public void GetCode_LargeOffset_FormatsCorrectly()
        {
            // Arrange
            var instruction = new ILInstruction
            {
                Code = OpCodes.Nop,
                Offset = 9999,
                Operand = null,
            };

            // Act
            var result = instruction.GetCode();

            // Assert
            result.Should().Contain("9999");
        }

        [TestMethod]
        public void GetCode_SmallOffset_PadsWithZeros()
        {
            // Arrange
            var instruction = new ILInstruction
            {
                Code = OpCodes.Ret,
                Offset = 1,
                Operand = null,
            };

            // Act
            var result = instruction.GetCode();

            // Assert
            result.Should().Contain("0001");
        }
    }
}
