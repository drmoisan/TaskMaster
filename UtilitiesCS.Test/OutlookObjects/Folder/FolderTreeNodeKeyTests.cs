using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public sealed class FolderTreeNodeKeyTests
    {
        [TestMethod]
        public void Equals_SameStoreEntryAndPath_ReturnsTrue()
        {
            var first = new FolderTreeNodeKey("store-a", "entry-1", "\\Inbox\\Archive");
            var second = new FolderTreeNodeKey("STORE-A", "entry-1", "\\Inbox\\Archive");

            first.Should().Be(second);
            first.GetHashCode().Should().Be(second.GetHashCode());
        }

        [TestMethod]
        public void Equals_NullComparison_ReturnsFalse()
        {
            var key = new FolderTreeNodeKey("store-a", "entry-1", "\\Inbox");

            key.Equals((FolderTreeNodeKey)null).Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ObjectWithSameValues_ReturnsTrue()
        {
            var key = new FolderTreeNodeKey("store-a", "entry-1", "\\Inbox");
            object other = new FolderTreeNodeKey("STORE-A", "entry-1", "\\inbox");

            key.Equals(other).Should().BeTrue();
        }

        [TestMethod]
        public void Equals_SamePathInDifferentStores_ReturnsFalse()
        {
            var primary = new FolderTreeNodeKey("store-a", "entry-1", "\\Inbox\\Archive");
            var archive = new FolderTreeNodeKey("store-b", "entry-1", "\\Inbox\\Archive");

            primary.Should().NotBe(archive);
        }

        [TestMethod]
        public void Equals_StoreAndPathUseCaseInsensitiveComparison()
        {
            var first = new FolderTreeNodeKey("store-a", "entry-1", "\\Inbox\\Archive");
            var second = new FolderTreeNodeKey("STORE-A", "entry-1", "\\inbox\\archive");

            first.Should().Be(second);
        }

        [TestMethod]
        public void Equals_EntryIdUsesCaseSensitiveComparison()
        {
            var first = new FolderTreeNodeKey("store-a", "entry-1", "\\Inbox");
            var second = new FolderTreeNodeKey("store-a", "ENTRY-1", "\\Inbox");

            first.Should().NotBe(second);
        }

        [TestMethod]
        public void Constructor_TrimsStoreIdAndFolderPath()
        {
            var key = new FolderTreeNodeKey(" store-a ", "entry-1", " \\Inbox ");

            key.StoreId.Should().Be("store-a");
            key.FolderPath.Should().Be("\\Inbox");
        }

        [TestMethod]
        public void Constructor_NullEntryId_StoresEmptyString()
        {
            var key = new FolderTreeNodeKey("store-a", null, "\\Inbox");

            key.EntryId.Should().BeEmpty();
            key.ToString().Should().Be("store-a::\\Inbox");
        }

        [TestMethod]
        public void Constructor_BlankStoreId_Throws()
        {
            Action act = () => new FolderTreeNodeKey(" ", "entry-1", "\\Inbox");

            act.Should().Throw<ArgumentException>().WithParameterName("storeId");
        }

        [TestMethod]
        public void Constructor_NullStoreId_Throws()
        {
            Action act = () => new FolderTreeNodeKey(null, "entry-1", "\\Inbox");

            act.Should().Throw<ArgumentException>().WithParameterName("storeId");
        }

        [TestMethod]
        public void Constructor_BlankFolderPath_Throws()
        {
            Action act = () => new FolderTreeNodeKey("store-a", "entry-1", "");

            act.Should().Throw<ArgumentException>().WithParameterName("folderPath");
        }

        [TestMethod]
        public void Constructor_NullFolderPath_Throws()
        {
            Action act = () => new FolderTreeNodeKey("store-a", "entry-1", null);

            act.Should().Throw<ArgumentException>().WithParameterName("folderPath");
        }

        [TestMethod]
        public void ToString_ReturnsStoreEntryAndFolderPath()
        {
            var key = new FolderTreeNodeKey("store-a", "entry-1", "\\Inbox");

            key.ToString().Should().Be("store-a:entry-1:\\Inbox");
        }
    }
}
