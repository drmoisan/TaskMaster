using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Regression tests for issue #493, covering the contract of
    /// <see cref="UiThreadDispatcherFixture"/> and <see cref="UiThreadDispatcherTransaction"/>.
    /// <para>
    /// R1 is the primary deterministic regression assertion and R4 is the supporting probabilistic
    /// one. R1 reproduces the issue #230 clobber precondition with no concurrency at all and proves
    /// the clobber itself is unreachable, and the clobber rather than the scheduling is the actual
    /// #230 mechanism. R4 exercises two concurrent transactions, but under a broken implementation it
    /// fails only probabilistically, because nothing can force the second caller to reach its
    /// acquisition point while the first still holds the gate and there is no deterministic way to
    /// prove the second caller is currently blocked without a timed wait, which the repository's
    /// determinism rules forbid.
    /// </para>
    /// <para>
    /// Every test carries the 60-second MSTest timeout attribute so a genuine deadlock becomes a
    /// test failure rather than a hung run. All cross-thread coordination uses
    /// <see cref="ManualResetEventSlim"/> or awaited <see cref="Task"/> completion; there is no
    /// sleep, no delay, no wall-clock wait, and no temporary file.
    /// </para>
    /// </summary>
    [TestClass]
    public class QfcItemController_UiThreadDispatcherFixtureTests
    {
        private const int GateTimeoutMs = 60000;

        /// <summary>
        /// R1 — the exact issue #230 clobber precondition. While a transaction holds a live
        /// dispatcher, the ensure helper must observe a non-null field and install nothing, so the
        /// live dispatcher survives both the ensure call and the disposal of the ensure scope.
        /// </summary>
        [TestMethod]
        [Timeout(GateTimeoutMs)]
        public async Task EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt()
        {
            // Arrange
            Dispatcher liveA = QfcItemControllerTestSupport.StartRunningDispatcher();
            try
            {
                UiThreadDispatcherTransaction transaction = await UiThreadDispatcherFixture
                    .BeginTransactionAsync()
                    .ConfigureAwait(false);
                try
                {
                    Dispatcher original = UiThreadDispatcherFixture.Current;
                    transaction.Install(liveA);

                    // Act
                    IDisposable ensureScope =
                        QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
                    Dispatcher afterEnsure = UiThreadDispatcherFixture.Current;
                    ensureScope.Dispose();
                    Dispatcher afterEnsureScopeDisposed = UiThreadDispatcherFixture.Current;

                    // Assert
                    afterEnsure
                        .Should()
                        .BeSameAs(
                            liveA,
                            because: "EnsureDispatcher installs only when the field is null, so a live "
                                + "transaction value must survive the ensure call"
                        );
                    afterEnsureScopeDisposed
                        .Should()
                        .BeSameAs(
                            liveA,
                            because: "an ensure scope that installed nothing is a no-op and must not "
                                + "write over the transaction's value"
                        );

                    transaction.Dispose();
                    UiThreadDispatcherFixture
                        .Current.Should()
                        .BeSameAs(
                            original,
                            because: "disposing the transaction restores the value captured at install"
                        );
                }
                finally
                {
                    transaction.Dispose();
                }
            }
            finally
            {
                QfcItemControllerTestSupport.ShutdownDispatcher(liveA);
            }
        }

        /// <summary>
        /// R2 — restore when no prior dispatcher existed. With the field forced to a known null
        /// baseline, the ensure helper installs the parked dispatcher and its scope reverts the field
        /// to null on disposal.
        /// </summary>
        [TestMethod]
        [Timeout(GateTimeoutMs)]
        public async Task EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose()
        {
            // Arrange
            UiThreadDispatcherTransaction transaction = await UiThreadDispatcherFixture
                .BeginTransactionAsync()
                .ConfigureAwait(false);
            try
            {
                Dispatcher original = UiThreadDispatcherFixture.Current;
                transaction.Install(null);

                // Act
                IDisposable ensureScope = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
                Dispatcher afterEnsure = UiThreadDispatcherFixture.Current;
                ensureScope.Dispose();
                Dispatcher afterEnsureScopeDisposed = UiThreadDispatcherFixture.Current;

                // Assert
                afterEnsure
                    .Should()
                    .NotBeNull(
                        because: "EnsureDispatcher seeds the parked dispatcher when the field is null"
                    );
                afterEnsureScopeDisposed
                    .Should()
                    .BeNull(
                        because: "the ensure scope reverts its own seeding, and null is the only value "
                            + "it can ever need to restore"
                    );

                transaction.Dispose();
                UiThreadDispatcherFixture
                    .Current.Should()
                    .BeSameAs(
                        original,
                        because: "disposing the transaction restores the value captured at install"
                    );
            }
            finally
            {
                transaction.Dispose();
            }
        }

        /// <summary>
        /// R3 — the ensure scope's disposal is idempotent. A second <c>Dispose</c> must neither throw
        /// nor change the field.
        /// </summary>
        [TestMethod]
        [Timeout(GateTimeoutMs)]
        public async Task EnsureDispatcher_ScopeDisposedTwice_IsIdempotent()
        {
            // Arrange
            UiThreadDispatcherTransaction transaction = await UiThreadDispatcherFixture
                .BeginTransactionAsync()
                .ConfigureAwait(false);
            try
            {
                transaction.Install(null);
                IDisposable ensureScope = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();

                // Act
                ensureScope.Dispose();
                Dispatcher afterFirstDispose = UiThreadDispatcherFixture.Current;
                Action secondDispose = () => ensureScope.Dispose();

                // Assert
                secondDispose
                    .Should()
                    .NotThrow(
                        because: "the ensure scope guards its disposal with a _disposed flag"
                    );
                UiThreadDispatcherFixture
                    .Current.Should()
                    .BeSameAs(
                        afterFirstDispose,
                        because: "a second Dispose must not re-write the static"
                    );
            }
            finally
            {
                transaction.Dispose();
            }
        }

        /// <summary>
        /// R4 — a second caller cannot install until the first has restored. The waiting transaction
        /// observes the pre-install value on acquisition, never the first transaction's installed
        /// value, because restore strictly precedes gate release.
        /// </summary>
        [TestMethod]
        [Timeout(GateTimeoutMs)]
        public async Task Transaction_SecondCallerCannotInstallUntilTheFirstRestores()
        {
            // Arrange
            Dispatcher liveA = QfcItemControllerTestSupport.StartRunningDispatcher();
            try
            {
                UiThreadDispatcherTransaction transactionA = await UiThreadDispatcherFixture
                    .BeginTransactionAsync()
                    .ConfigureAwait(false);
                Dispatcher original = UiThreadDispatcherFixture.Current;
                transactionA.Install(liveA);

                using (var secondCallerStarted = new ManualResetEventSlim(false))
                {
                    Dispatcher observedByB = null;

                    Task waiter = Task.Run(async () =>
                    {
                        secondCallerStarted.Set();
                        UiThreadDispatcherTransaction transactionB = await UiThreadDispatcherFixture
                            .BeginTransactionAsync()
                            .ConfigureAwait(false);
                        try
                        {
                            observedByB = UiThreadDispatcherFixture.Current;
                        }
                        finally
                        {
                            transactionB.Dispose();
                        }
                    });

                    // Act
                    secondCallerStarted.Wait();
                    transactionA.Dispose();
                    await waiter.ConfigureAwait(false);

                    // Assert
                    observedByB
                        .Should()
                        .BeSameAs(
                            original,
                            because: "the first transaction restores before it releases the gate, so "
                                + "the waiter cannot observe the pre-restore value"
                        );
                    observedByB
                        .Should()
                        .NotBeSameAs(
                            liveA,
                            because: "observing the first transaction's installed value would be the "
                                + "issue #230 lost update"
                        );
                }
            }
            finally
            {
                QfcItemControllerTestSupport.ShutdownDispatcher(liveA);
            }
        }

        /// <summary>
        /// R5 — a double-disposed transaction does not over-release the gate. A second
        /// <c>Release</c> on a <c>SemaphoreSlim(1, 1)</c> would throw
        /// <c>SemaphoreFullException</c> and would corrupt the gate for every later caller.
        /// </summary>
        [TestMethod]
        [Timeout(GateTimeoutMs)]
        public async Task Transaction_DisposedTwice_DoesNotOverReleaseTheGate()
        {
            // Arrange
            Dispatcher liveA = QfcItemControllerTestSupport.StartRunningDispatcher();
            try
            {
                UiThreadDispatcherTransaction transaction = await UiThreadDispatcherFixture
                    .BeginTransactionAsync()
                    .ConfigureAwait(false);
                transaction.Install(liveA);
                transaction.Dispose();

                // Act
                Action secondDispose = () => transaction.Dispose();

                // Assert
                secondDispose
                    .Should()
                    .NotThrow(
                        because: "a second Dispose must not call Release again, which would throw "
                            + "SemaphoreFullException on a SemaphoreSlim(1, 1)"
                    );

                UiThreadDispatcherTransaction roundTrip = await UiThreadDispatcherFixture
                    .BeginTransactionAsync()
                    .ConfigureAwait(false);
                roundTrip.Dispose();
                UiThreadDispatcherFixture
                    .Current.Should()
                    .NotBeSameAs(
                        liveA,
                        because: "the gate is still sound, so the round trip completed and left the "
                            + "restored value in place rather than the first transaction's install"
                    );
            }
            finally
            {
                QfcItemControllerTestSupport.ShutdownDispatcher(liveA);
            }
        }

        /// <summary>
        /// R6 — a second <c>Install</c> on the same transaction fails fast. Allowing it would discard
        /// the captured previous value and make the restore unsound.
        /// </summary>
        [TestMethod]
        [Timeout(GateTimeoutMs)]
        public async Task Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException()
        {
            // Arrange
            Dispatcher liveA = QfcItemControllerTestSupport.StartRunningDispatcher();
            try
            {
                UiThreadDispatcherTransaction transaction = await UiThreadDispatcherFixture
                    .BeginTransactionAsync()
                    .ConfigureAwait(false);
                try
                {
                    transaction.Install(null);

                    // Act
                    Action secondInstall = () => transaction.Install(liveA);

                    // Assert
                    secondInstall
                        .Should()
                        .Throw<InvalidOperationException>(
                            because: "Install is a one-shot operation per transaction and must fail "
                                + "fast rather than discard the captured previous value"
                        );
                }
                finally
                {
                    transaction.Dispose();
                }
            }
            finally
            {
                QfcItemControllerTestSupport.ShutdownDispatcher(liveA);
            }
        }
    }
}
