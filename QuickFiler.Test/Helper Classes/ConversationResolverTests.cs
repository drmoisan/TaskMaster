using System;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Helper_Classes;
using UtilitiesCS;

namespace QuickFiler.Test.HelperClasses
{
    /// <summary>
    /// Regression tests for two interrelated bugs in ConversationResolver:
    ///
    /// Bug 1 (StackOverflow): LoadConversationInfo() constructed an exception message by
    /// dereferencing the ConversationInfo and Df properties, which are themselves lazily
    /// loaded via LoadConversationInfo(). The property access triggered a recursive call
    /// back into LoadConversationInfo(), causing a StackOverflowException instead of the
    /// intended InvalidOperationException.
    ///
    /// Bug 2 (Count sentinel): _count was a value-type Pair&lt;int&gt; whose default value
    /// (0,0) is identical to a legitimate count when both DataFrames contain zero rows.
    /// GetOrLoad compared the backing field to default(Pair&lt;int&gt;) to decide whether
    /// loading was needed; a real (0,0) result was therefore never treated as initialized,
    /// causing LoadCount() to be called on every subsequent Count access, compounding the
    /// stack depth that led to Bug 1.
    /// </summary>
    [TestClass]
    public class ConversationResolverTests
    {
        private MockRepository _mockRepository;
        private Mock<IApplicationGlobals> _mockGlobals;
        private Mock<MailItem> _mockMailItem;

        /// <summary>
        /// Shared test setup: creates a lightweight ConversationResolver that holds
        /// a non-null _mailItem so GetOrLoad proceeds past the dependency check and
        /// invokes LoadConversationInfo when ConversationInfo is accessed.
        ///
        /// No COM interactions are expected; any unexpected call to a COM method
        /// (e.g. GetConversation) would cause Moq to throw, which would surface as
        /// a test failure with a clear message.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            _mockRepository = new MockRepository(MockBehavior.Loose);
            _mockGlobals = _mockRepository.Create<IApplicationGlobals>();
            _mockMailItem = _mockRepository.Create<MailItem>();
        }

