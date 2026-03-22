using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Reflection;
using UtilitiesCS.NewtonsoftHelpers.MonoExtension;

namespace UtilitiesCS.Test.NewtonsoftHelpers
{
    [TestClass]
    public class MonoExtension_Tests
    {
        private static (
            TypeBuilder Type,
            MethodBuilder Method,
            ILGenerator Gen
        ) CreateMethodContext(Type returnType = null, Type[] parameterTypes = null)
        {
            returnType = returnType ?? typeof(void);
            parameterTypes = parameterTypes ?? Type.EmptyTypes;
            var assembly = AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName("MonoExtTest_" + Guid.NewGuid().ToString("N")),
                AssemblyBuilderAccess.Run
            );
            var module = assembly.DefineDynamicModule("TestModule");
            var type = module.DefineType("TestType", TypeAttributes.Public);
            var method = type.DefineMethod(
                "TestMethod",
                MethodAttributes.Public | MethodAttributes.Static,
                returnType,
                parameterTypes
            );
            var gen = method.GetILGenerator();
            return (type, method, gen);
        }

        #region InlineNone

        [TestMethod]
        public void EmitOperand_InlineNone_EmitsWithoutThrow()
        {
            // Arrange: Nop and Ret are InlineNone
            var instructions = typeof(MonoExtension_Tests)
                .GetMethod(nameof(SimpleVoidMethod), BindingFlags.NonPublic | BindingFlags.Static)
                .GetInstructions();
            var inlineNoneInstrs = instructions
                .Where(i => i.OpCode.OperandType == OperandType.InlineNone)
                .ToArray();
            var (_, mb, gen) = CreateMethodContext();

            // Act & Assert
            inlineNoneInstrs
                .Should()
                .NotBeEmpty("SimpleVoidMethod should produce InlineNone opcodes");
            foreach (var instr in inlineNoneInstrs)
            {
                Action act = () => instr.EmitOperand(gen, mb);
                act.Should().NotThrow();
            }
        }

        private static void SimpleVoidMethod()
        {
            // nop + ret
        }

        #endregion

        #region InlineString

        [TestMethod]
        public void EmitOperand_InlineString_EmitsWithoutThrow()
        {
            // Arrange: ldstr is InlineString
            var instructions = typeof(MonoExtension_Tests)
                .GetMethod(nameof(StringReturnMethod), BindingFlags.NonPublic | BindingFlags.Static)
                .GetInstructions();
            var stringInstrs = instructions
                .Where(i => i.OpCode.OperandType == OperandType.InlineString)
                .ToArray();
            var (_, mb, gen) = CreateMethodContext(typeof(string));

            // Act & Assert
            stringInstrs.Should().NotBeEmpty("method should produce InlineString opcodes");
            foreach (var instr in stringInstrs)
            {
                Action act = () => instr.EmitOperand(gen, mb);
                act.Should().NotThrow();
            }
        }

        private static string StringReturnMethod()
        {
            return "hello world";
        }

        #endregion

        #region ShortInlineI

        [TestMethod]
        public void EmitOperand_ShortInlineI_Ldc_I4_S_EmitsWithoutThrow()
        {
            // Arrange: ldc.i4.s is ShortInlineI with Ldc_I4_S
            var instructions = typeof(MonoExtension_Tests)
                .GetMethod(nameof(SmallIntMethod), BindingFlags.NonPublic | BindingFlags.Static)
                .GetInstructions();
            var shortInInstr = instructions
                .Where(i =>
                    i.OpCode.OperandType == OperandType.ShortInlineI && i.OpCode == OpCodes.Ldc_I4_S
                )
                .ToArray();
            var (_, mb, gen) = CreateMethodContext(typeof(int));

            // Act & Assert
            shortInInstr.Should().NotBeEmpty("method should produce Ldc_I4_S opcodes");
            foreach (var instr in shortInInstr)
            {
                Action act = () => instr.EmitOperand(gen, mb);
                act.Should().NotThrow();
            }
        }

        private static int SmallIntMethod()
        {
            return 42; // ldc.i4.s 42
        }

        #endregion

        #region InlineI

