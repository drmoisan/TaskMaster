using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Store;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;
using OutlookStore = Microsoft.Office.Interop.Outlook.Store;

namespace UtilitiesCS.Test.OutlookObjects.Store
{
    [TestClass]
    public class StoresWrapperTests
    {
        [TestMethod]
        public async Task CreateAsync_WhenCancellationAlreadyRequested_ThrowsOperationCanceledException()
        {
            var globals = new Mock<IApplicationGlobals>();
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            Func<Task> act = async () =>
                await StoresWrapper.CreateAsync(globals.Object, cancellation.Token);

            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        [TestMethod]
        public async Task CreateAsync_WhenGlobalsAreNull_ThrowsNullReferenceException()
        {
            Func<Task> act = async () =>
                await StoresWrapper.CreateAsync(null, CancellationToken.None);

            await act.Should().ThrowAsync<NullReferenceException>();
        }

        [TestMethod]
        public void Constructor_WithoutGlobals_SetsDefaultFlagsAndCollections()
        {
            var wrapper = new StoresWrapper();

            wrapper.Globals.Should().BeNull();
            wrapper.Stores.Should().BeNull();
            wrapper.ExcludePublicFolderStores.Should().BeTrue();
            wrapper.ExcludeGwsoStores.Should().BeTrue();
            wrapper
                .GwsoFilePathContains.Should()
                .Equal(@"\Google\Google Apps Sync\", @"\Google\Google Workspace Sync\");
            wrapper.ExcludedStoreNameContains.Should().BeEmpty();
            wrapper.ExcludedStoreFilePathContains.Should().BeEmpty();
        }

        [TestMethod]
        public void Constructor_WithGlobals_PreservesGlobalsReference()
        {
            var globals = new Mock<IApplicationGlobals>();

            var wrapper = new StoresWrapper(globals.Object);

            wrapper.Globals.Should().BeSameAs(globals.Object);
        }

        [TestMethod]
        public void Init_WhenStoresMatchFilters_ProjectsOnlyIncludedStores()
        {
            var includedStore = CreateStore("Mailbox", @"C:\Data\mailbox.ost", "owner@example.com");
            var excludedByName = CreateStore(
                "Archive Mailbox",
                @"C:\Data\archive.ost",
                "archive@example.com"
            );
            var excludedPublicFolder = CreateStore(
                "Public Folders",
                @"C:\Data\public.ost",
                "public@example.com",
                OlExchangeStoreType.olExchangePublicFolder
            );
            var excludedByPath = CreateStore(
                "Temp Store",
                @"C:\Temp\store.pst",
                "temp@example.com"
            );
            var excludedGwso = CreateStore(
                "Google Workspace",
                @"C:\Users\Dan\Google\Google Workspace Sync\sync.ost",
                "gwso@example.com"
            );

            var wrapper = new StoresWrapper(
                CreateGlobalsWithStores(
                    includedStore.Object,
                    excludedByName.Object,
                    excludedPublicFolder.Object,
                    excludedByPath.Object,
                    excludedGwso.Object
                ).Object
            )
            {
                ExcludedStoreNameContains = new List<string> { "Archive" },
                ExcludedStoreFilePathContains = new List<string> { "Temp" },
            };

            var result = wrapper.Init();

            result.Should().BeSameAs(wrapper);
            wrapper.Stores.Should().ContainSingle();
            wrapper.Stores[0].DisplayName.Should().Be("Mailbox");
            wrapper.Stores[0].UserEmailAddress.Should().Be("owner@example.com");
        }

        [TestMethod]
        public async Task RewireOlObjectsAsync_WhenStoresCollectionIsNull_InitializesAndAddsFilteredStores()
        {
            var includedStore = CreateStore("Mailbox", @"C:\Data\mailbox.ost", "owner@example.com");
            var excludedStore = CreateStore(
                "Archive",
                @"C:\Temp\archive.pst",
                "archive@example.com"
            );

            var wrapper = new TestableStoresWrapper(
                CreateGlobalsWithStores(includedStore.Object, excludedStore.Object).Object
            )
            {
                Stores = null,
                ExcludedStoreNameContains = new List<string> { "Archive" },
                ExcludedStoreFilePathContains = new List<string> { "Temp" },
            };

            await wrapper.RewireOlObjectsAsync(new StreamingContext());

            wrapper.Stores.Should().ContainSingle();
            wrapper.Stores[0].DisplayName.Should().Be("Mailbox");
            wrapper.Stores[0].UserEmailAddress.Should().Be("owner@example.com");
        }

        [TestMethod]
        public async Task RewireOlObjectsAsync_WhenStoreDisplayNameAlreadyExists_RestoresExistingWrapperWithoutAddingDuplicate()
        {
            var originalStore = CreateStore("Mailbox", @"C:\Data\old.ost", "old@example.com");
            var updatedStore = CreateStore("Mailbox", @"C:\Data\new.ost", "new@example.com");
            var existingWrapper = new StoreWrapper(originalStore.Object).Init();

            var wrapper = new TestableStoresWrapper(
                CreateGlobalsWithStores(updatedStore.Object).Object
            )
            {
                Stores = new List<StoreWrapper> { existingWrapper },
                ExcludedStoreNameContains = new List<string>(),
                ExcludedStoreFilePathContains = new List<string>(),
                GwsoFilePathContains = new List<string>(),
                ExcludeGwsoStores = false,
                ExcludePublicFolderStores = false,
            };

            await wrapper.RewireOlObjectsAsync(new StreamingContext());

            wrapper.Stores.Should().ContainSingle();
            wrapper.Stores[0].Should().BeSameAs(existingWrapper);
            existingWrapper.InnerStore.Should().BeSameAs(updatedStore.Object);
            existingWrapper.UserEmailAddress.Should().Be("new@example.com");
        }

        [TestMethod]
        public void InclusionFilters_ExcludePublicFoldersWhenConfigured()
        {
            var store = CreateStore(
                "Public Folders",
                @"C:\Data\public.ost",
                "public@example.com",
                OlExchangeStoreType.olExchangePublicFolder
            );

            AssertInclusionDecision(
                store.Object,
                excludedNames: null,
                excludedPaths: null,
                gwsoPaths: new List<string>(),
                excludePublicFolders: true,
                excludeGwso: false,
                expected: false
            );
        }

        [TestMethod]
        [DataRow("Archive", "Team Archive")]
        [DataRow("archive", "TEAM ARCHIVE")]
        public void InclusionFilters_ExcludeMatchingDisplayNames_IgnoringCase(
            string excludedName,
            string displayName
        )
        {
            var store = CreateStore(displayName, @"C:\Data\mailbox.ost", "user@example.com");

            AssertInclusionDecision(
                store.Object,
                excludedNames: new List<string> { "", "  ", excludedName },
                excludedPaths: null,
                gwsoPaths: new List<string>(),
                excludePublicFolders: false,
                excludeGwso: false,
                expected: false
            );
        }

        [TestMethod]
        public void InclusionFilters_ExcludeMatchingGwsoPaths_IgnoringCase()
        {
            var store = CreateStore(
                "Workspace",
                @"C:\Users\Dan\GOOGLE\Google Apps Sync\sync.ost",
                "user@example.com"
            );

            AssertInclusionDecision(
                store.Object,
                excludedNames: null,
                excludedPaths: null,
                gwsoPaths: new List<string> { "", @"\google\google apps sync\" },
                excludePublicFolders: false,
                excludeGwso: true,
                expected: false
            );
        }

        [TestMethod]
        public void InclusionFilters_ExcludeMatchingFilePaths_IgnoringWhitespaceEntries()
        {
            var store = CreateStore("Mailbox", @"C:\Temp\mailbox.ost", "user@example.com");

            AssertInclusionDecision(
                store.Object,
                excludedNames: null,
                excludedPaths: new List<string> { "", "  ", "Temp" },
                gwsoPaths: new List<string>(),
                excludePublicFolders: false,
                excludeGwso: false,
                expected: false
            );
        }

        [TestMethod]
        public void InclusionFilters_WhenFilePathAccessThrows_TreatsPathAsUnavailable()
        {
            var store = CreateStore(
                "Mailbox",
                filePath: @"C:\ShouldNotMatter\mailbox.ost",
                primarySmtpAddress: "user@example.com",
                throwOnFilePathAccess: true
            );

            AssertInclusionDecision(
                store.Object,
                excludedNames: new List<string>(),
                excludedPaths: new List<string> { "Temp" },
                gwsoPaths: new List<string> { @"\Google\Google Apps Sync\" },
                excludePublicFolders: false,
                excludeGwso: true,
                expected: true
            );
        }

        [TestMethod]
        public void InclusionFilters_WhenNoExclusionMatches_ReturnsTrue()
        {
            var store = CreateStore("Mailbox", @"C:\Data\mailbox.ost", "user@example.com");

            AssertInclusionDecision(
                store.Object,
                excludedNames: new List<string> { "Archive" },
                excludedPaths: new List<string> { "Temp" },
                gwsoPaths: new List<string> { @"\Google\Google Apps Sync\" },
                excludePublicFolders: true,
                excludeGwso: true,
                expected: true
            );
        }

        private static void AssertInclusionDecision(
            OutlookStore store,
            IList<string> excludedNames,
            IList<string> excludedPaths,
            IList<string> gwsoPaths,
            bool excludePublicFolders,
            bool excludeGwso,
            bool expected
        )
        {
            var wrapper = new StoresWrapper
            {
                ExcludedStoreNameContains = excludedNames?.ToList(),
                ExcludedStoreFilePathContains = excludedPaths?.ToList(),
                GwsoFilePathContains = gwsoPaths?.ToList() ?? new List<string>(),
                ExcludePublicFolderStores = excludePublicFolders,
                ExcludeGwsoStores = excludeGwso,
            };

            wrapper.ShouldIncludeStore(store).Should().Be(expected);
            StoresWrapper
                .StoreIsIncluded(
                    store,
                    excludedNames,
                    excludedPaths,
                    gwsoPaths ?? new List<string>(),
                    excludePublicFolders,
                    excludeGwso
                )
                .Should()
                .Be(expected);
        }

        private static Mock<IApplicationGlobals> CreateGlobalsWithStores(
            params OutlookStore[] stores
        )
        {
            var storesCollection = new Mock<Stores>();
            storesCollection
                .As<IEnumerable>()
                .Setup(x => x.GetEnumerator())
                .Returns(() => stores.Cast<object>().GetEnumerator());

            var nameSpace = new Mock<NameSpace>();
            nameSpace.SetupGet(x => x.Stores).Returns(storesCollection.Object);

            var olObjects = new Mock<IOlObjects>();
            olObjects.SetupGet(x => x.NamespaceMAPI).Returns(nameSpace.Object);

            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(x => x.Ol).Returns(olObjects.Object);
            return globals;
        }

        private static Mock<OutlookStore> CreateStore(
            string displayName,
            string filePath,
            string primarySmtpAddress,
            OlExchangeStoreType exchangeStoreType = OlExchangeStoreType.olPrimaryExchangeMailbox,
            bool throwOnFilePathAccess = false
        )
        {
            var store = new Mock<OutlookStore>();
            var rootFolder = CreateRootFolderWithPrimarySmtpAddress(primarySmtpAddress);

            store.SetupGet(x => x.DisplayName).Returns(displayName);
            store.SetupGet(x => x.ExchangeStoreType).Returns(exchangeStoreType);
            store.Setup(x => x.GetRootFolder()).Returns(rootFolder.Object);

            if (exchangeStoreType != OlExchangeStoreType.olExchangePublicFolder)
            {
                store
                    .Setup(x => x.GetDefaultFolder(OlDefaultFolders.olFolderInbox))
                    .Returns(new Mock<OutlookFolder>().Object);
            }

            if (throwOnFilePathAccess)
            {
                store
                    .SetupGet(x => x.FilePath)
                    .Throws(new InvalidOperationException("FilePath unavailable"));
            }
            else
            {
                store.SetupGet(x => x.FilePath).Returns(filePath);
            }

            return store;
        }

        private static Mock<OutlookFolder> CreateRootFolderWithPrimarySmtpAddress(
            string primarySmtpAddress
        )
        {
            var rootFolder = new Mock<OutlookFolder>();
            var session = new Mock<NameSpace>();
            var currentUser = new Mock<Microsoft.Office.Interop.Outlook.Recipient>();
            var addressEntry = new Mock<AddressEntry>();
            var exchangeUser = new Mock<ExchangeUser>();

            exchangeUser.SetupGet(x => x.PrimarySmtpAddress).Returns(primarySmtpAddress);
            addressEntry.Setup(x => x.GetExchangeUser()).Returns(exchangeUser.Object);
            currentUser.SetupGet(x => x.AddressEntry).Returns(addressEntry.Object);
            session.SetupGet(x => x.CurrentUser).Returns(currentUser.Object);
            rootFolder.SetupGet(x => x.Session).Returns(session.Object);

            return rootFolder;
        }

        private sealed class TestableStoresWrapper : StoresWrapper
        {
            public TestableStoresWrapper(IApplicationGlobals globals)
                : base(globals) { }

            public new Task RewireOlObjectsAsync(StreamingContext context)
            {
                return base.RewireOlObjectsAsync(context);
            }
        }
    }
}
