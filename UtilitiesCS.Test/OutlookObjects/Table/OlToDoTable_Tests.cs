using System;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookExtensions;
using Exception = System.Exception;
using Outlook = Microsoft.Office.Interop.Outlook;

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
            mockStore
                .Setup(s => s.GetDefaultFolder(It.IsAny<OlDefaultFolders>()))
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

            mockStore
                .Setup(s => s.GetDefaultFolder(OlDefaultFolders.olFolderToDo))
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

            mockStore
                .Setup(s => s.GetDefaultFolder(OlDefaultFolders.olFolderToDo))
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

        [TestMethod]
        public void GetToDoTable_ItemWithEmptyEntryId_SkipsItem()
        {
            var mockStore = new Mock<Outlook.Store>();
            var mockFolder = new Mock<MAPIFolder>();
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            var mockItems = new Mock<Items>();
            var mockUserProps = new Mock<UserDefinedProperties>();

            mockStore
                .Setup(s => s.GetDefaultFolder(OlDefaultFolders.olFolderToDo))
                .Returns(mockFolder.Object);
            mockFolder.Setup(f => f.GetTable()).Returns(mockTable.Object);
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);
            mockFolder.Setup(f => f.UserDefinedProperties).Returns(mockUserProps.Object);
            mockUserProps.Setup(u => u[It.IsAny<object>()]).Throws(new Exception("not found"));

            // Item with empty EntryID
            var mockItem = new Mock<Outlook.MailItem>();
            mockItem.Setup(m => m.EntryID).Returns(string.Empty);
            var itemsList = new System.Collections.Generic.List<object> { mockItem.Object };

            mockFolder.Setup(f => f.Items).Returns(mockItems.Object);
            mockItems.Setup(i => i.Count).Returns(1);
            mockItems.Setup(i => i[1]).Returns(mockItem.Object);

            var result = OlToDoTable.GetToDoTable(mockStore.Object);
            result.Should().BeSameAs(mockTable.Object);
        }

        [TestMethod]
        public void GetToDoTable_ItemThrowsOnAccess_ContinuesToNextItem()
        {
            var mockStore = new Mock<Outlook.Store>();
            var mockFolder = new Mock<MAPIFolder>();
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            var mockItems = new Mock<Items>();
            var mockUserProps = new Mock<UserDefinedProperties>();

            mockStore
                .Setup(s => s.GetDefaultFolder(OlDefaultFolders.olFolderToDo))
                .Returns(mockFolder.Object);
            mockFolder.Setup(f => f.GetTable()).Returns(mockTable.Object);
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);
            mockFolder.Setup(f => f.UserDefinedProperties).Returns(mockUserProps.Object);
            mockUserProps.Setup(u => u[It.IsAny<object>()]).Throws(new Exception("not found"));

            mockFolder.Setup(f => f.Items).Returns(mockItems.Object);
            mockItems.Setup(i => i.Count).Returns(1);
            mockItems.Setup(i => i[1]).Returns((object)null);

            var result = OlToDoTable.GetToDoTable(mockStore.Object);
            result.Should().BeSameAs(mockTable.Object);
        }

        [TestMethod]
        public void GetToDoTable_UserPropsAddThrows_StillReturnsTable()
        {
            var mockStore = new Mock<Outlook.Store>();
            var mockFolder = new Mock<MAPIFolder>();
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            var mockItems = new Mock<Items>();
            var mockUserProps = new Mock<UserDefinedProperties>();

            mockStore
                .Setup(s => s.GetDefaultFolder(OlDefaultFolders.olFolderToDo))
                .Returns(mockFolder.Object);
            mockFolder.Setup(f => f.GetTable()).Returns(mockTable.Object);
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);
            mockFolder.Setup(f => f.UserDefinedProperties).Returns(mockUserProps.Object);
            mockFolder.Setup(f => f.Items).Returns(mockItems.Object);
            mockItems.Setup(i => i.Count).Returns(0);
            // Field not found, and Add throws
            mockUserProps.Setup(u => u[It.IsAny<object>()]).Throws(new Exception("not found"));
            mockUserProps
                .Setup(u =>
                    u.Add(
                        It.IsAny<string>(),
                        It.IsAny<OlUserPropertyType>(),
                        It.IsAny<object>(),
                        It.IsAny<object>()
                    )
                )
                .Throws(new Exception("provider limitation"));

            var result = OlToDoTable.GetToDoTable(mockStore.Object);
            result.Should().BeSameAs(mockTable.Object);
        }

        #endregion
    }
}
