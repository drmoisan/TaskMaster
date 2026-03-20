using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ToDoModel.Data_Model.People;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class PeopleScoDictionaryNew_Tests
    {
        [TestMethod]
        public void DefaultConstructor_ShouldCreateEmptyDictionary()
        {
            var dict = new PeopleScoDictionaryNew();

            dict.Count.Should().Be(0);
        }

        [TestMethod]
        public void Constructor_WithGlobals_ShouldSetGlobals()
        {
            var mockApp = new Mock<Microsoft.Office.Interop.Outlook.Application>();
            var globals = new TaskMaster.ApplicationGlobals(mockApp.Object, true);

            var dict = new PeopleScoDictionaryNew(globals);

            dict.Count.Should().Be(0);
        }

        [TestMethod]
        public void Prefix_SetAndGet_ShouldWork()
        {
            var dict = new PeopleScoDictionaryNew();
            var mockPrefix = new Mock<IPrefix>();
            mockPrefix.SetupGet(p => p.Value).Returns("P:");

            dict.Prefix = mockPrefix.Object;

            dict.Prefix.Should().BeSameAs(mockPrefix.Object);
        }

        [TestMethod]
        public void IsPeopleCategory_WhenStringStartsWithPrefix_ShouldReturnTrue()
        {
            var dict = new PeopleScoDictionaryNew();
            var mockPrefix = new Mock<IPrefix>();
            mockPrefix.SetupGet(p => p.Value).Returns("People:");
            dict.Prefix = mockPrefix.Object;

            dict.IsPeopleCategory("People:John").Should().BeTrue();
        }

        [TestMethod]
        public void IsPeopleCategory_WhenStringDoesNotStartWithPrefix_ShouldReturnFalse()
        {
            var dict = new PeopleScoDictionaryNew();
            var mockPrefix = new Mock<IPrefix>();
            mockPrefix.SetupGet(p => p.Value).Returns("People:");
            dict.Prefix = mockPrefix.Object;

            dict.IsPeopleCategory("Other:John").Should().BeFalse();
        }

        [TestMethod]
        public void IsPeopleCategory_WhenNull_ShouldReturnFalse()
        {
            var dict = new PeopleScoDictionaryNew();
            var mockPrefix = new Mock<IPrefix>();
            mockPrefix.SetupGet(p => p.Value).Returns("P:");
            dict.Prefix = mockPrefix.Object;

            dict.IsPeopleCategory(null).Should().BeFalse();
        }

        [TestMethod]
        public void IsPeopleCategory_WhenShorterThanPrefix_ShouldReturnFalse()
        {
            var dict = new PeopleScoDictionaryNew();
            var mockPrefix = new Mock<IPrefix>();
            mockPrefix.SetupGet(p => p.Value).Returns("People:");
            dict.Prefix = mockPrefix.Object;

            dict.IsPeopleCategory("P").Should().BeFalse();
        }

        [TestMethod]
        public void AddPrefix_WhenSeedDoesNotStartWithPrefix_ShouldPrepend()
        {
            var dict = new PeopleScoDictionaryNew();

            var result = dict.AddPrefix("John", "People:");

            result.Should().Be("People:John");
        }

        [TestMethod]
        public void AddPrefix_WhenSeedAlreadyStartsWithPrefix_ShouldReturnUnchanged()
        {
            var dict = new PeopleScoDictionaryNew();

            var result = dict.AddPrefix("People:John", "People:");

            result.Should().Be("People:John");
        }

        [TestMethod]
        public void AddPrefix_WhenSeedIsNull_ShouldThrowArgumentNullException()
        {
            var dict = new PeopleScoDictionaryNew();

            Action act = () => dict.AddPrefix(null, "P:");

            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void AddPrefix_WhenPrefixIsNull_ShouldThrowArgumentNullException()
        {
            var dict = new PeopleScoDictionaryNew();

            Action act = () => dict.AddPrefix("John", null);

            act.Should().Throw<ArgumentNullException>();
        }
    }
}
