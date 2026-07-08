using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public class StoresWrapperDisableTests
    {
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

        // --- Disabled-store filter integration + persistence (P7-T4, issue #261) ---

        [TestMethod]
        public void ShouldIncludeStore_ExcludesSessionDisabledStore_KeepsNonDisabled()
        {
            var disabled = CreateStore("Disabled Mailbox", @"C:\Data\d.ost", "d@example.com");
            var kept = CreateStore("Kept Mailbox", @"C:\Data\k.ost", "k@example.com");
            var wrapper = new StoresWrapper
            {
                ExcludePublicFolderStores = false,
                ExcludeGwsoStores = false,
            };
            wrapper.SessionDisabledStoreIdentities.Add("Disabled Mailbox");

            wrapper.ShouldIncludeStore(disabled.Object).Should().BeFalse();
            wrapper.ShouldIncludeStore(kept.Object).Should().BeTrue();
        }

        [TestMethod]
        public void ShouldIncludeStore_ExcludesFutureDisabledStore_KeepsNonDisabled()
        {
            var disabled = CreateStore("Disabled Mailbox", @"C:\Data\d.ost", "d@example.com");
            var kept = CreateStore("Kept Mailbox", @"C:\Data\k.ost", "k@example.com");
            var wrapper = new StoresWrapper
            {
                ExcludePublicFolderStores = false,
                ExcludeGwsoStores = false,
                DisabledStoreIdentities = new List<string> { "Disabled Mailbox" },
            };

            wrapper.ShouldIncludeStore(disabled.Object).Should().BeFalse();
            wrapper.ShouldIncludeStore(kept.Object).Should().BeTrue();
        }

        [TestMethod]
        public void StoreIsIncluded_WhenIsDisabledTrue_ReturnsFalse()
        {
            var store = CreateStore("Mailbox", @"C:\Data\m.ost", "m@example.com");

            StoresWrapper
                .StoreIsIncluded(
                    store.Object,
                    new List<string>(),
                    new List<string>(),
                    new List<string>(),
                    excludePublicFolderStores: false,
                    excludeGwsoStores: false,
                    isDisabled: true
                )
                .Should()
                .BeFalse();

            StoresWrapper
                .StoreIsIncluded(
                    store.Object,
                    new List<string>(),
                    new List<string>(),
                    new List<string>(),
                    excludePublicFolderStores: false,
                    excludeGwsoStores: false,
                    isDisabled: false
                )
                .Should()
                .BeTrue();
        }

        [TestMethod]
        public void Init_ExcludesSessionAndFutureDisabledStores_ViaInstrumentedPath()
        {
            var included = CreateStore("Mailbox", @"C:\Data\mailbox.ost", "o@example.com");
            var sessionDisabled = CreateStore("SessionStore", @"C:\Data\s.ost", "s@example.com");
            var futureDisabled = CreateStore("FutureStore", @"C:\Data\f.ost", "f@example.com");

            var wrapper = new StoresWrapper(
                CreateGlobalsWithStores(
                    included.Object,
                    sessionDisabled.Object,
                    futureDisabled.Object
                ).Object
            )
            {
                ExcludePublicFolderStores = false,
                ExcludeGwsoStores = false,
                DisabledStoreIdentities = new List<string> { "FutureStore" },
            };
            wrapper.SessionDisabledStoreIdentities.Add("SessionStore");

            wrapper.Init();

            // The instrumented filter path (the only path that populates Stores) excludes both the
            // session-disabled and future-disabled stores, leaving only the non-disabled store.
            wrapper.Stores.Should().ContainSingle();
            wrapper.Stores[0].DisplayName.Should().Be("Mailbox");
        }

        [TestMethod]
        public void Serialization_RoundTrip_PreservesDisabledListAndOmitsSessionSet()
        {
            var wrapper = new StoresWrapper
            {
                DisabledStoreIdentities = new List<string> { "PersistedStore" },
            };
            wrapper.SessionDisabledStoreIdentities.Add("SessionStore");

            var json = wrapper.SerializeToString();

            json.Should().Contain("DisabledStoreIdentities");
            json.Should().Contain("PersistedStore");
            json.Should()
                .NotContain(
                    "SessionDisabledStoreIdentities",
                    "the session-only set is [JsonIgnore] and must not be emitted"
                );
            json.Should().NotContain("SessionStore");

            var restored = wrapper.DeserializeObject(json, wrapper.Config.JsonSettings);

            restored.DisabledStoreIdentities.Should().Contain("PersistedStore");
            restored
                .SessionDisabledStoreIdentities.Should()
                .NotBeNull("Newtonsoft re-runs the field initializer on deserialize")
                .And.BeEmpty();
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
                    excludeGwso,
                    false
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
    }
}
