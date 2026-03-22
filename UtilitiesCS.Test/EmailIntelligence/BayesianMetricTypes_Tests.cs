using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using UtilitiesCS.EmailIntelligence.Bayesian.Performance;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class BayesianMetricTypes_Tests
    {
        [TestMethod]
        public void TestOutcome_PropertiesRoundTrip()
        {
            // Arrange / Act
            var outcome = new TestOutcome
            {
                Actual = "Inbox",
                Predicted = "Archive",
                SourceIndex = 5,
            };

            // Assert
            outcome.Actual.Should().Be("Inbox");
            outcome.Predicted.Should().Be("Archive");
            outcome.SourceIndex.Should().Be(5);
        }

        [TestMethod]
        public void VerboseTestOutcome_PropertiesRoundTrip()
        {
            // Arrange / Act
            var outcome = new VerboseTestOutcome
            {
                Actual = "A",
                Predicted = "B",
                SourceIndex = 1,
                Probability = 0.85,
                Drivers = new[] { ("token1", 0.5) },
            };

            // Assert
            outcome.Actual.Should().Be("A");
            outcome.Predicted.Should().Be("B");
            outcome.Probability.Should().Be(0.85);
            outcome.Drivers.Should().HaveCount(1);
        }

        [TestMethod]
        public void ClassCounts_PropertiesRoundTrip()
        {
            // Act
            var counts = new ClassCounts
            {
                Class = "Inbox",
                TP = 10,
                FP = 2,
                FN = 3,
                TN = 50,
            };

            // Assert
            counts.TP.Should().Be(10);
            counts.FP.Should().Be(2);
            counts.FN.Should().Be(3);
            counts.TN.Should().Be(50);
        }

        [TestMethod]
        public void VerboseClassCounts_VerboseOutcomes_JsonSerialization()
        {
            // Arrange
            var outcomes = new Dictionary<VerboseTestOutcome, string>
            {
                {
                    new VerboseTestOutcome { Actual = "A", Predicted = "B" },
                    "FP"
                },
            };
            var counts = new VerboseClassCounts
            {
                Class = "Test",
                TP = 1,
                FP = 1,
                VerboseOutcomes = outcomes,
            };

            // Act
            var json = JsonConvert.SerializeObject(counts);
            var deserialized = JsonConvert.DeserializeObject<VerboseClassCounts>(json);

            // Assert
            deserialized.Class.Should().Be("Test");
            deserialized.TP.Should().Be(1);
            // VerboseOutcomes is [JsonIgnore] but the private VerboseOutcomesJson
            // proxy ([JsonProperty]) round-trips the data, so it IS populated.
            deserialized.VerboseOutcomes.Should().NotBeNull();
            deserialized.VerboseOutcomes.Should().HaveCount(1);
        }

        [TestMethod]
        public void TestScores_PropertiesRoundTrip()
        {
            // Act
            var scores = new TestScores
            {
                Class = "Inbox",
                TP = 8,
                FP = 1,
                FN = 2,
                TN = 40,
                Precision = 0.89,
                Recall = 0.80,
                F1 = 0.84,
            };

            // Assert
            scores.Precision.Should().Be(0.89);
            scores.Recall.Should().Be(0.80);
            scores.F1.Should().Be(0.84);
        }

        [TestMethod]
        public void VerboseTestScores_VerboseOutcomes_JsonIgnored()
        {
            // Arrange
            var scores = new VerboseTestScores
            {
                Class = "Test",
                VerboseOutcomes = new Dictionary<VerboseTestOutcome, string>(),
            };

            // Act
            var json = JsonConvert.SerializeObject(scores);
            var deserialized = JsonConvert.DeserializeObject<VerboseTestScores>(json);

            // Assert – private VerboseOutcomesJson proxy round-trips the data.
            deserialized.VerboseOutcomes.Should().NotBeNull();
            deserialized.VerboseOutcomes.Should().BeEmpty();
        }

        [TestMethod]
        public void GroupedTestOutcome_PropertiesRoundTrip()
        {
            // Act
            var grouped = new GroupedTestOutcome
            {
                Actual = "A",
                Predicted = "B",
                Count = 5,
            };

            // Assert
            grouped.Count.Should().Be(5);
        }

        [TestMethod]
        public void VerboseGroupedTestOutcome_PropertiesRoundTrip()
        {
            // Act
            var grouped = new VerboseGroupedTestOutcome
            {
                Actual = "A",
                Predicted = "B",
                Count = 3,
                Details = new[]
                {
                    new VerboseTestOutcome { Actual = "A", Predicted = "B" },
                },
            };

            // Assert
            grouped.Details.Should().HaveCount(1);
        }

        [TestMethod]
        public void ClassificationErrors_DefaultConstructor_LeavesDefaults()
        {
            // Act
            var errors = new ClassificationErrors();

            // Assert
            errors.Class.Should().BeNull();
            errors.FP.Should().Be(0);
            errors.FN.Should().Be(0);
        }

        [TestMethod]
        public void ClassificationErrors_JsonConstructor_SetsAllProperties()
        {
            // Arrange
            var outcomes = new[]
            {
                new KeyValuePair<VerboseTestOutcome, string>(
                    new VerboseTestOutcome { Actual = "A", Predicted = "B" },
                    "FP"
                ),
            };

            // Act
            var errors = new ClassificationErrors(
                @class: "Inbox",
                verboseOutcomes: outcomes,
                falsePositives: 2,
                falseNegatives: 1,
                precision: 0.9,
                recall: 0.8,
                f1: 0.85
            );

            // Assert
            errors.Class.Should().Be("Inbox");
            errors.FP.Should().Be(2);
            errors.FN.Should().Be(1);
            errors.Precision.Should().Be(0.9);
            errors.Recall.Should().Be(0.8);
            errors.F1.Should().Be(0.85);
        }

        [TestMethod]
        public void ClassificationErrors_JsonSerialization_RoundTrips()
        {
            // Arrange
            var errors = new ClassificationErrors
            {
                Class = "Test",
                FP = 3,
                FN = 2,
                Precision = 0.75,
                Recall = 0.80,
                F1 = 0.77,
                VerboseOutcomes = new Dictionary<VerboseTestOutcome, string>
                {
                    {
                        new VerboseTestOutcome { Actual = "A", Predicted = "B" },
                        "FP"
                    },
                },
            };

            // Act
            var json = JsonConvert.SerializeObject(errors);
            var deserialized = JsonConvert.DeserializeObject<ClassificationErrors>(json);

            // Assert
            deserialized.Class.Should().Be("Test");
            deserialized.FP.Should().Be(3);
        }

        [TestMethod]
        public void ThresholdMetric_PropertiesRoundTrip()
        {
            // Act
            var metric = new ThresholdMetric
            {
                Threshold = 0.5,
                Precision = 0.9,
                PrecisionCount = 10,
                Recall = 0.8,
                RecallCount = 8,
                F1 = 0.85,
                F1Count = 9,
            };

            // Assert
            metric.Threshold.Should().Be(0.5);
            metric.PrecisionCount.Should().Be(10);
            metric.F1Count.Should().Be(9);
        }

        [TestMethod]
        public void VerboseOutcomeClass_CanBeInstantiated()
        {
            // Act
            var instance = new VerboseOutcomeClass();

            // Assert
            instance.Should().NotBeNull();
        }
    }
}
