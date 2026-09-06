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
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Issue #424 first-batch-deadline coverage for <c>QfcStreamingDequeueConfidenceGate</c>. Second
    /// part of the partial class declared in <c>QfcStreamingDequeueConfidenceGateTests.cs</c>, split
    /// to keep both files under the 500-line limit. <c>[TestClass]</c> stays on the base file only:
    /// it is <c>AllowMultiple = false</c>, so repeating it here would be CS0579. Shares the base
    /// file's reflection-based <c>CreateGate</c> / <c>DequeueAsync</c> helpers. Deterministic —
    /// <see cref="FakeTimeProvider"/> for all time, mocked <see cref="MailItem"/>, no COM, no sleeps.
    /// </summary>
    public partial class QfcStreamingDequeueConfidenceGateTests
    {
        /// <summary>Cutoff for the 0.90 threshold used here (threshold x 1000).</summary>
        private const long Cutoff = 900;

        /// <summary>The two deadline configurations cancellation must behave identically under.</summary>
        private static IEnumerable<TimeSpan?> DeadlineConfigurations =>
            new TimeSpan?[] { null, Timeout.InfiniteTimeSpan };

        /// <summary>
        /// Builds a gate over <paramref name="candidateCount"/> candidates that all score below the
        /// cutoff, each score consuming one second of the budget. <paramref name="source"/> and
        /// <paramref name="takeCounter"/> expose the residual queue and the take count.
        /// <para>
        /// Issue #791 made <paramref name="deadline"/> optional (it is now an advisory checkpoint)
        /// and added <paramref name="maxScanWithoutAcceptance"/> (the bound that now terminates a
        /// zero-acceptance scan). Both are forwarded unchanged. The optional parameters trail the
        /// <c>out</c> ones because C# requires optional parameters last.
        /// </para>
        /// </summary>
        private static object CreateLowYieldGate(
            int candidateCount,
            FakeTimeProvider fakeTime,
            out Queue<MailItem> source,
            out Func<int> takeCounter,
            TimeSpan? deadline = null,
            int? maxScanWithoutAcceptance = null
        )
        {
            source = new Queue<MailItem>(
                Enumerable
                    .Range(1, candidateCount)
                    .Select(i => CreateMailItem($"reject-{i}", $"entry-reject-{i}"))
            );
            Queue<MailItem> localSource = source;
            var takes = 0;
            takeCounter = () => takes;

            return CreateGate(
                () =>
                {
                    takes++;
                    return localSource.Count == 0 ? null : localSource.Dequeue();
                },
                (mail, token) =>
                {
                    fakeTime.Advance(TimeSpan.FromSeconds(1));
                    return Scored(100L);
                },
                threshold: 0.90,
                timeProvider: fakeTime,
                sourceActive: () => false,
                firstBatchDeadline: deadline,
                maxScanWithoutAcceptance: maxScanWithoutAcceptance
            );
        }

        /// <summary>
        /// Issue #424 regression test, retargeted by issue #791. A low-yield stream (1 qualifier in
        /// 50) at 1 s per score was bounded by the 12 s default deadline under #424; #791 made that
        /// deadline advisory, so it now continues past it to the qualifier at position 40 and on to
        /// source exhaustion. The intent is preserved, with the superseding outcome asserted.
        /// </summary>
        [TestMethod]
        public async Task DequeueAsync_LowYieldStream_ContinuesPastDefaultDeadlineToTheQualifier()
        {
            // Arrange
            const int candidateCount = 50;
            const int qualifyingPosition = 40; // 1-based position of the only qualifier
            var candidates = Enumerable
                .Range(1, candidateCount)
                .Select(i => CreateMailItem($"candidate-{i}", $"entry-{i}"))
                .ToList();
            MailItem qualifying = candidates[qualifyingPosition - 1];
            var source = new Queue<MailItem>(candidates);
            var fakeTime = new FakeTimeProvider();
            var takeCount = 0;
            var acceptedSoFar = new List<MailItem>();

            object gate = CreateGate(
                () =>
                {
                    takeCount++;
                    return source.Count == 0 ? null : source.Dequeue();
                },
                (mail, token) =>
                {
                    long score = ReferenceEquals(mail, qualifying) ? 950L : 100L;
                    if (score >= Cutoff)
                    {
                        acceptedSoFar.Add(mail);
                    }

                    // Each score costs a full second of the first-batch budget.
                    fakeTime.Advance(TimeSpan.FromSeconds(1));
                    return Scored(score);
                },
                threshold: 0.90,
                timeProvider: fakeTime,
                sourceActive: () => false
            );

            // Act
            QfcGateBatch batch = await DequeueBatchAsync(gate, 5, 0, CancellationToken.None);
            IList<MailItem> result = batch.Accepted.Select(x => x.MailItem).ToList();

            // Assert
            takeCount
                .Should()
                .Be(candidateCount + 1, "the advisory checkpoint no longer bounds the scan");
            result.Should().Equal(acceptedSoFar).And.Equal(qualifying);
            batch.Stop.Should().Be(QfcDequeueStop.SourceExhausted, "neither bound was reached");
        }

        /// <summary>
        /// Issue #424 AC 2, retargeted by issue #791. Zero acceptances at the checkpoint returned an
        /// empty list at the bound under #424; #791 supersedes that, so the same low-yield stream is
        /// now scanned to source exhaustion instead of truncating after three candidates.
        /// </summary>
        [TestMethod]
        public async Task DequeueAsync_ZeroAcceptedAtCheckpoint_ContinuesToSourceExhaustion()
        {
            // Arrange
            const int candidateCount = 20;
            var fakeTime = new FakeTimeProvider();
            object gate = CreateLowYieldGate(
                candidateCount,
                fakeTime,
                out Queue<MailItem> source,
                out Func<int> takeCounter,
                deadline: TimeSpan.FromSeconds(3)
            );

            // Act
            QfcGateBatch batch = await DequeueBatchAsync(gate, 2, 0, CancellationToken.None);

            // Assert
            batch.Accepted.Should().BeEmpty("no candidate reached the cutoff");
            takeCounter().Should().Be(candidateCount + 1, "the scan drains the source");
            source.Should().BeEmpty("no candidate is left unscanned");
            batch.Stop.Should().Be(QfcDequeueStop.SourceExhausted, "exhaustion, not a bound");
        }

        /// <summary>
        /// AC 4: a score in flight at expiry completes, and an item it accepts is included.
        /// </summary>
        [TestMethod]
        public async Task DequeueAsync_DeadlineExpiresDuringInFlightScore_IncludesFinalAcceptedItem()
        {
            // Arrange
            MailItem inFlight = CreateMailItem("in-flight", "entry-in-flight");
            MailItem never = CreateMailItem("never-scanned", "entry-never-scanned");
            var source = new Queue<MailItem>(new[] { inFlight, never });
            var fakeTime = new FakeTimeProvider();
            var scoreGate =
                new TaskCompletionSource<(
                    long Score,
                    string TopFolder,
                    IFolderSearchHandler Handler
                )>();
            var takeCount = 0;

            object gate = CreateGate(
                () =>
                {
                    takeCount++;
                    return source.Count == 0 ? null : source.Dequeue();
                },
                (mail, token) => ReferenceEquals(mail, inFlight) ? scoreGate.Task : Scored(950L),
                threshold: 0.90,
                timeProvider: fakeTime,
                sourceActive: () => false,
                firstBatchDeadline: TimeSpan.FromSeconds(5)
            );

            // Act
            Task<IList<MailItem>> pending = DequeueAsync(gate, 3, 0, CancellationToken.None);
            pending.IsCompleted.Should().BeFalse("the first score is still in flight");

            fakeTime.Advance(TimeSpan.FromSeconds(6));
            pending.IsCompleted.Should().BeFalse("expiry must not abandon the in-flight score");

            scoreGate.SetResult((950L, "", null));
            IList<MailItem> result = await pending;

            // Assert
            result
                .Should()
                .HaveCount(
                    2,
                    "the final in-flight acceptance is included and scanning continues until source exhaustion"
                );
            result[0].Should().BeSameAs(inFlight);
            result[1].Should().BeSameAs(never);
            takeCount
                .Should()
                .Be(3, "the gate takes the remaining candidate and confirms source exhaustion");
        }

        /// <summary>
        /// Issue #424 AC 1, retargeted by issue #791. The bounded-exit intent is unchanged — after
        /// the bound no further take occurs and unscanned candidates remain — but the bound is now
        /// the scan cap rather than the 4 s deadline. A cap of 4 keeps the existing take-count and
        /// residual assertions at exactly 4 and 6.
        /// </summary>
        [TestMethod]
        public async Task DequeueAsync_AfterScanCapReached_StopsTakingAndLeavesUnscannedCandidates()
        {
            // Arrange
            var fakeTime = new FakeTimeProvider();
            object gate = CreateLowYieldGate(
                candidateCount: 10,
                fakeTime,
                out Queue<MailItem> source,
                out Func<int> takeCounter,
                maxScanWithoutAcceptance: 4
            );

            // Act
            QfcGateBatch batch = await DequeueBatchAsync(gate, 5, 0, CancellationToken.None);
            int takesAtReturn = takeCounter();

            // Assert
            batch.Accepted.Should().BeEmpty();
            batch.Stop.Should().Be(QfcDequeueStop.ScanCapReached, "the cap ended this scan");
            takesAtReturn.Should().Be(4, "a cap of 4 admits exactly four candidates");
            takeCounter().Should().Be(takesAtReturn, "no take may occur after the method returns");
            source.Should().HaveCount(6, "the unscanned remainder stays for later dequeues");
            source.Dequeue().Should().NotBeNull("unscanned candidates remain takeable");
        }

        /// <summary>
        /// AC 3: quantity satisfied before expiry returns the pre-change batch, content and order.
        /// </summary>
        [TestMethod]
        public async Task DequeueAsync_QuantitySatisfiedBeforeExpiry_ReturnsUnchangedBatchAndOrder()
        {
            // Arrange
            MailItem first = CreateMailItem("first", "entry-first");
            MailItem second = CreateMailItem("second", "entry-second");
            MailItem third = CreateMailItem("third", "entry-third");
            var source = new Queue<MailItem>(new[] { first, second, third });
            var fakeTime = new FakeTimeProvider();
            var takeCount = 0;

            object gate = CreateGate(
                () =>
                {
                    takeCount++;
                    return source.Count == 0 ? null : source.Dequeue();
                },
                (mail, token) => Scored(950L),
                threshold: 0.90,
                timeProvider: fakeTime,
                sourceActive: () => false
            );

            // Act — the clock never advances, so the 12 s budget cannot elapse.
            Task<IList<MailItem>> pending = DequeueAsync(gate, 2, 0, CancellationToken.None);
            IList<MailItem> result = await pending;

            // Assert
            result.Should().Equal(new[] { first, second }, "master-queue order is preserved");
            takeCount.Should().Be(2, "the deadline adds no take on the fast path");
            pending.IsCompleted.Should().BeTrue("the deadline must not delay the fast path");
            source.Should().ContainSingle().Which.Should().BeSameAs(third);
        }

        /// <summary>
        /// AC 3: the <see cref="Timeout.InfiniteTimeSpan"/> sentinel disables the deadline and
        /// reproduces the pre-#424 unbounded scan even though 50 s of modeled time elapses.
        /// </summary>
        [TestMethod]
        public async Task DequeueAsync_DisabledSentinel_ReproducesUnboundedPreChangeBehavior()
        {
            // Arrange
            const int candidateCount = 50;
            var candidates = Enumerable
                .Range(1, candidateCount)
                .Select(i => CreateMailItem($"candidate-{i}", $"entry-{i}"))
                .ToList();
            MailItem qualifying = candidates[39];
            var source = new Queue<MailItem>(candidates);
            var fakeTime = new FakeTimeProvider();
            var takeCount = 0;

            object gate = CreateGate(
                () =>
                {
                    takeCount++;
                    return source.Count == 0 ? null : source.Dequeue();
                },
                (mail, token) =>
                {
                    fakeTime.Advance(TimeSpan.FromSeconds(1));
                    return Scored(ReferenceEquals(mail, qualifying) ? 950L : 100L);
                },
                threshold: 0.90,
                timeProvider: fakeTime,
                sourceActive: () => false,
                firstBatchDeadline: Timeout.InfiniteTimeSpan
            );

            // Act
            IList<MailItem> result = await DequeueAsync(gate, 5, 0, CancellationToken.None);

            // Assert
            takeCount.Should().Be(candidateCount + 1, "the sentinel restores the unbounded scan");
            result
                .Should()
                .ContainSingle("exhaustion returns the partial accepted set, as before")
                .Which.Should()
                .BeSameAs(qualifying);
        }

        /// <summary>
        /// Guard clause: a non-positive, non-sentinel deadline is rejected at construction. The gate
        /// is built reflectively, so the guard surfaces wrapped in
        /// <see cref="TargetInvocationException"/>.
        /// </summary>
        [TestMethod]
        public void Constructor_NonPositiveNonSentinelDeadline_IsRejectedByGuardClause()
        {
            foreach (TimeSpan invalid in new[] { TimeSpan.Zero, TimeSpan.FromSeconds(-5) })
            {
                // Act — `System.Action` is required: a bare `Action` is CS0104-ambiguous with
                // Microsoft.Office.Interop.Outlook.Action in this namespace.
                System.Action act = () =>
                    CreateGate(
                        () => null,
                        (mail, token) => Scored(0L),
                        threshold: 0.90,
                        firstBatchDeadline: invalid
                    );

                // Assert
                act.Should()
                    .Throw<TargetInvocationException>("the gate is constructed reflectively")
                    .WithInnerException<ArgumentOutOfRangeException>("{0} is invalid", invalid);
            }
        }

        /// <summary>
        /// Issue #424 logging test, retargeted by issue #791. Each checkpoint decision emits one
        /// debug line through the existing <c>_debugLog</c> seam carrying the accepted and scanned
        /// counts, and the per-candidate "Probability debug" logging is unchanged. Asserted via the
        /// injected delegate, not log capture. The pre-#791 total-count assertion of four is
        /// replaced by per-category counts: #791 adds a launch line and turns the single expiry line
        /// into one per checkpoint, so a total count would be brittle while proving nothing about
        /// which lines were emitted.
        /// </summary>
        [TestMethod]
        public async Task DequeueAsync_CheckpointExpiry_EmitsCheckpointLineAndKeepsPerCandidateLogging()
        {
            // Arrange
            var source = new Queue<MailItem>(
                Enumerable.Range(1, 10).Select(i => CreateMailItem($"reject-{i}", $"entry-{i}"))
            );
            var fakeTime = new FakeTimeProvider();
            var logs = new List<string>();

            object gate = CreateGate(
                () => source.Count == 0 ? null : source.Dequeue(),
                (mail, token) =>
                {
                    fakeTime.Advance(TimeSpan.FromSeconds(1));
                    return Scored(100L);
                },
                threshold: 0.90,
                timeProvider: fakeTime,
                debugLog: logs.Add,
                sourceActive: () => false,
                firstBatchDeadline: TimeSpan.FromSeconds(3)
            );

            // Act
            IList<MailItem> result = await DequeueAsync(gate, 5, 0, CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
            var checkpoints = logs.Where(x => x.Contains("Zero-acceptance checkpoint")).ToList();
            checkpoints
                .Should()
                .HaveCount(3, "ten candidates at 1 s per score cross a 3 s interval three times");
            checkpoints[0]
                .Should()
                .Contain("Accepted=0")
                .And.Contain("Scanned=3", "the first checkpoint reports the first three scores");
            logs.Where(log => log.Contains("Probability debug"))
                .Should()
                .HaveCount(10, "per-candidate logging is unchanged, one line per scored candidate");
            logs.Where(log => log.Contains("High-confidence dequeue launch"))
                .Should()
                .ContainSingle("the launch line is emitted exactly once per dequeue");
        }

        /// <summary>
        /// AC 8: cancelling while parked on the empty-source poll surfaces
        /// <see cref="OperationCanceledException"/>. The pre-existing cancellation test covers only
        /// cancellation before the first take. Asserted under both deadline configurations.
        /// </summary>
        [TestMethod]
        public async Task DequeueAsync_CancelledDuringEmptyQueueWait_ThrowsOperationCanceled()
        {
            foreach (TimeSpan? deadline in DeadlineConfigurations)
            {
                // Arrange — the source never yields but reports itself active, so the gate polls.
                var fakeTime = new FakeTimeProvider();
                object gate = CreateGate(
                    () => null,
                    (mail, token) => Scored(950L),
                    threshold: 0.90,
                    timeProvider: fakeTime,
                    sourceActive: () => true,
                    firstBatchDeadline: deadline
                );

                using (var cts = new CancellationTokenSource())
                {
                    // Act
                    Task<IList<MailItem>> pending = DequeueAsync(gate, 1, 200, cts.Token);
                    pending.IsCompleted.Should().BeFalse("the gate is on the injected delay");

                    cts.Cancel();

                    // Assert
                    Func<Task> act = () => pending;
                    await act.Should()
                        .ThrowAsync<OperationCanceledException>("deadline config {0}", deadline);
                }
            }
        }

        /// <summary>
        /// AC 8: cancelling while a score is in flight surfaces
        /// <see cref="OperationCanceledException"/> from the post-score check, under both deadline
        /// configurations.
        /// </summary>
        [TestMethod]
        public async Task DequeueAsync_CancelledDuringScoring_ThrowsOperationCanceled()
        {
            foreach (TimeSpan? deadline in DeadlineConfigurations)
            {
                // Arrange
                MailItem candidate = CreateMailItem("scoring", "entry-scoring");
                var source = new Queue<MailItem>(new[] { candidate });
                var fakeTime = new FakeTimeProvider();

                using (var cts = new CancellationTokenSource())
                {
                    object gate = CreateGate(
                        () => source.Count == 0 ? null : source.Dequeue(),
                        (mail, token) =>
                        {
                            // The score completes, but was cancelled while in flight.
                            cts.Cancel();
                            return Scored(950L);
                        },
                        threshold: 0.90,
                        timeProvider: fakeTime,
                        sourceActive: () => false,
                        firstBatchDeadline: deadline
                    );

                    // Act
                    Func<Task> act = () => DequeueAsync(gate, 2, 0, cts.Token);

                    // Assert
                    await act.Should()
                        .ThrowAsync<OperationCanceledException>("deadline config {0}", deadline);
                }
            }
        }
    }
}
