using Microsoft.Office.Interop.Outlook;
using Outlook = Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToDoModel.Data_Model.People;
using ToDoModel.Test.Properties;
using UtilitiesCS;
using UtilitiesCS.NewtonsoftHelpers.Sco;
using UtilitiesCS.ReusableTypeClasses;

namespace ToDoModel.Tests.Data_Model.People
{
    [TestClass]
    public class PeopleScoDictionaryNewTests
    {
        private Mock<IApplicationGlobals> _mockGlobals;
        private Mock<Outlook.Application> mockApplication;
        private Mock<IPrefix> _mockPrefix;
        private PeopleScoDictionaryNew _peopleScoDictionaryNew;

        [TestInitialize]
        public void Setup()
        {
            Console.SetOut(new DebugTextWriter());
            _mockGlobals = new Mock<IApplicationGlobals>();
            var fs = new Mock<IFileSystemFolderPaths>();
            var specialFolders = new Dictionary<string, string>()
            {
                { "AppData", "C:\\Users\\user\\AppData\\Roaming" },
                { "Flow", "C:\\Users\\user\\AppData\\Roaming\\Flow" },
                { "MyDocuments", "C:\\Users\\user\\Documents" },
                { "PreReads", "C:\\Users\\user\\Documents\\PreReads" },
                { "OneDrive", "C:\\Users\\user\\OneDrive" },
                { "PythonStaging", "C:\\Users\\user\\Documents\\PythonStaging" }
            }.ToConcurrentDictionary();
            fs.Setup(f => f.SpecialFolders).Returns(specialFolders);
            _mockGlobals.Setup(g => g.FS).Returns(fs.Object);
        }

        internal Category MockCategory(string name)
        {
            var m = new Mock<Category>();
            m.Setup(x => x.Name).Returns(name);
            return m.Object;
        }

        private JsonSerializerSettings GetSettings(IApplicationGlobals globals)
        {
            var settings = new JsonSerializerSettings()
            {
                //TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                TraceWriter = new NLogTraceWriter()
            };
            settings.Converters.Add(new AppGlobalsConverter(globals));
            settings.Converters.Add(new FilePathHelperConverter(globals.FS));

            return settings;
        }

        //[TestMethod]
        //public void IsPeopleCategory_ValidCategory_ReturnsTrue()
        //{
        //    // Arrange
        //    string testCategory = "prefixCategory";
        //    _mockPrefix.Setup(p => p.Value).Returns("prefix");

        //    // Act
        //    bool result = _peopleScoDictionaryNew.IsPeopleCategory(testCategory);

        //    // Assert
        //    Assert.IsTrue(result);
        //}

        //[TestMethod]
        //public void IsPeopleCategory_InvalidCategory_ReturnsFalse()
        //{
        //    // Arrange
        //    string testCategory = "invalidCategory";
        //    _mockPrefix.Setup(p => p.Value).Returns("prefix");

        //    // Act
        //    bool result = _peopleScoDictionaryNew.IsPeopleCategory(testCategory);

        //    // Assert
        //    Assert.IsFalse(result);
        //}


        //[TestMethod]
        //public void GetPeopleCatNames_ReturnsCategoryNames()
        //{
        //    // Arrange
        //    var categories = new List<Category>
        //    {
        //        MockCategory("prefixCategory1"),
        //        MockCategory("prefixCategory2"),
        //        MockCategory("otherCategory")
        //    };
        //    _mockGlobals.Setup(g => g.Ol.App.Session.Categories.Cast<Category>()).Returns(categories.Cast<Category>().AsQueryable());
        //    _mockPrefix.Setup(p => p.Value).Returns("prefix");

        //    // Act
        //    var result = _peopleScoDictionaryNew.GetPeopleCatNames();

        //    // Assert
        //    Assert.AreEqual(2, result.Count);
        //    Assert.IsTrue(result.Contains("prefixCategory1"));
        //    Assert.IsTrue(result.Contains("prefixCategory2"));
        //}

        //[TestMethod]
        //public void CategoryExists_CategoryExists_ReturnsTrue()
        //{
        //    // Arrange
        //    string category = "existingCategory";
        //    var categories = new List<Category>
        //    {
        //        MockCategory("existingCategory")
        //    };
        //    _mockGlobals.Setup(g => g.Ol.App.Session.Categories.Cast<Category>()).Returns(categories.Cast<Category>().AsQueryable());

        //    // Act
        //    bool result = _peopleScoDictionaryNew.CategoryExists(category);

        //    // Assert
        //    Assert.IsTrue(result);
        //}

        //[TestMethod]
        //public void CategoryExists_CategoryDoesNotExist_ReturnsFalse()
        //{
        //    // Arrange
        //    string category = "nonExistingCategory";
        //    var categories = new List<Category>
        //    {
        //        MockCategory("existingCategory")
        //    };
        //    _mockGlobals.Setup(g => g.Ol.App.Session.Categories.Cast<Category>()).Returns(categories.Cast<Category>().AsQueryable());

        //    // Act
        //    bool result = _peopleScoDictionaryNew.CategoryExists(category);

        //    // Assert
        //    Assert.IsFalse(result);
        //}

