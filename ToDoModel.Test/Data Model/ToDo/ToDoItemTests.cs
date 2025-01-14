using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using ToDoModel;
using UtilitiesCS;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS.OutlookExtensions;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using FluentAssertions;

namespace ToDoModel.Test
{
    [TestClass]
    public class ToDoItemTests
    {
        private Mock<MailItem> mockMailItem;
        private Mock<IOutlookItem> mockOutlookItem;
        //private Mock<IOutlookItemFlaggable> mockFlaggableItem;

        /// <summary>
        /// Sets up the mock objects before each test.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            mockMailItem = new Mock<MailItem>(MockBehavior.Strict);
            mockOutlookItem = new Mock<IOutlookItem>(MockBehavior.Strict);
            //mockFlaggableItem = new Mock<IOutlookItemFlaggable>(MockBehavior.Strict);
        }
                
        private ToDoModel.Test.Data_Model.ToDo.SpecialMockMail CreateSpecialMockMail(DateTime timestamp)
        {
            var mock = new ToDoModel.Test.Data_Model.ToDo.SpecialMockMail
            {
                TaskSubject = "Test Task",
                Importance = OlImportance.olImportanceNormal,

                CreationTime = timestamp,
                TaskStartDate = timestamp,
                Categories = "Category1,Category2"
            };
            var mockUserProps = GetMockUserProperties();
            mock.UserProperties = mockUserProps.Object;

            return mock;
        }
                
        private Mock<UserProperties> GetMockUserProperties()
        {
            var mock = new Mock<UserProperties>(MockBehavior.Loose);

            var userProperties = GetUserPropertyCollection().ToList();

            // Set up the Find method
            mock.Setup(x => x.Find(It.IsAny<string>(), It.IsAny<object>()))
                .Returns((string name, object custom) => userProperties.FirstOrDefault(p => p.Name == name));

            // Set up the IEnumerable implementation
            mock.As<IEnumerable>().Setup(x => x.GetEnumerator()).Returns(userProperties.GetEnumerator());

            // Set up the Count property
            mock.Setup(x => x.Count).Returns(userProperties.Count);

            return mock;
        }

        internal IEnumerable<UserProperty> GetUserPropertyCollection()
        {
            UserProperty TagProgram = MockProperty<string>("TagProgram", "TestProgram", OlUserPropertyType.olText);
            UserProperty AB = MockProperty<bool>("AB", true, OlUserPropertyType.olYesNo);
            UserProperty EC2 = MockProperty<bool>("EC2", true, OlUserPropertyType.olYesNo);
            UserProperty EC = MockProperty<string>("EC", "EcVal", OlUserPropertyType.olText);
            UserProperty EcState = MockProperty<string>("EcState", "EcStateVal", OlUserPropertyType.olText);

            var list = new List<UserProperty> { TagProgram, AB, EC2, EC, EcState };
            return list;
        }

        public UserProperty MockProperty<T>(string propertyName, T value, OlUserPropertyType olPropertyType = OlUserPropertyType.olText)
        {
            var mockUser = new Mock<UserProperty>();
            mockUser.Setup(x => x.Name).Returns(propertyName);
            mockUser.Setup(x => x.Type).Returns(olPropertyType);
            mockUser.Setup(x => x.Value).Returns(value);
            return mockUser.Object;
        }

