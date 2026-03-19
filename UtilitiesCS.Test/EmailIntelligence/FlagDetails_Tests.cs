using System.Collections.ObjectModel;
using System.Collections.Specialized;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class FlagDetails_Tests
    {
        [TestMethod]
        public void Constructor_WithNoArguments_InitializesEmptyDefaults()
        {
            // Arrange

            // Act
            var flagDetails = new FlagDetails();

            // Assert
            flagDetails.Identifier.Should().Be("not set");
            flagDetails.Prefix.Should().BeEmpty();
            flagDetails.List.Should().BeEmpty();
            flagDetails.ListWithPrefix.Should().BeEmpty();
            flagDetails.NoPrefix.Should().BeEmpty();
            flagDetails.WithPrefix.Should().BeEmpty();
        }

        [TestMethod]
        public void Constructor_WithPrefix_PopulatesPrefixedRepresentationsWhenListChanges()
        {
            // Arrange
            var flagDetails = new FlagDetails("Tag PPL ");

            // Act
            flagDetails.List.Add("John");
            flagDetails.List.Add("Jane");

            // Assert
            flagDetails.NoPrefix.Should().Be("John, Jane");
            flagDetails.WithPrefix.Should().Be("Tag PPL John, Tag PPL Jane");
            flagDetails.ListWithPrefix.Should().Equal("Tag PPL John", "Tag PPL Jane");
        }

        [TestMethod]
        public void ListSetter_WithPrefixedValues_StripsPrefixAndRaisesCollectionChanged()
        {
            // Arrange
            var flagDetails = new FlagDetails("Tag PROJECT ");
            NotifyCollectionChangedEventArgs eventArgs = null;
            flagDetails.CollectionChanged += (_, args) => eventArgs = args;
            var incomingList = new ObservableCollection<string>
            {
                "Tag PROJECT Alpha",
                "Tag PROJECT Beta",
            };

            // Act
            flagDetails.List = incomingList;

            // Assert
            flagDetails.List.Should().Equal("Alpha", "Beta");
            flagDetails.ListWithPrefix.Should().Equal("Tag PROJECT Alpha", "Tag PROJECT Beta");
            flagDetails.NoPrefix.Should().Be("Alpha, Beta");
            flagDetails.WithPrefix.Should().Be("Tag PROJECT Alpha, Tag PROJECT Beta");
            eventArgs.Should().NotBeNull();
            eventArgs.Action.Should().Be(NotifyCollectionChangedAction.Replace);
        }

        [TestMethod]
        public void ListSetter_WithNullValue_ClearsExistingItems()
        {
            // Arrange
            var flagDetails = new FlagDetails("Tag KB ");
            flagDetails.List = new ObservableCollection<string> { "Knowledge" };

            // Act
            flagDetails.List = null;

            // Assert
            flagDetails.List.Should().BeEmpty();
            flagDetails.ListWithPrefix.Should().BeEmpty();
            flagDetails.NoPrefix.Should().BeEmpty();
            flagDetails.WithPrefix.Should().BeEmpty();
        }

        [TestMethod]
        public void ListWithPrefix_WhenMutated_UpdatesListWithoutPrefixes()
        {
            // Arrange
            var flagDetails = new FlagDetails("_@");

            // Act
            flagDetails.ListWithPrefix.Add("_@ContextOne");

            // Assert
            flagDetails.List.Should().ContainSingle().Which.Should().Be("ContextOne");
            flagDetails.WithPrefix.Should().Be("_@ContextOne");
            flagDetails.NoPrefix.Should().Be("ContextOne");
        }

        [TestMethod]
        public void CloneAndDeepCopy_PreserveValuesWhileDeepCopyDetachesCollections()
        {
            // Arrange
            var flagDetails = new FlagDetails("Tag TOPIC ")
            {
                Identifier = "topics",
                List = new ObservableCollection<string> { "Alpha" },
            };

            // Act
            var shallowClone = (FlagDetails)flagDetails.Clone();
            var deepCopy = flagDetails.DeepCopy();
            flagDetails.List.Add("Beta");

            // Assert
            shallowClone.Should().NotBeSameAs(flagDetails);
            shallowClone.Identifier.Should().Be("topics");
            shallowClone.List.Should().Equal("Alpha", "Beta");
            deepCopy.Should().NotBeSameAs(flagDetails);
            deepCopy.Identifier.Should().Be("topics");
            deepCopy.List.Should().Equal("Alpha");
            deepCopy.ListWithPrefix.Should().Equal("Tag TOPIC Alpha");
        }
    }
}
