using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToDoModel;

namespace ToDoModel.Test
{
    /// <summary>
    /// Unit tests for <see cref="IDList.GetNextToDoID(string)"/>. The list is constructed only via
    /// the Outlook-free constructors (<see cref="IDList()"/>, <see cref="IDList(IList{string})"/>,
    /// <see cref="IDList(IEnumerable{string})"/>), so no live Outlook is required. Only the pure
    /// base-36 arithmetic path is exercised. Tests that call the successful path initialize the
    /// list's cached maximum ID length before generating an ID so the method does not persist
    /// <c>Settings.Default.MaxLengthOfID</c>. No <c>Filepath</c> is set, so the internal
    /// <c>Serialize()</c> branch is not taken.
    /// </summary>
    [TestClass]
    public class IDListGetNextToDoIDTests
    {
        [TestMethod]
        public void GetNextToDoID_NoCollision_ReturnsNextBase36ValueAndAddsIt()
        {
            // Arrange: seed "0001" => base10 1; next id is 2 => base36 padded "02".
            var list = CreateListWithCachedMaxLength("ZZZZ");

            // Act
            var result = list.GetNextToDoID("0001");

            // Assert
            result.Should().Be("02", "the next base-36 value after seed 1 is 2 => \"02\"");
            list.Should().Contain("02", "the produced id is appended to the list");
        }

        [TestMethod]
        public void GetNextToDoID_IdAlreadyPresent_AdvancesPastTheCollision()
        {
            // Arrange: seed "0001" => next candidate "02" collides; loop advances to "03".
            var list = CreateListWithCachedMaxLength("02");

            // Act
            var result = list.GetNextToDoID("0001");

            // Assert
            result.Should().Be("03", "the increment loop skips the colliding \"02\"");
            list.Should().Contain("03");
        }

        [TestMethod]
        public void GetNextToDoID_MultipleConsecutiveCollisions_AdvancesToFirstFreeValue()
        {
            // Arrange: candidates "02","03","04" all present; first free is "05".
            var list = CreateListWithCachedMaxLength("02", "03", "04");

            // Act
            var result = list.GetNextToDoID("0001");

            // Assert
            result.Should().Be("05", "the loop advances past every consecutive collision");
        }

        [TestMethod]
        public void GetNextToDoID_LengthBoundaryRollover_ProducesLongerId()
        {
            // Arrange: seed "ZZ" => base10 (35*36 + 35) = 1295; next is 1296 => base36 "100"
            // (three digits, padded to even length is still "100" because 3 % 2 == 1 prepends "0"
            // => "0100"). Assert the produced value and its length to cover the rollover branch.
            var list = CreateListWithCachedMaxLength("0000");

            // Act
            var result = list.GetNextToDoID("ZZ");

            // Assert
            result
                .Should()
                .Be("0100", "1296 in base-36 is \"100\", padded to even length \"0100\"");
            result.Length.Should().Be(4, "the id rolled over to a longer, even-padded string");
        }

        [TestMethod]
        public void GetNextToDoID_NullSeed_ThrowsArgumentException()
        {
            // Arrange
            var list = new IDList(new List<string>());

            // Act
            Action act = () => list.GetNextToDoID(null);

            // Assert: ThrowIfNullOrEmpty raises an ArgumentException-family exception for null.
            act.Should()
                .Throw<ArgumentException>(
                    "GetNextToDoID guards against a null seed via ThrowIfNullOrEmpty"
                );
        }

        [TestMethod]
        public void GetNextToDoID_EmptySeed_ThrowsArgumentException()
        {
            // Arrange
            var list = new IDList(new List<string>());

            // Act
            Action act = () => list.GetNextToDoID(string.Empty);

            // Assert
            act.Should()
                .Throw<ArgumentException>(
                    "GetNextToDoID guards against an empty seed via ThrowIfNullOrEmpty"
                );
        }

        private static IDList CreateListWithCachedMaxLength(params string[] ids)
        {
            var list = new IDList(new List<string>(ids));
            _ = list.MaxLengthOfID;

            return list;
        }
    }
}
