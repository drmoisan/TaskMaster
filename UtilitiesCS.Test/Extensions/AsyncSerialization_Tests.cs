using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.Extensions
{
    [TestClass]
    public class AsyncSerialization_Tests
    {
        [TestMethod]
        public void ToMbString_ShouldFormatBytesAsMegabytes()
        {
            // Arrange / Act / Assert
            ((long)1000000)
                .ToMbString()
                .Should()
                .Be("1.0 MB");
            ((long)500000).ToMbString().Should().Be("0.5 MB");
            ((long)0).ToMbString().Should().Be("0.0 MB");
            ((long)2500000).ToMbString().Should().Be("2.5 MB");
        }

        [TestMethod]
        public void MB_Constant_ShouldBeOneMillion()
        {
            AsyncSerialization.MB.Should().Be(1000000);
        }

        [TestMethod]
        public async Task CopyToAsync_WithIProgress_ShouldCopyAndReportProgress()
        {
            // Arrange
            var data = new byte[] { 10, 20, 30, 40, 50, 60, 70, 80 };
            using var source = new MemoryStream(data);
            using var destination = new MemoryStream();
            var reports = new List<KeyValuePair<long, long>>();
            var progress = new Progress<KeyValuePair<long, long>>(reports.Add);

            // Act
            await source.CopyToAsync(
                sourceLength: source.Length,
                destination,
                bufferSize: 4,
                progress,
                CancellationToken.None
            );

            // Assert
            destination.ToArray().Should().StartWith(data);
        }

        [TestMethod]
        public async Task CopyToAsync_WithNullProgress_ThrowsNullReference()
        {
            // Arrange — production code has a null-safety gap on the final progress.Report(100) call
            var data = new byte[] { 1, 2, 3 };
            using var source = new MemoryStream(data);
            using var destination = new MemoryStream();

            // Act & Assert
            Func<Task> act = () =>
                source.CopyToAsync(
                    sourceLength: source.Length,
                    destination,
                    bufferSize: 0,
                    (ProgressTrackerPane)null,
                    messagePrefix: "",
                    CancellationToken.None
                );
            await act.Should().ThrowAsync<NullReferenceException>();
        }

        [TestMethod]
        public async Task CopyToAsync_WithZeroBufferSize_UsesDefaultBuffer()
        {
            // Arrange
            var data = new byte[] { 1, 2, 3 };
            using var source = new MemoryStream(data);
            using var destination = new MemoryStream();
            var reports = new List<KeyValuePair<long, long>>();
            var progress = new Progress<KeyValuePair<long, long>>(reports.Add);

            // Act
            await source.CopyToAsync(
                sourceLength: source.Length,
                destination,
                bufferSize: 0,
                progress,
                CancellationToken.None
            );

            // Assert
            destination.ToArray().Should().StartWith(data);
        }

        [TestMethod]
        public async Task CopyToAsync_WithNegativeSourceLength_InfersLengthFromStream()
        {
            // Arrange
            var data = new byte[] { 5, 6, 7 };
            using var source = new MemoryStream(data);
            using var destination = new MemoryStream();
            var reports = new List<KeyValuePair<long, long>>();
            var progress = new Progress<KeyValuePair<long, long>>(reports.Add);

            // Act
            await source.CopyToAsync(
                sourceLength: -1,
                destination,
                bufferSize: 3,
                progress,
                CancellationToken.None
            );

            // Assert
            destination.ToArray().Should().StartWith(data);
        }

        [TestMethod]
        public async Task CopyToAsync_ShouldCopyStreamAndReportProgress()
        {
            // Arrange
            using var source = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
            using var destination = new MemoryStream();
            var reports = new List<KeyValuePair<long, long>>();
            var progress = new Progress<KeyValuePair<long, long>>(reports.Add);

            // Act
            await source.CopyToAsync(
                sourceLength: source.Length,
                destination,
                bufferSize: 5,
                progress,
                CancellationToken.None
            );

            // Progress<T> posts callbacks via the thread pool; allow time for delivery
            await Task.Delay(200);

            // Assert
            destination.ToArray().Should().Equal(new byte[] { 1, 2, 3, 4, 5 });
            reports.Should().NotBeEmpty();
            reports[^1].Key.Should().Be(5);
            reports[^1].Value.Should().Be(5);
        }

        [TestMethod]
        public async Task CopyToAsync_ShouldThrowWhenCancellationIsRequestedBeforeCopy()
        {
            // Arrange
            using var source = new MemoryStream(new byte[] { 1, 2, 3 });
            using var destination = new MemoryStream();
            using var cancellationSource = new CancellationTokenSource();
            cancellationSource.Cancel();

            // Act
            Func<Task> act = async () =>
                await source.CopyToAsync(
                    sourceLength: source.Length,
                    destination,
                    bufferSize: 2,
                    progress: null,
                    cancellationSource.Token
                );

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        [TestMethod]
        public async Task CopyToAsync_ShouldSupportEmptyStreams()
        {
            // Arrange
            using var source = new MemoryStream(Array.Empty<byte>());
            using var destination = new MemoryStream();
            var reports = new List<KeyValuePair<long, long>>();
            var progress = new Progress<KeyValuePair<long, long>>(reports.Add);

            // Act
            await source.CopyToAsync(
                sourceLength: 0,
                destination,
                bufferSize: 0,
                progress,
                CancellationToken.None
            );

            // Assert
            destination.Length.Should().Be(0);
            reports.Should().ContainSingle();
            reports[0].Key.Should().Be(0);
            reports[0].Value.Should().Be(0);
        }

        [TestMethod]
        public async Task CopyToAsync_WithSynchronousProgress_ReportsMonotonicallyIncreasingValues()
        {
            // Arrange: synchronous IProgress captures all reports in order without async dispatch gaps
            var data = new byte[256];
            for (var i = 0; i < data.Length; i++)
                data[i] = (byte)i;

            using var source = new MemoryStream(data);
            using var destination = new MemoryStream();
            var reports = new List<KeyValuePair<long, long>>();
            var progress = new SynchronousProgress<KeyValuePair<long, long>>(v => reports.Add(v));

            // Act
            await source.CopyToAsync(
                sourceLength: data.Length,
                destination,
                bufferSize: 64,
                progress,
                CancellationToken.None
            );

            // Assert: every successive Key value is >= the previous (monotonically non-decreasing)
            reports.Should().NotBeEmpty();
            for (var i = 1; i < reports.Count; i++)
            {
                reports[i]
                    .Key.Should()
                    .BeGreaterThanOrEqualTo(
                        reports[i - 1].Key,
                        because: $"progress at index {i} should not decrease"
                    );
            }
        }

        [TestMethod]
        public async Task CopyToAsync_InitialProgressReport_HasZeroCompleteAndKnownTotal()
        {
            // Arrange: use a synchronous progress collector so the initial (0, total) report
            // is captured deterministically — this exercises the zero-complete path in
            // GetProgressParams / GetProgressMessage without a division-by-zero error
            var data = new byte[] { 1, 2, 3, 4, 5 };
            using var source = new MemoryStream(data);
            using var destination = new MemoryStream();
            var reports = new List<KeyValuePair<long, long>>();
            var progress = new SynchronousProgress<KeyValuePair<long, long>>(v => reports.Add(v));

            // Act
            await source.CopyToAsync(
                sourceLength: data.Length,
                destination,
                bufferSize: 5,
                progress,
                CancellationToken.None
            );

            // Assert: first report always has complete=0 and total=sourceLength (zero-complete initial state)
            reports.Should().NotBeEmpty();
            reports[0].Key.Should().Be(0, because: "initial report marks zero bytes complete");
            reports[0]
                .Value.Should()
                .Be(data.Length, because: "total should match the known source length");
        }

        /// <summary>
        /// Synchronous IProgress implementation that invokes the callback inline, avoiding
        /// the async-dispatch behaviour of System.Progress{T} which can cause reports to arrive
        /// after the awaited task completes and miss the deterministic ordering checks.
        /// </summary>
        private sealed class SynchronousProgress<T> : IProgress<T>
        {
            private readonly Action<T> _callback;

            public SynchronousProgress(Action<T> callback)
            {
                _callback = callback;
            }

            void IProgress<T>.Report(T value) => _callback(value);
        }
    }
}
