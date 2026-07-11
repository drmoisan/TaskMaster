using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using UtilitiesCS;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Collection;
using static UtilitiesCS.Enums;

namespace UtilitiesCS.Test.EmailIntelligence
{
    /// <summary>
    /// Unit tests for <see cref="SubjectMapSco"/>.
    ///
    /// Purpose:
    ///     Verify three deterministically testable paths in SubjectMapSco using in-memory
    ///     state only (no file I/O, no Outlook COM):
    ///     (1) P36-T1: <c>Add</c> increments the lookup count for an existing token.
    ///     (2) P36-T2: <c>TryRepair</c> returns false without throwing for an absent entry
    ///         (the missing-encoding condition; entry not present in the map).
    ///     (3) P36-T3: <c>Find(key, FindBy.Folder)</c> returns only entries for the given
    ///         folder path, matching deterministically against known inputs.
    ///
    /// Constraints:
    ///     SubjectMapSco is constructed via the in-memory constructor (no filename) so
    ///     Serialize() calls are no-ops. SerializableList{string} with no entries is used
    ///     as the common-words list so tokenization is unaffected.
    /// </summary>
    [TestClass]
    public class SubjectMapSco_Tests
    {
        #region Helper: construct a minimal in-memory subject map

        /// <summary>
        /// Builds the smallest possible SubjectMapSco with no backing file and no
        /// common words, so Add/Find/TryRepair exercise pure in-memory logic.
        ///
        /// Returns:
        ///     SubjectMapSco with an empty SerializableList{string} as common words.
        /// </summary>
        private static SubjectMapSco BuildEmptyMap() =>
            new SubjectMapSco(new SerializableList<string>());

        private static SubjectMapSco BuildMapWithCommonWords(params string[] commonWords) =>
            new SubjectMapSco(new SerializableList<string>(commonWords));

        private static string CreateVirtualFilePath(string fileName) =>
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "SubjectMapScoCoverageVirtualFiles",
                fileName
            );

