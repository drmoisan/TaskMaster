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
                excludeGwsoStores: true
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
                excludeGwsoStores: true
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
                excludeGwsoStores: true
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
                excludeGwsoStores: true
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
                excludeGwsoStores: true
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
                excludeGwsoStores: true
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
                excludeGwsoStores: true
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
                excludeGwsoStores: true
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
                excludeGwsoStores: false
            );

            // Assert: with the GWSO flag off, the Gmail store is NOT excluded by the GWSO rule.
            result.Included.Should().BeTrue();
            result.Rule.Should().Be(StoreFilterRule.Included);
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
