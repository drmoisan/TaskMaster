using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.EmailIntelligence.Bayesian.Performance;

namespace QuickFiler.Controllers.Tests
{
    [TestClass]
    public class BayesianPerformanceControllerTests
    {
        [TestMethod]
        public void AssignFormValues_WithClassificationError_MapsMetricsAndVerboseOutcomes()
        {
            BayesianPerformanceControllerTestSupport.RunWithViewer(
                (controller, viewer) =>
                {
                    // Arrange
                    VerboseTestOutcome outcome =
                        BayesianPerformanceControllerTestSupport.BuildOutcome(
                            "first-subject",
                            ("alpha", 0.91)
                        );
                    ClassificationErrors error =
                        BayesianPerformanceControllerTestSupport.BuildError("Inbox", outcome);

                    // Act
                    controller.AssignFormValues(error);

                    // Assert
                    viewer.FpCount.Text.Should().Be("1,234");
                    viewer.FnCount.Text.Should().Be("56");
                    viewer.TotalCount.Text.Should().Be("1,290");
                    viewer.PrecisionScore.Text.Should().Be("81.23%");
                    viewer.RecallScore.Text.Should().Be("93.46%");
                    viewer.F1Score.Text.Should().Be("86.92%");
                    viewer
                        .OlvVerboseDetails.Objects.Cast<KeyValuePair<VerboseTestOutcome, string>>()
                        .Should()
                        .ContainSingle()
                        .Which.Key.Should()
                        .BeSameAs(outcome);
                }
            );
        }

        [TestMethod]
        public void ClassSelectorSelectedIndexChanged_WithKnownClass_UpdatesActiveErrorAndFormValues()
        {
            BayesianPerformanceControllerTestSupport.RunWithViewer(
                (controller, viewer) =>
                {
                    // Arrange
                    VerboseTestOutcome first =
                        BayesianPerformanceControllerTestSupport.BuildOutcome("first", ("a", 0.1));
                    VerboseTestOutcome second =
                        BayesianPerformanceControllerTestSupport.BuildOutcome("second", ("b", 0.2));
                    ClassificationErrors firstError =
                        BayesianPerformanceControllerTestSupport.BuildError("Inbox", first);
                    ClassificationErrors secondError =
                        BayesianPerformanceControllerTestSupport.BuildError("Archive", second);
                    secondError.FP = 2;
                    secondError.FN = 3;
                    secondError.Errors = 5;
                    secondError.Precision = 0.5;
                    secondError.Recall = 0.25;
                    secondError.F1 = 0.3333;
                    controller.Errors = new[] { firstError, secondError };
                    viewer.ClassSelector.Items.AddRange(new object[] { "Inbox", "Archive" });
                    viewer.ClassSelector.SelectedItem = "Archive";

                    // Act
                    controller.ClassSelector_SelectedIndexChanged();

                    // Assert
                    controller.ActiveError.Should().BeSameAs(secondError);
                    viewer.FpCount.Text.Should().Be("2");
                    viewer.FnCount.Text.Should().Be("3");
                    viewer.TotalCount.Text.Should().Be("5");
                    viewer.PrecisionScore.Text.Should().Be("50.00%");
                    viewer.RecallScore.Text.Should().Be("25.00%");
                    viewer.F1Score.Text.Should().Be("33.33%");
                }
            );
        }

        [TestMethod]
        public void OlvVerboseDetailsSelectionChanged_WithDrivers_PopulatesDriverList()
        {
            BayesianPerformanceControllerTestSupport.RunWithViewer(
                (controller, viewer) =>
                {
                    // Arrange
                    VerboseTestOutcome outcome =
                        BayesianPerformanceControllerTestSupport.BuildOutcome(
                            "first-subject",
                            ("alpha", 0.91),
                            ("beta", 0.32)
                        );
                    ClassificationErrors error =
                        BayesianPerformanceControllerTestSupport.BuildError("Inbox", outcome);
                    KeyValuePair<VerboseTestOutcome, string> selected =
                        error.VerboseOutcomes.Single();
                    viewer.OlvVerboseDetails.SetObjects(new[] { selected });
                    viewer.OlvVerboseDetails.SelectedObject = selected;

                    // Act
                    controller.OlvVerboseDetails_SelectionChanged();

                    // Assert
                    controller.ActiveOutcome.Should().BeSameAs(outcome);
                    viewer
                        .OlvDrivers.Objects.Cast<(string Token, double TokenProbability)>()
                        .Should()
                        .BeEquivalentTo(new[] { ("alpha", 0.91), ("beta", 0.32) });
                }
            );
        }