        //[TestMethod]
        //public void AddPrefix_ValidInputs_ReturnsPrefixedString()
        //{
        //    // Arrange
        //    string seed = "Category";
        //    string prefix = "prefix";

        //    // Act
        //    string result = _peopleScoDictionaryNew.AddPrefix(seed, prefix);

        //    // Assert
        //    Assert.AreEqual("prefixCategory", result);
        //}

        //[TestMethod]
        //[ExpectedException(typeof(ArgumentNullException))]
        //public void AddPrefix_NullSeed_ThrowsArgumentNullException()
        //{
        //    // Arrange
        //    string seed = null;
        //    string prefix = "prefix";

        //    // Act
        //    _peopleScoDictionaryNew.AddPrefix(seed, prefix);
        //}

        //[TestMethod]
        //[ExpectedException(typeof(ArgumentNullException))]
        //public void AddPrefix_NullPrefix_ThrowsArgumentNullException()
        //{
        //    // Arrange
        //    string seed = "Category";
        //    string prefix = null;

        //    // Act
        //    _peopleScoDictionaryNew.AddPrefix(seed, prefix);
        //}

        //[TestMethod]
        //public void SplitAddressToFirstLastName_ValidAddress_ReturnsFormattedName()
        //{
        //    // Arrange
        //    string address = "john.doe123@example.com";

        //    // Act
        //    string result = _peopleScoDictionaryNew.SplitAddressToFirstLastName(address);

        //    // Assert
        //    Assert.AreEqual("John Doe", result);
        //}

        //[TestMethod]
        //public void MatchToExisting_NoMatch_ReturnsNull()
        //{
        //    // Arrange
        //    var existingPeople = new List<string> { "John Doe", "Jane Smith" };
        //    string newPerson = "New Person";

        //    // Act
        //    string result = _peopleScoDictionaryNew.MatchToExisting(existingPeople, newPerson);

        //    // Assert
        //    Assert.IsNull(result);
        //}

        //[TestMethod]
        //public void MatchToExisting_MatchFound_ReturnsMatchedPerson()
        //{
        //    // Arrange
        //    var existingPeople = new List<string> { "John Doe", "Jane Smith" };
        //    string newPerson = "John Doe";

        //    // Act
        //    string result = _peopleScoDictionaryNew.MatchToExisting(existingPeople, newPerson);

        //    // Assert
        //    Assert.AreEqual("John Doe", result);
        //}


        [TestMethod]
        public void People_Deserialize_CanDeserializePatternCorrectly()
        {
            // Arrange
            string json = Encoding.UTF8.GetString(Resources.pplkey);
            var settings = GetSettings(_mockGlobals.Object);
            settings.Converters.Add(new ScoDictionaryConverter<PeopleScoDictionaryNew, string, string>());
            settings.TypeNameHandling = TypeNameHandling.None;
            var loader = new SmartSerializableNonTyped();


            //var loader = new SmartSerializableLoader(_mockGlobals.Object);
            //loader.Config.JsonSettings.Converters.Add(new ScoDictionaryConverter<PeopleScoDictionaryNew, string, string>());
            //loader.Config.JsonSettings.TypeNameHandling = TypeNameHandling.None;

            // Act
            //var obj = loader.DeserializeObject<PeopleScoDictionaryNew>(json, loader.Config.JsonSettings);
            var people = loader.DeserializeObject<PeopleScoDictionaryNew>(json, settings);

            // Assert
            Assert.IsNotNull(people, $"{nameof(people)} is null");
            //Assert.AreEqual(people.Globals, _mockGlobals.Object, $"{nameof(people)}.{nameof(people.Globals)} does not equal mock");
            Assert.IsNotNull(people.Config, $"{nameof(people)}.{nameof(people.Config)} is null" );
            Assert.AreEqual(people.Config.Disk.FileName, "pplkey.json");
        }

        [TestMethod]
        public void People_DeserializeShortcut_CanDeserializePatternCorrectly()
        {
            // Arrange
            string json = Encoding.UTF8.GetString(Resources.pplkey);
            var settings = GetSettings(_mockGlobals.Object);
            settings.Converters.Add(new PeopleScoConverter());
            //settings.TypeNameHandling = TypeNameHandling.None;
            var loader = new SmartSerializableNonTyped();

            //var loader = new SmartSerializableLoader(_mockGlobals.Object);
            //loader.Config.JsonSettings.Converters.Add(new ScoDictionaryConverter<PeopleScoDictionaryNew, string, string>());
            //loader.Config.JsonSettings.TypeNameHandling = TypeNameHandling.None;

            // Act
            //var obj = loader.DeserializeObject<PeopleScoDictionaryNew>(json, loader.Config.JsonSettings);
            var people = loader.DeserializeObject<PeopleScoDictionaryNew>(json, settings) as PeopleScoDictionaryNew;

            // Assert
            Assert.IsNotNull(people);
            //Assert.AreEqual(people.Globals, _mockGlobals.Object);
            Assert.IsNotNull(people.Config);
            Assert.AreEqual(people.Config.Disk.FileName, "pplkey.json");

        }
    }
}
