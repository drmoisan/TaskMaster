using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.EmailIntelligence;

namespace UtilitiesCS.Test.EmailIntelligence.ClassifierGroups.Triage
{
    [TestClass]
    public class TriageCreationTests
    {
        [TestMethod]
        public void CreateClassifier_ReturnsGroupWithClassifiersABC()
        {
            // Arrange

            // Act
            var group = UtilitiesCS.EmailIntelligence.Triage.CreateClassifier();

            // Assert
            group.Classifiers.Keys.Should().BeEquivalentTo(["A", "B", "C"]);
            group.Classifiers.Should().HaveCount(3);
        }

        [TestMethod]
        public void CreateClassifier_ReturnsGroupWithNonNullSharedTokenBase()
        {
            // Arrange

            // Act
            var group = UtilitiesCS.EmailIntelligence.Triage.CreateClassifier();

            // Assert
            group.SharedTokenBase.Should().NotBeNull();
        }
    }
}
