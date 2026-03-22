#nullable enable

using System;
using System.Reflection;
using System.Reflection.Emit;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Reflection;
using UtilitiesCS.NewtonsoftHelpers.MonoExtension;

namespace UtilitiesCS.Test.NewtonsoftHelpers.MonoExtensionTests
{
    [TestClass]
    public class MonoExtensionCoverageTests
    {
        private static readonly ConstructorInfo InstructionConstructor =
            typeof(Instruction).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                types: new[] { typeof(int), typeof(OpCode) },
                modifiers: null
            )
            ?? throw new InvalidOperationException(
                "Mono.Reflection.Instruction constructor was not found."
            );

        private static readonly FieldInfo TokenFieldInfo =
            typeof(MonoExtensionCoverageTests).GetField(
                nameof(TokenField),
                BindingFlags.NonPublic | BindingFlags.Static
            ) ?? throw new InvalidOperationException("Test field metadata was not found.");

        private static readonly MethodInfo TokenMethodInfo =
            typeof(MonoExtensionCoverageTests).GetMethod(
                nameof(TokenMethod),
                BindingFlags.NonPublic | BindingFlags.Static
            ) ?? throw new InvalidOperationException("Test method metadata was not found.");

        private static readonly FieldInfo InstructionOperandField =
            typeof(Instruction).GetField("operand", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Instruction operand field was not found.");

        private static readonly object UnsupportedOperand = "unsupported-operand";

        private static readonly int TokenField = 42;

        [TestMethod]
        public void EmitOperand_InlineBrTarget_ThrowsNotImplementedException()
        {
            // Arrange: br uses InlineBrTarget and currently throws by design.
            var (methodBuilder, generator) = CreateMethodContext();
            var instruction = CreateInstruction(OpCodes.Br);

            // Act
            Action act = () => instruction.EmitOperand(generator, methodBuilder);

            // Assert
            act.Should().Throw<NotImplementedException>();
        }

        [TestMethod]
        public void EmitOperand_ShortInlineI_NonLdcI4S_EmitsWithoutThrow()
        {
            // Arrange: unaligned. is a ShortInlineI opcode that uses the byte branch.
            var (methodBuilder, generator) = CreateMethodContext();
            var instruction = CreateInstruction(OpCodes.Unaligned, (byte)1);

            // Act
            Action act = () => instruction.EmitOperand(generator, methodBuilder);

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void EmitOperand_InlineTok_MethodInfoOperand_EmitsWithoutThrow()
        {
            // Arrange: ldtoken can legally target a method handle.
            var (methodBuilder, generator) = CreateMethodContext();
            var instruction = CreateInstruction(OpCodes.Ldtoken, TokenMethodInfo);

            // Act
            Action act = () => instruction.EmitOperand(generator, methodBuilder);

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void EmitOperand_InlineTok_FieldInfoOperand_EmitsWithoutThrow()
        {
            // Arrange: ldtoken can legally target a field handle.
            var (methodBuilder, generator) = CreateMethodContext();
            var instruction = CreateInstruction(OpCodes.Ldtoken, TokenFieldInfo);

            // Act
            Action act = () => instruction.EmitOperand(generator, methodBuilder);

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void EmitOperand_InlineTok_InvalidOperand_ThrowsInvalidCastException()
        {
            // Arrange: InlineTok accepts type, method, or field operands only.
            var (methodBuilder, generator) = CreateMethodContext();
            var instruction = CreateInstruction(OpCodes.Ldtoken, UnsupportedOperand);

            // Act
            Action act = () => instruction.EmitOperand(generator, methodBuilder);

            // Assert
            act.Should()
                .Throw<InvalidCastException>()
                .WithMessage("*valid type, method, or field*");
        }

        [TestMethod]
        public void EmitOperand_InlineMethod_InvalidOperand_ThrowsInvalidCastException()
        {
            // Arrange: InlineMethod accepts MethodInfo or ConstructorInfo operands only.
            var (methodBuilder, generator) = CreateMethodContext();
            var instruction = CreateInstruction(OpCodes.Call, UnsupportedOperand);

            // Act
            Action act = () => instruction.EmitOperand(generator, methodBuilder);

            // Assert
            act.Should()
                .Throw<InvalidCastException>()
                .WithMessage("*MethodInfo nor ConstructorInfo*");
        }

        private static (MethodBuilder MethodBuilder, ILGenerator Generator) CreateMethodContext()
        {
            var assembly = AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName($"MonoExtensionCoverage_{Guid.NewGuid():N}"),
                AssemblyBuilderAccess.Run
            );
            var module = assembly.DefineDynamicModule("CoverageModule");
            var typeBuilder = module.DefineType("CoverageType", TypeAttributes.Public);
            var methodBuilder = typeBuilder.DefineMethod(
                "CoverageMethod",
                MethodAttributes.Public | MethodAttributes.Static,
                typeof(void),
                Type.EmptyTypes
            );

            return (methodBuilder, methodBuilder.GetILGenerator());
        }

        private static Instruction CreateInstruction(
            OpCode opCode,
            object? operand = null,
            int offset = 0
        )
        {
            var instruction = (Instruction)
                InstructionConstructor.Invoke(new object[] { offset, opCode });
            InstructionOperandField.SetValue(instruction, operand);
            return instruction;
        }

        private static void TokenMethod() { }
    }
}
