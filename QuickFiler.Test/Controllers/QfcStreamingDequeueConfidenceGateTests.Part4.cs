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
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Issue #791 AC1 coverage for <c>QfcStreamingDequeueConfidenceGate</c>: the first-batch
    /// deadline becomes an advisory checkpoint, and two hard bounds — a cap on candidates scored
    /// without an acceptance and a time ceiling — terminate the extended zero-acceptance scan.
    /// Fourth part of the partial class declared in
    /// <c>QfcStreamingDequeueConfidenceGateTests.cs</c>; a new file because the three existing parts
    /// are already close to the 500-line limit. <c>[TestClass]</c> stays on the base file only (it is
    /// <c>AllowMultiple = false</c>, so repeating it would be CS0579). Shares the base file's
    /// reflection-based <c>CreateGate</c> / <c>DequeueBatchAsync</c> helpers and Part 3's
    /// <c>Scored</c> helper. Deterministic — <see cref="FakeTimeProvider"/> for all time, mocked
    /// <see cref="MailItem"/>, no COM, no sleeps, no wall-clock waits.
    /// </summary>
    public partial class QfcStreamingDequeueConfidenceGateTests
    {
        /// <summary>
        /// Builds <paramref name="count"/> mail items whose subjects and entry ids are indexed from
        /// one, so an assertion failure names the position that failed.
        /// </summary>
        private static List<MailItem> BuildCandidates(int count) =>
            Enumerable
                .Range(1, count)
                .Select(i => CreateMailItem($"candidate-{i}", $"entry-{i}"))
                .ToList();

        /// <summary>
        /// Issue #791 AC1, the reported defect. Zero acceptances when the first-batch deadline
        /// expires must no longer return an empty batch at the bound: the scan continues until the
        /// first acceptance. Forty below-cutoff candidates precede the single qualifier and each
        /// score consumes one second of a twelve-second checkpoint interval, so the pre-change gate
        /// returned empty after twelve scans.
        /// </summary>
        [TestMethod]
        public async Task DequeueAsync_ZeroAcceptedAtCheckpoint_ContinuesUntilFirstAcceptance()
        {
            // Arrange
            List<MailItem> candidates = BuildCandidates(41);
            MailItem qualifying = candidates[40];
            var source = new Queue<MailItem>(candidates);
            var fakeTime = new FakeTimeProvider();

            object gate = CreateGate(
                () => source.Count == 0 ? null : source.Dequeue(),
                (mail, token) =>
                {
                    fakeTime.Advance(TimeSpan.FromSeconds(1));
                    return Scored(ReferenceEquals(mail, qualifying) ? 950L : 100L);
                },
                threshold: 0.90,
                timeProvider: fakeTime,
                sourceActive: () => false
            );

            // Act — the default twelve-second interval is in force and is deliberately not overridden.
            QfcGateBatch batch = await DequeueBatchAsync(gate, 1, 0, CancellationToken.None);

            // Assert
            batch
                .Accepted.Should()
                .ContainSingle("the scan continues past the checkpoint until the first acceptance")
                .Which.MailItem.Should()
                .BeSameAs(qualifying);
            batch
                .Scanned.Should()
                .Be(41, "every candidate up to and including the qualifier is scored");
            batch
                .Stop.Should()
                .Be(
                    QfcDequeueStop.QuantitySatisfied,
                    "an acceptance inside the bounds satisfies the request"
                );
        }

        /// <summary>
        /// Issue #791 AC1. With neither bound reached and the producer dead, a zero-acceptance scan
        /// ends in genuine exhaustion, which is the one empty-batch case a caller may treat as a
        /// closed queue.
        /// </summary>
        [TestMethod]
        public async Task DequeueAsync_ZeroAcceptedAndSourceDrained_ReportsSourceExhausted()
        {
            // Arrange
            var source = new Queue<MailItem>(BuildCandidates(5));
            var fakeTime = new FakeTimeProvider();

            object gate = CreateGate(
                () => source.Count == 0 ? null : source.Dequeue(),
                (mail, token) =>
                {
                    fakeTime.Advance(TimeSpan.FromSeconds(1));
                    return Scored(100L);
                },
                threshold: 0.90,
                timeProvider: fakeTime,
                sourceActive: () => false,
                firstBatchDeadline: TimeSpan.FromSeconds(2)
            );

            // Act
            QfcGateBatch batch = await DequeueBatchAsync(gate, 3, 0, CancellationToken.None);

            // Assert
            batch.Accepted.Should().BeEmpty("no candidate reached the cutoff");
            batch.Scanned.Should().Be(5, "the whole source is scored before it drains");
            batch
                .Stop.Should()
                .Be(
                    QfcDequeueStop.SourceExhausted,
                    "a drained source with a dead producer is exhaustion, not a bound"
                );
            source.Should().BeEmpty("nothing is left unscanned");
        }

        /// <summary>
        /// Issue #791 AC1. The scan cap terminates the extended scan and reports the bounded exit as
        /// <see cref="QfcDequeueStop.ScanCapReached"/>. The cap is checked ahead of the take, so a
        /// capped scan cannot consume one extra candidate from the master queue.
        /// </summary>
        [TestMethod]
        public async Task DequeueAsync_ZeroAcceptedAndCapReached_StopsAndReportsScanCapReached()
        {
            // Arrange
            var source = new Queue<MailItem>(BuildCandidates(10));
            var fakeTime = new FakeTimeProvider();
            var takeCount = 0;

            object gate = CreateGate(
                () =>
                {
                    takeCount++;
                    return source.Count == 0 ? null : source.Dequeue();
                },
                (mail, token) => Scored(100L),
                threshold: 0.90,
                timeProvider: fakeTime,
                sourceActive: () => true,
                maxScanWithoutAcceptance: 4
            );

            // Act
            QfcGateBatch batch = await DequeueBatchAsync(gate, 5, 0, CancellationToken.None);

            // Assert
            batch
                .Stop.Should()
                .Be(
                    QfcDequeueStop.ScanCapReached,
                    "a bounded zero-acceptance exit is not source exhaustion"
                );
            batch.Accepted.Should().BeEmpty("no candidate reached the cutoff");
            batch.Scanned.Should().Be(4, "the cap bounds the scored count");
            takeCount.Should().Be(4, "no take may occur after the cap is reached");
            source.Should().HaveCount(6, "the unscanned remainder stays for later dequeues");
        }

        /// <summary>
        /// Issue #791 AC1. The scan cap alone cannot bound the pre-UI wait, because the empty-queue
        /// wait path does not increment the scored count while the loader is still refilling. The
        /// time ceiling is what terminates that wait: the source never yields and reports itself
        /// active, so only the ceiling can end the loop.
        /// </summary>
        [TestMethod]
        public async Task DequeueAsync_ZeroAcceptedAndCeilingReached_StopsWhileSourceStillRefilling()
        {
            // Arrange
            var fakeTime = new FakeTimeProvider();
            object gate = CreateGate(
                () => null,
                (mail, token) => Scored(950L),
                threshold: 0.90,
                timeProvider: fakeTime,
                sourceActive: () => true,
                zeroAcceptanceCeiling: TimeSpan.FromSeconds(120)
            );

            // Act — the gate parks on the injected empty-source delay, then the clock passes the
            // ceiling. Advancing the fake clock is the only thing that releases the delay, so the
            // test carries no wall-clock wait and no sleep.
            Task<QfcGateBatch> pending = DequeueBatchAsync(gate, 1, 200, CancellationToken.None);
            pending.IsCompleted.Should().BeFalse("the gate is parked on the empty-source delay");

            fakeTime.Advance(TimeSpan.FromSeconds(121));
            QfcGateBatch batch = await pending;

            // Assert
            batch
                .Stop.Should()
                .Be(
                    QfcDequeueStop.ScanCapReached,
                    "the ceiling is a bounded exit even though the producer is still active"
                );
            batch.Accepted.Should().BeEmpty("nothing was ever takeable");
            batch.Scanned.Should().Be(0, "the wait path scores nothing");
        }

        /// <summary>
        /// Issue #791 AC1 logging. Every checkpoint decision records the cutoff in force and the
        /// scanned and accepted counts, which the pre-change expiry line never carried. Asserted
        /// through the injected <c>debugLog</c> delegate, which is the convention this gate already
        /// established, rather than through a log4net appender.
        /// </summary>
        [TestMethod]
        public async Task DequeueAsync_CheckpointExpiry_LogsCutoffAndCounts()
        {
            // Arrange
            var source = new Queue<MailItem>(BuildCandidates(10));
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
            QfcGateBatch batch = await DequeueBatchAsync(gate, 5, 0, CancellationToken.None);

            // Assert
            batch.Accepted.Should().BeEmpty();
            List<string> checkpoints = logs.Where(log => log.Contains("Zero-acceptance checkpoint"))
                .ToList();
            checkpoints
                .Should()
                .HaveCount(
                    3,
                    "a ten-candidate scan at 1 s per score crosses a 3 s interval 3 times"
                );
            checkpoints[0]
                .Should()
                .Contain("Accepted=0")
                .And.Contain("Scanned=3")
                .And.Contain(
                    "Cutoff=900",
                    "the cutoff in effect must be recorded at each decision"
                );
        }

        /// <summary>
        /// Issue #791 AC1 logging. One launch line records the cutoff, the requested quantity and
        /// both bounds, so an operator reading the log can tell which cutoff and which bounds a run
        /// used without inferring them from the outcome.
        /// </summary>
        [TestMethod]
        public async Task DequeueAsync_Launch_LogsCutoffQuantityAndBounds()
        {
            // Arrange
            var source = new Queue<MailItem>(BuildCandidates(1));
            var logs = new List<string>();

            object gate = CreateGate(
                () => source.Count == 0 ? null : source.Dequeue(),
                (mail, token) => Scored(950L),
                threshold: 0.90,
                timeProvider: new FakeTimeProvider(),
                debugLog: logs.Add,
                sourceActive: () => false,
                maxScanWithoutAcceptance: 250,
                zeroAcceptanceCeiling: TimeSpan.FromSeconds(120)
            );

            // Act
            _ = await DequeueBatchAsync(gate, 7, 0, CancellationToken.None);

            // Assert
            logs.Should()
                .ContainSingle(log => log.Contains("High-confidence dequeue launch"))
                .Which.Should()
                .Contain("Cutoff=900")
                .And.Contain("0.9")
                .And.Contain("Quantity=7")
                .And.Contain("ScanCap=250")
                .And.Contain("Ceiling=00:02:00");
        }

        /// <summary>
        /// Issue #608 regression pin. Once one candidate has been accepted the checkpoint and both
        /// bounds are inert, so a non-empty prefix still fills or exhausts. The injected cap of two
        /// is deliberately smaller than the scan this test performs: if the guard were widened to
        /// evaluate the bounds after an acceptance, the run would stop early and this test would
        /// fail.
        /// </summary>
        [TestMethod]
        public async Task DequeueAsync_NonEmptyPrefix_UnchangedByCheckpoint()
        {
            // Arrange
            List<MailItem> candidates = BuildCandidates(21);
            MailItem qualifying = candidates[0];
            var source = new Queue<MailItem>(candidates);
            var fakeTime = new FakeTimeProvider();
            var logs = new List<string>();

            object gate = CreateGate(
                () => source.Count == 0 ? null : source.Dequeue(),
                (mail, token) =>
                {
                    fakeTime.Advance(TimeSpan.FromSeconds(10));
                    return Scored(ReferenceEquals(mail, qualifying) ? 950L : 100L);
                },
                threshold: 0.90,
                timeProvider: fakeTime,
                debugLog: logs.Add,
                sourceActive: () => false,
                firstBatchDeadline: TimeSpan.FromSeconds(3),
                maxScanWithoutAcceptance: 2
            );

            // Act — quantity 5 is never satisfied, so the scan runs to exhaustion.
            QfcGateBatch batch = await DequeueBatchAsync(gate, 5, 0, CancellationToken.None);

            // Assert
            batch
                .Accepted.Should()
                .ContainSingle("the accepted prefix is unchanged by #791")
                .Which.MailItem.Should()
                .BeSameAs(qualifying);
            batch.Scanned.Should().Be(21, "fill-or-exhaust is preserved after a non-empty prefix");
            batch
                .Stop.Should()
                .Be(QfcDequeueStop.SourceExhausted, "the source drained, no bound was reached");
            logs.Where(log => log.Contains("Zero-acceptance checkpoint"))
                .Should()
                .BeEmpty("the checkpoint is evaluated only while nothing has been accepted");
        }
    }
}
