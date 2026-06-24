using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Deterministic verification of the per-phase NET attribution formatter and net computation
    /// added to <see cref="TaskMaster.StartupDiagnosticsProbe"/> for the issue #211 Phase 3.6
    /// <c>[phase-net]</c> probe. The pure formatting and the clamp rule are exercised through an
    /// injected list-capturing sink and the static <c>ComputeNetMs</c> helper, with no live COM,
    /// live timer, live Dispatcher, network/filesystem, or temporary files.
    /// </summary>
    [TestClass]
    public class PhaseNetProbeTests
    {
        [TestMethod]
        public void EmitPhaseNet_RepresentativePhase_ProducesExactString()
        {
            // Arrange
            var captured = new List<string>();
            var probe = new TaskMaster.StartupDiagnosticsProbe(captured.Add);

            // Act
            probe.EmitPhaseNet("Engines", 68000.0, 67500.0, 500.0);

            // Assert
            captured
                .Should()
                .ContainSingle()
                .Which.Should()
                .Be(
                    "[phase-net] phase=Engines grossMs=68000.0 storeWrapperInitMs=67500.0 netMs=500.0"
                );
        }

        [TestMethod]
        public void EmitPhaseNet_FormatsAllMsFieldsWithF1AndInvariantCulture()
        {
            // Arrange
            var captured = new List<string>();
            var probe = new TaskMaster.StartupDiagnosticsProbe(captured.Add);

            // Act
            probe.EmitPhaseNet("ToDo", 1234.56, 12.34, 1222.22);

            // Assert (F1 rounds to one decimal; InvariantCulture uses '.' as the separator)
            captured[0]
                .Should()
                .Be("[phase-net] phase=ToDo grossMs=1234.6 storeWrapperInitMs=12.3 netMs=1222.2");
        }

        [TestMethod]
        public void EmitPhaseNet_EmitsExactlyOnce()
        {
            // Arrange
            var captured = new List<string>();
            var probe = new TaskMaster.StartupDiagnosticsProbe(captured.Add);

            // Act
            probe.EmitPhaseNet("IntelConfig", 10.0, 4.0, 6.0);

            // Assert
            captured.Should().HaveCount(1);
        }

        [TestMethod]
        public void ComputeNetMs_GrossGreaterThanStoreInit_ReturnsDifference()
        {
            // Arrange / Act
            var net = TaskMaster.StartupDiagnosticsProbe.ComputeNetMs(100.0, 30.0);

            // Assert
            net.Should().Be(70.0);
        }

        [TestMethod]
        public void ComputeNetMs_StoreInitGreaterThanGross_ClampsToZero()
        {
            // Arrange / Act (concurrent store init on another thread can exceed the phase window)
            var net = TaskMaster.StartupDiagnosticsProbe.ComputeNetMs(30.0, 100.0);

            // Assert
            net.Should().Be(0.0);
        }

        [TestMethod]
        public void ComputeNetMs_GrossEqualsStoreInit_ReturnsZero()
        {
            // Arrange / Act (boundary equality)
            var net = TaskMaster.StartupDiagnosticsProbe.ComputeNetMs(50.0, 50.0);

            // Assert
            net.Should().Be(0.0);
        }
    }
}
