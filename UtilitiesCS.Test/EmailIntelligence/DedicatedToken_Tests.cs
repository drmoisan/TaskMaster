using System.ComponentModel;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.EmailIntelligence.Bayesian;

namespace UtilitiesCS.Test.EmailIntelligence
{
#pragma warning disable CS0618 // Deprecated types are intentionally covered by this plan.
    [TestClass]
    public class DedicatedToken_Tests
    {
        [TestMethod]
        public void DefaultConstructor_LeavesPropertiesNull()
        {
            // Act
            var token = new DedicatedToken();

            // Assert
            token.Token.Should().BeNull();
            token.FolderPath.Should().BeNull();
            token.Count.Should().Be(0);
        }

        [TestMethod]
        public void Constructor_WithParameters_SetsAllProperties()
        {
            // Act
            var token = new DedicatedToken("hello", @"Inbox\Work", 5);

            // Assert
            token.Token.Should().Be("hello");
            token.FolderPath.Should().Be(@"Inbox\Work");
            token.Count.Should().Be(5);
        }

        [TestMethod]
        public void Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var token = new DedicatedToken();

            // Act
            token.Token = "test";
            token.FolderPath = "Archive";
            token.Count = 10;

            // Assert
            token.Token.Should().Be("test");
            token.FolderPath.Should().Be("Archive");
            token.Count.Should().Be(10);
        }

        [TestMethod]
        public void Equals_WithSameTokenAndFolder_ReturnsTrue()
        {
            // Arrange
            var a = new DedicatedToken("hello", "Inbox", 1);
            var b = new DedicatedToken("hello", "Inbox", 99);

            // Act / Assert
            a.Equals(b).Should().BeTrue();
        }

        [TestMethod]
        public void Equals_WithDifferentToken_ReturnsFalse()
        {
            // Arrange
            var a = new DedicatedToken("hello", "Inbox", 1);
            var b = new DedicatedToken("world", "Inbox", 1);

            // Act / Assert
            a.Equals(b).Should().BeFalse();
        }

        [TestMethod]
        public void Equals_WithDifferentFolder_ReturnsFalse()
        {
            // Arrange
            var a = new DedicatedToken("hello", "Inbox", 1);
            var b = new DedicatedToken("hello", "Archive", 1);

            // Act / Assert
            a.Equals(b).Should().BeFalse();
        }

        [TestMethod]
        public void Equals_WithNull_ReturnsFalse()
        {
            // Arrange
            var token = new DedicatedToken("hello", "Inbox", 1);

            // Act / Assert
            token.Equals(null).Should().BeFalse();
        }

        [TestMethod]
        public void PropertyChanged_CanBeSubscribedAndUnsubscribed()
        {
            // Arrange
            var token = new DedicatedToken();
            PropertyChangedEventHandler handler = (s, e) => { };

            // Act / Assert - no-op event should not throw
            token.PropertyChanged += handler;
            token.PropertyChanged -= handler;
        }
    }
#pragma warning restore CS0618
}
