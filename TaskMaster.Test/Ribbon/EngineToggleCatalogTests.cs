using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskMaster.Test.Ribbon
{
    /// <summary>
    /// Unit tests for the issue #505 engine-key to toggle-control-id map. The map is the single
    /// source of truth shared by <c>EngineToggleStateCoordinator</c> and the ribbon XML, and is
    /// deliberately separate from <c>EngineCommandCatalog</c> so the two toggle checkboxes never
    /// acquire readiness-gated <c>getEnabled</c> semantics.
    /// </summary>
    [TestClass]
    public class EngineToggleCatalogTests
    {
        [DataTestMethod]
        [DataRow("Spam", "SpamBayesEnabledToggle")]
        [DataRow("Triage", "TriageEnabledToggle")]
        public void TryGetControlId_ForEachToggleEngineKey_ReturnsExpectedControlId(
            string engineName,
            string expectedControlId
        )
        {
            // Act
            var mapped = EngineToggleCatalog.TryGetControlId(engineName, out var controlId);

            // Assert
            mapped.Should().BeTrue("'{0}' has a toggle checkbox", engineName);
            controlId.Should().Be(expectedControlId);
        }

        [TestMethod]
        public void TryGetControlId_ForUnknownEngineName_ReturnsFalse()
        {
            // Act
            var mapped = EngineToggleCatalog.TryGetControlId(
                "NotAToggleBackedEngine",
                out var controlId
            );

            // Assert
            mapped.Should().BeFalse("the catalog must not claim an engine it does not own");
            controlId.Should().BeNull();
        }

        [TestMethod]
        public void TryGetControlId_WithNullEngineName_ReturnsFalse()
        {
            // Act: the coordinator forwards whatever key the ribbon callback supplied.
            var mapped = EngineToggleCatalog.TryGetControlId(null, out var controlId);

            // Assert
            mapped.Should().BeFalse("a null engine key must be rejected, not thrown on");
            controlId.Should().BeNull();
        }

        [TestMethod]
        public void TryGetControlId_WithEmptyEngineName_ReturnsFalse()
        {
            // Act
            var mapped = EngineToggleCatalog.TryGetControlId(string.Empty, out var controlId);

            // Assert
            mapped.Should().BeFalse("an empty engine key must be rejected, not thrown on");
            controlId.Should().BeNull();
        }

        [TestMethod]
        public void EngineNames_ContainsExactlyTheTwoToggleEngineKeys()
        {
            // Arrange
            var expected = new[] { "Spam", "Triage" };

            // Act
            var actual = EngineToggleCatalog.EngineNames;

            // Assert: set equality, because iteration order carries no meaning.
            actual
                .Should()
                .BeEquivalentTo(
                    expected,
                    "exactly two engines expose an enable/disable toggle checkbox"
                );
        }

        [TestMethod]
        public void EngineNames_ContainsNoDuplicates()
        {
            // Act
            var engineNames = EngineToggleCatalog.EngineNames;

            // Assert: a duplicate would invalidate the same control twice on every refresh.
            engineNames
                .Should()
                .HaveCount(
                    engineNames.Distinct().Count(),
                    "each toggle engine key must appear exactly once"
                );
        }
    }
}
