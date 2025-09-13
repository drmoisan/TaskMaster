using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class FlagParserTests
    {
        [TestMethod]
        public void Constructor_WithCategoryString_InitializesCorrectly()
        {
            // Arrange
            string categoryString = "Tag PPL John, Tag PROJECT ProjectA, Tag TOPIC Topic1, _@Context1, Tag KB KB1";
            
            // Act
            var parser = new FlagParser(ref categoryString);

            // Assert
            Assert.AreEqual("John", parser.GetPeople());
            Assert.AreEqual("ProjectA", parser.GetProjects());
            Assert.AreEqual("Topic1", parser.GetTopics());
            Assert.AreEqual("Context1", parser.GetContext());
            Assert.AreEqual("KB1", parser.GetKb());
        }

        [TestMethod]
        public void Constructor_WithCategoryList_InitializesCorrectly()
        {
            // Arrange
            var categories = new List<string> { "Tag PPL John", "Tag PROJECT ProjectA", "Tag TOPIC Topic1", "_@Context1", "Tag KB KB1" };

            // Act
            var parser = new FlagParser(categories);

            // Assert
            Assert.AreEqual("John", parser.GetPeople());
            Assert.AreEqual("ProjectA", parser.GetProjects());
            Assert.AreEqual("Topic1", parser.GetTopics());
            Assert.AreEqual("Context1", parser.GetContext());
            Assert.AreEqual("KB1", parser.GetKb());
        }

        [TestMethod]
        public void SetContext_UpdatesContextCorrectly()
        {
            // Arrange
            var parser = new FlagParser(new List<string>());
            string newContext = "_@NewContext";

            // Act
            parser.SetContext(false, newContext);

            // Assert
            Assert.AreEqual("NewContext", parser.GetContext());
        }

        [TestMethod]
        public void SetProjects_UpdatesProjectsCorrectly()
        {
            // Arrange
            var parser = new FlagParser(new List<string>());
            string newProject = "Tag PROJECT NewProject";

            // Act
            parser.SetProjects(false, newProject);

            // Assert
            Assert.AreEqual("NewProject", parser.GetProjects());
        }

        [TestMethod]
        public void SetProgram_UpdatesProgramCorrectly()
        {
            // Arrange
            var parser = new FlagParser(new List<string>());
            string newProgram = "TagProgram NewProgram";

            // Act
            parser.SetProgram(false, newProgram);

            // Assert
            Assert.AreEqual("NewProgram", parser.GetProgram());
        }

        [TestMethod]
        public void SetTopics_UpdatesTopicsCorrectly()
        {
            // Arrange
            var parser = new FlagParser(new List<string>());
            string newTopic = "Tag TOPIC NewTopic";

            // Act
            parser.SetTopics(false, newTopic);

            // Assert
            Assert.AreEqual("NewTopic", parser.GetTopics());
        }

        [TestMethod]
        public void SetPeople_UpdatesPeopleCorrectly()
        {
            // Arrange
            var parser = new FlagParser(new List<string>());
            string newPeople = "Tag PPL NewPerson";

            // Act
            parser.SetPeople(false, newPeople);

            // Assert
            Assert.AreEqual("NewPerson", parser.GetPeople());
        }

        [TestMethod]
        public void SetKb_UpdatesKbCorrectly()
        {
            // Arrange
            var parser = new FlagParser(new List<string>());
            string newKb = "Tag KB NewKb";

            // Act
            parser.SetKb(false, newKb);

            // Assert
            Assert.AreEqual("NewKb", parser.GetKb());
        }

        [TestMethod]
        public void AddWildcards_AddsWildcardsCorrectly()
        {
            // Arrange
            var parser = new FlagParser(new List<string>());
            string sourceString = "TestString";

            // Act
            string result = parser.AddWildcards(sourceString);

            // Assert
            Assert.AreEqual("*TestString*", result);
        }

        [TestMethod]
        public void AreEquivalentTo_StringComparison_ReturnsTrue()
        {
            // Arrange
            string categoryString = "Tag PPL John, Tag PROJECT ProjectA, Tag TOPIC Topic1, _@Context1, Tag KB KB1";
            var parser = new FlagParser(ref categoryString);
            string other = "Tag PPL John, Tag PROJECT ProjectA, Tag TOPIC Topic1, _@Context1, Tag KB KB1";

            // Act
            bool result = parser.AreEquivalentTo(other);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void AreEquivalentTo_ListComparison_ReturnsTrue()
        {
            // Arrange
            string categoryString = "Tag PPL John, Tag PROJECT ProjectA, Tag TOPIC Topic1, _@Context1, Tag KB KB1";
            var parser = new FlagParser(ref categoryString);
            var other = new List<string> { "Tag PPL John", "Tag PROJECT ProjectA", "Tag TOPIC Topic1", "_@Context1", "Tag KB KB1" };

            // Act
            bool result = parser.AreEquivalentTo(other);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Clone_CreatesShallowCopy()
        {
            // Arrange
            string categoryString = "Tag PPL John, Tag PROJECT ProjectA, Tag TOPIC Topic1, _@Context1, Tag KB KB1";
            var parser = new FlagParser(ref categoryString);

            // Act
            var clone = (FlagParser)parser.Clone();

            // Assert
            Assert.AreEqual(parser.GetPeople(), clone.GetPeople());
            Assert.AreEqual(parser.GetProjects(), clone.GetProjects());
            Assert.AreEqual(parser.GetTopics(), clone.GetTopics());
            Assert.AreEqual(parser.GetContext(), clone.GetContext());
            Assert.AreEqual(parser.GetKb(), clone.GetKb());
        }

        [TestMethod]
        public void DeepCopy_CreatesDeepCopy()
        {
            // Arrange
            string categoryString = "Tag PPL John, Tag PROJECT ProjectA, Tag TOPIC Topic1, _@Context1, Tag KB KB1";
            var parser = new FlagParser(ref categoryString);

            // Act
            var deepCopy = parser.DeepCopy();

            // Assert
            Assert.AreEqual(parser.GetPeople(), deepCopy.GetPeople());
            Assert.AreEqual(parser.GetProjects(), deepCopy.GetProjects());
            Assert.AreEqual(parser.GetTopics(), deepCopy.GetTopics());
            Assert.AreEqual(parser.GetContext(), deepCopy.GetContext());
            Assert.AreEqual(parser.GetKb(), deepCopy.GetKb());
        }
    }
}
