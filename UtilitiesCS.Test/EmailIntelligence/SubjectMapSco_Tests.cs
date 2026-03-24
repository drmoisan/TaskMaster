using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;
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
    }
}
