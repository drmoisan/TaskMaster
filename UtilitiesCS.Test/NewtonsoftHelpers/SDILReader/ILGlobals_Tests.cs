using System.Reflection;
using System.Reflection.Emit;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDILReader;

namespace UtilitiesCS.Test.NewtonsoftHelpers.SDILReader
{
    [TestClass]
    public class ILGlobals_Tests
    {
        /// <summary>
        /// The first read of the static field is what triggers publication, so this test performs
        /// no Act: reading the field in the Assert is the property under test.
        /// </summary>
        [TestMethod]
        public void SingleByteOpCodes_IsPublishedWithFullLength()
        {
            // Assert
            ILGlobals.singleByteOpCodes.Should().NotBeNull();
            ILGlobals.singleByteOpCodes.Length.Should().Be(0x100);
        }

        /// <summary>
        /// The multi-byte counterpart of the single-byte publication assertion above. It likewise
        /// performs no Act, because reading the field is what triggers publication.
        /// </summary>
        [TestMethod]
        public void MultiByteOpCodes_IsPublishedWithFullLength()
        {
            // Assert
            ILGlobals.multiByteOpCodes.Should().NotBeNull();
            ILGlobals.multiByteOpCodes.Length.Should().Be(0x100);
        }

        /// <summary>
        /// Primary regression gate for issue #824. The opcode tables must be published exactly
        /// once, so a call to <see cref="ILGlobals.LoadOpCodes"/> must force type initialization
        /// rather than allocate and refill a replacement table. Reference identity across the call
        /// is what distinguishes the two behaviours.
        /// </summary>
        [TestMethod]
        public void LoadOpCodes_DoesNotRepublishPublishedTables()
        {
            // Arrange - capture the currently published table references.
            var singleBefore = ILGlobals.singleByteOpCodes;
            var multiBefore = ILGlobals.multiByteOpCodes;

            // Act
            ILGlobals.LoadOpCodes();

            // Assert
            ILGlobals
                .singleByteOpCodes.Should()
                .BeSameAs(
                    singleBefore,
                    "LoadOpCodes() must force type initialization rather than republish the "
                        + "single-byte table, because a concurrent reader would otherwise observe "
                        + "a freshly allocated array before the fill loop has populated it"
                );
            ILGlobals
                .multiByteOpCodes.Should()
                .BeSameAs(
                    multiBefore,
                    "LoadOpCodes() must force type initialization rather than republish the "
                        + "multi-byte table, because a concurrent reader would otherwise observe "
                        + "a freshly allocated array before the fill loop has populated it"
                );
        }

        /// <summary>
        /// Structural anti-regression gate for issue #824. A future edit that removes the
        /// <c>readonly</c> modifier from the single-byte table would reopen the publication window
        /// with no other test signal, so the field's init-only encoding is asserted directly.
        /// </summary>
        [TestMethod]
        public void SingleByteOpCodes_FieldIsInitOnly()
        {
            // Arrange & Act
            var field = typeof(ILGlobals).GetField(
                nameof(ILGlobals.singleByteOpCodes),
                BindingFlags.Public | BindingFlags.Static
            );

            // Assert
            field.Should().NotBeNull("the public static single-byte opcode table must exist");
            field!
                .IsInitOnly.Should()
                .BeTrue(
                    "a readonly field can only be assigned by the static constructor, so any "
                        + "future reassignment outside it is a compile-time error rather than a "
                        + "runtime race"
                );
        }

        /// <summary>
        /// Structural anti-regression gate for issue #824, the multi-byte counterpart of the
        /// single-byte init-only assertion above.
        /// </summary>
        [TestMethod]
        public void MultiByteOpCodes_FieldIsInitOnly()
        {
            // Arrange & Act
            var field = typeof(ILGlobals).GetField(
                nameof(ILGlobals.multiByteOpCodes),
                BindingFlags.Public | BindingFlags.Static
            );

            // Assert
            field.Should().NotBeNull("the public static multi-byte opcode table must exist");
            field!
                .IsInitOnly.Should()
                .BeTrue(
                    "a readonly field can only be assigned by the static constructor, so any "
                        + "future reassignment outside it is a compile-time error rather than a "
                        + "runtime race"
                );
        }

        /// <summary>
        /// Supporting test, not a gate for issue #824. It passes on the unfixed tree as well,
        /// because a single-threaded test observes a fully populated table either way. Its value is
        /// that it replaces two weak spot checks and would catch a fix that publishes the tables
        /// safely but fills them wrongly.
        /// </summary>
        [TestMethod]
        public void OpCodeTables_ContainEveryOpCodeDeclaredOnOpCodes()
        {
            // Arrange
            FieldInfo[] fields = typeof(OpCodes).GetFields(
                BindingFlags.Public | BindingFlags.Static
            );
            int assertedCount = 0;

            // Act & Assert
            foreach (FieldInfo field in fields)
            {
                if (field.FieldType != typeof(OpCode))
                {
                    continue;
                }

                OpCode declared = (OpCode)field.GetValue(null)!;
                ushort value = (ushort)declared.Value;
                if (value < 0x100)
                {
                    ILGlobals
                        .singleByteOpCodes[value]
                        .Should()
                        .Be(
                            declared,
                            "the single-byte table must hold {0} at index 0x{1:X2}",
                            field.Name,
                            value
                        );
                }
                else
                {
                    (value & 0xff00)
                        .Should()
                        .Be(
                            0xfe00,
                            "every opcode at or above 0x100 must carry the 0xFE prefix, and {0} "
                                + "does not",
                            field.Name
                        );
                    ILGlobals
                        .multiByteOpCodes[value & 0xff]
                        .Should()
                        .Be(
                            declared,
                            "the multi-byte table must hold {0} at index 0x{1:X2}",
                            field.Name,
                            value & 0xff
                        );
                }

                assertedCount++;
            }

            assertedCount
                .Should()
                .BeGreaterThan(
                    0,
                    "the reflection enumeration must find opcode fields, otherwise this test "
                        + "would assert nothing at all"
                );
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
