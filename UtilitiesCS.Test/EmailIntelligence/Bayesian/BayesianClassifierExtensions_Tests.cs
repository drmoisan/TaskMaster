using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.EmailIntelligence.Bayesian;

namespace UtilitiesCS.Test.EmailIntelligence.Bayesian
{
    [TestClass]
    public class BayesianClassifierExtensions_Tests
    {
        [TestMethod]
        public void GroupAndCount_DistinctTokens_ReturnsOneEach()
        {
            // Arrange
            var tokens = new[] { "a", "b", "c" };

            // Act
            var result = tokens.GroupAndCount();

            // Assert
            result.Should().HaveCount(3);
            result["a"].Should().Be(1);
            result["b"].Should().Be(1);
            result["c"].Should().Be(1);
        }

        [TestMethod]
        public void GroupAndCount_DuplicateTokens_SumsCorrectly()
        {
            // Arrange
            var tokens = new[] { "a", "b", "a", "c", "b", "a" };

            // Act
            var result = tokens.GroupAndCount();

            // Assert
            result["a"].Should().Be(3);
            result["b"].Should().Be(2);
            result["c"].Should().Be(1);
        }

        [TestMethod]
        public void GroupAndCount_EmptyCollection_ReturnsEmpty()
        {
            // Arrange
            var tokens = Array.Empty<string>();

            // Act
            var result = tokens.GroupAndCount();

            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GroupAndCountAsync_ReturnsCorrectCounts()
        {
            // Arrange
            var tokens = new[] { "x", "y", "x" };

            // Act
            var result = await tokens.GroupAndCountAsync();

            // Assert
            result["x"].Should().Be(2);
            result["y"].Should().Be(1);
        }

        [TestMethod]
        public async Task ToClassifierAsync_WithTokenEnumerable_ReturnsClassifier()
        {
            // Arrange
            var group = new BayesianClassifierGroup();
            group.Train("tag1", new[] { "a", "b", "c" }, 1);

            // Act
            var result = await group.ToClassifierAsync(
                "tag1",
                new[] { "a", "b" },
                1,
                true,
                System.Threading.CancellationToken.None
            );

            // Assert
            result.Should().NotBeNull();
            result.Tag.Should().Be("tag1");
        }

        [TestMethod]
        public async Task ToClassifierAsync_WithDictionary_ReturnsClassifier()
        {
            // Arrange
            var group = new BayesianClassifierGroup();
            group.Train("tag2", new[] { "x", "y" }, 1);
            var matchFreq = new Dictionary<string, int> { ["x"] = 1, ["y"] = 1 };

            // Act
            var result = await group.ToClassifierAsync(
                "tag2",
                matchFreq,
                1,
                true,
                System.Threading.CancellationToken.None
            );

            // Assert
            result.Should().NotBeNull();
            result.Tag.Should().Be("tag2");
        }
    }
}
