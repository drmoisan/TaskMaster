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
using UtilitiesCS.OutlookObjects.Fields;

namespace UtilitiesCS.Test.Extensions
{
    /// <summary>
    /// Regression tests for the QuickFiler column-add timeout path and its timing instrumentation.
    /// Not parallelized: the assembly parallelizes at class level and this class drives
    /// process-wide log4net state that a concurrent class would observe or overwrite.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class DfDeedleQfcColumnTimeoutTests
    {
        /// <summary>Production deadline, in milliseconds, for one column-add attempt.</summary>
        private const int DeadlineMilliseconds = 3000;

        /// <summary>The six column operations AC2 requires to be timed individually.</summary>
        private static readonly string[] ColumnOperationNames =
        {
            "SentOn",
            MAPIFields.Schemas.ConversationId,
            MAPIFields.Schemas.Triage,
            "Subject",
            "CreationTime",
            "LastModificationTime",
        };

        /// <summary>
        /// A <see cref="TimeProvider"/> forwarding every member to an inner
        /// <see cref="FakeTimeProvider"/> unchanged and, after forwarding <c>CreateTimer</c>,
        /// completing a signal that the production loop has armed its next deadline.
        /// </summary>
        /// <remarks>
        /// Forwarding keeps timer ownership with the inner provider. Consecutive <c>Advance</c>
        /// calls are prohibited: each deadline is armed only after the previous proxy faults, so
        /// advancing past a deadline the loop has not created hangs the test. <c>TrySetResult</c>
        /// is used because the loop can arm one more timer than a test drives.
        /// </remarks>
        private sealed class ArmingBarrierTimeProvider : TimeProvider
        {
            private readonly FakeTimeProvider _inner;
            private volatile TaskCompletionSource<bool> _armed = NewSignal();

            internal ArmingBarrierTimeProvider(FakeTimeProvider inner) => _inner = inner;

            internal Task Armed => _armed.Task;

            internal void ReArm() => _armed = NewSignal();

            internal void Advance(int ms) => _inner.Advance(TimeSpan.FromMilliseconds(ms));

            public override DateTimeOffset GetUtcNow() => _inner.GetUtcNow();

            public override long GetTimestamp() => _inner.GetTimestamp();

            public override TimeZoneInfo LocalTimeZone => _inner.LocalTimeZone;
            public override long TimestampFrequency => _inner.TimestampFrequency;

            public override ITimer CreateTimer(
                TimerCallback callback,
                object state,
                TimeSpan dueTime,
                TimeSpan period
            )
            {
                var timer = _inner.CreateTimer(callback, state, dueTime, period);
                _armed.TrySetResult(true);
                return timer;
            }

            // Continuations run asynchronously so a signal never resumes a test inline.
            internal static TaskCompletionSource<bool> NewSignal() =>
                new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        /// <summary>
        /// Injectable column adder that counts invocations, signals entry, then blocks on the gate
        /// so a deadline is the only thing able to end the attempt. The gate is a delegate.
        /// </summary>
        private sealed class ColumnAdderProbe
        {
            private readonly System.Action _waitForGate;
            private volatile TaskCompletionSource<bool> _entered =
                ArmingBarrierTimeProvider.NewSignal();
            private int _invocations;

            internal ColumnAdderProbe(System.Action waitForGate) => _waitForGate = waitForGate;

            internal int Invocations => Volatile.Read(ref _invocations);
            internal Task Entered => _entered.Task;

            internal void ReArm() => _entered = ArmingBarrierTimeProvider.NewSignal();

            internal Action<object, object> Adder =>
                (table, folder) =>
                {
                    Interlocked.Increment(ref _invocations);
                    _entered.TrySetResult(true);
                    _waitForGate();
                };
        }

        /// <summary>Starts the production call against the injected adder and the barrier clock.</summary>
        private static Task StartColumnAdd(
            ArmingBarrierTimeProvider barrier,
            ColumnAdderProbe probe,
            string folderName,
            CancellationToken token
        ) =>
            DfDeedle.AddQfcColumnsAsync(
                BuildTable(),
                BuildNamedFolder(folderName),
                token,
                0,
                probe.Adder,
                barrier
            );

        /// <summary>Releases one deadline: awaits adder entry and timer arming, re-arms only the
        /// arming signal, then advances. The held work enters the adder exactly once.</summary>
        private static async Task FireOneDeadlineAsync(
            ArmingBarrierTimeProvider barrier,
            ColumnAdderProbe probe
        )
        {
            await probe.Entered.ConfigureAwait(false);
            await barrier.Armed.ConfigureAwait(false);
            barrier.ReArm();
            barrier.Advance(DeadlineMilliseconds);
        }

        /// <summary>After cancellation, releases any deadline still armed until the call completes.
        /// Bounded by the remaining budget and guarded by <c>Task.WhenAny</c>.</summary>
        private static async Task DrainRemainingDeadlinesAsync(
            ArmingBarrierTimeProvider barrier,
            ColumnAdderProbe probe,
            Task call
        )
        {
            for (var remaining = 2; remaining > 0 && !call.IsCompleted; remaining--)
            {
                await Task.WhenAny(barrier.Armed, call).ConfigureAwait(false);
                if (call.IsCompleted)
                {
                    return;
                }

                probe.ReArm();
                barrier.ReArm();
                barrier.Advance(DeadlineMilliseconds);
            }
        }

        /// <summary>Yields <paramref name="exception"/> and every exception nested inside it.</summary>
        private static IEnumerable<System.Exception> Unwrap(System.Exception exception)
        {
            if (exception is null)
            {
                yield break;
            }

            yield return exception;
            var nested = exception is AggregateException aggregate
                ? aggregate.InnerExceptions.AsEnumerable()
                : new[] { exception.InnerException };
            foreach (var inner in nested.SelectMany(Unwrap))
            {
                yield return inner;
            }
        }

        /// <summary>Builds a <see cref="Table"/> whose column operations are no-ops.</summary>
        private static Table BuildTable()
        {
            var columns = new Mock<Columns>(MockBehavior.Loose);
            var table = new Mock<Table>(MockBehavior.Loose);
            table.SetupGet(t => t.Columns).Returns(columns.Object);
            return table.Object;
        }

        /// <summary>Builds a <see cref="MAPIFolder"/> reporting the supplied name.</summary>
        private static MAPIFolder BuildNamedFolder(string folderName)
        {
            var folder = new Mock<MAPIFolder>(MockBehavior.Loose);
            folder.SetupGet(f => f.Name).Returns(folderName);
            return folder.Object;
        }

        /// <summary>Builds a <c>T&amp;E</c> folder carrying one user-defined property, so that
        /// <c>EnsureTriageColumnExists</c> succeeds without reaching <c>MessageBoxInvoker</c>.</summary>
        private static MAPIFolder BuildFolderWithUdp(string udpName)
        {
            var property = new Mock<UserDefinedProperty>(MockBehavior.Loose);
            property.SetupGet(p => p.Name).Returns(udpName);
            var entries = new List<UserDefinedProperty> { property.Object };

            var properties = new Mock<UserDefinedProperties>(MockBehavior.Loose);
            properties
                .Setup(u => u.GetEnumerator())
                .Returns(() => (System.Collections.IEnumerator)entries.GetEnumerator());

            var folder = new Mock<MAPIFolder>(MockBehavior.Loose);
            folder.SetupGet(f => f.UserDefinedProperties).Returns(properties.Object);
            folder.SetupGet(f => f.Name).Returns("T&E");
            return folder.Object;
        }

        /// <summary>Runs <paramref name="action"/> with a <c>MemoryAppender</c> on the logger named
        /// by <c>typeof(DfDeedle).FullName</c>, restoring process-wide log4net state after.</summary>
        private static IReadOnlyList<string> CaptureDfDeedleLog(System.Action action)
        {
            var repository = (log4net.Repository.Hierarchy.Hierarchy)
                log4net.LogManager.GetRepository();
            var logger = (log4net.Repository.Hierarchy.Logger)
                repository.GetLogger(typeof(DfDeedle).FullName);
            var appender = new log4net.Appender.MemoryAppender();
            var previousLevel = logger.Level;
            // Neither UtilitiesCS nor UtilitiesCS.Test carries a log4net configurator attribute, so
            // this repository is unconfigured here and Hierarchy.IsDisabled reports every level
            // disabled, making the production Debug call a no-op however the appender is attached.
            var previousConfigured = repository.Configured;
            repository.Configured = true;
            appender.ActivateOptions();

            logger.AddAppender(appender);
            logger.Level = log4net.Core.Level.Debug;
            try
            {
                action();
                return appender.GetEvents().Select(entry => entry.RenderedMessage).ToList();
            }
            finally
            {
                logger.RemoveAppender(appender);
                logger.Level = previousLevel;
                repository.Configured = previousConfigured;
            }
        }

        /// <summary>Resolves a private static <see cref="DfDeedle"/> method by name.</summary>
        private static MethodInfo PrivateStatic(string name) =>
            typeof(DfDeedle).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>Reports whether any captured message is a timing line naming the token.</summary>
        private static bool HasTimingLineNaming(IReadOnlyList<string> messages, string token) =>
            messages.Any(message =>
                message.IndexOf("[Df timing]", StringComparison.Ordinal) >= 0
                && message.IndexOf(token, StringComparison.Ordinal) >= 0
            );

        /// <summary>
        /// AC1/AC7: the column-add work must be started exactly once and re-deadlined, so a slow
        /// COM call is never overlapped by a second concurrent call against the same table.
        /// </summary>
        [TestMethod]
        public async Task AddQfcColumnsAsync_ThreeDeadlines_InvokesColumnAdderExactlyOnce()
        {
            // Arrange
            var gate = new ManualResetEventSlim(false);
            var barrier = new ArmingBarrierTimeProvider(new FakeTimeProvider());
            var probe = new ColumnAdderProbe(gate.Wait);
            var cancellation = new CancellationTokenSource();

            try
            {
                // Act
                var call = StartColumnAdd(barrier, probe, "Inbox", cancellation.Token);
                await FireOneDeadlineAsync(barrier, probe);
                await FireOneDeadlineAsync(barrier, probe);
                await FireOneDeadlineAsync(barrier, probe);
                await FluentActions.Awaiting(() => call).Should().ThrowAsync<TimeoutException>();

                // Assert
                probe.Invocations.Should().Be(1, "the work is re-deadlined, never restarted");
            }
            finally
            {
                // Released in finally, not on the success path: the fail-before outcome is an
                // assertion failure, which would otherwise leave thread-pool threads blocked.
                gate.Set();
            }
        }

        /// <summary>
        /// AC1: once the final deadline expires the call must fail loudly with a message naming the
        /// folder and the step, rather than returning normally and degrading the launch silently.
        /// </summary>
        [TestMethod]
        public async Task AddQfcColumnsAsync_ThirdDeadlineExpires_ThrowsNamingFolderAndStep()
        {
            // Arrange
            var gate = new ManualResetEventSlim(false);
            var barrier = new ArmingBarrierTimeProvider(new FakeTimeProvider());
            var probe = new ColumnAdderProbe(gate.Wait);
            var cancellation = new CancellationTokenSource();

            try
            {
                // Act
                var call = StartColumnAdd(barrier, probe, "T&E", cancellation.Token);
                await FireOneDeadlineAsync(barrier, probe);
                await FireOneDeadlineAsync(barrier, probe);
                await FireOneDeadlineAsync(barrier, probe);

                // Assert
                Func<Task> awaitCall = () => call;
                var thrown = await awaitCall
                    .Should()
                    .ThrowAsync<System.Exception>("an exhausted budget must reach the caller");
                var flattened = string.Join(" | ", Unwrap(thrown.Which).Select(e => e.Message));
                flattened
                    .Should()
                    .Contain("T&E", "the failure must name the folder")
                    .And.Contain("column add", "the failure must name the step");
            }
            finally
            {
                gate.Set();
            }
        }

        /// <summary>Drives the loop so the adder completes just before deadline
        /// <paramref name="deadlineNumber"/>; returns the observed invocation count.</summary>
        /// <remarks>
        /// For a <paramref name="deadlineNumber"/> of 1 the barrier is not awaited and no deadline
        /// fires. A timer is still armed, because <c>Task.Run</c> leaves the task incomplete at the
        /// <c>TimeoutAfter</c> call, but the clock is never advanced so it cannot fire.
        /// </remarks>
        private static async Task<int> RunCompletingBeforeDeadlineAsync(int deadlineNumber)
        {
            // Arrange
            var gate = new ManualResetEventSlim(false);
            var barrier = new ArmingBarrierTimeProvider(new FakeTimeProvider());
            var probe = new ColumnAdderProbe(gate.Wait);
            var cancellation = new CancellationTokenSource();

            try
            {
                // Act
                var call = StartColumnAdd(barrier, probe, "Inbox", cancellation.Token);
                for (var fired = 0; fired < deadlineNumber - 1; fired++)
                {
                    await FireOneDeadlineAsync(barrier, probe);
                }

                gate.Set();
                await call;
                return probe.Invocations;
            }
            finally
            {
                gate.Set();
            }
        }

        /// <summary>AC1 positive path: the adder completes before the first deadline.</summary>
        [TestMethod]
        public async Task AddQfcColumnsAsync_AdderCompletesBeforeFirstDeadline_ReturnsWithoutThrowingAndStartsOneTask()
        {
            // Arrange
            var invocations = 0;
            Func<Task> act = async () => invocations = await RunCompletingBeforeDeadlineAsync(1);

            // Act / Assert
            await act.Should().NotThrowAsync("a column add inside its budget is not a failure");
            invocations.Should().Be(1, "no deadline expired, so no retry may start a second task");
        }

        /// <summary>AC1 positive path: the adder completes before the second deadline.</summary>
        [TestMethod]
        public async Task AddQfcColumnsAsync_AdderCompletesBeforeSecondDeadline_ReturnsWithoutThrowingAndStartsOneTask()
        {
            // Arrange
            var invocations = 0;
            Func<Task> act = async () => invocations = await RunCompletingBeforeDeadlineAsync(2);

            // Act / Assert
            await act.Should().NotThrowAsync("a column add inside its budget is not a failure");
            invocations.Should().Be(1, "one expired deadline re-deadlines the work in flight");
        }

        /// <summary>AC1 positive path: the adder completes before the third deadline.</summary>
        [TestMethod]
        public async Task AddQfcColumnsAsync_AdderCompletesBeforeThirdDeadline_ReturnsWithoutThrowingAndStartsOneTask()
        {
            // Arrange
            var invocations = 0;
            Func<Task> act = async () => invocations = await RunCompletingBeforeDeadlineAsync(3);

            // Act / Assert
            await act.Should().NotThrowAsync("a column add inside its budget is not a failure");
            invocations.Should().Be(1, "two expired deadlines re-deadline the work in flight");
        }

        /// <summary>
        /// AC1 guard: cancelling mid-loop must not be converted into the user-facing column-add
        /// timeout AC1 introduces for a genuinely exhausted budget. Passes before and after the fix.
        /// </summary>
        [TestMethod]
        public async Task AddQfcColumnsAsync_CancellationRequestedMidLoop_DoesNotThrowTimeoutExhausted()
        {
            // Arrange
            var gate = new ManualResetEventSlim(false);
            var barrier = new ArmingBarrierTimeProvider(new FakeTimeProvider());
            var probe = new ColumnAdderProbe(gate.Wait);
            var cancellation = new CancellationTokenSource();

            try
            {
                // Act
                var call = StartColumnAdd(barrier, probe, "T&E", cancellation.Token);
                await FireOneDeadlineAsync(barrier, probe);
                cancellation.Cancel();
                await DrainRemainingDeadlinesAsync(barrier, probe, call);

                System.Exception observed = null;
                try
                {
                    await call;
                }
                catch (System.Exception thrown)
                {
                    observed = thrown;
                }

                // Assert
                var surfaced = Unwrap(observed)
                    .Any(e =>
                        e is TimeoutException
                        && e.Message.IndexOf("column add", StringComparison.Ordinal) >= 0
                    );
                surfaced.Should().BeFalse("user cancellation must stay silent");
            }
            finally
            {
                gate.Set();
            }
        }

        /// <summary>
        /// AC2: each of the three <c>Columns.Add</c> and three <c>Columns.Remove</c> operations must
        /// emit its own <c>[Df timing]</c> line, so a slow column is attributable. Existence is
        /// asserted, never a count: a concurrent class can add events but can never remove them.
        /// </summary>
        [TestMethod]
        public void AddQfcColumns_EmitsDfTimingLineForEachColumnOperation()
        {
            // Arrange
            var method = PrivateStatic("AddQfcColumns");
            var table = BuildTable();
            var folder = BuildFolderWithUdp("Triage");

            // Act
            var messages = CaptureDfDeedleLog(() =>
                method.Invoke(null, new object[] { table, folder })
            );

            // Assert
            foreach (var columnName in ColumnOperationNames)
            {
                HasTimingLineNaming(messages, columnName)
                    .Should()
                    .BeTrue("no [Df timing] line names the column operation '{0}'", columnName);
            }
        }

        /// <summary>
        /// AC2: the user-defined-property enumeration must be timed and emitted through the
        /// existing <c>[Df timing]</c> helper. Existence is asserted, never a count.
        /// </summary>
        [TestMethod]
        public void HasUserDefinedProperty_EmitsDfTimingLineForPropertyEnumeration()
        {
            // Arrange
            var method = PrivateStatic("HasUserDefinedProperty");
            var folder = BuildFolderWithUdp("Triage");

            // Act
            var messages = CaptureDfDeedleLog(() =>
                method.Invoke(null, new object[] { folder, "Triage" })
            );

            // Assert
            HasTimingLineNaming(messages, "HasUserDefinedProperty")
                .Should()
                .BeTrue("the property enumeration must emit its own [Df timing] line");
        }
    }
}
