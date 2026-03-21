using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDILReader;

namespace UtilitiesCS.Test.NewtonsoftHelpers.SDILReader
{
    [TestClass]
    public class MethodBodyReader_Tests
    {
        private static readonly int StaticFieldValue = 42;

        #region Constructor and Parsing

        [TestMethod]
        public void Constructor_WithSimpleMethod_ParsesInstructions()
        {
            // Use a simple known method for IL parsing
            var methodInfo = typeof(MethodBodyReader_Tests).GetMethod(
                nameof(SimpleTestMethod),
                BindingFlags.NonPublic | BindingFlags.Static
            );

            var reader = CreateReader(methodInfo);

            reader.instructions.Should().NotBeNull();
            reader.instructions.Should().NotBeEmpty();
        }

        [TestMethod]
        public void Constructor_WithMethodWithoutBody_LeavesInstructionsNull()
        {
            var methodInfo = GetRequiredMethod(
                typeof(AbstractMethodContainer),
                nameof(AbstractMethodContainer.MethodWithoutBody),
                BindingFlags.Public | BindingFlags.Instance
            );

            var reader = CreateReader(methodInfo);

            reader.instructions.Should().BeNull();
            reader.GetBodyCode().Should().BeEmpty();
        }

        [TestMethod]
        public void Constructor_WithVoidMethod_ParsesSuccessfully()
        {
            var methodInfo = typeof(MethodBodyReader_Tests).GetMethod(
                nameof(VoidTestMethod),
                BindingFlags.NonPublic | BindingFlags.Static
            );

            var reader = CreateReader(methodInfo);
            reader.instructions.Should().NotBeNull();
        }

        [TestMethod]
        public void GetBodyCode_ReturnsConcatenatedInstructions()
        {
            var methodInfo = GetRequiredMethod(
                typeof(MethodBodyReader_Tests),
                nameof(MethodThatLoadsString),
                BindingFlags.NonPublic | BindingFlags.Static
            );

            var reader = CreateReader(methodInfo);
            string bodyCode = reader.GetBodyCode();

            bodyCode.Should().NotBeNullOrEmpty();
            bodyCode.Should().Contain("ldstr");
            reader.instructions.Should().Contain(i => Equals(i.Operand, "hello"));
        }

        [TestMethod]
        public void ConstructInstructions_WithNumericAndVariableOperands_ParsesExpectedOperands()
        {
            var contextMethod = GetRequiredMethod(
                typeof(MethodBodyReader_Tests),
                nameof(SimpleTestMethod),
                BindingFlags.NonPublic | BindingFlags.Static
            );

            var instructions = ConstructCustomInstructions(
                contextMethod,
                CombineBytes(
                    BuildInstructionBytes(OpCodes.Ldc_I4, BitConverter.GetBytes(1)),
                    BuildInstructionBytes(OpCodes.Ldc_I8, new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }),
                    BuildInstructionBytes(OpCodes.Ldc_R8, new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }),
                    BuildInstructionBytes(OpCodes.Ldc_R4, new byte[] { 1, 0, 0, 0 }),
                    BuildInstructionBytes(OpCodes.Ldc_I4_S, new byte[] { 0x7F }),
                    BuildInstructionBytes(OpCodes.Ldloc_S, new byte[] { 0x12 }),
                    BuildInstructionBytes(OpCodes.Ldloc, BitConverter.GetBytes((short)0x1234))
                )
            );

