using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Unit tests for <see cref="FolderBreadcrumbSegment"/> construction and property exposure,
    /// including the <c>HasChildren</c> flag used by the UI to render the expand affordance.
    /// </summary>
    [TestClass]
    public sealed class FolderBreadcrumbSegmentTests
    {
        [TestMethod]
        public void Constructor_WithAllProperties_ExposesEachValue()
        {
            // Arrange
            var key = new FolderTreeNodeKey("store-a", "entry-1", "\\Root\\Clients");

            // Act
            var segment = new FolderBreadcrumbSegment(key, "Clients", "\\Root\\Clients", true);

            // Assert
            segment.Key.Should().BeSameAs(key);
            segment.DisplayName.Should().Be("Clients");
            segment.FolderPath.Should().Be("\\Root\\Clients");
            segment.HasChildren.Should().BeTrue();
        }

        [TestMethod]
        public void Constructor_WithHasChildrenTrue_SetsExpandAffordanceFlag()
        {
            // Arrange
            var key = new FolderTreeNodeKey("store-a", "entry-2", "\\Root");

            // Act
            var segment = new FolderBreadcrumbSegment(key, "Root", "\\Root", true);

            // Assert
            segment.HasChildren.Should().BeTrue();
        }

        [TestMethod]
        public void Constructor_WithHasChildrenFalse_ClearsExpandAffordanceFlag()
        {
            // Arrange
            var key = new FolderTreeNodeKey("store-a", "entry-3", "\\Root\\Leaf");

            // Act
            var segment = new FolderBreadcrumbSegment(key, "Leaf", "\\Root\\Leaf", false);

            // Assert
            segment.HasChildren.Should().BeFalse();
        }

        [TestMethod]
        public void Constructor_WithNullKey_ThrowsArgumentNullException()
        {
            // Arrange, Act
            Action act = () => new FolderBreadcrumbSegment(null, "Name", "\\Path", false);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("key");
        }

        [TestMethod]
        public void Constructor_WithNullStrings_StoresEmptyStringsNeverNull()
        {
            // Arrange
            var key = new FolderTreeNodeKey("store-a", "entry-4", "\\Root");

            // Act
            var segment = new FolderBreadcrumbSegment(key, null, null, false);

            // Assert
            segment.DisplayName.Should().Be(string.Empty);
            segment.FolderPath.Should().Be(string.Empty);
        }
    }
}
