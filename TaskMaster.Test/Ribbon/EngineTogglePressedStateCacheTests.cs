using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskMaster.Test.Ribbon
{
    /// <summary>
    /// Unit tests for the versioned pressed-state cache extracted from the engine toggle state
    /// coordinator in issue #735: the monotonic ticket source, the synchronous read, and the
    /// compare-and-apply store that refuses a write whose observation began earlier than one
    /// already recorded.
    /// </summary>
    /// <remarks>
    /// Every case here is fully synchronous and deterministic. No test sleeps, polls, reads the
    /// wall clock, touches the filesystem, creates a temporary file, or starts a message pump.
    /// </remarks>
    [TestClass]
    public class EngineTogglePressedStateCacheTests
    {
        private const string SpamEngine = "Spam";
        private const string TriageEngine = "Triage";

        #region NextSequence

        [TestMethod]
        public void NextSequence_OnSuccessiveCalls_ReturnsStrictlyIncreasingTickets()
        {
            // Arrange
            var cache = new EngineTogglePressedStateCache();

            // Act
            var first = cache.NextSequence();
            var second = cache.NextSequence();
            var third = cache.NextSequence();

            // Assert
            second
                .Should()
                .BeGreaterThan(first, "each ticket must be newer than the one issued before it");
            third.Should().BeGreaterThan(second, "the counter is monotonic across every call");
        }

        [TestMethod]
        public void NextSequence_IsSharedAcrossKeys_SoTicketsAreGloballyOrdered()
        {
            // Arrange: a single process-wide counter is sufficient because tickets are only ever
            // compared within a key, but they must still be globally ordered.
            var cache = new EngineTogglePressedStateCache();

            // Act
            var spamTicket = cache.NextSequence();
            var triageTicket = cache.NextSequence();

            // Assert
            triageTicket
                .Should()
                .BeGreaterThan(
                    spamTicket,
                    "one counter serves every key, so a later call always wins"
                );
        }

        #endregion NextSequence

        #region TryGetActive

        [TestMethod]
        public void TryGetActive_ForKeyWithNoObservation_ReturnsFalseAndFalse()
        {
            // Arrange
            var cache = new EngineTogglePressedStateCache();

            // Act
            var found = cache.TryGetActive(SpamEngine, out var active);

            // Assert
            found.Should().BeFalse("the key has never been primed");
            active.Should().BeFalse("an unprimed key reports unchecked");
        }

        [TestMethod]
        public void TryGetActive_AfterAppliedWrite_ReturnsTheStoredValue()
        {
            // Arrange
            var cache = new EngineTogglePressedStateCache();
            cache.TryApplyState(SpamEngine, true, cache.NextSequence());

            // Act
            var found = cache.TryGetActive(SpamEngine, out var active);

            // Assert
            found.Should().BeTrue("an observation is now cached for the key");
            active.Should().BeTrue("the cached value is the one that was applied");
        }

        [TestMethod]
        public void TryGetActive_IsOrdinalAndCaseSensitive()
        {
            // Arrange
            var cache = new EngineTogglePressedStateCache();
            cache.TryApplyState(SpamEngine, true, cache.NextSequence());

            // Act
            var found = cache.TryGetActive("spam", out var active);

            // Assert: the dictionary is built with an ordinal comparer, so "spam" is not "Spam".
            found.Should().BeFalse("keys are compared ordinally and case-sensitively");
            active.Should().BeFalse("a miss reports unchecked");
        }

        #endregion TryGetActive

        #region TryApplyState

        [TestMethod]
        public void TryApplyState_OnFirstObservationForAKey_AppliesAndReportsApplied()
        {
            // Arrange
            var cache = new EngineTogglePressedStateCache();
            var ticket = cache.NextSequence();

            // Act
            var applied = cache.TryApplyState(SpamEngine, true, ticket);

            // Assert
            applied.Should().BeTrue("no observation was cached, so the write lands");
            cache.TryGetActive(SpamEngine, out var active).Should().BeTrue();
            active.Should().BeTrue();
        }

        [TestMethod]
        public void TryApplyState_WithNewerTicket_OverwritesAndReportsApplied()
        {
            // Arrange
            var cache = new EngineTogglePressedStateCache();
            var older = cache.NextSequence();
            var newer = cache.NextSequence();
            cache.TryApplyState(SpamEngine, false, older);

            // Act
            var applied = cache.TryApplyState(SpamEngine, true, newer);

            // Assert
            applied.Should().BeTrue("a strictly newer observation supersedes the cached one");
            cache.TryGetActive(SpamEngine, out var active).Should().BeTrue();
            active.Should().BeTrue("the newer value is the one now cached");
        }

        [TestMethod]
        public void TryApplyState_WithOlderTicket_IsRejectedAndLeavesTheCachedValue()
        {
            // Arrange: this is the #525 defect in miniature — a stale observation resolving late.
            var cache = new EngineTogglePressedStateCache();
            var older = cache.NextSequence();
            var newer = cache.NextSequence();
            cache.TryApplyState(SpamEngine, true, newer);

            // Act
            var applied = cache.TryApplyState(SpamEngine, false, older);

            // Assert
            applied
                .Should()
                .BeFalse(
                    "an observation that began earlier must not overwrite a newer one, and the "
                        + "caller relies on this result to skip a redundant invalidation"
                );
            cache.TryGetActive(SpamEngine, out var active).Should().BeTrue();
            active.Should().BeTrue("the newer value survives");
        }

        [TestMethod]
        public void TryApplyState_WithEqualTicket_IsRejected()
        {
            // Arrange: the comparison is "strictly newer wins", so an equal ticket loses. Only one
            // writer can ever hold a given ticket, so this is a boundary check on the comparison
            // rather than a reachable production interleaving.
            var cache = new EngineTogglePressedStateCache();
            var ticket = cache.NextSequence();
            cache.TryApplyState(SpamEngine, true, ticket);

            // Act
            var applied = cache.TryApplyState(SpamEngine, false, ticket);

            // Assert
            applied.Should().BeFalse("an equal ticket is not strictly newer");
            cache.TryGetActive(SpamEngine, out var active).Should().BeTrue();
            active.Should().BeTrue("the first write for that ticket stands");
        }

        [TestMethod]
        public void TryApplyState_KeepsKeysIndependent()
        {
            // Arrange: tickets are global but comparisons are per-key, so a newer ticket on one key
            // must not suppress a write to a different key.
            var cache = new EngineTogglePressedStateCache();
            var spamTicket = cache.NextSequence();
            var triageTicket = cache.NextSequence();
            cache.TryApplyState(TriageEngine, true, triageTicket);

            // Act
            var applied = cache.TryApplyState(SpamEngine, true, spamTicket);

            // Assert
            applied.Should().BeTrue("a newer ticket on another key says nothing about this key");
            cache.TryGetActive(SpamEngine, out var spamActive).Should().BeTrue();
            spamActive.Should().BeTrue();
            cache.TryGetActive(TriageEngine, out var triageActive).Should().BeTrue();
            triageActive.Should().BeTrue("the other key is undisturbed");
        }

        #endregion TryApplyState
    }
}
