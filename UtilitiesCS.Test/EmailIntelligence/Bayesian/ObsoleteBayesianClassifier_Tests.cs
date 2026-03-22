using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.EmailIntelligence.Bayesian;

#pragma warning disable CS0618 // Obsolete type under test

namespace UtilitiesCS.Test.EmailIntelligence.Bayesian
{
    [TestClass]
    public class ObsoleteBayesianClassifier_Tests
    {
        [TestMethod]
        public void DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var classifier = new BayesianClassifier();

            // Assert
            classifier.Should().NotBeNull();
        }

        [TestMethod]
        public void Constructor_WithTag_InitializesProperties()
        {
            // Arrange & Act
            var classifier = new BayesianClassifier("test-tag");

            // Assert
            classifier.Tag.Should().Be("test-tag");
            classifier.Prob.Should().NotBeNull();
            classifier.NotMatch.Should().NotBeNull();
        }

        [TestMethod]
        public void Tag_GetSet_RoundTrips()
        {
            // Arrange
            var classifier = new BayesianClassifier();

            // Act
            classifier.Tag = "new-tag";

            // Assert
            classifier.Tag.Should().Be("new-tag");
        }

        [TestMethod]
        public void Load_WithTokens_InitializesMatchAndNotMatch()
        {
            // Arrange
            var classifier = new BayesianClassifier("tag");
            var positive = new[] { "good1", "good2", "good1" };
            var negative = new[] { "bad1", "bad2" };

            // Act
            classifier.Load(positive, negative);

            // Assert
            classifier.Match.Should().NotBeNull();
            classifier.NotMatch.Should().NotBeNull();
        }

        [TestMethod]
        public void AddMatch_AddsTokensToMatchCorpus()
        {
            // Arrange
            var parent = new ClassifierGroup();
            var classifier = new BayesianClassifier("tag") { Parent = parent };

            // Act
            classifier.AddMatch(new[] { "word1", "word2" });

            // Assert
            classifier.Match.TokenFrequency.Should().ContainKey("word1");
        }

        [TestMethod]
        public void AddNotMatch_AddsTokensToNotMatchCorpus()
        {
            // Arrange
            var parent = new ClassifierGroup();
            var classifier = new BayesianClassifier("tag") { Parent = parent };

            // Act
            classifier.AddNotMatch(new[] { "word1", "word2" });

            // Assert
            classifier.NotMatch.TokenFrequency.Should().ContainKey("word1");
        }

        [TestMethod]
        public void Loaded_DefaultIsFalse()
        {
            // Arrange & Act
            var classifier = new BayesianClassifier();

            // Assert
            classifier.Loaded.Should().BeFalse();
        }

        [TestMethod]
        public void Constructor_WithPositiveAndNegativeTokens_InitializesCorrectly()
        {
            // Arrange & Act
            var classifier = new BayesianClassifier(
                "tag",
                new[] { "pos1", "pos2", "pos1" },
                new[] { "neg1", "neg2" }
            );

            // Assert
            classifier.Tag.Should().Be("tag");
            classifier.Prob.Should().NotBeNull();
        }

        [TestMethod]
        public void FromTokenBase_WithValidInputs_ReturnsClassifier()
        {
            // Arrange
            var parent = new ClassifierGroup();
            parent.SharedTokenBase.AddOrIncrementTokens(
                new[] { "hello", "world", "hello", "test" }
            );

            // Act
            var classifier = BayesianClassifier.FromTokenBase(
                parent,
                "tag",
                new[] { "hello", "hello" }
            );

            // Assert
            classifier.Tag.Should().Be("tag");
            classifier.Parent.Should().BeSameAs(parent);
        }
    }

    [TestClass]
    public class ObsoleteClassifierGroup_Tests
    {
        [TestMethod]
        public void DefaultConstructor_InitializesEmptyClassifiers()
        {
            // Arrange & Act
            var group = new ClassifierGroup();

            // Assert
            group.Classifiers.Should().NotBeNull();
            group.Classifiers.Should().BeEmpty();
        }

        [TestMethod]
        public void SharedTokenBase_DefaultIsEmpty()
        {
            // Arrange & Act
            var group = new ClassifierGroup();

            // Assert
            group.SharedTokenBase.Should().NotBeNull();
        }

        [TestMethod]
        public void DedicatedTokens_DefaultIsEmpty()
        {
            // Arrange & Act
            var group = new ClassifierGroup();

            // Assert
            group.DedicatedTokens.Should().NotBeNull();
            group.DedicatedTokens.Should().BeEmpty();
        }

        [TestMethod]
        public void ForceClassifierUpdate_CreatesNewClassifier()
        {
            // Arrange
            var group = new ClassifierGroup();

            // Act
            group.ForceClassifierUpdate("tag1", new[] { "pos1", "pos2", "pos1" }, new[] { "neg1" });

            // Assert
            group.Classifiers.Should().ContainKey("tag1");
            group.Classifiers["tag1"].Parent.Should().BeSameAs(group);
        }

        [TestMethod]
        public void AddOrUpdateClassifier_CreatesNewIfNotExists()
        {
            // Arrange
            var group = new ClassifierGroup();

            // Act
            group.AddOrUpdateClassifier("tag1", new[] { "pos1", "pos2", "pos1" }, new[] { "neg1" });

            // Assert
            group.Classifiers.Should().ContainKey("tag1");
        }

        [TestMethod]
        public void Classify_WithTokens_ReturnsResults()
        {
            // Arrange
            var group = new ClassifierGroup();
            group.ForceClassifierUpdate(
                "tag1",
                new[] { "hello", "world", "hello" },
                new[] { "bye", "world" }
            );

            // Act
            var results = group.Classify(new[] { "hello" });

            // Assert
            results.Should().NotBeNull();
        }

        [TestMethod]
        public void TotalTokenCount_GetSet_RoundTrips()
        {
            // Arrange
            var group = new ClassifierGroup();

            // Act
            group.TotalTokenCount = 100;

            // Assert
            group.TotalTokenCount.Should().Be(100);
        }

        [TestMethod]
        public void GetReportMessage_WithCompletedItems_FormatsCorrectly()
        {
            // Arrange
            var group = new ClassifierGroup();
            var sw = new UtilitiesCS.HelperClasses.SegmentStopWatch();
            sw.Start();
            System.Threading.Thread.Sleep(10);

            // Act
            var message = group.GetReportMessage(1, 10, sw);

            // Assert
            message.Should().Contain("Completed 1 of 10");
        }

        [TestMethod]
        public void GetReportMessage_ZeroCompleted_FormatsCorrectly()
        {
            // Arrange
            var group = new ClassifierGroup();
            var sw = new UtilitiesCS.HelperClasses.SegmentStopWatch();
            sw.Start();

            // Act
            var message = group.GetReportMessage(0, 10, sw);

            // Assert
            message.Should().Be("Completed 0 of 10");
        }
    }
}

#pragma warning restore CS0618
