using System;
using System.Collections;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace UtilitiesCS.Test.OutlookObjects.Category
{
    [TestClass]
    public class CreateCategoryTests
    {
        [TestMethod]
        public void CreateCategory_WhenNameNeedsPrefix_AddsPrefixedCategory()
        {
            // Arrange
            var prefix = CreatePrefix("PRJ:", OlCategoryColor.olCategoryColorBlue);
            var createdCategory = new Mock<Microsoft.Office.Interop.Outlook.Category>();
            var categories = CreateCategoriesCollection();
            categories
                .Setup(x => x.Add("PRJ:Inbox", It.IsAny<object>(), It.IsAny<object>()))
                .Returns(createdCategory.Object);
            var session = new Mock<NameSpace>();
            session.SetupGet(x => x.Categories).Returns(categories.Object);

            // Act
            var result = session.Object.CreateCategory(prefix.Object, "Inbox");

            // Assert
            result.Should().BeSameAs(createdCategory.Object);
            categories.Verify(
                x => x.Add("PRJ:Inbox", It.IsAny<object>(), It.IsAny<object>()),
                Times.Once);
        }

        [TestMethod]
        public void CreateCategory_WhenNameAlreadyHasPrefix_DoesNotPrefixTwice()
        {
            // Arrange
            var prefix = CreatePrefix("PRJ:", OlCategoryColor.olCategoryColorGreen);
            var createdCategory = new Mock<Microsoft.Office.Interop.Outlook.Category>();
            var categories = CreateCategoriesCollection();
            categories
                .Setup(x => x.Add("PRJ:Inbox", It.IsAny<object>(), It.IsAny<object>()))
                .Returns(createdCategory.Object);
            var session = new Mock<NameSpace>();
            session.SetupGet(x => x.Categories).Returns(categories.Object);

            // Act
            var result = session.Object.CreateCategory(prefix.Object, "PRJ:Inbox");

            // Assert
            result.Should().BeSameAs(createdCategory.Object);
            categories.Verify(
                x => x.Add("PRJ:Inbox", It.IsAny<object>(), It.IsAny<object>()),
                Times.Once);
        }

        [TestMethod]
        public void CreateCategory_WhenPrefixValueIsEmpty_UsesOriginalCategoryName()
        {
            // Arrange
            var prefix = CreatePrefix(string.Empty, OlCategoryColor.olCategoryColorYellow);
            var createdCategory = new Mock<Microsoft.Office.Interop.Outlook.Category>();
            var categories = CreateCategoriesCollection();
            categories
                .Setup(x => x.Add("Inbox", It.IsAny<object>(), It.IsAny<object>()))
                .Returns(createdCategory.Object);
            var session = new Mock<NameSpace>();
            session.SetupGet(x => x.Categories).Returns(categories.Object);

            // Act
            var result = session.Object.CreateCategory(prefix.Object, "Inbox");

            // Assert
            result.Should().BeSameAs(createdCategory.Object);
            categories.Verify(
                x => x.Add("Inbox", It.IsAny<object>(), It.IsAny<object>()),
                Times.Once);
        }

        [TestMethod]
        public void CreateCategory_WhenNewNameIsShorterThanPrefix_StillPrefixesName()
        {
            // Arrange
            var prefix = CreatePrefix("PRJ:", OlCategoryColor.olCategoryColorOrange);
            var createdCategory = new Mock<Microsoft.Office.Interop.Outlook.Category>();
            var categories = CreateCategoriesCollection();
            categories
                .Setup(x => x.Add("PRJ:A", It.IsAny<object>(), It.IsAny<object>()))
                .Returns(createdCategory.Object);
            var session = new Mock<NameSpace>();
            session.SetupGet(x => x.Categories).Returns(categories.Object);

            // Act
            var result = session.Object.CreateCategory(prefix.Object, "A");

            // Assert
            result.Should().BeSameAs(createdCategory.Object);
            categories.Verify(
                x => x.Add("PRJ:A", It.IsAny<object>(), It.IsAny<object>()),
                Times.Once);
        }

        [TestMethod]
        public void CreateCategory_WhenAddThrows_ReturnsNull()
        {
            // Arrange
            var prefix = CreatePrefix("PRJ:", OlCategoryColor.olCategoryColorBlue);
            var categories = CreateCategoriesCollection();
            categories
                .Setup(x => x.Add("PRJ:Inbox", OlCategoryColor.olCategoryColorBlue, OlCategoryShortcutKey.olCategoryShortcutKeyNone))
                .Throws(new InvalidOperationException("Add failed"));
            var session = new Mock<NameSpace>();
            session.SetupGet(x => x.Categories).Returns(categories.Object);

            // Act
            var result = session.Object.CreateCategory(prefix.Object, "Inbox");

            // Assert
            result.Should().BeNull();
        }

        private static Mock<IPrefix> CreatePrefix(string value, OlCategoryColor color)
        {
            var prefix = new Mock<IPrefix>();
            prefix.SetupProperty(x => x.Value, value);
            prefix.SetupProperty(x => x.Color, color);
            return prefix;
        }

        private static Mock<Categories> CreateCategoriesCollection(params Microsoft.Office.Interop.Outlook.Category[] categories)
        {
            var collection = new ArrayList(categories);
            var categoriesMock = new Mock<Categories>();
            categoriesMock.Setup(x => x.GetEnumerator()).Returns(() => collection.GetEnumerator());
            return categoriesMock;
        }
    }
}