        [TestMethod]
        public void EmitOperand_InlineI_EmitsWithoutThrow()
        {
            // Arrange: ldc.i4 is InlineI for large int constants
            var instructions = typeof(MonoExtension_Tests)
                .GetMethod(nameof(LargeIntMethod), BindingFlags.NonPublic | BindingFlags.Static)
                .GetInstructions();
            var intInstrs = instructions
                .Where(i => i.OpCode.OperandType == OperandType.InlineI)
                .ToArray();
            var (_, mb, gen) = CreateMethodContext(typeof(int));

            // Act & Assert
            if (intInstrs.Length > 0)
            {
                foreach (var instr in intInstrs)
                {
                    Action act = () => instr.EmitOperand(gen, mb);
                    act.Should().NotThrow();
                }
            }
        }

        private static int LargeIntMethod()
        {
            return 1000000; // ldc.i4 1000000
        }

        #endregion

        #region InlineR (double) and ShortInlineR (float)

        [TestMethod]
        public void EmitOperand_InlineR_EmitsWithoutThrow()
        {
            // Arrange: ldc.r8 is InlineR for double
            var instructions = typeof(MonoExtension_Tests)
                .GetMethod(nameof(DoubleMethod), BindingFlags.NonPublic | BindingFlags.Static)
                .GetInstructions();
            var doubleInstrs = instructions
                .Where(i => i.OpCode.OperandType == OperandType.InlineR)
                .ToArray();
            var (_, mb, gen) = CreateMethodContext(typeof(double));

            // Act & Assert
            doubleInstrs.Should().NotBeEmpty("method should produce ldc.r8 opcodes");
            foreach (var instr in doubleInstrs)
            {
                Action act = () => instr.EmitOperand(gen, mb);
                act.Should().NotThrow();
            }
        }

        private static double DoubleMethod()
        {
            return 3.14159265358979;
        }

        [TestMethod]
        public void EmitOperand_ShortInlineR_EmitsWithoutThrow()
        {
            // Arrange: ldc.r4 is ShortInlineR for float
            var instructions = typeof(MonoExtension_Tests)
                .GetMethod(nameof(FloatMethod), BindingFlags.NonPublic | BindingFlags.Static)
                .GetInstructions();
            var floatInstrs = instructions
                .Where(i => i.OpCode.OperandType == OperandType.ShortInlineR)
                .ToArray();
            var (_, mb, gen) = CreateMethodContext(typeof(float));

            // Act & Assert
            floatInstrs.Should().NotBeEmpty("method should produce ldc.r4 opcodes");
            foreach (var instr in floatInstrs)
            {
                Action act = () => instr.EmitOperand(gen, mb);
                act.Should().NotThrow();
            }
        }

        private static float FloatMethod()
        {
            return 2.71828f;
        }

        #endregion

        #region InlineI8 (long)

        [TestMethod]
        public void EmitOperand_InlineI8_EmitsWithoutThrow()
        {
            // Arrange: ldc.i8 is InlineI8 for long
            var instructions = typeof(MonoExtension_Tests)
                .GetMethod(nameof(LongMethod), BindingFlags.NonPublic | BindingFlags.Static)
                .GetInstructions();
            var longInstrs = instructions
                .Where(i => i.OpCode.OperandType == OperandType.InlineI8)
                .ToArray();
            var (_, mb, gen) = CreateMethodContext(typeof(long));

            // Act & Assert
            longInstrs.Should().NotBeEmpty("method should produce ldc.i8 opcodes");
            foreach (var instr in longInstrs)
            {
                Action act = () => instr.EmitOperand(gen, mb);
                act.Should().NotThrow();
            }
        }

        private static long LongMethod()
        {
            return 9999999999L;
        }

        #endregion

        #region InlineType

        [TestMethod]
        public void EmitOperand_InlineType_EmitsWithoutThrow()
        {
            // Arrange: newarr/castclass is InlineType
            var instructions = typeof(MonoExtension_Tests)
                .GetMethod(nameof(TypeMethod), BindingFlags.NonPublic | BindingFlags.Static)
                .GetInstructions();
            var typeInstrs = instructions
                .Where(i => i.OpCode.OperandType == OperandType.InlineType)
                .ToArray();
            var (_, mb, gen) = CreateMethodContext(typeof(int[]));

            // Act & Assert
            typeInstrs.Should().NotBeEmpty("method should produce InlineType opcodes");
            foreach (var instr in typeInstrs)
            {
                Action act = () => instr.EmitOperand(gen, mb);
                act.Should().NotThrow();
            }
        }

        private static int[] TypeMethod()
        {
            return new int[0]; // newarr int
        }

        #endregion

        #region InlineMethod (MethodInfo branch)

