using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.EmailIntelligence.Bayesian;

namespace UtilitiesCS.Test.EmailIntelligence.Bayesian
{
    [TestClass]
    public class BayesianClassifierShared_Tests
    {
        [TestMethod]
        public void DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var classifier = new BayesianClassifierShared();

            // Assert
            classifier.Should().NotBeNull();
        }

        [TestMethod]
        public void Constructor_WithTag_InitializesProperties()
        {
            // Arrange & Act
            var classifier = new BayesianClassifierShared("test-tag");

            // Assert
            classifier.Tag.Should().Be("test-tag");
            classifier.Match.Should().NotBeNull();
            classifier.Prob.Should().NotBeNull();
        }

        [TestMethod]
        public void Constructor_WithTagAndParent_SetsParent()
        {
            // Arrange
            var parent = new BayesianClassifierGroup();

            // Act
            var classifier = new BayesianClassifierShared("tag", parent);

            // Assert
            classifier.Tag.Should().Be("tag");
            classifier.Parent.Should().BeSameAs(parent);
        }

        [TestMethod]
        public void Tag_GetSet_RoundTrips()
        {
            // Arrange
            var classifier = new BayesianClassifierShared();

            // Act
            classifier.Tag = "new-tag";

            // Assert
            classifier.Tag.Should().Be("new-tag");
        }

        [TestMethod]
        public void MatchEmailCount_GetSet_RoundTrips()
        {
            // Arrange
            var classifier = new BayesianClassifierShared("tag");

            // Act
            classifier.MatchEmailCount = 42;

            // Assert
            classifier.MatchEmailCount.Should().Be(42);
        }

        [TestMethod]
        public void FromTokenBase_WithNullParent_ThrowsArgumentNullException()
        {
            // Arrange
            var matches = new Dictionary<string, int> { { "hello", 1 } };

            // Act
            System.Action act = () =>
                BayesianClassifierShared.FromTokenBase(null, "tag", matches, 1, false);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void FromTokenBase_WithNullTag_ThrowsArgumentNullException()
        {
            // Arrange
            var parent = new BayesianClassifierGroup();
            var matches = new Dictionary<string, int> { { "hello", 1 } };

            // Act
            System.Action act = () =>
                BayesianClassifierShared.FromTokenBase(parent, null, matches, 1, false);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void FromTokenBase_WithNullMatches_ThrowsArgumentNullException()
        {
            // Arrange
            var parent = new BayesianClassifierGroup();

            // Act
            System.Action act = () =>
                BayesianClassifierShared.FromTokenBase(parent, "tag", null, 1, false);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void FromTokenBase_WithZeroEmailCount_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var parent = new BayesianClassifierGroup();
            var matches = new Dictionary<string, int> { { "hello", 1 } };

            // Act
            System.Action act = () =>
                BayesianClassifierShared.FromTokenBase(parent, "tag", matches, 0, false);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void FromTokenBase_WithValidParams_CreatesClassifier()
        {
            // Arrange
            var parent = new BayesianClassifierGroup();
            parent.SharedTokenBase.TokenFrequency["hello"] = 5;
            var matches = new Dictionary<string, int> { { "hello", 3 } };

            // Act
            var classifier = BayesianClassifierShared.FromTokenBase(
                parent,
                "tag",
                matches,
                1,
                false
            );

            // Assert
            classifier.Tag.Should().Be("tag");
            classifier.MatchEmailCount.Should().Be(1);
            classifier.Parent.Should().BeSameAs(parent);
        }

        [TestMethod]
        public void FromTokenBase_AddToParent_UpdatesSharedTokenBase()
        {
            // Arrange
            var parent = new BayesianClassifierGroup();
            var matches = new Dictionary<string, int> { { "hello", 3 } };

            // Act
            var classifier = BayesianClassifierShared.FromTokenBase(
                parent,
                "tag",
                matches,
                1,
                true
            );

            // Assert
            parent.SharedTokenBase.TokenFrequency.Should().ContainKey("hello");
        }

        [TestMethod]
        public void Train_AddTokensToClassifier_UpdatesMatchCount()
        {
            // Arrange
            var parent = new BayesianClassifierGroup();
            var classifier = new BayesianClassifierShared("tag", parent);
            classifier.MatchEmailCount = 1;
            var tokens = new Dictionary<string, int> { { "word", 2 } };

            // Act
            classifier.Train(tokens, 1);

            // Assert
            classifier.MatchEmailCount.Should().Be(2);
            classifier.Match.TokenFrequency.Should().ContainKey("word");
        }
    }
}
