using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.EmailIntelligence.Bayesian;

namespace UtilitiesCS.Test.EmailIntelligence.Bayesian
{
    [TestClass]
    public class BayesianClassifierGroup_Tests
    {
        [TestMethod]
        public void DefaultConstructor_InitializesEmptyClassifiers()
        {
            // Arrange & Act
            var group = new BayesianClassifierGroup();

            // Assert
            group.Classifiers.Should().NotBeNull();
            group.Classifiers.Should().BeEmpty();
        }

        [TestMethod]
        public void SharedTokenBase_DefaultIsEmpty()
        {
            // Arrange & Act
            var group = new BayesianClassifierGroup();

            // Assert
            group.SharedTokenBase.Should().NotBeNull();
            group.SharedTokenBase.TokenFrequency.Should().BeEmpty();
        }

        [TestMethod]
        public void TotalEmailCount_GetSet_RoundTrips()
        {
            // Arrange
            var group = new BayesianClassifierGroup();

            // Act
            group.TotalEmailCount = 100;

            // Assert
            group.TotalEmailCount.Should().Be(100);
        }

        [TestMethod]
        public void MinimumProbability_DefaultIsZero()
        {
            // Arrange & Act
            var group = new BayesianClassifierGroup();

            // Assert
            group.MinimumProbability.Should().Be(0.0);
        }

        [TestMethod]
        public void MinimumProbability_GetSet_RoundTrips()
        {
            // Arrange
            var group = new BayesianClassifierGroup();

            // Act
            group.MinimumProbability = 0.5;

            // Assert
            group.MinimumProbability.Should().Be(0.5);
        }

        [TestMethod]
        public void AddToEmailCount_IncrementsCount()
        {
            // Arrange
            var group = new BayesianClassifierGroup();
            group.TotalEmailCount = 10;

            // Act
            group.AddToEmailCount(5);

            // Assert
            group.TotalEmailCount.Should().Be(15);
        }

        [TestMethod]
        public void AddToEmailCount_WithNegative_DecrementsCount()
        {
            // Arrange
            var group = new BayesianClassifierGroup();
            group.TotalEmailCount = 10;

            // Act
            group.AddToEmailCount(-3);

            // Assert
            group.TotalEmailCount.Should().Be(7);
        }

        [TestMethod]
        public void Train_CreatesNewClassifier()
        {
            // Arrange
            var group = new BayesianClassifierGroup();
            var tokens = new[] { "word1", "word2", "word1" };

            // Act
            group.Train("tag1", tokens, 1);

            // Assert
            group.Classifiers.Should().ContainKey("tag1");
            group.Classifiers["tag1"].MatchEmailCount.Should().Be(1);
        }

        [TestMethod]
        public void UnTrain_RemovesClassifierWhenCountReachesZero()
        {
            // Arrange
            var group = new BayesianClassifierGroup();
            var tokens = new[] { "word1" };
            group.Train("tag1", tokens, 1);

            // Act
            group.UnTrain("tag1", tokens, 1);

            // Assert
            group.Classifiers.Should().NotContainKey("tag1");
        }

        [TestMethod]
        public void UnTrain_NonexistentTag_DoesNotThrow()
        {
            // Arrange
            var group = new BayesianClassifierGroup();

            // Act
            System.Action act = () => group.UnTrain("nonexistent", new[] { "word" }, 1);

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void TrainMultiTag_TrainsMultipleClassifiers()
        {
            // Arrange
            var group = new BayesianClassifierGroup();
            var tags = new[] { "tag1", "tag2" };
            var tokens = new[] { "word1", "word2" };

            // Act
            group.TrainMultiTag(tags, tokens, 1);

            // Assert
            group.Classifiers.Should().ContainKey("tag1");
            group.Classifiers.Should().ContainKey("tag2");
        }

        [TestMethod]
        public void Classify_WithTokenArray_ReturnsResults()
        {
            // Arrange
            var group = new BayesianClassifierGroup();
            group.Train("tag1", new[] { "hello", "world" }, 1);
            group.TotalEmailCount = 1;

            // Act
            var results = group.Classify(new[] { "hello" });

            // Assert
            results.Should().NotBeNull();
        }

        [TestMethod]
        public void Classify_WithDictionary_ReturnsResults()
        {
            // Arrange
            var group = new BayesianClassifierGroup();
            group.Train("tag1", new[] { "hello", "world" }, 5);
            group.TotalEmailCount = 10;
            var tokenIncidence = new Dictionary<string, int> { { "hello", 1 } };

            // Act
            var results = group.Classify(tokenIncidence);

            // Assert
            results.Should().NotBeNull();
        }

        [TestMethod]
        public void GetReportMessage_WithCompletedItems_FormatsCorrectly()
        {
            // Arrange
            var group = new BayesianClassifierGroup();
            var sw = new HelperClasses.SegmentStopWatch();
            sw.Start();
            System.Threading.Thread.Sleep(10);

            // Act
            var message = group.GetReportMessage(1, 10, sw);

            // Assert
            message.Should().Contain("Completed 1 of 10");
        }

        [TestMethod]
        public void GetReportMessage_WithZeroCompleted_FormatsCorrectly()
        {
            // Arrange
            var group = new BayesianClassifierGroup();
            var sw = new HelperClasses.SegmentStopWatch();
            sw.Start();

            // Act
            var message = group.GetReportMessage(0, 10, sw);

            // Assert
            message.Should().Be("Completed 0 of 10");
        }

        [TestMethod]
        public void Globals_GetSet_RoundTrips()
        {
            // Arrange
            var group = new BayesianClassifierGroup();
            var mockGlobals = new Moq.Mock<IApplicationGlobals>().Object;

            // Act
            group.Globals = mockGlobals;

            // Assert
            group.Globals.Should().BeSameAs(mockGlobals);
        }
    }
}
