using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Interfaces;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Issue #424 incremental progress-callback coverage for
    /// <c>QfcStreamingDequeueConfidenceGate</c>. Third part of the partial class declared in
    /// <c>QfcStreamingDequeueConfidenceGateTests.cs</c>; the Phase 2 tests were relocated here
    /// verbatim so <c>Part2.cs</c> stays under the 500-line limit. <c>[TestClass]</c> stays on the
    /// base file only (it is <c>AllowMultiple = false</c>, so repeating it would be CS0579).
    /// Shares the base file's reflection-based <c>CreateGate</c> / <c>DequeueAsync</c> helpers.
    /// </summary>
    public partial class QfcStreamingDequeueConfidenceGateTests
    {
        /// <summary>
        /// AC 5: the callback fires exactly once per scanned candidate — rejected candidates
        /// included — reporting <c>(scanned, accepted, quantity)</c>, with <c>scanned</c> incrementing
        /// by one and both counters monotonically non-decreasing.
        /// </summary>
        [TestMethod]
        public async Task DequeueAsync_ProgressCallback_FiresOncePerScannedCandidateMonotonically()
        {
            // Arrange — 5 candidates, positions 2 and 4 qualify, so rejects are reported too.
            var candidates = Enumerable
                .Range(1, 5)
                .Select(i => CreateMailItem($"candidate-{i}", $"entry-{i}"))
                .ToList();
            var source = new Queue<MailItem>(candidates);
            var reports = new List<(int Scanned, int Accepted, int Quantity)>();

            object gate = CreateGate(
                () => source.Count == 0 ? null : source.Dequeue(),
                (mail, token) =>
                    Task.FromResult(
                        (
                            ReferenceEquals(mail, candidates[1])
                            || ReferenceEquals(mail, candidates[3])
                                ? 950L
                                : 100L,
                            ""
                        )
                    ),
                threshold: 0.90,
                timeProvider: new FakeTimeProvider(),
                sourceActive: () => false,
                progressCallback: (scanned, accepted, quantity) =>
                    reports.Add((scanned, accepted, quantity))
            );

            // Act — quantity 3 is never satisfied, so the source runs to exhaustion.
            IList<MailItem> result = await DequeueAsync(gate, 3, 0, CancellationToken.None);

            // Assert
            result.Should().Equal(new[] { candidates[1], candidates[3] });
            reports
                .Should()
                .HaveCount(5, "every scanned candidate reports, accepted and rejected alike");
            reports
                .Select(r => r.Scanned)
                .Should()
                .Equal(new[] { 1, 2, 3, 4, 5 }, "scanned increments by one per candidate");
            reports
                .Select(r => r.Accepted)
                .Should()
                .Equal(new[] { 0, 1, 1, 2, 2 }, "accepted is monotonically non-decreasing");
            reports.Should().OnlyContain(r => r.Quantity == 3, "quantity is reported unchanged");
        }

        /// <summary>
        /// AC 5: no callback invocation occurs after <c>DequeueAsync</c> returns, including on the
        /// deadline-expiry path where the method exits early.
        /// </summary>
        [TestMethod]
        public async Task DequeueAsync_ProgressCallback_StopsReportingOnceTheMethodReturns()
        {
            // Arrange
            var fakeTime = new FakeTimeProvider();
            var source = new Queue<MailItem>(
                Enumerable.Range(1, 20).Select(i => CreateMailItem($"reject-{i}", $"entry-{i}"))
            );
            var reports = new List<int>();

            object gate = CreateGate(
                () => source.Count == 0 ? null : source.Dequeue(),
                (mail, token) =>
                {
                    fakeTime.Advance(TimeSpan.FromSeconds(1));
                    return Task.FromResult((100L, ""));
                },
                threshold: 0.90,
                timeProvider: fakeTime,
                sourceActive: () => false,
                firstBatchDeadline: TimeSpan.FromSeconds(3),
                progressCallback: (scanned, accepted, quantity) => reports.Add(scanned)
            );

            // Act
            IList<MailItem> result = await DequeueAsync(gate, 5, 0, CancellationToken.None);
            int reportsAtReturn = reports.Count;

            // Assert
            result.Should().BeEmpty();
            reportsAtReturn.Should().Be(3, "the 3 s budget admits exactly three scored candidates");
            reports.Should().Equal(new[] { 1, 2, 3 });
            reports
                .Count.Should()
                .Be(reportsAtReturn, "no invocation may occur after the awaited method completes");
        }

        /// <summary>
        /// AC 5: a throwing callback propagates its exception out of <c>DequeueAsync</c> (fail fast,
        /// no swallow-and-log), and gate state observable to the caller is not corrupted — the
        /// un-taken remainder of the source is still takeable afterwards.
        /// </summary>
        [TestMethod]
        public async Task DequeueAsync_ThrowingProgressCallback_PropagatesAndLeavesSourceUsable()
        {
            // Arrange
            var candidates = Enumerable
                .Range(1, 6)
                .Select(i => CreateMailItem($"candidate-{i}", $"entry-{i}"))
                .ToList();
            var source = new Queue<MailItem>(candidates);
            var expected = new InvalidOperationException("progress sink failed");

            object gate = CreateGate(
                () => source.Count == 0 ? null : source.Dequeue(),
                (mail, token) => Task.FromResult((950L, "")),
                threshold: 0.90,
                timeProvider: new FakeTimeProvider(),
                sourceActive: () => false,
                progressCallback: (scanned, accepted, quantity) => throw expected
            );

            // Act
            Func<Task> act = () => DequeueAsync(gate, 4, 0, CancellationToken.None);

            // Assert
            (await act.Should().ThrowAsync<InvalidOperationException>("the gate must fail fast"))
                .Which.Should()
                .BeSameAs(expected);
            source
                .Should()
                .HaveCount(5, "only the first candidate was taken before the sink threw");
            source.Dequeue().Should().BeSameAs(candidates[1], "the remainder stays takeable");
        }

        /// <summary>
        /// Issue #446. A deadline-bounded empty result must be distinguishable from genuine source
        /// exhaustion, otherwise the caller closes the UI queue for the rest of the session while
        /// the master queue still holds unscanned items. Driven by <c>FakeTimeProvider</c>: each
        /// score consumes one second of a three-second budget and nothing qualifies, so the
        /// deadline exit is the one taken.
        /// </summary>
        [TestMethod]
        public async Task DequeueAsync_DeadlineExpiresWithZeroAccepted_ReportsDeadlineExpiredStop()
        {
            // Arrange
            var fakeTime = new FakeTimeProvider();
            var source = new Queue<MailItem>(
                Enumerable
                    .Range(1, 10)
                    .Select(i => CreateMailItem($"reject-{i}", $"entry-reject-{i}"))
            );
            object gate = CreateGate(
                () => source.Count == 0 ? null : source.Dequeue(),
                (mail, token) =>
                {
                    fakeTime.Advance(TimeSpan.FromSeconds(1));
                    return Task.FromResult((100L, ""));
                },
                threshold: 0.90,
                timeProvider: fakeTime,
                sourceActive: () => true,
                firstBatchDeadline: TimeSpan.FromSeconds(3)
            );

            // Act
            QfcGateBatch batch = await DequeueBatchAsync(gate, 1, 0, CancellationToken.None);

            // Assert
            batch
                .Stop.Should()
                .Be(
                    QfcDequeueStop.DeadlineExpired,
                    "an empty batch caused by the first-batch deadline is not source exhaustion"
                );
            batch.Accepted.Should().BeEmpty("no candidate qualified before the deadline");
        }

        /// <summary>
        /// Issue #446. The complementary exit: when the take delegate returns null and the producer
        /// reports it is no longer loading, the source really is drained and the caller may close
        /// the queue.
        /// </summary>
        [TestMethod]
        public async Task DequeueAsync_SourceDrained_ReportsSourceExhaustedStop()
        {
            // Arrange
            object gate = CreateGate(
                () => null,
                (mail, token) => Task.FromResult((950L, "")),
                threshold: 0.90,
                timeProvider: new FakeTimeProvider(),
                sourceActive: () => false
            );

            // Act
            QfcGateBatch batch = await DequeueBatchAsync(gate, 1, 0, CancellationToken.None);

            // Assert
            batch
                .Stop.Should()
                .Be(
                    QfcDequeueStop.SourceExhausted,
                    "a drained source with no active producer is genuine exhaustion"
                );
            batch.Accepted.Should().BeEmpty("there was nothing to take");
        }

        /// <summary>
        /// Issue #446 and Scope 427-A. The gate already computes the top-ranked folder for every
        /// candidate it scores. Discarding that folder for accepted candidates forces the consuming
        /// UI layer to re-score the same item against the same classifier, so the accepted carrier
        /// must expose the folder the gate scored it against.
        /// </summary>
        [TestMethod]
        public async Task DequeueAsync_AcceptedCandidate_CarriesTopFolderInPreScoredResult()
        {
            // Arrange
            const string ExpectedFolder = @"Inbox\Projects\Alpha";
            var candidate = CreateMailItem("accepted", "entry-accepted");
            var source = new Queue<MailItem>(new[] { candidate });

            object gate = CreateGate(
                () => source.Count == 0 ? null : source.Dequeue(),
                (mail, token) => Task.FromResult((950L, ExpectedFolder)),
                threshold: 0.90,
                timeProvider: new FakeTimeProvider(),
                sourceActive: () => false
            );

            // Act
            QfcGateBatch batch = await DequeueBatchAsync(gate, 1, 0, CancellationToken.None);

            // Assert
            QfcPreScoredItem accepted = batch
                .Accepted.Should()
                .ContainSingle("the single high-scoring candidate qualifies")
                .Which;
            accepted.MailItem.Should().BeSameAs(candidate);
            accepted
                .PredeterminedFolder.Should()
                .Be(
                    ExpectedFolder,
                    "the folder the score loader already returned must travel with the accepted "
                        + "candidate instead of being discarded and re-derived downstream"
                );
        }
    }
}
