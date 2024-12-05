using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using ToDoModel;
using UtilitiesCS;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS.OutlookExtensions;

namespace ToDoModel.Test
{
    [TestClass]
    public class ToDoItemTests
    {
        private Mock<MailItem> mockMailItem;
        private Mock<IOutlookItem> mockOutlookItem;
        private Mock<OutlookItemFlaggable> mockFlaggableItem;

        /// <summary>
        /// Sets up the mock objects before each test.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            mockMailItem = new Mock<MailItem>(MockBehavior.Strict);
            mockOutlookItem = new Mock<IOutlookItem>(MockBehavior.Strict);
            mockFlaggableItem = new Mock<OutlookItemFlaggable>(MockBehavior.Strict, mockOutlookItem.Object);
        }

        /// <summary>
        /// Verifies that the constructors initialize the properties correctly.
        /// </summary>
        [TestMethod]
        public void Constructor_WithOutlookItem_ShouldInitializeProperties()
        {
            // Arrange
            var timestamp = DateTime.Now;
            mockMailItem.SetupGet(x => x.TaskSubject).Returns("Test Task");
            mockMailItem.SetupGet(x => x.Importance).Returns(OlImportance.olImportanceNormal);
            mockMailItem.SetupGet(x => x.CreationTime).Returns(timestamp);
            mockMailItem.SetupGet(x => x.TaskStartDate).Returns(timestamp);
            mockOutlookItem.SetupGet(x => x.InnerObject).Returns(mockMailItem.Object);
            mockOutlookItem.SetupGet(x => x.Categories).Returns("Category1,Category2");
            object[] args = new object[] { };
            mockOutlookItem.SetupGet(x => x.Args).Returns(args);

            // Act
            var toDoItem = new ToDoItem(mockOutlookItem.Object);

            // Assert
            var flaggableItem = typeof(ToDoItem).GetProperty("FlaggableItem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(toDoItem);
            Assert.IsNotNull(flaggableItem);
            Assert.IsNotNull(toDoItem.Flags);
            Assert.AreEqual("Test Task", toDoItem.TaskSubject);
            Assert.AreEqual(OlImportance.olImportanceNormal, toDoItem.Priority);
            Assert.AreEqual(timestamp, toDoItem.TaskCreateDate);
            Assert.AreEqual(timestamp, toDoItem.StartDate);
        }

        [TestMethod]
        public void Constructor_WithOutlookItemAndOnDemand_ShouldNotInitializeProperties()
        {
            // Arrange
            mockOutlookItem.SetupGet(x => x.Categories).Returns("Category1,Category2");

            // Act
            var toDoItem = new ToDoItem(mockOutlookItem.Object, true);

            // Assert
            var flaggableItem = typeof(ToDoItem).GetProperty("FlaggableItem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(toDoItem);
            Assert.IsNotNull(flaggableItem);
            Assert.IsNull(toDoItem.Flags);
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

        [TestMethod]
        public async Task ForceSave_ShouldSaveFlaggableItem()
        {
            // Arrange
            var toDoItem = new ToDoItem(mockOutlookItem.Object);
            mockFlaggableItem.Setup(x => x.Save()).Verifiable();

            // Act
            await toDoItem.ForceSave();

            // Assert
            mockFlaggableItem.Verify(x => x.Save(), Times.Once);
        }

        [TestMethod]
        public void SetAndGetProperties_ShouldWorkCorrectly()
        {
            // Arrange
            var toDoItem = new ToDoItem(mockOutlookItem.Object);
            string taskSubject = "Test Task";
            DateTime startDate = DateTime.Now;
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

        [TestMethod]
        public void Clone_ShouldReturnDeepCopy()
        {
            // Arrange
            var toDoItem = new ToDoItem(mockOutlookItem.Object);

            // Act
            var clone = toDoItem.Clone() as ToDoItem;

            // Assert
            Assert.IsNotNull(clone);
            Assert.AreNotSame(toDoItem, clone);
        }

        [TestMethod]
        public async Task WriteFlagsBatch_ShouldUpdateFlaggableItemCategories()
        {
            // Arrange
            var toDoItem = new ToDoItem(mockOutlookItem.Object);
            mockFlaggableItem.Setup(x => x.Save()).Verifiable();

            // Act
            await toDoItem.WriteFlagsBatch();

            // Assert
            mockFlaggableItem.Verify(x => x.Save(), Times.Once);
        }
    }
}
