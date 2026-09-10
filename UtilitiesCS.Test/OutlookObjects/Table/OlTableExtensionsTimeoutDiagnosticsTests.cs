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
    /// Branch tests for the two timeout diagnostics in
    /// <see cref="OlTableExtensions.GetTableInViewAsync"/>. Each test forces entry into one of the
    /// two catch clauses that carry a diagnostic, by injecting a timeout-source factory that throws
    /// on its first invocation. The factory is invoked outside the try block in
    /// TimeOutTask.RunWithTimeout, so the thrown type escapes RunWithTimeout and selects the catch
    /// clause in GetTableInViewAsync. No test here waits on wall time, touches the filesystem, or
    /// asserts anything about the process-global console writer.
    /// </summary>
    [TestClass]
    public class OlTableExtensionsTimeoutDiagnosticsTests
    {
        /// <summary>
        /// Builds an Explorer whose TableView returns the supplied table and increments the supplied
        /// counter on every GetTable call.
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
        /// The parameter-Type array GetTableInViewAsync is bound by, re-derived from the current
        /// signature. It is declared once so a future signature change is corrected in one place
        /// rather than at every call site.
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
        /// types are embedded interop types, so a direct await of it from this assembly is rejected
        /// with CS1769.
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
        /// A factory that throws the supplied exception on its first invocation and returns a fresh
        /// parameterless CancellationTokenSource on every later invocation. A new source per call is
        /// required: RunWithTimeout holds the source in a using declaration and disposes it at the
        /// end of each attempt, so returning the same instance would make the retry read Token on a
        /// disposed source. The parameterless constructor is used deliberately so this file adds no
        /// new banned-symbol call site.
        /// </summary>
        private static Func<int, CancellationTokenSource> ThrowOnFirstCallFactory(
            Func<Exception> exceptionFactory,
            Action onInvoked
        )
        {
            var invocationCount = 0;

            return _ =>
            {
                invocationCount++;
                onInvoked();

                if (invocationCount == 1)
                {
                    throw exceptionFactory();
                }

                return new CancellationTokenSource();
            };
        }

        /// <summary>
        /// A TimeoutException thrown by the injected factory escapes RunWithTimeout and enters
        /// catch (TimeoutException) in GetTableInViewAsync, which retries once with counter + 1 and
        /// passes the same factory through. The second attempt's factory call succeeds and the
        /// acquisition returns the mocked table.
        /// </summary>
        [TestMethod]
        public async Task GetTableInViewAsync_TimeoutSourceThrowsTimeout_EntersTimeoutCatchAndRetriesOnce()
        {
            // Arrange
            var factoryInvocations = 0;
            var getTableCalls = 0;
            var mockTable = new Mock<Outlook.Table>();
            var mockExplorer = BuildExplorer(mockTable, () => getTableCalls++);
            var factory = ThrowOnFirstCallFactory(
                () =>
                    new TimeoutException(
                        "Injected by the test at the deadline-source construction point."
                    ),
                () => factoryInvocations++
            );

            // Act: the outer token is never cancelled, so no cancellation path is taken.
            var result = await InvokeGetTableInViewAsync(
                SignatureTypes,
                mockExplorer.Object,
                CancellationToken.None,
                0,
                750,
                factory,
                null
            );

            // Assert
            factoryInvocations
                .Should()
                .Be(
                    2,
                    "the first invocation throws before the try is entered and the bounded retry "
                        + "constructs a second deadline source"
                );
            getTableCalls
                .Should()
                .Be(
                    1,
                    "the first attempt throws before Task.Run is reached, so only the retry calls "
                        + "GetTable"
                );
            result
                .Should()
                .BeSameAs(
                    mockTable.Object,
                    "the retry completes normally and returns the acquired table"
                );
        }

        /// <summary>
        /// A TaskCanceledException thrown by the injected factory escapes RunWithTimeout and enters
        /// catch (TaskCanceledException) in GetTableInViewAsync. The outer token is not cancelled, so
        /// token.IsCancellationRequested is false and control enters the else branch that carries the
        /// second diagnostic, which retries once with counter + 1.
        /// </summary>
        [TestMethod]
        public async Task GetTableInViewAsync_TimeoutSourceThrowsTaskCanceled_EntersCancelCatchElseAndRetriesOnce()
        {
            // Arrange
            var factoryInvocations = 0;
            var getTableCalls = 0;
            var mockTable = new Mock<Outlook.Table>();
            var mockExplorer = BuildExplorer(mockTable, () => getTableCalls++);
            var factory = ThrowOnFirstCallFactory(
                () =>
                    new TaskCanceledException(
                        "Injected by the test at the deadline-source construction point."
                    ),
                () => factoryInvocations++
            );

            // Act: CancellationToken.None is never cancelled, which is what selects the else branch
            // rather than the table = null branch.
            var result = await InvokeGetTableInViewAsync(
                SignatureTypes,
                mockExplorer.Object,
                CancellationToken.None,
                0,
                750,
                factory,
                null
            );

            // Assert
            factoryInvocations
                .Should()
                .Be(
                    2,
                    "the first invocation throws before the try is entered and the bounded retry "
                        + "constructs a second deadline source"
                );
            getTableCalls
                .Should()
                .Be(
                    1,
                    "the first attempt throws before Task.Run is reached, so only the retry calls "
                        + "GetTable"
                );
            result
                .Should()
                .BeSameAs(
                    mockTable.Object,
                    "the retry completes normally and returns the acquired table"
                );
        }
    }
}
