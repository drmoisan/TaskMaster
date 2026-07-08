using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Store;

namespace UtilitiesCS.Test.OutlookObjects.Store
{
    /// <summary>
    /// Deterministic tests for the pure store-filter attribution helpers introduced for the
    /// issue #211 Phase 3.4 diagnosis probe. No live COM, no live timer, no network/filesystem,
    /// no temporary files.
    /// </summary>
    [TestClass]
    public class StoreFilterAttributionTests
    {
        private static readonly IList<string> GwsoTokens = new List<string>
        {
            @"\Google\Google Apps Sync\",
            @"\Google\Google Workspace Sync\",
        };

        // --- Decide: per-branch coverage (P3-T2) ---

        [TestMethod]
        public void Decide_PublicFolderStoreWhenExcluded_ReturnsFalsePublicFolder()
        {
            // Arrange
            // Act
            var result = StoreFilterAttribution.Decide(
                isPublicFolder: true,
                displayName: "Public Folders",
                filePath: @"C:\Data\public.ost",
                excludedStoreNameContains: new List<string>(),
                excludedStoreFilePathContains: new List<string>(),
                gwsoFilePathContains: GwsoTokens,
                excludePublicFolderStores: true,
                excludeGwsoStores: true,
                isDisabled: false
            );

            // Assert
            result.Included.Should().BeFalse();
            result.Rule.Should().Be(StoreFilterRule.PublicFolder);
        }

        [TestMethod]
        public void Decide_DisplayNameContainsExcludedToken_ReturnsFalseNameContains()
        {
            // Arrange / Act
            var result = StoreFilterAttribution.Decide(
                isPublicFolder: false,
                displayName: "Team Archive",
                filePath: @"C:\Data\mailbox.ost",
                excludedStoreNameContains: new List<string> { "", "  ", "Archive" },
                excludedStoreFilePathContains: new List<string>(),
                gwsoFilePathContains: GwsoTokens,
                excludePublicFolderStores: true,
                excludeGwsoStores: true,
                isDisabled: false
            );

            // Assert
            result.Included.Should().BeFalse();
            result.Rule.Should().Be(StoreFilterRule.NameContains);
        }

        [TestMethod]
        public void Decide_FilePathContainsGwsoToken_WhenGwsoExcluded_ReturnsFalseGwsoFilePath()
        {
            // Arrange / Act
            var result = StoreFilterAttribution.Decide(
                isPublicFolder: false,
                displayName: "Google Workspace",
                filePath: @"C:\Users\Dan\Google\Google Workspace Sync\sync.ost",
                excludedStoreNameContains: new List<string>(),
                excludedStoreFilePathContains: new List<string>(),
                gwsoFilePathContains: GwsoTokens,
                excludePublicFolderStores: true,
                excludeGwsoStores: true,
                isDisabled: false
            );

            // Assert
            result.Included.Should().BeFalse();
            result.Rule.Should().Be(StoreFilterRule.GwsoFilePath);
        }

        [TestMethod]
        public void Decide_FilePathContainsExcludedToken_ReturnsFalseFilePathContains()
        {
            // Arrange / Act
            var result = StoreFilterAttribution.Decide(
                isPublicFolder: false,
                displayName: "Temp Store",
                filePath: @"C:\Temp\store.pst",
                excludedStoreNameContains: new List<string>(),
                excludedStoreFilePathContains: new List<string> { "", "  ", "Temp" },
                gwsoFilePathContains: GwsoTokens,
                excludePublicFolderStores: true,
                excludeGwsoStores: true,
                isDisabled: false
            );

            // Assert
            result.Included.Should().BeFalse();
            result.Rule.Should().Be(StoreFilterRule.FilePathContains);
        }

        [TestMethod]
        public void Decide_NormalStoreWithNoMatchingExclusion_ReturnsTrueIncluded()
        {
            // Arrange / Act
            var result = StoreFilterAttribution.Decide(
                isPublicFolder: false,
                displayName: "Mailbox",
                filePath: @"C:\Data\mailbox.ost",
                excludedStoreNameContains: new List<string> { "Archive" },
                excludedStoreFilePathContains: new List<string> { "Temp" },
                gwsoFilePathContains: GwsoTokens,
                excludePublicFolderStores: true,
                excludeGwsoStores: true,
                isDisabled: false
            );

            // Assert
            result.Included.Should().BeTrue();
            result.Rule.Should().Be(StoreFilterRule.Included);
        }

        // --- Decide: short-circuit precedence and edge cases (P3-T3) ---

