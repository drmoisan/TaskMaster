using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using ToDoModel.Data_Model.People;
using UtilitiesCS.NewtonsoftHelpers;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Dictionary;

namespace UtilitiesCS.Test.NewtonsoftHelpers
{
    [TestClass]
    public class PeopleScoConverter_Tests
    {
        private MockRepository mockRepository;
        private Mock<Microsoft.Office.Interop.Outlook.Application> mockApplication;
        private IApplicationGlobals globals;

        [TestInitialize]
        public void TestInitialize()
        {
            Console.SetOut(new DebugTextWriter());
            mockRepository = new MockRepository(MockBehavior.Loose);
            mockApplication = mockRepository.Create<Microsoft.Office.Interop.Outlook.Application>();
            globals = new TaskMaster.ApplicationGlobals(
                mockApplication.Object,
                true,
                variable => variable == "OneDriveCommercial" ? @"C:\OneDrive" : null
            );
        }

        [TestMethod]
        public void Constructor_DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var converter = new PeopleScoConverter();

            // Assert
            converter.Should().NotBeNull();
        }

        [TestMethod]
        public void CanRead_Default_ReturnsBaseCanRead()
        {
            // Arrange
            var converter = new PeopleScoConverter();

            // Act
            var result = converter.CanRead;

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void WriteJson_ValidPeopleScoDictionary_ProducesJson()
        {
            // Arrange
            var converter = new PeopleScoConverter();
            var dict = new PeopleScoDictionaryNew();
            dict.TryAdd("person1", "info1");
            dict.TryAdd("person2", "info2");

            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = { converter },
            };

            // Act
            var json = JsonConvert.SerializeObject(dict, settings);

            // Assert
            json.Should().NotBeNullOrEmpty();
            json.Should().Contain("person1");
            json.Should().Contain("person2");
        }
    }
}
