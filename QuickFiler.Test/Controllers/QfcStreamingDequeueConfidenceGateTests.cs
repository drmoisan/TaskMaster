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
            Func<MailItem, CancellationToken, Task<long>> scoreLoader,
            double threshold,
            TimeProvider timeProvider = null,
            Action<string> debugLog = null,
            Func<bool> sourceActive = null,
            TimeSpan? firstBatchDeadline = null,
            Action<int, int, int> progressCallback = null
        )
        {
            Type gateType = typeof(QfcDatamodel).Assembly.GetType(
                "QuickFiler.Controllers.QfcStreamingDequeueConfidenceGate"
            );
            gateType.Should().NotBeNull("the dequeue-layer confidence gate must exist");

            // Issue #424: the gate gained an optional first-batch deadline and an optional progress
            // callback. Prefer the widest constructor; fall back to the older shapes so this helper
            // keeps compiling against a pre-#424 gate.
            ConstructorInfo constructorWithProgress = gateType.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                types: new[]
                {
                    typeof(Func<MailItem>),
                    typeof(Func<MailItem, CancellationToken, Task<long>>),
                    typeof(double),
                    typeof(TimeProvider),
                    typeof(Action<string>),
                    typeof(Func<bool>),
                    typeof(TimeSpan?),
                    typeof(Action<int, int, int>),
                },
                modifiers: null
            );
            if (constructorWithProgress != null)
            {
                return constructorWithProgress.Invoke(
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
                    }
                );
            }

            ConstructorInfo constructorWithDeadline = gateType.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                types: new[]
                {
                    typeof(Func<MailItem>),
                    typeof(Func<MailItem, CancellationToken, Task<long>>),
                    typeof(double),
                    typeof(TimeProvider),
                    typeof(Action<string>),
                    typeof(Func<bool>),
                    typeof(TimeSpan?),
                },
                modifiers: null
            );
            if (constructorWithDeadline != null)
            {
                return constructorWithDeadline.Invoke(
                    new object[]
                    {
                        tryTakeNext,
                        scoreLoader,
                        threshold,
                        timeProvider,
                        debugLog,
                        sourceActive,
                        firstBatchDeadline,
                    }
                );
            }

            ConstructorInfo constructorWithSourceState = gateType.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                types: new[]
                {
                    typeof(Func<MailItem>),
                    typeof(Func<MailItem, CancellationToken, Task<long>>),
                    typeof(double),
                    typeof(TimeProvider),
                    typeof(Action<string>),
                    typeof(Func<bool>),
                },
                modifiers: null
            );
            if (constructorWithSourceState != null)
            {
                return constructorWithSourceState.Invoke(
                    new object[]
                    {
                        tryTakeNext,
                        scoreLoader,
                        threshold,
                        timeProvider,
                        debugLog,
                        sourceActive,
                    }
                );
            }

            ConstructorInfo constructor = gateType.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                types: new[]
                {
                    typeof(Func<MailItem>),
                    typeof(Func<MailItem, CancellationToken, Task<long>>),
                    typeof(double),
                    typeof(TimeProvider),
                    typeof(Action<string>),
                },
                modifiers: null
            );
            constructor.Should().NotBeNull("the gate must expose the planned testable seam");

            return constructor.Invoke(
                new object[] { tryTakeNext, scoreLoader, threshold, timeProvider, debugLog }
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
            Action<int, int, int> progressCallback = null
        )
        {
            return CreateGate(
                () => source.Count == 0 ? null : source.Dequeue(),
                (mail, token) =>
                {
                    token.ThrowIfCancellationRequested();
                    return Task.FromResult(scores[mail]);
                },
                threshold,
                timeProvider,
                debugLog,
                sourceActive,
                firstBatchDeadline,
                progressCallback
            );
        }

        private static async Task<IList<MailItem>> DequeueAsync(
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
                (Task<IList<MailItem>>)
                    method.Invoke(gate, new object[] { quantity, timeOut, token });
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
                (mail, token) => Task.FromResult(1000L),
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
                (mail, token) => Task.FromResult(950L),
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
                (mail, token) => Task.FromResult(950L),
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
    }
}
