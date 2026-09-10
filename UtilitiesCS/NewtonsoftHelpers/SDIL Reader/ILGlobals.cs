#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;

namespace SDILReader
{
    //public enum AssemblyType
    //{
    //    None,
    //    Console,
    //    Application,
    //    Library
    //}

    //public enum BinaryOperator
    //{
    //    Add,
    //    Subtract,
    //    Multiply,
    //    Divide,
    //    Modulus,
    //    ShiftLeft,
    //    ShiftRight,
    //    IdentityEquality,
    //    IdentityInequality,
    //    ValueEquality,
    //    ValueInequality,
    //    BitwiseOr,
    //    BitwiseAnd,
    //    BitwiseExclusiveOr,
    //    BooleanOr,
    //    BooleanAnd,
    //    LessThan,
    //    LessThanOrEqual,
    //    GreaterThan,
    //    GreaterThanOrEqual
    //}

    //public enum ExceptionHandlerType
    //{
    //    Finally,
    //    Catch,
    //    Filter,
    //    Fault
    //}

    //public enum FieldVisibility
    //{
    //    Private,
    //    Public,
    //    Internal,
    //    Protected,
    //}

    //public enum MethodVisibility
    //{
    //    Private,
    //    Public,
    //    Internal,
    //    External,
    //    Protected,
    //}
    //public enum MethodModifier
    //{
    //    Static,
    //    Override,
    //    Abstract,
    //    Virtual,
    //    Final,
    //    None,
    //}

    //public enum ResourceVisibility
    //{
    //    Public,
    //    Private
    //}

    //public enum TypeVisibility
    //{
    //    vPublic,
    //    vProtected,
    //    vInternal,
    //    vProtectedInternal,
    //    vPrivate
    //}

    //public enum ClassModifiers
    //{
    //    mAbstract,
    //    mSealed,
    //    mStatic,
    //    mNone,
    //}

    //public enum UnaryOperator
    //{
    //    Negate,
    //    BooleanNot,
    //    BitwiseNot,
    //    PreIncrement,
    //    PreDecrement,
    //    PostIncrement,
    //    PostDecrement
    //}

    public static class ILGlobals
    {
        public static Dictionary<int, object> Cache = new Dictionary<int, object>();

        /// <summary>
        /// Multi-byte (0xFE-prefixed) opcode table, indexed by the low byte of the opcode value.
        /// Published once by the static constructor of <see cref="ILGlobals"/>, fully populated,
        /// and never reassigned thereafter. <c>readonly</c> prevents reassignment of the array
        /// reference; it does not prevent element mutation, so callers must treat the contents as
        /// read-only.
        /// </summary>
        public static readonly OpCode[] multiByteOpCodes;

        /// <summary>
        /// Single-byte opcode table, indexed by the opcode value. Published once by the static
        /// constructor of <see cref="ILGlobals"/>, fully populated, and never reassigned
        /// thereafter. <c>readonly</c> prevents reassignment of the array reference; it does not
        /// prevent element mutation, so callers must treat the contents as read-only.
        /// </summary>
        public static readonly OpCode[] singleByteOpCodes;
        public static Module[]? modules = null;

        /// <summary>
        /// Builds both opcode tables in locals and publishes each one exactly once, after the
        /// reflection loop has run to completion. Assigning the fields only at the end is what
        /// removes the window in which a reader could observe a table that had been allocated but
        /// not yet filled.
        /// </summary>
        static ILGlobals()
        {
            OpCode[] singleTable = new OpCode[0x100];
            OpCode[] multiTable = new OpCode[0x100];
            FieldInfo[] infoArray1 = typeof(OpCodes).GetFields();
            for (int num1 = 0; num1 < infoArray1.Length; num1++)
            {
                FieldInfo info1 = infoArray1[num1];
                if (info1.FieldType == typeof(OpCode))
                {
                    // Guarded by FieldType == typeof(OpCode) above, so the unbox is safe;
                    // ! preserves behavior against the nullable GetValue return.
                    OpCode code1 = (OpCode)info1.GetValue(null)!;
                    ushort num2 = (ushort)code1.Value;
                    if (num2 < 0x100)
                    {
                        singleTable[(int)num2] = code1;
                    }
                    else
                    {
                        if ((num2 & 0xff00) != 0xfe00)
                        {
                            throw new Exception("Invalid OpCode.");
                        }
                        multiTable[num2 & 0xff] = code1;
                    }
                }
            }
            singleByteOpCodes = singleTable;
            multiByteOpCodes = multiTable;
        }

        /// <summary>
        /// Forces the opcode tables to be published, if they have not been already. Retained for
        /// the five existing call sites; the tables themselves are built by the static constructor,
        /// so this method neither allocates nor reassigns anything and is safe to call repeatedly
        /// from any thread.
        /// </summary>
        public static void LoadOpCodes()
        {
            RuntimeHelpers.RunClassConstructor(typeof(ILGlobals).TypeHandle);
        }

        /// <summary>
        /// Retrieve the friendly name of a type
        /// </summary>
        /// <param name="typeName">
        /// The complete name to the type
        /// </param>
        /// <returns>
        /// The simplified name of the type (i.e. "int" instead f System.Int32)
        /// </returns>
        public static string ProcessSpecialTypes(string typeName)
        {
            string result = typeName;
            switch (typeName)
            {
                case "System.string":
                case "System.String":
                case "String":
                    result = "string";
                    break;
                case "System.Int32":
                case "Int":
                case "Int32":
                    result = "int";
                    break;
            }
            return result;
        }

        //public static string SpaceGenerator(int count)
        //{
        //    string result = "";
        //    for (int i = 0; i < count; i++) result += " ";
        //    return result;
        //}

        //public static string AddBeginSpaces(string source, int count)
        //{
        //    string[] elems = source.Split('\n');
        //    string result = "";
        //    for (int i = 0; i < elems.Length; i++)
        //    {
        //        result += SpaceGenerator(count) + elems[i] + "\n";
        //    }
        //    return result;
        //}
    }
}
