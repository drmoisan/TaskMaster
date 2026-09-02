using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    [TestClass]
    public partial class QfcStreamingDequeueConfidenceGateTests
    {
        private static MailItem CreateMailItem(string subject, string entryId)
        {
            var mail = new Mock<MailItem>(MockBehavior.Loose);
            mail.SetupGet(x => x.Subject).Returns(subject);
            mail.SetupGet(x => x.EntryID).Returns(entryId);
            return mail.Object;
        }

        private static object CreateGate(
            Func<MailItem> tryTakeNext,
            Func<
                MailItem,
                CancellationToken,
                Task<(long Score, string TopFolder, IFolderSearchHandler Handler)>
            > scoreLoader,
            double threshold,
            TimeProvider timeProvider = null,
            Action<string> debugLog = null,
            Func<bool> sourceActive = null,
            TimeSpan? firstBatchDeadline = null,
            Action<int, int, int> progressCallback = null,
            Action<MailItem> onRejected = null
        )
        {
            Type gateType = typeof(QfcDatamodel).Assembly.GetType(
                "QuickFiler.Controllers.QfcStreamingDequeueConfidenceGate"
            );
            gateType.Should().NotBeNull("the dequeue-layer confidence gate must exist");

            // Issue #446: one exact lookup for the widest declared constructor, guarded so the
            // helper fails CLOSED. The former four-step descending fallback chain failed OPEN:
            // when the wider lookups missed it silently succeeded on the five-type shape and
            // constructed a gate with sourceActive null, the default deadline and no progress
            // callback, across every consuming test method in this class.
            ConstructorInfo constructor = gateType.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                types: new[]
                {
                    typeof(Func<MailItem>),
                    typeof(Func<
                        MailItem,
                        CancellationToken,
                        Task<(long Score, string TopFolder, IFolderSearchHandler Handler)>
                    >),
                    typeof(double),
                    typeof(TimeProvider),
                    typeof(Action<string>),
                    typeof(Func<bool>),
                    typeof(TimeSpan?),
                    typeof(Action<int, int, int>),
                    typeof(Action<MailItem>),
                },
                modifiers: null
            );
            constructor
                .Should()
                .NotBeNull("the gate must expose the nine-parameter testable constructor seam");

            return constructor.Invoke(
                new object[]
                {
                    tryTakeNext,
                    scoreLoader,
                    threshold,
                    timeProvider,
                    debugLog,
                    sourceActive,
                    firstBatchDeadline,
                    progressCallback,
                    onRejected,
                }
            );
        }

        private static object CreateGate(
            Queue<MailItem> source,
            IDictionary<MailItem, long> scores,
            double threshold = 0.90,
            TimeProvider timeProvider = null,
            Action<string> debugLog = null,
            Func<bool> sourceActive = null,
            TimeSpan? firstBatchDeadline = null,
            Action<int, int, int> progressCallback = null,
            Action<MailItem> onRejected = null
        )
        {
            return CreateGate(
                () => source.Count == 0 ? null : source.Dequeue(),
                (mail, token) =>
                {
                    token.ThrowIfCancellationRequested();
                    return Scored(scores[mail]);
                },
                threshold,
                timeProvider,
                debugLog,
                sourceActive,
                firstBatchDeadline,
                progressCallback,
                onRejected
            );
        }

        private static async Task<IList<MailItem>> DequeueAsync(
            object gate,
            int quantity,
            int timeOut,
            CancellationToken token
        )
        {
            // Issue #446: the gate now returns a QfcGateBatch. Project Accepted back to
            // IList<MailItem> so the pre-existing gate tests keep their current shape; the
            // stop-reason and folder-carrying assertions use DequeueBatchAsync instead.
            QfcGateBatch batch = await DequeueBatchAsync(gate, quantity, timeOut, token)
                .ConfigureAwait(false);
            return batch.Accepted.Select(x => x.MailItem).ToList();
        }

        private static async Task<QfcGateBatch> DequeueBatchAsync(
            object gate,
            int quantity,
            int timeOut,
            CancellationToken token
        )
        {
            MethodInfo method = gate.GetType()
                .GetMethod(
                    "DequeueAsync",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    binder: null,
                    types: new[] { typeof(int), typeof(int), typeof(CancellationToken) },
                    modifiers: null
                );
            method.Should().NotBeNull("the gate must expose the planned dequeue operation");

            var task =
                (Task<QfcGateBatch>)method.Invoke(gate, new object[] { quantity, timeOut, token });
            return await task.ConfigureAwait(false);
        }

        [TestMethod]
        public async Task DequeueAsync_UsesDequeueTimeScoreSelection_AndLogsScoreContext()
        {
            var item = CreateMailItem("keep", "entry-keep");
            var source = new Queue<MailItem>(new[] { item });
            var scores = new Dictionary<MailItem, long> { [item] = 950 };
            var logs = new List<string>();
            object gate = CreateGate(source, scores, debugLog: logs.Add);

            IList<MailItem> result = await DequeueAsync(gate, 1, 0, CancellationToken.None);

            result.Should().ContainSingle().Which.Should().BeSameAs(item);
            logs.Should()
                .ContainSingle(log =>
                    log.Contains("QfcStreamingDequeueConfidenceGate.DequeueAsync")
                    && log.Contains("Subject='keep'")
                    && log.Contains("EntryID='entry-keep'")
                    && log.Contains("Score=950")
                );
        }

        [TestMethod]
        public async Task DequeueAsync_ScansManyToYieldFew_BackfillsUntilQuantityMet()
        {
            var low = CreateMailItem("low", "entry-low");
            var highOne = CreateMailItem("high-1", "entry-high-1");
            var highTwo = CreateMailItem("high-2", "entry-high-2");
            var source = new Queue<MailItem>(new[] { low, highOne, highTwo });
            var scores = new Dictionary<MailItem, long>
            {
                [low] = 899,
                [highOne] = 900,
                [highTwo] = 950,
            };
            object gate = CreateGate(source, scores);

            IList<MailItem> result = await DequeueAsync(gate, 2, 0, CancellationToken.None);

            result.Should().Equal(highOne, highTwo);
        }

        [TestMethod]
        public async Task DequeueAsync_SourceExhaustion_ReturnsEmptyAndPartialResults()
        {
            object emptyGate = CreateGate(new Queue<MailItem>(), new Dictionary<MailItem, long>());
            IList<MailItem> empty = await DequeueAsync(emptyGate, 2, 0, CancellationToken.None);
            empty.Should().BeEmpty();

            var only = CreateMailItem("only", "entry-only");
            object partialGate = CreateGate(
                new Queue<MailItem>(new[] { only }),
                new Dictionary<MailItem, long> { [only] = 990 }
            );
            IList<MailItem> partial = await DequeueAsync(partialGate, 2, 0, CancellationToken.None);
            partial.Should().ContainSingle().Which.Should().BeSameAs(only);
        }

        [TestMethod]
        public async Task DequeueAsync_ThresholdComparisonIsInclusive()
        {
            var item = CreateMailItem("boundary", "entry-boundary");
            object gate = CreateGate(
                new Queue<MailItem>(new[] { item }),
                new Dictionary<MailItem, long> { [item] = 900 },
                threshold: 0.90
            );

            IList<MailItem> result = await DequeueAsync(gate, 1, 0, CancellationToken.None);

            result.Should().ContainSingle().Which.Should().BeSameAs(item);
        }

        [TestMethod]
        public async Task DequeueAsync_PropagatesCancellationBeforeTakingSourceItem()
        {
            object gate = CreateGate(
                () => throw new AssertFailedException("source must not be read after cancellation"),
                (mail, token) => Scored(1000L),
                threshold: 0.90
            );
            using (var cts = new CancellationTokenSource())
            {
                cts.Cancel();

                Func<Task> act = () => DequeueAsync(gate, 1, 0, cts.Token);

                await act.Should().ThrowAsync<OperationCanceledException>();
            }
        }

        [TestMethod]
        public async Task DequeueAsync_BelowThresholdItemsAreDiscarded()
        {
            var item = CreateMailItem("discard", "entry-discard");
            object gate = CreateGate(
                new Queue<MailItem>(new[] { item }),
                new Dictionary<MailItem, long> { [item] = 899 }
            );

            IList<MailItem> result = await DequeueAsync(gate, 1, 0, CancellationToken.None);

            result.Should().BeEmpty();
        }

        [TestMethod]
        public async Task DequeueAsync_WhenSourceInitiallyEmpty_WaitsWithTimeProviderBeforeRetry()
        {
            var item = CreateMailItem("delayed", "entry-delayed");
            var fakeTime = new FakeTimeProvider();
            var takeCount = 0;
            object gate = CreateGate(
                () =>
                {
                    takeCount++;
                    return takeCount == 1 ? null : item;
                },
                (mail, token) => Scored(950L),
                threshold: 0.90,
                timeProvider: fakeTime
            );

            Task<IList<MailItem>> pending = DequeueAsync(gate, 1, 200, CancellationToken.None);
            pending.IsCompleted.Should().BeFalse();

            fakeTime.Advance(TimeSpan.FromMilliseconds(200));
            IList<MailItem> result = await pending;

            result.Should().ContainSingle().Which.Should().BeSameAs(item);
        }

        [TestMethod]
        public async Task DequeueAsync_SourceActiveAfterRepeatedEmptyReads_ContinuesPollingUntilCandidateArrives()
        {
            var item = CreateMailItem("late-qualifier", "entry-late-qualifier");
            var fakeTime = new FakeTimeProvider();
            var takeCount = 0;
            object gate = CreateGate(
                () =>
                {
                    takeCount++;
                    return takeCount < 3 ? null : item;
                },
                (mail, token) => Scored(950L),
                threshold: 0.90,
                timeProvider: fakeTime,
                sourceActive: () => takeCount < 3
            );

            Task<IList<MailItem>> pending = DequeueAsync(gate, 1, 200, CancellationToken.None);
            pending.IsCompleted.Should().BeFalse();

            fakeTime.Advance(TimeSpan.FromMilliseconds(200));
            await Task.Yield();
            pending
                .IsCompleted.Should()
                .BeFalse(
                    "the source is still active, so an empty poll must not be treated as exhaustion"
                );

            fakeTime.Advance(TimeSpan.FromMilliseconds(200));
            IList<MailItem> result = await pending;

            result.Should().ContainSingle().Which.Should().BeSameAs(item);
        }

        [TestMethod]
        public async Task DequeueAsync_InitialScreenNonEmptyAcceptedPrefixPastDeadline_ReturnsSevenInQueueOrder()
        {
            IList<MailItem> result = await DequeuePastDeadlineQualifiersAsync(7);

            result
                .Select(mail => mail.Subject)
                .Should()
                .Equal(Enumerable.Range(1, 7).Select(i => $"high-{i}"));
        }

        [TestMethod]
        public async Task DequeueAsync_SubsequentScreenNonEmptyAcceptedPrefixPastDeadline_ReturnsEightInQueueOrder()
        {
            IList<MailItem> result = await DequeuePastDeadlineQualifiersAsync(8);

            result
                .Select(mail => mail.Subject)
                .Should()
                .Equal(Enumerable.Range(1, 8).Select(i => $"high-{i}"));
        }

        /// <summary>
        /// Issue #426. A candidate the gate discards has already been removed from the source
        /// queue and never reaches the accepted-path unhook, so the gate must report it exactly
        /// once through the rejection sink. Asserting the invocation count is what makes this a
        /// real gate: a test that only asserted the item was discarded would pass vacuously.
        /// </summary>
        [TestMethod]
        public async Task DequeueAsync_BelowThresholdCandidate_InvokesOnRejectedOnce()
        {
            // Arrange
            var item = CreateMailItem("reject", "entry-reject");
            var rejected = new List<MailItem>();
            object gate = CreateGate(
                new Queue<MailItem>(new[] { item }),
                new Dictionary<MailItem, long> { [item] = 899 },
                onRejected: rejected.Add
            );

            // Act
            IList<MailItem> result = await DequeueAsync(gate, 1, 0, CancellationToken.None);

            // Assert
            result.Should().BeEmpty("the drop-on-reject contract is unchanged");
            rejected
                .Should()
                .ContainSingle("the gate must report each discarded candidate exactly once")
                .Which.Should()
                .BeSameAs(item);
        }

        /// <summary>
        /// Issue #426. A failing move monitor must not abort the dequeue scan. Drives one
        /// below-cutoff candidate whose rejection sink throws, followed by an above-cutoff
        /// candidate, and asserts both that the sink was invoked exactly once and that the scan
        /// went on to accept the second candidate. Asserting the invocation count is what makes
        /// this a real gate; a test that only asserted the scan continued would pass vacuously
        /// while no sink exists.
        /// </summary>
        [TestMethod]
        public async Task DequeueAsync_OnRejectedThrows_ScanContinues()
        {
            // Arrange
            var low = CreateMailItem("low", "entry-low");
            var high = CreateMailItem("high", "entry-high");
            var invocations = new List<MailItem>();
            object gate = CreateGate(
                new Queue<MailItem>(new[] { low, high }),
                new Dictionary<MailItem, long> { [low] = 899, [high] = 950 },
                onRejected: mail =>
                {
                    invocations.Add(mail);
                    throw new InvalidOperationException("monitor unavailable");
                }
            );

            // Act
            IList<MailItem> result = await DequeueAsync(gate, 1, 0, CancellationToken.None);

            // Assert
            invocations
                .Should()
                .ContainSingle("the throwing sink must still be invoked once for the rejected item")
                .Which.Should()
                .BeSameAs(low);
            result
                .Should()
                .ContainSingle("a sink failure must not abort the scan")
                .Which.Should()
                .BeSameAs(high);
        }

        /// <summary>
        /// Issue #426 negative control (AC13). An accepted candidate is unhooked on the accepted
        /// path by <c>UnhookDequeuedNodes</c>, so the rejection sink must not fire for it; a
        /// second release would be a double unhook. Green in both the pre-fix and post-fix states
        /// by construction, so it is not tagged expect-fail.
        /// </summary>
        [TestMethod]
        public async Task DequeueAsync_AcceptedCandidate_DoesNotInvokeOnRejected()
        {
            // Arrange
            var item = CreateMailItem("accept", "entry-accept");
            var rejected = new List<MailItem>();
            object gate = CreateGate(
                new Queue<MailItem>(new[] { item }),
                new Dictionary<MailItem, long> { [item] = 950 },
                onRejected: rejected.Add
            );

            // Act
            IList<MailItem> result = await DequeueAsync(gate, 1, 0, CancellationToken.None);

            // Assert
            result.Should().ContainSingle().Which.Should().BeSameAs(item);
            rejected
                .Should()
                .BeEmpty(
                    "an accepted candidate is released on the accepted path, not the rejection path"
                );
        }

        private static async Task<IList<MailItem>> DequeuePastDeadlineQualifiersAsync(int quantity)
        {
            var qualifiers = Enumerable
                .Range(1, quantity)
                .Select(i => CreateMailItem($"high-{i}", $"entry-high-{i}"))
                .ToList();
            var rejected = Enumerable
                .Range(1, 40)
                .Select(i => CreateMailItem($"low-{i}", $"entry-low-{i}"));
            var source = new Queue<MailItem>(
                new[] { qualifiers[0] }.Concat(rejected).Concat(qualifiers.Skip(1))
            );
            var fakeTime = new FakeTimeProvider();
            object gate = CreateGate(
                () => source.Dequeue(),
                (mail, token) =>
                {
                    fakeTime.Advance(TimeSpan.FromSeconds(1));
                    return Scored(qualifiers.Contains(mail) ? 950L : 100L);
                },
                threshold: 0.90,
                timeProvider: fakeTime,
                sourceActive: () => source.Count > 0,
                firstBatchDeadline: TimeSpan.FromSeconds(3)
            );

            return await DequeueAsync(gate, quantity, 0, CancellationToken.None);
        }
    }
}
