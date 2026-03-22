using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class FilterEntry_Tests
    {
        [TestMethod]
        public void DefaultConstructor_InitializesEmptyProperties()
        {
            // Act
            var entry = new FilterEntry();

            // Assert
            entry.Name.Should().BeEmpty();
            entry.Description.Should().BeEmpty();
            entry.Folders.Should().NotBeNull().And.BeEmpty();
            entry.Flags.Should().NotBeNull();
        }

        [TestMethod]
        public void Constructor_WithNameFoldersAndCategories_SetsProperties()
        {
            // Arrange
            var folders = new List<string> { "Inbox", "Archive" };
            var categories = new List<string> { "Work", "Personal" };

            // Act
            var entry = new FilterEntry("TestFilter", folders, categories);

            // Assert
            entry.Name.Should().Be("TestFilter");
            entry.Folders.Should().Equal("Inbox", "Archive");
            entry.Flags.Should().NotBeNull();
        }

        [TestMethod]
        public void Constructor_WithAllParameters_SetsAllProperties()
        {
            // Arrange
            var folders = new List<string> { "Inbox" };
            var flags = new FlagClassNoItem("");

            // Act
            var entry = new FilterEntry("Name", "Description", folders, flags);

            // Assert
            entry.Name.Should().Be("Name");
            entry.Description.Should().Be("Description");
            entry.Folders.Should().BeSameAs(folders);
            entry.Flags.Should().BeSameAs(flags);
        }

        [TestMethod]
        public void Clone_ReturnsDistinctCopyWithSameValues()
        {
            // Arrange
            var original = new FilterEntry(
                "Name",
                "Desc",
                new List<string> { "A", "B" },
                new FlagClassNoItem("")
            );

            // Act
            var clone = (FilterEntry)original.Clone();

            // Assert
            clone.Should().NotBeSameAs(original);
            clone.Name.Should().Be(original.Name);
            clone.Description.Should().Be(original.Description);
            clone.Folders.Should().Equal(original.Folders);
            clone.Folders.Should().NotBeSameAs(original.Folders);
            clone.Flags.Should().NotBeSameAs(original.Flags);
        }

        [TestMethod]
        public void Clone_ModifyingClone_DoesNotAffectOriginal()
        {
            // Arrange
            var original = new FilterEntry(
                "Name",
                "Desc",
                new List<string> { "A" },
                new FlagClassNoItem("")
            );

            // Act
            var clone = (FilterEntry)original.Clone();
            clone.Name = "Modified";
            clone.Folders.Add("B");

            // Assert
            original.Name.Should().Be("Name");
            original.Folders.Should().ContainSingle().Which.Should().Be("A");
        }

        [TestMethod]
        public void RevertToCopy_RestoresAllFields()
        {
            // Arrange
            var original = new FilterEntry(
                "A",
                "DescA",
                new List<string> { "X" },
                new FlagClassNoItem("")
            );
            var copy = new FilterEntry(
                "B",
                "DescB",
                new List<string> { "Y" },
                new FlagClassNoItem("")
            );

            // Act
            original.RevertToCopy(copy);

            // Assert
            original.Name.Should().Be("B");
            original.Description.Should().Be("DescB");
            original.Folders.Should().Equal("Y");
        }

        [TestMethod]
        public void Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var entry = new FilterEntry();

            // Act
            entry.Name = "TestName";
            entry.Description = "TestDesc";
            entry.Folders = new List<string> { "F1" };
            entry.Flags = new FlagClassNoItem("");

            // Assert
            entry.Name.Should().Be("TestName");
            entry.Description.Should().Be("TestDesc");
            entry.Folders.Should().ContainSingle().Which.Should().Be("F1");
            entry.Flags.Should().NotBeNull();
        }
    }
}
