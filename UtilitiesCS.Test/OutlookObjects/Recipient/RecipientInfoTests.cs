using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.OutlookObjects.RecipientCoverage
{
    [TestClass]
    public class RecipientInfoTests
    {
        [TestMethod]
        public void Constructor_WithValues_PopulatesProperties()
        {
            // Arrange / Act
            var recipient = new RecipientInfo("Ada", "ada@example.com", "<a>Ada</a>");

            // Assert
            recipient.Name.Should().Be("Ada");
            recipient.Address.Should().Be("ada@example.com");
            recipient.Html.Should().Be("<a>Ada</a>");
        }

        [TestMethod]
        public void Properties_CanBeUpdatedAfterDefaultConstruction()
        {
            // Arrange
            var recipient = new RecipientInfo();

            // Act
            recipient.Name = "Grace";
            recipient.Address = "grace@example.com";
            recipient.Html = "<span>Grace</span>";

            // Assert
            recipient.Name.Should().Be("Grace");
            recipient.Address.Should().Be("grace@example.com");
            recipient.Html.Should().Be("<span>Grace</span>");
        }

        [DataTestMethod]
        [DataRow("Ada", "ada@example.com", "Ada", "ada@example.com")]
        [DataRow(null, "ada@example.com", null, "ada@example.com")]
        [DataRow("Ada", null, "Ada", null)]
        public void Equals_ShouldReturnTrue_WhenNameAndAddressMatch(string leftName, string leftAddress, string rightName, string rightAddress)
        {
            // Arrange
            var left = new RecipientInfo(leftName, leftAddress, "<left />");
            var right = new RecipientInfo(rightName, rightAddress, "<right />");

            // Act
            bool result = left.Equals(right);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void Equals_ShouldReturnFalse_WhenOtherIsNull()
        {
            // Arrange
            var recipient = new RecipientInfo("Ada", "ada@example.com", null);

            // Act
            bool result = recipient.Equals(null);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldReturnFalse_WhenBothCandidatesLackNameAndAddress()
        {
            // Arrange
            var left = new RecipientInfo(null, null, "<left />");
            var right = new RecipientInfo(null, null, "<right />");

            // Act
            bool result = left.Equals(right);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldReturnFalse_WhenOnlyOneCandidateHasIdentityData()
        {
            // Arrange
            var left = new RecipientInfo(null, null, null);
            var right = new RecipientInfo("Ada", "ada@example.com", null);

            // Act
            bool result = left.Equals(right);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldReturnFalse_WhenNameOrAddressDiffers()
        {
            // Arrange
            var left = new RecipientInfo("Ada", "ada@example.com", null);
            var differentName = new RecipientInfo("Grace", "ada@example.com", null);
            var differentAddress = new RecipientInfo("Ada", "grace@example.com", null);

            // Act / Assert
            left.Equals(differentName).Should().BeFalse();
            left.Equals(differentAddress).Should().BeFalse();
        }

        [TestMethod]
        public void GetHashCode_ShouldMatchForEquivalentRecipients_AndDifferForDifferentIdentity()
        {
            // Arrange
            var baseline = new RecipientInfo("Ada", "ada@example.com", "<left />");
            var equivalent = new RecipientInfo("Ada", "ada@example.com", "<right />");
            var different = new RecipientInfo("Grace", "grace@example.com", "<other />");

            // Act / Assert
            baseline.GetHashCode().Should().Be(equivalent.GetHashCode());
            baseline.GetHashCode().Should().NotBe(different.GetHashCode());
        }
    }
}