        [TestMethod]
        public void EmitOperand_InlineMethod_MethodInfo_EmitsWithoutThrow()
        {
            // Arrange: call is InlineMethod with MethodInfo operand
            var instructions = typeof(MonoExtension_Tests)
                .GetMethod(nameof(CallMethodMethod), BindingFlags.NonPublic | BindingFlags.Static)
                .GetInstructions();
            var methodInstrs = instructions
                .Where(i =>
                    i.OpCode.OperandType == OperandType.InlineMethod && i.Operand is MethodInfo
                )
                .ToArray();
            var (_, mb, gen) = CreateMethodContext(typeof(string));

            // Act & Assert
            methodInstrs.Should().NotBeEmpty("method should produce call opcodes");
            foreach (var instr in methodInstrs)
            {
                Action act = () => instr.EmitOperand(gen, mb);
                act.Should().NotThrow();
            }
        }

        private static string CallMethodMethod()
        {
            return 42.ToString(); // calls Int32.ToString MethodInfo
        }

        [TestMethod]
        public void EmitOperand_InlineMethod_ConstructorInfo_EmitsWithoutThrow()
        {
            // Arrange: newobj is InlineMethod with ConstructorInfo operand
            var instructions = typeof(MonoExtension_Tests)
                .GetMethod(nameof(NewObjMethod), BindingFlags.NonPublic | BindingFlags.Static)
                .GetInstructions();
            var ctorInstrs = instructions
                .Where(i =>
                    i.OpCode.OperandType == OperandType.InlineMethod && i.Operand is ConstructorInfo
                )
                .ToArray();
            var (_, mb, gen) = CreateMethodContext(typeof(object));

            // Act & Assert
            ctorInstrs.Should().NotBeEmpty("method should produce newobj opcodes");
            foreach (var instr in ctorInstrs)
            {
                Action act = () => instr.EmitOperand(gen, mb);
                act.Should().NotThrow();
            }
        }

        private static object NewObjMethod()
        {
            return new object(); // newobj System.Object::.ctor
        }

        #endregion

        #region InlineField

        [TestMethod]
        public void EmitOperand_InlineField_EmitsWithoutThrow()
        {
            // Arrange: ldsfld/stsfld is InlineField
            var instructions = typeof(MonoExtension_Tests)
                .GetMethod(nameof(FieldMethod), BindingFlags.NonPublic | BindingFlags.Static)
                .GetInstructions();
            var fieldInstrs = instructions
                .Where(i => i.OpCode.OperandType == OperandType.InlineField)
                .ToArray();
            var (_, mb, gen) = CreateMethodContext(typeof(string));

            // Act & Assert
            fieldInstrs.Should().NotBeEmpty("method should produce field access opcodes");
            foreach (var instr in fieldInstrs)
            {
                Action act = () => instr.EmitOperand(gen, mb);
                act.Should().NotThrow();
            }
        }

        private static string _testField = "field_value";

        private static string FieldMethod()
        {
            return _testField; // ldsfld
        }

        #endregion

        #region ShortInlineBrTarget

        [TestMethod]
        public void EmitOperand_ShortInlineBrTarget_EmitsWithoutThrow()
        {
            // Arrange: br.s/brfalse.s is ShortInlineBrTarget
            var instructions = typeof(MonoExtension_Tests)
                .GetMethod(nameof(BranchMethod), BindingFlags.NonPublic | BindingFlags.Static)
                .GetInstructions();
            var brInstrs = instructions
                .Where(i => i.OpCode.OperandType == OperandType.ShortInlineBrTarget)
                .ToArray();
            var (_, mb, gen) = CreateMethodContext(typeof(int), new[] { typeof(int) });

            // Act & Assert
            brInstrs.Should().NotBeEmpty("method should produce short branch opcodes");
            foreach (var instr in brInstrs)
            {
                Action act = () => instr.EmitOperand(gen, mb);
                act.Should().NotThrow();
            }
        }

        private static int BranchMethod(int x)
        {
            if (x > 0)
                return 1;
            return 0;
        }

        #endregion

        #region Exception-Throwing Branches

