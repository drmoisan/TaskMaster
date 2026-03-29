using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ToDoModel.Data_Model.People;
using UtilitiesCS;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class PeopleScoDictionaryNew_Tests
    {
        [TestMethod]
        public void DefaultConstructor_ShouldCreateEmptyDictionary()
        {
            var dict = new PeopleScoDictionaryNew();

            dict.Count.Should().Be(0);
        }

        [TestMethod]
        public void Constructor_WithGlobals_ShouldSetGlobals()
        {
            var mockApp = new Mock<Microsoft.Office.Interop.Outlook.Application>();
            var globals = new TaskMaster.ApplicationGlobals(mockApp.Object, true);

            var dict = new PeopleScoDictionaryNew(globals);

            dict.Count.Should().Be(0);
        }

        [TestMethod]
        public void Prefix_SetAndGet_ShouldWork()
        {
            var dict = new PeopleScoDictionaryNew();
            var mockPrefix = new Mock<IPrefix>();
            mockPrefix.SetupGet(p => p.Value).Returns("P:");

            dict.Prefix = mockPrefix.Object;

            dict.Prefix.Should().BeSameAs(mockPrefix.Object);
        }

        [TestMethod]
        public void IsPeopleCategory_WhenStringStartsWithPrefix_ShouldReturnTrue()
        {
            var dict = new PeopleScoDictionaryNew();
            var mockPrefix = new Mock<IPrefix>();
            mockPrefix.SetupGet(p => p.Value).Returns("People:");
            dict.Prefix = mockPrefix.Object;

            dict.IsPeopleCategory("People:John").Should().BeTrue();
        }

        [TestMethod]
        public void IsPeopleCategory_WhenStringDoesNotStartWithPrefix_ShouldReturnFalse()
        {
            var dict = new PeopleScoDictionaryNew();
            var mockPrefix = new Mock<IPrefix>();
            mockPrefix.SetupGet(p => p.Value).Returns("People:");
            dict.Prefix = mockPrefix.Object;

            dict.IsPeopleCategory("Other:John").Should().BeFalse();
        }

        [TestMethod]
        public void IsPeopleCategory_WhenNull_ShouldReturnFalse()
        {
            var dict = new PeopleScoDictionaryNew();
            var mockPrefix = new Mock<IPrefix>();
            mockPrefix.SetupGet(p => p.Value).Returns("P:");
            dict.Prefix = mockPrefix.Object;

            dict.IsPeopleCategory(null).Should().BeFalse();
        }

        [TestMethod]
        public void IsPeopleCategory_WhenShorterThanPrefix_ShouldReturnFalse()
        {
            var dict = new PeopleScoDictionaryNew();
            var mockPrefix = new Mock<IPrefix>();
            mockPrefix.SetupGet(p => p.Value).Returns("People:");
            dict.Prefix = mockPrefix.Object;

            dict.IsPeopleCategory("P").Should().BeFalse();
        }

        [TestMethod]
        public void AddPrefix_WhenSeedDoesNotStartWithPrefix_ShouldPrepend()
        {
            var dict = new PeopleScoDictionaryNew();

            var result = dict.AddPrefix("John", "People:");

            result.Should().Be("People:John");
        }

        [TestMethod]
        public void AddPrefix_WhenSeedAlreadyStartsWithPrefix_ShouldReturnUnchanged()
        {
            var dict = new PeopleScoDictionaryNew();

            var result = dict.AddPrefix("People:John", "People:");

            result.Should().Be("People:John");
        }

        [TestMethod]
        public void AddPrefix_WhenSeedIsNull_ShouldThrowArgumentNullException()
        {
            var dict = new PeopleScoDictionaryNew();

            Action act = () => dict.AddPrefix(null, "P:");

            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void AddPrefix_WhenPrefixIsNull_ShouldThrowArgumentNullException()
        {
            var dict = new PeopleScoDictionaryNew();

            Action act = () => dict.AddPrefix("John", null);

            act.Should().Throw<ArgumentNullException>();
        }

        // -----------------------------------------------------------------------
        // P44-T1 — Matching prefers exact names/categories over partial matches
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that MatchToExisting returns the value provided by FindMatchingTag
        /// when an exact candidate is present in the existing-people list.
        ///
        /// Purpose:
        ///     Confirm the delegation to Globals.TD.FindMatchingTag propagates the
        ///     exact-match result back to the caller without modification.
        ///
        /// Returns:
        ///     Passes when the returned string equals the exact match supplied by the
        ///     mocked FindMatchingTag delegate.
        /// </summary>
        [TestMethod]
        public void MatchToExisting_WhenExactMatchAvailable_ReturnsExactMatchResult()
        {
            // Arrange
            var mockPrefix = new Mock<IPrefix>();
            mockPrefix.SetupGet(p => p.Value).Returns("People:");

            var mockTD = new Mock<IToDoObjects>();
            const string exactMatch = "People:John Smith";
            mockTD
                .SetupGet(x => x.FindMatchingTag)
                .Returns((cats, pref, email, search) => exactMatch);

            var mockOl = new Mock<IOlObjects>();
            mockOl.SetupGet(o => o.UserEmailAddress).Returns("user@test.com");

            var mockGlobals = new Mock<IApplicationGlobals>(MockBehavior.Loose);
            mockGlobals.SetupGet(g => g.TD).Returns(mockTD.Object);
            mockGlobals.SetupGet(g => g.Ol).Returns(mockOl.Object);

            var dict = new PeopleScoDictionaryNew(mockGlobals.Object);
            dict.Prefix = mockPrefix.Object;

            var existingPeople = new List<string> { "People:Jane Doe", exactMatch };

            // Act
            var result = dict.MatchToExisting(existingPeople, "John Smith");

            // Assert: the exact match returned by FindMatchingTag is propagated as-is
            result.Should().Be(exactMatch);
        }

        // -----------------------------------------------------------------------
        // P44-T2 — Add flow applies expected category prefix rules
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that after AddOrUpdate is called with a prefixed category value,
        /// the stored entry bears that prefix.
        ///
        /// Purpose:
        ///     Confirm that AddOrUpdate preserves the prefixed category string so
        ///     that callers relying on IsPeopleCategory can locate the entry.
        ///
        /// Returns:
        ///     Passes when the dictionary contains the key and its value starts with
        ///     the expected prefix.
        /// </summary>
        [TestMethod]
        public void AddFlow_WithPrefix_StoredEntryBearsPrefixedCategory()
        {
            // Arrange
            var dict = new PeopleScoDictionaryNew();
            const string key = "john@example.com";
            const string prefixedValue = "People:John Smith";

            // Act: simulate what AddMissingEntry does after AddPrefix is applied
            dict.AddOrUpdate(key, prefixedValue);

            // Assert: stored value carries the prefix
            dict.Should().ContainKey(key);
            dict[key].Should().StartWith("People:");
            dict[key].Should().Be(prefixedValue);
        }

        // -----------------------------------------------------------------------
        // P44-T3 — Duplicate additions are ignored or merged as coded
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that adding the same key twice updates the value in place rather
        /// than creating a second entry.
        ///
        /// Purpose:
        ///     Confirm AddOrUpdate semantics: a duplicate key results in count = 1
        ///     and the most recently supplied value is stored.
        ///
        /// Returns:
        ///     Passes when the dictionary count is 1 and the value equals the second
        ///     (update) value.
        /// </summary>
        [TestMethod]
        public void AddOrUpdate_DuplicateKey_UpdatesValueAndCountRemainsOne()
        {
            // Arrange
            var dict = new PeopleScoDictionaryNew();
            const string key = "alice@example.com";

            // Act: add then overwrite
            dict.AddOrUpdate(key, "People:Alice Original");
            dict.AddOrUpdate(key, "People:Alice Updated");

            // Assert: one entry; value reflects the update
            dict.Count.Should().Be(1);
            dict[key].Should().Be("People:Alice Updated");
        }

        // -----------------------------------------------------------------------
        // P2-T15 — Cleanup seam state between tests that inject InputBox.DialogInvoker
        // -----------------------------------------------------------------------

        /// <summary>
        /// Resets InputBox.DialogInvoker to its real implementation after each test
        /// so that seam injections in this class do not bleed into other tests.
        /// </summary>
        [TestCleanup]
        public void TestCleanup_ResetInputBoxSeam()
        {
            // Restore the real dialog so no seam injection bleeds out of this class.
            InputBox.DialogInvoker = viewer => viewer.ShowDialog();
        }

        // -----------------------------------------------------------------------
        // P2-T15 — SplitAddressToFirstLastName branches (COM-free)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that SplitAddressToFirstLastName parses a standard first.last@domain.com
        /// email into a title-cased "First Last -> Domain" string using the first-pass regex.
        ///
        /// Purpose:
        ///     This is the main mail-metadata extraction path (regex 1) used when inferring
        ///     a contact name from their email address before adding a People category.
        /// </summary>
        [TestMethod]
        public void SplitAddressToFirstLastName_WithDotFormat_ReturnsTitleCasedNameAndDomain()
        {
            // Arrange
            var dict = new PeopleScoDictionaryNew();

            // Act
            var result = dict.SplitAddressToFirstLastName("john.smith@example.com");

            // Assert: first-pass regex produces title-cased name and domain
            result.Should().Be("John Smith -> Example");
        }

        /// <summary>
        /// Verifies that SplitAddressToFirstLastName appends a middle-name segment when the
        /// first-pass regex captures a third group (the middle portion of the address).
        /// </summary>
        [TestMethod]
        public void SplitAddressToFirstLastName_WithMiddleNameSegment_IncludesMiddleNameInResult()
        {
            // Arrange
            var dict = new PeopleScoDictionaryNew();

            // Act: address with three-part local part triggers the middle-name branch
            var result = dict.SplitAddressToFirstLastName("john.smith.doe@example.com");

            // Assert: middle name is appended between first and last segments
            result.Should().Be("John Smith Doe -> Example");
        }

        /// <summary>
        /// Verifies that SplitAddressToFirstLastName falls back to the second-pass regex
        /// when the address has no dot or underscore separator in the local part.
        /// </summary>
        [TestMethod]
        public void SplitAddressToFirstLastName_WithNoSeparator_UsesFallbackRegexAndReturnsTitleCasedName()
        {
            // Arrange
            var dict = new PeopleScoDictionaryNew();

            // Act: no dot/underscore between first and second parts — triggers regex 2 path
            var result = dict.SplitAddressToFirstLastName("jsmith@company.com");

            // Assert: fallback regex extracts first char and remainder as name segments
            result.Should().Be("J Smith -> Company");
        }

        /// <summary>
        /// Verifies that SplitAddressToFirstLastName returns the original string unchanged
        /// when the address does not match either regex (no recognizable email structure).
        /// </summary>
        [TestMethod]
        public void SplitAddressToFirstLastName_WithNonEmailString_ReturnsOriginalString()
        {
            // Arrange
            var dict = new PeopleScoDictionaryNew();

            // Act: no @ and no recognized domain — neither regex matches
            var result = dict.SplitAddressToFirstLastName("notanemailaddress");

            // Assert: original string is returned unmodified
            result.Should().Be("notanemailaddress");
        }

        // -----------------------------------------------------------------------
        // P2-T15 — RefineValidateCategory cancel path (InputBox seam)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that RefineValidateCategory returns null when the InputBox dialog is
        /// cancelled, covering the cancel branch without touching Outlook COM.
        ///
        /// Purpose:
        ///     Uses the InputBox.DialogInvoker seam to simulate the user pressing Cancel,
        ///     exercising the null-return path of RefineValidateCategory.
        /// </summary>
        [TestMethod]
        public void RefineValidateCategory_WhenUserCancels_ReturnsNull()
        {
            // Arrange: InputBox seam returns Cancel so ShowDialog returns null
            InputBox.DialogInvoker = viewer => DialogResult.Cancel;

            var dict = new PeopleScoDictionaryNew();
            var mockPrefix = new Mock<IPrefix>();
            mockPrefix.SetupGet(p => p.Value).Returns("People:");

            // Act
            var result = dict.RefineValidateCategory("John Smith", mockPrefix.Object);

            // Assert: cancel path returns null
            result.Should().BeNull();
        }
    }
}
