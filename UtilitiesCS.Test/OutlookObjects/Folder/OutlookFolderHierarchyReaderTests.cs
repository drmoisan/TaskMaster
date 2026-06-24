using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.OutlookObjects.Store;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public sealed class OutlookFolderHierarchyReaderTests
    {
        [TestMethod]
        public void ReadRecords_IncludedStore_ReadsPrimitiveRootMetadata()
        {
            var folder = CreateFolder("entry-a", "Inbox", "\\Inbox");
            var store = CreateStore("store-a", include: true, folder.Object);
            var reader = new OutlookFolderHierarchyReader(
                () => new[] { store.Object },
                new StoresWrapper { ExcludedStoreNameContains = new List<string>() }
            );

            var records = reader.ReadRecords(FolderTreeRequest.AllStores(false), default);

            records.Should().ContainSingle();
            records[0].StoreId.Should().Be("store-a");
            records[0].EntryId.Should().Be("entry-a");
            records[0].DisplayName.Should().Be("Inbox");
            records[0].FolderPath.Should().Be("\\Inbox");
        }

        [TestMethod]
        public void ReadRecords_ExcludedStore_DoesNotReadRootFolder()
        {
            var folder = CreateFolder("entry-a", "Inbox", "\\Inbox");
            var store = CreateStore("store-a", include: false, folder.Object);
            var reader = new OutlookFolderHierarchyReader(
                () => new[] { store.Object },
                new StoresWrapper { ExcludedStoreNameContains = new List<string> { "Archive" } }
            );

            var records = reader.ReadRecords(FolderTreeRequest.AllStores(false), default);

            records.Should().BeEmpty();
            store.Verify(item => item.GetRootFolder(), Times.Never);
        }

        [TestMethod]
        public void HierarchyRecord_TrimsRequiredValuesAndCreatesKey()
        {
            var record = new OutlookFolderHierarchyRecord(
                " store-a ",
                " entry-a ",
                null,
                " Inbox ",
                " \\Inbox ",
                null
            );

            record.StoreId.Should().Be("store-a");
            record.EntryId.Should().Be("entry-a");
            record.ParentEntryId.Should().BeEmpty();
            record.DisplayName.Should().Be("Inbox");
            record.FolderPath.Should().Be("\\Inbox");
            record.RelativePath.Should().BeEmpty();
            record.Key.FolderPath.Should().Be("\\Inbox");
        }

        [TestMethod]
        public void HierarchyRecord_BlankRequiredValues_Throw()
        {
            Action blankStore = () =>
                new OutlookFolderHierarchyRecord(" ", "entry-a", "", "Inbox", "\\Inbox", "Inbox");
            Action blankEntry = () =>
                new OutlookFolderHierarchyRecord("store-a", " ", "", "Inbox", "\\Inbox", "Inbox");
            Action blankFolderPath = () =>
                new OutlookFolderHierarchyRecord("store-a", "entry-a", "", "Inbox", " ", "Inbox");

            blankStore.Should().Throw<ArgumentException>().WithParameterName("storeId");
            blankEntry.Should().Throw<ArgumentException>().WithParameterName("entryId");
            blankFolderPath.Should().Throw<ArgumentException>().WithParameterName("folderPath");
        }

        private static Mock<OutlookFolderHierarchyReader.IOutlookStoreAdapter> CreateStore(
            string storeId,
            bool include,
            OutlookFolderHierarchyReader.IOutlookFolderAdapter root
        )
        {
            var store = new Mock<OutlookFolderHierarchyReader.IOutlookStoreAdapter>();
            store.SetupGet(item => item.StoreId).Returns(storeId);
            store.Setup(item => item.ShouldInclude(It.IsAny<StoresWrapper>())).Returns(include);
            store.Setup(item => item.GetRootFolder()).Returns(root);
            return store;
        }

        private static Mock<OutlookFolderHierarchyReader.IOutlookFolderAdapter> CreateFolder(
            string entryId,
            string name,
            string path
        )
        {
            var folder = new Mock<OutlookFolderHierarchyReader.IOutlookFolderAdapter>();
            folder.SetupGet(item => item.EntryID).Returns(entryId);
            folder.SetupGet(item => item.Name).Returns(name);
            folder.SetupGet(item => item.FolderPath).Returns(path);
            folder
                .SetupGet(item => item.Children)
                .Returns(new OutlookFolderHierarchyReader.IOutlookFolderAdapter[0]);
            return folder;
        }
    }
}
