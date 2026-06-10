using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Interfaces;
using UtilitiesCS.Test.TestHelpers;
using UtilitiesCS.Threading;

namespace UtilitiesCS.Test.Threading
{
    /// <summary>
    /// Unit tests for <see cref="AsyncMultiTasker"/>, targeting the three overloads
    /// that are exercisable without COM or live-request infrastructure: the Action
    /// overload, the synchronous Func overload, and the progress-report contract.
    ///
    /// <para>
    /// Design constraints:
    /// <list type="bullet">
    ///   <item>Overloads that cast <c>TOut</c> to <c>IItemInfo</c> at completion
    ///         (the async <c>Func&lt;T,Task&lt;TOut&gt;&gt;</c> overload) are not
    ///         tested here because they require <c>TOut</c> to implement
    ///         <c>IItemInfo</c>; testing those would need a COM-bound domain object.</item>
    ///   <item>Input count is set to <c>Environment.ProcessorCount * 4</c> to
    ///         guarantee <c>chunkSize = count / (ProcessorCount-1) &gt;= 1</c> for
    ///         any machine with at least two logical cores.</item>
    /// </list>
    /// </para>
    /// </summary>
    [TestClass]
    public class AsyncMultiTasker_Tests
    {
        /// <summary>
        /// Deterministic <see cref="IProgress{T}"/> implementation that invokes the
        /// callback synchronously on the reporting thread.
        ///
        /// <para>
        /// Purpose:
        ///     Avoids the thread-pool asynchrony of <see cref="Progress{T}"/> so
        ///     callback invocations are observable immediately after the task
        ///     completes without adding arbitrary <c>Task.Delay</c> waits.
        /// </para>
        /// </summary>
        private sealed class SyncProgress : IProgress<(int Value, string JobName)>
        {
            private readonly Action<(int Value, string JobName)> _handler;

            internal SyncProgress(Action<(int Value, string JobName)> handler)
            {
                _handler = handler;
            }

            public void Report((int Value, string JobName) value) => _handler(value);
        }

        private static int GetChunkSafeInputCount() => Environment.ProcessorCount * 4;

        private static IItemInfo CreateItemInfo(string subject)
        {
            var mock = new Mock<IItemInfo>();
            mock.SetupProperty(x => x.Subject, subject);
            mock.SetupProperty(x => x.Sw, new SegmentStopWatch());
            return mock.Object;
        }

