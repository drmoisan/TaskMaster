using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Store;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;
using OutlookStore = Microsoft.Office.Interop.Outlook.Store;

namespace ToDoModel.Test
{
    /// <summary>
    /// Verifies that the three store-enumerating surfaces route store inclusion through the shared
    /// <see cref="StoresWrapper.ShouldIncludeStore(OutlookStore)"/> predicate (issue #328):
    /// <see cref="TreeOfToDoItems.GetToDoList"/>, <see cref="TreeOfToDoItems.GetToDoListAsync"/>, and
    /// <see cref="ToDoEvents.GetAsyncEnumerableOfToDoItemsInView"/>. A StoreID-excluded store must
    /// never have its default ToDo folder resolved. All Outlook boundaries are mocked with Moq; no
    /// live COM and no temporary files are used.
    /// </summary>
    [TestClass]
    public class StoreFilterRoutingTests
    {
        private const string KeepStoreId = "KEEP-STORE-ID";
        private const string DropStoreId = "DROP-STORE-ID";

        [TestMethod]
        public void GetToDoList_WhenStoreIdExcluded_DoesNotResolveExcludedStoreFolder()
        {
            // Arrange
            var keep = CreateStoreMock(KeepStoreId, "Keep Mailbox");
            var drop = CreateStoreMock(DropStoreId, "Drop Mailbox");
            var app = CreateAppWithStores(keep.Object, drop.Object);
            var wrapper = ExcludingWrapper(DropStoreId);
            var tree = new TreeOfToDoItems();

            // Act
            var result = tree.GetToDoList(
                TreeOfToDoItems.LoadOptions.vbLoadAll,
                app.Object,
                wrapper
            );

            // Assert
            result.Should().NotBeNull();
            keep.Verify(
                x => x.GetDefaultFolder(OlDefaultFolders.olFolderToDo),
                Times.Once(),
                "the included store must be enumerated"
            );
            drop.Verify(
                x => x.GetDefaultFolder(It.IsAny<OlDefaultFolders>()),
                Times.Never(),
                "the StoreID-excluded store must be filtered out before its folder is resolved"
            );
        }

        [TestMethod]
        public async Task GetToDoListAsync_WhenStoreIdExcluded_DoesNotResolveExcludedStoreFolder()
        {
            // Arrange
            var keep = CreateStoreMock(KeepStoreId, "Keep Mailbox");
            var drop = CreateStoreMock(DropStoreId, "Drop Mailbox");
            var app = CreateAppWithStores(keep.Object, drop.Object);
            var wrapper = ExcludingWrapper(DropStoreId);
            var tree = new TreeOfToDoItems();

            // Act
            var enumerable = tree.GetToDoListAsync(
                TreeOfToDoItems.LoadOptions.vbLoadAll,
                app.Object,
                wrapper
            );
            await DrainAsync(enumerable);

            // Assert
            keep.Verify(
                x => x.GetDefaultFolder(OlDefaultFolders.olFolderToDo),
                Times.Once(),
                "the included store must be enumerated"
            );
            drop.Verify(
                x => x.GetDefaultFolder(It.IsAny<OlDefaultFolders>()),
                Times.Never(),
                "the StoreID-excluded store must be filtered out before its folder is resolved"
            );
        }

        [TestMethod]
        public async Task GetAsyncEnumerableOfToDoItemsInView_WhenStoreIdExcluded_DoesNotResolveExcludedStoreFolder()
        {
            // Arrange
            var keep = CreateStoreMock(KeepStoreId, "Keep Mailbox");
            var drop = CreateStoreMock(DropStoreId, "Drop Mailbox");
            var app = CreateAppWithStores(keep.Object, drop.Object);
            var wrapper = ExcludingWrapper(DropStoreId);
            var globals = CreateGlobals(app.Object, wrapper);

            // Act
            var enumerable = ToDoEvents.GetAsyncEnumerableOfToDoItemsInView(globals.Object);
            await DrainAsync(enumerable);

            // Assert
            keep.Verify(
                x => x.GetDefaultFolder(OlDefaultFolders.olFolderToDo),
                Times.Once(),
                "the included store must be enumerated"
            );
            drop.Verify(
                x => x.GetDefaultFolder(It.IsAny<OlDefaultFolders>()),
                Times.Never(),
                "the StoreID-excluded store must be filtered out before its folder is resolved"
            );
        }

        private static StoresWrapper ExcludingWrapper(string excludedStoreId)
        {
            return new StoresWrapper
            {
                ExcludedStoreIds = new List<string> { excludedStoreId },
                ExcludedStoreNameContains = new List<string>(),
                ExcludedStoreFilePathContains = new List<string>(),
                GwsoFilePathContains = new List<string>(),
                ExcludeGwsoStores = false,
                ExcludePublicFolderStores = false,
            };
        }

        private static Mock<OutlookStore> CreateStoreMock(string storeId, string displayName)
        {
            var store = new Mock<OutlookStore>();
            store.SetupGet(x => x.StoreID).Returns(storeId);
            store.SetupGet(x => x.DisplayName).Returns(displayName);
            store
                .SetupGet(x => x.ExchangeStoreType)
                .Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            store.SetupGet(x => x.FilePath).Returns(@"C:\Data\" + storeId + ".ost");
            store
                .Setup(x => x.GetDefaultFolder(OlDefaultFolders.olFolderToDo))
                .Returns(CreateEmptyFolder().Object);
            return store;
        }

        private static Mock<OutlookFolder> CreateEmptyFolder()
        {
            var items = new Mock<Items>();
            items
                .As<IEnumerable>()
                .Setup(x => x.GetEnumerator())
                .Returns(() => Enumerable.Empty<object>().GetEnumerator());

            var folder = new Mock<OutlookFolder>();
            folder.SetupGet(x => x.Items).Returns(items.Object);
            return folder;
        }

        private static Mock<Application> CreateAppWithStores(params OutlookStore[] stores)
        {
            var storesCollection = new Mock<Stores>();
            storesCollection
                .As<IEnumerable>()
                .Setup(x => x.GetEnumerator())
                .Returns(() => stores.Cast<object>().GetEnumerator());

            var nameSpace = new Mock<NameSpace>();
            nameSpace.SetupGet(x => x.Stores).Returns(storesCollection.Object);

            var view = new Mock<View>();
            view.SetupGet(x => x.Filter).Returns(string.Empty);

            var explorer = new Mock<Explorer>();
            explorer.SetupGet(x => x.CurrentView).Returns(view.Object);

            var app = new Mock<Application>();
            app.SetupGet(x => x.Session).Returns(nameSpace.Object);
            app.Setup(x => x.ActiveExplorer()).Returns(explorer.Object);
            return app;
        }

        private static Mock<IApplicationGlobals> CreateGlobals(
            Application app,
            StoresWrapper wrapper
        )
        {
            var olObjects = new Mock<IOlObjects>();
            olObjects.SetupGet(x => x.App).Returns(app);
            olObjects.SetupGet(x => x.StoresWrapper).Returns(wrapper);

            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(x => x.Ol).Returns(olObjects.Object);
            return globals;
        }

        private static async Task DrainAsync(IAsyncEnumerable<object> source)
        {
            var enumerator = source.GetAsyncEnumerator();
            try
            {
                while (await enumerator.MoveNextAsync()) { }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }
        }
    }
}