        // ─────────────────────────────────────────────────────────────
        // Bug 1 regression: StackOverflow from recursive property access
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Regression test for AC-4 (Issue #103): when Count.Expanded == 0,
        /// LoadConversationInfo() now returns a single-item fallback containing MailHelper
        /// instead of throwing InvalidOperationException.
        ///
        /// Historical context: an earlier fix replaced a StackOverflowException (caused by
        /// accessing the ConversationInfo property inside the error message) with an
        /// InvalidOperationException using nameof(). AC-2 (Issue #103) further replaced that
        /// throw with a safe return so the VSTO UI thread is not disrupted for a recoverable
        /// scenario such as Junk E-mail items with all DataFrame rows filtered out.
        /// </summary>
        [TestMethod]
        public void LoadConversationInfo_WhenCountExpandedIsZero_ReturnsSingleItemFallbackContainingMailHelper()
        {
            // Arrange – Count (0,0) puts Expanded <= 0, triggering the fallback path.
            var resolver = new ConversationResolver(_mockGlobals.Object, _mockMailItem.Object);
            // Use the internal setter to inject the zero-count state without loading
            // DataFrames from COM.
            resolver.Count = new Pair<int>(0, 0);

            // Act – call the internal loader directly so the test is deterministic and
            // does not go through the GetOrLoad dependency check on _mailItem.
            var result = resolver.LoadConversationInfo();

            // Assert – single-item fallback containing the resolver's MailHelper is returned.
            result.Expanded.Should().HaveCount(1);
            result.SameFolder.Should().HaveCount(1);
            result.Expanded[0].Should().BeSameAs(resolver.MailHelper);
        }

        /// <summary>
        /// Complementary case: accessing the ConversationInfo property when Count.Expanded
        /// is zero now returns a single-item fallback via the public API path instead of
        /// throwing. GetOrLoad stores the fallback result so subsequent reads return the
        /// cached value without re-invoking LoadConversationInfo().
        /// </summary>
        [TestMethod]
        public void ConversationInfoGetter_WhenCountExpandedIsZero_ReturnsSingleItemFallback()
        {
            // Arrange – Count (0,0) triggers the fallback path in LoadConversationInfo.
            var resolver = new ConversationResolver(_mockGlobals.Object, _mockMailItem.Object);
            resolver.Count = new Pair<int>(0, 0);

            // Act – access through the public property getter (the real consumer path).
            var result = resolver.ConversationInfo;

            // Assert – single-item fallback returned, no throw.
            result.Expanded.Should().HaveCount(1);
            result.SameFolder.Should().HaveCount(1);
            result.Expanded[0].Should().BeSameAs(resolver.MailHelper);
        }

        // ─────────────────────────────────────────────────────────────
        // Bug 2 regression: Count sentinel (0,0) treated as uninitialized
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Regression test for Count re-loading when its value is legitimately (0,0).
        ///
        /// Before the fix, GetOrLoad compared _count to default(Pair&lt;int&gt;) == (0,0).
        /// A real (0,0) count matched the uninitialized sentinel, so LoadCount() was
        /// called again. LoadCount() calls Df which invokes COM, causing an unexpected
        /// call and (in this test) a Moq exception.
        ///
        /// After the fix, _count is initialized to (-1,-1) and the isInitialized
        /// predicate is Expanded &gt;= 0, so a loaded (0,0) is correctly treated as
        /// already initialized and no further loading occurs.
        /// </summary>
        [TestMethod]
        public void Count_WhenZeroCountIsSetViaInternalSetter_SubsequentGetDoesNotInvokeLoadCount()
        {
            // Arrange – inject (0,0) as an already-computed count.
            var resolver = new ConversationResolver(_mockGlobals.Object, _mockMailItem.Object);
            resolver.Count = new Pair<int>(0, 0);

            // Act – access Count twice. Before the fix each access called LoadCount(),
            // which called Df → GetConversation() (a COM call). The Loose mock would
            // return null causing a NullReferenceException inside LoadCount.
            // After the fix both accesses hit the isInitialized predicate (Expanded >= 0)
            // and return the cached (0,0) without any COM interaction.
            var count1 = resolver.Count;
            var count2 = resolver.Count;

            // Assert – the cached value is returned on both reads.
            count1.Expanded.Should().Be(0);
            count2.Expanded.Should().Be(0);
            count1.SameFolder.Should().Be(0);
            count2.SameFolder.Should().Be(0);
        }

        /// <summary>
        /// Verifies that the uninitialized sentinel (-1,-1) correctly triggers loading
        /// when Count has not been set, confirming the predicate does not suppress
        /// legitimate loading attempts.
        ///
        /// Because LoadCount() calls Df → LoadDf() → COM, and the mock MailItem returns
        /// null from GetConversation(), we expect a NullReferenceException to propagate
        /// rather than the silent (-1,-1) default that would have occurred before the fix.
        /// </summary>
        [TestMethod]
        public void Count_WhenNotYetInitialized_AttemptsToLoadCount()
        {
            // Arrange – fresh resolver; _count starts at (-1,-1), predicate Expanded >= 0
            // evaluates to false, so GetOrLoad must invoke LoadCount.
            var resolver = new ConversationResolver(_mockGlobals.Object, _mockMailItem.Object);

            // Act / Assert – LoadCount() calls Df → LoadDf() → _mailItem.GetConversation().
            // The Loose mock returns null, so GetConversationDf() will throw when called on
            // null. Any exception other than a clean return of (-1,-1) proves loading ran.
            System.Action act = () =>
            {
                var _ = resolver.Count;
            };

            // We accept any exception type here; what matters is that Count did NOT
            // silently return the uninitialized sentinel. Confirming an exception is thrown
            // verifies that LoadCount() was actually invoked rather than short-circuited.
            act.Should()
                .Throw<System.Exception>(
                    "loading was attempted because the sentinel was not reached"
                );
        }

        // ─────────────────────────────────────────────────────────────
        // Bug 3 regression: UpdateUI read-before-write ordering in
        // LoadConversationInfoAsync (Issue #103)
        //
        // Before the fix, LoadConversationInfoAsync called:
        //   UpdateUI(ConversationInfo.Expanded)   ← reads the lazy property
        //   ConversationInfo = pair               ← assigns too late
        //
        // When Count.Expanded == 0, reading ConversationInfo.Expanded before
        // the setter fires goes through GetOrLoad → LoadConversationInfo(),
        // which throws InvalidOperationException because the guard sees
        // Count.Expanded <= 0.
        //
        // After the fix, the order is swapped:
        //   ConversationInfo = pair               ← assigned first
        //   UpdateUI(pair.Expanded)               ← uses local var, not property
        //
        // These tests verify both sides of that contract without exercising
        // the async execution context (which requires COM infrastructure).
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Regression test for the async read-before-write scenario (Issue #103 Bug 3):
        /// reading ConversationInfo before it has been set, when Count.Expanded == 0, now
        /// returns a single-item fallback instead of throwing InvalidOperationException.
        ///
        /// With AC-2 applied, even if the old async ordering bug were triggered again,
        /// the sync path would return a safe fallback rather than crashing the UI thread.
        /// </summary>
        [TestMethod]
        public void ConversationInfo_WhenNotSetAndCountIsZero_ReturnsFallbackWithoutThrowing()
        {
            // Arrange – Count (0,0) triggers the fallback path in LoadConversationInfo.
            // ConversationInfo backing field is default (null), so GetOrLoad will invoke
            // LoadConversationInfo() when the property is accessed.
            var resolver = new ConversationResolver(_mockGlobals.Object, _mockMailItem.Object);
            resolver.Count = new Pair<int>(0, 0);

            // Act – accessing the property hits GetOrLoad → LoadConversationInfo() → fallback.
            var result = resolver.ConversationInfo;

            // Assert – fallback returned, no throw.
            result.Expanded.Should().HaveCount(1);
            result.SameFolder.Should().HaveCount(1);
        }

        /// <summary>
        /// Regression test confirming the fix: after ConversationInfo is assigned directly
        /// (as LoadConversationInfoAsync now does BEFORE calling UpdateUI), accessing
        /// ConversationInfo.Expanded returns the assigned value and does NOT re-enter
        /// LoadConversationInfo(), even when Count.Expanded == 0.
        ///
        /// This validates that the fix (assign first, pass pair.Expanded to UpdateUI) is safe:
        /// the GetOrLoad cache hit avoids re-triggering the throwing synchronous loader.
        /// </summary>
        [TestMethod]
        public void ConversationInfo_WhenSetBeforeAccessWithCountAtZero_ReturnsCachedValueWithoutThrowing()
        {
            // Arrange – Count (0,0) would cause LoadConversationInfo to throw if triggered.
            var resolver = new ConversationResolver(_mockGlobals.Object, _mockMailItem.Object);
            resolver.Count = new Pair<int>(0, 0);

            // Pre-assign ConversationInfo to a non-default pair (as the fixed async code does).
            // Non-null lists cause EqualityComparer to treat the field as non-default,
            // so GetOrLoad returns the cached value without invoking LoadConversationInfo().
            var expectedList = new System.Collections.Generic.List<MailItemHelper>();
            var pair = new Pair<System.Collections.Generic.List<MailItemHelper>>(
                sameFolder: expectedList,
                expanded: expectedList
            );
            resolver.ConversationInfo = pair;

            // Act – accessing the property after assignment must return the cached value.
            System.Action act = () =>
            {
                var result = resolver.ConversationInfo;
                // Verify the returned value is the one we set, confirming no reload occurred.
                result.Expanded.Should().BeSameAs(expectedList);
                result.SameFolder.Should().BeSameAs(expectedList);
            };

            // Assert – no exception; cached value is returned.
            act.Should().NotThrow();
        }
    }
}
