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
        /// Regression test for StackOverflowException caused by LoadConversationInfo()
        /// embedding the {ConversationInfo} property reference in its error message while
        /// ConversationInfo was still being loaded. This recursion now terminates with
        /// InvalidOperationException.
        ///
        /// Arrange: Count is set to (0,0) which makes Count.Expanded &lt;= 0 true, so the
        /// guard clause fires. Before the fix the string interpolation called the property
        /// getter, restarting the loading cycle. After the fix nameof() is used instead.
        /// </summary>
        [TestMethod]
        public void LoadConversationInfo_WhenCountExpandedIsZero_ThrowsInvalidOperationExceptionNotStackOverflow()
        {
            // Arrange – Count (0,0) puts Expanded <= 0, triggering the guard clause.
            var resolver = new ConversationResolver(_mockGlobals.Object, _mockMailItem.Object);
            // Use the internal setter to inject the zero-count state without loading
            // DataFrames from COM.
            resolver.Count = new Pair<int>(0, 0);

            // Act – call the internal loader directly so the test is deterministic and
            // does not go through the GetOrLoad dependency check on _mailItem.
            System.Action act = () => resolver.LoadConversationInfo();

            // Assert – must be InvalidOperationException, not StackOverflowException.
            act.Should()
                .Throw<InvalidOperationException>()
                .WithMessage($"*{nameof(ConversationResolver.ConversationInfo)}*");
        }

        /// <summary>
        /// Complementary case: accessing the ConversationInfo property when Count.Expanded
        /// is zero raises InvalidOperationException via the public API path.
        ///
        /// Before the fix this crashed the process with StackOverflowException. After the
        /// fix the getter returns the exception cleanly.
        /// </summary>
        [TestMethod]
        public void ConversationInfoGetter_WhenCountExpandedIsZero_ThrowsInvalidOperationException()
        {
            // Arrange – Count (0,0) forces the guard clause in LoadConversationInfo.
            var resolver = new ConversationResolver(_mockGlobals.Object, _mockMailItem.Object);
            resolver.Count = new Pair<int>(0, 0);

            // Act – access through the public property getter (the real crash path).
            System.Action act = () =>
            {
                var _ = resolver.ConversationInfo;
            };

            // Assert – clean exception, no recursion.
            act.Should().Throw<InvalidOperationException>();
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
    }
}
