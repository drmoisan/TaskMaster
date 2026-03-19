using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.HelperClasses;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class GenericBitwise_Tests
    {
        [TestMethod]
        public void BitwiseOperations_HandleAndOrXorAndNotForIntFlags()
        {
            // Arrange
            var sut = new GenericBitwise<IntFlags>();

            // Act / Assert
            sut.And(IntFlags.First | IntFlags.Second, IntFlags.Second | IntFlags.Third)
                .Should()
                .Be(IntFlags.Second);
            sut.Or(IntFlags.First, IntFlags.Third).Should().Be(IntFlags.First | IntFlags.Third);
            sut.Xor(IntFlags.First | IntFlags.Second, IntFlags.Second | IntFlags.Third)
                .Should()
                .Be(IntFlags.First | IntFlags.Third);
            sut.Not(IntFlags.First).Should().Be(~IntFlags.First);
        }

        [TestMethod]
        public void SequenceOverloads_CombineFlagsIncludingZeroAndDuplicates()
        {
            // Arrange
            var sut = new GenericBitwise<IntFlags>();

            // Act / Assert
            sut.Or(new[] { IntFlags.None, IntFlags.First, IntFlags.Third })
                .Should()
                .Be(IntFlags.First | IntFlags.Third);
            sut.And(new[] { IntFlags.All, IntFlags.First | IntFlags.Second, IntFlags.First })
                .Should()
                .Be(IntFlags.First);
            sut.Xor(new[] { IntFlags.First, IntFlags.Second, IntFlags.Second })
                .Should()
                .Be(IntFlags.First);
        }

        [TestMethod]
        public void All_ReturnsCombinationOfAllDefinedByteFlagsIncludingHighBit()
        {
            // Arrange
            var sut = new GenericBitwise<ByteFlags>();

            // Act
            var result = sut.All();

            // Assert
            result.Should().Be(ByteFlags.All);
            sut.Or(ByteFlags.First, ByteFlags.High).Should().Be(ByteFlags.First | ByteFlags.High);
            sut.And(ByteFlags.All, ByteFlags.High).Should().Be(ByteFlags.High);
            sut.Not(ByteFlags.None).Should().Be(~ByteFlags.None);
        }

        [TestMethod]
        public void Operations_WorkWithLongBackedEnumsAndBoundaryBitValues()
        {
            // Arrange
            var sut = new GenericBitwise<LongFlags>();

            // Act / Assert
            sut.Or(LongFlags.Low, LongFlags.High).Should().Be(LongFlags.Low | LongFlags.High);
            sut.And(LongFlags.All, LongFlags.High).Should().Be(LongFlags.High);
            sut.Xor(LongFlags.All, LongFlags.High).Should().Be(LongFlags.Low);
            sut.All().Should().Be(LongFlags.All);
        }

        [Flags]
        private enum IntFlags
        {
            None = 0,
            First = 1,
            Second = 2,
            Third = 4,
            All = First | Second | Third,
        }

        [Flags]
        private enum ByteFlags : byte
        {
            None = 0,
            First = 1,
            Second = 2,
            High = 128,
            All = 131,
        }

        [Flags]
        private enum LongFlags : long
        {
            None = 0,
            Low = 1L,
            High = 1L << 40,
            All = Low | High,
        }
    }
}