        [TestMethod]
        public void OlvVerboseDetailsSelectionChanged_WithoutDrivers_ClearsDriverList()
        {
            BayesianPerformanceControllerTestSupport.RunWithViewer(
                (controller, viewer) =>
                {
                    // Arrange
                    VerboseTestOutcome outcome =
                        BayesianPerformanceControllerTestSupport.BuildOutcome("first-subject");
                    ClassificationErrors error =
                        BayesianPerformanceControllerTestSupport.BuildError("Inbox", outcome);
                    KeyValuePair<VerboseTestOutcome, string> selected =
                        error.VerboseOutcomes.Single();
                    viewer.OlvDrivers.SetObjects(new[] { ("existing", 0.44) });
                    viewer.OlvVerboseDetails.SetObjects(new[] { selected });
                    viewer.OlvVerboseDetails.SelectedObject = selected;

                    // Act
                    controller.OlvVerboseDetails_SelectionChanged();

                    // Assert
                    controller.ActiveOutcome.Should().BeSameAs(outcome);
                    viewer.OlvDrivers.Items.Count.Should().Be(0);
                }
            );
        }

        [TestMethod]
        public void OlvDriversSelectionChanged_WithSelectedToken_PopulatesDriverPresence()
        {
            BayesianPerformanceControllerTestSupport.RunWithViewer(
                (controller, viewer) =>
                {
                    // Arrange
                    VerboseTestOutcome selectedOutcome =
                        BayesianPerformanceControllerTestSupport.BuildOutcome(
                            "selected-subject",
                            ("alpha", 0.91)
                        );
                    VerboseTestOutcome relatedOutcome =
                        BayesianPerformanceControllerTestSupport.BuildOutcome(
                            "related-subject",
                            ("alpha", 0.63)
                        );
                    VerboseTestOutcome unrelatedOutcome =
                        BayesianPerformanceControllerTestSupport.BuildOutcome(
                            "unrelated-subject",
                            ("beta", 0.11)
                        );
                    var selectedDriver = ("alpha", 0.91);
                    controller.ActiveError = new ClassificationErrors
                    {
                        VerboseOutcomes = new Dictionary<VerboseTestOutcome, string>
                        {
                            [selectedOutcome] = "False Positive",
                            [relatedOutcome] = "False Negative",
                            [unrelatedOutcome] = "False Negative",
                        },
                    };
                    viewer.OlvDrivers.SetObjects(new[] { selectedDriver });
                    viewer.OlvDrivers.SelectedObject = selectedDriver;

                    // Act
                    controller.OlvDrivers_SelectionChanged();

                    // Assert
                    viewer
                        .OlvDriverPresence.Objects.Cast<(string Subject, double TokenProbability)>()
                        .Should()
                        .BeEquivalentTo(
                            new[] { ("selected-subject", 0.91), ("related-subject", 0.63) }
                        );
                }
            );
        }

        [TestMethod]
        public void OlvDriversSelectionChanged_WithoutSelection_ClearsDriverPresence()
        {
            BayesianPerformanceControllerTestSupport.RunWithViewer(
                (controller, viewer) =>
                {
                    // Arrange
                    viewer.OlvDriverPresence.SetObjects(new[] { ("existing", 0.44) });
                    viewer.OlvDrivers.SetObjects(Enumerable.Empty<(string, double)>());

                    // Act
                    controller.OlvDrivers_SelectionChanged();

                    // Assert
                    viewer.OlvDriverPresence.Items.Count.Should().Be(0);
                }
            );
        }
    }
}