        [TestMethod]
        public void Decide_WhenStoreMatchesPublicFolderAndLaterRule_ReturnsPublicFolderEarliestWins()
        {
            // Arrange: a public-folder store whose path would also match a GWSO token.
            // Act
            var result = StoreFilterAttribution.Decide(
                isPublicFolder: true,
                displayName: "Public Folders",
                filePath: @"C:\Users\Dan\Google\Google Apps Sync\sync.ost",
                excludedStoreNameContains: new List<string>(),
                excludedStoreFilePathContains: new List<string>(),
                gwsoFilePathContains: GwsoTokens,
                excludePublicFolderStores: true,
                excludeGwsoStores: true,
                isDisabled: false
            );

            // Assert: the earliest matching rule (PublicFolder) wins.
            result.Included.Should().BeFalse();
            result.Rule.Should().Be(StoreFilterRule.PublicFolder);
        }

        [TestMethod]
        public void Decide_WhenDisplayNameAndFilePathAreNull_DoesNotThrowAndIncludes()
        {
            // Arrange / Act
            var result = StoreFilterAttribution.Decide(
                isPublicFolder: false,
                displayName: null,
                filePath: null,
                excludedStoreNameContains: new List<string> { "Archive" },
                excludedStoreFilePathContains: new List<string> { "Temp" },
                gwsoFilePathContains: GwsoTokens,
                excludePublicFolderStores: true,
                excludeGwsoStores: true,
                isDisabled: false
            );

            // Assert: null name/path are treated as non-matching for the contains rules.
            result.Included.Should().BeTrue();
            result.Rule.Should().Be(StoreFilterRule.Included);
        }

        [TestMethod]
        public void Decide_WhenDisplayNameAndFilePathAreEmpty_DoesNotThrowAndIncludes()
        {
            // Arrange / Act
            var result = StoreFilterAttribution.Decide(
                isPublicFolder: false,
                displayName: "",
                filePath: "",
                excludedStoreNameContains: new List<string> { "Archive" },
                excludedStoreFilePathContains: new List<string> { "Temp" },
                gwsoFilePathContains: GwsoTokens,
                excludePublicFolderStores: true,
                excludeGwsoStores: true,
                isDisabled: false
            );

            // Assert
            result.Included.Should().BeTrue();
            result.Rule.Should().Be(StoreFilterRule.Included);
        }

        [TestMethod]
        public void Decide_WhenGwsoExclusionDisabled_GmailFilePathIsNotExcludedByGwsoRule()
        {
            // Arrange: a Gmail-style FilePath but excludeGwsoStores=false (flag guard).
            // Act
            var result = StoreFilterAttribution.Decide(
                isPublicFolder: false,
                displayName: "Google Workspace",
                filePath: @"C:\Users\Dan\Google\Google Workspace Sync\sync.ost",
                excludedStoreNameContains: new List<string>(),
                excludedStoreFilePathContains: new List<string>(),
                gwsoFilePathContains: GwsoTokens,
                excludePublicFolderStores: true,
                excludeGwsoStores: false,
                isDisabled: false
            );

            // Assert: with the GWSO flag off, the Gmail store is NOT excluded by the GWSO rule.
            result.Included.Should().BeTrue();
            result.Rule.Should().Be(StoreFilterRule.Included);
        }

        // --- Decide: Disabled reason checked last (P3-T2, issue #261) ---

        [TestMethod]
        public void Decide_WhenDisabledAndNoEarlierRuleMatches_ReturnsDisabled()
        {
            // Arrange / Act: no exclusion rule matches, but the store is disabled.
            var result = StoreFilterAttribution.Decide(
                isPublicFolder: false,
                displayName: "Mailbox",
                filePath: @"C:\Data\mailbox.ost",
                excludedStoreNameContains: new List<string>(),
                excludedStoreFilePathContains: new List<string>(),
                gwsoFilePathContains: GwsoTokens,
                excludePublicFolderStores: true,
                excludeGwsoStores: true,
                isDisabled: true
            );

            // Assert: the Disabled reason is attributed only when no earlier rule matched.
            result.Included.Should().BeFalse();
            result.Rule.Should().Be(StoreFilterRule.Disabled);
        }

        [TestMethod]
        public void Decide_WhenPublicFolderExcludedAndAlsoDisabled_KeepsPublicFolderRule()
        {
            // Arrange / Act: an earlier rule (public folder) matches while the store is also disabled.
            var result = StoreFilterAttribution.Decide(
                isPublicFolder: true,
                displayName: "Public Folders",
                filePath: @"C:\Data\public.ost",
                excludedStoreNameContains: new List<string>(),
                excludedStoreFilePathContains: new List<string>(),
                gwsoFilePathContains: GwsoTokens,
                excludePublicFolderStores: true,
                excludeGwsoStores: true,
                isDisabled: true
            );

            // Assert: the pre-existing rule wins; attribution is byte-for-byte unchanged.
            result.Included.Should().BeFalse();
            result.Rule.Should().Be(StoreFilterRule.PublicFolder);
        }