        [TestMethod]
        public void EmitOperand_InlineSwitch_ThrowsNotImplementedException()
        {
            // Arrange: switch IL instruction has InlineSwitch operand type
            var instructions = typeof(MonoExtension_Tests)
                .GetMethod(nameof(SwitchMethod), BindingFlags.NonPublic | BindingFlags.Static)
                .GetInstructions();
            var switchInstr = instructions.FirstOrDefault(i =>
                i.OpCode.OperandType == OperandType.InlineSwitch
            );

            if (switchInstr != null)
            {
                var (_, mb, gen) = CreateMethodContext(typeof(int), new[] { typeof(int) });

                // Act
                Action act = () => switchInstr.EmitOperand(gen, mb);

                // Assert
                act.Should().Throw<NotImplementedException>();
            }
        }

        private static int SwitchMethod(int x)
        {
            switch (x)
            {
                case 0:
                    return 10;
                case 1:
                    return 20;
                case 2:
                    return 30;
                case 3:
                    return 40;
                case 4:
                    return 50;
                case 5:
                    return 60;
                case 6:
                    return 70;
                case 7:
                    return 80;
                default:
                    return -1;
            }
        }

        [TestMethod]
        public void EmitOperand_ShortInlineVar_LocalVariable_EmitsWithoutThrow()
        {
            // Arrange: stloc.s/ldloc.s is ShortInlineVar with LocalVariableInfo
            var instructions = typeof(MonoExtension_Tests)
                .GetMethod(nameof(LocalVarMethod), BindingFlags.NonPublic | BindingFlags.Static)
                .GetInstructions();
            var localInstrs = instructions
                .Where(i =>
                    (
                        i.OpCode.OperandType == OperandType.ShortInlineVar
                        || i.OpCode.OperandType == OperandType.InlineVar
                    )
                    && i.Operand is LocalVariableInfo
                    && i.OpCode.Name.Contains("loc")
                )
                .ToArray();
            var (_, mb, gen) = CreateMethodContext(typeof(int));

            // Act & Assert
            if (localInstrs.Length > 0)
            {
                foreach (var instr in localInstrs)
                {
                    Action act = () => instr.EmitOperand(gen, mb);
                    act.Should().NotThrow();
                }
            }
        }

        private static int LocalVarMethod()
        {
            int a = 1;
            int b = 2;
            int c = a + b;
            return c;
        }

        [TestMethod]
        public void EmitOperand_ShortInlineVar_NonLocal_ThrowsNotImplementedException()
        {
            // Arrange: ldarg.s is ShortInlineVar with ParameterInfo (not a local)
            var instructions = typeof(MonoExtension_Tests)
                .GetMethod(nameof(ManyArgMethod), BindingFlags.NonPublic | BindingFlags.Static)
                .GetInstructions();
            var argInstrs = instructions
                .Where(i =>
                    (
                        i.OpCode.OperandType == OperandType.ShortInlineVar
                        || i.OpCode.OperandType == OperandType.InlineVar
                    )
                    && i.Operand is ParameterInfo
                )
                .ToArray();

            if (argInstrs.Length > 0)
            {
                var (_, mb, gen) = CreateMethodContext(
                    typeof(int),
                    Enumerable.Repeat(typeof(int), 10).ToArray()
                );

                // Act
                Action act = () => argInstrs[0].EmitOperand(gen, mb);

                // Assert
                act.Should().Throw<NotImplementedException>();
            }
        }

        private static int ManyArgMethod(
            int a,
            int b,
            int c,
            int d,
            int e,
            int f,
            int g,
            int h,
            int i,
            int j
        )
        {
            // ldarg.s for args beyond index 3
            return a + b + c + d + e + f + g + h + i + j;
        }

        #endregion

        #region InlineTok with Type

        [TestMethod]
        public void EmitOperand_InlineTok_TypeOperand_EmitsWithoutThrow()
        {
            // Arrange: ldtoken is InlineTok, used with typeof
            var instructions = typeof(MonoExtension_Tests)
                .GetMethod(nameof(TypeTokenMethod), BindingFlags.NonPublic | BindingFlags.Static)
                .GetInstructions();
            var tokInstrs = instructions
                .Where(i => i.OpCode.OperandType == OperandType.InlineTok && i.Operand is Type)
                .ToArray();
            var (_, mb, gen) = CreateMethodContext(typeof(Type));

            // Act & Assert
            tokInstrs.Should().NotBeEmpty("method should produce ldtoken opcodes");
            foreach (var instr in tokInstrs)
            {
                Action act = () => instr.EmitOperand(gen, mb);
                act.Should().NotThrow();
            }
        }

        private static Type TypeTokenMethod()
        {
            return typeof(string); // ldtoken + GetTypeFromHandle
        }

        #endregion
    }
}
