using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Store;
using OutlookStore = Microsoft.Office.Interop.Outlook.Store;

namespace UtilitiesCS.Test.OutlookObjects.Store
{
    /// <summary>
    /// StoreID-exclusion behavior for <see cref="StoresWrapper"/> (issue #328). Covers the Gap-1
    /// requirements (exact-match, case-insensitivity, whitespace handling, independence from other
    /// rules, precedence/attribution, filtered enumeration, and fail-open) plus JSON round-trip and
    /// legacy backward-compatibility. Partial of <see cref="StoresWrapperTests"/> so it reuses the
    /// existing COM-free mock harness (CreateStore/CreateGlobalsWithStores).
    /// </summary>
    public partial class StoresWrapperTests
    {
        [TestMethod]
        public void ShouldIncludeStore_WhenStoreIdExactlyMatchesExcludedSet_ExcludesStore()
        {
            var store = CreateStore(
                "Mailbox",
                @"C:\Data\mailbox.ost",
                "user@example.com",
                storeId: "00112233ABCDEF"
            );

            var wrapper = new StoresWrapper
            {
                ExcludedStoreIds = new List<string> { "00112233ABCDEF" },
            };

            wrapper.ShouldIncludeStore(store.Object).Should().BeFalse();
        }

        [TestMethod]
        public void ShouldIncludeStore_WhenStoreIdNearButNotEqual_DoesNotExclude()
        {
            var store = CreateStore(
                "Mailbox",
                @"C:\Data\mailbox.ost",
                "user@example.com",
                storeId: "00112233ABCDEF"
            );

            var wrapper = new StoresWrapper
            {
                // Substring of the real StoreID: must NOT match (exact-match only).
                ExcludedStoreIds = new List<string> { "00112233ABCDE" },
            };

            wrapper.ShouldIncludeStore(store.Object).Should().BeTrue();
        }

        [TestMethod]
        public void ShouldIncludeStore_WhenStoreIdDiffersOnlyByCase_ExcludesStore()
        {
            var store = CreateStore(
                "Mailbox",
                @"C:\Data\mailbox.ost",
                "user@example.com",
                storeId: "00112233abcdef"
            );

            var wrapper = new StoresWrapper
            {
                ExcludedStoreIds = new List<string> { "00112233ABCDEF" },
            };

            wrapper.ShouldIncludeStore(store.Object).Should().BeFalse();
        }

        [TestMethod]
        public void ShouldIncludeStore_WhenExcludedSetHasEmptyOrWhitespaceEntries_TheyAreIgnored()
        {
            var store = CreateStore(
                "Mailbox",
                @"C:\Data\mailbox.ost",
                "user@example.com",
                storeId: "REALID"
            );

            var wrapper = new StoresWrapper
            {
                ExcludedStoreIds = new List<string> { "", "   ", null },
            };

            wrapper.ShouldIncludeStore(store.Object).Should().BeTrue();
        }

        [TestMethod]
        public void ShouldIncludeStore_WhenOnlyStoreIdRuleConfigured_ExcludesIndependentOfOtherRules()
        {
            var store = CreateStore(
                "Mailbox",
                @"C:\Data\mailbox.ost",
                "user@example.com",
                storeId: "EXCLUDED"
            );

            // No name/path/gwso/public-folder rule matches this store; only the StoreID rule does.
            var wrapper = new StoresWrapper
            {
                ExcludedStoreIds = new List<string> { "EXCLUDED" },
                ExcludedStoreNameContains = new List<string>(),
                ExcludedStoreFilePathContains = new List<string>(),
                GwsoFilePathContains = new List<string>(),
                ExcludeGwsoStores = false,
                ExcludePublicFolderStores = false,
            };

            wrapper.ShouldIncludeStore(store.Object).Should().BeFalse();
        }

        [TestMethod]
        public void Decide_WhenStoreIdMatchesAndOtherRulesAlsoMatch_ExcludesAndAttributesStoreId()
        {
            // Arrange: a store whose DisplayName would also be excluded by NameContains, but the
            // StoreID rule is the most authoritative and must win the attribution.
            var (included, rule) = StoreFilterAttribution.Decide(
                storeId: "EXCLUDED",
                excludedStoreIds: new List<string> { "EXCLUDED" },
                isPublicFolder: true,
                displayName: "Archive",
                filePath: @"C:\Temp\store.pst",
                excludedStoreNameContains: new List<string> { "Archive" },
                excludedStoreFilePathContains: new List<string> { "Temp" },
                gwsoFilePathContains: new List<string>(),
                excludePublicFolderStores: true,
                excludeGwsoStores: false,
                isDisabled: true
            );

            included.Should().BeFalse();
            rule.Should().Be(StoreFilterRule.StoreId);
        }

        [TestMethod]
        public void Init_WhenStoreIdExcluded_OmitsStoreFromProjectedSet()
        {
            var includedStore = CreateStore(
                "Mailbox",
                @"C:\Data\mailbox.ost",
                "owner@example.com",
                storeId: "KEEP"
            );
            var excludedByStoreId = CreateStore(
                "Second Mailbox",
                @"C:\Data\second.ost",
                "second@example.com",
                storeId: "DROP"
            );

            var wrapper = new StoresWrapper(
                CreateGlobalsWithStores(includedStore.Object, excludedByStoreId.Object).Object
            )
            {
                ExcludedStoreIds = new List<string> { "DROP" },
            };

            wrapper.Init();

            wrapper.Stores.Should().ContainSingle();
            wrapper.Stores[0].DisplayName.Should().Be("Mailbox");
        }

        [TestMethod]
        public void ShouldIncludeStore_WhenStoreIdReadThrows_IsFailOpenAndDoesNotExclude()
        {
            var store = CreateStore(
                "Mailbox",
                @"C:\Data\mailbox.ost",
                "user@example.com",
                throwOnStoreIdAccess: true
            );

            var wrapper = new StoresWrapper
            {
                // Even though the set is populated, an unreadable StoreID must not exclude.
                ExcludedStoreIds = new List<string> { "ANYTHING" },
            };

            wrapper.ShouldIncludeStore(store.Object).Should().BeTrue();
        }

        [TestMethod]
        public void Serialization_RoundTrip_PreservesExcludedStoreIds()
        {
            var wrapper = new StoresWrapper
            {
                ExcludedStoreIds = new List<string> { "PersistedStoreId" },
            };

            var json = wrapper.SerializeToString();

            json.Should().Contain("ExcludedStoreIds");
            json.Should().Contain("PersistedStoreId");

            var restored = wrapper.DeserializeObject(json, wrapper.Config.JsonSettings);

            restored
                .ExcludedStoreIds.Should()
                .ContainSingle()
                .Which.Should()
                .Be("PersistedStoreId");
        }

        [TestMethod]
        public void Deserialize_LegacyJsonWithoutExcludedStoreIdsKey_RestoresEmptyDefault()
        {
            // Legacy payload predating issue #328: no ExcludedStoreIds key present.
            const string legacyJson = "{\"ExcludePublicFolderStores\":true}";

            var seed = new StoresWrapper();
            var restored = seed.DeserializeObject(legacyJson, seed.Config.JsonSettings);

            restored.ExcludedStoreIds.Should().NotBeNull().And.BeEmpty();
        }
    }
}
