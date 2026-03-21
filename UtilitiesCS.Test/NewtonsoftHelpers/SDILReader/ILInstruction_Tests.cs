using System;
using System.Reflection;
using System.Reflection.Emit;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDILReader;

namespace UtilitiesCS.Test.NewtonsoftHelpers.SDILReader
{
    [TestClass]
    public class ILInstruction_Tests
    {
        private sealed class TestOperandContainer
        {
            static TestOperandContainer() { }

            public int ValueField = 1;

            public TestOperandContainer() { }

            public int InstanceMethod()
            {
                return ValueField;
            }

            public static int StaticMethod()
            {
                return 42;
            }
        }

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

        [TestMethod]
        public void GetCode_WithFieldOperand_ShowsFieldSignature()
        {
            // Arrange
            var field = typeof(TestOperandContainer).GetField(
                nameof(TestOperandContainer.ValueField)
            );
            var instruction = new ILInstruction
            {
                Code = OpCodes.Ldfld,
                Offset = 2,
                Operand = field,
            };

            // Act
            var result = instruction.GetCode();

            // Assert
            result.Should().Contain("int");
            result
                .Should()
                .Contain(
                    "UtilitiesCS.Test.NewtonsoftHelpers.SDILReader.ILInstruction_Tests+TestOperandContainer::ValueField"
                );
        }

        [TestMethod]
        public void GetCode_WithInstanceMethodOperand_IncludesInstanceKeyword()
        {
            // Arrange
            var method = typeof(TestOperandContainer).GetMethod(
                nameof(TestOperandContainer.InstanceMethod)
            );
            var instruction = new ILInstruction
            {
                Code = OpCodes.Call,
                Offset = 3,
                Operand = method,
            };

            // Act
            var result = instruction.GetCode();

            // Assert
            result.Should().Contain("instance int");
            result.Should().Contain("TestOperandContainer::InstanceMethod()");
        }

        [TestMethod]
        public void GetCode_WithStaticMethodOperand_OmitsInstanceKeyword()
        {
            // Arrange
            var method = typeof(TestOperandContainer).GetMethod(
                nameof(TestOperandContainer.StaticMethod)
            );
            var instruction = new ILInstruction
            {
                Code = OpCodes.Call,
                Offset = 4,
                Operand = method,
            };

            // Act
            var result = instruction.GetCode();

            // Assert
            result.Should().Contain("int");
            result.Should().Contain("TestOperandContainer::StaticMethod()");
            result.Should().NotContain("instance");
        }

        [TestMethod]
        public void GetCode_WithConstructorOperand_ShowsInstanceConstructorSignature()
        {
            // Arrange
            var constructor = typeof(TestOperandContainer).GetConstructor(Type.EmptyTypes);
            var instruction = new ILInstruction
            {
                Code = OpCodes.Newobj,
                Offset = 5,
                Operand = constructor,
            };

            // Act
            var result = instruction.GetCode();

            // Assert
            result.Should().Contain("instance void");
            result.Should().Contain("TestOperandContainer::.ctor()");
        }

        [TestMethod]
        public void GetCode_WithStaticConstructorOperand_OmitsInstanceKeyword()
        {
            // Arrange
            var staticConstructor = typeof(TestOperandContainer).TypeInitializer;
            var instruction = new ILInstruction
            {
                Code = OpCodes.Newobj,
                Offset = 6,
                Operand = staticConstructor,
            };

            // Act
            var result = instruction.GetCode();

            // Assert
            result.Should().Contain("void");
            result.Should().Contain("TestOperandContainer::.cctor()");
            result.Should().NotContain("instance");
        }

        [TestMethod]
        public void GetCode_WithInvalidInlineMethodOperand_ReturnsOpcodeOnly()
        {
            // Arrange
            var instruction = new ILInstruction
            {
                Code = OpCodes.Call,
                Offset = 0,
                Operand = "not a method",
            };

            // Act
            var result = instruction.GetCode();

            // Assert
            result.Should().Be("0000 : call");
        }

        [TestMethod]
        public void GetCode_WithShortInlineVarOperand_AppendsOperandValue()
        {
            // Arrange
            var instruction = new ILInstruction
            {
                Code = OpCodes.Ldarg_S,
                Offset = 7,
                Operand = (byte)3,
            };

            // Act
            var result = instruction.GetCode();

            // Assert
            result.Should().Contain("ldarg.s");
            result.Should().EndWith("3");
        }

        [TestMethod]
        public void GetCode_WithNonTypeTokenOperand_ShowsNotSupported()
        {
            // Arrange
            var instruction = new ILInstruction
            {
                Code = OpCodes.Ldtoken,
                Offset = 8,
                Operand = "token",
            };

            // Act
            var result = instruction.GetCode();

            // Assert
            result.Should().Contain("not supported");
        }

        [TestMethod]
        public void GetCode_WithUnsupportedOperandType_ShowsNotSupported()
        {
            // Arrange
            var instruction = new ILInstruction
            {
                Code = OpCodes.Switch,
                Offset = 9,
                Operand = new[] { 1, 2, 3 },
            };

            // Act
            var result = instruction.GetCode();

            // Assert
            result.Should().Contain("switch");
            result.Should().Contain("not supported");
        }
    }
}
