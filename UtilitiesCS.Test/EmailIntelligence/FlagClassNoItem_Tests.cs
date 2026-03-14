using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class FlagClassNoItem_Tests
    {
        [TestMethod]
        public void Constructor_WithNullCategoryString_NormalizesToEmptyFlags()
        {
            // Arrange
            string categoryNames = null;

            // Act
            var flagClass = new FlagClassNoItem(categoryNames);

            // Assert
            flagClass.CategoryNames.Should().BeEmpty();
            flagClass.Flags.GetPeople().Should().BeEmpty();
            flagClass.Flags.GetProjects().Should().BeEmpty();
            flagClass.Flags.GetTopics().Should().BeEmpty();
            flagClass.Flags.GetContext().Should().BeEmpty();
            flagClass.Flags.GetKb().Should().BeEmpty();
            flagClass.Today.Should().BeFalse();
            flagClass.Bullpin.Should().BeFalse();
            flagClass.OlCategories.Should().BeNull();
            flagClass.OlCategorySelection.Should().BeNull();
        }

        [TestMethod]
        public void Constructor_WithCategoryString_ParsesKnownFlagsAndExposesTranslators()
        {
            // Arrange
            const string categoryNames = "Tag PPL John, Tag PROJECT Alpha, Tag TOPIC TopicOne, _@ContextOne, Tag KB Knowledge, Tag A Top Priority Today, Tag Bullpin Priorities";

            // Act
            var flagClass = new FlagClassNoItem(categoryNames);

            // Assert
            flagClass.CategoryNames.Should().Be(categoryNames);
            flagClass.People.AsStringWithPrefix.Should().Be("Tag PPL John");
            flagClass.Projects.AsStringWithPrefix.Should().Be("Tag PROJECT Alpha");
            flagClass.Topics.AsStringWithPrefix.Should().Be("Tag TOPIC TopicOne");
            flagClass.Context.AsStringWithPrefix.Should().Be("_@ContextOne");
            flagClass.KB.AsStringWithPrefix.Should().Be("Tag KB Knowledge");
            flagClass.Today.Should().BeTrue();
            flagClass.Bullpin.Should().BeTrue();
        }

        [TestMethod]
        public void Constructor_WithCategoryList_NormalizesCategoryNamesFromParsedFlags()
        {
            // Arrange
            var categoryList = new List<string>
            {
                "Tag PROJECT Alpha",
                "Tag PPL John",
                "Tag KB Knowledge",
            };

            // Act
            var flagClass = new FlagClassNoItem(categoryList);

            // Assert
            flagClass.Flags.GetPeople().Should().Be("John");
            flagClass.Flags.GetProjects().Should().Be("Alpha");
            flagClass.Flags.GetKb().Should().Be("Knowledge");
            flagClass.CategoryNames.Should().Contain("Tag PPL John");
            flagClass.CategoryNames.Should().Contain("Tag PROJECT Alpha");
            flagClass.CategoryNames.Should().Contain("Tag KB Knowledge");
        }

        [TestMethod]
        public void Constructor_WithOutlookCategoryList_PopulatesOlCategoriesAndSelection()
        {
            // Arrange
            var selectedCategory = CreateCategory("Tag PPL John");
            var unselectedCategory = CreateCategory("Tag PROJECT Alpha");
            IList<Category> categories = new List<Category> { selectedCategory, unselectedCategory };

            // Act
            var flagClass = new FlagClassNoItem(categories);

            // Assert
            flagClass.OlCategories.Should().BeSameAs(categories);
            flagClass.CategoryNames.Should().Contain("Tag PPL John");
            flagClass.CategoryNames.Should().Contain("Tag PROJECT Alpha");
            flagClass.OlCategorySelection.Should().BeEquivalentTo(categories);
        }

        [TestMethod]
        public void CategoryNames_Setter_RaisesPropertyChangedAndReparsesFlags()
        {
            // Arrange
            var flagClass = new FlagClassNoItem("Tag PPL John");
            var changedProperties = new List<string>();
            flagClass.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);

            // Act
            flagClass.CategoryNames = "Tag PROJECT Alpha, _@ContextOne";

            // Assert
            changedProperties.Should().Contain(nameof(FlagClassNoItem.CategoryNames));
            flagClass.Flags.GetPeople().Should().BeEmpty();
            flagClass.Flags.GetProjects().Should().Be("Alpha");
            flagClass.Flags.GetContext().Should().Be("ContextOne");
        }

        [TestMethod]
        public void SelectionToOlCategories_FiltersConfiguredCategoriesByCurrentCategoryNames()
        {
            // Arrange
            var peopleCategory = CreateCategory("Tag PPL John");
            var contextCategory = CreateCategory("_@ContextOne");
            var unrelatedCategory = CreateCategory("Tag PROJECT Alpha");
            var flagClass = new FlagClassNoItem("Tag PPL John, _@ContextOne")
            {
                OlCategories = new List<Category> { peopleCategory, contextCategory, unrelatedCategory },
            };

            // Act
            var selection = flagClass.SelectionToOlCategories();

            // Assert
            selection.Should().BeEquivalentTo(new[] { peopleCategory, contextCategory });
        }

        [TestMethod]
        public void Clone_ReturnsDistinctInstanceWithEquivalentVisibleState()
        {
            // Arrange
            var flagClass = new FlagClassNoItem("Tag PPL John, Tag PROJECT Alpha");

            // Act
            var clone = (FlagClassNoItem)flagClass.Clone();

            // Assert
            clone.Should().NotBeSameAs(flagClass);
            clone.CategoryNames.Should().Be(flagClass.CategoryNames);
            clone.Flags.GetPeople().Should().Be(flagClass.Flags.GetPeople());
            clone.Flags.GetProjects().Should().Be(flagClass.Flags.GetProjects());
            clone.OlCategories.Should().BeNull();
        }

        [TestMethod]
        public void TodayAndBullpin_Setters_ProxyToUnderlyingFlags()
        {
            // Arrange
            var flagClass = new FlagClassNoItem(string.Empty);

            // Act
            flagClass.Today = true;
            flagClass.Bullpin = true;

            // Assert
            flagClass.Today.Should().BeTrue();
            flagClass.Bullpin.Should().BeTrue();
            flagClass.Flags.Today.Should().BeTrue();
            flagClass.Flags.Bullpin.Should().BeTrue();
        }

        private static Category CreateCategory(string name)
        {
            var mock = new Mock<Category>(MockBehavior.Loose);
            mock.SetupGet(category => category.Name).Returns(name);
            return mock.Object;
        }
    }
}