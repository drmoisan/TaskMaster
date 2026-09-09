using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.Test.TestHelpers;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS.Test.OutlookObjects.Table
{
    /// <summary>
    /// Deadline-mechanics tests for <see cref="OlTableExtensions.GetTableInViewAsync"/>. Every
    /// deadline these tests exercise is driven by an injected clock or neutralised by a
    /// never-cancelling source supplied through the factory seam, so no test here waits on wall
    /// time.
    /// </summary>
    [TestClass]
    // Not parallelized: this class drives a real Task.Run gate.
    [DoNotParallelize]
    public class GetTableInViewAsyncClockTests
    {
        /// <summary>
        /// Builds an Explorer whose TableView returns the supplied table. The gate action runs
        /// inside the GetTable setup before the table is returned, which lets a test hold the
        /// acquisition open while it inspects the deadline the call armed.
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
        /// Wraps a never-advancing clock in the arming barrier. The barrier signals its Armed task
        /// after forwarding CreateTimer, so awaiting Armed proves a timer was created on the
        /// injected provider.
        /// </summary>
        private static ArmingBarrierTimeProvider CreateArmingBarrier() =>
            new ArmingBarrierTimeProvider(new FakeTimeProvider());

        /// <summary>
        /// Invokes GetTableInViewAsync reflectively and returns its boxed result. Reflection is
        /// required rather than preferred: the method returns Task of Outlook.Table, and Outlook
        /// types are embedded interop types, so a direct await from this assembly is rejected with
        /// CS1769. The same mechanism is used by the four existing binding sites in
        /// OlTableExtensions_Tests.
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
        /// The TimeoutException retry must arm its second attempt on the caller's timeoutMs rather
        /// than on the literal 2000 the pre-change file passed. The supplied factory records every
        /// millisecond value it receives and throws on its first invocation; the factory is invoked
        /// outside the try in RunWithTimeout, so the throw reaches the TimeoutException catch in
        /// GetTableInViewAsync and the recursion invokes the factory a second time.
        /// </summary>
        [TestMethod]
        public async Task GetTableInViewAsync_TimeoutRetry_UsesCallerTimeoutMsNotLiteral2000()
        {
            // Arrange
            var recordedTimeouts = new List<int>();
            var invocationCount = 0;
            var mockTable = new Mock<Outlook.Table>();
            var mockExplorer = BuildExplorer(mockTable, () => { });

            Func<int, CancellationTokenSource> recordingFactory = ms =>
            {
                recordedTimeouts.Add(ms);
                invocationCount++;

                if (invocationCount == 1)
                {
                    throw new TimeoutException(
                        "Injected by the test at the deadline-source construction point."
                    );
                }

                return new CancellationTokenSource();
            };

            // Act
            await InvokeGetTableInViewAsync(
                SignatureTypes,
                mockExplorer.Object,
                CancellationToken.None,
                0,
                750,
                recordingFactory,
                null
            );

            // Assert
            recordedTimeouts
                .Should()
                .HaveCount(
                    2,
                    "the first attempt throws from the factory and the retry constructs a second "
                        + "deadline source"
                );
            recordedTimeouts[1]
                .Should()
                .Be(
                    750,
                    "the retry must arm on the caller's timeoutMs rather than on a literal 2000"
                );
        }

        /// <summary>
        /// The table-acquisition deadline must be armed on the caller's clock. The awaited
        /// barrier.Armed cannot complete unless a timer was created on the injected provider, so
        /// this test is the empirical proof that the deadline is under the caller's control. The
        /// fake clock is never advanced: advancing past timeoutMs would cancel the acquisition and
        /// RunWithTimeout would return default, making the returned table null.
        /// </summary>
        [TestMethod]
        public async Task GetTableInViewAsync_InjectedClock_ArmsAcquisitionDeadlineOnInjectedProvider()
        {
            // Arrange
            var acquisitionGate = new ManualResetEventSlim(false);
            var barrier = CreateArmingBarrier();
            var mockTable = new Mock<Outlook.Table>();
            var mockExplorer = BuildExplorer(mockTable, acquisitionGate.Wait);

            try
            {
                // Act: start the call without awaiting it, so the acquisition is still open when
                // the barrier is inspected.
                Task<object> call = InvokeGetTableInViewAsync(
                    SignatureTypes,
                    mockExplorer.Object,
                    CancellationToken.None,
                    0,
                    2000,
                    null,
                    barrier
                );

                await barrier.Armed;

                // The gate is released inside the try. Releasing it only in the finally would
                // leave the awaited call blocked on a gate nothing sets, and because the assertion
                // sits inside the try the failure mode would be a hang rather than a failure.
                acquisitionGate.Set();

                var result = await call;

                // Assert
                result
                    .Should()
                    .BeSameAs(
                        mockTable.Object,
                        "the acquisition completes normally once the gate is released"
                    );
            }
            finally
            {
                // Released a second time so an orphaned Task.Run body cannot outlive the test on
                // the failure path.
                acquisitionGate.Set();
            }
        }

        /// <summary>
        /// An explicitly supplied timeoutSourceFactory must still win over the clock-derived one,
        /// which is what keeps the existing direct-caller seam working unchanged. The barrier's
        /// Armed task stays incomplete because no timer is ever created on the provider.
        /// </summary>
        [TestMethod]
        public async Task GetTableInViewAsync_ExplicitFactorySupplied_TakesPrecedenceOverTimeProvider()
        {
            // Arrange
            var factoryInvocations = 0;
            var barrier = CreateArmingBarrier();
            var mockTable = new Mock<Outlook.Table>();
            var mockExplorer = BuildExplorer(mockTable, () => { });

            Func<int, CancellationTokenSource> recordingFactory = _ =>
            {
                factoryInvocations++;
                return new CancellationTokenSource();
            };

            // Act
            var result = await InvokeGetTableInViewAsync(
                SignatureTypes,
                mockExplorer.Object,
                CancellationToken.None,
                0,
                2000,
                recordingFactory,
                barrier
            );

            // Assert
            result.Should().BeSameAs(mockTable.Object);
            factoryInvocations
                .Should()
                .Be(1, "the explicitly supplied factory is the one the deadline is built from");
            barrier
                .Armed.IsCompleted.Should()
                .BeFalse("the provider must not have created a timer when a factory was supplied");
        }

        /// <summary>
        /// The default path: with both optional parameters omitted the method runs on the system
        /// clock exactly as it did before this change. This guards production timing against an
        /// accidental change.
        /// </summary>
        [TestMethod]
        public async Task GetTableInViewAsync_NoTimeProviderSupplied_UsesSystemClockAndCompletes()
        {
            // Arrange
            var getTableCalls = 0;
            var mockTable = new Mock<Outlook.Table>();
            var mockExplorer = BuildExplorer(mockTable, () => getTableCalls++);

            // Act
            var result = await InvokeGetTableInViewAsync(
                SignatureTypes,
                mockExplorer.Object,
                CancellationToken.None,
                0,
                2000,
                null,
                null
            );

            // Assert
            result.Should().BeSameAs(mockTable.Object);
            getTableCalls
                .Should()
                .Be(1, "the immediate return needs no retry on the default system clock");
        }
    }
}