        private static string InvokeGetReportMessage(
            string messagePrefix,
            int complete,
            int count,
            Stopwatch stopwatch
        )
        {
            var method = typeof(AsyncMultiTasker).GetMethod(
                "GetReportMessage",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            method.Should().NotBeNull();
            var result = method.Invoke(null, [messagePrefix, complete, count, stopwatch]);
            result.Should().NotBeNull();
            return (string)result;
        }

        /// <summary>
        /// Verifies that the <see cref="AsyncMultiTasker"/> Action overload invokes
        /// the supplied action for every element in the input list.
        ///
        /// <para>
        /// Purpose:
        ///     Exercises the chunking/partition logic by confirming that all N inputs
        ///     are processed, regardless of the number of physical chunks produced.
        ///     (Exact chunk count is machine-dependent and not asserted.)
        /// </para>
        ///
        /// <para>
        /// Args:
        ///     n (int): <c>Environment.ProcessorCount * 4</c> — ensures chunkSize &gt;= 1
        ///     on any machine with &gt;= 2 logical cores.
        /// Returns:
        ///     Asserts that the processed-item counter equals n after await.
        /// </para>
        /// </summary>
        [TestMethod]
        public async Task AsyncMultiTaskChunker_ActionOverload_ProcessesAllItems()
        {
            // Arrange — count large enough to guarantee chunkSize >= 1 on any modern machine
            int n = GetChunkSafeInputCount();
            var inputs = Enumerable.Range(1, n).ToList();
            int processedCount = 0;
            var progress = new SyncProgress(_ => { });

            // Act — void-action overload; chunkNum = ProcessorCount-1
            await AsyncMultiTasker.AsyncMultiTaskChunker<int>(
                inputs,
                (item) => Interlocked.Increment(ref processedCount),
                progress,
                "Test",
                CancellationToken.None
            );

            // Assert — every input must be processed exactly once
            processedCount
                .Should()
                .Be(n, "all {0} items must be processed by the action overload", n);
        }

        /// <summary>
        /// Verifies that the synchronous <see cref="Func{T, TOut}"/> overload of
        /// <see cref="AsyncMultiTasker.AsyncMultiTaskChunker{T, TOut}"/> returns a
        /// result bag whose count equals the input count, and which contains a
        /// representative sample of the expected transformed values.
        ///
        /// <para>
        /// Purpose:
        ///     The async TOut overload requires TOut to implement IItemInfo; this sync
        ///     overload is the practical counterpart for arbitrary return types.
        ///     Tests result completeness and spot-checks membership.
        /// </para>
        ///
        /// <para>
        /// Returns:
        ///     Asserts ConcurrentBag count equals n and bag contains first and last
        ///     expected values.
        /// </para>
        /// </summary>
        [TestMethod]
        public async Task AsyncMultiTaskChunker_SyncFuncOverload_ReturnsCompleteResultBag()
        {
            // Arrange
            int n = GetChunkSafeInputCount();
            var inputs = Enumerable.Range(1, n).ToList();
            var progress = new SyncProgress(_ => { });

            // Act — sync Func<T, TOut> overload; finally always runs progress.Report(100,...)
            ConcurrentBag<string> results = await AsyncMultiTasker.AsyncMultiTaskChunker<
                int,
                string
            >(inputs, (item) => item.ToString(), progress, "Test", CancellationToken.None);

            // Assert — all n items must appear in the result bag
            results
                .Count.Should()
                .Be(n, "the result bag must contain one entry per input element");

            // Spot-check first and last expected string values
            results.Should().Contain("1", "the first input must produce a result");
            results.Should().Contain(n.ToString(), "the last input must produce a result");
        }

        [TestMethod]
        public async Task AsyncMultiTaskChunker_SyncFuncOverload_WhenWorkSpansTimerInterval_ReportsProgress()
        {
            int n = GetChunkSafeInputCount();
            var inputs = Enumerable.Range(1, n).ToList();
            var reports = new ConcurrentBag<(int Value, string JobName)>();
            var progress = new SyncProgress(r => reports.Add(r));

            // Inject a deterministic timer that fires its Elapsed event synchronously when the
            // production code starts it, producing a non-final progress report without a wall-clock
            // sleep simulating slow work.
            using var timerStub = new ManualFireTimerWrapper { FireOnStart = true };

            await AsyncMultiTasker.AsyncMultiTaskChunker<int, string>(
                inputs,
                item => item.ToString(),
                progress,
                "SyncFuncProgress",
                CancellationToken.None,
                timerFactory: _ => timerStub
            );

            reports.Should().Contain(r => r.JobName.StartsWith("SyncFuncProgress Completed "));
        }

        /// <summary>
        /// Verifies that the progress callback receives a terminal (100 %,
        /// "Operation Complete") notification after the async Func overload completes,
        /// as guaranteed by the finally block in <see cref="AsyncMultiTasker"/>.
        ///
        /// <para>
        /// Purpose:
        ///     The finally block of every AsyncMultiTaskChunker overload unconditionally
        ///     calls <c>progress.Report((100, "Operation Complete"))</c>.
        ///     This test confirms that contract, so callers relying on the 100 % signal
        ///     to finalize UI or pipeline state are protected against regressions.
        /// </para>
        ///
        /// <para>
        /// Returns:
        ///     Asserts the report bag contains at least one entry with Value == 100 and
        ///     JobName == "Operation Complete".
        /// </para>
        /// </summary>
        [TestMethod]
        public async Task AsyncMultiTaskChunker_WhenComplete_ReportsTerminalProgressSignal()
        {
            // Arrange
            int n = GetChunkSafeInputCount();
            var inputs = Enumerable.Range(1, n).ToList();
            var reports = new ConcurrentBag<(int Value, string JobName)>();

            // SyncProgress fires the handler on the thread that calls Report, which is the
            // AsyncMultiTaskChunker's own thread inside the finally block
            var progress = new SyncProgress(r => reports.Add(r));

            // Act
            await AsyncMultiTasker.AsyncMultiTaskChunker<int, string>(
                inputs,
                (item) => item.ToString(),
                progress,
                "Test",
                CancellationToken.None
            );

            // Assert — finally block guarantees exactly one terminal (100, "Operation Complete")
            reports
                .Should()
                .Contain(
                    r => r.Value == 100 && r.JobName == "Operation Complete",
                    "the finally block must always report (100, 'Operation Complete')"
                );
        }

        [TestMethod]
        public async Task AsyncMultiTaskChunker_AsyncFuncOverload_ReturnsCompleteItemBag()
        {
            int n = GetChunkSafeInputCount();
            var inputs = Enumerable.Range(1, n).ToList();
            var progress = new SyncProgress(_ => { });

            ConcurrentBag<IItemInfo> results = await AsyncMultiTasker.AsyncMultiTaskChunker<
                int,
                IItemInfo
            >(
                inputs,
                async item =>
                {
                    await Task.Delay(10);
                    return CreateItemInfo(item.ToString());
                },
                progress,
                "AsyncFunc",
                CancellationToken.None
            );

            results.Count.Should().Be(n);
            results.Select(x => x.Subject).Should().Contain("1");
            results.Select(x => x.Subject).Should().Contain(n.ToString());
        }

        [TestMethod]
        public async Task AsyncMultiTaskChunker_AsyncFuncOverload_WhenWorkSpansTimerInterval_ReportsProgress()
        {
            int n = GetChunkSafeInputCount();
            var inputs = Enumerable.Range(1, n).ToList();
            var reports = new ConcurrentBag<(int Value, string JobName)>();
            var progress = new SyncProgress(r => reports.Add(r));

            await AsyncMultiTasker.AsyncMultiTaskChunker<int, IItemInfo>(
                inputs,
                async item =>
                {
                    await Task.Delay(350);
                    return CreateItemInfo(item.ToString());
                },
                progress,
                "AsyncFunc",
                CancellationToken.None
            );

            reports.Should().Contain(r => r.JobName.StartsWith("AsyncFunc Completed "));
        }

        [TestMethod]
        public async Task AsyncMultiTaskChunker_AsyncFuncOverload_WhenWorkerCancels_ReturnsEmptyBag()
        {
            int n = GetChunkSafeInputCount();
            var inputs = Enumerable.Range(1, n).ToList();
            var progress = new SyncProgress(_ => { });
            Func<int, Task<IItemInfo>> func = async item =>
            {
                await Task.Yield();
                throw new OperationCanceledException($"cancel {item}");
            };

            ConcurrentBag<IItemInfo> results = await AsyncMultiTasker.AsyncMultiTaskChunker<
                int,
                IItemInfo
            >(inputs, func, progress, "AsyncFuncCancel", CancellationToken.None);

            results.Should().BeEmpty();
        }

        [TestMethod]
        public async Task AsyncMultiTaskChunker_AsyncFuncOverload_WhenResultsDoNotImplementIItemInfo_Throws()
        {
            int n = GetChunkSafeInputCount();
            var inputs = Enumerable.Range(1, n).ToList();
            var progress = new SyncProgress(_ => { });

            Func<Task> act = async () =>
                await AsyncMultiTasker.AsyncMultiTaskChunker<int, string>(
                    inputs,
                    async item =>
                    {
                        await Task.Delay(10);
                        return item.ToString();
                    },
                    progress,
                    "AsyncFuncBadResult",
                    CancellationToken.None
                );

            await act.Should().ThrowAsync<InvalidCastException>();
        }

        [TestMethod]
        public async Task AsyncMultiTaskChunker_AsyncTaskOverload_ProcessesAllItems()
        {
            int n = GetChunkSafeInputCount();
            var inputs = Enumerable.Range(1, n).ToList();
            int processedCount = 0;
            var progress = new SyncProgress(_ => { });

            await AsyncMultiTasker.AsyncMultiTaskChunker<int>(
                inputs,
                async item =>
                {
                    await Task.Delay(10);
                    Interlocked.Increment(ref processedCount);
                },
                progress,
                "AsyncTask",
                CancellationToken.None
            );

            processedCount.Should().Be(n);
        }

        [TestMethod]
        public async Task AsyncMultiTaskChunker_AsyncTaskOverload_WhenWorkSpansTimerInterval_ReportsProgressAndCompletion()
        {
            int n = GetChunkSafeInputCount();
            var inputs = Enumerable.Range(1, n).ToList();
            var reports = new ConcurrentBag<(int Value, string JobName)>();
            var progress = new SyncProgress(r => reports.Add(r));

            await AsyncMultiTasker.AsyncMultiTaskChunker<int>(
                inputs,
                async item =>
                {
                    await Task.Delay(350);
                },
                progress,
                "AsyncTask",
                CancellationToken.None
            );

            reports.Should().Contain(r => r.JobName.StartsWith("AsyncTask Completed "));
            reports.Should().Contain(r => r.Value == 100 && r.JobName == "Operation Complete");
        }

        [TestMethod]
        public async Task AsyncMultiTaskChunker_AsyncTaskOverload_WhenTokenAlreadyCanceled_CompletesGracefully()
        {
            int n = GetChunkSafeInputCount();
            var inputs = Enumerable.Range(1, n).ToList();
            var reports = new ConcurrentBag<(int Value, string JobName)>();
            var progress = new SyncProgress(r => reports.Add(r));
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Func<Task> act = async () =>
                await AsyncMultiTasker.AsyncMultiTaskChunker<int>(
                    inputs,
                    async item =>
                    {
                        await Task.Delay(10);
                    },
                    progress,
                    "AsyncTaskCanceled",
                    cts.Token
                );

            await act.Should().NotThrowAsync();
            reports.Should().Contain(r => r.Value == 100 && r.JobName == "Operation Complete");
        }

        [TestMethod]
        public async Task AsyncMultiTaskChunker_SyncFuncOverload_WhenTokenAlreadyCanceled_ThrowsTaskCanceledException()
        {
            int n = GetChunkSafeInputCount();
            var inputs = Enumerable.Range(1, n).ToList();
            var reports = new ConcurrentBag<(int Value, string JobName)>();
            var progress = new SyncProgress(r => reports.Add(r));
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Func<Task> act = async () =>
                await AsyncMultiTasker.AsyncMultiTaskChunker<int, string>(
                    inputs,
                    item => item.ToString(),
                    progress,
                    "SyncFuncCanceled",
                    cts.Token
                );

            await act.Should().ThrowAsync<TaskCanceledException>();
            reports.Should().Contain(r => r.Value == 100 && r.JobName == "Operation Complete");
        }

        [TestMethod]
        public async Task AsyncMultiTaskChunker_ActionOverload_WhenTokenAlreadyCanceled_CompletesAndReportsCompletion()
        {
            int n = GetChunkSafeInputCount();
            var inputs = Enumerable.Range(1, n).ToList();
            var reports = new ConcurrentBag<(int Value, string JobName)>();
            var progress = new SyncProgress(r => reports.Add(r));
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Func<Task> act = async () =>
                await AsyncMultiTasker.AsyncMultiTaskChunker<int>(
                    inputs,
                    _ => { },
                    progress,
                    "ActionCanceled",
                    cts.Token
                );

            await act.Should().NotThrowAsync();
            reports.Should().Contain(r => r.Value == 100 && r.JobName == "Operation Complete");
        }

        [TestMethod]
        public async Task AsyncMultiTaskChunker_ActionOverload_WhenWorkSpansTimerInterval_ReportsProgress()
        {
            int n = GetChunkSafeInputCount();
            var inputs = Enumerable.Range(1, n).ToList();
            var reports = new ConcurrentBag<(int Value, string JobName)>();
            var progress = new SyncProgress(r => reports.Add(r));

            // Inject a deterministic timer that fires its Elapsed event synchronously when the
            // production code starts it, producing a non-final progress report without a wall-clock
            // sleep simulating slow work.
            using var timerStub = new ManualFireTimerWrapper { FireOnStart = true };

            await AsyncMultiTasker.AsyncMultiTaskChunker<int>(
                inputs,
                _ => { },
                progress,
                "ActionProgress",
                CancellationToken.None,
                timerFactory: _ => timerStub
            );

            reports.Should().Contain(r => r.JobName.StartsWith("ActionProgress Completed "));
        }

        [TestMethod]
        public void GetReportMessage_WhenInvoked_FormatsPrefixAndCounts()
        {
            // A started-then-stopped Stopwatch provides a real (tiny, non-zero) elapsed for message
            // formatting without a wall-clock sleep. SpinUntil bounds the advance deterministically.
            var stopwatch = Stopwatch.StartNew();
            SpinWait.SpinUntil(() => stopwatch.Elapsed > TimeSpan.Zero, 100);
            stopwatch.Stop();

            var message = InvokeGetReportMessage("Report", 0, 5, stopwatch);

            message.Should().Contain("Report Completed 0 of 5");
            message.Should().Contain("elapsed");
            message.Should().Contain("remaining");
        }
    }
}
