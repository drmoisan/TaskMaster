using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS.OutlookExtensions;
using Outlook = Microsoft.Office.Interop.Outlook;
using Exception = System.Exception;

namespace UtilitiesCS.Test.OutlookObjects.Table
{
    [TestClass]
    public class OlToDoTable_Tests
    {
        #region GetToDoTable

        [TestMethod]
        public void GetToDoTable_StoreThrowsOnGetDefaultFolder_ReturnsNull()
        {
            var mockStore = new Mock<Outlook.Store>();
            mockStore.Setup(s => s.GetDefaultFolder(It.IsAny<OlDefaultFolders>()))
                .Throws(new Exception("not available"));

            var result = OlToDoTable.GetToDoTable(mockStore.Object);
            result.Should().BeNull();
        }

        [TestMethod]
        public void GetToDoTable_ValidStore_ReturnsTableWithColumns()
        {
            var mockStore = new Mock<Outlook.Store>();
            var mockFolder = new Mock<MAPIFolder>();
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            var mockItems = new Mock<Items>();
            var mockUserProps = new Mock<UserDefinedProperties>();

            mockStore.Setup(s => s.GetDefaultFolder(OlDefaultFolders.olFolderToDo))
                .Returns(mockFolder.Object);
            mockFolder.Setup(f => f.GetTable()).Returns(mockTable.Object);
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);
            mockFolder.Setup(f => f.UserDefinedProperties).Returns(mockUserProps.Object);
            mockFolder.Setup(f => f.Items).Returns(mockItems.Object);
            mockItems.Setup(i => i.Count).Returns(0);

            // UserDefinedProperties[] throws to simulate field not found
            mockUserProps.Setup(u => u[It.IsAny<object>()]).Throws(new Exception("not found"));

            var result = OlToDoTable.GetToDoTable(mockStore.Object);
            result.Should().BeSameAs(mockTable.Object);
            mockColumns.Verify(c => c.RemoveAll(), Times.Once);
            mockColumns.Verify(c => c.Add(It.IsAny<string>()), Times.AtLeast(2));
        }

        [TestMethod]
        public void GetToDoTable_FolderWithExistingField_ReturnsTable()
        {
            var mockStore = new Mock<Outlook.Store>();
            var mockFolder = new Mock<MAPIFolder>();
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            var mockItems = new Mock<Items>();
            var mockUserProps = new Mock<UserDefinedProperties>();
            var mockField = new Mock<UserDefinedProperty>();

            mockStore.Setup(s => s.GetDefaultFolder(OlDefaultFolders.olFolderToDo))
                .Returns(mockFolder.Object);
            mockFolder.Setup(f => f.GetTable()).Returns(mockTable.Object);
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);
            mockFolder.Setup(f => f.UserDefinedProperties).Returns(mockUserProps.Object);
            mockFolder.Setup(f => f.Items).Returns(mockItems.Object);
            mockItems.Setup(i => i.Count).Returns(0);
            mockUserProps.Setup(u => u["ToDoID"]).Returns(mockField.Object);

            var result = OlToDoTable.GetToDoTable(mockStore.Object);
            result.Should().BeSameAs(mockTable.Object);
        }

        #endregion
    }
}
