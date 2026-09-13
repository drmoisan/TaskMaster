using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS.Test.OutlookObjects.Table
{
    /// <summary>
    /// Failure-contract tests for <see cref="OlTableExtensions.GetTableInViewAsync"/>. Each test
    /// drives one of the routes on which the shared time-out helper returns its default value or
    /// raises, and asserts the exception the method's documented contract promises. No test here
    /// waits on wall time: every deadline source is supplied through the factory seam and is either
    /// already cancelled or never cancelled, so the outcome does not depend on elapsed time.
    /// </summary>
    [TestClass]
    public class GetTableInViewAsyncFailureContractTests
    {
        /// <summary>
        /// Builds an Explorer whose TableView returns the supplied table. The gate action runs
        /// inside the GetTable setup before the table is returned, which lets a test count the
        /// table-read invocations the acquisition actually performed.
        /// </summary>
        private static Mock<Outlook.Explorer> BuildExplorer(
            Mock<Outlook.Table> table,
            System.Action onGetTable
        )
        {
            var mockTableView = new Mock<Outlook.TableView>();
            var mockExplorer = new Mock<Outlook.Explorer>();

            mockTableView
                .Setup(x => x.GetTable())
                .Returns(() =>
                {
                    onGetTable();
                    return table.Object;
                });
            mockExplorer.SetupGet(x => x.CurrentView).Returns(mockTableView.Object);

            return mockExplorer;
        }

        /// <summary>
        /// The parameter-Type array GetTableInViewAsync is bound by. It is declared once so a
        /// future signature change is corrected in one place rather than at every call site.
        /// </summary>
        private static Type[] SignatureTypes =>
            new[]
            {
                typeof(Outlook.Explorer),
                typeof(CancellationToken),
                typeof(int),
                typeof(int),
                typeof(Func<int, CancellationTokenSource>),
                typeof(TimeProvider),
            };

        /// <summary>
        /// Invokes GetTableInViewAsync reflectively and returns its boxed result. Reflection is
        /// required rather than preferred: the method returns Task of Outlook.Table, and Outlook
        /// types are embedded interop types, so a direct await from this assembly is rejected with
        /// CS1769.
        /// </summary>
        private static async Task<object> InvokeGetTableInViewAsync(
            Type[] parameterTypes,
            params object[] args
        )
        {
            var method = typeof(OlTableExtensions).GetMethod(
                "GetTableInViewAsync",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                types: parameterTypes,
                modifiers: null
            );

            method.Should().NotBeNull("the reflective binding must match the current signature");

            var taskObject = method.Invoke(null, args);
            taskObject.Should().BeAssignableTo<Task>();

            var task = (Task)taskObject;
            await task;
            return task.GetType().GetProperty("Result").GetValue(task);
        }

        /// <summary>
        /// When the shared helper exhausts its own retry budget it returns the default value rather
        /// than raising, so the method reaches its final guard with a null local while the caller's
        /// token is uncancelled. The contract for that state is a TimeoutException. The injected
        /// factory returns an already-cancelled source, so the acquisition's linked token is
        /// cancelled before the work is scheduled and the table read is never entered.
        /// </summary>
        [TestMethod]
        public async Task GetTableInViewAsync_RunWithTimeoutExhaustsRetries_ThrowsTimeoutException()
        {
            // Arrange
            var factoryInvocations = 0;
            var tableReadInvocations = 0;
            using var outerSource = new CancellationTokenSource();
            var mockTable = new Mock<Outlook.Table>();
            var mockExplorer = BuildExplorer(mockTable, () => tableReadInvocations++);

            Func<int, CancellationTokenSource> alreadyCancelledFactory = _ =>
            {
                factoryInvocations++;
                var source = new CancellationTokenSource();
                source.Cancel();
                return source;
            };

            // Act
            Func<Task> act = () =>
                InvokeGetTableInViewAsync(
                    SignatureTypes,
                    mockExplorer.Object,
                    outerSource.Token,
                    0,
                    750,
                    alreadyCancelledFactory,
                    null
                );

            // Assert
            await act.Should()
                .ThrowAsync<TimeoutException>(
                    "an exhausted acquisition budget is a timeout, not a silent null"
                );
            factoryInvocations.Should().Be(2);
            tableReadInvocations.Should().Be(0);
        }

        /// <summary>
        /// A caller's token cancelled while the acquisition is in flight must surface as
        /// cancellation, not as a null table. The factory cancels the test-owned source from inside
        /// its own body rather than before the call, because the shared helper checks the outer token
        /// before it invokes the factory, so a token already cancelled at method entry would take a
        /// different route and would not exercise the cancellation branch of the catch under test.
        /// </summary>
        [TestMethod]
        public async Task GetTableInViewAsync_TimeoutSourceThrowsTaskCanceledAfterCancellingOuterToken_PropagatesCancellation()
        {
            // Arrange
            using var outerSource = new CancellationTokenSource();
            var mockTable = new Mock<Outlook.Table>();
            var mockExplorer = BuildExplorer(mockTable, () => { });

            Func<int, CancellationTokenSource> cancellingFactory = _ =>
            {
                outerSource.Cancel();
                throw new TaskCanceledException(
                    "Injected by the test at the deadline-source construction point."
                );
            };

            // Act
            Func<Task> act = () =>
                InvokeGetTableInViewAsync(
                    SignatureTypes,
                    mockExplorer.Object,
                    outerSource.Token,
                    0,
                    750,
                    cancellingFactory,
                    null
                );

            // Assert
            await act.Should()
                .ThrowAsync<OperationCanceledException>(
                    "a cancelled caller token is reported as cancellation rather than as a null table"
                );
        }

        /// <summary>
        /// At the retry ceiling the task-cancelled route has no further attempt to make, so it must
        /// report a timeout naming the retry it gave up on and the budget each attempt was given. The
        /// caller's token is never cancelled, so the cancellation branch of the catch is not taken.
        /// </summary>
        [TestMethod]
        public async Task GetTableInViewAsync_CounterAtRetryCeilingWithTaskCanceled_ThrowsTimeoutException()
        {
            // Arrange
            using var outerSource = new CancellationTokenSource();
            var mockTable = new Mock<Outlook.Table>();
            var mockExplorer = BuildExplorer(mockTable, () => { });

            Func<int, CancellationTokenSource> throwingFactory = _ =>
                throw new TaskCanceledException(
                    "Injected by the test at the deadline-source construction point."
                );

            // Act
            Func<Task> act = () =>
                InvokeGetTableInViewAsync(
                    SignatureTypes,
                    mockExplorer.Object,
                    outerSource.Token,
                    2,
                    750,
                    throwingFactory,
                    null
                );

            // Assert
            var thrown = await act.Should().ThrowAsync<TimeoutException>();
            thrown.Which.Message.Should().Contain("retry 2");
            thrown.Which.Message.Should().Contain("750 ms");
        }

        /// <summary>
        /// At the retry ceiling the timeout route must wrap the originating exception rather than
        /// rethrow it, so the cause survives for the caller. Asserting reference identity of the
        /// inner exception is what distinguishes the new wrapper from a bare rethrow: a test that
        /// asserted only the thrown type would pass in both designs.
        /// </summary>
        [TestMethod]
        public async Task GetTableInViewAsync_CounterAtRetryCeilingWithTimeout_ThrowsTimeoutExceptionPreservingInner()
        {
            // Arrange
            using var outerSource = new CancellationTokenSource();
            var mockTable = new Mock<Outlook.Table>();
            var mockExplorer = BuildExplorer(mockTable, () => { });
            var injectedTimeout = new TimeoutException(
                "Injected by the test at the deadline-source construction point."
            );

            Func<int, CancellationTokenSource> throwingFactory = _ => throw injectedTimeout;

            // Act
            Func<Task> act = () =>
                InvokeGetTableInViewAsync(
                    SignatureTypes,
                    mockExplorer.Object,
                    outerSource.Token,
                    2,
                    750,
                    throwingFactory,
                    null
                );

            // Assert
            var thrown = await act.Should().ThrowAsync<TimeoutException>();
            thrown.Which.InnerException.Should().BeSameAs(injectedTimeout);
        }

        /// <summary>
        /// The shared helper's generic catch absorbs any exception other than the task-cancelled type
        /// when it is called without the strict flag, and then returns its default value. The method
        /// under fix therefore reaches its final guard with a null local while the caller's token is
        /// cancelled. This is the production-reachable state in which a guard that threw the timeout
        /// before checking the token would mislabel cancellation as a timeout, so the test passes only
        /// when the cancellation check comes first.
        /// </summary>
        [TestMethod]
        public async Task GetTableInViewAsync_AbsorbedDefaultWithOuterTokenCancelled_ThrowsOperationCanceledNotTimeout()
        {
            // Arrange
            var tableReadInvocations = 0;
            using var outerSource = new CancellationTokenSource();
            var mockTable = new Mock<Outlook.Table>();
            var mockExplorer = BuildExplorer(
                mockTable,
                () =>
                {
                    tableReadInvocations++;
                    outerSource.Cancel();
                    throw new InvalidOperationException(
                        "Injected by the test from inside the table read."
                    );
                }
            );

            Func<int, CancellationTokenSource> neverCancellingFactory =
                _ => new CancellationTokenSource();

            // Act
            Func<Task> act = () =>
                InvokeGetTableInViewAsync(
                    SignatureTypes,
                    mockExplorer.Object,
                    outerSource.Token,
                    0,
                    750,
                    neverCancellingFactory,
                    null
                );

            // Assert
            await act.Should()
                .ThrowAsync<OperationCanceledException>(
                    "the guard checks the caller's token before it reports a timeout"
                );
            tableReadInvocations.Should().Be(1);
        }
    }
}
