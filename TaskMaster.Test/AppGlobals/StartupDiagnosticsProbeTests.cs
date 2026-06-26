using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Deterministic verification of the startup-diagnostics formatter (issue #211, Phase 3.1,
    /// <see cref="TaskMaster.StartupDiagnosticsProbe"/>). The probe's pure gap/GC-delta formatting
    /// is exercised through an injected list-capturing sink so the line structure is asserted
    /// without a live appender, live COM, a live timer, a live Dispatcher, or live GC reads. No
    /// network/filesystem and no temporary files are used.
    /// </summary>
    [TestClass]
    public class StartupDiagnosticsProbeTests
    {
        [TestMethod]
        public void EmitHeartbeat_LargeGapAndNearNominal_ComputesGapAndFormatsFields()
        {
            // Arrange: capture emitted lines through the injected sink.
            var emitted = new List<string>();
            var probe = new TaskMaster.StartupDiagnosticsProbe(s => emitted.Add(s));

            // Act: one tick with a large stall (1200 ms vs 250 ms nominal) and one near-nominal tick.
            probe.EmitHeartbeat(250, 1200);
            probe.EmitHeartbeat(250, 251);

            // Assert: each call emits exactly one [ui-heartbeat] line; the gap is actual - nominal,
            // formatted F1 (gapMs=950.0 for the stall, gapMs=1.0 for the near-nominal tick).
            emitted.Should().HaveCount(2);

            var stallLine = emitted[0];
            stallLine.Should().StartWith("[ui-heartbeat] ");
            stallLine.Should().Contain("nominalMs=250.0 ");
            stallLine.Should().Contain("actualMs=1200.0 ");
            stallLine.Should().Contain("gapMs=950.0");

            var nearNominalLine = emitted[1];
            nearNominalLine.Should().Contain("nominalMs=250.0 ");
            nearNominalLine.Should().Contain("actualMs=251.0 ");
            nearNominalLine.Should().Contain("gapMs=1.0");
        }

        [TestMethod]
        public void Constructor_NullSink_ThrowsArgumentNullException()
        {
            // Act / Assert: the emit sink is a required collaborator.
            Action act = () => new TaskMaster.StartupDiagnosticsProbe(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void EmitHeartbeatAndGcDelta_RepeatedCalls_EmitExactlyOneLinePerCall()
        {
            // Arrange
            var emitted = new List<string>();
            var probe = new TaskMaster.StartupDiagnosticsProbe(s => emitted.Add(s));

            // Act: three heartbeat ticks and two GC-delta emissions.
            probe.EmitHeartbeat(250, 300);
            probe.EmitHeartbeat(250, 260);
            probe.EmitHeartbeat(250, 1000);
            probe.EmitGcDelta(1, 0, 0, 100, false, "Interactive");
            probe.EmitGcDelta(2, 1, 0, 200, true, "Batch");

            // Assert: one line per call, no extra or missing lines (5 calls => 5 lines).
            emitted.Should().HaveCount(5);
            emitted[0].Should().StartWith("[ui-heartbeat] ");
            emitted[1].Should().StartWith("[ui-heartbeat] ");
            emitted[2].Should().StartWith("[ui-heartbeat] ");
            emitted[3].Should().StartWith("[gc-delta] ");
            emitted[4].Should().StartWith("[gc-delta] ");
        }

        [TestMethod]
        public void EmitGcDelta_TypicalDeltas_EmitsOneLineWithEveryField()
        {
            // Arrange
            var emitted = new List<string>();
            var probe = new TaskMaster.StartupDiagnosticsProbe(s => emitted.Add(s));

            // Act
            probe.EmitGcDelta(
                gen0Delta: 5,
                gen1Delta: 2,
                gen2Delta: 1,
                allocatedBytesDelta: 1048576,
                isServerGC: false,
                latencyMode: "Interactive"
            );

            // Assert: exactly one [gc-delta] line carrying every field value.
            emitted.Should().ContainSingle();
            var line = emitted[0];
            line.Should().StartWith("[gc-delta] ");
            line.Should().Contain("gen0=5 ");
            line.Should().Contain("gen1=2 ");
            line.Should().Contain("gen2=1 ");
            line.Should().Contain("allocatedBytesDelta=1048576 ");
            line.Should().Contain("isServerGC=False ");
            line.Should().Contain("latencyMode=Interactive");
        }

        [TestMethod]
        public void EmitGcDelta_NegativeAndZeroDeltas_FormatsBoundaryValuesWithoutSuppression()
        {
            // Arrange
            var emitted = new List<string>();
            var probe = new TaskMaster.StartupDiagnosticsProbe(s => emitted.Add(s));

            // Act: a net-collection case (negative allocated-bytes delta) and an all-zero case.
            probe.EmitGcDelta(
                gen0Delta: 0,
                gen1Delta: 0,
                gen2Delta: 1,
                allocatedBytesDelta: -2048,
                isServerGC: true,
                latencyMode: "SustainedLowLatency"
            );
            probe.EmitGcDelta(
                gen0Delta: 0,
                gen1Delta: 0,
                gen2Delta: 0,
                allocatedBytesDelta: 0,
                isServerGC: false,
                latencyMode: "Interactive"
            );

            // Assert: negative and zero boundary values are rendered literally, not suppressed.
            emitted.Should().HaveCount(2);

            var negativeLine = emitted[0];
            negativeLine.Should().StartWith("[gc-delta] ");
            negativeLine.Should().Contain("gen0=0 ");
            negativeLine.Should().Contain("gen2=1 ");
            negativeLine.Should().Contain("allocatedBytesDelta=-2048 ");
            negativeLine.Should().Contain("isServerGC=True ");
            negativeLine.Should().Contain("latencyMode=SustainedLowLatency");

            var zeroLine = emitted[1];
            zeroLine.Should().StartWith("[gc-delta] ");
            zeroLine.Should().Contain("gen0=0 ");
            zeroLine.Should().Contain("gen1=0 ");
            zeroLine.Should().Contain("gen2=0 ");
            zeroLine.Should().Contain("allocatedBytesDelta=0 ");
            zeroLine.Should().Contain("isServerGC=False ");
            zeroLine.Should().Contain("latencyMode=Interactive");
        }

        [TestMethod]
        public void EmitHeartbeatWithPhase_MultiplePhases_AnnotatesEachLineAndComputesGap()
        {
            // Arrange: capture emitted lines through the injected sink (issue #211, Phase 3.2).
            var emitted = new List<string>();
            var probe = new TaskMaster.StartupDiagnosticsProbe(s => emitted.Add(s));

            // Act: one tick attributed to the IntelConfig phase with a large stall (1200 ms vs
            // 250 ms nominal) and one near-nominal tick attributed to the ToDo phase.
            probe.EmitHeartbeat("IntelConfig", 250, 1200);
            probe.EmitHeartbeat("ToDo", 250, 251);

            // Assert: each call emits exactly one phase-annotated [ui-heartbeat] line with the
            // existing nominal/actual/gap fields and gap arithmetic (actual - nominal, F1).
            emitted.Should().HaveCount(2);

            var intelLine = emitted[0];
            intelLine.Should().StartWith("[ui-heartbeat] ");
            intelLine.Should().Contain("phase=IntelConfig ");
            intelLine.Should().Contain("nominalMs=250.0 ");
            intelLine.Should().Contain("actualMs=1200.0 ");
            intelLine.Should().Contain("gapMs=950.0");

            var toDoLine = emitted[1];
            toDoLine.Should().Contain("phase=ToDo ");
            toDoLine.Should().Contain("nominalMs=250.0 ");
            toDoLine.Should().Contain("actualMs=251.0 ");
            toDoLine.Should().Contain("gapMs=1.0");
        }

        [TestMethod]
        public void EmitGcDeltaWithPhase_TwoPhases_AnnotatesPhaseAndRendersBoundaryValues()
        {
            // Arrange (issue #211, Phase 3.2)
            var emitted = new List<string>();
            var probe = new TaskMaster.StartupDiagnosticsProbe(s => emitted.Add(s));

            // Act: a net-collection case (negative allocated-bytes delta) attributed to the Engines
            // phase and an all-zero case attributed to the Events phase.
            probe.EmitGcDelta(
                phase: "Engines",
                gen0Delta: 0,
                gen1Delta: 0,
                gen2Delta: 1,
                allocatedBytesDelta: -2048,
                isServerGC: true,
                latencyMode: "SustainedLowLatency"
            );
            probe.EmitGcDelta(
                phase: "Events",
                gen0Delta: 0,
                gen1Delta: 0,
                gen2Delta: 0,
                allocatedBytesDelta: 0,
                isServerGC: false,
                latencyMode: "Interactive"
            );

            // Assert: each line carries the phase annotation plus all existing fields, with
            // negative and zero boundary values rendered literally (not suppressed).
            emitted.Should().HaveCount(2);

            var enginesLine = emitted[0];
            enginesLine.Should().StartWith("[gc-delta] ");
            enginesLine.Should().Contain("phase=Engines ");
            enginesLine.Should().Contain("gen0=0 ");
            enginesLine.Should().Contain("gen1=0 ");
            enginesLine.Should().Contain("gen2=1 ");
            enginesLine.Should().Contain("allocatedBytesDelta=-2048 ");
            enginesLine.Should().Contain("isServerGC=True ");
            enginesLine.Should().Contain("latencyMode=SustainedLowLatency");

            var eventsLine = emitted[1];
            eventsLine.Should().StartWith("[gc-delta] ");
            eventsLine.Should().Contain("phase=Events ");
            eventsLine.Should().Contain("gen0=0 ");
            eventsLine.Should().Contain("gen1=0 ");
            eventsLine.Should().Contain("gen2=0 ");
            eventsLine.Should().Contain("allocatedBytesDelta=0 ");
            eventsLine.Should().Contain("isServerGC=False ");
            eventsLine.Should().Contain("latencyMode=Interactive");
        }

        [TestMethod]
        public void PhaseAnnotated_MixedHeartbeatAndGcDeltaSequence_EmitsOneLinePerCallInOrder()
        {
            // Arrange (issue #211, Phase 3.2)
            var emitted = new List<string>();
            var probe = new TaskMaster.StartupDiagnosticsProbe(s => emitted.Add(s));

            // Act: three phase-annotated heartbeat ticks then two phase-annotated GC-delta emissions.
            probe.EmitHeartbeat("IntelConfig", 250, 300);
            probe.EmitHeartbeat("IntelConfig", 250, 260);
            probe.EmitHeartbeat("ToDo", 250, 1000);
            probe.EmitGcDelta("ToDo", 1, 0, 0, 100, false, "Interactive");
            probe.EmitGcDelta("Engines", 2, 1, 0, 200, true, "Batch");

            // Assert: exactly one line per call (5 calls => 5 lines), in call order, each annotated.
            emitted.Should().HaveCount(5);
            emitted[0].Should().StartWith("[ui-heartbeat] ").And.Contain("phase=IntelConfig ");
            emitted[1].Should().StartWith("[ui-heartbeat] ").And.Contain("phase=IntelConfig ");
            emitted[2].Should().StartWith("[ui-heartbeat] ").And.Contain("phase=ToDo ");
            emitted[3].Should().StartWith("[gc-delta] ").And.Contain("phase=ToDo ");
            emitted[4].Should().StartWith("[gc-delta] ").And.Contain("phase=Engines ");
        }

        [TestMethod]
        public void EmitLifetimeHeartbeat_MultipleStages_AnnotatesStageAndComputesGap()
        {
            // Arrange: capture lines through the injected sink (issue #211, Phase 3.3). No live
            // timer/Dispatcher/GC; no temporary files.
            var emitted = new List<string>();
            var probe = new TaskMaster.StartupDiagnosticsProbe(s => emitted.Add(s));

            // Act: one tick attributed to the Loading stage with a multi-minute stall (108000 ms vs
            // 250 ms nominal) and one near-nominal tick attributed to the PreGlobalsCtor stage.
            probe.EmitLifetimeHeartbeat(TaskMaster.StartupStageLabels.Loading, 250, 108000);
            probe.EmitLifetimeHeartbeat(TaskMaster.StartupStageLabels.PreGlobalsCtor, 250, 251);

            // Assert: each call emits exactly one [startup-lifetime-heartbeat] line with the
            // stageLabel field and nominal/actual/gap fields formatted F1 InvariantCulture; the gap
            // is actual - nominal (gapMs=107750.0 for the stall, gapMs=1.0 for the near-nominal tick).
            emitted.Should().HaveCount(2);

            var stallLine = emitted[0];
            stallLine.Should().StartWith("[startup-lifetime-heartbeat] ");
            stallLine.Should().Contain("stageLabel=Loading ");
            stallLine.Should().Contain("nominalMs=250.0 ");
            stallLine.Should().Contain("actualMs=108000.0 ");
            stallLine.Should().Contain("gapMs=107750.0");

            var nearNominalLine = emitted[1];
            nearNominalLine.Should().StartWith("[startup-lifetime-heartbeat] ");
            nearNominalLine.Should().Contain("stageLabel=PreGlobalsCtor ");
            nearNominalLine.Should().Contain("nominalMs=250.0 ");
            nearNominalLine.Should().Contain("actualMs=251.0 ");
            nearNominalLine.Should().Contain("gapMs=1.0");
        }

        [TestMethod]
        public void StopDecider_MaxCapBranch_StopsAtOrAboveCapWhenPostLoadNotReached()
        {
            // Arrange: a decider with an explicit small cap (1000 ms) so the max-cap branch is
            // exercised deterministically with synthetic elapsed-since-start values (issue #211,
            // Phase 3.3). PostLoad is NOT reached for any tick.
            var decider = new TaskMaster.StartupLifetimeStopDecider(
                maxCapMs: 1000.0,
                responsiveThresholdMs: 50.0,
                requiredSustainedTicks: 8
            );

            // Act / Assert: below the cap the decider does not stop, even with large gaps.
            decider
                .ShouldStop(elapsedSinceStartMs: 250, gapMs: 500, postLoadReached: false)
                .Should()
                .BeFalse();
            decider
                .ShouldStop(elapsedSinceStartMs: 999, gapMs: 500, postLoadReached: false)
                .Should()
                .BeFalse();

            // At the cap it stops; above the cap it also stops.
            decider
                .ShouldStop(elapsedSinceStartMs: 1000, gapMs: 500, postLoadReached: false)
                .Should()
                .BeTrue();
            decider
                .ShouldStop(elapsedSinceStartMs: 5000, gapMs: 500, postLoadReached: false)
                .Should()
                .BeTrue();
        }

        [TestMethod]
        public void StopDecider_SustainedResponsiveness_StopsOnlyAfterRunAndResetsOnNonResponsiveTick()
        {
            // Arrange: a decider requiring 3 consecutive responsive ticks (gap <= 50 ms) after
            // PostLoad, with a large cap so only the responsiveness branch can stop (issue #211,
            // Phase 3.3).
            var decider = new TaskMaster.StartupLifetimeStopDecider(
                maxCapMs: 1_000_000.0,
                responsiveThresholdMs: 50.0,
                requiredSustainedTicks: 3
            );

            // Act / Assert: two responsive ticks are not yet sufficient.
            decider.ShouldStop(1000, 10, postLoadReached: true).Should().BeFalse();
            decider.ShouldStop(1250, 20, postLoadReached: true).Should().BeFalse();
            decider.ConsecutiveResponsiveTicks.Should().Be(2);

            // A non-responsive tick (gap above threshold) resets the consecutive counter.
            decider.ShouldStop(1500, 400, postLoadReached: true).Should().BeFalse();
            decider.ConsecutiveResponsiveTicks.Should().Be(0);

            // The sustained run must restart: two more responsive ticks still do not stop.
            decider.ShouldStop(1750, 5, postLoadReached: true).Should().BeFalse();
            decider.ShouldStop(2000, 5, postLoadReached: true).Should().BeFalse();
            // The third consecutive responsive tick meets the required sustained count and stops.
            decider.ShouldStop(2250, 5, postLoadReached: true).Should().BeTrue();
            decider.ConsecutiveResponsiveTicks.Should().Be(3);
        }

        [TestMethod]
        public void StopDecider_PrePostLoad_SustainedResponsivenessDoesNotStop()
        {
            // Arrange: a decider requiring 3 sustained responsive ticks with a large cap (issue
            // #211, Phase 3.3). PostLoad is NOT reached, so only the max cap may stop.
            var decider = new TaskMaster.StartupLifetimeStopDecider(
                maxCapMs: 1_000_000.0,
                responsiveThresholdMs: 50.0,
                requiredSustainedTicks: 3
            );

            // Act / Assert: many sustained low-gap ticks before PostLoad never trigger the
            // responsiveness stop, even though the consecutive counter exceeds the required count.
            for (var i = 0; i < 10; i++)
            {
                decider
                    .ShouldStop(
                        elapsedSinceStartMs: 250 * (i + 1),
                        gapMs: 5,
                        postLoadReached: false
                    )
                    .Should()
                    .BeFalse();
            }

            decider.ConsecutiveResponsiveTicks.Should().BeGreaterThanOrEqualTo(3);
        }

        [TestMethod]
        public void StartupStageLabels_All_ContainsExactlyTheFiveCanonicalLabelsInOrder()
        {
            // Act / Assert: the canonical stage-label set is exactly the five labels in startup
            // order (issue #211, Phase 3.3). The call site references these constants rather than
            // string literals.
            TaskMaster
                .StartupStageLabels.All.Should()
                .Equal("PreGlobalsCtor", "GlobalsCtor", "AwaitingIdleQueue", "Loading", "PostLoad");

            TaskMaster.StartupStageLabels.PreGlobalsCtor.Should().Be("PreGlobalsCtor");
            TaskMaster.StartupStageLabels.GlobalsCtor.Should().Be("GlobalsCtor");
            TaskMaster.StartupStageLabels.AwaitingIdleQueue.Should().Be("AwaitingIdleQueue");
            TaskMaster.StartupStageLabels.Loading.Should().Be("Loading");
            TaskMaster.StartupStageLabels.PostLoad.Should().Be("PostLoad");
        }

        [TestMethod]
        public void StopDecider_BoundingParameters_ExposeDefaultsAndExplicitValues()
        {
            // Arrange / Act: the default constructor exposes the documented defaults (issue #211,
            // Phase 3.3): max cap 180000 ms, responsiveness threshold 50 ms, sustained count 8.
            var defaults = new TaskMaster.StartupLifetimeStopDecider();

            // Assert: defaults are surfaced for the call site / diagnostics.
            defaults.MaxCapMs.Should().Be(180000.0);
            defaults.ResponsiveThresholdMs.Should().Be(50.0);
            defaults.RequiredSustainedTicks.Should().Be(8);
            defaults.ConsecutiveResponsiveTicks.Should().Be(0);

            // Act: explicit bounding parameters are surfaced verbatim.
            var custom = new TaskMaster.StartupLifetimeStopDecider(
                maxCapMs: 1234.0,
                responsiveThresholdMs: 17.0,
                requiredSustainedTicks: 3
            );

            // Assert
            custom.MaxCapMs.Should().Be(1234.0);
            custom.ResponsiveThresholdMs.Should().Be(17.0);
            custom.RequiredSustainedTicks.Should().Be(3);
        }
    }
}
