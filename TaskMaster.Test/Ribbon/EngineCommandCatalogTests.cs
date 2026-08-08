using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskMaster.Test.Ribbon
{
    /// <summary>
    /// Unit tests for the issue #503 control-id to engine-key catalog. The catalog is the single
    /// source of truth shared by the ribbon XML, the <c>getEnabled</c> callback, the click guards,
    /// and the post-initialization refresh, so these tests pin the exact mapping.
    /// </summary>
    [TestClass]
    public class EngineCommandCatalogTests
    {
        [DataTestMethod]
        [DataRow("TrainSpam", "Spam")]
        [DataRow("TrainHam", "Spam")]
        [DataRow("TestSpam", "Spam")]
        [DataRow("TriageSetA", "Triage")]
        [DataRow("TriageSetB", "Triage")]
        [DataRow("TriageSetC", "Triage")]
        [DataRow("ClearTriage", "Triage")]
        [DataRow("FilterTriageGroup", "Triage")]
        public void TryGetEngineName_ForEachEngineBackedControlId_ReturnsExpectedEngineName(
            string controlId,
            string expectedEngineName
        )
        {
            // Act
            var mapped = EngineCommandCatalog.TryGetEngineName(controlId, out var engineName);

            // Assert
            mapped.Should().BeTrue("'{0}' is an engine-backed control id", controlId);
            engineName.Should().Be(expectedEngineName);
        }

        [TestMethod]
        public void TryGetEngineName_ForUnknownControlId_ReturnsFalse()
        {
            // Act
            var mapped = EngineCommandCatalog.TryGetEngineName(
                "NotAnEngineBackedControl",
                out var engineName
            );

            // Assert
            mapped.Should().BeFalse("the catalog must not claim a control it does not own");
            engineName.Should().BeNull();
        }

        [TestMethod]
        public void TryGetEngineName_WithNullControlId_ReturnsFalse()
        {
            // Act: Office supplies control.Id, which the shim passes through unvalidated.
            var mapped = EngineCommandCatalog.TryGetEngineName(null, out var engineName);

            // Assert
            mapped.Should().BeFalse("a null id must be rejected, not thrown on");
            engineName.Should().BeNull();
        }

        [TestMethod]
        public void TryGetEngineName_WithEmptyControlId_ReturnsFalse()
        {
            // Act
            var mapped = EngineCommandCatalog.TryGetEngineName(string.Empty, out var engineName);

            // Assert
            mapped.Should().BeFalse("an empty id must be rejected, not thrown on");
            engineName.Should().BeNull();
        }

        [TestMethod]
        public void ControlIds_ContainsExactlyTheEightEngineBackedControlIds()
        {
            // Arrange
            var expected = new[]
            {
                "TrainSpam",
                "TrainHam",
                "TestSpam",
                "TriageSetA",
                "TriageSetB",
                "TriageSetC",
                "ClearTriage",
                "FilterTriageGroup",
            };

            // Act
            var actual = EngineCommandCatalog.ControlIds;

            // Assert: set equality, because callback ordering is unspecified in Office.
            actual
                .Should()
                .BeEquivalentTo(
                    expected,
                    "the verified defect surface is exactly these eight controls"
                );
        }

        [TestMethod]
        public void ControlIds_ContainsNoDuplicates()
        {
            // Act
            var controlIds = EngineCommandCatalog.ControlIds;

            // Assert: a duplicate would cause a control to be invalidated twice on every refresh.
            controlIds
                .Should()
                .HaveCount(
                    controlIds.Distinct().Count(),
                    "each engine-backed control id must appear exactly once"
                );
        }
    }
}
