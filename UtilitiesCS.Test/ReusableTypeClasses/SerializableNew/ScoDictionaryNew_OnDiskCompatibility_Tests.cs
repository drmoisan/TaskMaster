using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.ReusableTypeClasses.SerializableNew
{
    /// <summary>
    /// On-disk serialization compatibility tests for the persisted dictionaries migrated to the
    /// vendored-dependency-free <see cref="ScoDictionaryNew{TKey, TValue}"/> lineage (issue #306, child F1).
    ///
    /// The four persisted dictionaries covered here (DictRemap, FilteredFolderScraping, FolderRemap,
    /// and the SubjectMap Encoder) store a flat <c>{"key": value}</c> JSON object on disk. The
    /// production write path uses the default <see cref="NewSmartSerializableConfig.GetDefaultSettings"/>
    /// settings (TypeNameHandling.Auto, no converters, no PreserveReferencesHandling), so a bare type
    /// swap preserves on-disk compatibility without a serialization binder or converter. These tests
    /// prove both directions: an existing flat payload loads with entry fidelity, and the default
    /// write path re-emits a flat payload that never contains the globals-based
    /// <c>ScoDictionaryConverter</c> / <c>PreserveReferencesHandling.All</c> wrapper tokens.
    ///
    /// The read seam is exercised through the string-based <see cref="ScoDictionaryNew{TKey,TValue}"/>
    /// <c>DeserializeObject(json, settings)</c> method and the write seam through
    /// <c>SerializeToString()</c>. Both operate entirely in memory: no temporary files are created and
    /// no wall-clock wait or deferred-timer path is used.
    /// </summary>
    [TestClass]
    public class ScoDictionaryNew_OnDiskCompatibility_Tests
    {
        /// <summary>
        /// Tokens that appear only in the prohibited globals-based serialization path
        /// (<c>ScoDictionaryNew.GetSettingsJson(globals)</c> / <c>ScoDictionaryConverter</c> /
        /// <c>PreserveReferencesHandling.All</c>). None of the persisted dictionaries may use that path,
        /// so a compatible flat payload must contain none of these tokens.
        /// </summary>
        private static readonly string[] WrapperTokens = new[]
        {
            "$type",
            "$id",
            "CoDictionary",
            "RemainingObject",
        };

        private static JsonSerializerSettings DefaultSettings() =>
            NewSmartSerializableConfig.GetDefaultSettings();

        /// <summary>
        /// Loads an embedded flat on-disk payload through the production string-read path, asserts
        /// entry fidelity, re-serializes through the production default write path, asserts the output
        /// is a flat object free of the globals-converter wrapper tokens, and confirms the written
        /// payload round-trips back to the same entries.
        /// </summary>
        private static void AssertFlatRoundTrip<TKey, TValue>(
            string embeddedPayload,
            IReadOnlyDictionary<TKey, TValue> expectedEntries
        )
        {
            // Arrange: production default read/write settings (flat, no converters).
            var settings = DefaultSettings();

            // Act: load the embedded on-disk payload through the production string-read path.
            var loaded = new ScoDictionaryNew<TKey, TValue>().DeserializeObject(
                embeddedPayload,
                settings
            );

            // Assert: entry fidelity on load.
            loaded
                .Should()
                .NotBeNull("the flat on-disk payload must deserialize into the new lineage");
            loaded.Should().HaveCount(expectedEntries.Count);
            foreach (var kvp in expectedEntries)
            {
                loaded.TryGetValue(kvp.Key, out var value).Should().BeTrue();
                value.Should().Be(kvp.Value);
            }

            // Act: re-serialize through the production default write path.
            var written = loaded.SerializeToString();

            // Assert: the output stays on the flat on-disk format (no globals wrapper tokens).
            foreach (var token in WrapperTokens)
            {
                written
                    .Should()
                    .NotContain(
                        token,
                        "the persisted dictionary must remain on the flat on-disk format and must "
                            + "not use the globals ScoDictionaryConverter / PreserveReferencesHandling "
                            + "path (token '{0}')",
                        token
                    );
            }

            // Assert: round-trip fidelity — the written payload deserializes back to the same entries.
            var reloaded = new ScoDictionaryNew<TKey, TValue>().DeserializeObject(
                written,
                DefaultSettings()
            );
            reloaded.Should().NotBeNull();
            reloaded.Should().HaveCount(expectedEntries.Count);
            foreach (var kvp in expectedEntries)
            {
                reloaded.TryGetValue(kvp.Key, out var value).Should().BeTrue();
                value.Should().Be(kvp.Value);
            }
        }

        [TestMethod]
        public void DictRemap_FlatOnDiskPayload_RoundTripsWithoutWrapperTokens()
        {
            // Arrange: a representative DictRemap payload (folder-relative-path -> remapped-path).
            var payload =
                "{\"Inbox\\\\OldTeam\":\"Inbox\\\\NewTeam\",\"Archive\\\\2019\":\"Archive\\\\History\"}";
            var expected = new Dictionary<string, string>
            {
                ["Inbox\\OldTeam"] = "Inbox\\NewTeam",
                ["Archive\\2019"] = "Archive\\History",
            };

            // Act + Assert.
            AssertFlatRoundTrip(payload, expected);
        }

        [TestMethod]
        public void FilteredFolderScraping_FlatOnDiskPayload_RoundTripsWithoutWrapperTokens()
        {
            // Arrange: a representative FilteredFolderScraping payload (folder-relative-path -> flag).
            var payload = "{\"Inbox\\\\Projects\":1,\"Archive\\\\Done\":0}";
            var expected = new Dictionary<string, int>
            {
                ["Inbox\\Projects"] = 1,
                ["Archive\\Done"] = 0,
            };

            // Act + Assert.
            AssertFlatRoundTrip(payload, expected);
        }

        [TestMethod]
        public void FolderRemap_FlatOnDiskPayload_RoundTripsWithoutWrapperTokens()
        {
            // Arrange: a representative FolderRemap payload (source-path -> destination-path).
            var payload =
                "{\"Inbox\\\\Legacy\":\"Inbox\\\\Current\",\"Sent\\\\2018\":\"Sent\\\\Archive\"}";
            var expected = new Dictionary<string, string>
            {
                ["Inbox\\Legacy"] = "Inbox\\Current",
                ["Sent\\2018"] = "Sent\\Archive",
            };

            // Act + Assert.
            AssertFlatRoundTrip(payload, expected);
        }

        [TestMethod]
        public void SubjectMapEncoder_FlatOnDiskPayload_RoundTripsWithoutWrapperTokens()
        {
            // Arrange: a representative SubjectMap Encoder payload (word-token -> integer code).
            var payload = "{\"invoice\":0,\"meeting\":1,\"report\":2}";
            var expected = new Dictionary<string, int>
            {
                ["invoice"] = 0,
                ["meeting"] = 1,
                ["report"] = 2,
            };

            // Act + Assert.
            AssertFlatRoundTrip(payload, expected);
        }

        [TestMethod]
        public void DefaultWritePath_ForAllPersistedTypes_NeverEmitsGlobalsWrapperTokens()
        {
            // Arrange: construct each persisted type via the normal in-memory constructor (the default
            // write path) rather than the prohibited globals path.
            var dictRemap = new ScoDictionaryNew<string, string>
            {
                ["Inbox\\OldTeam"] = "Inbox\\NewTeam",
            };
            var folderRemap = new ScoDictionaryNew<string, string>
            {
                ["Inbox\\Legacy"] = "Inbox\\Current",
            };
            var filteredFolderScraping = new ScoDictionaryNew<string, int>
            {
                ["Inbox\\Projects"] = 1,
            };
            var encoder = new ScoDictionaryNew<string, int> { ["invoice"] = 0 };

            // Act.
            var outputs = new[]
            {
                dictRemap.SerializeToString(),
                folderRemap.SerializeToString(),
                filteredFolderScraping.SerializeToString(),
                encoder.SerializeToString(),
            };

            // Assert: the default write path for every persisted type emits a flat object and never the
            // globals ScoDictionaryConverter / PreserveReferencesHandling.All wrapper shape.
            foreach (var output in outputs)
            {
                output.Should().NotBeNullOrEmpty();
                foreach (var token in WrapperTokens)
                {
                    output
                        .Should()
                        .NotContain(
                            token,
                            "the default write path must not emit the globals-converter wrapper "
                                + "token '{0}'",
                            token
                        );
                }
            }
        }
    }
}