        [TestMethod]
        public void Decide_WhenNameExcludedAndAlsoDisabled_KeepsNameContainsRule()
        {
            var result = StoreFilterAttribution.Decide(
                isPublicFolder: false,
                displayName: "Team Archive",
                filePath: @"C:\Data\archive.ost",
                excludedStoreNameContains: new List<string> { "Archive" },
                excludedStoreFilePathContains: new List<string>(),
                gwsoFilePathContains: GwsoTokens,
                excludePublicFolderStores: true,
                excludeGwsoStores: true,
                isDisabled: true
            );

            result.Included.Should().BeFalse();
            result.Rule.Should().Be(StoreFilterRule.NameContains);
        }

        [TestMethod]
        public void Decide_WhenGwsoExcludedAndAlsoDisabled_KeepsGwsoFilePathRule()
        {
            var result = StoreFilterAttribution.Decide(
                isPublicFolder: false,
                displayName: "Google Workspace",
                filePath: @"C:\Users\Dan\Google\Google Workspace Sync\sync.ost",
                excludedStoreNameContains: new List<string>(),
                excludedStoreFilePathContains: new List<string>(),
                gwsoFilePathContains: GwsoTokens,
                excludePublicFolderStores: true,
                excludeGwsoStores: true,
                isDisabled: true
            );

            result.Included.Should().BeFalse();
            result.Rule.Should().Be(StoreFilterRule.GwsoFilePath);
        }

        [TestMethod]
        public void Decide_WhenFilePathExcludedAndAlsoDisabled_KeepsFilePathContainsRule()
        {
            var result = StoreFilterAttribution.Decide(
                isPublicFolder: false,
                displayName: "Mailbox",
                filePath: @"C:\Temp\mailbox.ost",
                excludedStoreNameContains: new List<string>(),
                excludedStoreFilePathContains: new List<string> { "Temp" },
                gwsoFilePathContains: GwsoTokens,
                excludePublicFolderStores: true,
                excludeGwsoStores: true,
                isDisabled: true
            );

            result.Included.Should().BeFalse();
            result.Rule.Should().Be(StoreFilterRule.FilePathContains);
        }

        [TestMethod]
        public void StoreFilterRule_EnumOrder_PlacesDisabledImmediatelyBeforeIncluded()
        {
            // Assert: the enum mirrors evaluation order with Disabled inserted just before Included.
            ((int)StoreFilterRule.FilePathContains)
                .Should()
                .Be((int)StoreFilterRule.Disabled - 1);
            ((int)StoreFilterRule.Disabled).Should().Be((int)StoreFilterRule.Included - 1);
        }

        // --- FormatLine (P3-T4) ---

        [TestMethod]
        public void FormatLine_WithExcludedGwsoStore_RendersAllFieldsF1Invariant()
        {
            // Arrange / Act
            var line = StoreFilterAttribution.FormatLine(
                displayName: "Google Workspace",
                exchangeStoreTypeMs: 12.0,
                filePathMs: 4500.5,
                included: false,
                rule: StoreFilterRule.GwsoFilePath
            );

            // Assert
            line.Should().StartWith("[store-filter] ");
            line.Should().Contain("displayName=Google Workspace");
            line.Should().Contain("exchangeStoreTypeMs=12.0");
            line.Should().Contain("filePathMs=4500.5");
            line.Should().Contain("included=false");
            line.Should().Contain("rule=GwsoFilePath");
        }

        [TestMethod]
        public void FormatLine_WithIncludedStore_RendersIncludedTrueAndIncludedRule()
        {
            // Arrange / Act
            var line = StoreFilterAttribution.FormatLine(
                displayName: "Mailbox",
                exchangeStoreTypeMs: 0.0,
                filePathMs: 1.2,
                included: true,
                rule: StoreFilterRule.Included
            );

            // Assert
            line.Should().StartWith("[store-filter] ");
            line.Should().Contain("displayName=Mailbox");
            line.Should().Contain("exchangeStoreTypeMs=0.0");
            line.Should().Contain("filePathMs=1.2");
            line.Should().Contain("included=true");
            line.Should().Contain("rule=Included");
        }

        [TestMethod]
        public void FormatLine_WithNullDisplayName_RendersNullPlaceholder()
        {
            // Arrange / Act
            var line = StoreFilterAttribution.FormatLine(
                displayName: null,
                exchangeStoreTypeMs: 3.0,
                filePathMs: 7.0,
                included: true,
                rule: StoreFilterRule.Included
            );

            // Assert
            line.Should().Contain("displayName=<null>");
        }

        [TestMethod]
        public void FormatLine_WithEmptyDisplayName_RendersNullPlaceholder()
        {
            // Arrange / Act
            var line = StoreFilterAttribution.FormatLine(
                displayName: "",
                exchangeStoreTypeMs: 3.0,
                filePathMs: 7.0,
                included: true,
                rule: StoreFilterRule.Included
            );

            // Assert
            line.Should().Contain("displayName=<null>");
        }
    }
}
