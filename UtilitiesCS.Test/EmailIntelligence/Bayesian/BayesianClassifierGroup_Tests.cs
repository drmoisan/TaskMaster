using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.HelperClasses;

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
            var sw = new SegmentStopWatch();
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
            var sw = new SegmentStopWatch();
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

        [TestMethod]
        public void Train_AppendToExistingClassifier_IncrementMatchEmailCount()
        {
            // Arrange: create the group and train the first batch under "tag1".
            var group = new BayesianClassifierGroup();
            group.Train("tag1", new[] { "word1" }, 1);

            // Act: train a second batch under the same tag — the existing classifier must be
            // reused via GetOrAdd rather than replaced, so email counts accumulate.
            group.Train("tag1", new[] { "word2" }, 2);

            // Assert: only one classifier exists for "tag1" and its count reflects both trains.
            group.Classifiers.Should().ContainKey("tag1");
            group.Classifiers["tag1"].MatchEmailCount.Should().Be(3);
        }

        [TestMethod]
        public void Classify_WithDistinctTokenSets_ReturnsPredictionsInDescendingProbabilityOrder()
        {
            // Arrange: train two classifiers with non-overlapping tokens so that querying
            // "spam-word" produces measurably higher probability for "spam-tag" than "ham-tag".
            var group = new BayesianClassifierGroup();
            group.Train("spam-tag", new[] { "spam-word", "spam-word", "spam-word" }, 5);
            group.Train("ham-tag", new[] { "ham-word" }, 2);
            group.TotalEmailCount = 7;

            // Act: classify with the spam token.
            var results = group.Classify(new string[] { "spam-word" }).ToList();

            // Assert: at least two predictions exist and they are ordered from highest to lowest.
            results.Should().HaveCountGreaterThanOrEqualTo(2);
            results.Should().BeInDescendingOrder(p => p.Probability);
        }

        [TestMethod]
        public void TrainMultiTag_UpdatesBothSharedTokenBaseAndDedicatedClassifiers()
        {
            // Arrange: start with an empty group.
            var group = new BayesianClassifierGroup();

            // Act: train across two tags simultaneously — this must update the shared token
            // base as well as each per-tag classifier.
            group.TrainMultiTag(new[] { "tag-a", "tag-b" }, new[] { "shared-word" }, 1);

            // Assert: shared base received the token, and both dedicated classifiers were created.
            group.SharedTokenBase.TokenFrequency.Should().ContainKey("shared-word");
            group.Classifiers.Should().ContainKey("tag-a");
            group.Classifiers.Should().ContainKey("tag-b");
        }
    }
}
