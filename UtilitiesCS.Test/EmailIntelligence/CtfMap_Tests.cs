using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class CtfMap_Tests
    {
        [TestMethod]
        public void Constructor_WithNoArguments_StartsEmpty()
        {
            // Arrange

            // Act
            var map = new CtfMap();

            // Assert
            map.Should().BeEmpty();
        }

        [TestMethod]
        public void Constructor_WithEnumerable_SeedsEntriesInOrder()
        {
            // Arrange
            var seed = new[]
            {
                new CtfMapEntry("Inbox", "conv-1", 1),
                new CtfMapEntry("Archive", "conv-2", 2),
            };

            // Act
            var map = new CtfMap(seed);

            // Assert
            map.Should().BeEquivalentTo(seed, options => options.WithStrictOrdering());
        }

        [TestMethod]
        public void TopEntriesById_WhenMapIsEmpty_ReturnsEmptyArray()
        {
            // Arrange
            var map = new CtfMap();

            // Act
            var entries = map.TopEntriesById(id: "missing", topN: 3);

            // Assert
            entries.Should().BeEmpty();
        }

        [TestMethod]
        public void TopEntriesById_WithSingleMatch_ReturnsOnlyMatchingEntry()
        {
            // Arrange
            var map = new CtfMap
            {
                new CtfMapEntry("Inbox", "conv-1", 3),
                new CtfMapEntry("Archive", "conv-2", 7),
            };

            // Act
            var entries = map.TopEntriesById(id: "conv-2", topN: 5);

            // Assert
            entries.Should().ContainSingle();
            entries[0].Should().BeEquivalentTo(new CtfMapEntry("Archive", "conv-2", 7));
        }

        [TestMethod]
        public void ContainsId_WhenConversationExists_ReturnsTrue()
        {
            // Arrange
            var map = new CtfMap { new CtfMapEntry("Inbox", "conv-1", 3) };

            // Act
            var contains = map.ContainsId("conv-1");

            // Assert
            contains.Should().BeTrue();
        }

        [TestMethod]
        public void ContainsId_WhenConversationIsMissing_ReturnsFalse()
        {
            // Arrange
            var map = new CtfMap();

            // Act
            var contains = map.ContainsId("missing");

            // Assert
            contains.Should().BeFalse();
        }

        [TestMethod]
        public void FindId_WhenConversationExists_ReturnsIndex()
        {
            // Arrange
            var map = new CtfMap
            {
                new CtfMapEntry("Inbox", "conv-1", 1),
                new CtfMapEntry("Archive", "conv-2", 2),
            };

            // Act
            var index = map.FindId("conv-2");

            // Assert
            index.Should().Be(1);
        }

        [TestMethod]
        public void FindId_WhenConversationIsMissing_ReturnsMinusOne()
        {
            // Arrange
            var map = new CtfMap();

            // Act
            var index = map.FindId("missing");

            // Assert
            index.Should().Be(-1);
        }

        [TestMethod]
        public void Add_WhenEntryAlreadyExists_AccumulatesEmailCount()
        {
            // Arrange
            var map = new CtfMap { new CtfMapEntry("Inbox", "conv-1", 2) };

            // Act
            map.Add(emailFolder: "Inbox", conversationID: "conv-1", emailCount: 5);

            // Assert
            map.Should().ContainSingle();
            map[0].EmailCount.Should().Be(7);
        }

        [TestMethod]
        public void Add_WhenEntryDoesNotExist_CreatesNewEntry()
        {
            // Arrange
            var map = new CtfMap { new CtfMapEntry("Inbox", "conv-1", 2) };

            // Act
            map.Add(emailFolder: "Archive", conversationID: "conv-2", emailCount: 3);

            // Assert
            map.Should().HaveCount(2);
            map[1].EmailFolder.Should().Be("Archive");
            map[1].ConversationID.Should().Be("conv-2");
            map[1].EmailCount.Should().Be(3);
        }

        [TestMethod]
        public void TopEntriesById_WithMultipleMatches_ReturnsTopNByEmailCountDescending()
        {
            // Arrange
            var map = new CtfMap
            {
                new CtfMapEntry("Inbox", "conv-1", 3),
                new CtfMapEntry("Archive", "conv-1", 7),
                new CtfMapEntry("Projects", "conv-1", 5),
                new CtfMapEntry("Other", "conv-2", 10),
            };

            // Act
            var entries = map.TopEntriesById(id: "conv-1", topN: 2);

            // Assert
            entries.Should().HaveCount(2);
            entries[0].EmailCount.Should().Be(7);
            entries[1].EmailCount.Should().Be(5);
        }

        [TestMethod]
        public void ProcessQueue_WithValidEntries_ReturnsAllEntries()
        {
            // Arrange
            var lines = new Queue<string>(
                new[] { "Inbox", "AAAABBBBCCCCDDDDEEEEFFFFGGGGHHHH", "5" }
            );

            // Act
            var result = CtfMap.ProcessQueue(lines);

            // Assert
            result.Should().ContainSingle();
            result[0].EmailFolder.Should().Be("Inbox");
            result[0].ConversationID.Should().Be("AAAABBBBCCCCDDDDEEEEFFFFGGGGHHHH");
            result[0].EmailCount.Should().Be(5);
        }

        [TestMethod]
        public void TryDequeueEntry_WithValidEntry_ReturnsEntry()
        {
            // Arrange
            var lines = new Queue<string>(new[] { "Inbox", "conv-1", "7" });

            // Act
            var result = CtfMap.TryDequeueEntry(ref lines);

            // Assert
            result.Should().NotBeNull();
            result.EmailFolder.Should().Be("Inbox");
            result.EmailCount.Should().Be(7);
        }

        [TestMethod]
        public void TryDequeueEntry_WithInvalidIntegerFormat_ReturnsNull()
        {
            // Arrange
            var lines = new Queue<string>(new[] { "Inbox", "conv-1", "not-a-number" });

            // Act
            var result = CtfMap.TryDequeueEntry(ref lines);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void IsEntryID_WithValid32CharString_ReturnsTrue()
        {
            // Act / Assert
            CtfMap.IsEntryID("AAAABBBBCCCCDDDDEEEEFFFFGGGGHHHH").Should().BeTrue();
        }

        [TestMethod]
        public void IsEntryID_WithSpacesOrBackslash_ReturnsFalse()
        {
            // Act / Assert
            CtfMap.IsEntryID("AAAABBBB CCCCDDDDEEEEFFFFGGGGHHHH").Should().BeFalse();
            CtfMap.IsEntryID("AAAABBBB\\CCCDDDDEEEEFFFFGGGGHHHH").Should().BeFalse();
        }

        [TestMethod]
        public void IsEntryID_WhenLengthIsNot32_ReturnsFalse()
        {
            // Act / Assert
            CtfMap.IsEntryID("short").Should().BeFalse();
        }

        [TestMethod]
        public void DequeueToNextRecord_SkipsLinesUntilEntryIdFound()
        {
            // Arrange – two garbage lines precede the entry-ID so the method
            // actually dequeues one of them (it looks ahead at ElementAt(1)).
            var lines = new Queue<string>(
                new[]
                {
                    "garbage-1",
                    "garbage-2",
                    "AAAABBBBCCCCDDDDEEEEFFFFGGGGHHHH",
                    "next-record",
                }
            );

            // Act
            CtfMap.DequeueToNextRecord(ref lines);

            // Assert – the method stops when ElementAt(1) is an entry-ID,
            // leaving the element just before the entry-ID at the front.
            lines.Count.Should().Be(3);
            lines.Peek().Should().Be("garbage-2");
        }

        [TestMethod]
        public void TryDequeueEntry_WithOverflowInteger_ReturnsNull()
        {
            // Arrange
            var lines = new Queue<string>(
                new[]
                {
                    "Inbox",
                    "AAAABBBBCCCCDDDDEEEEFFFFGGGGHHHH",
                    ((long)int.MaxValue + 1).ToString(),
                }
            );

            // Act
            var result = CtfMap.TryDequeueEntry(ref lines);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void PrivateHelpers_ArrayToQueueAndReadFileToArray_CoverRemainingFileBranches()
        {
            // Arrange
            var arrayToQueue = typeof(CtfMap).GetMethod(
                "ArrayToQueue",
                BindingFlags.Static | BindingFlags.NonPublic
            );
            var readFileToArray = typeof(CtfMap).GetMethod(
                "ReadFileToArray",
                BindingFlags.Static | BindingFlags.NonPublic
            );

            // Act
            var queue =
                (Queue<string>)
                    arrayToQueue.Invoke(
                        null,
                        new object[] { new[] { "header", "Inbox", "conv", "1" } }
                    );
            Action act = () =>
                readFileToArray.Invoke(null, new object[] { GetMissingFilePath("ctf-map") });

            // Assert
            queue.Should().ContainInOrder("Inbox", "conv", "1");
            act.Should()
                .Throw<TargetInvocationException>()
                .WithInnerException<FileNotFoundException>();
        }

        [TestMethod]
        public void ReadTextFile_WhenFileIsMissing_ThrowsFileNotFoundException()
        {
            // Arrange

            // Act
            Action act = () => CtfMap.ReadTextFile(GetMissingFilePath("ctf-map-read"));

            // Assert
            act.Should().Throw<FileNotFoundException>();
        }

        private static string GetMissingFilePath(string name)
        {
            return Path.Combine(Environment.CurrentDirectory, $"{name}-{Guid.NewGuid():N}.txt");
        }
    }
}
