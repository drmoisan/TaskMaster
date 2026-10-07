using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Issue #968 tests for the reference-counted ensure pin of
    /// <see cref="UiThreadDispatcherFixture"/>. The regression lives at the fixture level because
    /// the theme tests the issue was filed against never read the shared static: their path
    /// dispatches through the theme's injected <c>IUiDispatcher</c> mock, so no value in the static
    /// can make a theme test fail, while the defect (the installing pin's release writing
    /// <c>null</c> while another pin is still live) is observable here on one thread with no
    /// concurrency.
    /// <para>
    /// Every test acquires a transaction first, so the pin count is zero and the baseline is known
    /// for the whole test, and carries the 60-second MSTest timeout of the sibling fixture test
    /// file. No sleep, delay, wall-clock wait, mock or temporary file is used; Moq is therefore
    /// not imported.
    /// </para>
    /// </summary>
    [TestClass]
    public class QfcItemController_UiThreadDispatcherPinCountTests
    {
        private const int GateTimeoutMs = 60000;

        /// <summary>
        /// Regression test: fails before the fix. With two pins held on a null baseline, releasing
        /// the first pin must leave the parked dispatcher in place. Before counting, the first pin
        /// was the installer and its release wrote <c>null</c> while the second pin was still live.
        /// </summary>
        [TestMethod]
        [Timeout(GateTimeoutMs)]
        public async Task EnsureDispatcher_TwoPinsHeld_ReleasingTheFirstKeepsTheDispatcherUntilTheLastRelease()
        {
            // Arrange
            UiThreadDispatcherTransaction transaction = await UiThreadDispatcherFixture
                .BeginTransactionAsync()
                .ConfigureAwait(false);
            try
            {
                transaction.Install(null);
                IDisposable pinA = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
                IDisposable pinB = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
                Dispatcher afterBothPins = UiThreadDispatcherFixture.Current;

                // Act
                pinA.Dispose();
                Dispatcher afterFirstRelease = UiThreadDispatcherFixture.Current;
                pinB.Dispose();
                Dispatcher afterLastRelease = UiThreadDispatcherFixture.Current;

                // Assert
                afterBothPins
                    .Should()
                    .NotBeNull(
                        because: "the first pin seeds the parked dispatcher into a null field"
                    );
                afterFirstRelease
                    .Should()
                    .BeSameAs(
                        afterBothPins,
                        because: "a holder that did not take the last pin must not lose the dispatcher"
                    );
                afterLastRelease
                    .Should()
                    .BeNull(because: "the last release reverts the fixture's own seeding");
            }
            finally
            {
                transaction.Dispose();
            }
        }

        /// <summary>
        /// Specification test: passes before and after the fix. Releasing the pins in the reverse
        /// order must produce the same outcome, so the count rather than the identity of the
        /// installing scope decides when the field reverts.
        /// </summary>
        [TestMethod]
        [Timeout(GateTimeoutMs)]
        public async Task EnsureDispatcher_TwoPinsHeld_ReleaseOrderDoesNotChangeTheOutcome()
        {
            // Arrange
            UiThreadDispatcherTransaction transaction = await UiThreadDispatcherFixture
                .BeginTransactionAsync()
                .ConfigureAwait(false);
            try
            {
                transaction.Install(null);
                IDisposable pinA = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
                IDisposable pinB = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
                Dispatcher afterBothPins = UiThreadDispatcherFixture.Current;

                // Act
                pinB.Dispose();
                Dispatcher afterFirstRelease = UiThreadDispatcherFixture.Current;
                pinA.Dispose();
                Dispatcher afterLastRelease = UiThreadDispatcherFixture.Current;

                // Assert
                afterBothPins
                    .Should()
                    .NotBeNull(
                        because: "the first pin seeds the parked dispatcher into a null field"
                    );
                afterFirstRelease
                    .Should()
                    .BeSameAs(
                        afterBothPins,
                        because: "a holder that did not take the last pin must not lose the dispatcher"
                    );
                afterLastRelease
                    .Should()
                    .BeNull(because: "the last release reverts the fixture's own seeding");
            }
            finally
            {
                transaction.Dispose();
            }
        }

        /// <summary>
        /// Specification test: passes before and after the fix; extends the existing fixture test
        /// EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt. While a
        /// transaction holds a live dispatcher, two pins install nothing, and releasing both must
        /// leave the live dispatcher in place: the count reaching zero writes nothing because the
        /// fixture did not seed the field. The live dispatcher is shut down in a finally block.
        /// </summary>
        [TestMethod]
        [Timeout(GateTimeoutMs)]
        public async Task EnsureDispatcher_UnderATransactionHoldingALiveDispatcher_ReleasingAllPinsLeavesTheLiveDispatcher()
        {
            // Arrange
            Dispatcher live = QfcItemControllerTestSupport.StartRunningDispatcher();
            try
            {
                UiThreadDispatcherTransaction transaction = await UiThreadDispatcherFixture
                    .BeginTransactionAsync()
                    .ConfigureAwait(false);
                try
                {
                    transaction.Install(live);
                    IDisposable pinA = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
                    IDisposable pinB = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();

                    // Act
                    pinA.Dispose();
                    pinB.Dispose();
                    Dispatcher afterAllReleased = UiThreadDispatcherFixture.Current;

                    // Assert
                    afterAllReleased
                        .Should()
                        .BeSameAs(
                            live,
                            because: "pins that installed nothing must not write over the transaction value "
                                + "when the count reaches zero"
                        );
                }
                finally
                {
                    transaction.Dispose();
                }
            }
            finally
            {
                QfcItemControllerTestSupport.ShutdownDispatcher(live);
            }
        }

        /// <summary>
        /// Specification test: passes before and after the fix. After a full two-pin cycle inside the
        /// same transaction, a fresh single pin on the null baseline must still seed the parked
        /// dispatcher and its release must still restore null. A second transaction then installs
        /// that parked instance as its own value, and a pin taken and released under it must leave
        /// the value in place: had the earlier cycle's last release left the install-ownership flag
        /// set, this release would revert a value the fixture did not seed.
        /// </summary>
        [TestMethod]
        [Timeout(GateTimeoutMs)]
        public async Task EnsureDispatcher_AfterAFullPinCycle_AFreshSinglePinStillInstallsAndRestores()
        {
            // Arrange
            Dispatcher parked;
            UiThreadDispatcherTransaction transaction = await UiThreadDispatcherFixture
                .BeginTransactionAsync()
                .ConfigureAwait(false);
            try
            {
                transaction.Install(null);
                IDisposable pinA = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
                IDisposable pinB = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
                parked = UiThreadDispatcherFixture.Current;
                pinA.Dispose();
                pinB.Dispose();

                // Act
                IDisposable freshPin = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
                Dispatcher afterFreshPin = UiThreadDispatcherFixture.Current;
                freshPin.Dispose();
                Dispatcher afterFreshRelease = UiThreadDispatcherFixture.Current;

                // Assert
                afterFreshPin
                    .Should()
                    .NotBeNull(
                        because: "a pin on a null field seeds the parked dispatcher whatever earlier cycles did"
                    );
                afterFreshRelease
                    .Should()
                    .BeNull(
                        because: "the fresh pin is the only live pin, so its release reverts the seeding"
                    );
            }
            finally
            {
                transaction.Dispose();
            }

            // Act (second transaction): the parked instance is now a transaction value, not a seeding
            UiThreadDispatcherTransaction foreignTransaction = await UiThreadDispatcherFixture
                .BeginTransactionAsync()
                .ConfigureAwait(false);
            try
            {
                foreignTransaction.Install(parked);
                IDisposable foreignPin = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
                foreignPin.Dispose();
                Dispatcher afterForeignRelease = UiThreadDispatcherFixture.Current;

                // Assert
                afterForeignRelease
                    .Should()
                    .BeSameAs(
                        parked,
                        because: "the last release cleared the install-ownership flag, so a pin that seeded nothing leaves a transaction value in place"
                    );
            }
            finally
            {
                foreignTransaction.Dispose();
            }
        }
    }
}
