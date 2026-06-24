using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OutlookApplication = Microsoft.Office.Interop.Outlook.Application;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Deterministic verification of the continuation-latency attribution probe (issue #211).
    /// Drives the real <see cref="ApplicationGlobals.LoadSequentialAsync"/> sequence through a
    /// subclass that overrides every phase wrapper to a no-op and overrides the probe to record
    /// the prior-phase name WITHOUT calling base, so no live COM, live timer, or static
    /// <c>ApplicationIdleTimer</c> reads occur in CI.
    /// </summary>
    [TestClass]
    public class ContinuationProbeSequenceTests
    {
        // DoNotParallelize: the recording subclass drives LoadAsync against the process-global
        // ApplicationGlobals seam; keep it serialized with the other AppGlobals timing tests that
        // share that global state.
        [TestMethod]
        [DoNotParallelize]
        public async Task LoadSequentialAsync_InvokesProbeForEachBoundaryInStartupOrder()
        {
            // Arrange
            var recorded = new List<string>();
            var sut = new RecordingApplicationGlobals(CreateOutlookApplicationStub(), recorded);

            // Act
            await sut.LoadAsync(parallel: false);

            // Assert: one probe invocation per inter-phase boundary, in startup order, with the
            // preceding phase name. Fails if any name or ordering changes.
            recorded.Should().Equal("IntelConfig", "OlObjects", "ToDo", "AutoFile", "Engines");
        }

        [TestMethod]
        [DoNotParallelize]
        public async Task LoadSequentialAsync_InvokesProbeExactlyOncePerBoundary()
        {
            // Arrange
            var recorded = new List<string>();
            var sut = new RecordingApplicationGlobals(CreateOutlookApplicationStub(), recorded);

            // Act
            await sut.LoadAsync(parallel: false);

            // Assert: exactly five inter-phase boundaries are probed, preserving the original yield
            // count.
            recorded.Should().HaveCount(5);
        }

        private static OutlookApplication CreateOutlookApplicationStub()
        {
            return new Mock<OutlookApplication>().Object;
        }

        private sealed class RecordingApplicationGlobals : ApplicationGlobals
        {
            private readonly List<string> _recordedPriorPhases;

            public RecordingApplicationGlobals(
                OutlookApplication application,
                List<string> recordedPriorPhases
            )
                : base(application, false)
            {
                _recordedPriorPhases = recordedPriorPhases;
            }

            // Set a fixed non-zero LoadBasic elapsed so LoadAsync's ForceBasicLoad path is stable
            // without constructing live COM collaborators.
            protected internal override void LoadBasicMethod()
            {
                typeof(ApplicationGlobals)
                    .GetField("_loadBasicElapsed", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .SetValue(this, TimeSpan.FromMilliseconds(7));
            }

            // Override every phase wrapper to a no-op so the real COM-bound phase bodies (and the
            // Engines initialization) never run; the inter-phase probe still fires after each phase.
            protected internal override Task LoadIntelConfigPhaseAsync() => Task.CompletedTask;

            protected internal override Task LoadOlObjectsPhaseAsync() => Task.CompletedTask;

            protected internal override Task LoadToDoPhaseAsync() => Task.CompletedTask;

            protected internal override Task LoadAutoFilePhaseAsync() => Task.CompletedTask;

            protected internal override Task InitializeEnginesPhaseAsync() => Task.CompletedTask;

            protected internal override Task LoadEventsPhaseAsync() => Task.CompletedTask;

            // Record the prior-phase name without calling base, so the static ApplicationIdleTimer
            // reads in the production probe never execute under the unit-test seam.
            protected internal override Task YieldWithContinuationProbeAsync(string priorPhaseName)
            {
                _recordedPriorPhases.Add(priorPhaseName);
                return Task.CompletedTask;
            }

            // No-op the issue #211 Phase 3.2 host-bound diagnostics seams so the heartbeat
            // DispatcherTimer (which needs a live UiThread.Dispatcher) and the live GC.* reads never
            // execute under the unit-test seam. Mirrors the phase-wrapper override pattern above.
            protected internal override void StartStartupUiHeartbeat(
                TaskMaster.StartupDiagnosticsProbe probe
            ) { }

            protected internal override void StopStartupUiHeartbeat() { }

            protected internal override void BeginPhaseGcCapture(string phase) { }

            protected internal override void EmitPhaseGcDelta(
                TaskMaster.StartupDiagnosticsProbe probe,
                string phase
            ) { }
        }
    }
}