        private static string CreateCollectionJson(params SubjectMapEntry[] entries)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
            };

            return JsonConvert.SerializeObject(
                new ConcurrentObservableCollection<SubjectMapEntry>(entries),
                settings
            );
        }

        #endregion

        #region P36-T1 — Add increments lookup count for an existing token

        /// <summary>
        /// Verifies that adding the same subject/folder combination twice increments the
        /// EmailSubjectCount from 1 to 2 rather than creating a duplicate entry.
        ///
        /// Purpose:
        ///     Confirm the deduplication branch in Add: when an entry with the same
        ///     EmailSubject and Folderpath already exists, the count is incremented.
        ///
        /// Args:
        ///     smc: In-memory SubjectMapSco with no backing file.
        ///     "meeting" / "inbox": lowercase inputs so the tokenizer round-trips cleanly.
        ///
        /// Returns:
        ///     Passes when the found entry's EmailSubjectCount equals 2.
        /// </summary>
        [TestMethod]
        public void Add_WhenSameTokenAddedTwice_IncrementsLookupCount()
        {
            // Arrange
            var smc = BuildEmptyMap();

            // Act: add the same subject/folder pair twice — second call should increment count
            smc.Add("meeting", "inbox");
            smc.Add("meeting", "inbox");

            // Assert: only one entry exists and its count equals 2
            var entry = smc.Find("meeting", "inbox");
            entry.Should().NotBeNull();
            entry.EmailSubjectCount.Should().Be(2);
        }

        #endregion

        #region P36-T2 — TryRepair returns false for an absent entry (missing-encoding condition)

        /// <summary>
        /// Verifies that TryRepair returns false without throwing when the provided entry is
        /// not present in the map (idx == -1 path), which models the missing-encoding
        /// condition where the entry has no corresponding record in the collection.
        ///
        /// Purpose:
        ///     Confirm the boundary guard in SubjectMapSco.TryRepair: when FindIndex
        ///     returns -1 (entry absent), the method returns false gracefully and neither
        ///     throws nor modifies map state.
        ///
        /// Args:
        ///     smc: Empty in-memory map.
        ///     absentEntry: A SubjectMapEntry constructed directly (never added to smc).
        ///
        /// Returns:
        ///     Passes when TryRepair returns false.
        /// </summary>
        [TestMethod]
        public void TryRepair_WhenEntryAbsentFromMap_ReturnsFalse()
        {
            // Arrange: build two disjoint maps.
            // smc2 holds the entry; smc1 (the map under test) never had it added.
            // Constructing SubjectMapEntry directly via new SubjectMapEntry(string, int)
            // calls StripCommonWords with a null _commonWords field, causing a
            // NullReferenceException.  Use smc2.Find to retrieve a properly-initialized
            // entry that is absent from smc1.
            var smc1 = BuildEmptyMap();
            var smc2 = BuildEmptyMap();
            smc2.Add("meeting", "inbox");
            var entryFromOtherMap = smc2.Find("meeting", "inbox");

            // Act: entryFromOtherMap exists in smc2 but not in smc1 (idx == -1)
            var result = smc1.TryRepair(entryFromOtherMap);

            // Assert: absent entry → missing condition → TryRepair returns false
            result.Should().BeFalse();
        }

        #endregion

        #region P36-T3 — Find by folder returns deterministic matches for known inputs

        /// <summary>
        /// Verifies that Find(key, FindBy.Folder) returns exactly the entries whose
        /// Folderpath matches the key, excluding entries in other folders.
        ///
        /// Purpose:
        ///     Confirm the folder-filter branch of the query helper: only entries whose
        ///     Folderpath equals the supplied key are returned.
        ///
        /// Args:
        ///     smc: In-memory map populated with entries in two distinct folders.
        ///     "inbox" / "sent": two folder names used as discriminators.
        ///
        /// Returns:
        ///     Passes when querying "inbox" returns exactly the two inbox entries and no
        ///     sent-folder entries.
        /// </summary>
        [TestMethod]
        public void Find_ByFolder_ReturnsDeterministicMatchesForKnownInputs()
        {
            // Arrange: two subjects in "inbox", one subject in "sent"
            var smc = BuildEmptyMap();
            smc.Add("meeting", "inbox");
            smc.Add("report", "inbox");
            smc.Add("receipt", "sent");

            // Act: query by folder — expect exactly the inbox entries
            IList<SubjectMapEntry> inboxMatches = smc.Find("inbox", FindBy.Folder);

            // Assert: exactly two entries from inbox; no sent-folder entry leaks through
            inboxMatches.Should().HaveCount(2);
            inboxMatches.Should().OnlyContain(e => e.Folderpath == "inbox");
        }

        #endregion

        [TestMethod]
        public void Constructors_WhenSeedDataProvided_PreserveEntriesAcrossInMemoryOverloads()
        {
            // Arrange
            var commonWords = new SerializableList<string>(new[] { "the" });
            var seedEntry = new SubjectMapEntry("inbox", "meeting", 1, commonWords);

            // Act
            var fromList = new SubjectMapSco(new List<SubjectMapEntry> { seedEntry }, commonWords);
            var fromEnumerable = new SubjectMapSco(
                (IEnumerable<SubjectMapEntry>)new[] { seedEntry },
                commonWords
            );

            // Assert
            fromList.Should().ContainSingle();
            fromEnumerable.Should().ContainSingle();
            fromList.Find("meeting", "inbox").Should().NotBeNull();
            fromEnumerable.Find("meeting", "inbox").Should().NotBeNull();
        }

        [TestMethod]
        public void Constructors_WhenSerializedSourceExists_LoadEntriesAcrossFileOverloads()
        {
            // Arrange
            var commonWords = new SerializableList<string>(new[] { "the" });
            var fixturePath = CreateVirtualFilePath("subject-map.json");
            var folderPath = Path.GetDirectoryName(fixturePath)!;
            var fileName = Path.GetFileName(fixturePath);
            var fileSystem = new InMemoryCollectionFileSystem(
                new Dictionary<string, string>
                {
                    [fixturePath] = CreateCollectionJson(
                        new SubjectMapEntry("inbox", "the meeting", 1, commonWords)
                    ),
                }
            );
            ConcurrentObservableCollection<SubjectMapEntry>.AltListLoader backupLoader =
                _ => new List<SubjectMapEntry>();

            // Act
            using var scope = new CollectionDependencyScope<SubjectMapEntry>(fileSystem);
            var fromFile = new SubjectMapSco(fileName, folderPath, commonWords);
            var fromBackup = new SubjectMapSco(
                fileName,
                folderPath,
                backupLoader,
                Path.Combine(folderPath, "backup.csv"),
                false,
                commonWords
            );

            // Assert
            fromFile.Find("the meeting", FindBy.Subject).Should().ContainSingle();
            fromBackup.Find("the meeting", FindBy.Subject).Should().ContainSingle();
        }

        [TestMethod]
        public void EncodeAll_WhenRegexProvided_EncodesEveryEntry()
        {
            // Arrange
            var smc = BuildEmptyMap();
            smc.Add("meeting", "inbox");
            smc.Add("status", "sent");
            var encoder = new DeterministicEncoder();

            // Act
            smc.SetTokenizerRegex(Tokenizer.GetRegex());
            smc.EncodeAll(encoder, Tokenizer.GetRegex());

            // Assert
            smc.Should().OnlyContain(entry => entry.SubjectEncoded != null);
            smc.Should().OnlyContain(entry => entry.FolderEncoded != null);
        }

        [TestMethod]
        public void Add_WhenFolderPathIsNull_SwallowsArgumentNullException()
        {
            // Arrange
            var smc = BuildEmptyMap();

            // Act
            var action = () => smc.Add("meeting", null);

            // Assert
            action.Should().NotThrow();
            smc.Should().BeEmpty();
        }

        [TestMethod]
        public void Add_WhenSubjectHasNoTokens_SwallowsInvalidOperationException()
        {
            // Arrange
            var smc = BuildEmptyMap();

            // Act
            var action = () => smc.Add("", "inbox");

            // Assert
            action.Should().NotThrow();
            smc.Should().BeEmpty();
        }

        [TestMethod]
        public void Find_BySubject_WhenEntryMissing_ReturnsNullOrNormalizedMatches()
        {
            // Arrange
            var smc = BuildMapWithCommonWords("the");
            smc.Add("the meeting", "inbox");

            // Act
            var matches = smc.Find("the meeting", FindBy.Subject);
            var missing = smc.Find("missing", "inbox");

            // Assert
            matches.Should().ContainSingle();
            matches[0].Folderpath.Should().Be("inbox");
            missing.Should().BeNull();
        }

        [TestMethod]
        public void TryRepair_WhenEntryPresentAndEncoderIsAvailable_ReturnsTrue()
        {
            // Arrange
            var smc = BuildEmptyMap();
            smc.Add("meeting", "inbox");
            var entry = smc.Find("meeting", "inbox");
            entry.Encoder = new DeterministicEncoder();

            // Act
            var result = smc.TryRepair(entry);

            // Assert
            result.Should().BeTrue();
        }

        private sealed class InMemoryCollectionFileSystem
            : IConcurrentObservableCollectionFileSystem
        {
            private readonly IReadOnlyDictionary<string, string> _files;

            public InMemoryCollectionFileSystem(IReadOnlyDictionary<string, string> files)
            {
                _files = files;
            }

            public bool Exists(string filePath) => _files.ContainsKey(filePath);

            public string ReadAllText(string filePath)
            {
                if (_files.TryGetValue(filePath, out var contents))
                {
                    return contents;
                }

                throw new FileNotFoundException($"Virtual file not found: {filePath}", filePath);
            }

            public StreamWriter CreateText(string filePath)
            {
                throw new NotSupportedException("Virtual file system is read-only for this test.");
            }
        }

        private sealed class DeterministicEncoder : ISubjectMapEncoder
        {
            public IScoDictionary<string, int> Encoder =>
                throw new NotSupportedException("Encoder dictionary is not needed for this test.");

            public void AugmentTokenDict(string[] tokens) { }

            public void AugmentTokenDict(string text) { }

            public string Decode(int[] encodedWords)
            {
                throw new NotSupportedException("Decode is not needed for this test.");
            }

            public int[] Encode(string text)
            {
                return text.Tokenize().Select((_, index) => index + 1).ToArray();
            }

            public int[] Encode(string[] words)
            {
                return words.Select((_, index) => index + 1).ToArray();
            }

            public void RebuildEncoding(SubjectMapSco map)
            {
                throw new NotSupportedException("RebuildEncoding is not needed for this test.");
            }

            public void RebuildEncoding()
            {
                throw new NotSupportedException("RebuildEncoding is not needed for this test.");
            }
        }

        private sealed class CollectionDependencyScope<T> : IDisposable
        {
            private readonly IConcurrentObservableCollectionFileSystem _originalFileSystem;
            private readonly IConcurrentObservableCollectionPrompt _originalPrompt;

            public CollectionDependencyScope(
                IConcurrentObservableCollectionFileSystem fileSystem,
                IConcurrentObservableCollectionPrompt prompt = null
            )
            {
                _originalFileSystem = ConcurrentObservableCollection<T>.FileSystem;
                _originalPrompt = ConcurrentObservableCollection<T>.Prompt;
                ConcurrentObservableCollection<T>.FileSystem = fileSystem;
                if (prompt is not null)
                {
                    ConcurrentObservableCollection<T>.Prompt = prompt;
                }
            }

            public void Dispose()
            {
                ConcurrentObservableCollection<T>.FileSystem = _originalFileSystem;
                ConcurrentObservableCollection<T>.Prompt = _originalPrompt;
            }
        }
    }
}
