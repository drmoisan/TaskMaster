using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Store;

namespace UtilitiesCS.Test.OutlookObjects.Store
{
    /// <summary>
    /// Deterministic tests for <see cref="StoreWrapperInitClock"/>, the process-global thread-safe
    /// accumulator introduced for the issue #211 Phase 3.6 store-init shared-cost attribution probe.
    /// The class is marked <see cref="DoNotParallelizeAttribute"/> and resets the accumulator before
    /// and after each test because the clock is process-global static state shared across tests.
    /// No live COM, no live timer, no network/filesystem, no temporary files.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class StoreWrapperInitClockTests
    {
        [TestInitialize]
        public void TestInitialize() => StoreWrapperInitClock.Reset();

        [TestCleanup]
        public void TestCleanup() => StoreWrapperInitClock.Reset();

        [TestMethod]
        public void TotalMs_FreshAfterReset_IsZero()
        {
            // Arrange / Act (Reset is performed in TestInitialize)
            // Assert
            StoreWrapperInitClock.TotalMs.Should().Be(0.0);
        }

        [TestMethod]
        public void Add_SingleValue_TotalMsEqualsThatValue()
        {
            // Arrange
            const double ms = 67500.0;

            // Act
            StoreWrapperInitClock.Add(ms);

            // Assert (microsecond rounding tolerance: 0.001 ms = 1 microsecond)
            StoreWrapperInitClock.TotalMs.Should().BeApproximately(ms, 0.001);
        }

        [TestMethod]
        public void Add_MultipleSequentialValues_SumsTotal()
        {
            // Arrange
            // Act
            StoreWrapperInitClock.Add(10.0);
            StoreWrapperInitClock.Add(25.5);
            StoreWrapperInitClock.Add(4.5);

            // Assert
            StoreWrapperInitClock.TotalMs.Should().BeApproximately(40.0, 0.001);
        }

        [TestMethod]
        public void Reset_AfterAdds_ReturnsTotalMsToZero()
        {
            // Arrange
            StoreWrapperInitClock.Add(123.4);

            // Act
            StoreWrapperInitClock.Reset();

            // Assert
            StoreWrapperInitClock.TotalMs.Should().Be(0.0);
        }

        [TestMethod]
        public void Add_NegativeInput_IsTreatedAsZeroAndDoesNotDecrease()
        {
            // Arrange
            StoreWrapperInitClock.Add(50.0);

            // Act
            StoreWrapperInitClock.Add(-100.0);

            // Assert (no decrease; negative add contributes 0)
            StoreWrapperInitClock.TotalMs.Should().BeApproximately(50.0, 0.001);
        }

        [TestMethod]
        public void Add_ConcurrentCalls_AccumulatesWithoutLostUpdates()
        {
            // Arrange
            const int iterations = 1000;
            const double fixedMs = 2.0;

            // Act
            Parallel.For(0, iterations, _ => StoreWrapperInitClock.Add(fixedMs));

            // Assert (thread-safe accumulation: total equals N * fixedMs with no lost updates)
            StoreWrapperInitClock.TotalMs.Should().BeApproximately(iterations * fixedMs, 0.001);
        }
    }
}
