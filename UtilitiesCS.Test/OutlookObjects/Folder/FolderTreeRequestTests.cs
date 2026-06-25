using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public sealed class FolderTreeRequestTests
    {
        [TestMethod]
        public void AllStores_IncludesAnyStore()
        {
            var request = FolderTreeRequest.AllStores(allowStaleSnapshot: true);

            request.IsAllStores.Should().BeTrue();
            request.AllowStaleSnapshot.Should().BeTrue();
            request.IncludesStore("store-a").Should().BeTrue();
        }

        [TestMethod]
        public void ForStore_FiltersByStoreIgnoringCase()
        {
            var request = FolderTreeRequest.ForStore("store-a", allowStaleSnapshot: false);

            request.IsAllStores.Should().BeFalse();
            request.AllowStaleSnapshot.Should().BeFalse();
            request.IncludesStore("STORE-A").Should().BeTrue();
            request.IncludesStore("store-b").Should().BeFalse();
        }

        [TestMethod]
        public void Constructor_DeduplicatesAndCopiesStores()
        {
            var stores = new List<string> { "store-a", "STORE-A", "store-b" };

            var request = new FolderTreeRequest(stores, allowStaleSnapshot: true);
            stores.Clear();

            request.StoreIds.Should().Equal("store-a", "store-b");
        }

        [TestMethod]
        public void Constructor_NullAndBlankStores_ReturnsAllStoresRequest()
        {
            var request = new FolderTreeRequest(new[] { null, " ", "\t" }, false);

            request.IsAllStores.Should().BeTrue();
            request.IncludesStore("store-a").Should().BeTrue();
        }

        [TestMethod]
        public void ForStore_BlankStore_Throws()
        {
            Action act = () => FolderTreeRequest.ForStore(" ", allowStaleSnapshot: false);

            act.Should().Throw<ArgumentException>().WithParameterName("storeId");
        }

        [TestMethod]
        public void SnapshotChangedEventArgs_CopiesAffectedStores()
        {
            var stores = new List<string> { "store-a" };
            var args = new FolderTreeSnapshotChangedEventArgs(
                new FolderTreeSnapshot(
                    Array.Empty<FolderTreeNodeKey>(),
                    Array.Empty<FolderTreeSnapshotNode>()
                ),
                FolderTreeRefreshReason.FolderChanged,
                stores
            );
            stores.Clear();

            args.Reason.Should().Be(FolderTreeRefreshReason.FolderChanged);
            args.AffectedStoreIds.Should().ContainSingle().Which.Should().Be("store-a");
        }

        [TestMethod]
        public void SnapshotChangedEventArgs_NullAndDuplicateStores_AreFiltered()
        {
            var snapshot = new FolderTreeSnapshot(
                Array.Empty<FolderTreeNodeKey>(),
                Array.Empty<FolderTreeSnapshotNode>()
            );

            var args = new FolderTreeSnapshotChangedEventArgs(
                snapshot,
                FolderTreeRefreshReason.StoreRemoved,
                new[] { " store-a ", "STORE-A", null, " " }
            );

            args.AffectedStoreIds.Should().ContainSingle().Which.Should().Be("store-a");
        }

        [TestMethod]
        public void SnapshotChangedEventArgs_NullSnapshot_Throws()
        {
            Action act = () =>
                new FolderTreeSnapshotChangedEventArgs(
                    null,
                    FolderTreeRefreshReason.ManualRefresh,
                    Array.Empty<string>()
                );

            act.Should().Throw<ArgumentNullException>().WithParameterName("snapshot");
        }
    }
}
