using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.EmailIntelligence.Bayesian;

namespace UtilitiesCS.Test.EmailIntelligence.Bayesian
{
    /// <summary>
    /// Conformance tests for the <see cref="IFolderPredictor"/> seam: a
    /// <see cref="BayesianClassifierGroup"/> must be assignable to the interface and
    /// calls dispatched through the interface must reach the flat implementation.
    /// </summary>
    [TestClass]
    public class IFolderPredictor_Tests
    {
        private static readonly string[] SampleTokens = ["alpha", "beta", "beta", "gamma"];

        [TestMethod]
        public void BayesianClassifierGroup_IsAssignableToIFolderPredictor()
        {
            // Arrange & Act
            var group = new BayesianClassifierGroup();

            // Assert
            group.Should().BeAssignableTo<IFolderPredictor>();
        }

        [TestMethod]
        public void Train_ThroughInterface_DispatchesToFlatImplementation()
        {
            // Arrange
            var group = new BayesianClassifierGroup();
            IFolderPredictor predictor = group;

            // Act
            predictor.Train("Inbox", SampleTokens, 1);

            // Assert
            group
                .Classifiers.Should()
                .ContainKey("Inbox", "interface Train must reach the flat classifier dictionary");
            group.Classifiers["Inbox"].MatchEmailCount.Should().Be(1);
        }

        [TestMethod]
        public void UnTrain_ThroughInterface_DispatchesToFlatImplementation()
        {
            // Arrange
            var group = new BayesianClassifierGroup();
            IFolderPredictor predictor = group;
            predictor.Train("Inbox", SampleTokens, 1);

            // Act
            predictor.UnTrain("Inbox", SampleTokens, 1);

            // Assert: flat UnTrain removes the tag once its email count reaches zero
            group
                .Classifiers.Should()
                .NotContainKey(
                    "Inbox",
                    "interface UnTrain must decrement counts and remove the empty classifier"
                );
        }

        [TestMethod]
        public void Classify_ThroughInterface_ReturnsSameResultsAsFlatCall()
        {
            // Arrange
            var group = new BayesianClassifierGroup { TotalEmailCount = 2 };
            group.Train("Inbox", SampleTokens, 1);
            group.Train("Archive", new[] { "delta", "epsilon" }, 1);
            IFolderPredictor predictor = group;

            // Act
            var interfaceResults = predictor.Classify(SampleTokens).ToArray();
            var flatResults = group.Classify(SampleTokens).ToArray();

            // Assert: identical ordering and probabilities through both call paths
            interfaceResults.Select(p => p.Class).Should().Equal(flatResults.Select(p => p.Class));
            interfaceResults
                .Select(p => p.Probability)
                .Should()
                .Equal(flatResults.Select(p => p.Probability));
        }

        [TestMethod]
        public void Serialize_ThroughInterface_IsCallableAndDoesNotThrow()
        {
            // Arrange: default config has an empty FilePath, so Serialize() is a safe no-op
            IFolderPredictor predictor = new BayesianClassifierGroup();

            // Act
            var act = () => predictor.Serialize();

            // Assert
            act.Should().NotThrow("Serialize() is inherited from SmartSerializable<T>");
        }
    }
}
