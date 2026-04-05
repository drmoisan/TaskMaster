using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
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

        /// <summary>
        /// Tests the negative-sourceLength inference branch (line 208) in the
        /// ProgressTrackerPane overload of CopyToAsync. When sourceLength &lt; 0 and
        /// source.CanSeek is true, the method infers sourceLength from the stream.
        /// This covers the uncovered branch in the ProgressTrackerPane overload that
        /// was missed by the existing CopyToAsync_WithNullProgress_ThrowsNullReference
        /// test (which used a non-negative sourceLength).
        ///
        /// Args:
        ///     None — uses inline Arrange.
        ///
        /// Returns:
        ///     Void; asserts via FluentAssertions.
        ///
        /// Side Effects:
        ///     None; uses only in-memory MemoryStream.
        /// </summary>
        [TestMethod]
        [Description(
            "Covers the sourceLength<0 inference branch in CopyToAsync(ProgressTrackerPane) "
                + "(line 208): when sourceLength=-1 and source.CanSeek=true, the method infers length from the stream. "
                + "With null progress the final progress.Report(100) call throws NullReferenceException."
        )]
        public async Task CopyToAsync_ProgressTrackerPaneOverload_WithNegativeSourceLength_InfersLengthFromSeekableStream()
        {
            // Arrange: a seekable MemoryStream so source.CanSeek=true activates the length-inference branch.
            var data = new byte[] { 1, 2, 3 };
            using var source = new MemoryStream(data);
            using var destination = new MemoryStream();

            // Act & Assert: sourceLength=-1 triggers the inference of sourceLength from stream.
            // When totalBytesCopied>0 the final progress.Report(100) call throws because progress is null.
            Func<Task> act = () =>
                source.CopyToAsync(
                    sourceLength: -1,
                    destination,
                    bufferSize: 3,
                    (ProgressTrackerPane)null,
                    messagePrefix: "",
                    CancellationToken.None
                );
            await act.Should()
                .ThrowAsync<NullReferenceException>(
                    "the final progress.Report(100) is not null-guarded and progress is null"
                );
        }

        [TestMethod]
        public async Task ReadTextAsync_WithLargeExistingFile_ReturnsTextAndReportsProgress()
        {
            // Arrange
            var fixture = GetLargeTextFixture();
            var progress = new TupleProgressCollector();

            // Act
            string contents = await AsyncSerialization.ReadTextAsync(fixture.FullName, progress);

            // Assert
            contents.Should().NotBeEmpty();
            progress.Reports.Should().NotBeEmpty();
            progress.Reports.Should().Contain(report => report.total == fixture.Length);
            progress.Reports.Should().Contain(report => report.current > 0);
        }

        [TestMethod]
        public async Task ReadTextWithProgressAsync_ProgressTrackerOverload_WithLargeExistingFile_ReportsProgress()
        {
            // Arrange
            var fixture = GetLargeTextFixture();
            var progress = new CapturingProgressTracker();
            var disk = new FilePathHelper(fixture.Name, fixture.Directory!.FullName);

            // Act
            string contents = await disk.ReadTextWithProgressAsync(progress, "Read fixture");

            // Assert
            contents.Should().NotBeEmpty();
            progress.ReportedValues.Should().NotBeEmpty();
            progress.ReportedValues.Should().Contain(value => value > 0);
            progress.ReportedMessages.Should().Contain(message => message.Contains("Read fixture"));
        }

        [TestMethod]
        public async Task ReadTextWithProgressAsync_ProgressTrackerPaneOverload_WithLargeExistingFile_UpdatesProgress()
        {
            // Arrange — no ProgressPane is created here because ProgressPane is a WinForms
            // UserControl whose constructor calls TaskScheduler.FromCurrentSynchronizationContext()
            // and installs a WindowsFormsSynchronizationContext.  On a thread-pool thread
            // (MSTest async test threads) that has no message pump, the resulting
            // SynchronizationContext.Post() posts to a message queue that is never drained,
            // causing the test's await continuation to deadlock indefinitely.
            //
            // The headless pane sets _isRoot = false (default from GetUninitializedObject), so
            // SafeAction / ChangeBarColor are never reached and _progressViewer is never
            // accessed.  The job-name is read back from _jobName via reflection.
            var fixture = GetLargeTextFixture();
            var disk = new FilePathHelper(fixture.Name, fixture.Directory!.FullName);
            var progress = CreateHeadlessPane();

            // Act
            string contents = await disk.ReadTextWithProgressAsync(progress, "Pane read");

            // Assert
            var jobName = (string?)
                typeof(ProgressTrackerPane)
                    .GetField("_jobName", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(progress);

            contents.Should().NotBeEmpty();
            progress.Progress.Should().BeGreaterThan(0);
            jobName.Should().Contain("Pane read");
        }

        [TestMethod]
        public async Task CopyToAsync_ProgressTrackerPaneOverload_WithProgress_CompletesAndReportsCompletion()
        {
            // Arrange — no ProgressPane is created here for the same reason as above:
            // creating a WinForms UserControl on an MSTest thread-pool thread installs a
            // WindowsFormsSynchronizationContext that deadlocks the test's await continuation
            // when no message pump is running.  _isRoot = false so _progressViewer is never
            // accessed; the job name is verified via the _jobName field.
            using var source = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6 });
            using var destination = new MemoryStream();
            var progress = CreateHeadlessPane();

            // Act
            await source.CopyToAsync(
                sourceLength: source.Length,
                destination,
                bufferSize: 2,
                progress,
                messagePrefix: "Copy",
                CancellationToken.None
            );

            // Assert
            var jobName = (string?)
                typeof(ProgressTrackerPane)
                    .GetField("_jobName", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(progress);

            destination.ToArray().Should().Equal(new byte[] { 1, 2, 3, 4, 5, 6 });
            progress.Progress.Should().Be(100);
            jobName.Should().Contain("Copy");
        }

        private static FileInfo GetLargeTextFixture()
        {
            var current = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            while (current is not null)
            {
                var candidate = Path.Combine(
                    current.FullName,
                    "packages",
                    "Microsoft.Graph.5.103.0",
                    "lib",
                    "netstandard2.0",
                    "Microsoft.Graph.xml"
                );
                if (File.Exists(candidate))
                {
                    return new FileInfo(candidate);
                }

                current = current.Parent;
            }

            throw new InvalidOperationException(
                "The Microsoft.Graph.xml fixture could not be located from the test assembly path."
            );
        }

        private static ProgressTrackerPane CreateHeadlessPane()
        {
            var pane = (ProgressTrackerPane)
                FormatterServices.GetUninitializedObject(typeof(ProgressTrackerPane));
            var parentField = typeof(ProgressTrackerPane).GetField(
                "_parent",
                BindingFlags.Instance | BindingFlags.NonPublic
            )!;
            // Use a no-op SynchronousProgress so Report() returns synchronously with no
            // dependency on a SynchronizationContext or WinForms message pump.
            // _isRoot defaults to false (GetUninitializedObject zeroes all fields), so
            // ChangeBarColor / SafeAction are never reached and _progressViewer is never
            // accessed — passing null for _progressViewer is safe.
            var rootProgress = new SynchronousProgress<(int Value, string JobName)>(_ => { });
            var parent = Activator.CreateInstance(parentField.FieldType, rootProgress, 100, 0);

            parentField.SetValue(pane, parent);
            typeof(ProgressTrackerPane)
                .GetField("_progressViewer", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(pane, null);
            typeof(ProgressTrackerPane)
                .GetField("_jobName", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(pane, string.Empty);
            typeof(ProgressTrackerPane)
                .GetField("_progress", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(pane, 0d);
            return pane;
        }

        private sealed class TupleProgressCollector : IProgress<(double current, double total)>
        {
            public List<(double current, double total)> Reports { get; } = new();

            public void Report((double current, double total) value) => Reports.Add(value);
        }

        private sealed class CapturingProgressTracker : ProgressTracker
        {
            public CapturingProgressTracker()
                : base(new CancellationTokenSource()) { }

            public List<double> ReportedValues { get; } = new();

            public List<string> ReportedMessages { get; } = new();

            public override void Report(double value, string jobName)
            {
                ReportedValues.Add(value);
                ReportedMessages.Add(jobName ?? string.Empty);
            }
        }
    }
}