        /// <summary>
        /// Verifies that the constructors initialize the properties correctly.
        /// </summary>
        [TestMethod]
        public void Constructor_WithOutlookItem_ShouldInitializeProperties()
        {
            // Arrange
            var timestamp = DateTime.Now;
            var mockMail = CreateSpecialMockMail(timestamp);           

            var outlookItem = new OutlookItem(mockMail);
            
            // Act
            var toDoItem = new ToDoItem(outlookItem);            

            // Assert
            var flaggableItem = typeof(ToDoItem).GetProperty("FlaggableItem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(toDoItem);
            Assert.IsNotNull(flaggableItem);
            Assert.IsNotNull(toDoItem.Flags);
            Assert.AreEqual("Test Task", toDoItem.TaskSubject);
            Assert.AreEqual(OlImportance.olImportanceNormal, toDoItem.Priority);
            Assert.AreEqual(timestamp, toDoItem.TaskCreateDate);
            Assert.AreEqual(timestamp, toDoItem.StartDate);
            Assert.AreEqual("Category1,Category2", toDoItem.FlaggableItem.Categories);
            //Assert.AreEqual("TestProgram", toDoItem.Program.AsStringNoPrefix);
            Assert.IsTrue(toDoItem.ActiveBranch);
            Assert.IsTrue(toDoItem.EC2);
            Assert.AreEqual("EcVal", toDoItem.ExpandChildren);
            Assert.AreEqual("EcStateVal", toDoItem.ExpandChildrenState);
        }
                
        [TestMethod]
        public void Constructor_WithOutlookItemAndOnDemand_ShouldNotInitializeProperties()
        {
            // Arrange
            mockMailItem.SetupGet(x => x.Categories).Returns("Category1,Category2");
            mockOutlookItem.SetupGet(x => x.InnerObject).Returns(mockMailItem.Object);
            mockOutlookItem.SetupGet(x => x.Args).Returns(new object[0]);

            // Act
            var toDoItem = new ToDoItem(mockOutlookItem.Object, true);

            // Assert
            
            // Verify the flaggableItem was initialized
            var flaggableItem = typeof(ToDoItem).GetProperty("FlaggableItem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(toDoItem);
            Assert.IsNotNull(flaggableItem);

            // Verify that flags was not initialized
            var flags = typeof(ToDoItem).GetField("_flags", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(toDoItem);
            Assert.IsNull(flags);

            // Verify that flags could have been initialized but was not
            _ = toDoItem.Flags;
            flags = typeof(ToDoItem).GetField("_flags", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(toDoItem);
            Assert.IsNotNull(flags);
        }

        [TestMethod]
        public void Constructor_WithString_ShouldInitializeToDoID()
        {
            // Arrange
            string toDoID = "12345";

            // Act
            var toDoItem = new ToDoItem(toDoID);

            // Assert
            Assert.AreEqual(toDoID, toDoItem.ToDoID);
        }

        //[TestMethod]
        //public async Task ForceSave_ShouldSaveFlaggableItem()
        //{
        //    // Arrange
        //    var timestamp = DateTime.Now;
        //    var mockMail = CreateSpecialMockMail(timestamp);
        //    var outlookItem = new OutlookItem(mockMail);

        //    var mockFlaggableItem = new Mock<IOutlookItemFlaggable>(MockBehavior.Strict);
        //    mockFlaggableItem.SetupAllProperties();

        //    // Set up properties to reference outlookItem
        //    mockFlaggableItem.SetupGet(x => x.BillingInformation).Returns(outlookItem.BillingInformation);
        //    mockFlaggableItem.SetupGet(x => x.Body).Returns(outlookItem.Body);
        //    mockFlaggableItem.SetupGet(x => x.Categories).Returns(outlookItem.Categories);
        //    mockFlaggableItem.SetupGet(x => x.Companies).Returns(outlookItem.Companies);
        //    mockFlaggableItem.SetupGet(x => x.EntryID).Returns("474747");
        //    mockFlaggableItem.SetupGet(x => x.Importance).Returns(outlookItem.Importance);
        //    mockFlaggableItem.SetupGet(x => x.ItemProperties).Returns(outlookItem.ItemProperties);
        //    mockFlaggableItem.SetupGet(x => x.MarkForDownload).Returns(outlookItem.MarkForDownload);
        //    mockFlaggableItem.SetupGet(x => x.MessageClass).Returns(outlookItem.MessageClass);
        //    mockFlaggableItem.SetupGet(x => x.Mileage).Returns(outlookItem.Mileage);
        //    mockFlaggableItem.SetupGet(x => x.NoAging).Returns(outlookItem.NoAging);
        //    //mockFlaggableItem.SetupGet(x => x.OutlookInternalVersion).Returns(outlookItem.OutlookInternalVersion);
        //    mockFlaggableItem.SetupGet(x => x.OutlookVersion).Returns(outlookItem.OutlookVersion);
        //    mockFlaggableItem.SetupGet(x => x.ReminderTime).Returns(outlookItem.ReminderTime);
        //    mockFlaggableItem.SetupGet(x => x.Sensitivity).Returns(outlookItem.Sensitivity);
        //    mockFlaggableItem.SetupGet(x => x.Subject).Returns(outlookItem.Subject);
        //    mockFlaggableItem.SetupGet(x => x.UnRead).Returns(outlookItem.UnRead);
        //    mockFlaggableItem.SetupGet(x => x.UserProperties).Returns(outlookItem.UserProperties);

        //    // Set up additional properties specific to IOutlookItemFlaggable
        //    mockFlaggableItem.SetupGet(x => x.Complete).Returns(false);
        //    mockFlaggableItem.SetupGet(x => x.DueDate).Returns(DateTime.MinValue);
        //    mockFlaggableItem.SetupGet(x => x.FlagAsTask).Returns(false);
        //    mockFlaggableItem.SetupGet(x => x.TaskSubject).Returns(outlookItem.Subject);
        //    mockFlaggableItem.SetupGet(x => x.TotalWork).Returns(47);

        //    // Set up methods to reference outlookItem
        //    mockFlaggableItem.Setup(x => x.Close(It.IsAny<OlInspectorClose>())).Callback<OlInspectorClose>(outlookItem.Close);
        //    mockFlaggableItem.Setup(x => x.Copy()).Returns(outlookItem.Copy);
        //    mockFlaggableItem.Setup(x => x.Display()).Callback(outlookItem.Display);
        //    mockFlaggableItem.Setup(x => x.Move(It.IsAny<Folder>())).Returns<Folder>(outlookItem.Move);
        //    mockFlaggableItem.Setup(x => x.PrintOut()).Callback(outlookItem.PrintOut);
        //    //mockFlaggableItem.Setup(x => x.Save()).Callback(outlookItem.Save);
        //    mockFlaggableItem.Setup(x => x.SaveAs(It.IsAny<string>(), It.IsAny<OlSaveAsType>())).Callback<string, OlSaveAsType>(outlookItem.SaveAs);
        //    mockFlaggableItem.Setup(x => x.ShowCategoriesDialog()).Callback(outlookItem.ShowCategoriesDialog);

        //    mockFlaggableItem.Setup(x => x.Save()).Verifiable();            
        //    var toDoItem = new ToDoItem(mockFlaggableItem.Object);

        //    // Act
        //    await toDoItem.ForceSave();

        //    // Assert
        //    mockFlaggableItem.Verify(x => x.Save(), Times.Once);
        //}

        [TestMethod]
        public void SetAndGetProperties_ShouldWorkCorrectly()
        {
            // Arrange
            var timestamp = DateTime.Now;
            var mockMail = CreateSpecialMockMail(timestamp);
            var outlookItem = new OutlookItem(mockMail);
            var toDoItem = new ToDoItem(outlookItem);
            string taskSubject = "Test Task";
            DateTime startDate = timestamp;
            OlImportance priority = OlImportance.olImportanceHigh;

            // Act
            toDoItem.TaskSubject = taskSubject;
            toDoItem.StartDate = startDate;
            toDoItem.Priority = priority;

            // Assert
            Assert.AreEqual(taskSubject, toDoItem.TaskSubject);
            Assert.AreEqual(startDate, toDoItem.StartDate);
            Assert.AreEqual(priority, toDoItem.Priority);
        }

        //[TestMethod]
        //public void Clone_ShouldReturnDeepCopy()
        //{
        //    // Arrange
        //    var timestamp = DateTime.Now;
        //    var mockMail = CreateSpecialMockMail(timestamp);
        //    var outlookItem = new OutlookItem(mockMail);
        //    var toDoItem = new ToDoItem(outlookItem);

        //    // Act
        //    var clone = toDoItem.Clone() as ToDoItem;

        //    // Assert
        //    Assert.IsNotNull(clone);
        //    Assert.AreNotSame(toDoItem, clone);
        //    toDoItem.Should().BeEquivalentTo(clone);
        //}

        //[TestMethod]
        //public async Task WriteFlagsBatch_ShouldUpdateFlaggableItemCategories()
        //{
        //    // Arrange
        //    var toDoItem = new ToDoItem(mockOutlookItem.Object);
        //    mockFlaggableItem.Setup(x => x.Save()).Verifiable();

        //    // Act
        //    await toDoItem.WriteFlagsBatch();

        //    // Assert
        //    mockFlaggableItem.Verify(x => x.Save(), Times.Once);
        //}
    }
}