            instructions.Should().HaveCount(7);
            instructions[0].Operand.Should().Be(1);
            instructions[1].Operand.Should().Be(1UL);
            instructions[2].Operand.Should().Be(1d);
            instructions[3].Operand.Should().Be(1f);
            instructions[4].Operand.Should().Be((sbyte)0x7F);
            instructions[5].Operand.Should().Be((byte)0x12);
            instructions[6].Operand.Should().Be((ushort)0x1234);
        }

        [TestMethod]
        public void ConstructInstructions_WithMetadataOperands_ResolvesFieldMethodAndType()
        {
            var contextMethod = GetRequiredMethod(
                typeof(MethodBodyReader_Tests),
                nameof(SimpleTestMethod),
                BindingFlags.NonPublic | BindingFlags.Static
            );
            var fieldToken = GetRequiredField(
                typeof(MethodBodyReader_Tests),
                nameof(StaticFieldValue),
                BindingFlags.NonPublic | BindingFlags.Static
            ).MetadataToken;
            var methodToken = GetRequiredMethod(
                typeof(MethodBodyReader_Tests),
                nameof(MethodThatReturnsConstant),
                BindingFlags.NonPublic | BindingFlags.Static
            ).MetadataToken;
            var typeToken = typeof(MethodBodyReader_Tests).MetadataToken;

            var instructions = ConstructCustomInstructions(
                contextMethod,
                CombineBytes(
                    BuildInstructionBytes(OpCodes.Ldsfld, BitConverter.GetBytes(fieldToken)),
                    BuildInstructionBytes(OpCodes.Call, BitConverter.GetBytes(methodToken)),
                    BuildInstructionBytes(OpCodes.Ldtoken, BitConverter.GetBytes(typeToken)),
                    BuildInstructionBytes(OpCodes.Box, BitConverter.GetBytes(typeToken))
                )
            );

            instructions.Should().HaveCount(4);
            instructions[0].Operand.Should().BeAssignableTo<FieldInfo>();
            ((FieldInfo)instructions[0].Operand).Name.Should().Be(nameof(StaticFieldValue));
            instructions[1].Operand.Should().BeAssignableTo<MethodInfo>();
            ((MethodInfo)instructions[1].Operand)
                .Name.Should()
                .Be(nameof(MethodThatReturnsConstant));
            instructions[2].Operand.Should().Be(typeof(MethodBodyReader_Tests));
            instructions[3].Operand.Should().Be(typeof(MethodBodyReader_Tests));
        }

        [TestMethod]
        public void ConstructInstructions_WithInvalidMetadataTokens_LeavesOperandsNull()
        {
            var contextMethod = GetRequiredMethod(
                typeof(MethodBodyReader_Tests),
                nameof(SimpleTestMethod),
                BindingFlags.NonPublic | BindingFlags.Static
            );

            var instructions = ConstructCustomInstructions(
                contextMethod,
                CombineBytes(
                    BuildInstructionBytes(OpCodes.Ldsfld, BitConverter.GetBytes(int.MaxValue)),
                    BuildInstructionBytes(OpCodes.Call, BitConverter.GetBytes(int.MaxValue)),
                    BuildInstructionBytes(OpCodes.Ldtoken, BitConverter.GetBytes(int.MaxValue))
                )
            );

            instructions.Should().HaveCount(3);
            instructions.Should().OnlyContain(instruction => instruction.Operand == null);
        }

        [TestMethod]
        public void ConstructInstructions_WithBranchOperands_ComputesAbsoluteTargets()
        {
            var contextMethod = GetRequiredMethod(
                typeof(MethodBodyReader_Tests),
                nameof(SimpleTestMethod),
                BindingFlags.NonPublic | BindingFlags.Static
            );

            var shortBranchInstruction = ConstructCustomInstructions(
                    contextMethod,
                    BuildInstructionBytes(OpCodes.Br_S, new byte[] { 0x00 })
                )
                .Single();
            var longBranchInstruction = ConstructCustomInstructions(
                    contextMethod,
                    BuildInstructionBytes(OpCodes.Br, BitConverter.GetBytes(0))
                )
                .Single();

            shortBranchInstruction.Operand.Should().Be(2);
            longBranchInstruction.Operand.Should().Be(5);
        }

        [TestMethod]
        public void ConstructInstructions_WithSwitchOperand_ParsesWithoutThrowing()
        {
            var contextMethod = GetRequiredMethod(
                typeof(MethodBodyReader_Tests),
                nameof(SimpleTestMethod),
                BindingFlags.NonPublic | BindingFlags.Static
            );

            var instructions = ConstructCustomInstructions(
                contextMethod,
                BuildInstructionBytes(
                    OpCodes.Switch,
                    CombineBytes(
                        BitConverter.GetBytes(2),
                        BitConverter.GetBytes(0),
                        BitConverter.GetBytes(4)
                    )
                )
            );

            instructions.Should().ContainSingle();
            instructions[0].Code.Should().Be(OpCodes.Switch);
            instructions[0].Operand.Should().BeNull();
        }

        [TestMethod]
        public void GetRefferencedOperand_WithKnownMetadataToken_ReturnsResolvedType()
        {
            var methodInfo = GetRequiredMethod(
                typeof(MethodBodyReader_Tests),
                nameof(SimpleTestMethod),
                BindingFlags.NonPublic | BindingFlags.Static
            );
            var reader = CreateReader(methodInfo);

            var resolvedType = reader.GetRefferencedOperand(
                methodInfo.Module,
                typeof(string).MetadataToken
            );

            resolvedType.Should().Be(typeof(string));
        }

        [TestMethod]
        public void GetRefferencedOperand_WithUnknownMetadataToken_ReturnsNull()
        {
            var methodInfo = GetRequiredMethod(
                typeof(MethodBodyReader_Tests),
                nameof(SimpleTestMethod),
                BindingFlags.NonPublic | BindingFlags.Static
            );
            var reader = CreateReader(methodInfo);

            var resolvedType = reader.GetRefferencedOperand(methodInfo.Module, int.MaxValue);

            resolvedType.Should().BeNull();
        }

        [TestMethod]
        public void PrivateReadMethods_ReturnExpectedIntegralValues()
        {
            var reader = CreateReaderWithoutBody();

            InvokeReadMethod<int>(
                    reader,
                    "ReadInt16",
                    new byte[] { 0x34, 0x12 },
                    out int int16Position
                )
                .Should()
                .Be(0x1234);
            InvokeReadMethod<ushort>(
                    reader,
                    "ReadUInt16",
                    new byte[] { 0x78, 0x56 },
                    out int uint16Position
                )
                .Should()
                .Be((ushort)0x5678);
            InvokeReadMethod<int>(
                    reader,
                    "ReadInt32",
                    new byte[] { 1, 0, 0, 0 },
                    out int int32Position
                )
                .Should()
                .Be(1);
            InvokeReadMethod<ulong>(
                    reader,
                    "ReadInt64",
                    new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 },
                    out int int64Position
                )
                .Should()
                .Be(1UL);

            int16Position.Should().Be(2);
            uint16Position.Should().Be(2);
            int32Position.Should().Be(4);
            int64Position.Should().Be(8);
        }

        [TestMethod]
        public void PrivateReadMethods_ReturnExpectedFloatingPointValues()
        {
            var reader = CreateReaderWithoutBody();

            InvokeReadMethod<double>(
                    reader,
                    "ReadDouble",
                    new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 },
                    out int doublePosition
                )
                .Should()
                .Be(1d);
            InvokeReadMethod<float>(
                    reader,
                    "ReadSingle",
                    new byte[] { 1, 0, 0, 0 },
                    out int singlePosition
                )
                .Should()
                .Be(1f);

            doublePosition.Should().Be(8);
            singlePosition.Should().Be(4);
        }

        [TestMethod]
        public void PrivateReadMethods_ReturnExpectedByteValues()
        {
            var reader = CreateReaderWithoutBody();

            InvokeReadMethod<sbyte>(reader, "ReadSByte", new byte[] { 0xFF }, out int sbytePosition)
                .Should()
                .Be(-1);
            InvokeReadMethod<byte>(reader, "ReadByte", new byte[] { 0xAB }, out int bytePosition)
                .Should()
                .Be(0xAB);

            sbytePosition.Should().Be(1);
            bytePosition.Should().Be(1);
        }

        #endregion

        #region Helper Methods

        private abstract class AbstractMethodContainer
        {
            public abstract void MethodWithoutBody();
        }

        private static MethodBodyReader CreateReaderWithoutBody()
        {
            return CreateReader(
                GetRequiredMethod(
                    typeof(AbstractMethodContainer),
                    nameof(AbstractMethodContainer.MethodWithoutBody),
                    BindingFlags.Public | BindingFlags.Instance
                )
            );
        }

        private static MethodBodyReader CreateReader(MethodInfo methodInfo)
        {
            ILGlobals.LoadOpCodes();
            return new MethodBodyReader(methodInfo);
        }

        private static IReadOnlyList<ILInstruction> ConstructCustomInstructions(
            MethodInfo contextMethod,
            byte[] customIl
        )
        {
            var reader = CreateReader(contextMethod);

            SetIl(reader, customIl);
            InvokePrivate(reader, "ConstructInstructions", contextMethod.Module);

            return reader.instructions
                ?? throw new InvalidOperationException("Instructions were not constructed.");
        }

        private static byte[] BuildInstructionBytes(OpCode opCode, byte[] operandBytes)
        {
            var bytes = new List<byte>();
            if (opCode.Size == 1)
            {
                bytes.Add((byte)opCode.Value);
            }
            else
            {
                bytes.Add(0xFE);
                bytes.Add((byte)(opCode.Value & 0xFF));
            }

            bytes.AddRange(operandBytes);
            return bytes.ToArray();
        }

        private static byte[] CombineBytes(params byte[][] segments)
        {
            return segments.SelectMany(segment => segment).ToArray();
        }

        private static FieldInfo GetRequiredField(
            Type declaringType,
            string name,
            BindingFlags bindingFlags
        )
        {
            return declaringType.GetField(name, bindingFlags)
                ?? throw new InvalidOperationException($"Could not find field '{name}'.");
        }

        private static MethodInfo GetRequiredMethod(
            Type declaringType,
            string name,
            BindingFlags bindingFlags
        )
        {
            return declaringType.GetMethod(name, bindingFlags)
                ?? throw new InvalidOperationException($"Could not find method '{name}'.");
        }

        private static T InvokeReadMethod<T>(
            MethodBodyReader reader,
            string methodName,
            byte[] bytes,
            out int finalPosition
        )
        {
            SetIl(reader, bytes);
            object[] parameters = { bytes, 0 };
            var result = InvokePrivate(reader, methodName, parameters);
            finalPosition = (int)parameters[1];
            return (T)result;
        }

        private static object InvokePrivate(
            MethodBodyReader reader,
            string methodName,
            params object[] parameters
        )
        {
            var method = typeof(MethodBodyReader).GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            method.Should().NotBeNull();
            return method.Invoke(reader, parameters);
        }

        private static void SetIl(MethodBodyReader reader, byte[] bytes)
        {
            var field = typeof(MethodBodyReader).GetField(
                "il",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            field.Should().NotBeNull();
            field.SetValue(reader, bytes);
        }

        private static int SimpleTestMethod()
        {
            int x = 1;
            int y = 2;
            return x + y;
        }

        private static string MethodThatLoadsString()
        {
            return "hello";
        }

        private static int MethodThatReturnsConstant()
        {
            return StaticFieldValue;
        }

        private static void VoidTestMethod()
        {
            var s = "hello";
            _ = s.Length;
        }

        #endregion
    }
}
