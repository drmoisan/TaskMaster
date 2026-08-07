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
        /// </summary>
        private static object CreateLowYieldGate(
            int candidateCount,
            TimeSpan deadline,
            FakeTimeProvider fakeTime,
            out Queue<MailItem> source,
            out Func<int> takeCounter
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
                    return Task.FromResult(100L);
                },
                threshold: 0.90,
                timeProvider: fakeTime,
                sourceActive: () => false,
                firstBatchDeadline: deadline
            );
        }

        /// <summary>
        /// Issue #424 regression test. A low-yield stream (1 qualifier in 50) at 1 s per score is
        /// scanned to exhaustion before the fix and bounded by the 12 s default deadline after it.
        /// </summary>
        [TestMethod]
        public async Task DequeueAsync_LowYieldStream_StopsScanningAtDefaultFirstBatchDeadline()
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
                    return Task.FromResult(score);
                },
                threshold: 0.90,
                timeProvider: fakeTime,
                sourceActive: () => false
            );

            // Act
            IList<MailItem> result = await DequeueAsync(gate, 5, 0, CancellationToken.None);

            // Assert
            takeCount.Should().BeLessThanOrEqualTo(13, "12 s at 1 s per score bounds the scan");
            result.Should().Equal(acceptedSoFar, "only pre-expiry acceptances may be returned");
        }

        /// <summary>AC 2: zero acceptances before expiry returns an empty list at the bound.</summary>
        [TestMethod]
        public async Task DequeueAsync_DeadlineExpiresWithZeroAccepted_ReturnsEmptyListAtTheBound()
        {
            // Arrange
            var fakeTime = new FakeTimeProvider();
            object gate = CreateLowYieldGate(
                candidateCount: 20,
                deadline: TimeSpan.FromSeconds(3),
                fakeTime,
                out Queue<MailItem> source,
                out Func<int> takeCounter
            );

            // Act
            IList<MailItem> result = await DequeueAsync(gate, 2, 0, CancellationToken.None);

            // Assert
            result.Should().BeEmpty("no candidate reached the cutoff before expiry");
            takeCounter().Should().Be(3, "a 3 s budget at 1 s per score admits three candidates");
            source.Should().HaveCount(17, "unscanned candidates stay queued, not discarded");
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
            var scoreGate = new TaskCompletionSource<long>();
            var takeCount = 0;

            object gate = CreateGate(
                () =>
                {
                    takeCount++;
                    return source.Count == 0 ? null : source.Dequeue();
                },
                (mail, token) =>
                    ReferenceEquals(mail, inFlight) ? scoreGate.Task : Task.FromResult(950L),
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

            scoreGate.SetResult(950L);
            IList<MailItem> result = await pending;

            // Assert
            result
                .Should()
                .ContainSingle("the final in-flight acceptance is included")
                .Which.Should()
                .BeSameAs(inFlight);
            takeCount.Should().Be(1, "no further candidate may be taken after expiry");
        }

        /// <summary>
        /// AC 1: after a deadline return no further takes occur and unscanned candidates remain.
        /// </summary>
        [TestMethod]
        public async Task DequeueAsync_AfterDeadlineReturn_StopsTakingAndLeavesUnscannedCandidates()
        {
            // Arrange
            var fakeTime = new FakeTimeProvider();
            object gate = CreateLowYieldGate(
                candidateCount: 10,
                deadline: TimeSpan.FromSeconds(4),
                fakeTime,
                out Queue<MailItem> source,
                out Func<int> takeCounter
            );

            // Act
            IList<MailItem> result = await DequeueAsync(gate, 5, 0, CancellationToken.None);
            int takesAtReturn = takeCounter();

            // Assert
            result.Should().BeEmpty();
            takesAtReturn.Should().Be(4, "a 4 s budget at 1 s per score admits four candidates");
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
                (mail, token) => Task.FromResult(950L),
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
                    return Task.FromResult(ReferenceEquals(mail, qualifying) ? 950L : 100L);
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
                        (mail, token) => Task.FromResult(0L),
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
        /// Expiry emits exactly one debug line through the existing <c>_debugLog</c> seam carrying
        /// the accepted and scanned counts, and the per-candidate "Probability debug" logging is
        /// unchanged. Asserted via the injected delegate, not log capture.
        /// </summary>
        [TestMethod]
        public async Task DequeueAsync_DeadlineExpiry_EmitsOneExpiryLineAndKeepsPerCandidateLogging()
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
                    return Task.FromResult(100L);
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
            logs.Should()
                .ContainSingle(log =>
                    log.Contains("First-batch deadline expired")
                    && log.Contains("Accepted=0")
                    && log.Contains("Scanned=3")
                );
            logs.Where(log => log.Contains("Probability debug"))
                .Should()
                .HaveCount(3, "per-candidate logging is unchanged, one line per scored candidate");
            logs.Should().HaveCount(4, "three per-candidate lines plus one expiry line");
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
                    (mail, token) => Task.FromResult(950L),
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
                            return Task.FromResult(950L);
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
