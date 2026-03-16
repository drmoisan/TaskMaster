using System.Collections.Generic;
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
                new CtfMapEntry("Archive", "conv-2", 2)
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
                new CtfMapEntry("Archive", "conv-2", 7)
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
            var map = new CtfMap
            {
                new CtfMapEntry("Inbox", "conv-1", 3)
            };

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
                new CtfMapEntry("Archive", "conv-2", 2)
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
            var map = new CtfMap
            {
                new CtfMapEntry("Inbox", "conv-1", 2)
            };

            // Act
            map.Add(emailFolder: "Inbox", conversationID: "conv-1", emailCount: 5);

            // Assert
            map.Should().ContainSingle();
            map[0].EmailCount.Should().Be(7);
        }
    }
}